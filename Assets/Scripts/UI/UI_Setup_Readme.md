# UI Setup Guide for Core Fracture (Tower Defense)

This guide explains how to set up the user interface (UI) in Unity for this project, including what UI elements to create, where to place them, what scripts to attach, and how to connect everything in the Inspector.

---

## 1. Canvas Setup
- **Create a Canvas:**
  - Right-click in the Hierarchy → `UI` → `Canvas`.
  - Set Render Mode to `Screen Space - Overlay` (default).
  - Ensure an `EventSystem` exists in the scene (created automatically with the Canvas).

---

## 2. Tower Health Bar
- **Create:**
  - Right-click on Canvas → `UI` → `Slider`. Rename to `TowerHealthBar`.
  - Remove the Handle (expand the Slider, right-click the Handle child, and delete it).
  - Resize and position at the top left or top center of the screen.
  - Select the Fill child and set its color to green.
- **Attach Script:**
  - Add the `HealthBarUI` script to the `TowerHealthBar` GameObject.
- **Assign References:**
  - Drag the Slider component to the `healthSlider` field in the Inspector.
  - Drag the Fill Image (child of the slider) to the `fillImage` field.

---

## 3. Resource Counter
- **Create:**
  - Right-click on Canvas → `UI` → `Text - TextMeshPro`. Rename to `ResourceCounterText`.
  - Position at the top right of the screen.
  - Increase font size and set alignment as desired.
- **Attach Script:**
  - Add the `ResourceCounterUI` script to this GameObject.
- **Assign Reference:**
  - Drag the TMP_Text component to the `resourceText` field in the Inspector.

---

## 4. Defender Cost Display
- **Create:**
  - Right-click on Canvas → `UI` → `Text - TextMeshPro`. Rename to `DefenderCostText`.
  - Position near the resource counter or wherever you prefer.
- **Attach Script:**
  - Add the `DefenderCostUI` script to this GameObject.
- **Assign Reference:**
  - Drag the TMP_Text component to the `costText` field in the Inspector.

---

## 5. Game Over Panel
- **Create:**
  - Right-click on Canvas → `UI` → `Panel`. Rename to `GameOverPanel`.
  - Set the panel’s color to semi-transparent black.
  - Right-click on the panel → `UI` → `Text - TextMeshPro`. Rename to `GameOverText`.
  - Center the text, increase font size, and set a suitable message (e.g., “Game Over!”).
- **Attach Script:**
  - Add the `GameOverUI` script to the `GameOverPanel`.
- **Assign References:**
  - Drag the `GameOverPanel` to the `panel` field.
  - Drag the `GameOverText` to the `messageText` field.
- **Set the panel to inactive by default** (uncheck the checkbox in the Inspector).

---

## 6. Pause Menu Panel
- **Create:**
  - Right-click on Canvas → `UI` → `Panel`. Rename to `PauseMenuPanel`.
  - Set the panel’s color to semi-transparent (different from Game Over if you like).
  - Add buttons for Resume and Restart (Right-click on the panel → `UI` → `Button - TextMeshPro`).
  - Rename them to `ResumeButton` and `RestartButton` and change their text accordingly.
- **Attach Script:**
  - Add the `PauseMenuUI` script to the `PauseMenuPanel`.
- **Assign Reference:**
  - Drag the `PauseMenuPanel` to the `panel` field.
- **Set the panel to inactive by default**.

---

## 7. Assign UI References in GameManager
- Select your GameManager object in the scene.
- Drag each UI GameObject (with the relevant script) into the corresponding field in the Inspector:
  - `towerHealthBar` → TowerHealthBar GameObject
  - `resourceCounterUI` → ResourceCounterText GameObject
  - `defenderCostUI` → DefenderCostText GameObject
  - `gameOverUI` → GameOverPanel GameObject
  - `pauseMenuUI` → PauseMenuPanel GameObject

---

## 8. (Optional) Defender Health Bars
- For defender health bars, repeat the Tower Health Bar steps, but make the health bar a child of each defender prefab and assign the `HealthBarUI` script there.

---

## 9. Tips
- Use TextMeshPro for all text (import TMP Essentials if prompted).
- Use anchors to keep UI elements in the right place for different resolutions.
- You can further customize the look and feel in the Inspector.
- Save your scene and make UI panels prefabs if you want to reuse them.

---

