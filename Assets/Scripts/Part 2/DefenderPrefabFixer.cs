using UnityEngine;

/// <summary>
/// Utility script to automatically fix missing Renderer components on defender prefabs.
/// This helps resolve the "Missing Renderer" errors when spawning defenders.
/// </summary>
public class DefenderPrefabFixer : MonoBehaviour
{
    [Header("Auto-Fix Settings")]
    [Tooltip("Automatically add missing Renderer components on Start")]
    public bool autoFixOnStart = true;
    
    [Tooltip("Add MeshFilter component if missing")]
    public bool addMeshFilter = true;
    
    [Tooltip("Default mesh to use if no mesh exists")]
    public Mesh defaultMesh;
    
    void Start()
    {
        if (autoFixOnStart)
        {
            FixMissingComponents();
        }
    }
    
    /// <summary>
    /// Fixes missing Renderer and MeshFilter components
    /// </summary>
    [ContextMenu("Fix Missing Components")]
    public void FixMissingComponents()
    {
        Debug.Log($"Fixing components on {gameObject.name}...");
        
        // Check for Renderer component
        Renderer renderer = GetComponent<Renderer>();
        if (renderer == null)
        {
            Debug.Log($"Adding MeshRenderer to {gameObject.name}");
            renderer = gameObject.AddComponent<MeshRenderer>();
            
            // Create a default material
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = GetDefaultColor();
            renderer.material = mat;
        }
        
        // Check for MeshFilter component
        if (addMeshFilter)
        {
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                Debug.Log($"Adding MeshFilter to {gameObject.name}");
                meshFilter = gameObject.AddComponent<MeshFilter>();
                
                // Use default mesh if provided
                if (defaultMesh != null)
                {
                    meshFilter.mesh = defaultMesh;
                }
                else
                {
                    // Create a simple cube mesh
                    meshFilter.mesh = CreateDefaultCubeMesh();
                }
            }
        }
        
        Debug.Log($"Component fix complete for {gameObject.name}");
    }
    
    /// <summary>
    /// Gets the default color based on the defender type
    /// </summary>
    Color GetDefaultColor()
    {
        string name = gameObject.name.ToLower();
        
        if (name.Contains("frost"))
            return Color.cyan;
        else if (name.Contains("lightning"))
            return Color.yellow;
        else if (name.Contains("kamikaze"))
            return Color.red;
        else if (name.Contains("armored"))
            return Color.gray;
        else
            return Color.white;
    }
    
    /// <summary>
    /// Creates a simple cube mesh as default
    /// </summary>
    Mesh CreateDefaultCubeMesh()
    {
        Mesh mesh = new Mesh();
        
        Vector3[] vertices = new Vector3[]
        {
            new Vector3(-0.5f, -0.5f, -0.5f),
            new Vector3(0.5f, -0.5f, -0.5f),
            new Vector3(0.5f, 0.5f, -0.5f),
            new Vector3(-0.5f, 0.5f, -0.5f),
            new Vector3(-0.5f, -0.5f, 0.5f),
            new Vector3(0.5f, -0.5f, 0.5f),
            new Vector3(0.5f, 0.5f, 0.5f),
            new Vector3(-0.5f, 0.5f, 0.5f)
        };
        
        int[] triangles = new int[]
        {
            0, 2, 1, 0, 3, 2,
            2, 3, 4, 2, 4, 5,
            1, 2, 5, 5, 2, 6,
            0, 7, 4, 0, 4, 3,
            5, 6, 7, 5, 7, 4,
            0, 1, 5, 0, 5, 4
        };
        
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        
        return mesh;
    }
}
