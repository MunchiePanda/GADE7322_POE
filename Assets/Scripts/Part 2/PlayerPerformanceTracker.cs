using System.Collections.Generic;
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
    
    [Tooltip("Average path progression of enemies (0-100%)")]
    public float averageEnemyPathProgression { get; private set; } = 0f;
    
    [Tooltip("Maximum path progression reached by any enemy")]
    public float maxEnemyPathProgression { get; private set; } = 0f;
    
    [Tooltip("Strategic placement score based on tower positioning")]
    public float strategicPlacementScore { get; private set; } = 50f;
    
    [Tooltip("Resources spent in current wave")]
    public int resourcesSpentThisWave { get; private set; } = 0;
    
    [Tooltip("Resources gained in current wave")]
    public int resourcesGainedThisWave { get; private set; } = 0;
    
    [Header("Performance Calculation")]
    [Tooltip("Overall performance score (0-100, 50 = neutral)")]
    public float performanceScore { get; private set; } = 50f;
    
    // This is basically how I judge if you're good at the game or not
    
    [Tooltip("How much enemy kills affects performance score (30% as per planning document)")]
    public float killEfficiencyWeight = 30f;
    
    [Tooltip("How much defender survival affects performance score (20% as per planning document)")]
    public float defenderSurvivalWeight = 20f;
    
    [Tooltip("How much resource efficiency affects performance score (20% as per planning document)")]
    public float resourceEfficiencyWeight = 20f;
    
    [Tooltip("How much wave completion affects performance score (30% as per planning document)")]
    public float waveCompletionWeight = 30f;
    
    [Header("Debug")]
    [Tooltip("Show performance calculations in console")]
    public bool showDebugInfo = true;
    
    private GameManager gameManager;
    private Tower tower;
    private float waveStartTime;
    private float initialTowerHealth;
    private int initialDefenderCount;
    
    // Path progression tracking
    private List<float> enemyPathProgressions = new List<float>();
    private int totalEnemiesSpawned = 0;
    private int totalEnemiesKilled = 0;
    
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
        // Reset everything for the new wave - I track each wave separately
        waveStartTime = Time.time;
        enemiesKilledThisWave = 0;
        enemiesReachedTowerThisWave = 0;
        defendersLostThisWave = 0;
        resourcesSpentThisWave = 0;
        resourcesGainedThisWave = 0;
        waveCompletionTime = 0f;
        
        // Reset path progression tracking
        enemyPathProgressions.Clear();
        totalEnemiesSpawned = 0;
        totalEnemiesKilled = 0;
        averageEnemyPathProgression = 0f;
        maxEnemyPathProgression = 0f;
        
        if (showDebugInfo)
        {
            // Debug.Log($"ADAPTIVE SCALING: Wave starting - Performance Level: {GetPerformanceLevel()}");
            LogPlayerBehaviorAnalysis();
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
            // Debug.Log($"WAVE COMPLETED in {waveCompletionTime:F1}s - Performance: {GetPerformanceLevel()}");
            LogWaveCompletionSummary();
            LogPerformanceBreakdown();
            LogScalingRecommendations();
        }
    }
    
    /// <summary>
    /// Called when an enemy is killed
    /// </summary>
    public void OnEnemyKilled()
    {
        enemiesKilledThisWave++;
        totalEnemiesKilled++;
        UpdatePerformanceScore();
        
        // Removed excessive logging
    }
    
    /// <summary>
    /// Called when an enemy is spawned
    /// </summary>
    public void OnEnemySpawned()
    {
        totalEnemiesSpawned++;
        // Removed excessive logging
    }
    
    /// <summary>
    /// Called to update enemy path progression
    /// </summary>
    public void OnEnemyPathProgression(float progressionPercentage)
    {
        enemyPathProgressions.Add(progressionPercentage);
        
        // Update max progression
        if (progressionPercentage > maxEnemyPathProgression)
        {
            maxEnemyPathProgression = progressionPercentage;
        }
        
        // Calculate average progression
        if (enemyPathProgressions.Count > 0)
        {
            float totalProgression = 0f;
            foreach (float prog in enemyPathProgressions)
            {
                totalProgression += prog;
            }
            averageEnemyPathProgression = totalProgression / enemyPathProgressions.Count;
        }
        
        UpdatePerformanceScore();
    }
    
    /// <summary>
    /// Called when an enemy reaches the tower
    /// </summary>
    public void OnEnemyReachedTower()
    {
        enemiesReachedTowerThisWave++;
        UpdatePerformanceScore();
        
        // Removed excessive logging
    }
    
    /// <summary>
    /// Called when a defender is lost
    /// </summary>
    public void OnDefenderLost()
    {
        defendersLostThisWave++;
        UpdatePerformanceScore();
        
        // Removed excessive logging
    }
    
    /// <summary>
    /// Called when resources are spent
    /// </summary>
    public void OnResourcesSpent(int amount)
    {
        resourcesSpentThisWave += amount;
        UpdatePerformanceScore();
        
        // Removed excessive logging
    }
    
    /// <summary>
    /// Called when resources are gained
    /// </summary>
    public void OnResourcesGained(int amount)
    {
        resourcesGainedThisWave += amount;
        UpdatePerformanceScore();
        
        // Removed excessive logging
    }
    
    /// <summary>
    /// Updates the overall performance score based on planning document formula:
    /// PerformanceScore = (EnemyKills × 0.3) + (DefenderSurvival × 0.2) + (ResourceEfficiency × 0.2) + (WaveCompletion × 0.3)
    /// </summary>
    private void UpdatePerformanceScore()
    {
        // Calculate each component according to planning document
        float enemyKillsScore = CalculateEnemyKillsScore();
        float defenderSurvivalScore = CalculateDefenderSurvivalScore();
        float resourceEfficiencyScore = CalculateResourceEfficiencyScore();
        float waveCompletionScore = CalculateWaveCompletionScore();
        
        // Apply exact formula from planning document
        performanceScore = (enemyKillsScore * 0.3f) + (defenderSurvivalScore * 0.2f) + 
                          (resourceEfficiencyScore * 0.2f) + (waveCompletionScore * 0.3f);
        
        performanceScore = Mathf.Clamp(performanceScore, 0f, 100f);
    }
    
    /// <summary>
    /// Calculates enemy kills score (0-100)
    /// </summary>
    private float CalculateEnemyKillsScore()
    {
        if (totalEnemiesSpawned == 0) return 50f; // Neutral if no enemies yet
        
        float killRatio = (float)totalEnemiesKilled / totalEnemiesSpawned;
        return killRatio * 100f; // 0-100 scale
    }
    
    /// <summary>
    /// Calculates defender survival score (0-100)
    /// </summary>
    private float CalculateDefenderSurvivalScore()
    {
        if (initialDefenderCount == 0) return 50f; // Neutral if no defenders
        
        int currentDefenders = initialDefenderCount - defendersLostThisWave;
        float survivalRatio = (float)currentDefenders / initialDefenderCount;
        return survivalRatio * 100f; // 0-100 scale
    }
    
    /// <summary>
    /// Calculates resource efficiency score (0-100)
    /// </summary>
    private float CalculateResourceEfficiencyScore()
    {
        if (resourcesSpentThisWave == 0) return 50f; // Neutral if no spending
        
        float efficiency = (float)resourcesGainedThisWave / resourcesSpentThisWave;
        return Mathf.Clamp(efficiency * 50f, 0f, 100f); // Scale to 0-100
    }
    
    /// <summary>
    /// Calculates wave completion score (0-100)
    /// </summary>
    private float CalculateWaveCompletionScore()
    {
        if (waveCompletionTime == 0) return 50f; // Neutral if wave not complete
        
        // Faster completion = higher score
        float expectedTime = 30f; // Expected 30 seconds per wave
        float speedRatio = Mathf.Clamp(expectedTime / waveCompletionTime, 0f, 2f);
        return Mathf.Clamp(speedRatio * 50f, 0f, 100f); // Scale to 0-100
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
    
    /// <summary>
    /// Logs detailed analysis of player behavior and performance
    /// </summary>
    private void LogPlayerBehaviorAnalysis()
    {
        // Debug.Log($"PLAYER BEHAVIOR ANALYSIS:");
        
        // Tower Health Analysis
        if (towerHealthPercentage >= 90f)
        {
            // Debug.Log($"   Tower Health: {towerHealthPercentage:F1}% - Player is protecting tower well!");
        }
        else if (towerHealthPercentage >= 70f)
        {
            // Debug.Log($"   Tower Health: {towerHealthPercentage:F1}% - Some damage taken, but manageable");
        }
        else if (towerHealthPercentage >= 50f)
        {
            // Debug.Log($"   Tower Health: {towerHealthPercentage:F1}% - Significant damage! Player needs better defense");
        }
        else
        {
            // Debug.Log($"   Tower Health: {towerHealthPercentage:F1}% - Critical damage! Player is struggling");
        }
        
        // Performance Level Analysis
        string performanceLevel = GetPerformanceLevel();
        switch (performanceLevel)
        {
            case "Struggling":
                // Debug.Log($"   Performance: Player is struggling - System will make enemies EASIER");
                break;
            case "Neutral":
                // Debug.Log($"   Performance: Player is balanced - System will use NORMAL scaling");
                break;
            case "Doing Well":
                // Debug.Log($"   Performance: Player is doing well - System will make enemies HARDER");
                break;
            case "Excellent":
                // Debug.Log($"   Performance: Player is excellent - System will make enemies MUCH HARDER");
                break;
        }
    }
    
    /// <summary>
    /// Logs comprehensive wave completion summary
    /// </summary>
    private void LogWaveCompletionSummary()
    {
        // Debug.Log($"WAVE SUMMARY:");
        // Debug.Log($"   Completion Time: {waveCompletionTime:F1}s");
        // Debug.Log($"   Enemies Killed: {enemiesKilledThisWave}");
        // Debug.Log($"   Enemies Reached Tower: {enemiesReachedTowerThisWave}");
        // Debug.Log($"   Defenders Lost: {defendersLostThisWave}");
        // Debug.Log($"   Resources Spent: {resourcesSpentThisWave}");
        // Debug.Log($"   Resources Gained: {resourcesGainedThisWave}");
        // Debug.Log($"   Tower Health: {towerHealthPercentage:F1}%");
        // Debug.Log($"   Average Enemy Path Progression: {averageEnemyPathProgression:F1}%");
        // Debug.Log($"   Max Enemy Path Progression: {maxEnemyPathProgression:F1}%");
        // Debug.Log($"   Strategic Placement Score: {strategicPlacementScore:F1}");
    }
    
    /// <summary>
    /// Logs detailed performance metric breakdown
    /// </summary>
    private void LogPerformanceBreakdown()
    {
        float enemyKillsScore = CalculateEnemyKillsScore();
        float defenderSurvivalScore = CalculateDefenderSurvivalScore();
        float resourceEfficiencyScore = CalculateResourceEfficiencyScore();
        float waveCompletionScore = CalculateWaveCompletionScore();
        
        // Debug.Log($"PERFORMANCE BREAKDOWN:");
        // Debug.Log($"   Enemy Kills Score: {enemyKillsScore:F1} - {(enemyKillsScore/100*100):F1}%");
        // Debug.Log($"   Defender Survival Score: {defenderSurvivalScore:F1} - {(defenderSurvivalScore/100*100):F1}%");
        // Debug.Log($"   Resource Efficiency Score: {resourceEfficiencyScore:F1} - {(resourceEfficiencyScore/100*100):F1}%");
        // Debug.Log($"   Wave Completion Score: {waveCompletionScore:F1} - {(waveCompletionScore/100*100):F1}%");
        // Debug.Log($"   TOTAL PERFORMANCE SCORE: {performanceScore:F1}/100");
    }
    
    /// <summary>
    /// Logs scaling recommendations based on performance
    /// </summary>
    private void LogScalingRecommendations()
    {
        // Debug.Log($"SCALING RECOMMENDATIONS:");
        
        if (performanceScore >= 70f)
        {
            // Debug.Log($"   Player is EXCELLENT - Increasing difficulty:");
            // Debug.Log($"      - More enemies (1.5x - 3x more)");
            // Debug.Log($"      - Stronger enemies (1.2x - 2.5x stats)");
            // Debug.Log($"      - More fast enemies (up to 4x frequency)");
            // Debug.Log($"      - Faster enemies (up to 1.5x speed)");
            // Debug.Log($"      - Tank enemies first (30% chance)");
        }
        else if (performanceScore >= 50f)
        {
            // Debug.Log($"   Player is DOING WELL - Moderate difficulty increase:");
            // Debug.Log($"      - Slightly more enemies");
            // Debug.Log($"      - Slightly stronger enemies");
            // Debug.Log($"      - Some fast enemy frequency increase");
        }
        else if (performanceScore >= 30f)
        {
            // Debug.Log($"   Player is NEUTRAL - Normal scaling:");
            // Debug.Log($"      - Standard enemy count");
            // Debug.Log($"      - Standard enemy stats");
            // Debug.Log($"      - Normal enemy frequency");
        }
        else
        {
            // Debug.Log($"   Player is STRUGGLING - Decreasing difficulty:");
            // Debug.Log($"      - Fewer enemies (0.3x - 0.7x)");
            // Debug.Log($"      - Weaker enemies (0.4x - 0.8x stats)");
            // Debug.Log($"      - Base enemy frequency only");
            // Debug.Log($"      - Only basic enemies (no tank enemies)");
        }
    }
}
