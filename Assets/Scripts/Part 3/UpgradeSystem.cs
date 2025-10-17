using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Central upgrade system managing both global and individual upgrades for defenders and tower.
/// Handles upgrade costs, effects, and visual changes.
/// </summary>
public class UpgradeSystem : MonoBehaviour
{
    [Header("Global Upgrade Costs")]
    [Tooltip("Cost for global health upgrade")]
    public int globalHealthUpgradeCost = 100;
    
    [Tooltip("Cost for global damage upgrade")]
    public int globalDamageUpgradeCost = 150;
    
    [Tooltip("Cost for global attack speed upgrade")]
    public int globalAttackSpeedUpgradeCost = 200;
    
    [Header("Individual Upgrade Costs")]
    [Tooltip("Cost for individual health upgrade")]
    public int individualHealthUpgradeCost = 50;
    
    [Tooltip("Cost for individual damage upgrade")]
    public int individualDamageUpgradeCost = 75;
    
    [Tooltip("Cost for individual attack speed upgrade")]
    public int individualAttackSpeedUpgradeCost = 100;
    
    [Header("Upgrade Effects")]
    [Tooltip("Health increase per upgrade")]
    public float healthUpgradeAmount = 20f;
    
    [Tooltip("Damage increase per upgrade")]
    public float damageUpgradeAmount = 5f;
    
    [Tooltip("Attack speed increase per upgrade (multiplier)")]
    public float attackSpeedUpgradeAmount = 0.2f;
    
    [Header("References")]
    [Tooltip("Reference to GameManager for resource management")]
    public GameManager gameManager;
    
    [Tooltip("Reference to UpgradeUI for interface")]
    public UpgradeUI upgradeUI;
    
    // Global upgrade levels
    private int globalHealthLevel = 0;
    private int globalDamageLevel = 0;
    private int globalAttackSpeedLevel = 0;
    
    // Maximum upgrade levels
    private const int MAX_UPGRADE_LEVEL = 5;
    
    // Events for UI updates
    public System.Action OnUpgradeApplied;
    
    void Start()
    {
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();
            
        if (upgradeUI == null)
            upgradeUI = FindFirstObjectByType<UpgradeUI>();
    }
    
    /// <summary>
    /// Attempts to apply a global upgrade to all defenders and tower
    /// </summary>
    /// <param name="upgradeType">Type of upgrade to apply</param>
    /// <returns>True if upgrade was successful, false otherwise</returns>
    public bool TryApplyGlobalUpgrade(UpgradeType upgradeType)
    {
        if (gameManager == null) return false;
        
        int cost = GetGlobalUpgradeCost(upgradeType);
        if (!gameManager.SpendResources(cost)) return false;
        
        // Apply upgrade to all defenders
        Defender[] allDefenders = FindObjectsByType<Defender>(FindObjectsSortMode.None);
        foreach (Defender defender in allDefenders)
        {
            if (defender != null)
            {
                ApplyUpgradeToDefender(defender, upgradeType, true);
            }
        }
        
        // Apply upgrade to tower
        Tower tower = FindFirstObjectByType<Tower>();
        if (tower != null)
        {
            ApplyUpgradeToTower(tower, upgradeType, true);
        }
        
        // Increment global level
        IncrementGlobalLevel(upgradeType);
        
        OnUpgradeApplied?.Invoke();
        return true;
    }
    
    /// <summary>
    /// Attempts to apply an individual upgrade to a specific defender
    /// </summary>
    /// <param name="defender">Defender to upgrade</param>
    /// <param name="upgradeType">Type of upgrade to apply</param>
    /// <returns>True if upgrade was successful, false otherwise</returns>
    public bool TryApplyIndividualUpgrade(Defender defender, UpgradeType upgradeType)
    {
        if (gameManager == null || defender == null) return false;
        
        int cost = GetIndividualUpgradeCost(upgradeType);
        if (!gameManager.SpendResources(cost)) return false;
        
        ApplyUpgradeToDefender(defender, upgradeType, false);
        
        OnUpgradeApplied?.Invoke();
        return true;
    }
    
    /// <summary>
    /// Attempts to apply an individual upgrade to the tower
    /// </summary>
    /// <param name="tower">Tower to upgrade</param>
    /// <param name="upgradeType">Type of upgrade to apply</param>
    /// <returns>True if upgrade was successful, false otherwise</returns>
    public bool TryApplyIndividualUpgrade(Tower tower, UpgradeType upgradeType)
    {
        if (gameManager == null || tower == null) return false;
        
        int cost = GetIndividualUpgradeCost(upgradeType);
        if (!gameManager.SpendResources(cost)) return false;
        
        ApplyUpgradeToTower(tower, upgradeType, false);
        
        OnUpgradeApplied?.Invoke();
        return true;
    }
    
