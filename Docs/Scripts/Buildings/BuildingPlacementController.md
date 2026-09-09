# BuildingPlacementController

`Assets/_Scripts/Buildings/BuildingPlacementController.cs`

Consume [InputReader.BuildRequestEvent](../_CORE%20Scripts/InputReader.md) (hotkeys mock Numpad 1/2) y resuelve el flujo de colocación de Fase 1 del [roadmap](../../Roadmap-VerticalSlice.md): ghost siguiendo el mouse → validación en vivo → confirmar (gasta recursos, instancia el edificio real) o cancelar.

## Ghost
Al recibir un `BuildRequestEvent(BuildingType)`, busca el [BuildingDataSO](../ScriptableObjects/BuildingDataSO.md) correspondiente en `availableBuildings` e instancia su `BuildingPrefab` como ghost. `StripFunctionalComponents` le saca todo lo que no sea visual — `NavMeshObstacle`, `Collider`, [BuildingPlacement](BuildingPlacement.md), [DropOffBuilding](DropOffBuilding.md), `Health`, `UnitSelectionHandler` — para que un edificio en preview no carve el NavMesh, no bloquee físicamente, no se registre como drop-off ni sea seleccionable/dañable.

Cada frame (`Update`, mientras `IsPlacing`), raycastea el mouse contra `groundMask`, snapea el punto a la grilla con `BuildingPlacement.GetFootprintOrigin` y tiñe el ghost verde/rojo según `BuildingPlacement.IsAreaBuildable`.

## Confirmar / cancelar
- **Confirmar** (`SelectEvent`, click izquierdo): solo si la posición es válida y `ResourceManager.HasEnoughResources` alcanza — descuenta con `TrySpend` por cada entrada de `ConstructionCost` e instancia el prefab real en la posición del ghost, pero **no como edificio terminado**: le agrega un [ConstructionSite](ConstructionSite.md) (`AddComponent<ConstructionSite>().Initialize(...)`), que lo deja inerte hasta que un worker lo construya (ver [WorkerBuildState](../StateMachines/WorkerBuildState.md)). `SelectEvent` dispara tanto en press como en release (ver `InputReader.OnSelect`), así que se filtra con `inputReader.IsLeftClickHeld` para no confirmar dos veces por click — mismo patrón que ya usa `SelectionManager.HandleSelect`.
- **Cancelar** (`CommandEvent`, click derecho): destruye el ghost sin gastar nada.
- Pedir otro edificio mientras ya hay uno en preview cancela el anterior antes de empezar el nuevo.

## Por qué SelectionManager necesitó un guard
`SelectEvent`/`CommandEvent` los escuchan tanto este controller como [SelectionManager](../_CORE%20Scripts/SelectionManager.md) — sin coordinación, un click para confirmar/cancelar la colocación *también* dispararía selección de unidades o una orden de movimiento sobre lo ya seleccionado. `SelectionManager.HandleSelect`/`HandleMoveCommand` ahora cortan temprano si `BuildingPlacementController.Instance.IsPlacing` es `true`.

## Setup en escena
No se auto-instancia: usar el menú `Tools/RTS/Ensure Building Placement Controller In Scene` ([BuildingPlacementSceneSetup](../Editor/BuildingPlacementSceneSetup.md)), que copia `inputReader`/`mainCamera`/`groundMask` desde el `SelectionManager` ya configurado y carga todos los `BuildingDataSO` del proyecto en `availableBuildings`.

## Pendiente
Es el mínimo funcional de Fase 1, no el sistema final: sin menú/UI real para elegir edificio (depende de los hotkeys mock), sin límite de cuántos se pueden colocar en cola, sin feedback más allá de logs de consola y el tinte del ghost. Sin barra de progreso visual de construcción (solo `ConstructionSite.BuildProgress`, todavía sin consumidor).
