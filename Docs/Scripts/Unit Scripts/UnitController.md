# UnitController

`Assets/_Scripts/Unit Scripts/UnitController.cs`

Clase base de toda unidad controlable: combina configuración de combate (rango, daño, cooldown, tipo de daño), detección (rango de visión, `enemyMask`) y la [StateMachine](../StateMachines/StateMachine.md) que gobierna su comportamiento. [WorkerController](WorkerController.md) hereda de esta clase y sobreescribe `SetTarget`/`SetCommand` para agregar el flujo de recolección.

- `Awake`: cachea `UnitMovement` y `Animator` (hijo), crea la `StateMachine`.
- `Start`: entra a `UnitIdleState` por defecto.
- `Update`: delega a `stateMachine.Update()` — el propio `UnitController` no tiene lógica de comportamiento, es solo el contenedor de datos + fachada de comandos.
- `SetCommand(destination)`: cambia a `UnitMoveState` (movimiento libre a un punto).
- `SetTarget(IInteractable)`: entrada de las órdenes **del jugador** sobre un interactuable (la llama [SelectionManager](../_CORE%20Scripts/SelectionManager.md) con lo que haya bajo el click derecho). Despacha por `InteractionType`:
  - `Attack` (hoy solo [EnemyUnit](EnemyUnit.md)): cambia al estado de ataque vía `GetAttackState` (ver abajo).
  - Cualquier otro tipo (`None`, `Harvest`, `Build`, `Repair`): no hay nada que esta unidad pueda hacerle a ese objetivo, así que la orden se resuelve como movimiento — `SetCommand(GetApproachPoint(target))`. Un soldado con click derecho sobre un edificio propio o un recurso camina hasta su borde y queda en `Idle`; nunca entra a un estado de ataque.

  `WorkerController` la sobreescribe para interceptar `Harvest`/drop-off/construcción antes de llegar acá.
- `GetApproachPoint(target)` (privado): `Collider.ClosestPoint` del objetivo respecto de la unidad, con fallback a la posición del transform si no tiene collider. Mismo criterio que [WorkerMoveToDropOffState](../StateMachines/WorkerMoveToDropOffState.md): el centro de un edificio queda dentro del hueco que su `NavMeshObstacle` talla en el NavMesh, el borde no.
- `GetAttackState(target)`: **factory method** que decide `MeleeAttackState` o `RangedAttackState` según `combatType` — evita que `UnitIdleState`/`SelectionManager` necesiten conocer el tipo de combate de la unidad. **No filtra por `InteractionType`** a propósito (ver abajo).
- `TriggerAttackDamage()`: llamado por [AnimationEventRelay](AnimationEventRelay.md) en el frame de impacto de la animación; hace type-check del estado actual (`is MeleeAttackState` / `is RangedAttackState`) y delega `ApplyDamage()` a ese estado.

## Dónde vive el filtro de "qué se puede atacar"

`InteractionType.Attack` responde a "¿el jugador puede ordenar atacar esto?", no a "¿esto puede recibir daño?" — los edificios propios tienen `Health` y `Type = None`. Por eso el filtro está en `SetTarget` (la entrada de comandos del jugador) y no en `GetAttackState` ni en los estados de ataque:

- Sin el filtro, un click derecho sobre un Lumbermill, una Farm o el City Center con una unidad militar seleccionada entraba a `MeleeAttackState`/`RangedAttackState` y aplicaba daño real vía `IDamageable` — en el City Center eso termina levantando `Channel_GameOver`.
- `GetAttackState` queda libre para quien decide el objetivo por código: hoy el auto-aggro de [UnitIdleState](../StateMachines/UnitIdleState.md) (que ya filtra por `enemyMask`), y en la Fase 3 del [roadmap](../../Roadmap-VerticalSlice.md) la IA enemiga. Cuando [EnemyController](EnemyController.md) herede de esta clase, tiene que atacar edificios del jugador con `ChangeState(GetAttackState(building))` directo — **no** pasando por `SetTarget`, que los descartaría por ser `Type = None`.

Se eligió mover en vez de ignorar la orden porque un click derecho que no hace nada se lee como input perdido, y en una selección mixta los workers sí reaccionan (recolectan/depositan) mientras los soldados se quedarían clavados; acercarse es además el comportamiento de referencia en AoE2. La excepción deliberada sigue siendo el force-drop rechazado de [WorkerController](WorkerController.md), que se ignora.

## Fuente / patrón
[`Collider.ClosestPoint`](https://docs.unity3d.com/ScriptReference/Collider.ClosestPoint.html) para el punto de acercamiento (solo válido en colliders box/sphere/capsule o mesh convexo — todos los prefabs interactuables actuales usan primitivos).

Factory Method (`GetAttackState`) para desacoplar la creación del estado de ataque concreto del código que lo solicita. State pattern para el comportamiento (ver [IState](../Interfaces/IState.md)).
