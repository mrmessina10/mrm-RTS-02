# DropOffBuilding

`Assets/_Scripts/Buildings/DropOffBuilding.cs`

Implementa [IDropOffPoint](../Resources/ResourceType.md) (`Type = InteractionType.None`, no es interactuable por click directo). Un edificio de entrega declara qué recursos acepta vía `acceptedResources` (lista configurable desde el inspector — ej. Town Center acepta todos, Lumber Camp solo madera).

`AcceptsResource(type)` chequea pertenencia a esa lista. `Deposit(type, amount)` hoy es un mock (`Debug.Log`, no llama a [ResourceManager.AddResource](../_CORE%20Scripts/ResourceManager.md) todavía).

`OnEnable()` debería registrar el edificio en [BuildingManager](../_CORE%20Scripts/BuildingManager.md) para que [WorkerMoveToDropOffState](../StateMachines/WorkerMoveToDropOffState.md) pueda encontrarlo — esa llamada está comentada por ahora, con lo cual `BuildingManager` nunca recibe altas en runtime; falta también el `Unregister` simétrico en `OnDisable`.
