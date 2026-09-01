# MeleeAttackState

`Assets/_Scripts/StateMachines/MeleeAttackState.cs`

Estado de combate cuerpo a cuerpo. En `Tick()`: si el objetivo murió (`target == null` o el `MonoBehaviour` subyacente fue destruido — chequeo de "falso null" de Unity), vuelve a `UnitIdleState`. Si no, calcula el rango efectivo (`InteractionRange` + radio del collider del objetivo) y persigue o ataca según distancia, con animaciones `IsMoving`/`Attack` sincronizadas.

El daño real no se aplica en el ataque en sí, sino en `ApplyDamage()`, invocado por [UnitController.TriggerAttackDamage](../Unit%20Scripts/UnitController.md) cuando el Animation Event de impacto se dispara (vía [AnimationEventRelay](../Unit%20Scripts/AnimationEventRelay.md)) — así el daño queda sincronizado al frame visual del golpe y no al inicio del ataque. Construye un [DamageData](../Data/DamageData.md) y lo aplica si el objetivo implementa `IDamageable`; si no, cae a `target.Interact(unit)` como fallback.

`Exit()` fuerza `IsMoving = false` para que la animación no quede trabada si el estado se interrumpe a mitad de persecución.
