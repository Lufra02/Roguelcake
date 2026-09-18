using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{

    [SerializeField, Min(1)] private int maxHealth = 100;
    [SerializeField] private int currentHealth;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public virtual void TakeDamage(int dmg) { 
        currentHealth = Mathf.Clamp(currentHealth - dmg, 0, maxHealth);
    }

    public virtual void Heal(int amount)
    {
        if (amount <= 0) return;
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
    }

    protected void SetMaxHealth(int value)
    {
        maxHealth = Mathf.Max(1, value);
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }

    public virtual void Die() { }
}

public enum Owner
{
    PLAYER,
    ENEMY
}
