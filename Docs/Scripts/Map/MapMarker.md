# MapMarker

`Assets/_Scripts/Map/MapMarker.cs`

Punto del mapa con significado para el loop de juego. Un solo componente con `MarkerType` (`MapMarkerType`, declarado en [MapDataSO](../ScriptableObjects/MapDataSO.md)); se dibuja siempre con gizmo de color y etiqueta.

| Tipo | Cantidad | Qué representa (según [Design-WavesAndCaravans.md](../../Design-WavesAndCaravans.md)) |
|---|---|---|
| `EnemySpawn` | 1 o más | Por dónde entra el tráfico del camino: oleadas y caravanas. |
| `WaveEnd` | 0 o 1 | Extremo de salida del camino en el layout "A → B con el jugador en el medio": por ahí se va la caravana y lo que no fue detenido. Si no existe, el camino termina en el HQ (layout base). |
| `PlayerStart` | 1 | Posición del City Center (HQ) y del grupo inicial; referencia para centrar la cámara al arrancar (Fase 7). |

La zona de construcción del jugador no es un marcador: es la máscara por celda del `MapDataSO` (ver [BuildZoneTool](../Editor/MapEditor/BuildZoneTool.md)).

Se colocan con [MarkerTool](../Editor/MapEditor/MarkerTool.md) y se consultan vía [MapRoot](MapRoot.md). Hoy ningún sistema de gameplay los consume todavía: son el contrato que van a leer el spawner de rondas (Fase 3) y el bootstrap de partida (Fase 7).
