using UnityEngine;

public class RoundManager : MonoBehaviour
{
    [Header("Pools de Enemigos")]
    [SerializeField] private ObjectPool basicEnemyPool;
    [SerializeField] private ObjectPool explosiveEnemyPool;

    [Header("Referencias")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private GameObject experienceOrbPrefab;
    [SerializeField] private Transform playerTransform;

    [Header("Estado de Rondas")]
    [Tooltip("Rondas completadas exitosamente por el jugador.")]
    [SerializeField] private int completedRounds = 0;
    public int CompletedRounds => completedRounds;

    [Header("Configuración de Oleadas")]
    [SerializeField] private int initialWaveSize = 10;
    [SerializeField] private int enemiesIncreasePerRound = 5;
    [SerializeField] private float gracePeriodDuration = 5f;

    [Header("Enemigos Periódicos (Background)")]
    [SerializeField] private float periodicSpawnInterval = 5f;

    [Header("Configuración de Enemigos")]
    [SerializeField]
    [Range(0f, 1f)]
    private float explosiveEnemyChance = 0.25f;

    [SerializeField]
    private int explosiveEnemyUnlockRound = 4;

    private int waveEnemiesRemaining = 0;
    private bool isGracePeriod = false;

    // --- Timers (reemplazan a las corrutinas) ---
    private float periodicSpawnTimer;
    private float graceTimer;
    private bool isInitialized;   // true solo si Start pasó la validación de spawn points

    // --- Pausa ---
    private GameManager gameManager;
    private bool isGamePaused;

    private static RoundManager Instance;

    public static RoundManager GetInstance()
    {
        return Instance;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Suscripción en Start (y no en OnEnable) para asegurar que GameManager.Instance ya existe
        gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            gameManager.onChangeGameState += OnChangeGameStateCallback;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("[RoundManager] No hay spawn points configurados.", this);
            return;
        }

        isInitialized = true;
        periodicSpawnTimer = periodicSpawnInterval;
        StartNextRound();
    }

    private void OnChangeGameStateCallback(GameState newState)
    {
        isGamePaused = newState == GameState.Pause;
    }

    private void Update()
    {
        if (!isInitialized || isGamePaused)
            return;

        UpdatePeriodicSpawn();
        UpdateGracePeriod();
    }

    // ---------------- SPAWN PERIÓDICO ----------------

    private void UpdatePeriodicSpawn()
    {
        periodicSpawnTimer -= Time.deltaTime;
        if (periodicSpawnTimer > 0f) return;

        // Se reinicia siempre, igual que el bucle original
        // (si estaba en gracia, simplemente se salta ese spawn y espera otro intervalo)
        periodicSpawnTimer = periodicSpawnInterval;

        if (isGracePeriod) return;

        Transform randomSpawn = spawnPoints[Random.Range(0, spawnPoints.Length)];
        SpawnEnemy(randomSpawn.position, randomSpawn.rotation, isWaveEnemy: false);
    }

    // ---------------- PERIODO DE GRACIA ----------------

    private void UpdateGracePeriod()
    {
        if (!isGracePeriod) return;

        graceTimer -= Time.deltaTime;
        if (graceTimer <= 0f)
        {
            StartNextRound();
        }
    }

    private void StartGracePeriod()
    {
        isGracePeriod = true;
        graceTimer = gracePeriodDuration;

        // SE TERMINÓ LA RONDA: Aquí se incrementa la variable serializada
        completedRounds++;
        Debug.Log($"<color=green>[RoundManager] ¡Ronda terminada! Total completadas: {completedRounds}. Tiempo de gracia: {gracePeriodDuration}s...</color>");
    }

    // ---------------- RONDAS ----------------

    private void StartNextRound()
    {
        isGracePeriod = false;

        // La dificultad escala en función de las rondas ya completadas
        int enemiesToSpawn = initialWaveSize + (completedRounds * enemiesIncreasePerRound);
        waveEnemiesRemaining = enemiesToSpawn;

        Debug.Log($"<color=cyan>[RoundManager] Iniciando Ronda {completedRounds + 1}. Enemigos de oleada: {enemiesToSpawn}</color>");

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            Transform spawnPoint = spawnPoints[i % spawnPoints.Length];
            SpawnEnemy(spawnPoint.position, spawnPoint.rotation, isWaveEnemy: true);
        }
    }

    private ObjectPool GetEnemyPool()
    {
        int currentRound = completedRounds + 1;

        if (currentRound >= explosiveEnemyUnlockRound)
        {
            if (Random.value <= explosiveEnemyChance)
            {
                return explosiveEnemyPool;
            }
        }

        return basicEnemyPool;
    }

    private void SpawnEnemy(Vector3 position, Quaternion rotation, bool isWaveEnemy)
    {
        ObjectPool selectedPool = GetEnemyPool();

        if (selectedPool == null)
        {
            Debug.LogError("[RoundManager] No se encontró un ObjectPool.");
            return;
        }

        GameObject enemyObj = selectedPool.Get(position, rotation, isWaveEnemy);

        if (enemyObj == null)
            return;

        PooledEnemy pooledComp = enemyObj.GetComponent<PooledEnemy>();

        if (pooledComp != null)
        {
            pooledComp.OnEnemyDeath -= HandleEnemyDeath;
            pooledComp.OnEnemyDeath += HandleEnemyDeath;
        }

        Enemy enemy = enemyObj.GetComponent<Enemy>();

        if (enemy != null)
        {
            enemy.Initialize(experienceOrbPrefab, playerTransform);
        }
    }

    private void HandleEnemyDeath(PooledEnemy enemy)
    {
        enemy.OnEnemyDeath -= HandleEnemyDeath;

        if (!enemy.IsWaveEnemy) return;

        waveEnemiesRemaining--;

        if (waveEnemiesRemaining <= 0 && !isGracePeriod)
        {
            StartGracePeriod();
        }
    }

    private void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.onChangeGameState -= OnChangeGameStateCallback;
        }
    }
}