using UnityEngine;

[RequireComponent(typeof(PlayerStats))]
public class PlayerHealth : Health
{
    private PlayerStats stats;
    private float regenerationAccumulator;

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();
        if (stats != null)
            SetMaxHealth(stats.maxHealth);
    }

    private void Update()
    {
        if (stats == null || stats.healthRegenerationPerSecond <= 0f || CurrentHealth >= MaxHealth) return;

        regenerationAccumulator += stats.healthRegenerationPerSecond * Time.deltaTime;
        int healthToRestore = Mathf.FloorToInt(regenerationAccumulator);
        if (healthToRestore <= 0) return;

        Heal(healthToRestore);
        regenerationAccumulator -= healthToRestore;
    }

    public void GetHealthBack() 
    {
        Heal(stats.gummyHealAmount);
    }

    public void RestoreHealth(int amount) => Heal(amount);

    public void SetMaximumHealth(int amount) => SetMaxHealth(amount);

    public override void TakeDamage(int dmg)
    {
        base.TakeDamage(dmg);
        if (CurrentHealth <= 0) 
        {
            Die();
        }
    }

    public override void Die()
    {
        base.Die();
        print("se murio");
    }
}
