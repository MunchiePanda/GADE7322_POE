using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages wave progression with proper enemy type unlocking and adaptive scaling
/// </summary>
public class WaveProgressionManager : MonoBehaviour
{
    [Header("Wave Progression")]
    [Tooltip("Current wave number")]
    public int currentWave = 1;
    
    [Tooltip("Maximum wave number")]
    public int maxWave = 50;
    
    [Header("Enemy Type Unlocking")]
    [Tooltip("Wave when bomber enemies are introduced")]
    public int bomberUnlockWave = 4;
    
    [Tooltip("Wave when armored enemies are introduced")]
    public int armoredUnlockWave = 8;
    
    [Tooltip("Wave when fast enemies are introduced")]
    public int fastUnlockWave = 2;
    
    [Header("Enemy Scaling")]
    [Tooltip("Base enemy count for wave 1")]
    public int baseEnemyCount = 5;
    
    [Tooltip("Enemy count scaling per wave")]
    public float enemyCountScaling = 1.3f;
    
    [Tooltip("Health scaling per wave")]
    public float healthScaling = 1.15f;
    
    [Tooltip("Speed scaling per wave")]
    public float speedScaling = 1.05f;
    
    [Header("Adaptive Scaling")]
    [Tooltip("Reference to performance tracker")]
    public PlayerPerformanceTracker performanceTracker;
    
    [Tooltip("How much performance affects enemy count (0-1)")]
    public float performanceCountInfluence = 0.4f;
    
    [Tooltip("How much performance affects enemy difficulty (0-1)")]
    public float performanceDifficultyInfluence = 0.3f;
    
    [Tooltip("Minimum enemy count multiplier")]
    public float minEnemyMultiplier = 0.5f;
    
    [Tooltip("Maximum enemy count multiplier")]
    public float maxEnemyMultiplier = 2.0f;
    
    [Header("Enemy Type Probabilities")]
    [Tooltip("Probability of each enemy type based on wave")]
    public EnemyTypeProbabilities[] waveProbabilities;
    
    [System.Serializable]
    public class EnemyTypeProbabilities
    {
        [Tooltip("Wave range this applies to")]
        public int minWave;
        public int maxWave;
        
        [Tooltip("Probability of basic enemies")]
        [Range(0f, 1f)] public float basicEnemyChance = 0.5f;
        
        [Tooltip("Probability of fast enemies")]
        [Range(0f, 1f)] public float fastEnemyChance = 0.2f;
        
        [Tooltip("Probability of bomber enemies")]
        [Range(0f, 1f)] public float bomberEnemyChance = 0.0f;
        
        [Tooltip("Probability of armored enemies")]
        [Range(0f, 1f)] public float armoredEnemyChance = 0.0f;
    }
    
    private void Start()
    {
        InitializeWaveProbabilities();
    }
    
    /// <summary>
    /// Initializes the wave probability system
    /// </summary>
    void InitializeWaveProbabilities()
    {
        if (waveProbabilities == null || waveProbabilities.Length == 0)
        {
            // Create default wave probabilities
            waveProbabilities = new EnemyTypeProbabilities[]
            {
                // Waves 1-3: Only basic enemies
                new EnemyTypeProbabilities
                {
                    minWave = 1,
                    maxWave = 3,
                    basicEnemyChance = 1.0f,
                    fastEnemyChance = 0.0f,
                    bomberEnemyChance = 0.0f,
                    armoredEnemyChance = 0.0f
                },
                
                // Waves 4-7: Basic + Fast + Bomber
                new EnemyTypeProbabilities
                {
                    minWave = 4,
                    maxWave = 7,
                    basicEnemyChance = 0.4f,
                    fastEnemyChance = 0.3f,
                    bomberEnemyChance = 0.3f,
                    armoredEnemyChance = 0.0f
                },
                
                // Waves 8+: All enemy types
                new EnemyTypeProbabilities
                {
                    minWave = 8,
                    maxWave = 999,
                    basicEnemyChance = 0.2f,
                    fastEnemyChance = 0.2f,
                    bomberEnemyChance = 0.3f,
                    armoredEnemyChance = 0.3f
                }
            };
        }
    }
    
