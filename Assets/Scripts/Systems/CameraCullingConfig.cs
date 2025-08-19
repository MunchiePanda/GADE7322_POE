using UnityEngine;

// Attach this to your Main Camera to set per-layer cull distances.
public class CameraCullingConfig : MonoBehaviour
{
    [Tooltip("Layer index used by the combined terrain mesh.")]
    public int terrainLayer = 8;

    [Header("Cull Distances (meters)")]
    [Tooltip("Cull distance for the terrain layer.")]
    public float terrainCullDistance = 200f;
    [Tooltip("Default cull distance for all other layers not explicitly set.")]
    public float defaultCullDistance = 1000f;

    [Header("Rendering Tweaks")]
    [Tooltip("Enable spherical culling which is usually better for outdoor scenes.")]
    public bool useSphericalCulling = true;
    [Tooltip("Optional: Clamp shadow rendering distance to save GPU.")]
    public float shadowDistance = 75f;
    public bool applyShadowDistance = false;

    void Start()
    {
        Camera cam = GetComponent<Camera>();
        if (cam == null)
        {
            cam = Camera.main;
            if (cam == null) return;
        }

        float[] distances = new float[32];
        for (int i = 0; i < distances.Length; i++)
        {
            distances[i] = defaultCullDistance;
        }
        if (terrainLayer >= 0 && terrainLayer < 32)
        {
            distances[terrainLayer] = terrainCullDistance;
        }
        cam.layerCullDistances = distances;
        cam.layerCullSpherical = useSphericalCulling;

        if (applyShadowDistance)
        {
            QualitySettings.shadowDistance = shadowDistance;
        }
    }
}


