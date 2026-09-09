using UnityEngine;
public abstract class Enemy : MonoBehaviour, IDamageable
{
    [Header("Main Atributes")]
    [SerializeField] protected float maxHealth;
    [SerializeField] protected float currentHealth;
    [SerializeField] protected float moveSpeed;
    [SerializeField] protected float damage;
    [SerializeField] protected Transform target;

    [Header("Detection & attack")]
    [SerializeField] protected float detectionRange = 8f;
    [SerializeField] protected float attackRange = 1.5f;
    [SerializeField] protected float attackCooldown = 1f;

    protected Rigidbody enemyRB;
    protected Collider enemyCollider;
    protected bool isDead = false;


    protected virtual void Awake()
    {
        enemyRB = GetComponent<Rigidbody>();
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

    // Update is called once per frame
    protected virtual void Update()
    {
        if (isDead || target == null)
            return;

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance <= attackRange)
        {
            Attack();
        }
        else if (distance <= detectionRange)
        {
            Vector3 direction = target.position - transform.position;
            Move(direction);
        }
        else
        {
            Idle();
        }

    }

    protected virtual void Move(Vector3 direction)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        direction.Normalize();

        transform.position += direction * moveSpeed * Time.deltaTime;

        transform.forward = direction;
    }




    protected virtual void Attack()
    {
        Debug.Log($"{name} attacks for {damage} damage.");
    }

    protected virtual void Idle()
    {
        enemyRB.linearVelocity = Vector2.zero;
    }

    public void TakeDamage(float amount)
    {
        if (isDead)
            return;

        amount = Mathf.Max(0f, amount);

        currentHealth -= amount;

        Debug.Log(
            $"{name} recibió {amount} de daño. " +
            $"Vida restante: {currentHealth}"
        );

        OnDamageTaken(amount);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    protected virtual void OnDamageTaken(float damage)
    {
        //esta funcion se le hace override para un comportamiento especial
    }

    protected virtual void Die()
    {
        if (isDead)
            return;

        isDead = true;
        currentHealth = 0f;

        enemyRB.linearVelocity = Vector3.zero;

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
