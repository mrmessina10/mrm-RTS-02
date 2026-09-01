# RangedAttackState

`Assets/_Scripts/StateMachines/RangedAttackState.cs`

Contraparte a distancia de [MeleeAttackState](MeleeAttackState.md) — misma estructura de persecución/rango/cooldown/animación. Se diferencia solo en `ApplyDamage()`: en vez de aplicar daño directo, instancia `unit.projectilePrefab` en `unit.firePoint` e inicializa un [Projectile](../Unit%20Scripts/Projectile.md) con el objetivo, daño y tipo — el daño real ocurre después, cuando el proyectil impacta en vuelo.

Si falta `projectilePrefab` o `firePoint` en el `UnitController`, loguea error en vez de fallar silenciosamente.

Nota: hay duplicación considerable de lógica (persecución, rotación, cooldown) entre este estado y `MeleeAttackState` — candidato a extraer una clase base común si se agregan más tipos de ataque.
