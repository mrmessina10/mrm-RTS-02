# MapGenerationApplier

`Assets/_Scripts/Editor/MapEditor/MapGenerationApplier.cs`

Vuelca un [GeneratedMap](../../Map/Generation/GeneratedMap.md) al mapa de la escena. El generador no sabe nada de `Terrain` ni de assets; esta clase es la traducción, y la hace con las piezas que ya tenía el editor de mapas:

| Paso | Con qué |
|---|---|
| Relieve | Remuestrea el campo de alturas a la resolución del heightmap y lo escribe con `TerrainData.SetHeights`. |
| Nivel de agua | `MapSceneSetup.ApplyWaterLevel`. |
| Zona edificable | `MapDataSO.SetCellBuildable` por celda. |
| Objetos, marcadores, caminos | `MapDataSO.SetContents` y después `MapSceneSync.RebuildSceneFromMapData`: el mapa generado entra por el mismo camino que un mapa cargado desde un asset. |
| Texturas | `TerrainBrushUtility.AutoTexture` (pasto, roca en pendientes, arena en orillas), los parches del generador con `PaintLayer` y los caminos con `PathTool.PaintPath`. |

Las texturas van al final porque `AutoTexture` lee la pendiente del relieve ya escrito y `PaintPath` necesita los `MapPath` reconstruidos en la escena.

Todo queda en un único grupo de undo ("Mapa: generar"). Al terminar recentra la cámara RTS en el HQ nuevo y marca la escena como modificada; el `MapData` se guarda al guardar la escena, como siempre ([MapSceneSync](MapSceneSync.md)).

Es de editor: usa `Undo`, `PrefabUtility` (vía `MapSceneSync`) y `TerrainBrushUtility`. Para generar en runtime hace falta otro aplicador que escriba lo mismo sin esas dependencias; el `GeneratedMap` no cambia.
