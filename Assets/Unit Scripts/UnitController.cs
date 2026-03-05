using UnityEngine;

[RequireComponent(typeof(UnitMovement))]
public class UnitController : MonoBehaviour
{
    [SerializeField] private float interactionRange = 2f;

    private UnitMovement movement;
    private IInteractable currentTarget;

    private void Awake()
    {
        movement = GetComponent<UnitMovement>();

    }

    private void Update()
    {
        if (currentTarget != null)
        {
            float distanceSqr = (transform.position - currentTarget.GetTransform().position).sqrMagnitude;
            float interactionRangeSqr = interactionRange * interactionRange;

            if (distanceSqr > interactionRangeSqr)
            // Si la distancia al objetivo es mayor que el rango de interacción, mueve hacia el objetivo
            {
                movement.MoveTo(currentTarget.GetTransform().position);
            }
            else
            // Si la unidad está dentro del rango de interacción, realiza la interacción
            {
                movement.Stop();

                RotateTowards(currentTarget.GetTransform());
                // Ejecutar la interacción (Atacar, Recolectar, etc.)
                // OJO: Esto se llamará en cada frame mientras esté en rango.
                // Más adelante usaremos un Timer (Cooldown) para no atacar 60 veces por segundo.
                // currentTarget.Interact(this); 
                Debug.Log($"Interactuando con {currentTarget.GetTransform().name} (En rango)");
            }
        }
    }

    public void SetCommand (Vector3 destination)
    {
        currentTarget = null; // Limpiar el objetivo actual al recibir un comando de movimiento
        movement.MoveTo(destination);
    }

    public void SetTarget (IInteractable newTarget)
    {
        currentTarget = newTarget; 
    }

    private void RotateTowards(Transform target)
    {
        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0; // Mantener la rotación solo en el plano horizontal
        if (direction.sqrMagnitude > 0.01f) // Evitar rotar si la dirección es muy pequeña
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f); // Ajusta la velocidad de rotación según sea necesario
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Dibuja un círculo para visualizar el rango de interacción en la escena
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);

    }
}
