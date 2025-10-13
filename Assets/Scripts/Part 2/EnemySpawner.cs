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

    [Tooltip("Assign in Inspector: WaveCountdownUI component for displaying countdown.")]
    public WaveCountdownUI waveCountdownUI;

    [Tooltip("Assign in Inspector: Prefabs for enemy types.")]
    public GameObject defaultEnemyPrefab;
    public GameObject fastEnemyPrefab;
    public GameObject tankEnemyPrefab;
    public GameObject bomberEnemyPrefab;

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

    [Header("Wave Timer")]
    [Tooltip("Time to wait between waves (seconds).")]
    public float waveDelay = 10f;

    [Header("Adaptive Scaling")]
    [Tooltip("Reference to the performance tracker for adaptive difficulty")]
    public PlayerPerformanceTracker performanceTracker;

    [Tooltip("How much player performance affects enemy count (0.5 = 50% influence)")]
    public float performanceCountMultiplier = 0.5f;

    [Tooltip("Maximum enemy count multiplier")]
    public float maxEnemyCountMultiplier = 3.0f;

    [Tooltip("Minimum enemy count multiplier")]
    public float minEnemyCountMultiplier = 0.3f;

    [Tooltip("How much player performance affects enemy stats (0.3 = 30% influence)")]
    public float performancePowerMultiplier = 0.3f;

    [Tooltip("Maximum stat multiplier")]
    public float maxStatMultiplier = 2.5f;

    [Tooltip("Minimum stat multiplier")]
    public float minStatMultiplier = 0.4f;


    private int currentWave = 0; // Start at 0 so first wave is 1
    private int enemiesRemainingInWave;
    private bool isSpawning = false;
    private List<GameObject> activeEnemies = new List<GameObject>();

    void Start()
    {
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();

        // Find performance tracker if not assigned
        if (performanceTracker == null)
        {
            performanceTracker = FindFirstObjectByType<PlayerPerformanceTracker>();
            if (performanceTracker == null)
            {
                Debug.LogWarning("EnemySpawner: No PlayerPerformanceTracker found! Adaptive scaling will be disabled.");
            }
        }

        // Check if enemy prefabs are assigned
        if (defaultEnemyPrefab == null)
        {
            Debug.LogError("EnemySpawner: No enemy prefabs assigned! Please assign at least defaultEnemyPrefab in the Inspector.");
            enabled = false;
            return;
        }

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
    /// Starts the next wave with a delay timer.
    /// </summary>
    IEnumerator StartNextWaveWithDelay()
    {
        Debug.Log($"Wave {currentWave} complete! Next wave starting in {waveDelay} seconds...");
        
        // Start the countdown UI
        if (waveCountdownUI != null)
        {
            waveCountdownUI.StartCountdown(waveDelay);
        }
        
        // Wait for the delay
        yield return new WaitForSeconds(waveDelay);
        
        // Stop the countdown UI
        if (waveCountdownUI != null)
        {
            waveCountdownUI.StopCountdown();
        }
        
        StartNextWave();
    }

    /// <summary>
    /// Starts the next wave with scaled enemy count and stats.
    /// </summary>
    public void StartNextWave()
    {
        if (isSpawning) return;

        currentWave++;
        
        // Notify performance tracker of wave start
        if (performanceTracker != null)
        {
            performanceTracker.OnWaveStart();
        }
        
        // Calculate adaptive enemy count
        enemiesRemainingInWave = CalculateAdaptiveEnemyCount();
        isSpawning = true;

        // Update GameManager's wave counter for UI display
        if (gameManager != null)
        {
            gameManager.currentWave = currentWave;
            
            // Update critical hit system with new wave
            if (gameManager.criticalHitSystem != null)
            {
                gameManager.criticalHitSystem.SetCurrentWave(currentWave);
            }
        }

        Debug.Log($"Starting Wave {currentWave} with {enemiesRemainingInWave} enemies (Adaptive scaling: {GetPerformanceLevel()}).");
        StartCoroutine(SpawnWave());
    }

    /// <summary>
    /// Spawns enemies in the current wave at regular intervals.
    /// </summary>
    IEnumerator SpawnWave()
    {
        int enemiesSpawned = 0;
        Debug.Log($"Starting to spawn {enemiesRemainingInWave} enemies for Wave {currentWave}");
        
        while (enemiesSpawned < enemiesRemainingInWave)
        {
            SpawnRandomEnemy();
            enemiesSpawned++;
            Debug.Log($"Spawned enemy {enemiesSpawned}/{enemiesRemainingInWave} for Wave {currentWave}");
            yield return new WaitForSeconds(spawnInterval);
        }

        isSpawning = false;
        Debug.Log($"Wave {currentWave} spawning complete. Waiting for all enemies to die... Active enemies: {activeEnemies.Count}");
    }

    /// <summary>
    /// Spawns a random enemy type at a random path entrance.
    /// Uses adaptive enemy selection based on player performance.
    /// </summary>
    void SpawnRandomEnemy()
    {
        // Choose enemy type based on performance and wave progression
        GameObject enemyPrefab = SelectEnemyTypeWithAdaptiveScaling();

        // Get a random path and spawn point
        List<List<Vector3Int>> paths = gameManager.terrainGenerator.GetPaths();
        if (paths.Count == 0)
        {
            Debug.LogError("No paths available for spawning enemies!");
            return;
        }

        List<Vector3Int> randomPath = paths[Random.Range(0, paths.Count)];
        Vector3 spawnPosition = new Vector3(randomPath[0].x, gameManager.terrainGenerator.height, randomPath[0].z);
        
        // Special spawn height for tank enemies (more important, spawn higher)
        if (enemyPrefab == tankEnemyPrefab)
        {
            spawnPosition.y += 2f; // Spawn 2 units higher for more importance
            Debug.Log("Tank Enemy spawning with elevated importance!");
        }

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

        // Apply adaptive scaling to enemy stats
        ApplyAdaptiveScaling(enemy);

        // Initialize the enemy
        Tower towerComponent = gameManager.TowerInstance != null ? gameManager.TowerInstance.GetComponent<Tower>() : null;
        if (towerComponent == null)
        {
            Debug.LogError("Tower instance or Tower component is not available!");
        }

        enemy.Initialize(randomPath, gameManager.terrainGenerator.height, towerComponent, gameManager);

        activeEnemies.Add(enemyObject);
        
        // Notify performance tracker of enemy spawn
        if (performanceTracker != null)
        {
            performanceTracker.OnEnemySpawned();
        }
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
            // Mid waves: introduce bomber enemies (wave 3+) and tank enemies
            float rand = Random.Range(0f, 1f);
            if (rand < 0.4f) return defaultEnemyPrefab;
            else if (rand < 0.6f) return fastEnemyPrefab;
            else if (rand < 0.8f) return bomberEnemyPrefab; // Bomber introduced at wave 3
            else return tankEnemyPrefab;
        }
        else
        {
            // Late waves: all available types including bombers
            float rand = Random.Range(0f, 1f);
            if (rand < 0.3f) return defaultEnemyPrefab;
            else if (rand < 0.5f) return fastEnemyPrefab;
            else if (rand < 0.7f) return bomberEnemyPrefab;
            else return tankEnemyPrefab;
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

        // Notify performance tracker of enemy kill
        if (performanceTracker != null)
        {
            performanceTracker.OnEnemyKilled();
        }

        // Check if wave is complete and no enemies are left
        if (!isSpawning && activeEnemies.Count == 0)
        {
            Debug.Log($"All enemies in Wave {currentWave} defeated! Active enemies: {activeEnemies.Count}");
            
            // Notify performance tracker of wave completion
            if (performanceTracker != null)
            {
                performanceTracker.OnWaveComplete();
            }
            
            StartCoroutine(StartNextWaveWithDelay()); // Start the next wave after delay
        }
    }

    /// <summary>
    /// Calculates adaptive enemy count based on player performance
    /// </summary>
    int CalculateAdaptiveEnemyCount()
    {
        float baseCount = initialWaveEnemyCount * Mathf.Pow(waveScalingFactor, currentWave - 1);
        
        if (performanceTracker == null)
        {
            Debug.Log($"ENEMY COUNT: No performance tracker - using base count: {baseCount:F1}");
            return Mathf.RoundToInt(baseCount);
        }
        
        float performanceScore = performanceTracker.performanceScore;
        
        // If player doing well (high score), spawn MORE enemies
        // If player struggling (low score), spawn FEWER enemies
        float performanceMultiplier = 1.0f + (performanceScore - 50f) / 100f * performanceCountMultiplier;
        
        // Clamp the multiplier
        performanceMultiplier = Mathf.Clamp(performanceMultiplier, minEnemyCountMultiplier, maxEnemyCountMultiplier);
        
        int adaptiveCount = Mathf.RoundToInt(baseCount * performanceMultiplier);
        
        // Clear, player-friendly adaptive scaling messages
        if (performanceScore >= 70f)
        {
            Debug.Log($"ADAPTIVE SCALING: Spawning {adaptiveCount} enemies (more than usual) because you're doing GREAT!");
        }
        else if (performanceScore >= 50f)
        {
            Debug.Log($"ADAPTIVE SCALING: Spawning {adaptiveCount} enemies (slightly more) because you're doing well!");
        }
        else if (performanceScore >= 30f)
        {
            Debug.Log($"ADAPTIVE SCALING: Spawning {adaptiveCount} enemies (standard amount) for balanced challenge");
        }
        else
        {
            Debug.Log($"ADAPTIVE SCALING: Spawning {adaptiveCount} enemies (fewer than usual) to help you succeed!");
        }
        
        return adaptiveCount;
    }

    /// <summary>
    /// Selects enemy type with adaptive scaling based on player performance
    /// </summary>
    GameObject SelectEnemyTypeWithAdaptiveScaling()
    {
        if (performanceTracker == null)
        {
            Debug.Log($"ENEMY TYPE: No performance tracker - using wave-based selection");
            return SelectEnemyTypeBasedOnWave();
        }
        
        float performanceScore = performanceTracker.performanceScore;
        
        // If player doing well, spawn harder enemies more frequently
        if (performanceScore > 70f && currentWave > 3)
        {
            // 30% chance to spawn tank enemy for high-performing players
            if (Random.Range(0f, 1f) < 0.3f)
            {
                Debug.Log($"ADAPTIVE SCALING: Spawning TANK ENEMY because you're doing GREAT and need a real challenge!");
                return tankEnemyPrefab;
            }
        }
        
        // Spawn bombers more frequently for high-performing players (wave 3+)
        if (performanceScore > 60f && currentWave >= 3)
        {
            float bomberChance = 0.2f + (performanceScore - 60f) / 40f * 0.3f; // 20% to 50% chance
            if (Random.Range(0f, 1f) < bomberChance)
            {
                Debug.Log($"ADAPTIVE SCALING: Spawning BOMBER because you're doing well and can handle the challenge!");
                return bomberEnemyPrefab;
            }
        }
        
        // If player struggling, spawn easier enemies
        if (performanceScore < 30f)
        {
            Debug.Log($"ADAPTIVE SCALING: Spawning basic enemy only because you're struggling - let's help you learn!");
            return defaultEnemyPrefab; // Only basic enemies
        }
        
        // Adaptive fast enemy frequency for high-performing players
        if (performanceScore > 60f)
        {
            float fastEnemyChance = 0.3f + (performanceScore - 60f) / 40f * 0.4f; // 30% to 70% chance
            if (Random.Range(0f, 1f) < fastEnemyChance)
            {
                Debug.Log($"ADAPTIVE SCALING: Spawning FAST ENEMY because you're doing well and can handle speed!");
                return fastEnemyPrefab;
            }
        }
        
        // Otherwise use normal wave-based selection
        return SelectEnemyTypeBasedOnWave();
    }


    /// <summary>
    /// Applies adaptive scaling to enemy stats
    /// </summary>
    void ApplyAdaptiveScaling(Enemy enemy)
    {
        if (performanceTracker == null)
        {
            // Fallback to wave-only scaling
            float originalMaxHealth = enemy.GetMaxHealth();
            enemy.SetMaxHealth(Mathf.RoundToInt(originalMaxHealth * Mathf.Pow(healthScalingFactor, currentWave - 1)));
            enemy.SetCurrentHealth(enemy.GetMaxHealth());
            enemy.SetMoveSpeed(enemy.GetMoveSpeed() * Mathf.Pow(speedScalingFactor, currentWave - 1));
            return;
        }
        
        float performanceScore = performanceTracker.performanceScore;
        
        // Calculate adaptive multipliers
        float healthMultiplier = 1.0f + (performanceScore - 50f) / 100f * performancePowerMultiplier;
        float speedMultiplier = 1.0f + (performanceScore - 50f) / 100f * performancePowerMultiplier;
        float damageMultiplier = 1.0f + (performanceScore - 50f) / 100f * performancePowerMultiplier;
        
        // Clamp multipliers
        healthMultiplier = Mathf.Clamp(healthMultiplier, minStatMultiplier, maxStatMultiplier);
        speedMultiplier = Mathf.Clamp(speedMultiplier, minStatMultiplier, maxStatMultiplier);
        damageMultiplier = Mathf.Clamp(damageMultiplier, minStatMultiplier, maxStatMultiplier);
        
        // Apply both wave scaling AND performance scaling
        float baseMaxHealth = enemy.GetMaxHealth();
        float finalHealth = baseMaxHealth * Mathf.Pow(healthScalingFactor, currentWave - 1) * healthMultiplier;
        float finalSpeed = enemy.GetMoveSpeed() * Mathf.Pow(speedScalingFactor, currentWave - 1) * speedMultiplier;
        float finalDamage = enemy.GetAttackDamage() * damageMultiplier;
        
        enemy.SetMaxHealth(Mathf.RoundToInt(finalHealth));
        enemy.SetCurrentHealth(enemy.GetMaxHealth());
        enemy.SetMoveSpeed(finalSpeed);
        enemy.SetAttackDamage(finalDamage);
        
        // Adaptive scaling applied
        
        // Clear, player-friendly stat scaling messages
        if (performanceScore >= 70f)
        {
            Debug.Log($"ADAPTIVE SCALING: Making enemies STRONGER because you're doing GREAT!");
        }
        else if (performanceScore < 30f)
        {
            Debug.Log($"ADAPTIVE SCALING: Making enemies WEAKER to help you succeed!");
        }
    }


    
    /// <summary>
    /// Gets the current performance level for debug display
    /// </summary>
    string GetPerformanceLevel()
    {
        if (performanceTracker == null) return "No Tracker";
        return performanceTracker.GetPerformanceLevel();
    }
}
