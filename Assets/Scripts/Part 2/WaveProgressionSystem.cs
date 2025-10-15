using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Implements the mathematical wave progression and adaptive scaling system
/// </summary>
public class WaveProgressionSystem : MonoBehaviour
{
    [Header("Wave Progression Settings")]
    [Tooltip("Waves 1-3: Only regular enemies")]
    public int learningPhaseWaves = 3;
    
    [Tooltip("Wave when bombers are introduced")]
    public int bomberIntroductionWave = 4;
    
    [Tooltip("Wave when armored enemies are introduced")]
    public int armoredIntroductionWave = 6;
    
    [Header("Enemy Type Probabilities")]
    [Tooltip("Probability distribution for waves 1-3 (learning phase)")]
    public EnemyTypeDistribution learningPhase = new EnemyTypeDistribution
    {
        regular = 1.0f,
        fast = 0.0f,
        bomber = 0.0f,
        armored = 0.0f
    };
    
    [Tooltip("Probability distribution for waves 4-5 (bomber introduction)")]
    public EnemyTypeDistribution bomberPhase = new EnemyTypeDistribution
    {
        regular = 0.6f,
        fast = 0.2f,
        bomber = 0.2f,
        armored = 0.0f
    };
    
    [Tooltip("Probability distribution for waves 6+ (full complexity)")]
    public EnemyTypeDistribution fullComplexity = new EnemyTypeDistribution
    {
        regular = 0.4f,
        fast = 0.2f,
        bomber = 0.2f,
        armored = 0.2f
    };
    
    [Header("Adaptive Scaling Parameters")]
    [Tooltip("How much performance affects enemy count (0.4 = 40% influence)")]
    [Range(0f, 1f)]
    public float countInfluence = 0.4f;
    
    [Tooltip("How much performance affects enemy type selection (0.5 = 50% influence)")]
    [Range(0f, 1f)]
    public float typeInfluence = 0.5f;
    
    [Tooltip("How much performance affects enemy stats (0.3 = 30% influence)")]
    [Range(0f, 1f)]
    public float statInfluence = 0.3f;
    
    [Header("Scaling Factors")]
    [Tooltip("Base enemy count scaling per wave")]
    public float countScalingFactor = 1.2f;
    
    [Tooltip("Enemy health scaling per wave")]
    public float healthScalingFactor = 1.15f;
    
    [Tooltip("Enemy speed scaling per wave")]
    public float speedScalingFactor = 1.05f;
    
    [Tooltip("Enemy damage scaling per wave")]
    public float damageScalingFactor = 1.1f;
    
    [Header("Performance Tracking")]
    [Tooltip("Reference to performance tracker")]
    public PlayerPerformanceTracker performanceTracker;
    
    [System.Serializable]
    public class EnemyTypeDistribution
    {
        [Range(0f, 1f)] public float regular = 0.5f;
        [Range(0f, 1f)] public float fast = 0.2f;
        [Range(0f, 1f)] public float bomber = 0.2f;
        [Range(0f, 1f)] public float armored = 0.1f;
    }
    
    public enum EnemyType
    {
        Regular,
        Fast,
        Bomber,
        Armored
    }
    
    void Start()
    {
        if (performanceTracker == null)
        {
            performanceTracker = FindFirstObjectByType<PlayerPerformanceTracker>();
        }
    }
    
    /// <summary>
    /// Gets the enemy type to spawn based on wave and performance
    /// </summary>
    public EnemyType GetEnemyTypeForWave(int currentWave)
    {
        EnemyTypeDistribution baseDistribution = GetBaseDistribution(currentWave);
        EnemyTypeDistribution adaptiveDistribution = ApplyAdaptiveScaling(baseDistribution, currentWave);
        
        return SelectEnemyTypeFromDistribution(adaptiveDistribution);
    }
    
