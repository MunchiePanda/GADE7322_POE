using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles UI for selecting defender types (e.g., Sniper, AoE).
/// Attach to a UI GameObject with buttons for each defender type.
/// </summary>
public class DefenderSelectionUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Assign in Inspector: GameManager reference.")]
    public GameManager gameManager;

    [Tooltip("Assign in Inspector: DefenderPlacement reference.")]
    public DefenderPlacement defenderPlacement;

    [Tooltip("Assign in Inspector: Button for selecting Sniper Defender.")]
    public Button sniperDefenderButton;

    [Tooltip("Assign in Inspector: Button for selecting AoE Defender.")]
    public Button aoeDefenderButton;

    [Tooltip("Assign in Inspector: Prefabs for defender types.")]
    public GameObject sniperDefenderPrefab;
    public GameObject aoeDefenderPrefab;

    [Tooltip("Assign in Inspector: Text to display defender cost.")]
    public TextMeshProUGUI defenderCostText;

    private GameObject currentDefenderPrefab;

    void Start()
    {
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }

        if (defenderPlacement == null)
        {
            defenderPlacement = FindFirstObjectByType<DefenderPlacement>();
        }

        if (sniperDefenderButton != null)
        {
            sniperDefenderButton.onClick.AddListener(() => SelectDefenderType(sniperDefenderPrefab));
        }

        if (aoeDefenderButton != null)
        {
            aoeDefenderButton.onClick.AddListener(() => SelectDefenderType(aoeDefenderPrefab));
        }

        // Default to sniper defender
        SelectDefenderType(sniperDefenderPrefab);
    }

    /// <summary>
    /// Selects a defender type and updates the placement prefab.
    /// </summary>
    void SelectDefenderType(GameObject defenderPrefab)
    {
        if (defenderPrefab == null) return;

        currentDefenderPrefab = defenderPrefab;
        defenderPlacement.defenderPrefab = defenderPrefab;

        // Update cost text
        if (defenderCostText != null)
        {
            defenderCostText.text = $"Cost: {gameManager.defenderCost}";
        }

        Debug.Log($"Selected defender type: {defenderPrefab.name}");
    }
}
