using UnityEngine;
using UnityEditor;

// Herramienta de texturas: pinta las capas (TerrainLayer) del terreno con pincel, rellena el mapa con una capa
// o las reparte automáticamente según pendiente y cercanía al agua.
[System.Serializable]
public class TerrainPaintTool : MapEditorTool
{
    private const float ThumbnailSize = 56f;

    [SerializeField] private int layerIndex;
    [SerializeField] private float radius = 5f;
    [SerializeField] private float hardness = 0.4f;
    [SerializeField] private float opacity = 0.8f;

    public override string DisplayName => "Texturas";
    public override string Hint => "Elegir una capa y arrastrar sobre el terreno para pintarla.";

    public override float GetBrushRadius(MapEditorContext context) => radius;
    public override Color GetBrushColor(MapEditorContext context) => new Color(0.6f, 1f, 0.4f);

    public override void OnToolGUI(MapEditorContext context)
    {
        TerrainLayer[] layers = context.TerrainData.terrainLayers;
        if (layers.Length == 0)
        {
            EditorGUILayout.HelpBox("El terreno no tiene capas. Generalas con Tools/RTS/Generate Map Editor Assets y asignalas al TerrainData.", MessageType.Warning);
            return;
        }

        layerIndex = Mathf.Clamp(layerIndex, 0, layers.Length - 1);

        GUIContent[] contents = new GUIContent[layers.Length];
        for (int i = 0; i < layers.Length; i++)
        {
            contents[i] = new GUIContent(layers[i] != null ? layers[i].name : "(vacía)", layers[i] != null ? layers[i].diffuseTexture : null);
        }

        GUIStyle layerButtonStyle = new GUIStyle(GUI.skin.button) { imagePosition = ImagePosition.ImageAbove, fixedHeight = ThumbnailSize + 20f };
        layerIndex = GUILayout.SelectionGrid(layerIndex, contents, Mathf.Min(layers.Length, 5), layerButtonStyle);
        EditorGUILayout.Space();

        radius = EditorGUILayout.Slider("Radio", radius, 0.5f, 40f);
        hardness = EditorGUILayout.Slider("Dureza del borde", hardness, 0f, 1f);
        opacity = EditorGUILayout.Slider("Opacidad", opacity, 0.05f, 1f);
        EditorGUILayout.Space();

        if (GUILayout.Button($"Rellenar todo el mapa con '{contents[layerIndex].text}'"))
        {
            TerrainBrushUtility.RegisterAlphamapUndo(context.Terrain, "Terreno: rellenar capa");
            TerrainBrushUtility.FillLayer(context.Terrain, layerIndex);
            TerrainBrushUtility.FinishAlphamapEdit(context.Terrain);
        }

        if (GUILayout.Button($"Auto-texturizar (base '{contents[layerIndex].text}', roca en pendientes, arena en orillas)"))
        {
            TerrainData data = context.TerrainData;
            TerrainBrushUtility.RegisterAlphamapUndo(context.Terrain, "Terreno: auto-texturizar");
            TerrainBrushUtility.AutoTexture(
                context.Terrain,
                context.MapData.WaterLevel,
                layerIndex,
                TerrainBrushUtility.FindLayerIndex(data, MapEditorAssetGenerator.RockLayerName),
                TerrainBrushUtility.FindLayerIndex(data, MapEditorAssetGenerator.SandLayerName),
                TerrainBrushUtility.FindLayerIndex(data, MapEditorAssetGenerator.RoadLayerName));
            TerrainBrushUtility.FinishAlphamapEdit(context.Terrain);
        }
    }

    public override void OnStrokeBegin(MapEditorContext context)
    {
        TerrainBrushUtility.RegisterAlphamapUndo(context.Terrain, "Terreno: pintar capa");
    }

    public override void OnStrokeStep(MapEditorContext context, float deltaTime)
    {
        float stepOpacity = opacity * Mathf.Clamp01(deltaTime * 12f);
        TerrainBrushUtility.PaintLayer(context.Terrain, context.HitPoint, radius, hardness, layerIndex, stepOpacity);
    }

    public override void OnStrokeEnd(MapEditorContext context)
    {
        TerrainBrushUtility.FinishAlphamapEdit(context.Terrain);
    }
}
