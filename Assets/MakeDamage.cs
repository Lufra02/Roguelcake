using UnityEngine;

public class MakeDamage : MonoBehaviour
{
    public int dmg = 15;

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<IDamageable>();
        if (player != null)
        {
            player.TakeDamage(dmg);
        }
    }
}
