using UnityEngine;
using System.Collections.Generic;

public struct MapReservedZone
{
    public Vector2 Center;
    public float Radius;
}

public struct MapRamp
{
    public Vector2 Top;
    public Vector2 Bottom;
}

public class MapPlateau
{
    public Vector2 Center;
    public float Radius;
    public float TopHeight;
    public float BaseHeight;
}

public class MapRoute
{
    public List<Vector2Int> Cells = new List<Vector2Int>();
    public List<Vector2> Waypoints = new List<Vector2>();
    public List<int> WaypointCells = new List<int>();
    public List<Vector2> Smoothed = new List<Vector2>();
    public int SpawnIndex = -1;
    public int LengthToDefense;
    public int SharedStretch;
}

public class MapResourceCluster
{
    public ResourceType Type;
    public bool IsStarter;
    public Vector2 Center;
    public float Radius;
    public int NodeCount;
    public Vector2 DropOffSpot;
}

// Estado de trabajo de un intento de generación, en coordenadas locales del mapa (0..Size, 1 unidad = 1 celda).
// Las etapas lo van completando en orden (layout, relieve, caminos, objetos, reglas) y de acá sale el GeneratedMap.
public class MapGenerationContext
{
    public const float PathDistanceReach = 20f;

    private const float ReserveBlend = 8f;

    public MapGenerationRequest Request { get; }
    public int Attempt { get; }
    public int Width { get; }
    public int Depth { get; }
    public float Extent { get; }
    public MapHeightField Heights { get; }

    public MapLayout Layout;
    public MapWaterMode WaterMode;
    public float PathWidth;
    public int DefenseCellIndex;

    public readonly List<MapReservedZone> ReservedZones = new List<MapReservedZone>();
    public readonly List<MapPlateau> Plateaus = new List<MapPlateau>();
    public readonly List<MapRamp> Ramps = new List<MapRamp>();
    public readonly List<Vector2> Fords = new List<Vector2>();
    public readonly List<MapRoute> Routes = new List<MapRoute>();
    public readonly List<MapResourceCluster> Clusters = new List<MapResourceCluster>();
    public readonly List<GeneratedMapObject> Objects = new List<GeneratedMapObject>();
    public readonly List<GeneratedTexturePatch> TexturePatches = new List<GeneratedTexturePatch>();
    public readonly List<ResourceType> MissingResourceTypes = new List<ResourceType>();

    public readonly float[] CellSlope;
    public readonly float[] CellHeight;
    public readonly float[] PathDistance;
    public readonly bool[] TerrainBlocked;
    public readonly bool[] Obstacle;
    public readonly bool[] KeepClear;
    public readonly bool[] NotBuildable;
    public readonly bool[] Reachable;

    public MapGenerationContext(MapGenerationRequest request, int attempt)
    {
        Request = request;
        Attempt = attempt;
        Width = request.Size.x;
        Depth = request.Size.y;
        Extent = Mathf.Min(Width, Depth);
        Heights = new MapHeightField(request.Size);

        int cellCount = Width * Depth;
        CellSlope = new float[cellCount];
        CellHeight = new float[cellCount];
        PathDistance = new float[cellCount];
        TerrainBlocked = new bool[cellCount];
        Obstacle = new bool[cellCount];
        KeepClear = new bool[cellCount];
        NotBuildable = new bool[cellCount];
        Reachable = new bool[cellCount];
    }

    public MapRandom CreateRandom(int stage)
    {
        return new MapRandom(Request.Seed, Attempt * 64 + stage);
    }

    public int CellIndex(int x, int z)
    {
        return z * Width + x;
    }

    public int CellIndex(Vector2 point)
    {
        Vector2Int cell = ToCell(point);
        return cell.y * Width + cell.x;
    }

    public Vector2Int ToCell(Vector2 point)
    {
        return new Vector2Int(Mathf.Clamp(Mathf.FloorToInt(point.x), 0, Width - 1), Mathf.Clamp(Mathf.FloorToInt(point.y), 0, Depth - 1));
    }

    public static Vector2 CellCenter(Vector2Int cell)
    {
        return new Vector2(cell.x + 0.5f, cell.y + 0.5f);
    }

    public bool IsInsideMargin(Vector2 point, float margin)
    {
        return point.x >= margin && point.y >= margin && point.x <= Width - margin && point.y <= Depth - margin;
    }

    public Vector2 ClampToMargin(Vector2 point, float margin)
    {
        return new Vector2(Mathf.Clamp(point.x, margin, Width - margin), Mathf.Clamp(point.y, margin, Depth - margin));
    }

