# PathTool

`Assets/_Scripts/Editor/MapEditor/PathTool.cs`

Herramienta **Caminos**. Edita los [MapPath](../../Map/MapPath.md) del mapa:

- Click agrega un waypoint al final; `Ctrl` + click lo inserta en el tramo más cercano; arrastrar un punto lo mueve sobre el terreno; `Shift` + click sobre un punto lo borra.
- Lista de caminos con selección del activo, alta y baja. El primer click crea un camino si no hay ninguno.

El camino es un dato lógico; volcarlo al terreno son tres acciones separadas y opcionales, todas sobre la polilínea suavizada (`MapPath.GetSmoothedPoints`):

| Acción | Efecto |
|---|---|
| Pintar camino sobre el terreno | Pinta la capa `Road` con el ancho del camino. |
| Nivelar terreno bajo el camino | Suaviza el relieve a lo largo del recorrido para que sea caminable y reapoya los waypoints. |
| Bloquear construcción sobre el camino | Marca como no edificables las celdas dentro de `ancho/2 + margen` en la máscara del [MapDataSO](../../ScriptableObjects/MapDataSO.md) — la "zona no edificable" del camino que pide el doc de diseño. |

Los waypoints y el ancho se escriben por `SerializedObject` (campos sin setter público), con lo cual el undo es automático.

`PaintPath` es público: [MapGenerationApplier](MapGenerationApplier.md) lo reutiliza para pintar los caminos de un mapa generado. Nivelar y bloquear no se reutilizan porque el generador ya los resuelve sobre sus propios datos (necesita el relieve y la máscara finales para chequear las reglas).
