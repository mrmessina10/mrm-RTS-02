# EnemyUnit

`Assets/_Scripts/Unit Scripts/EnemyUnit.cs`

Implementación mínima de [IInteractable](../Interfaces/IInteractable.md) (`Type = InteractionType.Attack`) para marcar un objeto como objetivo de ataque. `Interact()` solo loguea — el daño real no pasa por acá, pasa por [IDamageable](../Interfaces/IDamageable.md)/[Health](../_CORE%20Scripts/Health.md) directamente desde [MeleeAttackState](../StateMachines/MeleeAttackState.md)/[Projectile](Projectile.md). `Interact()` queda como fallback para objetivos sin `Health` (ver `MeleeAttackState.ApplyDamage`).

En el GameObject enemigo convive con [EnemyController](EnemyController.md) (movimiento/patrullaje) y `Health` (daño) — `EnemyUnit` sigue siendo solo el marcador de la interfaz, no absorbe esas responsabilidades.
