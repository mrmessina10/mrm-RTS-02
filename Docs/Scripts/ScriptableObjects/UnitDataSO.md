# UnitDataSO

`Assets/_Scripts/ScriptableObjects/UnitDataSO.cs`

ScriptableObject de datos de solo lectura para un tipo de unidad producible (uno por asset). Equivalente de [BuildingDataSO](BuildingDataSO.md) para unidades, misma convención: lo que no cambia en runtime vive en un SO.

Campos:
- `UnitType`: tipo de unidad (enum declarado en [UnitSelectionHandler](../_CORE%20Scripts/UnitSelectionHandler.md)). Es la clave con la que [UnitProducer](../Buildings/UnitProducer.md) resuelve qué asset corresponde a un pedido.
- `DisplayName`: nombre para UI y logs.
- `UnitPrefab`: prefab a instanciar al terminar la producción.
- `ProductionCost`: lista de [ResourceCost](../Resources/ResourceType.md) que se descuenta al encolar.
- `ProductionTime`: segundos de producción una vez que el pedido llega al frente de la cola.

Un edificio declara qué unidades produce listando estos assets en `BuildingDataSO.ProducibleUnits`.

Assets existentes: `UnitData_Worker` (20 Food, 10 s, prefab `testWorkerUnit`), creado por [CityCenterPrefabGenerator](../Editor/CityCenterPrefabGenerator.md). El razonamiento de los números está en [Design-EconomyBalance.md](../../Design-EconomyBalance.md).

Los stats de combate/recolección de la unidad siguen en su prefab (`UnitController`/`WorkerController`); este SO solo cubre lo que necesita la producción.
