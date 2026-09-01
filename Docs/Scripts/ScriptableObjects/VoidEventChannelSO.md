# VoidEventChannelSO

`Assets/_Scripts/ScriptableObjects/VoidEventChannelSO.cs`

Event Channel sin parámetros: un ScriptableObject con un `UnityAction OnEventRaised` y un método `RaiseEvent()`. Sirve para desacoplar el emisor de un evento de sus escuchas — ninguno de los dos necesita conocerse, ambos solo referencian el mismo asset.

Usado como `onGameOverEvent` en [GameManager](../_CORE%20Scripts/GameManager.md) y como `onDeathChannel` en [Health](../_CORE%20Scripts/Health.md).

Mismo patrón que [FloatEventChannelSO](FloatEventChannelSO.md) e [IntEventChannelSO](IntEventChannelSO.md), variando solo el tipo de payload (o la ausencia de él, en este caso).

## Fuente / patrón
ScriptableObject Event Channel, patrón de Ryan Hipple (Unity, GDC 2017 "Game Architecture with Scriptable Objects"): eventos como assets en vez de referencias directas o un EventBus estático.
