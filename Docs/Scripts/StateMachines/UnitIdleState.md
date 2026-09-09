# UnitIdleState

`Assets/_Scripts/StateMachines/UnitIdleState.cs`

Estado por defecto de toda unidad (`UnitController.Start`). Detiene el movimiento y escanea periódicamente (`SCAN_INTERVAL` = 0.25s, no cada frame, por costo) en busca de enemigos dentro de `VisionRange` vía `Physics.OverlapSphere` contra `EnemyMask`.

Si encuentra un `IInteractable` en el primer collider detectado, transiciona automáticamente al estado de ataque correspondiente usando el factory method `unit.GetAttackState(target)` de [UnitController](../Unit%20Scripts/UnitController.md) — así el idle no necesita saber si la unidad es melee o ranged.

Al entrar, si `unit is WorkerController`, llama a [WorkerController.UpdateCarryAnimation](../Unit%20Scripts/WorkerController.md) (setea `IsCarrying` según si lleva recursos) — es el único lugar de este estado *compartido* por todas las unidades que sabe algo específico de worker, con el chequeo de tipo explícito para no afectar a melee/ranged.
