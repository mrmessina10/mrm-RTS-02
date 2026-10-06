using UnityEngine;
using System.Collections.Generic;

public enum MapObjectCategory
{
    Resource,
    Nature,
    ManMade
}

public enum MapMarkerType
{
    EnemySpawn,
    WaveEnd,
    PlayerStart
}

[System.Serializable]
public struct MapObjectEntry
{
    public GameObject Prefab;
    public MapObjectCategory Category;
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 Scale;
}

[System.Serializable]
public struct MapMarkerEntry
{
    public MapMarkerType Type;
    public Vector3 Position;
    public Quaternion Rotation;
}

[System.Serializable]
public class MapPathEntry
{
    public float Width;
    public List<Vector3> Waypoints = new List<Vector3>();
}

// Formato de datos común de un mapa: terreno, objetos colocados, marcadores del loop de juego, caminos y máscara
// de celdas edificables. Lo escriben las herramientas de autoría (editor de mapas hoy, generador procedural después)
// y el gameplay solo lo lee.
public class MapDataSO : ScriptableObject
{
    [field: SerializeField] public Vector2Int Size { get; private set; } = new Vector2Int(128, 128);
    [field: SerializeField] public Vector3 Origin { get; private set; }
    [field: SerializeField] public TerrainData TerrainData { get; private set; }
    [field: SerializeField] public float WaterLevel { get; private set; } = -1f;

    [SerializeField] private List<MapObjectEntry> objects = new List<MapObjectEntry>();
    [SerializeField] private List<MapMarkerEntry> markers = new List<MapMarkerEntry>();
    [SerializeField] private List<MapPathEntry> paths = new List<MapPathEntry>();
    [SerializeField, HideInInspector] private byte[] blockedCells = new byte[0];

    public IReadOnlyList<MapObjectEntry> Objects => objects;
    public IReadOnlyList<MapMarkerEntry> Markers => markers;
    public IReadOnlyList<MapPathEntry> Paths => paths;

    public void Initialize(Vector2Int size, Vector3 origin, TerrainData terrainData, float waterLevel)
    {
        Size = size;
        Origin = origin;
        TerrainData = terrainData;
        WaterLevel = waterLevel;
        blockedCells = new byte[size.x * size.y];
    }

    public void SetWaterLevel(float waterLevel)
    {
        WaterLevel = waterLevel;
    }

    public void SetContents(List<MapObjectEntry> newObjects, List<MapMarkerEntry> newMarkers, List<MapPathEntry> newPaths)
    {
        objects = newObjects;
        markers = newMarkers;
        paths = newPaths;
    }

    public Vector2Int WorldToCell(Vector3 worldPosition)
    {
        return new Vector2Int(Mathf.FloorToInt(worldPosition.x - Origin.x), Mathf.FloorToInt(worldPosition.z - Origin.z));
    }

    public Vector3 CellToWorldCenter(Vector2Int cell)
    {
        return new Vector3(Origin.x + cell.x + 0.5f, 0f, Origin.z + cell.y + 0.5f);
    }

    public bool IsInsideMap(Vector2Int cell)
    {
        return cell.x >= 0 && cell.y >= 0 && cell.x < Size.x && cell.y < Size.y;
    }

    public bool IsCellBuildable(Vector2Int cell)
    {
        if (!IsInsideMap(cell)) return false;

        int index = cell.y * Size.x + cell.x;
        return index >= blockedCells.Length || blockedCells[index] == 0;
    }

    public bool IsBuildable(Vector3 worldPosition)
    {
        return IsCellBuildable(WorldToCell(worldPosition));
    }

    public void SetCellBuildable(Vector2Int cell, bool buildable)
    {
        if (!IsInsideMap(cell)) return;

        EnsureMaskSize();
        blockedCells[cell.y * Size.x + cell.x] = buildable ? (byte)0 : (byte)1;
    }

    public void SetAllBuildable(bool buildable)
    {
        EnsureMaskSize();

        byte value = buildable ? (byte)0 : (byte)1;
        for (int i = 0; i < blockedCells.Length; i++)
        {
            blockedCells[i] = value;
        }
    }

    private void EnsureMaskSize()
    {
        int expectedLength = Size.x * Size.y;
        if (blockedCells != null && blockedCells.Length == expectedLength) return;

        blockedCells = new byte[expectedLength];
    }
}
