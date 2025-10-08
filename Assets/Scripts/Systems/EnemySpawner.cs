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
    public GameObject berserkerEnemyPrefab;
    public GameObject swarmEnemyPrefab;

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
    /// Includes wave-based enemy type weighting for progression.
    /// </summary>
    void SpawnRandomEnemy()
    {
        // Choose enemy type based on wave progression
        GameObject enemyPrefab = ChooseEnemyType();
        bool isSwarmEnemy = (enemyPrefab == swarmEnemyPrefab);

        // Get a random path and spawn point
        List<List<Vector3Int>> paths = gameManager.terrainGenerator.GetPaths();
        if (paths.Count == 0)
        {
            Debug.LogError("No paths available for spawning enemies!");
            return;
        }

        List<Vector3Int> randomPath = paths[Random.Range(0, paths.Count)];
        Vector3 spawnPosition = new Vector3(randomPath[0].x, gameManager.terrainGenerator.height, randomPath[0].z);

        // Handle swarm enemies differently (spawn multiple)
        if (isSwarmEnemy && swarmEnemyPrefab != null)
        {
            SpawnSwarmEnemies(spawnPosition, randomPath);
            return;
        }

        // Spawn single enemy (normal case)
        SpawnSingleEnemy(enemyPrefab, spawnPosition, randomPath);
    }

    /// <summary>
    /// Chooses enemy type based on current wave with weighted probabilities.
    /// </summary>
    GameObject ChooseEnemyType()
    {
        // Calculate weights based on current wave
        float[] weights = new float[5];
        weights[0] = GetEnemyTypeWeight(0, currentWave); // Default
        weights[1] = GetEnemyTypeWeight(1, currentWave); // Fast
        weights[2] = GetEnemyTypeWeight(2, currentWave); // Tank
        weights[3] = GetEnemyTypeWeight(3, currentWave); // Berserker
        weights[4] = GetEnemyTypeWeight(4, currentWave); // Swarm

        // Choose based on weights
        int chosenType = ChooseWeightedRandom(weights);
        
        switch (chosenType)
        {
            case 1: return fastEnemyPrefab;
            case 2: return tankEnemyPrefab;
            case 3: return berserkerEnemyPrefab;
            case 4: return swarmEnemyPrefab;
            default: return defaultEnemyPrefab;
        }
    }

    /// <summary>
    /// Returns the spawn weight for a specific enemy type based on wave number.
    /// </summary>
    float GetEnemyTypeWeight(int enemyType, int waveNumber)
    {
        switch (enemyType)
        {
            case 0: return waveNumber < 4 ? 0.6f : 0.3f; // Default enemies common early
            case 1: return waveNumber > 2 ? 0.3f : 0.1f; // Fast enemies from wave 3
            case 2: return waveNumber > 4 ? 0.3f : 0.05f; // Tank enemies from wave 5
            case 3: return waveNumber > 3 ? 0.2f : 0f; // Berserkers from wave 4
            case 4: return waveNumber > 5 ? 0.3f : 0f; // Swarms from wave 6
            default: return 0.2f;
        }
    }

    /// <summary>
    /// Chooses a random index based on weights.
    /// </summary>
    int ChooseWeightedRandom(float[] weights)
    {
        float totalWeight = 0f;
        for (int i = 0; i < weights.Length; i++)
        {
            totalWeight += weights[i];
        }

        if (totalWeight <= 0f) return 0; // Fallback to default

        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        for (int i = 0; i < weights.Length; i++)
        {
            currentWeight += weights[i];
            if (randomValue <= currentWeight)
            {
                return i;
            }
        }

        return 0; // Fallback
    }

    /// <summary>
    /// Spawns multiple swarm enemies with slight position offsets.
    /// </summary>
    void SpawnSwarmEnemies(Vector3 basePosition, List<Vector3Int> path)
    {
        int swarmCount = SwarmEnemy.swarmSize;
        
        for (int i = 0; i < swarmCount; i++)
        {
            Vector3 offset = new Vector3(
                Random.Range(-1.5f, 1.5f), 
                0, 
                Random.Range(-1.5f, 1.5f)
            );
            
            Vector3 spawnPosition = basePosition + offset;
            SpawnSingleEnemy(swarmEnemyPrefab, spawnPosition, path);
        }
    }

    /// <summary>
    /// Spawns a single enemy and initializes it.
    /// </summary>
    void SpawnSingleEnemy(GameObject enemyPrefab, Vector3 spawnPosition, List<Vector3Int> path)
    {
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

        enemy.Initialize(path, gameManager.terrainGenerator.height, towerComponent, gameManager);

        activeEnemies.Add(enemyObject);
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
            
            StrategicWaveManager waveManager = FindFirstObjectByType<StrategicWaveManager>();
            if (waveManager != null)
            {
                waveManager.OnWaveComplete();
            }
            else
            {
                StartNextWave();
            }
        }
    }
}
