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

    protected override void Start()
    {
        base.Start();
        // Frost tower characteristics
        attackRange = 12f;           // Long range
        attackDamage = 8f;          // Moderate damage
        attackIntervalSeconds = 3f; // Slow attack speed
        
        // Visual setup
        GetComponent<Renderer>().material.color = Color.cyan;
    }

    void Update()
    {
        if (!IsAlive()) return;

        AcquireEnemyIfAny();
        TryFrostAttack();
    }

    void TryFrostAttack()
    {
        if (currentEnemyTarget == null) return;
        
        float time = Time.time;
        if (time - lastAttackTime >= attackIntervalSeconds)
        {
            lastAttackTime = time;
            PerformFrostAttack();
        }
    }

    void PerformFrostAttack()
    {
        // Find all enemies in the frost radius
        Collider[] enemiesInRange = Physics.OverlapSphere(transform.position, frostRadius);
        
        foreach (Collider enemyCollider in enemiesInRange)
        {
            Enemy enemy = enemyCollider.GetComponent<Enemy>();
            if (enemy != null)
            {
                // Deal damage
                enemy.TakeDamage(attackDamage);
                
                // Apply slow effect
                ApplySlowEffect(enemy);
            }
        }
        
        // Play visual effects
        PlayFrostEffects();
    }

    void ApplySlowEffect(Enemy enemy)
    {
        // Add a slow effect component or use a coroutine
        StartCoroutine(SlowEnemyCoroutine(enemy));
    }

    System.Collections.IEnumerator SlowEnemyCoroutine(Enemy enemy)
    {
        // Store original speed
        float originalSpeed = enemy.GetMoveSpeed();
        float slowedSpeed = originalSpeed * slowMultiplier;
        
        // Apply slow
        enemy.SetMoveSpeed(slowedSpeed);
        
        // Wait for slow duration
        yield return new WaitForSeconds(slowDuration);
        
        // Restore original speed
        enemy.SetMoveSpeed(originalSpeed);
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
    }

    // Visual debugging
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, frostRadius);
    }
}
