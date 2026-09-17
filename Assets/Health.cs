using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{

    public int maxHealth;
    public int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public virtual void TakeDamage(int dmg) { 
       
        currentHealth -= dmg;
    }

    public virtual void Die() { }
}

public enum Owner
{
    PLAYER,
    ENEMY
}
