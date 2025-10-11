using UnityEngine;

/// <summary>
/// Manages critical hit system for all defenders.
/// Provides visual and audio feedback for critical hits.
/// </summary>
public class CriticalHitSystem : MonoBehaviour
{
    [Header("Critical Hit Settings")]
    [Tooltip("Base chance for critical hits (0.1 = 10%)")]
    public float baseCriticalChance = 0.1f;
    
    [Tooltip("Critical hit damage multiplier")]
    public float criticalMultiplier = 2.0f;
    
    [Tooltip("Critical chance bonus per wave")]
    public float criticalChanceBonus = 0.02f; // 2% bonus per wave
    
    [Header("Visual Feedback")]
    [Tooltip("Screen shake intensity for critical hits")]
    public float screenShakeIntensity = 0.2f;
    
    [Tooltip("Screen shake duration for critical hits")]
    public float screenShakeDuration = 0.3f;
    
    [Tooltip("Color for critical hit damage numbers")]
    public Color criticalHitColor = Color.red;
    
    [Tooltip("Color for normal hit damage numbers")]
    public Color normalHitColor = Color.white;
    
    private Camera mainCamera;
    private int currentWave = 1;
    
    void Start()
    {
        mainCamera = Camera.main;
    }
    
    /// <summary>
    /// Calculates if a hit should be critical based on current wave.
    /// </summary>
    public bool RollCriticalHit()
    {
        float totalChance = baseCriticalChance + (currentWave - 1) * criticalChanceBonus;
        return Random.Range(0f, 1f) < totalChance;
    }
    
    /// <summary>
    /// Calculates final damage with critical hit multiplier.
    /// </summary>
    public float CalculateDamage(float baseDamage, bool isCritical)
    {
        if (isCritical)
        {
            return baseDamage * criticalMultiplier;
        }
        return baseDamage;
    }
    
    /// <summary>
    /// Shows damage number with appropriate color and size.
    /// </summary>
    public void ShowDamageNumber(float damage, Vector3 position, bool isCritical)
    {
        // Create floating damage number
        GameObject damageText = new GameObject("DamageNumber");
        damageText.transform.position = position + Vector3.up * 2f;
        
        // Add TextMeshPro component
        var textMesh = damageText.AddComponent<TextMesh>();
        textMesh.text = Mathf.RoundToInt(damage).ToString();
        textMesh.color = isCritical ? criticalHitColor : normalHitColor;
        textMesh.fontSize = isCritical ? 2 : 1;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        
        // Make it face the camera
        damageText.transform.LookAt(mainCamera.transform);
        damageText.transform.Rotate(0, 180, 0);
        
        // Animate the damage number
        StartCoroutine(AnimateDamageNumber(damageText, isCritical));
    }
    
    /// <summary>
    /// Animates the damage number floating up and fading out.
    /// </summary>
    System.Collections.IEnumerator AnimateDamageNumber(GameObject damageText, bool isCritical)
    {
        Vector3 startPosition = damageText.transform.position;
        Vector3 endPosition = startPosition + Vector3.up * 3f;
        float duration = isCritical ? 1.5f : 1f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Move up
            damageText.transform.position = Vector3.Lerp(startPosition, endPosition, t);
            
            // Fade out
            TextMesh textMesh = damageText.GetComponent<TextMesh>();
            Color color = textMesh.color;
            color.a = 1f - t;
            textMesh.color = color;
            
            // Scale for critical hits
            if (isCritical)
            {
                float scale = 1f + Mathf.Sin(t * Mathf.PI) * 0.5f;
                damageText.transform.localScale = Vector3.one * scale;
            }
            
            yield return null;
        }
        
        Destroy(damageText);
    }
    
    /// <summary>
    /// Triggers screen shake for critical hits.
    /// </summary>
    public void TriggerScreenShake(bool isCritical)
    {
        if (isCritical && mainCamera != null)
        {
            StartCoroutine(ScreenShakeCoroutine());
        }
    }
    
    /// <summary>
    /// Screen shake coroutine for critical hits.
    /// </summary>
    System.Collections.IEnumerator ScreenShakeCoroutine()
    {
        Vector3 originalPosition = mainCamera.transform.position;
        float elapsed = 0f;
        
        while (elapsed < screenShakeDuration)
        {
            float x = Random.Range(-1f, 1f) * screenShakeIntensity;
            float y = Random.Range(-1f, 1f) * screenShakeIntensity;
            
            mainCamera.transform.position = originalPosition + new Vector3(x, y, 0);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        mainCamera.transform.position = originalPosition;
    }
    
    /// <summary>
    /// Updates the current wave for critical chance scaling.
    /// </summary>
    public void SetCurrentWave(int wave)
    {
        currentWave = wave;
    }
    
    /// <summary>
    /// Gets the current critical hit chance.
    /// </summary>
    public float GetCriticalChance()
    {
        return baseCriticalChance + (currentWave - 1) * criticalChanceBonus;
    }
}
