using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using Assets.RTSCamera.Scripts;

// Deja la escena de un mapa con el rig de cámara RTS tal como está armado en SampleScene: instancia el prefab
// CameraSystem con los mismos overrides (InputReader asignado, PlayerInput legacy removido, altura del rig),
// lo centra sobre el mapa y desactiva cualquier otra MainCamera que haya quedado en la escena.
public static class MapCameraSetup
{
    private const string CameraSystemPrefabPath = "Assets/Prefabs/CameraSystem.prefab";
    private const string InputReaderProperty = "inputReader";
    private const string CameraTargetProperty = "cameraTarget";
    private const float RigHeight = 14.2f;

    public static Player FindCameraRig(Scene scene)
    {
        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            Player player = rootObject.GetComponentInChildren<Player>(true);
            if (player != null) return player;
        }
        return null;
    }

    public static Player EnsureCameraRig(MapRoot mapRoot)
    {
        Scene scene = mapRoot.gameObject.scene;

        Player existing = FindCameraRig(scene);
        if (existing != null) return existing;

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CameraSystemPrefabPath);
        if (prefab == null)
        {
            Debug.LogWarning($"[MapCameraSetup] No existe {CameraSystemPrefabPath}; la escena queda sin cámara RTS.");
            return null;
        }

        DisableOtherMainCameras(scene);

        GameObject rig = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
        Player player = rig.GetComponentInChildren<Player>(true);
        if (player == null)
        {
            Debug.LogWarning($"[MapCameraSetup] {CameraSystemPrefabPath} no tiene el componente Player de la cámara RTS.");
            Object.DestroyImmediate(rig);
            return null;
        }

        PlayerInput legacyInput = rig.GetComponentInChildren<PlayerInput>(true);
        if (legacyInput != null) Object.DestroyImmediate(legacyInput);

        SerializedObject serializedPlayer = new SerializedObject(player);
        SerializedProperty inputReaderProperty = serializedPlayer.FindProperty(InputReaderProperty);
        if (inputReaderProperty.objectReferenceValue == null) inputReaderProperty.objectReferenceValue = FindInputReader();
        serializedPlayer.ApplyModifiedPropertiesWithoutUndo();

        if (inputReaderProperty.objectReferenceValue == null)
        {
            Debug.LogWarning("[MapCameraSetup] No se encontró un asset InputReader en el proyecto; asignalo a mano en CameraSystem/Player.");
        }

        Undo.RegisterCreatedObjectUndo(rig, "Mapa: cámara RTS");
        FocusCameraRig(mapRoot);
        EditorSceneManager.MarkSceneDirty(scene);

        Debug.Log("[MapCameraSetup] Cámara RTS agregada a la escena con la configuración de SampleScene.");
        return player;
    }

    public static void FocusCameraRig(MapRoot mapRoot)
    {
        Player player = FindCameraRig(mapRoot.gameObject.scene);
        if (player == null) return;

        Transform cameraTarget = new SerializedObject(player).FindProperty(CameraTargetProperty).objectReferenceValue as Transform;
        if (cameraTarget == null) return;

        Vector3 focusPoint = GetFocusPoint(mapRoot);
        Vector3 targetPosition = new Vector3(focusPoint.x, RigHeight, focusPoint.z);
        Transform rigRoot = player.transform.root;

        Undo.RecordObject(rigRoot, "Mapa: centrar cámara RTS");
        rigRoot.position += targetPosition - cameraTarget.position;
    }

    private static Vector3 GetFocusPoint(MapRoot mapRoot)
    {
        MapMarker playerStart = mapRoot.GetMarker(MapMarkerType.PlayerStart);
        if (playerStart != null) return playerStart.Position;

        MapDataSO mapData = mapRoot.MapData;
        if (mapData == null) return mapRoot.transform.position;

        return new Vector3(mapData.Origin.x + mapData.Size.x / 2f, 0f, mapData.Origin.z + mapData.Size.y / 2f);
    }

    private static InputReader FindInputReader()
    {
        string[] guids = AssetDatabase.FindAssets("t:InputReader");
        return guids.Length > 0 ? AssetDatabase.LoadAssetAtPath<InputReader>(AssetDatabase.GUIDToAssetPath(guids[0])) : null;
    }

    private static void DisableOtherMainCameras(Scene scene)
    {
        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            foreach (Camera camera in rootObject.GetComponentsInChildren<Camera>(false))
            {
                if (!camera.CompareTag("MainCamera")) continue;

                Undo.RecordObject(camera.gameObject, "Mapa: desactivar cámara anterior");
                camera.gameObject.SetActive(false);
                Debug.Log($"[MapCameraSetup] Se desactivó '{camera.name}': la MainCamera de la escena pasa a ser la del CameraSystem.");
            }
        }
    }
}
