using UnityEngine;

/// <summary>
/// Setup guide for the camera controller
/// </summary>
public class CameraSetupGuide : MonoBehaviour
{
    [Header("Setup Instructions")]
    [TextArea(10, 15)]
    public string setupInstructions = 
        "FREE-FLYING CAMERA CONTROLLER SETUP:\n\n" +
        "1. Select your Main Camera in the scene\n" +
        "2. Add the CameraController script to it\n" +
        "3. Configure the settings:\n" +
        "   - Move Speed: 10 (adjust as needed)\n" +
        "   - Mouse Sensitivity: 2 (adjust as needed)\n" +
        "   - Vertical Speed: 5 (adjust as needed)\n\n" +
        "CONTROLS:\n" +
        "• WASD - Move forward/back/left/right\n" +
        "• Q/E - Move up/down\n" +
        "• Right Mouse + Drag - Look around\n\n" +
        "The camera is completely free-flying!";

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
        
        // Configure settings for the free-flying camera
        controller.moveSpeed = 10f;
        controller.mouseSensitivity = 2f;
        controller.verticalSpeed = 5f;
        
        Debug.Log("Main Camera configured with Simple CameraController!");
    }
    
    /// <summary>
    /// Resets the main camera to default position above tower
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
            // Reset camera to default position
            mainCamera.transform.position = new Vector3(0, 10, 0);
            mainCamera.transform.rotation = Quaternion.identity;
            Debug.Log("Camera reset to default position!");
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
