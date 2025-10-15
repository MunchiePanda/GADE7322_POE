# GADE7322_POE - Tower Defense Game

## Project Overview
This is a Unity-based tower defense game developed for GADE7322 (Game Development) Portfolio of Evidence (PoE). The game features procedural enemy wave generation, adaptive difficulty scaling, and multiple enemy and defender types.

## Features
- **Procedural Enemy Waves**: Dynamic wave generation with adaptive difficulty
- **Multiple Enemy Types**: Regular, Fast, Kamikaze (Bomber), and Armored Dragons
- **Multiple Defender Types**: Basic, Frost, and Lightning Towers
- **Adaptive Difficulty**: System adjusts based on player performance
- **3D Terrain Generation**: Procedural voxel-based terrain with path unlocking
- **Drag & Drop Defender Placement**: Intuitive defender placement system

## Asset Credits

### 3D Models and Assets
- **Low Poly Dragon Models**: [Poly Pizza - Free Low Poly Nature Pack](https://assetstore.unity.com/packages/3d/vegetation/free-low-poly-nature-pack-103127)
- **Evolved Dragon Assets**: [Poly Pizza - Dragon Collection](https://assetstore.unity.com/packages/3d/characters/creatures/dragon-collection-102127)

### References
Poly Pizza. (2024). *Free Low Poly Nature Pack* [Unity Asset Store]. Unity Technologies. Available at: https://assetstore.unity.com/packages/3d/vegetation/free-low-poly-nature-pack-103127

Poly Pizza. (2024). *Dragon Collection* [Unity Asset Store]. Unity Technologies. Available at: https://assetstore.unity.com/packages/3d/characters/creatures/dragon-collection-102127

## Technical Implementation

### Key Scripts
- **EnemySpawner.cs**: Handles procedural enemy wave generation
- **WaveProgressionSystem.cs**: Manages adaptive difficulty scaling
- **PlayerPerformanceTracker.cs**: Tracks player metrics for adaptive gameplay
- **KamikazeDragon.cs**: Bomber enemy with area damage
- **ArmoredDragon.cs**: Tank enemy with damage reduction
- **FrostTowerDefender.cs**: AoE slow defender
- **LightningTowerDefender.cs**: Chain lightning defender

### Game Balance
- **Tower Attack Range**: 8-12 units (reduced for balance)
- **Defender Limit**: 25 defenders maximum
- **Bomber Explosion**: 50-unit radius with 5 damage to all defenders
- **Armored Dragon**: 30% damage reduction (balanced)

## Development Notes
- All emojis removed from debug logs for cleaner code
- Unused setup scripts removed for streamlined codebase
- Adaptive scaling formulas match detailed planning document
- Performance tracking uses 4-metric system: Enemy Kills, Defender Survival, Resource Efficiency, Wave Completion

## Installation
1. Clone the repository
2. Open in Unity 2022.3 or later
3. Import required packages
4. Build and run

## License
This project is for educational purposes as part of GADE7322 coursework.
