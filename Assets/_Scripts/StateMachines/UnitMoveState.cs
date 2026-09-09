using UnityEngine;

public class UnitMoveState : IState
{
    private UnitController unit;
    private Vector3 destination;
    private float stuckTimer = 0f;
    private const float STUCK_THRESHOLD = 0.25f; // Segundos de tolerancia de atasco

    public UnitMoveState(UnitController unit, Vector3 destination)
    {
        this.unit = unit;
        this.destination = destination;
    }

    public void Enter()
    {
        unit.Movement.MoveTo(destination);
        if (unit.UnitAnimator != null)
        {
            unit.UnitAnimator.SetBool("IsMoving", true);
        }

        // Caminata con recursos encima: solo aplica a workers, el resto de las unidades no cargan nada
        if (unit is WorkerController worker)
        {
            worker.UpdateCarryAnimation();
        }
    }

    public void Tick()
    {

        // 1. Si el agente aún está calculando la ruta, esperamos.
        if (unit.Movement.IsPathPending) return;

        // 2. Llegada limpia: Alcanzó su destino asignado por el NavMesh.
        if (unit.Movement.RemainingDistance <= unit.Movement.StoppingDistance)
        {
            unit.ChangeState(new UnitIdleState(unit));
            return;
        }

        // 3. Sistema Anti-Crowding (Detector de atascos)
        // Si la unidad no avanza (velocidad casi nula) pero no ha llegado a destino,
        // significa que está chocando con sus compañeros.
        if (unit.Movement.VelocitySqr < 0.05f)
        {
            stuckTimer += Time.deltaTime;

            // Si está atascada por más del tiempo permitido, aborta el movimiento.
            if (stuckTimer >= STUCK_THRESHOLD)
            {
                unit.ChangeState(new UnitIdleState(unit));
            }
        }
        else
        {
            stuckTimer = 0f; // Si vuelve a moverse libremente, reiniciamos el contador
        }

        //// Calculamos si ya llegamos al destino para volver a Idle
        //// Usamos sqrMagnitude por rendimiento en lugar de Vector3.Distance
        //float distanceSqr = (unit.transform.position - destination).sqrMagnitude;
        //if (distanceSqr <= 1f) // Umbral de parada
        //{
        //    unit.ChangeState(new UnitIdleState(unit));
        //}
    }

    public void Exit()
    {
        if (unit.UnitAnimator != null)
        {
            unit.UnitAnimator.SetBool("IsMoving", false);
        }
    }
}
