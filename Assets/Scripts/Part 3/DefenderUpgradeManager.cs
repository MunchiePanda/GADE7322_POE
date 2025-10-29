using UnityEngine;
using GADE7322_POE.Core;

/// <summary>
/// Centralized manager that handles global, per-type upgrades for defenders.
/// - Deducts resources for the upgrade
/// - Swaps ALL existing defenders of a given type to a provided Tier 2 prefab (visual + stats)
/// - Redirects future placements of that defender type to the Tier 2 prefab via GameManager
///
/// Usage:
///   DefenderUpgradeManager.Instance.TryUpgrade(DefenderType.Basic);
/// Ensure the Tier 2 prefabs and GameManager are assigned at runtime.
/// </summary>
public class DefenderUpgradeManager : MonoBehaviour
{
	/// <summary>
	/// Singleton instance for easy access from UI/shop code.
	/// </summary>
	public static DefenderUpgradeManager Instance { get; private set; }

	[Header("Tier 2 Prefabs")]
	/// <summary>Prefab used to replace all Basic defenders after upgrade level 1 (Tier 2).</summary>
	public GameObject basicTier2Prefab;
	/// <summary>Prefab used to replace all Frost defenders after upgrade level 1 (Tier 2).</summary>
	public GameObject frostTier2Prefab;
	/// <summary>Prefab used to replace all Lightning defenders after upgrade level 1 (Tier 2).</summary>
	public GameObject lightningTier2Prefab;

	[Header("Tier 3 Prefabs")]
	/// <summary>Prefab used to replace all Basic defenders after upgrade level 2 (Tier 3).</summary>
	public GameObject basicTier3Prefab;
	/// <summary>Prefab used to replace all Frost defenders after upgrade level 2 (Tier 3).</summary>
	public GameObject frostTier3Prefab;
	/// <summary>Prefab used to replace all Lightning defenders after upgrade level 2 (Tier 3).</summary>
	public GameObject lightningTier3Prefab;

	[Header("Upgrade Settings")]
	/// <summary>Total resource cost for a per-type global upgrade.</summary>
	public int upgradeCost = 1000;

	// Per-type upgrade levels: 0=Tier1 (base), 1=Tier2, 2=Tier3 (max)
	private int basicLevel;
	private int frostLevel;
	private int lightningLevel;

	private GameManager gameManager;

	/// <summary>
	/// Enforces a single instance at runtime.
	/// </summary>
	void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
	}

	/// <summary>
	/// Caches GameManager reference (used for spending resources and swapping prefabs).
	/// </summary>
	void Start()
	{
		gameManager = FindFirstObjectByType<GameManager>();
	}

	/// <summary>
	/// Query upgrade state for a defender type.
	/// </summary>
	/// <param name="type">Defender type to check.</param>
	/// <returns>True if type has been upgraded.</returns>
	public bool IsUpgraded(DefenderType type)
	{
		return GetLevel(type) > 0;
	}

	/// <summary>
	/// Returns current upgrade level for a type (0..2).
	/// </summary>
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
	/// Attempts to perform a global upgrade for the given defender type.
	/// Deducts cost, swaps existing instances, and redirects future placements.
	/// </summary>
	/// <param name="type">Defender type to upgrade.</param>
	/// <returns>True if upgrade succeeded.</returns>
	public bool TryUpgrade(DefenderType type)
	{
		if (gameManager == null) return false;
		int currentLevel = GetLevel(type);
		if (currentLevel >= 2) return false; // already at max tier
		if (!gameManager.SpendResources(upgradeCost)) return false;

		// Increment level (0->1 or 1->2)
		SetLevel(type, currentLevel + 1);
		int targetLevel = GetLevel(type);
		SwapExistingDefenders(type, targetLevel);
		ApplyFuturePrefabSwap(type, targetLevel);
		return true;
	}

	/// <summary>
	/// Internal helper to toggle the upgraded flag for a type.
	/// </summary>
	private void SetLevel(DefenderType type, int value)
	{
		switch (type)
		{
			case DefenderType.Basic: basicLevel = Mathf.Clamp(value, 0, 2); break;
			case DefenderType.FrostTower: frostLevel = Mathf.Clamp(value, 0, 2); break;
			case DefenderType.LightningTower: lightningLevel = Mathf.Clamp(value, 0, 2); break;
		}
	}

	/// <summary>
	/// Replaces all existing defenders of the specified type with the target Tier prefab (level 1=Tier2, 2=Tier3).
	/// Preserves world transform and approximates current health via ratio of MaxHealth.
	/// </summary>
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

			// Preserve a rough health ratio so players don't lose progress on upgrade.
			float ratio = 1f;
			var oldHealth = defender.GetComponent<Health>();
			if (oldHealth != null && oldHealth.MaxHealth > 0.01f)
			{
				ratio = Mathf.Clamp01(oldHealth.CurrentHealth / oldHealth.MaxHealth);
			}

			Destroy(defender.gameObject);
			GameObject upgraded = Instantiate(targetPrefab, pos, rot, parent);

			// Apply preserved health ratio to the new instance if it has a Health component.
			var newHealth = upgraded.GetComponent<Health>();
			if (newHealth != null)
			{
				newHealth.CurrentHealth = Mathf.RoundToInt(newHealth.MaxHealth * ratio);
			}
		}
	}

	/// <summary>
	/// Redirects future placements of the specified type to the target Tier prefab by
	/// updating GameManager's prefab references used by placement code.
	/// </summary>
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

/// <summary>
/// Returns the assigned Tier prefab for a defender type and level.
/// Level: 0=Tier1 (original, not provided here), 1=Tier2, 2=Tier3.
/// </summary>
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

	/// <summary>
	/// Best-effort type detection for an existing defender instance.
	/// Adjust component checks if your subclass names differ.
	/// </summary>
	private DefenderType GetDefenderType(GameObject go)
	{
		if (go.GetComponent<FrostTowerDefender>() != null) return DefenderType.FrostTower;
		if (go.GetComponent<LightningTowerDefender>() != null) return DefenderType.LightningTower;
		return DefenderType.Basic;
	}
}


