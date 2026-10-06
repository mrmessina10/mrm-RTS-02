using UnityEngine;
using System.Collections.Generic;

public enum MapEdge
{
    South,
    East,
    North,
    West
}

public class MapLayout
{
    public MapLayoutType Type;
    public MapEdge EntryEdge;
    public Vector2 PlayerStart;
    public List<Vector2> Spawns = new List<Vector2>();
    public bool HasWaveEnd;
    public Vector2 WaveEnd;
    public Vector2 Junction;
    public int PlannedSpawnCount;
}

// Primera etapa del generador: decide el esqueleto del mapa (borde de entrada, spawns, HQ y, en el layout A → B,
// fin de oleada y punto de empalme del acceso al HQ) y reserva esas zonas para que relieve y agua no las tapen.
public static class MapLayoutPlanner
{
    private const int Stage = 1;
    private const int SpawnPlacementTries = 40;
    private const float JunctionPadding = 2f;

    public static void Plan(MapGenerationContext context)
    {
        MapLayoutRules rules = context.Request.Layout;
        MapStartRules startRules = context.Request.Start;
        MapRandom random = context.CreateRandom(Stage);

        MapLayout layout = new MapLayout();
        layout.Type = random.PickWeighted(rules.PathToStartWeight, rules.PassThroughWeight) == 0 ? MapLayoutType.PathToStart : MapLayoutType.PassThrough;
        layout.EntryEdge = (MapEdge)random.Index(4);
        layout.PlannedSpawnCount = Mathf.Max(1, random.Range(rules.SpawnCount));

        float startMargin = rules.EdgeMargin + startRules.ClearRadius;
        float spawnLateral = random.Range(0.25f, 0.75f);
        layout.Spawns.Add(context.ClampToMargin(GetPoint(context, layout.EntryEdge, spawnLateral, 0f), rules.EdgeMargin));

        if (layout.Type == MapLayoutType.PathToStart)
        {
            Vector2 start = GetPoint(context, layout.EntryEdge, random.Range(rules.StartLateral), random.Range(rules.StartDepth));
            layout.PlayerStart = context.ClampToMargin(start, startMargin);
        }
        else
        {
            float endLateral = random.Range(0.25f, 0.75f);
            layout.HasWaveEnd = true;
            layout.WaveEnd = context.ClampToMargin(GetPoint(context, layout.EntryEdge, endLateral, 1f), rules.EdgeMargin);

            float junctionLateral = Mathf.Lerp(spawnLateral, endLateral, 0.5f) + random.Range(-0.08f, 0.08f);
            layout.Junction = GetPoint(context, layout.EntryEdge, junctionLateral, random.Range(0.45f, 0.6f));
            layout.PlayerStart = PlaceStartBesidePath(context, layout, random, startMargin);
        }

        PlaceExtraSpawns(context, layout, random);

        context.Layout = layout;
        context.AddReservedZone(layout.PlayerStart, startRules.ClearRadius + 2f);
        foreach (Vector2 spawn in layout.Spawns)
        {
            context.AddReservedZone(spawn, startRules.SpawnClearRadius);
        }
        if (layout.HasWaveEnd)
        {
            context.AddReservedZone(layout.WaveEnd, startRules.SpawnClearRadius);
            context.AddReservedZone(layout.Junction, context.Request.Path.Width.y + JunctionPadding);
        }
    }

    // Punto del mapa medido desde el borde de entrada: lateral recorre ese borde y depth se aleja de él (0..1)
    public static Vector2 GetPoint(MapGenerationContext context, MapEdge entryEdge, float lateral, float depth)
    {
        switch (entryEdge)
        {
            case MapEdge.South: return new Vector2(lateral * context.Width, depth * context.Depth);
            case MapEdge.North: return new Vector2(lateral * context.Width, (1f - depth) * context.Depth);
            case MapEdge.West: return new Vector2(depth * context.Width, lateral * context.Depth);
            default: return new Vector2((1f - depth) * context.Width, lateral * context.Depth);
        }
    }

    public static float GetDistanceToEdge(MapGenerationContext context, MapEdge edge, float x, float z)
    {
        switch (edge)
        {
            case MapEdge.South: return z;
            case MapEdge.North: return context.Depth - z;
            case MapEdge.West: return x;
            default: return context.Width - x;
        }
    }

    private static Vector2 PlaceStartBesidePath(MapGenerationContext context, MapLayout layout, MapRandom random, float startMargin)
    {
        Vector2 axis = (layout.WaveEnd - layout.Spawns[0]).normalized;
        Vector2 perpendicular = new Vector2(-axis.y, axis.x);
        float offset = random.Range(context.Request.Layout.StartPathOffset);
        float side = random.Chance(0.5f) ? 1f : -1f;

        Vector2 preferred = layout.Junction + perpendicular * (offset * side);
        if (context.IsInsideMargin(preferred, startMargin)) return preferred;

        Vector2 opposite = layout.Junction - perpendicular * (offset * side);
        if (context.IsInsideMargin(opposite, startMargin)) return opposite;

        return context.ClampToMargin(preferred, startMargin);
    }

    private static void PlaceExtraSpawns(MapGenerationContext context, MapLayout layout, MapRandom random)
    {
        MapLayoutRules rules = context.Request.Layout;
        float minStartDistance = rules.MinSpawnDistance * context.Extent;
        float minSeparation = rules.MinSpawnSeparation * context.Extent;

        for (int i = 1; i < layout.PlannedSpawnCount; i++)
        {
            for (int attempt = 0; attempt < SpawnPlacementTries; attempt++)
            {
                Vector2 candidate = random.Chance(0.5f)
                    ? GetPoint(context, layout.EntryEdge, random.Chance(0.5f) ? 0f : 1f, random.Range(0.1f, 0.4f))
                    : GetPoint(context, layout.EntryEdge, random.Range(0.12f, 0.88f), 0f);
                candidate = context.ClampToMargin(candidate, rules.EdgeMargin);

                if (Vector2.Distance(candidate, layout.PlayerStart) < minStartDistance) continue;
                if (layout.Spawns.Exists(spawn => Vector2.Distance(spawn, candidate) < minSeparation)) continue;

                layout.Spawns.Add(candidate);
                break;
            }
        }
    }
}
