using UnityEngine;
using TMPro;

public class RoundManager : MonoBehaviour
{
    [Header("Pools de Enemigos")]
    [SerializeField] private ObjectPool basicEnemyPool;
    [SerializeField] private ObjectPool explosiveEnemyPool;

    [Header("Referencias")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private GameObject experienceOrbPrefab;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float spawnRadius = 3f;           // área alrededor del spawn point
    [SerializeField] private float enemyClearance = 0.8f;      // espacio libre mínimo entre enemigos
    [SerializeField] private LayerMask enemyLayer;             // layer "Enemy"
    [SerializeField] private int maxPlacementAttempts = 10;
    [SerializeField] private float waveSpawnInterval = 0.15f;  // pausa entre cada enemigo de la oleada

    private int pendingWaveSpawns;
    private int waveSpawnIndex;
    private float waveSpawnTimer;

    [Header("Estado de Rondas")]
    [Tooltip("Rondas completadas exitosamente por el jugador.")]
    [SerializeField] private int completedRounds = 0;
    public int CompletedRounds => completedRounds;
    [SerializeField] private TextMeshProUGUI roundText;

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

        UpdateWaveSpawning();
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

        int enemiesToSpawn = initialWaveSize + (completedRounds * enemiesIncreasePerRound);
        waveEnemiesRemaining = enemiesToSpawn;

        // En lugar de spawnear todo aquí, se encola
        pendingWaveSpawns = enemiesToSpawn;
        waveSpawnIndex = 0;
        waveSpawnTimer = 0f;

        Debug.Log($"<color=cyan>[RoundManager] Iniciando Ronda {completedRounds + 1}. Enemigos de oleada: {enemiesToSpawn}</color>");
        UpdateRoundUI();
    }
    private void UpdateRoundUI()
    {
        if (roundText != null)
            roundText.text = $"Ronda: {completedRounds + 1}";
    }

    private void UpdateWaveSpawning()
    {
        if (pendingWaveSpawns <= 0) return;

        waveSpawnTimer -= Time.deltaTime;
        if (waveSpawnTimer > 0f) return;

        waveSpawnTimer = waveSpawnInterval;

        Transform spawnPoint = spawnPoints[waveSpawnIndex % spawnPoints.Length];
        SpawnEnemy(spawnPoint.position, spawnPoint.rotation, isWaveEnemy: true);

        waveSpawnIndex++;
        pendingWaveSpawns--;
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

        TryGetFreePosition(position, out Vector3 freePosition);

        GameObject enemyObj = selectedPool.Get(freePosition, rotation, isWaveEnemy);
        if (enemyObj == null) return;

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
            enemy.Teleport(freePosition);   // ver nota abajo
        }
    }

    private bool TryGetFreePosition(Vector3 center, out Vector3 result)
    {
        Physics.SyncTransforms(); // asegura que los enemigos recién colocados cuenten en la comprobación

        for (int i = 0; i < maxPlacementAttempts; i++)
        {
            Vector2 offset = Random.insideUnitCircle * spawnRadius;
            Vector3 candidate = center + new Vector3(offset.x, 0f, offset.y);

            // Debe caer sobre el NavMesh
            if (!UnityEngine.AI.NavMesh.SamplePosition(candidate, out UnityEngine.AI.NavMeshHit hit, 2f, UnityEngine.AI.NavMesh.AllAreas))
                continue;

            // Debe estar libre de otros enemigos
            if (Physics.CheckSphere(hit.position, enemyClearance, enemyLayer))
                continue;

            result = hit.position;
            return true;
        }

        // Si no encontró hueco, usa el punto original (mejor eso que no spawnear)
        result = center;
        return false;
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