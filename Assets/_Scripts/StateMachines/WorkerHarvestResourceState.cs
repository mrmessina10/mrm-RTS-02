using UnityEngine;

public class WorkerHarvestResourceState : IState
{
    private WorkerController worker;
    private float harvestTimer;

    public WorkerHarvestResourceState(WorkerController worker)
    {
        this.worker = worker;
    }
    public void Enter()
    {
        worker.Movement.Stop();
        harvestTimer = 0f;
    }

    public void Tick()
    {
        if (worker.currentResourceNode == null || worker.currentResourceNode.IsDepleted)
        {
            worker.ChangeState(new WorkerMoveToResourceState(worker));
            return;
        }

        harvestTimer += Time.deltaTime;

        if (harvestTimer >= worker.HarvestRate)
        {
            harvestTimer = 0f;
            int harvested = worker.currentResourceNode.Harvest(worker.HarvestAmountPerCycle);
            worker.currentCarriedAmount += harvested;

            if(worker.currentCarriedAmount >= worker.MaxCarryCapacity)
            {
                worker.ChangeState(new WorkerMoveToDropOffState(worker));
            }

        }

    }

    public void Exit()
    {
        Debug.Log("[WorkerHarvestResourceState] Exiting harvest state.");
    }
}