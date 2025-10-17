using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GADE7322_POE.UI;



public class GameManager : MonoBehaviour
{
   

    [Header("References")]
    [Tooltip("Reference to the VoxelTerrainGenerator, which generates the game terrain.")]
    public VoxelTerrainGenerator terrainGenerator;
    [Tooltip("Prefab for the central tower, which the player must defend.")]
    public GameObject towerPrefab;
    [Tooltip("Prefab for the default enemy type.")]
    public GameObject defaultEnemyPrefab;
    [Tooltip("Prefab for the fast enemy type.")]
    public GameObject fastEnemyPrefab;
    [Tooltip("Prefab for the tank enemy type (higher health).")]
    public GameObject tankEnemyPrefab;
    [Tooltip("Time delay (in seconds) between spawning enemies in a wave.")]
    public float enemySpawnInterval = 2f;
    [Tooltip("Number of enemies in the first wave.")]
    public int initialWaveEnemyCount = 5;
    [Tooltip("Multiplier for increasing the number of enemies per wave.")]
    public float waveScalingFactor = 1.5f;
    [Tooltip("Multiplier for increasing enemy health per wave.")]
    public float healthScalingFactor = 1.2f;
    [Tooltip("Multiplier for increasing enemy speed per wave.")]
    public float speedScalingFactor = 1.1f;
    [Tooltip("Time to wait between waves (seconds).")]
    public float waveDelay = 10f;

    [Header("Defenders & Resources")]
    [Tooltip("Prefab for the basic defender that the player can place on the terrain.")]
    public GameObject defenderPrefab;
    [Tooltip("Prefab for the frost tower defender.")]
    public GameObject frostTowerPrefab;
    [Tooltip("Prefab for the lightning tower defender.")]
    public GameObject lightningTowerPrefab;
    [Tooltip("Cost in resources to place a basic defender.")]
    public int defenderCost = 25;
    [Tooltip("Cost in resources to place a frost tower.")]
    public int frostTowerCost = 40;
    [Tooltip("Cost in resources to place a lightning tower.")]
    public int lightningTowerCost = 35;
    [Tooltip("Initial amount of resources the player starts with.")]
    public int startingResources = 100;
    
    [Header("Defender Limits")]
    [Tooltip("Maximum number of defenders allowed at once")]
    public int maxDefenderCount = 25;
    [Tooltip("Current number of active defenders")]
    public int currentDefenderCount = 0;

    [Header("Placement Offsets")]
    [Tooltip("Vertical offset for placing the tower (adjust based on tower model pivot).")]
    public float towerYOffset = 1f;
    [Tooltip("Vertical offset for spawning enemies (adjust based on enemy model pivot).")]
    public float enemyYOffset = 1f;
    [Tooltip("Vertical offset for placing defenders (adjust based on defender model pivot).")]
    public float defenderYOffset = 1f;

    // UI References
    [Tooltip("UI element for displaying the tower's health.")]
    public HealthBarUI towerHealthBar;
    [Tooltip("UI element for displaying the player's current resources.")]
    public ResourceCounterUI resourceCounterUI;
    [Tooltip("UI element for displaying the cost of placing a defender.")]
    public DefenderCostUI defenderCostUI;
    [Tooltip("UI panel for displaying the game over screen.")]
    public GameOverUI gameOverUI;
    [Tooltip("UI panel for pausing the game.")]
    public PauseMenuUI pauseMenuUI;
    [Tooltip("UI element for displaying the wave countdown timer.")]
    public WaveCountdownUI waveCountdownUI;
    [Tooltip("Critical hit system for enhanced combat feedback.")]
    public CriticalHitSystem criticalHitSystem;
    
    [Tooltip("Performance tracker for adaptive difficulty scaling.")]
    public PlayerPerformanceTracker performanceTracker;

    // Reference to the instantiated tower in the scene.
    private GameObject towerInstance;
    public GameObject TowerInstance { get { return towerInstance; } }

    // Current wave number (starts at 0, will be updated by EnemySpawner).
    public int currentWave = 0;

