using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using Unity.AI.Navigation;
using Unity.AI.Navigation.Editor;
using System.Collections.Generic;

// Arma en la escena la jerarquía de un mapa nuevo (MapRoot + Terrain + agua + contenedores + NavMeshSurface),
// crea sus assets (MapDataSO y TerrainData) y centraliza las operaciones de escena del editor de mapas:
// buscar el MapRoot, mover el nivel de agua y hacer el bake del NavMesh.
public static class MapSceneSetup
{
    public const float DefaultHeightRange = 40f;
    public const float DefaultBaseHeight = 10f;
    public const float DefaultWaterLevel = -1f;

    private const string MapsFolder = "Assets/Maps";
    private const float WadeDepth = 0.3f;
    private const int NotWalkableArea = 1;
    private const int MaxHeightmapResolution = 1025;
    private const int MaxAlphamapResolution = 1024;

    public static MapRoot FindMapRoot()
    {
        foreach (GameObject rootObject in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            MapRoot mapRoot = rootObject.GetComponentInChildren<MapRoot>(true);
            if (mapRoot != null) return mapRoot;
        }
        return null;
    }

    public static MapRoot CreateMap(string mapName, int size)
    {
        string folder = $"{MapsFolder}/{mapName}";
        string mapDataPath = $"{folder}/{mapName}.asset";

        if (AssetDatabase.LoadAssetAtPath<MapDataSO>(mapDataPath) != null)
        {
            Debug.LogWarning($"[MapSceneSetup] Ya existe {mapDataPath}. Elegí otro nombre o cargá ese MapData en la escena.");
            return null;
        }

        TerrainLayer[] layers = MapEditorAssetGenerator.EnsureTerrainLayers();
        Material waterMaterial = MapEditorAssetGenerator.EnsureWaterMaterial();
        MapEditorAssetGenerator.EnsurePalette();
        MapEditorAssetGenerator.EnsureFolder(folder);

        Vector3 origin = new Vector3(-size / 2f, -DefaultBaseHeight, -size / 2f);
        TerrainData terrainData = CreateTerrainData(size, layers, $"{folder}/{mapName}_Terrain.asset");

        MapDataSO mapData = ScriptableObject.CreateInstance<MapDataSO>();
        mapData.Initialize(new Vector2Int(size, size), origin, terrainData, DefaultWaterLevel);
        AssetDatabase.CreateAsset(mapData, mapDataPath);

        GameObject rootObject = new GameObject($"Map_{mapName}");
        MapRoot mapRoot = rootObject.AddComponent<MapRoot>();

        Terrain terrain = CreateTerrainObject(rootObject.transform, terrainData, origin);
        Transform water = CreateWater(rootObject.transform, size, waterMaterial);

        SerializedObject serializedRoot = new SerializedObject(mapRoot);
        SetReference(serializedRoot, "MapData", mapData);
        SetReference(serializedRoot, "Terrain", terrain);
        SetReference(serializedRoot, "Water", water);
        SetReference(serializedRoot, "ResourcesContainer", CreateContainer(rootObject.transform, "Resources"));
        SetReference(serializedRoot, "NatureContainer", CreateContainer(rootObject.transform, "Nature"));
        SetReference(serializedRoot, "ManMadeContainer", CreateContainer(rootObject.transform, "ManMade"));
        SetReference(serializedRoot, "MarkersContainer", CreateContainer(rootObject.transform, "Markers"));
        SetReference(serializedRoot, "PathsContainer", CreateContainer(rootObject.transform, "Paths"));
        serializedRoot.ApplyModifiedPropertiesWithoutUndo();

        ConfigureNavMeshSurface(rootObject.AddComponent<NavMeshSurface>());
        ApplyWaterLevel(mapRoot, DefaultWaterLevel);

        Undo.RegisterCreatedObjectUndo(rootObject, "Crear mapa");
        MapCameraSetup.EnsureCameraRig(mapRoot);
        EditorSceneManager.MarkSceneDirty(rootObject.scene);
        AssetDatabase.SaveAssets();

        Debug.Log($"[MapSceneSetup] Mapa '{mapName}' creado ({size}x{size}). Assets en {folder}. Guardá la escena (Ctrl+S).");
        return mapRoot;
    }

    public static void ApplyWaterLevel(MapRoot mapRoot, float waterLevel)
    {
        if (mapRoot.Water != null)
        {
            Undo.RecordObject(mapRoot.Water, "Mapa: nivel de agua");
            Vector3 position = mapRoot.Water.position;
            mapRoot.Water.position = new Vector3(position.x, waterLevel, position.z);
        }

        if (mapRoot.MapData != null)
        {
            Undo.RecordObject(mapRoot.MapData, "Mapa: nivel de agua");
            mapRoot.MapData.SetWaterLevel(waterLevel);
            EditorUtility.SetDirty(mapRoot.MapData);
        }
    }

    public static void BakeNavMesh(MapRoot mapRoot)
    {
        NavMeshSurface surface = mapRoot.GetComponent<NavMeshSurface>();
        if (surface == null)
        {
            Debug.LogWarning("[MapSceneSetup] El MapRoot no tiene NavMeshSurface.");
            return;
        }

        if (string.IsNullOrEmpty(mapRoot.gameObject.scene.path))
        {
            Debug.LogWarning("[MapSceneSetup] Guardá la escena antes de hacer el bake: el NavMesh se guarda como asset junto a ella.");
            return;
        }

        NavMeshAssetManager.instance.StartBakingSurfaces(new Object[] { surface });
        Debug.Log("[MapSceneSetup] Bake de NavMesh iniciado.");
    }

