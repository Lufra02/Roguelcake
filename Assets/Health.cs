using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{

    public int maxHealth;
    public int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int dmg) { 
       
        currentHealth -= dmg;
        if (currentHealth <= 0) {
            // TODO Die
        }
    }

    public void Die() 
    {
        // Desaparecer el pj - tal vez

        // No permitir la pausa

        // Dejar un tiempo para generar una pequeña anim

        // Mostrar HUD "has muerto"
        

    }


}

public enum Owner
{
    PLAYER,
    ENEMY
}
