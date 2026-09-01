# IDamageable

`Assets/_Scripts/Interfaces/IDamageable.cs`

Contrato mínimo para cualquier objeto que pueda recibir daño: un único método `TakeDamage(DamageData damageData)`.

Implementado por [Health](../_CORE%20Scripts/Health.md). Consumido por [MeleeAttackState](../StateMachines/MeleeAttackState.md) y [Projectile](../Unit%20Scripts/Projectile.md) (impacto de proyectiles a distancia), que hacen `TryGetComponent<IDamageable>` sobre el objetivo antes de aplicar daño.

Ver [DamageData](../Data/DamageData.md) para el payload que viaja en cada llamada.
