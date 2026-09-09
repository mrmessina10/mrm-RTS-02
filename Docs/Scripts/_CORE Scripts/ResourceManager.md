# ResourceManager

`Assets/_Scripts/_CORE Scripts/ResourceManager.cs`

Singleton que lleva el inventario global de recursos del jugador: un diccionario `ResourceType → int` inicializado con todos los valores del enum en 0 (`Awake`). `AddResource(type, amount)` suma al inventario y dispara `OnResourceChanged` (para HUD/UI).

- `HasEnoughResources(List<ResourceCost>)`: chequeo de solo lectura, sin efectos secundarios — confirma que el inventario alcanza para cubrir toda una lista de costos (ej. `BuildingDataSO.ConstructionCost`) antes de gastar nada.
- `TrySpend(type, amount)`: descuenta un único recurso si hay stock suficiente, dispara `OnResourceChanged` y devuelve `bool` de éxito. No es atómico entre varios `ResourceType` a la vez — para gastar un costo compuesto, el llamador valida primero con `HasEnoughResources` y después llama `TrySpend` por cada entrada de la lista.

Quien le aporta recursos hoy es [DropOffBuilding.Deposit](../Buildings/DropOffBuilding.md), que ya invoca `AddResource` (Fase 0 del roadmap cerrada). Quien va a consumir `TrySpend`/`HasEnoughResources` es el futuro flujo de construcción (Fase 1), todavía no implementado.

`[DefaultExecutionOrder(-100)]` (mismo patrón que [GlobalUnitManager](GlobalUnitManager.md) y [BuildingManager](BuildingManager.md)): asegura que `Instance` exista antes de que `DropOffBuilding.Deposit` u otro consumidor lo necesiten.
