using UnityEngine;

/// <summary>
/// Frost Tower defender with AoE slow effect and large area coverage.
/// Shoots slowly but affects multiple enemies with movement speed reduction.
/// </summary>
public class FrostTowerDefender : Defender
{
    [Header("Frost Tower Settings")]
    [Tooltip("Radius of the frost AoE attack")]
    public float frostRadius = 8f;
    
    [Tooltip("Duration of the slow effect in seconds")]
    public float slowDuration = 3f;
    
    [Tooltip("Speed reduction multiplier (0.5 = 50% speed, 0.3 = 70% speed reduction)")]
    [Range(0.1f, 0.9f)]
    public float slowMultiplier = 0.4f;
    
    [Tooltip("Visual effect for frost attack")]
    public GameObject frostEffectPrefab;
    
    [Tooltip("Particle system for frost visual")]
    public ParticleSystem frostParticles;
    
    [Header("Visual Feedback")]
    [Tooltip("Line renderer to show frost beam")]
    public LineRenderer frostLine;
    
    [Tooltip("Projectile visual for frost attack")]
    public GameObject frostProjectilePrefab;
    
    [Tooltip("Color of frost effects")]
    public Color frostColor = Color.cyan;

    protected override void Start()
    {
        base.Start();
        // Frost tower characteristics
        attackRange = 12f;           // Long range
        attackDamage = 8f;          // Moderate damage
        attackIntervalSeconds = 3f; // Slow attack speed
        
        // Visual setup
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.cyan;
        }
        else
        {
            // Add a renderer if none exists
            renderer = gameObject.AddComponent<MeshRenderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = Color.cyan;
            renderer.material = mat;
        }
    }

    void Update()
    {
        if (!IsAlive()) return;

        AcquireEnemyIfAny();
        TryFrostAttack();
        
        // Debug current state
        if (currentEnemyTarget != null)
        {
            // Debug.Log($"Frost Tower: Aiming at {currentEnemyTarget.name} (Distance: {Vector3.Distance(transform.position, currentEnemyTarget.transform.position):F1})");
        }
    }

    void TryFrostAttack()
    {
        if (currentEnemyTarget == null) 
        {
            // Debug.Log("Frost Tower: No target in range");
            return;
        }
        
        float time = Time.time;
        float timeSinceLastAttack = time - lastAttackTime;
        
        if (timeSinceLastAttack >= attackIntervalSeconds)
        {
            // Debug.Log($"Frost Tower: ATTACKING! Target: {currentEnemyTarget.name}, Cooldown: {timeSinceLastAttack:F1}s");
            lastAttackTime = time;
            PerformFrostAttack();
        }
        else
        {
            // Debug.Log($"Frost Tower: On cooldown ({timeSinceLastAttack:F1}s / {attackIntervalSeconds}s)");
        }
    }

     void PerformFrostAttack()
     {
         // Debug.Log($"Frost Tower: Performing AoE attack with radius {frostRadius}");
         
         // This is the AoE slow system - it hits multiple enemies at once
         Collider[] enemiesInRange = Physics.OverlapSphere(transform.position, frostRadius);
         int enemiesHit = 0;
         
         // Debug.Log($"Frost Tower: Found {enemiesInRange.Length} colliders in range");
         
         foreach (Collider enemyCollider in enemiesInRange)
         {
             Enemy enemy = enemyCollider.GetComponent<Enemy>();
             if (enemy != null)
             {
                 enemiesHit++;
                 float distance = Vector3.Distance(transform.position, enemy.transform.position);
                 // Debug.Log($"Frost Tower: HIT {enemy.name} at distance {distance:F1} for {attackDamage} damage");
                 
                 // Deal damage
                 enemy.TakeDamage(attackDamage);
                 
                 // Apply slow effect - this makes enemies move slower
                 ApplySlowEffect(enemy);
                 
                 // Apply frost visual effect - players can see which enemies are slowed
                 ApplyFrostVisualEffect(enemy);
             }
         }
         
         // Debug.Log($"Frost Tower: Attack complete! Hit {enemiesHit} enemies");
         
         // Play visual effects
         PlayFrostEffects();
     }

     void ApplySlowEffect(Enemy enemy)
     {
         // Debug.Log($"Frost Tower: Applying slow effect to {enemy.name} for {slowDuration}s");
         // Add a slow effect component or use a coroutine
         StartCoroutine(SlowEnemyCoroutine(enemy));
         
         // Apply visual indicator for slowed enemy
         ApplySlowVisualEffect(enemy);
     }

    System.Collections.IEnumerator SlowEnemyCoroutine(Enemy enemy)
    {
        if (enemy == null) yield break;
        
        // Store original speed
        float originalSpeed = enemy.GetMoveSpeed();
        float slowedSpeed = originalSpeed * slowMultiplier;
        
        // Debug.Log($"Frost Tower: Slowing {enemy.name} from {originalSpeed:F1} to {slowedSpeed:F1} speed");
        
        // Apply slow
        enemy.SetMoveSpeed(slowedSpeed);
        
        // Wait for slow duration
        yield return new WaitForSeconds(slowDuration);
        
        // Restore original speed (only if enemy still exists)
        if (enemy != null)
        {
            enemy.SetMoveSpeed(originalSpeed);
            // Debug.Log($"Frost Tower: Restored {enemy.name} speed to {originalSpeed:F1}");
        }
    }

    void PlayFrostEffects()
    {
        // Play particle effect
        if (frostParticles != null)
        {
            frostParticles.Play();
        }
        
        // Instantiate frost effect at tower position
        if (frostEffectPrefab != null)
        {
            GameObject effect = Instantiate(frostEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 2f); // Clean up after 2 seconds
        }
        
        // Show frost beam to target
        if (currentEnemyTarget != null)
        {
            ShowFrostBeam(currentEnemyTarget.transform.position);
        }
        
        // Create frost projectile visual
        if (frostProjectilePrefab != null)
        {
            GameObject projectile = Instantiate(frostProjectilePrefab, transform.position, Quaternion.identity);
            Destroy(projectile, 1f);
        }
        
        // Simple visual feedback without prefabs
        CreateSimpleFrostEffect();
        
        // Debug visual feedback
        // Debug.Log($"Frost Tower attacking! Radius: {frostRadius}, Target: {currentEnemyTarget?.name}");
    }
    
    void ShowFrostBeam(Vector3 targetPosition)
    {
        if (frostLine != null)
        {
            frostLine.enabled = true;
            frostLine.positionCount = 2;
            frostLine.SetPosition(0, transform.position + Vector3.up * 0.5f);
            frostLine.SetPosition(1, targetPosition);
            frostLine.material.color = frostColor;
            
            // Hide beam after short time
            StartCoroutine(HideFrostBeam());
        }
    }
    
    System.Collections.IEnumerator HideFrostBeam()
    {
        yield return new WaitForSeconds(0.3f);
        if (frostLine != null)
        {
            frostLine.enabled = false;
        }
    }
    
    void CreateSimpleFrostEffect()
    {
        // Create a simple visual effect without requiring prefabs
        GameObject effect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        effect.transform.position = transform.position;
        effect.transform.localScale = Vector3.one * 0.5f;
        effect.name = "FrostEffect";
        
        // Make it cyan and semi-transparent
        Renderer renderer = effect.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(frostColor.r, frostColor.g, frostColor.b, 0.5f);
        mat.SetFloat("_Mode", 3); // Transparent mode
        renderer.material = mat;
        
        // Remove collider
        Destroy(effect.GetComponent<Collider>());
        
        // Destroy after short time
        Destroy(effect, 0.5f);
    }

     void ApplyFrostVisualEffect(Enemy enemy)
     {
         // Create a blue/cyan effect on the enemy
         StartCoroutine(FlashEnemyFrost(enemy));
     }
     
     System.Collections.IEnumerator FlashEnemyFrost(Enemy enemy)
     {
         if (enemy == null) yield break;
         
         // Get the enemy's renderer
         Renderer enemyRenderer = enemy.GetComponent<Renderer>();
         if (enemyRenderer == null) yield break;
         
         // Store original material
         Material originalMaterial = enemyRenderer.material;
         Color originalColor = originalMaterial.color;
         
         // Create frost material
         Material frostMaterial = new Material(Shader.Find("Standard"));
         frostMaterial.color = frostColor;
         frostMaterial.SetFloat("_Emission", 1.5f); // Bright cyan glow
         
         // Flash the enemy 2 times
         for (int i = 0; i < 2; i++)
         {
             // Flash cyan
             enemyRenderer.material = frostMaterial;
             yield return new WaitForSeconds(0.15f);
             
             // Flash back to original
             enemyRenderer.material = originalMaterial;
             yield return new WaitForSeconds(0.15f);
         }
         
         // Ensure we end with the original material
         if (enemyRenderer != null)
         {
             enemyRenderer.material = originalMaterial;
         }
     }
     
     void ApplySlowVisualEffect(Enemy enemy)
     {
         // Create a persistent blue tint to show the enemy is slowed
         StartCoroutine(ShowSlowedEnemy(enemy));
     }
     
     System.Collections.IEnumerator ShowSlowedEnemy(Enemy enemy)
     {
         if (enemy == null) yield break;
         
         // Get the enemy's renderer
         Renderer enemyRenderer = enemy.GetComponent<Renderer>();
         if (enemyRenderer == null) yield break;
         
         // Store original material
         Material originalMaterial = enemyRenderer.material;
         Color originalColor = originalMaterial.color;
         
         // Create slowed material (blue tint)
         Material slowedMaterial = new Material(Shader.Find("Standard"));
         slowedMaterial.color = new Color(0.3f, 0.6f, 1f, 1f); // Light blue tint
         slowedMaterial.SetFloat("_Emission", 0.3f); // Subtle glow
         
         // Apply slowed visual for the duration
         enemyRenderer.material = slowedMaterial;
         
         // Wait for slow duration
         yield return new WaitForSeconds(slowDuration);
         
         // Restore original material
         if (enemyRenderer != null)
         {
             enemyRenderer.material = originalMaterial;
         }
     }

     // Visual debugging
     void OnDrawGizmosSelected()
     {
         Gizmos.color = Color.cyan;
         Gizmos.DrawWireSphere(transform.position, frostRadius);
     }
}
