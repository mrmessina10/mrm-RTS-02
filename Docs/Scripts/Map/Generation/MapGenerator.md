# MapGenerator

`Assets/_Scripts/Map/Generation/MapGenerator.cs`

Punto de entrada del generador procedural de mapas ([Design-MapEditorAndProceduralGeneration.md §4](../../../Design-MapEditorAndProceduralGeneration.md#4-generación-procedural)). `Generate(request)` recibe un [MapGenerationRequest](MapGenerationRequest.md) y devuelve un [GeneratedMap](GeneratedMap.md). No toca escena, assets ni `Debug.Log`: es código de runtime sobre datos planos, y por eso se puede correr fuera de Unity para probar semillas en tanda.

**Etapas de un intento**, cada una sobre el mismo [MapGenerationContext](MapGenerationContext.md):

| # | Etapa | Script |
|---|---|---|
| 1 | Layout: borde de entrada, spawns, HQ, fin de oleada; zonas reservadas | [MapLayoutPlanner](MapLayoutPlanner.md) |
| 2 | Relieve de base, mesetas con rampas, agua | [MapTerrainShaper](MapTerrainShaper.md) |
| 3 | Rutas con A* sobre el relieve | [MapPathRouter](MapPathRouter.md) |
| 4 | Nivelado del terreno bajo cada ruta (rampas, vados) | `MapTerrainShaper.CarveRoutes` |
| 5 | Máscaras: distancia al camino, zonas despejadas, no edificable | `MapGenerator.BuildMasks` |
| 6 | Objetos: depósitos iniciales, expansiones, conjuntos, decoración | [MapObjectScatterer](MapObjectScatterer.md) |
| 7 | Reglas de estructura | [MapRuleChecker](MapRuleChecker.md) |

**Generar y verificar.** Si el intento rompe una regla obligatoria se descarta y se corre otro (`MaxAttempts` del asset de reglas). Cada intento usa streams de [MapRandom](MapRandom.md) distintos, derivados de la semilla, el número de intento y la etapa: el resultado final depende solo de (semilla, reglas, tamaño), y cambiar un parámetro de una etapa no altera el sorteo de las demás. Si ningún intento pasa, devuelve el que menos reglas rompió, marcado como no válido, con el informe de qué falló. `DiscardedRules` junta las reglas que descartaron intentos, para que la tanda de semillas del panel muestre dónde aprietan las reglas.

**Máscaras** (`BuildMasks`):

- `PathDistance`: distancia de cada celda a la ruta más cercana.
- `KeepClear`: donde la decoración no puede caer — camino, zona de inicio, spawns, rampas y vados. Evita que un objeto tape un paso obligado.
- `NotBuildable`: camino con su margen, spawns/fin de oleada y dos celdas de borde. Agua y acantilados no se marcan: en juego ya los excluye el NavMesh (`BuildingPlacement.IsAreaBuildable`).

La conectividad se calcula dos veces con relleno por inundación desde el HQ: una antes de colocar objetos (dónde se puede poner un depósito) y otra después, con los objetos que bloquean el paso (lo que chequean las reglas).

Fuente del enfoque: Shaker, Togelius & Nelson, *Procedural Content Generation in Games* (2016), cap. 1 — métodos constructivos vs. generar-y-verificar — <https://www.pcgbook.com/>.
