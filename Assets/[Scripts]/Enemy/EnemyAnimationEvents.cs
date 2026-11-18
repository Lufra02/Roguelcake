using UnityEngine;

// Va en el mismo GameObject que contiene el Animator del enemigo.
public class EnemyAnimationEvents : MonoBehaviour
{
    private Enemy enemy;

    private void Awake()
    {
        enemy = GetComponentInParent<Enemy>();
        Debug.Assert(enemy != null, "EnemyAnimationEvents necesita un componente Enemy en un objeto padre.", this);
    }

    public void ActivateHitbox()
    {
        enemy?.ActivateHitbox();
    }
}