    /// <summary>
    /// Gets the base probability distribution for a given wave
    /// </summary>
    EnemyTypeDistribution GetBaseDistribution(int wave)
    {
        if (wave <= learningPhaseWaves)
        {
            return learningPhase;
        }
        else if (wave < armoredIntroductionWave)
        {
            return bomberPhase;
        }
        else
        {
            return fullComplexity;
        }
    }
    
    /// <summary>
    /// Applies adaptive scaling to enemy type probabilities
    /// </summary>
    EnemyTypeDistribution ApplyAdaptiveScaling(EnemyTypeDistribution baseDist, int wave)
    {
        if (performanceTracker == null) return baseDist;
        
        float performanceScore = performanceTracker.performanceScore;
        EnemyTypeDistribution adaptiveDist = new EnemyTypeDistribution();
        
        // Copy base probabilities
        adaptiveDist.regular = baseDist.regular;
        adaptiveDist.fast = baseDist.fast;
        adaptiveDist.bomber = baseDist.bomber;
        adaptiveDist.armored = baseDist.armored;
        
        // Apply performance-based scaling
        if (performanceScore > 70f)
        {
            // High performance: increase difficulty
            float difficultyBonus = (performanceScore - 70f) / 30f * typeInfluence;
            float easeReduction = difficultyBonus * 0.5f;
            
            // Increase hard enemies
            adaptiveDist.bomber = Mathf.Min(0.8f, adaptiveDist.bomber + difficultyBonus);
            if (wave >= armoredIntroductionWave)
            {
                adaptiveDist.armored = Mathf.Min(0.6f, adaptiveDist.armored + difficultyBonus);
            }
            
            // Decrease easy enemies
            adaptiveDist.regular = Mathf.Max(0.1f, adaptiveDist.regular - easeReduction);
            adaptiveDist.fast = Mathf.Max(0.05f, adaptiveDist.fast - easeReduction);
        }
        else if (performanceScore < 30f)
        {
            // Low performance: decrease difficulty
            float easeBonus = (30f - performanceScore) / 30f * typeInfluence;
            float difficultyReduction = easeBonus * 0.5f;
            
            // Increase easy enemies
            adaptiveDist.regular = Mathf.Min(0.9f, adaptiveDist.regular + easeBonus);
            adaptiveDist.fast = Mathf.Min(0.6f, adaptiveDist.fast + easeBonus * 0.5f);
            
            // Decrease hard enemies
            adaptiveDist.bomber = Mathf.Max(0.05f, adaptiveDist.bomber - difficultyReduction);
            adaptiveDist.armored = Mathf.Max(0.05f, adaptiveDist.armored - difficultyReduction);
        }
        
        // Normalize probabilities
        return NormalizeDistribution(adaptiveDist);
    }
    
    /// <summary>
    /// Normalizes probability distribution to sum to 1.0
    /// </summary>
    EnemyTypeDistribution NormalizeDistribution(EnemyTypeDistribution dist)
    {
        float total = dist.regular + dist.fast + dist.bomber + dist.armored;
        if (total <= 0f) return dist;
        
        EnemyTypeDistribution normalized = new EnemyTypeDistribution();
        normalized.regular = dist.regular / total;
        normalized.fast = dist.fast / total;
        normalized.bomber = dist.bomber / total;
        normalized.armored = dist.armored / total;
        
        return normalized;
    }
    
    /// <summary>
    /// Selects enemy type based on probability distribution
    /// </summary>
    EnemyType SelectEnemyTypeFromDistribution(EnemyTypeDistribution dist)
    {
        float randomValue = Random.Range(0f, 1f);
        float cumulativeProbability = 0f;
        
        // Regular
        cumulativeProbability += dist.regular;
        if (randomValue <= cumulativeProbability) return EnemyType.Regular;
        
        // Fast
        cumulativeProbability += dist.fast;
        if (randomValue <= cumulativeProbability) return EnemyType.Fast;
        
        // Bomber
        cumulativeProbability += dist.bomber;
        if (randomValue <= cumulativeProbability) return EnemyType.Bomber;
        
        // Armored (default)
        return EnemyType.Armored;
    }
    
