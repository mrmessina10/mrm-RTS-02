using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

// Vuelca un GeneratedMap al mapa de la escena usando las mismas piezas que el editor de mapas: relieve al
// TerrainData, nivel de agua, máscara edificable y contenido al MapDataSO, reconstrucción de la escena desde ese
// asset y texturas (auto-texturizado, parches y pintado de caminos). Todo queda en un único paso de undo.
public static class MapGenerationApplier
{
    private const string UndoName = "Mapa: generar";
    private const float PatchHardness = 0.4f;

    public static void Apply(MapRoot mapRoot, GeneratedMap map)
    {
        Terrain terrain = mapRoot.Terrain;
        MapDataSO mapData = mapRoot.MapData;

        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName(UndoName);
        int undoGroup = Undo.GetCurrentGroup();

        TerrainBrushUtility.RegisterHeightUndo(terrain, UndoName);
        TerrainBrushUtility.RegisterAlphamapUndo(terrain, UndoName);
        Undo.RegisterCompleteObjectUndo(mapData, UndoName);

        WriteHeights(terrain, map);
        MapSceneSetup.ApplyWaterLevel(mapRoot, map.WaterLevel);
        WriteBuildableMask(mapData, map);
        mapData.SetContents(BuildObjects(map), BuildMarkers(map), BuildPaths(map));
        EditorUtility.SetDirty(mapData);

        MapSceneSync.RebuildSceneFromMapData(mapRoot);
        PaintTextures(mapRoot, map);

        Undo.CollapseUndoOperations(undoGroup);
        MapCameraSetup.FocusCameraRig(mapRoot);
        EditorSceneManager.MarkSceneDirty(mapRoot.gameObject.scene);
    }

    private static void WriteHeights(Terrain terrain, GeneratedMap map)
    {
        TerrainData data = terrain.terrainData;
        int resolution = data.heightmapResolution;
        float terrainY = terrain.transform.position.y;
        float[,] heights = new float[resolution, resolution];

        for (int z = 0; z < resolution; z++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float localX = x / (float)(resolution - 1) * data.size.x;
                float localZ = z / (float)(resolution - 1) * data.size.z;
                heights[z, x] = Mathf.Clamp01((map.Heights.Sample(localX, localZ) - terrainY) / data.size.y);
            }
        }

        data.SetHeights(0, 0, heights);
        TerrainBrushUtility.FinishHeightEdit(terrain);
    }

    private static void WriteBuildableMask(MapDataSO mapData, GeneratedMap map)
    {
        for (int z = 0; z < map.Size.y; z++)
        {
            for (int x = 0; x < map.Size.x; x++)
            {
                mapData.SetCellBuildable(new Vector2Int(x, z), !map.NotBuildableCells[z * map.Size.x + x]);
            }
        }
    }

    private static List<MapObjectEntry> BuildObjects(GeneratedMap map)
    {
        List<MapObjectEntry> objects = new List<MapObjectEntry>(map.Objects.Count);
        foreach (GeneratedMapObject mapObject in map.Objects)
        {
            MapGenerationCatalogEntry entry = map.Catalog.Entries[mapObject.CatalogIndex];
            if (entry.Prefab == null) continue;

            objects.Add(new MapObjectEntry
            {
                Prefab = entry.Prefab,
                Category = entry.Category,
                Position = mapObject.Position,
                Rotation = Quaternion.Euler(0f, mapObject.Yaw, 0f),
                Scale = entry.Prefab.transform.localScale * mapObject.Scale
            });
        }
        return objects;
    }

    private static List<MapMarkerEntry> BuildMarkers(GeneratedMap map)
    {
        List<MapMarkerEntry> markers = new List<MapMarkerEntry>(map.Markers.Count);
        foreach (GeneratedMapMarker marker in map.Markers)
        {
            markers.Add(new MapMarkerEntry { Type = marker.Type, Position = marker.Position, Rotation = Quaternion.identity });
        }
        return markers;
    }

    private static List<MapPathEntry> BuildPaths(GeneratedMap map)
    {
        List<MapPathEntry> paths = new List<MapPathEntry>(map.Paths.Count);
        foreach (GeneratedMapPath path in map.Paths)
        {
            paths.Add(new MapPathEntry { Width = path.Width, Waypoints = new List<Vector3>(path.Waypoints) });
        }
        return paths;
    }

    private static void PaintTextures(MapRoot mapRoot, GeneratedMap map)
    {
        Terrain terrain = mapRoot.Terrain;
        TerrainData data = terrain.terrainData;
        if (data.alphamapLayers == 0)
        {
            Debug.LogWarning("[MapGenerationApplier] El terreno no tiene capas de textura; el mapa queda sin texturizar.");
            return;
        }

        int baseLayer = Mathf.Max(0, TerrainBrushUtility.FindLayerIndex(data, MapEditorAssetGenerator.GrassLayerName));
        TerrainBrushUtility.AutoTexture(
            terrain,
            map.WaterLevel,
            baseLayer,
            TerrainBrushUtility.FindLayerIndex(data, MapEditorAssetGenerator.RockLayerName),
            TerrainBrushUtility.FindLayerIndex(data, MapEditorAssetGenerator.SandLayerName),
            -1);

        foreach (GeneratedTexturePatch patch in map.TexturePatches)
        {
            string layerName = patch.Ground == MapGroundPatch.Sand ? MapEditorAssetGenerator.SandLayerName : MapEditorAssetGenerator.DirtLayerName;
            TerrainBrushUtility.PaintLayer(terrain, patch.Center, patch.Radius, PatchHardness, TerrainBrushUtility.FindLayerIndex(data, layerName), patch.Opacity);
        }

        MapEditorContext context = new MapEditorContext { Root = mapRoot, Terrain = terrain, MapData = mapRoot.MapData };
        foreach (MapPath path in mapRoot.GetPaths())
        {
            PathTool.PaintPath(context, path);
        }

        TerrainBrushUtility.FinishAlphamapEdit(terrain);
    }
}
