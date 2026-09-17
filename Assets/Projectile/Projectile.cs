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

    [Header("Referencias")]
    private GameManager gameManager;
    private Rigidbody rb;
    private Collider projectileCollider;

    [Header("Estado del recorrido")]
    private Vector3 initPosition;
    private bool spawnToGround;
    private bool isSpawningInGround;
    private float groundSpawnTimer;

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
        Vector3 launchVelocity = direction.normalized * speed;

        if (isGamePaused)
            velocityBeforePause = launchVelocity;
        else
            rb.linearVelocity = launchVelocity;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) return; // ignora al propio jugador

        var damageable = other.GetComponent<IDamageable>();
        if (damageable != null)
        {
            // CUANDO GOLPEE AL ENEMIGO
            damageable.TakeDamage(damage);

            // LA GOMITA DEBE CAER EN EL LUGAR DE IMPACTO
            spawnToGround = true;
        }
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
