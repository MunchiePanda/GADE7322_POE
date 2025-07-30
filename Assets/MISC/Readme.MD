# 🧱 Core Fracture – Voxel Tower Defense (Unity GADE7322)

**Author:** Mikale Himraj  
**Module:** Game Development 3B – GADE7322  
**Project Type:** Practical PoE (Parts 1–3)  
**Engine:** Unity 2022.3 LTS  
**Language:** C#  

---

## 🎯 Project Overview

"Core Fracture" is a 3D voxel-based tower defense game where enemies procedurally attack through dynamic terrain paths. The player strategically places defenders to protect a central tower. Everything — from terrain to waves to enemy types — is procedurally generated.

The development is structured to match the three parts of the GADE7322 PoE assessment brief.

---

## 🔁 PART 1 — Procedural Terrain, Tower, and Basic Gameplay

### 🔨 Step-by-Step Development Plan

#### 1. Procedural Voxel Terrain Generator
- Create a `VoxelTerrainGenerator.cs` script.
- At runtime, generate a **3D grid** of cubes (voxels).
- Carve **at least 3 unique enemy paths** toward the center.
- Ensure:
  - Terrain is different each game.
  - Mesh is built at runtime.
  - Defender placement zones are outside of the path grid.
  - Paths lead to one central tile: the **Tower Point**.

#### 2. Tower Mechanics
- Create `Tower.cs`:
  - Auto-attacks enemies in range.
  - Has `maxHealth` and `currentHealth`.
  - Game ends when health = 0.
- Visual feedback for damage.
- Place tower at center of terrain (where all paths lead).

#### 3. Defender System
- Create `Defender.cs`:
  - Auto-attacks nearby enemies.
  - Can be placed only on valid terrain (not paths).
  - Has its own health.
  - Use mouse click to place (grid-based).
- Placement locations generated based on terrain.

#### 4. Enemy Setup
- Create `Enemy.cs`:
  - Spawns from path entrances at intervals.
  - Uses NavMesh or A* to follow carved path to tower.
  - Deals damage to tower or defenders.
  - Has health and can die.

#### 5. Game Loop
- Create `GameManager.cs`:
  - Starts terrain gen, initializes tower and spawns first wave.
  - Tracks resources (earned by killing enemies).
  - Allows defender placement via UI.
  - Ends game when tower HP = 0.

#### 6. User Interface
- Show:
  - Tower & defender health bars.
  - Resource counter.
  - Defender cost.
  - Game over screen.
- Allow pause & restart functionality.

#### 7. GitHub Setup
- Push project to GitHub.
- Add lecturer as collaborator.
- Maintain commits for each milestone.

---

## ⚔️ PART 2 — Advanced Defenders, Enemies & Procedural Enemy Waves

### 📘 Planning Document
Each student writes a 400–500 word document detailing:
- How procedural wave logic adapts to player skill.
- How difficulty scales (time-based, kill-count, upgrade-count).
- How enemies are varied and spawned.
- Use diagrams to support wave logic design.

### 🔨 Step-by-Step Development Plan

#### 1. New Enemy Types
- Add 2 new enemies with distinct:
  - Speeds
  - Attack methods (e.g., ranged or AoE)
  - HP and damage values
- Must look visually different.

#### 2. New Defender Types
- Add 2 new defenders with:
  - Unique fire modes (e.g., slow enemy, splash)
  - Different costs and cooldowns
  - Unique visuals

#### 3. Procedural Wave System
- Adjust `GameManager.cs` or create `WaveSpawner.cs`:
  - Enemy spawn intervals and amounts scale with:
    - Player kills
    - Resource usage
    - Elapsed time
  - Different enemy types appear based on current wave tier.

#### 4. Difficulty & Adaptation
- Spawn more enemies as time increases.
- Introduce elite enemies based on player success.
- Increase spawn rate or HP dynamically.

#### 5. UI Enhancements
- Wave number indicator.
- Mini enemy preview.
- Alert if elite/boss enemies are incoming.

#### 6. Video Demonstration
- Record gameplay.
- Explain:
  - New enemies/defenders.
  - Procedural wave logic.
  - Key scripts (WaveSpawner, Enemy AI, Defender attack).

---

## 🌌 PART 3 — Upgrades, Visual Effects, and Custom Procedural Feature

### 📘 Planning Document
Write a 400–500 word doc explaining your unique procedural system idea.  
Examples:
- Procedural soundtrack or mood lighting.
- Randomized weather events.
- Morphing enemy types with stats/looks that shift.

Include diagrams to explain logic and implementation.

---

### 🔨 Step-by-Step Development Plan

#### 1. Defender & Tower Upgrades
- Implement upgrade trees (at least 2 levels per unit).
- Upgrade effects:
  - More health, faster fire rate, or area damage.
  - Visual change (glow, armor, size).
- Pay for upgrades using resources.

#### 2. Custom Shader Effects
- Use Shader Graph to create:
  - A shader that **displaces mesh vertices** (e.g., tower shaking on damage).
  - A shader that **changes color** dynamically (e.g., defenders glow when upgraded).

#### 3. Custom Visual Effects
- 1 custom **particle system** (e.g., fire trail, defender attack).
- 1 **post-processing** screen-space effect:
  - Damage vignette
  - Wave transition blur
- Must be custom-made and not just Unity presets.

#### 4. Custom Procedural Feature
- Choose one of:
  - Procedural weather (fog, rain, light flickers)
  - Procedural loot drops
  - Randomized daily challenge modifiers
- Integrate it cleanly into gameplay loop.

#### 5. UI Updates
- Show upgrade buttons and info.
- Alert player when upgrades are unlocked or resources are ready.

#### 6. Final Presentation Video
- Show complete gameplay loop.
- Demonstrate:
  - Custom shader + VFX
  - New upgrade system
  - Custom procedural feature
- Explain key parts of code.

---

## 🚀 How to Run

1. Clone this repo
2. Open in Unity 2022.3 or later
3. Press ▶️ Play
4. Defend your tower and survive

---

## 🧩 Project Features Summary

| Feature | Status |
|--------|--------|
| Procedural voxel terrain | ✅ |
| Pathfinding + enemy AI | ✅ |
| Tower & defenders with auto-attack | ✅ |
| Resources & placement logic | ✅ |
| Procedural enemy waves | 🔄 In progress |
| Upgrades, VFX & shaders | 🔄 Coming in Part 3 |
| Final procedural custom system | 🔄 Coming in Part 3 |


