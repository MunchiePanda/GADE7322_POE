using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Handles the generation and mesh construction of a chunk of voxels in the terrain.
/// </summary>
public class VoxelChunk : MonoBehaviour
{
    // Chunk dimensions
    private int chunkSizeX, chunkSizeY, chunkSizeZ;
    // Starting position in the world grid
    private int startX, startZ;
    // Prefab used for voxel appearance/material
    private GameObject voxelPrefab;
    // Layer to assign to the chunk for rendering/collision
    private int terrainLayer;
    // Full terrain dimensions
    private int fullWidth, fullDepth;
    // Heights of terrain columns for mesh generation
    private int[,] columnHeights;

    /// <summary>
    /// Initializes the chunk with its size, position, prefab, layer, and terrain data.
    /// </summary>
    public void Initialize(int sizeX, int sizeY, int sizeZ, int x, int z, GameObject prefab, int layer, int fullW, int fullD, int[,] heights)
    {
        chunkSizeX = sizeX;
        chunkSizeY = sizeY;
        chunkSizeZ = sizeZ;
        startX = x;
        startZ = z;
        voxelPrefab = prefab;
        terrainLayer = layer;
        fullWidth = fullW;
        fullDepth = fullD;
        columnHeights = heights;
    }

    /// <summary>
    /// Generates the voxel mesh for this chunk using terrain and biome data.
    /// </summary>
    public void Generate(bool useNoise, int minColumnHeight, int noiseAmplitude, float noiseScale, Vector2 noiseOffset, bool useBiomes, float biomeScale, Vector2 biomeOffset)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();

        // Iterate over each voxel position in the chunk
        for (int lx = 0; lx < chunkSizeX; lx++)
        {
            int worldX = startX + lx;
            for (int lz = 0; lz < chunkSizeZ; lz++)
            {
                int worldZ = startZ + lz;

                // Ensure within terrain bounds
                if (worldX >= 0 && worldX < fullWidth && worldZ >= 0 && worldZ < fullDepth)
                {
                    int colHeight = columnHeights[worldX, worldZ];

                    // Only generate voxels where terrain exists
                    if (colHeight > 0)
                    {
                        for (int ly = 0; ly < colHeight; ly++)
                        {
                            Vector3 localPos = new Vector3(lx, ly, lz);
                            // Add visible faces for this voxel
                            AddVoxelFaces(localPos, worldX, ly, worldZ, colHeight, vertices, uvs, triangles, useBiomes, biomeScale, biomeOffset);
                        }
                    }
                }
            }
        }

