# IHeadquarters

`Assets/_Scripts/Interfaces/IHeadquarters.cs`

Contrato del HQ del jugador: extiende [IInteractable](IInteractable.md) y suma `Position`. Es el tipo con el que [BuildingManager](../_CORE%20Scripts/BuildingManager.md) expone el HQ (`BuildingManager.Instance.Headquarters`), para que la IA de oleadas lo consuma sin conocer la clase concreta.

Extiende `IInteractable` a propósito: los estados de ataque existentes reciben su objetivo como `IInteractable`, así que el HQ se les puede pasar directo.

Implementado por [Headquarters](../Buildings/Headquarters.md).
