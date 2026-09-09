# BuildingDataSO

`Assets/_Scripts/ScriptableObjects/BuildingDataSO.cs`

ScriptableObject de datos de solo lectura para un tipo de edificio (uno por asset: Lumbermill, Farm, TownCenter, Barracks — ver enum `BuildingType` declarado en el mismo archivo). Sigue la convención del proyecto: todo dato que no cambia en runtime vive en un SO, no en campos de un MonoBehaviour (ver [FactionDataSO](FactionDataSO.md) como precedente).

Campos:
- `BuildingType`: identifica el tipo de edificio que describe este asset.
- `DisplayName`: nombre para UI (menú de construcción).
- `BuildingPrefab`: prefab a instanciar cuando se construye este edificio.
- `ConstructionCost`: lista de [ResourceCost](../Resources/ResourceType.md) (`ResourceType` + cantidad) que hay que descontar del inventario del jugador para construirlo.
- `ConstructionTime`: segundos que tarda en construirse una vez confirmada la colocación. Default `0` (instantáneo) — declarado pero todavía sin consumidor, ver abajo.
- `Footprint`: tamaño en celdas de grilla (ancho x profundidad) que ocupa el edificio sobre el terreno. Default `Vector2Int.one` (1x1).

Cualquier otro stat de edificio que surja durante el diseño (HP base, etc.) se agrega acá mismo como campo nuevo — es el único SO de datos de edificio, no se crea uno por stat.

Consumido por [BuildingPlacement](../Buildings/BuildingPlacement.md) (`Footprint`, para el `NavMeshObstacle` y la validación de placement) y por [BuildingPlacementController](../Buildings/BuildingPlacementController.md) (`BuildingPrefab`, `ConstructionCost`). `ConstructionTime` es el único campo sin cablear todavía: hoy `BuildingPlacementController.HandleConfirm` instancia el prefab real de forma instantánea al confirmar — falta un estado de "en construcción" (demora, feedback visual, tal vez que no sea funcional hasta terminar) que lo use.
