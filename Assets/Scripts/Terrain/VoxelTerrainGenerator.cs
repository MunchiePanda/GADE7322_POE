using UnityEngine;
using System.Collections.Generic;

public class VoxelTerrainGenerator : MonoBehaviour
{
    [Header("Terrain Dimensions")]
    public int width = 150;
    public int depth = 150;
    public int height = 8;

    [Header("Chunk Settings")]
    public int chunkSize = 16;

    [Header("Noise Settings")]
    public bool useNoise = true;
    public int minColumnHeight = 4;
    public int noiseAmplitude = 4;
    public float noiseScale = 0.03f;
    public Vector2 noiseOffset = new Vector2(137.13f, 59.87f);
    public int octaves = 4;
    public float persistence = 0.5f;
    public float lacunarity = 2f;

    [Header("Biome Settings")]
    public bool useBiomes = true;
    public float biomeScale = 0.02f;
    public Vector2 biomeOffset = new Vector2(42f, 73f);

    [Header("Voxel Prefab")]
    public GameObject voxelPrefab;

    [Header("Tree Spawning")]
    public GameObject treePrefab;
    [Range(0, 1)] public float treeDensity = 0.05f; // Percentage of grass tiles that will have trees

    [Header("Path Settings")]
    public int numPaths = 3;
    public int pathWidth = 2;

    [Header("Layer Settings")]
    public int terrainLayer = 8;

    private int[,] columnHeights;
    private List<List<Vector3Int>> paths = new List<List<Vector3Int>>();
    private Vector3Int center;
    private HashSet<Vector2Int> pathPositions = new HashSet<Vector2Int>();
    private bool isGenerated = false;
    public bool IsReady => isGenerated;

    private List<Vector3Int> defenderLocations = new List<Vector3Int>();
    public List<Vector3Int> DefenderLocations => defenderLocations;

    void Awake() => GenerateAllIfNeeded();
    void Start() => GenerateAllIfNeeded();

    void GenerateAllIfNeeded()
    {
        if (isGenerated) return;

        center = new Vector3Int(width / 2, 0, depth / 2);
        columnHeights = new int[width, depth];

        // Generate heights
        for (int x = 0; x < width; x++)
            for (int z = 0; z < depth; z++)
                columnHeights[x, z] = ComputeColumnHeight(x, z);

        // Carve paths
        CarvePaths();

        // Create chunks
        CreateChunks();

        // Spawn trees in grass areas
        SpawnTrees();

        isGenerated = true;

        GenerateDefenderLocations();
    }

    private void GenerateDefenderLocations()
    {
        defenderLocations.Clear();

        foreach (var path in paths)
        {
            foreach (var pos in path)
            {
                int surfaceY = GetSurfaceY(pos.x, pos.z);

                // Check nearby positions for valid placement
                for (int x = -2; x <= 2; x++)
                {
                    for (int z = -2; z <= 2; z++)
                    {
                        Vector3Int testPosition = new Vector3Int(pos.x + x, surfaceY - 1, pos.z + z);
                        testPosition.x = Mathf.Clamp(testPosition.x, 0, width - 1);
                        testPosition.z = Mathf.Clamp(testPosition.z, 0, depth - 1);

                        if (IsValidDefenderPlacement(testPosition) && !defenderLocations.Contains(testPosition))
                        {
                            defenderLocations.Add(testPosition);
                        }
                    }
                }
            }
        }
    }

