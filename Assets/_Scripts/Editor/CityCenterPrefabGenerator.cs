using UnityEngine;
using UnityEditor;

public static class CityCenterPrefabGenerator
{
    private const string DataFolderPath = "Assets/_Scripts/ScriptableObjects/AssetsFromSO";
    private const string PrefabFolderPath = "Assets/Prefabs/Buildings/Pop";
    private const string PrefabPath = PrefabFolderPath + "/CityCenter.prefab";
    private const string WorkerPrefabPath = "Assets/Prefabs/testWorkerUnit.prefab";
    private const string GameOverChannelPath = DataFolderPath + "/Channel_GameOver.asset";

    private const float PlaceholderHeight = 3f;
    private const int FootprintSize = 4;
    private const float SpawnPointMargin = 1.5f;
    private const int MaxHealth = 500;
    private const int WorkerFoodCost = 20;
    private const float WorkerProductionTime = 10f;

    [MenuItem("Tools/RTS/Generate City Center Prefab")]
    public static void GenerateCityCenter()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null)
        {
            Debug.LogWarning($"[CityCenterPrefabGenerator] Ya existe {PrefabPath}, no se sobreescribe.");
            return;
        }

        UnitDataSO workerData = CreateWorkerData();
        BuildingDataSO data = CreateBuildingData(workerData);
        BuildPrefab(data);

        GameObject savedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        SerializedObject dataSO = new SerializedObject(data);
        dataSO.FindProperty("<BuildingPrefab>k__BackingField").objectReferenceValue = savedPrefab;
        dataSO.ApplyModifiedProperties();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[CityCenterPrefabGenerator] Prefab creado: {PrefabPath}");
    }

    private static UnitDataSO CreateWorkerData()
    {
        string dataPath = $"{DataFolderPath}/UnitData_Worker.asset";

        UnitDataSO existing = AssetDatabase.LoadAssetAtPath<UnitDataSO>(dataPath);
        if (existing != null) return existing;

        GameObject workerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(WorkerPrefabPath);
        if (workerPrefab == null)
        {
            Debug.LogWarning($"[CityCenterPrefabGenerator] No se encontró {WorkerPrefabPath}; UnitData_Worker queda sin UnitPrefab.");
        }

        UnitDataSO data = ScriptableObject.CreateInstance<UnitDataSO>();
        SerializedObject so = new SerializedObject(data);
        so.FindProperty("<UnitType>k__BackingField").enumValueIndex = (int)UnitType.Worker;
        so.FindProperty("<DisplayName>k__BackingField").stringValue = "Worker";
        so.FindProperty("<UnitPrefab>k__BackingField").objectReferenceValue = workerPrefab;
        so.FindProperty("<ProductionTime>k__BackingField").floatValue = WorkerProductionTime;

        SerializedProperty costProp = so.FindProperty("<ProductionCost>k__BackingField");
        costProp.arraySize = 1;
        SerializedProperty foodCost = costProp.GetArrayElementAtIndex(0);
        foodCost.FindPropertyRelative("Type").enumValueIndex = (int)ResourceType.Food;
        foodCost.FindPropertyRelative("Amount").intValue = WorkerFoodCost;
        so.ApplyModifiedProperties();

        AssetDatabase.CreateAsset(data, dataPath);
        return data;
    }

    private static BuildingDataSO CreateBuildingData(UnitDataSO workerData)
    {
        string dataPath = $"{DataFolderPath}/BuildingData_CityCenter.asset";

        BuildingDataSO existing = AssetDatabase.LoadAssetAtPath<BuildingDataSO>(dataPath);
        if (existing != null) return existing;

        BuildingDataSO data = ScriptableObject.CreateInstance<BuildingDataSO>();
        SerializedObject so = new SerializedObject(data);
        so.FindProperty("<BuildingType>k__BackingField").enumValueIndex = (int)BuildingType.CityCenter;
        so.FindProperty("<DisplayName>k__BackingField").stringValue = "City Center";
        so.FindProperty("<Footprint>k__BackingField").vector2IntValue = new Vector2Int(FootprintSize, FootprintSize);

        SerializedProperty producibleProp = so.FindProperty("<ProducibleUnits>k__BackingField");
        producibleProp.arraySize = 1;
        producibleProp.GetArrayElementAtIndex(0).objectReferenceValue = workerData;
        so.ApplyModifiedProperties();

        AssetDatabase.CreateAsset(data, dataPath);
        return data;
    }

    private static void BuildPrefab(BuildingDataSO data)
    {
        GameObject root = new GameObject("CityCenter");

        int buildingsLayer = LayerMask.NameToLayer("Buildings");
        if (buildingsLayer >= 0) root.layer = buildingsLayer;

        BoxCollider collider = root.AddComponent<BoxCollider>();
        collider.size = new Vector3(FootprintSize, PlaceholderHeight, FootprintSize);
        collider.center = new Vector3(0f, PlaceholderHeight / 2f, 0f);

        BuildingPlacement placement = root.AddComponent<BuildingPlacement>();
        SerializedObject placementSO = new SerializedObject(placement);
        placementSO.FindProperty("buildingData").objectReferenceValue = data;
        placementSO.ApplyModifiedProperties();

        DropOffBuilding dropOff = root.AddComponent<DropOffBuilding>();
        ResourceType[] acceptedResources = { ResourceType.Wood, ResourceType.Food, ResourceType.Stone };
        SerializedObject dropOffSO = new SerializedObject(dropOff);
        SerializedProperty acceptedProp = dropOffSO.FindProperty("acceptedResources");
        acceptedProp.arraySize = acceptedResources.Length;
        for (int i = 0; i < acceptedResources.Length; i++)
        {
            acceptedProp.GetArrayElementAtIndex(i).enumValueIndex = (int)acceptedResources[i];
        }
        dropOffSO.ApplyModifiedProperties();

        GameObject spawnPoint = new GameObject("SpawnPoint");
        spawnPoint.transform.SetParent(root.transform);
        spawnPoint.transform.localPosition = new Vector3(0f, 0f, -(FootprintSize / 2f + SpawnPointMargin));

        UnitProducer producer = root.AddComponent<UnitProducer>();
        SerializedObject producerSO = new SerializedObject(producer);
        producerSO.FindProperty("buildingData").objectReferenceValue = data;
        producerSO.FindProperty("spawnPoint").objectReferenceValue = spawnPoint.transform;
        producerSO.ApplyModifiedProperties();

        Health health = root.AddComponent<Health>();
        VoidEventChannelSO gameOverChannel = AssetDatabase.LoadAssetAtPath<VoidEventChannelSO>(GameOverChannelPath);
        if (gameOverChannel == null)
        {
            Debug.LogWarning($"[CityCenterPrefabGenerator] No se encontró {GameOverChannelPath}; la destrucción del City Center no va a disparar la derrota.");
        }
        SerializedObject healthSO = new SerializedObject(health);
        healthSO.FindProperty("maxHealth").intValue = MaxHealth;
        healthSO.FindProperty("onDeathChannel").objectReferenceValue = gameOverChannel;
        healthSO.ApplyModifiedProperties();

        root.AddComponent<Headquarters>();

        UnitSelectionHandler selection = root.AddComponent<UnitSelectionHandler>();
        SerializedObject selectionSO = new SerializedObject(selection);
        selectionSO.FindProperty("unitType").enumValueIndex = (int)UnitType.Building;
        selectionSO.ApplyModifiedProperties();

        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.name = "Visual";
        Object.DestroyImmediate(visual.GetComponent<BoxCollider>());
        visual.transform.SetParent(root.transform);
        visual.transform.localPosition = new Vector3(0f, PlaceholderHeight / 2f, 0f);
        visual.transform.localScale = new Vector3(FootprintSize, PlaceholderHeight, FootprintSize);

        if (!AssetDatabase.IsValidFolder(PrefabFolderPath))
        {
            AssetDatabase.CreateFolder("Assets/Prefabs/Buildings", "Pop");
        }

        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);
    }
}
