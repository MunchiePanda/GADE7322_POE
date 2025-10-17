using UnityEngine;

/// <summary>
/// Base class for all environmental hazards with common functionality.
/// Handles basic hazard behavior, visual effects, and lifecycle management.
/// </summary>
public abstract class EnvironmentalHazard : MonoBehaviour
{
    [Header("Hazard Settings")]
    [Tooltip("Radius of the hazard effect")]
    public float effectRadius = 5f;
    
    [Tooltip("Duration of the hazard in seconds (-1 for permanent)")]
    public float duration = -1f;
    
    [Tooltip("Intensity multiplier for hazard effects")]
    public float intensity = 1f;
    
    [Header("Visual Effects")]
    [Tooltip("Material with HazardDistortion shader")]
    public Material hazardMaterial;
    
    [Tooltip("Particle system for ambient effects")]
    public ParticleSystem ambientParticles;
    
    [Tooltip("Hazard type for shader effects")]
    public HazardType hazardType = HazardType.Lava;
    
    protected float spawnTime;
    protected bool isActive = true;
    protected Collider hazardCollider;
    
    protected virtual void Start()
    {
        spawnTime = Time.time;
        hazardCollider = GetComponent<Collider>();
        
        if (hazardCollider == null)
        {
            // Add trigger collider if none exists
            hazardCollider = gameObject.AddComponent<SphereCollider>();
            hazardCollider.isTrigger = true;
        }
        
        SetupVisualEffects();
        ApplyHazardMaterial();
    }
    
    protected virtual void Update()
    {
        if (!isActive) return;
        
        // Check duration
        if (duration > 0 && Time.time - spawnTime >= duration)
        {
            DestroyHazard();
            return;
        }
        
        // Update hazard effects
        UpdateHazardEffects();
    }
    
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;
        
        // Apply hazard effects to entering units
        ApplyHazardEffect(other.gameObject, true);
    }
    
    protected virtual void OnTriggerExit(Collider other)
    {
        if (!isActive) return;
        
        // Remove hazard effects from exiting units
        ApplyHazardEffect(other.gameObject, false);
    }
    
    protected virtual void OnTriggerStay(Collider other)
    {
        if (!isActive) return;
        
        // Apply continuous hazard effects
        ApplyHazardEffect(other.gameObject, true);
    }
    
    /// <summary>
    /// Applies the specific hazard effect to a game object
    /// </summary>
    /// <param name="target">Game object to affect</param>
    /// <param name="isEntering">True if entering hazard, false if exiting</param>
    protected abstract void ApplyHazardEffect(GameObject target, bool isEntering);
    
    /// <summary>
    /// Updates hazard-specific effects each frame
    /// </summary>
    protected abstract void UpdateHazardEffects();
    
    /// <summary>
    /// Sets up visual effects for the hazard
    /// </summary>
    protected virtual void SetupVisualEffects()
    {
        // Setup particle system
        if (ambientParticles == null)
        {
            GameObject particleObj = new GameObject("HazardParticles");
            particleObj.transform.SetParent(transform);
            particleObj.transform.localPosition = Vector3.zero;
            
            ambientParticles = particleObj.AddComponent<ParticleSystem>();
            SetupParticleSystem();
        }
    }
    
    /// <summary>
    /// Sets up the particle system for this hazard type
    /// </summary>
    protected abstract void SetupParticleSystem();
    
    /// <summary>
    /// Applies the hazard material with distortion shader
    /// </summary>
    protected virtual void ApplyHazardMaterial()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null && hazardMaterial != null)
        {
            // Create instance of hazard material
            Material instanceMaterial = new Material(hazardMaterial);
            instanceMaterial.SetFloat("_HazardType", (float)hazardType);
            instanceMaterial.SetFloat("_DistortionAmount", intensity * 0.5f);
            renderer.material = instanceMaterial;
        }
    }
    
    /// <summary>
    /// Destroys the hazard and cleans up effects
    /// </summary>
    protected virtual void DestroyHazard()
    {
        isActive = false;
        
        // Stop particles
        if (ambientParticles != null)
        {
            ambientParticles.Stop();
        }
        
        // Fade out effect
        StartCoroutine(FadeOutAndDestroy());
    }
    
    private System.Collections.IEnumerator FadeOutAndDestroy()
    {
        float fadeTime = 2f;
        float elapsed = 0f;
        
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - (elapsed / fadeTime);
            
            // Fade material
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                Color color = renderer.material.color;
                color.a = alpha;
                renderer.material.color = color;
            }
            
            yield return null;
        }
        
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Gets the current intensity of the hazard
    /// </summary>
    public float GetIntensity()
    {
        return intensity;
    }
    
    /// <summary>
    /// Sets the intensity of the hazard
    /// </summary>
    public void SetIntensity(float newIntensity)
    {
        intensity = newIntensity;
        
        // Update material
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.SetFloat("_DistortionAmount", intensity * 0.5f);
        }
    }
}

public enum HazardType
{
    Lava = 0,
    Ice = 1,
    Wind = 2
}
