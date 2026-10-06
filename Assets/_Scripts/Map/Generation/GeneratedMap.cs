using UnityEngine;
using System.Collections.Generic;

public struct GeneratedMapObject
{
    public int CatalogIndex;
    public Vector3 Position;
    public float Yaw;
    public float Scale;
}

public struct GeneratedMapMarker
{
    public MapMarkerType Type;
    public Vector3 Position;
}

public class GeneratedMapPath
{
    public float Width;
    public List<Vector3> Waypoints = new List<Vector3>();
}

public enum MapGroundPatch
{
    Dirt,
    Sand
}

public struct GeneratedTexturePatch
{
    public MapGroundPatch Ground;
    public Vector3 Center;
    public float Radius;
    public float Opacity;
}

public struct MapRuleResult
{
    public string Rule;
    public bool Passed;
    public bool Required;
    public string Detail;
}

// Resultado de una corrida del generador, en posiciones de mundo: relieve, objetos, marcadores, caminos, máscara no
// edificable y el informe de reglas. Es dato plano; quien lo aplica (editor hoy) lo vuelca al Terrain y al MapDataSO.
public class GeneratedMap
{
    public int Seed;
    public int Attempt;
    public int AttemptsUsed;
    public Vector2Int Size;
    public Vector3 Origin;
    public float WaterLevel;
    public float PathWidth;
    public MapLayoutType Layout;
    public MapWaterMode WaterMode;
    public int PlateauCount;

    public MapHeightField Heights;
    public MapGenerationCatalog Catalog;
    public List<GeneratedMapObject> Objects = new List<GeneratedMapObject>();
    public List<GeneratedMapMarker> Markers = new List<GeneratedMapMarker>();
    public List<GeneratedMapPath> Paths = new List<GeneratedMapPath>();
    public List<GeneratedTexturePatch> TexturePatches = new List<GeneratedTexturePatch>();
    public List<MapRuleResult> Rules = new List<MapRuleResult>();
    public List<MapRuleResult> DiscardedRules = new List<MapRuleResult>();

    public bool[] NotBuildableCells;
    public bool[] WalkableCells;
    public bool[] ReachableCells;
    public float[] PathDistance;

    public int FailedRequiredRules
    {
        get
        {
            int failed = 0;
            foreach (MapRuleResult rule in Rules)
            {
                if (rule.Required && !rule.Passed) failed++;
            }
            return failed;
        }
    }

    public bool IsValid => FailedRequiredRules == 0;

    public int CountMarkers(MapMarkerType type)
    {
        int count = 0;
        foreach (GeneratedMapMarker marker in Markers)
        {
            if (marker.Type == type) count++;
        }
        return count;
    }

    public int CountResourceNodes(ResourceType type)
    {
        int count = 0;
        foreach (GeneratedMapObject mapObject in Objects)
        {
            MapGenerationCatalogEntry entry = Catalog.Entries[mapObject.CatalogIndex];
            if (entry.IsResource && entry.ResourceType == type) count++;
        }
        return count;
    }
}
