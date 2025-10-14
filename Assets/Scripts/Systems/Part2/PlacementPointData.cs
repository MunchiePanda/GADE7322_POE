using UnityEngine;

/// <summary>
/// Component that stores the valid grid position for a placement point
/// </summary>
public class PlacementPointData : MonoBehaviour
{
    [Header("Placement Data")]
    [Tooltip("The valid grid position for defender placement at this point")]
    public Vector3Int validGridPosition;
    
    [Tooltip("Whether this placement point is currently occupied")]
    public bool isOccupied = false;
    
    /// <summary>
    /// Gets the valid grid position for placement
    /// </summary>
    public Vector3Int GetValidGridPosition()
    {
        return validGridPosition;
    }
    
    /// <summary>
    /// Marks this placement point as occupied
    /// </summary>
    public void MarkAsOccupied()
    {
        isOccupied = true;
    }
    
    /// <summary>
    /// Checks if this placement point is available for placement
    /// </summary>
    public bool IsAvailable()
    {
        return !isOccupied;
    }
}