        // Build the mesh from the collected vertices, uvs, and triangles
        BuildMesh(vertices, uvs, triangles);
    }

    /// <summary>
    /// Adds all visible faces for a voxel at the given position.
    /// </summary>
    private void AddVoxelFaces(Vector3 localPos, int worldX, int localY, int worldZ, int colHeight,
                             List<Vector3> vertices, List<Vector2> uvs, List<int> triangles,
                             bool useBiomes, float biomeScale, Vector2 biomeOffset)
    {
        // Check and add each of the 6 faces
        CheckFace(FaceDirection.Top, localPos, worldX, localY, worldZ, colHeight, vertices, uvs, triangles, useBiomes, biomeScale, biomeOffset);
        CheckFace(FaceDirection.Bottom, localPos, worldX, localY, worldZ, colHeight, vertices, uvs, triangles, useBiomes, biomeScale, biomeOffset);
        CheckFace(FaceDirection.Front, localPos, worldX, localY, worldZ, colHeight, vertices, uvs, triangles, useBiomes, biomeScale, biomeOffset);
        CheckFace(FaceDirection.Back, localPos, worldX, localY, worldZ, colHeight, vertices, uvs, triangles, useBiomes, biomeScale, biomeOffset);
        CheckFace(FaceDirection.Left, localPos, worldX, localY, worldZ, colHeight, vertices, uvs, triangles, useBiomes, biomeScale, biomeOffset);
        CheckFace(FaceDirection.Right, localPos, worldX, localY, worldZ, colHeight, vertices, uvs, triangles, useBiomes, biomeScale, biomeOffset);
    }

    /// <summary>
    /// Checks if a face should be visible and adds it to the mesh if so.
    /// </summary>
    private void CheckFace(FaceDirection face, Vector3 localPos, int worldX, int localY, int worldZ, int colHeight,
                          List<Vector3> vertices, List<Vector2> uvs, List<int> triangles,
                          bool useBiomes, float biomeScale, Vector2 biomeOffset)
    {
        bool isVisible = IsFaceVisible(face, worldX, localY, worldZ, colHeight);
        if (isVisible)
        {
            Debug.Log($"Adding {face} face at {worldX}, {localY}, {worldZ}");
            AddFace(face, localPos, worldX, localY, worldZ, colHeight, vertices, uvs, triangles, useBiomes, biomeScale, biomeOffset);
        }
    }

    /// <summary>
    /// Determines if a voxel face is visible (not covered by another voxel).
    /// </summary>
    private bool IsFaceVisible(FaceDirection face, int worldX, int localY, int worldZ, int colHeight)
    {
        Vector3Int offset = GetFaceOffset(face);
        int nX = worldX + offset.x;
        int nY = localY + offset.y;
        int nZ = worldZ + offset.z;

        // Top face is always visible if it's the highest voxel in the column
        if (face == FaceDirection.Top && localY == colHeight - 1)
        {
            Debug.Log($"Showing top face at {worldX}, {localY}, {worldZ}");
            return true;
        }

        // If neighbor is out of bounds, face is visible
        if (nX < 0 || nX >= fullWidth || nZ < 0 || nZ >= fullDepth) return true;
        if (nY < 0 || nY >= chunkSizeY) return true;

        // If neighbor voxel is not solid, face is visible
        if (nX >= 0 && nX < fullWidth && nZ >= 0 && nZ < fullDepth)
        {
            int neighborHeight = columnHeights[nX, nZ];
            bool isVisible = nY >= neighborHeight || localY >= neighborHeight;
            if (face == FaceDirection.Top)
            {
                Debug.Log($"Top face at {worldX}, {localY}, {worldZ} is {(isVisible ? "visible" : "not visible")}");
            }
            return isVisible;
        }

        return true;
    }

    /// <summary>
    /// Returns the offset vector for a given face direction.
    /// </summary>
    private Vector3Int GetFaceOffset(FaceDirection face)
    {
        switch (face)
        {
            case FaceDirection.Top: return new Vector3Int(0, 1, 0);
            case FaceDirection.Bottom: return new Vector3Int(0, -1, 0);
            case FaceDirection.Front: return new Vector3Int(0, 0, 1);
            case FaceDirection.Back: return new Vector3Int(0, 0, -1);
            case FaceDirection.Left: return new Vector3Int(-1, 0, 0);
            case FaceDirection.Right: return new Vector3Int(1, 0, 0);
            default: return Vector3Int.zero;
        }
    }

    /// <summary>
    /// Adds a face to the mesh, including vertices, UVs, and triangles.
    /// </summary>
    private void AddFace(FaceDirection face, Vector3 pos, int worldX, int localY, int worldZ, int colHeight,
                        List<Vector3> vertices, List<Vector2> uvs, List<int> triangles,
                        bool useBiomes, float biomeScale, Vector2 biomeOffset)
    {
        // Determine texture type for this face
        TextureType type = GetTextureType(face, localY, colHeight, worldX, worldZ, useBiomes, biomeScale, biomeOffset);
        Vector2[] atlasUVs = TextureAtlas.GetUVs(type);
        Vector3[] faceVerts = GetFaceVertices(face, pos);

        int vCount = vertices.Count;
        vertices.AddRange(faceVerts);
        uvs.AddRange(atlasUVs);

        // Adjust triangle winding order for top/back faces
        bool reverseWinding = (face == FaceDirection.Top || face == FaceDirection.Back);
        if (reverseWinding)
        {
            triangles.Add(vCount + 0);
            triangles.Add(vCount + 2);
            triangles.Add(vCount + 1);
            triangles.Add(vCount + 0);
            triangles.Add(vCount + 3);
            triangles.Add(vCount + 2);
        }
        else
        {
            triangles.Add(vCount + 0);
            triangles.Add(vCount + 1);
            triangles.Add(vCount + 2);
            triangles.Add(vCount + 0);
            triangles.Add(vCount + 2);
            triangles.Add(vCount + 3);
        }
    }

    /// <summary>
    /// Returns the four vertices for a given face direction at the specified position.
    /// </summary>
    private Vector3[] GetFaceVertices(FaceDirection face, Vector3 pos)
    {
        switch (face)
        {
            case FaceDirection.Top: return new Vector3[] { pos + new Vector3(0, 1, 0), pos + new Vector3(1, 1, 0), pos + new Vector3(1, 1, 1), pos + new Vector3(0, 1, 1) };
            case FaceDirection.Bottom: return new Vector3[] { pos + new Vector3(0, 0, 0), pos + new Vector3(1, 0, 0), pos + new Vector3(1, 0, 1), pos + new Vector3(0, 0, 1) };
            case FaceDirection.Front: return new Vector3[] { pos + new Vector3(0, 0, 1), pos + new Vector3(1, 0, 1), pos + new Vector3(1, 1, 1), pos + new Vector3(0, 1, 1) };
            case FaceDirection.Back: return new Vector3[] { pos + new Vector3(0, 0, 0), pos + new Vector3(1, 0, 0), pos + new Vector3(1, 1, 0), pos + new Vector3(0, 1, 0) };
            case FaceDirection.Left: return new Vector3[] { pos + new Vector3(0, 0, 0), pos + new Vector3(0, 0, 1), pos + new Vector3(0, 1, 1), pos + new Vector3(0, 1, 0) };
            case FaceDirection.Right: return new Vector3[] { pos + new Vector3(1, 0, 0), pos + new Vector3(1, 1, 0), pos + new Vector3(1, 1, 1), pos + new Vector3(1, 0, 1) };
            default: return null;
        }
    }

    /// <summary>
    /// Determines the texture type for a face based on biome and position.
    /// </summary>
    private TextureType GetTextureType(FaceDirection face, int localY, int colHeight, int worldX, int worldZ,
                                     bool useBiomes, float biomeScale, Vector2 biomeOffset)
    {
        float biomeValue = 0f;
        if (useBiomes)
        {
            float nx = (worldX + biomeOffset.x) * biomeScale;
            float nz = (worldZ + biomeOffset.y) * biomeScale;
            biomeValue = Mathf.PerlinNoise(nx, nz);
        }

        TextureType topType = (biomeValue < 0.5f) ? TextureType.Grass : TextureType.Sand;
        TextureType sideType = (biomeValue < 0.5f) ? TextureType.Dirt : TextureType.Sand;
        TextureType bottomType = TextureType.Stone;

        if (face == FaceDirection.Top && localY == colHeight - 1) return topType;
        else if (face == FaceDirection.Bottom && localY == 0) return bottomType;
        else return sideType;
    }

    /// <summary>
    /// Builds the mesh from the provided vertices, uvs, and triangles, and assigns it to the chunk.
    /// </summary>
    private void BuildMesh(List<Vector3> vertices, List<Vector2> uvs, List<int> triangles)
    {
        if (vertices.Count == 0) return;

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        // Assign mesh to MeshFilter
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null) meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = mesh;

        // Assign material from voxel prefab to MeshRenderer
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null) meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = voxelPrefab.GetComponent<MeshRenderer>().sharedMaterial;

        // Assign mesh to MeshCollider for physics
        MeshCollider meshCollider = GetComponent<MeshCollider>();
        if (meshCollider == null) meshCollider = gameObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;

        // Set the layer for rendering/collision
        gameObject.layer = terrainLayer;
    }
}
