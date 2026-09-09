using UnityEngine;

public class UnitIdleState : IState
{
    private UnitController unit;

    private float scanTimer = 0f;
    private const float SCAN_INTERVAL = 0.25f; // Escaneo optimizado 4 veces por segundo

    // Buffer reutilizado entre llamadas para evitar el alloc de Physics.OverlapSphere
    private static readonly Collider[] scanBuffer = new Collider[16];

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

        // Idle con recursos encima es un estado propio del worker; el resto de las unidades no cargan nada
        if (unit is WorkerController worker)
        {
            worker.UpdateCarryAnimation();
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
        int hitCount = Physics.OverlapSphereNonAlloc(unit.transform.position, unit.VisionRange, scanBuffer, unit.EnemyMask);

        if (hitCount > 0)
        {
            if (scanBuffer[0].TryGetComponent<IInteractable>(out IInteractable enemyTarget))
            {
                // ANTES: unit.ChangeState(new UnitAttackState(unit, enemyTarget));

                // AHORA: Factory Method
                unit.ChangeState(unit.GetAttackState(enemyTarget));
            }
        }
    }
}