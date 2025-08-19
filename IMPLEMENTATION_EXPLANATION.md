# GADE7322 – Tower Defence (Part 1) Implementation Explanation

This document explains how the current project implements the Part 1 requirements: procedural terrain and base mechanics, with defenders, enemies, a basic game loop, and UI.

## High-level Architecture
- **Terrain generation**: `Assets/Scripts/Terrain/VoxelTerrainGenerator.cs`
- **Game loop & spawning**: `Assets/Scripts/Core/GameManager.cs`
- **Player base (tower)**: `Assets/Scripts/Core/Tower.cs`
- **Enemies**: `Assets/Scripts/Systems/Enemy.cs`
- **Defenders**: `Assets/Scripts/Systems/Defender.cs`
- **Defender placement**: `Assets/Scripts/Systems/DefenderPlacement.cs`
- **UI**: `Assets/Scripts/UI/HealthBarUI.cs`, `ResourceCounterUI.cs`, `DefenderCostUI.cs`, `PauseMenuUI.cs`, `GameOverUI.cs`, and setup guide `Assets/Scripts/UI/UI_Setup_Readme.md`

## How requirements are met
- **Procedural Terrain & ≥3 Paths**: `VoxelTerrainGenerator` builds a 3D voxel grid and carves multiple unique paths from random edges to the center point. The number of paths is configurable (default 3). Paths differ every run due to randomness.
- **Central Tower with Health & Auto-Attack**: `Tower` tracks health, updates the UI, and when destroyed triggers game over. It auto-attacks the nearest enemy within range at an interval.
- **Defenders**: `Defender` can be placed on valid tiles (not on paths), auto-targets/attacks enemies within range, has health, and can be destroyed.
- **Enemies**: `Enemy` follows a generated path to the tower, attacks defenders that come within detection range, and damages the tower on arrival. On death, it rewards resources.
- **Game Loop & Resources**: `GameManager` initializes terrain/tower, spawns an initial wave on play, tracks resources, and provides a placement cost. UI elements display resources and costs.
- **UI/UX**: Health bar for the tower, resource/cost counters, pause/resume/restart, and game over panel are supported through the UI scripts.

## Detailed Components

### VoxelTerrainGenerator.cs
- Public inspector fields: `width`, `depth`, `height`, `voxelPrefab`, `numPaths`.
- Generates a voxel grid and carves paths using a biased random-walk from random edges to the center.
- Exposes:
  - `List<List<Vector3Int>> GetPaths()` – public access for consumers.
  - `bool IsValidDefenderPlacement(Vector3Int)` – prevents placement on carved path tiles.
  - `void HighlightPath(int)` – optional visual path highlight for selection/feedback.
- Debug: draws path cells (red) and the center (yellow) using gizmos.

### GameManager.cs
- References: `terrainGenerator`, `towerPrefab`, `enemyPrefab`.
- Timing: `enemySpawnInterval`, `initialWaveEnemyCount`.
- Defenders/Resources: `defenderPrefab`, `defenderCost`, `startingResources`.
- UI references: `towerHealthBar`, `resourceCounterUI`, `defenderCostUI`, `gameOverUI`, `pauseMenuUI`.
- Startup flow: hides panels → validates terrain → initializes resources and UI → spawns tower → starts first enemy wave.
- Enemy spawn: now uses `terrainGenerator.GetPaths()` to pick a random path entrance and instantiates an `Enemy`, initializing it with the path, terrain height, and the tower.
- Resource API: `AddResources(int)`, `SpendResources(int)`, `GetResources()`; updates UI accordingly.
- Pause/game over: ESC toggles pause; `GameOver()` shows UI; `RestartGame()` reloads the scene.
- Defender placement API:
  - `bool TryPlaceDefender(Vector3Int gridPosition)` – validates path exclusion via `IsValidDefenderPlacement`, checks resources, instantiates defender at the top of the terrain (y = `height`).

### Tower.cs
- Health: `maxHealth`, `currentHealth`, updates `HealthBarUI` through `GameManager.towerHealthBar`.
- Auto-attack: scans for nearest `Enemy` within `attackRange` using `Physics.OverlapSphere` and applies damage at `attackIntervalSeconds`.
- On death: calls `GameManager.GameOver()`.

