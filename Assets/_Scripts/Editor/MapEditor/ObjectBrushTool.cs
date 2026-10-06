using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

// Herramienta de objetos: coloca prefabs de la paleta (recursos, naturaleza, construcciones) de a uno, con pincel
// de dispersión respetando el espaciado de cada entrada, o en anillo para armar una aldea. Shift borra dentro del radio.
[System.Serializable]
public class ObjectBrushTool : MapEditorTool
{
    private enum PlacementMode
    {
        Single,
        Scatter,
        Village
    }

    private const int EntriesPerRow = 3;
    private const float EntryButtonHeight = 72f;

    private static readonly string[] CategoryLabels = { "Recursos", "Naturaleza", "Construcciones" };
    private static readonly string[] ModeLabels = { "Individual", "Pincel", "Aldea" };
    private static readonly MapObjectCategory[] AllCategories = { MapObjectCategory.Resource, MapObjectCategory.Nature, MapObjectCategory.ManMade };

    [SerializeField] private MapObjectCategory category = MapObjectCategory.Resource;
    [SerializeField] private PlacementMode mode = PlacementMode.Scatter;
    [SerializeField] private List<int> selectedEntries = new List<int>();
    [SerializeField] private float radius = 6f;
    [SerializeField] private float density = 12f;
    [SerializeField] private float maxSlope = 30f;
    [SerializeField] private bool avoidWater = true;
    [SerializeField] private float manualYaw;
    [SerializeField] private int villageCount = 5;
    [SerializeField] private bool eraseAllCategories;

    [System.NonSerialized] private float pendingPlacements;
    [System.NonSerialized] private bool isErasing;

    public override string DisplayName => "Objetos";
    public override string Hint => "Elegir una o más entradas de la paleta y hacer click/arrastrar. Shift + arrastrar borra los objetos dentro del radio.";

    public override float GetBrushRadius(MapEditorContext context)
    {
        if (mode != PlacementMode.Single || context.Shift) return radius;

        List<MapPaletteEntry> entries = GetSelectedEntries(context);
        return entries.Count > 0 ? Mathf.Max(entries[entries.Count - 1].Spacing, 0.5f) : 1f;
    }

    public override Color GetBrushColor(MapEditorContext context) => context.Shift ? new Color(1f, 0.3f, 0.3f) : new Color(1f, 0.9f, 0.4f);

    public override void OnToolGUI(MapEditorContext context)
    {
        if (context.Palette == null)
        {
            EditorGUILayout.HelpBox("No hay paleta asignada. Generala con Tools/RTS/Generate Map Editor Assets.", MessageType.Warning);
            return;
        }

        EditorGUI.BeginChangeCheck();
        category = (MapObjectCategory)GUILayout.Toolbar((int)category, CategoryLabels);
        if (EditorGUI.EndChangeCheck()) selectedEntries.Clear();

        DrawPaletteGrid(context.Palette);
        EditorGUILayout.Space();

        EditorGUI.BeginChangeCheck();
        mode = (PlacementMode)GUILayout.Toolbar((int)mode, ModeLabels);
        if (EditorGUI.EndChangeCheck() && mode == PlacementMode.Single && selectedEntries.Count > 1)
        {
            selectedEntries.RemoveRange(0, selectedEntries.Count - 1);
        }

        if (mode != PlacementMode.Single)
        {
            radius = EditorGUILayout.Slider("Radio", radius, 1f, 40f);
            maxSlope = EditorGUILayout.Slider("Pendiente máxima (°)", maxSlope, 0f, 90f);
            avoidWater = EditorGUILayout.Toggle("Evitar agua", avoidWater);
        }

        if (mode == PlacementMode.Scatter) density = EditorGUILayout.Slider("Intentos por segundo", density, 1f, 80f);
        if (mode == PlacementMode.Village) villageCount = EditorGUILayout.IntSlider("Construcciones por aldea", villageCount, 2, 12);

        manualYaw = EditorGUILayout.Slider("Rotación fija (°)", manualYaw, 0f, 360f);
        eraseAllCategories = EditorGUILayout.Toggle("Shift borra todas las categorías", eraseAllCategories);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"En el mapa — Recursos: {CountObjects(context, MapObjectCategory.Resource)} · Naturaleza: {CountObjects(context, MapObjectCategory.Nature)} · Construcciones: {CountObjects(context, MapObjectCategory.ManMade)}", EditorStyles.miniLabel);

