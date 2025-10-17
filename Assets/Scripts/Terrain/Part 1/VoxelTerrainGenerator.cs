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
    
    [Header("Efficient Highlighting")]
    [Tooltip("Use efficient material-based highlighting instead of individual GameObjects")]
    public bool useEfficientHighlighting = true;
    
    [Tooltip("Use simplified highlighting (creates fewer, larger highlights)")]
    public bool useSimplifiedHighlighting = true;
    
    [Tooltip("Highlight material for valid placement areas")]
    public Material validPlacementMaterial;
    
    [Tooltip("Highlight material for invalid placement areas")]
    public Material invalidPlacementMaterial;
    
    // Cache for highlighted chunks
    private List<GameObject> highlightedChunks = new List<GameObject>();
    private bool isHighlighting = false;

    [Header("Tree Spawning")]
    // Prefab for trees to spawn on grass tiles.
    public GameObject treePrefab;
    // Percentage of grass tiles that will spawn trees (0 to 1).
    [Range(0, 1)] public float treeDensity = 0.05f;


    [Header("Path Settings")]
    // Number of paths to carve from edges to the terrain center.
    public int numPaths = 3;
    // Maximum number of paths allowed (predetermined progression)
    public int maxPaths = 5;
    // Width of paths in voxels.
    public int pathWidth = 2;
    
    [Header("Path Unlock Progression")]
    [Tooltip("Wave when path 4 is unlocked")]
    public int path4UnlockWave = 10;
    [Tooltip("Wave when path 5 is unlocked")]
    public int path5UnlockWave = 15;
    
    [Header("Performance Requirements")]
    [Tooltip("Minimum performance score required to unlock path 4 (0-100)")]
    public float path4PerformanceRequirement = 70f;
    [Tooltip("Minimum performance score required to unlock path 5 (0-100)")]
    public float path5PerformanceRequirement = 80f;

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
    
    [Header("Defender Zone Settings")]
    [Tooltip("Number of defender zones to generate")]
    public int numDefenderZones = 10;
    
    [Tooltip("Size of each defender zone (radius)")]
    public int defenderZoneSize = 3;
    
    [Tooltip("Minimum distance between zones")]
    public int minZoneDistance = 8;
    
    [Tooltip("Minimum distance from paths")]
    public int minDistanceFromPaths = 5;
    
    [Tooltip("Minimum distance from tower")]
    public int minDistanceFromTower = 10;
    
    // List of defender zones (clusters of valid placement positions)
    private List<DefenderZone> defenderZones = new List<DefenderZone>();
    public List<DefenderZone> DefenderZones => defenderZones;

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
        // Generate defender zones for placement.
        GenerateDefenderZones();
        // Create terrain chunks for efficient rendering.
        CreateChunks();
        // Spawn trees on grass tiles.
        SpawnTrees();

        // Mark generation as complete.
        isGenerated = true;
    }

    // Generates defender zones for strategic placement.
    private void GenerateDefenderZones()
    {
        defenderZones.Clear();
        defenderLocations.Clear();
        
        int attempts = 0;
        int maxAttempts = numDefenderZones * 10;
        
        while (defenderZones.Count < numDefenderZones && attempts < maxAttempts)
        {
            attempts++;
            
            // Generate random position
            Vector3Int zoneCenter = new Vector3Int(
                Random.Range(defenderZoneSize, width - defenderZoneSize),
                0,
                Random.Range(defenderZoneSize, depth - defenderZoneSize)
            );
            
            // Check if position is valid for a zone
            if (IsValidZonePosition(zoneCenter))
            {
                // Create zone
                DefenderZone zone = new DefenderZone();
                zone.center = zoneCenter;
                zone.positions = GenerateZonePositions(zoneCenter);
                zone.isActive = true;
                
                defenderZones.Add(zone);
                
                // Add zone positions to defender locations
                foreach (Vector3Int pos in zone.positions)
                {
                    if (!defenderLocations.Contains(pos))
                    {
                        defenderLocations.Add(pos);
                    }
                }
            }
        }
    }
    
    // Generates positions within a defender zone
    private List<Vector3Int> GenerateZonePositions(Vector3Int center)
    {
        List<Vector3Int> zonePositions = new List<Vector3Int>();
        int surfaceY = GetSurfaceY(center.x, center.z);
        
        // Generate positions in a square around the center
        for (int x = -defenderZoneSize; x <= defenderZoneSize; x++)
        {
            for (int z = -defenderZoneSize; z <= defenderZoneSize; z++)
            {
                Vector3Int testPos = new Vector3Int(center.x + x, surfaceY - 1, center.z + z);
                
                // Clamp to terrain bounds
                testPos.x = Mathf.Clamp(testPos.x, 0, width - 1);
                testPos.z = Mathf.Clamp(testPos.z, 0, depth - 1);
                
                // Check if position is valid for defender placement
                if (IsValidDefenderPlacement(testPos))
                {
                    zonePositions.Add(testPos);
                }
            }
        }
        
        return zonePositions;
    }
    
    // Checks if a position is valid for a defender zone
    private bool IsValidZonePosition(Vector3Int center)
    {
        // Check distance from tower
        Vector3Int towerPos = new Vector3Int(width / 2, 0, depth / 2);
        float distanceFromTower = Vector3Int.Distance(center, towerPos);
        if (distanceFromTower < minDistanceFromTower) return false;
        
        // Check distance from paths
        foreach (Vector2Int pathPos in pathPositions)
        {
            float distanceFromPath = Vector2Int.Distance(
                new Vector2Int(center.x, center.z), 
                pathPos
            );
            if (distanceFromPath < minDistanceFromPaths) return false;
        }
        
        // Check distance from other zones
        foreach (DefenderZone existingZone in defenderZones)
        {
            float distanceFromZone = Vector3Int.Distance(center, existingZone.center);
            if (distanceFromZone < minZoneDistance) return false;
        }
        
        return true;
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
        // Allow placement if y is 0 (grid position) or if it's on the surface
        if (pos.y != 0 && pos.y != colHeight - 1) return false;
        // Prevent placement on path tiles.
        return !pathPositions.Contains(new Vector2Int(pos.x, pos.z));
    }

    // Returns the list of all paths.
    public List<List<Vector3Int>> GetPaths() => paths;
    
    /// <summary>
    /// Checks for path unlocks based on wave progression AND performance
    /// </summary>
    public void CheckPathUnlocks()
    {
        // Get current wave
        EnemySpawner enemySpawner = FindFirstObjectByType<EnemySpawner>();
        if (enemySpawner == null) return;
        
        // Get performance tracker
        PlayerPerformanceTracker performanceTracker = FindFirstObjectByType<PlayerPerformanceTracker>();
        if (performanceTracker == null) return;
        
        int currentWave = enemySpawner.currentWave;
        float performanceScore = performanceTracker.performanceScore;
        
        // Check for path 4 unlock (wave requirement AND performance requirement)
        if (currentWave >= path4UnlockWave && 
            performanceScore >= path4PerformanceRequirement && 
            paths.Count < 4)
        {
            Debug.Log($" WAVE {currentWave}: Lane 4 Unlocked! (Performance: {performanceScore:F1}/{path4PerformanceRequirement})");
            UnlockPath(4);
            ShowPathUnlockNotification(4);
        }
        else if (currentWave >= path4UnlockWave && paths.Count < 4)
        {
            Debug.Log($" Lane 4 locked - Performance too low! ({performanceScore:F1}/{path4PerformanceRequirement})");
            ShowPerformanceRequirementNotification(4, performanceScore, path4PerformanceRequirement);
        }
        
        // Check for path 5 unlock (wave requirement AND performance requirement)
        if (currentWave >= path5UnlockWave && 
            performanceScore >= path5PerformanceRequirement && 
            paths.Count < 5)
        {
            Debug.Log($" WAVE {currentWave}: Lane 5 Unlocked! (Performance: {performanceScore:F1}/{path5PerformanceRequirement})");
            UnlockPath(5);
            ShowPathUnlockNotification(5);
        }
        else if (currentWave >= path5UnlockWave && paths.Count < 5)
        {
            Debug.Log($" Lane 5 locked - Performance too low! ({performanceScore:F1}/{path5PerformanceRequirement})");
            ShowPerformanceRequirementNotification(5, performanceScore, path5PerformanceRequirement);
        }
    }
    
    /// <summary>
    /// Unlocks a predetermined path
    /// </summary>
    void UnlockPath(int pathNumber)
    {
        if (paths.Count >= pathNumber) return; // Already unlocked
        
        HashSet<Vector3Int> usedEntrances = new HashSet<Vector3Int>();
        
        // Get existing entrance positions
        foreach (var path in paths)
        {
            if (path.Count > 0)
            {
                usedEntrances.Add(path[0]);
            }
        }
        
        int attempts = 0;
        int maxAttempts = 20;
        
        while (attempts < maxAttempts)
        {
            Vector3Int entrance = GetRandomEdgePosition();
            
            // Skip if entrance is already used
            if (usedEntrances.Contains(entrance))
            {
                attempts++;
                continue;
            }
            
            usedEntrances.Add(entrance);
            
            // Generate a path from entrance to center
            List<Vector3Int> newPath = GeneratePath(entrance, center);
            
            // Accept path if it's long enough and has minimal overlap
            if (newPath.Count > 10 && !PathsIntersect(newPath))
            {
                paths.Add(newPath);
                
                // Mark path positions with specified width
                foreach (var pos in newPath)
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
                
                Debug.Log($" Lane {pathNumber} unlocked! Total paths: {paths.Count}/{maxPaths}");
                return;
            }
            
            attempts++;
        }
        
        Debug.LogWarning($" Failed to unlock lane {pathNumber} - no valid entrance found");
    }
    
    /// <summary>
    /// Shows UI notification for path unlock
    /// </summary>
    void ShowPathUnlockNotification(int pathNumber)
    {
        PathUnlockNotification notification = FindFirstObjectByType<PathUnlockNotification>();
        if (notification != null)
        {
            notification.ShowPathUnlockNotification(pathNumber);
        }
    }
    
    /// <summary>
    /// Shows UI notification for performance requirement
    /// </summary>
    void ShowPerformanceRequirementNotification(int pathNumber, float currentPerformance, float requiredPerformance)
    {
        PathUnlockNotification notification = FindFirstObjectByType<PathUnlockNotification>();
        if (notification != null)
        {
            notification.ShowPerformanceRequirementNotification(pathNumber, currentPerformance, requiredPerformance);
        }
    }

    // Returns the terrain's center grid position.
    public Vector3Int GetCenterGrid() => center;

    // Returns the surface height at grid position (x, z).
    public int GetSurfaceY(int x, int z) 
    {
        // Add bounds checking to prevent array out of bounds
        if (x < 0 || x >= width || z < 0 || z >= depth)
        {
            Debug.LogWarning($"GetSurfaceY: Position ({x}, {z}) is outside terrain bounds ({width}x{depth})");
            return 0; // Return 0 for out of bounds positions
        }
        return columnHeights[x, z];
    }

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

    /// <summary>
    /// Converts world position to grid position for drag-drop system
    /// </summary>
    public Vector3Int WorldToGridPosition(Vector3 worldPos)
    {
        // Convert world position back to grid coordinates
        Vector3 localPos = transform.InverseTransformPoint(worldPos);
        int x = Mathf.RoundToInt(localPos.x);
        int z = Mathf.RoundToInt(localPos.z);
        int y = GetSurfaceY(x, z) - 1; // Surface level
        
        return new Vector3Int(x, y, z);
    }

    /// <summary>
    /// Gets all valid defender placement positions for highlighting
    /// </summary>
    public List<Vector3Int> GetAllValidDefenderPositions()
    {
        List<Vector3Int> validPositions = new List<Vector3Int>();
        
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                int surfaceY = GetSurfaceY(x, z);
                Vector3Int testPos = new Vector3Int(x, surfaceY - 1, z);
                
                if (IsValidDefenderPlacement(testPos))
                {
                    validPositions.Add(testPos);
                }
            }
        }
        
        return validPositions;
    }

    /// <summary>
    /// Highlights all valid defender placement areas using efficient material-based system
    /// </summary>
    public void HighlightAllValidDefenderAreas()
    {
        if (isHighlighting) return;
        
        ClearPlacementHighlights();
        
        if (useEfficientHighlighting)
        {
            if (useSimplifiedHighlighting)
            {
                HighlightAreasSimplified();
            }
            else
            {
                HighlightAreasEfficiently();
            }
        }
        else
        {
            HighlightAreasLegacy();
        }
    }
    
    /// <summary>
    /// Simplified highlighting - creates large area highlights instead of individual positions
    /// </summary>
    void HighlightAreasSimplified()
    {
        isHighlighting = true;
        
        // Create default materials if not assigned
        if (validPlacementMaterial == null)
        {
            validPlacementMaterial = CreateHighlightMaterial(Color.green, 0.3f);
        }
        
        List<Vector3Int> validPositions = GetAllValidDefenderPositions();
        
        // Group positions into larger areas (every 5x5 grid)
        Dictionary<Vector2Int, List<Vector3Int>> areaGroups = new Dictionary<Vector2Int, List<Vector3Int>>();
        
        foreach (Vector3Int pos in validPositions)
        {
            // Group by 5x5 areas
            Vector2Int areaKey = new Vector2Int(pos.x / 5, pos.z / 5);
            if (!areaGroups.ContainsKey(areaKey))
            {
                areaGroups[areaKey] = new List<Vector3Int>();
            }
            areaGroups[areaKey].Add(pos);
        }
        
        // Create one highlight per area
        foreach (var area in areaGroups)
        {
            if (area.Value.Count > 0)
            {
                // Find the center of this area
                Vector3 center = Vector3.zero;
                foreach (Vector3Int pos in area.Value)
                {
                    center += GetSurfaceWorldPosition(pos);
                }
                center /= area.Value.Count;
                
                // Create a large highlight for this area
                GameObject highlight = CreateAreaHighlight(center, 5f);
                if (highlight != null)
                {
                    highlightedChunks.Add(highlight);
                }
            }
        }
        
        isHighlighting = false;
        Debug.Log($"Simplified highlighting: {highlightedChunks.Count} area highlights created");
    }
    
    /// <summary>
    /// Creates a large area highlight
    /// </summary>
    GameObject CreateAreaHighlight(Vector3 center, float size)
    {
        GameObject highlight = new GameObject("AreaHighlight");
        highlight.transform.position = center + Vector3.up * 0.1f;
        
        // Create a large quad
        MeshRenderer renderer = highlight.AddComponent<MeshRenderer>();
        MeshFilter filter = highlight.AddComponent<MeshFilter>();
        
        // Create a simple quad mesh
        Mesh quadMesh = CreateQuadMesh(size, size);
        filter.mesh = quadMesh;
        
        // Apply highlight material
        renderer.material = validPlacementMaterial;
        
        return highlight;
    }
    
    /// <summary>
    /// Efficient highlighting using material changes on existing terrain chunks
    /// </summary>
    void HighlightAreasEfficiently()
    {
        isHighlighting = true;
        
        // Create default materials if not assigned
        if (validPlacementMaterial == null)
        {
            validPlacementMaterial = CreateHighlightMaterial(Color.green, 0.3f);
        }
        
        // Get all terrain chunks
        VoxelChunk[] chunks = GetComponentsInChildren<VoxelChunk>();
        List<Vector3Int> validPositions = GetAllValidDefenderPositions();
        
        // Create a set for faster lookup
        HashSet<Vector3Int> validPositionsSet = new HashSet<Vector3Int>(validPositions);
        
        foreach (VoxelChunk chunk in chunks)
        {
            if (chunk == null) continue;
            
            // Get chunk bounds from transform position and chunk size
            Vector3 chunkWorldPos = chunk.transform.position;
            int currentChunkSize = chunkSize; // Use the chunk size from terrain generator
            
            // Check if this chunk contains any valid positions
            bool hasValidPositions = false;
            for (int x = 0; x < currentChunkSize && !hasValidPositions; x++)
            {
                for (int z = 0; z < currentChunkSize && !hasValidPositions; z++)
                {
                    Vector3Int worldPos = new Vector3Int(
                        Mathf.RoundToInt(chunkWorldPos.x) + x,
                        0, // We'll check surface height
                        Mathf.RoundToInt(chunkWorldPos.z) + z
                    );
                    
                    int surfaceY = GetSurfaceY(worldPos.x, worldPos.z);
                    Vector3Int testPos = new Vector3Int(worldPos.x, surfaceY - 1, worldPos.z);
                    
                    if (validPositionsSet.Contains(testPos))
                    {
                        hasValidPositions = true;
                    }
                }
            }
            
            if (hasValidPositions)
            {
                // Create a highlight overlay for this chunk
                GameObject highlightOverlay = CreateChunkHighlight(chunk, currentChunkSize);
                if (highlightOverlay != null)
                {
                    highlightedChunks.Add(highlightOverlay);
                }
            }
        }
        
        isHighlighting = false;
        Debug.Log($"Efficiently highlighted {highlightedChunks.Count} chunks with valid placement areas");
    }
    
    /// <summary>
    /// Creates a highlight overlay for a chunk
    /// </summary>
    GameObject CreateChunkHighlight(VoxelChunk chunk, int chunkSize)
    {
        GameObject highlight = new GameObject($"ChunkHighlight_{chunk.name}");
        highlight.transform.SetParent(chunk.transform);
        highlight.transform.localPosition = Vector3.zero;
        
        // Create a large quad that covers the chunk
        MeshRenderer renderer = highlight.AddComponent<MeshRenderer>();
        MeshFilter filter = highlight.AddComponent<MeshFilter>();
        
        // Create a simple quad mesh
        Mesh quadMesh = CreateQuadMesh(chunkSize, chunkSize);
        filter.mesh = quadMesh;
        
        // Position it slightly above the terrain
        highlight.transform.localPosition = new Vector3(
            chunkSize * 0.5f,
            0.1f,
            chunkSize * 0.5f
        );
        
        // Apply highlight material
        renderer.material = validPlacementMaterial;
        
        return highlight;
    }
    
    /// <summary>
    /// Creates a quad mesh for highlighting
    /// </summary>
    Mesh CreateQuadMesh(float width, float depth)
    {
        Mesh mesh = new Mesh();
        
        Vector3[] vertices = new Vector3[]
        {
            new Vector3(-width * 0.5f, 0, -depth * 0.5f),
            new Vector3(width * 0.5f, 0, -depth * 0.5f),
            new Vector3(width * 0.5f, 0, depth * 0.5f),
            new Vector3(-width * 0.5f, 0, depth * 0.5f)
        };
        
        Vector2[] uvs = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1)
        };
        
        int[] triangles = new int[] { 0, 1, 2, 0, 2, 3 };
        
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        
        return mesh;
    }
    
    /// <summary>
    /// Creates a highlight material
    /// </summary>
    Material CreateHighlightMaterial(Color color, float alpha)
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(color.r, color.g, color.b, alpha);
        mat.SetFloat("_Mode", 3); // Transparent mode
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
        return mat;
    }
    
    /// <summary>
    /// Legacy highlighting method (creates individual GameObjects)
    /// </summary>
    void HighlightAreasLegacy()
    {
        List<Vector3Int> validPositions = GetAllValidDefenderPositions();
        
        // Limit highlights for performance
        int maxHighlights = 50;
        int count = 0;
        
        foreach (Vector3Int pos in validPositions)
        {
            if (count >= maxHighlights) break;
            
            Vector3 worldPos = GetSurfaceWorldPosition(pos);
            worldPos.y += 0.1f; // Slightly above surface
            
            // Create highlight marker
            GameObject highlight = GameObject.CreatePrimitive(PrimitiveType.Cube);
            highlight.transform.position = worldPos;
            highlight.transform.localScale = Vector3.one * 0.8f;
            highlight.name = "DefenderPlacementHighlight";
            
            // Make it semi-transparent green
            Renderer renderer = highlight.GetComponent<Renderer>();
            Material highlightMaterial = new Material(Shader.Find("Standard"));
            highlightMaterial.color = new Color(0, 1, 0, 0.3f);
            highlightMaterial.SetFloat("_Mode", 3); // Transparent mode
            renderer.material = highlightMaterial;
            
            // Remove collider to prevent interference
            Destroy(highlight.GetComponent<Collider>());
            
            count++;
        }
        
        Debug.Log($"Legacy highlighting: {count} individual highlights created");
    }

    /// <summary>
    /// Clears all placement highlights
    /// </summary>
    public void ClearPlacementHighlights()
    {
        // Clear efficient highlighting
        foreach (GameObject highlight in highlightedChunks)
        {
            if (highlight != null)
            {
                Destroy(highlight);
            }
        }
        highlightedChunks.Clear();
        
        // Clear legacy highlighting (fallback)
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == "DefenderPlacementHighlight" || obj.name.StartsWith("ChunkHighlight_"))
            {
                Destroy(obj);
            }
        }
        
        isHighlighting = false;
    }
}

/// <summary>
/// Represents a defender zone containing multiple valid placement positions.
/// </summary>
[System.Serializable]
public class DefenderZone
{
    [Tooltip("Center position of the zone")]
    public Vector3Int center;
    
    [Tooltip("List of valid placement positions within the zone")]
    public List<Vector3Int> positions = new List<Vector3Int>();
    
    [Tooltip("Whether the zone is currently active")]
    public bool isActive = true;
    
    [Tooltip("Number of defenders currently placed in this zone")]
    public int defendersPlaced = 0;
    
    /// <summary>
    /// Gets the number of available positions in this zone
    /// </summary>
    public int AvailablePositions => positions.Count - defendersPlaced;
    
    /// <summary>
    /// Checks if a position is within this zone
    /// </summary>
    public bool ContainsPosition(Vector3Int position)
    {
        return positions.Contains(position);
    }
    
    /// <summary>
    /// Adds a defender to this zone
    /// </summary>
    public void AddDefender()
    {
        defendersPlaced++;
    }
    
    /// <summary>
    /// Removes a defender from this zone
    /// </summary>
    public void RemoveDefender()
    {
        defendersPlaced = Mathf.Max(0, defendersPlaced - 1);
    }
}