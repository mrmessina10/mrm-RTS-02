# WorkerMoveToDropOffState

`Assets/_Scripts/StateMachines/WorkerMoveToDropOffState.cs`

Último estado del ciclo de recolección. Tiene dos modos, según cómo se lo instancie:

- **Auto-depósito** (constructor sin segundo argumento, disparado por [WorkerHarvestResourceState](WorkerHarvestResourceState.md) al llenar `MaxCarryCapacity`): al entrar, resuelve el punto de entrega más cercano vía [BuildingManager.GetNearestDropOff](../_CORE%20Scripts/BuildingManager.md) para el `currentCarriedType` del worker. Si no hay ninguno registrado, vuelve a `UnitIdleState` con un warning.
- **Force-drop** (constructor con `explicitTarget`, disparado por [WorkerController.SetTarget](../Unit%20Scripts/WorkerController.md) cuando el jugador clickea un `IDropOffPoint` puntualmente): va directo a ese edificio, sin buscar el más cercano. El chequeo de `AcceptsResource` ya se hizo antes, en `WorkerController.SetTarget` — si el edificio no acepta el recurso que lleva el worker, este estado ni se instancia.

Al llegar a rango de interacción, llama `targetDropOffPoint.Deposit(...)` y vacía `currentCarriedAmount`. Qué pasa después difiere según el modo: auto-depósito vuelve a [WorkerMoveToResourceState](WorkerMoveToResourceState.md) para retomar la recolección (cierra el loop worker → recurso → depósito → recurso); force-drop lo deja en `UnitIdleState` — el jugador interrumpió al worker a propósito, no se supone que retome la recolección solo (mismo criterio que Age of Empires 2: el force-drop es una orden de una sola vez, no reanuda la tarea).

`Enter`/`Exit` setean `UnitAnimator.SetBool("IsMoving", true/false)` (no en el caso `targetDropOffPoint == null`, donde ni siquiera se mueve). Mismo parámetro compartido con el resto de los estados de movimiento, ver [WorkerMoveToResourceState](WorkerMoveToResourceState.md). `Enter` también llama a `worker.UpdateCarryAnimation()` — normalmente `true` (viene a depositar porque lleva algo), pero se calcula igual en vez de asumirlo, por consistencia.
