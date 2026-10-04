using UnityEngine;

// Proyectil simple: viaja en línea recta y aplica daño a objetos con IDamageable.
[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    [Header("Configuracion del proyectil")]
    private float damage = 5;
    private int maxTravelDistance = 15;

    [Header("Prefab generado al impactar")]
    [SerializeField] private GameObject gummyPrefab;
    [SerializeField] private float spawnOffset = 0.5f;

    [Header("Caída al perder alcance")]
    [SerializeField, Min(0.1f)] private float horizontalDeceleration = 15f;
    [SerializeField] private float groundLevel = 0f;

    [Header("Rebote entre enemigos")]
    private bool isBouncy;
    private int maxBounces;

    [SerializeField] private LayerMask enemyLayer;
    [SerializeField, Min(0f)] private float bounceSearchRadius = 10f;

    [Header("Explosion")]
    private bool isExplosive;
    private int sizeOfExplosion;
    private float explosionDamage;

    private bool isHugeGun;

    [Header("Referencias")]
    private GameManager gameManager;
    private Rigidbody rb;
    private Collider projectileCollider;

    [Header("Estado del recorrido")]
    private Vector3 initPosition;
    private bool isFalling;
    private float projectileSpeed;
    private int bouncesDone;

    [Header("Estado de pausa")]
    private Vector3 velocityBeforePause;
    private bool isGamePaused;

    private bool hasExploded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        projectileCollider = GetComponent<Collider>();

        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        initPosition = transform.position;

        bouncesDone = 0;
        maxBounces = 0;
    }

    private void Start()
    {
        gameManager = GameManager.Instance;

        if (gameManager != null)
        {
            gameManager.onChangeGameState += OnChangeGameStateCallback;

            isGamePaused = gameManager.gameState == GameState.Pause;

            if (isGamePaused)
            {
                velocityBeforePause = rb.linearVelocity;
                rb.linearVelocity = Vector3.zero;
                rb.isKinematic = true;
            }
        }
    }

    public void OnChangeGameStateCallback(GameState newState)
    {
        isGamePaused = newState == GameState.Pause;

        if (isGamePaused)
        {
            velocityBeforePause = rb.linearVelocity;
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
        else
        {
            rb.isKinematic = false;
            rb.linearVelocity = velocityBeforePause;
        }
    }

    private void Update()
    {
        if (isGamePaused)
            return;

        if (!isFalling &&
            !hasExploded &&
            Vector3.Distance(initPosition, transform.position) > maxTravelDistance)
        {
            StartFalling();
        }
    }

    private void FixedUpdate()
    {
        if (isGamePaused)
            return;

        if (isFalling)
            UpdateFalling();
    }

    public void Launch(Vector3 direction, float speed)
    {
        projectileSpeed = speed;

        Vector3 launchVelocity = direction.normalized * speed;

        if (isGamePaused)
            velocityBeforePause = launchVelocity;
        else
            rb.linearVelocity = launchVelocity;
    }

    public void Configure(
        int newDamage,
        int newMaxBounces,
        int newMaxTravelDistance,
        bool isBouncy_,
        bool isExplosive_,
        bool isHugeBullet_,
        int explosionDamage_,
        int explosiveRange_,
        float hugeDamagePercent_ = 0)
    {
        damage = Mathf.Max(0, newDamage);

        isHugeGun = isHugeBullet_;

        if (isHugeBullet_)
        {
            damage += (hugeDamagePercent_ / 100f) * damage;
            transform.localScale *= 3f;
        }

        maxTravelDistance = Mathf.Max(0, newMaxTravelDistance);

        isBouncy = isBouncy_;
        maxBounces = Mathf.Max(0, newMaxBounces);

        isExplosive = isExplosive_;
        explosionDamage = (damage + explosionDamage_) / 2f;
        sizeOfExplosion = Mathf.Max(0, explosiveRange_);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasExploded)
            return;

        if (other.CompareTag("Player"))
            return;

        IDamageable damageable = other.GetComponent<IDamageable>();

        if (damageable == null)
            return;

        damageable.TakeDamage(damage);

        if (isExplosive)
        {
            Explode();
            return;
        }

        if (isBouncy && bouncesDone < maxBounces)
        {
            if (TryBounceToClosestEnemy(other))
            {
                bouncesDone++;
                return;
            }
        }

        StartFalling();
    }

    private bool TryBounceToClosestEnemy(Collider enemyJustHit)
    {
        Collider[] enemies = Physics.OverlapSphere(
            transform.position,
            bounceSearchRadius,
            enemyLayer
        );

        Collider closestEnemy = null;
        float closestDistanceSqr = float.MaxValue;

        foreach (Collider enemy in enemies)
        {
            if (enemy == enemyJustHit)
                continue;

            IDamageable damageable = enemy.GetComponent<IDamageable>();

            if (damageable == null)
                continue;

            float distanceSqr =
                (enemy.bounds.center - transform.position).sqrMagnitude;

            if (distanceSqr < closestDistanceSqr)
            {
                closestDistanceSqr = distanceSqr;
                closestEnemy = enemy;
            }
        }

        if (closestEnemy == null)
            return false;

        Vector3 bounceDirection =
            closestEnemy.bounds.center - transform.position;

        if (bounceDirection.sqrMagnitude < 0.0001f)
            return false;

        transform.rotation =
            Quaternion.LookRotation(bounceDirection, Vector3.up);

        Launch(bounceDirection, projectileSpeed);

        return true;
    }

    private void Explode()
    {
        if (hasExploded)
            return;

        hasExploded = true;

        Collider[] enemiesInBlast = Physics.OverlapSphere(
            transform.position,
            sizeOfExplosion,
            enemyLayer
        );

        foreach (Collider enemy in enemiesInBlast)
        {
            IDamageable damageable =
                enemy.GetComponent<IDamageable>();

            if (damageable != null)
                damageable.TakeDamage(explosionDamage);
        }

        if (gummyPrefab != null)
        {
            Instantiate(
                gummyPrefab,
                transform.position + Vector3.up * spawnOffset,
                transform.rotation
            );
        }

        Destroy(gameObject);
    }

    private void StartFalling()
    {
        if (isFalling)
            return;

        isFalling = true;

        rb.useGravity = true;
    }

    private void UpdateFalling()
    {
        Vector3 velocity = rb.linearVelocity;

        Vector3 horizontalVelocity =
            new Vector3(velocity.x, 0f, velocity.z);

        Vector3 dampedHorizontal =
            Vector3.MoveTowards(
                horizontalVelocity,
                Vector3.zero,
                horizontalDeceleration * Time.fixedDeltaTime
            );

        rb.linearVelocity = new Vector3(
            dampedHorizontal.x,
            velocity.y,
            dampedHorizontal.z
        );

        if (transform.position.y <= groundLevel)
        {
            Land();
        }
    }

    private void Land()
    {
        if (isExplosive)
        {
            Explode();
            return;
        }

        if (gummyPrefab != null)
        {
            Vector3 spawnPosition = transform.position;
            spawnPosition.y = groundLevel + spawnOffset;

            Instantiate(
                gummyPrefab,
                spawnPosition,
                transform.rotation
            );
        }

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.onChangeGameState -=
                OnChangeGameStateCallback;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!isExplosive)
            return;

        Gizmos.color = new Color(1f, 0.4f, 0f, 0.15f);
        Gizmos.DrawSphere(
            transform.position,
            sizeOfExplosion
        );

        Gizmos.color = new Color(1f, 0.4f, 0f, 0.8f);
        Gizmos.DrawWireSphere(
            transform.position,
            sizeOfExplosion
        );
    }
}