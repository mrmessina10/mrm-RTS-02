# TerrainBrushUtility

`Assets/_Scripts/Editor/MapEditor/TerrainBrushUtility.cs`

Única clase del editor de mapas que toca `TerrainData.GetHeights/SetHeights/GetAlphamaps/SetAlphamaps`. Las herramientas trabajan en posiciones y alturas de mundo; acá se hace la conversión a índices del heightmap/splatmap y a alturas normalizadas 0–1.

**Heightmap**

- `ModifyHeights(terrain, minXZ, maxXZ, operation)` — recorre las muestras del rectángulo y le pasa a `operation` la posición de mundo y la altura actual; escribe lo que devuelve.
- `ModifyHeightsInBrush(...)` — lo mismo para un pincel circular, con el peso de `GetFalloff(distancia normalizada, dureza)`: 1 hasta `dureza`, después cae con `SmoothStep` hasta 0 en el borde. La fórmula vive en [MapHeightField](../../Map/Generation/MapHeightField.md) (runtime) para que pincel y generador procedural usen exactamente el mismo perfil.
- `SmoothHeights` — promedio 3x3 (box blur) mezclado por el peso del pincel.
- `ApplyRamp(from, to, width, edgeBlend)` — interpola linealmente la altura a lo largo del segmento y funde los bordes.
- `LevelAlongPolyline` — suaviza el perfil de alturas de una polilínea con un promedio móvil y aplica una rampa por tramo. Lo usa [PathTool](PathTool.md).

Las ediciones de altura usan `SetHeightsDelayLOD` durante el trazo y `SyncHeightmap` al soltar (`FinishHeightEdit`), que es el patrón que documenta Unity para edición interactiva: evita recalcular LOD y collider en cada paso.

**Splatmap**

- `PaintLayer` — sube el peso de una capa hacia 1 y reescala las demás para que la suma siga dando 1.
- `FillLayer`, `FindLayerIndex` (por nombre de `TerrainLayer`).
- `AutoTexture` — roca según `GetSteepness` (entre 28° y 42°), arena en la franja sobre el nivel de agua, capa base en el resto; preserva el peso de la capa de camino.

**Undo**: `RegisterHeightUndo` registra el `TerrainData` completo y `RegisterAlphamapUndo` sus `alphamapTextures` (`Undo.RegisterCompleteObjectUndo`), una vez por trazo.

Referencias: Unity Scripting API — `TerrainData.SetHeightsDelayLOD`, `TerrainData.SyncHeightmap`, `TerrainData.SetAlphamaps`, `TerrainData.GetSteepness`.
