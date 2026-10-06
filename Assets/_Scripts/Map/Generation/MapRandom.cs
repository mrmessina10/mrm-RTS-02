using UnityEngine;

// Generador pseudoaleatorio determinista (SplitMix64) del generador de mapas: la misma semilla y el mismo stream
// producen la misma secuencia en cualquier plataforma, sin tocar el estado global de UnityEngine.Random.
public class MapRandom
{
    private const ulong Increment = 0x9E3779B97F4A7C15UL;

    private ulong state;

    public MapRandom(int seed, int stream = 0)
    {
        state = ((ulong)(uint)seed << 32) | (uint)stream;
        NextULong();
    }

    public float Value => (NextULong() >> 40) / 16777216f;

    public ulong NextULong()
    {
        state += Increment;

        ulong z = state;
        z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
        z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
        return z ^ (z >> 31);
    }

    public float Range(float min, float max)
    {
        return min + (max - min) * Value;
    }

    public float Range(Vector2 range)
    {
        return Range(range.x, range.y);
    }

    public int Range(int minInclusive, int maxInclusive)
    {
        if (maxInclusive <= minInclusive) return minInclusive;
        return minInclusive + (int)(NextULong() % (ulong)(maxInclusive - minInclusive + 1));
    }

    public int Range(Vector2Int range)
    {
        return Range(range.x, range.y);
    }

    public int Index(int count)
    {
        return (int)(NextULong() % (ulong)count);
    }

    public bool Chance(float probability)
    {
        return Value < probability;
    }

    public Vector2 Direction()
    {
        float angle = Range(0f, Mathf.PI * 2f);
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }

    public Vector2 InsideUnitCircle()
    {
        float radius = Mathf.Sqrt(Value);
        return Direction() * radius;
    }

    public int PickWeighted(params float[] weights)
    {
        float total = 0f;
        foreach (float weight in weights)
        {
            total += Mathf.Max(0f, weight);
        }
        if (total <= 0f) return 0;

        float roll = Value * total;
        for (int i = 0; i < weights.Length; i++)
        {
            roll -= Mathf.Max(0f, weights[i]);
            if (roll < 0f) return i;
        }
        return weights.Length - 1;
    }
}
