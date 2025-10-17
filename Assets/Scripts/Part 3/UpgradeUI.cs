using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GADE7322_POE.Core;

/// <summary>
/// UI system for managing upgrade interface, showing costs, levels, and handling user input.
/// Provides both global and individual upgrade options.
/// </summary>
public class UpgradeUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Panel containing all upgrade UI elements")]
    public GameObject upgradePanel;
    
    [Tooltip("Button to toggle upgrade panel")]
    public Button toggleUpgradeButton;
    
    [Header("Global Upgrade UI")]
    [Tooltip("Button for global health upgrade")]
    public Button globalHealthButton;
    
    [Tooltip("Button for global damage upgrade")]
    public Button globalDamageButton;
    
    [Tooltip("Button for global attack speed upgrade")]
    public Button globalAttackSpeedButton;
    
    [Tooltip("Text showing global health level")]
    public TextMeshProUGUI globalHealthLevelText;
    
    [Tooltip("Text showing global damage level")]
    public TextMeshProUGUI globalDamageLevelText;
    
    [Tooltip("Text showing global attack speed level")]
    public TextMeshProUGUI globalAttackSpeedLevelText;
    
    [Header("Individual Upgrade UI")]
    [Tooltip("Button for individual health upgrade")]
    public Button individualHealthButton;
    
    [Tooltip("Button for individual damage upgrade")]
    public Button individualDamageButton;
    
    [Tooltip("Button for individual attack speed upgrade")]
    public Button individualAttackSpeedButton;
    
    [Tooltip("Text showing selected unit info")]
    public TextMeshProUGUI selectedUnitInfoText;
    
    [Header("Cost Display")]
    [Tooltip("Text showing global health cost")]
    public TextMeshProUGUI globalHealthCostText;
    
    [Tooltip("Text showing global damage cost")]
    public TextMeshProUGUI globalDamageCostText;
    
    [Tooltip("Text showing global attack speed cost")]
    public TextMeshProUGUI globalAttackSpeedCostText;
    
    [Tooltip("Text showing individual health cost")]
    public TextMeshProUGUI individualHealthCostText;
    
    [Tooltip("Text showing individual damage cost")]
    public TextMeshProUGUI individualDamageCostText;
    
    [Tooltip("Text showing individual attack speed cost")]
    public TextMeshProUGUI individualAttackSpeedCostText;
    
    [Header("References")]
    [Tooltip("Reference to upgrade system")]
    public UpgradeSystem upgradeSystem;
    
    [Tooltip("Reference to game manager for resources")]
    public GameManager gameManager;
    
    // Currently selected unit for individual upgrades
    private GameObject selectedUnit = null;
    private bool isUpgradePanelOpen = false;
    
    void Start()
    {
        if (upgradeSystem == null)
            upgradeSystem = FindFirstObjectByType<UpgradeSystem>();
            
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();
        
        SetupUI();
        UpdateUI();
        
        // Hide panel initially
        if (upgradePanel != null)
            upgradePanel.SetActive(false);
    }
    
    void Update()
    {
        // Handle unit selection with mouse clicks
        if (Input.GetMouseButtonDown(0))
        {
            HandleUnitSelection();
        }
        
        // Update UI periodically
        if (Time.frameCount % 30 == 0) // Update every 30 frames
        {
            UpdateUI();
        }
    }
    
    private void SetupUI()
    {
        // Setup button listeners
        if (toggleUpgradeButton != null)
            toggleUpgradeButton.onClick.AddListener(ToggleUpgradePanel);
        
        if (globalHealthButton != null)
            globalHealthButton.onClick.AddListener(() => TryGlobalUpgrade(UpgradeType.Health));
        
        if (globalDamageButton != null)
            globalDamageButton.onClick.AddListener(() => TryGlobalUpgrade(UpgradeType.Damage));
        
        if (globalAttackSpeedButton != null)
            globalAttackSpeedButton.onClick.AddListener(() => TryGlobalUpgrade(UpgradeType.AttackSpeed));
        
        if (individualHealthButton != null)
            individualHealthButton.onClick.AddListener(() => TryIndividualUpgrade(UpgradeType.Health));
        
        if (individualDamageButton != null)
            individualDamageButton.onClick.AddListener(() => TryIndividualUpgrade(UpgradeType.Damage));
        
        if (individualAttackSpeedButton != null)
            individualAttackSpeedButton.onClick.AddListener(() => TryIndividualUpgrade(UpgradeType.AttackSpeed));
    }
    
    private void HandleUnitSelection()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit))
        {
            GameObject hitObject = hit.collider.gameObject;
            
            // Check if clicked object is a defender or tower
            if (hitObject.GetComponent<Defender>() != null || hitObject.GetComponent<Tower>() != null)
            {
                SelectUnit(hitObject);
            }
        }
    }
    
    private void SelectUnit(GameObject unit)
    {
        selectedUnit = unit;
        UpdateSelectedUnitInfo();
    }
    
    private void UpdateSelectedUnitInfo()
    {
        if (selectedUnitInfoText == null || selectedUnit == null) return;
        
        string info = "Selected: ";
        
        Defender defender = selectedUnit.GetComponent<Defender>();
        Tower tower = selectedUnit.GetComponent<Tower>();
        
        if (defender != null)
        {
            // Get health from defender's hit points or health component
            Health healthComponent = defender.GetComponent<Health>();
            int health = healthComponent != null ? (int)healthComponent.CurrentHealth : 15;
            info += $"Defender (Health: {health})";
        }
        else if (tower != null)
        {
            // Use a simple approach - just show "Tower" without health for now
            info += "Tower";
        }
        
        selectedUnitInfoText.text = info;
    }
    
    private void ToggleUpgradePanel()
    {
        isUpgradePanelOpen = !isUpgradePanelOpen;
        
        if (upgradePanel != null)
            upgradePanel.SetActive(isUpgradePanelOpen);
    }
    
    private void TryGlobalUpgrade(UpgradeType upgradeType)
    {
        if (upgradeSystem == null) return;
        
        bool success = upgradeSystem.TryApplyGlobalUpgrade(upgradeType);
        
        if (success)
        {
            UpdateUI();
            // Show success feedback
            ShowUpgradeFeedback("Global upgrade applied!");
        }
        else
        {
            // Show failure feedback
            ShowUpgradeFeedback("Not enough resources!");
        }
    }
    
    private void TryIndividualUpgrade(UpgradeType upgradeType)
    {
        if (upgradeSystem == null || selectedUnit == null) return;
        
        bool success = false;
        
        Defender defender = selectedUnit.GetComponent<Defender>();
        Tower tower = selectedUnit.GetComponent<Tower>();
        
        if (defender != null)
        {
            success = upgradeSystem.TryApplyIndividualUpgrade(defender, upgradeType);
        }
        else if (tower != null)
        {
            success = upgradeSystem.TryApplyIndividualUpgrade(tower, upgradeType);
        }
        
        if (success)
        {
            UpdateUI();
            ShowUpgradeFeedback("Individual upgrade applied!");
        }
        else
        {
            ShowUpgradeFeedback("Not enough resources or no unit selected!");
        }
    }
    
    private void ShowUpgradeFeedback(string message)
    {
        // Simple feedback - could be enhanced with proper UI feedback
        Debug.Log($"Upgrade Feedback: {message}");
    }
    
    private void UpdateUI()
    {
        if (upgradeSystem == null) return;
        
        // Update global upgrade levels
        if (globalHealthLevelText != null)
            globalHealthLevelText.text = $"Level: {upgradeSystem.GetGlobalHealthLevel()}";
        
        if (globalDamageLevelText != null)
            globalDamageLevelText.text = $"Level: {upgradeSystem.GetGlobalDamageLevel()}";
        
        if (globalAttackSpeedLevelText != null)
            globalAttackSpeedLevelText.text = $"Level: {upgradeSystem.GetGlobalAttackSpeedLevel()}";
        
        // Update costs
        if (globalHealthCostText != null)
            globalHealthCostText.text = $"Cost: {upgradeSystem.GetGlobalHealthCost()}";
        
        if (globalDamageCostText != null)
            globalDamageCostText.text = $"Cost: {upgradeSystem.GetGlobalDamageCost()}";
        
        if (globalAttackSpeedCostText != null)
            globalAttackSpeedCostText.text = $"Cost: {upgradeSystem.GetGlobalAttackSpeedCost()}";
        
        if (individualHealthCostText != null)
            individualHealthCostText.text = $"Cost: {upgradeSystem.GetIndividualHealthCost()}";
        
        if (individualDamageCostText != null)
            individualDamageCostText.text = $"Cost: {upgradeSystem.GetIndividualDamageCost()}";
        
        if (individualAttackSpeedCostText != null)
            individualAttackSpeedCostText.text = $"Cost: {upgradeSystem.GetIndividualAttackSpeedCost()}";
        
        // Update button states based on resources
        UpdateButtonStates();
    }
    
    private void UpdateButtonStates()
    {
        if (gameManager == null) return;
        
        int currentResources = gameManager.GetResources();
        
        // Update global button states
        if (globalHealthButton != null)
            globalHealthButton.interactable = currentResources >= upgradeSystem.GetGlobalHealthCost();
        
        if (globalDamageButton != null)
            globalDamageButton.interactable = currentResources >= upgradeSystem.GetGlobalDamageCost();
        
        if (globalAttackSpeedButton != null)
            globalAttackSpeedButton.interactable = currentResources >= upgradeSystem.GetGlobalAttackSpeedCost();
        
        // Update individual button states
        bool hasSelectedUnit = selectedUnit != null;
        
        if (individualHealthButton != null)
            individualHealthButton.interactable = hasSelectedUnit && currentResources >= upgradeSystem.GetIndividualHealthCost();
        
        if (individualDamageButton != null)
            individualDamageButton.interactable = hasSelectedUnit && currentResources >= upgradeSystem.GetIndividualDamageCost();
        
        if (individualAttackSpeedButton != null)
            individualAttackSpeedButton.interactable = hasSelectedUnit && currentResources >= upgradeSystem.GetIndividualAttackSpeedCost();
    }
}
