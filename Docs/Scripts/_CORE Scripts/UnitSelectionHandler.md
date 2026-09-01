# UnitSelectionHandler

`Assets/_Scripts/_CORE Scripts/UnitSelectionHandler.cs`

Implementa [ISelectable](../Interfaces/ISelectable.md) para una unidad. Guarda el `UnitType` (`None`, `Worker`, `Melee`, `Ranged`, `Siege`, `Building`) usado para clasificar unidades y activa/desactiva un `selectionRing` visual al seleccionar/deseleccionar.

Se registra/desregistra en [GlobalUnitManager](GlobalUnitManager.md) en `OnEnable`/`OnDisable` (no en `Start`/`OnDestroy`) para que desactivar temporalmente una unidad también la saque de la selección disponible.

`UnitType` (enum) vive en este mismo archivo. Es un componente separado de [UnitController](../Unit%20Scripts/UnitController.md): éste maneja combate/movimiento, `UnitSelectionHandler` solo maneja selección — se asume que ambos coexisten en el mismo GameObject.
