using UnityEngine;

/// <summary>
/// Simple UI bridge for three dedicated upgrade buttons (Basic/Frost/Lightning).
/// Attach this to a Canvas object and wire each public method to a Button OnClick.
/// </summary>
public class DefenderUpgradeButtons : MonoBehaviour
{
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


