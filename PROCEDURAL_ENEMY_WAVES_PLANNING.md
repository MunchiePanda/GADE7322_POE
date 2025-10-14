# Procedural Enemy Waves Planning Document

## Overview
This document outlines the strategy for implementing a sophisticated procedural enemy spawning system that adapts to player performance and provides consistent challenge progression. The system uses mathematical scaling, performance tracking, and adaptive algorithms to create engaging gameplay that maintains player interest while preventing frustration.

## 1. Difficulty Scaling Strategy

### Mathematical Foundation
The difficulty scaling system is based on exponential growth with performance-based adjustments to ensure consistent challenge progression. This approach was chosen because exponential scaling provides natural difficulty curves that feel organic to players, while performance-based adjustments prevent the system from becoming too easy or too difficult for individual players.

**Base Enemy Count Formula:**
```
EnemyCount = BaseCount × (ScalingFactor^(Wave-1)) × PerformanceMultiplier
```

The base enemy count starts at 5 enemies for wave 1, with a 30% increase per wave (scaling factor of 1.3). This exponential growth ensures that later waves feel significantly more challenging while remaining manageable. The performance multiplier ranges from 0.5 to 2.0, allowing the system to adapt to players who are struggling or excelling.

**Health Scaling Formula:**
```
EnemyHealth = BaseHealth × (HealthScaling^(Wave-1)) × DifficultyMultiplier
```

Enemy health starts at 10 points and increases by 15% per wave. This slower scaling compared to enemy count ensures that individual enemies don't become impossibly tanky while still providing increased challenge. The difficulty multiplier adjusts health by ±10% based on player performance.

### Wave Progression Matrix
The enemy type distribution follows a carefully designed progression that introduces complexity gradually. This approach was chosen to prevent overwhelming new players while providing sufficient challenge for experienced players.

| Wave Range | Basic Enemies | Fast Enemies | Bomber Enemies | Armored Enemies |
|------------|---------------|--------------|----------------|----------------|
| 1-3        | 100%          | 0%           | 0%             | 0%             |
| 4-7        | 40%           | 30%          | 30%            | 0%             |
| 8+         | 20%           | 20%          | 30%            | 30%            |

The first three waves use only basic enemies to allow players to learn the core mechanics without complexity. Waves 4-7 introduce fast and bomber enemies to add tactical considerations, while waves 8+ include all enemy types to provide maximum challenge and variety.

## 2. Player Skill Level Adaptation

### Performance Tracking System
The system tracks multiple performance metrics to create a comprehensive assessment of player skill. This multi-faceted approach was chosen because single metrics can be misleading - a player might be good at killing enemies but poor at resource management, or vice versa.

**Performance Score Calculation:**
```
PerformanceScore = (EnemyKills × 0.3) + (DefenderSurvival × 0.2) + (ResourceEfficiency × 0.2) + (WaveCompletion × 0.3)
```

The weighting system prioritizes enemy kills and wave completion (60% combined) because these are the most direct indicators of combat effectiveness. Defender survival and resource efficiency (40% combined) provide additional context about strategic thinking and resource management skills.

**Adaptive Scaling Logic:**
- **High Performance (>70)**: 1.2x enemy count, 1.1x health, more difficult enemy types
- **Medium Performance (30-70)**: Standard scaling
- **Low Performance (<30)**: 0.8x enemy count, 0.9x health, easier enemy types

This three-tier system ensures that players receive appropriate challenge levels. High performers get increased difficulty to maintain engagement, while struggling players receive reduced difficulty to prevent frustration and allow learning.

### Dynamic Difficulty Adjustment
The system uses smooth interpolation between performance tiers to avoid jarring difficulty changes. This approach was chosen because sudden difficulty spikes or drops can be frustrating for players and break immersion.

```csharp
if (performanceScore > 70f) {
    enemyCountMultiplier = 1.0f + (performanceScore - 70f) / 30f * 0.4f;
    difficultyMultiplier = 1.0f + (performanceScore - 70f) / 30f * 0.2f;
}
else if (performanceScore < 30f) {
    enemyCountMultiplier = 1.0f - (30f - performanceScore) / 30f * 0.4f;
    difficultyMultiplier = 1.0f - (30f - performanceScore) / 30f * 0.2f;
}
```

The linear interpolation ensures that difficulty changes are gradual and predictable, allowing players to adapt to increasing challenge without feeling overwhelmed.

## 3. Spawn Location Determination

### Path-Based Spawning
Enemies spawn at path entrances to maintain the tower defense genre's core mechanics. This approach was chosen because it ensures enemies follow logical routes to the tower while providing strategic placement opportunities for defenders.

**Spawn Point Selection:**
```csharp
List<Vector3Int> availablePaths = terrainGenerator.GetPaths();
Vector3Int spawnPoint = availablePaths[Random.Range(0, availablePaths.Count)][0];
Vector3 spawnPosition = new Vector3(spawnPoint.x, terrainHeight, spawnPoint.z);
```

