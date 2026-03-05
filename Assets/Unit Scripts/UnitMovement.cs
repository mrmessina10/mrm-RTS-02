using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class UnitMovement : MonoBehaviour
{
    private NavMeshAgent agent;
    private Camera mainCamera;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void MoveTo(Vector3 destination)
    {
        //Cancelo cualquier ruta anterior
        agent.isStopped = false;
        agent.SetDestination(destination);

        Debug.Log($"{name} se mueve a {destination}");
    }

    public void Stop()
    {
        agent.isStopped = true;
    }
}
