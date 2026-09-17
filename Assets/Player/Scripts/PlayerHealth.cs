using UnityEngine;

public class PlayerHealth : Health
{

    public void GetHealthBack() 
    {
        
    }

    public override void TakeDamage(int dmg)
    {
        base.TakeDamage(dmg);
    }

    public override void Die()
    {
        base.Die();
    }
}
