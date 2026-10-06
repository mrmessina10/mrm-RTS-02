# MapRoot

`Assets/_Scripts/Map/MapRoot.cs`

Raíz del mapa en la escena y punto de consulta del gameplay para todo lo que dependa del mapa. Jerarquía que arma [MapSceneSetup](../Editor/MapEditor/MapSceneSetup.md):

```
Map_<Nombre>      MapRoot + NavMeshSurface
├─ Terrain        Terrain + TerrainCollider (layer Ground)
├─ Water          NavMeshModifierVolume (Not Walkable) + hijo Surface (plano)
├─ Resources      nodos de recurso (incluye todos los árboles)
├─ Nature         rocas, matorrales
├─ ManMade        casas, cercas, ruinas
├─ Markers        MapMarker
└─ Paths          MapPath
```

Referencia el [MapDataSO](../ScriptableObjects/MapDataSO.md) activo y expone:

- `IsBuildable(worldPosition)` — delega en la máscara del `MapDataSO`. Lo consulta [BuildingPlacement](../Buildings/BuildingPlacement.md).
- `GetMarkers()` / `GetMarker(type)` — [MapMarker](MapMarker.md) del loop de juego (spawns, fin de oleada, inicio del jugador). Es lo que va a leer el spawner de oleadas (Fase 3) y el bootstrap de partida (Fase 7).
- `GetPaths()` — [MapPath](MapPath.md).
- `GetContainer(category)` — contenedor de cada `MapObjectCategory`.

Singleton con el patrón del resto de los managers (`Instance` en `Awake`, `[DefaultExecutionOrder(-100)]`), con una diferencia deliberada: ante un duplicado loguea un warning en vez de hacer `Destroy(gameObject)`, porque destruir un `MapRoot` se lleva el terreno y todos los objetos del mapa.

`Instance` solo existe en play mode. Las herramientas de editor lo buscan con `MapSceneSetup.FindMapRoot()`.
