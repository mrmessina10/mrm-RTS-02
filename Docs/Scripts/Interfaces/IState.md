# IState

`Assets/_Scripts/Interfaces/IState.cs`

Contrato base para todos los estados de la FSM del proyecto (`Enter`, `Tick`, `Exit`). Lo implementan todos los archivos de `StateMachines/` (UnitIdleState, UnitMoveState, MeleeAttackState, RangedAttackState, WorkerMoveToResourceState, WorkerHarvestResourceState, WorkerMoveToDropOffState).

- `Enter()`: se ejecuta una vez al entrar al estado.
- `Tick()`: se ejecuta cada frame mientras el estado está activo.
- `Exit()`: se ejecuta una vez al salir del estado.

Consumido por [StateMachine](../StateMachines/StateMachine.md), que guarda la instancia actual y llama a estos tres métodos en los momentos correspondientes.

## Fuente / patrón
Finite State Machine clásica (patrón GoF State), variante habitual en juegos: cada estado es un objeto con Enter/Update/Exit en vez de un enum + switch gigante. Referencia: [Game Programming Patterns – State](https://gameprogrammingpatterns.com/state.html).
