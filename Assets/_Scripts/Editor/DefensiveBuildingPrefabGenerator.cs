using UnityEngine;
using UnityEditor;

public static class DefensiveBuildingPrefabGenerator
{
    private const string DataFolderPath = "Assets/_Scripts/ScriptableObjects/AssetsFromSO";
    private const string PrefabFolderPath = "Assets/Prefabs/Buildings/Defense";
    private const float PlaceholderHeight = 2f;
    private const float FootprintSize = 1f; // 1x1 unidad de grilla, un segmento por celda

    [MenuItem("Tools/RTS/Generate Palisade Wall Prefabs")]
    public static void GeneratePalisadeAndGate()
    {
        CreateSegmentBuilding("PalisadeSegment", BuildingType.Palisade);
        CreateSegmentBuilding("Gate", BuildingType.Gate);

        AssetDatabase.Refresh();
    }

    private static void CreateSegmentBuilding(string buildingName, BuildingType buildingType)
    {
        string prefabPath = $"{PrefabFolderPath}/{buildingName}.prefab";

        if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
        {
            Debug.LogWarning($"[DefensiveBuildingPrefabGenerator] Ya existe {prefabPath}, no se sobreescribe.");
            return;
        }

        BuildingDataSO data = CreateBuildingData(buildingName, buildingType);
        BuildSegmentPrefab(buildingName, prefabPath, data);

        // Segunda pasada: con el prefab ya guardado como asset, el SO lo referencia de vuelta
        GameObject savedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        SerializedObject dataSO = new SerializedObject(data);
        dataSO.FindProperty("<BuildingPrefab>k__BackingField").objectReferenceValue = savedPrefab;
        dataSO.ApplyModifiedProperties();

        Debug.Log($"[DefensiveBuildingPrefabGenerator] Prefab creado: {prefabPath}");
    }

    private static BuildingDataSO CreateBuildingData(string buildingName, BuildingType buildingType)
    {
        string dataPath = $"{DataFolderPath}/BuildingData_{buildingName}.asset";

        BuildingDataSO existing = AssetDatabase.LoadAssetAtPath<BuildingDataSO>(dataPath);
        if (existing != null) return existing;

        BuildingDataSO data = ScriptableObject.CreateInstance<BuildingDataSO>();
        SerializedObject so = new SerializedObject(data);
        so.FindProperty("<BuildingType>k__BackingField").enumValueIndex = (int)buildingType;
        so.FindProperty("<DisplayName>k__BackingField").stringValue = buildingName;
        so.FindProperty("<Footprint>k__BackingField").vector2IntValue = new Vector2Int(1, 1);
        so.ApplyModifiedProperties();

        AssetDatabase.CreateAsset(data, dataPath);
        return data;
    }

    private static void BuildSegmentPrefab(string buildingName, string prefabPath, BuildingDataSO data)
    {
        GameObject root = new GameObject(buildingName);

        int buildingsLayer = LayerMask.NameToLayer("Buildings");
        if (buildingsLayer >= 0) root.layer = buildingsLayer;

        BoxCollider collider = root.AddComponent<BoxCollider>();
        collider.size = new Vector3(FootprintSize, PlaceholderHeight, FootprintSize);
        collider.center = new Vector3(0f, PlaceholderHeight / 2f, 0f);

        BuildingPlacement placement = root.AddComponent<BuildingPlacement>(); // agrega NavMeshObstacle vía RequireComponent
        SerializedObject placementSO = new SerializedObject(placement);
        placementSO.FindProperty("buildingData").objectReferenceValue = data;
        placementSO.ApplyModifiedProperties();

        root.AddComponent<Health>();

        // Visual placeholder: hijo separado, sin collider propio, solo para ver el segmento en escena
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.name = "Visual";
        Object.DestroyImmediate(visual.GetComponent<BoxCollider>());
        visual.transform.SetParent(root.transform);
        visual.transform.localPosition = new Vector3(0f, PlaceholderHeight / 2f, 0f);
        visual.transform.localScale = new Vector3(FootprintSize, PlaceholderHeight, FootprintSize);

        if (!AssetDatabase.IsValidFolder(PrefabFolderPath))
        {
            AssetDatabase.CreateFolder("Assets/Prefabs/Buildings", "Defense");
        }

        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);
    }
}