    /// <summary>
    /// Gets the enemy count for the current wave with adaptive scaling
    /// </summary>
    public int GetEnemyCountForWave()
    {
        // Base enemy count with wave scaling
        int baseCount = Mathf.RoundToInt(baseEnemyCount * Mathf.Pow(enemyCountScaling, currentWave - 1));
        
        // Apply adaptive scaling based on performance
        float performanceMultiplier = GetPerformanceMultiplier();
        int adaptiveCount = Mathf.RoundToInt(baseCount * performanceMultiplier);
        
        // Clamp to reasonable bounds
        adaptiveCount = Mathf.RoundToInt(Mathf.Clamp(adaptiveCount, 
            baseCount * minEnemyMultiplier, 
            baseCount * maxEnemyMultiplier));
        
        Debug.Log($"Wave {currentWave}: Base count {baseCount}, Performance multiplier {performanceMultiplier:F2}, Final count {adaptiveCount}");
        
        return adaptiveCount;
    }
    
    /// <summary>
    /// Gets the performance multiplier for adaptive scaling
    /// </summary>
    float GetPerformanceMultiplier()
    {
        if (performanceTracker == null) return 1.0f;
        
        float performanceScore = performanceTracker.performanceScore;
        
        // Performance-based scaling
        if (performanceScore > 70f)
        {
            // High performance: more enemies
            return 1.0f + (performanceScore - 70f) / 30f * performanceCountInfluence;
        }
        else if (performanceScore < 30f)
        {
            // Low performance: fewer enemies
            return 1.0f - (30f - performanceScore) / 30f * performanceCountInfluence;
        }
        else
        {
            // Average performance: standard count
            return 1.0f;
        }
    }
    
    /// <summary>
    /// Selects an enemy type based on wave progression and performance
    /// </summary>
    public GameObject SelectEnemyType(GameObject basicEnemy, GameObject fastEnemy, GameObject bomberEnemy, GameObject armoredEnemy)
    {
        // Get probabilities for current wave
        EnemyTypeProbabilities probabilities = GetProbabilitiesForWave(currentWave);
        
        // Apply performance-based adjustments
        probabilities = AdjustProbabilitiesForPerformance(probabilities);
        
        // Select enemy type based on probabilities
        float random = Random.Range(0f, 1f);
        float cumulative = 0f;
        
        // Basic enemies
        cumulative += probabilities.basicEnemyChance;
        if (random <= cumulative)
        {
            Debug.Log($"Wave {currentWave}: Selected BASIC enemy");
            return basicEnemy;
        }
        
        // Fast enemies
        cumulative += probabilities.fastEnemyChance;
        if (random <= cumulative)
        {
            Debug.Log($"Wave {currentWave}: Selected FAST enemy");
            return fastEnemy;
        }
        
        // Bomber enemies
        cumulative += probabilities.bomberEnemyChance;
        if (random <= cumulative)
        {
            Debug.Log($"Wave {currentWave}: Selected BOMBER enemy");
            return bomberEnemy;
        }
        
        // Armored enemies
        cumulative += probabilities.armoredEnemyChance;
        if (random <= cumulative)
        {
            Debug.Log($"Wave {currentWave}: Selected ARMORED enemy");
            return armoredEnemy;
        }
        
        // Fallback to basic enemy
        Debug.Log($"Wave {currentWave}: Fallback to BASIC enemy");
        return basicEnemy;
    }
    
    /// <summary>
    /// Gets probabilities for a specific wave
    /// </summary>
    EnemyTypeProbabilities GetProbabilitiesForWave(int wave)
    {
        foreach (var prob in waveProbabilities)
        {
            if (wave >= prob.minWave && wave <= prob.maxWave)
            {
                return prob;
            }
        }
        
        // Fallback to last probability set
        return waveProbabilities[waveProbabilities.Length - 1];
    }
    
