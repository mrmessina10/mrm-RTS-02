using UnityEngine;
using UnityEngine.AI;

public class EnemyPatrolState : IState
{
    private EnemyController enemy;
    private float waitTimer;
    private bool isWaiting;

    public EnemyPatrolState(EnemyController enemy)
    {
        this.enemy = enemy;
    }

    public void Enter()
    {
        MoveToNewPatrolPoint();
    }

    public void Tick()
    {
        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                MoveToNewPatrolPoint();
            }
            return;
        }

        if (enemy.Movement.IsPathPending) return;

        if (enemy.Movement.RemainingDistance <= enemy.Movement.StoppingDistance)
        {
            enemy.Movement.Stop();

            if (enemy.UnitAnimator != null)
            {
                enemy.UnitAnimator.SetBool("IsMoving", false);
            }

            isWaiting = true;
            waitTimer = enemy.WaitTimeAtPoint;
        }
    }

    public void Exit()
    {
        if (enemy.UnitAnimator != null)
        {
            enemy.UnitAnimator.SetBool("IsMoving", false);
        }
    }

    private void MoveToNewPatrolPoint()
    {
        isWaiting = false;

        if (TryGetRandomPointInPatrolArea(out Vector3 point))
        {
            enemy.Movement.MoveTo(point);

            if (enemy.UnitAnimator != null)
            {
                enemy.UnitAnimator.SetBool("IsMoving", true);
            }
        }
        else
        {
            // No se encontró un punto válido en el NavMesh: reintenta en el próximo Tick
            waitTimer = 0.1f;
            isWaiting = true;
        }
    }

    // Fuente: patrón estándar de Unity para muestrear un punto aleatorio dentro de un radio sobre el NavMesh
    // https://docs.unity3d.com/ScriptReference/AI.NavMesh.SamplePosition.html
    private bool TryGetRandomPointInPatrolArea(out Vector3 result)
    {
        Vector3 randomPoint = enemy.PatrolCenter + Random.insideUnitSphere * enemy.PatrolRadius;

        if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, enemy.PatrolRadius, NavMesh.AllAreas))
        {
            result = hit.position;
            return true;
        }

        result = Vector3.zero;
        return false;
    }
}
