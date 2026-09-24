# BuildingPlacementSceneSetup

`Assets/_Scripts/Editor/BuildingPlacementSceneSetup.cs`

Script de Editor con dos menús, uno por controller de colocación.

## Tools/RTS/Ensure Building Placement Controller In Scene
Crea el GameObject de [BuildingPlacementController](../Buildings/BuildingPlacementController.md) si no existe (hijo de `GameArchitecture`, mismo criterio que [CoreManagerSceneSetup](CoreManagerSceneSetup.md)), y **siempre** — exista o no de antes — le refresca el wiring:

- `inputReader` y `mainCamera`: copiados directamente del `SelectionManager` ya configurado en la escena.
- `groundMask`: copiado bit a bit del `groundMask` de `SelectionManager`.
- `availableBuildings`: todos los `BuildingDataSO` que encuentre en el proyecto (`AssetDatabase.FindAssets("t:BuildingDataSO")`), sin filtrar.

El refresco incondicional de `availableBuildings` es a propósito: antes solo se cableaba una vez al crear el GameObject, así que un `BuildingDataSO` nuevo (ej. `Gate`) quedaba afuera de la lista hasta arrastrarlo a mano. Ahora correr el menú de nuevo alcanza.

## Tools/RTS/Ensure Wall Placement Controller In Scene
Mismo patrón para [WallPlacementController](../Buildings/WallPlacementController.md): crea el GameObject si falta, copia `inputReader`/`mainCamera`/`groundMask` de `SelectionManager`, y busca entre los `BuildingDataSO` del proyecto el que tenga `BuildingType.Palisade` para asignarlo a `wallSegmentData`. Si no encuentra ninguno, avisa por warning y no hace nada — hay que generar los prefabs de muro primero (`Tools/RTS/Generate Palisade Wall Prefabs`).

Ambos menús: si no encuentran un `SelectionManager` en la escena, no hacen nada (no tienen de dónde copiar cámara/mask). Marcan la escena como modificada — hay que guardarla (Ctrl+S) a mano.
