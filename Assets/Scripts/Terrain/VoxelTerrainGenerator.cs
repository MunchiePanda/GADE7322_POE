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
    [Tooltip("Maximum column height (Y) for the voxel grid.")]
    public int height = 6;

    [Header("Noise Settings (Uneven Terrain)")]
    [Tooltip("Enable Perlin-noise-based uneven terrain.")]
    public bool useNoise = true;
    [Tooltip("Minimum column height.")]
    public int minColumnHeight = 2;
    [Tooltip("Maximum additional height above minColumnHeight.")]
    public int noiseAmplitude = 4;
    [Tooltip("Perlin noise scale for x/z.")]
    public float noiseScale = 0.12f;
    [Tooltip("Random noise offset.")]
    public Vector2 noiseOffset = new Vector2(137.13f, 59.87f);

    [Header("Voxel Prefab")]
    [Tooltip("Prefab for a single voxel cube.")]
    public GameObject voxelPrefab;

    [Header("Path Settings")]
    [Tooltip("Number of unique enemy paths to carve.")]
    public int numPaths = 3;

    [Header("Placement Rules")]
    [Tooltip("Allow defenders to be placed on carved paths.")]
    public bool allowPlacementOnPaths = true;

    [Header("Mesh Combination & Layering")]
    [Tooltip("Combine individual voxel meshes into a single mesh after generation.")]
    public bool combineMeshes = true;
    [Tooltip("Layer index to assign to the combined terrain for camera culling.")]
    public int terrainLayer = 8; // Ensure this exists in Project Settings > Tags and Layers

    // -----------------------------
    // INTERNAL DATA STRUCTURES
    // -----------------------------

    // 3D array to store voxel references
    private GameObject[,,] voxels;
    // Column heights per x,z (for surface queries)
    private int[,] columnHeights;
    // List of path positions (for each path)
    private List<List<Vector3Int>> paths = new List<List<Vector3Int>>();
    // Center position (tower point)
    private Vector3Int center;
    // Currently highlighted path index (-1 means none)
    private int highlightedPathIndex = -1;
    // Original materials of path voxels for highlighting
    private Dictionary<Vector3Int, Material> originalMaterials = new Dictionary<Vector3Int, Material>();
    // Whether meshes were combined (disables per-voxel highlighting)
    private bool meshesCombined = false;
    // Generation state
    private bool isGenerated = false;
    public bool IsReady { get { return isGenerated; } }

    // -----------------------------
    // UNITY LIFECYCLE
    // -----------------------------

    void Awake()
    {
        GenerateAllIfNeeded();
    }

    void Start()
    {
        // Safeguard in case script execution order causes Start() to precede Awake()
        GenerateAllIfNeeded();
    }

    void GenerateAllIfNeeded()
    {
        if (isGenerated) return;
        // Calculate the center of the grid (where the tower will be placed)
        center = new Vector3Int(width / 2, 0, depth / 2);

        // Step 1: Generate the voxel grid
        GenerateVoxelGrid();

        // Step 2: Carve unique paths from edges to the center
        CarvePaths();

        // Step 3: Optionally combine voxel meshes into a single mesh with collider
        if (combineMeshes)
        {
            CombineVoxelMeshes();
        }
        isGenerated = true;
    }

    // -----------------------------
    // VOXEL GRID GENERATION
    // -----------------------------

    // Instantiates a 3D grid of voxel cubes with optional noise-based column heights
    void GenerateVoxelGrid()
    {
        voxels = new GameObject[width, height, depth];
        columnHeights = new int[width, depth];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < depth; z++)
                {
                    // Determine column height using noise only once per column
                    if (y == 0)
                    {
                        int colHeight = ComputeColumnHeight(x, z);
                        columnHeights[x, z] = Mathf.Clamp(colHeight, 1, height);
                    }
                    if (y < columnHeights[x, z])
                    {
                        GameObject voxel = Instantiate(voxelPrefab, this.transform);
                        voxel.transform.localPosition = new Vector3(x, y, z);
                        voxel.transform.localRotation = Quaternion.identity;
                        voxel.transform.localScale = Vector3.one;
                        voxels[x, y, z] = voxel;
                    }
                }
            }
        }
    }

    int ComputeColumnHeight(int x, int z)
    {
        if (!useNoise)
        {
            return Mathf.Clamp(minColumnHeight + noiseAmplitude, 1, height);
        }
        float nx = (x + noiseOffset.x) * noiseScale;
        float nz = (z + noiseOffset.y) * noiseScale;
        float n = Mathf.PerlinNoise(nx, nz); // 0..1
        int h = minColumnHeight + Mathf.RoundToInt(n * noiseAmplitude);
        return Mathf.Clamp(h, 1, height);
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
                // Remove voxels along the path (carve a walkable path)
                foreach (var pos in path)
                {
                    int colHeight = columnHeights != null ? columnHeights[pos.x, pos.z] : height;
                    for (int y = 0; y < colHeight; y++)
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

    // Returns true if a voxel is a valid defender placement
    public bool IsValidDefenderPlacement(Vector3Int pos)
    {
        bool isPath = IsPathTile(pos);
        if (allowPlacementOnPaths)
        {
            return true;
        }
        // If placement on paths is disabled, only allow non-path tiles
        return !isPath;
    }

    public bool IsPathTile(Vector3Int pos)
    {
        foreach (var path in paths)
        {
            if (path.Contains(new Vector3Int(pos.x, 0, pos.z)))
                return true;
        }
        return false;
    }

    // -----------------------------
    // PATH ACCESS & HIGHLIGHTING
    // -----------------------------

    // Returns the list of paths for external access
    public List<List<Vector3Int>> GetPaths()
    {
        return paths;
    }

    public Vector3Int GetCenterGrid()
    {
        GenerateAllIfNeeded();
        return center;
    }

    // Highlights a specific path by changing its color (only works if meshes are not combined)
    public void HighlightPath(int pathIndex)
    {
        if (meshesCombined)
        {
            Debug.LogWarning("Cannot highlight paths when meshes are combined.");
            return;
        }

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
        if (meshesCombined) return;
        var path = paths[pathIndex];
        Material highlightMaterial = new Material(Shader.Find("Standard"));
        highlightMaterial.color = Color.cyan; // You can change this color

        foreach (var pos in path)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int voxelPos = new Vector3Int(pos.x, y, pos.z);
                if (voxels[pos.x, y, pos.z] != null && !originalMaterials.ContainsKey(voxelPos))
                {
                    Renderer renderer = voxels[pos.x, y, pos.z].GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        originalMaterials[voxelPos] = renderer.material;
                        renderer.material = highlightMaterial;
                    }
                }
            }
        }
    }

    // Resets highlighting for a specific path
    private void ResetPathHighlighting(int pathIndex)
    {
        if (meshesCombined) return;
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
                float sy = GetSurfaceYSafe(pos.x, pos.z) + 0.5f;
                Gizmos.DrawCube(new Vector3(pos.x, sy, pos.z), Vector3.one * 0.5f);
            }
        }
        // Draw center (tower point)
        Gizmos.color = Color.yellow;
        float cy = GetSurfaceYSafe(center.x, center.z) + 0.5f;
        Gizmos.DrawCube(new Vector3(center.x, cy, center.z), Vector3.one * 0.7f);
    }

    // -----------------------------
    // SURFACE & COMBINE HELPERS
    // -----------------------------

    public int GetSurfaceY(int x, int z)
    {
        if (columnHeights == null) return height;
        if (x < 0 || x >= width || z < 0 || z >= depth)
        {
            return height;
        }
        return Mathf.Clamp(columnHeights[x, z], 1, height);
    }

    public Vector3 GetSurfaceWorldPosition(Vector3Int grid)
    {
        int y = GetSurfaceY(grid.x, grid.z);
        Vector3 local = new Vector3(grid.x, y, grid.z);
        return transform.TransformPoint(local);
    }

    public Vector3 GridToWorld(int x, int z)
    {
        int y = GetSurfaceY(x, z);
        return transform.TransformPoint(new Vector3(x, y, z));
    }

    private float GetSurfaceYSafe(int x, int z)
    {
        if (columnHeights == null) return 0.0f;
        return Mathf.Clamp(columnHeights[x, z], 1, height) - 0.5f;
    }



    void CombineVoxelMeshes()
    {
        List<MeshFilter> meshFilters = new List<MeshFilter>(GetComponentsInChildren<MeshFilter>());
        // Exclude the parent if it already has a MeshFilter
        meshFilters.RemoveAll(mf => mf.gameObject == this.gameObject);
        if (meshFilters.Count == 0) return;

        List<CombineInstance> combine = new List<CombineInstance>(meshFilters.Count);
        foreach (var mf in meshFilters)
        {
            if (mf.sharedMesh == null) continue;
            CombineInstance ci = new CombineInstance();
            ci.mesh = mf.sharedMesh;
            ci.transform = mf.transform.localToWorldMatrix;
            combine.Add(ci);
        }

        Mesh combinedMesh = new Mesh();
        combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // allow large meshes
        combinedMesh.CombineMeshes(combine.ToArray());

        // Assign to parent
        MeshFilter parentMF = GetComponent<MeshFilter>();
        if (parentMF == null) parentMF = gameObject.AddComponent<MeshFilter>();
        parentMF.sharedMesh = combinedMesh;

        MeshRenderer parentMR = GetComponent<MeshRenderer>();
        if (parentMR == null) parentMR = gameObject.AddComponent<MeshRenderer>();
        // Use voxel prefab's material if available
        var voxelRenderer = voxelPrefab != null ? voxelPrefab.GetComponent<MeshRenderer>() : null;
        if (voxelRenderer != null)
        {
            parentMR.sharedMaterial = voxelRenderer.sharedMaterial;
        }

        MeshCollider mc = GetComponent<MeshCollider>();
        if (mc == null) mc = gameObject.AddComponent<MeshCollider>();
        mc.sharedMesh = combinedMesh;

        // Assign layer for camera culling
        gameObject.layer = terrainLayer;

        // Clean up child voxels
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < depth; z++)
                {
                    if (voxels != null && voxels[x, y, z] != null)
                    {
                        Destroy(voxels[x, y, z]);
                        voxels[x, y, z] = null;
                    }
                }
            }
        }

        meshesCombined = true;
    }
}