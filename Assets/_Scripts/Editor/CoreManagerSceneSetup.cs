using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class CoreManagerSceneSetup
{
    [MenuItem("Tools/RTS/Ensure Core Managers In Scene")]
    public static void EnsureCoreManagers()
    {
        Transform parent = FindArchitectureParent();

        bool created = false;
        created |= EnsureManager<ResourceManager>("ResourceManager", parent);
        created |= EnsureManager<BuildingManager>("BuildingManager", parent);

        if (created)
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("[CoreManagerSceneSetup] Manager(s) agregados a la escena. Guardá la escena (Ctrl+S) para persistir el cambio.");
        }
        else
        {
            Debug.Log("[CoreManagerSceneSetup] Ya estaban los dos managers en la escena, no se creó nada.");
        }
    }

    private static Transform FindArchitectureParent()
    {
        // Mismo contenedor donde ya viven GlobalUnitManager/GameManager/SelectionManager en la escena
        GameObject architecture = GameObject.Find("GameArchitecture");
        return architecture != null ? architecture.transform : null;
    }

    private static bool EnsureManager<T>(string objectName, Transform parent) where T : Component
    {
        if (Object.FindFirstObjectByType<T>() != null) return false;

        GameObject go = new GameObject(objectName);
        if (parent != null) go.transform.SetParent(parent);
        go.AddComponent<T>();
        Undo.RegisterCreatedObjectUndo(go, $"Create {objectName}");

        return true;
    }
}
