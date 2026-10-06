using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text;

// Sección "Generación procedural" de la ventana del editor de mapas: elige reglas y semilla, muestra la vista previa
// con el informe de reglas sin tocar la escena, vuelca el mapa elegido al mapa activo y corre tandas de semillas
// para medir cuántas cumplen las reglas y qué regla descarta más intentos.
[System.Serializable]
public class MapGeneratorPanel
{
    private const float PreviewMaxSize = 384f;
    private const float SmallButtonWidth = 72f;

    private static readonly string[] LayoutLabels = { "Spawn → HQ", "A → B con HQ al costado" };
    private static readonly string[] WaterLabels = { "sin agua", "lagos", "río", "costa" };

    [SerializeField] private MapGenerationRulesSO rules;
    [SerializeField] private int seed = 1;
    [SerializeField] private bool isExpanded = true;
    [SerializeField] private bool bakeAfterGenerate = true;
    [SerializeField] private bool showAllRules;
    [SerializeField] private int batchSize = 50;

    [System.NonSerialized] private GeneratedMap preview;
    [System.NonSerialized] private MapGenerationRulesSO previewRules;
    [System.NonSerialized] private Texture2D previewTexture;
    [System.NonSerialized] private string batchReport;

    // Devuelve true cuando volcó un mapa a la escena, para que la ventana suelte la herramienta activa
    public bool OnGUI(MapEditorContext context)
    {
        isExpanded = EditorGUILayout.Foldout(isExpanded, "Generación procedural", true, EditorStyles.foldoutHeader);
        if (!isExpanded) return false;

        if (rules == null) rules = FindRules();

        EditorGUILayout.BeginHorizontal();
        rules = (MapGenerationRulesSO)EditorGUILayout.ObjectField("Reglas", rules, typeof(MapGenerationRulesSO), false);
        if (rules == null && GUILayout.Button("Crear", GUILayout.Width(SmallButtonWidth)))
        {
            rules = MapEditorAssetGenerator.EnsureGenerationRules();
            AssetDatabase.SaveAssets();
        }
        EditorGUILayout.EndHorizontal();

        if (rules == null)
        {
            EditorGUILayout.HelpBox("Asigná un asset de reglas de generación o creá el de por defecto.", MessageType.Info);
            return false;
        }

        if (context.Palette == null)
        {
            EditorGUILayout.HelpBox("Sin paleta no hay nodos de recurso para colocar. Generala con Tools/RTS/Generate Map Editor Assets.", MessageType.Warning);
        }

        EditorGUILayout.BeginHorizontal();
        seed = EditorGUILayout.IntField("Semilla", seed);
        if (GUILayout.Button("Al azar", GUILayout.Width(SmallButtonWidth)))
        {
            seed = Random.Range(1, int.MaxValue);
            GeneratePreview(context);
        }
        if (GUILayout.Button("Siguiente", GUILayout.Width(SmallButtonWidth)))
        {
            seed++;
            GeneratePreview(context);
        }
        EditorGUILayout.EndHorizontal();

        bakeAfterGenerate = EditorGUILayout.Toggle("Bake NavMesh al generar", bakeAfterGenerate);

        bool applied = false;
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Vista previa", GUILayout.Height(26f))) GeneratePreview(context);
        if (GUILayout.Button("Generar en la escena", GUILayout.Height(26f))) applied = ApplyToScene(context);
        EditorGUILayout.EndHorizontal();

        if (applied) return true;

        DrawPreview(context);
        DrawBatchSection(context);
        return false;
    }

    public void Dispose()
    {
        if (previewTexture != null) Object.DestroyImmediate(previewTexture);
    }

    private static MapGenerationRulesSO FindRules()
    {
        string[] guids = AssetDatabase.FindAssets("t:MapGenerationRulesSO");
        return guids.Length > 0 ? AssetDatabase.LoadAssetAtPath<MapGenerationRulesSO>(AssetDatabase.GUIDToAssetPath(guids[0])) : null;
    }

    private GeneratedMap Generate(MapEditorContext context, int mapSeed)
    {
        return MapGenerator.Generate(MapGenerationRequest.Create(rules, context.Palette, context.MapData, mapSeed));
    }

    private bool IsPreviewCurrent(MapEditorContext context)
    {
        return preview != null && preview.Seed == seed && previewRules == rules && preview.Size == context.MapData.Size;
    }

    private void GeneratePreview(MapEditorContext context)
    {
        preview = Generate(context, seed);
        previewRules = rules;

        Color32[] pixels = MapGenerationPreview.Render(preview, out int width, out int height);
        if (previewTexture == null || previewTexture.width != width || previewTexture.height != height)
        {
            Dispose();
            previewTexture = new Texture2D(width, height, TextureFormat.RGBA32, false) { hideFlags = HideFlags.HideAndDontSave };
        }

        previewTexture.SetPixels32(pixels);
        previewTexture.Apply();
    }

