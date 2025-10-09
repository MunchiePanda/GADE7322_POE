using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the defender shop UI and drag-drop systems.
/// Handles button states, costs, and defender selection.
/// </summary>
public class DefenderShopManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Button for basic defender")]
    public Button basicDefenderButton;
    
    [Tooltip("Button for frost tower")]
    public Button frostTowerButton;
    
    [Tooltip("Button for lightning tower")]
    public Button lightningTowerButton;
    
    [Header("Cost Display Text")]
    [Tooltip("Text to display basic defender cost")]
    public TextMeshProUGUI basicDefenderCostText;
    
    [Tooltip("Text to display frost tower cost")]
    public TextMeshProUGUI frostTowerCostText;
    
    [Tooltip("Text to display lightning tower cost")]
    public TextMeshProUGUI lightningTowerCostText;
    
    [Header("Defender Costs")]
    [Tooltip("Cost for basic defender")]
    public int basicDefenderCost = 25;
    
    [Tooltip("Cost for frost tower")]
    public int frostTowerCost = 40;
    
    [Tooltip("Cost for lightning tower")]
    public int lightningTowerCost = 35;
    
    [Header("Drag-Drop Components")]
    [Tooltip("Drag-drop component for basic defender")]
    public DragDropDefenderSystem basicDefenderDragDrop;
    
    [Tooltip("Drag-drop component for frost tower")]
    public DragDropDefenderSystem frostTowerDragDrop;
    
    [Tooltip("Drag-drop component for lightning tower")]
    public DragDropDefenderSystem lightningTowerDragDrop;
    
    [Header("References")]
    [Tooltip("Reference to the game manager")]
    public GameManager gameManager;
    
    private void Start()
    {
        // Find game manager if not assigned
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();
        
        // Set up button events
        if (basicDefenderButton != null)
            basicDefenderButton.onClick.AddListener(() => SelectDefender(DefenderType.Basic));
        if (frostTowerButton != null)
            frostTowerButton.onClick.AddListener(() => SelectDefender(DefenderType.FrostTower));
        if (lightningTowerButton != null)
            lightningTowerButton.onClick.AddListener(() => SelectDefender(DefenderType.LightningTower));
        
        // Set cost displays
        if (basicDefenderCostText != null)
            basicDefenderCostText.text = basicDefenderCost.ToString();
        if (frostTowerCostText != null)
            frostTowerCostText.text = frostTowerCost.ToString();
        if (lightningTowerCostText != null)
            lightningTowerCostText.text = lightningTowerCost.ToString();
        
        // Configure drag-drop systems
        ConfigureDragDropSystems();
    }
    
    /// <summary>
    /// Configures all drag-drop systems with their respective settings
    /// </summary>
    void ConfigureDragDropSystems()
    {
        // Configure basic defender drag-drop
        if (basicDefenderDragDrop != null)
        {
            basicDefenderDragDrop.defenderType = DefenderType.Basic;
            basicDefenderDragDrop.cost = basicDefenderCost;
        }
        
        // Configure frost tower drag-drop
        if (frostTowerDragDrop != null)
        {
            frostTowerDragDrop.defenderType = DefenderType.FrostTower;
            frostTowerDragDrop.cost = frostTowerCost;
        }
        
        // Configure lightning tower drag-drop
        if (lightningTowerDragDrop != null)
        {
            lightningTowerDragDrop.defenderType = DefenderType.LightningTower;
            lightningTowerDragDrop.cost = lightningTowerCost;
        }
    }
    
    /// <summary>
    /// Called when a defender type is selected
    /// </summary>
    void SelectDefender(DefenderType type)
    {
        Debug.Log($"Selected {type} defender for drag-drop placement");
        // The drag-drop system will handle the actual placement
    }
    
    private void Update()
    {
        // Update button states based on available resources
        if (gameManager != null)
        {
            int resources = gameManager.GetResources();
            
            if (basicDefenderButton != null)
                basicDefenderButton.interactable = resources >= basicDefenderCost;
            if (frostTowerButton != null)
                frostTowerButton.interactable = resources >= frostTowerCost;
            if (lightningTowerButton != null)
                lightningTowerButton.interactable = resources >= lightningTowerCost;
        }
    }
}
