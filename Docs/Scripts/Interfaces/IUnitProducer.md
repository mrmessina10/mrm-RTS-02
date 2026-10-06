# IUnitProducer

`Assets/_Scripts/Interfaces/IUnitProducer.cs`

Contrato de un edificio que produce unidades: `TryEnqueueUnit(UnitType)` para pedir una unidad, y `QueuedCount` / `CurrentProgress` (0 a 1) para leer el estado de la cola.

[SelectionManager](../_CORE%20Scripts/SelectionManager.md) emite la orden de producción contra esta interfaz sobre lo que esté seleccionado, sin conocer qué edificio es.

Implementado por [UnitProducer](../Buildings/UnitProducer.md).
