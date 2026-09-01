# EnemyPatrolState

`Assets/_Scripts/StateMachines/EnemyPatrolState.cs`

Estado único que gobierna todo el ciclo de patrullaje de [EnemyController](../Unit%20Scripts/EnemyController.md): elige un punto aleatorio dentro de `PatrolRadius` (alrededor de `PatrolCenter`), se mueve hacia él, espera `WaitTimeAtPoint` segundos al llegar, y repite. No hay un estado separado para la espera — se maneja con un timer interno (`isWaiting`/`waitTimer`) para no crear un estado que solo espera, siguiendo el mismo criterio que otros estados del proyecto (ver [WorkerMoveToDropOffState](WorkerMoveToDropOffState.md)).

- `MoveToNewPatrolPoint()`: obtiene un punto válido con `TryGetRandomPointInPatrolArea` y llama a `enemy.Movement.MoveTo()`. Si el NavMesh no devuelve un punto válido (área sin bakear, radio fuera del mesh), reintenta en el próximo `Tick` en vez de trabarse.
- `Tick()`: mientras no está esperando, revisa `IsPathPending`/`RemainingDistance` igual que [UnitMoveState](UnitMoveState.md) para detectar la llegada; al llegar, para el movimiento y arranca el timer de espera.
- No tiene lógica de detección de enemigos ni de combate — es deliberadamente solo movimiento, para poder probar el sistema de ataque/persecución de las unidades del jugador (`UnitIdleState` → `MeleeAttackState`/`RangedAttackState`) contra un objetivo que se mueve.

## Fuente / patrón
Punto aleatorio sobre el NavMesh: `Random.insideUnitSphere` + [`NavMesh.SamplePosition`](https://docs.unity3d.com/ScriptReference/AI.NavMesh.SamplePosition.html) (patrón estándar de Unity para samplear un punto navegable dentro de un radio).
