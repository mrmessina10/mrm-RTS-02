using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

// Genera (sin sobreescribir lo que ya exista) los assets placeholder que necesita el editor de mapas: capas de
// terreno con textura procedural, material de agua, prefabs de naturaleza/construcciones armados con primitivas,
// la paleta por defecto que los agrupa junto a los nodos de recurso existentes y las reglas de generación por defecto.
public static class MapEditorAssetGenerator
{
    public const string GrassLayerName = "Grass";
    public const string DirtLayerName = "Dirt";
    public const string RockLayerName = "Rock";
    public const string SandLayerName = "Sand";
    public const string RoadLayerName = "Road";

    private const string TerrainFolder = "Assets/Materials/Terrain";
    private const string FeatureMaterialFolder = "Assets/Materials/MapFeatures";
    private const string NatureFolder = "Assets/Prefabs/MapFeatures/Nature";
    private const string ManMadeFolder = "Assets/Prefabs/MapFeatures/ManMade";
    private const string ResourceNodeFolder = "Assets/Prefabs/ResourceNodes";
    private const string TreePrefabName = "WoodNode";
    private const string WaterMaterialPath = "Assets/Materials/Water.mat";
    private const string PalettePath = "Assets/_Scripts/ScriptableObjects/AssetsFromSO/MapPalette_Default.asset";
    private const string GenerationRulesPath = "Assets/_Scripts/ScriptableObjects/AssetsFromSO/MapGenerationRules_Default.asset";
    private const int TextureSize = 128;
    private const float LayerTileSize = 8f;

    [MenuItem("Tools/RTS/Generate Map Editor Assets")]
    public static void GenerateAll()
    {
        EnsureTerrainLayers();
        EnsureWaterMaterial();
        EnsurePalette();
        EnsureGenerationRules();

        AssetDatabase.SaveAssets();
        Debug.Log("[MapEditorAssetGenerator] Assets del editor de mapas listos (capas de terreno, material de agua, prefabs, paleta y reglas de generación).");
    }

    public static TerrainLayer[] EnsureTerrainLayers()
    {
        EnsureFolder(TerrainFolder);

        return new[]
        {
            EnsureTerrainLayer(GrassLayerName, new Color(0.33f, 0.47f, 0.2f), 0.1f, 11f),
            EnsureTerrainLayer(DirtLayerName, new Color(0.42f, 0.31f, 0.2f), 0.08f, 23f),
            EnsureTerrainLayer(RockLayerName, new Color(0.45f, 0.44f, 0.42f), 0.14f, 37f),
            EnsureTerrainLayer(SandLayerName, new Color(0.76f, 0.69f, 0.5f), 0.05f, 51f),
            EnsureTerrainLayer(RoadLayerName, new Color(0.56f, 0.47f, 0.35f), 0.06f, 67f)
        };
    }

    public static Material EnsureWaterMaterial()
    {
        Material existing = AssetDatabase.LoadAssetAtPath<Material>(WaterMaterialPath);
        if (existing != null) return existing;

        EnsureFolder("Assets/Materials");

        Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        material.SetColor("_BaseColor", new Color(0.12f, 0.36f, 0.55f, 0.72f));
        material.SetFloat("_Smoothness", 0.9f);
        material.SetFloat("_Surface", 1f);
        material.SetFloat("_Blend", 0f);
        material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
        material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
        material.SetFloat("_ZWrite", 0f);
        material.SetOverrideTag("RenderType", "Transparent");
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.renderQueue = (int)RenderQueue.Transparent;

        AssetDatabase.CreateAsset(material, WaterMaterialPath);
        return material;
    }

