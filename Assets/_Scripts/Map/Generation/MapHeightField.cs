using UnityEngine;
using System.Collections.Generic;

// Campo de alturas del generador de mapas: grilla regular en coordenadas locales del mapa (0..Size) con alturas en
// unidades de mundo. Replica sobre datos planos las operaciones de TerrainBrushUtility (falloff, rampa, nivelado a
// lo largo de una polilínea) para que el generador no dependa de un Terrain ni del editor.
public class MapHeightField
{
    public const int SamplesPerUnit = 2;

    public delegate float HeightOperation(float x, float z, float currentHeight);

    public int Width { get; }
    public int Depth { get; }
    public float[] Values { get; }

    public MapHeightField(Vector2Int sizeInCells)
    {
        Width = sizeInCells.x * SamplesPerUnit + 1;
        Depth = sizeInCells.y * SamplesPerUnit + 1;
        Values = new float[Width * Depth];
    }

    public static float GetFalloff(float normalizedDistance, float hardness)
    {
        if (normalizedDistance >= 1f) return 0f;
        if (normalizedDistance <= hardness) return 1f;

        float t = (normalizedDistance - hardness) / (1f - hardness);
        return 1f - Mathf.SmoothStep(0f, 1f, t);
    }

    public float Sample(Vector2 point)
    {
        return Sample(point.x, point.y);
    }

    public float Sample(float x, float z)
    {
        float sampleX = Mathf.Clamp(x * SamplesPerUnit, 0f, Width - 1);
        float sampleZ = Mathf.Clamp(z * SamplesPerUnit, 0f, Depth - 1);
        int minX = Mathf.Min((int)sampleX, Width - 2);
        int minZ = Mathf.Min((int)sampleZ, Depth - 2);
        int index = minZ * Width + minX;

        float bottom = Mathf.Lerp(Values[index], Values[index + 1], sampleX - minX);
        float top = Mathf.Lerp(Values[index + Width], Values[index + Width + 1], sampleX - minX);
        return Mathf.Lerp(bottom, top, sampleZ - minZ);
    }

    public float GetCellHeight(int cellX, int cellZ)
    {
        return Sample(cellX + 0.5f, cellZ + 0.5f);
    }

    public float GetCellSlope(int cellX, int cellZ)
    {
        float maxGradientSquared = 0f;

        for (int subZ = 0; subZ < SamplesPerUnit; subZ++)
        {
            for (int subX = 0; subX < SamplesPerUnit; subX++)
            {
                int index = (cellZ * SamplesPerUnit + subZ) * Width + cellX * SamplesPerUnit + subX;
                float h00 = Values[index];
                float h10 = Values[index + 1];
                float h01 = Values[index + Width];
                float h11 = Values[index + Width + 1];

                float gradientX = (h10 + h11 - h00 - h01) * 0.5f * SamplesPerUnit;
                float gradientZ = (h01 + h11 - h00 - h10) * 0.5f * SamplesPerUnit;
                maxGradientSquared = Mathf.Max(maxGradientSquared, gradientX * gradientX + gradientZ * gradientZ);
            }
        }

        return Mathf.Atan(Mathf.Sqrt(maxGradientSquared)) * Mathf.Rad2Deg;
    }

    public float[] CreateBuffer(float value)
    {
        float[] buffer = new float[Values.Length];
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = value;
        }
        return buffer;
    }

    public void ModifyAll(HeightOperation operation)
    {
        Modify(Vector2.zero, new Vector2(Width, Depth), operation);
    }

    public void Modify(Vector2 min, Vector2 max, HeightOperation operation)
    {
        int minX = Mathf.Clamp(Mathf.FloorToInt(min.x * SamplesPerUnit), 0, Width - 1);
        int maxX = Mathf.Clamp(Mathf.CeilToInt(max.x * SamplesPerUnit), 0, Width - 1);
        int minZ = Mathf.Clamp(Mathf.FloorToInt(min.y * SamplesPerUnit), 0, Depth - 1);
        int maxZ = Mathf.Clamp(Mathf.CeilToInt(max.y * SamplesPerUnit), 0, Depth - 1);

        for (int z = minZ; z <= maxZ; z++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                int index = z * Width + x;
                Values[index] = operation(x / (float)SamplesPerUnit, z / (float)SamplesPerUnit, Values[index]);
            }
        }
    }

    public void ApplyRamp(Vector2 from, float fromHeight, Vector2 to, float toHeight, float width, float edgeBlend)
    {
        Vector2 segment = to - from;
        float segmentLengthSquared = segment.sqrMagnitude;
        if (segmentLengthSquared < 0.0001f) return;

        float halfWidth = width / 2f;
        float reach = halfWidth + edgeBlend;
        Vector2 extent = new Vector2(reach, reach);

        Modify(Vector2.Min(from, to) - extent, Vector2.Max(from, to) + extent, (x, z, currentHeight) =>
        {
            Vector2 point = new Vector2(x, z);
            float t = Mathf.Clamp01(Vector2.Dot(point - from, segment) / segmentLengthSquared);
            float distance = Vector2.Distance(point, from + segment * t);
            float weight = GetFalloff(distance / reach, halfWidth / reach);
            return weight <= 0f ? currentHeight : Mathf.Lerp(currentHeight, Mathf.Lerp(fromHeight, toHeight, t), weight);
        });
    }

    public void StampPolyline(IReadOnlyList<Vector2> points, float reach, float[] distance, float[] parameter)
    {
        StampPolyline(points, reach, 1f / SamplesPerUnit, 0f, Width, Depth, distance, parameter);
    }

    // Para cada muestra de una grilla (posición = offset + índice * step) guarda la distancia a la polilínea si es
    // menor a la que ya había, y opcionalmente el parámetro del punto más cercano (índice de tramo + fracción).
    public static void StampPolyline(IReadOnlyList<Vector2> points, float reach, float step, float offset, int width, int depth, float[] distance, float[] parameter)
    {
        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector2 from = points[i];
            Vector2 to = points[i + 1];
            Vector2 segment = to - from;
            float segmentLengthSquared = segment.sqrMagnitude;

            int minX = Mathf.Clamp(Mathf.FloorToInt((Mathf.Min(from.x, to.x) - reach - offset) / step), 0, width - 1);
            int maxX = Mathf.Clamp(Mathf.CeilToInt((Mathf.Max(from.x, to.x) + reach - offset) / step), 0, width - 1);
            int minZ = Mathf.Clamp(Mathf.FloorToInt((Mathf.Min(from.y, to.y) - reach - offset) / step), 0, depth - 1);
            int maxZ = Mathf.Clamp(Mathf.CeilToInt((Mathf.Max(from.y, to.y) + reach - offset) / step), 0, depth - 1);

            for (int z = minZ; z <= maxZ; z++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    Vector2 point = new Vector2(offset + x * step, offset + z * step);
                    float t = segmentLengthSquared > 0.0001f ? Mathf.Clamp01(Vector2.Dot(point - from, segment) / segmentLengthSquared) : 0f;
                    float pointDistance = Vector2.Distance(point, from + segment * t);

                    int index = z * width + x;
                    if (pointDistance >= distance[index]) continue;

                    distance[index] = pointDistance;
                    if (parameter != null) parameter[index] = i + t;
                }
            }
        }
    }
}
