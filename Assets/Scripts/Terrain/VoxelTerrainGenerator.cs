using UnityEngine;
using System.Collections.Generic;

// VoxelTerrainGenerator
// ---------------------
// This script generates a 3D grid of voxels (cubes) at runtime, carves at least 3 unique paths to a central tower point,
// and provides valid zones for defender placement. The terrain is different each game.
// Attach this script to an empty GameObject in your scene.

public class VoxelTerrainGenerator : MonoBehaviour
{
    // -----------------------------
    // CONFIGURABLE FIELDS
    // -----------------------------

    [Header("Terrain Dimensions")]
    [Tooltip("Width (X) of the voxel grid.")]
    public int width = 15;
    [Tooltip("Depth (Z) of the voxel grid.")]
    public int depth = 15;
    [Tooltip("Height (Y) of the voxel grid.")]
    public int height = 3;

    [Header("Voxel Prefab")]
    [Tooltip("Prefab for a single voxel cube.")]
    public GameObject voxelPrefab;

    [Header("Path Settings")]
    [Tooltip("Number of unique enemy paths to carve.")]
    public int numPaths = 3;

    // -----------------------------
    // INTERNAL DATA STRUCTURES
    // -----------------------------

    // 3D array to store voxel references
    private GameObject[,,] voxels;
    // List of path positions (for each path)
    private List<List<Vector3Int>> paths = new List<List<Vector3Int>>();
    // Center position (tower point)
    private Vector3Int center;
    // Currently highlighted path index (-1 means none)
    private int highlightedPathIndex = -1;
    // Original materials of path voxels for highlighting
    private Dictionary<Vector3Int, Material> originalMaterials = new Dictionary<Vector3Int, Material>();

    // -----------------------------
    // UNITY LIFECYCLE
    // -----------------------------

    void Start()
    {
        // Calculate the center of the grid (where the tower will be placed)
        center = new Vector3Int(width / 2, 0, depth / 2);

        // Step 1: Generate the voxel grid
        GenerateVoxelGrid();

        // Step 2: Carve unique paths from edges to the center
        CarvePaths();
    }

    // -----------------------------
    // VOXEL GRID GENERATION
    // -----------------------------

