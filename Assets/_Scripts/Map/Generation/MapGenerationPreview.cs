using UnityEngine;

// Dibuja un GeneratedMap como imagen cenital (relieve sombreado, agua, acantilados, camino, zonas inalcanzables,
// objetos y marcadores) para revisar una semilla sin tener que volcarla a la escena. Devuelve píxeles planos,
// con la fila 0 en el borde sur del mapa, igual que una Texture2D.
public static class MapGenerationPreview
{
    private const int TargetSize = 512;
    private const float ShadeStrength = 0.3f;

    private static readonly Color32 GrassColor = new Color32(104, 146, 78, 255);
    private static readonly Color32 HighGrassColor = new Color32(168, 178, 118, 255);
    private static readonly Color32 CliffColor = new Color32(116, 112, 106, 255);
    private static readonly Color32 ShallowWaterColor = new Color32(84, 150, 196, 255);
    private static readonly Color32 DeepWaterColor = new Color32(34, 84, 148, 255);
    private static readonly Color32 RoadColor = new Color32(196, 168, 120, 255);
    private static readonly Color32 UnreachableColor = new Color32(128, 44, 140, 255);
    private static readonly Color32 NatureColor = new Color32(58, 92, 50, 255);
    private static readonly Color32 ManMadeColor = new Color32(150, 86, 46, 255);
    private static readonly Color32 OutlineColor = new Color32(255, 255, 255, 255);

    public static Color32[] Render(GeneratedMap map, out int width, out int height)
    {
        int pixelsPerCell = Mathf.Clamp(TargetSize / Mathf.Max(map.Size.x, map.Size.y), 1, 8);
        width = map.Size.x * pixelsPerCell;
        height = map.Size.y * pixelsPerCell;

        Color32[] pixels = new Color32[width * height];
        float texel = 1f / pixelsPerCell;
        float roadHalfWidth = map.PathWidth / 2f;

        for (int pixelZ = 0; pixelZ < height; pixelZ++)
        {
            for (int pixelX = 0; pixelX < width; pixelX++)
            {
                float x = (pixelX + 0.5f) * texel;
                float z = (pixelZ + 0.5f) * texel;
                int cellIndex = Mathf.Min((int)z, map.Size.y - 1) * map.Size.x + Mathf.Min((int)x, map.Size.x - 1);
                float terrainHeight = map.Heights.Sample(x, z);

                Color color;
                if (terrainHeight < map.WaterLevel)
                {
                    color = Color32.Lerp(ShallowWaterColor, DeepWaterColor, Mathf.Clamp01((map.WaterLevel - terrainHeight) / 2f));
                }
                else
                {
                    color = Color32.Lerp(GrassColor, HighGrassColor, Mathf.Clamp01(terrainHeight / 6f));
                    if (!map.WalkableCells[cellIndex]) color = Color.Lerp(color, CliffColor, 0.75f);
                    if (map.PathDistance[cellIndex] <= roadHalfWidth) color = RoadColor;
                    else if (map.WalkableCells[cellIndex] && !map.ReachableCells[cellIndex]) color = Color.Lerp(color, UnreachableColor, 0.5f);

                    float slopeLight = map.Heights.Sample(x - texel, z + texel) - map.Heights.Sample(x + texel, z - texel);
                    color *= 1f + Mathf.Clamp(slopeLight * 0.6f, -ShadeStrength, ShadeStrength);
                }

                color.a = 1f;
                pixels[pixelZ * width + pixelX] = color;
            }
        }

        foreach (GeneratedMapObject mapObject in map.Objects)
        {
            MapGenerationCatalogEntry entry = map.Catalog.Entries[mapObject.CatalogIndex];
            float radius = pixelsPerCell * (entry.IsResource ? 0.7f : Mathf.Clamp(entry.Spacing * 0.2f, 0.35f, 1.2f));
            DrawDisc(pixels, width, height, ToPixel(map, mapObject.Position, pixelsPerCell), radius, GetObjectColor(entry));
        }

        foreach (GeneratedMapMarker marker in map.Markers)
        {
            Vector2 center = ToPixel(map, marker.Position, pixelsPerCell);
            DrawDisc(pixels, width, height, center, pixelsPerCell * 2.4f + 1.5f, OutlineColor);
            DrawDisc(pixels, width, height, center, pixelsPerCell * 2.4f, MapMarker.GetColor(marker.Type));
        }

        return pixels;
    }

    public static Color32 GetObjectColor(MapGenerationCatalogEntry entry)
    {
        if (!entry.IsResource) return entry.Category == MapObjectCategory.ManMade ? ManMadeColor : NatureColor;

        switch (entry.ResourceType)
        {
            case ResourceType.Wood: return new Color32(18, 62, 30, 255);
            case ResourceType.Food: return new Color32(226, 64, 104, 255);
            case ResourceType.Stone: return new Color32(214, 214, 222, 255);
            default: return new Color32(240, 200, 60, 255);
        }
    }

    private static Vector2 ToPixel(GeneratedMap map, Vector3 worldPosition, int pixelsPerCell)
    {
        return new Vector2((worldPosition.x - map.Origin.x) * pixelsPerCell, (worldPosition.z - map.Origin.z) * pixelsPerCell);
    }

    private static void DrawDisc(Color32[] pixels, int width, int height, Vector2 center, float radius, Color32 color)
    {
        int reach = Mathf.CeilToInt(radius);
        int centerX = Mathf.FloorToInt(center.x);
        int centerZ = Mathf.FloorToInt(center.y);

        for (int z = Mathf.Max(0, centerZ - reach); z <= Mathf.Min(height - 1, centerZ + reach); z++)
        {
            for (int x = Mathf.Max(0, centerX - reach); x <= Mathf.Min(width - 1, centerX + reach); x++)
            {
                if (Vector2.Distance(new Vector2(x + 0.5f, z + 0.5f), center) <= radius) pixels[z * width + x] = color;
            }
        }
    }
}
