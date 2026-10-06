using UnityEngine;

public class WorkerMoveToDropOffState : IState
{
    private WorkerController worker;
    private IDropOffPoint targetDropOffPoint;
    private readonly IDropOffPoint explicitTarget;

    // explicitTarget != null: force-drop, orden manual de depositar en un edificio específico en vez de buscar el más cercano que acepte el recurso
    public WorkerMoveToDropOffState(WorkerController worker, IDropOffPoint explicitTarget = null)
    {
        this.worker = worker;
        this.explicitTarget = explicitTarget;
    }

    public void Enter()
    {
        targetDropOffPoint = explicitTarget ?? FindNearestDropOff(worker.currentCarriedType);
        if (targetDropOffPoint == null)
        {
            Debug.LogWarning("[Worker] No drop-off point found for resource type: " + worker.currentCarriedType);
            worker.ChangeState(new UnitIdleState(worker));
            return;
        }

        worker.Movement.MoveTo(GetApproachPoint());

        if (worker.UnitAnimator != null)
        {
            worker.UnitAnimator.SetBool("IsMoving", true);
        }
        worker.UpdateCarryAnimation();
    }

    // Punto del borde del edificio más cercano al worker: el centro de un edificio grande queda demasiado lejos del NavMesh tallado
    private Vector3 GetApproachPoint()
    {
        if (targetDropOffPoint.GetTransform().TryGetComponent<Collider>(out Collider dropOffCollider))
        {
            return dropOffCollider.ClosestPoint(worker.transform.position);
        }
        return targetDropOffPoint.Position;
    }

    public void Tick()
    {
        if (targetDropOffPoint == null) return;
        if (!worker.Movement.IsPathPending && worker.Movement.RemainingDistance <= worker.InteractionRange)
        // si ha llegado al punto de depósito, realiza la acción de depositar recursos
        {
            targetDropOffPoint.Deposit(worker.currentCarriedType, worker.currentCarriedAmount);
            worker.currentCarriedAmount = 0;

            // Force-drop (orden manual): el worker se queda ahí, no retoma la recolección por su cuenta.
            // Auto-depósito (se llenó recolectando): vuelve a buscar recursos como siempre.
            if (explicitTarget != null)
                worker.ChangeState(new UnitIdleState(worker));
            else
                worker.ChangeState(new WorkerMoveToResourceState(worker));
        }
    }

    public void Exit()
    {
        worker.Movement.Stop();

        if (worker.UnitAnimator != null)
        {
            worker.UnitAnimator.SetBool("IsMoving", false);
        }
    }

    private IDropOffPoint FindNearestDropOff(ResourceType resourceType)
    {
        if (BuildingManager.Instance == null) return null;

        return BuildingManager.Instance.GetNearestDropOff(worker.transform.position, resourceType);
    }
}

