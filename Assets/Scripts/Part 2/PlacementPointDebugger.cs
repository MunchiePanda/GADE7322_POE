using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Debug script to check placement point setup and test placement
/// </summary>
public class PlacementPointDebugger : MonoBehaviour
{
    [Header("Debug Settings")]
    [Tooltip("Key to test placement point detection")]
    public Key testKey = Key.T;
    
    [Tooltip("Key to highlight all placement points")]
    public Key highlightKey = Key.H;
    
    [Tooltip("Key to clear highlights")]
    public Key clearKey = Key.C;
    
    private Camera cam;
    private Keyboard keyboard;
    
    void Start()
    {
        cam = Camera.main;
        if (cam == null)
        {
            cam = FindFirstObjectByType<Camera>();
        }
        
        keyboard = Keyboard.current;
    }
    
    void Update()
    {
        if (keyboard == null) return;
        
        if (keyboard[testKey].wasPressedThisFrame)
        {
            TestPlacementPointDetection();
        }
        
        if (keyboard[highlightKey].wasPressedThisFrame)
        {
            HighlightAllPlacementPoints();
        }
        
        if (keyboard[clearKey].wasPressedThisFrame)
        {
            ClearAllHighlights();
        }
    }
    
    /// <summary>
    /// Tests if placement points are being detected correctly
    /// </summary>
    void TestPlacementPointDetection()
    {
        Debug.Log("=== PLACEMENT POINT DETECTION TEST ===");
        
        // Find all placement points
        GameObject[] placementPoints = GameObject.FindGameObjectsWithTag("PlacementPoint");
        Debug.Log($"Found {placementPoints.Length} placement points with 'PlacementPoint' tag");
        
        foreach (GameObject point in placementPoints)
        {
            Debug.Log($"Placement Point: {point.name}");
            
            // Check if it has PlacementPointData component
            PlacementPointData pointData = point.GetComponent<PlacementPointData>();
            if (pointData != null)
            {
                Debug.Log($"  - Has PlacementPointData: {pointData.validGridPosition}");
                Debug.Log($"  - Is Available: {pointData.IsAvailable()}");
            }
            else
            {
                Debug.LogWarning($"  - MISSING PlacementPointData component!");
            }
            
            // Check if it has a collider
            Collider collider = point.GetComponent<Collider>();
            if (collider != null)
            {
                Debug.Log($"  - Has Collider: {collider.GetType().Name}");
            }
            else
            {
                Debug.LogWarning($"  - MISSING Collider component!");
            }
        }
        
        // Test raycast from center of screen
        if (cam != null)
        {
            Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log($"Raycast hit: {hit.collider.name}");
                if (hit.collider.CompareTag("PlacementPoint"))
                {
                    Debug.Log("  - Hit a PlacementPoint!");
                    PlacementPointData pointData = hit.collider.GetComponent<PlacementPointData>();
                    if (pointData != null)
                    {
                        Debug.Log($"  - Valid grid position: {pointData.validGridPosition}");
                    }
                }
                else
                {
                    Debug.Log($"  - Hit {hit.collider.name} (not a PlacementPoint)");
                }
            }
            else
            {
                Debug.Log("Raycast hit nothing");
            }
        }
    }
    
    /// <summary>
    /// Highlights all placement points
    /// </summary>
    void HighlightAllPlacementPoints()
    {
        GameObject[] placementPoints = GameObject.FindGameObjectsWithTag("PlacementPoint");
        
        foreach (GameObject point in placementPoints)
        {
            Renderer renderer = point.GetComponent<Renderer>();
            if (renderer != null)
            {
                // Create a bright material for highlighting
                Material highlightMaterial = new Material(Shader.Find("Standard"));
                highlightMaterial.color = Color.yellow;
                highlightMaterial.SetFloat("_Metallic", 0f);
                highlightMaterial.SetFloat("_Smoothness", 0f);
                renderer.material = highlightMaterial;
            }
        }
        
        Debug.Log($"Highlighted {placementPoints.Length} placement points");
    }
    
    /// <summary>
    /// Clears all highlights
    /// </summary>
    void ClearAllHighlights()
    {
        GameObject[] placementPoints = GameObject.FindGameObjectsWithTag("PlacementPoint");
        
        foreach (GameObject point in placementPoints)
        {
            Renderer renderer = point.GetComponent<Renderer>();
            if (renderer != null)
            {
                // Reset to default material
                Material defaultMaterial = new Material(Shader.Find("Standard"));
                defaultMaterial.color = Color.red;
                renderer.material = defaultMaterial;
            }
        }
        
        Debug.Log($"Cleared highlights from {placementPoints.Length} placement points");
    }
    
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 150));
        GUILayout.Label("Placement Point Debugger", GUI.skin.box);
        GUILayout.Label($"Press {testKey} to test detection");
        GUILayout.Label($"Press {highlightKey} to highlight points");
        GUILayout.Label($"Press {clearKey} to clear highlights");
        
        // Count placement points
        GameObject[] placementPoints = GameObject.FindGameObjectsWithTag("PlacementPoint");
        GUILayout.Label($"Placement Points: {placementPoints.Length}");
        
        GUILayout.EndArea();
    }
}
