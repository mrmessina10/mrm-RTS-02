using UnityEngine;

public class WorkerMoveToDropOffState : IState
{
    private WorkerController worker;
    private IDropOffPoint targetDropOffPoint;

    public WorkerMoveToDropOffState(WorkerController worker)
    {
        this.worker = worker;
    }

    public void Enter()
    {
        targetDropOffPoint = FindNearestDropOff(worker.currentCarriedType);
        if (targetDropOffPoint == null)
        {
            Debug.LogWarning("[Worker] No drop-off point found for resource type: " + worker.currentCarriedType);
            worker.ChangeState(new UnitIdleState(worker));
            return;
        }

        worker.Movement.MoveTo(targetDropOffPoint.Position);
    }

    public void Tick()
    {
        if (targetDropOffPoint == null) return;
        if (!worker.Movement.IsPathPending && worker.Movement.RemainingDistance <= worker.InteractionRange)
        // si ha llegado al punto de depósito, realiza la acción de depositar recursos
        {
            targetDropOffPoint.Deposit(worker.currentCarriedType, worker.currentCarriedAmount);
            worker.currentCarriedAmount = 0;

            //regresa a buscar recursos después de depositar
            worker.ChangeState(new WorkerMoveToResourceState(worker));
        }
    }

    public void Exit()
    {
        worker.Movement.Stop();
    }

    private IDropOffPoint FindNearestDropOff(ResourceType resourceType)
    {
        if (BuildingManager.Instance == null) return null;

        return BuildingManager.Instance.GetNearestDropOff(worker.transform.position, resourceType);
    }
}

