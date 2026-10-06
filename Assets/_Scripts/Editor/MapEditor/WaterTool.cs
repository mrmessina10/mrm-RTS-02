using UnityEngine;
using UnityEditor;

// Herramienta de agua: el agua es un único plano a una altura global; un cuerpo de agua aparece donde el terreno
// queda por debajo de ese nivel. El pincel cava cuencas (o las rellena con Shift) y puede pintar arena en la orilla.
[System.Serializable]
public class WaterTool : MapEditorTool
{
    private const float MinWaterLevel = -8f;
    private const float MaxWaterLevel = 4f;
    private const float DigSpeedPerDepth = 2f;

    [SerializeField] private float radius = 8f;
    [SerializeField] private float shoreHardness = 0.35f;
    [SerializeField] private float depth = 2f;
    [SerializeField] private bool paintSandOnShore = true;

    [System.NonSerialized] private bool paintedSand;

    public override string DisplayName => "Agua";
    public override string Hint => "Arrastrar para cavar un cuerpo de agua (lago, río, costa). Shift + arrastrar rellena hasta el nivel del suelo base.";

    public override float GetBrushRadius(MapEditorContext context) => radius;
    public override Color GetBrushColor(MapEditorContext context) => context.Shift ? new Color(0.8f, 0.6f, 0.3f) : new Color(0.2f, 0.5f, 1f);

    public override void OnToolGUI(MapEditorContext context)
    {
        EditorGUI.BeginChangeCheck();
        float waterLevel = EditorGUILayout.Slider("Nivel de agua (mapa)", context.MapData.WaterLevel, MinWaterLevel, MaxWaterLevel);
        if (EditorGUI.EndChangeCheck())
        {
            MapSceneSetup.ApplyWaterLevel(context.Root, waterLevel);
        }

        EditorGUILayout.Space();
        radius = EditorGUILayout.Slider("Radio", radius, 1f, 40f);
        shoreHardness = EditorGUILayout.Slider("Dureza de la orilla", shoreHardness, 0f, 0.95f);
        depth = EditorGUILayout.Slider("Profundidad", depth, 0.5f, 8f);
        paintSandOnShore = EditorGUILayout.Toggle("Pintar arena en la orilla", paintSandOnShore);

        EditorGUILayout.HelpBox("El NavMesh excluye todo lo que quede sumergido más allá de la profundidad de vadeo: después de cavar hay que volver a hacer Bake.", MessageType.None);
    }

    public override void OnStrokeBegin(MapEditorContext context)
    {
        paintedSand = paintSandOnShore && !context.Shift;

        TerrainBrushUtility.RegisterHeightUndo(context.Terrain, "Terreno: agua");
        if (paintedSand) TerrainBrushUtility.RegisterAlphamapUndo(context.Terrain, "Terreno: agua");
    }

    public override void OnStrokeStep(MapEditorContext context, float deltaTime)
    {
        float stepDistance = Mathf.Max(depth, 1f) * DigSpeedPerDepth * deltaTime;

        if (context.Shift)
        {
            TerrainBrushUtility.ModifyHeightsInBrush(context.Terrain, context.HitPoint, radius, shoreHardness,
                (height, weight) => height >= 0f ? height : Mathf.Min(height + stepDistance * weight, 0f));
            return;
        }

        float bottomHeight = context.MapData.WaterLevel - depth;
        TerrainBrushUtility.ModifyHeightsInBrush(context.Terrain, context.HitPoint, radius, shoreHardness,
            (height, weight) => height <= bottomHeight ? height : Mathf.Max(height - stepDistance * weight, bottomHeight));

        if (!paintedSand) return;

        int sandLayer = TerrainBrushUtility.FindLayerIndex(context.TerrainData, MapEditorAssetGenerator.SandLayerName);
        TerrainBrushUtility.PaintLayer(context.Terrain, context.HitPoint, radius * 1.1f, 0.5f, sandLayer, 1f);
    }

    public override void OnStrokeEnd(MapEditorContext context)
    {
        TerrainBrushUtility.FinishHeightEdit(context.Terrain);
        if (paintedSand) TerrainBrushUtility.FinishAlphamapEdit(context.Terrain);
    }
}
