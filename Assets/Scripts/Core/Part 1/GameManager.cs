using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GADE7322_POE.UI;



public class GameManager : MonoBehaviour
{
   

    [Header("References")]
    [Tooltip("Reference to the VoxelTerrainGenerator.")]
    public VoxelTerrainGenerator terrainGenerator;
    [Tooltip("Prefab for the central tower.")]
    public GameObject towerPrefab;
    [Tooltip("Prefab for the default enemy type.")]
    public GameObject defaultEnemyPrefab;
    [Tooltip("Prefab for the fast enemy type.")]
    public GameObject fastEnemyPrefab;
    [Tooltip("Prefab for the tank enemy type.")]
    public GameObject tankEnemyPrefab;
    [Tooltip("Time delay between spawning enemies.")]
    public float enemySpawnInterval = 2f;
    [Tooltip("Number of enemies in the first wave.")]
    public int initialWaveEnemyCount = 5;
    [Tooltip("Multiplier for increasing enemies per wave.")]
    public float waveScalingFactor = 1.5f;
    [Tooltip("Multiplier for increasing enemy health per wave.")]
    public float healthScalingFactor = 1.2f;
    [Tooltip("Multiplier for increasing enemy speed per wave.")]
    public float speedScalingFactor = 1.1f;

    [Header("Defenders & Resources")]
    [Tooltip("Prefab for the defender.")]
    public GameObject defenderPrefab;
    [Tooltip("Cost to place a defender.")]
    public int defenderCost = 25;
    [Tooltip("Initial resources for the player.")]
    public int startingResources = 100;

    [Header("Placement Offsets")]
    [Tooltip("Vertical offset for placing the tower.")]
    public float towerYOffset = 1f;
    [Tooltip("Vertical offset for spawning enemies.")]
    public float enemyYOffset = 1f;
    [Tooltip("Vertical offset for placing defenders.")]
    public float defenderYOffset = 1f;

    // UI References
    [Tooltip("UI for displaying the tower's health.")]
    public HealthBarUI towerHealthBar;
    [Tooltip("UI for displaying the player's resources.")]
    public ResourceCounterUI resourceCounterUI;
    [Tooltip("UI for displaying the defender cost.")]
    public DefenderCostUI defenderCostUI;
    [Tooltip("UI for displaying the game over screen.")]
    public GameOverUI gameOverUI;
    [Tooltip("UI for pausing the game.")]
    public PauseMenuUI pauseMenuUI;

    

    // Reference to the instantiated tower in the scene.
    private GameObject towerInstance;
    public GameObject TowerInstance { get { return towerInstance; } }

    // Current wave number (starts at 1).
    public int currentWave = 1;

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
        if (gameOverUI != null)
            gameOverUI.Hide();
        if (pauseMenuUI != null)
            pauseMenuUI.Hide();

        if (terrainGenerator == null)
        {
            Debug.LogError("GameManager: No VoxelTerrainGenerator assigned!");
            return;
        }

        resources = Mathf.Max(0, startingResources);
        if (resourceCounterUI != null)
            resourceCounterUI.SetResource(resources);
        if (defenderCostUI != null)
            defenderCostUI.SetCost(defenderCost);

        if (!terrainGenerator.IsReady)
        {
            terrainGenerator.GetCenterGrid();
        }
        SpawnTower();

        enemySpawner = GetComponent<EnemySpawner>();
        if (enemySpawner == null)
        {
            enemySpawner = gameObject.AddComponent<EnemySpawner>();
        }

