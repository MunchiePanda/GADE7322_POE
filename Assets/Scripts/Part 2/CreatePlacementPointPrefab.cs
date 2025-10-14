using UnityEngine;

/// <summary>
/// Script to create a proper placement point prefab
/// Attach this to a GameObject and run it to create a placement point prefab
/// </summary>
public class CreatePlacementPointPrefab : MonoBehaviour
{
    [Header("Placement Point Settings")]
    [Tooltip("Size of the placement point")]
    public Vector3 size = new Vector3(1f, 0.1f, 1f);
    
    [Tooltip("Color of the placement point")]
    public Color color = Color.red;
    
    [Tooltip("Create the prefab when this is checked")]
    public bool createPrefab = false;
    
    void Update()
    {
        if (createPrefab)
        {
            createPrefab = false;
            CreatePlacementPoint();
        }
    }
    
    /// <summary>
    /// Creates a proper placement point prefab
    /// </summary>
    void CreatePlacementPoint()
    {
        // Create the placement point GameObject
        GameObject placementPoint = new GameObject("PlacementPoint");
        
        // Add a visual component (cube)
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.transform.SetParent(placementPoint.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = size;
        visual.name = "Visual";
        
        // Set the material color
        Renderer renderer = visual.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material material = new Material(Shader.Find("Standard"));
            material.color = color;
            material.SetFloat("_Metallic", 0f);
            material.SetFloat("_Smoothness", 0f);
            renderer.material = material;
        }
        
        // Remove the default collider from the visual
        Collider visualCollider = visual.GetComponent<Collider>();
        if (visualCollider != null)
        {
            DestroyImmediate(visualCollider);
        }
        
        // Add a trigger collider to the main object
        BoxCollider triggerCollider = placementPoint.AddComponent<BoxCollider>();
        triggerCollider.isTrigger = true;
        triggerCollider.size = size;
        
        // Set the tag
        placementPoint.tag = "PlacementPoint";
        
        // Add PlacementPointData component
        PlacementPointData pointData = placementPoint.AddComponent<PlacementPointData>();
        pointData.validGridPosition = Vector3Int.zero; // Will be set when instantiated
        
        // Position it at this object's position
        placementPoint.transform.position = transform.position;
        
        Debug.Log($"Created placement point at {transform.position}");
        Debug.Log($"Placement point has tag: {placementPoint.tag}");
        Debug.Log($"Placement point has PlacementPointData: {pointData != null}");
        Debug.Log($"Placement point has collider: {triggerCollider != null}");
    }
    
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 100));
        GUILayout.Label("Placement Point Creator", GUI.skin.box);
        GUILayout.Label("Check 'Create Prefab' to create a placement point");
        GUILayout.Label("This will create a proper placement point with:");
        GUILayout.Label("- PlacementPoint tag");
        GUILayout.Label("- PlacementPointData component");
        GUILayout.Label("- Trigger collider");
        GUILayout.EndArea();
    }
}
