using UnityEngine;

public class ExplosiveEnemy : Enemy
{
    [Header("Explosion")]
    [SerializeField] private float explosionDistance = 2f;
    [SerializeField] private float explosionDelay = 1.5f;

    private bool isPreparingExplosion;
    private float explosionTimer;

    protected override void Awake()
    {
        base.Awake();

        // La base fija stoppingDistance = attackRange; este enemigo debe acercarse más
        if (agent != null)
            agent.stoppingDistance = 0f;
    }

    public override void Initialize(GameObject xpPrefab, Transform playerTarget)
    {
        base.Initialize(xpPrefab, playerTarget);

        isPreparingExplosion = false;
        explosionTimer = 0f;
    }

    // Sin ataque básico: dentro de attackRange sigue persiguiendo al jugador
    protected override void Attack()
    {
        ChaseTarget();
    }

    // Por seguridad, por si algo más llama a PerformAttack
    protected override void PerformAttack() { }

    protected override void Update()
    {
        if (isGamePaused)
            return;

        if (isPreparingExplosion)
        {
            UpdateExplosionTimer();
            base.Update();
            return;
        }

        base.Update();

        if (target == null || isDead)
            return;

        float distanceToPlayer = Vector3.Distance(transform.position, target.position);

        if (distanceToPlayer <= explosionDistance)
            StartExplosion();
    }

    private void StartExplosion()
    {
        isPreparingExplosion = true;
        explosionTimer = explosionDelay;

        // animator.SetTrigger("PrepareExplosion");
    }

    private void UpdateExplosionTimer()
    {
        if (isDead)
        {
            isPreparingExplosion = false;
            return;
        }

        explosionTimer -= Time.deltaTime;
        if (explosionTimer <= 0f)
        {
            isPreparingExplosion = false;
            Explode();
        }
    }

    private void Explode()
    {
        if (playerHealth != null)
            playerHealth.RecieveExplosion();

        // VFX, sonido, partículas, animación

        Die();
    }
}