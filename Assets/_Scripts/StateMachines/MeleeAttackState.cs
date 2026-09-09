using UnityEngine;

public class MeleeAttackState : IState
{
    private UnitController unit;
    private IInteractable target;
    private float attackCooldown;
    private float targetRadius;

    public MeleeAttackState(UnitController unit, IInteractable target)
    {
        this.unit = unit;
        this.target = target;
    }

    public void Enter()
    {
        attackCooldown = 0f; // Permite atacar inmediatamente al llegar

        // El collider del target no cambia durante el estado, se cachea una sola vez en vez de por tick
        targetRadius = 0.5f;
        if (target.GetTransform().TryGetComponent<Collider>(out Collider col))
        {
            targetRadius = Mathf.Max(col.bounds.extents.x, col.bounds.extents.z);
        }
    }

    public void Tick()
    {
        // 1. Check de Falso Null (Si el objetivo murió)
        if (target == null || (target as MonoBehaviour) == null)
        {
            Debug.Log($"{unit.name}: Target destroyed. Going Idle.");
            unit.ChangeState(new UnitIdleState(unit));
            return;
        }

        // 2. Cálculo de distancias dinámicas
        float effectiveRange = unit.InteractionRange + targetRadius;
        float distanceSqr = (unit.transform.position - target.GetTransform().position).sqrMagnitude;

        // 3. Lógica de persecución y ataque
        if (distanceSqr > effectiveRange * effectiveRange)
        {
            unit.Movement.MoveTo(target.GetTransform().position);

            // ACTIVAR ANIMACIÓN: Persecución
            if (unit.UnitAnimator != null)
            {
                unit.UnitAnimator.SetBool("IsMoving", true);
            }
        }
        else
        {
            unit.Movement.Stop();

            // DESACTIVAR ANIMACIÓN: Llegó a rango
            if (unit.UnitAnimator != null)
            {
                unit.UnitAnimator.SetBool("IsMoving", false);
            }

            RotateTowards(target.GetTransform());

            if (Time.time >= attackCooldown)
            {
                ExecuteAttack();
                attackCooldown = Time.time + unit.AttackRate;
            }
        }
    }

    public void Exit()
    {
        // SEGURIDAD: Previene que la animación se trabe si se interrumpe el estado
        if (unit.UnitAnimator != null)
        {
            unit.UnitAnimator.SetBool("IsMoving", false);
        }
    }

    // Métodos auxiliares mudados desde el UnitController
    private void ExecuteAttack()
    {
        if (unit.UnitAnimator != null)
        {
            unit.UnitAnimator.SetTrigger("Attack");
        }
    }

    public void ApplyDamage()
    {
        Debug.Log("Eslabón 3: AttackState procesando el daño.");

        if (target == null || (target as MonoBehaviour) == null) return;

        if (target.GetTransform().TryGetComponent<IDamageable>(out IDamageable targetDamageable))
        {
            Debug.Log("Eslabón 4: ¡Daño enviado al enemigo con éxito!");
            DamageData payload = new DamageData
            {
                BaseDamage = unit.AttackDamage,
                Type = unit.AttackType,
                SourcePosition = unit.transform.position
            };
            targetDamageable.TakeDamage(payload);
        }
        else
        {
            target.Interact(unit);
        }
    }

    private void RotateTowards(Transform targetTransform)
    {
        Vector3 direction = (targetTransform.position - unit.transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        unit.transform.rotation = Quaternion.Slerp(unit.transform.rotation, lookRotation, Time.deltaTime * 5f);
    }
}