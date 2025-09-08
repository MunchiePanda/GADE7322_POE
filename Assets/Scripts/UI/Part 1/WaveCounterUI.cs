using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays the current wave number in the UI.
/// Attach to a UI GameObject with a TextMeshPro component.
/// </summary>
public class WaveCounterUI : MonoBehaviour
{
    [Tooltip("Assign in Inspector: TextMeshPro component to display the wave number.")]
    public TextMeshProUGUI waveText;

    [Tooltip("Assign in Inspector: GameManager reference.")]
    public GameManager gameManager;

    void Start()
    {
        if (waveText == null)
        {
            waveText = GetComponent<TextMeshProUGUI>();
        }

        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }

        UpdateWaveText();
    }

    void Update()
    {
        UpdateWaveText();
    }

    /// <summary>
    /// Updates the wave text to reflect the current wave.
    /// </summary>
    void UpdateWaveText()
    {
        if (waveText != null && gameManager != null)
        {
            waveText.text = $"Wave: {gameManager.currentWave}";
        }
    }
}
