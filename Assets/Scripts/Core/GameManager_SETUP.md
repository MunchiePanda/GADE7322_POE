# GameManager Setup Guide

This guide explains how to set up and use the `GameManager` script in your Unity project.

---

## 1. **Add the Script to the Scene**
- In the Hierarchy, right-click and select **Create Empty** to make a new empty GameObject (e.g., name it `GameManager`).
- With the new GameObject selected, click **Add Component** and search for `GameManager`.
- Add the script to the GameObject.

## 2. **Assign References in the Inspector**
- **VoxelTerrainGenerator**: Drag your `VoxelTerrainGenerator` GameObject from the Hierarchy into this field.
- **Tower Prefab**: Drag your tower prefab (a GameObject to represent the central tower) from the Project window into this field.
- **Enemy Prefab**: Drag your enemy prefab (a GameObject to represent an enemy) from the Project window into this field.

## 3. **Configure Parameters**
- **Enemy Spawn Interval**: Time (in seconds) between enemy spawns during a wave (default: 2).
- **Initial Wave Enemy Count**: Number of enemies to spawn in the first wave (default: 5).

## 4. **How It Works**
- On Play, the GameManager will:
    1. Check for the terrain generator and initialize the terrain.
    2. Spawn the tower at the center of the terrain.
    3. Start the first wave of enemies, spawning them at random path entrances.
    4. Track resources and game state (game over, pause, restart).

## 5. **Extending the GameManager**
- You can add more features, such as:
    - Additional wave logic (increasing difficulty, more waves)
    - UI hooks for resources, game over, and pause
    - Defender placement logic
    - Integration with other systems (e.g., upgrades, VFX)

---

**Script Location:**
- `Assets/Scripts/Core/GameManager.cs` 