## 10. Summary Table
| UI Element         | Script             | Assign In Inspector To           |
|--------------------|--------------------|----------------------------------|
| TowerHealthBar     | HealthBarUI        | GameManager.towerHealthBar       |
| ResourceCounterText| ResourceCounterUI  | GameManager.resourceCounterUI    |
| DefenderCostText   | DefenderCostUI     | GameManager.defenderCostUI       |
| GameOverPanel      | GameOverUI         | GameManager.gameOverUI           |
| PauseMenuPanel     | PauseMenuUI        | GameManager.pauseMenuUI          |

---

---

## 11. Wave System and Ready-Up UI

### **A. WaveManager Setup**
1. **Attach the Script:**
   - Create an empty GameObject in your scene (e.g., `GameManager` or `WaveManager`).
   - Attach the `WaveManager` script to it.

2. **Configure Waves:**
   - Set `TotalWaves` to `3` (or your desired number).
   - Assign your `EnemySpawner` GameObject to the `EnemySpawner` field in the Inspector.

3. **Events:**
   - Use the events in `WaveManager` (e.g., `OnWaveStart`, `OnReadyUpPhaseStart`) to trigger UI updates or camera switches.
   - Example: Add a listener to `OnReadyUpPhaseStart` to show the ready-up panel.

---

### **B. Ready-Up UI Panel**
1. **Create the Panel:**
   - Right-click on Canvas → `UI` → `Panel`. Rename to `ReadyUpPanel`.
   - Set the panel’s color to semi-transparent black or a theme color.

2. **Add a "Ready" Button:**
   - Right-click on the panel → `UI` → `Button - TextMeshPro`. Rename to `ReadyButton`.
   - Change the button text to "Ready".
   - Position the button in the center of the panel.

3. **Attach Script (Optional):**
   - If you want to handle the ready-up logic in a separate script, create a `ReadyUpUI` script and attach it to the panel.
   - Example `ReadyUpUI` script:
     ```csharp
     using UnityEngine;
     using UnityEngine.UI;

     public class ReadyUpUI : MonoBehaviour
     {
         public Button readyButton;
         public GameObject readyUpPanel;

         private void Start()
         {
             readyButton.onClick.AddListener(OnReadyButtonClicked);
             readyUpPanel.SetActive(false); // Hide by default
         }

         private void OnReadyButtonClicked()
         {
             WaveManager.Instance.OnReadyButtonClicked();
             readyUpPanel.SetActive(false);
         }
     }
     ```

4. **Assign References:**
   - Drag the `ReadyButton` to the `readyButton` field in the `ReadyUpUI` script.
   - Drag the `ReadyUpPanel` to the `readyUpPanel` field.

5. **Connect to WaveManager:**
   - In the `WaveManager` Inspector, add a listener to `OnReadyUpPhaseStart`:
     - Click `+` in the event list.
     - Drag the `ReadyUpPanel` GameObject into the slot.
     - Select `ReadyUpUI` → `gameObject.SetActive` → `True`.
   - Add a listener to `OnReadyUpPhaseEnd`:
     - Select `ReadyUpUI` → `gameObject.SetActive` → `False`.

---

### **C. Overhead Camera Setup**
1. **Create the Camera:**
   - Right-click in the Hierarchy → `Camera`. Rename to `OverheadCamera`.
   - Position it above the game area (e.g., `Y = 20`, rotation `X = 90` for top-down view).
   - Set `Clear Flags` to `Depth` and disable audio listener if not needed.

2. **Camera Controller Script:**
   - Create a `CameraController.cs` script (see below) and attach it to an empty GameObject (e.g., `CameraController`).
   - Assign both `MainCamera` and `OverheadCamera` in the Inspector.

   ```csharp
   using UnityEngine;

   public class CameraController : MonoBehaviour
   {
       public Camera MainCamera;
       public Camera OverheadCamera;

       private void Start()
       {
           // Start with the main camera
           SwitchToMainCamera();
       }

       public void SwitchToMainCamera()
       {
           MainCamera.enabled = true;
           OverheadCamera.enabled = false;
       }

       public void SwitchToOverheadCamera()
       {
           MainCamera.enabled = false;
           OverheadCamera.enabled = true;
       }
   }
   ```

3. **Connect to WaveManager:**
   - In the `WaveManager` Inspector:
     - Add a listener to `OnReadyUpPhaseStart`:
       - Drag the `CameraController` GameObject into the slot.
       - Select `CameraController` → `SwitchToOverheadCamera`.
     - Add a listener to `OnReadyUpPhaseEnd`:
       - Select `CameraController` → `SwitchToMainCamera`.

---

