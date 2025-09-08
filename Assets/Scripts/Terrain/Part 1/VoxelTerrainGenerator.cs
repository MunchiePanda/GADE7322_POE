using UnityEngine;
using System.Collections.Generic;

// Generates a voxel-based terrain with noise, biomes, paths, defender locations, and tree spawning.
public class VoxelTerrainGenerator : MonoBehaviour
{
    [Header("Terrain Dimensions")]
    // Width, depth, and height of the terrain grid in voxels.
    public int width = 150;
    public int depth = 150;
    public int height = 8;

    [Header("Chunk Settings")]
    // Size of each chunk for dividing the terrain into manageable sections.
    public int chunkSize = 16;

    [Header("Noise Settings")]
    // Toggle for using Perlin noise to generate terrain height.
    public bool useNoise = true;
    // Minimum height for terrain columns.
    public int minColumnHeight = 4;
    // Maximum variation in height added by noise.
    public int noiseAmplitude = 4;
    // Scale of the Perlin noise, controlling terrain smoothness.
    public float noiseScale = 0.03f;
    // Offset for noise to ensure unique terrain generation.
    public Vector2 noiseOffset = new Vector2(137.13f, 59.87f);
    // Number of noise layers for more detailed terrain (fractal Brownian motion).
    public int octaves = 4;
    // Controls amplitude reduction per octave.
    public float persistence = 0.5f;
    // Controls frequency increase per octave.
    public float lacunarity = 2f;

    [Header("Biome Settings")]
    // Toggle for biome-based texturing (grass or sand).
    public bool useBiomes = true;
    // Scale of biome noise, determining biome distribution.
    public float biomeScale = 0.02f;
    // Offset for biome noise to ensure unique biome patterns.
    public Vector2 biomeOffset = new Vector2(42f, 73f);

    [Header("Voxel Prefab")]
    // Prefab used for individual voxels in the terrain.
    public GameObject voxelPrefab;

    [Header("Atlas Settings")]
    // Texture used as a heightmap for terrain elevation (optional).
    public Texture2D atlasTexture;
    // Scales the grayscale heightmap values to terrain height.
    public float atlasElevationScale = 10.0f;

    [Header("Highlight Materials")]
    // Material for highlighting paths.
    public Material pathHighlightMaterial;
    // Material for highlighting defender placement locations.
    public Material defenderHighlightMaterial;

    [Header("Tree Spawning")]
    // Prefab for trees to spawn on grass tiles.
    public GameObject treePrefab;
    // Percentage of grass tiles that will spawn trees (0 to 1).
    [Range(0, 1)] public float treeDensity = 0.05f;

    [Header("Path Settings")]
    // Number of paths to carve from edges to the terrain center.
    public int numPaths = 3;
    // Width of paths in voxels.
    public int pathWidth = 2;

    [Header("Layer Settings")]
    // Unity layer for terrain objects.
    public int terrainLayer = 8;

    // 2D array storing the height of each terrain column.
    private int[,] columnHeights;
    // List of paths, each path being a list of 3D grid positions.
    private List<List<Vector3Int>> paths = new List<List<Vector3Int>>();
    // Center point of the terrain grid.
    private Vector3Int center;
    // Set of 2D positions occupied by paths to avoid defender placement.
    private HashSet<Vector2Int> pathPositions = new HashSet<Vector2Int>();
    // Flag indicating if terrain generation is complete.
    private bool isGenerated = false;
    // Public property to check if generation is complete.
    public bool IsReady => isGenerated;
    // List of valid defender placement positions.
    private List<Vector3Int> defenderLocations = new List<Vector3Int>();
    // Public property to access defender locations.
    public List<Vector3Int> DefenderLocations => defenderLocations;

    // Ensures terrain is generated when the script is initialized.
    void Awake() => GenerateAllIfNeeded();

    // Ensures terrain is generated when the scene starts, if not already done.
    void Start() => GenerateAllIfNeeded();

