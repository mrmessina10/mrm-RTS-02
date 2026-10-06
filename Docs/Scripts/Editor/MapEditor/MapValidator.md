# MapValidator

`Assets/_Scripts/Editor/MapEditor/MapValidator.cs`

Botón "Validar mapa" de [MapEditorWindow](MapEditorWindow.md). Devuelve una lista de `MapValidationMessage` (error / aviso / info) que la ventana muestra como HelpBox.

| Chequeo | Nivel |
|---|---|
| Al menos un Spawn de enemigos; exactamente un Inicio del jugador; a lo sumo un Fin de oleada | Error |
| Marcador bajo el nivel de agua | Error |
| Cada marcador sobre NavMesh y ruta completa (`NavMesh.CalculatePath`) desde cada spawn hasta el Inicio del jugador y hasta el Fin de oleada | Error |
| Sin Fin de oleada | Info (el camino termina en el HQ) |
| Sin caminos, o camino con menos de 2 waypoints | Aviso |
| Sin nodos de Wood o de Food | Aviso |
| Menos del 40 % de celdas edificables alrededor del Inicio del jugador | Aviso |

Los chequeos de ruta dependen de que el NavMesh esté al día: hay que hacer Bake después de cambiar relieve, agua u obstáculos.
