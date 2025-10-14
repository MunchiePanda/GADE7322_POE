using UnityEngine;

/// <summary>
/// Handles individual defender placement areas - visual indicators where defenders can be placed
/// </summary>
public class DefenderPlacementArea : MonoBehaviour
{
    [Header("Placement Settings")]
    [Tooltip("Type of defender that can be placed here")]
    public DefenderPlacementType placementType = DefenderPlacementType.Basic;
    
    [Tooltip("Whether this placement area is currently occupied")]
    public bool isOccupied = false;
    
    [Tooltip("Grid position of this placement area")]
    public Vector3Int gridPosition;
    
    [Tooltip("Lane index this placement area belongs to")]
    public int laneIndex = -1;
    
    [Header("Visual Feedback")]
    [Tooltip("Material for this placement area")]
    public Material placementMaterial;
    
    private Renderer areaRenderer;
    private VoxelTerrainGenerator terrainGenerator;
    
    void Start()
    {
        areaRenderer = GetComponent<Renderer>();
        terrainGenerator = FindFirstObjectByType<VoxelTerrainGenerator>();
        
        // Ensure we have a collider for raycast detection
        Collider collider = GetComponent<Collider>();
        if (collider == null)
        {
            // Add a box collider if none exists
            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.isTrigger = true; // Make it a trigger for easier detection
            Debug.Log($"Added BoxCollider to placement area {gameObject.name}");
        }
        
        // Set initial visual state
        UpdateVisualState();
    }
    
    /// <summary>
    /// Updates the visual appearance based on current state
    /// </summary>
    public void UpdateVisualState()
    {
        if (areaRenderer == null) return;
        
        // Simple visual state - just use the placement material
        if (placementMaterial != null)
        {
            areaRenderer.material = placementMaterial;
        }
    }
    
    /// <summary>
    /// Checks if a specific defender type can be placed here
    /// </summary>
    public bool CanPlaceDefender(DefenderType defenderType)
    {
        if (isOccupied) return false;
        
        switch (defenderType)
        {
            case DefenderType.Basic:
                return true; // Basic defenders can be placed anywhere
                
            case DefenderType.FrostTower:
            case DefenderType.LightningTower:
                return placementType == DefenderPlacementType.Advanced;
                
            default:
                return false;
        }
    }
    
    /// <summary>
    /// Marks this placement area as occupied
    /// </summary>
    public void MarkOccupied()
    {
        isOccupied = true;
        UpdateVisualState();
        
        // Notify terrain generator
        if (terrainGenerator != null)
        {
            terrainGenerator.MarkDefenderSpotOccupied(gridPosition);
        }
    }
    
    /// <summary>
    /// Marks this placement area as available
    /// </summary>
    public void MarkAvailable()
    {
        isOccupied = false;
        UpdateVisualState();
        
        // Notify terrain generator
        if (terrainGenerator != null)
        {
            terrainGenerator.MarkDefenderSpotUnoccupied(gridPosition);
        }
    }
    
    /// <summary>
    /// Sets the placement type and updates visuals accordingly
    /// </summary>
    public void SetPlacementType(DefenderPlacementType type)
    {
        placementType = type;
        UpdateVisualState();
    }
    
    /// <summary>
    /// Gets the world position for placing defenders
    /// </summary>
    public Vector3 GetDefenderSpawnPosition()
    {
        return transform.position + Vector3.up * 0.1f; // Slightly above the plane
    }
    
    /// <summary>
    /// Highlights this area for a specific defender type
    /// </summary>
    public void HighlightForDefender(DefenderType defenderType)
    {
        // Simple highlighting - just use normal material
        UpdateVisualState();
    }
    
    /// <summary>
    /// Removes highlight and returns to normal state
    /// </summary>
    public void RemoveHighlight()
    {
        UpdateVisualState();
    }
}
