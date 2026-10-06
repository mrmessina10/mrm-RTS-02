# MarkerTool

`Assets/_Scripts/Editor/MapEditor/MarkerTool.cs`

Herramienta **Marcadores**. Coloca los [MapMarker](../../Map/MapMarker.md) del loop de juego: Spawn de enemigos (varios), Fin de oleada e Inicio del jugador (únicos — colocar otro mueve el existente).

Click coloca, arrastrar mueve, `Shift` + click borra. La ventana lista los marcadores existentes con botones para encuadrarlos o borrarlos, y muestra qué significa cada tipo.

`CreateMarker` es público y estático porque también lo usa [MapSceneSync](MapSceneSync.md) al reconstruir la escena desde un `MapDataSO`.
