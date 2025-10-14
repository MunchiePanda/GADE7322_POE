using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Test script to verify the placement point system is working correctly
/// </summary>
public class PlacementPointTestScript : MonoBehaviour
{
    [Header("Test Settings")]
    [Tooltip("Key to regenerate placement points")]
    public Key regenerateKey = Key.R;
    
    [Tooltip("Key to highlight all placement points")]
    public Key highlightKey = Key.H;
    
    [Tooltip("Key to clear highlights")]
    public Key clearKey = Key.C;
    
    private VoxelTerrainGenerator terrainGenerator;
    private Keyboard keyboard;
    
    void Start()
    {
        terrainGenerator = FindFirstObjectByType<VoxelTerrainGenerator>();
        keyboard = Keyboard.current;
        
        if (terrainGenerator == null)
        {
            Debug.LogError("PlacementPointTestScript: No VoxelTerrainGenerator found!");
        }
    }
    
    void Update()
    {
        if (terrainGenerator == null || keyboard == null) return;
        
        // Regenerate placement points
        if (keyboard[regenerateKey].wasPressedThisFrame)
        {
            Debug.Log("Regenerating placement points...");
            terrainGenerator.SpawnPlacementPrefabs();
        }
        
        // Highlight all placement points
        if (keyboard[highlightKey].wasPressedThisFrame)
        {
            Debug.Log("Highlighting all placement points...");
            HighlightAllPlacementPoints();
        }
        
        // Clear highlights
        if (keyboard[clearKey].wasPressedThisFrame)
        {
            Debug.Log("Clearing all highlights...");
            ClearAllHighlights();
        }
    }
    
    /// <summary>
    /// Highlights all placement points by changing their material
    /// </summary>
    void HighlightAllPlacementPoints()
    {
        GameObject[] placementPoints = GameObject.FindGameObjectsWithTag("PlacementPoint");
        
        foreach (GameObject point in placementPoints)
        {
            PlacementPointData pointData = point.GetComponent<PlacementPointData>();
            Renderer renderer = point.GetComponent<Renderer>();
            
            if (renderer != null)
            {
                // Create a material based on availability
                Material highlightMaterial = new Material(Shader.Find("Standard"));
                
                if (pointData != null && pointData.IsAvailable())
                {
                    highlightMaterial.color = new Color(0, 1f, 0, 0.8f); // Green for available
                }
                else
                {
                    highlightMaterial.color = new Color(1f, 0, 0, 0.8f); // Red for occupied
                }
                
                renderer.material = highlightMaterial;
            }
        }
        
        Debug.Log($"Highlighted {placementPoints.Length} placement points");
    }
    
    /// <summary>
    /// Clears all highlights by resetting materials
    /// </summary>
    void ClearAllHighlights()
    {
        GameObject[] placementPoints = GameObject.FindGameObjectsWithTag("PlacementPoint");
        
        foreach (GameObject point in placementPoints)
        {
            Renderer renderer = point.GetComponent<Renderer>();
            if (renderer != null)
            {
                // Reset to default material (you might want to store the original material)
                Material defaultMaterial = new Material(Shader.Find("Standard"));
                defaultMaterial.color = new Color(0, 1f, 0, 0.5f); // Green
                renderer.material = defaultMaterial;
            }
        }
        
        Debug.Log($"Cleared highlights from {placementPoints.Length} placement points");
    }
    
    void OnGUI()
    {
        if (terrainGenerator == null) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 150));
        GUILayout.Label("Placement Point Test", GUI.skin.box);
        GUILayout.Label($"Press {regenerateKey} to regenerate placement points");
        GUILayout.Label($"Press {highlightKey} to highlight all points");
        GUILayout.Label($"Press {clearKey} to clear highlights");
        
        // Count placement points
        GameObject[] placementPoints = GameObject.FindGameObjectsWithTag("PlacementPoint");
        int availablePoints = 0;
        int occupiedPoints = 0;
        
        foreach (GameObject point in placementPoints)
        {
            PlacementPointData pointData = point.GetComponent<PlacementPointData>();
            if (pointData != null)
            {
                if (pointData.IsAvailable())
                    availablePoints++;
                else
                    occupiedPoints++;
            }
        }
        
        GUILayout.Label($"Total Points: {placementPoints.Length}");
        GUILayout.Label($"Available: {availablePoints}");
        GUILayout.Label($"Occupied: {occupiedPoints}");
        
        GUILayout.EndArea();
    }
}
