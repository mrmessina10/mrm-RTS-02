using UnityEngine;
using System.Collections.Generic;

// Etapa de relieve del generador: ondulación de base, mesetas por niveles con sus rampas, agua (lagos, río con
// vados o costa) y, una vez trazadas las rutas, el nivelado del terreno debajo de cada camino. Todo se atenúa
// con la máscara de zonas reservadas para que el inicio y los spawns queden siempre en suelo plano y seco.
public static class MapTerrainShaper
{
    private const int Stage = 2;
    private const int PlacementTries = 20;
    private const int AverageWindow = 4;
    private const float MinDryClearance = 0.4f;
    private const float CliffWidth = 1.2f;
    private const float ShapeFrequency = 0.08f;
    private const float MinPlateauRadius = 5f;
    private const float MinSecondLevelRadius = 10f;
    private const float MinLakeRadius = 4f;
    private const float ShoreHardness = 0.45f;
    public const float RiverShoreWidth = 3f;
    private const float CoastShoreWidth = 4f;
    private const float MinCoastDepth = 5f;
    private const float RampInset = 1.5f;
    private const float FordBlend = 2f;
    public const float FordRampLength = 6f;

    public static void Shape(MapGenerationContext context)
    {
        MapRandom random = context.CreateRandom(Stage);

        ApplyRelief(context, random);
        AddPlateaus(context, random);
        AddWater(context, random);
    }

    private static void ApplyRelief(MapGenerationContext context, MapRandom random)
    {
        MapTerrainRules rules = context.Request.Terrain;
        MapNoise noise = new MapNoise(random);
        float amplitude = random.Range(rules.ReliefAmplitude);
        float frequency = 1f / Mathf.Max(1f, random.Range(rules.ReliefWavelength));
        float floor = Mathf.Min(0f, context.Request.Water.WaterLevel + MinDryClearance);

        context.Heights.ModifyAll((x, z, height) =>
        {
            float relief = (noise.Fractal(x * frequency, z * frequency, 4) - 0.5f) * 2f * amplitude;
            return Mathf.Max(floor, relief * context.GetReserveMask(x, z));
        });
    }

    private static void AddPlateaus(MapGenerationContext context, MapRandom random)
    {
        MapTerrainRules rules = context.Request.Terrain;
        MapNoise shapeNoise = new MapNoise(random);
        int count = random.Range(rules.PlateauCount);

        for (int i = 0; i < count; i++)
        {
            for (int attempt = 0; attempt < PlacementTries; attempt++)
            {
                float radius = Mathf.Max(MinPlateauRadius, random.Range(rules.PlateauRadius) * context.Extent);
                Vector2 center = new Vector2(random.Range(0f, context.Width), random.Range(0f, context.Depth));
                if (!context.IsClearOfReservedZones(center, radius * 1.3f + 4f)) continue;

                RaiseBlob(context, shapeNoise, center, radius, rules.LevelHeight);
                context.Plateaus.Add(new MapPlateau { Center = center, Radius = radius, TopHeight = rules.LevelHeight, BaseHeight = 0f });

                if (radius >= MinSecondLevelRadius && random.Chance(rules.SecondLevelChance))
                {
                    float innerRadius = radius * 0.5f;
                    RaiseBlob(context, shapeNoise, center, innerRadius, rules.LevelHeight * 2f);
                    context.Plateaus.Add(new MapPlateau { Center = center, Radius = innerRadius, TopHeight = rules.LevelHeight * 2f, BaseHeight = rules.LevelHeight });
                }
                break;
            }
        }

        foreach (MapPlateau plateau in context.Plateaus)
        {
            int rampCount = random.Range(rules.RampsPerPlateau);
            for (int i = 0; i < rampCount; i++)
            {
                Vector2 direction = random.Direction();
                if (i == 0)
                {
                    Vector2 towardStart = (context.Layout.PlayerStart - plateau.Center).normalized;
                    direction = (towardStart + direction * 0.5f).normalized;
                }
                AddRamp(context, plateau, direction);
            }
        }
    }

    private static void RaiseBlob(MapGenerationContext context, MapNoise shapeNoise, Vector2 center, float radius, float topHeight)
    {
        float hardness = Mathf.Clamp01(1f - CliffWidth / radius);
        Vector2 extent = new Vector2(radius, radius) * 1.3f;

        context.Heights.Modify(center - extent, center + extent, (x, z, height) =>
        {
            float shapedRadius = radius * (0.7f + 0.6f * shapeNoise.Fractal(x * ShapeFrequency, z * ShapeFrequency, 2));
            float distance = Vector2.Distance(new Vector2(x, z), center);
            float weight = MapHeightField.GetFalloff(distance / shapedRadius, hardness) * context.GetReserveMask(x, z);
            return weight <= 0f ? height : Mathf.Lerp(height, Mathf.Max(height, topHeight), weight);
        });
    }

