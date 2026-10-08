using UnityEngine;

public class ExplosiveEnemy : Enemy
{
    [Header("Explosion")]
    [SerializeField] private float explosionDistance = 2f;
    [SerializeField] private float explosionDelay = 1.5f;
    [SerializeField] private float explosionRadius = 3f;       // alcance real del daño
    [SerializeField] private float cancelDistance = 4f;        // si el jugador se aleja más que esto, se cancela
    [SerializeField] private bool cancelIfPlayerEscapes = true;

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

        // El jugador escapó: se cancela y el enemigo vuelve a perseguirlo
        if (cancelIfPlayerEscapes && target != null)
        {
            float dist = Vector3.Distance(transform.position, target.position);
            if (dist > cancelDistance)
            {
                isPreparingExplosion = false;
                return;
            }
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
        if (playerHealth != null && target != null)
        {
            float dist = Vector3.Distance(transform.position, target.position);

            if (dist <= explosionRadius)
                playerHealth.RecieveExplosion();
        }

        // VFX, sonido, partículas, animación (la explosión se ve aunque no alcance al jugador)

        Die();
    }
}