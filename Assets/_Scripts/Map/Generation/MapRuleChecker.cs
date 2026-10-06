using UnityEngine;
using System.Collections.Generic;

// Última etapa del generador: comprueba sobre el mapa ya armado las reglas de estructura que todo mapa tiene que
// cumplir. Las obligatorias descartan el intento; las de aviso solo se informan. Es el equivalente, sin NavMesh,
// de lo que MapValidator chequea después del bake.
public static class MapRuleChecker
{
    private const float DropOffCheckRadius = 3.5f;
    private const float MinDropOffRatio = 0.6f;
    private const float MaxWaterCoverage = 0.35f;
    private const float ClusterSizeTolerance = 0.75f;

    public static List<MapRuleResult> Check(MapGenerationContext context)
    {
        List<MapRuleResult> results = new List<MapRuleResult>();

        CheckMarkers(context, results);
        CheckRoutes(context, results);
        CheckStartArea(context, results);
        CheckResources(context, results);
        CheckPlayableArea(context, results);
        CheckAdvisories(context, results);

        return results;
    }

    private static void CheckMarkers(MapGenerationContext context, List<MapRuleResult> results)
    {
        MapLayout layout = context.Layout;
        float minDistance = context.Request.Layout.MinSpawnDistance * context.Extent;

        bool startOnGround = context.IsWalkable(context.CellIndex(layout.PlayerStart));
        Add(results, "Inicio del jugador en suelo firme", startOnGround, true, startOnGround ? "" : "El HQ cayó sobre agua, un acantilado o un obstáculo.");

        int connected = 0;
        float closest = float.MaxValue;
        foreach (Vector2 spawn in layout.Spawns)
        {
            if (context.Reachable[context.CellIndex(spawn)]) connected++;
            closest = Mathf.Min(closest, Vector2.Distance(spawn, layout.PlayerStart));
        }

        Add(results, "Spawns conectados con el HQ", connected == layout.Spawns.Count, true, $"{connected} de {layout.Spawns.Count} spawns tienen ruta a pie hasta el HQ.");
        Add(results, "Distancia mínima spawn → HQ", closest >= minDistance, true, $"El spawn más cercano está a {closest:0} celdas (mínimo {minDistance:0}).");

        if (layout.HasWaveEnd)
        {
            bool endConnected = context.Reachable[context.CellIndex(layout.WaveEnd)];
            Add(results, "Fin de oleada conectado", endConnected, true, endConnected ? "" : "No hay ruta a pie entre el HQ y el fin de oleada.");
        }
    }

    private static void CheckRoutes(MapGenerationContext context, List<MapRuleResult> results)
    {
        MapLayoutRules rules = context.Request.Layout;
        float minLength = rules.MinRouteLength * context.Extent;
        float minShared = rules.MinSharedStretch * context.Extent;

        int shortestRoute = int.MaxValue;
        int shortestShared = int.MaxValue;
        int blockedPoints = 0;

        foreach (MapRoute route in context.Routes)
        {
            foreach (Vector2 point in route.Smoothed)
            {
                if (!context.IsWalkable(context.CellIndex(point))) blockedPoints++;
            }

            if (route.SpawnIndex < 0) continue;

            shortestRoute = Mathf.Min(shortestRoute, route.LengthToDefense);
            if (route.SpawnIndex > 0) shortestShared = Mathf.Min(shortestShared, route.SharedStretch);
        }

        Add(results, "Camino transitable de punta a punta", blockedPoints == 0, true, blockedPoints == 0 ? "" : $"{blockedPoints} puntos del camino quedaron sobre agua, acantilado u obstáculos.");
        Add(results, "Largo mínimo de ruta", shortestRoute >= minLength, true, $"La ruta más corta hasta la zona de defensa mide {shortestRoute} celdas (mínimo {minLength:0}).");

        if (shortestShared != int.MaxValue)
        {
            Add(results, "Tramo final compartido", shortestShared >= minShared, true, $"Las rutas comparten las últimas {shortestShared} celdas (mínimo {minShared:0}).");
        }
    }

