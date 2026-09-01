# AnimationEventRelay

`Assets/_Scripts/Unit Scripts/AnimationEventRelay.cs`

Puente entre un Animation Event de Unity (que solo puede llamar métodos en el mismo GameObject que el Animator) y la lógica de gameplay que vive en el padre. Busca `UnitController` en el padre (`GetComponentInParent`) y, cuando la animación de ataque llega al frame de impacto, llama `unitController.TriggerAttackDamage()`.

Existe porque el Animator suele estar en un GameObject hijo (modelo visual) separado del GameObject raíz donde vive `UnitController` — sin este relay, el evento de animación no tendría a quién avisarle.
