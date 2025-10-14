using UnityEngine;

/// <summary>
/// Setup guide for the mathematical wave progression system
/// </summary>
public class WaveProgressionSetupGuide : MonoBehaviour
{
    [Header("Setup Instructions")]
    [TextArea(15, 20)]
    public string setupInstructions = 
        "MATHEMATICAL WAVE PROGRESSION SYSTEM SETUP:\n\n" +
        "1. Add WaveProgressionSystem component to GameManager\n" +
        "2. Assign WaveProgressionSystem reference in EnemySpawner\n" +
        "3. Configure wave progression settings:\n" +
        "   - Learning Phase Waves: 3 (waves 1-3)\n" +
        "   - Bomber Introduction Wave: 4\n" +
        "   - Armored Introduction Wave: 6\n\n" +
        "WAVE PROGRESSION MATHEMATICAL MODEL:\n" +
        "• Waves 1-3: Regular enemies only (100%)\n" +
        "• Waves 4-5: Regular (60%) + Fast (20%) + Bomber (20%)\n" +
        "• Waves 6+: Regular (40%) + Fast (20%) + Bomber (20%) + Armored (20%)\n\n" +
        "ADAPTIVE SCALING MATHEMATICAL MODEL:\n" +
        "• Performance Score: P = (K×0.3) + (R×0.2) + (T×0.2) + (D×0.15) + (E×0.15)\n" +
        "• Enemy Count: BaseCount × (1.0 + (P-50)/50 × 0.4)\n" +
        "• Enemy Stats: BaseStats × (1.0 + (P-50)/50 × 0.3)\n" +
        "• Type Selection: Adaptive probabilities based on performance\n\n" +
        "PERFORMANCE TRACKING:\n" +
        "• K = Kill Efficiency (enemies killed / spawned)\n" +
        "• R = Resource Efficiency (gained / spent)\n" +
        "• T = Time Efficiency (optimal / actual time)\n" +
        "• D = Defender Survival Rate (alive / placed)\n" +
        "• E = Enemy Progression Rate (average path progression)";

    [Header("Quick Setup")]
    [Tooltip("Click to create WaveProgressionSystem")]
    public bool createWaveProgressionSystem = false;
    
    [Tooltip("Click to setup EnemySpawner references")]
    public bool setupEnemySpawner = false;
    
    [Tooltip("Click to test wave progression")]
    public bool testWaveProgression = false;
    
    void Update()
    {
        if (createWaveProgressionSystem)
        {
            createWaveProgressionSystem = false;
            CreateWaveProgressionSystem();
        }
        
        if (setupEnemySpawner)
        {
            setupEnemySpawner = false;
            SetupEnemySpawnerReferences();
        }
        
        if (testWaveProgression)
        {
            testWaveProgression = false;
            TestWaveProgression();
        }
    }
    
    /// <summary>
    /// Creates a WaveProgressionSystem component
    /// </summary>
    void CreateWaveProgressionSystem()
    {
        // Find GameManager
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("No GameManager found! Please create one first.");
            return;
        }
        
        // Add WaveProgressionSystem to GameManager
        WaveProgressionSystem waveSystem = gameManager.GetComponent<WaveProgressionSystem>();
        if (waveSystem == null)
        {
            waveSystem = gameManager.gameObject.AddComponent<WaveProgressionSystem>();
            Debug.Log("Added WaveProgressionSystem to GameManager");
        }
        else
        {
            Debug.Log("WaveProgressionSystem already exists on GameManager");
        }
        
        // Configure mathematical model settings
        waveSystem.learningPhaseWaves = 3;
        waveSystem.bomberIntroductionWave = 4;
        waveSystem.armoredIntroductionWave = 6;
        
        // Set adaptive scaling parameters
        waveSystem.countInfluence = 0.4f;
        waveSystem.typeInfluence = 0.5f;
        waveSystem.statInfluence = 0.3f;
        
        // Set scaling factors
        waveSystem.countScalingFactor = 1.2f;
        waveSystem.healthScalingFactor = 1.15f;
        waveSystem.speedScalingFactor = 1.05f;
        waveSystem.damageScalingFactor = 1.1f;
        
        Debug.Log("WaveProgressionSystem created with mathematical model settings!");
    }
    
    /// <summary>
    /// Sets up EnemySpawner references
    /// </summary>
    void SetupEnemySpawnerReferences()
    {
        // Find EnemySpawner
        EnemySpawner enemySpawner = FindFirstObjectByType<EnemySpawner>();
        if (enemySpawner == null)
        {
            Debug.LogError("No EnemySpawner found! Please create one first.");
            return;
        }
        
        // Find WaveProgressionSystem
        WaveProgressionSystem waveSystem = FindFirstObjectByType<WaveProgressionSystem>();
        if (waveSystem == null)
        {
            Debug.LogError("No WaveProgressionSystem found! Please create one first.");
            return;
        }
        
        // Assign reference
        enemySpawner.waveProgressionSystem = waveSystem;
        
        Debug.Log("EnemySpawner references configured!");
    }
    
    /// <summary>
    /// Tests the wave progression system
    /// </summary>
    void TestWaveProgression()
    {
        WaveProgressionSystem waveSystem = FindFirstObjectByType<WaveProgressionSystem>();
        if (waveSystem == null)
        {
            Debug.LogError("No WaveProgressionSystem found!");
            return;
        }
        
        Debug.Log("=== TESTING WAVE PROGRESSION SYSTEM ===");
        
        // Test different waves
        for (int wave = 1; wave <= 8; wave++)
        {
            Debug.Log($"\n--- Wave {wave} ---");
            Debug.Log($"Description: {waveSystem.GetWaveDescription(wave)}");
            
            // Test enemy type selection
            WaveProgressionSystem.EnemyType enemyType = waveSystem.GetEnemyTypeForWave(wave);
            Debug.Log($"Selected Enemy Type: {enemyType}");
            
            // Test enemy count
            int baseCount = 5;
            int adaptiveCount = waveSystem.GetAdaptiveEnemyCount(baseCount, wave);
            Debug.Log($"Base Count: {baseCount}, Adaptive Count: {adaptiveCount}");
            
            // Test stat multipliers
            Vector3 statMultipliers = waveSystem.GetAdaptiveStatMultipliers(wave);
            Debug.Log($"Stat Multipliers - Health: {statMultipliers.x:F2}, Speed: {statMultipliers.y:F2}, Damage: {statMultipliers.z:F2}");
        }
        
        Debug.Log("=== WAVE PROGRESSION TEST COMPLETE ===");
    }
    
    void OnGUI()
    {
        if (Application.isPlaying) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 600, 500));
        GUILayout.Label("Mathematical Wave Progression System Setup", GUI.skin.box);
        GUILayout.Label(setupInstructions);
        GUILayout.EndArea();
    }
}
