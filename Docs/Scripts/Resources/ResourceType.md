# ResourceType.cs

`Assets/_Scripts/Resources/ResourceType.cs`

No es una sola clase — es el archivo donde viven los enums/interfaces compartidos del sistema de economía:

- **`ResourceType`** (enum): Wood, Food, Gold, Stone. Usado en todo el sistema de recolección/inventario/edificios.
- **`WorkerState`** (enum): Idle, MovingToResource, Gathering, MovingToDropOff, Fleeing. Declarado pero no referenciado todavía — el estado real del worker lo maneja la FSM ([WorkerController](../Unit%20Scripts/WorkerController.md) + estados en `StateMachines/`), no este enum.
- **`IHarvestable`** (interfaz, extiende [IInteractable](../Interfaces/IInteractable.md)): `ResourceType`, `Harvest(amount)`, `IsDepleted`, `Position`. Implementada por [ResourceNode](ResourceNode.md).
- **`IDropOffPoint`** (interfaz, extiende `IInteractable`): `AcceptsResource(type)`, `Deposit(type, amount)`, `Position`. Implementada por [DropOffBuilding](../Buildings/DropOffBuilding.md).

Agrupar estos tipos acá (en vez de un archivo por tipo) evita fragmentar demasiado el namespace de economía, que es chico todavía.
