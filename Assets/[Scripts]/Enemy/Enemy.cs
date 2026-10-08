using UnityEngine;
using UnityEngine.AI;

public abstract class Enemy : MonoBehaviour, IDamageable
{
    [Header("Main Attributes")]
    [SerializeField] protected float maxHealth = 100f;
    [SerializeField] protected float currentHealth;
    [SerializeField] protected float moveSpeed = 3.5f;
    [SerializeField] protected float damage = 1f;
    [SerializeField] protected Transform target;

    [Header("Detection & Attack")]
    [SerializeField] protected float detectionRange = 8f;
    [SerializeField] protected float attackRange = 1f;
    [SerializeField] protected float attackCooldown = 1.7f;   // segundos entre golpes
    [SerializeField] protected GameObject experienceOrbPrefab;
    protected PooledEnemy pooledEnemyComponent;
    [SerializeField] private Animator animator;

    [Header("Attack Hitbox")]
    [SerializeField] protected GameObject attackHitbox;        // hijo con SphereCollider (Is Trigger)
    [SerializeField] protected float hitboxActiveTime = 0.2f;  // cuánto tiempo queda activa la esfera

    private float hitboxTimer;
    private bool hitboxActive;
    private bool hasHitThisAttack;

    // Componentes de navegación y física
    protected NavMeshAgent agent;
    protected Rigidbody enemyRB;
    protected Collider enemyCollider;
    protected bool isDead = false;

    // Ataque
    protected PlayerHealth playerHealth;
    private float attackTimer;

    // Pausa (los hijos solo leen isGamePaused)
    private GameManager gameManager;
    protected bool isGamePaused;

    protected virtual void Awake()
    {
        enemyRB = GetComponent<Rigidbody>();
        enemyCollider = GetComponent<Collider>();
        agent = GetComponent<NavMeshAgent>();
        pooledEnemyComponent = GetComponent<PooledEnemy>();
        animator = GetComponentInChildren<Animator>();

        if (agent != null)
        {
            agent.speed = moveSpeed;
            agent.stoppingDistance = attackRange;
        }

        currentHealth = maxHealth;
    }

    // ---------------- PAUSA ----------------

    protected virtual void OnEnable()
    {
        gameManager = GameManager.Instance;
        if (gameManager != null)
            gameManager.onChangeGameState += OnChangeGameStateCallback;
    }

    protected virtual void OnDisable()
    {
        DeactivateHitbox();
        if (gameManager != null)
            gameManager.onChangeGameState -= OnChangeGameStateCallback;

        isGamePaused = false;
    }

    private void OnChangeGameStateCallback(GameState newState)
    {
        isGamePaused = newState == GameState.Pause;

        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
            agent.isStopped = isGamePaused;

        if (animator != null)
            animator.speed = isGamePaused ? 0f : 1f;   // congela también la animación
    }

    // ---------------- UPDATE ----------------

    protected virtual void Update()
    {
        if (isGamePaused || isDead || target == null || agent == null)
            return;

        if (attackTimer > 0f)
            attackTimer -= Time.deltaTime;

        if (attackTimer > 0f)
            attackTimer -= Time.deltaTime;

        UpdateHitbox();

        if (animator != null)
            animator.Play("Caminar");

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance <= attackRange)
        {
            Attack();
        }
        else if (distance <= detectionRange)
        {
            ChaseTarget();
        }
        else
        {
            Idle();
        }

        if (currentHealth <= 0f && !isDead)
        {
            Die();
        }
    }

    protected virtual void ChaseTarget()
    {
        if (agent.isStopped)
            agent.isStopped = false;

        agent.SetDestination(target.position);
    }

    protected virtual void Idle()
    {
        if (agent.hasPath)
            agent.ResetPath();
    }

    protected virtual void Attack()
    {
        // Se frena y rota hacia el jugador (esto sí ocurre cada frame)
        if (agent.hasPath)
            agent.ResetPath();

        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0;
        if (direction.sqrMagnitude > 0.001f)
            transform.forward = direction;

        // El golpe solo se da cuando el cooldown terminó
        if (attackTimer > 0f)
            return;

        attackTimer = attackCooldown;
        PerformAttack();
    }

    // El golpe en sí. Los hijos pueden sobrescribirlo (o dejarlo vacío).
    protected virtual void PerformAttack()
    {
        ActivateHitbox();
    }

    private void ActivateHitbox()
    {
        if (attackHitbox == null) return;
        Debug.Log($"<color=yellow>{name} activa hitbox de ataque!</color>");

        hasHitThisAttack = false;
        hitboxActive = true;
        hitboxTimer = hitboxActiveTime;
        attackHitbox.SetActive(true);
    }

    private void DeactivateHitbox()
    {
        hitboxActive = false;
        if (attackHitbox != null)
            attackHitbox.SetActive(false);
    }

    private void UpdateHitbox()
    {
        if (!hitboxActive) return;

        hitboxTimer -= Time.deltaTime;
        if (hitboxTimer <= 0f)
            DeactivateHitbox();
    }

    // Lo llama EnemyAttackHitbox cuando el trigger toca algo
    public void OnHitboxTouched(Collider other)
    {
        if (!hitboxActive || hasHitThisAttack || isDead || isGamePaused)
            return;

        PlayerHealth ph = other.GetComponentInParent<PlayerHealth>();
        if (ph == null || !ph.canBeDamaged)
            return;

        var pm = PlayerManager.Instance;
        if (pm != null && pm.isDead)
            return;

        hasHitThisAttack = true;   // un solo golpe por ataque
        ph.TakeDamage(damage);
        DeactivateHitbox();
    }

    // ---------------- VIDA ----------------

    public void TakeDamage(float amount)
    {
        if (isDead)
            return;

        print("Fue golpeado por el player");

        amount = Mathf.Max(0f, amount);
        currentHealth -= amount;

        OnDamageTaken(amount);
        if (currentHealth <= 0f)
            Die();
    }

    protected virtual void OnDamageTaken(float damage) { }

    protected virtual void Die()
    {
        DeactivateHitbox();
        isDead = true;
        currentHealth = 0f;

        if (agent != null && agent.enabled)
            agent.enabled = false;

        if (enemyCollider != null)
            enemyCollider.enabled = false;

        this.enabled = false;
        OnDeath();
    }

    public virtual void Initialize(GameObject xpPrefab, Transform playerTarget)
    {
        experienceOrbPrefab = xpPrefab;
        target = playerTarget;
        playerHealth = playerTarget != null ? playerTarget.GetComponent<PlayerHealth>() : null;

        isDead = false;
        currentHealth = maxHealth;
        attackTimer = 0f;
        DeactivateHitbox();
        this.enabled = true;

        if (enemyCollider != null)
            enemyCollider.enabled = true;

        if (agent != null)
        {
            agent.enabled = true;
            agent.isStopped = false;
        }
    }

    protected virtual void OnDeath()
    {
        Instantiate(experienceOrbPrefab, transform.position, Quaternion.identity);
        pooledEnemyComponent?.Die();
    }

    public void Teleport(Vector3 position)
    {
        if (agent != null && agent.isActiveAndEnabled)
            agent.Warp(position);
        else
            transform.position = position;

        if (enemyRB != null)
        {
            enemyRB.linearVelocity = Vector3.zero;     // en Unity 2022 o anterior: enemyRB.velocity
            enemyRB.angularVelocity = Vector3.zero;
        }
    }
}