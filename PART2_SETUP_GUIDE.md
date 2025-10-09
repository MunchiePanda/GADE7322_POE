# Part 2 Setup Guide - Procedural Enemy Waves

This guide will walk you through setting up the new enemy types and defender types for Part 2 of your tower defense game.

## Overview of New Components

### New Enemy Types
1. **KamikazeDragon** - Fast, low health, explodes on contact
2. **ArmoredDragon** - Slow, high health, damage reduction

### New Defender Types  
1. **FrostTowerDefender** - AoE slow effect
2. **LightningTowerDefender** - Chain lightning attacks

## Step-by-Step Setup Instructions

### 1. Create Enemy Prefabs

#### Kamikaze Dragon Prefab
1. **Duplicate existing Enemy prefab:**
   - Right-click `Assets/Prefabs/Enemy.prefab`
   - Select "Duplicate"
   - Rename to `KamikazeDragon.prefab`

2. **Configure the prefab:**
   - Select the KamikazeDragon prefab
   - In the Inspector, remove the `Enemy` component
   - Add the `KamikazeDragon` component
   - Set the following values:
     - `Charge Speed Multiplier`: 3
     - `Explosion Radius`: 4
     - `Explosion Damage`: 25
     - `Detection Range`: 10

3. **Visual Setup:**
   - Scale the model to 0.7x (smaller than regular enemies)
   - Change material color to red
   - Add a Particle System child object for charge effects
   - Add an explosion effect prefab reference

#### Armored Dragon Prefab
1. **Duplicate existing Enemy prefab:**
   - Right-click `Assets/Prefabs/Enemy.prefab`
   - Select "Duplicate"
   - Rename to `ArmoredDragon.prefab`

2. **Configure the prefab:**
   - Select the ArmoredDragon prefab
   - In the Inspector, remove the `Enemy` component
   - Add the `ArmoredDragon` component
   - Set the following values:
     - `Armor Reduction`: 0.6 (60% damage reduction)
     - `Max Health`: 80
     - `Move Speed`: 1.2
     - `Attack Damage`: 3

3. **Visual Setup:**
   - Scale the model to 1.3x (larger than regular enemies)
   - Change material color to gray/dark gray
   - Add armor visual effects
   - Add Particle System for armor sparks

### 2. Create Defender Prefabs

#### Frost Tower Prefab
1. **Duplicate existing Defender prefab:**
   - Right-click `Assets/Prefabs/Defender.prefab`
   - Select "Duplicate"
   - Rename to `FrostTower.prefab`

2. **Configure the prefab:**
   - Select the FrostTower prefab
   - In the Inspector, remove the `Defender` component
   - Add the `FrostTowerDefender` component
   - Set the following values:
     - `Frost Radius`: 8
     - `Slow Duration`: 3
     - `Slow Multiplier`: 0.4
     - `Attack Range`: 12
     - `Attack Damage`: 8
     - `Attack Interval`: 3

3. **Visual Setup:**
   - Change material color to cyan
   - Add Particle System for frost effects
   - Create frost effect prefab for visual feedback

#### Lightning Tower Prefab
1. **Duplicate existing Defender prefab:**
   - Right-click `Assets/Prefabs/Defender.prefab`
   - Select "Duplicate"
   - Rename to `LightningTower.prefab`

2. **Configure the prefab:**
   - Select the LightningTower prefab
   - In the Inspector, remove the `Defender` component
   - Add the `LightningTowerDefender` component
   - Set the following values:
     - `Max Chain Targets`: 3
     - `Chain Range`: 4
     - `Chain Damage Reduction`: 0.8
     - `Attack Range`: 8
     - `Attack Damage`: 15
     - `Attack Interval`: 1.2

3. **Visual Setup:**
   - Change material color to yellow
   - Add LineRenderer component for lightning visual
   - Create lightning effect prefab

### 3. Update GameManager

1. **Open the GameManager in the Inspector**
2. **Add new prefab references:**
   - Assign `KamikazeDragon.prefab` to `kamikazeDragonPrefab`
   - Assign `ArmoredDragon.prefab` to `armoredDragonPrefab`
   - Assign `FrostTower.prefab` to `frostTowerPrefab`
   - Assign `LightningTower.prefab` to `lightningTowerPrefab`

3. **Set defender costs:**
   - `Frost Tower Cost`: 40
   - `Lightning Tower Cost`: 35

