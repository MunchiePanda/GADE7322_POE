using UnityEngine;

public class DefenderPlacementMarker : MonoBehaviour
{
    public Vector3Int GridPosition { get; private set; }
    public bool IsOccupied { get; private set; }
    
    private StrategicDefenderPlacement placementManager;
    private Renderer markerRenderer;
    private Material defaultMaterial;
    private Material hoveredMaterial;
    
    public void Initialize(Vector3Int gridPos, StrategicDefenderPlacement manager, Material defaultMat, Material hoveredMat)
    {
        GridPosition = gridPos;
        placementManager = manager;
        defaultMaterial = defaultMat;
        hoveredMaterial = hoveredMat;
        
        markerRenderer = GetComponent<Renderer>();
        markerRenderer.material = defaultMaterial;
        
        if (GetComponent<Collider>() == null)
        {
            gameObject.AddComponent<CapsuleCollider>();
        }
    }
    
    public void OnHoverEnter()
    {
        if (!IsOccupied && markerRenderer != null)
        {
            markerRenderer.material = hoveredMaterial;
        }
    }
    
    public void OnHoverExit()
    {
        if (!IsOccupied && markerRenderer != null)
        {
            markerRenderer.material = defaultMaterial;
        }
    }
    
    public void SetOccupied(bool occupied, Material occupiedMaterial = null)
    {
        IsOccupied = occupied;
        
        if (occupied && occupiedMaterial != null && markerRenderer != null)
        {
            markerRenderer.material = occupiedMaterial;
        }
        else if (!occupied && markerRenderer != null)
        {
            markerRenderer.material = defaultMaterial;
        }
    }
}