# IInteractable

`Assets/_Scripts/Interfaces/IInteractable.cs`

Contrato para cualquier objeto del mundo que una unidad pueda tener como objetivo de un comando (click derecho sobre él): recursos, edificios, enemigos.

- `InteractionType Type`: clasifica la interacción (`None`, `Attack`, `Harvest`, `Build`, `Repair`). Lo usa [UnitController.SetTarget](../Unit%20Scripts/UnitController.md) y [WorkerController.SetTarget](../Unit%20Scripts/WorkerController.md) para decidir a qué estado transicionar.
- `GetTransform()`: posición del objeto interactuable.
- `Interact(UnitController unit)`: acción a ejecutar cuando una unidad interactúa directamente (ej. click manual, o fallback cuando el objetivo no es `IDamageable`).

Implementado por: [ResourceNode](../Resources/ResourceNode.md) (`Harvest`), [DropOffBuilding](../Buildings/DropOffBuilding.md) (`None`, no interactuable directo), [EnemyUnit](../Unit%20Scripts/EnemyUnit.md) (`Attack`). También heredado indirectamente por `IHarvestable` e `IDropOffPoint`, que lo extienden.

`InteractionType` es el enum que vive en este mismo archivo.
