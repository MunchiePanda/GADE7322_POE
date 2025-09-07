using UnityEngine;
using TMPro;

/// <summary>
/// Displays available tower power-ups in the UI.
/// Attach to a UI GameObject with a TextMeshPro component.
/// </summary>
public class TowerPowerUpUI : MonoBehaviour
{
    [Tooltip("Assign in Inspector: TextMeshPro component to display power-up info.")]
    public TextMeshProUGUI powerUpText;

    [Tooltip("Assign in Inspector: Tower reference.")]
    public Tower tower;

    [Tooltip("Assign in Inspector: GameManager reference.")]
    public GameManager gameManager;

    void Start()
    {
        if (powerUpText == null)
        {
            powerUpText = GetComponent<TextMeshProUGUI>();
        }

        if (tower == null)
        {
            tower = FindFirstObjectByType<Tower>();
        }

        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }

        UpdatePowerUpText();
    }

    void Update()
    {
        UpdatePowerUpText();
    }

    /// <summary>
    /// Updates the power-up text to reflect available upgrades.
    /// </summary>
    void UpdatePowerUpText()
    {
        if (powerUpText == null || tower == null || gameManager == null)
            return;

        int resources = gameManager.GetResources();
        bool canUpgradeHealth = resources >= tower.healthUpgradeCost;
        bool canUpgradeDamage = resources >= tower.damageUpgradeCost;

        powerUpText.text =
            $"Power-Ups:\n" +
            $"• Health Upgrade: {tower.healthUpgradeCost} resources {(canUpgradeHealth ? "(Available)" : "(Not Enough Resources)")}\n" +
            $"• Damage Upgrade: {tower.damageUpgradeCost} resources {(canUpgradeDamage ? "(Available)" : "(Not Enough Resources)")}";
    }
}