    // Generates the entire terrain if it hasn't been generated yet.
    void GenerateAllIfNeeded()
    {
        if (isGenerated) return;

        // Set the center of the terrain grid.
        center = new Vector3Int(width / 2, 0, depth / 2);
        // Initialize the heightmap array.
        columnHeights = new int[width, depth];

        // Generate heights for each column.
        for (int x = 0; x < width; x++)
            for (int z = 0; z < depth; z++)
                columnHeights[x, z] = ComputeColumnHeight(x, z);

        // Carve paths from edges to the center.
        CarvePaths();
        // Generate valid defender placement locations along paths.
        GenerateDefenderLocations();
        // Create terrain chunks for efficient rendering.
        CreateChunks();
        // Spawn trees on grass tiles.
        SpawnTrees();

        // Mark generation as complete.
        isGenerated = true;
    }

    // Generates valid defender placement locations near paths.
    private void GenerateDefenderLocations()
    {
        defenderLocations.Clear();
        foreach (var path in paths)
        {
            foreach (var pos in path)
            {
                int surfaceY = GetSurfaceY(pos.x, pos.z);
                // Check a 5x5 grid around each path position for valid defender spots.
                for (int x = -2; x <= 2; x++)
                {
                    for (int z = -2; z <= 2; z++)
                    {
                        Vector3Int testPosition = new Vector3Int(pos.x + x, surfaceY - 1, pos.z + z);
                        // Clamp position to terrain bounds.
                        testPosition.x = Mathf.Clamp(testPosition.x, 0, width - 1);
                        testPosition.z = Mathf.Clamp(testPosition.z, 0, depth - 1);
                        // Add position if valid and not already included.
                        if (IsValidDefenderPlacement(testPosition) && !defenderLocations.Contains(testPosition))
                        {
                            defenderLocations.Add(testPosition);
                        }
                    }
                }
            }
        }
    }

