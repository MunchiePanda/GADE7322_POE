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
        
        // Visual setup
        transform.localScale *= 1.3f; // Larger than regular enemies
        
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.gray;
        }
        else
        {
            // Add a renderer if none exists
            renderer = gameObject.AddComponent<MeshRenderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = Color.gray;
            renderer.material = mat;
        }
        
        // Add armor visual effect
        AddArmorVisuals();
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
    protected void Die()
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

        Destroy(gameObject);
    }
}
