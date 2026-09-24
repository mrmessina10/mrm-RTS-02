# PlacementModeState

`Assets/_Scripts/Buildings/PlacementModeState.cs`

Flag estático compartido (`public static bool IsActive`) entre todos los controllers de "modo colocación" — [BuildingPlacementController](BuildingPlacementController.md) y [WallPlacementController](WallPlacementController.md) hoy, cualquiera que se sume después.

Cada controller lo pone en `true` al entrar en su propio modo y en `false` al salir (confirmar del todo, cancelar). [SelectionManager](../_CORE%20Scripts/SelectionManager.md) lo chequea una sola vez (`HandleSelect`/`HandleMoveCommand`) para saber si debe ceder el click al controller de colocación activo en vez de procesar selección/comando — sin necesitar una referencia a cada controller individual. Reemplaza el chequeo anterior (`BuildingPlacementController.Instance.IsPlacing`), que no escalaba a un segundo modo de colocación.
