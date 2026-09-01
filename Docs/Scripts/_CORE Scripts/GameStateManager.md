# GameStateManager

`Assets/_Scripts/_CORE Scripts/GameStateManager.cs`

Singleton (`Instance`) que trackea el estado global de la partida (`Initializing`, `Playing`, `Pause`, `GameOver`) y expone un evento `OnGameStateChanged` para que otros sistemas reaccionen a los cambios.

En `Start()` pasa automáticamente de `Initializing` a `Playing` (no hay lógica de carga real todavía, es un placeholder). Ningún otro script se suscribe a `OnGameStateChanged` por ahora.

## Fuente / patrón
Singleton clásico de Unity (`Instance` estático con guarda en `Awake`) + evento C# para notificar cambios de estado (patrón Observer). Mismo patrón que [BuildingManager](BuildingManager.md), [ResourceManager](ResourceManager.md) y [GlobalUnitManager](GlobalUnitManager.md).
