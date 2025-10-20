using UnityEngine;
using UnityEngine.UI;
using GADE7322_POE.Core;

public class WorldHealthBar : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image fillImage;
    
    [Header("Colors")]
    [SerializeField] private Color fullHealthColor = Color.green;
    [SerializeField] private Color midHealthColor = Color.yellow;
    [SerializeField] private Color lowHealthColor = Color.red;
    
    [Header("Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0, 2f, 0);
    [SerializeField] private bool hideWhenFull = false;
    [SerializeField] private float canvasScale = 0.01f;
    
    private Transform targetTransform;
    private Health healthComponent;
    private Enemy enemyComponent;
    private Defender defenderComponent;
    private Camera mainCamera;
    private CanvasGroup canvasGroup;
    
    private float maxHealth;
    private float currentHealth;
    
    void Awake()
    {
        if (canvas == null)
        {
            canvas = GetComponentInChildren<Canvas>();
        }
        
        if (healthSlider == null)
        {
            healthSlider = GetComponentInChildren<Slider>();
        }
        
        if (fillImage == null && healthSlider != null)
        {
            fillImage = healthSlider.fillRect?.GetComponent<Image>();
        }
        
        canvasGroup = canvas.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = canvas.gameObject.AddComponent<CanvasGroup>();
        }
        
        mainCamera = Camera.main;
    }
    
    public void Initialize(Transform target)
    {
        targetTransform = target;
        
        healthComponent = target.GetComponent<Health>();
        enemyComponent = target.GetComponent<Enemy>();
        defenderComponent = target.GetComponent<Defender>();
        
        if (healthComponent != null)
        {
            maxHealth = healthComponent.MaxHealth;
            currentHealth = healthComponent.CurrentHealth;
            healthComponent.OnTakeDamage.AddListener(UpdateHealthFromComponent);
            healthComponent.OnDeath.AddListener(OnTargetDeath);
        }
        else if (enemyComponent != null)
        {
            maxHealth = enemyComponent.GetMaxHealth();
            currentHealth = enemyComponent.GetCurrentHealth();
        }
        else if (defenderComponent != null)
        {
            maxHealth = 100f;
            currentHealth = maxHealth;
        }
        
        SetupCanvas();
        UpdateHealthBar();
    }
    
    void SetupCanvas()
    {
        if (canvas == null) return;
        
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = mainCamera;
        
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(100, 20);
        canvasRect.localScale = Vector3.one * canvasScale;
    }
    
    void Update()
    {
        if (targetTransform == null)
        {
            Destroy(gameObject);
            return;
        }
        
        UpdatePosition();
        FaceCamera();
        
        if (healthComponent == null && (enemyComponent != null || defenderComponent != null))
        {
            UpdateHealthManually();
        }
    }
    
    void UpdatePosition()
    {
        if (targetTransform != null && canvas != null)
        {
            canvas.transform.position = targetTransform.position + offset;
        }
    }
    
    void FaceCamera()
    {
        if (mainCamera != null && canvas != null)
        {
            canvas.transform.rotation = Quaternion.LookRotation(canvas.transform.position - mainCamera.transform.position);
        }
    }
    
    void UpdateHealthFromComponent()
    {
        if (healthComponent != null)
        {
            currentHealth = healthComponent.CurrentHealth;
            maxHealth = healthComponent.MaxHealth;
            UpdateHealthBar();
        }
    }
    
    void UpdateHealthManually()
    {
        if (enemyComponent != null)
        {
            currentHealth = enemyComponent.GetCurrentHealth();
            maxHealth = enemyComponent.GetMaxHealth();
        }
        else if (defenderComponent != null)
        {
            currentHealth = defenderComponent.IsAlive() ? 100f : 0f;
        }
        
        UpdateHealthBar();
    }
    
    void UpdateHealthBar()
    {
        if (healthSlider == null) return;
        
        float healthPercent = maxHealth > 0 ? currentHealth / maxHealth : 0;
        healthSlider.value = healthPercent;
        
        if (fillImage != null)
        {
            if (healthPercent > 0.6f)
                fillImage.color = Color.Lerp(midHealthColor, fullHealthColor, (healthPercent - 0.6f) / 0.4f);
            else if (healthPercent > 0.3f)
                fillImage.color = Color.Lerp(lowHealthColor, midHealthColor, (healthPercent - 0.3f) / 0.3f);
            else
                fillImage.color = lowHealthColor;
        }
        
        if (canvasGroup != null)
        {
            if (hideWhenFull && healthPercent >= 1f)
            {
                canvasGroup.alpha = 0f;
            }
            else
            {
                canvasGroup.alpha = 1f;
            }
        }
    }
    
    void OnTargetDeath()
    {
        Destroy(gameObject);
    }
    
    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
    }
    
    public void SetHideWhenFull(bool hide)
    {
        hideWhenFull = hide;
    }
}
