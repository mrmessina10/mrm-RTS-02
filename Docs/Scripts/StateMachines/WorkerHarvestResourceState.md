# WorkerHarvestResourceState

`Assets/_Scripts/StateMachines/WorkerHarvestResourceState.cs`

Estado de recolección activa. Detiene al worker y acumula `harvestTimer`; cada `HarvestRate` segundos extrae `HarvestAmountPerCycle` del nodo (`IHarvestable.Harvest`) y lo suma a `currentCarriedAmount`.

Si el nodo se agota o desaparece mientras cosecha, vuelve a [WorkerMoveToResourceState](WorkerMoveToResourceState.md) (que decide el siguiente paso: buscar otro nodo o ir a depositar). Si la carga llega a `MaxCarryCapacity`, transiciona a [WorkerMoveToDropOffState](WorkerMoveToDropOffState.md).