    // Spawns trees randomly on grass tiles based on biome and density settings.
    void SpawnTrees()
    {
        if (treePrefab == null || !useBiomes)
            return;

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                // Determine biome using Perlin noise.
                float nx = (x + biomeOffset.x) * biomeScale;
                float nz = (z + biomeOffset.y) * biomeScale;
                float biomeValue = Mathf.PerlinNoise(nx, nz);
                TextureType topType = (biomeValue < 0.5f) ? TextureType.Grass : TextureType.Sand;

                // Spawn tree on grass tiles with probability based on treeDensity.
                if (topType == TextureType.Grass && columnHeights[x, z] > 0)
                {
                    if (Random.value < treeDensity)
                    {
                        Vector3 spawnPosition = new Vector3(x, columnHeights[x, z], z);
                        GameObject tree = Instantiate(treePrefab, spawnPosition, Quaternion.identity);
                        tree.transform.SetParent(transform);
                    }
                }
            }
        }
    }

    // Divides the terrain into chunks and generates them.
    private void CreateChunks()
    {
        // Calculate number of chunks in each dimension.
        int chunksX = Mathf.CeilToInt((float)width / chunkSize);
        int chunksZ = Mathf.CeilToInt((float)depth / chunkSize);

        // Create each chunk.
        for (int cx = 0; cx < chunksX; cx++)
        {
            int chunkStartX = cx * chunkSize;
            int chunkWidth = Mathf.Min(chunkSize, width - chunkStartX);
            for (int cz = 0; cz < chunksZ; cz++)
            {
                int chunkStartZ = cz * chunkSize;
                int chunkDepth = Mathf.Min(chunkSize, depth - chunkStartZ);

                // Create a new GameObject for the chunk.
                GameObject chunkGO = new GameObject($"Chunk_{cx}_{cz}");
                chunkGO.transform.SetParent(transform);
                chunkGO.transform.localPosition = new Vector3(chunkStartX, 0, chunkStartZ);

                // Add and initialize VoxelChunk component.
                VoxelChunk chunk = chunkGO.AddComponent<VoxelChunk>();
                chunk.Initialize(chunkWidth, height, chunkDepth, chunkStartX, chunkStartZ,
                               voxelPrefab, terrainLayer, width, depth, columnHeights);
                chunk.Generate(useNoise, minColumnHeight, noiseAmplitude, noiseScale, noiseOffset,
                             useBiomes, biomeScale, biomeOffset);
            }
        }
    }

    // Computes the height of a terrain column at position (x, z).
    int ComputeColumnHeight(int x, int z)
    {
        // Use heightmap texture if provided.
        if (atlasTexture != null)
        {
            float atlasX = (float)x / width;
            float atlasZ = (float)z / depth;
            Color atlasColor = atlasTexture.GetPixelBilinear(atlasX, atlasZ);
            float atlasElevation = atlasColor.grayscale * atlasElevationScale;
            return Mathf.Clamp(Mathf.RoundToInt(atlasElevation), 1, height);
        }
        // Use flat terrain if noise is disabled.
        else if (!useNoise)
        {
            return Mathf.Clamp(minColumnHeight + noiseAmplitude, 1, height);
        }
        // Use Perlin noise with multiple octaves for varied terrain.
        else
        {
            float totalNoise = 0f;
            float frequency = 1f;
            float amplitude = 1f;
            float maxValue = 0f;

            // Sum noise from multiple octaves.
            for (int o = 0; o < octaves; o++)
            {
                float nx = (x + noiseOffset.x) * noiseScale * frequency;
                float nz = (z + noiseOffset.y) * noiseScale * frequency;
                totalNoise += Mathf.PerlinNoise(nx, nz) * amplitude;
                maxValue += amplitude;
                amplitude *= persistence;
                frequency *= lacunarity;
            }

            // Normalize noise and compute final height.
            float noiseValue = totalNoise / maxValue;
            int h = minColumnHeight + Mathf.RoundToInt(noiseValue * noiseAmplitude);
            return Mathf.Clamp(h, minColumnHeight, height);
        }
    }

    // Generates paths from random edge points to the terrain center.
    void CarvePaths()
    {
        HashSet<Vector3Int> usedEntrances = new HashSet<Vector3Int>();
        int attempts = 0;
        int maxAttempts = 20;

        // Try to generate the specified number of paths.
        while (paths.Count < numPaths && attempts < maxAttempts)
        {
            Vector3Int entrance = GetRandomEdgePosition();
            // Skip if entrance is already used.
            if (usedEntrances.Contains(entrance))
            {
                attempts++;
                continue;
            }
            usedEntrances.Add(entrance);

            // Generate a path from entrance to center.
            List<Vector3Int> path = GeneratePath(entrance, center);
            // Accept path if it's long enough and has minimal overlap with existing paths.
            if (path.Count > 10 && !PathsIntersect(path))
            {
                paths.Add(path);
                // Mark path positions with specified width.
                foreach (var pos in path)
                {
                    for (int dx = -pathWidth / 2; dx <= pathWidth / 2; dx++)
                    {
                        for (int dz = -pathWidth / 2; dz <= pathWidth / 2; dz++)
                        {
                            int px = pos.x + dx;
                            int pz = pos.z + dz;
                            if (px >= 0 && px < width && pz >= 0 && pz < depth)
                            {
                                pathPositions.Add(new Vector2Int(px, pz));
                            }
                        }
                    }
                }
            }
            attempts++;
        }
    }

    // Checks if a new path intersects too much with existing paths.
    bool PathsIntersect(List<Vector3Int> newPath)
    {
        int overlapCount = 0;
        foreach (var path in paths)
        {
            foreach (var pos in path)
            {
                if (newPath.Contains(pos))
                {
                    overlapCount++;
                    // Allow up to 2 overlapping positions to permit some path convergence.
                    if (overlapCount > 2)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    // Returns a random position on the terrain's edge.
    Vector3Int GetRandomEdgePosition()
    {
        int edge = Random.Range(0, 4);
        int x = 0, z = 0;
        switch (edge)
        {
            case 0: x = 0; z = Random.Range(0, depth); break; // Left edge
            case 1: x = width - 1; z = Random.Range(0, depth); break; // Right edge
            case 2: x = Random.Range(0, width); z = 0; break; // Bottom edge
            case 3: x = Random.Range(0, width); z = depth - 1; break; // Top edge
        }
        return new Vector3Int(x, 0, z);
    }

    // Generates a path from start to end using a simple manhattan-like algorithm.
    List<Vector3Int> GeneratePath(Vector3Int start, Vector3Int end)
    {
        List<Vector3Int> path = new List<Vector3Int> { start };
        Vector3Int current = start;
        int safety = 0;
        int maxSteps = width + depth;

        // Move toward the end position one step at a time.
        while (current != end && safety < maxSteps)
        {
            int dx = end.x - current.x;
            int dz = end.z - current.z;
            List<Vector3Int> options = new List<Vector3Int>();

            // Add possible next steps (horizontal or vertical).
            if (dx != 0) options.Add(new Vector3Int(current.x + (int)Mathf.Sign(dx), 0, current.z));
            if (dz != 0) options.Add(new Vector3Int(current.x, 0, current.z + (int)Mathf.Sign(dz)));

            // Prefer steps with smaller elevation changes.
            if (options.Count > 1)
            {
                int currentHeight = columnHeights[current.x, current.z];
                int option1Height = columnHeights[options[0].x, options[0].z];
                int option2Height = columnHeights[options[1].x, options[1].z];
                int delta1 = Mathf.Abs(option1Height - currentHeight);
                int delta2 = Mathf.Abs(option2Height - currentHeight);
                if (delta1 < delta2)
                {
                    options.Reverse();
                }
            }

            // Choose a random next step from available options.
            Vector3Int next = options[Random.Range(0, options.Count)];
            current = next;
            if (!path.Contains(current)) path.Add(current);
            safety++;
        }
        return path;
    }

    // Checks if a position is valid for defender placement.
    public bool IsValidDefenderPlacement(Vector3Int pos)
    {
        if (!isGenerated) return false;
        // Ensure position is within terrain bounds.
        if (pos.x < 0 || pos.x >= width || pos.z < 0 || pos.z >= depth) return false;
        int colHeight = columnHeights[pos.x, pos.z];
        // Ensure column has height and position is on the surface.
        if (colHeight <= 0) return false;
        if (pos.y != colHeight - 1) return false;
        // Prevent placement on path tiles.
        return !pathPositions.Contains(new Vector2Int(pos.x, pos.z));
    }

    // Returns the list of all paths.
    public List<List<Vector3Int>> GetPaths() => paths;

    // Returns the terrain's center grid position.
    public Vector3Int GetCenterGrid() => center;

    // Returns the surface height at grid position (x, z).
    public int GetSurfaceY(int x, int z) => columnHeights[x, z];

    // Converts a grid position to a world position at the surface.
    public Vector3 GetSurfaceWorldPosition(Vector3Int grid)
    {
        int y = GetSurfaceY(grid.x, grid.z);
        return transform.TransformPoint(new Vector3(grid.x, y, grid.z));
    }

    // Converts a 2D grid position (x, z) to a world position at the surface.
    public Vector3 GridToWorld(int x, int z)
    {
        int y = GetSurfaceY(x, z);
        return transform.TransformPoint(new Vector3(x, y, z));
    }

    // Highlights a specific path with visual overlays.
    public void HighlightPath(int pathIndex)
    {
        // Remove existing path highlights.
        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("Highlight_"))
                Destroy(child.gameObject);
        }

        if (pathIndex < 0 || pathIndex >= paths.Count) return;

        var path = paths[pathIndex];
        foreach (var pos in path)
        {
            // Create a quad to visually highlight the path tile.
            GameObject highlight = GameObject.CreatePrimitive(PrimitiveType.Quad);
            highlight.name = "Highlight_Path";
            highlight.transform.SetParent(transform);
            highlight.transform.localPosition = new Vector3(pos.x + 0.5f, GetSurfaceY(pos.x, pos.z) + 0.01f, pos.z + 0.5f);
            highlight.transform.localRotation = Quaternion.Euler(90, 0, 0);
            highlight.transform.localScale = Vector3.one * 1.01f;
            Renderer rend = highlight.GetComponent<Renderer>();
            rend.material = new Material(Shader.Find("Standard"))
            {
                color = new Color(0, 1, 1, 0.5f) // Cyan with transparency
            };
        }
    }

    // Finds valid defender locations near a specific path within a given range.
    public List<Vector3Int> GetDefenderLocationsNearPath(int pathIndex, int numLocations = 3, int range = 2)
    {
        List<Vector3Int> validLocations = new List<Vector3Int>();
        if (pathIndex < 0 || pathIndex >= paths.Count) return validLocations;

        var path = paths[pathIndex];
        foreach (var pos in path)
        {
            int surfaceY = GetSurfaceY(pos.x, pos.z);
            // Check nearby positions within the specified range.
            for (int x = -range; x <= range; x++)
            {
                for (int z = -range; z <= range; z++)
                {
                    Vector3Int testPosition = new Vector3Int(pos.x + x, surfaceY - 1, pos.z + z);
                    testPosition.x = Mathf.Clamp(testPosition.x, 0, width - 1);
                    testPosition.z = Mathf.Clamp(testPosition.z, 0, depth - 1);
                    if (IsValidDefenderPlacement(testPosition) && !validLocations.Contains(testPosition))
                    {
                        validLocations.Add(testPosition);
                    }
                }
            }
        }

        // Randomly select up to numLocations valid locations.
        if (validLocations.Count > 0)
        {
            // Shuffle the list for randomization.
            for (int i = 0; i < validLocations.Count; i++)
            {
                Vector3Int temp = validLocations[i];
                int randomIndex = Random.Range(i, validLocations.Count);
                validLocations[i] = validLocations[randomIndex];
                validLocations[randomIndex] = temp;
            }
            // Limit to the requested number of locations.
            int locationsToShow = Mathf.Min(numLocations, validLocations.Count);
            validLocations = validLocations.GetRange(0, locationsToShow);
        }
        return validLocations;
    }

    // Highlights defender locations near a specific path.
    public void HighlightDefenderLocations(int pathIndex, int numLocations = 3, int range = 2)
    {
        // Remove existing defender highlights.
        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("Highlight_Defender"))
                Destroy(child.gameObject);
        }

        List<Vector3Int> validLocations = GetDefenderLocationsNearPath(pathIndex, numLocations, range);
        foreach (Vector3Int location in validLocations)
        {
            int surfaceY = GetSurfaceY(location.x, location.z);
            // Create a quad to highlight the defender location.
            GameObject highlight = GameObject.CreatePrimitive(PrimitiveType.Quad);
            highlight.name = "Highlight_Defender_" + location.x + "_" + location.z;
            highlight.transform.SetParent(transform);
            highlight.transform.localPosition = new Vector3(location.x + 0.5f, surfaceY + 0.01f, location.z + 0.5f);
            highlight.transform.localRotation = Quaternion.Euler(90, 0, 0);
            highlight.transform.localScale = Vector3.one * 0.9f;
            Renderer rend = highlight.GetComponent<Renderer>();
            rend.material = defenderHighlightMaterial != null ? defenderHighlightMaterial : new Material(Shader.Find("Standard"))
            {
                color = new Color(0, 0, 1, 0.5f) // Blue with transparency
            };
        }
    }
}