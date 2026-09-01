# Health

`Assets/_Scripts/_CORE Scripts/Health.cs`

Implementa [IDamageable](../Interfaces/IDamageable.md). Componente de vida genérico: guarda `currentHealth`, aplica el daño recibido en `TakeDamage(DamageData)` (clampeado a mínimo 1 de daño real y a 0 de vida mínima), dispara `onHealthChanged` (para barras de vida u otra UI) y, al llegar a 0, llama `Die()`.

`Die()` levanta el canal `onDeathChannel` ([VoidEventChannelSO](../ScriptableObjects/VoidEventChannelSO.md)) antes de destruir el `gameObject` — así otros sistemas pueden reaccionar a la muerte sin acoplarse directamente a `Health`.

Queda comentado en el código un método viejo `TakeDamage(int)` sin `DamageType`, reemplazado por la versión con `DamageData`.
