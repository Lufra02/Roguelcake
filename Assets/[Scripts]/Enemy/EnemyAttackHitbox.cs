using UnityEngine;

public class EnemyAttackHitbox : MonoBehaviour
{
    private Enemy owner;

    private void Awake()
    {
        owner = GetComponentInParent<Enemy>();
    }

    private void OnTriggerEnter(Collider other)
    {
        owner?.OnHitboxTouched(other);
    }

    // Por si el jugador ya estaba dentro cuando se activó la esfera
    private void OnTriggerStay(Collider other)
    {
        owner?.OnHitboxTouched(other);
    }
}