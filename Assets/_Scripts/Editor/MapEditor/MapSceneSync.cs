using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

// Sincroniza la copia de trabajo de la escena con el MapDataSO: Capture vuelca objetos, marcadores y caminos de la
// escena al asset (también automáticamente al guardar la escena) y Rebuild reconstruye la escena desde el asset.
[InitializeOnLoad]
public static class MapSceneSync
{
    private const string WaypointsProperty = "waypoints";
    private const string WidthProperty = "<Width>k__BackingField";

    private static readonly MapObjectCategory[] AllCategories = { MapObjectCategory.Resource, MapObjectCategory.Nature, MapObjectCategory.ManMade };

    static MapSceneSync()
    {
        EditorSceneManager.sceneSaving += OnSceneSaving;
    }

    private static void OnSceneSaving(Scene scene, string path)
    {
        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            foreach (MapRoot mapRoot in rootObject.GetComponentsInChildren<MapRoot>(true))
            {
                if (mapRoot.MapData == null) continue;

                CaptureSceneToMapData(mapRoot);
                AssetDatabase.SaveAssetIfDirty(mapRoot.MapData);
            }
        }
    }

    public static void CaptureSceneToMapData(MapRoot mapRoot)
    {
        List<MapObjectEntry> objects = new List<MapObjectEntry>();
        int skipped = 0;

        foreach (MapObjectCategory category in AllCategories)
        {
            Transform container = mapRoot.GetContainer(category);
            if (container == null) continue;

            for (int i = 0; i < container.childCount; i++)
            {
                Transform child = container.GetChild(i);
                GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource(child.gameObject);
                if (prefab == null)
                {
                    skipped++;
                    continue;
                }

                objects.Add(new MapObjectEntry
                {
                    Prefab = prefab,
                    Category = category,
                    Position = child.position,
                    Rotation = child.rotation,
                    Scale = child.localScale
                });
            }
        }

        List<MapMarkerEntry> markers = new List<MapMarkerEntry>();
        foreach (MapMarker marker in mapRoot.GetMarkers())
        {
            markers.Add(new MapMarkerEntry { Type = marker.MarkerType, Position = marker.Position, Rotation = marker.transform.rotation });
        }

        List<MapPathEntry> paths = new List<MapPathEntry>();
        foreach (MapPath path in mapRoot.GetPaths())
        {
            paths.Add(new MapPathEntry { Width = path.Width, Waypoints = new List<Vector3>(path.Waypoints) });
        }

        mapRoot.MapData.SetContents(objects, markers, paths);
        EditorUtility.SetDirty(mapRoot.MapData);

        if (skipped > 0)
        {
            Debug.LogWarning($"[MapSceneSync] {skipped} objeto(s) del mapa no son instancias de prefab y no se guardaron en {mapRoot.MapData.name}.");
        }
    }

    public static void RebuildSceneFromMapData(MapRoot mapRoot)
    {
        MapDataSO mapData = mapRoot.MapData;

        foreach (MapObjectCategory category in AllCategories)
        {
            ClearChildren(mapRoot.GetContainer(category));
        }
        ClearChildren(mapRoot.MarkersContainer);
        ClearChildren(mapRoot.PathsContainer);

        foreach (MapObjectEntry entry in mapData.Objects)
        {
            if (entry.Prefab == null) continue;

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(entry.Prefab, mapRoot.GetContainer(entry.Category));
            instance.transform.SetPositionAndRotation(entry.Position, entry.Rotation);
            instance.transform.localScale = entry.Scale;
            Undo.RegisterCreatedObjectUndo(instance, "Mapa: reconstruir desde MapData");
        }

        foreach (MapMarkerEntry entry in mapData.Markers)
        {
            MarkerTool.CreateMarker(mapRoot, entry.Type, entry.Position, entry.Rotation);
        }

        for (int i = 0; i < mapData.Paths.Count; i++)
        {
            MapPathEntry entry = mapData.Paths[i];

            GameObject pathObject = new GameObject($"Path {i + 1}");
            pathObject.transform.SetParent(mapRoot.PathsContainer, false);

            SerializedObject serializedPath = new SerializedObject(pathObject.AddComponent<MapPath>());
            serializedPath.FindProperty(WidthProperty).floatValue = entry.Width;

            SerializedProperty waypoints = serializedPath.FindProperty(WaypointsProperty);
            waypoints.arraySize = entry.Waypoints.Count;
            for (int j = 0; j < entry.Waypoints.Count; j++)
            {
                waypoints.GetArrayElementAtIndex(j).vector3Value = entry.Waypoints[j];
            }
            serializedPath.ApplyModifiedPropertiesWithoutUndo();

            Undo.RegisterCreatedObjectUndo(pathObject, "Mapa: reconstruir desde MapData");
        }

        if (mapRoot.Terrain != null && mapData.TerrainData != null && mapRoot.Terrain.terrainData != mapData.TerrainData)
        {
            Undo.RecordObject(mapRoot.Terrain, "Mapa: reconstruir desde MapData");
            mapRoot.Terrain.terrainData = mapData.TerrainData;

            TerrainCollider terrainCollider = mapRoot.Terrain.GetComponent<TerrainCollider>();
            Undo.RecordObject(terrainCollider, "Mapa: reconstruir desde MapData");
            terrainCollider.terrainData = mapData.TerrainData;
        }

        if (mapRoot.Terrain != null)
        {
            Undo.RecordObject(mapRoot.Terrain.transform, "Mapa: reconstruir desde MapData");
            mapRoot.Terrain.transform.position = mapData.Origin;
        }

        if (mapRoot.Water != null)
        {
            Undo.RecordObject(mapRoot.Water, "Mapa: reconstruir desde MapData");
            mapRoot.Water.position = new Vector3(mapRoot.Water.position.x, mapData.WaterLevel, mapRoot.Water.position.z);
        }

        EditorSceneManager.MarkSceneDirty(mapRoot.gameObject.scene);
        Debug.Log($"[MapSceneSync] Escena reconstruida desde {mapData.name}: {mapData.Objects.Count} objetos, {mapData.Markers.Count} marcadores, {mapData.Paths.Count} caminos.");
    }

    public static void LoadMapData(MapRoot mapRoot, MapDataSO mapData)
    {
        SerializedObject serializedRoot = new SerializedObject(mapRoot);
        serializedRoot.FindProperty("<MapData>k__BackingField").objectReferenceValue = mapData;
        serializedRoot.ApplyModifiedProperties();

        RebuildSceneFromMapData(mapRoot);
    }

    private static void ClearChildren(Transform container)
    {
        if (container == null) return;

        for (int i = container.childCount - 1; i >= 0; i--)
        {
            Undo.DestroyObjectImmediate(container.GetChild(i).gameObject);
        }
    }
}
