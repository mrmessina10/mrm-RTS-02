using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public static class DropOffBuildingPrefabGenerator
{
    private const string DataFolderPath = "Assets/_Scripts/ScriptableObjects/AssetsFromSO";
    private const string PrefabFolderPath = "Assets/Prefabs/Buildings/Eco";
    private const float PlaceholderHeight = 1.5f;
    private const float FootprintSize = 2f; // 2x2 unidades de grilla

    [MenuItem("Tools/RTS/Generate Drop-Off Building Prefabs")]
    public static void GenerateDropOffBuildings()
    {
        CreateDropOffBuilding("Lumbermill", BuildingType.Lumbermill, new List<ResourceType> { ResourceType.Wood });
        CreateDropOffBuilding("Farm", BuildingType.Farm, new List<ResourceType> { ResourceType.Food });

        AssetDatabase.Refresh();
    }

    private static void CreateDropOffBuilding(string buildingName, BuildingType buildingType, List<ResourceType> acceptedResources)
    {
        string prefabPath = $"{PrefabFolderPath}/{buildingName}.prefab";

        if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
        {
            Debug.LogWarning($"[DropOffBuildingPrefabGenerator] Ya existe {prefabPath}, no se sobreescribe.");
            return;
        }

        BuildingDataSO data = CreateBuildingData(buildingName, buildingType);
        BuildBuildingPrefab(buildingName, prefabPath, data, acceptedResources);

        // Segunda pasada: con el prefab ya guardado como asset, el SO lo referencia de vuelta
        GameObject savedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        SerializedObject dataSO = new SerializedObject(data);
        dataSO.FindProperty("<BuildingPrefab>k__BackingField").objectReferenceValue = savedPrefab;
        dataSO.ApplyModifiedProperties();

        Debug.Log($"[DropOffBuildingPrefabGenerator] Prefab creado: {prefabPath}");
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
        so.FindProperty("<Footprint>k__BackingField").vector2IntValue = new Vector2Int(2, 2);
        so.ApplyModifiedProperties();

        AssetDatabase.CreateAsset(data, dataPath);
        return data;
    }

    private static void BuildBuildingPrefab(string buildingName, string prefabPath, BuildingDataSO data, List<ResourceType> acceptedResources)
    {
        GameObject root = new GameObject(buildingName);

        int buildingsLayer = LayerMask.NameToLayer("Buildings");
        if (buildingsLayer >= 0) root.layer = buildingsLayer;

        // Collider en el root (no en el visual): la selección busca ISelectable en el mismo GameObject que el collider golpeado
        BoxCollider collider = root.AddComponent<BoxCollider>();
        collider.size = new Vector3(FootprintSize, PlaceholderHeight, FootprintSize);
        collider.center = new Vector3(0f, PlaceholderHeight / 2f, 0f);

        BuildingPlacement placement = root.AddComponent<BuildingPlacement>(); // agrega NavMeshObstacle vía RequireComponent
        SerializedObject placementSO = new SerializedObject(placement);
        placementSO.FindProperty("buildingData").objectReferenceValue = data;
        placementSO.ApplyModifiedProperties();

        DropOffBuilding dropOff = root.AddComponent<DropOffBuilding>();
        SerializedObject dropOffSO = new SerializedObject(dropOff);
        SerializedProperty acceptedProp = dropOffSO.FindProperty("acceptedResources");
        acceptedProp.arraySize = acceptedResources.Count;
        for (int i = 0; i < acceptedResources.Count; i++)
        {
            acceptedProp.GetArrayElementAtIndex(i).enumValueIndex = (int)acceptedResources[i];
        }
        dropOffSO.ApplyModifiedProperties();

        root.AddComponent<Health>();

        UnitSelectionHandler selection = root.AddComponent<UnitSelectionHandler>();
        SerializedObject selectionSO = new SerializedObject(selection);
        selectionSO.FindProperty("unitType").enumValueIndex = (int)UnitType.Building;
        selectionSO.ApplyModifiedProperties();

        // Visual placeholder: hijo separado, sin collider propio, solo para ver el footprint en escena
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.name = "Visual";
        Object.DestroyImmediate(visual.GetComponent<BoxCollider>());
        visual.transform.SetParent(root.transform);
        visual.transform.localPosition = new Vector3(0f, PlaceholderHeight / 2f, 0f);
        visual.transform.localScale = new Vector3(FootprintSize, PlaceholderHeight, FootprintSize);

        if (!AssetDatabase.IsValidFolder(PrefabFolderPath))
        {
            AssetDatabase.CreateFolder("Assets/Prefabs/Buildings", "Eco");
        }

        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);
    }
}
