using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Simple camera controller for WASD movement and positioning
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Camera Settings")]
    [Tooltip("Movement speed of the camera")]
    public float moveSpeed = 10f;
    
    [Tooltip("How fast the camera rotates")]
    public float rotationSpeed = 50f;
    
    [Tooltip("How fast the camera zooms")]
    public float zoomSpeed = 5f;
    
    [Header("Movement Constraints")]
    [Tooltip("Minimum X position")]
    public float minX = -200f;
    
    [Tooltip("Maximum X position")]
    public float maxX = 200f;
    
    [Tooltip("Minimum Z position")]
    public float minZ = -200f;
    
    [Tooltip("Maximum Z position")]
    public float maxZ = 200f;
    
    [Tooltip("Minimum Y position (height)")]
    public float minY = 5f;
    
    [Tooltip("Maximum Y position (height)")]
    public float maxY = 200f;
    
    [Header("Initial Position")]
    [Tooltip("Set initial camera position to match the image")]
    public bool setInitialPosition = true;
    
    [Tooltip("Initial position (matches the image)")]
    public Vector3 initialPosition = new Vector3(77f, 55f, 71f);
    
    [Tooltip("Initial rotation (matches the image) - fixed to prevent snapping")]
    public Vector3 initialRotation = new Vector3(15f, 0f, 0f);
    
    private Camera cam;
    private Keyboard keyboard;
    private Mouse mouse;
    
    void Start()
    {
        cam = GetComponent<Camera>();
        keyboard = Keyboard.current;
        mouse = Mouse.current;
        
        // Set initial position to match the image
        if (setInitialPosition)
        {
            transform.position = initialPosition;
            transform.rotation = Quaternion.Euler(initialRotation);
        }
    }
    
    void Update()
    {
        if (keyboard == null || mouse == null) return;
        
        HandleMovement();
        HandleRotation();
        HandleZoom();
        HandleReset();
    }
    
    /// <summary>
    /// Handles WASD movement - Baldur's Gate style (camera-relative)
    /// </summary>
    void HandleMovement()
    {
        Vector3 movement = Vector3.zero;
        
        // WASD movement - Baldur's Gate style (camera-relative)
        if (keyboard[Key.W].isPressed)
        {
            // Move forward in camera direction
            movement += transform.forward;
        }
        if (keyboard[Key.S].isPressed)
        {
            // Move backward in camera direction
            movement -= transform.forward;
        }
        if (keyboard[Key.A].isPressed)
        {
            // Move left in camera direction
            movement -= transform.right;
        }
        if (keyboard[Key.D].isPressed)
        {
            // Move right in camera direction
            movement += transform.right;
        }
        
        // Q and E for up and down movement (separate from WASD)
        if (keyboard[Key.Q].isPressed)
        {
            movement += Vector3.up;
        }
        if (keyboard[Key.E].isPressed)
        {
            movement -= Vector3.up;
        }
        
        // Apply movement
        if (movement != Vector3.zero)
        {
            Vector3 newPosition = transform.position + movement * moveSpeed * Time.deltaTime;
            
            // Clamp position within bounds
            newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
            newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);
            newPosition.z = Mathf.Clamp(newPosition.z, minZ, maxZ);
            
            transform.position = newPosition;
        }
    }
    
    /// <summary>
    /// Handles camera rotation with mouse
    /// </summary>
    void HandleRotation()
    {
        // Right mouse button for rotation
        if (mouse.rightButton.isPressed)
        {
            float mouseX = mouse.delta.x.ReadValue() * rotationSpeed * Time.deltaTime;
            float mouseY = mouse.delta.y.ReadValue() * rotationSpeed * Time.deltaTime;
            
            // Get current rotation
            Vector3 currentRotation = transform.eulerAngles;
            
            // Rotate around Y axis (horizontal) - no restrictions
            float newYRotation = currentRotation.y + mouseX;
            
            // Rotate around X axis (vertical) - clamp to prevent flipping
            float newXRotation = currentRotation.x - mouseY;
            
            // Clamp vertical rotation to prevent camera flipping
            if (newXRotation > 180f)
            {
                newXRotation = Mathf.Clamp(newXRotation, 270f, 360f);
            }
            else
            {
                newXRotation = Mathf.Clamp(newXRotation, 0f, 90f);
            }
            
            // Apply the new rotation
            transform.rotation = Quaternion.Euler(newXRotation, newYRotation, 0f);
        }
    }
    
    /// <summary>
    /// Handles camera zoom with mouse scroll
    /// </summary>
    void HandleZoom()
    {
        float scroll = mouse.scroll.ReadValue().y;
        
        if (scroll != 0f)
        {
            // Zoom by moving forward/backward in the camera's direction
            Vector3 zoomDirection = transform.forward;
            Vector3 newPosition = transform.position + zoomDirection * scroll * zoomSpeed * Time.deltaTime;
            
            // Clamp position within bounds
            newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
            newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);
            newPosition.z = Mathf.Clamp(newPosition.z, minZ, maxZ);
            
            transform.position = newPosition;
        }
    }
    
    /// <summary>
    /// Handles camera reset with R key
    /// </summary>
    void HandleReset()
    {
        if (keyboard[Key.R].wasPressedThisFrame)
        {
            ResetCamera();
        }
        
        // T key to reset rotation only (if camera gets stuck)
        if (keyboard[Key.T].wasPressedThisFrame)
        {
            ResetRotation();
        }
    }
    
    /// <summary>
    /// Resets camera to initial position
    /// </summary>
    public void ResetCamera()
    {
        transform.position = initialPosition;
        transform.rotation = Quaternion.Euler(initialRotation);
        Debug.Log("Camera reset to initial position!");
    }
    
    /// <summary>
    /// Resets only the camera rotation to a safe angle
    /// </summary>
    public void ResetRotation()
    {
        transform.rotation = Quaternion.Euler(15f, transform.eulerAngles.y, 0f);
        Debug.Log("Camera rotation reset to safe angle!");
    }
    
    /// <summary>
    /// Sets camera to look at a specific target
    /// </summary>
    public void LookAtTarget(Transform target)
    {
        if (target != null)
        {
            transform.LookAt(target);
        }
    }
    
    void OnGUI()
    {
        if (Application.isPlaying)
        {
            GUILayout.BeginArea(new Rect(10, 10, 350, 250));
            GUILayout.Label("Camera Controls", GUI.skin.box);
            GUILayout.Label("WASD - Move camera");
            GUILayout.Label("Q/E - Move up/down");
            GUILayout.Label("Right Mouse - Rotate camera");
            GUILayout.Label("Mouse Scroll - Zoom in/out");
            GUILayout.Space(10);
            GUILayout.Label("Movement Mode: Baldur's Gate Style");
            GUILayout.Label($"Position: {transform.position}");
            GUILayout.Label($"Rotation: {transform.eulerAngles}");
            GUILayout.Space(10);
            GUILayout.Label("Press R to reset camera");
            GUILayout.Label("Press T to reset rotation");
            GUILayout.EndArea();
        }
    }
}
