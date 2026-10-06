using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

// Herramienta de caminos: edita los waypoints de cada MapPath sobre la Scene view y vuelca el camino al terreno
// (pintar la capa de camino, nivelar el relieve debajo, bloquear la construcción sobre su ancho).
[System.Serializable]
public class PathTool : MapEditorTool
{
    private const string WaypointsProperty = "waypoints";
    private const string WidthProperty = "<Width>k__BackingField";
    private const float HandleSizeFactor = 0.12f;

    [SerializeField] private int activePathIndex;
    [SerializeField] private float buildBlockMargin = 1f;

    public override string DisplayName => "Caminos";
    public override string Hint => "Click agrega un waypoint al final. Ctrl + click lo inserta en el tramo más cercano. Arrastrar un punto lo mueve. Shift + click sobre un punto lo borra.";

    public override void OnToolGUI(MapEditorContext context)
    {
        MapPath[] paths = context.Root.GetPaths();

        for (int i = 0; i < paths.Length; i++)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Toggle(i == activePathIndex, $"{paths[i].name} ({paths[i].Waypoints.Count} puntos)", EditorStyles.radioButton)) activePathIndex = i;
            if (GUILayout.Button("Borrar", GUILayout.Width(60f)))
            {
                Undo.DestroyObjectImmediate(paths[i].gameObject);
                activePathIndex = 0;
                GUIUtility.ExitGUI();
            }
            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Nuevo camino"))
        {
            CreatePath(context);
            activePathIndex = paths.Length;
            return;
        }

        MapPath activePath = GetActivePath(context);
        if (activePath == null)
        {
            EditorGUILayout.HelpBox("No hay caminos todavía. El primer click sobre el terreno crea uno.", MessageType.Info);
            return;
        }

        EditorGUILayout.Space();

        SerializedObject serializedPath = new SerializedObject(activePath);
        EditorGUILayout.Slider(serializedPath.FindProperty(WidthProperty), 1f, 20f, "Ancho");
        serializedPath.ApplyModifiedProperties();

        buildBlockMargin = EditorGUILayout.Slider("Margen no edificable", buildBlockMargin, 0f, 8f);
        EditorGUILayout.Space();

