# BuildingPlacementSceneSetup

`Assets/_Scripts/Editor/BuildingPlacementSceneSetup.cs`

Script de Editor. Menú `Tools/RTS/Ensure Building Placement Controller In Scene`: si no hay un [BuildingPlacementController](../Buildings/BuildingPlacementController.md) en la escena abierta, crea el GameObject (hijo de `GameArchitecture`, mismo criterio que [CoreManagerSceneSetup](CoreManagerSceneSetup.md)) y lo cablea automáticamente:

- `inputReader` y `mainCamera`: copiados directamente del `SelectionManager` ya configurado en la escena (mismo `Camera`, mismo asset de `InputReader`) — evita tener que repetir el drag & drop a mano.
- `groundMask`: copiado bit a bit del `groundMask` de `SelectionManager`.
- `availableBuildings`: todos los `BuildingDataSO` que encuentre en el proyecto (`AssetDatabase.FindAssets("t:BuildingDataSO")`), sin filtrar — hoy son `Lumbermill` y `Farm`.

Si no encuentra un `SelectionManager` en la escena, no crea nada (no tiene de dónde copiar cámara/mask). Si ya existe un `BuildingPlacementController`, tampoco hace nada. Marca la escena como modificada — hay que guardarla (Ctrl+S) a mano.
