# Headquarters

`Assets/_Scripts/Buildings/Headquarters.cs`

Componente composable que marca a un edificio como HQ del jugador. Implementa [IHeadquarters](../Interfaces/IHeadquarters.md). Hoy lo lleva únicamente el City Center.

`OnEnable`/`OnDisable` lo registran/desregistran en [BuildingManager](../_CORE%20Scripts/BuildingManager.md) (`BuildingManager.Instance.Headquarters`), que es por donde la IA de oleadas (Fase 3) va a resolver su objetivo principal. Al implementar `IInteractable`, ese valor se le puede pasar tal cual a `UnitController.GetAttackState` / `MeleeAttackState` / `RangedAttackState`, que resuelven el daño contra el `Health` del mismo GameObject.

No maneja la derrota: la destrucción la detecta [Health](../_CORE%20Scripts/Health.md), que en el prefab del City Center tiene `onDeathChannel = Channel_GameOver`. [GameManager](../_CORE%20Scripts/GameManager.md) ya escucha ese canal (hoy solo loguea; el cambio de estado a `GameOver` es Fase 6).

## Orden de componentes en el prefab

El City Center tiene dos `IInteractable` en el mismo GameObject: `DropOffBuilding` y `Headquarters`. `SelectionManager` resuelve el click derecho con `TryGetComponent<IInteractable>`, que devuelve el primero en el orden de componentes. `DropOffBuilding` tiene que ir **antes** que `Headquarters` para que el click derecho de un worker siga siendo un force-drop — [CityCenterPrefabGenerator](../Editor/CityCenterPrefabGenerator.md) los agrega en ese orden. `Type` es `InteractionType.None`: desde el lado del jugador el HQ no es un objetivo de ataque.
