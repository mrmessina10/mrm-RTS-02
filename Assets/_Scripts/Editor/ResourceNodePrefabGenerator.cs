using UnityEngine;
using UnityEditor;

public static class ResourceNodePrefabGenerator
{
    private const string FolderPath = "Assets/Prefabs/ResourceNodes";

    [MenuItem("Tools/RTS/Generate Resource Node Prefabs")]
    public static void GenerateResourceNodePrefabs()
    {
        EnsureFolderExists();

        CreateResourceNodePrefab("WoodNode", ResourceType.Wood, PrimitiveType.Cylinder);
        CreateResourceNodePrefab("FoodNode", ResourceType.Food, PrimitiveType.Sphere);

        AssetDatabase.Refresh();
    }

    private static void CreateResourceNodePrefab(string prefabName, ResourceType resourceType, PrimitiveType shape)
    {
        string prefabPath = $"{FolderPath}/{prefabName}.prefab";

        if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
        {
            Debug.LogWarning($"[ResourceNodePrefabGenerator] Ya existe {prefabPath}, no se sobreescribe.");
            return;
        }

        GameObject go = GameObject.CreatePrimitive(shape);
        go.name = prefabName;

        int resourcesLayer = LayerMask.NameToLayer("Resources");
        if (resourcesLayer >= 0) go.layer = resourcesLayer;

        ResourceNode node = go.AddComponent<ResourceNode>();

        // ResourceType es [field: SerializeField] con setter privado: se asigna vía SerializedObject, no reflection directa
        SerializedObject serializedNode = new SerializedObject(node);
        serializedNode.FindProperty("<ResourceType>k__BackingField").enumValueIndex = (int)resourceType;
        serializedNode.ApplyModifiedProperties();

        PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        Object.DestroyImmediate(go);

        Debug.Log($"[ResourceNodePrefabGenerator] Prefab creado: {prefabPath}");
    }

    private static void EnsureFolderExists()
    {
        if (!AssetDatabase.IsValidFolder(FolderPath))
        {
            AssetDatabase.CreateFolder("Assets/Prefabs", "ResourceNodes");
        }
    }
}
