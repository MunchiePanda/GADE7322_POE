using UnityEngine;
using System.Collections.Generic;

public class VoxelChunk : MonoBehaviour
{
    public int chunkSizeX { get; private set; }
    public int chunkSizeY { get; private set; }
    public int chunkSizeZ { get; private set; }
    public int startX { get; private set; }
    public int startY { get; private set; }
    public int startZ { get; private set; }

    private GameObject[,,] voxels;
    private bool isActive = false;
    private GameObject voxelPrefab;
    private int terrainLayer;

    public void Initialize(int sizeX, int sizeY, int sizeZ, int x, int y, int z, GameObject prefab, int layer)
    {
        chunkSizeX = sizeX;
        chunkSizeY = sizeY;
        chunkSizeZ = sizeZ;
        startX = x;
        startY = y;
        startZ = z;
        voxelPrefab = prefab;
        terrainLayer = layer;

        voxels = new GameObject[sizeX, sizeY, sizeZ];
    }

    public void Generate(int[,] columnHeights, bool useNoise, int minColumnHeight, int noiseAmplitude, float noiseScale, Vector2 noiseOffset)
    {
        for (int x = 0; x < chunkSizeX; x++)
        {
            for (int y = 0; y < chunkSizeY; y++)
            {
                for (int z = 0; z < chunkSizeZ; z++)
                {
                    int worldX = startX + x;
                    int worldZ = startZ + z;

                    if (y == 0)
                    {
                        int colHeight = ComputeColumnHeight(worldX, worldZ, useNoise, minColumnHeight, noiseAmplitude, noiseScale, noiseOffset);
                        columnHeights[worldX, worldZ] = Mathf.Clamp(colHeight, 1, chunkSizeY);
                    }

                    if (y < columnHeights[worldX, worldZ])
                    {
                        GameObject voxel = Instantiate(voxelPrefab, transform);
                        voxel.transform.localPosition = new Vector3(x, y, z);
                        voxel.transform.localScale = Vector3.one;
                        voxel.name = $"{x},{y},{z}";
                    }
                }
            }
        }
    }

    private int ComputeColumnHeight(int x, int z, bool useNoise, int minColumnHeight, int noiseAmplitude, float noiseScale, Vector2 noiseOffset)
    {
        if (!useNoise)
        {
            return Mathf.Clamp(minColumnHeight + noiseAmplitude, 1, chunkSizeY);
        }
        float nx = (x + noiseOffset.x) * noiseScale;
        float nz = (z + noiseOffset.y) * noiseScale;
        float n = Mathf.PerlinNoise(nx, nz);
        int h = minColumnHeight + Mathf.RoundToInt(n * noiseAmplitude);
        return Mathf.Clamp(h, 1, chunkSizeY);
    }

    public void SetActive(bool active)
    {
        if (isActive == active) return;

        isActive = active;
        gameObject.SetActive(active);
    }

    public bool IsActive()
    {
        return isActive;
    }

    public void CombineMeshes()
    {
        List<MeshFilter> meshFilters = new List<MeshFilter>(GetComponentsInChildren<MeshFilter>());
        meshFilters.RemoveAll(mf => mf.gameObject == gameObject);

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
        combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        combinedMesh.CombineMeshes(combine.ToArray());

        MeshFilter parentMF = GetComponent<MeshFilter>();
        if (parentMF == null) parentMF = gameObject.AddComponent<MeshFilter>();
        parentMF.sharedMesh = combinedMesh;

        MeshRenderer parentMR = GetComponent<MeshRenderer>();
        if (parentMR == null) parentMR = gameObject.AddComponent<MeshRenderer>();
        parentMR.sharedMaterial = voxelPrefab.GetComponent<MeshRenderer>().sharedMaterial;

        MeshCollider mc = GetComponent<MeshCollider>();
        if (mc == null) mc = gameObject.AddComponent<MeshCollider>();
        mc.sharedMesh = combinedMesh;

        gameObject.layer = terrainLayer;

        foreach (var mf in meshFilters)
        {
            Destroy(mf.gameObject);
        }
    }
}
