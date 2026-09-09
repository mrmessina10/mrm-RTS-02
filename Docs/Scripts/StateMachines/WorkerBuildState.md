# WorkerBuildState

`Assets/_Scripts/StateMachines/WorkerBuildState.cs`

Estado de construcción: se acerca al [ConstructionSite](../Buildings/ConstructionSite.md) objetivo (`Movement.MoveTo` en `Enter`) y, al llegar a `InteractionRange` (mismo patrón que [WorkerMoveToDropOffState](WorkerMoveToDropOffState.md): `!IsPathPending && RemainingDistance <= InteractionRange`), se detiene y le suma `Time.deltaTime` cada frame vía `AddBuildProgress`.

Combina movimiento + acción en un solo estado (a diferencia del par Move/Harvest del worker) porque, a diferencia de recolectar → depositar, construir no involucra moverse a un segundo lugar — es el mismo patrón de "acercarse y accionar en el lugar" que usa [MeleeAttackState](MeleeAttackState.md) para combate.

Al completarse (`site.IsComplete`) o si el sitio desaparece (`ConstructionSite` se autodestruye al terminar — chequeo de falso null, mismo patrón que `MeleeAttackState`), vuelve a `UnitIdleState`. No hay retorno automático a otra tarea: si el worker estaba cargando un recurso antes de recibir la orden de construir, esa carga no se toca (mismo criterio que el resto de las interrupciones de `WorkerController`).

Disparado por [WorkerController.SetTarget](../Unit%20Scripts/WorkerController.md) cuando el objetivo es `IConstructable` (click derecho sobre un cimiento).

Animator: `IsMoving` mientras se acerca (mismo parámetro que el resto de los estados de movimiento, más `UpdateCarryAnimation()` para reflejar si viene cargando algo de camino), `IsBuilding` mientras está en rango sumando progreso — se togglean entre sí en el momento exacto en que entra en rango. `Exit` resetea `IsMoving`/`IsBuilding` a `false` por si se interrumpe la construcción con otra orden (`IsCarrying` no se resetea acá, es un status de inventario, no de este estado — ver [WorkerController.UpdateCarryAnimation](../Unit%20Scripts/WorkerController.md)).
