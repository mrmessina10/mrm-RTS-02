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

        if (worker.UnitAnimator != null)
        {
            worker.UnitAnimator.SetBool("IsHarvestingWood", worker.currentCarriedType == ResourceType.Wood);
            worker.UnitAnimator.SetBool("IsHarvestingFood", worker.currentCarriedType == ResourceType.Food);
        }
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

            // Log temporal: reemplaza feedback visual/UI que todavía no existe (ni HUD de carga ni animación de recolección)
            Debug.Log($"[WorkerHarvestResourceState] {worker.name} +{harvested} {worker.currentCarriedType}. Carga: {worker.currentCarriedAmount}/{worker.MaxCarryCapacity}");

            if(worker.currentCarriedAmount >= worker.MaxCarryCapacity)
            {
                worker.ChangeState(new WorkerMoveToDropOffState(worker));
            }

        }

    }

    public void Exit()
    {
        // Reset explícito: si se interrumpe el estado con otra orden, evita que la animación de cosecha quede trabada
        if (worker.UnitAnimator != null)
        {
            worker.UnitAnimator.SetBool("IsHarvestingWood", false);
            worker.UnitAnimator.SetBool("IsHarvestingFood", false);
        }
    }
}