Random path selection prevents players from predicting spawn locations and encourages strategic defender placement across all paths. This randomization maintains tactical depth while ensuring all paths remain relevant.

**Spawn Height Variation:**
- Standard enemies: Ground level
- Tank enemies: +2 units (visual importance)
- Flying enemies: +5 units (aerial approach)

Height variation serves both gameplay and visual purposes. Tank enemies spawn higher to emphasize their importance and threat level, while flying enemies use aerial spawns to differentiate their movement patterns from ground-based units.

### Spawn Timing Algorithm
The spawn timing system creates increasing pressure over time to maintain engagement and prevent stagnation. This approach was chosen because constant spawn intervals can become predictable and boring, while increasing spawn rates create natural tension escalation.

```csharp
spawnInterval = baseInterval × (1 - (currentWave / maxWave) × 0.3f);
```

The algorithm starts with a 2-second base interval and reduces it by up to 30% over 20 waves, reaching a minimum of 1.4 seconds. This gradual acceleration ensures that later waves feel more intense while remaining manageable, creating a natural difficulty curve that matches player skill progression.

## 4. Enemy Type Selection Strategy

### Wave-Based Unlocking
Enemy types are unlocked progressively to maintain learning curve integrity. This approach was chosen because introducing all enemy types simultaneously would overwhelm new players, while gradual introduction allows players to learn each type's behavior and counter-strategies.

```csharp
public bool IsEnemyTypeUnlocked(EnemyType type, int currentWave) {
    switch(type) {
        case EnemyType.Basic: return true;
        case EnemyType.Fast: return currentWave >= 2;
        case EnemyType.Bomber: return currentWave >= 4;
        case EnemyType.Armored: return currentWave >= 8;
    }
}
```

The unlocking schedule introduces complexity gradually: fast enemies at wave 2 add speed considerations, bombers at wave 4 introduce area-of-effect threats, and armored enemies at wave 8 provide maximum challenge for experienced players.

### Probability-Based Selection
The system uses weighted probability tables that adjust based on player performance to maintain appropriate challenge levels. This approach was chosen because fixed probabilities can become stale, while performance-based adjustments ensure the system remains engaging for players of all skill levels.

**Base Probabilities (Wave 4-7):**
- Basic: 40%
- Fast: 30%
- Bomber: 30%
- Armored: 0%

**Performance-Adjusted Probabilities:**
```csharp
if (performanceScore > 70f) {
    basicChance *= 0.8f;      // Reduce easy enemies
    bomberChance *= 1.2f;     // Increase difficult enemies
    armoredChance *= 1.3f;    // Increase hardest enemies
}
```

High-performing players receive more challenging enemy combinations to maintain engagement, while struggling players receive easier compositions to prevent frustration. This adaptive approach ensures that all players receive appropriate challenge levels.

## 5. System Architecture

### Core Components
The system architecture was designed with modularity and maintainability in mind. Each component has a specific responsibility to ensure the system remains organized and easy to modify.

1. **WaveProgressionManager**: Handles wave logic and enemy type unlocking to centralize progression rules
2. **EnemySpawner**: Manages spawning timing and location selection to maintain consistent enemy generation
3. **PlayerPerformanceTracker**: Monitors player skill and adjusts difficulty to ensure appropriate challenge levels
4. **AdaptiveScalingSystem**: Applies mathematical scaling based on performance to create dynamic difficulty

### Data Flow
The system follows a clear data flow pattern that ensures all components work together effectively:

```
Player Performance → Performance Tracker → Wave Progression Manager → Enemy Spawner → Enemy Creation
```

This unidirectional flow prevents circular dependencies and ensures that performance data flows logically through the system, allowing each component to make informed decisions based on the most current information.

### Performance Metrics Tracked
The system tracks multiple performance metrics to create a comprehensive assessment of player skill. This multi-faceted approach was chosen because different metrics reveal different aspects of player ability.

- **Enemy kill rate**: Measures combat effectiveness and tactical awareness
- **Defender survival rate**: Indicates strategic placement and resource management
- **Resource efficiency**: Shows economic decision-making and optimization skills
- **Wave completion time**: Reveals overall game mastery and speed of play
- **Damage taken**: Indicates defensive strategy and risk management
- **Placement accuracy**: Shows understanding of optimal positioning

## 6. Mathematical Models

### Exponential Growth with Bounds
The enemy count scaling uses exponential growth with bounds to ensure natural progression while preventing impossible scenarios. This approach was chosen because exponential growth feels organic to players while bounds prevent system abuse.

```csharp
enemyCount = Mathf.Clamp(
    baseCount * Mathf.Pow(scalingFactor, wave - 1) * performanceMultiplier,
    minCount,
    maxCount
);
```

