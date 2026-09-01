# WorkerMoveToResourceState

`Assets/_Scripts/StateMachines/WorkerMoveToResourceState.cs`

Primer estado del ciclo de recolección: mueve al worker hacia `worker.currentResourceNode`. En `Tick()`, si el nodo se destruyó o agotó, intenta encontrar otro cercano del mismo tipo ([WorkerController.TryFindNearbyResourceNode](../Unit%20Scripts/WorkerController.md)); si no hay ninguno, decide entre ir a depositar lo que ya lleva ([WorkerMoveToDropOffState](WorkerMoveToDropOffState.md)) o volver a `UnitIdleState` si no carga nada.

Al llegar a rango de interacción (`RemainingDistance <= InteractionRange`), transiciona a [WorkerHarvestResourceState](WorkerHarvestResourceState.md).
