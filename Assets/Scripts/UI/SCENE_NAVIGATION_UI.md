# Scene Navigation UI Setup Guide

This guide explains how to set up the `SceneNavigationUI.cs` script to handle navigation between the **Main Menu**, **Game**, and **End Menu** scenes.

---

## Table of Contents
1. [Overview](#overview)
2. [Script Setup](#script-setup)
3. [Scene Setup](#scene-setup)
   - [Main Menu](#main-menu)
   - [Game Scene](#game-scene)
   - [End Menu](#end-menu)
4. [Button Configuration](#button-configuration)
5. [Build Settings](#build-settings)
6. [Testing](#testing)

---

## Overview
The `SceneNavigationUI.cs` script provides a simple way to navigate between scenes using UI buttons. It supports:
- **Main Menu**: Start Game, Quit.
- **Game Scene**: Back to Menu.
- **End Menu**: Restart, Back to Menu.

---

## Script Setup
1. **Create the Script**:
   - The script is located at `Assets/Scripts/UI/SceneNavigationUI.cs`.
   - If it doesn’t exist, create it using the code snippet provided in the [IMPLEMENTATION_EXPLANATION.md](../IMPLEMENTATION_EXPLANATION.md).

2. **Attach the Script**:
   - In each scene (`MainMenu`, `SampleScene`, `EndMenu`), add a **UI Canvas** (if not already present).
   - Create a new **Empty GameObject** (e.g., `UIManager`) and attach the `SceneNavigationUI.cs` script.

---

## Scene Setup
### Main Menu
1. **Canvas Setup**:
   - Add a **Canvas** to the scene (GameObject > UI > Canvas).
   - Add **Button** objects for `Start Game` and `Quit` (GameObject > UI > Button).

2. **Button Configuration**:
   - Rename the buttons to `StartGameButton` and `QuitButton`.
   - Assign the buttons to the `startGameButton` and `quitButton` fields in the `SceneNavigationUI` inspector.

3. **Text**:
   - Update the button text to "Start Game" and "Quit".

---

### Game Scene
1. **Canvas Setup**:
   - Ensure the game scene has a **Canvas** (usually already present for in-game UI).
   - Add a **Button** for `Back to Menu` (GameObject > UI > Button).

2. **Button Configuration**:
   - Rename the button to `MainMenuButton`.
   - Assign the button to the `mainMenuButton` field in the `SceneNavigationUI` inspector.

3. **Text**:
   - Update the button text to "Back to Menu".

---

### End Menu
1. **Canvas Setup**:
   - Add a **Canvas** to the scene.
   - Add **Button** objects for `Restart` and `Back to Menu`.

2. **Button Configuration**:
   - Rename the buttons to `RestartButton` and `MainMenuButton`.
   - Assign the buttons to the `restartButton` and `mainMenuButton` fields in the `SceneNavigationUI` inspector.

3. **Text**:
   - Update the button text to "Restart" and "Back to Menu".

---

## Button Configuration
1. **Inspector Setup**:
   - Select the `UIManager` object in the scene.
   - In the Inspector, assign the button references to the corresponding fields in `SceneNavigationUI`:
     - `mainMenuButton`: Button for navigating to the Main Menu.
     - `startGameButton`: Button for starting the game.
     - `restartButton`: Button for restarting the current scene.
     - `quitButton`: Button for quitting the game.

2. **OnClick Events**:
   - The script automatically assigns `onClick` listeners in the `Start()` method. No manual setup is required.

---

## Build Settings
1. **Add Scenes to Build**:
   - Open **File > Build Settings**.
   - Add the following scenes:
     - `Assets/Scenes/MainMenu.unity`
     - `Assets/Scenes/SampleScene.unity`
     - `Assets/Scenes/EndMenu.unity`
   - Ensure the scenes are in the correct order (e.g., `MainMenu` first).

2. **Scene Names**:
   - The script uses the scene names as strings (e.g., `"MainMenu"`, `"SampleScene"`). Ensure these match exactly with the scene names in the Build Settings.

---

## Testing
1. **Main Menu**:
   - Press `Start Game`: Should load the `SampleScene`.
   - Press `Quit`: Should quit the game (or stop play mode in the Editor).

2. **Game Scene**:
   - Press `Back to Menu`: Should return to the `MainMenu`.

3. **End Menu**:
   - Press `Restart`: Should reload the `SampleScene`.
   - Press `Back to Menu`: Should return to the `MainMenu`.

---

## Troubleshooting
- **Buttons Not Working**:
  - Ensure buttons are assigned in the Inspector.
  - Check that scene names in `LoadScene()` match the Build Settings.
- **Scene Not Loading**:
  - Verify the scene is added to **Build Settings**.
  - Check for typos in scene names.
- **Script Errors**:
  - Ensure `using UnityEngine.SceneManagement` and `using UnityEngine.UI` are included in `SceneNavigationUI.cs`.

---

## Example: SceneNavigationUI.cs
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
        // Assign button click listeners
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(() => LoadScene("MainMenu"));
        if (startGameButton != null) startGameButton.onClick.AddListener(() => LoadScene("SampleScene"));
        if (restartButton != null) restartButton.onClick.AddListener(() => LoadScene(SceneManager.GetActiveScene().name));
        if (quitButton != null) quitButton.onClick.AddListener(QuitGame);
    }

    // Load a scene by name
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // Quit the game
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
- **Consistency**: Use the same script across all scenes for uniformity.
- **Extensibility**: Add more buttons/methods as needed (e.g., `LoadScene("SettingsMenu")`).
- **Localization**: Update button text for different languages if needed.
