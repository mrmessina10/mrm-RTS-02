# MapEditorWindow

`Assets/_Scripts/Editor/MapEditor/MapEditorWindow.cs`

Ventana del editor de mapas (`Tools/RTS/Map Editor`). Es la implementación del editor descrito en [Design-MapEditorAndProceduralGeneration.md §3](../../../Design-MapEditorAndProceduralGeneration.md#3-editor-de-mapas).

**Sin mapa en la escena** muestra el formulario de creación (nombre + tamaño) y delega en [MapSceneSetup](MapSceneSetup.md).

**Con mapa** muestra tres secciones:

- *Mapa*: `MapData` activo, paleta, aviso si queda otro `NavMeshSurface` activo (el suelo anterior) con botón para desactivarlo, cámara RTS (agregarla si falta o recentrarla, vía [MapCameraSetup](MapCameraSetup.md)), Bake NavMesh, validación ([MapValidator](MapValidator.md)) y guardar/reconstruir/cargar contra el `MapDataSO` ([MapSceneSync](MapSceneSync.md)).
- *Generación procedural*: [MapGeneratorPanel](MapGeneratorPanel.md), una clase `[Serializable]` aparte igual que las herramientas. Cuando vuelca un mapa a la escena la ventana suelta la herramienta activa y descarta el resultado de la última validación.
- *Herramienta*: grilla con las 7 herramientas ([TerrainSculptTool](TerrainSculptTool.md), [TerrainPaintTool](TerrainPaintTool.md), [WaterTool](WaterTool.md), [ObjectBrushTool](ObjectBrushTool.md), [PathTool](PathTool.md), [MarkerTool](MarkerTool.md), [BuildZoneTool](BuildZoneTool.md)) y los parámetros de la activa.

**Input sobre la Scene view** (`SceneView.duringSceneGui`). La ventana resuelve lo común a todas las herramientas, que heredan de [MapEditorTool](MapEditorTool.md):

- Raycast del mouse contra el `TerrainCollider` del mapa (no contra toda la física, así los objetos colocados no tapan el terreno).
- Cursor de pincel: círculo apoyado sobre el relieve con el radio/color que informa la herramienta.
- Ciclo de trazo: registra un control por defecto (`HandleUtility.AddDefaultControl`) para que el click izquierdo no seleccione objetos, y solo arranca el trazo si ningún handle de la herramienta está más cerca (`HandleUtility.nearestControl`). Mientras el botón está apretado, `EditorApplication.update` llama a `OnStrokeStep` a ritmo fijo (40 Hz) con `deltaTime` real — por eso subir/bajar/dispersar siguen actuando con el mouse quieto y la velocidad no depende de cuántos eventos de mouse lleguen.
- Undo: cada trazo es un único grupo (`Undo.IncrementCurrentGroup` al empezar, `Undo.CollapseUndoOperations` al soltar).
- `Alt` + arrastre y botón derecho no se consumen: la navegación de cámara de la Scene view sigue funcionando. `Esc` suelta la herramienta. Con una herramienta activa se ocultan los gizmos de transformación (`Tools.hidden`).

Las herramientas son clases `[Serializable]` guardadas como campos de la ventana, así sus parámetros sobreviven a los domain reloads.

Referencias: Unity Manual, "Editor windows"; Scripting API `SceneView.duringSceneGui`, `HandleUtility.AddDefaultControl`, `Handles`.
