using System;
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
    [Header("ATAQUE CAC")]
    [Tooltip("Ataques cuerpo a cuerpo por segundo antes del multiplicador de velocidad de ataque.")]
    [Min(0.01f)] public float physicalAttackRate = 2.5f;
    [Min(0)] public int physicalDamage = 10;
    [Tooltip("Alcance total del golpe físico hacia delante.")]
    [Min(0.01f)] public float physicalAttackSize = 1.2f;

    [Tooltip("Tope máximo de ataques cuerpo a cuerpo por segundo, para que no supere la animación.")] [Min(0.01f)]
    public float physicalAttackRateCap = 6f;
    
    [Header("ATAQUE DISPARO")]
    [Tooltip("Disparos por segundo antes del multiplicador de velocidad de ataque.")]
    [Min(0.01f)] public float shootingRate = 1.67f;
    [Min(0)] public int shootingDamage = 7;
    [Min(0f)] public float projectileSpeed = 12f;
    [Min(0)] public int projectileBounces = 0;
    [Tooltip("Tope máximo de disparos por segundo, para que no supere la animación/sonido.")]
    [Min(0.01f)] public float shootingRateCap = 4f;
    [Min(10)] public int maxTravelDistance = 10;

    public float PhysicalAttackCooldown => 1f / (physicalAttackRate * attackSpeedMultiplier);
    public float ShootingCooldown => 1f / (shootingRate * attackSpeedMultiplier);

    
    PlayerManager playerManager;
    private void Start()
    {
        playerManager = PlayerManager.Instance;
    }

    // Health System
    public void AddMaxHealth(int amount)
    {
        if (amount <= 0) return;

        maxHealth += amount;
        playerManager.playerHealth.SetMaximumHealth(maxHealth);
    }
    public void AddHealthRegen(int amount)
    {
        if (amount <= 0) return;
        healthRegenerationPerSecond += amount;
    }
    public void Heal(int amount)
    {
        if (amount <= 0) return;
        playerManager.playerHealth.RestoreHealth(amount);
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
    public void AddPhysicalAttackRate(float amount)
    {
        if (amount <= 0) return;
        physicalAttackRate = Mathf.Min(physicalAttackRate + amount, physicalAttackRateCap);
    }

    // Range Attacks
    public void AddProjectileDamage(int amount)
    {
        if(amount <= 0) return;
        shootingDamage += amount;
    }
    
    public void AddShootingRate(float amount)
    {
        if (amount <= 0) return;
        shootingRate = Mathf.Min(shootingRate + amount, shootingRateCap);
    }
    
}
