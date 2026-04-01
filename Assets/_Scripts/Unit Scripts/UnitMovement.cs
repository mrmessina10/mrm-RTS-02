using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class UnitMovement : MonoBehaviour
{
    private NavMeshAgent agent;

    public bool IsPathPending => agent.pathPending;
    public float RemainingDistance => agent.remainingDistance;
    public float StoppingDistance => agent.stoppingDistance;
    public float VelocitySqr => agent.velocity.sqrMagnitude;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Método que llama el UnitMoveState
    public void MoveTo(Vector3 destination)
    {
        agent.isStopped = false;
        agent.SetDestination(destination);
    }

    // Método que llama el UnitIdleState o cuando llega a rango de ataque
    public void Stop()
    {
        if (agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }
}
