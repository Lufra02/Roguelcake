using UnityEngine;

public enum ShopCardEffect
{
    // HEALTH SYSTEM
    MaxHealth,
    HealthRegen,
    Heal,
    GummyHealAmount,
    
    // MOVEMENT
    IncreaseMovementSpeed,
    
    // PHYSICAL ATTACK
    MaxMeeleDmg,
    IncreaseCaCRate,
    AddMeeleRange,
    
    // DISTANCE ATTACK
    MaxProjectileDmg,
    IncreaseShootingRate,
    
    // REBOUND GUN
    IncreaseRebounds,
    
    // EXPLOSIVE GUN
    ExplosiveBullet,
    ExplosiveDamage,
    ExplosionRadious,
    
    ProjectileSpped,
    ProjectileTravelDistance
}

[CreateAssetMenu(fileName = "New Shop Card", menuName = "Roguelcake/Shop Card")]
public class ShopCard : ScriptableObject
{
    [Header("Contenido")]
    [SerializeField] private string cardTitle;
    [SerializeField] private Sprite cardImage;
    [SerializeField, Min(0)] private int cost;
    [SerializeField, TextArea(2, 5)] private string description;

    [Header("Efecto")]
    [SerializeField] private ShopCardEffect effect;
    [SerializeField, Min(1)] private int effectValue = 10;
    
    [Tooltip("Usado solo por efectos decimales (cadencias de ataque, ej. 0.3). Se ignora en el resto de los efectos.")]
    [SerializeField, Min(0.01f)] private float effectValueDecimal = 0.3f;

    public string Title => cardTitle;
    public Sprite Image => cardImage;
    public int Cost => cost;
    public string Description => description;

    public bool BounceUpgrade;
    public bool ExplosiveUpgrade;
    [Tooltip("Márcalo SOLO en la carta que desbloquea la categoría (Explosive Bullet / Increase Rebound). Las demás mejoras de esa categoría déjalo en false.")]
    public bool IsUnlockCard;
    

    public bool ApplyTo(PlayerStats playerStats)
    {
        if (playerStats == null) return false;

        switch (effect)
        {
            // HEALTH SYSTEM
            case ShopCardEffect.MaxHealth:
                playerStats.AddMaxHealth(effectValue);
                return true;
            case ShopCardEffect.HealthRegen:
                playerStats.AddHealthRegen(effectValue);
                return  true;
            case ShopCardEffect.Heal:
                playerStats.Heal(effectValue);
                return true;
            case ShopCardEffect.GummyHealAmount:
                playerStats.gummyHealAmount += effectValue;
                return true;
            
            // MOVEMENT
            case ShopCardEffect.IncreaseMovementSpeed:
                playerStats.movementSpeed += effectValue;
                return true;
            
            // PHYSICAL ATTACK
            case ShopCardEffect.IncreaseCaCRate:
                playerStats.AddPhysicalAttackRate(effectValueDecimal);
                return true;
            case ShopCardEffect.MaxMeeleDmg:
                playerStats.AddMaxMeeleDmg(effectValue);
                return true;
            case ShopCardEffect.AddMeeleRange:
                playerStats.AddMeeleRange(effectValue);
                return true;
            
            // RANGE ATTACK
            case ShopCardEffect.MaxProjectileDmg:
                playerStats.AddProjectileDamage(effectValue);
                return true;
            case ShopCardEffect.IncreaseShootingRate:
                playerStats.AddShootingRate(effectValueDecimal);
                return true;
            case ShopCardEffect.ProjectileSpped:
                playerStats.projectileSpeed += effectValue;
                return true;
            case ShopCardEffect.ProjectileTravelDistance:
                playerStats.maxTravelDistance += effectValue;
                return true;
            
            // MEJORAS DEL ARMA A DISTANCIA
            
            // ARMA DE REBOTE
            case ShopCardEffect.IncreaseRebounds:
                playerStats.projectileBounces += effectValue;
                if (playerStats.projectileBounces == 1)
                {
                    playerStats.ChangeTypeOfBullet(true, false, false);
                }
                return true;
            
            // EXPLOSIVE GUN - IF I HAVE THIS UPGRADE I BLOCK THE OTHERS AND APPEAR SOME UPGRADES ABOUT THE EXPLOSION
            case ShopCardEffect.ExplosiveBullet:
                playerStats.ChangeTypeOfBullet(false, true, false);
                return true;
            
            case ShopCardEffect.ExplosiveDamage:
                playerStats.explosiveDamage += effectValue;
                return true;
            
            case ShopCardEffect.ExplosionRadious:
                playerStats.explosiveRange += effectValue;
                return true;
            
            default:
                return false;
        }
    }
}
