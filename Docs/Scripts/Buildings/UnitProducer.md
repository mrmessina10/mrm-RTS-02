# UnitProducer

`Assets/_Scripts/Buildings/UnitProducer.cs`

Componente composable que le da a un edificio la capacidad de producir unidades — misma idea que [DropOffBuilding](DropOffBuilding.md): es una faceta que se suma al prefab, no una clase de edificio. Implementa [IUnitProducer](../Interfaces/IUnitProducer.md). Hoy lo lleva el City Center (workers); el Barracks va a reusar el mismo componente con otro `BuildingDataSO`.

Qué produce no vive en el componente: lee `ProducibleUnits` y `ProductionQueueCapacity` del [BuildingDataSO](../ScriptableObjects/BuildingDataSO.md) referenciado, y de cada [UnitDataSO](../ScriptableObjects/UnitDataSO.md) el costo, el tiempo y el prefab.

- `TryEnqueueUnit(UnitType)`: busca el `UnitDataSO` de ese tipo entre los producibles, valida cupo de cola y recursos ([ResourceManager](../_CORE%20Scripts/ResourceManager.md)), **cobra el costo al encolar** y agrega el pedido. Devuelve `false` (con log) si el edificio no produce ese tipo, la cola está llena o no alcanzan los recursos. También devuelve `false` si el componente está deshabilitado (edificio en obra, ver [ConstructionSite](ConstructionSite.md)).
- `Update`: cola FIFO, solo avanza el tiempo del pedido que está al frente. Al llegar a `ProductionTime` instancia el prefab y pasa al siguiente.
- Punto de salida: `spawnPoint` (hijo del prefab, fuera del footprint) ajustado al NavMesh con `NavMesh.SamplePosition`; si no hay `spawnPoint` usa la posición del edificio.
- `QueuedCount` / `CurrentProgress` (0 a 1): expuestos para el HUD (Fase 8), hoy sin consumidor.

Quién lo dispara: [SelectionManager](../_CORE%20Scripts/SelectionManager.md), al recibir `InputReader.TrainUnitRequestEvent` con el edificio seleccionado.

## Decisiones

- **Cobro al encolar, no al terminar**: el recurso queda comprometido en el momento de la decisión, así la cola nunca queda trabada esperando recursos. Convención de Age of Empires 2.
- **Sin cancelación ni reembolso**: no hay forma de sacar un pedido de la cola todavía. Si el edificio se destruye con pedidos pendientes, lo cobrado se pierde.
- **Sin rally point**: la unidad aparece en el punto de salida y queda en idle.
- **Sin límite poblacional**: la producción no consulta ningún cap — queda para cuando se implemente ese ítem de la Fase 2.
