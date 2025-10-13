using UnityEngine;

/// <summary>
/// Tracks player performance metrics to drive adaptive difficulty scaling.
/// Monitors tower health, enemy kills, wave completion time, and resource efficiency.
/// </summary>
public class PlayerPerformanceTracker : MonoBehaviour
{
    [Header("Performance Metrics")]
    [Tooltip("Current tower health percentage (0-100)")]
    public float towerHealthPercentage { get; private set; } = 100f;
    
    [Tooltip("Enemies killed in current wave")]
    public int enemiesKilledThisWave { get; private set; } = 0;
    
    [Tooltip("Enemies that reached the tower in current wave")]
    public int enemiesReachedTowerThisWave { get; private set; } = 0;
    
    [Tooltip("Defenders lost in current wave")]
    public int defendersLostThisWave { get; private set; } = 0;
    
    [Tooltip("Time taken to complete current wave (seconds)")]
    public float waveCompletionTime { get; private set; } = 0f;
    
    [Tooltip("Resources spent in current wave")]
    public int resourcesSpentThisWave { get; private set; } = 0;
    
    [Tooltip("Resources gained in current wave")]
    public int resourcesGainedThisWave { get; private set; } = 0;
    
    [Header("Performance Calculation")]
    [Tooltip("Overall performance score (0-100, 50 = neutral)")]
    public float performanceScore { get; private set; } = 50f;
    
    [Tooltip("How much tower health affects performance score")]
    public float towerHealthWeight = 30f;
    
    [Tooltip("How much kill efficiency affects performance score")]
    public float killEfficiencyWeight = 25f;
    
    [Tooltip("How much wave speed affects performance score")]
    public float waveSpeedWeight = 20f;
    
    [Tooltip("How much resource efficiency affects performance score")]
    public float resourceEfficiencyWeight = 15f;
    
    [Tooltip("How much defender losses affect performance score")]
    public float defenderLossWeight = 10f;
    
    [Header("Debug")]
    [Tooltip("Show performance calculations in console")]
    public bool showDebugInfo = true;
    
