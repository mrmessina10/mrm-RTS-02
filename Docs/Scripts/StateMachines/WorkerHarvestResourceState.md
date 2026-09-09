# WorkerHarvestResourceState

`Assets/_Scripts/StateMachines/WorkerHarvestResourceState.cs`

Estado de recolección activa. Detiene al worker y acumula `harvestTimer`; cada `HarvestRate` segundos extrae `HarvestAmountPerCycle` del nodo (`IHarvestable.Harvest`) y lo suma a `currentCarriedAmount`.

Si el nodo se agota o desaparece mientras cosecha, vuelve a [WorkerMoveToResourceState](WorkerMoveToResourceState.md) (que decide el siguiente paso: buscar otro nodo o ir a depositar). Si la carga llega a `MaxCarryCapacity`, transiciona a [WorkerMoveToDropOffState](WorkerMoveToDropOffState.md).

Cada ciclo de cosecha loguea `[WorkerHarvestResourceState] {worker} +{harvested} {tipo}. Carga: {actual}/{max}` — reemplazo temporal de feedback visual (no hay HUD de carga del worker todavía). Sacarlo cuando exista.

`Enter` setea `UnitAnimator.SetBool("IsHarvestingWood"/"IsHarvestingFood", ...)` según `currentCarriedType` (mutuamente excluyentes). `Exit` resetea los dos a `false` explícitamente — si no, interrumpir la cosecha con otra orden (ataque, force-drop, etc.) dejaría la animación de cosechar trabada, porque el próximo estado no sabe nada de estos parámetros.