    private static void CheckStartArea(MapGenerationContext context, List<MapRuleResult> results)
    {
        MapStartRules rules = context.Request.Start;
        float ratio = context.GetDiscRatio(context.Layout.PlayerStart, rules.ClearRadius, index => IsBuildable(context, index));

        Add(results, "Zona de inicio edificable", ratio >= rules.MinBuildableRatio, true, $"{ratio * 100f:0}% de la zona de inicio es edificable (mínimo {rules.MinBuildableRatio * 100f:0}%).");
    }

    private static void CheckResources(MapGenerationContext context, List<MapRuleResult> results)
    {
        foreach (MapResourceRule rule in context.Request.Resources)
        {
            if (context.MissingResourceTypes.Contains(rule.Type))
            {
                Add(results, $"Prefab de {rule.Type}", false, false, $"La paleta no tiene un nodo de {rule.Type}; las reglas de ese recurso se ignoran.");
                continue;
            }

            MapResourceCluster starter = context.Clusters.Find(cluster => cluster.IsStarter && cluster.Type == rule.Type);
            bool hasStarter = starter != null && starter.NodeCount >= rule.StarterNodes.x && IsClusterReachable(context, starter);
            Add(results, $"Depósito inicial de {rule.Type}", hasStarter, true, starter != null ? $"{starter.NodeCount} nodos junto al HQ (mínimo {rule.StarterNodes.x})." : "No hubo lugar junto al HQ.");

            int usable = 0;
            foreach (MapResourceCluster cluster in context.Clusters)
            {
                if (cluster.IsStarter || cluster.Type != rule.Type) continue;
                if (cluster.NodeCount < rule.ClusterSize.x * ClusterSizeTolerance) continue;
                if (!context.Reachable[context.CellIndex(cluster.DropOffSpot)]) continue;
                if (context.GetDiscRatio(cluster.DropOffSpot, DropOffCheckRadius, index => context.Reachable[index] && IsBuildable(context, index)) < MinDropOffRatio) continue;

                usable++;
            }

            Add(results, $"Expansiones de {rule.Type}", usable >= rule.ExpansionClusters.x, true, $"{usable} depósitos grandes alcanzables y con lugar para el drop-off (mínimo {rule.ExpansionClusters.x}).");
        }
    }

    private static void CheckPlayableArea(MapGenerationContext context, List<MapRuleResult> results)
    {
        int playable = 0;
        for (int i = 0; i < context.Reachable.Length; i++)
        {
            if (context.Reachable[i] && IsBuildable(context, i)) playable++;
        }

        float ratio = playable / (float)context.Reachable.Length;
        Add(results, "Área jugable", ratio >= context.Request.MinPlayableArea, true, $"{ratio * 100f:0}% del mapa es alcanzable y edificable (mínimo {context.Request.MinPlayableArea * 100f:0}%).");
    }

    private static void CheckAdvisories(MapGenerationContext context, List<MapRuleResult> results)
    {
        MapLayout layout = context.Layout;
        if (layout.Spawns.Count < layout.PlannedSpawnCount)
        {
            Add(results, "Cantidad de spawns", false, false, $"Se pidieron {layout.PlannedSpawnCount} spawns y solo entraron {layout.Spawns.Count} respetando distancias.");
        }

        int water = 0;
        float waterLevel = context.Request.Water.WaterLevel;
        foreach (float height in context.CellHeight)
        {
            if (height < waterLevel) water++;
        }

        float coverage = water / (float)context.CellHeight.Length;
        if (coverage > MaxWaterCoverage)
        {
            Add(results, "Cobertura de agua", false, false, $"{coverage * 100f:0}% del mapa quedó bajo el agua.");
        }
    }

    private static bool IsBuildable(MapGenerationContext context, int cellIndex)
    {
        return !context.NotBuildable[cellIndex] && context.IsWalkable(cellIndex);
    }

    private static bool IsClusterReachable(MapGenerationContext context, MapResourceCluster cluster)
    {
        return context.GetDiscRatio(cluster.Center, cluster.Radius + 2f, index => context.Reachable[index]) > 0f;
    }

    private static void Add(List<MapRuleResult> results, string rule, bool passed, bool required, string detail)
    {
        results.Add(new MapRuleResult { Rule = rule, Passed = passed, Required = required, Detail = detail });
    }
}
