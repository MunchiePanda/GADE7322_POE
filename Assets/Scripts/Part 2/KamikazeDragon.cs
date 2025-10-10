using UnityEngine;

/// <summary>
/// Kamikaze Dragon - Fast, low health, explodes on contact with defenders or tower.
/// Once it locks onto a target, it charges at maximum speed and explodes on impact.
/// </summary>
public class KamikazeDragon : Enemy
{
    [Header("Kamikaze Settings")]
    [Tooltip("Speed multiplier when charging at target")]
    public float chargeSpeedMultiplier = 3f;
    
    [Tooltip("Explosion damage radius")]
    public float explosionRadius = 4f;
    
    [Tooltip("Explosion damage amount")]
    public float explosionDamage = 25f;
    
    [Tooltip("Visual effect for explosion")]
    public GameObject explosionEffectPrefab;
    
    [Tooltip("Particle system for charge effect")]
    public ParticleSystem chargeParticles;
    
    private bool isCharging = false;
    private Transform chargeTarget = null;
    private float originalSpeed;

    protected override void Start()
    {
        base.Start();
        // Kamikaze characteristics
        moveSpeed = 6f;           // Very fast base speed
        maxHealth = 8f;          // Low health
        currentHealth = maxHealth;
        attackDamage = 0f;       // No regular attack damage (explodes instead)
        detectionRange = 10f;    // Long detection range
        originalSpeed = moveSpeed;
        
        // Visual setup
        transform.localScale *= 0.7f; // Smaller than regular enemies
        
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.red;
        }
        else
        {
            // Add a renderer if none exists
            renderer = gameObject.AddComponent<MeshRenderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = Color.red;
            renderer.material = mat;
        }
    }

    protected void Update()
    {
        if (currentHealth <= 0f) return;

        // If we're charging, move directly at target
        if (isCharging && chargeTarget != null)
        {
            ChargeAtTarget();
            return;
        }

        // Check for targets to charge at
        AcquireChargeTarget();
        
        // If no target, continue normal path following
        if (!isCharging)
        {
            FollowPathTowardsTower();
        }
    }

    void AcquireChargeTarget()
    {
        // Look for defenders first
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRange);
        float nearestDistance = float.MaxValue;
        Transform nearestTarget = null;

        foreach (var hit in hits)
        {
            Defender defender = hit.GetComponentInParent<Defender>();
            if (defender != null && defender.IsAlive())
            {
                float distance = Vector3.Distance(transform.position, defender.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestTarget = defender.transform;
                }
            }
        }

        // If no defender found, check for tower
        if (nearestTarget == null && targetTower != null)
        {
            float towerDistance = Vector3.Distance(transform.position, targetTower.transform.position);
            if (towerDistance <= detectionRange)
            {
                nearestTarget = targetTower.transform;
            }
        }

        // Start charging if we found a target
        if (nearestTarget != null)
        {
            StartCharge(nearestTarget);
        }
    }

    void StartCharge(Transform target)
    {
        isCharging = true;
        chargeTarget = target;
        moveSpeed = originalSpeed * chargeSpeedMultiplier;
        
        // Play charge effect
        if (chargeParticles != null)
        {
            chargeParticles.Play();
        }
        
        // Face the target
        Vector3 direction = (target.position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(direction);
        
        Debug.Log("Kamikaze Dragon charging at target!");
    }

    void ChargeAtTarget()
    {
        if (chargeTarget == null)
        {
            isCharging = false;
            moveSpeed = originalSpeed;
            return;
        }

        // Move directly at target
        Vector3 direction = (chargeTarget.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;
        
        // Check if we're close enough to explode
        float distanceToTarget = Vector3.Distance(transform.position, chargeTarget.position);
        if (distanceToTarget <= 1.5f) // Close enough to explode
        {
            Explode();
        }
    }

    void Explode()
    {
        Debug.Log("Kamikaze Dragon exploding!");
        
        // Deal explosion damage to nearby targets
        Collider[] explosionHits = Physics.OverlapSphere(transform.position, explosionRadius);
        
        foreach (Collider hit in explosionHits)
        {
            // Damage defenders
            Defender defender = hit.GetComponent<Defender>();
            if (defender != null)
            {
                defender.TakeDamage(explosionDamage);
            }
            
            // Damage tower
            Tower tower = hit.GetComponent<Tower>();
            if (tower != null)
            {
                tower.TakeDamage(explosionDamage);
            }
            
            // Damage other enemies (friendly fire)
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null && enemy != this)
            {
                enemy.TakeDamage(explosionDamage * 0.5f); // Half damage to other enemies
            }
        }
        
        // Play explosion effect
        if (explosionEffectPrefab != null)
        {
            GameObject explosion = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            Destroy(explosion, 3f);
        }
        
        // Destroy self
        Die();
    }

    // Override to prevent normal death behavior
    protected void Die()
    {
        Debug.Log("Kamikaze Dragon died!");
        
        // Give resources for killing it before it explodes
        if (gameManager != null)
        {
            int resourceReward = Random.Range(minResourceRewardOnDeath, maxResourceRewardOnDeath + 1);
            gameManager.AddResources(resourceReward);
            
            EnemySpawner spawner = FindFirstObjectByType<EnemySpawner>();
            if (spawner != null)
            {
                spawner.OnEnemyDeath(gameObject);
            }
        }
        
        Destroy(gameObject);
    }

    // Visual debugging
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        if (isCharging)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}