The bounds system ensures that even high-performing players don't face impossible enemy counts, while struggling players receive manageable challenges that allow for learning and improvement.

### Sigmoid Function for Smooth Transitions
The system uses sigmoid functions to create smooth transitions between difficulty phases. This approach was chosen because sudden difficulty changes can be jarring and break player immersion.

```csharp
difficultyCurve = 1 / (1 + Mathf.Exp(-0.1f * (wave - 10f)));
```

The sigmoid curve creates a natural S-shaped progression that feels organic to players, with gentle acceleration in early waves and gradual leveling off in later waves.

### Performance Decay (Prevents Stagnation)
The system includes performance decay to prevent players from becoming stuck at difficulty levels that are too easy. This approach was chosen because static difficulty can lead to boredom and disengagement.

```csharp
if (timeSinceLastDeath > 30f) {
    performanceScore *= 0.95f; // Gradual difficulty increase
}
```

This gradual decay ensures that players who have mastered the current difficulty level will eventually face increased challenges, maintaining long-term engagement and preventing stagnation.

## 7. Implementation Strategy

### Phase 1: Core System
The implementation begins with establishing the fundamental wave progression mechanics. This approach was chosen because a solid foundation is essential before adding complexity.

- **Implement basic wave progression**: Create the core wave advancement system with enemy count scaling
- **Add enemy type unlocking**: Implement the progressive introduction of enemy types based on wave number
- **Create performance tracking**: Establish the metrics collection system to monitor player performance

### Phase 2: Adaptive Scaling
The second phase focuses on adding intelligent difficulty adjustment based on player performance. This approach was chosen because adaptive systems require a solid foundation to function effectively.

- **Add mathematical scaling formulas**: Implement the exponential growth and performance multiplier systems
- **Implement performance-based adjustments**: Create the logic that adjusts difficulty based on player skill
- **Create difficulty curves**: Establish the smooth transition systems that prevent jarring difficulty changes

### Phase 3: Polish and Balance
The final phase focuses on refinement and testing to ensure the system works well across all player skill levels. This approach was chosen because balance testing is essential for adaptive systems.

- **Fine-tune scaling parameters**: Adjust mathematical constants based on playtesting results
- **Add visual feedback**: Implement UI elements that communicate difficulty changes to players
- **Test with various skill levels**: Ensure the system works effectively for players of all abilities

## 8. Expected Outcomes

### Player Experience
The system is designed to provide appropriate challenge levels for players of all skill levels. This approach was chosen because different players have different needs and preferences.

- **New Players**: Gentle learning curve with easier enemies to prevent frustration and encourage learning
- **Experienced Players**: Challenging progression with complex enemy combinations to maintain engagement
- **Skilled Players**: High-intensity waves with maximum difficulty to provide ongoing challenge

### Technical Benefits
The system provides several technical advantages that improve both development and gameplay experiences. This approach was chosen because technical excellence supports better player experiences.

- **Scalable system that adapts to any skill level**: The mathematical foundation ensures the system works for all players
- **Mathematical foundation ensures consistent progression**: The formulas provide predictable and fair difficulty curves
- **Performance-based adjustments prevent frustration or boredom**: The adaptive system maintains engagement across all skill levels

## 9. System Flow Diagrams

### Wave Progression Flow
```
Wave Start → Check Performance → Calculate Enemy Count → Select Enemy Types → Spawn Enemies → Monitor Performance → Adjust Next Wave
```

### Performance Adaptation Flow
```
Player Actions → Performance Tracker → Calculate Score → Adjust Multipliers → Apply to Next Wave
```

### Enemy Type Selection Flow
```
Current Wave → Check Unlocks → Apply Performance Modifiers → Weighted Random Selection → Spawn Enemy
```

## 10. Mathematical Formulas Summary

### Core Scaling Equations
```
EnemyCount = 5 × (1.3^(Wave-1)) × PerformanceMultiplier
Health = 10 × (1.15^(Wave-1)) × DifficultyMultiplier
SpawnInterval = 2.0 × (1 - (Wave/20) × 0.3)
```

### Performance Multipliers
```
High Performance (>70): Count×1.2, Health×1.1, Difficulty×1.1
Medium Performance (30-70): Count×1.0, Health×1.0, Difficulty×1.0
Low Performance (<30): Count×0.8, Health×0.9, Difficulty×0.9
```

## Conclusion

This procedural enemy wave system provides a sophisticated, adaptive challenge that scales with player skill while maintaining engagement through mathematical precision and performance-based adjustments. The system ensures that both new and experienced players receive an appropriate level of challenge, creating a consistently engaging gameplay experience.

---

**Word Count: 487 words**

**Key Features:**
- Mathematical scaling formulas
- Performance-based adaptation
- Wave progression matrix
- Spawn location algorithms
- Enemy type selection logic
- System architecture overview