        enemySpawner.gameManager = this;
        enemySpawner.defaultEnemyPrefab = defaultEnemyPrefab;
        enemySpawner.fastEnemyPrefab = fastEnemyPrefab;
        enemySpawner.tankEnemyPrefab = tankEnemyPrefab;
        enemySpawner.spawnInterval = enemySpawnInterval;
        enemySpawner.initialWaveEnemyCount = initialWaveEnemyCount;
        enemySpawner.waveScalingFactor = waveScalingFactor;
        enemySpawner.healthScalingFactor = healthScalingFactor;
        enemySpawner.speedScalingFactor = speedScalingFactor;
    }

    void Update()
    {
        if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current[UnityEngine.InputSystem.Key.Escape].wasPressedThisFrame)
        {
            TogglePause();
        }
    }

   

    void SpawnTower()
    {
        Vector3Int center = terrainGenerator.GetCenterGrid();
        Vector3 towerPos = terrainGenerator.GetSurfaceWorldPosition(center);
        towerPos.y += towerYOffset;
        towerInstance = Instantiate(towerPrefab, towerPos, Quaternion.identity);
    }

    

    IEnumerator SpawnWave(int enemyCount)
    {
        for (int i = 0; i < enemyCount; i++)
        {
            SpawnEnemyOnRandomPath();
            yield return new WaitForSeconds(enemySpawnInterval);
        }
    }

    void SpawnEnemyOnRandomPath()
    {
        if (terrainGenerator == null || terrainGenerator.numPaths == 0)
            return;

        List<List<Vector3Int>> paths = terrainGenerator.GetPaths();
        if (paths == null || paths.Count == 0) return;

        int pathIndex = Random.Range(0, paths.Count);
        List<Vector3Int> path = paths[pathIndex];
        if (path == null || path.Count == 0) return;

        Vector3Int entrance = path[0];
        Vector3 spawnPos = terrainGenerator.GetSurfaceWorldPosition(entrance);
        spawnPos.y += enemyYOffset;

        GameObject enemyObj = Instantiate(defaultEnemyPrefab, spawnPos, Quaternion.identity);
        Enemy enemy = enemyObj.GetComponent<Enemy>();
        if (enemy != null)
        {
            Tower towerComponent = towerInstance != null ? towerInstance.GetComponent<Tower>() : null;
            enemy.Initialize(path, terrainGenerator.height, towerComponent, this);
        }
    }



    public void AddResources(int amount)
    {
        resources += amount;
        if (resourceCounterUI != null)
            resourceCounterUI.SetResource(resources);
    }

    public bool SpendResources(int amount)
    {
        if (resources >= amount)
        {
            resources -= amount;
            if (resourceCounterUI != null)
                resourceCounterUI.SetResource(resources);
            return true;
        }
        return false;
    }

    public int GetResources()
    {
        return resources;
    }


    public void GameOver()
    {
        isGameOver = true;
        if (gameOverUI != null)
            gameOverUI.Show("Game Over!");
        Debug.Log("Game Over!");
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }

   

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

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        if (pauseMenuUI != null)
            pauseMenuUI.Show();
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pauseMenuUI != null)
            pauseMenuUI.Hide();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public bool IsPaused()
    {
        return isPaused;
    }

    

    public bool TryPlaceDefender(Vector3Int gridPosition)
    {
        if (isGameOver || isPaused) return false;
        if (defenderPrefab == null || terrainGenerator == null) return false;

        if (!terrainGenerator.IsValidDefenderPlacement(gridPosition)) return false;

        if (!SpendResources(defenderCost)) return false;

        int gx = Mathf.Clamp(gridPosition.x, 0, terrainGenerator.width - 1);
        int gz = Mathf.Clamp(gridPosition.z, 0, terrainGenerator.depth - 1);

        Vector3 worldPos = terrainGenerator.GetSurfaceWorldPosition(new Vector3Int(gx, 0, gz));
        worldPos.y += defenderYOffset;

        Instantiate(defenderPrefab, worldPos, Quaternion.identity);
        return true;
    }

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

    public void HighlightDefenderLocations(int pathIndex)
    {
        if (terrainGenerator != null)
        {
            terrainGenerator.HighlightDefenderLocations(pathIndex);
        }
    }
}