        using (new EditorGUI.DisabledScope(activePath.Waypoints.Count < 2))
        {
            if (GUILayout.Button("Pintar camino sobre el terreno")) PaintPath(context, activePath);
            if (GUILayout.Button("Nivelar terreno bajo el camino")) LevelPath(context, activePath);
            if (GUILayout.Button("Bloquear construcción sobre el camino")) BlockPath(context, activePath);
        }
    }

    public override void OnSceneGUI(MapEditorContext context)
    {
        MapPath activePath = GetActivePath(context);
        if (activePath == null) return;

        SerializedObject serializedPath = new SerializedObject(activePath);
        SerializedProperty waypoints = serializedPath.FindProperty(WaypointsProperty);

        for (int i = 0; i < waypoints.arraySize; i++)
        {
            SerializedProperty waypoint = waypoints.GetArrayElementAtIndex(i);
            Vector3 position = waypoint.vector3Value;
            float handleSize = HandleUtility.GetHandleSize(position) * HandleSizeFactor;

            Handles.Label(position + Vector3.up * 1.2f, i.ToString());

            if (context.Shift)
            {
                Handles.color = Color.red;
                if (Handles.Button(position, Quaternion.identity, handleSize, handleSize * 1.5f, Handles.SphereHandleCap))
                {
                    waypoints.DeleteArrayElementAtIndex(i);
                    break;
                }
                continue;
            }

            Handles.color = Color.yellow;
            EditorGUI.BeginChangeCheck();
            Handles.FreeMoveHandle(position, handleSize, Vector3.zero, Handles.SphereHandleCap);
            if (EditorGUI.EndChangeCheck() && context.HasHit)
            {
                waypoint.vector3Value = context.HitPoint;
            }
        }

        serializedPath.ApplyModifiedProperties();
    }

    public override void OnStrokeBegin(MapEditorContext context)
    {
        if (context.Shift) return;

        MapPath activePath = GetActivePath(context);
        if (activePath == null)
        {
            activePath = CreatePath(context);
            activePathIndex = context.Root.GetPaths().Length - 1;
        }

        SerializedObject serializedPath = new SerializedObject(activePath);
        SerializedProperty waypoints = serializedPath.FindProperty(WaypointsProperty);

        int insertIndex = context.Control ? FindInsertIndex(activePath, context.HitPoint) : waypoints.arraySize;
        waypoints.InsertArrayElementAtIndex(insertIndex);
        waypoints.GetArrayElementAtIndex(insertIndex).vector3Value = context.HitPoint;
        serializedPath.ApplyModifiedProperties();
    }

    private MapPath GetActivePath(MapEditorContext context)
    {
        MapPath[] paths = context.Root.GetPaths();
        if (paths.Length == 0) return null;

        activePathIndex = Mathf.Clamp(activePathIndex, 0, paths.Length - 1);
        return paths[activePathIndex];
    }

    private static MapPath CreatePath(MapEditorContext context)
    {
        Transform container = context.Root.PathsContainer;
        GameObject pathObject = new GameObject($"Path {container.childCount + 1}");
        pathObject.transform.SetParent(container, false);

        MapPath path = pathObject.AddComponent<MapPath>();
        Undo.RegisterCreatedObjectUndo(pathObject, "Mapa: nuevo camino");
        return path;
    }

    private static int FindInsertIndex(MapPath path, Vector3 point)
    {
        IReadOnlyList<Vector3> waypoints = path.Waypoints;
        if (waypoints.Count < 2) return waypoints.Count;

        int bestSegment = 0;
        float bestDistance = float.MaxValue;
        for (int i = 0; i < waypoints.Count - 1; i++)
        {
            float distance = HandleUtility.DistancePointLine(point, waypoints[i], waypoints[i + 1]);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestSegment = i;
            }
        }
        return bestSegment + 1;
    }

    public static void PaintPath(MapEditorContext context, MapPath path)
    {
        int roadLayer = TerrainBrushUtility.FindLayerIndex(context.TerrainData, MapEditorAssetGenerator.RoadLayerName);
        if (roadLayer < 0)
        {
            Debug.LogWarning($"[PathTool] El terreno no tiene una capa llamada '{MapEditorAssetGenerator.RoadLayerName}'.");
            return;
        }

        float radius = path.Width / 2f + 0.75f;
        float hardness = (path.Width / 2f) / radius;

        TerrainBrushUtility.RegisterAlphamapUndo(context.Terrain, "Mapa: pintar camino");
        foreach (Vector3 point in path.GetSmoothedPoints(1f))
        {
            TerrainBrushUtility.PaintLayer(context.Terrain, point, radius, hardness, roadLayer, 1f);
        }
        TerrainBrushUtility.FinishAlphamapEdit(context.Terrain);
    }

    private static void LevelPath(MapEditorContext context, MapPath path)
    {
        TerrainBrushUtility.RegisterHeightUndo(context.Terrain, "Mapa: nivelar camino");
        TerrainBrushUtility.LevelAlongPolyline(context.Terrain, path.GetSmoothedPoints(1f), path.Width, path.Width * 0.5f);
        TerrainBrushUtility.FinishHeightEdit(context.Terrain);

        SerializedObject serializedPath = new SerializedObject(path);
        SerializedProperty waypoints = serializedPath.FindProperty(WaypointsProperty);
        for (int i = 0; i < waypoints.arraySize; i++)
        {
            SerializedProperty waypoint = waypoints.GetArrayElementAtIndex(i);
            waypoint.vector3Value = context.SnapToTerrain(waypoint.vector3Value);
        }
        serializedPath.ApplyModifiedProperties();
    }

    private void BlockPath(MapEditorContext context, MapPath path)
    {
        MapDataSO mapData = context.MapData;
        float reach = path.Width / 2f + buildBlockMargin;
        int cellReach = Mathf.CeilToInt(reach);

        Undo.RegisterCompleteObjectUndo(mapData, "Mapa: bloquear camino");
        foreach (Vector3 point in path.GetSmoothedPoints(1f))
        {
            Vector2Int centerCell = mapData.WorldToCell(point);
            for (int x = -cellReach; x <= cellReach; x++)
            {
                for (int z = -cellReach; z <= cellReach; z++)
                {
                    Vector2Int cell = centerCell + new Vector2Int(x, z);
                    Vector3 offset = mapData.CellToWorldCenter(cell) - point;
                    offset.y = 0f;
                    if (offset.magnitude <= reach) mapData.SetCellBuildable(cell, false);
                }
            }
        }
        EditorUtility.SetDirty(mapData);

        Debug.Log($"[PathTool] Celdas bajo '{path.name}' marcadas como no edificables (ancho {path.Width} + margen {buildBlockMargin}).");
    }
}
