# ResourceType.cs

`Assets/_Scripts/Resources/ResourceType.cs`

No es una sola clase — es el archivo donde viven los enums/interfaces compartidos del sistema de economía:

- **`ResourceType`** (enum): Wood, Food, Gold, Stone. Usado en todo el sistema de recolección/inventario/edificios.
- **`IHarvestable`** (interfaz, extiende [IInteractable](../Interfaces/IInteractable.md)): `ResourceType`, `Harvest(amount)`, `IsDepleted`, `Position`. Implementada por [ResourceNode](ResourceNode.md).
- **`IDropOffPoint`** (interfaz, extiende `IInteractable`): `AcceptsResource(type)`, `Deposit(type, amount)`, `Position`. Implementada por [DropOffBuilding](../Buildings/DropOffBuilding.md).
- **`ResourceCost`** (struct serializable): par `ResourceType` + `Amount`. Reutilizable para cualquier costo (edificios, y a futuro unidades/mejoras). Primer consumidor: [BuildingDataSO](../ScriptableObjects/BuildingDataSO.md).

Agrupar estos tipos acá (en vez de un archivo por tipo) evita fragmentar demasiado el namespace de economía, que es chico todavía.

## Dirección futura

`ResourceType` va a quedar acotado exclusivamente a recursos recolectables (`Harvestable`) — madera, comida, piedra. El oro dejó de ser recolectable por diseño: solo se consigue comerciando con caravanas (ver [Design-WavesAndCaravans.md §3](../../Design-WavesAndCaravans.md#3-caravanas)). El valor `Gold` sigue en el enum; falta decidir si se queda como recurso de inventario sin nodos ni drop-off o si pasa al sistema de comercio. Los recursos estratégicos de comercio ("strategic trading resources") que se obtienen a través de caravanas van a vivir en un enum separado y correr por un sistema paralelo independiente del de recolección/inventario actual, sin mezclarse con `ResourceType`, `IHarvestable` ni `IDropOffPoint`.
