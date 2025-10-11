using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays the wave countdown timer in the UI.
/// Shows time remaining until the next wave starts.
/// Attach to a UI GameObject with a TextMeshPro component.
/// </summary>
public class WaveCountdownUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Assign in Inspector: TextMeshPro component to display the countdown.")]
    public TextMeshProUGUI countdownText;

    [Tooltip("Assign in Inspector: GameManager reference.")]
    public GameManager gameManager;

    [Header("Display Settings")]
    [Tooltip("Format for displaying the countdown. {0} will be replaced with the time.")]
    public string countdownFormat = "Next Wave: {0}s";

    [Tooltip("Color when countdown is active.")]
    public Color activeColor = Color.yellow;

    [Tooltip("Color when no countdown is active.")]
    public Color inactiveColor = Color.gray;

    private bool isCountingDown = false;
    private float timeRemaining = 0f;

    void Start()
    {
        if (countdownText == null)
        {
            countdownText = GetComponent<TextMeshProUGUI>();
        }

        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }

        // Initially hide the countdown
        if (countdownText != null)
        {
            countdownText.text = "";
        }
    }

    void Update()
    {
        if (isCountingDown)
        {
            timeRemaining -= Time.deltaTime;
            UpdateCountdownDisplay();

            if (timeRemaining <= 0f)
            {
                StopCountdown();
            }
        }
    }

    /// <summary>
    /// Starts the countdown timer for the next wave.
    /// </summary>
    /// <param name="duration">Duration of the countdown in seconds.</param>
    public void StartCountdown(float duration)
    {
        isCountingDown = true;
        timeRemaining = duration;
        
        if (countdownText != null)
        {
            countdownText.color = activeColor;
        }
        
        UpdateCountdownDisplay();
    }

    /// <summary>
    /// Stops the countdown timer and hides the display.
    /// </summary>
    public void StopCountdown()
    {
        isCountingDown = false;
        timeRemaining = 0f;
        
        if (countdownText != null)
        {
            countdownText.text = "";
            countdownText.color = inactiveColor;
        }
    }

    /// <summary>
    /// Updates the countdown display text.
    /// </summary>
    void UpdateCountdownDisplay()
    {
        if (countdownText != null && isCountingDown)
        {
            int secondsRemaining = Mathf.CeilToInt(timeRemaining);
            countdownText.text = string.Format(countdownFormat, secondsRemaining);
        }
    }

    /// <summary>
    /// Gets whether the countdown is currently active.
    /// </summary>
    /// <returns>True if countdown is active, false otherwise.</returns>
    public bool IsCountingDown()
    {
        return isCountingDown;
    }

    /// <summary>
    /// Gets the remaining time in the countdown.
    /// </summary>
    /// <returns>Time remaining in seconds.</returns>
    public float GetTimeRemaining()
    {
        return timeRemaining;
    }
}
