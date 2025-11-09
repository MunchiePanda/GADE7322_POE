using UnityEngine;
using GADE7322_POE.Core;

/// <summary>
/// Handles global upgrades for defender types:
/// - Deducts resources
/// - Swaps all existing defenders of a type to the upgraded prefab
/// - Redirects future placements to the upgraded prefab
/// </summary>
public class DefenderUpgradeManager : MonoBehaviour
{
    public static DefenderUpgradeManager Instance { get; private set; }

    [Header("Tier 2 Prefabs")]
    public GameObject basicTier2Prefab;
    public GameObject frostTier2Prefab;
    public GameObject lightningTier2Prefab;

    [Header("Tier 3 Prefabs")]
    public GameObject basicTier3Prefab;
    public GameObject frostTier3Prefab;
    public GameObject lightningTier3Prefab;

    [Header("Upgrade Settings")]
    public int upgradeCost = 10;

    // 0 = base, 1 = tier 2, 2 = tier 3
    private int basicLevel;
    private int frostLevel;
    private int lightningLevel;

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

    // Returns true if the defender type has been upgraded at least once
    public bool IsUpgraded(DefenderType type)
    {
        return GetLevel(type) > 0;
    }

    // Returns the current upgrade level for the defender type (0, 1, or 2)
    public int GetLevel(DefenderType type)
    {
        switch (type)
        {
            case DefenderType.Basic: return basicLevel;
            case DefenderType.FrostTower: return frostLevel;
            case DefenderType.LightningTower: return lightningLevel;
        }
        return 0;
    }

    /// <summary>
    /// Tries to upgrade the given defender type. Deducts cost, swaps all existing, and updates prefab for future placements.
    /// </summary>
    public bool TryUpgrade(DefenderType type)
    {
        if (gameManager == null) return false;
        int currentLevel = GetLevel(type);
        if (currentLevel >= 2) return false; // already maxed
        if (!gameManager.SpendResources(upgradeCost)) return false;

        SetLevel(type, currentLevel + 1);
        int targetLevel = GetLevel(type);
        SwapExistingDefenders(type, targetLevel);
        ApplyFuturePrefabSwap(type, targetLevel);
        return true;
    }

    // Sets the upgrade level for the defender type (clamped 0-2)
    private void SetLevel(DefenderType type, int value)
    {
        switch (type)
        {
            case DefenderType.Basic: basicLevel = Mathf.Clamp(value, 0, 2); break;
            case DefenderType.FrostTower: frostLevel = Mathf.Clamp(value, 0, 2); break;
            case DefenderType.LightningTower: lightningLevel = Mathf.Clamp(value, 0, 2); break;
        }
    }

    // Swaps all existing defenders of the given type to the upgraded prefab, preserving position and health ratio
    private void SwapExistingDefenders(DefenderType type, int targetLevel)
    {
        Defender[] allDefenders = FindObjectsByType<Defender>(FindObjectsSortMode.None);
        foreach (var defender in allDefenders)
        {
            if (defender == null) continue;
            var foundType = GetDefenderType(defender.gameObject);
            if (foundType != type) continue;

            GameObject targetPrefab = GetTierPrefab(type, targetLevel);
            if (targetPrefab == null) continue;

            Transform tr = defender.transform;
            Vector3 pos = tr.position;
            Quaternion rot = tr.rotation;
            Transform parent = tr.parent;

            // Preserve health ratio
            float ratio = 1f;
            var oldHealth = defender.GetComponent<Health>();
            if (oldHealth != null && oldHealth.MaxHealth > 0.01f)
                ratio = Mathf.Clamp01(oldHealth.CurrentHealth / oldHealth.MaxHealth);

            Destroy(defender.gameObject);
            GameObject upgraded = Instantiate(targetPrefab, pos, rot, parent);

            var newHealth = upgraded.GetComponent<Health>();
            if (newHealth != null)
                newHealth.CurrentHealth = Mathf.RoundToInt(newHealth.MaxHealth * ratio);
        }
    }

    // Updates GameManager so future placements use the upgraded prefab
    private void ApplyFuturePrefabSwap(DefenderType type, int targetLevel)
    {
        if (gameManager == null) return;
        GameObject targetPrefab = GetTierPrefab(type, targetLevel);
        if (targetPrefab == null) return;

        switch (type)
        {
            case DefenderType.Basic:
                gameManager.defenderPrefab = targetPrefab;
                break;
            case DefenderType.FrostTower:
                gameManager.frostTowerPrefab = targetPrefab;
                break;
            case DefenderType.LightningTower:
                gameManager.lightningTowerPrefab = targetPrefab;
                break;
        }
    }

    // Returns the prefab for the given defender type and level (1 = tier 2, 2 = tier 3)
    private GameObject GetTierPrefab(DefenderType type, int level)
    {
        level = Mathf.Clamp(level, 0, 2);
        switch (type)
        {
            case DefenderType.Basic:
                return level == 1 ? basicTier2Prefab : (level == 2 ? basicTier3Prefab : null);
            case DefenderType.FrostTower:
                return level == 1 ? frostTier2Prefab : (level == 2 ? frostTier3Prefab : null);
            case DefenderType.LightningTower:
                return level == 1 ? lightningTier2Prefab : (level == 2 ? lightningTier3Prefab : null);
        }
        return null;
    }

    // Tries to detect the defender type from the GameObject's components
    private DefenderType GetDefenderType(GameObject go)
    {
        if (go.GetComponent<FrostTowerDefender>() != null) return DefenderType.FrostTower;
        if (go.GetComponent<LightningTowerDefender>() != null) return DefenderType.LightningTower;
        return DefenderType.Basic;
    }
}


