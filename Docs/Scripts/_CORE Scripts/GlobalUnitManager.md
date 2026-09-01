# GlobalUnitManager

`Assets/_Scripts/_CORE Scripts/GlobalUnitManager.cs`

Singleton (`[DefaultExecutionOrder(-100)]` para inicializarse antes que cualquier unidad) que lleva el registro de todos los objetos seleccionables de la partida.

- `AllSelectables`: lista plana de todo lo `ISelectable` registrado, usada por [SelectionManager](SelectionManager.md) para el box selection (recorre todo).
- `UnitsByType`: diccionario `UnitType → List<UnitSelectionHandler>` para lookup O(1) por tipo, usado en el doble-click ("seleccionar todos los de este tipo visibles").

`Register`/`Unregister` los llama [UnitSelectionHandler](UnitSelectionHandler.md) en sus propios `OnEnable`/`OnDisable`. El registro se limita a instancias de `UnitSelectionHandler`; cualquier otro `ISelectable` solo entra a `AllSelectables`.

## Fuente / patrón
Singleton + patrón Registry (registro central de instancias activas) para evitar `FindObjectsOfType` costoso en cada selección.
