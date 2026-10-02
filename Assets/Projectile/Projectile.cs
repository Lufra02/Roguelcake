using UnityEngine;

// Proyectil simple: viaja en línea recta en la dirección hacia el mouse
// y aplica daño al primer objeto con IDamageable que golpee.
// El GameObject necesita un Collider 3D marcado como "Is Trigger".
[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    [Header("Configuracion del proyectil")]
    private int damage = 5;
    private int maxTravelDistance = 15;

    [Header("Prefab generado al impactar")]
    [SerializeField] private GameObject gummyPrefab;
    [SerializeField] private float spawnOffset = 0.5f;
    
    [Header("Caída al perder alcance")]
    [Tooltip("Qué tan rápido se frena la velocidad horizontal una vez que empieza a caer (unidades/seg²). Más alto = frena más rápido.")]
    [SerializeField, Min(0.1f)] private float horizontalDeceleration = 15f;
    [Tooltip("Altura Y del suelo en el mundo. Cuando el proyectil cae por debajo de este valor, aterriza. Ajusta esto a la altura real de tu piso.")]
    [SerializeField] private float groundLevel = 0f;

    [Header("Rebote entre enemigos")]
    private bool isBouncy;
    private int maxBounces;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField, Min(0f)] private float bounceSearchRadius = 10f;

    [Header("Explosion")]
    private bool isExplosive;
    private int sizeOfExplosion;
    private int explosionDamage;

    [Header("Referencias")]
    private GameManager gameManager;
    private Rigidbody rb;
    private Collider projectileCollider;

    [Header("Estado del recorrido")]
    private Vector3 initPosition;
    private bool isFalling; // true desde que supera maxTravelDistance hasta que aterriza
    private float projectileSpeed;
    private int bouncesDone;

    [Header("Estado de pausa")]
    private Vector3 velocityBeforePause;
    private bool isGamePaused;

    // Evita que la explosión (y la gomita/destrucción que dispara) ocurra más de una vez.
    private bool hasExploded;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        projectileCollider = GetComponent<Collider>();
        rb.useGravity = false; // el proyectil viaja recto mientras vuela; se activa sola al caer

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
            }
        }
    }

    //Esta es la funcion que se hace cada vez que pauso o despauso el juego
    public void OnChangeGameStateCallback(GameState newState)
    {
        isGamePaused = newState == GameState.Pause;

        if (isGamePaused)
        {
            // linearVelocity ya contiene rapidez y dirección (sea volando o cayendo).
            velocityBeforePause = rb.linearVelocity;
            rb.linearVelocity = Vector3.zero;
        }
        else
        {
            rb.linearVelocity = velocityBeforePause;
        }
    }

    private void Update()
    {
        if (isGamePaused) return;

        if (!isFalling && !hasExploded && Vector3.Distance(initPosition, transform.position) > maxTravelDistance)
        {
            StartFalling();
        }

        if (isFalling)
        {
            UpdateFalling();
        }
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

    // PlayerCombat lo llama al crear el proyectil para aplicar las estadísticas actuales.
    public void Configure(int newDamage, int newMaxBounces, int newMaxTravelDistance, bool isBouncy_, bool isExplosive_, bool isHugeBullet_, int explosionDamage_, int explosiveRange_)
    {
        damage = Mathf.Max(0, newDamage);
        maxTravelDistance = Mathf.Max(0, newMaxTravelDistance);

        isBouncy = isBouncy_;
        maxBounces = Mathf.Max(0, newMaxBounces);

        isExplosive = isExplosive_;
        explosionDamage = (damage + explosionDamage_) / 2;
        sizeOfExplosion = explosiveRange_;
    }

    void OnTriggerEnter(Collider other)
    {
        if (isFalling || hasExploded || other.CompareTag("Player")) return; // ignora al propio jugador

        var damageable = other.GetComponent<IDamageable>();
        if (damageable == null) return;

        damageable.TakeDamage(damage);

        // COMPORTAMIENTOS DE LA BALA DEPENDIENDO DE QUE MEJORA TENGA
        if (isExplosive)
        {
            // Explode() aplica el daño en área, instancia la gomita UNA sola vez y destruye el proyectil.
            Explode();
            return;
        }

        // ---> REBOTE
        // Solo se excluye al enemigo recién golpeado. Así puede rebotar A -> B -> A.
        if (isBouncy && bouncesDone < maxBounces && TryBounceToClosestEnemy(other))
        {
            bouncesDone++;
            return;
        }

        // Si no quedan rebotes o no hay otro enemigo al alcance, empieza a caer.
        StartFalling();
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

    // Aplica daño en área a todos los enemigos dentro de sizeOfExplosion (incluye al que
    // ya recibió el golpe directo), instancia la gomita UNA sola vez y destruye el proyectil.
    private void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        Collider[] enemiesInBlast = Physics.OverlapSphere(transform.position, sizeOfExplosion, enemyLayer);

        foreach (Collider enemy in enemiesInBlast)
        {
            var damageable = enemy.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(explosionDamage);
            }
        }

        // TODO: agregar efecto visual/sonido de explosión aquí, además de la gomita.
        if (gummyPrefab != null)
            Instantiate(gummyPrefab, transform.position + (Vector3.up * spawnOffset), transform.rotation);

        Destroy(gameObject);
    }

    // Empieza la caída: deja de dañar, activa la gravedad real de Unity (que ya da el efecto
    // de "cae cada vez más rápido") y de ahí en más solo frenamos la velocidad horizontal a mano.
    private void StartFalling()
    {
        if (isFalling) return;
        isFalling = true;

        projectileCollider.enabled = false; // ya no debe seguir dañando mientras cae
        rb.useGravity = true;
    }

    private void UpdateFalling()
    {
        // Frena solo la velocidad horizontal (X/Z); la vertical (Y) la sigue manejando la gravedad.
        Vector3 velocity = rb.linearVelocity;
        Vector3 horizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);
        Vector3 dampedHorizontal = Vector3.MoveTowards(horizontalVelocity, Vector3.zero, horizontalDeceleration * Time.deltaTime);
        rb.linearVelocity = new Vector3(dampedHorizontal.x, velocity.y, dampedHorizontal.z);

        if (transform.position.y <= groundLevel)
        {
            Land();
        }
    }

    // Toca el suelo: si es explosiva, Explode() ya resuelve todo (daño en área + gomita + destrucción).
    // Si no, aparece el prefab de inmediato, sin ningún delay.
    private void Land()
    {
        if (isExplosive)
        {
            Explode();
            return;
        }

        if (gummyPrefab != null)
            Instantiate(gummyPrefab, transform.position + (Vector3.up * spawnOffset), transform.rotation);

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (gameManager != null)
            gameManager.onChangeGameState -= OnChangeGameStateCallback;
    }

    private void OnDrawGizmosSelected()
    {
        if (!isExplosive) return;

        // Relleno semitransparente para ver el área cubierta de un vistazo
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.15f);
        Gizmos.DrawSphere(transform.position, sizeOfExplosion);

        // Contorno sólido para distinguir el borde exacto del radio
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, sizeOfExplosion);
    }
}