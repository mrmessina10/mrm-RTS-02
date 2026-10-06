# MapGenerationContext

`Assets/_Scripts/Map/Generation/MapGenerationContext.cs`

Estado de trabajo de un intento de generación. Las etapas de [MapGenerator](MapGenerator.md) lo van completando en orden y al final se convierte en un [GeneratedMap](GeneratedMap.md).

**Coordenadas.** Todo el generador trabaja en coordenadas locales del mapa: XZ entre 0 y `Size`, 1 unidad = 1 celda (el mismo criterio de grilla de `BuildingPlacement` y `MapDataSO`), y alturas en unidades de mundo con el suelo base en 0. `ToWorld` suma el origen del mapa recién al armar el resultado.

**Qué guarda**

- Layout, campo de alturas, modo de agua, ancho de camino.
- Listas: zonas reservadas, mesetas, rampas, vados, rutas (`MapRoute`: celdas del A*, waypoints, polilínea suavizada, largo hasta la zona de defensa y tramo compartido), depósitos de recursos (`MapResourceCluster`, con su punto de drop-off) y objetos colocados.
- Grillas por celda: pendiente, altura, `TerrainBlocked` (acantilado o agua), `Obstacle` (objetos que bloquean), `KeepClear`, `NotBuildable`, `Reachable`, `PathDistance`.

**Zonas reservadas.** `GetReserveMask` vale 0 dentro de una zona (inicio, spawns, fin de oleada, empalme) y sube a 1 en 8 celdas. Relieve, mesetas y agua se multiplican por esa máscara: es lo que garantiza por construcción que el HQ y los spawns queden en suelo plano y seco, sin depender de que la ubicación de cada accidente haya salido bien.

**Caminabilidad.** Una celda está bloqueada si su pendiente supera `MaxWalkableSlope` (35° por defecto, más estricto que los 45° del NavMesh) o su centro queda bajo el nivel de agua. `FloodFillReachable` marca lo alcanzable desde el HQ con vecindad de 4 (también más estricto que el NavMesh, que pasa en diagonal). Es una aproximación conservadora del bake: si el generador dice que hay ruta, el NavMesh debería tenerla.

`CreateRandom(stage)` entrega el [MapRandom](MapRandom.md) de cada etapa.
