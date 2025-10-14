using UnityEngine;
using UnityEngine.UI;

public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    
    private GameManager gameManager;

    void Start()
    {
        // Get GameManager reference
        gameManager = FindFirstObjectByType<GameManager>();
        
        // Set up button listeners
        if (resumeButton != null)
            resumeButton.onClick.AddListener(OnResumeClicked);
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);
    }

    public void Show()
    {
        if (panel != null) panel.SetActive(true);
    }

    public void Hide()
    {
        if (panel != null) panel.SetActive(false);
    }
    
    // Button click handlers
    public void OnResumeClicked()
    {
        if (gameManager != null)
            gameManager.ResumeGame();
    }
    
    public void OnRestartClicked()
    {
        if (gameManager != null)
            gameManager.RestartGame();
    }
} 