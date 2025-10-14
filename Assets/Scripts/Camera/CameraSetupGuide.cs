using UnityEngine;

/// <summary>
/// Setup guide for the scrolling camera system
/// </summary>
public class CameraSetupGuide : MonoBehaviour
{
    [Header("Camera Setup Instructions")]
    [TextArea(8, 10)]
    public string setupInstructions = 
        "CAMERA SETUP GUIDE:\n\n" +
        "1. Add ScrollingCamera script to Main Camera\n" +
        "2. Assign the tower transform in the inspector\n" +
        "3. Adjust movement speed and bounds as needed\n" +
        "4. Camera will start above the tower automatically\n\n" +
        "CONTROLS:\n" +
        "• WASD - Move camera horizontally\n" +
        "• Q/E - Move camera up/down\n" +
        "• Left Shift - Move faster\n" +
        "• Camera starts at tower position (77, 55, 71)\n" +
        "• Smooth movement with boundary limits";

    [Header("Quick Setup")]
    [Tooltip("Click to setup the camera automatically")]
    public bool setupCamera = false;
    
    [Tooltip("The tower transform to start above")]
    public Transform towerTransform;
    
    void Update()
    {
        if (setupCamera)
        {
            setupCamera = false;
            SetupScrollingCamera();
        }
    }
    
    /// <summary>
    /// Automatically setup the scrolling camera
    /// </summary>
    void SetupScrollingCamera()
    {
        // Find the main camera
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindFirstObjectByType<Camera>();
        }
        
        if (mainCamera == null)
        {
            Debug.LogError("No camera found! Please ensure there's a camera in the scene.");
            return;
        }
        
        // Add the scrolling camera script
        ScrollingCamera scrollingCamera = mainCamera.GetComponent<ScrollingCamera>();
        if (scrollingCamera == null)
        {
            scrollingCamera = mainCamera.gameObject.AddComponent<ScrollingCamera>();
        }
        
        // Set the tower transform
        if (towerTransform != null)
        {
            scrollingCamera.towerTransform = towerTransform;
        }
        else
        {
            // Try to find a tower by name
            GameObject tower = GameObject.Find("Tower") ?? GameObject.Find("MainTower") ?? GameObject.Find("Base");
            if (tower != null)
            {
                scrollingCamera.towerTransform = tower.transform;
            }
        }
        
        // Set initial position based on the image
        mainCamera.transform.position = new Vector3(77f, 55f, 71f);
        mainCamera.transform.rotation = Quaternion.Euler(81.979f, 0f, 0f);
        
        Debug.Log("Scrolling camera setup complete!");
        Debug.Log("Use WASD to move, Q/E for up/down, Left Shift for speed boost");
    }
    
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 400, 300));
        GUILayout.Label("Camera Setup Guide", GUI.skin.box);
        GUILayout.Label(setupInstructions);
        
        if (GUILayout.Button("Setup Scrolling Camera"))
        {
            SetupScrollingCamera();
        }
        
        GUILayout.EndArea();
    }
}