    // Current resources available to the player.
    private int resources = 0;

    // Flags to track game state.
    private bool isGameOver = false;
    private bool isPaused = false;

    // Reference to the EnemySpawner component.
    private EnemySpawner enemySpawner;

    // Reference to the Tower component.
    public Tower tower { get; private set; }

 

    void Start()
    {
        // Hide UI panels that should not be visible at the start.
        if (gameOverUI != null)
            gameOverUI.Hide();
        if (pauseMenuUI != null)
            pauseMenuUI.Hide();

        // Check if the terrain generator is assigned.
        if (terrainGenerator == null)
        {
            // Debug logging disabled
            return;
        }

        // Initialize player resources and update the UI.
        resources = Mathf.Max(0, startingResources);
        if (resourceCounterUI != null)
            resourceCounterUI.SetResource(resources);
        if (defenderCostUI != null)
            defenderCostUI.SetCost(defenderCost);
            
        // Initialize defender count
        CountCurrentDefenders();

        // Ensure the terrain is generated before placing the tower.
        if (!terrainGenerator.IsReady)
        {
            // Force terrain generation if it hasn't been done yet.
            terrainGenerator.GetCenterGrid();
        }
        SpawnTower();

        // Initialize or get the EnemySpawner component.
        enemySpawner = GetComponent<EnemySpawner>();
        if (enemySpawner == null)
        {
            enemySpawner = gameObject.AddComponent<EnemySpawner>();
        }

        // Configure the EnemySpawner with the necessary settings.
        enemySpawner.gameManager = this;
        enemySpawner.defaultEnemyPrefab = defaultEnemyPrefab;
        enemySpawner.fastEnemyPrefab = fastEnemyPrefab;
        enemySpawner.tankEnemyPrefab = tankEnemyPrefab;
        enemySpawner.spawnInterval = enemySpawnInterval;
        enemySpawner.initialWaveEnemyCount = initialWaveEnemyCount;
        enemySpawner.waveScalingFactor = waveScalingFactor;
        enemySpawner.healthScalingFactor = healthScalingFactor;
        enemySpawner.speedScalingFactor = speedScalingFactor;
        enemySpawner.waveDelay = waveDelay;
        enemySpawner.waveCountdownUI = waveCountdownUI;
        
        // Initialize critical hit system
        if (criticalHitSystem == null)
        {
            criticalHitSystem = gameObject.AddComponent<CriticalHitSystem>();
        }
        
        // Initialize performance tracker
        if (performanceTracker == null)
        {
            performanceTracker = GetComponent<PlayerPerformanceTracker>();
            if (performanceTracker == null)
            {
                performanceTracker = gameObject.AddComponent<PlayerPerformanceTracker>();
            }
        }
        
        // Connect performance tracker to enemy spawner
        if (enemySpawner != null && performanceTracker != null)
        {
            enemySpawner.performanceTracker = performanceTracker;
        }
    }

