using UnityEngine;
using UnityEditor;
using Unity.AI.Navigation;
using System.Collections.Generic;

// Ventana del editor de mapas (Tools/RTS/Map Editor). Crea/carga el mapa de la escena, aloja el panel de generación
// procedural y las herramientas (elevación, texturas, agua, objetos, caminos, marcadores, zona edificable) y resuelve
// para la herramienta activa el input sobre la Scene view: raycast contra el terreno, cursor de pincel y ciclo de trazo.
public class MapEditorWindow : EditorWindow
{
    private const float StrokeStepInterval = 1f / 40f;
    private const float MaxStrokeDeltaTime = 0.1f;
    private const float RaycastDistance = 10000f;
    private const int CursorSegments = 48;
    private const int ToolbarColumns = 4;

    private static readonly int[] MapSizes = { 64, 128, 256 };
    private static readonly string[] MapSizeLabels = { "64 x 64", "128 x 128", "256 x 256" };

    [SerializeField] private TerrainSculptTool sculptTool = new TerrainSculptTool();
    [SerializeField] private TerrainPaintTool paintTool = new TerrainPaintTool();
    [SerializeField] private WaterTool waterTool = new WaterTool();
    [SerializeField] private ObjectBrushTool objectTool = new ObjectBrushTool();
    [SerializeField] private PathTool pathTool = new PathTool();
    [SerializeField] private MarkerTool markerTool = new MarkerTool();
    [SerializeField] private BuildZoneTool buildZoneTool = new BuildZoneTool();
    [SerializeField] private MapGeneratorPanel generatorPanel = new MapGeneratorPanel();

    [SerializeField] private int activeToolIndex = -1;
    [SerializeField] private MapPaletteSO palette;
    [SerializeField] private string newMapName = "Map01";
    [SerializeField] private int newMapSizeIndex = 1;
    [SerializeField] private MapDataSO mapDataToLoad;
    [SerializeField] private Vector2 scrollPosition;

    private readonly MapEditorContext context = new MapEditorContext();
    private MapEditorTool[] tools;
    private string[] toolLabels;
    private MapRoot mapRoot;
    private List<MapValidationMessage> validationMessages;
    private bool isStroking;
    private Ray lastMouseRay;
    private double lastStrokeStepTime;
    private int strokeUndoGroup;

    private MapEditorTool ActiveTool => activeToolIndex >= 0 && activeToolIndex < tools.Length ? tools[activeToolIndex] : null;

    [MenuItem("Tools/RTS/Map Editor")]
    public static void Open()
    {
        GetWindow<MapEditorWindow>("Map Editor");
    }

    private void OnEnable()
    {
        tools = new MapEditorTool[] { sculptTool, paintTool, waterTool, objectTool, pathTool, markerTool, buildZoneTool };

        toolLabels = new string[tools.Length + 1];
        toolLabels[0] = "Ninguna";
        for (int i = 0; i < tools.Length; i++)
        {
            toolLabels[i + 1] = tools[i].DisplayName;
        }

        SceneView.duringSceneGui += OnSceneGUI;
        EditorApplication.update += OnEditorUpdate;
        Undo.undoRedoPerformed += OnUndoRedo;

        Tools.hidden = ActiveTool != null;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        EditorApplication.update -= OnEditorUpdate;
        Undo.undoRedoPerformed -= OnUndoRedo;

        EndStroke();
        if (ActiveTool != null) ActiveTool.OnDeactivated();
        Tools.hidden = false;
        generatorPanel.Dispose();
    }

    private void OnHierarchyChange()
    {
        mapRoot = null;
        Repaint();
    }

    private void OnInspectorUpdate()
    {
        Repaint();
    }

    private void OnUndoRedo()
    {
        if (ActiveTool != null) ActiveTool.OnUndoRedo();
        Repaint();
    }

    private bool RefreshContext()
    {
        if (mapRoot == null) mapRoot = MapSceneSetup.FindMapRoot();
        if (palette == null) palette = FindPalette();

        context.Root = mapRoot;
        context.Terrain = mapRoot != null ? mapRoot.Terrain : null;
        context.MapData = mapRoot != null ? mapRoot.MapData : null;
        context.Palette = palette;

        return context.Root != null && context.Terrain != null && context.MapData != null;
    }

    private static MapPaletteSO FindPalette()
    {
        string[] guids = AssetDatabase.FindAssets("t:MapPaletteSO");
        return guids.Length > 0 ? AssetDatabase.LoadAssetAtPath<MapPaletteSO>(AssetDatabase.GUIDToAssetPath(guids[0])) : null;
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        if (!RefreshContext())
        {
            SetActiveTool(-1);
            DrawCreateMapSection();
            EditorGUILayout.EndScrollView();
            return;
        }

        DrawMapSection();
        EditorGUILayout.Space();

        if (generatorPanel.OnGUI(context))
        {
            SetActiveTool(-1);
            validationMessages = null;
            GUIUtility.ExitGUI();
        }

        EditorGUILayout.Space();
        DrawToolSection();

        EditorGUILayout.EndScrollView();
    }

