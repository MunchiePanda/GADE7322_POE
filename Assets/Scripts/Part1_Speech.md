# **GADE7322 POE - Part 1 Script Explanations**
*(For 7-Minute Speech & Video)*

---

## **5. Terrain Scripts**
### **VoxelChunk.cs**
**Purpose:**
Handles the generation and mesh construction of a chunk of voxels in the terrain.

**Technical Breakdown:**
- **Chunk Initialization:** Sets up the chunk with its size, position, prefab, layer, and terrain data.
- **Mesh Generation:** Iterates over each voxel position in the chunk and adds visible faces to the mesh.
- **Face Visibility:** Determines if a voxel face is visible (not covered by another voxel) using `IsFaceVisible`.
- **Texture Mapping:** Assigns textures to faces based on biome data and position.
- **Mesh Construction:** Builds the mesh from vertices, UVs, and triangles, and assigns it to the chunk.

---

### **VoxelEnums.cs**
**Purpose:**
Defines enums for voxel face directions and texture types.

**Technical Breakdown:**
- **FaceDirection:** Defines the six possible directions for voxel faces (Top, Bottom, Front, Back, Left, Right).
- **TextureType:** Defines the types of textures that can be applied to voxel faces (Grass, Dirt, Stone, Sand).

---

### **TextureAtlas.cs**
**Purpose:**
Provides UV coordinates for different texture types used in the voxel terrain.

**Technical Breakdown:**
- **UV Mapping:** Returns UV coordinates for each texture type (Grass, Dirt, Stone, Sand) to map textures correctly on voxel faces.

---

### **VoxelTerrainGenerator.cs**
**Purpose:**
Generates the voxel-based terrain, including paths, defender locations, and trees.

**Technical Breakdown:**
- **Terrain Generation:** Uses Perlin noise to generate varied terrain heights.
- **Path Carving:** Creates paths from the edges of the terrain to the center.
- **Defender Locations:** Generates valid defender placement locations near paths.
- **Tree Spawning:** Randomly spawns trees on grass tiles based on biome data.
- **Chunk Management:** Divides the terrain into chunks for efficient rendering.
- **Highlighting:** Highlights paths and defender locations for player interaction.

---
## **1. Core Scripts**
### **Tower.cs**
**Purpose:**
Manages the central tower's health, combat, and upgrades.

**Technical Breakdown:**
- **Health System:** Tracks `currentHealth` and `maxHealth`, with methods like `TakeDamage` and `Heal`.
- **Combat:** Automatically attacks the nearest enemy using `AutoAttackNearestEnemy` and `LobProjectileAtEnemy`.
- **Upgrades:** Supports upgrading health and damage for improved performance.
- **Game State:** Triggers game over when the tower is destroyed.

---

### **Health.cs**
**Purpose:**
A reusable health system for all game entities (defenders, tower, enemies).

**Technical Breakdown:**
- **Health Management:** Tracks `MaxHealth` and `CurrentHealth`, with methods like `TakeDamage` and `Heal`.
- **Events:** Uses Unity's `UnityEvent` to trigger actions when damage is taken (`OnTakeDamage`) or when the entity dies (`OnDeath`).
- **Death Handling:** Destroys the GameObject and invokes the `OnDeath` event when health reaches zero.

---

### **Projectile.cs**
**Purpose:**
Manages projectile behavior, including tracking, movement, and collision with enemies.

**Technical Breakdown:**
- **Initialization:** Sets the projectile's target, damage, and speed using `Initialize`.
- **Movement:** Moves toward the target using `Vector3` direction and speed.
- **Collision Handling:** Uses `OnTriggerEnter` to detect collisions with enemies or terrain.
- **Cleanup:** Destroys the projectile after hitting a target or if the target is lost.

---

### **GameManager.cs**
**Purpose:**
The central controller for the game, managing game state, resources, enemy spawning, and UI.

**Technical Breakdown:**
- **Game State:** Tracks whether the game is over, paused, or active.
- **Resource Management:** Handles player resources (e.g., `AddResources`, `SpendResources`).
- **Enemy Spawning:** Uses `EnemySpawner` to spawn waves of enemies with increasing difficulty.
- **Tower & Defender Placement:** Spawns the central tower and allows players to place defenders on valid terrain tiles.
- **UI Management:** Controls UI elements like health bars, resource counters, and pause menus.

