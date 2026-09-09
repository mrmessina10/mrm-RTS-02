# WorkerMoveToResourceState

`Assets/_Scripts/StateMachines/WorkerMoveToResourceState.cs`

Primer estado del ciclo de recolección: mueve al worker hacia `worker.currentResourceNode`. En `Tick()`, si el nodo se destruyó o agotó, intenta encontrar otro cercano del mismo tipo ([WorkerController.TryFindNearbyResourceNode](../Unit%20Scripts/WorkerController.md)); si no hay ninguno, decide entre ir a depositar lo que ya lleva ([WorkerMoveToDropOffState](WorkerMoveToDropOffState.md)) o volver a `UnitIdleState` si no carga nada.

Al llegar a rango de interacción (`RemainingDistance <= InteractionRange`), transiciona a [WorkerHarvestResourceState](WorkerHarvestResourceState.md).

`Enter`/`Exit` setean `UnitAnimator.SetBool("IsMoving", true/false)` — mismo parámetro que ya usan `UnitMoveState`/`MeleeAttackState`/`RangedAttackState`, así que el Animator Controller del worker no necesita un parámetro nuevo para caminar, solo su propio clip de caminata. `Enter` también llama a `worker.UpdateCarryAnimation()` para que la caminata refleje si va cargando o no.