    /// <summary>
    /// Calculates adaptive enemy count for a wave
    /// </summary>
    public int GetAdaptiveEnemyCount(int baseEnemyCount, int currentWave)
    {
        // Base wave scaling
        int baseCount = Mathf.RoundToInt(baseEnemyCount * Mathf.Pow(countScalingFactor, currentWave - 1));
        
        if (performanceTracker == null) return baseCount;
        
        float performanceScore = performanceTracker.performanceScore;
        
        // Calculate performance multiplier
        float countMultiplier = 1.0f;
        if (performanceScore > 50f)
        {
            // High performance: more enemies
            countMultiplier = 1.0f + (performanceScore - 50f) / 50f * countInfluence;
        }
        else
        {
            // Low performance: fewer enemies
            countMultiplier = 1.0f - (50f - performanceScore) / 50f * (countInfluence * 0.75f);
        }
        
        int adaptiveCount = Mathf.RoundToInt(baseCount * countMultiplier);
        return Mathf.Max(1, adaptiveCount); // Ensure at least 1 enemy
    }
    
    /// <summary>
    /// Gets adaptive stat multipliers for enemies
    /// </summary>
    public Vector3 GetAdaptiveStatMultipliers(int currentWave)
    {
        // Base wave scaling
        float baseHealthMultiplier = Mathf.Pow(healthScalingFactor, currentWave - 1);
        float baseSpeedMultiplier = Mathf.Pow(speedScalingFactor, currentWave - 1);
        float baseDamageMultiplier = Mathf.Pow(damageScalingFactor, currentWave - 1);
        
        if (performanceTracker == null)
        {
            return new Vector3(baseHealthMultiplier, baseSpeedMultiplier, baseDamageMultiplier);
        }
        
        float performanceScore = performanceTracker.performanceScore;
        
        // Apply performance-based scaling
        float statMultiplier = 1.0f;
        if (performanceScore > 50f)
        {
            // High performance: stronger enemies
            statMultiplier = 1.0f + (performanceScore - 50f) / 50f * statInfluence;
        }
        else
        {
            // Low performance: weaker enemies
            statMultiplier = 1.0f - (50f - performanceScore) / 50f * (statInfluence * 0.67f);
        }
        
        return new Vector3(
            baseHealthMultiplier * statMultiplier,
            baseSpeedMultiplier * statMultiplier,
            baseDamageMultiplier * statMultiplier
        );
    }
    
    /// <summary>
    /// Gets a description of the current wave
    /// </summary>
    public string GetWaveDescription(int currentWave)
    {
        if (currentWave <= learningPhaseWaves)
        {
            return $"Wave {currentWave}: Learning Phase - Regular enemies only";
        }
        else if (currentWave < armoredIntroductionWave)
        {
            return $"Wave {currentWave}: Bomber Introduction - Regular + Bombers";
        }
        else
        {
            return $"Wave {currentWave}: Full Complexity - All enemy types including Armored";
        }
    }
    
    /// <summary>
    /// Logs wave progression information
    /// </summary>
    public void LogWaveProgression(int currentWave)
    {
        // Debug.Log($"=== WAVE PROGRESSION INFO ===");
        // Debug.Log($"Current Wave: {currentWave}");
        // Debug.Log($"Description: {GetWaveDescription(currentWave)}");
        
        EnemyTypeDistribution baseDist = GetBaseDistribution(currentWave);
        // Debug.Log($"Base Enemy Probabilities:");
        // Debug.Log($"  Regular: {baseDist.regular:P0}");
        // Debug.Log($"  Fast: {baseDist.fast:P0}");
        // Debug.Log($"  Bomber: {baseDist.bomber:P0}");
        // Debug.Log($"  Armored: {baseDist.armored:P0}");
        
        if (performanceTracker != null)
        {
            // Debug.Log($"Performance Score: {performanceTracker.performanceScore:F1}");
            // Debug.Log($"Performance Level: {performanceTracker.GetPerformanceLevel()}");
        }
    }
}