### **D. Enemy Spawner Integration**
1. **Modify Enemy Spawner:**
   - Ensure your enemy spawner script listens to `WaveManager.Instance.IsSpawningEnemies`.
   - Example:
     ```csharp
     private void Update()
     {
         if (WaveManager.Instance.IsSpawningEnemies)
         {
             // Spawn enemies for the current wave
         }
     }
     ```

2. **Wave Difficulty:**
   - Use `WaveManager.Instance.CurrentWave` to scale enemy health/speed.
   - Example:
     ```csharp
     float healthMultiplier = 1.0f + (WaveManager.Instance.CurrentWave * 0.5f);
     enemy.GetComponent<Health>().MaxHealth *= healthMultiplier;
     ```

---

### **E. Testing**
1. **Start the Game:**
   - The first wave should start automatically.
   - After the wave ends, the ready-up panel and overhead camera should activate.

2. **Ready Up:**
   - Click the "Ready" button to start the next wave.
   - The camera should switch back, and enemies should spawn for the next wave.

3. **Repeat:**
   - After 3 waves, `OnAllWavesComplete` should trigger (e.g., show a win screen).

---

---

## 12. Drag-and-Drop Defender Placement

### **A. Defender Inventory UI Setup**
1. **Create the Inventory Panel:**
   - Right-click on Canvas → `UI` → `Panel`. Rename to `DefenderInventoryPanel`.
   - Add a `Grid Layout Group` component to the panel for automatic icon arrangement.
   - Set `Child Alignment` to `Middle Center` and adjust `Cell Size` and `Spacing` as needed.

2. **Create the Defender Icon Prefab:**
   - Right-click on the panel → `UI` → `Image`. Rename to `DefenderIcon`.
   - Add a `TextMeshPro - Text` child for the cost display.
   - Add the `DefenderIcon` script to this GameObject.
   - Drag this GameObject into the `Assets/UI` folder to create a prefab.

3. **Attach the `DefenderInventoryUI` Script:**
   - Add the `DefenderInventoryUI` script to the `DefenderInventoryPanel`.
   - Assign the following in the Inspector:
     - `InventoryPanel`: The `DefenderInventoryPanel` GameObject.
     - `DefenderIconPrefab`: The prefab you created in step 2.
     - `IconContainer`: The `DefenderInventoryPanel` (or a child with a `Grid Layout Group`).
     - `DefenderPrefabs`: Drag your defender prefabs into this list.
     - `DefenderCosts`: Set the cost for each defender (parallel to `DefenderPrefabs`).
     - `PlacementLayerMask`: Set to the layer mask for valid placement (e.g., "Ground").

4. **Configure Placement Settings:**
   - Set `MinPathDistance` to the maximum allowed distance from the path.
   - Customize `ValidPlacementColor` and `InvalidPlacementColor` for visual feedback.

---

### **B. Defender Prefabs**
1. **Prepare Defender Prefabs:**
   - Ensure each defender has a collider for placement validation.
   - Add a `Health` component to each defender for health bars (see next section).

2. **Defender Icons:**
   - Create icon sprites for each defender (e.g., 128x128 PNGs) and place them in `Assets/UI/Icons/`.
   - The `DefenderIcon` script will attempt to load sprites from `Resources/Icons/{DefenderName}_Icon`.

---

### **C. Path Validation (Optional)**
To validate defender placement near the path:
1. **Tag Your Path:**
   - Tag your path GameObject as "Path" or add it to a specific layer.
2. **Modify `IsPlacementValid` in `DefenderInventoryUI`:**
   - Use `Physics.OverlapSphere` or `Physics.Raycast` to check proximity to the path.
   - Example:
     ```csharp
     private bool IsPlacementValid(Vector3 position)
     {
         Collider[] colliders = Physics.OverlapSphere(position, MinPathDistance, PathLayerMask);
         return colliders.Length > 0; // Valid if near the path
     }
     ```

---

### **D. Testing**
1. **Enter Ready-Up Phase:**
   - Start the game and complete the first wave to trigger the ready-up phase.
   - The defender inventory should appear, and the camera should switch to overhead view.
2. **Drag and Drop:**
   - Drag a defender icon from the inventory and drop it near the path.
   - Valid locations will show a green indicator; invalid locations will show red.
3. **Spawn Defenders:**
   - If placement is valid, the defender prefab will spawn at the drop location.

---

---

## 13. Defender Health Bars

### **A. Health Component Setup**
1. **Add `Health` to Defender Prefabs:**
   - Select each defender prefab in the `Assets/Prefabs` folder.
   - Add the `Health` component and set `MaxHealth` (e.g., `100`).
   - Add the `DefenderHealthBarSpawner` component.

