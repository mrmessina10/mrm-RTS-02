# BuildingManager

`Assets/_Scripts/_CORE Scripts/BuildingManager.cs`

Singleton que lleva el registro de edificios de entrega activos (`List<IDropOffPoint>`) y resuelve, para un worker cargado de un recurso, cuál es el punto de entrega más cercano que lo acepta.

- `RegisterDropOff`/`UnregisterDropOff`: alta/baja en la lista.
- `GetNearestDropOff(position, type)`: búsqueda lineal O(n) filtrando por `AcceptsResource(type)` y quedándose con el de menor `Vector3.Distance`.

Consumido por [WorkerMoveToDropOffState](../StateMachines/WorkerMoveToDropOffState.md). Alimentado por [DropOffBuilding](../Buildings/DropOffBuilding.md), que se registra/desregistra en `OnEnable`/`OnDisable` (Fase 0 del roadmap cerrada).

`[DefaultExecutionOrder(-100)]`, mismo patrón que [GlobalUnitManager](GlobalUnitManager.md): garantiza que `Awake()` (y por lo tanto `Instance`) exista antes de que un `DropOffBuilding` ya puesto en la escena dispare su propio `OnEnable`. Sin esto, el orden de `Awake`/`OnEnable` entre distintos GameObjects no está garantizado en Unity y el registro podría fallar en silencio para edificios pre-colocados en la escena.
