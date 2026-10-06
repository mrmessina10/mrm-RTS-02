# MapDataSO

`Assets/_Scripts/ScriptableObjects/MapDataSO.cs`

Formato de datos común de un mapa, el que define [Design-MapEditorAndProceduralGeneration.md §2](../../Design-MapEditorAndProceduralGeneration.md#2-formato-de-datos-común--mapdata). Un asset por mapa, en `Assets/Maps/<Nombre>/<Nombre>.asset`, creado por [MapSceneSetup](../Editor/MapEditor/MapSceneSetup.md).

Contenido:

- `Size` (celdas, 1 unidad = 1 celda), `Origin` (esquina mínima del terreno en mundo), `TerrainData` (heightmap + splatmap, asset propio al lado) y `WaterLevel`.
- `Objects` (`MapObjectEntry`: prefab, categoría, posición, rotación, escala), `Markers` (`MapMarkerEntry`) y `Paths` (`MapPathEntry`: ancho + waypoints).
- Máscara de celdas bloqueadas (`byte[]`, 1 = no edificable; vacío o corto = todo edificable).

El archivo también declara los enums `MapObjectCategory` (`Resource`/`Nature`/`ManMade`) y `MapMarkerType` (`EnemySpawn`/`WaveEnd`/`PlayerStart`).

**Quién escribe y quién lee.** El gameplay solo lee (`IsBuildable`, `IsCellBuildable`, listas `IReadOnlyList`). Los métodos de escritura (`Initialize`, `SetContents`, `SetCellBuildable`, `SetAllBuildable`, `SetWaterLevel`) son la API de autoría: hoy la usa el editor de mapas y más adelante el generador procedural, que necesita producir este mismo asset en runtime — por eso son métodos públicos y no escritura por `SerializedObject`.

**Qué es fuente de verdad de qué.** El relieve y las texturas viven en el `TerrainData` y la máscara edificable vive acá: ambos se editan directo sobre el asset. Objetos, marcadores y caminos se editan en la escena (copia de trabajo, se pueden mover a mano con los gizmos de Unity) y [MapSceneSync](../Editor/MapEditor/MapSceneSync.md) los vuelca a las listas al guardar la escena.

**Grilla.** `WorldToCell` usa `FloorToInt(world - Origin)`; como `Origin` es entero, coincide con la grilla de [BuildingPlacement](../Buildings/BuildingPlacement.md) (`GetFootprintOrigin`). Una celda fuera del mapa nunca es edificable.
