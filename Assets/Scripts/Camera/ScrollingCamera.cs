using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Simple scrolling camera controller using WASD keys
/// Starts positioned above the tower and allows free movement
/// </summary>
public class ScrollingCamera : MonoBehaviour
{
    [Header("Camera Movement Settings")]
    [Tooltip("Movement speed of the camera")]
    public float moveSpeed = 10f;
    
    [Tooltip("Speed multiplier when holding shift")]
    public float fastMoveMultiplier = 2f;
    
    [Tooltip("Smooth movement damping")]
    public float smoothTime = 0.1f;
    
    [Header("Camera Bounds")]
    [Tooltip("Minimum Y position (height above ground)")]
    public float minHeight = 20f;
    
    [Tooltip("Maximum Y position (height above ground)")]
    public float maxHeight = 100f;
    
    [Tooltip("Boundary limits for X and Z movement")]
    public float boundaryLimit = 200f;
    
    [Header("Tower Position")]
    [Tooltip("The tower transform to start above")]
    public Transform towerTransform;
    
    [Tooltip("Offset from tower position when starting")]
    public Vector3 towerOffset = new Vector3(0, 40f, 0);
    
    // Private variables for smooth movement
    private Vector3 velocity = Vector3.zero;
    private Vector3 targetPosition;
    private bool isInitialized = false;
    
    // Input System variables
    private Keyboard keyboard;
    
    void Start()
    {
        // Initialize input system
        keyboard = Keyboard.current;
        
        // Set initial position above the tower
        InitializeCameraPosition();
    }
    
    void Update()
    {
        HandleInput();
        MoveCamera();
    }
    
    /// <summary>
    /// Initialize camera position above the tower
    /// </summary>
    void InitializeCameraPosition()
    {
        if (towerTransform != null)
        {
            // Start above the tower
            targetPosition = towerTransform.position + towerOffset;
        }
        else
        {
            // Fallback to hardcoded position from the image
            targetPosition = new Vector3(77f, 55f, 71f);
        }
        
        transform.position = targetPosition;
        transform.rotation = Quaternion.Euler(81.979f, 0f, 0f);
        isInitialized = true;
    }
    
    /// <summary>
    /// Handle WASD input for camera movement
    /// </summary>
    void HandleInput()
    {
        if (!isInitialized || keyboard == null) return;
        
        Vector3 inputDirection = Vector3.zero;
        
        // Get input from WASD keys using Input System
        if (keyboard[Key.W].isPressed)
        {
            inputDirection += Vector3.forward;
        }
        if (keyboard[Key.S].isPressed)
        {
            inputDirection += Vector3.back;
        }
        if (keyboard[Key.A].isPressed)
        {
            inputDirection += Vector3.left;
        }
        if (keyboard[Key.D].isPressed)
        {
            inputDirection += Vector3.right;
        }
        
        // Handle vertical movement with Q and E
        if (keyboard[Key.Q].isPressed)
        {
            inputDirection += Vector3.up;
        }
        if (keyboard[Key.E].isPressed)
        {
            inputDirection += Vector3.down;
        }
        
        // Normalize input to prevent faster diagonal movement
        if (inputDirection.magnitude > 0)
        {
            inputDirection.Normalize();
            
            // Apply speed multiplier
            float currentSpeed = moveSpeed;
            if (keyboard[Key.LeftShift].isPressed)
            {
                currentSpeed *= fastMoveMultiplier;
            }
            
            // Calculate target position
            Vector3 movement = inputDirection * currentSpeed * Time.deltaTime;
            targetPosition += movement;
            
            // Apply boundary limits
            targetPosition.x = Mathf.Clamp(targetPosition.x, -boundaryLimit, boundaryLimit);
            targetPosition.z = Mathf.Clamp(targetPosition.z, -boundaryLimit, boundaryLimit);
            targetPosition.y = Mathf.Clamp(targetPosition.y, minHeight, maxHeight);
        }
    }
    
    /// <summary>
    /// Smoothly move camera to target position
    /// </summary>
    void MoveCamera()
    {
        if (!isInitialized) return;
        
        // Smoothly move to target position
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
    
    /// <summary>
    /// Reset camera to tower position
    /// </summary>
    public void ResetToTower()
    {
        if (towerTransform != null)
        {
            targetPosition = towerTransform.position + towerOffset;
        }
        else
        {
            targetPosition = new Vector3(77f, 55f, 71f);
        }
    }
    
    /// <summary>
    /// Set camera bounds for the terrain
    /// </summary>
    public void SetBounds(float minX, float maxX, float minZ, float maxZ)
    {
        boundaryLimit = Mathf.Max(Mathf.Abs(minX), Mathf.Abs(maxX), Mathf.Abs(minZ), Mathf.Abs(maxZ));
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw boundary limits in scene view
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(boundaryLimit * 2, 0, boundaryLimit * 2));
        
        // Draw height limits
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(new Vector3(-boundaryLimit, minHeight, 0), new Vector3(boundaryLimit, minHeight, 0));
        Gizmos.DrawLine(new Vector3(-boundaryLimit, maxHeight, 0), new Vector3(boundaryLimit, maxHeight, 0));
    }
}
