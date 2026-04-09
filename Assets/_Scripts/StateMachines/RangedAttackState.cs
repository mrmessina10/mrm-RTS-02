using UnityEngine;
using UnityEngine.ProBuilder;

public class RangedAttackState : IState
{
    private UnitController unit;
    private IInteractable target;
    private float attackCooldown;

    public RangedAttackState(UnitController unit, IInteractable target)
    {
        this.unit = unit;
        this.target = target;
    }

    public void Enter()
    {
       // Debug.Log($"{unit.name} entró en RangedAttackState");
        attackCooldown = 0f;
    }

    public void Tick()
    {
        if (target == null || (target as MonoBehaviour) == null)
        {
            unit.ChangeState(new UnitIdleState(unit));
            return;
        }

        float targetRadius = 0.5f;
        if (target.GetTransform().TryGetComponent<Collider>(out Collider col))
        {
            targetRadius = Mathf.Max(col.bounds.extents.x, col.bounds.extents.z);
        }

        float effectiveRange = unit.InteractionRange + targetRadius;
        float distanceSqr = (unit.transform.position - target.GetTransform().position).sqrMagnitude;

        if (distanceSqr > effectiveRange * effectiveRange)
        {
            unit.Movement.MoveTo(target.GetTransform().position);

            if (unit.UnitAnimator != null)
                unit.UnitAnimator.SetBool("IsMoving", true);
        }
        else
        {
            unit.Movement.Stop();

            if (unit.UnitAnimator != null)
                unit.UnitAnimator.SetBool("IsMoving", false);

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
        if (unit.UnitAnimator != null)
        {
            unit.UnitAnimator.SetBool("IsMoving", false);
        }
    }

    private void ExecuteAttack()
    {
        if (unit.UnitAnimator != null)
        {
            unit.UnitAnimator.SetTrigger("Attack");
        }
    }

    // En lugar de restar vida como en MeleeAttackState, fabricamos la flecha
    public void ApplyDamage()
    {
        if (target == null || (target as MonoBehaviour) == null) return;

        if (unit.projectilePrefab != null && unit.firePoint != null)
        {
            GameObject arrowObj = Object.Instantiate(unit.projectilePrefab, unit.firePoint.transform.position, unit.firePoint.transform.rotation);

            if (arrowObj.TryGetComponent<Projectile>(out Projectile projectile))
            {
                projectile.Initialize(target.GetTransform(), unit.AttackDamage, unit.AttackType);
            }
        }
        else
        {
            Debug.LogError($"[{unit.name}] RangedAttackState no puede disparar. Falta asignar el ProjectilePrefab o el FirePoint en el UnitController.");
        }
    }

    private void RotateTowards(Transform targetTransform)
    {
        Vector3 direction = (targetTransform.position - unit.transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        unit.transform.rotation = Quaternion.Slerp(unit.transform.rotation, lookRotation, Time.deltaTime * 5f);
    }
}