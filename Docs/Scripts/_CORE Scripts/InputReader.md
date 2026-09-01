# InputReader

`Assets/_Scripts/_CORE Scripts/InputReader.cs`

ScriptableObject que centraliza todo el input crudo del jugador (New Input System, `GameInput.IPlayerActions`) y lo re-expone como eventos C# (`UnityAction`/`Action`) desacoplados: movimiento y rotación de cámara, zoom, posición del puntero, selección, comando (click derecho), pausa, teclas numéricas para grupos de control, y flags de modificadores (`IsCtrlHeld`, `IsShiftHeld`, `IsLeftClickHeld`).

Al ser un ScriptableObject (asset, no un componente de escena) cualquier script puede suscribirse a sus eventos sin necesitar una referencia a un GameObject puntual — es la capa de desacople entre el Input System y la lógica de gameplay. Consumido por [Player](../RTSCamera/Scripts/Player.md) (cámara) y [SelectionManager](SelectionManager.md) (selección y comandos).

Grupos de control: `OnNumberKey` usa Shift para asignar grupo (modo testeo en editor) en vez de Ctrl, para no chocar con atajos nativos del editor de Unity; hay un bloque comentado con la versión "modo build final" que usa Ctrl, pendiente de activar antes de un build.

## Fuente / patrón
Event Channel / ScriptableObject-based input, patrón recomendado por Unity para desacoplar el Input System de la lógica de gameplay. Referencia: [Unity Learn – ScriptableObject Architecture (Ryan Hipple)](https://www.youtube.com/watch?v=raQ3iHhE_Kk).
