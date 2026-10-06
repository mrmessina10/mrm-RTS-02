using UnityEngine;
using UnityEditor;

// Herramienta de elevación: subir/bajar, suavizar, aplanar, mesetas y barrancos por niveles discretos,
// rampas entre dos puntos y ruido para variación orgánica.
[System.Serializable]
public class TerrainSculptTool : MapEditorTool
{
    private enum SculptMode
    {
        Raise,
        Lower,
        Smooth,
        Flatten,
        Plateau,
        Ramp,
        Noise
    }

    private static readonly string[] ModeLabels = { "Subir", "Bajar", "Suavizar", "Aplanar", "Meseta / Barranco", "Rampa", "Ruido" };

    [SerializeField] private SculptMode mode = SculptMode.Raise;
    [SerializeField] private float radius = 6f;
    [SerializeField] private float hardness = 0.3f;
    [SerializeField] private float strength = 6f;
    [SerializeField] private int plateauLevel = 1;
    [SerializeField] private float levelHeight = 2.5f;
    [SerializeField] private float rampWidth = 5f;
    [SerializeField] private float noiseScale = 6f;

    [System.NonSerialized] private bool isStroking;
    [System.NonSerialized] private float flattenHeight;
    [System.NonSerialized] private Vector3 rampStart;

    public override string DisplayName => "Elevación";
    public override string Hint => "Click y arrastrar para esculpir. Shift invierte Subir/Bajar. En Rampa: click en el inicio, arrastrar y soltar en el final.";

    public override float GetBrushRadius(MapEditorContext context) => mode == SculptMode.Ramp ? rampWidth / 2f : radius;
    public override Color GetBrushColor(MapEditorContext context) => new Color(0.3f, 0.8f, 1f);

    public override void OnToolGUI(MapEditorContext context)
    {
        mode = (SculptMode)GUILayout.SelectionGrid((int)mode, ModeLabels, 3);
        EditorGUILayout.Space();

        if (mode == SculptMode.Ramp)
        {
            rampWidth = EditorGUILayout.Slider("Ancho de rampa", rampWidth, 1f, 20f);
            return;
        }

        radius = EditorGUILayout.Slider("Radio", radius, 0.5f, 40f);

        if (mode == SculptMode.Plateau)
        {
            hardness = EditorGUILayout.Slider("Dureza del borde", hardness, 0f, 1f);
            plateauLevel = EditorGUILayout.IntSlider("Nivel", plateauLevel, -4, 8);
            levelHeight = EditorGUILayout.Slider("Altura por nivel", levelHeight, 0.5f, 5f);
            EditorGUILayout.HelpBox($"Altura objetivo: {plateauLevel * levelHeight:0.##} (0 = nivel del suelo base). Niveles negativos cavan barrancos. El desnivel queda como acantilado no caminable: para dar acceso usá Rampa.", MessageType.None);

            if (plateauLevel * levelHeight < context.MapData.WaterLevel)
            {
                EditorGUILayout.HelpBox($"Esa altura queda por debajo del nivel de agua del mapa ({context.MapData.WaterLevel:0.##}): el barranco se va a inundar. Para un barranco seco, bajá el nivel de agua en la herramienta Agua.", MessageType.Warning);
            }
            return;
        }

        hardness = EditorGUILayout.Slider("Dureza del borde", hardness, 0f, 1f);
        strength = EditorGUILayout.Slider("Fuerza", strength, 0.5f, 30f);

        if (mode == SculptMode.Noise)
        {
            noiseScale = EditorGUILayout.Slider("Escala del ruido", noiseScale, 1f, 30f);
        }
    }

    public override void OnSceneGUI(MapEditorContext context)
    {
        if (mode != SculptMode.Ramp || !isStroking || !context.HasHit) return;

        Handles.color = Color.yellow;
        Handles.DrawAAPolyLine(4f, rampStart + Vector3.up * 0.1f, context.HitPoint + Vector3.up * 0.1f);
    }

    public override void OnStrokeBegin(MapEditorContext context)
    {
        isStroking = true;
        flattenHeight = context.HitPoint.y;
        rampStart = context.HitPoint;

        TerrainBrushUtility.RegisterHeightUndo(context.Terrain, $"Terreno: {ModeLabels[(int)mode]}");
    }

    public override void OnStrokeStep(MapEditorContext context, float deltaTime)
    {
        Terrain terrain = context.Terrain;
        Vector3 center = context.HitPoint;

        switch (mode)
        {
            case SculptMode.Raise:
            case SculptMode.Lower:
                float direction = (mode == SculptMode.Raise) != context.Shift ? 1f : -1f;
                float delta = direction * strength * deltaTime;
                TerrainBrushUtility.ModifyHeightsInBrush(terrain, center, radius, hardness, (height, weight) => height + delta * weight);
                break;

            case SculptMode.Smooth:
                TerrainBrushUtility.SmoothHeights(terrain, center, radius, hardness, Mathf.Clamp01(strength * deltaTime));
                break;

            case SculptMode.Flatten:
                float flattenAmount = Mathf.Clamp01(strength * deltaTime);
                TerrainBrushUtility.ModifyHeightsInBrush(terrain, center, radius, hardness, (height, weight) => Mathf.Lerp(height, flattenHeight, weight * flattenAmount));
                break;

            case SculptMode.Plateau:
                float targetHeight = plateauLevel * levelHeight;
                TerrainBrushUtility.ModifyHeightsInBrush(terrain, center, radius, hardness, (height, weight) => Mathf.Lerp(height, targetHeight, weight));
                break;

            case SculptMode.Noise:
                float amplitude = strength * deltaTime;
                float frequency = 1f / noiseScale;
                Vector2 centerXZ = new Vector2(center.x, center.z);
                Vector2 extent = new Vector2(radius, radius);
                TerrainBrushUtility.ModifyHeights(terrain, centerXZ - extent, centerXZ + extent, (worldX, worldZ, height) =>
                {
                    float weight = TerrainBrushUtility.GetFalloff(Vector2.Distance(new Vector2(worldX, worldZ), centerXZ) / radius, hardness);
                    float noise = Mathf.PerlinNoise(worldX * frequency + 1000f, worldZ * frequency + 1000f) * 2f - 1f;
                    return height + noise * amplitude * weight;
                });
                break;
        }
    }

    public override void OnStrokeEnd(MapEditorContext context)
    {
        isStroking = false;

        if (mode == SculptMode.Ramp && context.HasHit)
        {
            TerrainBrushUtility.ApplyRamp(context.Terrain, rampStart, context.HitPoint, rampWidth, rampWidth * 0.4f);
        }

        TerrainBrushUtility.FinishHeightEdit(context.Terrain);
    }
}
