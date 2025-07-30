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

You’re now ready to hook up your UI to the game logic! See GameManager for code examples on updating the UI at runtime. 