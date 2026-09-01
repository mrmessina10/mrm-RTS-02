# ISelectable

`Assets/_Scripts/Interfaces/ISelectable.cs`

Contrato para cualquier objeto que pueda ser seleccionado por el jugador con click o box-selection: `OnSelect()` / `OnDeselect()`.

Implementado por [UnitSelectionHandler](../_CORE%20Scripts/UnitSelectionHandler.md), que además se auto-registra en [GlobalUnitManager](../_CORE%20Scripts/GlobalUnitManager.md) al activarse.

Consumido por [SelectionManager](../_CORE%20Scripts/SelectionManager.md), que mantiene la lista de unidades seleccionadas y llama `OnSelect`/`OnDeselect` según la interacción del jugador (click simple, shift-click, box selection, grupos de control).