    private void DrawCreateMapSection()
    {
        if (mapRoot != null)
        {
            EditorGUILayout.HelpBox($"El MapRoot '{mapRoot.name}' no tiene Terrain o MapData asignado.", MessageType.Error);
            return;
        }

        EditorGUILayout.LabelField("Crear mapa", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("La escena activa no tiene un mapa. Se crea un Terrain centrado en el origen con el suelo base en Y = 0, junto con su MapData y TerrainData en Assets/Maps.", MessageType.Info);

        newMapName = EditorGUILayout.TextField("Nombre", newMapName);
        newMapSizeIndex = EditorGUILayout.Popup("Tamaño (celdas)", newMapSizeIndex, MapSizeLabels);

        using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(newMapName)))
        {
            if (GUILayout.Button("Crear mapa en la escena", GUILayout.Height(28f)))
            {
                mapRoot = MapSceneSetup.CreateMap(newMapName.Trim(), MapSizes[newMapSizeIndex]);
                GUIUtility.ExitGUI();
            }
        }
    }

    private void DrawMapSection()
    {
        EditorGUILayout.LabelField("Mapa", EditorStyles.boldLabel);

        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.ObjectField("MapData activo", context.MapData, typeof(MapDataSO), false);
        }
        palette = (MapPaletteSO)EditorGUILayout.ObjectField("Paleta", palette, typeof(MapPaletteSO), false);

        DrawForeignGroundWarning();
        DrawCameraRigSection();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Bake NavMesh")) MapSceneSetup.BakeNavMesh(mapRoot);
        if (GUILayout.Button("Validar mapa")) validationMessages = MapValidator.Validate(mapRoot);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Guardar escena → MapData"))
        {
            MapSceneSync.CaptureSceneToMapData(mapRoot);
            AssetDatabase.SaveAssetIfDirty(mapRoot.MapData);
            Debug.Log($"[MapEditorWindow] {mapRoot.MapData.name} actualizado: {mapRoot.MapData.Objects.Count} objetos, {mapRoot.MapData.Markers.Count} marcadores, {mapRoot.MapData.Paths.Count} caminos.");
        }
        if (GUILayout.Button("Reconstruir escena ← MapData")
            && EditorUtility.DisplayDialog("Reconstruir escena", "Se reemplazan los objetos, marcadores y caminos de la escena por los guardados en el MapData.", "Reconstruir", "Cancelar"))
        {
            MapSceneSync.RebuildSceneFromMapData(mapRoot);
            GUIUtility.ExitGUI();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        mapDataToLoad = (MapDataSO)EditorGUILayout.ObjectField("Cargar otro MapData", mapDataToLoad, typeof(MapDataSO), false);
        using (new EditorGUI.DisabledScope(mapDataToLoad == null || mapDataToLoad == context.MapData))
        {
            if (GUILayout.Button("Cargar", GUILayout.Width(60f))
                && EditorUtility.DisplayDialog("Cargar MapData", $"Se reemplaza el mapa de la escena por '{mapDataToLoad.name}'. Los cambios no guardados en el MapData actual se pierden.", "Cargar", "Cancelar"))
            {
                MapSceneSync.LoadMapData(mapRoot, mapDataToLoad);
                mapDataToLoad = null;
                GUIUtility.ExitGUI();
            }
        }
        EditorGUILayout.EndHorizontal();

        if (validationMessages == null) return;

        foreach (MapValidationMessage message in validationMessages)
        {
            EditorGUILayout.HelpBox(message.Text, message.Type);
        }
    }

    private void DrawCameraRigSection()
    {
        if (MapCameraSetup.FindCameraRig(mapRoot.gameObject.scene) != null)
        {
            if (GUILayout.Button("Centrar cámara RTS en el Inicio del jugador (o el centro del mapa)")) MapCameraSetup.FocusCameraRig(mapRoot);
            return;
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.HelpBox("La escena no tiene la cámara RTS (prefab CameraSystem).", MessageType.Warning);
        if (GUILayout.Button("Agregar", GUILayout.Width(80f), GUILayout.Height(38f)))
        {
            MapCameraSetup.EnsureCameraRig(mapRoot);
            GUIUtility.ExitGUI();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawForeignGroundWarning()
    {
        List<NavMeshSurface> foreignSurfaces = MapSceneSetup.FindForeignNavMeshSurfaces(mapRoot);
        if (foreignSurfaces.Count == 0) return;

        foreach (NavMeshSurface surface in foreignSurfaces)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.HelpBox($"'{surface.name}' tiene otro NavMeshSurface activo (suelo anterior). Mientras siga activo se superpone con el terreno del mapa.", MessageType.Warning);
            if (GUILayout.Button("Desactivar", GUILayout.Width(80f), GUILayout.Height(38f)))
            {
                Undo.RecordObject(surface.gameObject, "Desactivar suelo anterior");
                surface.gameObject.SetActive(false);
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    private void DrawToolSection()
    {
        EditorGUILayout.LabelField("Herramienta", EditorStyles.boldLabel);

        int selected = GUILayout.SelectionGrid(activeToolIndex + 1, toolLabels, ToolbarColumns) - 1;
        if (selected != activeToolIndex) SetActiveTool(selected);

        MapEditorTool tool = ActiveTool;
        if (tool == null)
        {
            EditorGUILayout.HelpBox("Elegí una herramienta para editar el mapa sobre la Scene view. Con 'Ninguna' la Scene view vuelve a su comportamiento normal (selección y gizmos de Unity).", MessageType.Info);
            return;
        }

        EditorGUILayout.HelpBox(tool.Hint, MessageType.Info);
        tool.OnToolGUI(context);
    }

    private void SetActiveTool(int toolIndex)
    {
        if (toolIndex == activeToolIndex) return;

        EndStroke();
        if (ActiveTool != null) ActiveTool.OnDeactivated();

        activeToolIndex = toolIndex;
        Tools.hidden = ActiveTool != null;

        if (ActiveTool != null) ActiveTool.OnActivated(context);
        SceneView.RepaintAll();
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        MapEditorTool tool = ActiveTool;
        if (tool == null || !RefreshContext()) return;

        Event currentEvent = Event.current;
        context.Shift = currentEvent.shift;
        context.Control = currentEvent.control || currentEvent.command;

        if (currentEvent.isMouse)
        {
            lastMouseRay = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);
            UpdateHit();
        }

        if (currentEvent.type == EventType.KeyDown && currentEvent.keyCode == KeyCode.Escape)
        {
            SetActiveTool(-1);
            currentEvent.Use();
            Repaint();
            return;
        }

        Handles.BeginGUI();
        GUI.Label(new Rect(12f, 8f, 420f, 20f), $"Map Editor — {tool.DisplayName} (Esc para soltar la herramienta)", EditorStyles.whiteBoldLabel);
        Handles.EndGUI();

        int controlId = GUIUtility.GetControlID(FocusType.Passive);
        tool.OnSceneGUI(context);

        switch (currentEvent.GetTypeForControl(controlId))
        {
            case EventType.Layout:
                HandleUtility.AddDefaultControl(controlId);
                break;

            case EventType.MouseDown:
                if (currentEvent.button == 0 && !currentEvent.alt && context.HasHit && HandleUtility.nearestControl == controlId)
                {
                    GUIUtility.hotControl = controlId;
                    BeginStroke();
                    currentEvent.Use();
                }
                break;

            case EventType.MouseDrag:
                if (GUIUtility.hotControl == controlId) currentEvent.Use();
                break;

            case EventType.MouseUp:
                if (GUIUtility.hotControl == controlId)
                {
                    GUIUtility.hotControl = 0;
                    EndStroke();
                    currentEvent.Use();
                }
                break;

            case EventType.MouseMove:
                sceneView.Repaint();
                break;

            case EventType.Repaint:
                DrawBrushCursor(tool);
                break;
        }
    }

    private void UpdateHit()
    {
        TerrainCollider terrainCollider = context.Terrain.GetComponent<TerrainCollider>();
        if (terrainCollider != null && terrainCollider.Raycast(lastMouseRay, out RaycastHit hit, RaycastDistance))
        {
            context.HasHit = true;
            context.HitPoint = hit.point;
        }
        else
        {
            context.HasHit = false;
        }
    }

    private void BeginStroke()
    {
        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName($"Mapa: {ActiveTool.DisplayName}");
        strokeUndoGroup = Undo.GetCurrentGroup();

        isStroking = true;
        lastStrokeStepTime = EditorApplication.timeSinceStartup;
        ActiveTool.OnStrokeBegin(context);
    }

    private void EndStroke()
    {
        if (!isStroking) return;

        isStroking = false;
        if (ActiveTool != null && RefreshContext()) ActiveTool.OnStrokeEnd(context);

        Undo.CollapseUndoOperations(strokeUndoGroup);
        SceneView.RepaintAll();
    }

    private void OnEditorUpdate()
    {
        if (!isStroking) return;

        MapEditorTool tool = ActiveTool;
        if (tool == null || !RefreshContext())
        {
            isStroking = false;
            return;
        }

        double now = EditorApplication.timeSinceStartup;
        float deltaTime = (float)(now - lastStrokeStepTime);
        if (deltaTime < StrokeStepInterval) return;

        lastStrokeStepTime = now;
        UpdateHit();
        if (context.HasHit) tool.OnStrokeStep(context, Mathf.Min(deltaTime, MaxStrokeDeltaTime));

        SceneView.RepaintAll();
    }

    private void DrawBrushCursor(MapEditorTool tool)
    {
        float radius = tool.GetBrushRadius(context);
        if (radius <= 0f || !context.HasHit) return;

        Vector3[] points = new Vector3[CursorSegments + 1];
        for (int i = 0; i <= CursorSegments; i++)
        {
            float angle = i / (float)CursorSegments * Mathf.PI * 2f;
            Vector3 point = context.HitPoint + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
            points[i] = context.SnapToTerrain(point) + Vector3.up * 0.05f;
        }

        Handles.color = tool.GetBrushColor(context);
        Handles.DrawAAPolyLine(3f, points);
    }
}
