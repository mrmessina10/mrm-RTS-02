# WorkerController

`Assets/_Scripts/Unit Scripts/WorkerController.cs`

Hereda de [UnitController](UnitController.md), agrega el estado de recolección: capacidad de carga (`MaxCarryCapacity`), velocidad de recolección (`HarvestRate`, `HarvestAmountPerCycle`), y el radio/máscara de auto-búsqueda de recursos (`AutoSeekRadius`, `ResourceMask`).

- `SetTarget(IInteractable)`: si el objetivo es `Harvest` y castea a `IHarvestable`, entra a [WorkerMoveToResourceState](../StateMachines/WorkerMoveToResourceState.md). Si se reasigna a un recurso de *distinto tipo* al que ya llevaba cargado, descarta la carga actual (regla de diseño explícita: no se puede mezclar tipos de recurso en el inventario del worker). Cualquier otro tipo de objetivo delega a `base.SetTarget` (ataque, vía `UnitController`).
- `SetCommand(destination)`: cancela `currentResourceNode` antes de delegar a `base.SetCommand` — un click derecho en el suelo corta cualquier objetivo de recolección en curso.
- `TryFindNearbyResourceNode()`: `Physics.OverlapSphere` dentro de `AutoSeekRadius`, filtra por mismo `ResourceType` que ya lleva cargado y no agotado, y elige el más cercano. Lo usa [WorkerMoveToResourceState](../StateMachines/WorkerMoveToResourceState.md) cuando el nodo actual se agota, para no volver a `Idle` innecesariamente.

Campos `currentResourceNode`/`currentCarriedType`/`currentCarriedAmount` son `[HideInInspector] public` — se leen y escriben directamente desde los estados de `StateMachines/`, que actúan como clases muy acopladas a `WorkerController` (comparten estado en vez de comunicarse por API).
