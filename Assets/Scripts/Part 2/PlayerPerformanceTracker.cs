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
    
    [Tooltip("How much path progression affects performance score")]
    public float pathProgressionWeight = 15f;
    
    [Tooltip("How much strategic placement affects performance score")]
    public float strategicPlacementWeight = 10f;
    
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
    /// Updates the overall performance score based on all metrics
    /// </summary>
    private void UpdatePerformanceScore()
    {
        // This is where I calculate how well you're doing - it's pretty complex
        float towerHealthScore = CalculateTowerHealthScore();
        float killEfficiencyScore = CalculateKillEfficiencyScore();
        float waveSpeedScore = CalculateWaveSpeedScore();
        float resourceEfficiencyScore = CalculateResourceEfficiencyScore();
        float defenderLossScore = CalculateDefenderLossScore();
        float pathProgressionScore = CalculatePathProgressionScore();
        float strategicPlacementScore = CalculateStrategicPlacementScore();
        
        performanceScore = towerHealthScore + killEfficiencyScore + waveSpeedScore + 
                          resourceEfficiencyScore + defenderLossScore + pathProgressionScore + strategicPlacementScore;
        
        performanceScore = Mathf.Clamp(performanceScore, 0f, 100f);
        
        // Removed detailed performance logging
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
    /// Calculates score based on enemy path progression
    /// </summary>
    private float CalculatePathProgressionScore()
    {
        if (totalEnemiesSpawned == 0) return pathProgressionWeight * 0.5f; // Neutral if no enemies
        
        // Lower average progression = higher score (enemies not getting far)
        float progressionPenalty = averageEnemyPathProgression / 100f;
        float score = (1f - progressionPenalty) * pathProgressionWeight;
        
        // Bonus for preventing enemies from reaching the tower
        if (enemiesReachedTowerThisWave == 0 && totalEnemiesSpawned > 0)
        {
            score += pathProgressionWeight * 0.2f; // 20% bonus for perfect defense
        }
        
        return Mathf.Clamp(score, 0f, pathProgressionWeight);
    }
    
    /// <summary>
    /// Calculates score based on strategic placement and behavior
    /// </summary>
    private float CalculateStrategicPlacementScore()
    {
        // Analyze strategic behavior
        float strategicScore = 50f; // Base neutral score
        
        // Bonus for efficient resource usage
        if (resourcesSpentThisWave > 0)
        {
            float resourceEfficiency = (float)resourcesGainedThisWave / resourcesSpentThisWave;
            if (resourceEfficiency > 1.5f) strategicScore += 20f; // Good resource management
            else if (resourceEfficiency < 0.5f) strategicScore -= 15f; // Poor resource management
        }
        
        // Bonus for quick wave completion (shows good strategy)
        if (waveCompletionTime > 0)
        {
            if (waveCompletionTime < 20f) strategicScore += 15f; // Very fast completion
            else if (waveCompletionTime > 60f) strategicScore -= 10f; // Slow completion
        }
        
        // Bonus for low defender losses (shows good placement)
        if (defendersLostThisWave == 0) strategicScore += 10f;
        else if (defendersLostThisWave > 3) strategicScore -= 15f;
        
        // Penalty for enemies reaching tower (shows poor defense strategy)
        if (enemiesReachedTowerThisWave > 0)
        {
            strategicScore -= enemiesReachedTowerThisWave * 5f;
        }
        
        return Mathf.Clamp(strategicScore / 100f * strategicPlacementWeight, 0f, strategicPlacementWeight);
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
        float towerHealthScore = CalculateTowerHealthScore();
        float killEfficiencyScore = CalculateKillEfficiencyScore();
        float waveSpeedScore = CalculateWaveSpeedScore();
        float resourceEfficiencyScore = CalculateResourceEfficiencyScore();
        float defenderLossScore = CalculateDefenderLossScore();
        float pathProgressionScore = CalculatePathProgressionScore();
        float strategicPlacementScore = CalculateStrategicPlacementScore();
        
        // Debug.Log($"PERFORMANCE BREAKDOWN:");
        // Debug.Log($"   Tower Health Score: {towerHealthScore:F1}/{towerHealthWeight} - {(towerHealthScore/towerHealthWeight*100):F1}%");
        // Debug.Log($"   Kill Efficiency Score: {killEfficiencyScore:F1}/{killEfficiencyWeight} - {(killEfficiencyScore/killEfficiencyWeight*100):F1}%");
        // Debug.Log($"   Wave Speed Score: {waveSpeedScore:F1}/{waveSpeedWeight} - {(waveSpeedScore/waveSpeedWeight*100):F1}%");
        // Debug.Log($"   Resource Efficiency Score: {resourceEfficiencyScore:F1}/{resourceEfficiencyWeight} - {(resourceEfficiencyScore/resourceEfficiencyWeight*100):F1}%");
        // Debug.Log($"   Defender Loss Score: {defenderLossScore:F1}/{defenderLossWeight} - {(defenderLossScore/defenderLossWeight*100):F1}%");
        // Debug.Log($"   Path Progression Score: {pathProgressionScore:F1}/{pathProgressionWeight} - {(pathProgressionScore/pathProgressionWeight*100):F1}%");
        // Debug.Log($"   Strategic Placement Score: {strategicPlacementScore:F1}/{strategicPlacementWeight} - {(strategicPlacementScore/strategicPlacementWeight*100):F1}%");
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
