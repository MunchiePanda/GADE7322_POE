using UnityEngine;
using UnityEngine.InputSystem;

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
    public float panSpeed = 15f;
    
    [Tooltip("Speed of camera zooming")]
    public float zoomSpeed = 5f;
    
    [Tooltip("Minimum zoom distance")]
    public float minZoom = 10f;
    
    [Tooltip("Maximum zoom distance")]
    public float maxZoom = 30f;
    
    [Tooltip("Camera movement responsiveness (higher = less floaty)")]
    public float movementResponsiveness = 8f;
    
    [Tooltip("Boundary margin from terrain edges")]
    public float boundaryMargin = 5f;
    
    [Header("Mouse Edge Panning")]
    [Tooltip("Enable mouse edge panning (like Baldur's Gate)")]
    public bool enableMouseEdgePanning = true;
    
    [Tooltip("Distance from screen edge to start panning (in pixels)")]
    public float edgePanDistance = 15f;
    
    [Tooltip("Speed multiplier for mouse edge panning")]
    public float mousePanSpeedMultiplier = 1.2f;
    
    [Tooltip("Smooth factor for mouse edge panning")]
    public float mousePanSmoothness = 8f;
    
    [Header("Mouse Drag Panning")]
    [Tooltip("Enable middle mouse button drag panning")]
    public bool enableMouseDragPanning = true;
    
    [Tooltip("Speed multiplier for mouse drag panning")]
    public float mouseDragSpeedMultiplier = 1.5f;
    
    [Header("References")]
    [Tooltip("Reference to the terrain generator for bounds")]
    public VoxelTerrainGenerator terrainGenerator;
    
    [Tooltip("Input Actions asset for camera controls")]
    public InputActionAsset inputActions;
    
    private Camera cam;
    private Vector3 targetPosition;
    private float targetZoom;
    private Vector3 initialPosition;
    private Vector3 mousePanInput;
    private bool isDragging = false;
    private Vector3 lastMousePosition;
    
    // Input System references
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction scrollAction;
    private InputAction middleClickAction;
    
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
        
        // Set up Input System
        SetupInputActions();
        
        // Ensure camera starts within bounds
        ConstrainCameraToBounds();
    }
    
    void SetupInputActions()
    {
        // If no input actions asset is assigned, try to find the default one
        if (inputActions == null)
        {
            inputActions = Resources.Load<InputActionAsset>("InputSystem_Actions");
        }
        
        if (inputActions != null)
        {
            // Get actions from the Player action map
            var playerActionMap = inputActions.FindActionMap("Player");
            if (playerActionMap != null)
            {
                moveAction = playerActionMap.FindAction("Move");
                lookAction = playerActionMap.FindAction("Look");
            }
            
            // Get scroll wheel from UI action map
            var uiActionMap = inputActions.FindActionMap("UI");
            if (uiActionMap != null)
            {
                scrollAction = uiActionMap.FindAction("ScrollWheel");
                middleClickAction = uiActionMap.FindAction("MiddleClick");
            }
        }
        
        // Enable actions
        if (moveAction != null) moveAction.Enable();
        if (lookAction != null) lookAction.Enable();
        if (scrollAction != null) scrollAction.Enable();
        if (middleClickAction != null) middleClickAction.Enable();
    }
    
    void Update()
    {
        HandleCameraMovement();
        HandleMouseEdgePanning();
        HandleMouseDragPanning();
        HandleZoom();
        HandleReset();
    }
    
    /// <summary>
    /// Handles camera movement with WASD, arrow keys, and additional controls
    /// </summary>
    void HandleCameraMovement()
    {
        Vector3 input = Vector3.zero;
        
        // Get movement input from Input System
        if (moveAction != null)
        {
            Vector2 moveInput = moveAction.ReadValue<Vector2>();
            input.x = moveInput.x;
            input.z = moveInput.y;
        }
        else
        {
            // Fallback to direct Input System calls if action map is not available
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
                    input.z += 1;
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
                    input.z -= 1;
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                    input.x -= 1;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                    input.x += 1;
            }
        }
        
        // Speed boost with Shift
        bool isShiftPressed = false;
        if (moveAction != null)
        {
            // Check for shift key through Input System
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                isShiftPressed = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
            }
        }
        
        float speedMultiplier = isShiftPressed ? 2f : 1f;
            
        targetPosition += input * panSpeed * speedMultiplier * Time.deltaTime;
        
        // Constrain camera to terrain bounds
        ConstrainCameraToBounds();
        
        // Use more responsive movement to reduce floatiness
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * movementResponsiveness);
    }
    
    /// <summary>
    /// Handles mouse edge panning (like Baldur's Gate)
    /// </summary>
    void HandleMouseEdgePanning()
    {
        if (!enableMouseEdgePanning) return;
        
        Vector3 mouseInput = Vector3.zero;
        Vector3 mousePosition = Vector3.zero;
        
        // Get mouse position from Input System
        var mouse = Mouse.current;
        if (mouse != null)
        {
            mousePosition = mouse.position.ReadValue();
        }
        
        // Get screen dimensions
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        
        // Check if mouse is near screen edges
        if (mousePosition.x <= edgePanDistance)
        {
            // Left edge - pan left
            float panStrength = 1f - (mousePosition.x / edgePanDistance);
            mouseInput.x = -panStrength;
        }
        else if (mousePosition.x >= screenWidth - edgePanDistance)
        {
            // Right edge - pan right
            float panStrength = 1f - ((screenWidth - mousePosition.x) / edgePanDistance);
            mouseInput.x = panStrength;
        }
        
        if (mousePosition.y <= edgePanDistance)
        {
            // Bottom edge - pan down
            float panStrength = 1f - (mousePosition.y / edgePanDistance);
            mouseInput.z = -panStrength;
        }
        else if (mousePosition.y >= screenHeight - edgePanDistance)
        {
            // Top edge - pan up
            float panStrength = 1f - ((screenHeight - mousePosition.y) / edgePanDistance);
            mouseInput.z = panStrength;
        }
        
        // Apply mouse panning input
        if (mouseInput != Vector3.zero)
        {
            Vector3 mousePanMovement = mouseInput * panSpeed * mousePanSpeedMultiplier * Time.deltaTime;
            targetPosition += mousePanMovement;
            
            // Constrain camera to terrain bounds
            ConstrainCameraToBounds();
        }
    }
    
    /// <summary>
    /// Handles middle mouse button drag panning
    /// </summary>
    void HandleMouseDragPanning()
    {
        if (!enableMouseDragPanning) return;
        
        var mouse = Mouse.current;
        if (mouse == null) return;
        
        // Check for middle mouse button press
        if (mouse.middleButton.wasPressedThisFrame)
        {
            isDragging = true;
            lastMousePosition = mouse.position.ReadValue();
        }
        
        // Handle dragging
        if (isDragging)
        {
            if (mouse.middleButton.isPressed)
            {
                Vector3 currentMousePosition = mouse.position.ReadValue();
                Vector3 mouseDelta = currentMousePosition - lastMousePosition;
                
                // Convert screen delta to world movement
                Vector3 worldDelta = new Vector3(-mouseDelta.x, 0, -mouseDelta.y) * mouseDragSpeedMultiplier * Time.deltaTime;
                targetPosition += worldDelta;
                
                // Constrain camera to terrain bounds
                ConstrainCameraToBounds();
                
                lastMousePosition = currentMousePosition;
            }
            else
            {
                isDragging = false;
            }
        }
    }
    
    /// <summary>
    /// Handles camera zoom with mouse scroll wheel
    /// </summary>
    void HandleZoom()
    {
        float scroll = 0f;
        
        // Get scroll input from Input System
        if (scrollAction != null)
        {
            Vector2 scrollInput = scrollAction.ReadValue<Vector2>();
            scroll = scrollInput.y;
        }
        else
        {
            // Fallback to direct Input System calls
            var mouse = Mouse.current;
            if (mouse != null)
            {
                scroll = mouse.scroll.ReadValue().y;
            }
        }
        
        targetZoom -= scroll * zoomSpeed;
        targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        
        Vector3 newPos = transform.position;
        newPos.y = targetZoom;
        transform.position = Vector3.Lerp(transform.position, newPos, Time.deltaTime * 5f);
    }
    
    /// <summary>
    /// Handles camera reset and additional keyboard shortcuts
    /// </summary>
    void HandleReset()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;
        
        // Reset camera to initial position
        if (keyboard.rKey.wasPressedThisFrame)
        {
            targetPosition = initialPosition;
            targetZoom = cameraHeight;
        }
        
        // Center camera on terrain
        if (keyboard.cKey.wasPressedThisFrame)
        {
            CenterOnTerrain();
        }
        
        // Focus on terrain center with F key
        if (keyboard.fKey.wasPressedThisFrame)
        {
            CenterOnTerrain();
        }
    }
    
    void OnDestroy()
    {
        // Disable input actions when the component is destroyed
        if (moveAction != null) moveAction.Disable();
        if (lookAction != null) lookAction.Disable();
        if (scrollAction != null) scrollAction.Disable();
        if (middleClickAction != null) middleClickAction.Disable();
    }
    
    /// <summary>
    /// Gets the proper terrain bounds for camera constraints
    /// </summary>
    Vector2 GetTerrainBounds()
    {
        if (terrainGenerator == null) return Vector2.zero;
        
        // Get terrain dimensions and apply boundary margin
        float halfWidth = (terrainGenerator.width * 0.5f) - boundaryMargin;
        float halfDepth = (terrainGenerator.depth * 0.5f) - boundaryMargin;
        
        return new Vector2(halfWidth, halfDepth);
    }
    
    /// <summary>
    /// Constrains camera position to terrain bounds
    /// </summary>
    void ConstrainCameraToBounds()
    {
        if (terrainGenerator == null) return;
        
        Vector2 bounds = GetTerrainBounds();
        
        targetPosition.x = Mathf.Clamp(targetPosition.x, -bounds.x, bounds.x);
        targetPosition.z = Mathf.Clamp(targetPosition.z, -bounds.y, bounds.y);
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
    
    /// <summary>
    /// Gets the current camera bounds for debugging
    /// </summary>
    public Vector2 GetCurrentBounds()
    {
        return GetTerrainBounds();
    }
    
    /// <summary>
    /// Checks if the camera is within bounds
    /// </summary>
    public bool IsWithinBounds()
    {
        if (terrainGenerator == null) return true;
        
        Vector2 bounds = GetTerrainBounds();
        return Mathf.Abs(targetPosition.x) <= bounds.x && Mathf.Abs(targetPosition.z) <= bounds.y;
    }
}
