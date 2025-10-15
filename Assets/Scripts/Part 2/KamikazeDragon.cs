using UnityEngine;

/// <summary>
/// Kamikaze Dragon - Fast, low health, explodes on contact with defenders or tower.
/// Once it locks onto a target, it charges at maximum speed and explodes on impact.
/// </summary>
public class KamikazeDragon : Enemy
{
    [Header("Kamikaze Settings")]
    [Tooltip("Speed multiplier when charging at target")]
    public float chargeSpeedMultiplier = 4f;
    
    [Tooltip("Explosion damage radius (AOE)")]
    public float explosionRadius = 10f;
    
    [Tooltip("Explosion damage amount")]
    public float explosionDamage = 50f;
    
    [Tooltip("Visual effect for explosion")]
    public GameObject explosionEffectPrefab;
    
    [Tooltip("Particle system for charge effect")]
    public ParticleSystem chargeParticles;
    
    [Header("Scaling Explosion")]
    [Tooltip("Explosion radius scales with wave")]
    public float explosionRadiusScaling = 1.1f;
    
    [Tooltip("Explosion damage scales with wave")]
    public float explosionDamageScaling = 1.2f;
    
    private bool isCharging = false;
    private Transform chargeTarget = null;
    private float originalSpeed;

    protected override void Start()
    {
        base.Start();
        // Kamikaze characteristics
        moveSpeed = 8f;           // Very fast base speed (increased)
        maxHealth = 6f;          // Lower health for faster gameplay
        currentHealth = maxHealth;
        attackDamage = 0f;       // No regular attack damage (explodes instead)
        detectionRange = 30f;    // Very long detection range to find any defender
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
        // Look for ANY defender in range - much more flexible targeting
        float searchRange = detectionRange * 2f; // Even larger detection range
        Collider[] hits = Physics.OverlapSphere(transform.position, searchRange);
        Debug.Log($"🎯 BOMBER SEARCH: Looking for ANY defender in range {searchRange}, found {hits.Length} colliders");
        
        float nearestDistance = float.MaxValue;
        Transform nearestTarget = null;

        // First pass: Look for any defender in range
        foreach (var hit in hits)
        {
            // Check for Defender component in the hit object or its children
            Defender defender = hit.GetComponent<Defender>();
            if (defender == null)
            {
                defender = hit.GetComponentInParent<Defender>();
            }
            if (defender == null)
            {
                defender = hit.GetComponentInChildren<Defender>();
            }
            
            if (defender != null && defender.IsAlive())
            {
                float distance = Vector3.Distance(transform.position, defender.transform.position);
                Debug.Log($"🎯 BOMBER FOUND: Defender {defender.name} at {defender.transform.position}, distance {distance:F2}");
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestTarget = defender.transform;
                    Debug.Log($"🎯 BOMBER SELECTED: {defender.name} as target (distance: {distance:F2})");
                }
            }
        }

        // If no defender found in physics overlap, try finding all defenders in scene
        if (nearestTarget == null)
        {
            Debug.Log("🎯 BOMBER: No defenders found via physics, searching all defenders in scene...");
            Defender[] allDefenders = FindObjectsByType<Defender>(FindObjectsSortMode.None);
            Debug.Log($"🎯 BOMBER: Found {allDefenders.Length} total defenders in scene");
            
            foreach (Defender defender in allDefenders)
            {
                if (defender != null && defender.IsAlive())
                {
                    float distance = Vector3.Distance(transform.position, defender.transform.position);
                    Debug.Log($"🎯 BOMBER SCENE: Defender {defender.name} at {defender.transform.position}, distance {distance:F2}");
                    
                    if (distance <= searchRange && distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestTarget = defender.transform;
                        Debug.Log($"🎯 BOMBER SCENE SELECTED: {defender.name} as target (distance: {distance:F2})");
                    }
                }
            }
        }