    public static MapPaletteSO EnsurePalette()
    {
        MapPaletteSO existing = AssetDatabase.LoadAssetAtPath<MapPaletteSO>(PalettePath);
        if (existing != null) return existing;

        EnsureFolder(FeatureMaterialFolder);
        EnsureFolder(NatureFolder);
        EnsureFolder(ManMadeFolder);

        Material stone = EnsureFeatureMaterial("Stone", new Color(0.5f, 0.5f, 0.48f));
        Material trunk = EnsureFeatureMaterial("Trunk", new Color(0.36f, 0.25f, 0.15f));
        Material foliage = EnsureFeatureMaterial("Foliage", new Color(0.16f, 0.36f, 0.14f));
        Material wall = EnsureFeatureMaterial("Wall", new Color(0.82f, 0.76f, 0.62f));
        Material roof = EnsureFeatureMaterial("Roof", new Color(0.55f, 0.22f, 0.16f));
        Material straw = EnsureFeatureMaterial("Straw", new Color(0.85f, 0.72f, 0.3f));

        MapPaletteSO palette = ScriptableObject.CreateInstance<MapPaletteSO>();
        List<MapPaletteEntry> entries = palette.Entries;

        AddExistingResource(entries, "Árbol (madera)", TreePrefabName, 1.6f, new Vector2(0.8f, 1.3f), 1f);
        AddExistingResource(entries, "Arbusto (comida)", "FoodNode", 1.5f, new Vector2(0.8f, 1.1f), 0.5f);

        entries.Add(CreateEntry("Roca chica", MapObjectCategory.Nature, 1.5f, new Vector2(0.7f, 1.4f), true,
            BuildPrefab(NatureFolder, "Rock_Small", root =>
            {
                AddPart(root, PrimitiveType.Sphere, new Vector3(0f, 0.3f, 0f), new Vector3(1.2f, 0.8f, 1f), Vector3.zero, stone, true);
            })));

        entries.Add(CreateEntry("Roca grande", MapObjectCategory.Nature, 3f, new Vector2(0.8f, 1.6f), true,
            BuildPrefab(NatureFolder, "Rock_Large", root =>
            {
                AddPart(root, PrimitiveType.Cube, new Vector3(0f, 0.8f, 0f), new Vector3(2.5f, 2f, 2.2f), new Vector3(10f, 25f, 5f), stone, true);
                AddPart(root, PrimitiveType.Sphere, new Vector3(1.1f, 0.5f, 0.6f), new Vector3(1.6f, 1.2f, 1.6f), Vector3.zero, stone, true);
            })));

        entries.Add(CreateEntry("Matorral", MapObjectCategory.Nature, 1f, new Vector2(0.6f, 1.3f), true,
            BuildPrefab(NatureFolder, "Bush", root =>
            {
                AddPart(root, PrimitiveType.Sphere, new Vector3(0f, 0.3f, 0f), new Vector3(1f, 0.7f, 1f), Vector3.zero, foliage, false);
            })));

        entries.Add(CreateEntry("Casa chica", MapObjectCategory.ManMade, 5f, Vector2.one, false,
            BuildPrefab(ManMadeFolder, "House_Small", root =>
            {
                AddPart(root, PrimitiveType.Cube, new Vector3(0f, 1f, 0f), new Vector3(3f, 2f, 3f), Vector3.zero, wall, true);
                AddPart(root, PrimitiveType.Cube, new Vector3(0f, 2f, 0f), new Vector3(2.2f, 2.2f, 3.2f), new Vector3(0f, 0f, 45f), roof, false);
            })));

        entries.Add(CreateEntry("Casa grande", MapObjectCategory.ManMade, 6.5f, Vector2.one, false,
            BuildPrefab(ManMadeFolder, "House_Large", root =>
            {
                AddPart(root, PrimitiveType.Cube, new Vector3(0f, 1.25f, 0f), new Vector3(5f, 2.5f, 3.5f), Vector3.zero, wall, true);
                AddPart(root, PrimitiveType.Cube, new Vector3(0f, 2.5f, 0f), new Vector3(5.2f, 2.5f, 2.5f), new Vector3(45f, 0f, 0f), roof, false);
            })));

        entries.Add(CreateEntry("Pozo", MapObjectCategory.ManMade, 2.5f, Vector2.one, true,
            BuildPrefab(ManMadeFolder, "Well", root =>
            {
                AddPart(root, PrimitiveType.Cylinder, new Vector3(0f, 0.4f, 0f), new Vector3(1.4f, 0.4f, 1.4f), Vector3.zero, stone, true);
                AddPart(root, PrimitiveType.Cube, new Vector3(0f, 1.6f, 0f), new Vector3(1.8f, 0.15f, 0.15f), Vector3.zero, trunk, false);
                AddPart(root, PrimitiveType.Cube, new Vector3(-0.8f, 1f, 0f), new Vector3(0.15f, 1.2f, 0.15f), Vector3.zero, trunk, false);
                AddPart(root, PrimitiveType.Cube, new Vector3(0.8f, 1f, 0f), new Vector3(0.15f, 1.2f, 0.15f), Vector3.zero, trunk, false);
            })));

        entries.Add(CreateEntry("Cerca", MapObjectCategory.ManMade, 1.9f, Vector2.one, false,
            BuildPrefab(ManMadeFolder, "Fence", root =>
            {
                AddPart(root, PrimitiveType.Cube, new Vector3(0f, 0.55f, 0f), new Vector3(2f, 0.12f, 0.1f), Vector3.zero, trunk, true);
                AddPart(root, PrimitiveType.Cube, new Vector3(-0.9f, 0.4f, 0f), new Vector3(0.15f, 0.8f, 0.15f), Vector3.zero, trunk, false);
                AddPart(root, PrimitiveType.Cube, new Vector3(0.9f, 0.4f, 0f), new Vector3(0.15f, 0.8f, 0.15f), Vector3.zero, trunk, false);
            })));

        entries.Add(CreateEntry("Parva de heno", MapObjectCategory.ManMade, 2f, new Vector2(0.8f, 1.2f), true,
            BuildPrefab(ManMadeFolder, "Haystack", root =>
            {
                AddPart(root, PrimitiveType.Cylinder, new Vector3(0f, 0.6f, 0f), new Vector3(1.4f, 0.6f, 1.4f), Vector3.zero, straw, true);
            })));

        entries.Add(CreateEntry("Ruina", MapObjectCategory.ManMade, 3.5f, Vector2.one, true,
            BuildPrefab(ManMadeFolder, "Ruin_Wall", root =>
            {
                AddPart(root, PrimitiveType.Cube, new Vector3(0f, 0.75f, 0f), new Vector3(3f, 1.5f, 0.5f), Vector3.zero, stone, true);
                AddPart(root, PrimitiveType.Cube, new Vector3(1.25f, 0.5f, 1f), new Vector3(0.5f, 1f, 1.5f), Vector3.zero, stone, true);
                AddPart(root, PrimitiveType.Cube, new Vector3(-0.6f, 0.2f, 0.7f), new Vector3(0.6f, 0.4f, 0.5f), new Vector3(0f, 30f, 0f), stone, false);
            })));

        AssetDatabase.CreateAsset(palette, PalettePath);
        return palette;
    }