2. **Assign Health Bar Prefab:**
   - Drag the `DefenderHealthBar` prefab (created below) into the `HealthBarPrefab` field of `DefenderHealthBarSpawner`.
   - Optionally, assign a `WorldSpaceCanvas` if you have one (otherwise, it will find the first canvas).

---

### **B. Create the Health Bar Prefab**
1. **Create a World-Space Canvas:**
   - Right-click in the Hierarchy → `UI` → `Canvas`. Rename to `WorldSpaceCanvas`.
   - Set `Render Mode` to `World Space`.
   - Position at `(0, 0, 0)` and scale to `(0.01, 0.01, 0.01)`.

2. **Create the Health Bar UI:**
   - Right-click on `WorldSpaceCanvas` → `UI` → `Slider`. Rename to `DefenderHealthBar`.
   - Remove the `Handle Slide Area` child.
   - Resize the slider to your preferred health bar size.
   - Set the `Fill` child’s color to green.

3. **Add the `HealthBarUI` Script:**
   - Add the `HealthBarUI` script to the `DefenderHealthBar` GameObject.
   - Assign the `healthSlider` and `fillImage` fields in the Inspector.

4. **Create a Prefab:**
   - Drag the `DefenderHealthBar` GameObject into the `Assets/UI` folder to create a prefab.
   - Delete the instance from the scene.

---

### **C. Enemy Damage Integration**
1. **Modify Enemy Scripts:**
   - Ensure enemies have a collider and a script to handle collisions with defenders.
   - Example:
     ```csharp
     private void OnCollisionEnter(Collision collision)
     {
         Health defenderHealth = collision.gameObject.GetComponent<Health>();
         if (defenderHealth != null)
         {
             defenderHealth.TakeDamage(10.0f); // Deal 10 damage
         }
     }
     ```

2. **Test Damage:**
   - Start the game and verify that enemies damage defenders.
   - The health bar should update and turn red as health decreases.

---

### **D. Testing**
1. **Spawn Defenders:**
   - Enter the ready-up phase and place a defender using drag-and-drop.
   - A health bar should appear above the defender.
2. **Damage Defenders:**
   - Let enemies collide with the defender. The health bar should update dynamically.
3. **Defender Death:**
   - When the defender’s health reaches 0, it should be destroyed, and the health bar should disappear.

---

---

## 14. Path-Based Defender Placement

### **A. Overview**
This system allows players to place defenders on paths using a button when they are near a path. It replaces the drag-and-drop system and avoids the need for a second camera.

---

### **B. Setup Steps**

#### **1. Create a Placement UI Panel**
- Right-click on your `Canvas` → `UI` → `Panel`. Rename to `PlacementUIPanel`.
- Add a `Button` (rename to `PlaceDefenderButton`) with text like "Place Defender".
- Position the panel and button appropriately (e.g., bottom center).

#### **2. Tag or Layer Your Paths**
- Ensure your path GameObjects are on a specific layer (e.g., "Path").
- Set the `PathLayerMask` in the `PathPlacementManager` to this layer.

#### **3. Attach the `PathPlacementManager` Script**
- Create an empty GameObject (e.g., `PathPlacementManager`).
- Attach the `PathPlacementManager` script (`Assets/Scripts/UI/PathPlacementManager.cs`).
- Assign the following in the Inspector:
  - `Player`: Your player GameObject.
  - `PlaceDefenderButton`: The button created in step 1.
  - `PlacementUIPanel`: The panel created in step 1.
  - `DefenderPrefab`: The defender prefab to spawn.
  - `DefenderCost`: The cost to place a defender (e.g., `50`).
  - `PathLayerMask`: The layer mask for path detection.
  - `MaxPathDistance`: The maximum distance from the path to allow placement (e.g., `2.0`).

#### **4. Configure WaveManager Events**
- The `PathPlacementManager` automatically subscribes to `WaveManager` events to show/hide the placement UI during the ready-up phase.

---

### **C. Testing**
1. **Start the Game**:
   - Wave 1 begins automatically.
2. **Complete Wave 1**:
   - The ready-up phase starts, and the placement UI appears.
3. **Move Near a Path**:
   - The "Place Defender" button becomes interactable when you are near a path.
4. **Place a Defender**:
   - Click the button to spawn a defender at the nearest path location.
5. **Ready Up**:
   - Click the "Ready" button to start Wave 2.

---

You’re now ready to hook up your UI to the game logic! See `WaveManager`, `CameraController`, `PathPlacementManager`, and `HealthBarUI` for further customization.