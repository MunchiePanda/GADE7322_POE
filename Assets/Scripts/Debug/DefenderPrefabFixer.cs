using UnityEngine;

/// <summary>
/// Quick fix script to add missing components to defender prefabs.
/// Attach this to any GameObject and run it to fix your defender prefabs.
/// </summary>
public class DefenderPrefabFixer : MonoBehaviour
{
    [Header("Defender Prefabs to Fix")]
    [Tooltip("Drag your defender prefabs here")]
    public GameObject[] defenderPrefabs;
    
    [Header("Fix Options")]
    [Tooltip("Add renderer components to prefabs")]
    public bool addRenderers = true;
    
    [Tooltip("Create preview versions of prefabs")]
    public bool createPreviews = true;
    
    [ContextMenu("Fix Defender Prefabs")]
    public void FixDefenderPrefabs()
    {
        Debug.Log("=== Fixing Defender Prefabs ===");
        
        foreach (GameObject prefab in defenderPrefabs)
        {
            if (prefab == null) continue;
            
            Debug.Log($"Fixing prefab: {prefab.name}");
            
            // Check if it has a renderer
            Renderer renderer = prefab.GetComponent<Renderer>();
            if (renderer == null && addRenderers)
            {
                // Add a simple renderer
                MeshRenderer meshRenderer = prefab.AddComponent<MeshRenderer>();
                MeshFilter meshFilter = prefab.GetComponent<MeshFilter>();
                
                if (meshFilter == null)
                {
                    meshFilter = prefab.AddComponent<MeshFilter>();
                    meshFilter.mesh = CreateSimpleMesh();
                }
                
                // Add a simple material
                Material material = new Material(Shader.Find("Standard"));
                material.color = Color.white;
                meshRenderer.material = material;
                
                Debug.Log($"Added renderer to {prefab.name}");
            }
            
            // Create preview version
            if (createPreviews)
            {
                CreatePreviewPrefab(prefab);
            }
        }
        
        Debug.Log("=== Defender Prefab Fixing Complete ===");
    }
    
    void CreatePreviewPrefab(GameObject originalPrefab)
    {
        // Create a preview version
        GameObject preview = Instantiate(originalPrefab);
        preview.name = originalPrefab.name + "_Preview";
        
        // Make it semi-transparent
        Renderer[] renderers = preview.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            Material material = new Material(Shader.Find("Standard"));
            material.color = new Color(1, 1, 1, 0.5f); // Semi-transparent
            material.SetFloat("_Mode", 3); // Transparent mode
            renderer.material = material;
        }
        
        // Remove all scripts except Transform
        Component[] components = preview.GetComponents<Component>();
        foreach (Component component in components)
        {
            if (!(component is Transform) && !(component is Renderer) && !(component is MeshFilter))
            {
                DestroyImmediate(component);
            }
        }
        
        // Remove colliders
        Collider[] colliders = preview.GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
        {
            DestroyImmediate(collider);
        }
        
        Debug.Log($"Created preview prefab: {preview.name}");
        
        // Save as prefab (you'll need to do this manually in the editor)
        Debug.Log($"Please save {preview.name} as a prefab in your Assets/Prefabs folder");
        
        // Clean up the instance
        DestroyImmediate(preview);
    }
    
    Mesh CreateSimpleMesh()
    {
        // Create a simple cube mesh
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
