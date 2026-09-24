# DefensiveBuildingPrefabGenerator

`Assets/_Scripts/Editor/DefensiveBuildingPrefabGenerator.cs`

Script de Editor. Menú `Tools/RTS/Generate Palisade Wall Prefabs` genera, en una sola corrida, los dos edificios defensivos de footprint 1x1 pensados para trazarse/colocarse en la Empalizada:

- **`PalisadeSegment`** (`BuildingType.Palisade`): un segmento de muro, la unidad mínima que traza [WallPlacementController](../Buildings/WallPlacementController.md).
- **`Gate`** (`BuildingType.Gate`): edificio aparte de footprint único, colocado con el `BuildingPlacementController` normal — se supone que se funde visualmente con los muros contiguos (responsabilidad de arte).

Mismo patrón que [DropOffBuildingPrefabGenerator](DropOffBuildingPrefabGenerator.md), simplificado: sin `DropOffBuilding` (no aceptan depósitos) ni `UnitSelectionHandler` (no son seleccionables individualmente, igual que un `ResourceNode`). Cada prefab lleva `BoxCollider` (1x1x2, placeholder), [BuildingPlacement](../Buildings/BuildingPlacement.md) (`NavMeshObstacle` vía `RequireComponent`) y `Health`. Se guardan en `Assets/Prefabs/Buildings/Defense/`, y sus `BuildingDataSO` (`Footprint = (1,1)`, `ConstructionCost` vacío) en `Assets/_Scripts/ScriptableObjects/AssetsFromSO/`.

`ConstructionCost`/`ConstructionTime` quedan sin cargar — todavía no se decidió el número de balance para ninguno de los dos (ver [Design-EconomyBalance.md](../../Design-EconomyBalance.md), que hoy solo cubre Lumbermill/Farm/worker).

Si un prefab con ese nombre ya existe, lo saltea con warning.
