# MapSceneSync

`Assets/_Scripts/Editor/MapEditor/MapSceneSync.cs`

Puente entre la escena y el [MapDataSO](../../ScriptableObjects/MapDataSO.md). La escena es la copia de trabajo de objetos, marcadores y caminos; el asset es el archivo del mapa.

- **`CaptureSceneToMapData`** — recorre los contenedores de [MapRoot](../../Map/MapRoot.md) y reemplaza las listas del asset. Corre desde el botón de la ventana y automáticamente en cada guardado de escena (`EditorSceneManager.sceneSaving`, registrado con `[InitializeOnLoad]`), así el asset no queda desfasado. Los objetos que no son instancia de prefab no se pueden guardar (no hay prefab que referenciar) y se informan con un warning.
- **`RebuildSceneFromMapData`** — borra el contenido de los contenedores y lo vuelve a instanciar desde el asset; también reasigna `TerrainData`, origen y nivel de agua.
- **`LoadMapData`** — cambia el `MapData` activo del `MapRoot` y reconstruye.

Por qué no se escribe directo al asset en cada pincelada, como planteaba el doc de diseño original: con la escena como copia de trabajo los objetos se pueden ajustar a mano con las herramientas estándar de Unity y el undo es el nativo, sin mantener dos representaciones sincronizadas en cada operación. El asset sigue siendo el formato común que va a producir el generador procedural, y `RebuildSceneFromMapData` es el camino para abrir un mapa generado y retocarlo.

Relieve, texturas y máscara edificable no pasan por acá: se editan directo sobre sus assets.