    public bool IsWalkable(int cellIndex)
    {
        return !TerrainBlocked[cellIndex] && !Obstacle[cellIndex];
    }

    public Vector3 ToWorld(Vector2 point)
    {
        return new Vector3(Request.Origin.x + point.x, Heights.Sample(point), Request.Origin.z + point.y);
    }

    public void AddReservedZone(Vector2 center, float radius)
    {
        ReservedZones.Add(new MapReservedZone { Center = center, Radius = radius });
    }

    // 0 dentro de una zona reservada (inicio, spawns, fin de oleada), 1 lejos de todas: atenúa relieve y agua
    public float GetReserveMask(float x, float z)
    {
        Vector2 point = new Vector2(x, z);
        float mask = 1f;

        foreach (MapReservedZone zone in ReservedZones)
        {
            float distance = Vector2.Distance(point, zone.Center);
            mask = Mathf.Min(mask, Mathf.SmoothStep(0f, 1f, (distance - zone.Radius) / ReserveBlend));
        }
        return mask;
    }

    public bool IsClearOfReservedZones(Vector2 center, float clearance)
    {
        foreach (MapReservedZone zone in ReservedZones)
        {
            if (Vector2.Distance(center, zone.Center) < zone.Radius + clearance) return false;
        }
        return true;
    }

    public void MarkDisc(bool[] grid, Vector2 center, float radius)
    {
        ForEachCellInDisc(center, radius, index => grid[index] = true);
    }

    public float GetDiscRatio(Vector2 center, float radius, System.Func<int, bool> predicate)
    {
        int total = 0;
        int matching = 0;

        int reach = Mathf.CeilToInt(radius);
        Vector2Int centerCell = ToCell(center);
        for (int z = centerCell.y - reach; z <= centerCell.y + reach; z++)
        {
            for (int x = centerCell.x - reach; x <= centerCell.x + reach; x++)
            {
                if (Vector2.Distance(new Vector2(x + 0.5f, z + 0.5f), center) > radius) continue;

                total++;
                if (x >= 0 && z >= 0 && x < Width && z < Depth && predicate(CellIndex(x, z))) matching++;
            }
        }
        return total > 0 ? matching / (float)total : 0f;
    }

    public void RefreshTerrainGrids()
    {
        float maxSlope = Request.Terrain.MaxWalkableSlope;
        float waterLevel = Request.Water.WaterLevel;

        for (int z = 0; z < Depth; z++)
        {
            for (int x = 0; x < Width; x++)
            {
                int index = CellIndex(x, z);
                CellSlope[index] = Heights.GetCellSlope(x, z);
                CellHeight[index] = Heights.GetCellHeight(x, z);
                TerrainBlocked[index] = CellSlope[index] > maxSlope || CellHeight[index] < waterLevel;
            }
        }
    }

    // Marca las celdas alcanzables a pie desde el inicio del jugador (relleno por inundación, vecindad de 4)
    public void FloodFillReachable(bool includeObstacles)
    {
        System.Array.Clear(Reachable, 0, Reachable.Length);

        int startIndex = CellIndex(Layout.PlayerStart);
        if (TerrainBlocked[startIndex]) return;

        Queue<int> pending = new Queue<int>();
        Reachable[startIndex] = true;
        pending.Enqueue(startIndex);

        while (pending.Count > 0)
        {
            int index = pending.Dequeue();
            int x = index % Width;
            int z = index / Width;

            if (x > 0) Visit(index - 1, includeObstacles, pending);
            if (x < Width - 1) Visit(index + 1, includeObstacles, pending);
            if (z > 0) Visit(index - Width, includeObstacles, pending);
            if (z < Depth - 1) Visit(index + Width, includeObstacles, pending);
        }
    }

    private void Visit(int index, bool includeObstacles, Queue<int> pending)
    {
        if (Reachable[index] || TerrainBlocked[index]) return;
        if (includeObstacles && Obstacle[index]) return;

        Reachable[index] = true;
        pending.Enqueue(index);
    }

    private void ForEachCellInDisc(Vector2 center, float radius, System.Action<int> action)
    {
        int reach = Mathf.CeilToInt(radius);
        Vector2Int centerCell = ToCell(center);

        for (int z = Mathf.Max(0, centerCell.y - reach); z <= Mathf.Min(Depth - 1, centerCell.y + reach); z++)
        {
            for (int x = Mathf.Max(0, centerCell.x - reach); x <= Mathf.Min(Width - 1, centerCell.x + reach); x++)
            {
                if (Vector2.Distance(new Vector2(x + 0.5f, z + 0.5f), center) <= radius) action(CellIndex(x, z));
            }
        }
    }
}