    private void ApplyUpgradeToDefender(Defender defender, UpgradeType upgradeType, bool isGlobal)
    {
        switch (upgradeType)
        {
            case UpgradeType.Health:
                defender.UpgradeHealth();
                break;
            case UpgradeType.Damage:
                defender.UpgradeDamage();
                break;
            case UpgradeType.AttackSpeed:
                defender.UpgradeAttackSpeed();
                break;
        }
        
        // Apply visual upgrade effects
        ApplyVisualUpgradeEffects(defender.gameObject, upgradeType);
    }
    
    private void ApplyUpgradeToTower(Tower tower, UpgradeType upgradeType, bool isGlobal)
    {
        switch (upgradeType)
        {
            case UpgradeType.Health:
                tower.UpgradeHealth();
                break;
            case UpgradeType.Damage:
                tower.UpgradeDamage();
                break;
            case UpgradeType.AttackSpeed:
                tower.UpgradeAttackSpeed();
                break;
        }
        
        // Apply visual upgrade effects
        ApplyVisualUpgradeEffects(tower.gameObject, upgradeType);
    }
    
    private void ApplyVisualUpgradeEffects(GameObject target, UpgradeType upgradeType)
    {
        // Scale up the object slightly
        target.transform.localScale *= 1.05f;
        
        // Add upgrade glow effect
        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer != null)
        {
            // Create upgrade glow material
            Material upgradeMaterial = new Material(Shader.Find("Custom/UpgradeGlow"));
            upgradeMaterial.SetFloat("_UpgradeLevel", GetUpgradeLevel(target));
            upgradeMaterial.SetColor("_GlowColor", GetUpgradeColor(upgradeType));
            renderer.material = upgradeMaterial;
        }
        
        // Play upgrade particle effect
        PlayUpgradeParticles(target.transform.position);
    }
    
    private void PlayUpgradeParticles(Vector3 position)
    {
        // Create simple upgrade particle effect
        GameObject particleObj = new GameObject("UpgradeParticles");
        particleObj.transform.position = position;
        
        ParticleSystem particles = particleObj.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.startLifetime = 1f;
        main.startSpeed = 5f;
        main.startSize = 0.5f;
        main.startColor = Color.cyan;
        main.maxParticles = 20;
        
        var emission = particles.emission;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0.0f, 20)
        });
        
        // Destroy after effect
        Destroy(particleObj, 2f);
    }
    
    private int GetUpgradeLevel(GameObject target)
    {
        // Simple upgrade level calculation based on scale
        float scale = target.transform.localScale.x;
        return Mathf.RoundToInt((scale - 1f) / 0.05f);
    }
    
    private Color GetUpgradeColor(UpgradeType upgradeType)
    {
        switch (upgradeType)
        {
            case UpgradeType.Health:
                return Color.green;
            case UpgradeType.Damage:
                return Color.red;
            case UpgradeType.AttackSpeed:
                return Color.yellow;
            default:
                return Color.cyan;
        }
    }
    
    private int GetGlobalUpgradeCost(UpgradeType upgradeType)
    {
        switch (upgradeType)
        {
            case UpgradeType.Health:
                return globalHealthUpgradeCost;
            case UpgradeType.Damage:
                return globalDamageUpgradeCost;
            case UpgradeType.AttackSpeed:
                return globalAttackSpeedUpgradeCost;
            default:
                return 0;
        }
    }
    
    private int GetIndividualUpgradeCost(UpgradeType upgradeType)
    {
        switch (upgradeType)
        {
            case UpgradeType.Health:
                return individualHealthUpgradeCost;
            case UpgradeType.Damage:
                return individualDamageUpgradeCost;
            case UpgradeType.AttackSpeed:
                return individualAttackSpeedUpgradeCost;
            default:
                return 0;
        }
    }
    
    private void IncrementGlobalLevel(UpgradeType upgradeType)
    {
        switch (upgradeType)
        {
            case UpgradeType.Health:
                globalHealthLevel++;
                break;
            case UpgradeType.Damage:
                globalDamageLevel++;
                break;
            case UpgradeType.AttackSpeed:
                globalAttackSpeedLevel++;
                break;
        }
    }
    
    // Getters for UI
    public int GetGlobalHealthLevel() => globalHealthLevel;
    public int GetGlobalDamageLevel() => globalDamageLevel;
    public int GetGlobalAttackSpeedLevel() => globalAttackSpeedLevel;
    
    public int GetGlobalHealthCost() => globalHealthUpgradeCost;
    public int GetGlobalDamageCost() => globalDamageUpgradeCost;
    public int GetGlobalAttackSpeedCost() => globalAttackSpeedUpgradeCost;
    
    public int GetIndividualHealthCost() => individualHealthUpgradeCost;
    public int GetIndividualDamageCost() => individualDamageUpgradeCost;
    public int GetIndividualAttackSpeedCost() => individualAttackSpeedUpgradeCost;
}

public enum UpgradeType
{
    Health,
    Damage,
    AttackSpeed
}