    // Instantiates a 3D grid of voxel cubes
    void GenerateVoxelGrid()
    {
        voxels = new GameObject[width, height, depth];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < depth; z++)
                {
                    Vector3 pos = new Vector3(x, y, z);
                    voxels[x, y, z] = Instantiate(voxelPrefab, pos, Quaternion.identity, this.transform);
                }
            }
        }
    }

    // -----------------------------
    // PATH CARVING LOGIC
    // -----------------------------

    // Carves at least 3 unique paths from random edge points to the center
    void CarvePaths()
    {
        HashSet<Vector3Int> usedEntrances = new HashSet<Vector3Int>();
        int attempts = 0;
        int maxAttempts = 20;
        while (paths.Count < numPaths && attempts < maxAttempts)
        {
            // Pick a random edge entrance
            Vector3Int entrance = GetRandomEdgePosition();
            if (usedEntrances.Contains(entrance))
            {
                attempts++;
                continue;
            }
            usedEntrances.Add(entrance);

            // Generate a path from entrance to center
            List<Vector3Int> path = GeneratePath(entrance, center);
            if (path.Count > 0)
            {
                paths.Add(path);
                // Remove voxels along the path (carve)
                foreach (var pos in path)
                {
                    for (int y = 0; y < height; y++)
                    {
                        if (voxels[pos.x, y, pos.z] != null)
                        {
                            Destroy(voxels[pos.x, y, pos.z]);
                            voxels[pos.x, y, pos.z] = null;
                        }
                    }
                }
            }
            attempts++;
        }
    }

    // Returns a random position on the edge of the grid (for path entrances)
    Vector3Int GetRandomEdgePosition()
    {
        int edge = Random.Range(0, 4);
        int x = 0, z = 0;
        switch (edge)
        {
            case 0: // Left edge
                x = 0;
                z = Random.Range(0, depth);
                break;
            case 1: // Right edge
                x = width - 1;
                z = Random.Range(0, depth);
                break;
            case 2: // Top edge
                x = Random.Range(0, width);
                z = 0;
                break;
            case 3: // Bottom edge
                x = Random.Range(0, width);
                z = depth - 1;
                break;
        }
        return new Vector3Int(x, 0, z);
    }

    // Generates a simple path (random walk with bias) from entrance to center
    List<Vector3Int> GeneratePath(Vector3Int start, Vector3Int end)
    {
        List<Vector3Int> path = new List<Vector3Int>();
        Vector3Int current = start;
        path.Add(current);
        int safety = 0;
        int maxSteps = width + depth;
        while (current != end && safety < maxSteps)
        {
            // Decide next step: bias towards the center
            int dx = end.x - current.x;
            int dz = end.z - current.z;
            List<Vector3Int> options = new List<Vector3Int>();
            if (dx != 0) options.Add(new Vector3Int(current.x + (int)Mathf.Sign(dx), 0, current.z));
            if (dz != 0) options.Add(new Vector3Int(current.x, 0, current.z + (int)Mathf.Sign(dz)));
            // Add some randomness
            if (Random.value < 0.3f && options.Count == 2)
                options.Reverse();
            Vector3Int next = options[Random.Range(0, options.Count)];
            current = next;
            if (!path.Contains(current))
                path.Add(current);
            safety++;
        }
        return path;
    }

    // -----------------------------
    // DEFENDER PLACEMENT ZONES
    // -----------------------------

    // Returns true if a voxel is a valid defender placement (not on a path)
    public bool IsValidDefenderPlacement(Vector3Int pos)
    {
        // Check if this position is part of any path
        foreach (var path in paths)
        {
            if (path.Contains(new Vector3Int(pos.x, 0, pos.z)))
                return false;
        }
        // Not a path, so valid
        return true;
    }

    // -----------------------------
    // PATH ACCESS & HIGHLIGHTING
    // -----------------------------

    // Returns the list of paths for external access
    public List<List<Vector3Int>> GetPaths()
    {
        return paths;
    }

    // Highlights a specific path by changing its color
    public void HighlightPath(int pathIndex)
    {
        // Reset previous highlighting
        if (highlightedPathIndex >= 0 && highlightedPathIndex < paths.Count)
        {
            ResetPathHighlighting(highlightedPathIndex);
        }

        // Apply new highlighting
        if (pathIndex >= 0 && pathIndex < paths.Count)
        {
            ApplyPathHighlighting(pathIndex);
            highlightedPathIndex = pathIndex;
        }
        else
        {
            highlightedPathIndex = -1;
        }
    }

    // Applies highlighting to a specific path
    private void ApplyPathHighlighting(int pathIndex)
    {
        var path = paths[pathIndex];
        foreach (var pos in path)
        {
            // Create a highlight material (bright color)
            Material highlightMaterial = new Material(Shader.Find("Standard"));
            highlightMaterial.color = Color.cyan; // You can change this color

            // Store original material and apply highlight
            for (int y = 0; y < height; y++)
            {
                Vector3Int voxelPos = new Vector3Int(pos.x, y, pos.z);
                if (voxels[pos.x, y, pos.z] != null)
                {
                    Renderer renderer = voxels[pos.x, y, pos.z].GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        if (!originalMaterials.ContainsKey(voxelPos))
                        {
                            originalMaterials[voxelPos] = renderer.material;
                        }
                        renderer.material = highlightMaterial;
                    }
                }
            }
        }
    }

    // Resets highlighting for a specific path
    private void ResetPathHighlighting(int pathIndex)
    {
        var path = paths[pathIndex];
        foreach (var pos in path)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int voxelPos = new Vector3Int(pos.x, y, pos.z);
                if (originalMaterials.ContainsKey(voxelPos))
                {
                    if (voxels[pos.x, y, pos.z] != null)
                    {
                        Renderer renderer = voxels[pos.x, y, pos.z].GetComponent<Renderer>();
                        if (renderer != null)
                        {
                            renderer.material = originalMaterials[voxelPos];
                        }
                    }
                    originalMaterials.Remove(voxelPos);
                }
            }
        }
    }

    // -----------------------------
    // GIZMOS FOR DEBUGGING
    // -----------------------------

    // Draws the carved paths in the Scene view for debugging
    void OnDrawGizmos()
    {
        if (paths == null) return;
        Gizmos.color = Color.red;
        foreach (var path in paths)
        {
            foreach (var pos in path)
            {
                Gizmos.DrawCube(new Vector3(pos.x, 0.5f, pos.z), Vector3.one * 0.5f);
            }
        }
        // Draw center (tower point)
        Gizmos.color = Color.yellow;
        Gizmos.DrawCube(new Vector3(center.x, 0.5f, center.z), Vector3.one * 0.7f);
    }
} 