    private static void AddRamp(MapGenerationContext context, MapPlateau plateau, Vector2 direction)
    {
        MapTerrainRules rules = context.Request.Terrain;
        MapHeightField heights = context.Heights;
        float levelHeight = plateau.TopHeight - plateau.BaseHeight;
        float midHeight = plateau.BaseHeight + levelHeight * 0.5f;
        float searchStart = plateau.Radius * 1.4f;

        float edgeDistance = -1f;
        for (float distance = searchStart; distance > RampInset; distance -= 0.5f)
        {
            if (heights.Sample(plateau.Center + direction * distance) <= midHeight) continue;

            edgeDistance = distance;
            break;
        }
        if (edgeDistance < 0f || edgeDistance >= searchStart) return;

        Vector2 top = plateau.Center + direction * (edgeDistance - RampInset);
        Vector2 bottom = top + direction * (levelHeight / rules.RampGradient);
        if (!context.IsInsideMargin(bottom, context.Request.Layout.EdgeMargin)) return;

        float topHeight = heights.Sample(top);
        float bottomHeight = heights.Sample(bottom);
        if (topHeight - bottomHeight < levelHeight * 0.6f) return;
        if (bottomHeight < context.Request.Water.WaterLevel + MinDryClearance) return;

        heights.ApplyRamp(top, topHeight, bottom, bottomHeight, rules.RampWidth, rules.RampWidth * 0.4f);
        context.Ramps.Add(new MapRamp { Top = top, Bottom = bottom });
    }

    private static void AddWater(MapGenerationContext context, MapRandom random)
    {
        MapWaterRules rules = context.Request.Water;
        context.WaterMode = (MapWaterMode)random.PickWeighted(rules.NoneWeight, rules.LakesWeight, rules.RiverWeight, rules.CoastWeight);

        bool placed = true;
        switch (context.WaterMode)
        {
            case MapWaterMode.Lakes: placed = AddLakes(context, random); break;
            case MapWaterMode.River: placed = AddRiver(context, random); break;
            case MapWaterMode.Coast: placed = AddCoast(context, random); break;
        }

        if (!placed) context.WaterMode = MapWaterMode.None;
    }

    private static bool AddLakes(MapGenerationContext context, MapRandom random)
    {
        MapWaterRules rules = context.Request.Water;
        MapNoise shapeNoise = new MapNoise(random);
        float bottom = rules.WaterLevel - rules.Depth;
        int count = random.Range(rules.LakeCount);
        int placed = 0;

        for (int i = 0; i < count; i++)
        {
            for (int attempt = 0; attempt < PlacementTries; attempt++)
            {
                float radius = Mathf.Max(MinLakeRadius, random.Range(rules.LakeRadius) * context.Extent);
                Vector2 center = new Vector2(random.Range(radius, context.Width - radius), random.Range(radius, context.Depth - radius));
                if (!context.IsClearOfReservedZones(center, radius * 1.3f + 6f)) continue;
                if (context.Plateaus.Exists(plateau => Vector2.Distance(plateau.Center, center) < (plateau.Radius + radius) * 1.1f)) continue;

                Vector2 extent = new Vector2(radius, radius) * 1.3f;
                context.Heights.Modify(center - extent, center + extent, (x, z, height) =>
                {
                    float shapedRadius = radius * (0.7f + 0.6f * shapeNoise.Fractal(x * ShapeFrequency, z * ShapeFrequency, 2));
                    float distance = Vector2.Distance(new Vector2(x, z), center);
                    float weight = MapHeightField.GetFalloff(distance / shapedRadius, ShoreHardness) * context.GetReserveMask(x, z);
                    return weight <= 0f ? height : Mathf.Lerp(height, Mathf.Min(height, bottom), weight);
                });

                placed++;
                break;
            }
        }

        return placed > 0;
    }

