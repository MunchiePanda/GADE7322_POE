using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles spawning enemies in waves with random types and scaling difficulty.
/// Attach to the GameManager or a dedicated SpawnManager GameObject.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Assign in Inspector: GameManager instance.")]
    public GameManager gameManager;

    [Tooltip("Assign in Inspector: Prefabs for enemy types.")]
    public GameObject defaultEnemyPrefab;
    public GameObject fastEnemyPrefab;
    public GameObject tankEnemyPrefab;
    public GameObject kamikazeDragonPrefab;
    public GameObject armoredDragonPrefab;

    [Header("Spawning Settings")]
    [Tooltip("Time between enemy spawns in a wave (seconds).")]
    public float spawnInterval = 2f;

    [Tooltip("Number of enemies to spawn in the first wave.")]
    public int initialWaveEnemyCount = 5;

    [Tooltip("Factor by which enemy count increases per wave.")]
    public float waveScalingFactor = 1.5f;

    [Tooltip("Factor by which enemy health increases per wave.")]
    public float healthScalingFactor = 1.2f;

    [Tooltip("Factor by which enemy speed increases per wave.")]
    public float speedScalingFactor = 1.1f;

    private int currentWave = 1;
    private int enemiesRemainingInWave;
    private bool isSpawning = false;
    private List<GameObject> activeEnemies = new List<GameObject>();

    void Start()
    {
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();

        StartCoroutine(StartFirstWave());
    }

    /// <summary>
    /// Starts the first wave after a delay.
    /// </summary>
    IEnumerator StartFirstWave()
    {
        yield return new WaitForSeconds(3f); // Initial delay
        StartNextWave();
    }

    /// <summary>
    /// Starts the next wave with scaled enemy count and stats.
    /// </summary>
    public void StartNextWave()
    {
        if (isSpawning) return;

        currentWave++;
        enemiesRemainingInWave = Mathf.RoundToInt(initialWaveEnemyCount * Mathf.Pow(waveScalingFactor, currentWave - 1));
        isSpawning = true;

        Debug.Log($"Starting Wave {currentWave} with {enemiesRemainingInWave} enemies.");
        StartCoroutine(SpawnWave());
    }

    /// <summary>
    /// Spawns enemies in the current wave at regular intervals.
    /// </summary>
    IEnumerator SpawnWave()
    {
        int enemiesSpawned = 0;
        while (enemiesSpawned < enemiesRemainingInWave)
        {
            SpawnRandomEnemy();
            enemiesSpawned++;
            yield return new WaitForSeconds(spawnInterval);
        }

        isSpawning = false;
        Debug.Log($"Wave {currentWave} complete. Waiting for all enemies to die...");
    }

    /// <summary>
    /// Spawns a random enemy type at a random path entrance.
    /// Uses wave-based enemy selection for better progression.
    /// </summary>
    void SpawnRandomEnemy()
    {
        // Choose enemy type based on wave progression
        GameObject enemyPrefab = SelectEnemyTypeBasedOnWave();

        // Get a random path and spawn point
        List<List<Vector3Int>> paths = gameManager.terrainGenerator.GetPaths();
        if (paths.Count == 0)
        {
            Debug.LogError("No paths available for spawning enemies!");
            return;
        }

        List<Vector3Int> randomPath = paths[Random.Range(0, paths.Count)];
        Vector3 spawnPosition = new Vector3(randomPath[0].x, gameManager.terrainGenerator.height, randomPath[0].z);

        // Instantiate the enemy
        if (enemyPrefab == null)
        {
            Debug.LogError("Enemy prefab is not assigned!");
            return;
        }

        GameObject enemyObject = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        Enemy enemy = enemyObject.GetComponent<Enemy>();

        if (enemy == null)
        {
            Debug.LogError("Instantiated object does not have an Enemy component!");
            Destroy(enemyObject);
            return;
        }

        // Scale stats based on wave
        float originalMaxHealth = enemy.GetMaxHealth();
        enemy.SetMaxHealth(Mathf.RoundToInt(originalMaxHealth * Mathf.Pow(healthScalingFactor, currentWave - 1)));
        enemy.SetCurrentHealth(enemy.GetMaxHealth());
        enemy.SetMoveSpeed(enemy.GetMoveSpeed() * Mathf.Pow(speedScalingFactor, currentWave - 1));

        // Initialize the enemy
        Tower towerComponent = gameManager.TowerInstance != null ? gameManager.TowerInstance.GetComponent<Tower>() : null;
        if (towerComponent == null)
        {
            Debug.LogError("Tower instance or Tower component is not available!");
        }

        enemy.Initialize(randomPath, gameManager.terrainGenerator.height, towerComponent, gameManager);

        activeEnemies.Add(enemyObject);
    }

    /// <summary>
    /// Selects enemy type based on current wave for better progression.
    /// </summary>
    GameObject SelectEnemyTypeBasedOnWave()
    {
        if (currentWave <= 2)
        {
            // Early waves: mostly default enemies
            return Random.Range(0f, 1f) < 0.8f ? defaultEnemyPrefab : fastEnemyPrefab;
        }
        else if (currentWave <= 5)
        {
            // Mid waves: introduce kamikaze dragons
            float rand = Random.Range(0f, 1f);
            if (rand < 0.5f) return defaultEnemyPrefab;
            else if (rand < 0.7f) return fastEnemyPrefab;
            else if (rand < 0.9f) return tankEnemyPrefab;
            else return kamikazeDragonPrefab;
        }
        else
        {
            // Late waves: all types including armored dragons
            float rand = Random.Range(0f, 1f);
            if (rand < 0.25f) return defaultEnemyPrefab;
            else if (rand < 0.4f) return fastEnemyPrefab;
            else if (rand < 0.55f) return tankEnemyPrefab;
            else if (rand < 0.75f) return kamikazeDragonPrefab;
            else return armoredDragonPrefab;
        }
    }

    /// <summary>
    /// Called by Enemy.onDeath to remove from active list and check wave completion.
    /// </summary>
    public void OnEnemyDeath(GameObject enemy)
    {
        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
        }

        // Check if wave is complete and no enemies are left
        if (!isSpawning && activeEnemies.Count == 0)
        {
            Debug.Log($"All enemies in Wave {currentWave} defeated!");
            StartNextWave(); // Start the next wave
        }
    }
}
