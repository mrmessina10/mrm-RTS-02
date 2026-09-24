# BuildingDataSO

`Assets/_Scripts/ScriptableObjects/BuildingDataSO.cs`

ScriptableObject de datos de solo lectura para un tipo de edificio (uno por asset: `TownCenter`, `Lumbermill`, `Farm`, `Barracks`, `ArcherTower`, `Palisade`, `Gate` — ver enum `BuildingType` declarado en el mismo archivo). Sigue la convención del proyecto: todo dato que no cambia en runtime vive en un SO, no en campos de un MonoBehaviour (ver [FactionDataSO](FactionDataSO.md) como precedente).

Campos:
- `BuildingType`: identifica el tipo de edificio que describe este asset.
- `DisplayName`: nombre para UI (menú de construcción).
- `BuildingPrefab`: prefab a instanciar cuando se construye este edificio.
- `ConstructionCost`: lista de [ResourceCost](../Resources/ResourceType.md) (`ResourceType` + cantidad) que hay que descontar del inventario del jugador para construirlo.
- `ConstructionTime`: segundos que tarda en construirse una vez confirmada la colocación. Default `0` (instantáneo) — declarado pero todavía sin consumidor, ver abajo.
- `Footprint`: tamaño en celdas de grilla (ancho x profundidad) que ocupa el edificio sobre el terreno. Default `Vector2Int.one` (1x1).

Cualquier otro stat de edificio que surja durante el diseño (HP base, etc.) se agrega acá mismo como campo nuevo — es el único SO de datos de edificio, no se crea uno por stat.

Consumido por [BuildingPlacement](../Buildings/BuildingPlacement.md) (`Footprint`, para el `NavMeshObstacle` y la validación de placement), por [BuildingPlacementController](../Buildings/BuildingPlacementController.md)/[WallPlacementController](../Buildings/WallPlacementController.md) (`BuildingPrefab`, `ConstructionCost` — en el caso del muro, multiplicado por la cantidad de celdas del tramo) y por [ConstructionSite](../Buildings/ConstructionSite.md) (`ConstructionTime`).

Valores concretos ya decididos (costo, tiempo) para `Lumbermill`/`Farm` están en [Design-EconomyBalance.md](../../Design-EconomyBalance.md), no en este archivo — acá solo vive la estructura del dato, no el balance. Costo/tiempo de `Palisade`/`Gate`/`ArcherTower` todavía sin definir.
