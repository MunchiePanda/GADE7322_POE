using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Handles navigation between scenes (Main Menu, Game, End Menu).
/// Attach to a UI GameObject in each scene and assign button references in the Inspector.
/// </summary>
public class SceneNavigationUI : MonoBehaviour
{
    [Header("Button References")]
    [Tooltip("Assign in Inspector: Button for navigating to Main Menu.")]
    public Button mainMenuButton;

    [Tooltip("Assign in Inspector: Button for starting the game.")]
    public Button startGameButton;

    [Tooltip("Assign in Inspector: Button for restarting the current scene.")]
    public Button restartButton;

    [Tooltip("Assign in Inspector: Button for quitting the game.")]
    public Button quitButton;

    void Start()
    {
        // Assign button click listeners
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(() => LoadScene("MainMenu"));

        if (startGameButton != null)
            startGameButton.onClick.AddListener(() => LoadScene("SampleScene"));

        if (restartButton != null)
            restartButton.onClick.AddListener(() => LoadScene(SceneManager.GetActiveScene().name));

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
    }

    /// <summary>
    /// Loads a scene by name.
    /// </summary>
    /// <param name="sceneName">Name of the scene to load (must be in Build Settings).</param>
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Quits the game (or stops play mode in the Editor).
    /// </summary>
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
