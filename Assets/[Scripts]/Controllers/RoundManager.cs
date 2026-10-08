using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private ObjectPool enemyPool;
    [SerializeField] private Transform[] spawnPoints;

    [Header("Configuración de Oleadas")]
    [SerializeField] private int initialWaveSize = 10;

    [SerializeField] private int enemiesIncreasePerRound = 5;

    [SerializeField] private float gracePeriodDuration = 5f;

    [Header("Enemigos Periódicos (Background)")]
    [SerializeField] private float periodicSpawnInterval = 5f;

    private int currentRound = 0;
    private int waveEnemiesRemaining = 0;
    private bool isGracePeriod = false;
    private Coroutine periodicSpawnCoroutine;

    private void Start()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("[RoundManager] No hay spawn points configurados.", this);
            return;
        }

        StartNextRound();
        periodicSpawnCoroutine = StartCoroutine(PeriodicSpawnRoutine());
    }

    private void StartNextRound()
    {
        currentRound++;
        isGracePeriod = false;

        // Cálculo de enemigos para la oleada principal de esta ronda
        int enemiesToSpawn = initialWaveSize + (currentRound - 1) * enemiesIncreasePerRound;
        waveEnemiesRemaining = enemiesToSpawn;

        Debug.Log($"<color=cyan>[RoundManager] Iniciando Ronda {currentRound}. Enemigos de oleada: {enemiesToSpawn}</color>");

        // Distribución equitativa entre los spawns disponible
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            Transform spawnPoint = spawnPoints[i % spawnPoints.Length];
            SpawnEnemy(spawnPoint.position, spawnPoint.rotation, isWaveEnemy: true);
        }
    }

//Bug muy probable cuando el juego este en pausa
    private IEnumerator PeriodicSpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(periodicSpawnInterval);

            // Opcional: no spawnear extras durante el tiempo de gracia
            if (isGracePeriod) continue;

            Transform randomSpawn = spawnPoints[Random.Range(0, spawnPoints.Length)];
            SpawnEnemy(randomSpawn.position, randomSpawn.rotation, isWaveEnemy: false);
        }
    }

    private void SpawnEnemy(Vector3 position, Quaternion rotation, bool isWaveEnemy)
    {
        GameObject enemyObj = enemyPool.Get(position, rotation);
        if (enemyObj == null) return;

        PooledEnemy enemy = enemyObj.GetComponent<PooledEnemy>();
        if (enemy == null)
        {
            Debug.LogError("[RoundManager] El prefab del pool debe contener el script 'PooledEnemy'.", enemyObj);
            return;
        }

        enemy.Setup(enemyPool, isWaveEnemy);
        enemy.OnEnemyDeath += HandleEnemyDeath;
    }

    private void HandleEnemyDeath(PooledEnemy enemy)
    {
        // Desuscribir el evento para evitar fugas de memoria o múltiples llamadas
        enemy.OnEnemyDeath -= HandleEnemyDeath;

        // Los enemigos del temporizador de 5s no cuentan para terminar la ronda
        if (!enemy.IsWaveEnemy) return;

        waveEnemiesRemaining--;
        Debug.Log($"[RoundManager] Enemigo de oleada eliminado. Restantes: {waveEnemiesRemaining}");

        if (waveEnemiesRemaining <= 0 && !isGracePeriod)
        {
            StartCoroutine(GracePeriodRoutine());
        }
    }

    private IEnumerator GracePeriodRoutine()
    {
        isGracePeriod = true;
        Debug.Log($"<color=green>[RoundManager] ¡Ronda {currentRound} completada! Tiempo de gracia: {gracePeriodDuration}s...</color>");

        yield return new WaitForSeconds(gracePeriodDuration);

        StartNextRound();
    }

    private void OnDestroy()
    {
        if (periodicSpawnCoroutine != null)
        {
            StopCoroutine(periodicSpawnCoroutine);
        }
    }
}