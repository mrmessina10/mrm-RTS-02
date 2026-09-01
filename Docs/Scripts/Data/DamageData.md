# DamageData

`Assets/_Scripts/Data/DamageData.cs`

Struct que empaqueta la información de un evento de daño: `BaseDamage`, `Type` (enum `DamageType`: Default, Melee, Piercing, Siege) y `SourcePosition` (para futuros cálculos direccionales o de daño por elevación, todavía no usados).

Es el payload que viaja en [IDamageable.TakeDamage](../Interfaces/IDamageable.md). Lo construyen [MeleeAttackState.ApplyDamage](../StateMachines/MeleeAttackState.md) y [Projectile.HitTarget](../Unit%20Scripts/Projectile.md) antes de invocar `TakeDamage` sobre el objetivo.

`DamageType` vive en este mismo archivo; por ahora [Health.TakeDamage](../_CORE%20Scripts/Health.md) no diferencia lógica según el tipo (queda como groundwork para resistencias futuras).
