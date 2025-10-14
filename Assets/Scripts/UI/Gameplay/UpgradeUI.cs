using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles UI for upgrading the tower and defenders.
/// Attach to a UI GameObject with buttons for upgrades.
/// </summary>
public class UpgradeUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Assign in Inspector: GameManager reference.")]
    public GameManager gameManager;

    [Tooltip("Assign in Inspector: Tower reference.")]
    public Tower tower;

    [Tooltip("Assign in Inspector: Button for upgrading tower health.")]
    public Button upgradeTowerHealthButton;

    [Tooltip("Assign in Inspector: Button for upgrading tower damage.")]
    public Button upgradeTowerDamageButton;

    [Tooltip("Assign in Inspector: Text to display tower health upgrade cost.")]
    public TextMeshProUGUI towerHealthUpgradeCostText;

    [Tooltip("Assign in Inspector: Text to display tower damage upgrade cost.")]
    public TextMeshProUGUI towerDamageUpgradeCostText;

    void Start()
    {
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }

        if (tower == null)
        {
            tower = FindFirstObjectByType<Tower>();
        }

        if (upgradeTowerHealthButton != null)
        {
            upgradeTowerHealthButton.onClick.AddListener(UpgradeTowerHealth);
        }

        if (upgradeTowerDamageButton != null)
        {
            upgradeTowerDamageButton.onClick.AddListener(UpgradeTowerDamage);
        }

        UpdateUpgradeCostTexts();
    }

    /// <summary>
    /// Upgrades the tower's health.
    /// </summary>
    void UpgradeTowerHealth()
    {
        if (tower != null)
        {
            tower.UpgradeHealth();
            UpdateUpgradeCostTexts();
        }
    }

    /// <summary>
    /// Upgrades the tower's damage.
    /// </summary>
    void UpgradeTowerDamage()
    {
        if (tower != null)
        {
            tower.UpgradeDamage();
            UpdateUpgradeCostTexts();
        }
    }

    /// <summary>
    /// Updates the UI text for upgrade costs.
    /// </summary>
    void UpdateUpgradeCostTexts()
    {
        if (towerHealthUpgradeCostText != null && tower != null)
        {
            towerHealthUpgradeCostText.text = $"Cost: {tower.healthUpgradeCost}";
        }

        if (towerDamageUpgradeCostText != null && tower != null)
        {
            towerDamageUpgradeCostText.text = $"Cost: {tower.damageUpgradeCost}";
        }
    }
}