    public static List<NavMeshSurface> FindForeignNavMeshSurfaces(MapRoot mapRoot)
    {
        List<NavMeshSurface> foreignSurfaces = new List<NavMeshSurface>();

        foreach (GameObject rootObject in mapRoot.gameObject.scene.GetRootGameObjects())
        {
            foreach (NavMeshSurface surface in rootObject.GetComponentsInChildren<NavMeshSurface>(false))
            {
                if (surface.gameObject != mapRoot.gameObject) foreignSurfaces.Add(surface);
            }
        }
        return foreignSurfaces;
    }

    private static TerrainData CreateTerrainData(int size, TerrainLayer[] layers, string assetPath)
    {
        int heightmapResolution = Mathf.Min(Mathf.NextPowerOfTwo(size * 2), MaxHeightmapResolution - 1) + 1;

        TerrainData terrainData = new TerrainData();
        terrainData.heightmapResolution = heightmapResolution;
        terrainData.size = new Vector3(size, DefaultHeightRange, size);
        terrainData.alphamapResolution = Mathf.Min(Mathf.NextPowerOfTwo(size * 4), MaxAlphamapResolution);
        terrainData.baseMapResolution = 1024;
        AssetDatabase.CreateAsset(terrainData, assetPath);

        terrainData.terrainLayers = layers;

        float baseHeight = DefaultBaseHeight / DefaultHeightRange;
        float[,] heights = new float[heightmapResolution, heightmapResolution];
        for (int z = 0; z < heightmapResolution; z++)
        {
            for (int x = 0; x < heightmapResolution; x++)
            {
                heights[z, x] = baseHeight;
            }
        }
        terrainData.SetHeights(0, 0, heights);
        FillFirstLayer(terrainData);

        EditorUtility.SetDirty(terrainData);
        return terrainData;
    }

    private static void FillFirstLayer(TerrainData terrainData)
    {
        float[,,] alphamaps = new float[terrainData.alphamapHeight, terrainData.alphamapWidth, terrainData.alphamapLayers];
        for (int z = 0; z < terrainData.alphamapHeight; z++)
        {
            for (int x = 0; x < terrainData.alphamapWidth; x++)
            {
                alphamaps[z, x, 0] = 1f;
            }
        }
        terrainData.SetAlphamaps(0, 0, alphamaps);
    }

    private static Terrain CreateTerrainObject(Transform parent, TerrainData terrainData, Vector3 origin)
    {
        GameObject terrainObject = Terrain.CreateTerrainGameObject(terrainData);
        terrainObject.name = "Terrain";
        terrainObject.transform.SetParent(parent, false);
        terrainObject.transform.position = origin;

        int groundLayer = LayerMask.NameToLayer("Ground");
        if (groundLayer >= 0) terrainObject.layer = groundLayer;

        Terrain terrain = terrainObject.GetComponent<Terrain>();
        terrain.basemapDistance = 2000f;
        terrain.drawInstanced = true;

        RenderPipelineAsset pipeline = GraphicsSettings.currentRenderPipeline;
        if (pipeline != null && pipeline.defaultTerrainMaterial != null) terrain.materialTemplate = pipeline.defaultTerrainMaterial;

        return terrain;
    }

    private static Transform CreateWater(Transform parent, int size, Material waterMaterial)
    {
        GameObject waterObject = new GameObject("Water");
        waterObject.transform.SetParent(parent, false);

        int waterLayer = LayerMask.NameToLayer("Water");
        if (waterLayer >= 0) waterObject.layer = waterLayer;

        GameObject surface = GameObject.CreatePrimitive(PrimitiveType.Plane);
        surface.name = "Surface";
        Object.DestroyImmediate(surface.GetComponent<Collider>());
        surface.transform.SetParent(waterObject.transform, false);
        surface.transform.localScale = new Vector3(size / 10f, 1f, size / 10f);
        surface.layer = waterObject.layer;
        surface.GetComponent<MeshRenderer>().sharedMaterial = waterMaterial;
        surface.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;

        NavMeshModifierVolume blocker = waterObject.AddComponent<NavMeshModifierVolume>();
        blocker.size = new Vector3(size, DefaultHeightRange, size);
        blocker.center = new Vector3(0f, -WadeDepth - DefaultHeightRange / 2f, 0f);
        blocker.area = NotWalkableArea;

        return waterObject.transform;
    }

    private static Transform CreateContainer(Transform parent, string containerName)
    {
        GameObject container = new GameObject(containerName);
        container.transform.SetParent(parent, false);
        return container.transform;
    }

    private static void ConfigureNavMeshSurface(NavMeshSurface surface)
    {
        surface.collectObjects = CollectObjects.All;
        surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
        surface.layerMask = LayerMask.GetMask("Default", "Ground", "Water");
    }

    private static void SetReference(SerializedObject serializedObject, string propertyName, Object value)
    {
        serializedObject.FindProperty($"<{propertyName}>k__BackingField").objectReferenceValue = value;
    }
}
