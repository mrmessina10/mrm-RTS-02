using UnityEngine;

public class WorkerMoveToResourceState : IState
{
    private WorkerController worker;

    public WorkerMoveToResourceState(WorkerController worker)
    {
        this.worker = worker;
    }

    public void Enter()
    {
        if (worker.currentResourceNode != null)
        {
            worker.Movement.MoveTo(worker.currentResourceNode.Position);
        }
    }

    public void Tick()
    {
        // Si se destruye o agota
        if (worker.currentResourceNode == null || worker.currentResourceNode.IsDepleted)
        {
            if (worker.TryFindNearbyResourceNode())
            {
                worker.Movement.MoveTo(worker.currentResourceNode.Position);
            }
            else
            {
                if (worker.currentCarriedAmount > 0)
                {
                    worker.ChangeState(new WorkerMoveToDropOffState(worker));
                }
                else
                {
                    worker.ChangeState(new UnitIdleState(worker));
                }
            }
            return;
        }

        // Comprobar si llegó al nodo
        if (!worker.Movement.IsPathPending && worker.Movement.RemainingDistance <= worker.InteractionRange)
        {
            worker.ChangeState(new WorkerHarvestResourceState(worker));
        }
    }

    public void Exit()
    {
        worker.Movement.Stop();
    }
}
