using UnityEngine;
using UnityEngine.AI; // 1. IMPORTANTE: Necesario para NavMeshAgent

public abstract class Enemy : MonoBehaviour, IDamageable
{
    [Header("Main Attributes")]
    [SerializeField] protected float maxHealth = 100f;
    [SerializeField] protected float currentHealth;
    [SerializeField] protected float moveSpeed = 3.5f;
    [SerializeField] protected float damage = 10f;
    [SerializeField] protected Transform target;

    [Header("Detection & Attack")]
    [SerializeField] protected float detectionRange = 8f;
    [SerializeField] protected float attackRange = 2.5f;
    [SerializeField] protected float attackCooldown = 1f;

    // Componentes de navegación y física
    protected NavMeshAgent agent;
    protected Rigidbody enemyRB;
    protected Collider enemyCollider;
    protected bool isDead = false;

    protected virtual void Awake()
    {
        enemyRB = GetComponent<Rigidbody>();
        enemyCollider = GetComponent<Collider>();
        agent = GetComponent<NavMeshAgent>();

        // Sincronizamos la velocidad y la distancia de freno con tus variables
        if (agent != null)
        {
            agent.speed = moveSpeed;
            agent.stoppingDistance = attackRange;
        }

        currentHealth = maxHealth;
    }

    protected virtual void Start()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
        }
    }

    protected virtual void Update()
    {
        if (isDead || target == null || agent == null)
            return;

        // Medimos la distancia real
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
    }

    // El agente calcula la ruta y esquiva los obstáculos automáticamente
    protected virtual void ChaseTarget()
    {
        if (agent.isStopped)
            agent.isStopped = false;

        agent.SetDestination(target.position);
    }

    protected virtual void Idle()
    {
        // Detiene el movimiento del agente
        if (agent.hasPath)
            agent.ResetPath();
    }

    protected virtual void Attack()
    {
        // Se frena para atacar
        if (agent.hasPath)
            agent.ResetPath();

        // Rota hacia el jugador mientras ataca
        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0;
        if (direction.sqrMagnitude > 0.001f)
            transform.forward = direction;

        // Debug.Log($"{name} ataca...");
    }

    public void TakeDamage(float amount)
    {
        if (isDead)
            return;

        amount = Mathf.Max(0f, amount);
        currentHealth -= amount;

        OnDamageTaken(amount);

        if (currentHealth <= 0f)
            Die();
    }

    protected virtual void OnDamageTaken(float damage) { }

    protected virtual void Die()
    {
        if (isDead)
            return;

        isDead = true;
        currentHealth = 0f;

        // Desactivamos el agente para que no siga calculando rutas al morir
        if (agent != null && agent.enabled)
            agent.enabled = false;

        if (enemyCollider != null)
            enemyCollider.enabled = false;

        this.enabled = false;
        OnDeath();
    }

    protected virtual void OnDeath()
    {
        Destroy(gameObject, 1.5f);
    }
}