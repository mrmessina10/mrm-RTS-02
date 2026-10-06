# MapSceneSetup

`Assets/_Scripts/Editor/MapEditor/MapSceneSetup.cs`

Arma un mapa nuevo en la escena activa y centraliza las operaciones de escena del editor de mapas.

**`CreateMap(nombre, tamaño)`** crea en `Assets/Maps/<nombre>/` el `TerrainData` y el [MapDataSO](../../ScriptableObjects/MapDataSO.md), y en la escena la jerarquía documentada en [MapRoot](../../Map/MapRoot.md). No sobreescribe un mapa existente con el mismo nombre.

Decisiones:

- **Suelo base en Y = 0.** El terreno mide 40 de alto y se posiciona en Y = −10, con el heightmap inicializado al 25 %. Así lo que ya estaba apoyado en el plano anterior sigue apoyado, y hay margen para cavar (agua, barrancos) además de levantar. `Terrain` no admite alturas negativas en su propio espacio, por eso el offset.
- **Centrado en el origen**, con `Origin` entero para que la grilla de celdas coincida con la de `BuildingPlacement`.
- **Resolución**: 2 muestras de altura y 4 texeles de splatmap por unidad (257 y 512 para un mapa de 128).
- **Orden de creación del `TerrainData`**: resolución → tamaño (cambiar la resolución resetea el tamaño) → `CreateAsset` → capas y alturas, para que las texturas de splatmap se creen ya como sub-assets.
- **Layer `Ground`** en el terreno: es el que usan `SelectionManager` y los controllers de colocación para sus raycasts.
- **Material**: `defaultTerrainMaterial` del render pipeline activo (URP Terrain Lit).
- **NavMeshSurface** en el `MapRoot`: geometría por colliders físicos y solo los layers `Default`, `Ground` y `Water`. Quedan fuera unidades, edificios (tallan con `NavMeshObstacle`) y recursos.

- **Cámara**: al terminar llama a [MapCameraSetup](MapCameraSetup.md)`.EnsureCameraRig`, que deja la cámara RTS con la configuración de `SampleScene` si la escena todavía no la tiene.

**Otras operaciones**: `FindMapRoot`, `ApplyWaterLevel`, `BakeNavMesh` (usa `NavMeshAssetManager`, el mismo camino que el botón Bake del inspector, que guarda el NavMesh como asset junto a la escena) y `FindForeignNavMeshSurfaces` (detecta el suelo anterior para que la ventana ofrezca desactivarlo).

Referencias: Unity Manual, "Terrain scripting" / Scripting API `TerrainData`; paquete AI Navigation, `NavMeshSurface`.