    private GameManager gameManager;
    private Tower tower;
    private float waveStartTime;
    private float initialTowerHealth;
    private int initialDefenderCount;
    
    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            tower = gameManager.tower;
        if (tower != null)
        {
            initialTowerHealth = tower.GetMaxHealth();
        }
        }
        
        // Count initial defenders
        CountInitialDefenders();
    }
    
    void Update()
    {
        // Update tower health percentage
        if (tower != null)
        {
            towerHealthPercentage = (tower.GetCurrentHealth() / tower.GetMaxHealth()) * 100f;
        }
    }
    
    /// <summary>
    /// Called when a new wave starts
    /// </summary>
    public void OnWaveStart()
    {
        waveStartTime = Time.time;
        enemiesKilledThisWave = 0;
        enemiesReachedTowerThisWave = 0;
        defendersLostThisWave = 0;
        resourcesSpentThisWave = 0;
        resourcesGainedThisWave = 0;
        waveCompletionTime = 0f;
        
        if (showDebugInfo)
        {
            Debug.Log($"Performance Tracker: Wave started. Tower Health: {towerHealthPercentage:F1}%");
        }
    }
    
    /// <summary>
    /// Called when a wave is completed
    /// </summary>
    public void OnWaveComplete()
    {
        waveCompletionTime = Time.time - waveStartTime;
        UpdatePerformanceScore();
        
        if (showDebugInfo)
        {
            Debug.Log($"Performance Tracker: Wave completed in {waveCompletionTime:F1}s. Performance Score: {performanceScore:F1}");
        }
    }
    
    /// <summary>
    /// Called when an enemy is killed
    /// </summary>
    public void OnEnemyKilled()
    {
        enemiesKilledThisWave++;
        UpdatePerformanceScore();
        
        if (showDebugInfo)
        {
            Debug.Log($"Performance Tracker: Enemy killed. Total: {enemiesKilledThisWave}");
        }
    }
    
    /// <summary>
    /// Called when an enemy reaches the tower
    /// </summary>
    public void OnEnemyReachedTower()
    {
        enemiesReachedTowerThisWave++;
        UpdatePerformanceScore();
        
        if (showDebugInfo)
        {
            Debug.Log($"Performance Tracker: Enemy reached tower. Total: {enemiesReachedTowerThisWave}");
        }
    }
    
    /// <summary>
    /// Called when a defender is lost
    /// </summary>
    public void OnDefenderLost()
    {
        defendersLostThisWave++;
        UpdatePerformanceScore();
        
        if (showDebugInfo)
        {
            Debug.Log($"Performance Tracker: Defender lost. Total: {defendersLostThisWave}");
        }
    }
    
    /// <summary>
    /// Called when resources are spent
    /// </summary>
    public void OnResourcesSpent(int amount)
    {
        resourcesSpentThisWave += amount;
        UpdatePerformanceScore();
        
        if (showDebugInfo)
        {
            Debug.Log($"Performance Tracker: Resources spent: {amount}. Total: {resourcesSpentThisWave}");
        }
    }
    
    /// <summary>
    /// Called when resources are gained
    /// </summary>
    public void OnResourcesGained(int amount)
    {
        resourcesGainedThisWave += amount;
        UpdatePerformanceScore();
        
        if (showDebugInfo)
        {
            Debug.Log($"Performance Tracker: Resources gained: {amount}. Total: {resourcesGainedThisWave}");
        }
    }
    
    /// <summary>
    /// Updates the overall performance score based on all metrics
    /// </summary>
    private void UpdatePerformanceScore()
    {
        float towerHealthScore = CalculateTowerHealthScore();
        float killEfficiencyScore = CalculateKillEfficiencyScore();
        float waveSpeedScore = CalculateWaveSpeedScore();
        float resourceEfficiencyScore = CalculateResourceEfficiencyScore();
        float defenderLossScore = CalculateDefenderLossScore();
        
        performanceScore = towerHealthScore + killEfficiencyScore + waveSpeedScore + 
                          resourceEfficiencyScore + defenderLossScore;
        
        performanceScore = Mathf.Clamp(performanceScore, 0f, 100f);
        
        if (showDebugInfo)
        {
            Debug.Log($"Performance Score: {performanceScore:F1} (Tower: {towerHealthScore:F1}, " +
                     $"Kills: {killEfficiencyScore:F1}, Speed: {waveSpeedScore:F1}, " +
                     $"Resources: {resourceEfficiencyScore:F1}, Defenders: {defenderLossScore:F1})");
        }
    }
    
    /// <summary>
    /// Calculates score based on tower health
    /// </summary>
    private float CalculateTowerHealthScore()
    {
        return (towerHealthPercentage / 100f) * towerHealthWeight;
    }
    
    /// <summary>
    /// Calculates score based on kill efficiency
    /// </summary>
    private float CalculateKillEfficiencyScore()
    {
        int totalEnemies = enemiesKilledThisWave + enemiesReachedTowerThisWave;
        if (totalEnemies == 0) return killEfficiencyWeight * 0.5f; // Neutral if no enemies yet
        
        float efficiency = (float)enemiesKilledThisWave / totalEnemies;
        return efficiency * killEfficiencyWeight;
    }
    
    /// <summary>
    /// Calculates score based on wave completion speed
    /// </summary>
    private float CalculateWaveSpeedScore()
    {
        if (waveCompletionTime == 0) return waveSpeedWeight * 0.5f; // Neutral if wave not complete
        
        // Faster completion = higher score
        float expectedTime = 30f; // Expected 30 seconds per wave
        float speedRatio = Mathf.Clamp(expectedTime / waveCompletionTime, 0f, 2f);
        return speedRatio * waveSpeedWeight;
    }
    
    /// <summary>
    /// Calculates score based on resource efficiency
    /// </summary>
    private float CalculateResourceEfficiencyScore()
    {
        if (resourcesSpentThisWave == 0) return resourceEfficiencyWeight * 0.5f; // Neutral if no spending
        
        float efficiency = Mathf.Clamp((float)resourcesGainedThisWave / resourcesSpentThisWave, 0f, 2f);
        return efficiency * resourceEfficiencyWeight;
    }
    
    /// <summary>
    /// Calculates score based on defender losses
    /// </summary>
    private float CalculateDefenderLossScore()
    {
        // Fewer losses = higher score
        float lossRatio = Mathf.Clamp(1f - (defendersLostThisWave * 0.1f), 0f, 1f);
        return lossRatio * defenderLossWeight;
    }
    
    /// <summary>
    /// Counts initial defenders in the scene
    /// </summary>
    private void CountInitialDefenders()
    {
        Defender[] defenders = FindObjectsByType<Defender>(FindObjectsSortMode.None);
        initialDefenderCount = defenders.Length;
    }
    
    /// <summary>
    /// Gets the current performance level (Struggling, Neutral, Doing Well, Excellent)
    /// </summary>
    public string GetPerformanceLevel()
    {
        if (performanceScore < 30f) return "Struggling";
        if (performanceScore < 50f) return "Neutral";
        if (performanceScore < 70f) return "Doing Well";
        return "Excellent";
    }
    
    /// <summary>
    /// Gets a difficulty multiplier based on performance (0.5 = easier, 2.0 = harder)
    /// </summary>
    public float GetDifficultyMultiplier()
    {
        // Higher performance = higher difficulty
        return 0.5f + (performanceScore / 100f) * 1.5f;
    }
}
