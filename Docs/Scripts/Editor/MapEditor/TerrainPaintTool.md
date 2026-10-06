# TerrainPaintTool

`Assets/_Scripts/Editor/MapEditor/TerrainPaintTool.cs`

Herramienta **Texturas**. Muestra las `TerrainLayer` del `TerrainData` como grilla con miniatura y pinta la elegida con pincel (radio, dureza, opacidad) vía [TerrainBrushUtility](TerrainBrushUtility.md)`.PaintLayer`.

Dos acciones de mapa completo:

- **Rellenar** todo con la capa elegida.
- **Auto-texturizar**: la capa elegida como base, roca en pendientes y arena en orillas, preservando lo pintado con la capa de camino. Sirve como primera pasada después de esculpir.

Las capas por defecto (Grass, Dirt, Rock, Sand, Road) las genera [MapEditorAssetGenerator](MapEditorAssetGenerator.md); roca, arena y camino se buscan por nombre. Se pueden agregar capas propias al `TerrainData` desde el inspector del Terrain y aparecen acá.
