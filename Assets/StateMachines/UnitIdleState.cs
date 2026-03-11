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
        // Debug 1: ¿Está escaneando y con qué valores?
        // Descomenta la siguiente línea solo si dudas de que el Tick se esté ejecutando.
        // Debug.Log($"{unit.name} escaneando en radio {unit.VisionRange} buscando la máscara {unit.EnemyMask.value}");

        Collider[] colliders = Physics.OverlapSphere(unit.transform.position, unit.VisionRange, unit.EnemyMask);

        if (colliders.Length > 0)
        {
            // Debug 2: Encontramos algo físicamente. ¿Qué es?
            Debug.Log($"¡{unit.name} tocó {colliders.Length} objetos físicos! El primero es: {colliders[0].name}");

            if (colliders[0].TryGetComponent<IInteractable>(out IInteractable enemyTarget))
            {
                // Debug 3: El objeto tiene el script correcto.
                Debug.Log($"{unit.name} detectó a {colliders[0].name} como IInteractable. Cambiando a AttackState.");
                unit.ChangeState(new UnitAttackState(unit, enemyTarget));
            }
            else
            {
                // Debug 4: El objeto físico NO tiene el script.
                Debug.LogWarning($"ATENCIÓN: {unit.name} tocó a {colliders[0].name}, pero ese objeto NO tiene un componente IInteractable.");
            }
        }
    }
}