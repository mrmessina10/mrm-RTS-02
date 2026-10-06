using UnityEngine;
using System.Collections.Generic;

public class MapGenerationCatalogEntry
{
    public GameObject Prefab;
    public MapObjectCategory Category;
    public float Spacing = 1.5f;
    public Vector2 ScaleRange = Vector2.one;
    public float VerticalOffset;
    public bool RandomYaw = true;
    public bool BlocksMovement;
    public bool IsResource;
    public ResourceType ResourceType;
}

// Catálogo de lo que el generador puede colocar, ya resuelto a datos planos: parámetros de pincel de la paleta,
// tipo de recurso de cada nodo (con una entrada por cada variante de aspecto) y si el prefab bloquea el paso. Las etapas del generador trabajan con índices de
// este catálogo y nunca tocan los prefabs.
public class MapGenerationCatalog
{
    public List<MapGenerationCatalogEntry> Entries { get; } = new List<MapGenerationCatalogEntry>();
    public List<List<int>> ScatterEntries { get; } = new List<List<int>>();
    public List<List<int>> LandmarkEntries { get; } = new List<List<int>>();

    public static MapGenerationCatalog Build(MapPaletteSO palette, IReadOnlyList<MapScatterRule> scatterRules, IReadOnlyList<MapLandmarkRule> landmarkRules)
    {
        MapGenerationCatalog catalog = new MapGenerationCatalog();

        if (palette != null)
        {
            foreach (MapPaletteEntry paletteEntry in palette.Entries)
            {
                if (paletteEntry.Category != MapObjectCategory.Resource) continue;

                foreach (GameObject prefab in paletteEntry.GetPrefabs())
                {
                    if (prefab.GetComponent<IHarvestable>() == null || catalog.Entries.Exists(entry => entry.Prefab == prefab)) continue;

                    catalog.Entries.Add(CreateEntry(prefab, palette, MapObjectCategory.Resource));
                }
            }
        }

        foreach (MapScatterRule rule in scatterRules)
        {
            catalog.ScatterEntries.Add(catalog.AddPrefabs(rule.Prefabs, palette, MapObjectCategory.Nature));
        }

        foreach (MapLandmarkRule rule in landmarkRules)
        {
            catalog.LandmarkEntries.Add(catalog.AddPrefabs(rule.Prefabs, palette, MapObjectCategory.ManMade));
        }

        return catalog;
    }

    public List<int> GetResourceEntries(ResourceType type)
    {
        List<int> result = new List<int>();
        for (int i = 0; i < Entries.Count; i++)
        {
            if (Entries[i].IsResource && Entries[i].ResourceType == type) result.Add(i);
        }
        return result;
    }

    // Un prefab de regla que es el principal de una entrada de paleta arrastra también las variantes de esa entrada
    private List<int> AddPrefabs(List<GameObject> prefabs, MapPaletteSO palette, MapObjectCategory fallbackCategory)
    {
        List<int> indices = new List<int>();
        foreach (GameObject rulePrefab in prefabs)
        {
            if (rulePrefab == null) continue;

            MapPaletteEntry paletteEntry = palette != null ? palette.FindEntry(rulePrefab) : null;
            List<GameObject> expanded = paletteEntry != null && paletteEntry.Prefab == rulePrefab
                ? paletteEntry.GetPrefabs()
                : new List<GameObject> { rulePrefab };

            foreach (GameObject prefab in expanded)
            {
                int existing = Entries.FindIndex(entry => entry.Prefab == prefab);
                if (existing < 0)
                {
                    Entries.Add(CreateEntry(prefab, palette, fallbackCategory));
                    existing = Entries.Count - 1;
                }
                indices.Add(existing);
            }
        }
        return indices;
    }

    private static MapGenerationCatalogEntry CreateEntry(GameObject prefab, MapPaletteSO palette, MapObjectCategory fallbackCategory)
    {
        MapGenerationCatalogEntry entry = new MapGenerationCatalogEntry
        {
            Prefab = prefab,
            Category = fallbackCategory,
            BlocksMovement = prefab.GetComponentInChildren<Collider>() != null
        };

        IHarvestable harvestable = prefab.GetComponent<IHarvestable>();
        if (harvestable != null)
        {
            entry.Category = MapObjectCategory.Resource;
            entry.IsResource = true;
            entry.ResourceType = harvestable.ResourceType;
        }

        MapPaletteEntry paletteEntry = palette != null ? palette.FindEntry(prefab) : null;
        if (paletteEntry == null) return entry;

        entry.Category = paletteEntry.Category;
        entry.Spacing = paletteEntry.Spacing;
        entry.ScaleRange = paletteEntry.ScaleRange;
        entry.VerticalOffset = paletteEntry.VerticalOffset;
        entry.RandomYaw = paletteEntry.RandomYaw;
        return entry;
    }
}

// Todo lo que necesita una corrida del generador: tamaño y origen del mapa, semilla, los grupos de reglas y el
// catálogo de objetos. Es la frontera entre los assets de Unity (reglas, paleta, MapData) y el generador.
public class MapGenerationRequest
{
    public Vector2Int Size;
    public Vector3 Origin;
    public int Seed;
    public int MaxAttempts = 12;
    public float MinPlayableArea = 0.3f;

    public MapLayoutRules Layout = new MapLayoutRules();
    public MapStartRules Start = new MapStartRules();
    public MapTerrainRules Terrain = new MapTerrainRules();
    public MapWaterRules Water = new MapWaterRules();
    public MapPathRules Path = new MapPathRules();
    public List<MapResourceRule> Resources = new List<MapResourceRule>();
    public List<MapScatterRule> Scatter = new List<MapScatterRule>();
    public List<MapLandmarkRule> Landmarks = new List<MapLandmarkRule>();
    public MapGenerationCatalog Catalog = new MapGenerationCatalog();

    public static MapGenerationRequest Create(MapGenerationRulesSO rules, MapPaletteSO palette, MapDataSO mapData, int seed)
    {
        return new MapGenerationRequest
        {
            Size = mapData.Size,
            Origin = mapData.Origin,
            Seed = seed,
            MaxAttempts = rules.MaxAttempts,
            MinPlayableArea = rules.MinPlayableArea,
            Layout = rules.Layout,
            Start = rules.Start,
            Terrain = rules.Terrain,
            Water = rules.Water,
            Path = rules.Path,
            Resources = rules.Resources,
            Scatter = rules.Scatter,
            Landmarks = rules.Landmarks,
            Catalog = MapGenerationCatalog.Build(palette, rules.Scatter, rules.Landmarks)
        };
    }
}
