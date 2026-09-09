using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class BuildingPlacementSceneSetup
{
    [MenuItem("Tools/RTS/Ensure Building Placement Controller In Scene")]
    public static void EnsureBuildingPlacementController()
    {
        if (Object.FindFirstObjectByType<BuildingPlacementController>() != null)
        {
            Debug.Log("[BuildingPlacementSceneSetup] Ya existe un BuildingPlacementController en la escena, no se creó nada.");
            return;
        }

        SelectionManager selectionManager = Object.FindFirstObjectByType<SelectionManager>();
        if (selectionManager == null)
        {
            Debug.LogWarning("[BuildingPlacementSceneSetup] No se encontró SelectionManager en la escena; no se pudo copiar cámara/inputReader/groundMask.");
            return;
        }

        GameObject architecture = GameObject.Find("GameArchitecture");
        GameObject go = new GameObject("BuildingPlacementController");
        if (architecture != null) go.transform.SetParent(architecture.transform);

        BuildingPlacementController controller = go.AddComponent<BuildingPlacementController>();
        Undo.RegisterCreatedObjectUndo(go, "Create BuildingPlacementController");

        // Copia las mismas referencias que ya usa SelectionManager para no duplicar wiring manual
        SerializedObject sourceSO = new SerializedObject(selectionManager);
        SerializedObject targetSO = new SerializedObject(controller);

        targetSO.FindProperty("inputReader").objectReferenceValue = sourceSO.FindProperty("inputReader").objectReferenceValue;
        targetSO.FindProperty("mainCamera").objectReferenceValue = sourceSO.FindProperty("mainCamera").objectReferenceValue;
        targetSO.FindProperty("groundMask").FindPropertyRelative("m_Bits").intValue = sourceSO.FindProperty("groundMask").FindPropertyRelative("m_Bits").intValue;

        // Todos los BuildingDataSO del proyecto quedan disponibles para construir
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
        Debug.Log($"[BuildingPlacementSceneSetup] BuildingPlacementController creado con {guids.Length} BuildingDataSO cargados. Guardá la escena (Ctrl+S).");
    }
}
