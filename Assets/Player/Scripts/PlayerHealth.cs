using UnityEngine;

public class PlayerHealth : Health
{

    public void GetHealthBack() 
    {
        currentHealth += PlayerManager.Instance.healthForGummies;
    }

    public override void TakeDamage(int dmg)
    {
        base.TakeDamage(dmg);
        if (currentHealth <= 0) 
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
