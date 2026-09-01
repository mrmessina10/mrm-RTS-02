# ResourceNode

`Assets/_Scripts/Resources/ResourceNode.cs`

Implementa [IHarvestable](ResourceType.md) e [IInteractable](../Interfaces/IInteractable.md) (`Type = InteractionType.Harvest`). Representa un depósito de recurso en el mundo con una cantidad finita (`maxCapacity` → `currentAmount`).

`Harvest(amount)` extrae `Min(amount, currentAmount)` (nunca saca de más) y, al agotarse (`IsDepleted`), destruye el GameObject — no queda un nodo vacío en escena, desaparece directamente. Es el mismo evento que dispara la búsqueda de nodo alternativo en [WorkerMoveToResourceState](../StateMachines/WorkerMoveToResourceState.md).

`Interact()` es un placeholder (loguea info del nodo); pensado para futura UI de inspección al clickear un recurso manualmente.
