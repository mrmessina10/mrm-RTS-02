using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class BuildingPlacementSceneSetup
{
    [MenuItem("Tools/RTS/Ensure Building Placement Controller In Scene")]
    public static void EnsureBuildingPlacementController()
    {
        SelectionManager selectionManager = Object.FindFirstObjectByType<SelectionManager>();
        if (selectionManager == null)
        {
            Debug.LogWarning("[BuildingPlacementSceneSetup] No se encontró SelectionManager en la escena; no se pudo copiar cámara/inputReader/groundMask.");
            return;
        }

        BuildingPlacementController controller = Object.FindFirstObjectByType<BuildingPlacementController>();
        bool created = controller == null;

        if (created)
        {
            GameObject architecture = GameObject.Find("GameArchitecture");
            GameObject go = new GameObject("BuildingPlacementController");
            if (architecture != null) go.transform.SetParent(architecture.transform);

            controller = go.AddComponent<BuildingPlacementController>();
            Undo.RegisterCreatedObjectUndo(go, "Create BuildingPlacementController");
        }

        // Copia las mismas referencias que ya usa SelectionManager para no duplicar wiring manual
        SerializedObject sourceSO = new SerializedObject(selectionManager);
        SerializedObject targetSO = new SerializedObject(controller);

        targetSO.FindProperty("inputReader").objectReferenceValue = sourceSO.FindProperty("inputReader").objectReferenceValue;
        targetSO.FindProperty("mainCamera").objectReferenceValue = sourceSO.FindProperty("mainCamera").objectReferenceValue;
        targetSO.FindProperty("groundMask").FindPropertyRelative("m_Bits").intValue = sourceSO.FindProperty("groundMask").FindPropertyRelative("m_Bits").intValue;

        // Todos los BuildingDataSO del proyecto quedan disponibles para construir — se refresca siempre,
        // aunque el controller ya existiera, para levantar tipos de edificio nuevos sin wiring manual
        SerializedProperty availableProp = targetSO.FindProperty("availableBuildings");
        string[] guids = AssetDatabase.FindAssets("t:BuildingDataSO");
        availableProp.arraySize = guids.Length;
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            availableProp.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<BuildingDataSO>(path);
        }

        targetSO.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        string verb = created ? "creado" : "actualizado";
        Debug.Log($"[BuildingPlacementSceneSetup] BuildingPlacementController {verb} con {guids.Length} BuildingDataSO cargados. Guardá la escena (Ctrl+S).");
    }

    [MenuItem("Tools/RTS/Ensure Wall Placement Controller In Scene")]
    public static void EnsureWallPlacementController()
    {
        SelectionManager selectionManager = Object.FindFirstObjectByType<SelectionManager>();
        if (selectionManager == null)
        {
            Debug.LogWarning("[BuildingPlacementSceneSetup] No se encontró SelectionManager en la escena; no se pudo copiar cámara/inputReader/groundMask.");
            return;
        }

        BuildingDataSO wallData = FindBuildingDataByType(BuildingType.Palisade);
        if (wallData == null)
        {
            Debug.LogWarning("[BuildingPlacementSceneSetup] No hay un BuildingDataSO de tipo Palisade en el proyecto — generá los prefabs de muro primero (Tools/RTS/Generate Palisade Wall Prefabs).");
            return;
        }

        WallPlacementController controller = Object.FindFirstObjectByType<WallPlacementController>();
        bool created = controller == null;

        if (created)
        {
            GameObject architecture = GameObject.Find("GameArchitecture");
            GameObject go = new GameObject("WallPlacementController");
            if (architecture != null) go.transform.SetParent(architecture.transform);

            controller = go.AddComponent<WallPlacementController>();
            Undo.RegisterCreatedObjectUndo(go, "Create WallPlacementController");
        }

        SerializedObject sourceSO = new SerializedObject(selectionManager);
        SerializedObject targetSO = new SerializedObject(controller);

        targetSO.FindProperty("inputReader").objectReferenceValue = sourceSO.FindProperty("inputReader").objectReferenceValue;
        targetSO.FindProperty("mainCamera").objectReferenceValue = sourceSO.FindProperty("mainCamera").objectReferenceValue;
        targetSO.FindProperty("groundMask").FindPropertyRelative("m_Bits").intValue = sourceSO.FindProperty("groundMask").FindPropertyRelative("m_Bits").intValue;
        targetSO.FindProperty("wallSegmentData").objectReferenceValue = wallData;

        targetSO.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        string verb = created ? "creado" : "actualizado";
        Debug.Log($"[BuildingPlacementSceneSetup] WallPlacementController {verb}. Guardá la escena (Ctrl+S).");
    }

    private static BuildingDataSO FindBuildingDataByType(BuildingType type)
    {
        foreach (string guid in AssetDatabase.FindAssets("t:BuildingDataSO"))
        {
            BuildingDataSO data = AssetDatabase.LoadAssetAtPath<BuildingDataSO>(AssetDatabase.GUIDToAssetPath(guid));
            if (data != null && data.BuildingType == type) return data;
        }
        return null;
    }
}
