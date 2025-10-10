using UnityEngine;

/// <summary>
/// Controls a top-down camera for tower defense gameplay.
/// Provides WASD movement and mouse zoom functionality.
/// </summary>
public class TopDownCamera : MonoBehaviour
{
    [Header("Camera Settings")]
    [Tooltip("Height of the camera above the terrain")]
    public float cameraHeight = 20f;
    
    [Tooltip("Angle of the camera (60 degrees is good for top-down view)")]
    public float cameraAngle = 60f;
    
    [Tooltip("Offset from the center of the terrain")]
    public Vector3 cameraOffset = new Vector3(0, 0, -10f);
    
    [Header("Camera Movement")]
    [Tooltip("Speed of camera panning")]
    public float panSpeed = 10f;
    
    [Tooltip("Speed of camera zooming")]
    public float zoomSpeed = 5f;
    
    [Tooltip("Minimum zoom distance")]
    public float minZoom = 10f;
    
    [Tooltip("Maximum zoom distance")]
    public float maxZoom = 30f;
    
    [Header("References")]
    [Tooltip("Reference to the terrain generator for bounds")]
    public VoxelTerrainGenerator terrainGenerator;
    
    private Camera cam;
    private Vector3 targetPosition;
    private float targetZoom;
    private Vector3 initialPosition;
    
    void Start()
    {
        cam = GetComponent<Camera>();
        targetPosition = transform.position;
        targetZoom = cameraHeight;
        initialPosition = transform.position;
        
        // Set up top-down view
        transform.rotation = Quaternion.Euler(cameraAngle, 0, 0);
        
        // Find terrain generator if not assigned
        if (terrainGenerator == null)
            terrainGenerator = FindFirstObjectByType<VoxelTerrainGenerator>();
    }
    
    void Update()
    {
        HandleCameraMovement();
        HandleZoom();
        HandleReset();
    }
    
    /// <summary>
    /// Handles camera movement with WASD or arrow keys
    /// </summary>
    void HandleCameraMovement()
    {
        // WASD or Arrow Keys for camera movement
        Vector3 input = Vector3.zero;
        
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            input.z += 1;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            input.z -= 1;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            input.x -= 1;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            input.x += 1;
            
        targetPosition += input * panSpeed * Time.deltaTime;
        
        // Clamp camera position to terrain bounds if terrain generator is available
        if (terrainGenerator != null)
        {
            float halfWidth = terrainGenerator.width * 0.5f;
            float halfDepth = terrainGenerator.depth * 0.5f;
            
            targetPosition.x = Mathf.Clamp(targetPosition.x, -halfWidth, halfWidth);
            targetPosition.z = Mathf.Clamp(targetPosition.z, -halfDepth, halfDepth);
        }
        
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 5f);
    }
    
    /// <summary>
    /// Handles camera zoom with mouse scroll wheel
    /// </summary>
    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        targetZoom -= scroll * zoomSpeed;
        targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        
        Vector3 newPos = transform.position;
        newPos.y = targetZoom;
        transform.position = Vector3.Lerp(transform.position, newPos, Time.deltaTime * 5f);
    }
    
    /// <summary>
    /// Handles camera reset to initial position
    /// </summary>
    void HandleReset()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            targetPosition = initialPosition;
            targetZoom = cameraHeight;
        }
    }
    
    /// <summary>
    /// Centers the camera on a specific world position
    /// </summary>
    public void CenterOnPosition(Vector3 worldPosition)
    {
        targetPosition = new Vector3(worldPosition.x, targetPosition.y, worldPosition.z);
    }
    
    /// <summary>
    /// Centers the camera on the terrain center
    /// </summary>
    public void CenterOnTerrain()
    {
        if (terrainGenerator != null)
        {
            Vector3Int center = terrainGenerator.GetCenterGrid();
            Vector3 worldCenter = terrainGenerator.GetSurfaceWorldPosition(center);
            CenterOnPosition(worldCenter);
        }
    }
}