---

### **DefenderAttack.cs**
**Purpose:**
Manages the attacking behavior of defenders.

**Technical Breakdown:**
- **Attack Logic:** Uses `Physics.OverlapSphere` to detect enemies within `AttackRange`.
- **Target Selection:** Finds the nearest enemy and fires a projectile at it.
- **Cooldown System:** Ensures defenders attack at a controlled rate using `AttackCooldown`.
- **Visualization:** Uses `OnDrawGizmosSelected` to display the attack range in the Unity Editor.

---

### **TowerController.cs**
**Purpose:**
Manages the tower's rotation, path selection, and shooting mechanics.

**Technical Breakdown:**
- **Rotation:** Handles smooth rotation of the tower turret using `Quaternion.RotateTowards`.
- **Path Selection:** Allows players to select paths using number keys (1-9) and rotates the tower to face the selected path's entrance.
- **Shooting:** Fires projectiles in the direction the turret is facing when the player presses the left mouse button or the `E` key.
- **Input Handling:** Uses Unity's Input System to detect keyboard and mouse inputs.

---

### **ProjectileDamage.cs**
**Purpose:**
Handles damage application when projectiles collide with enemies.

**Technical Breakdown:**
- **Collision Detection:** Uses `OnTriggerEnter` to detect collisions with enemies.
- **Damage Application:** Applies damage to the enemy's `Health` component.
- **Cleanup:** Destroys the projectile after hitting a target.

---

## **2. Systems Scripts**
### **Enemy.cs**
**Purpose:**
Manages enemy behavior, including movement along paths, attacking defenders or the tower, and handling damage.

**Technical Breakdown:**
- **Path Following:** Enemies move along predefined paths using `FollowPathTowardsTower`.
- **Combat:** Detects and attacks nearby defenders or the tower using `AcquireDefenderIfAny` and `TryAttackTower`.
- **Damage Handling:** Takes damage and dies when health reaches zero, rewarding the player with resources.
- **Initialization:** Enemies are initialized with a path, terrain height, and references to the tower and `GameManager`.

---

### **Defender.cs**
**Purpose:**
Manages defender behavior, including attacking enemies, taking damage, and upgrading.

**Technical Breakdown:**
- **Combat:** Detects and attacks nearby enemies using `AcquireEnemyIfAny` and `TryAttackEnemy`.
- **Projectile Handling:** Fires projectiles at enemies using `LobProjectileAtEnemy`.
- **Damage Handling:** Takes damage and is destroyed when health reaches zero.
- **Upgrades:** Supports upgrading health and damage for improved performance.

---

### **CameraCullingConfig.cs**
**Purpose:**
Optimizes rendering performance by adjusting camera culling distances for different layers.

**Technical Breakdown:**
- **Layer Culling:** Sets custom culling distances for specific layers (e.g., terrain) to improve performance.
- **Shadow Optimization:** Optionally clamps shadow rendering distance to reduce GPU load.
- **Spherical Culling:** Enables spherical culling for better outdoor scene rendering.

---

## **3. UI Scripts**
### **DefenderPlacementManager.cs**
**Purpose:**
Manages the placement of defenders near paths.

**Technical Breakdown:**
- **Path Selection:** Allows players to select paths using number keys (1-5).
- **Location Highlighting:** Highlights valid defender placement locations near the selected path.
- **Defender Placement:** Places defenders at selected locations when the player clicks the "Place Defender" button.

---

## **4. Gameplay Flow (For Speech & Video)**
*(Leave space for gameplay explanations here.)*

### **Key Gameplay Mechanics:**
1. **Tower Defense:** Players defend the central tower from waves of enemies.
2. **Resource Management:** Players earn resources by defeating enemies and spend them to place defenders or upgrade the tower.
3. **Path Selection:** Players select paths to highlight defender placement locations.
4. **Combat:** Defenders and the tower automatically attack enemies within range.
5. **Upgrades:** Players can upgrade the tower's health and damage for better survivability.

---
