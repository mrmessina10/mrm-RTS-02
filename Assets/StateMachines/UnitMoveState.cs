using UnityEngine;

public class UnitMoveState : IState
{
    private UnitController unit;
    private Vector3 destination;

    public UnitMoveState(UnitController unit, Vector3 destination)
    {
        this.unit = unit;
        this.destination = destination;
    }

    public void Enter()
    {
        unit.Movement.MoveTo(destination);
        // A futuro: unit.PlayAnimation("Run");
    }

    public void Tick()
    {
        // Calculamos si ya llegamos al destino para volver a Idle
        // Usamos sqrMagnitude por rendimiento en lugar de Vector3.Distance
        float distanceSqr = (unit.transform.position - destination).sqrMagnitude;
        if (distanceSqr <= 1f) // Umbral de parada
        {
            unit.ChangeState(new UnitIdleState(unit));
        }
    }

    public void Exit()
    {
    }
}