    void Update()
    {
        // Check for pause input (Escape key) using the new Input System.
        if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current[UnityEngine.InputSystem.Key.Escape].wasPressedThisFrame)
        {
            TogglePause();
        }
    }

   

    /// <summary>
    /// Spawns the central tower at the center of the terrain.
    /// </summary>
    void SpawnTower()
    {
        // Get the center grid position of the terrain.
        Vector3Int center = terrainGenerator.GetCenterGrid();

        // Calculate the world position at the surface of the terrain.
        Vector3 towerPos = terrainGenerator.GetSurfaceWorldPosition(center);
        towerPos.y += towerYOffset;

        // Instantiate the tower prefab at the calculated position.
        towerInstance = Instantiate(towerPrefab, towerPos, Quaternion.identity);
    }

    

    /// <summary>
    /// Coroutine to spawn a wave of enemies.
    /// </summary>
    /// <param name="enemyCount">Number of enemies to spawn in this wave.</param>
    IEnumerator SpawnWave(int enemyCount)
    {
        // Spawn each enemy in the wave with a delay between spawns.
        for (int i = 0; i < enemyCount; i++)
        {
            SpawnEnemyOnRandomPath();
            yield return new WaitForSeconds(enemySpawnInterval);
        }
        // After the wave, you could automatically start the next wave or wait for player input.
    }

    /// <summary>
    /// Spawns an enemy at the entrance of a randomly selected path.
    /// </summary>
    void SpawnEnemyOnRandomPath()
    {
        // Exit if there are no paths or the terrain generator is not set.
        if (terrainGenerator == null || terrainGenerator.numPaths == 0)
            return;

        // Get the list of paths from the terrain generator.
        System.Collections.Generic.List<System.Collections.Generic.List<UnityEngine.Vector3Int>> paths = terrainGenerator.GetPaths();
        if (paths == null || paths.Count == 0) return;

        // Select a random path.
        int pathIndex = Random.Range(0, paths.Count);
        System.Collections.Generic.List<UnityEngine.Vector3Int> path = paths[pathIndex];
        if (path == null || path.Count == 0) return;

        // Get the entrance position of the selected path.
        UnityEngine.Vector3Int entrance = path[0];
        Vector3 spawnPos = terrainGenerator.GetSurfaceWorldPosition(entrance);
        spawnPos.y += enemyYOffset;

        // Instantiate the enemy prefab at the spawn position.
        GameObject enemyObj = Instantiate(defaultEnemyPrefab, spawnPos, Quaternion.identity);
        Enemy enemy = enemyObj.GetComponent<Enemy>();
        if (enemy != null)
        {
            // Initialize the enemy with the path, terrain height, and tower reference.
            Tower towerComponent = towerInstance != null ? towerInstance.GetComponent<Tower>() : null;
            enemy.Initialize(path, terrainGenerator.height, towerComponent, this);
        }
    }



    /// <summary>
    /// Adds resources to the player's total.
    /// </summary>
    /// <param name="amount">Amount of resources to add.</param>
    public void AddResources(int amount)
    {
        resources += amount;
        if (resourceCounterUI != null)
            resourceCounterUI.SetResource(resources);
            
        // Notify performance tracker of resource gain
        if (performanceTracker != null)
        {
            performanceTracker.OnResourcesGained(amount);
        }
    }

    /// <summary>
    /// Attempts to spend resources. Returns true if successful.
    /// </summary>
    /// <param name="amount">Amount of resources to spend.</param>
    /// <returns>True if the player had enough resources, false otherwise.</returns>
    public bool SpendResources(int amount)
    {
        if (resources >= amount)
        {
            resources -= amount;
            if (resourceCounterUI != null)
                resourceCounterUI.SetResource(resources);
                
            // Notify performance tracker of resource spending
            if (performanceTracker != null)
            {
                performanceTracker.OnResourcesSpent(amount);
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// Gets the current amount of resources.
    /// </summary>
    /// <returns>Current resources.</returns>
    public int GetResources()
    {
        return resources;
    }
    
    /// <summary>
    /// Checks if a defender type is unlocked based on current wave progression
    /// </summary>
    /// <param name="defenderType">The defender type to check</param>
    /// <returns>True if the defender type is unlocked, false otherwise</returns>
    public bool IsDefenderTypeUnlocked(DefenderType defenderType)
    {
        switch (defenderType)
        {
            case DefenderType.Basic:
                return true; // Basic defender is always available
            case DefenderType.FrostTower:
                return currentWave >= 3; // Unlocks after wave 2 (bomber introduction)
            case DefenderType.LightningTower:
                return currentWave >= 6; // Unlocks after armored dragon threshold
            default:
                return false;
        }
    }


    /// <summary>
    /// Called when the game ends (e.g., tower health reaches zero).
    /// </summary>
    public void GameOver()
    {
        isGameOver = true;
        // Show the game over UI and stop enemy spawning.
        if (gameOverUI != null)
            gameOverUI.Show("Game Over!");
        // Debug logging disabled
    }

    /// <summary>
    /// Checks if the game is over.
    /// </summary>
    /// <returns>True if the game is over, false otherwise.</returns>
    public bool IsGameOver()
    {
        return isGameOver;
    }

   

    /// <summary>
    /// Toggles the pause state of the game.
    /// </summary>
    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    /// <summary>
    /// Pauses the game and shows the pause menu.
    /// </summary>
    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; // Freeze the game.
        if (pauseMenuUI != null)
            pauseMenuUI.Show();
    }

    /// <summary>
    /// Resumes the game and hides the pause menu.
    /// </summary>
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; // Unfreeze the game.
        if (pauseMenuUI != null)
            pauseMenuUI.Hide();
    }

    /// <summary>
    /// Restarts the game by reloading the current scene.
    /// </summary>
    public void RestartGame()
    {
        // Resume time scale before restarting to avoid issues.
        Time.timeScale = 1f;
        // Reload the current scene.
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Checks if the game is currently paused.
    /// </summary>
    /// <returns>True if the game is paused, false otherwise.</returns>
    public bool IsPaused()
    {
        return isPaused;
    }

    

    /// <summary>
    /// Attempts to place a defender at the specified grid position.
    /// </summary>
    /// <param name="gridPosition">Grid position where the defender should be placed.</param>
    /// <returns>True if the defender was placed successfully, false otherwise.</returns>
    public bool TryPlaceDefender(Vector3Int gridPosition)
    {
        return TryPlaceDefender(gridPosition, DefenderType.Basic);
    }

    /// <summary>
    /// Attempts to place a specific type of defender at the specified world position.
    /// </summary>
    /// <param name="worldPosition">Exact world position where the defender should be placed.</param>
    /// <param name="defenderType">Type of defender to place.</param>
    /// <returns>True if the defender was placed successfully, false otherwise.</returns>
    public bool TryPlaceDefenderAtWorldPosition(Vector3 worldPosition, DefenderType defenderType)
    {
        // Convert world position to grid position for validation
        Vector3Int gridPos = new Vector3Int(
            Mathf.RoundToInt(worldPosition.x),
            0,
            Mathf.RoundToInt(worldPosition.z)
        );
        
        // Use the existing grid-based validation but place at exact world position
        return TryPlaceDefenderAtExactPosition(worldPosition, gridPos, defenderType);
    }
    
    /// <summary>
    /// Attempts to place a specific type of defender at the specified grid position.
    /// </summary>
    /// <param name="gridPosition">Grid position where the defender should be placed.</param>
    /// <param name="defenderType">Type of defender to place.</param>
    /// <returns>True if the defender was placed successfully, false otherwise.</returns>
    public bool TryPlaceDefender(Vector3Int gridPosition, DefenderType defenderType)
    {
        // Exit if the game is over, paused, or required references are missing.
        if (isGameOver || isPaused) return false;
        if (terrainGenerator == null) return false;
        
        // Check progressive unlocking system
        if (!IsDefenderTypeUnlocked(defenderType))
        {
            // Debug.Log($"Defender type {defenderType} is not yet unlocked! Current wave: {currentWave}");
            return false;
        }

        // Get the appropriate prefab and cost based on defender type
        GameObject defenderPrefabToUse;
        int cost;
        
        switch (defenderType)
        {
            case DefenderType.Basic:
                defenderPrefabToUse = defenderPrefab;
                cost = defenderCost;
                break;
            case DefenderType.FrostTower:
                defenderPrefabToUse = frostTowerPrefab;
                cost = frostTowerCost;
                break;
            case DefenderType.LightningTower:
                defenderPrefabToUse = lightningTowerPrefab;
                cost = lightningTowerCost;
                break;
            default:
                defenderPrefabToUse = defenderPrefab;
                cost = defenderCost;
                break;
        }

        if (defenderPrefabToUse == null) return false;

        // Only allow placement in defender zones
        if (!IsValidZonePlacement(gridPosition)) return false;

        // Check if the player has enough resources.
        if (!SpendResources(cost)) return false;
        
        // Check defender count limit
        if (currentDefenderCount >= maxDefenderCount)
        {
            // Debug.Log($" Defender limit reached! ({currentDefenderCount}/{maxDefenderCount}) - Remove a defender first!");
            return false;
        }

        // Clamp the grid position to ensure it's within terrain bounds.
        int gx = Mathf.Clamp(gridPosition.x, 0, terrainGenerator.width - 1);
        int gz = Mathf.Clamp(gridPosition.z, 0, terrainGenerator.depth - 1);

        // Calculate the world position for the defender.
        Vector3 worldPos = terrainGenerator.GetSurfaceWorldPosition(new Vector3Int(gx, 0, gz));
        worldPos.y += defenderYOffset;

        // Instantiate the defender prefab at the calculated position.
        GameObject newDefender = Instantiate(defenderPrefabToUse, worldPos, Quaternion.identity);
        
        // Increment defender count
        currentDefenderCount++;
        
        // Update zone defender count
        UpdateZoneDefenderCount(gridPosition, true);
        
        // Debug.Log($" Defender placed! Count: {currentDefenderCount}/{maxDefenderCount}");
        
        return true;
    }
    
    /// <summary>
    /// Places a defender at an exact world position with grid-based validation
    /// </summary>
    /// <param name="worldPosition">Exact world position for placement</param>
    /// <param name="gridPosition">Grid position for validation</param>
    /// <param name="defenderType">Type of defender to place</param>
    /// <returns>True if placement was successful</returns>
    bool TryPlaceDefenderAtExactPosition(Vector3 worldPosition, Vector3Int gridPosition, DefenderType defenderType)
    {
        // Exit if the game is over, paused, or required references are missing.
        if (isGameOver || isPaused) return false;
        if (terrainGenerator == null) return false;
        
        // Check progressive unlocking system
        if (!IsDefenderTypeUnlocked(defenderType))
        {
            // Debug.Log($"Defender type {defenderType} is not yet unlocked! Current wave: {currentWave}");
            return false;
        }

        // Get the appropriate prefab and cost based on defender type
        GameObject defenderPrefabToUse;
        int cost;
        
        switch (defenderType)
        {
            case DefenderType.Basic:
                defenderPrefabToUse = defenderPrefab;
                cost = defenderCost;
                break;
            case DefenderType.FrostTower:
                defenderPrefabToUse = frostTowerPrefab;
                cost = frostTowerCost;
                break;
            case DefenderType.LightningTower:
                defenderPrefabToUse = lightningTowerPrefab;
                cost = lightningTowerCost;
                break;
            default:
                defenderPrefabToUse = defenderPrefab;
                cost = defenderCost;
                break;
        }

        if (defenderPrefabToUse == null) return false;

        // Only allow placement on valid non-path tiles (using grid position for validation)
        if (!terrainGenerator.IsValidDefenderPlacement(gridPosition)) return false;
        
        // Check defender count limit BEFORE spending resources
        if (currentDefenderCount >= maxDefenderCount)
        {
            // Debug.Log($" Defender limit reached! ({currentDefenderCount}/{maxDefenderCount}) - Remove a defender first!");
            return false;
        }

        // Check if the player has enough resources.
        if (!SpendResources(cost)) return false;

        // Use the exact world position for placement
        Vector3 finalPosition = worldPosition;
        finalPosition.y += defenderYOffset;

        // Debug.Log($" PLACEMENT DEBUG: World position: {worldPosition}, Final position: {finalPosition}");

        // Instantiate the defender prefab at the exact position
        GameObject newDefender = Instantiate(defenderPrefabToUse, finalPosition, Quaternion.identity);
        
        // Debug.Log($" DEFENDER PLACED: {newDefender.name} at {newDefender.transform.position}");
        
        // Increment defender count
        currentDefenderCount++;
        // Debug.Log($" Defender placed at exact position! Count: {currentDefenderCount}/{maxDefenderCount}");
        
        return true;
    }
    
    /// <summary>
    /// Called when a defender dies to update the count
    /// </summary>
    public void OnDefenderDestroyed()
    {
        if (currentDefenderCount > 0)
        {
            currentDefenderCount--;
            // Debug.Log($" Defender destroyed! Count: {currentDefenderCount}/{maxDefenderCount}");
        }
    }
    
    /// <summary>
    /// Counts current defenders in the scene
    /// </summary>
    void CountCurrentDefenders()
    {
        Defender[] defenders = FindObjectsByType<Defender>(FindObjectsSortMode.None);
        currentDefenderCount = defenders.Length;
        // Debug.Log($" Initial defender count: {currentDefenderCount}/{maxDefenderCount}");
    }

    /// <summary>
    /// Highlights the specified path on the terrain.
    /// </summary>
    /// <param name="pathIndex">Index of the path to highlight.</param>
    public void HighlightPath(int pathIndex)
    {
        if (terrainGenerator != null)
        {
            terrainGenerator.HighlightPath(pathIndex);
        }

        if (tower != null)
        {
            tower.ExtendAttackRange(true);
        }
    }

    /// <summary>
    /// Highlights valid defender placement locations near the specified path.
    /// </summary>
    /// <param name="pathIndex">Index of the path to highlight defender locations for.</param>
    public void HighlightDefenderLocations(int pathIndex)
    {
        if (terrainGenerator != null)
        {
            terrainGenerator.HighlightDefenderLocations(pathIndex);
        }
    }
    
    /// <summary>
    /// Checks if a position is valid for defender placement within a zone
    /// </summary>
    /// <param name="gridPosition">Grid position to check</param>
    /// <returns>True if position is in a valid zone</returns>
    private bool IsValidZonePlacement(Vector3Int gridPosition)
    {
        if (terrainGenerator == null) return false;
        
        // Check if position is in any defender zone
        foreach (DefenderZone zone in terrainGenerator.DefenderZones)
        {
            if (zone.isActive && zone.ContainsPosition(gridPosition))
            {
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Updates the defender count for a specific zone
    /// </summary>
    /// <param name="gridPosition">Position of the defender</param>
    /// <param name="isAdding">True if adding defender, false if removing</param>
    private void UpdateZoneDefenderCount(Vector3Int gridPosition, bool isAdding)
    {
        if (terrainGenerator == null) return;
        
        foreach (DefenderZone zone in terrainGenerator.DefenderZones)
        {
            if (zone.ContainsPosition(gridPosition))
            {
                if (isAdding)
                {
                    zone.AddDefender();
                }
                else
                {
                    zone.RemoveDefender();
                }
                break;
            }
        }
    }
    
    /// <summary>
    /// Gets the zone containing a specific position
    /// </summary>
    /// <param name="gridPosition">Position to check</param>
    /// <returns>The zone containing the position, or null if not found</returns>
    public DefenderZone GetZoneForPosition(Vector3Int gridPosition)
    {
        if (terrainGenerator == null) return null;
        
        foreach (DefenderZone zone in terrainGenerator.DefenderZones)
        {
            if (zone.isActive && zone.ContainsPosition(gridPosition))
            {
                return zone;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Gets all defenders in a specific zone
    /// </summary>
    /// <param name="zone">Zone to check</param>
    /// <returns>List of defenders in the zone</returns>
    public List<Defender> GetDefendersInZone(DefenderZone zone)
    {
        List<Defender> defendersInZone = new List<Defender>();
        
        if (zone == null) return defendersInZone;
        
        // Find all defenders in the scene
        Defender[] allDefenders = FindObjectsByType<Defender>(FindObjectsSortMode.None);
        
        foreach (Defender defender in allDefenders)
        {
            if (defender != null)
            {
                Vector3Int defenderPos = terrainGenerator.WorldToGridPosition(defender.transform.position);
                if (zone.ContainsPosition(defenderPos))
                {
                    defendersInZone.Add(defender);
                }
            }
        }
        
        return defendersInZone;
    }
}