    void SpawnTrees()
    {
        if (treePrefab == null || !useBiomes)
            return;

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                // Check if this is a grass area
                float nx = (x + biomeOffset.x) * biomeScale;
                float nz = (z + biomeOffset.y) * biomeScale;
                float biomeValue = Mathf.PerlinNoise(nx, nz);

                TextureType topType = (biomeValue < 0.5f) ? TextureType.Grass : TextureType.Sand;

                if (topType == TextureType.Grass && columnHeights[x, z] > 0)
                {
                    // Randomly decide if a tree should be placed here
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

    private void CreateChunks()
    {
        // Calculate number of chunks needed in each dimension
        int chunksX = Mathf.CeilToInt((float)width / chunkSize);
        int chunksZ = Mathf.CeilToInt((float)depth / chunkSize);

        // Create each chunk
        for (int cx = 0; cx < chunksX; cx++)
        {
            int chunkStartX = cx * chunkSize;
            int chunkWidth = Mathf.Min(chunkSize, width - chunkStartX);

            for (int cz = 0; cz < chunksZ; cz++)
            {
                int chunkStartZ = cz * chunkSize;
                int chunkDepth = Mathf.Min(chunkSize, depth - chunkStartZ);

                // Create chunk GameObject
                GameObject chunkGO = new GameObject($"Chunk_{cx}_{cz}");
                chunkGO.transform.SetParent(transform);
                chunkGO.transform.localPosition = new Vector3(chunkStartX, 0, chunkStartZ);

                // Add and initialize VoxelChunk component
                VoxelChunk chunk = chunkGO.AddComponent<VoxelChunk>();
                chunk.Initialize(chunkWidth, height, chunkDepth, chunkStartX, chunkStartZ,
                               voxelPrefab, terrainLayer, width, depth, columnHeights);
                chunk.Generate(useNoise, minColumnHeight, noiseAmplitude, noiseScale, noiseOffset,
                             useBiomes, biomeScale, biomeOffset);
            }
        }
    }

    int ComputeColumnHeight(int x, int z)
    {
        if (!useNoise) return Mathf.Clamp(minColumnHeight + noiseAmplitude, 1, height);

        float totalNoise = 0f;
        float frequency = 1f;
        float amplitude = 1f;
        float maxValue = 0f;

        for (int o = 0; o < octaves; o++)
        {
            float nx = (x + noiseOffset.x) * noiseScale * frequency;
            float nz = (z + noiseOffset.y) * noiseScale * frequency;
            totalNoise += Mathf.PerlinNoise(nx, nz) * amplitude;

            maxValue += amplitude;
            amplitude *= persistence;
            frequency *= lacunarity;
        }

        float noiseValue = totalNoise / maxValue;
        int h = minColumnHeight + Mathf.RoundToInt(noiseValue * noiseAmplitude);
        return Mathf.Clamp(h, minColumnHeight, height);
    }

    void CarvePaths()
    {
        HashSet<Vector3Int> usedEntrances = new HashSet<Vector3Int>();
        int attempts = 0;
        int maxAttempts = 20;

        while (paths.Count < numPaths && attempts < maxAttempts)
        {
            Vector3Int entrance = GetRandomEdgePosition();
            if (usedEntrances.Contains(entrance))
            {
                attempts++;
                continue;
            }

            usedEntrances.Add(entrance);
            List<Vector3Int> path = GeneratePath(entrance, center);

            if (path.Count > 10 && !PathsIntersect(path))
            {
                paths.Add(path);

                // Mark path positions without changing height
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

    bool PathsIntersect(List<Vector3Int> newPath)
    {
        // Allow some overlap to generate more paths
        int overlapCount = 0;
        foreach (var path in paths)
        {
            foreach (var pos in path)
            {
                if (newPath.Contains(pos))
                {
                    overlapCount++;
                    if (overlapCount > 2) // Allow up to 2 overlapping positions
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    Vector3Int GetRandomEdgePosition()
    {
        int edge = Random.Range(0, 4);
        int x = 0, z = 0;
        switch (edge)
        {
            case 0: x = 0; z = Random.Range(0, depth); break;
            case 1: x = width - 1; z = Random.Range(0, depth); break;
            case 2: x = Random.Range(0, width); z = 0; break;
            case 3: x = Random.Range(0, width); z = depth - 1; break;
        }
        return new Vector3Int(x, 0, z);
    }

    List<Vector3Int> GeneratePath(Vector3Int start, Vector3Int end)
    {
        List<Vector3Int> path = new List<Vector3Int> { start };
        Vector3Int current = start;
        int safety = 0;
        int maxSteps = width + depth;

        while (current != end && safety < maxSteps)
        {
            int dx = end.x - current.x;
            int dz = end.z - current.z;
            List<Vector3Int> options = new List<Vector3Int>();

            if (dx != 0) options.Add(new Vector3Int(current.x + (int)Mathf.Sign(dx), 0, current.z));
            if (dz != 0) options.Add(new Vector3Int(current.x, 0, current.z + (int)Mathf.Sign(dz)));

            if (Random.value < 0.3f && options.Count == 2) options.Reverse();
            Vector3Int next = options[Random.Range(0, options.Count)];
            current = next;

            if (!path.Contains(current)) path.Add(current);
            safety++;
        }
        return path;
    }

    public bool IsValidDefenderPlacement(Vector3Int pos)
    {
        if (!isGenerated) return false;
        if (pos.x < 0 || pos.x >= width || pos.z < 0 || pos.z >= depth) return false;

        int colHeight = columnHeights[pos.x, pos.z];
        if (colHeight <= 0) return false;

        if (pos.y != colHeight - 1) return false;

        // Block placement on path tiles
        return !pathPositions.Contains(new Vector2Int(pos.x, pos.z));
    }

    public List<List<Vector3Int>> GetPaths() => paths;
    public Vector3Int GetCenterGrid() => center;
    public int GetSurfaceY(int x, int z) => columnHeights[x, z];

    public Vector3 GetSurfaceWorldPosition(Vector3Int grid)
    {
        int y = GetSurfaceY(grid.x, grid.z);
        return transform.TransformPoint(new Vector3(grid.x, y, grid.z));
    }

    public Vector3 GridToWorld(int x, int z)
    {
        int y = GetSurfaceY(x, z);
        return transform.TransformPoint(new Vector3(x, y, z));
    }

    public void HighlightPath(int pathIndex)
    {
        // Reset previous highlights
        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("Highlight_"))
                Destroy(child.gameObject);
        }

        if (pathIndex < 0 || pathIndex >= paths.Count) return;

        var path = paths[pathIndex];
        foreach (var pos in path)
        {
            // Spawn flat quad overlay for highlight
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

    public List<Vector3Int> GetDefenderLocationsNearPath(int pathIndex, int numLocations = 3, int range = 2)
    {
        List<Vector3Int> validLocations = new List<Vector3Int>();

        if (pathIndex < 0 || pathIndex >= paths.Count) return validLocations;

        var path = paths[pathIndex];

        foreach (var pos in path)
        {
            int surfaceY = GetSurfaceY(pos.x, pos.z);

            // Check nearby positions for valid placement within the specified range
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

        // Randomly select a subset of valid locations
        if (validLocations.Count > 0)
        {
            // Shuffle the list to randomize selection
            for (int i = 0; i < validLocations.Count; i++)
            {
                Vector3Int temp = validLocations[i];
                int randomIndex = Random.Range(i, validLocations.Count);
                validLocations[i] = validLocations[randomIndex];
                validLocations[randomIndex] = temp;
            }

            // Select up to numLocations locations
            int locationsToShow = Mathf.Min(numLocations, validLocations.Count);
            validLocations = validLocations.GetRange(0, locationsToShow);
        }

        return validLocations;
    }

    public void HighlightDefenderLocations(int pathIndex, int numLocations = 3, int range = 2)
    {
        // Reset previous highlights
        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("Highlight_Defender"))
                Destroy(child.gameObject);
        }

        List<Vector3Int> validLocations = GetDefenderLocationsNearPath(pathIndex, numLocations, range);

        // Randomly select a subset of valid locations
        if (validLocations.Count > 0)
        {
            // Shuffle the list to randomize selection
            for (int i = 0; i < validLocations.Count; i++)
            {
                Vector3Int temp = validLocations[i];
                int randomIndex = Random.Range(i, validLocations.Count);
                validLocations[i] = validLocations[randomIndex];
                validLocations[randomIndex] = temp;
            }

            // Select up to numLocations locations
            int locationsToShow = Mathf.Min(numLocations, validLocations.Count);
            validLocations = validLocations.GetRange(0, locationsToShow);

            foreach (Vector3Int location in validLocations)
            {
                int surfaceY = GetSurfaceY(location.x, location.z);

                // Spawn flat quad overlay for highlight
                GameObject highlight = GameObject.CreatePrimitive(PrimitiveType.Quad);
                highlight.name = "Highlight_Defender_" + location.x + "_" + location.z;
                highlight.transform.SetParent(transform);
                highlight.transform.localPosition = new Vector3(location.x + 0.5f, surfaceY + 0.01f, location.z + 0.5f);
                highlight.transform.localRotation = Quaternion.Euler(90, 0, 0);
                highlight.transform.localScale = Vector3.one * 0.9f;

                Renderer rend = highlight.GetComponent<Renderer>();
                rend.material = new Material(Shader.Find("Standard"))
                {
                    color = new Color(1, 0, 1, 0.5f) // Pink with transparency
                };
            }
        }
    }
}
