using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{

    [Header("Nivel")] 
    public int currentLevel;
    public int currentXP;
    public int requiredXP;
    
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] TextMeshProUGUI currentExpText;
    [SerializeField] Slider expSlider; 
    
    [Tooltip("XP necesaria para pasar del nivel 1 al 2. Base de la curva de progresión.")]
    [SerializeField, Min(1)] private int baseXPRequirement = 100;
    [Tooltip("Cuánto crece la XP requerida por cada nivel. 1.15 = 15% más por nivel que el anterior.")]
    [SerializeField, Min(1f)] private float xpGrowthRate = 1.15f;
    
    [Tooltip("La misma tienda que usas para caminar y comprar. Se abre sola al subir de nivel.")]
    [SerializeField] private ShopInteractable levelUpShop;
    
    [Header("Supervivencia")]
    [Min(1)] public int maxHealth = 100;
    [Min(0f)] public float secondsToRegenerateHealth = 0f;
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
    [Min(0)] public int shootingCost = 1;
    [Min(0f)] public float projectileSpeed = 12f;
    
    [Tooltip("Tope máximo de disparos por segundo, para que no supere la animación/sonido.")]
    [Min(0.01f)] public float shootingRateCap = 4f;
    [Min(10)] public int maxTravelDistance = 10;
    
    [Header("Mejoras")] 
    public bool bouncyGun;
    public bool explosiveGun;
    public bool hugeGun;
    
    [Header("ARMA DE REBOTE")]
    [Min(0)] public int projectileBounces = 0;

    [Header("ARMA EXPLOSIVA")] 
    public int explosiveDamage;
    public int explosiveRange = 10;
    
    [Header("ARMA GIGANTE")] 
    public float hugeDamagePercent;
    
    public float PhysicalAttackCooldown => 1f / (physicalAttackRate * attackSpeedMultiplier);
    public float ShootingCooldown => 1f / (shootingRate * attackSpeedMultiplier);

    
    PlayerManager playerManager;
    private void Start()
    {
        playerManager = PlayerManager.Instance;
        currentLevel = 1;
        currentXP = 0;
        requiredXP = CalculateRequiredXP(currentLevel);
        RefreshExpUI();
        levelText.text = "Nivel " + currentLevel;
    }

    // Progresión de nivel
    public void AddXP(int amount)
    {
        if (amount <= 0) return;
 
        currentXP += amount;
        
        // Implementacion en UI
        RefreshExpUI();

        if (currentXP >= requiredXP)
        {
            LevelUp();
        }
    }
 
    private void LevelUp()
    {
        currentLevel++;
        currentXP = 0;
        requiredXP = CalculateRequiredXP(currentLevel);
        
        // INTEGRAR LA TIENDA DE EXP
        // Abre la tienda para elegir una mejora; se cierra sola en cuanto se compre una carta.
        levelUpShop?.OpenForLevelUp(gameObject);
        
        // Implementacion en UI
        RefreshExpUI();
        levelText.text = "Nivel " + currentLevel;   
    }
 
    // XP necesaria para pasar del nivel actual al siguiente. Crece de forma exponencial,
    // así que nunca queda "fija": cada nivel exige más que el anterior.
    private int CalculateRequiredXP(int level)
    {
        return Mathf.RoundToInt(baseXPRequirement * Mathf.Pow(xpGrowthRate, level - 1));
    }

    public void RefreshExpUI()
    {
        currentExpText.text = currentXP + " / " + requiredXP;
        expSlider.value = currentXP;
        expSlider.maxValue = requiredXP;
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
        //healthRegenerationPerSecond += amount;
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

    public void ChangeTypeOfBullet(bool isRebound, bool isExplosive, bool isHugeGun)
    {
        bouncyGun = isRebound;
        explosiveGun = isExplosive;
        hugeGun = isHugeGun;
    }
    
}
