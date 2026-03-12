using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float speed = 15f;

    private Transform target;
    private int damage;
    private DamageType type;
    private bool isInitialized = false;

    // Método inyectado por el RangedAttackState al nacer
    public void Initialize(Transform target, int damage, DamageType type)
    {
        this.target = target;
        this.damage = damage;
        this.type = type;
        isInitialized = true;
    }

    private void Update()
    {
        if (!isInitialized) return;

        // Si el enemigo muere mientras la flecha vuela, se destruye (por ahora)
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // Moverse hacia el objetivo
        Vector3 direction = target.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;

        // Detección de impacto matemática (más eficiente que colisionadores físicos)
        if (direction.magnitude <= distanceThisFrame)
        {
            HitTarget();
            return;
        }

        transform.Translate(direction.normalized * distanceThisFrame, Space.World);

        // Rota el modelo visual para que "apunte" al objetivo
        transform.LookAt(target);
    }

    private void HitTarget()
    {
        if (target.TryGetComponent<IDamageable>(out IDamageable targetDamageable))
        {
            DamageData payload = new DamageData
            {
                BaseDamage = damage,
                Type = type,
                SourcePosition = transform.position
            };
            targetDamageable.TakeDamage(payload);
        }

        Destroy(gameObject); // La flecha desaparece al impactar
    }
}