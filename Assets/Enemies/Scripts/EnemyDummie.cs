using UnityEngine;

// Enemigo mínimo solo para probar que el melee y el proyectil funcionan.
// Colócalo en un GameObject con Collider 3D (no trigger) en la capa "Enemy".
public class EnemyDummie : MonoBehaviour, IDamageable
{
    public float maxHealth = 30f;
    private float currentHealth;
 
    void Awake()
    {
        currentHealth = maxHealth;
    }
 
    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log($"{name} recibió {amount} de daño. Vida restante: {currentHealth}");
 
        if (currentHealth <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
