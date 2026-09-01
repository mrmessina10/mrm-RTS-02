# WorkerMoveToDropOffState

`Assets/_Scripts/StateMachines/WorkerMoveToDropOffState.cs`

Último estado del ciclo de recolección: al entrar, resuelve el punto de entrega más cercano vía [BuildingManager.GetNearestDropOff](../_CORE%20Scripts/BuildingManager.md) para el `currentCarriedType` del worker. Si no hay ninguno registrado, vuelve a `UnitIdleState` con un warning.

Al llegar a rango de interacción, llama `targetDropOffPoint.Deposit(...)`, vacía `currentCarriedAmount` y vuelve a [WorkerMoveToResourceState](WorkerMoveToResourceState.md) para retomar la recolección — cierra el loop worker → recurso → depósito → recurso.
