# EnemyController

`Assets/_Scripts/Unit Scripts/EnemyController.cs`

Contraparte de [UnitController](UnitController.md) para IA enemiga: cachea [UnitMovement](UnitMovement.md) y `Animator`, arma su propia [StateMachine](../StateMachines/StateMachine.md) y arranca en [EnemyPatrolState](../StateMachines/EnemyPatrolState.md). A diferencia de `UnitController`, no tiene configuración de combate ni `SetCommand`/`SetTarget` — es exclusivamente el contenedor de datos de patrullaje (`PatrolCenter`, `PatrolRadius`, `WaitTimeAtPoint`) y la fachada de `ChangeState`.

Convive en el mismo GameObject con [EnemyUnit](EnemyUnit.md) (marca el objeto como `IInteractable` atacable) y `Health` (lo hace `IDamageable`) — cada componente cubre una sola responsabilidad, en vez de fusionarlas en una sola clase.

- `patrolCenter` es opcional: si no se asigna en el Inspector, se usa la posición inicial del propio objeto (`transform.position` en `Awake`).
- Requiere que el GameObject tenga un `NavMeshAgent` (vía `UnitMovement`) y que el área alrededor de `PatrolCenter` esté bakeada en el NavMesh.
- `OnDrawGizmosSelected` dibuja el radio de patrullaje en el editor para facilitar el ajuste del área.
