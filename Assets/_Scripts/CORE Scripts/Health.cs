using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour, IDamageable
{
    [Header("Configuration")]
    [SerializeField] private int maxHealth = 100;

    [Header("Broadcast events")]
    [SerializeField] private VoidEventChannelSO onDeathChannel;

    private int currentHealth;

    public event UnityAction<float> onHealthChanged;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    //public void TakeDamage(int damage)
    //{
    //    currentHealth -= damage;
    //    currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    //    onHealthChanged?.Invoke((float)currentHealth / maxHealth);
    //    if (currentHealth <= 0)
    //    {
    //        Die();
    //    }
    //}


    public void TakeDamage(DamageData damageData)
    {
        int damage = damageData.BaseDamage;

        //Groundwork para futuros cálculos de daño basado en tipo, resistencias, etc.

        damage = Mathf.Max(damage, 1); // Asegura que el daño no sea menor a 1

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        Debug.Log($"{name} recibió {damage} de daño {damageData.Type}. Vida actual: {currentHealth}/{maxHealth}");

        onHealthChanged?.Invoke((float)currentHealth / maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (onDeathChannel != null)
            onDeathChannel.RaiseEvent();

        Destroy(gameObject);
    }

    //[ContextMenu("Test: Recibir 10 de Daño")]
    //public void TestDamage10() // prueba de daño de 10 puntos
    //{
    //    TakeDamage(10);
    //    Debug.Log($"Prueba: Daño recibido. Vida actual: {currentHealth}");
    //}

    //[ContextMenu("Test: Matar Unidad")]
    //public void TestKill()
    //{
    //    TakeDamage(maxHealth); // prueba de daño suficiente para matar a la unidad
    //    Debug.Log("Prueba: Muerte forzada ejecutada.");
    //}

}