    /// <summary>
    /// Adjusts probabilities based on player performance
    /// </summary>
    EnemyTypeProbabilities AdjustProbabilitiesForPerformance(EnemyTypeProbabilities baseProb)
    {
        if (performanceTracker == null) return baseProb;
        
        float performanceScore = performanceTracker.performanceScore;
        EnemyTypeProbabilities adjusted = new EnemyTypeProbabilities();
        
        // Copy base probabilities
        adjusted.minWave = baseProb.minWave;
        adjusted.maxWave = baseProb.maxWave;
        adjusted.basicEnemyChance = baseProb.basicEnemyChance;
        adjusted.fastEnemyChance = baseProb.fastEnemyChance;
        adjusted.bomberEnemyChance = baseProb.bomberEnemyChance;
        adjusted.armoredEnemyChance = baseProb.armoredEnemyChance;
        
        // Adjust based on performance
        if (performanceScore > 70f)
        {
            // High performance: more difficult enemies
            float difficultyBoost = (performanceScore - 70f) / 30f * performanceDifficultyInfluence;
            adjusted.basicEnemyChance *= (1f - difficultyBoost);
            adjusted.fastEnemyChance *= (1f + difficultyBoost);
            adjusted.bomberEnemyChance *= (1f + difficultyBoost);
            adjusted.armoredEnemyChance *= (1f + difficultyBoost);
        }
        else if (performanceScore < 30f)
        {
            // Low performance: easier enemies
            float difficultyReduction = (30f - performanceScore) / 30f * performanceDifficultyInfluence;
            adjusted.basicEnemyChance *= (1f + difficultyReduction);
            adjusted.fastEnemyChance *= (1f - difficultyReduction);
            adjusted.bomberEnemyChance *= (1f - difficultyReduction);
            adjusted.armoredEnemyChance *= (1f - difficultyReduction);
        }
        
        // Normalize probabilities to ensure they sum to 1
        float total = adjusted.basicEnemyChance + adjusted.fastEnemyChance + 
                     adjusted.bomberEnemyChance + adjusted.armoredEnemyChance;
        
        if (total > 0f)
        {
            adjusted.basicEnemyChance /= total;
            adjusted.fastEnemyChance /= total;
            adjusted.bomberEnemyChance /= total;
            adjusted.armoredEnemyChance /= total;
        }
        
        return adjusted;
    }
    
    /// <summary>
    /// Gets scaling factors for enemy stats
    /// </summary>
    public EnemyScaling GetEnemyScaling()
    {
        float waveScaling = Mathf.Pow(healthScaling, currentWave - 1);
        float speedScalingValue = Mathf.Pow(speedScaling, currentWave - 1);
        
        // Apply performance-based adjustments
        if (performanceTracker != null)
        {
            float performanceScore = performanceTracker.performanceScore;
            if (performanceScore > 70f)
            {
                waveScaling *= 1.1f; // 10% harder for high performers
                speedScalingValue *= 1.05f;
            }
            else if (performanceScore < 30f)
            {
                waveScaling *= 0.9f; // 10% easier for struggling players
                speedScalingValue *= 0.95f;
            }
        }
        
        return new EnemyScaling
        {
            healthMultiplier = waveScaling,
            speedMultiplier = speedScalingValue,
            damageMultiplier = waveScaling * 0.8f // Damage scales slower than health
        };
    }
    
    /// <summary>
    /// Advances to the next wave
    /// </summary>
    public void AdvanceWave()
    {
        currentWave++;
        Debug.Log($"Advanced to Wave {currentWave}");
    }
    
    /// <summary>
    /// Checks if a specific enemy type is unlocked for the current wave
    /// </summary>
    public bool IsEnemyTypeUnlocked(EnemyType enemyType)
    {
        switch (enemyType)
        {
            case EnemyType.Basic:
                return true; // Always available
            case EnemyType.Fast:
                return currentWave >= fastUnlockWave;
            case EnemyType.Bomber:
                return currentWave >= bomberUnlockWave;
            case EnemyType.Armored:
                return currentWave >= armoredUnlockWave;
            default:
                return false;
        }
    }
    
    /// <summary>
    /// Gets the current wave information
    /// </summary>
    public WaveInfo GetCurrentWaveInfo()
    {
        return new WaveInfo
        {
            waveNumber = currentWave,
            enemyCount = GetEnemyCountForWave(),
            scaling = GetEnemyScaling(),
            probabilities = GetProbabilitiesForWave(currentWave)
        };
    }
}

/// <summary>
/// Enemy type enumeration
/// </summary>
public enum EnemyType
{
    Basic,
    Fast,
    Bomber,
    Armored
}

/// <summary>
/// Enemy scaling information
/// </summary>
[System.Serializable]
public class EnemyScaling
{
    public float healthMultiplier = 1f;
    public float speedMultiplier = 1f;
    public float damageMultiplier = 1f;
}

/// <summary>
/// Wave information structure
/// </summary>
[System.Serializable]
public class WaveInfo
{
    public int waveNumber;
    public int enemyCount;
    public EnemyScaling scaling;
    public WaveProgressionManager.EnemyTypeProbabilities probabilities;
}
