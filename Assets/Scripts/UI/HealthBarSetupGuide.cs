using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Helper script to set up health bars for units
/// This script can be attached to a unit to automatically set up its health bar
/// </summary>
public class HealthBarSetupGuide : MonoBehaviour
{
    [Header("Setup Instructions")]
    [TextArea(5, 10)]
    public string setupInstructions = 
        "HEALTH BAR SETUP GUIDE:\n\n" +
        "1. Create a Canvas (UI > Canvas)\n" +
        "2. Set Canvas Render Mode to 'World Space'\n" +
        "3. Add a Slider as child of Canvas\n" +
        "4. Position Canvas above the unit\n" +
        "5. Add HealthBarController script to Canvas\n" +
        "6. Assign references in HealthBarController\n" +
        "7. Assign HealthBarController to unit's healthBarController field\n\n" +
        "The health bar will automatically face the camera and follow the unit!";

    [Header("Quick Setup")]
    [Tooltip("Click this button to create a health bar for this unit")]
    public bool createHealthBar = false;
    
    [Tooltip("Height offset above the unit")]
    public float heightOffset = 2f;
    
    [Tooltip("Health bar size")]
    public Vector2 healthBarSize = new Vector2(2f, 0.2f);

    void Update()
    {
        // Disabled automatic creation to prevent serialization issues
        // Use the button in the inspector instead
    }
    
    /// <summary>
    /// Creates a health bar setup for this unit
    /// </summary>
    public void CreateHealthBarForUnit()
    {
        // Create Canvas
        GameObject canvasObj = new GameObject("HealthBarCanvas");
        canvasObj.transform.SetParent(transform);
        canvasObj.transform.localPosition = new Vector3(0, heightOffset, 0);
        
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;
        
        // Create Slider
        GameObject sliderObj = new GameObject("HealthBarSlider");
        sliderObj.transform.SetParent(canvasObj.transform);
        sliderObj.transform.localPosition = Vector3.zero;
        sliderObj.transform.localScale = Vector3.one;
        
        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;
        
        // Create Slider Background
        GameObject backgroundObj = new GameObject("Background");
        backgroundObj.transform.SetParent(sliderObj.transform);
        backgroundObj.transform.localPosition = Vector3.zero;
        backgroundObj.transform.localScale = Vector3.one;
        
        Image backgroundImage = backgroundObj.AddComponent<Image>();
        backgroundImage.color = Color.red;
        RectTransform backgroundRect = backgroundImage.GetComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.sizeDelta = Vector2.zero;
        backgroundRect.anchoredPosition = Vector2.zero;
        
        // Create Slider Fill
        GameObject fillAreaObj = new GameObject("Fill Area");
        fillAreaObj.transform.SetParent(sliderObj.transform);
        fillAreaObj.transform.localPosition = Vector3.zero;
        fillAreaObj.transform.localScale = Vector3.one;
        
        RectTransform fillAreaRect = fillAreaObj.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = Vector2.zero;
        fillAreaRect.anchoredPosition = Vector2.zero;
        
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillAreaObj.transform);
        fillObj.transform.localPosition = Vector3.zero;
        fillObj.transform.localScale = Vector3.one;
        
        Image fillImage = fillObj.AddComponent<Image>();
        fillImage.color = Color.green;
        RectTransform fillRect = fillImage.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        fillRect.anchoredPosition = Vector2.zero;
        
        // Set up slider
        slider.fillRect = fillRect;
        
        // Set canvas size
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = healthBarSize;
        
        // Add HealthBarController
        HealthBarController healthBarController = canvasObj.AddComponent<HealthBarController>();
        healthBarController.healthBarCanvas = canvas;
        healthBarController.healthBarSlider = slider;
        healthBarController.targetUnit = transform;
        healthBarController.worldOffset = new Vector3(0, heightOffset, 0);
        
        // Assign to unit's health bar controller
        Defender defender = GetComponent<Defender>();
        if (defender != null)
        {
            defender.healthBarController = healthBarController;
        }
        
        Enemy enemy = GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.healthBarController = healthBarController;
        }
        
        Debug.Log($"Health bar created for {gameObject.name}!");
    }
    
    void OnGUI()
    {
        if (Application.isPlaying) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 400, 200));
        GUILayout.Label("Health Bar Setup Guide", GUI.skin.box);
        GUILayout.Label(setupInstructions);
        GUILayout.EndArea();
    }
}