    private bool ApplyToScene(MapEditorContext context)
    {
        if (!IsPreviewCurrent(context)) GeneratePreview(context);

        string message = $"Se reemplazan el relieve, las texturas, los objetos, los marcadores, los caminos y la zona edificable de '{context.MapData.name}' por el mapa de la semilla {seed}.";
        if (!preview.IsValid) message += "\n\nEsta semilla no cumple todas las reglas obligatorias (ver el informe debajo de la vista previa).";
        if (!EditorUtility.DisplayDialog("Generar mapa", message, "Generar", "Cancelar")) return false;

        try
        {
            EditorUtility.DisplayProgressBar("Generar mapa", $"Volcando {preview.Objects.Count} objetos a la escena...", 0.5f);
            MapGenerationApplier.Apply(context.Root, preview);
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        if (bakeAfterGenerate) MapSceneSetup.BakeNavMesh(context.Root);

        Debug.Log($"[MapGeneratorPanel] Mapa generado en {context.MapData.name}: semilla {seed}, reglas '{rules.name}', {GetSummary(preview)}.");
        return true;
    }

    private void DrawPreview(MapEditorContext context)
    {
        if (preview == null || previewTexture == null) return;

        EditorGUILayout.Space();
        if (!IsPreviewCurrent(context))
        {
            EditorGUILayout.HelpBox($"La vista previa es de la semilla {preview.Seed} con otras reglas o tamaño; volvé a generarla.", MessageType.None);
        }

        Rect rect = GUILayoutUtility.GetAspectRect(previewTexture.width / (float)previewTexture.height, GUILayout.MaxWidth(PreviewMaxSize));
        EditorGUI.DrawPreviewTexture(rect, previewTexture);

        EditorGUILayout.LabelField(GetSummary(preview), EditorStyles.wordWrappedMiniLabel);
        EditorGUILayout.LabelField("Rojo: spawn · naranja: fin de oleada · azul: HQ · verde oscuro: madera · rosa: comida · marrón: construcciones · violeta: suelo inalcanzable desde el HQ.", EditorStyles.wordWrappedMiniLabel);

        if (preview.IsValid)
        {
            EditorGUILayout.HelpBox($"Cumple todas las reglas obligatorias (intento {preview.AttemptsUsed} de {rules.MaxAttempts}).", MessageType.Info);
        }

        foreach (MapRuleResult rule in preview.Rules)
        {
            if (!rule.Passed) EditorGUILayout.HelpBox($"{rule.Rule}: {rule.Detail}", rule.Required ? MessageType.Error : MessageType.Warning);
        }

        showAllRules = EditorGUILayout.Foldout(showAllRules, "Detalle de reglas", true);
        if (!showAllRules) return;

        foreach (MapRuleResult rule in preview.Rules)
        {
            EditorGUILayout.LabelField($"{(rule.Passed ? "✓" : "✗")} {rule.Rule}{(string.IsNullOrEmpty(rule.Detail) ? "" : $" — {rule.Detail}")}", EditorStyles.wordWrappedMiniLabel);
        }
    }

    private static string GetSummary(GeneratedMap map)
    {
        return $"{LayoutLabels[(int)map.Layout]}, {WaterLabels[(int)map.WaterMode]}, {map.PlateauCount} mesetas, {map.CountMarkers(MapMarkerType.EnemySpawn)} spawn(s), "
            + $"{map.Paths.Count} camino(s), {map.Objects.Count} objetos ({map.CountResourceNodes(ResourceType.Wood)} de madera, {map.CountResourceNodes(ResourceType.Food)} de comida)";
    }

    private void DrawBatchSection(MapEditorContext context)
    {
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        batchSize = EditorGUILayout.IntSlider("Probar semillas", batchSize, 10, 500);
        if (GUILayout.Button("Probar", GUILayout.Width(SmallButtonWidth))) RunBatch(context);
        EditorGUILayout.EndHorizontal();

        if (!string.IsNullOrEmpty(batchReport)) EditorGUILayout.HelpBox(batchReport, MessageType.None);
    }

    private void RunBatch(MapEditorContext context)
    {
        int valid = 0;
        int validFirstTry = 0;
        int attempts = 0;
        int completed = 0;
        Dictionary<string, int> discarded = new Dictionary<string, int>();
        Dictionary<string, int> variants = new Dictionary<string, int>();

        try
        {
            for (int i = 0; i < batchSize; i++)
            {
                if (EditorUtility.DisplayCancelableProgressBar("Probar semillas", $"Semilla {seed + i}", i / (float)batchSize)) break;

                GeneratedMap map = Generate(context, seed + i);
                completed++;
                attempts += map.AttemptsUsed;
                if (map.IsValid) valid++;
                if (map.IsValid && map.AttemptsUsed == 1) validFirstTry++;

                foreach (MapRuleResult rule in map.DiscardedRules)
                {
                    Increment(discarded, rule.Rule);
                }
                Increment(variants, $"{LayoutLabels[(int)map.Layout]}, {WaterLabels[(int)map.WaterMode]}");
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        if (completed == 0) return;

        StringBuilder report = new StringBuilder();
        report.AppendLine($"Semillas {seed} a {seed + completed - 1}: {valid} de {completed} cumplen las reglas ({validFirstTry} al primer intento, {attempts / (float)completed:0.0} intentos en promedio).");
        AppendCounts(report, "Intentos descartados por regla", discarded);
        AppendCounts(report, "Variantes que salieron", variants);

        batchReport = report.ToString().TrimEnd();
        Debug.Log($"[MapGeneratorPanel] {batchReport}");
    }

    private static void Increment(Dictionary<string, int> counts, string key)
    {
        counts.TryGetValue(key, out int count);
        counts[key] = count + 1;
    }

    private static void AppendCounts(StringBuilder report, string title, Dictionary<string, int> counts)
    {
        if (counts.Count == 0) return;

        List<KeyValuePair<string, int>> sorted = new List<KeyValuePair<string, int>>(counts);
        sorted.Sort((a, b) => b.Value.CompareTo(a.Value));

        report.AppendLine($"{title}:");
        foreach (KeyValuePair<string, int> pair in sorted)
        {
            report.AppendLine($"  {pair.Value} · {pair.Key}");
        }
    }
}
