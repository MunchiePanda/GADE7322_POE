# GADE7322 – Tower Defence Implementation Explanation

This document explains how the project implements the requirements for **Parts 1, 2, and 3**, including procedural terrain, base mechanics, enemy waves, upgrades, shaders, and a custom procedural feature.

---

## Table of Contents
1. [Part 1: Procedural Terrain & Base Mechanics](#part-1-procedural-terrain--base-mechanics)
2. [Part 2: Procedural Enemy Waves](#part-2-procedural-enemy-waves)
3. [Part 3: Upgrades, Shaders, Custom Procedural Feature](#part-3-upgrades-shaders-custom-procedural-feature)
4. [Scene Navigation](#scene-navigation)
5. [General Setup Instructions](#general-setup-instructions)

---

## Part 1: Procedural Terrain & Base Mechanics
### High-level Architecture
- **Terrain generation**: `Assets/Scripts/Terrain/VoxelTerrainGenerator.cs`
- **Game loop & spawning**: `Assets/Scripts/Core/GameManager.cs`
- **Player base (tower)**: `Assets/Scripts/Core/Tower.cs`
- **Enemies**: `Assets/Scripts/Systems/Enemy.cs`
- **Defenders**: `Assets/Scripts/Systems/Defender.cs`
- **Defender placement**: `Assets/Scripts/Systems/DefenderPlacement.cs`
- **UI**: `Assets/Scripts/UI/` (Health bars, resource counters, pause menu, game over screen)

### How Requirements Are Met
- **Procedural Terrain & ≥3 Paths**: `VoxelTerrainGenerator` builds a 3D voxel grid and carves multiple unique paths from random edges to the center.
- **Central Tower**: Tracks health, auto-attacks enemies, and triggers game over when destroyed.
- **Defenders**: Can be placed on valid tiles, auto-target enemies, and have health.
- **Enemies**: Follow generated paths, attack defenders/tower, and reward resources on death.
- **Game Loop**: Manages resources, spawning, and UI.
- **UI/UX**: Health bars, resource counters, pause/restart, and game over screens.

### Detailed Components
#### VoxelTerrainGenerator.cs
- Generates a voxel grid and carves paths using biased random walks.
- Exposes:
  - `List<List<Vector3Int>> GetPaths()`: Public access to paths.
  - `bool IsValidDefenderPlacement(Vector3Int)`: Prevents placement on paths.
  - `void HighlightPath(int)`: Visual feedback for path selection.

#### GameManager.cs
- Initializes terrain, tower, and enemies.
- Manages resources, spawning, and game state (pause/restart).
- Exposes APIs for resource management and defender placement.

#### Tower.cs
- Tracks health, auto-attacks enemies, and triggers game over on destruction.

#### Enemy.cs
- Follows paths, attacks defenders/tower, and rewards resources on death.

#### Defender.cs
- Auto-targets enemies, deals damage, and can be destroyed.

#### DefenderPlacement.cs
- Handles input for defender placement using the Input System.

---

## Part 2: Procedural Enemy Waves
### New Enemy Types
1. **FastEnemy.cs**
   - High speed, low health.
   - Inherits from `Enemy.cs` and overrides `moveSpeed` and `maxHealth`.

2. **TankEnemy.cs**
   - Low speed, high health.
   - Inherits from `Enemy.cs` and overrides `moveSpeed` and `maxHealth`.

### New Defender Types
1. **SniperDefender.cs**
   - Long range, high damage, low attack speed.
   - Inherits from `Defender.cs` and overrides `attackRange` and `attackDamage`.

2. **AoEDefender.cs**
   - Short range, area damage.
   - Uses `Physics.OverlapSphere` to damage multiple enemies.

### Procedural Wave System
#### EnemySpawner.cs (Updated)
- Spawns random enemy types (`FastEnemy`, `TankEnemy`, or default `Enemy`).
- Scales enemy stats (health, speed) based on the current wave.

#### GameManager.cs (Updated)
- Tracks `currentWave` and increases difficulty:
  - Enemy count scales exponentially.
  - Enemy health/speed increases per wave.
- Adaptive difficulty: Reduces spawn intervals if the player has many resources.

### Implementation Steps
1. **Create New Enemy/Defender Scripts**:
   - Add `FastEnemy.cs`, `TankEnemy.cs`, `SniperDefender.cs`, and `AoEDefender.cs` to `Assets/Scripts/Systems/`.
   - Inherit from `Enemy.cs`/`Defender.cs` and override stats.

2. **Update EnemySpawner.cs**:
   - Add logic to randomly instantiate enemy types.
   - Scale enemy stats based on `GameManager.currentWave`.

3. **Update GameManager.cs**:
   - Add wave tracking and difficulty scaling.
   - Call `StartNextWave()` when all enemies in the current wave are defeated.

4. **Update DefenderPlacement.cs**:
   - Support multiple defender types (e.g., via a UI dropdown).

5. **Update UI**:
   - Add wave counter (`WaveCounterUI.cs`).
   - Add defender type selection buttons.

---

## Part 3: Upgrades, Shaders, Custom Procedural Feature
### Upgrades
#### Tower.cs (Updated)
- Add `UpgradeHealth()` and `UpgradeDamage()` methods.
- Visual feedback: Scale the tower or change its material.

#### Defender.cs (Updated)
- Add upgrade methods and visual feedback.

#### UI
- Add upgrade buttons to `TowerUI.cs` and `DefenderUI.cs`.
- Connect buttons to upgrade methods.

### Shaders
1. **Vertex Displacement Shader**
   - Apply to terrain for dynamic effects (e.g., waving grass).
   - Save as `Assets/Shaders/VertexDisplacement.shader`.

2. **Base Color Modification Shader**
   - Apply to defenders/enemies for dynamic color changes (e.g., damage feedback).
   - Save as `Assets/Shaders/ColorModification.shader`.

### Visual Effects
1. **Particle System**
   - Add explosion effects on enemy/death (`Assets/Prefabs/Explosion.prefab`).

2. **Post-Processing**
   - Add bloom/vignette via Unity’s Post-Processing Stack.

### Custom Procedural Feature: Dynamic Weather
#### WeatherManager.cs (New)
- Randomly triggers weather events (rain, snow).
- Affects gameplay (e.g., rain slows enemies).

#### Implementation Steps
1. Create `WeatherManager.cs` in `Assets/Scripts/Systems/`.
2. Add particle systems for rain/snow (`Assets/Prefabs/Rain.prefab`, `Snow.prefab`).
3. Connect weather events to gameplay (e.g., modify enemy speed).

---

## Scene Navigation
### SceneNavigationUI.cs (New)
- Handles navigation between `MainMenu`, `Game`, and `EndMenu` scenes.
- Attach to a UI canvas in each scene.

#### Features
- **Main Menu**: Buttons for `Start Game` and `Quit`.
- **Game Scene**: Button for `Back to Menu`.
- **End Menu**: Buttons for `Restart` and `Back to Menu`.

#### Setup Instructions
1. Create a new UI canvas in each scene.
2. Add buttons and attach `SceneNavigationUI.cs`.
3. Configure button `onClick` events to call `LoadScene(string sceneName)`.

---

## General Setup Instructions
### 1. Scene Setup
- **MainMenu.unity**: Add `Start Game` and `Quit` buttons.
- **SampleScene.unity**: Add `Back to Menu` button.
- **EndMenu.unity**: Add `Restart` and `Back to Menu` buttons.

### 2. Script Setup
- Attach `SceneNavigationUI.cs` to UI canvases.
- Configure public fields in the Inspector (e.g., assign button references).

### 3. Build Settings
- Add all scenes to **File > Build Settings**:
  - `Assets/Scenes/MainMenu.unity`
  - `Assets/Scenes/SampleScene.unity`
  - `Assets/Scenes/EndMenu.unity`

### 4. Input System
- Ensure **Project Settings > Player > Active Input Handling** is set to "Both" or "Input System Package".

### 5. Testing
- Test scene transitions:
  - Main Menu → Game → End Menu → Main Menu.
  - Verify pause/restart functionality in the game scene.

---

## Example Code Snippets
### 1. FastEnemy.cs
```csharp
using UnityEngine;

public class FastEnemy : Enemy
{
    protected override void Start()
    {
        base.Start();
        moveSpeed *= 2f; // Double speed
        maxHealth = 5;  // Lower health
    }
}
```

### 2. Wave Scaling in GameManager.cs
```csharp
public class GameManager : MonoBehaviour
{
    public int currentWave = 1;
    public float waveScalingFactor = 1.5f;

    void StartNextWave()
    {
        currentWave++;
        int enemyCount = Mathf.RoundToInt(initialWaveEnemyCount * Mathf.Pow(waveScalingFactor, currentWave));
        StartCoroutine(SpawnWave(enemyCount));
    }
}
```

### 3. Upgrade Method in Defender.cs
```csharp
public void UpgradeDamage()
{
    if (GameManager.Instance.SpendResources(upgradeCost))
    {
        attackDamage *= 1.5f;
        transform.localScale *= 1.2f; // Visual feedback
    }
}
```

### 4. SceneNavigationUI.cs
```csharp
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneNavigationUI : MonoBehaviour
{
    public Button mainMenuButton;
    public Button startGameButton;
    public Button restartButton;
    public Button quitButton;

    void Start()
    {
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(() => LoadScene("MainMenu"));
        if (startGameButton != null) startGameButton.onClick.AddListener(() => LoadScene("SampleScene"));
        if (restartButton != null) restartButton.onClick.AddListener(() => LoadScene(SceneManager.GetActiveScene().name));
        if (quitButton != null) quitButton.onClick.AddListener(QuitGame);
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
```

---

## Final Notes
- **Balance**: Test upgrades, enemy waves, and resources for fair difficulty.
- **Polish**: Add sound effects, particle effects, and UI animations.
- **Documentation**: Update this file as you implement new features.
- **GitHub**: Commit changes frequently and include this file in your repo.
