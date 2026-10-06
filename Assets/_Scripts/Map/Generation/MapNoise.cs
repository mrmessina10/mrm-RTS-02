using UnityEngine;

// Ruido de valor 2D con interpolación suave y suma fractal (fBm), sembrado desde un MapRandom. Reemplaza a
// Mathf.PerlinNoise en el generador de mapas para que el resultado dependa solo de la semilla.
public class MapNoise
{
    private readonly uint seed;

    public MapNoise(MapRandom random)
    {
        seed = (uint)random.NextULong();
    }

    public float Sample(float x, float y)
    {
        int cellX = Mathf.FloorToInt(x);
        int cellY = Mathf.FloorToInt(y);
        float tx = Fade(x - cellX);
        float ty = Fade(y - cellY);

        float bottom = Mathf.Lerp(Hash(cellX, cellY), Hash(cellX + 1, cellY), tx);
        float top = Mathf.Lerp(Hash(cellX, cellY + 1), Hash(cellX + 1, cellY + 1), tx);
        return Mathf.Lerp(bottom, top, ty);
    }

    public float Fractal(float x, float y, int octaves)
    {
        float sum = 0f;
        float amplitude = 1f;
        float totalAmplitude = 0f;

        for (int i = 0; i < octaves; i++)
        {
            sum += Sample(x, y) * amplitude;
            totalAmplitude += amplitude;

            x = x * 2f + 17.3f;
            y = y * 2f + 31.7f;
            amplitude *= 0.5f;
        }

        return sum / totalAmplitude;
    }

    private static float Fade(float t)
    {
        return t * t * t * (t * (t * 6f - 15f) + 10f);
    }

    private float Hash(int x, int y)
    {
        uint hash = (uint)x * 374761393u + (uint)y * 668265263u + seed * 2246822519u;
        hash = (hash ^ (hash >> 13)) * 1274126177u;
        hash ^= hash >> 16;
        return (hash & 0xFFFFFF) / 16777215f;
    }
}