    public static MapGenerationRulesSO EnsureGenerationRules()
    {
        MapGenerationRulesSO existing = AssetDatabase.LoadAssetAtPath<MapGenerationRulesSO>(GenerationRulesPath);
        if (existing != null) return existing;

        EnsurePalette();

        MapGenerationRulesSO rules = ScriptableObject.CreateInstance<MapGenerationRulesSO>();

        rules.Scatter.Add(new MapScatterRule
        {
            Name = "Bosque",
            Prefabs = LoadPrefabs(ResourceNodeFolder, TreePrefabName),
            Spacing = 2.8f,
            Coverage = 0.85f,
            Clumping = 0.85f,
            ClumpScale = 26f
        });
        rules.Scatter.Add(new MapScatterRule
        {
            Name = "Rocas",
            Prefabs = LoadPrefabs(NatureFolder, "Rock_Small", "Rock_Small", "Rock_Large"),
            Spacing = 7f,
            Coverage = 0.3f,
            Clumping = 0.5f,
            ClumpScale = 14f,
            PathClearance = 3f,
            StartClearance = 16f,
            MaxSlope = 35f
        });
        rules.Scatter.Add(new MapScatterRule
        {
            Name = "Matorrales",
            Prefabs = LoadPrefabs(NatureFolder, "Bush"),
            Spacing = 3f,
            Coverage = 0.25f,
            Clumping = 0.6f,
            ClumpScale = 12f,
            PathClearance = 1f,
            StartClearance = 10f
        });

        rules.Landmarks.Add(new MapLandmarkRule
        {
            Name = "Aldea",
            Prefabs = LoadPrefabs(ManMadeFolder, "House_Small", "House_Small", "House_Large", "Well", "Haystack"),
            Pieces = new Vector2Int(4, 6),
            Radius = new Vector2(6f, 8f)
        });
        rules.Landmarks.Add(new MapLandmarkRule
        {
            Name = "Ruinas",
            Prefabs = LoadPrefabs(ManMadeFolder, "Ruin_Wall"),
            Pieces = new Vector2Int(2, 4),
            Radius = new Vector2(4f, 6f)
        });

        AssetDatabase.CreateAsset(rules, GenerationRulesPath);
        return rules;
    }

