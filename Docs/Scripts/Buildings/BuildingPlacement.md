# BuildingPlacement

`Assets/_Scripts/Buildings/BuildingPlacement.cs`

Componente de edificio que resuelve dos cosas relacionadas con NavMesh, usando como unidad de grilla 1 unidad de Unity = 1 celda (el `Footprint` de [BuildingDataSO](../ScriptableObjects/BuildingDataSO.md) está expresado directamente en esas celdas):

1. **Instancia construida → obstáculo estático.** Requiere `NavMeshObstacle` (`[RequireComponent]`) y en `Awake()` lo configura como caja del tamaño del `Footprint`, con `carving = true` y `carveOnlyStationary = true` (el edificio nunca se mueve, así que no hace falta recalcular el carve cada frame — ver [docs de Unity sobre NavMeshObstacle](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/AI.NavMeshObstacle.html)).
2. **Antes de construir → validación de área navegable.** `IsAreaBuildable(desiredCenter, footprint)` es estático (no depende de que el edificio exista todavía) y samplea el centro de cada celda del footprint con `NavMesh.SamplePosition`, comparando la posición muestreada contra la celda con una tolerancia chica — así confirma que el punto cae *sobre* el NavMesh navegable y no simplemente cerca. `GetFootprintOrigin` alinea el centro deseado a la esquina de grilla más cercana para que el footprint ocupe celdas enteras.

No está conectado a nada todavía: falta el sistema de placement/ghost que llame a `IsAreaBuildable` con la posición del cursor antes de confirmar la construcción, y la parte de UI que instancie el `BuildingPrefab` de `BuildingDataSO` en esa posición. `buildingData` es una referencia serializada al SO del propio edificio (no hay lookup automático todavía).
