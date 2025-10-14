using UnityEngine;

/// <summary>
/// Setup guide for the camera controller
/// </summary>
public class CameraSetupGuide : MonoBehaviour
{
    [Header("Setup Instructions")]
    [TextArea(10, 15)]
    public string setupInstructions = 
        "CAMERA CONTROLLER SETUP:\n\n" +
        "1. Select your Main Camera in the scene\n" +
        "2. Add the CameraController script to it\n" +
        "3. Configure the settings:\n" +
        "   - Move Speed: 10 (adjust as needed)\n" +
        "   - Rotation Speed: 100 (adjust as needed)\n" +
        "   - Zoom Speed: 5 (adjust as needed)\n" +
        "4. Set movement constraints (min/max X, Y, Z)\n" +
        "5. Enable 'Set Initial Position' to match the image\n\n" +
        "CONTROLS:\n" +
        "• WASD - Move camera around\n" +
        "• Q/E - Move camera up/down\n" +
        "• Right Mouse + Drag - Rotate camera\n" +
        "• Mouse Scroll - Zoom in/out\n\n" +
        "The camera will start at the position shown in the image!";

    [Header("Quick Setup")]
    [Tooltip("Click to setup the main camera")]
    public bool setupMainCamera = false;
    
    [Tooltip("Click to reset camera to initial position")]
    public bool resetCamera = false;
    
    void Update()
    {
        if (setupMainCamera)
        {
            setupMainCamera = false;
            SetupMainCamera();
        }
        
        if (resetCamera)
        {
            resetCamera = false;
            ResetMainCamera();
        }
    }
    
    /// <summary>
    /// Sets up the main camera with the controller
    /// </summary>
    void SetupMainCamera()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("No Main Camera found!");
            return;
        }
        
        // Add CameraController if it doesn't exist
        CameraController controller = mainCamera.GetComponent<CameraController>();
        if (controller == null)
        {
            controller = mainCamera.gameObject.AddComponent<CameraController>();
            Debug.Log("Added CameraController to Main Camera");
        }
        else
        {
            Debug.Log("CameraController already exists on Main Camera");
        }
        
        // Configure settings to match the image
        controller.setInitialPosition = true;
        controller.initialPosition = new Vector3(77f, 55f, 71f);
        controller.initialRotation = new Vector3(81.979f, 0f, 0f);
        controller.moveSpeed = 10f;
        controller.rotationSpeed = 100f;
        controller.zoomSpeed = 5f;
        
        // Set reasonable movement bounds
        controller.minX = -100f;
        controller.maxX = 100f;
        controller.minY = 10f;
        controller.maxY = 100f;
        controller.minZ = -100f;
        controller.maxZ = 100f;
        
        Debug.Log("Main Camera configured with CameraController!");
    }
    
    /// <summary>
    /// Resets the main camera to initial position
    /// </summary>
    void ResetMainCamera()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("No Main Camera found!");
            return;
        }
        
        CameraController controller = mainCamera.GetComponent<CameraController>();
        if (controller != null)
        {
            controller.ResetCamera();
            Debug.Log("Camera reset to initial position!");
        }
        else
        {
            Debug.LogError("No CameraController found on Main Camera!");
        }
    }
    
    void OnGUI()
    {
        if (Application.isPlaying) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 400, 300));
        GUILayout.Label("Camera Controller Setup", GUI.skin.box);
        GUILayout.Label(setupInstructions);
        GUILayout.EndArea();
    }
}