### 4. Update EnemySpawner

1. **Open the EnemySpawner component in the Inspector**
2. **Add new enemy prefab references:**
   - Assign `KamikazeDragon.prefab` to `kamikazeDragonPrefab`
   - Assign `ArmoredDragon.prefab` to `armoredDragonPrefab`

### 5. Update DefenderPlacement System

You'll need to update your defender placement system to support multiple defender types:

#### Option A: Simple UI Buttons
1. **Create UI buttons for each defender type:**
   - Basic Defender (25 resources)
   - Frost Tower (40 resources)
   - Lightning Tower (35 resources)

2. **Update DefenderPlacement.cs:**
```csharp
public DefenderType selectedDefenderType = DefenderType.Basic;

public void SetDefenderType(DefenderType type)
{
    selectedDefenderType = type;
}

// In your placement method, use:
gameManager.TryPlaceDefender(gridPosition, selectedDefenderType);
```

#### Option B: Hotkey System
1. **Add hotkeys for defender selection:**
   - 1 = Basic Defender
   - 2 = Frost Tower  
   - 3 = Lightning Tower

2. **Update input handling:**
```csharp
void Update()
{
    if (Input.GetKeyDown(KeyCode.Alpha1))
        selectedDefenderType = DefenderType.Basic;
    else if (Input.GetKeyDown(KeyCode.Alpha2))
        selectedDefenderType = DefenderType.FrostTower;
    else if (Input.GetKeyDown(KeyCode.Alpha3))
        selectedDefenderType = DefenderType.LightningTower;
}
```

### 6. Create Visual Effects (Optional but Recommended)

#### Explosion Effect for Kamikaze Dragon
1. **Create explosion prefab:**
   - Create empty GameObject
   - Add Particle System
   - Configure for explosion effect
   - Save as `ExplosionEffect.prefab`

#### Frost Effect for Frost Tower
1. **Create frost effect prefab:**
   - Create empty GameObject
   - Add Particle System with ice/snow particles
   - Save as `FrostEffect.prefab`

#### Lightning Effect for Lightning Tower
1. **Create lightning effect prefab:**
   - Create empty GameObject
   - Add LineRenderer for lightning bolt
   - Add Particle System for sparks
   - Save as `LightningEffect.prefab`

### 7. Testing and Balancing

1. **Test enemy spawning:**
   - Play the game and verify new enemies spawn in later waves
   - Check that Kamikaze Dragons explode properly
   - Verify Armored Dragons have damage reduction

2. **Test defender placement:**
   - Ensure all defender types can be placed
   - Verify resource costs are correct
   - Test that Frost Tower slows enemies
   - Test that Lightning Tower chains between enemies

3. **Balance adjustments:**
   - Adjust enemy spawn rates in `EnemySpawner.cs`
   - Modify damage values and costs as needed
   - Fine-tune visual effects

## Troubleshooting

### Common Issues:

1. **Enemies not spawning:**
   - Check that prefabs are assigned in EnemySpawner
   - Verify enemy scripts are attached to prefabs

2. **Defenders not placing:**
   - Ensure GameManager has all prefab references
   - Check that DefenderType enum is accessible

3. **Visual effects not working:**
   - Verify Particle Systems are configured
   - Check that effect prefabs are assigned in scripts

4. **Performance issues:**
   - Limit particle system emission rates
   - Use object pooling for frequent effects

## Next Steps

After completing this setup:

1. **Create the Planning Document** (400-500 words) explaining your procedural strategy
2. **Test and balance** the gameplay
3. **Add visual polish** with better models and effects
4. **Implement adaptive difficulty** features
5. **Create UI elements** for defender selection

## Files Created/Modified

### New Scripts:
- `FrostTowerDefender.cs`
- `LightningTowerDefender.cs`
- `KamikazeDragon.cs`
- `ArmoredDragon.cs`
- `DefenderType.cs`

### Modified Scripts:
- `EnemySpawner.cs` - Added new enemy types and wave-based selection
- `GameManager.cs` - Added defender type support and costs

### New Prefabs (to be created):
- `KamikazeDragon.prefab`
- `ArmoredDragon.prefab`
- `FrostTower.prefab`
- `LightningTower.prefab`

This setup provides a solid foundation for Part 2 of your tower defense game with sophisticated procedural enemy waves and strategic defender placement options.
