using UnityEngine;

public class WorkerBuildState : IState
{
    private WorkerController worker;
    private IConstructable site;

    public WorkerBuildState(WorkerController worker, IConstructable site)
    {
        this.worker = worker;
        this.site = site;
    }

    public void Enter()
    {
        worker.Movement.MoveTo(site.Position);

        if (worker.UnitAnimator != null)
        {
            worker.UnitAnimator.SetBool("IsMoving", true);
        }
        worker.UpdateCarryAnimation();
    }

    public void Tick()
    {
        if (site == null || (site as MonoBehaviour) == null)
        {
            worker.ChangeState(new UnitIdleState(worker));
            return;
        }

        if (!worker.Movement.IsPathPending && worker.Movement.RemainingDistance <= worker.InteractionRange)
        {
            worker.Movement.Stop();

            if (worker.UnitAnimator != null)
            {
                worker.UnitAnimator.SetBool("IsMoving", false);
                worker.UnitAnimator.SetBool("IsBuilding", true);
            }

            site.AddBuildProgress(Time.deltaTime);

            if (site.IsComplete)
            {
                Debug.Log($"[WorkerBuildState] {worker.name} terminó de construir.");
                worker.ChangeState(new UnitIdleState(worker));
            }
        }
    }

    public void Exit()
    {
        worker.Movement.Stop();

        // Reset explícito de los dos flags que puede haber dejado prendidos este estado (acercándose o construyendo)
        if (worker.UnitAnimator != null)
        {
            worker.UnitAnimator.SetBool("IsMoving", false);
            worker.UnitAnimator.SetBool("IsBuilding", false);
        }
    }
}
