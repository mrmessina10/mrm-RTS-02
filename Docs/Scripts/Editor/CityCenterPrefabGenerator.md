# CityCenterPrefabGenerator

`Assets/_Scripts/Editor/CityCenterPrefabGenerator.cs`

Script de Editor. Menú `Tools/RTS/Generate City Center Prefab` genera el City Center (Centro de la Ciudad) con su rol triple ya cableado — ver [Design-EconomyBalance.md §3](../../Design-EconomyBalance.md#3-city-center--rol-triple). Mismo patrón que [DropOffBuildingPrefabGenerator](DropOffBuildingPrefabGenerator.md).

Crea, si no existen:
- `UnitData_Worker.asset` ([UnitDataSO](../ScriptableObjects/UnitDataSO.md)): `UnitType = Worker`, prefab `Assets/Prefabs/testWorkerUnit.prefab`, costo 20 Food, 10 s de producción.
- `BuildingData_CityCenter.asset` ([BuildingDataSO](../ScriptableObjects/BuildingDataSO.md)): `BuildingType = CityCenter`, `Footprint = (4,4)`, `ProducibleUnits = [UnitData_Worker]`. Sin `ConstructionCost`/`ConstructionTime`: el City Center arranca ya construido (Fase 7), el jugador no lo coloca.
- `Assets/Prefabs/Buildings/Pop/CityCenter.prefab`.

Estructura del prefab:
- **Root** (layer `Buildings`, escala 1): `BoxCollider` (4 x 3 x 4), [BuildingPlacement](../Buildings/BuildingPlacement.md), [DropOffBuilding](../Buildings/DropOffBuilding.md) (`acceptedResources = [Wood, Food, Stone]` — sin Gold, que solo entra por comercio), [UnitProducer](../Buildings/UnitProducer.md), [Health](../_CORE%20Scripts/Health.md) (500 HP, `onDeathChannel = Channel_GameOver`), [Headquarters](../Buildings/Headquarters.md), `UnitSelectionHandler` (`unitType = Building`).
- **Hijo "SpawnPoint"**: punto de salida de las unidades producidas, 1.5 unidades por fuera del borde del footprint sobre -Z.
- **Hijo "Visual"**: cubo placeholder 4x3x4, sin collider.

`DropOffBuilding` se agrega antes que `Headquarters` a propósito — ver [Headquarters](../Buildings/Headquarters.md#orden-de-componentes-en-el-prefab).

Si el prefab ya existe no hace nada (warning). Los SO existentes se reutilizan sin pisarlos.

Valores placeholder, sin diseño detrás todavía: footprint 4x4, 500 HP.
