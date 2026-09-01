# BuildingManager

`Assets/_Scripts/_CORE Scripts/BuildingManager.cs`

Singleton que lleva el registro de edificios de entrega activos (`List<IDropOffPoint>`) y resuelve, para un worker cargado de un recurso, cuál es el punto de entrega más cercano que lo acepta.

- `RegisterDropOff`/`UnregisterDropOff`: alta/baja en la lista.
- `GetNearestDropOff(position, type)`: búsqueda lineal O(n) filtrando por `AcceptsResource(type)` y quedándose con el de menor `Vector3.Distance`.

Consumido por [WorkerMoveToDropOffState](../StateMachines/WorkerMoveToDropOffState.md). Alimentado por [DropOffBuilding](../Buildings/DropOffBuilding.md), que debería registrarse a sí mismo en `OnEnable` (por ahora esa llamada está comentada, así que la lista siempre está vacía en runtime).