        if (GUILayout.Button($"Borrar todos los objetos de '{CategoryLabels[(int)category]}'")
            && EditorUtility.DisplayDialog("Borrar objetos", $"¿Borrar todos los objetos de la categoría {CategoryLabels[(int)category]}?", "Borrar", "Cancelar"))
        {
            Transform container = context.Root.GetContainer(category);
            for (int i = container.childCount - 1; i >= 0; i--)
            {
                Undo.DestroyObjectImmediate(container.GetChild(i).gameObject);
            }
        }
    }

    private void DrawPaletteGrid(MapPaletteSO palette)
    {
        GUIStyle entryStyle = new GUIStyle(GUI.skin.button) { imagePosition = ImagePosition.ImageAbove, wordWrap = true };
        List<MapPaletteEntry> entries = palette.Entries;
        int column = 0;

        EditorGUILayout.BeginHorizontal();
        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].Category != category || entries[i].Prefab == null) continue;

            if (column == EntriesPerRow)
            {
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                column = 0;
            }

            Texture2D preview = AssetPreview.GetAssetPreview(entries[i].Prefab);
            if (preview == null) preview = AssetPreview.GetMiniThumbnail(entries[i].Prefab);

            bool wasSelected = selectedEntries.Contains(i);
            int prefabCount = entries[i].GetPrefabs().Count;
            string label = prefabCount > 1 ? $"{entries[i].DisplayName} (x{prefabCount})" : entries[i].DisplayName;

            bool isSelected = GUILayout.Toggle(wasSelected, new GUIContent(label, preview), entryStyle, GUILayout.Height(EntryButtonHeight), GUILayout.MinWidth(40f));

            if (isSelected && !wasSelected)
            {
                if (mode == PlacementMode.Single) selectedEntries.Clear();
                selectedEntries.Add(i);
            }
            else if (!isSelected && wasSelected)
            {
                selectedEntries.Remove(i);
            }

            column++;
        }
        EditorGUILayout.EndHorizontal();

        if (selectedEntries.Count == 0)
        {
            EditorGUILayout.HelpBox("Seleccioná al menos una entrada. En Pincel y Aldea se pueden combinar varias.", MessageType.Info);
        }
    }

    public override void OnStrokeBegin(MapEditorContext context)
    {
        isErasing = context.Shift;
        if (isErasing)
        {
            EraseInRadius(context);
            return;
        }

        List<MapPaletteEntry> entries = GetSelectedEntries(context);
        if (entries.Count == 0) return;

        switch (mode)
        {
            case PlacementMode.Single:
                MapPaletteEntry entry = entries[entries.Count - 1];
                TryPlace(context, entry, context.HitPoint, GetYaw(entry), false);
                break;

            case PlacementMode.Village:
                PlaceVillage(context, entries);
                break;

            case PlacementMode.Scatter:
                pendingPlacements = 1f;
                break;
        }
    }

    public override void OnStrokeStep(MapEditorContext context, float deltaTime)
    {
        if (isErasing)
        {
            EraseInRadius(context);
            return;
        }

        if (mode != PlacementMode.Scatter) return;

        List<MapPaletteEntry> entries = GetSelectedEntries(context);
        if (entries.Count == 0) return;

        pendingPlacements += density * deltaTime;
        while (pendingPlacements >= 1f)
        {
            pendingPlacements -= 1f;

            MapPaletteEntry entry = entries[Random.Range(0, entries.Count)];
            Vector2 offset = Random.insideUnitCircle * radius;
            TryPlace(context, entry, context.HitPoint + new Vector3(offset.x, 0f, offset.y), GetYaw(entry), true);
        }
    }

    public override void OnStrokeEnd(MapEditorContext context)
    {
        isErasing = false;
    }

    private List<MapPaletteEntry> GetSelectedEntries(MapEditorContext context)
    {
        List<MapPaletteEntry> result = new List<MapPaletteEntry>();
        if (context.Palette == null) return result;

        foreach (int index in selectedEntries)
        {
            if (index < 0 || index >= context.Palette.Entries.Count) continue;

            MapPaletteEntry entry = context.Palette.Entries[index];
            if (entry.Prefab != null && entry.Category == category) result.Add(entry);
        }
        return result;
    }

    private float GetYaw(MapPaletteEntry entry)
    {
        return entry.RandomYaw ? Random.Range(0f, 360f) : manualYaw;
    }

    private void PlaceVillage(MapEditorContext context, List<MapPaletteEntry> entries)
    {
        Vector3 center = context.HitPoint;
        float startAngle = Random.Range(0f, 360f);

        for (int i = 0; i < villageCount; i++)
        {
            float angle = (startAngle + i * 360f / villageCount + Random.Range(-12f, 12f)) * Mathf.Deg2Rad;
            float distance = radius * Random.Range(0.6f, 1f);
            Vector3 position = center + new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle)) * distance;
            Vector3 toCenter = center - position;
            float yaw = Mathf.Atan2(toCenter.x, toCenter.z) * Mathf.Rad2Deg;

            TryPlace(context, entries[Random.Range(0, entries.Count)], position, yaw, true);
        }
    }

    private bool TryPlace(MapEditorContext context, MapPaletteEntry entry, Vector3 position, float yaw, bool validateSurface)
    {
        if (entry.SnapToGrid)
        {
            position = new Vector3(Mathf.Floor(position.x) + 0.5f, position.y, Mathf.Floor(position.z) + 0.5f);
        }

        if (!context.MapData.IsInsideMap(context.MapData.WorldToCell(position))) return false;

        position = context.SnapToTerrain(position);

        if (validateSurface)
        {
            if (avoidWater && position.y < context.MapData.WaterLevel) return false;
            if (GetSteepness(context, position) > maxSlope) return false;
            if (IsTooClose(context, position, entry.Spacing)) return false;
        }

        float scale = Random.Range(entry.ScaleRange.x, entry.ScaleRange.y);

        List<GameObject> prefabs = entry.GetPrefabs();
        GameObject prefab = prefabs[Random.Range(0, prefabs.Count)];

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, context.Root.GetContainer(entry.Category));
        instance.transform.position = position + Vector3.up * (entry.VerticalOffset * scale);
        instance.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        instance.transform.localScale = prefab.transform.localScale * scale;
        Undo.RegisterCreatedObjectUndo(instance, "Mapa: colocar objeto");

        return true;
    }

    private static float GetSteepness(MapEditorContext context, Vector3 position)
    {
        Vector3 local = position - context.Terrain.transform.position;
        Vector3 size = context.TerrainData.size;
        return context.TerrainData.GetSteepness(local.x / size.x, local.z / size.z);
    }

    private static bool IsTooClose(MapEditorContext context, Vector3 position, float spacing)
    {
        float spacingSquared = spacing * spacing;

        foreach (MapObjectCategory objectCategory in AllCategories)
        {
            Transform container = context.Root.GetContainer(objectCategory);
            for (int i = 0; i < container.childCount; i++)
            {
                Vector3 offset = container.GetChild(i).position - position;
                offset.y = 0f;
                if (offset.sqrMagnitude < spacingSquared) return true;
            }
        }
        return false;
    }

    private void EraseInRadius(MapEditorContext context)
    {
        float radiusSquared = radius * radius;

        foreach (MapObjectCategory objectCategory in AllCategories)
        {
            if (!eraseAllCategories && objectCategory != category) continue;

            Transform container = context.Root.GetContainer(objectCategory);
            for (int i = container.childCount - 1; i >= 0; i--)
            {
                Vector3 offset = container.GetChild(i).position - context.HitPoint;
                offset.y = 0f;
                if (offset.sqrMagnitude <= radiusSquared) Undo.DestroyObjectImmediate(container.GetChild(i).gameObject);
            }
        }
    }

    private static int CountObjects(MapEditorContext context, MapObjectCategory objectCategory)
    {
        Transform container = context.Root.GetContainer(objectCategory);
        return container != null ? container.childCount : 0;
    }
}
