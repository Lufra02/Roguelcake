using UnityEngine;

public enum ShopCardEffect
{
    MaxHealth,
    MaxMeeleDmg,
    MaxProjectileDmg,
    AddMeeleRange
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

    public string Title => cardTitle;
    public Sprite Image => cardImage;
    public int Cost => cost;
    public string Description => description;

    public bool ApplyTo(PlayerStats playerStats)
    {
        if (playerStats == null) return false;

        switch (effect)
        {
            case ShopCardEffect.MaxHealth:
                playerStats.AddMaxHealth(effectValue);
                return true;
            case ShopCardEffect.MaxMeeleDmg:
                playerStats.AddMaxMeeleDmg(effectValue);
                return true;
            case ShopCardEffect.MaxProjectileDmg:
                playerStats.AddProjectileDamage(effectValue);
                return true;
            case ShopCardEffect.AddMeeleRange:
                playerStats.AddMeeleRange(effectValue);
                return true;
            default:
                return false;
        }
    }
}
