# MapEditorTool / MapEditorContext

`Assets/_Scripts/Editor/MapEditor/MapEditorTool.cs`

`MapEditorTool` es la clase base abstracta de cada herramienta del editor de mapas. [MapEditorWindow](MapEditorWindow.md) resuelve el input común y la herramienta solo sobreescribe lo que necesita:

| Miembro | Para qué |
|---|---|
| `DisplayName`, `Hint` | Etiqueta en la grilla y texto de ayuda. |
| `GetBrushRadius`, `GetBrushColor` | Cursor de pincel (radio 0 = sin cursor). |
| `OnToolGUI` | Parámetros de la herramienta dentro de la ventana. |
| `OnSceneGUI` | Handles y dibujo propio sobre la Scene view. |
| `OnStrokeBegin` / `OnStrokeStep(deltaTime)` / `OnStrokeEnd` | Click, pasos a ritmo fijo mientras se arrastra, soltar. |
| `OnActivated` / `OnDeactivated` / `OnUndoRedo` | Ciclo de vida (crear/liberar recursos, invalidar caches). |

Mismo criterio que la FSM de unidades (`IState` con `Enter/Tick/Exit`): una herramienta nueva es una clase nueva, no una rama dentro de la ventana. Para sumarla hay que agregarla como campo serializado en `MapEditorWindow` y al array `tools`.

`MapEditorContext` es el estado que la ventana le pasa a la herramienta: `MapRoot`, `Terrain`, `MapDataSO`, paleta, punto de impacto del mouse sobre el terreno (`HasHit`/`HitPoint`) y modificadores (`Shift`, `Control`). `SnapToTerrain` apoya un punto sobre el relieve.
