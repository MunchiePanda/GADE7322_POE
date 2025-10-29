using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple UI bridge for three dedicated upgrade buttons (Basic/Frost/Lightning).
/// Attach this to a Canvas object and wire each public method to a Button OnClick.
/// </summary>
public class DefenderUpgradeButtons : MonoBehaviour
{
	[Header("Buttons")]
	/// <summary>Button that upgrades Basic defenders (Tier1->2->3).</summary>
	public Button basicUpgradeButton;
	/// <summary>Button that upgrades Frost defenders (Tier1->2->3).</summary>
	public Button frostUpgradeButton;
	/// <summary>Button that upgrades Lightning defenders (Tier1->2->3).</summary>
	public Button lightningUpgradeButton;

	// Cached refs for state checks
	private GameManager gameManager;
	private DefenderUpgradeManager upgradeManager;

	void Start()
	{
		upgradeManager = DefenderUpgradeManager.Instance;
		if (upgradeManager == null)
		{
			upgradeManager = FindFirstObjectByType<DefenderUpgradeManager>();
		}
		gameManager = FindFirstObjectByType<GameManager>();
	}

	void Update()
	{
		UpdateButtonStates();
	}

	/// <summary>
	/// Enables/disables each button based on resources, unlock state, and max tier.
	/// </summary>
	private void UpdateButtonStates()
	{
		if (gameManager == null || upgradeManager == null)
		{
			SetAll(false);
			return;
		}

		int resources = gameManager.GetResources();
		int cost = upgradeManager.upgradeCost;

		// Basic
		bool basicInteractable =
			resources >= cost &&
			gameManager.IsDefenderTypeUnlocked(DefenderType.Basic) &&
			upgradeManager.GetLevel(DefenderType.Basic) < 2;
		SetInteractable(basicUpgradeButton, basicInteractable);

		// Frost
		bool frostInteractable =
			resources >= cost &&
			gameManager.IsDefenderTypeUnlocked(DefenderType.FrostTower) &&
			upgradeManager.GetLevel(DefenderType.FrostTower) < 2;
		SetInteractable(frostUpgradeButton, frostInteractable);

		// Lightning
		bool lightningInteractable =
			resources >= cost &&
			gameManager.IsDefenderTypeUnlocked(DefenderType.LightningTower) &&
			upgradeManager.GetLevel(DefenderType.LightningTower) < 2;
		SetInteractable(lightningUpgradeButton, lightningInteractable);
	}

	private void SetAll(bool interactable)
	{
		SetInteractable(basicUpgradeButton, interactable);
		SetInteractable(frostUpgradeButton, interactable);
		SetInteractable(lightningUpgradeButton, interactable);
	}

	private void SetInteractable(Button btn, bool interactable)
	{
		if (btn != null) btn.interactable = interactable;
	}

	/// <summary>
	/// Called by the Basic upgrade button.
	/// </summary>
	public void OnUpgradeBasic()
	{
		if (DefenderUpgradeManager.Instance == null)
		{
			Debug.LogWarning("DefenderUpgradeManager instance not found in scene.");
			return;
		}
		bool ok = DefenderUpgradeManager.Instance.TryUpgrade(DefenderType.Basic);
		if (!ok) Debug.Log("Basic upgrade failed (insufficient resources or already upgraded).");
	}

	/// <summary>
	/// Called by the Frost upgrade button.
	/// </summary>
	public void OnUpgradeFrost()
	{
		if (DefenderUpgradeManager.Instance == null)
		{
			Debug.LogWarning("DefenderUpgradeManager instance not found in scene.");
			return;
		}
		bool ok = DefenderUpgradeManager.Instance.TryUpgrade(DefenderType.FrostTower);
		if (!ok) Debug.Log("Frost upgrade failed (insufficient resources or already upgraded).");
	}

	/// <summary>
	/// Called by the Lightning upgrade button.
	/// </summary>
	public void OnUpgradeLightning()
	{
		if (DefenderUpgradeManager.Instance == null)
		{
			Debug.LogWarning("DefenderUpgradeManager instance not found in scene.");
			return;
		}
		bool ok = DefenderUpgradeManager.Instance.TryUpgrade(DefenderType.LightningTower);
		if (!ok) Debug.Log("Lightning upgrade failed (insufficient resources or already upgraded).");
	}
}



