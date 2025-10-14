using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls a health bar that always faces the camera and appears above a unit
/// </summary>
public class HealthBarController : MonoBehaviour
{
    [Header("Health Bar Settings")]
    [Tooltip("The UI Canvas that contains the health bar")]
    public Canvas healthBarCanvas;
    
    [Tooltip("The slider component for the health bar")]
    public Slider healthBarSlider;
    
    [Tooltip("The unit this health bar is attached to")]
    public Transform targetUnit;
    
    [Tooltip("Offset above the unit (in world units)")]
    public Vector3 worldOffset = new Vector3(0, 2f, 0);
    
    [Tooltip("Camera to face (if null, uses main camera)")]
    public Camera targetCamera;
    
    [Header("Display Settings")]
    [Tooltip("Hide health bar when at full health")]
    public bool hideWhenFull = true;
    
    [Tooltip("Hide health bar when unit is dead")]
    public bool hideWhenDead = true;
    
    [Tooltip("Fade out time when hiding")]
    public float fadeOutTime = 0.5f;
    
    private CanvasGroup canvasGroup;
    private bool isVisible = true;
    private float fadeTimer = 0f;
    
    void Start()
    {
        // Get or create canvas group for fading
        canvasGroup = healthBarCanvas.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = healthBarCanvas.gameObject.AddComponent<CanvasGroup>();
        }
        
        // Set up canvas for world space
        healthBarCanvas.renderMode = RenderMode.WorldSpace;
        healthBarCanvas.worldCamera = targetCamera != null ? targetCamera : Camera.main;
        
        // Position the canvas above the unit
        UpdatePosition();
        
        // Initialize visibility
        if (hideWhenFull && healthBarSlider.value >= 1f)
        {
            SetVisible(false);
        }
    }
    
    void Update()
    {
        if (targetUnit == null) return;
        
        // Update position to stay above the unit
        UpdatePosition();
        
        // Face the camera
        FaceCamera();
        
        // Handle visibility based on health
        UpdateVisibility();
    }
    
    /// <summary>
    /// Updates the health bar position to stay above the unit
    /// </summary>
    void UpdatePosition()
    {
        if (targetUnit == null) return;
        
        // Position the canvas above the unit
        Vector3 worldPosition = targetUnit.position + worldOffset;
        healthBarCanvas.transform.position = worldPosition;
    }
    
    /// <summary>
    /// Makes the health bar always face the camera
    /// </summary>
    void FaceCamera()
    {
        Camera cam = targetCamera != null ? targetCamera : Camera.main;
        if (cam == null) return;
        
        // Make the health bar face the camera
        Vector3 lookDirection = cam.transform.position - healthBarCanvas.transform.position;
        lookDirection.y = 0; // Keep it level (don't tilt up/down)
        
        if (lookDirection != Vector3.zero)
        {
            healthBarCanvas.transform.rotation = Quaternion.LookRotation(lookDirection);
        }
    }
    
    /// <summary>
    /// Updates the health bar visibility based on health and settings
    /// </summary>
    void UpdateVisibility()
    {
        if (healthBarSlider == null) return;
        
        bool shouldBeVisible = true;
        
        // Hide when dead
        if (hideWhenDead && healthBarSlider.value <= 0f)
        {
            shouldBeVisible = false;
        }
        // Hide when full health
        else if (hideWhenFull && healthBarSlider.value >= 1f)
        {
            shouldBeVisible = false;
        }
        
        SetVisible(shouldBeVisible);
    }
    
    /// <summary>
    /// Sets the health bar visibility with smooth fading
    /// </summary>
    void SetVisible(bool visible)
    {
        if (isVisible == visible) return;
        
        isVisible = visible;
        fadeTimer = 0f;
    }
    
    void LateUpdate()
    {
        // Handle fading
        if (fadeTimer < fadeOutTime)
        {
            fadeTimer += Time.deltaTime;
            float alpha = isVisible ? 1f : Mathf.Lerp(1f, 0f, fadeTimer / fadeOutTime);
            canvasGroup.alpha = alpha;
        }
        else if (!isVisible)
        {
            canvasGroup.alpha = 0f;
        }
    }
    
    /// <summary>
    /// Updates the health bar value (called by the unit's health system)
    /// </summary>
    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        if (healthBarSlider == null) return;
        
        if (maxHealth > 0)
        {
            healthBarSlider.value = currentHealth / maxHealth;
        }
    }
    
    /// <summary>
    /// Sets the target unit for this health bar
    /// </summary>
    public void SetTargetUnit(Transform unit)
    {
        targetUnit = unit;
        if (unit != null)
        {
            UpdatePosition();
        }
    }
    
    /// <summary>
    /// Sets the target camera for this health bar
    /// </summary>
    public void SetTargetCamera(Camera cam)
    {
        targetCamera = cam;
        if (healthBarCanvas != null)
        {
            healthBarCanvas.worldCamera = cam;
        }
    }
}
