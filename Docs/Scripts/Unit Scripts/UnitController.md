# UnitController

`Assets/_Scripts/Unit Scripts/UnitController.cs`

Clase base de toda unidad controlable: combina configuración de combate (rango, daño, cooldown, tipo de daño), detección (rango de visión, `enemyMask`) y la [StateMachine](../StateMachines/StateMachine.md) que gobierna su comportamiento. [WorkerController](WorkerController.md) hereda de esta clase y sobreescribe `SetTarget`/`SetCommand` para agregar el flujo de recolección.

- `Awake`: cachea `UnitMovement` y `Animator` (hijo), crea la `StateMachine`.
- `Start`: entra a `UnitIdleState` por defecto.
- `Update`: delega a `stateMachine.Update()` — el propio `UnitController` no tiene lógica de comportamiento, es solo el contenedor de datos + fachada de comandos.
- `SetCommand(destination)`: cambia a `UnitMoveState` (movimiento libre a un punto).
- `SetTarget(IInteractable)`: cambia al estado de ataque correspondiente vía `GetAttackState` (ver abajo). `WorkerController` la sobreescribe para interceptar objetivos `Harvest` antes de llegar acá.
- `GetAttackState(target)`: **factory method** que decide `MeleeAttackState` o `RangedAttackState` según `combatType` — evita que `UnitIdleState`/`SelectionManager` necesiten conocer el tipo de combate de la unidad.
- `TriggerAttackDamage()`: llamado por [AnimationEventRelay](AnimationEventRelay.md) en el frame de impacto de la animación; hace type-check del estado actual (`is MeleeAttackState` / `is RangedAttackState`) y delega `ApplyDamage()` a ese estado.

## Fuente / patrón
Factory Method (`GetAttackState`) para desacoplar la creación del estado de ataque concreto del código que lo solicita. State pattern para el comportamiento (ver [IState](../Interfaces/IState.md)).
