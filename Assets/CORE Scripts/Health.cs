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

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
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

}