    private static bool AddRiver(MapGenerationContext context, MapRandom random)
    {
        const int ControlPoints = 5;

        MapWaterRules rules = context.Request.Water;
        MapHeightField heights = context.Heights;
        float width = random.Range(rules.RiverWidth);
        float halfWidth = width / 2f;

        for (int attempt = 0; attempt < PlacementTries; attempt++)
        {
            float startDepth = random.Range(0.15f, 0.85f);
            float endDepth = Mathf.Clamp(startDepth + random.Range(-0.2f, 0.2f), 0.12f, 0.88f);

            List<Vector3> controls = new List<Vector3>();
            for (int i = 0; i < ControlPoints; i++)
            {
                float t = i / (float)(ControlPoints - 1);
                bool isInterior = i > 0 && i < ControlPoints - 1;
                float depth = Mathf.Lerp(startDepth, endDepth, t) + (isInterior ? random.Range(-0.1f, 0.1f) : 0f);
                Vector2 point = MapLayoutPlanner.GetPoint(context, context.Layout.EntryEdge, Mathf.Lerp(-0.1f, 1.1f, t), depth);
                controls.Add(new Vector3(point.x, 0f, point.y));
            }

            List<Vector2> line = ToPlanar(MapPath.GetSmoothedPoints(controls, 1f));
            if (line.Exists(point => !context.IsClearOfReservedZones(point, halfWidth + 8f))) continue;

            float reach = halfWidth + RiverShoreWidth;
            float bottom = rules.WaterLevel - rules.Depth;
            float[] distance = heights.CreateBuffer(float.MaxValue);
            heights.StampPolyline(line, reach, distance, null);

            for (int index = 0; index < heights.Values.Length; index++)
            {
                if (distance[index] >= reach) continue;

                float x = (index % heights.Width) / (float)MapHeightField.SamplesPerUnit;
                float z = (index / heights.Width) / (float)MapHeightField.SamplesPerUnit;
                float weight = MapHeightField.GetFalloff(distance[index] / reach, halfWidth * 0.6f / reach) * context.GetReserveMask(x, z);
                heights.Values[index] = Mathf.Lerp(heights.Values[index], Mathf.Min(heights.Values[index], bottom), weight);
            }

            AddFords(context, random, line, halfWidth);
            return true;
        }

        return false;
    }

    // Cada vado es una calzada que cruza el río perpendicular a su curso: un tramo plano apenas sobre el agua y una
    // rampa suave hacia cada orilla, para que la barranca del río no lo deje aislado
    private static void AddFords(MapGenerationContext context, MapRandom random, List<Vector2> riverLine, float halfWidth)
    {
        MapWaterRules rules = context.Request.Water;
        MapHeightField heights = context.Heights;
        float fordHeight = rules.WaterLevel + rules.FordClearance;
        float shoreDistance = halfWidth + RiverShoreWidth;
        float minSeparation = context.Extent * 0.15f;
        float margin = context.Request.Layout.EdgeMargin + shoreDistance + FordRampLength;
        int count = random.Range(rules.Fords);

        for (int i = 0; i < count; i++)
        {
            for (int attempt = 0; attempt < PlacementTries; attempt++)
            {
                int lineIndex = random.Range(1, riverLine.Count - 2);
                Vector2 center = riverLine[lineIndex];
                if (!context.IsInsideMargin(center, margin)) continue;
                if (context.Fords.Exists(ford => Vector2.Distance(ford, center) < minSeparation)) continue;

                Vector2 tangent = (riverLine[lineIndex + 1] - riverLine[lineIndex - 1]).normalized;
                Vector2 across = new Vector2(-tangent.y, tangent.x);
                Vector2 nearShore = center - across * shoreDistance;
                Vector2 farShore = center + across * shoreDistance;
                Vector2 nearBank = nearShore - across * FordRampLength;
                Vector2 farBank = farShore + across * FordRampLength;
                float nearBankHeight = Mathf.Max(fordHeight, heights.Sample(nearBank));
                float farBankHeight = Mathf.Max(fordHeight, heights.Sample(farBank));

                heights.ApplyRamp(nearShore, fordHeight, farShore, fordHeight, rules.FordWidth, FordBlend);
                heights.ApplyRamp(nearShore, fordHeight, nearBank, nearBankHeight, rules.FordWidth, FordBlend);
                heights.ApplyRamp(farShore, fordHeight, farBank, farBankHeight, rules.FordWidth, FordBlend);

                context.Fords.Add(center);
                break;
            }
        }
    }

