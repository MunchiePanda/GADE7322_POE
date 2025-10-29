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
	/// <summary>Prefab used to replace all Basic defenders after upgrade.</summary>
	public GameObject basicTier2Prefab;
	/// <summary>Prefab used to replace all Frost defenders after upgrade.</summary>
	public GameObject frostTier2Prefab;
	/// <summary>Prefab used to replace all Lightning defenders after upgrade.</summary>
	public GameObject lightningTier2Prefab;

	[Header("Upgrade Settings")]
	/// <summary>Total resource cost for a per-type global upgrade.</summary>
	public int upgradeCost = 1000;

	// Flags that lock an upgrade per type so the player can't purchase it twice.
	private bool basicUpgraded;
	private bool frostUpgraded;
	private bool lightningUpgraded;

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
		switch (type)
		{
			case DefenderType.Basic: return basicUpgraded;
			case DefenderType.FrostTower: return frostUpgraded;
			case DefenderType.LightningTower: return lightningUpgraded;
		}
		return false;
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
		if (IsUpgraded(type)) return false;
		if (!gameManager.SpendResources(upgradeCost)) return false;

		SetUpgradedFlag(type, true);
		SwapExistingDefenders(type);
		ApplyFuturePrefabSwap(type);
		return true;
	}

	/// <summary>
	/// Internal helper to toggle the upgraded flag for a type.
	/// </summary>
	private void SetUpgradedFlag(DefenderType type, bool value)
	{
		switch (type)
		{
			case DefenderType.Basic: basicUpgraded = value; break;
			case DefenderType.FrostTower: frostUpgraded = value; break;
			case DefenderType.LightningTower: lightningUpgraded = value; break;
		}
	}

	/// <summary>
	/// Replaces all existing defenders of the specified type with the Tier 2 prefab.
	/// Preserves world transform and approximates current health via ratio of MaxHealth.
	/// </summary>
	private void SwapExistingDefenders(DefenderType type)
	{
		Defender[] allDefenders = FindObjectsByType<Defender>(FindObjectsSortMode.None);
		foreach (var defender in allDefenders)
		{
			if (defender == null) continue;

			var foundType = GetDefenderType(defender.gameObject);
			if (foundType != type) continue;

			GameObject tier2 = GetTier2Prefab(type);
			if (tier2 == null) continue;

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
			GameObject upgraded = Instantiate(tier2, pos, rot, parent);

			// Apply preserved health ratio to the new instance if it has a Health component.
			var newHealth = upgraded.GetComponent<Health>();
			if (newHealth != null)
			{
				newHealth.CurrentHealth = Mathf.RoundToInt(newHealth.MaxHealth * ratio);
			}
		}
	}

	/// <summary>
	/// Redirects future placements of the specified type to the Tier 2 prefab by
	/// updating GameManager's prefab references used by placement code.
	/// </summary>
	private void ApplyFuturePrefabSwap(DefenderType type)
	{
		if (gameManager == null) return;
		GameObject tier2 = GetTier2Prefab(type);
		if (tier2 == null) return;

		switch (type)
		{
			case DefenderType.Basic:
				gameManager.defenderPrefab = tier2;
				break;
			case DefenderType.FrostTower:
				gameManager.frostTowerPrefab = tier2;
				break;
			case DefenderType.LightningTower:
				gameManager.lightningTowerPrefab = tier2;
				break;
		}
	}

	/// <summary>
	/// Returns the assigned Tier 2 prefab for a defender type.
	/// </summary>
	private GameObject GetTier2Prefab(DefenderType type)
	{
		switch (type)
		{
			case DefenderType.Basic: return basicTier2Prefab;
			case DefenderType.FrostTower: return frostTier2Prefab;
			case DefenderType.LightningTower: return lightningTier2Prefab;
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


