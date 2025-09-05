using UnityEngine;
using System.Collections.Generic;

public class VoxelChunk : MonoBehaviour
{
    private int chunkSizeX, chunkSizeY, chunkSizeZ;
    private int startX, startZ;
    private GameObject voxelPrefab;
    private int terrainLayer;
    private int fullWidth, fullDepth;
    private int[,] columnHeights;

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

    public void Generate(bool useNoise, int minColumnHeight, int noiseAmplitude, float noiseScale, Vector2 noiseOffset, bool useBiomes, float biomeScale, Vector2 biomeOffset)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();

        for (int lx = 0; lx < chunkSizeX; lx++)
        {
            int worldX = startX + lx;
            for (int lz = 0; lz < chunkSizeZ; lz++)
            {
                int worldZ = startZ + lz;

                // Check if we're within bounds of the global columnHeights array
                if (worldX >= 0 && worldX < fullWidth && worldZ >= 0 && worldZ < fullDepth)
                {
                    int colHeight = columnHeights[worldX, worldZ];

                    // Only generate if there's terrain here
                    if (colHeight > 0)
                    {
                        for (int ly = 0; ly < colHeight; ly++)
                        {
                            Vector3 localPos = new Vector3(lx, ly, lz);
                            AddVoxelFaces(localPos, worldX, ly, worldZ, colHeight, vertices, uvs, triangles, useBiomes, biomeScale, biomeOffset);
                        }
                    }
                }
            }
        }

        BuildMesh(vertices, uvs, triangles);
    }

    private void AddVoxelFaces(Vector3 localPos, int worldX, int localY, int worldZ, int colHeight,
                             List<Vector3> vertices, List<Vector2> uvs, List<int> triangles,
                             bool useBiomes, float biomeScale, Vector2 biomeOffset)
    {
        // Check all 6 faces
        CheckFace(FaceDirection.Top, localPos, worldX, localY, worldZ, colHeight, vertices, uvs, triangles, useBiomes, biomeScale, biomeOffset);
        CheckFace(FaceDirection.Bottom, localPos, worldX, localY, worldZ, colHeight, vertices, uvs, triangles, useBiomes, biomeScale, biomeOffset);
        CheckFace(FaceDirection.Front, localPos, worldX, localY, worldZ, colHeight, vertices, uvs, triangles, useBiomes, biomeScale, biomeOffset);
        CheckFace(FaceDirection.Back, localPos, worldX, localY, worldZ, colHeight, vertices, uvs, triangles, useBiomes, biomeScale, biomeOffset);
        CheckFace(FaceDirection.Left, localPos, worldX, localY, worldZ, colHeight, vertices, uvs, triangles, useBiomes, biomeScale, biomeOffset);
        CheckFace(FaceDirection.Right, localPos, worldX, localY, worldZ, colHeight, vertices, uvs, triangles, useBiomes, biomeScale, biomeOffset);
    }

    private void CheckFace(FaceDirection face, Vector3 localPos, int worldX, int localY, int worldZ, int colHeight,
                          List<Vector3> vertices, List<Vector2> uvs, List<int> triangles,
                          bool useBiomes, float biomeScale, Vector2 biomeOffset)
    {
        if (IsFaceVisible(face, worldX, localY, worldZ, colHeight))
        {
            AddFace(face, localPos, worldX, localY, worldZ, colHeight, vertices, uvs, triangles, useBiomes, biomeScale, biomeOffset);
        }
    }

    private bool IsFaceVisible(FaceDirection face, int worldX, int localY, int worldZ, int colHeight)
    {
        Vector3Int offset = GetFaceOffset(face);
        int nX = worldX + offset.x;
        int nY = localY + offset.y;
        int nZ = worldZ + offset.z;

        // Always show top faces
        if (face == FaceDirection.Top && localY == colHeight - 1) return true;

        // Check if neighbor is out of bounds
        if (nX < 0 || nX >= fullWidth || nZ < 0 || nZ >= fullDepth) return true;
        if (nY < 0 || nY >= chunkSizeY) return true;

        // Check if neighbor is solid
        if (nX >= 0 && nX < fullWidth && nZ >= 0 && nZ < fullDepth)
        {
            int neighborHeight = columnHeights[nX, nZ];
            return nY >= neighborHeight || localY >= neighborHeight;
        }

        return true;
    }

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

    private void AddFace(FaceDirection face, Vector3 pos, int worldX, int localY, int worldZ, int colHeight,
                        List<Vector3> vertices, List<Vector2> uvs, List<int> triangles,
                        bool useBiomes, float biomeScale, Vector2 biomeOffset)
    {
        TextureType type = GetTextureType(face, localY, colHeight, worldX, worldZ, useBiomes, biomeScale, biomeOffset);
        Vector2[] atlasUVs = TextureAtlas.GetUVs(type);
        Vector3[] faceVerts = GetFaceVertices(face, pos);

        int vCount = vertices.Count;
        vertices.AddRange(faceVerts);
        uvs.AddRange(atlasUVs);

        bool reverseWinding = (face == FaceDirection.Bottom || face == FaceDirection.Back || face == FaceDirection.Left);
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

    private Vector3[] GetFaceVertices(FaceDirection face, Vector3 pos)
    {
        switch (face)
        {
            case FaceDirection.Top: return new Vector3[] { pos + new Vector3(0,1,0), pos + new Vector3(1,1,0), pos + new Vector3(1,1,1), pos + new Vector3(0,1,1) };
            case FaceDirection.Bottom: return new Vector3[] { pos + new Vector3(0,0,0), pos + new Vector3(1,0,0), pos + new Vector3(1,0,1), pos + new Vector3(0,0,1) };
            case FaceDirection.Front: return new Vector3[] { pos + new Vector3(0,0,1), pos + new Vector3(1,0,1), pos + new Vector3(1,1,1), pos + new Vector3(0,1,1) };
            case FaceDirection.Back: return new Vector3[] { pos + new Vector3(0,0,0), pos + new Vector3(1,0,0), pos + new Vector3(1,1,0), pos + new Vector3(0,1,0) };
            case FaceDirection.Left: return new Vector3[] { pos + new Vector3(0,0,0), pos + new Vector3(0,0,1), pos + new Vector3(0,1,1), pos + new Vector3(0,1,0) };
            case FaceDirection.Right: return new Vector3[] { pos + new Vector3(1,0,0), pos + new Vector3(1,1,0), pos + new Vector3(1,1,1), pos + new Vector3(1,0,1) };
            default: return null;
        }
    }

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

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null) meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = mesh;

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null) meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = voxelPrefab.GetComponent<MeshRenderer>().sharedMaterial;

        MeshCollider meshCollider = GetComponent<MeshCollider>();
        if (meshCollider == null) meshCollider = gameObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;

        gameObject.layer = terrainLayer;
    }
}
