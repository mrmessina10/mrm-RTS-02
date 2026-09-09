# DropOffBuilding

`Assets/_Scripts/Buildings/DropOffBuilding.cs`

Implementa [IDropOffPoint](../Resources/ResourceType.md) (`Type = InteractionType.None`, no es interactuable por click directo). Un edificio de entrega declara qué recursos acepta vía `acceptedResources` (lista configurable desde el inspector — ej. Town Center acepta todos, Lumber Camp solo madera).

`AcceptsResource(type)` chequea pertenencia a esa lista. `Deposit(type, amount)` llama a [ResourceManager.AddResource](../_CORE%20Scripts/ResourceManager.md).

`OnEnable()`/`OnDisable()` registran/desregistran el edificio en [BuildingManager](../_CORE%20Scripts/BuildingManager.md) para que [WorkerMoveToDropOffState](../StateMachines/WorkerMoveToDropOffState.md) pueda encontrarlo. Con esto queda cerrada la Fase 0 del roadmap (loop economía: recolectar → depositar → sumar al inventario global).
