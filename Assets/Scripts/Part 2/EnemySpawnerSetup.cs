using UnityEngine;

/// <summary>
/// Setup script to automatically assign WaveProgressionSystem to EnemySpawner
/// </summary>
public class EnemySpawnerSetup : MonoBehaviour
{
    [Header("Auto Setup")]
    [Tooltip("Click to automatically setup EnemySpawner references")]
    public bool setupReferences = false;
    
    [Tooltip("Click to find and assign WaveProgressionSystem")]
    public bool findWaveProgressionSystem = false;
    
    void Update()
    {
        if (setupReferences)
        {
            setupReferences = false;
            SetupEnemySpawnerReferences();
        }
        
        if (findWaveProgressionSystem)
        {
            findWaveProgressionSystem = false;
            FindAndAssignWaveProgressionSystem();
        }
    }
    
    /// <summary>
    /// Sets up all EnemySpawner references automatically
    /// </summary>
    void SetupEnemySpawnerReferences()
    {
        // Find EnemySpawner
        EnemySpawner enemySpawner = FindFirstObjectByType<EnemySpawner>();
        if (enemySpawner == null)
        {
            Debug.LogError("No EnemySpawner found in scene!");
            return;
        }
        
        // Find WaveProgressionSystem
        WaveProgressionSystem waveSystem = FindFirstObjectByType<WaveProgressionSystem>();
        if (waveSystem == null)
        {
            Debug.LogError("No WaveProgressionSystem found in scene! Please add one to GameManager first.");
            return;
        }
        
        // Assign the reference
        enemySpawner.waveProgressionSystem = waveSystem;
        
        Debug.Log($"Successfully assigned WaveProgressionSystem to EnemySpawner!");
        Debug.Log($"WaveProgressionSystem found on: {waveSystem.gameObject.name}");
        Debug.Log($"EnemySpawner found on: {enemySpawner.gameObject.name}");
    }
    
    /// <summary>
    /// Finds and assigns WaveProgressionSystem specifically
    /// </summary>
    void FindAndAssignWaveProgressionSystem()
    {
        // Find all WaveProgressionSystem components
        WaveProgressionSystem[] waveSystems = FindObjectsByType<WaveProgressionSystem>(FindObjectsSortMode.None);
        
        if (waveSystems.Length == 0)
        {
            Debug.LogError("No WaveProgressionSystem found in scene!");
            return;
        }
        
        if (waveSystems.Length > 1)
        {
            Debug.LogWarning($"Found {waveSystems.Length} WaveProgressionSystem components. Using the first one.");
        }
        
        WaveProgressionSystem waveSystem = waveSystems[0];
        
        // Find EnemySpawner
        EnemySpawner enemySpawner = FindFirstObjectByType<EnemySpawner>();
        if (enemySpawner == null)
        {
            Debug.LogError("No EnemySpawner found in scene!");
            return;
        }
        
        // Assign the reference
        enemySpawner.waveProgressionSystem = waveSystem;
        
        Debug.Log($"Successfully assigned WaveProgressionSystem to EnemySpawner!");
        Debug.Log($"WaveProgressionSystem: {waveSystem.gameObject.name}");
        Debug.Log($"EnemySpawner: {enemySpawner.gameObject.name}");
    }
    
    void OnGUI()
    {
        if (Application.isPlaying) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 400, 200));
        GUILayout.Label("EnemySpawner Setup", GUI.skin.box);
        GUILayout.Label("This script helps setup EnemySpawner references.");
        GUILayout.Space(10);
        GUILayout.Label("1. Make sure WaveProgressionSystem is on GameManager");
        GUILayout.Label("2. Click 'Setup References' to auto-assign");
        GUILayout.Label("3. Or manually drag WaveProgressionSystem to EnemySpawner");
        GUILayout.EndArea();
    }
}
