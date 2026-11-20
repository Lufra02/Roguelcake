using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public abstract class Enemy : MonoBehaviour, IDamageable
{
    [Header("Hit Flash Effect")]
    [SerializeField] private Renderer[] enemyRenderers; // Cambió a arreglo
    [SerializeField] private Material whiteFlashMaterial; // <--- NUEVO: Arrastra aquí un material blanco básico
    [SerializeField] private float flashDuration = 0.1f;

    private Coroutine flashCoroutine;
    private Material[][] originalMaterials; // Guarda los materiales originales de cada pieza

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
    private PlayerManager playerManager;
    protected bool isGamePaused;

    protected virtual void Awake()
    {
        // Busca automáticamente TODOS los renderers que estén en este objeto y sus hijos
        if (enemyRenderers == null || enemyRenderers.Length == 0)
            enemyRenderers = GetComponentsInChildren<Renderer>();

        // Guardamos los materiales originales de forma segura al iniciar
        if (enemyRenderers != null && enemyRenderers.Length > 0)
        {
            originalMaterials = new Material[enemyRenderers.Length][];
            for (int i = 0; i < enemyRenderers.Length; i++)
            {
                if (enemyRenderers[i] != null)
                {
                    originalMaterials[i] = enemyRenderers[i].materials;
                }
            }
        }

        enemyRB = GetComponent<Rigidbody>();
        enemyCollider = GetComponent<Collider>();
        agent = GetComponent<NavMeshAgent>();
        pooledEnemyComponent = GetComponent<PooledEnemy>();
        animator = GetComponentInChildren<Animator>();
        // Debug.Log(damage);

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

        playerManager = PlayerManager.Instance;
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

        IsPausedOrInShop();
    }

    protected bool IsPausedOrInShop()
    {
        if (playerManager == null)
            playerManager = PlayerManager.Instance;

        bool shouldStop = isGamePaused || (playerManager != null && playerManager.isInShop);

        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
            agent.isStopped = shouldStop;

        if (animator != null)
            animator.speed = shouldStop ? 0f : 1f;

        Debug.Assert(!shouldStop || agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh || agent.isStopped,
            "Un enemigo detenido debe tener su NavMeshAgent detenido.", this);

        return shouldStop;
    }

    // ---------------- UPDATE ----------------

    protected virtual void Update()
    {
        if (IsPausedOrInShop() || isDead || target == null || agent == null)
            return;

        if (attackTimer > 0f)
            attackTimer -= Time.deltaTime;


        UpdateHitbox();

        if (animator != null)
        {
            // Solo reproduce caminar si la distancia es mayor al rango de ataque (es decir, está persiguiendo)
            float distanciaActual = Vector3.Distance(transform.position, target.position);
            if (distanciaActual > attackRange)
            {
                animator.Play("Caminar");
            }
        }

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
        //ActivateHitbox();

        // Activa la animación de ataque si el Animator existe
        if (animator != null)
        {
            animator.SetTrigger("AttackTrigger");
        }
    }

    public void ActivateHitbox()
    {
        if (attackHitbox == null) return;

        attackHitbox.SetActive(true);
        hasHitThisAttack = false;
        hitboxActive = true;
        hitboxTimer = hitboxActiveTime;
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
        if (!hitboxActive || hasHitThisAttack || isDead || IsPausedOrInShop())
            return;

        PlayerHealth ph = other.GetComponentInParent<PlayerHealth>();
        if (ph == null || !ph.canBeDamaged)
            return;

        var pm = PlayerManager.Instance;
        if (pm != null && pm.isDead)
            return;

        hasHitThisAttack = true;   // un solo golpe por ataque
        // Debug.Log($"<color=red>{name} golpea al jugador con {damage} de daño!</color>");
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

    protected virtual void OnDamageTaken(float damage)
    {
        if (enemyRenderers != null && enemyRenderers.Length > 0 && !isDead)
        {
            if (flashCoroutine != null)
                StopCoroutine(flashCoroutine);

            flashCoroutine = StartCoroutine(FlashRoutine());
        }
    }

    protected virtual void Die()
    {
        DeactivateHitbox();
        isDead = true;
        currentHealth = 0f;

        // Detenemos la corrutina de flash si estaba activa
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        // Restauramos materiales originales al morir para evitar bugs visuales en el pool
        if (enemyRenderers != null && originalMaterials != null)
        {
            for (int i = 0; i < enemyRenderers.Length; i++)
            {
                if (enemyRenderers[i] != null && i < originalMaterials.Length)
                {
                    enemyRenderers[i].materials = originalMaterials[i];
                    enemyRenderers[i].enabled = true;
                }
            }
        }

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

        // Restauramos renderers y materiales al inicializar (indispensable para Object Pooling)
        if (enemyRenderers != null && originalMaterials != null)
        {
            for (int i = 0; i < enemyRenderers.Length; i++)
            {
                if (enemyRenderers[i] != null && i < originalMaterials.Length)
                {
                    enemyRenderers[i].materials = originalMaterials[i];
                    enemyRenderers[i].enabled = true;
                }
            }
        }

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

    private IEnumerator FlashRoutine()
    {
        // Si no asignaste un material blanco en el Inspector, salimos para evitar errores
        if (whiteFlashMaterial == null) yield break;

        // 1. Cambiamos todos los materiales de las piezas al material blanco
        foreach (var rend in enemyRenderers)
        {
            if (rend != null)
            {
                rend.enabled = true;
                Material[] whiteMats = new Material[rend.materials.Length];
                for (int i = 0; i < whiteMats.Length; i++)
                {
                    whiteMats[i] = whiteFlashMaterial; // Reemplaza por el blanco
                }
                rend.materials = whiteMats;
            }
        }

        // 2. Esperamos el tiempo del destello
        yield return new WaitForSeconds(flashDuration);

        // 3. Restauramos los materiales originales exactos
        if (!isDead && enemyRenderers != null)
        {
            for (int i = 0; i < enemyRenderers.Length; i++)
            {
                if (enemyRenderers[i] != null && originalMaterials != null && i < originalMaterials.Length)
                {
                    enemyRenderers[i].materials = originalMaterials[i];
                }
            }
        }
    }
}
