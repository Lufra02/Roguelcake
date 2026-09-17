using Unity.VisualScripting;
using UnityEngine;

// Proyectil simple: viaja en línea recta en la dirección hacia el mouse
// y aplica daño al primer objeto con IDamageable que golpee.
// El GameObject necesita un Collider 3D marcado como "Is Trigger".
[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    [Header("Configuracion del proyectil")]
    public int damage = 5;
    public int maxTravelDistance = 15;

    [Header("Prefab generado al impactar")]
    [SerializeField] private GameObject gummyPrefab;

    [Header("Rebote entre enemigos")]
    private int maxBounces;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField, Min(0f)] private float bounceSearchRadius = 10f;

    [Header("Referencias")]
    private GameManager gameManager;
    private Rigidbody rb;
    private Collider projectileCollider;

    [Header("Estado del recorrido")]
    private Vector3 initPosition;
    private bool spawnToGround;
    private bool isSpawningInGround;
    private float groundSpawnTimer;
    private float projectileSpeed;
    private int bouncesDone;

    [Header("Estado de pausa")]
    private Vector3 velocityBeforePause;
    private bool isGamePaused;

    [Header("Tiempos")]
    private const float GroundSpawnDelay = 1f;

    void Awake()
    {

        rb = GetComponent<Rigidbody>();
        projectileCollider = GetComponent<Collider>();
        rb.useGravity = false; // el proyectil viaja recto; actívalo si quieres que caiga por gravedad

        spawnToGround = false;
        groundSpawnTimer = 0;
        initPosition = transform.position;

        bouncesDone = 0;
        maxBounces = PlayerManager.Instance.gummyBounces;
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
            }
        }
    }

    //Esta es la funcion que quiero que se haga cada vez que pauso o despauso el juego
    public void OnChangeGameStateCallback(GameState newState)
    {
        isGamePaused = newState == GameState.Pause;

        if (isGamePaused)
        {
            // linearVelocity ya contiene rapidez y dirección.
            velocityBeforePause = rb.linearVelocity;
            rb.linearVelocity = Vector3.zero;
        }
        else if (!isSpawningInGround)
        {
            rb.linearVelocity = velocityBeforePause;
        }
    }

    private void Update()
    {
        if (isGamePaused) return;

        if (!isSpawningInGround && Vector3.Distance(initPosition, transform.position) > maxTravelDistance)
        {
            spawnToGround = true;
        }

        if (spawnToGround && !isSpawningInGround) {
            SpawnInGround();
        }

        if (isSpawningInGround)
            UpdateGroundSpawn();
    }

    public void Launch(Vector3 direction, float speed)
    {
        // Nota: en Unity 6 es "linearVelocity"; en versiones anteriores usa "velocity"
        projectileSpeed = speed;
        Vector3 launchVelocity = direction.normalized * speed;

        if (isGamePaused)
            velocityBeforePause = launchVelocity;
        else
            rb.linearVelocity = launchVelocity;
    }

    void OnTriggerEnter(Collider other)
    {
        if (isSpawningInGround || other.CompareTag("Player")) return; // ignora al propio jugador

        var damageable = other.GetComponent<IDamageable>();
        if (damageable == null) return;

        damageable.TakeDamage(damage);

        // Solo se excluye al enemigo recién golpeado. Así puede rebotar A -> B -> A.
        if (bouncesDone < maxBounces && TryBounceToClosestEnemy(other))
        {
            bouncesDone++;
            return;
        }

        // Si no quedan rebotes o no hay otro enemigo al alcance, termina el proyectil.
        spawnToGround = true;
    }

    private bool TryBounceToClosestEnemy(Collider enemyJustHit)
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, bounceSearchRadius, enemyLayer);
        Collider closestEnemy = null;
        float closestDistanceSqr = float.MaxValue;

        foreach (Collider enemy in enemies)
        {
            if (enemy == enemyJustHit || enemy.GetComponent<IDamageable>() == null) continue;

            float distanceSqr = (enemy.bounds.center - transform.position).sqrMagnitude;
            if (distanceSqr < closestDistanceSqr)
            {
                closestDistanceSqr = distanceSqr;
                closestEnemy = enemy;
            }
        }

        if (closestEnemy == null) return false;

        Vector3 bounceDirection = closestEnemy.bounds.center - transform.position;
        if (bounceDirection.sqrMagnitude < 0.0001f) return false;

        transform.rotation = Quaternion.LookRotation(bounceDirection, Vector3.up);
        Launch(bounceDirection, projectileSpeed);
        return true;
    }

    public void SpawnInGround() 
    {
        if (isSpawningInGround) return;

        isSpawningInGround = true;

        spawnToGround = false;
        // Detiene el proyectil e impide que haga más daño de inmediato.
        rb.linearVelocity = Vector3.zero;
        projectileCollider.enabled = false;
    }

    private void UpdateGroundSpawn()
    {
        groundSpawnTimer += Time.deltaTime;
        if (groundSpawnTimer < GroundSpawnDelay) return;

        if (gummyPrefab != null)
            Instantiate(gummyPrefab, transform.position, transform.rotation);

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (gameManager != null)
            gameManager.onChangeGameState -= OnChangeGameStateCallback;
    }
}
