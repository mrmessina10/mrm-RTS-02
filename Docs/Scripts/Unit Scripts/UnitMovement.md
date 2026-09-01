# UnitMovement

`Assets/_Scripts/Unit Scripts/UnitMovement.cs`

Envoltorio delgado sobre `NavMeshAgent` (`[RequireComponent]`). Expone solo lo que los estados de la FSM necesitan leer (`IsPathPending`, `RemainingDistance`, `StoppingDistance`, `VelocitySqr`) y dos comandos (`MoveTo`, `Stop`), en vez de exponer el `NavMeshAgent` completo.

Usado por todos los estados en `StateMachines/` a través de `UnitController.Movement`. `Stop()` valida `agent.isOnNavMesh` antes de tocar el agente, para evitar errores si la unidad quedó fuera de la malla.
