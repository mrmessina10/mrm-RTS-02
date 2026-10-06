using UnityEngine;
using System.Collections.Generic;

// Punto de entrada del generador procedural de mapas. Corre las etapas en orden (layout → relieve y agua → caminos →
// nivelado → máscaras → objetos → reglas) y, si un intento rompe una regla obligatoria, lo descarta y prueba el
// siguiente con la misma semilla. Misma semilla + mismas reglas + mismo tamaño = mismo mapa.
public static class MapGenerator
{
    private const float PathKeepClearPadding = 1f;
    private const float FordKeepClearPadding = 1f;
    private const float StartPatchRadiusFactor = 0.7f;
    private const int BorderNoBuildCells = 2;

    public static GeneratedMap Generate(MapGenerationRequest request)
    {
        GeneratedMap best = null;
        List<MapRuleResult> discardedRules = new List<MapRuleResult>();
        int attemptsUsed = 0;

        for (int attempt = 0; attempt < Mathf.Max(1, request.MaxAttempts); attempt++)
        {
            GeneratedMap candidate = RunAttempt(request, attempt);
            attemptsUsed++;

            if (best == null || candidate.FailedRequiredRules < best.FailedRequiredRules) best = candidate;
            if (candidate.IsValid) break;

            discardedRules.AddRange(candidate.Rules.FindAll(rule => rule.Required && !rule.Passed));
        }

        best.AttemptsUsed = attemptsUsed;
        best.DiscardedRules = discardedRules;
        return best;
    }

    private static GeneratedMap RunAttempt(MapGenerationRequest request, int attempt)
    {
        MapGenerationContext context = new MapGenerationContext(request, attempt);

        MapLayoutPlanner.Plan(context);
        MapTerrainShaper.Shape(context);

        context.RefreshTerrainGrids();
        MapPathRouter.Route(context);
        MapTerrainShaper.CarveRoutes(context);

        context.RefreshTerrainGrids();
        BuildMasks(context);
        context.FloodFillReachable(false);

        new MapObjectScatterer(context).Populate();
        context.FloodFillReachable(true);

        return BuildResult(context, MapRuleChecker.Check(context));
    }

    // Distancia de cada celda al camino, zonas que la decoración no puede tapar (camino, rampas, vados, inicio,
    // spawns) y máscara no edificable (camino con su margen, spawns y borde del mapa)
    private static void BuildMasks(MapGenerationContext context)
    {
        MapLayout layout = context.Layout;
        MapStartRules startRules = context.Request.Start;
        float halfWidth = context.PathWidth / 2f;

        for (int i = 0; i < context.PathDistance.Length; i++)
        {
            context.PathDistance[i] = float.MaxValue;
        }
        foreach (MapRoute route in context.Routes)
        {
            MapHeightField.StampPolyline(route.Smoothed, MapGenerationContext.PathDistanceReach, 1f, 0.5f, context.Width, context.Depth, context.PathDistance, null);
        }

        float noBuildReach = halfWidth + context.Request.Path.BuildMargin;
        for (int z = 0; z < context.Depth; z++)
        {
            for (int x = 0; x < context.Width; x++)
            {
                int index = context.CellIndex(x, z);
                bool isBorder = x < BorderNoBuildCells || z < BorderNoBuildCells || x >= context.Width - BorderNoBuildCells || z >= context.Depth - BorderNoBuildCells;

                context.KeepClear[index] = context.PathDistance[index] <= halfWidth + PathKeepClearPadding;
                context.NotBuildable[index] = isBorder || context.PathDistance[index] <= noBuildReach;
            }
        }

        context.MarkDisc(context.KeepClear, layout.PlayerStart, startRules.ClearRadius);
        foreach (Vector2 spawn in layout.Spawns)
        {
            context.MarkDisc(context.KeepClear, spawn, startRules.SpawnClearRadius);
            context.MarkDisc(context.NotBuildable, spawn, startRules.SpawnClearRadius);
        }
        if (layout.HasWaveEnd)
        {
            context.MarkDisc(context.KeepClear, layout.WaveEnd, startRules.SpawnClearRadius);
            context.MarkDisc(context.NotBuildable, layout.WaveEnd, startRules.SpawnClearRadius);
        }

        float rampRadius = context.Request.Terrain.RampWidth / 2f + 1f;
        foreach (MapRamp ramp in context.Ramps)
        {
            int steps = Mathf.CeilToInt(Vector2.Distance(ramp.Top, ramp.Bottom));
            for (int step = -2; step <= steps + 2; step++)
            {
                context.MarkDisc(context.KeepClear, Vector2.LerpUnclamped(ramp.Top, ramp.Bottom, step / (float)steps), rampRadius);
            }
        }

        float fordRadius = Mathf.Max(context.Request.Water.RiverWidth.y / 2f, halfWidth) + MapTerrainShaper.RiverShoreWidth + MapTerrainShaper.FordRampLength + FordKeepClearPadding;
        foreach (Vector2 ford in context.Fords)
        {
            context.MarkDisc(context.KeepClear, ford, fordRadius);
            context.TexturePatches.Add(new GeneratedTexturePatch { Ground = MapGroundPatch.Sand, Center = context.ToWorld(ford), Radius = fordRadius, Opacity = 0.8f });
        }

        context.TexturePatches.Add(new GeneratedTexturePatch
        {
            Ground = MapGroundPatch.Dirt,
            Center = context.ToWorld(layout.PlayerStart),
            Radius = startRules.ClearRadius * StartPatchRadiusFactor,
            Opacity = 0.6f
        });
    }

    private static GeneratedMap BuildResult(MapGenerationContext context, List<MapRuleResult> rules)
    {
        MapGenerationRequest request = context.Request;
        MapLayout layout = context.Layout;

        GeneratedMap map = new GeneratedMap
        {
            Seed = request.Seed,
            Attempt = context.Attempt,
            Size = request.Size,
            Origin = request.Origin,
            WaterLevel = request.Water.WaterLevel,
            PathWidth = context.PathWidth,
            Layout = layout.Type,
            WaterMode = context.WaterMode,
            PlateauCount = context.Plateaus.Count,
            Heights = context.Heights,
            Catalog = request.Catalog,
            Objects = context.Objects,
            TexturePatches = context.TexturePatches,
            Rules = rules,
            NotBuildableCells = context.NotBuildable,
            ReachableCells = context.Reachable,
            PathDistance = context.PathDistance,
            WalkableCells = new bool[context.TerrainBlocked.Length]
        };

        for (int i = 0; i < map.WalkableCells.Length; i++)
        {
            map.WalkableCells[i] = !context.TerrainBlocked[i];
        }

        foreach (Vector2 spawn in layout.Spawns)
        {
            map.Markers.Add(new GeneratedMapMarker { Type = MapMarkerType.EnemySpawn, Position = context.ToWorld(spawn) });
        }
        if (layout.HasWaveEnd) map.Markers.Add(new GeneratedMapMarker { Type = MapMarkerType.WaveEnd, Position = context.ToWorld(layout.WaveEnd) });
        map.Markers.Add(new GeneratedMapMarker { Type = MapMarkerType.PlayerStart, Position = context.ToWorld(layout.PlayerStart) });

        foreach (MapRoute route in context.Routes)
        {
            GeneratedMapPath path = new GeneratedMapPath { Width = context.PathWidth };
            foreach (Vector2 waypoint in route.Waypoints)
            {
                path.Waypoints.Add(context.ToWorld(waypoint));
            }
            map.Paths.Add(path);
        }

        return map;
    }
}