### Enemy.cs
- Initialization: `Initialize(List<Vector3Int> path, int terrainTopY, Tower tower, GameManager gm)` called by `GameManager` on spawn.
- Movement: follows `path` by moving toward the next waypoint at `moveSpeed` on the top surface (y = `terrainTopY`).
- Targeting: if an in-range `Defender` is found (via `OverlapSphere`), the enemy attacks it at `attackIntervalSeconds`; otherwise continues along its path.
- Tower damage: when reaching the final waypoint, attacks the `Tower` at the same interval.
- Death: awards `resourceRewardOnDeath` to the player (`GameManager.AddResources`) and destroys itself.

### Defender.cs
- Health and simple death.
- Targeting: acquires nearest `Enemy` in `attackRange` via `OverlapSphere`.
- Attack: damages target at `attackIntervalSeconds`.

### DefenderPlacement.cs
- Input: uses the new Input System (`UnityEngine.InputSystem`). On left-mouse click, it raycasts from the camera and snaps to integer grid coordinates.
- Layer mask: `placementPlaneMask` should include the layer(s) used by your voxel cubes so the ray hits the terrain surface.
- Calls `GameManager.TryPlaceDefender(gridPosition)`; placement succeeds only if not on a path tile and enough resources are available.

## Scene & Inspector Setup (Checklist)
1. Create an empty `GameManager` object and add `GameManager` component.
   - Assign `terrainGenerator` (your `VoxelTerrainGenerator` in the scene).
   - Assign `towerPrefab`, `enemyPrefab`, `defenderPrefab`.
   - Set `enemySpawnInterval`, `initialWaveEnemyCount`, `startingResources`, and `defenderCost`.
   - Link UI references (see UI section).
2. Add `VoxelTerrainGenerator` on an empty object.
   - Assign `voxelPrefab` (the cube prefab). Ensure it has a collider and is on a `Terrain` (or similar) layer.
3. Add `DefenderPlacement` to an object (e.g., `GameManager`).
   - Assign `mainCamera` and `gameManager`. Set `placementPlaneMask` to include the `Terrain` layer.
4. Prefabs:
   - `Tower.prefab`: must include `Tower` component.
   - `Enemy.prefab`: include `Enemy` component and a collider (Capsule/Sphere). Rigidbody is optional (set kinematic if used).
   - `Defender.prefab`: include `Defender` component and a collider.
5. Input System: Project Settings → Player → Active Input Handling = “Both” or “Input System Package”.
6. UI: Follow `Assets/Scripts/UI/UI_Setup_Readme.md` and then assign the created UI objects to `GameManager`.

## UI Wiring
- `HealthBarUI` → controlled by `Tower` through `GameManager.towerHealthBar`.
- `ResourceCounterUI` → updated by `GameManager.AddResources/SpendResources`.
- `DefenderCostUI` → set on start with `defenderCost`.
- `PauseMenuUI` → ESC toggles; Resume/Restart buttons invoke `GameManager` methods.
- `GameOverUI` → shown from `GameManager.GameOver()`.

## Notes, Limitations, and Next Steps
- Placement grid snap is integer rounding; adjust if your terrain origin/scale changes.
- OverlapSphere target acquisition requires colliders on `Enemy` and `Defender` objects.
- Simple instant-damage attacks are used (no projectiles/VFX) to keep mechanics clear. These can be replaced with projectile systems later.
- Waves currently spawn a fixed count; extend `GameManager` to scale `enemyCount`, `enemy stats`, and timing per wave for Part 2.
- Consider disabling further spawns after `GameOver()` and clearing remaining enemies.

## Test Plan
- Press Play: terrain generates and tower spawns at center.
- Enemies spawn at random path entrances and move toward the tower.
- Tower auto-attacks enemies in range; enemy deaths increase resources.
- Left-click on non-path tiles places defenders (cost deducted; fails on path tiles or insufficient resources).
- ESC pauses; Resume/Restart work; tower destruction triggers Game Over panel.

---

For UI specifics and a field mapping table, see `Assets/Scripts/UI/UI_Setup_Readme.md`.
