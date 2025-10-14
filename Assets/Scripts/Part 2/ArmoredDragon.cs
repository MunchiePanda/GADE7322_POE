using UnityEngine;

/// <summary>
/// Armored Dragon - Slow, high health, moderate damage.
/// A heavily armored dragon that takes time to kill but doesn't deal much damage.
/// </summary>
public class ArmoredDragon : Enemy
{
    [Header("Armor Settings")]
    [Tooltip("Damage reduction from armor (0.5 = 50% damage reduction)")]
    [Range(0f, 0.8f)]
    public float armorReduction = 0.6f;
    
    [Tooltip("Visual effect when armor is hit")]
    public GameObject armorHitEffectPrefab;
    
    [Tooltip("Particle system for armor sparks")]
    public ParticleSystem armorSparks;
    
    private float originalMaxHealth;

    protected override void Start()
    {
        base.Start();
        // Armored Dragon characteristics
        moveSpeed = 1.2f;         // Very slow
        maxHealth = 80f;           // Very high health
        currentHealth = maxHealth;
        attackDamage = 3f;        // Low damage
        attackIntervalSeconds = 2.5f; // Slow attack speed
        originalMaxHealth = maxHealth;
        
        // Visual setup - Make it significantly bigger for more importance
        transform.localScale *= 1.8f; // Much larger than regular enemies (was 1.3f)
        
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            // Make it more distinct with a darker, more metallic color
            renderer.material.color = new Color(0.3f, 0.3f, 0.4f, 1f); // Dark metallic blue-gray
        }
        else
        {
            // Add a renderer if none exists
            renderer = gameObject.AddComponent<MeshRenderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.3f, 0.3f, 0.4f, 1f); // Dark metallic blue-gray
            renderer.material = mat;
        }
        
        // Add armor visual effect
        AddArmorVisuals();
        
        // Add imposing visual effects for importance
        AddImposingVisuals();
        
        // Add dramatic spawn effect
        StartCoroutine(DramaticSpawnEffect());
    }

    public override void TakeDamage(float amount)
    {
        // Apply armor reduction
        float actualDamage = amount * (1f - armorReduction);
        
        Debug.Log($"Armored Dragon taking {amount} damage, reduced to {actualDamage} by armor");
        
        // Play armor hit effect
        PlayArmorHitEffect();
        
        // Apply the reduced damage
        currentHealth -= actualDamage;
        
        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            Die();
        }
    }

    void PlayArmorHitEffect()
    {
        // Play sparks effect
        if (armorSparks != null)
        {
            armorSparks.Play();
        }
        
        // Instantiate hit effect
        if (armorHitEffectPrefab != null)
        {
            GameObject hitEffect = Instantiate(armorHitEffectPrefab, transform.position, Quaternion.identity);
            Destroy(hitEffect, 1f);
        }
    }

    void AddArmorVisuals()
    {
        // Add armor plates as child objects or modify material
        // This could be done with additional GameObjects or material changes
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            // Make it look more metallic/armored
            renderer.material.color = new Color(0.6f, 0.6f, 0.6f, 1f); // Dark gray
        }
    }

    // Override to show armor status
    protected void Update()
    {
        if (currentHealth <= 0f) return;

        // Show armor status in debug
        if (Time.frameCount % 60 == 0) // Every second
        {
            float armorPercentage = (currentHealth / maxHealth) * 100f;
            if (armorPercentage < 50f)
            {
                Debug.Log($"Armored Dragon armor at {armorPercentage:F1}%");
            }
        }

        // Call the base enemy behavior
        if (currentHealth <= 0f) return;

        // If a defender is in range, attack it
        AcquireDefenderIfAny();
        if (currentDefenderTarget != null)
        {
            TryAttackDefender();
            return;
        }

        // Otherwise move along the path towards the tower
        FollowPathTowardsTower();
    }

    // Override to give more resources when killed (harder to kill)
    protected override void Die()
    {
        Debug.Log("Armored Dragon defeated!");
        
        // Play destruction effect if available
        // Note: ExplosionEffect script was removed - add particle effects here if needed

        if (gameManager != null)
        {
            // Give bonus resources for killing armored dragon
            int baseReward = Random.Range(minResourceRewardOnDeath, maxResourceRewardOnDeath + 1);
            int bonusReward = Mathf.RoundToInt(baseReward * 1.5f); // 50% bonus
            gameManager.AddResources(bonusReward);
            
            EnemySpawner spawner = FindFirstObjectByType<EnemySpawner>();
            if (spawner != null)
            {
                spawner.OnEnemyDeath(gameObject);
            }
        }

        Debug.Log($"Destroying Armored Dragon: {gameObject.name}");
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Adds imposing visual effects to make the dragon look more important
    /// </summary>
    void AddImposingVisuals()
    {
        // Add a subtle glow effect by creating a child object with a light
        GameObject glowObject = new GameObject("ArmorGlow");
        glowObject.transform.SetParent(transform);
        glowObject.transform.localPosition = Vector3.zero;
        glowObject.transform.localScale = Vector3.one * 1.2f; // Slightly larger than the dragon
        
        // Add a point light for the glow effect
        Light glowLight = glowObject.AddComponent<Light>();
        glowLight.type = LightType.Point;
        glowLight.color = new Color(0.2f, 0.3f, 0.8f, 1f); // Blue glow
        glowLight.intensity = 0.5f;
        glowLight.range = 8f;
        
        // Add a subtle pulsing effect
        StartCoroutine(PulseGlow(glowLight));
    }
    
    /// <summary>
    /// Creates a subtle pulsing glow effect
    /// </summary>
    System.Collections.IEnumerator PulseGlow(Light glowLight)
    {
        float baseIntensity = glowLight.intensity;
        float time = 0f;
        
        while (glowLight != null)
        {
            time += Time.deltaTime;
            glowLight.intensity = baseIntensity + Mathf.Sin(time * 2f) * 0.2f; // Subtle pulsing
            yield return null;
        }
    }
    
    /// <summary>
    /// Creates a dramatic spawn effect for the armored dragon
    /// </summary>
    System.Collections.IEnumerator DramaticSpawnEffect()
    {
        // Start with the dragon slightly transparent and smaller
        Vector3 originalScale = transform.localScale;
        transform.localScale = originalScale * 0.5f;
        
        // Make it transparent initially
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            Color originalColor = renderer.material.color;
            Color transparentColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0.3f);
            renderer.material.color = transparentColor;
        }
        
        // Animate the dragon growing and becoming opaque
        float duration = 1.5f; // 1.5 seconds for dramatic effect
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            
            // Scale up from 0.5x to full size
            transform.localScale = Vector3.Lerp(originalScale * 0.5f, originalScale, progress);
            
            // Fade in from transparent to opaque
            if (renderer != null)
            {
                Color currentColor = renderer.material.color;
                currentColor.a = Mathf.Lerp(0.3f, 1f, progress);
                renderer.material.color = currentColor;
            }
            
            yield return null;
        }
        
        // Ensure final values are correct
        transform.localScale = originalScale;
        if (renderer != null)
        {
            Color finalColor = renderer.material.color;
            finalColor.a = 1f;
            renderer.material.color = finalColor;
        }
        
        Debug.Log("Armored Dragon has fully materialized with imposing presence!");
    }
}
