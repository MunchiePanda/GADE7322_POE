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
    public float explosionRadius = 50f;
    
    [Tooltip("Explosion damage amount")]
    public float explosionDamage = 5f;
    
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
    private Vector3 targetPosition = Vector3.zero;
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
        // Direct approach: Find all defenders in scene and pick the closest one
        Defender[] allDefenders = FindObjectsByType<Defender>(FindObjectsSortMode.None);
        // Debug.Log($"BOMBER DIRECT SEARCH: Found {allDefenders.Length} defenders in scene");
        
        float nearestDistance = float.MaxValue;
        Transform nearestTarget = null;
        Vector3 nearestTargetPosition = Vector3.zero;

        foreach (Defender defender in allDefenders)
        {
            if (defender != null && defender.IsAlive())
            {
                // Get the defender's actual world position
                Vector3 defenderPosition = defender.transform.position;
                float distance = Vector3.Distance(transform.position, defenderPosition);
                
                // Debug.Log($"BOMBER CHECKING: Defender {defender.name} at {defenderPosition}, distance {distance:F2}");
                
                if (distance <= detectionRange && distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestTarget = defender.transform;
                    nearestTargetPosition = defenderPosition;
                    // Debug.Log($"BOMBER SELECTED: {defender.name} as target (distance: {distance:F2})");
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
                nearestTargetPosition = targetTower.transform.position;
                // Debug.Log($"BOMBER TOWER: Targeting tower at distance {towerDistance:F2}");
            }
        }

        // Start charging if we found any target
        if (nearestTarget != null)
        {
            // Debug.Log($"BOMBER CHARGE: Starting charge at {nearestTarget.name} at position {nearestTargetPosition}!");
            StartCharge(nearestTarget, nearestTargetPosition);
        }
        else
        {
            // Debug.Log("BOMBER: No targets found, continuing on path");
        }
    }

    void StartCharge(Transform target, Vector3 targetPos)
    {
        isCharging = true;
        chargeTarget = target;
        targetPosition = targetPos; // Store the exact position we're aiming for
        moveSpeed = originalSpeed * chargeSpeedMultiplier;
        
        // Play charge effect
        if (chargeParticles != null)
        {
            chargeParticles.Play();
        }
        
        // Face the target using the stored position
        Vector3 direction = (targetPos - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(direction);
        
        // Debug.Log($"BOMBER CHARGE START: Targeting {target.name} at position {targetPos}");
        // Debug.Log($"BOMBER POSITION: {transform.position}, TARGET POSITION: {targetPos}");
        // Debug.Log($"BOMBER DIRECTION: {direction}, DISTANCE: {Vector3.Distance(transform.position, targetPos):F2}");
    }

    void ChargeAtTarget()
    {
        if (chargeTarget == null)
        {
            isCharging = false;
            moveSpeed = originalSpeed;
            return;
        }

        // Move directly at the stored target position (not the transform position)
        Vector3 direction = (targetPosition - transform.position).normalized;
        Vector3 newPosition = transform.position + direction * moveSpeed * Time.deltaTime;
        transform.position = newPosition;
        
        // Update rotation to face target
        transform.rotation = Quaternion.LookRotation(direction);
        
        // Check if we're close enough to explode using the stored position
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
        // Debug.Log($"BOMBER CHARGING: Current pos {transform.position}, Target pos {targetPosition}, Distance {distanceToTarget:F2}, Threshold 1.5f");
        
        if (distanceToTarget <= 1.5f) // Close enough to explode
        {
            // Debug.Log($"BOMBER EXPLODING: Reached target at distance {distanceToTarget:F2}!");
            Explode();
        }
    }

    void Explode()
    {
        // Debug.Log("Kamikaze Dragon exploding with AOE damage!");
        
        // The explosion gets bigger and more damaging as waves progress
        // This makes later waves way more dangerous
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
                // Debug.Log($"SCALED EXPLOSION: Wave {currentWave} - Radius: {scaledRadius:F1}, Damage: {scaledDamage:F1}");
            }
        }
        
        // Deal AOE explosion damage to nearby targets
        Collider[] explosionHits = Physics.OverlapSphere(transform.position, scaledRadius);
        // Debug.Log($"BOMBER EXPLOSION: Hit {explosionHits.Length} objects in radius {scaledRadius:F1}");
        
        foreach (Collider hit in explosionHits)
        {
            // Debug.Log($"BOMBER HIT: {hit.name} at distance {Vector3.Distance(transform.position, hit.transform.position):F2}");
            
            float distance = Vector3.Distance(transform.position, hit.transform.position);
            
            // Small damage to all defenders within 50-unit radius
            Defender defender = hit.GetComponent<Defender>();
            if (defender != null)
            {
                // Small, consistent damage to all defenders in range
                float defenderDamage = scaledDamage; // Small damage, no distance falloff
                // Debug.Log($"BOMBER DEFENDER: {defenderDamage:F1} damage to defender {defender.name} (distance: {distance:F1})");
                defender.TakeDamage(defenderDamage);
            }
            
            // AOE damage to tower (with distance falloff)
            Tower tower = hit.GetComponent<Tower>();
            if (tower != null)
            {
                float damageMultiplier = 1f - (distance / scaledRadius); // 1.0 at center, 0.0 at edge
                damageMultiplier = Mathf.Clamp01(damageMultiplier);
                float towerDamage = scaledDamage * 2f * damageMultiplier; // Higher damage to tower
                // Debug.Log($"BOMBER TOWER: {towerDamage:F1} damage to tower (distance: {distance:F1})");
                tower.TakeDamage(towerDamage);
            }
            
            // AOE damage to other enemies (friendly fire)
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null && enemy != this)
            {
                float friendlyDamage = scaledDamage * 0.2f; // Very small friendly fire
                // Debug.Log($"BOMBER FRIENDLY: {friendlyDamage:F1} friendly fire to {enemy.name}");
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

    // Override TakeDamage to ensure proper bomber damage handling
    public override void TakeDamage(float amount)
    {
        // Prevent damage if already dead
        if (currentHealth <= 0f)
        {
            // Debug.Log($"BOMBER {gameObject.name} is already dead, ignoring damage");
            return;
        }
        
        // Debug.Log($"BOMBER DAMAGE: {gameObject.name} taking {amount} damage. Health: {currentHealth} -> {currentHealth - amount}");
        currentHealth -= amount;

        // Debug.Log($"BOMBER HEALTH: {gameObject.name} health after damage: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0f)
        {
            // Debug.Log($"BOMBER DEATH: {gameObject.name} health reached zero. Calling Die().");
            currentHealth = 0f; // Ensure health doesn't go negative
            Die();
        }
        else
        {
            // Debug.Log($"BOMBER ALIVE: {gameObject.name} still alive with {currentHealth} health");
        }
    }

    // Override to prevent normal death behavior
    protected void Die()
    {
        // Debug.Log("Kamikaze Dragon died!");
        
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
            
            // Draw line to stored target position
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, targetPosition);
            
            // Draw stored target position (where we're actually aiming)
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(targetPosition, 1f);
            
            // Draw current target transform position (for comparison)
            if (chargeTarget != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(chargeTarget.position, 0.5f);
            }
        }
    }
}
