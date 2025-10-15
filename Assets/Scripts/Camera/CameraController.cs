using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Simple free-flying camera controller.
/// WASD to move, mouse to look around, Q/E to go up/down.
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Movement speed")]
    public float moveSpeed = 10f;
    
    [Tooltip("Mouse sensitivity for looking around")]
    public float mouseSensitivity = 2f;
    
    [Tooltip("Vertical movement speed")]
    public float verticalSpeed = 5f;
    
    private float mouseX;
    private float mouseY;
    private bool isRightClicking = false;
    
    void Start()
    {
        // Lock cursor when right-clicking
        Cursor.lockState = CursorLockMode.None;
    }
    
    void Update()
    {
        HandleInput();
    }
    
    void HandleInput()
    {
        // Right-click to look around
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            isRightClicking = true;
            Cursor.lockState = CursorLockMode.Locked;
        }
        
        if (Mouse.current.rightButton.wasReleasedThisFrame)
        {
            isRightClicking = false;
            Cursor.lockState = CursorLockMode.None;
        }
        
        // Mouse look when right-clicking
        if (isRightClicking)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            mouseX += mouseDelta.x * mouseSensitivity * 0.01f;
            mouseY -= mouseDelta.y * mouseSensitivity * 0.01f;
            mouseY = Mathf.Clamp(mouseY, -80f, 80f);
            
            // Apply rotation
            transform.rotation = Quaternion.Euler(mouseY, mouseX, 0);
        }
        
        // Movement
        Vector3 moveDirection = Vector3.zero;
        
        // WASD movement
        if (Keyboard.current.wKey.isPressed)
            moveDirection += transform.forward;
        if (Keyboard.current.sKey.isPressed)
            moveDirection -= transform.forward;
        if (Keyboard.current.aKey.isPressed)
            moveDirection -= transform.right;
        if (Keyboard.current.dKey.isPressed)
            moveDirection += transform.right;
        
        // Q and E for vertical movement
        if (Keyboard.current.qKey.isPressed)
            moveDirection += Vector3.up;
        if (Keyboard.current.eKey.isPressed)
            moveDirection -= Vector3.up;
        
        // Apply movement
        if (moveDirection != Vector3.zero)
        {
            moveDirection.Normalize();
            transform.position += moveDirection * moveSpeed * Time.deltaTime;
        }
    }
}