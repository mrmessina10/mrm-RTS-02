# Projectile

`Assets/_Scripts/Unit Scripts/Projectile.cs`

Comportamiento de un proyectil disparado por una unidad ranged. Se inicializa vía `Initialize(target, damage, type)` desde [RangedAttackState.ApplyDamage](../StateMachines/RangedAttackState.md), que lo instancia y lo empuja hacia el objetivo.

En `Update()` se mueve en línea recta hacia `target` y rota para "apuntarlo" (`transform.LookAt`). La detección de impacto es matemática (compara `direction.magnitude` contra la distancia que recorrería ese frame) en vez de usar colliders físicos — más barato y determinístico para proyectiles rápidos. Si el objetivo se destruye en vuelo, el proyectil se autodestruye sin aplicar daño.

Al impactar (`HitTarget`), arma un [DamageData](../Data/DamageData.md) y lo aplica vía [IDamageable](../Interfaces/IDamageable.md) si el objetivo lo implementa.
