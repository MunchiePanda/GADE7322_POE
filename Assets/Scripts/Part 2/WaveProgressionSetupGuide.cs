using UnityEngine;

/// <summary>
/// Setup guide for the new wave progression system
/// </summary>
public class WaveProgressionSetupGuide : MonoBehaviour
{
    [Header("Setup Instructions")]
    [TextArea(10, 15)]
    public string setupInstructions = 
        "WAVE PROGRESSION SETUP GUIDE:\n\n" +
        "1. Create a GameObject called 'WaveProgressionManager'\n" +
        "2. Add the WaveProgressionManager script to it\n" +
        "3. Assign the PlayerPerformanceTracker reference\n" +
        "4. In EnemySpawner, assign the WaveProgressionManager reference\n" +
        "5. Configure wave probabilities in the inspector\n\n" +
        "WAVE PROGRESSION:\n" +
        "• Waves 1-3: Only basic enemies\n" +
        "• Waves 4-7: Basic + Fast + Bomber enemies\n" +
        "• Waves 8+: All enemy types including Armored\n\n" +
        "ADAPTIVE SCALING:\n" +
        "• High performance (>70): More enemies, harder types\n" +
        "• Low performance (<30): Fewer enemies, easier types\n" +
        "• Performance affects both count and difficulty\n\n" +
        "The system will automatically:\n" +
        "• Unlock enemy types at the right waves\n" +
        "• Scale enemy count based on performance\n" +
        "• Adjust difficulty based on player skill\n" +
        "• Provide clear progression curve";

    [Header("Quick Setup")]
    [Tooltip("Click to create the WaveProgressionManager setup")]
    public bool createSetup = false;
    
    void Update()
    {
        if (createSetup)
        {
            createSetup = false;
            CreateWaveProgressionSetup();
        }
    }
    
    /// <summary>
    /// Creates the wave progression setup
    /// </summary>
    void CreateWaveProgressionSetup()
    {
        // Find or create WaveProgressionManager
        WaveProgressionManager waveManager = FindFirstObjectByType<WaveProgressionManager>();
        if (waveManager == null)
        {
            GameObject waveManagerObj = new GameObject("WaveProgressionManager");
            waveManager = waveManagerObj.AddComponent<WaveProgressionManager>();
        }
        
        // Find PlayerPerformanceTracker
        PlayerPerformanceTracker performanceTracker = FindFirstObjectByType<PlayerPerformanceTracker>();
        if (performanceTracker != null)
        {
            waveManager.performanceTracker = performanceTracker;
        }
        
        // Find EnemySpawner and assign reference
        EnemySpawner enemySpawner = FindFirstObjectByType<EnemySpawner>();
        if (enemySpawner != null)
        {
            enemySpawner.waveProgressionManager = waveManager;
        }
        
        Debug.Log("Wave Progression System setup complete!");
        Debug.Log("Configure wave probabilities in the WaveProgressionManager inspector");
    }
    
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 500, 400));
        GUILayout.Label("Wave Progression Setup Guide", GUI.skin.box);
        GUILayout.Label(setupInstructions);
        
        if (GUILayout.Button("Create Wave Progression Setup"))
        {
            CreateWaveProgressionSetup();
        }
        
        GUILayout.EndArea();
    }
}