    private static List<GameObject> LoadPrefabs(string folder, params string[] prefabNames)
    {
        List<GameObject> prefabs = new List<GameObject>();
        foreach (string prefabName in prefabNames)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{folder}/{prefabName}.prefab");
            if (prefab != null) prefabs.Add(prefab);
        }
        return prefabs;
    }

    private static TerrainLayer EnsureTerrainLayer(string layerName, Color baseColor, float variation, float seed)
    {
        string layerPath = $"{TerrainFolder}/{layerName}.terrainlayer";
        TerrainLayer existing = AssetDatabase.LoadAssetAtPath<TerrainLayer>(layerPath);
        if (existing != null) return existing;

        string texturePath = $"{TerrainFolder}/Terrain_{layerName}.png";
        if (AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath) == null)
        {
            Texture2D generated = GenerateSeamlessTexture(baseColor, variation, seed);
            File.WriteAllBytes(texturePath, generated.EncodeToPNG());
            Object.DestroyImmediate(generated);
            AssetDatabase.ImportAsset(texturePath);
        }

        TerrainLayer layer = new TerrainLayer
        {
            diffuseTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath),
            tileSize = new Vector2(LayerTileSize, LayerTileSize),
            smoothness = 0f,
            metallic = 0f
        };

        AssetDatabase.CreateAsset(layer, layerPath);
        return layer;
    }

    private static Texture2D GenerateSeamlessTexture(Color baseColor, float variation, float seed)
    {
        const float NoisePeriod = 6f;

        Texture2D texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGB24, false);
        System.Random random = new System.Random((int)seed);

        for (int y = 0; y < TextureSize; y++)
        {
            for (int x = 0; x < TextureSize; x++)
            {
                float u = x / (float)TextureSize;
                float v = y / (float)TextureSize;

                float noise = SampleSeamlessNoise(u, v, NoisePeriod, seed) * 0.7f + SampleSeamlessNoise(u, v, NoisePeriod * 3f, seed + 100f) * 0.3f;
                float speckle = (float)random.NextDouble() - 0.5f;
                float brightness = 1f + (noise - 0.5f) * 2f * variation + speckle * variation * 0.6f;

                texture.SetPixel(x, y, new Color(baseColor.r * brightness, baseColor.g * brightness, baseColor.b * brightness));
            }
        }

        texture.Apply();
        return texture;
    }

    private static float SampleSeamlessNoise(float u, float v, float period, float seed)
    {
        float x = u * period;
        float y = v * period;

        float a = Mathf.PerlinNoise(seed + x, seed + y);
        float b = Mathf.PerlinNoise(seed + x - period, seed + y);
        float c = Mathf.PerlinNoise(seed + x - period, seed + y - period);
        float d = Mathf.PerlinNoise(seed + x, seed + y - period);

        return a * (1f - u) * (1f - v) + b * u * (1f - v) + c * u * v + d * (1f - u) * v;
    }

    private static Material EnsureFeatureMaterial(string materialName, Color color)
    {
        string path = $"{FeatureMaterialFolder}/MapFeature_{materialName}.mat";
        Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null) return existing;

        Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        material.SetColor("_BaseColor", color);
        material.SetFloat("_Smoothness", 0.1f);

        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    private static GameObject BuildPrefab(string folder, string prefabName, System.Action<Transform> buildParts)
    {
        string prefabPath = $"{folder}/{prefabName}.prefab";
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (existing != null) return existing;

        GameObject root = new GameObject(prefabName);
        buildParts(root.transform);

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static void AddPart(Transform parent, PrimitiveType shape, Vector3 localPosition, Vector3 localScale, Vector3 localEuler, Material material, bool keepCollider)
    {
        GameObject part = GameObject.CreatePrimitive(shape);
        part.name = shape.ToString();
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPosition;
        part.transform.localRotation = Quaternion.Euler(localEuler);
        part.transform.localScale = localScale;
        part.GetComponent<MeshRenderer>().sharedMaterial = material;

        if (!keepCollider) Object.DestroyImmediate(part.GetComponent<Collider>());
    }

    private static MapPaletteEntry CreateEntry(string displayName, MapObjectCategory category, float spacing, Vector2 scaleRange, bool randomYaw, GameObject prefab)
    {
        return new MapPaletteEntry
        {
            DisplayName = displayName,
            Prefab = prefab,
            Category = category,
            Spacing = spacing,
            ScaleRange = scaleRange,
            RandomYaw = randomYaw
        };
    }

    private static void AddExistingResource(List<MapPaletteEntry> entries, string displayName, string prefabName, float spacing, Vector2 scaleRange, float verticalOffset)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{ResourceNodeFolder}/{prefabName}.prefab");
        if (prefab == null)
        {
            Debug.LogWarning($"[MapEditorAssetGenerator] No existe {ResourceNodeFolder}/{prefabName}.prefab (Tools/RTS/Generate Resource Node Prefabs); no se agrega a la paleta.");
            return;
        }

        MapPaletteEntry entry = CreateEntry(displayName, MapObjectCategory.Resource, spacing, scaleRange, true, prefab);
        entry.VerticalOffset = verticalOffset;
        entries.Add(entry);
    }

    public static void EnsureFolder(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath)) return;

        string parent = Path.GetDirectoryName(folderPath).Replace('\\', '/');
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(folderPath));
    }
}
