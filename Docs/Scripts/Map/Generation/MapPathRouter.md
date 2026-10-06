# MapPathRouter

`Assets/_Scripts/Map/Generation/MapPathRouter.cs`

Etapa de caminos de [MapGenerator](MapGenerator.md): traza cada ruta con A* sobre la grilla de celdas del mapa, como plantea [Design-MapEditorAndProceduralGeneration.md §4](../../../Design-MapEditorAndProceduralGeneration.md#4-generación-procedural).

**Costo por celda** = 1 + ruido × `Wander` + pendiente + agua + cercanía al borde.

- El ruido (fBm de baja frecuencia, no ruido blanco) es lo que hace que el camino serpentee en curvas amplias en vez de ir recto o temblar celda a celda.
- Acantilado y agua no son infranqueables, son caros (`CliffCost`, `WaterCost`): el camino rodea una meseta o un lago si el desvío es razonable y, si no, lo cruza. Después [MapTerrainShaper.CarveRoutes](MapTerrainShaper.md) talla la rampa o levanta el vado. Por eso siempre existe una ruta.

**Rutas**

- *Principal*: spawn 1 → HQ (se corta `PathStopDistance` celdas antes del HQ), o spawn 1 → empalme → fin de oleada en el layout A → B, con un costo extra alrededor del HQ para que el camino no pase por el medio de la base.
- *Spawns extra*: van hacia el mismo destino pagando solo el 25 % en las celdas que ya son camino. Así se pegan al camino existente y lo siguen hacia adelante. La ruta se corta en el primer contacto con la principal y desde ahí **reusa sus mismos waypoints**, para que el tramo compartido sea exactamente el mismo camino y no dos trazas casi superpuestas.
- *Acceso* (solo A → B): empalme → HQ.

De cada ruta quedan registrados el largo hasta la zona de defensa y el tramo compartido, que son los que chequea [MapRuleChecker](MapRuleChecker.md).

**Waypoints**: una celda cada `WaypointSpacing` del recorrido del A*. El `MapPath` los une con Catmull-Rom (`MapPath.GetSmoothedPoints`), y esa misma curva es la que se usa para nivelar el terreno.

**Implementación**: A* con vecindad de 8, heurística octil (escalada por el menor costo posible de una celda para que siga siendo admisible) y un heap binario propio (`CellHeap`), porque `PriorityQueue<T>` no está en el perfil .NET Standard 2.1 de Unity.

Fuentes: Hart, Nilsson & Raphael, "A Formal Basis for the Heuristic Determination of Minimum Cost Paths" (1968); Amit Patel, "Heuristics" (distancia octil) — <https://theory.stanford.edu/~amitp/GameProgramming/Heuristics.html>.
