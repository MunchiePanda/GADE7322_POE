using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple UI hook for a Button to upgrade the Main Tower.
/// Assign this to a Button and wire the OnClick to TowerUpgradeButton.OnClick.
/// Configure the target action and (optionally) auto-disable when max tier.
/// </summary>
public class TowerUpgradeButton : MonoBehaviour
{
    [Header("References")]
    public TowerUpgradeManager upgradeManager;

    [Header("Action")]
    public TowerUpgradeManager.UpgradeAction action = TowerUpgradeManager.UpgradeAction.NextTier;

    [Header("Optional")] 
    [Tooltip("If assigned, will be set interactable=false when tower is already at max tier.")]
    public Button button;

    void Reset()
    {
        button = GetComponent<Button>();
    }

    void Update()
    {
        if (button != null && upgradeManager != null)
        {
            // Disable button if already at max tier
            button.interactable = upgradeManager.towerLevel < 2;
        }
    }

    public void OnClick()
    {
        if (upgradeManager == null) return;

        switch (action)
        {
            case TowerUpgradeManager.UpgradeAction.Tier2:
                upgradeManager.UpgradeToTier2();
                break;
            case TowerUpgradeManager.UpgradeAction.Tier3:
                upgradeManager.UpgradeToTier3();
                break;
            case TowerUpgradeManager.UpgradeAction.NextTier:
                upgradeManager.UpgradeNextTier();
                break;
        }
    }
}