    private static bool AddCoast(MapGenerationContext context, MapRandom random)
    {
        MapWaterRules rules = context.Request.Water;
        MapLayout layout = context.Layout;
        float depth = random.Range(rules.CoastDepth) * context.Extent;

        List<Vector2> markers = new List<Vector2>(layout.Spawns);
        if (layout.HasWaveEnd) markers.Add(layout.WaveEnd);

        List<MapEdge> freeEdges = new List<MapEdge>();
        foreach (MapEdge edge in new[] { MapEdge.South, MapEdge.East, MapEdge.North, MapEdge.West })
        {
            if (!markers.Exists(marker => MapLayoutPlanner.GetDistanceToEdge(context, edge, marker.x, marker.y) < depth * 1.4f + 12f)) freeEdges.Add(edge);
        }
        if (freeEdges.Count == 0) return false;

        MapEdge coastEdge = freeEdges[random.Index(freeEdges.Count)];
        float startDistance = MapLayoutPlanner.GetDistanceToEdge(context, coastEdge, layout.PlayerStart.x, layout.PlayerStart.y);
        depth = Mathf.Min(depth, (startDistance - context.Request.Start.ClearRadius - 10f) / 1.4f);
        if (depth < MinCoastDepth) return false;

        MapNoise shoreNoise = new MapNoise(random);
        float bottom = rules.WaterLevel - rules.Depth;
        bool runsAlongX = coastEdge == MapEdge.South || coastEdge == MapEdge.North;

        context.Heights.ModifyAll((x, z, height) =>
        {
            float edgeDistance = MapLayoutPlanner.GetDistanceToEdge(context, coastEdge, x, z);
            float shore = depth * (1f + (shoreNoise.Fractal((runsAlongX ? x : z) * 0.035f, 3.7f, 2) - 0.5f) * 1.4f);
            if (edgeDistance > shore + CoastShoreWidth) return height;

            float weight = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(shore + CoastShoreWidth, shore - CoastShoreWidth, edgeDistance)) * context.GetReserveMask(x, z);
            return Mathf.Lerp(height, Mathf.Min(height, bottom), weight);
        });

        return true;
    }

    public static void CarveRoutes(MapGenerationContext context)
    {
        foreach (MapRoute route in context.Routes)
        {
            CarveRoute(context, route);
        }
    }

    // Nivela el terreno bajo una ruta: perfil de alturas a lo largo de la polilínea (promedio móvil + pendiente
    // máxima), levantado a altura de vado donde cruza agua, y volcado con el mismo falloff que la rampa del editor.
    private static void CarveRoute(MapGenerationContext context, MapRoute route)
    {
        MapHeightField heights = context.Heights;
        List<Vector2> points = route.Smoothed;
        if (points.Count < 2) return;

        float fordHeight = context.Request.Water.WaterLevel + context.Request.Water.FordClearance;
        float[] sampled = new float[points.Count];
        int fordStart = -1;

        for (int i = 0; i < points.Count; i++)
        {
            float height = heights.Sample(points[i]);
            bool isWet = height < fordHeight;
            sampled[i] = isWet ? fordHeight : height;

            if (isWet && fordStart < 0) fordStart = i;
            if (!isWet && fordStart >= 0)
            {
                context.Fords.Add(points[(fordStart + i) / 2]);
                fordStart = -1;
            }
        }

        float[] profile = new float[points.Count];
        for (int i = 0; i < points.Count; i++)
        {
            float sum = 0f;
            int count = 0;
            for (int j = Mathf.Max(0, i - AverageWindow); j <= Mathf.Min(points.Count - 1, i + AverageWindow); j++)
            {
                sum += sampled[j];
                count++;
            }
            profile[i] = sum / count;
        }

        float maxGradient = context.Request.Path.MaxGradient;
        for (int i = 1; i < points.Count; i++)
        {
            float maxStep = maxGradient * Vector2.Distance(points[i], points[i - 1]);
            profile[i] = Mathf.Clamp(profile[i], profile[i - 1] - maxStep, profile[i - 1] + maxStep);
        }
        for (int i = points.Count - 2; i >= 0; i--)
        {
            float maxStep = maxGradient * Vector2.Distance(points[i], points[i + 1]);
            profile[i] = Mathf.Clamp(profile[i], profile[i + 1] - maxStep, profile[i + 1] + maxStep);
        }

        float halfWidth = context.PathWidth / 2f;
        float reach = context.PathWidth;
        float[] distance = heights.CreateBuffer(float.MaxValue);
        float[] parameter = heights.CreateBuffer(0f);
        heights.StampPolyline(points, reach, distance, parameter);

        for (int index = 0; index < heights.Values.Length; index++)
        {
            if (distance[index] >= reach) continue;

            int segment = Mathf.Min((int)parameter[index], points.Count - 2);
            float target = Mathf.Max(fordHeight, Mathf.Lerp(profile[segment], profile[segment + 1], parameter[index] - segment));
            float weight = MapHeightField.GetFalloff(distance[index] / reach, halfWidth / reach);
            heights.Values[index] = Mathf.Lerp(heights.Values[index], target, weight);
        }
    }

    public static List<Vector2> ToPlanar(List<Vector3> points)
    {
        List<Vector2> planar = new List<Vector2>(points.Count);
        foreach (Vector3 point in points)
        {
            planar.Add(new Vector2(point.x, point.z));
        }
        return planar;
    }
}
