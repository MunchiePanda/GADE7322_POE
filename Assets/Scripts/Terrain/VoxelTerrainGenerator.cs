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
    public int minColumnHeight = 2;
    public int noiseAmplitude = 6;
    public float noiseScale = 0.05f;
    public Vector2 noiseOffset = new Vector2(137.13f, 59.87f);

    [Header("Biome Settings")]
    public bool useBiomes = true;
    public float biomeScale = 0.02f;
    public Vector2 biomeOffset = new Vector2(42f, 73f);

    [Header("Voxel Prefab")]
    public GameObject voxelPrefab;

    [Header("Path Settings")]
    public int numPaths = 3;
    public int pathHeight = 1;
    public int pathWidth = 2;

    [Header("Layer Settings")]
    public int terrainLayer = 8;

    private int[,] columnHeights;
    private List<List<Vector3Int>> paths = new List<List<Vector3Int>>();
    private Vector3Int center;
    private bool isGenerated = false;
    public bool IsReady => isGenerated;

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

        isGenerated = true;
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

        float nx = (x + noiseOffset.x) * noiseScale;
        float nz = (z + noiseOffset.y) * noiseScale;
        float noiseValue = Mathf.PerlinNoise(nx, nz);
        int h = minColumnHeight + Mathf.RoundToInt(noiseValue * noiseAmplitude);
        return Mathf.Clamp(h, 1, height);
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

            if (path.Count > 0)
            {
                paths.Add(path);
                // Carve wider paths
                foreach (var pos in path)
                {
                    for (int dx = -pathWidth / 2; dx <= pathWidth / 2; dx++)
                    {
                        for (int dz = -pathWidth / 2; dz <= pathWidth / 2; dz++)
                        {
                            int nx = pos.x + dx;
                            int nz = pos.z + dz;
                            if (nx >= 0 && nx < width && nz >= 0 && nz < depth)
                            {
                                columnHeights[nx, nz] = pathHeight;
                            }
                        }
                    }
                }
            }
            attempts++;
        }
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
        return true;
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
}
