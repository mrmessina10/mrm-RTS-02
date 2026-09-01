# GameManager

`Assets/_Scripts/_CORE Scripts/GameManager.cs`

Escucha el canal [VoidEventChannelSO](../ScriptableObjects/VoidEventChannelSO.md) `onGameOverEvent` y reacciona al game over (por ahora solo loguea; falta implementar pausa/pantalla de game over/carga de menú).

No es singleton — se asume una única instancia en escena que se suscribe/desuscribe en `OnEnable`/`OnDisable`. No tiene relación directa con [GameStateManager](GameStateManager.md) todavía (son dos piezas de "estado de partida" que conviene unificar más adelante).
