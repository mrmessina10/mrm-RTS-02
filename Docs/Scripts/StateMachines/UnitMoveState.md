# UnitMoveState

`Assets/_Scripts/StateMachines/UnitMoveState.cs`

Estado de movimiento libre a un punto (resultado de un click derecho en el suelo, vía `UnitController.SetCommand`). Inicia el `NavMeshAgent` hacia `destination` en `Enter()`.

En `Tick()`:
1. Espera a que el NavMesh termine de calcular la ruta (`IsPathPending`).
2. Si llegó (`RemainingDistance <= StoppingDistance`), pasa a `UnitIdleState`.
3. **Sistema anti-crowding**: si la velocidad es casi nula (`VelocitySqr < 0.05f`) pero no llegó a destino, acumula `stuckTimer`; pasado `STUCK_THRESHOLD` (0.25s) asume que está trabada contra otras unidades y aborta a `UnitIdleState` en vez de quedar bloqueada indefinidamente empujando a sus compañeras.

Hay un bloque comentado con una versión anterior de detección de llegada por distancia cuadrada directa (`sqrMagnitude`), reemplazada por el chequeo del propio `NavMeshAgent`.
