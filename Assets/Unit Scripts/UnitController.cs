using UnityEngine;

[RequireComponent(typeof(UnitMovement))]
public class UnitController : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 2f;

    [Header("Combat Settings")]
    [SerializeField] private float attackRate = 1f; // Frecuencia de ataque (1f = 1 ataque por segundo) + alto es + lento
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private DamageType attackType = DamageType.Default; // Tipo de daño para futuras implementaciones de resistencias

    private float attackCooldown = 0f; // Temporizador para controlar la frecuencia de ataque

    private UnitMovement movement;
    private IInteractable currentTarget;

    private void Awake()
    {
        movement = GetComponent<UnitMovement>();

    }

    private void Update()
    {
        // Cast to MonoBehaviour to force Unity's overloaded null check 
        // which correctly detects if the underlying GameObject was destroyed.
        if (currentTarget == null || (currentTarget as MonoBehaviour) == null)
        {
            // If the target was destroyed, clear the reference and stop the unit.
            if (currentTarget != null)
            {
                currentTarget = null;
                movement.Stop();
                Debug.Log($"{name}: Target destroyed. Stopping action.");
            }
            return;
        }

        float targetRadius = 0.5f; // Radio de colisión del objetivo (ajusta según el tamaño del objetivo)

        float distanceSqr = (transform.position - currentTarget.GetTransform().position).sqrMagnitude;
        float interactionRangeSqr = interactionRange * interactionRange;
        // La distancia real de parada es: Mi Rango + Radio del Enemigo
        float effectiveRange = interactionRange + targetRadius;

        if (currentTarget.GetTransform().TryGetComponent<Collider>(out Collider col))
        {
            // evito que se solapen las unidades
            targetRadius = Mathf.Max(col.bounds.extents.x, col.bounds.extents.z);
        }

        if (distanceSqr > effectiveRange * effectiveRange)
        {
            // lejos -> moverse
            movement.MoveTo(currentTarget.GetTransform().position);
        }
        else
        {
            movement.Stop();
            RotateTowards(currentTarget.GetTransform());
        }

        if (Time.time >= attackCooldown)
        {
            // Buscamos si el objetivo tiene vida y puede recibir daño
            if (currentTarget.GetTransform().TryGetComponent<IDamageable>(out IDamageable damageableTarget))
            {
                /* --- LEGACY ---
                targetDamageable.TakeDamage(attackDamage); // Error: enviaba un int
                */

                // 1. Creamos el paquete de datos de daño con toda la información relevante
                DamageData payload = new DamageData
                {
                    BaseDamage = attackDamage,
                    Type = attackType,
                    SourcePosition = transform.position
                };

                // 2. Enviamos el paquete completo
                damageableTarget.TakeDamage(payload);
            }
            else
            {
                currentTarget.Interact(this);
            }

            attackCooldown = Time.time + attackRate;
        }
    }

    public void SetCommand(Vector3 destination)
    {
        currentTarget = null; // Limpiar el objetivo actual al recibir un comando de movimiento
        movement.MoveTo(destination);
    }

    public void SetTarget(IInteractable newTarget)
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
