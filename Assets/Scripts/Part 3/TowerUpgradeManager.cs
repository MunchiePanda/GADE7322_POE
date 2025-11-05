using UnityEngine;

/// <summary>
/// Handles upgrading the Main Tower by swapping its prefab to Tier 2 or Tier 3.
/// - Spends resources via GameManager
/// - Replaces the current tower instance and updates GameManager references
/// - Preserves health ratio across swaps
///
/// Usage: Hook UI Buttons to UpgradeToTier2() / UpgradeToTier3()
/// Assign T2/T3 prefabs in the inspector.
/// </summary>
public class TowerUpgradeManager : MonoBehaviour
{
    public static TowerUpgradeManager Instance { get; private set; }

    [Header("Tower Tier Prefabs")]
    public GameObject tier2TowerPrefab;
    public GameObject tier3TowerPrefab;

    [Header("Costs")]
    public int tier2Cost = 250;
    public int tier3Cost = 500;

    public enum UpgradeAction { Tier2, Tier3, NextTier }
    [Header("Button Upgrade Config")]
    [Tooltip("Which upgrade to perform when calling PerformUpgrade() from a UI Button")]
    public UpgradeAction buttonUpgrade = UpgradeAction.NextTier;

    [Header("State")] 
    [Tooltip("0 = base, 1 = tier2, 2 = tier3")] public int towerLevel = 0;

    private GameManager gameManager;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
    }

    public bool UpgradeToTier2()
    {
        return TryUpgradeToLevel(1);
    }

    public bool UpgradeToTier3()
    {
        return TryUpgradeToLevel(2);
    }

    public bool UpgradeNextTier()
    {
        int target = Mathf.Clamp(towerLevel + 1, 0, 2);
        return TryUpgradeToLevel(target);
    }

    /// <summary>
    /// Mirrors DefenderUpgradeManager.TryUpgrade(...) style: upgrades the Main Tower by one tier.
    /// Hook your UI button to this for a single-call upgrade like defenders.
    /// </summary>
    public bool TryUpgrade()
    {
        return UpgradeNextTier();
    }

    /// <summary>
    /// Call this from a UI Button. Uses the selected 'buttonUpgrade' action.
    /// </summary>
    public void PerformUpgrade()
    {
        switch (buttonUpgrade)
        {
            case UpgradeAction.Tier2:
                UpgradeToTier2();
                break;
            case UpgradeAction.Tier3:
                UpgradeToTier3();
                break;
            case UpgradeAction.NextTier:
                UpgradeNextTier();
                break;
        }
    }

    private bool TryUpgradeToLevel(int targetLevel)
    {
        if (gameManager == null) return false;
        if (targetLevel <= towerLevel) return false; // no downgrade or duplicate
        if (targetLevel < 1 || targetLevel > 2) return false;

        int cost = targetLevel == 1 ? tier2Cost : tier3Cost;
        if (!gameManager.SpendResources(cost)) return false;

        GameObject prefab = GetPrefabForLevel(targetLevel);
        if (prefab == null) return false;

        bool replaced = gameManager.ReplaceTower(prefab);
        if (!replaced) return false;

        towerLevel = targetLevel;
        return true;
    }

    private GameObject GetPrefabForLevel(int level)
    {
        switch (level)
        {
            case 1: return tier2TowerPrefab;
            case 2: return tier3TowerPrefab;
        }
        return null;
    }
}


