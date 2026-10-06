using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

// Operaciones de pincel sobre el heightmap y el splatmap de un Terrain, en coordenadas y alturas de mundo.
// Es la única clase del editor de mapas que toca TerrainData.GetHeights/SetHeights/GetAlphamaps/SetAlphamaps.
public static class TerrainBrushUtility
{
    public delegate float HeightOperation(float worldX, float worldZ, float currentHeight);

    public static float GetFalloff(float normalizedDistance, float hardness)
    {
        return MapHeightField.GetFalloff(normalizedDistance, hardness);
    }

    public static void RegisterHeightUndo(Terrain terrain, string undoName)
    {
        Undo.RegisterCompleteObjectUndo(terrain.terrainData, undoName);
    }

    public static void RegisterAlphamapUndo(Terrain terrain, string undoName)
    {
        Undo.RegisterCompleteObjectUndo(terrain.terrainData.alphamapTextures, undoName);
    }

    public static void FinishHeightEdit(Terrain terrain)
    {
        terrain.terrainData.SyncHeightmap();
        EditorUtility.SetDirty(terrain.terrainData);
    }

    public static void FinishAlphamapEdit(Terrain terrain)
    {
        EditorUtility.SetDirty(terrain.terrainData);
    }

    public static void ModifyHeights(Terrain terrain, Vector2 minXZ, Vector2 maxXZ, HeightOperation operation)
    {
        TerrainData data = terrain.terrainData;
        Vector3 terrainPosition = terrain.transform.position;
        int resolution = data.heightmapResolution;
        float samplesPerUnitX = (resolution - 1) / data.size.x;
        float samplesPerUnitZ = (resolution - 1) / data.size.z;

        int minX = Mathf.Clamp(Mathf.FloorToInt((minXZ.x - terrainPosition.x) * samplesPerUnitX), 0, resolution - 1);
        int maxX = Mathf.Clamp(Mathf.CeilToInt((maxXZ.x - terrainPosition.x) * samplesPerUnitX), 0, resolution - 1);
        int minZ = Mathf.Clamp(Mathf.FloorToInt((minXZ.y - terrainPosition.z) * samplesPerUnitZ), 0, resolution - 1);
        int maxZ = Mathf.Clamp(Mathf.CeilToInt((maxXZ.y - terrainPosition.z) * samplesPerUnitZ), 0, resolution - 1);

        int width = maxX - minX + 1;
        int depth = maxZ - minZ + 1;
        if (width <= 0 || depth <= 0) return;

        float[,] heights = data.GetHeights(minX, minZ, width, depth);
        float heightRange = data.size.y;

        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                float worldX = terrainPosition.x + (minX + x) / samplesPerUnitX;
                float worldZ = terrainPosition.z + (minZ + z) / samplesPerUnitZ;
                float currentHeight = terrainPosition.y + heights[z, x] * heightRange;

                float newHeight = operation(worldX, worldZ, currentHeight);
                heights[z, x] = Mathf.Clamp01((newHeight - terrainPosition.y) / heightRange);
            }
        }

        data.SetHeightsDelayLOD(minX, minZ, heights);
    }

    public static void ModifyHeightsInBrush(Terrain terrain, Vector3 center, float radius, float hardness, System.Func<float, float, float> weightedOperation)
    {
        Vector2 centerXZ = new Vector2(center.x, center.z);
        Vector2 extent = new Vector2(radius, radius);

        ModifyHeights(terrain, centerXZ - extent, centerXZ + extent, (worldX, worldZ, currentHeight) =>
        {
            float distance = Vector2.Distance(new Vector2(worldX, worldZ), centerXZ);
            float weight = GetFalloff(distance / radius, hardness);
            return weight <= 0f ? currentHeight : weightedOperation(currentHeight, weight);
        });
    }

    public static void SmoothHeights(Terrain terrain, Vector3 center, float radius, float hardness, float amount)
    {
        TerrainData data = terrain.terrainData;
        Vector3 terrainPosition = terrain.transform.position;
        int resolution = data.heightmapResolution;
        float samplesPerUnitX = (resolution - 1) / data.size.x;
        float samplesPerUnitZ = (resolution - 1) / data.size.z;

        int minX = Mathf.Clamp(Mathf.FloorToInt((center.x - radius - terrainPosition.x) * samplesPerUnitX), 0, resolution - 1);
        int maxX = Mathf.Clamp(Mathf.CeilToInt((center.x + radius - terrainPosition.x) * samplesPerUnitX), 0, resolution - 1);
        int minZ = Mathf.Clamp(Mathf.FloorToInt((center.z - radius - terrainPosition.z) * samplesPerUnitZ), 0, resolution - 1);
        int maxZ = Mathf.Clamp(Mathf.CeilToInt((center.z + radius - terrainPosition.z) * samplesPerUnitZ), 0, resolution - 1);

        int width = maxX - minX + 1;
        int depth = maxZ - minZ + 1;
        if (width <= 0 || depth <= 0) return;

        float[,] source = data.GetHeights(minX, minZ, width, depth);
        float[,] result = (float[,])source.Clone();

        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                float worldX = terrainPosition.x + (minX + x) / samplesPerUnitX;
                float worldZ = terrainPosition.z + (minZ + z) / samplesPerUnitZ;
                float distance = Vector2.Distance(new Vector2(worldX, worldZ), new Vector2(center.x, center.z));
                float weight = GetFalloff(distance / radius, hardness);
                if (weight <= 0f) continue;

                float sum = 0f;
                int count = 0;
                for (int offsetZ = -1; offsetZ <= 1; offsetZ++)
                {
                    for (int offsetX = -1; offsetX <= 1; offsetX++)
                    {
                        int sampleX = Mathf.Clamp(x + offsetX, 0, width - 1);
                        int sampleZ = Mathf.Clamp(z + offsetZ, 0, depth - 1);
                        sum += source[sampleZ, sampleX];
                        count++;
                    }
                }

                result[z, x] = Mathf.Lerp(source[z, x], sum / count, weight * amount);
            }
        }

        data.SetHeightsDelayLOD(minX, minZ, result);
    }

    public static void ApplyRamp(Terrain terrain, Vector3 from, Vector3 to, float width, float edgeBlend)
    {
        Vector2 fromXZ = new Vector2(from.x, from.z);
        Vector2 toXZ = new Vector2(to.x, to.z);
        Vector2 segment = toXZ - fromXZ;
        float segmentLengthSquared = segment.sqrMagnitude;
        if (segmentLengthSquared < 0.0001f) return;

        float halfWidth = width / 2f;
        float reach = halfWidth + edgeBlend;
        Vector2 extent = new Vector2(reach, reach);

        ModifyHeights(terrain, Vector2.Min(fromXZ, toXZ) - extent, Vector2.Max(fromXZ, toXZ) + extent, (worldX, worldZ, currentHeight) =>
        {
            Vector2 point = new Vector2(worldX, worldZ);
            float t = Mathf.Clamp01(Vector2.Dot(point - fromXZ, segment) / segmentLengthSquared);
            float distance = Vector2.Distance(point, fromXZ + segment * t);
            float weight = GetFalloff(distance / reach, halfWidth / reach);
            return weight <= 0f ? currentHeight : Mathf.Lerp(currentHeight, Mathf.Lerp(from.y, to.y, t), weight);
        });
    }

    public static void LevelAlongPolyline(Terrain terrain, List<Vector3> points, float width, float edgeBlend)
    {
        const int AverageWindow = 4;
        if (points.Count < 2) return;

        float[] sampledHeights = new float[points.Count];
        for (int i = 0; i < points.Count; i++)
        {
            sampledHeights[i] = terrain.SampleHeight(points[i]) + terrain.transform.position.y;
        }

        List<Vector3> leveledPoints = new List<Vector3>(points.Count);
        for (int i = 0; i < points.Count; i++)
        {
            float sum = 0f;
            int count = 0;
            for (int j = Mathf.Max(0, i - AverageWindow); j <= Mathf.Min(points.Count - 1, i + AverageWindow); j++)
            {
                sum += sampledHeights[j];
                count++;
            }
            leveledPoints.Add(new Vector3(points[i].x, sum / count, points[i].z));
        }

        for (int i = 0; i < leveledPoints.Count - 1; i++)
        {
            ApplyRamp(terrain, leveledPoints[i], leveledPoints[i + 1], width, edgeBlend);
        }
    }

    public static int FindLayerIndex(TerrainData data, string layerName)
    {
        TerrainLayer[] layers = data.terrainLayers;
        for (int i = 0; i < layers.Length; i++)
        {
            if (layers[i] != null && layers[i].name == layerName) return i;
        }
        return -1;
    }

    public static void PaintLayer(Terrain terrain, Vector3 center, float radius, float hardness, int layerIndex, float opacity)
    {
        TerrainData data = terrain.terrainData;
        if (layerIndex < 0 || layerIndex >= data.alphamapLayers) return;

        Vector3 terrainPosition = terrain.transform.position;
        float texelsPerUnitX = data.alphamapWidth / data.size.x;
        float texelsPerUnitZ = data.alphamapHeight / data.size.z;

        int minX = Mathf.Clamp(Mathf.FloorToInt((center.x - radius - terrainPosition.x) * texelsPerUnitX), 0, data.alphamapWidth - 1);
        int maxX = Mathf.Clamp(Mathf.CeilToInt((center.x + radius - terrainPosition.x) * texelsPerUnitX), 0, data.alphamapWidth - 1);
        int minZ = Mathf.Clamp(Mathf.FloorToInt((center.z - radius - terrainPosition.z) * texelsPerUnitZ), 0, data.alphamapHeight - 1);
        int maxZ = Mathf.Clamp(Mathf.CeilToInt((center.z + radius - terrainPosition.z) * texelsPerUnitZ), 0, data.alphamapHeight - 1);

        int width = maxX - minX + 1;
        int depth = maxZ - minZ + 1;
        if (width <= 0 || depth <= 0) return;

        float[,,] alphamaps = data.GetAlphamaps(minX, minZ, width, depth);
        int layerCount = data.alphamapLayers;

        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                float worldX = terrainPosition.x + (minX + x + 0.5f) / texelsPerUnitX;
                float worldZ = terrainPosition.z + (minZ + z + 0.5f) / texelsPerUnitZ;
                float distance = Vector2.Distance(new Vector2(worldX, worldZ), new Vector2(center.x, center.z));
                float weight = GetFalloff(distance / radius, hardness) * opacity;
                if (weight <= 0f) continue;

                float current = alphamaps[z, x, layerIndex];
                float target = Mathf.Lerp(current, 1f, weight);
                float othersSum = 1f - current;

                for (int layer = 0; layer < layerCount; layer++)
                {
                    if (layer == layerIndex) alphamaps[z, x, layer] = target;
                    else alphamaps[z, x, layer] = othersSum > 0.0001f ? alphamaps[z, x, layer] * (1f - target) / othersSum : 0f;
                }
            }
        }

        data.SetAlphamaps(minX, minZ, alphamaps);
    }

    public static void FillLayer(Terrain terrain, int layerIndex)
    {
        TerrainData data = terrain.terrainData;
        if (layerIndex < 0 || layerIndex >= data.alphamapLayers) return;

        float[,,] alphamaps = new float[data.alphamapHeight, data.alphamapWidth, data.alphamapLayers];
        for (int z = 0; z < data.alphamapHeight; z++)
        {
            for (int x = 0; x < data.alphamapWidth; x++)
            {
                alphamaps[z, x, layerIndex] = 1f;
            }
        }

        data.SetAlphamaps(0, 0, alphamaps);
    }

    // Reparte las capas según el relieve: roca en pendientes fuertes, arena cerca del nivel de agua y la capa base
    // en el resto. La capa preservada (caminos pintados a mano) conserva su peso.
    public static void AutoTexture(Terrain terrain, float waterLevel, int baseLayer, int rockLayer, int sandLayer, int preservedLayer)
    {
        const float RockStartAngle = 28f;
        const float RockFullAngle = 42f;
        const float SandBand = 0.7f;

        TerrainData data = terrain.terrainData;
        float terrainY = terrain.transform.position.y;
        int layerCount = data.alphamapLayers;
        float[,,] alphamaps = data.GetAlphamaps(0, 0, data.alphamapWidth, data.alphamapHeight);

        for (int z = 0; z < data.alphamapHeight; z++)
        {
            for (int x = 0; x < data.alphamapWidth; x++)
            {
                float u = (x + 0.5f) / data.alphamapWidth;
                float v = (z + 0.5f) / data.alphamapHeight;

                float steepness = data.GetSteepness(u, v);
                float height = terrainY + data.GetInterpolatedHeight(u, v);

                float preserved = preservedLayer >= 0 ? alphamaps[z, x, preservedLayer] : 0f;
                float rock = rockLayer >= 0 ? Mathf.InverseLerp(RockStartAngle, RockFullAngle, steepness) : 0f;
                float sand = sandLayer >= 0 ? (1f - rock) * Mathf.InverseLerp(waterLevel + SandBand, waterLevel, height) : 0f;
                float remaining = 1f - preserved;

                for (int layer = 0; layer < layerCount; layer++)
                {
                    alphamaps[z, x, layer] = 0f;
                }

                if (preservedLayer >= 0) alphamaps[z, x, preservedLayer] = preserved;
                if (rockLayer >= 0) alphamaps[z, x, rockLayer] += rock * remaining;
                if (sandLayer >= 0) alphamaps[z, x, sandLayer] += sand * remaining;
                alphamaps[z, x, baseLayer] += (1f - rock - sand) * remaining;
            }
        }

        data.SetAlphamaps(0, 0, alphamaps);
    }
}
