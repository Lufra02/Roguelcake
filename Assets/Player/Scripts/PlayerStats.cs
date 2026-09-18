using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Supervivencia")]
    [Min(1)] public int maxHealth = 100;
    [Min(0f)] public float healthRegenerationPerSecond = 0f;
    [Min(0)] public int gummyHealAmount = 10;

    [Header("Movimiento")]
    [Min(0f)] public float movementSpeed = 6f;

    [Header("Ataques")]
    [Tooltip("Multiplicador global aplicado a las dos cadencias. 1 = velocidad normal.")]
    [Min(0.01f)] public float attackSpeedMultiplier = 1f;
    [Tooltip("Ataques cuerpo a cuerpo por segundo antes del multiplicador de velocidad de ataque.")]
    [Min(0.01f)] public float physicalAttackRate = 2.5f;
    [Tooltip("Disparos por segundo antes del multiplicador de velocidad de ataque.")]
    [Min(0.01f)] public float shootingRate = 1.67f;
    [Min(0)] public int physicalDamage = 10;
    [Tooltip("Alcance total del golpe físico hacia delante.")]
    [Min(0.01f)] public float physicalAttackSize = 1.2f;
    [Min(0)] public int shootingDamage = 7;
    [Min(0f)] public float projectileSpeed = 12f;
    [Min(0)] public int projectileBounces = 0;

    public float PhysicalAttackCooldown => 1f / (physicalAttackRate * attackSpeedMultiplier);
    public float ShootingCooldown => 1f / (shootingRate * attackSpeedMultiplier);

    // Health System
    public void AddMaxHealth(int amount)
    {
        if (amount <= 0) return;

        maxHealth += amount;
        PlayerHealth playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth != null)
            playerHealth.SetMaximumHealth(maxHealth);
    }

    // Meele Attacks
    public void AddMaxMeeleDmg(int amount)
    {
        if (amount <= 0) return;
        physicalDamage += amount;
    }

    public void AddMeeleRange(int amount)
    {
        if (amount <= 0) return;
        physicalAttackSize += amount;
    }

    // Range Attacks
    public void AddProjectileDamage(int amount)
    {
        if(amount <= 0) return;
        shootingDamage += amount;
    }
    
    
}
