using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using System.Collections.Generic;

// Herramienta de zona edificable: pinta por celda la máscara del MapDataSO que BuildingPlacement consulta en runtime
// y la muestra como overlay rojo (celdas bloqueadas) apoyado sobre el relieve.
[System.Serializable]
public class BuildZoneTool : MapEditorTool
{
    private const float OverlayHeightOffset = 0.08f;

    private static readonly string[] PaintLabels = { "Pintar bloqueado", "Pintar edificable" };
    private static readonly Color BlockedColor = new Color(1f, 0.15f, 0.1f, 0.35f);

    [SerializeField] private int paintBuildable;
    [SerializeField] private float radius = 4f;

    [System.NonSerialized] private Mesh overlayMesh;
    [System.NonSerialized] private Material overlayMaterial;
    [System.NonSerialized] private bool overlayDirty = true;

    public override string DisplayName => "Zona edificable";
    public override string Hint => "Arrastrar para pintar celdas. Shift invierte el modo. Rojo = no se puede construir.";

    public override float GetBrushRadius(MapEditorContext context) => radius;
    public override Color GetBrushColor(MapEditorContext context) => PaintsBuildable(context) ? Color.green : Color.red;

    public override void OnActivated(MapEditorContext context)
    {
        overlayDirty = true;
    }

    public override void OnDeactivated()
    {
        if (overlayMesh != null) Object.DestroyImmediate(overlayMesh);
        if (overlayMaterial != null) Object.DestroyImmediate(overlayMaterial);
    }

    public override void OnUndoRedo()
    {
        overlayDirty = true;
    }

    public override void OnToolGUI(MapEditorContext context)
    {
        paintBuildable = GUILayout.Toolbar(paintBuildable, PaintLabels);
        radius = EditorGUILayout.Slider("Radio (celdas)", radius, 0.5f, 30f);
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Todo edificable")) SetAll(context, true);
        if (GUILayout.Button("Nada edificable")) SetAll(context, false);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.HelpBox("Para una zona de construcción designada: 'Nada edificable' y después pintar la zona. Para bloquear solo los caminos: herramienta Caminos → 'Bloquear construcción sobre el camino'. El agua, los acantilados y los obstáculos ya quedan excluidos por el NavMesh.", MessageType.None);
    }

    public override void OnSceneGUI(MapEditorContext context)
    {
        if (Event.current.type != EventType.Repaint) return;

        if (overlayDirty || overlayMesh == null) RebuildOverlay(context);
        if (overlayMaterial == null) overlayMaterial = CreateOverlayMaterial();

        overlayMaterial.SetPass(0);
        Graphics.DrawMeshNow(overlayMesh, Matrix4x4.identity);
    }

    public override void OnStrokeBegin(MapEditorContext context)
    {
        Undo.RegisterCompleteObjectUndo(context.MapData, "Mapa: zona edificable");
    }

    public override void OnStrokeStep(MapEditorContext context, float deltaTime)
    {
        MapDataSO mapData = context.MapData;
        bool buildable = PaintsBuildable(context);
        Vector2Int centerCell = mapData.WorldToCell(context.HitPoint);
        int cellReach = Mathf.CeilToInt(radius);

        for (int x = -cellReach; x <= cellReach; x++)
        {
            for (int z = -cellReach; z <= cellReach; z++)
            {
                Vector2Int cell = centerCell + new Vector2Int(x, z);
                Vector3 offset = mapData.CellToWorldCenter(cell) - context.HitPoint;
                offset.y = 0f;
                if (offset.magnitude <= radius) mapData.SetCellBuildable(cell, buildable);
            }
        }

        overlayDirty = true;
    }

    public override void OnStrokeEnd(MapEditorContext context)
    {
        EditorUtility.SetDirty(context.MapData);
    }

    private bool PaintsBuildable(MapEditorContext context)
    {
        return (paintBuildable == 1) != context.Shift;
    }

    private void SetAll(MapEditorContext context, bool buildable)
    {
        Undo.RegisterCompleteObjectUndo(context.MapData, "Mapa: zona edificable");
        context.MapData.SetAllBuildable(buildable);
        EditorUtility.SetDirty(context.MapData);
        overlayDirty = true;
        SceneView.RepaintAll();
    }

    private void RebuildOverlay(MapEditorContext context)
    {
        MapDataSO mapData = context.MapData;
        TerrainData terrainData = context.TerrainData;
        Vector2Int size = mapData.Size;
        float terrainY = context.Terrain.transform.position.y;

        float[,] cornerHeights = new float[size.x + 1, size.y + 1];
        for (int x = 0; x <= size.x; x++)
        {
            for (int z = 0; z <= size.y; z++)
            {
                cornerHeights[x, z] = terrainY + terrainData.GetInterpolatedHeight(x / (float)size.x, z / (float)size.y) + OverlayHeightOffset;
            }
        }

        List<Vector3> vertices = new List<Vector3>();
        List<Color> colors = new List<Color>();
        List<int> triangles = new List<int>();

        for (int x = 0; x < size.x; x++)
        {
            for (int z = 0; z < size.y; z++)
            {
                if (mapData.IsCellBuildable(new Vector2Int(x, z))) continue;

                float worldX = mapData.Origin.x + x;
                float worldZ = mapData.Origin.z + z;
                int firstVertex = vertices.Count;

                vertices.Add(new Vector3(worldX, cornerHeights[x, z], worldZ));
                vertices.Add(new Vector3(worldX, cornerHeights[x, z + 1], worldZ + 1f));
                vertices.Add(new Vector3(worldX + 1f, cornerHeights[x + 1, z + 1], worldZ + 1f));
                vertices.Add(new Vector3(worldX + 1f, cornerHeights[x + 1, z], worldZ));

                for (int i = 0; i < 4; i++) colors.Add(BlockedColor);

                triangles.Add(firstVertex);
                triangles.Add(firstVertex + 1);
                triangles.Add(firstVertex + 2);
                triangles.Add(firstVertex);
                triangles.Add(firstVertex + 2);
                triangles.Add(firstVertex + 3);
            }
        }

        if (overlayMesh == null)
        {
            overlayMesh = new Mesh { hideFlags = HideFlags.HideAndDontSave, indexFormat = IndexFormat.UInt32 };
        }

        overlayMesh.Clear();
        overlayMesh.SetVertices(vertices);
        overlayMesh.SetColors(colors);
        overlayMesh.SetTriangles(triangles, 0);
        overlayDirty = false;
    }

    private static Material CreateOverlayMaterial()
    {
        Material material = new Material(Shader.Find("Hidden/Internal-Colored")) { hideFlags = HideFlags.HideAndDontSave };
        material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
        material.SetInt("_Cull", (int)CullMode.Off);
        material.SetInt("_ZWrite", 0);
        material.SetInt("_ZTest", (int)CompareFunction.LessEqual);
        return material;
    }
}
