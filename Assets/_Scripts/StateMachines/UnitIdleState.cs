using UnityEngine;

public class UnitIdleState : IState
{
    private UnitController unit;

    private float scanTimer = 0f;
    private const float SCAN_INTERVAL = 0.25f; // Escaneo optimizado 4 veces por segundo

    public UnitIdleState(UnitController unit)
    {
        this.unit = unit;
    }

    public void Enter()
    {
        unit.Movement.Stop();
        scanTimer = 0f;

        if (unit.UnitAnimator != null)
        {
            unit.UnitAnimator.SetBool("IsMoving", false);
        }
    }

    public void Tick()
    {
        scanTimer += Time.deltaTime;

        if (scanTimer >= SCAN_INTERVAL)
        {
            scanTimer = 0f;
            ScanForEnemies();
        }
    }

    public void Exit()
    {
    }

    private void ScanForEnemies()
    {
        Collider[] colliders = Physics.OverlapSphere(unit.transform.position, unit.VisionRange, unit.EnemyMask);

        if (colliders.Length > 0)
        {
            if (colliders[0].TryGetComponent<IInteractable>(out IInteractable enemyTarget))
            {
                // ANTES: unit.ChangeState(new UnitAttackState(unit, enemyTarget));

                // AHORA: Factory Method
                unit.ChangeState(unit.GetAttackState(enemyTarget));
            }
        }
    }
}