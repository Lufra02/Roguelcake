using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{

    private int maxHealth = 100;
    private float currentHealth;

    public int MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public virtual void TakeDamage(float dmg) { 
        currentHealth -= dmg;
    }

    public void SetHealth(int health)
    {
        currentHealth = health;
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