        // If still no defender found, check for tower
        if (nearestTarget == null && targetTower != null)
        {
            float towerDistance = Vector3.Distance(transform.position, targetTower.transform.position);
            if (towerDistance <= detectionRange)
            {
                nearestTarget = targetTower.transform;
                Debug.Log($"🎯 BOMBER TOWER: Targeting tower at distance {towerDistance:F2}");
            }
        }

        // Start charging if we found any target
        if (nearestTarget != null)
        {
            Debug.Log($"🎯 BOMBER CHARGE: Starting charge at {nearestTarget.name}!");
            StartCharge(nearestTarget);
        }
        else
        {
            Debug.Log("🎯 BOMBER: No targets found, continuing on path");
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
        Debug.Log($"Bomber distance to target: {distanceToTarget:F2}, explosion threshold: 1.5f");
        if (distanceToTarget <= 1.5f) // Close enough to explode
        {
            Debug.Log("Bomber is close enough to explode!");
            Explode();
        }
    }

    void Explode()
    {
        Debug.Log("💥 Kamikaze Dragon exploding with AOE damage!");
        
        // Calculate scaled explosion values based on current wave
        float scaledRadius = explosionRadius;
        float scaledDamage = explosionDamage;
        
        // Get current wave for scaling
        EnemySpawner spawner = FindFirstObjectByType<EnemySpawner>();
        if (spawner != null)
        {
            int currentWave = spawner.currentWave;
            if (currentWave > 1)
            {
                scaledRadius = explosionRadius * Mathf.Pow(explosionRadiusScaling, currentWave - 1);
                scaledDamage = explosionDamage * Mathf.Pow(explosionDamageScaling, currentWave - 1);
                Debug.Log($"💥 SCALED EXPLOSION: Wave {currentWave} - Radius: {scaledRadius:F1}, Damage: {scaledDamage:F1}");
            }
        }
        
        // Deal AOE explosion damage to nearby targets
        Collider[] explosionHits = Physics.OverlapSphere(transform.position, scaledRadius);
        Debug.Log($"💥 AOE EXPLOSION: Hit {explosionHits.Length} objects in radius {scaledRadius:F1}");
        
        foreach (Collider hit in explosionHits)
        {
            Debug.Log($"💥 Explosion hit: {hit.name}");
            
            // Calculate distance-based damage falloff
            float distance = Vector3.Distance(transform.position, hit.transform.position);
            float damageMultiplier = 1f - (distance / scaledRadius); // 1.0 at center, 0.0 at edge
            damageMultiplier = Mathf.Clamp01(damageMultiplier);
            float finalDamage = scaledDamage * damageMultiplier;
            
            // AOE damage to defenders (not instant kill, but high damage)
            Defender defender = hit.GetComponent<Defender>();
            if (defender != null)
            {
                Debug.Log($"💥 AOE DAMAGE: {finalDamage:F1} damage to defender {defender.name} (distance: {distance:F1})");
                defender.TakeDamage(finalDamage);
            }
            
            // AOE damage to tower
            Tower tower = hit.GetComponent<Tower>();
            if (tower != null)
            {
                Debug.Log($"💥 AOE DAMAGE: {finalDamage:F1} damage to tower (distance: {distance:F1})");
                tower.TakeDamage(finalDamage);
            }
            
            // AOE damage to other enemies (friendly fire)
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null && enemy != this)
            {
                float friendlyDamage = finalDamage * 0.3f; // Reduced friendly fire
                Debug.Log($"💥 AOE DAMAGE: {friendlyDamage:F1} friendly fire to {enemy.name}");
                enemy.TakeDamage(friendlyDamage);
            }
        }
        
        // Play scaled explosion effect
        if (explosionEffectPrefab != null)
        {
            GameObject explosion = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            // Scale the explosion effect with the radius
            explosion.transform.localScale = Vector3.one * (scaledRadius / explosionRadius);
            Destroy(explosion, 5f); // Longer duration for larger explosions
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
