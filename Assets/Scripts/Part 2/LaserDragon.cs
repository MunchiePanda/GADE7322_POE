using UnityEngine;

/// <summary>
/// Laser Dragon - Ranged enemy that charges and fires powerful laser beams.
/// Stands still to charge laser, then fires a beam that can kill defenders in 2 shots.
/// </summary>
public class LaserDragon : Enemy
{
    [Header("Laser Settings")]
    [Tooltip("Time to charge laser before firing")]
    public float laserChargeTime = 2f;
    
    [Tooltip("Damage per laser shot (should be half defender health)")]
    public float laserDamage = 7.5f; // Half of typical defender health (15)
    
    [Tooltip("Laser range")]
    public float laserRange = 50f;
    
    [Tooltip("Cooldown between laser shots")]
    public float laserCooldown = 3f;
    
    [Tooltip("Visual effect for laser beam")]
    public LineRenderer laserBeam;
    
    [Tooltip("Particle system for laser charge effect")]
    public ParticleSystem chargeParticles;
    
    [Tooltip("Material for laser beam")]
    public Material laserMaterial;
    
    [Header("Laser Visual")]
    [Tooltip("Laser beam width")]
    public float laserWidth = 0.2f;
    
    [Tooltip("Laser beam color")]
    public Color laserColor = Color.red;
    
    private bool isCharging = false;
    private bool isOnCooldown = false;
    private float chargeStartTime = 0f;
    private float lastShotTime = 0f;
    private Transform currentTarget = null;
    private Vector3 targetPosition = Vector3.zero;
    private float originalSpeed;

    protected override void Start()
    {
        base.Start();
        // Laser Dragon characteristics
        moveSpeed = 4f;           // Slower than regular enemies
        maxHealth = 12f;         // Higher health than bomber
        currentHealth = maxHealth;
        attackDamage = 0f;       // No regular attack damage (uses laser instead)
        detectionRange = laserRange; // Detection range = laser range
        originalSpeed = moveSpeed;
        
        // Visual setup
        transform.localScale *= 0.8f; // Slightly larger than bomber
        
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
        
        // Setup laser beam
        SetupLaserBeam();
    }

    void SetupLaserBeam()
    {
        // Create laser beam LineRenderer if not assigned
        if (laserBeam == null)
        {
            GameObject laserObj = new GameObject("LaserBeam");
            laserObj.transform.SetParent(transform);
            laserBeam = laserObj.AddComponent<LineRenderer>();
        }
        
        // Configure laser beam
        laserBeam.material = laserMaterial != null ? laserMaterial : new Material(Shader.Find("Sprites/Default"));
        laserBeam.startColor = laserColor;
        laserBeam.endColor = laserColor;
        laserBeam.startWidth = laserWidth;
        laserBeam.endWidth = laserWidth;
        laserBeam.positionCount = 2;
        laserBeam.enabled = false;
        
        // Set sorting order to appear above other objects
        laserBeam.sortingOrder = 10;
    }

    protected void Update()
    {
        if (currentHealth <= 0f) return;

        // Check cooldown
        if (isOnCooldown && Time.time - lastShotTime >= laserCooldown)
        {
            isOnCooldown = false;
        }

        // If we're charging, continue charging
        if (isCharging)
        {
            ContinueCharging();
            return;
        }

        // If not on cooldown, look for targets to laser
        if (!isOnCooldown)
        {
            AcquireLaserTarget();
        }
        
        // If no target and not charging, continue normal path following
        if (!isCharging && !isOnCooldown)
        {
            FollowPathTowardsTower();
        }
    }

    void AcquireLaserTarget()
    {
        // Find all defenders in scene and pick the closest one
        Defender[] allDefenders = FindObjectsByType<Defender>(FindObjectsSortMode.None);
        
        float nearestDistance = float.MaxValue;
        Transform nearestTarget = null;
        Vector3 nearestTargetPosition = Vector3.zero;

        // Debug: Log all defenders found
        Debug.Log($"Laser Dragon found {allDefenders.Length} defenders");

        foreach (Defender defender in allDefenders)
        {
            if (defender != null && defender.IsAlive())
            {
                Vector3 defenderPosition = defender.transform.position;
                float distance = Vector3.Distance(transform.position, defenderPosition);
                
                Debug.Log($"Defender at distance {distance:F1}, laser range {laserRange}");
                
                if (distance <= laserRange && distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestTarget = defender.transform;
                    nearestTargetPosition = defenderPosition;
                    Debug.Log($"Selected defender at distance {distance:F1}");
                }
            }
        }

        // If no defender found, check for tower
        if (nearestTarget == null && targetTower != null)
        {
            float towerDistance = Vector3.Distance(transform.position, targetTower.transform.position);
            Debug.Log($"No defenders in range, checking tower at distance {towerDistance:F1}");
            if (towerDistance <= laserRange)
            {
                nearestTarget = targetTower.transform;
                nearestTargetPosition = targetTower.transform.position;
                Debug.Log("Targeting tower");
            }
        }

        // Start charging if we found any target
        if (nearestTarget != null)
        {
            Debug.Log($"Laser Dragon targeting: {nearestTarget.name} at distance {nearestDistance:F1}");
            StartLaserCharge(nearestTarget, nearestTargetPosition);
        }
        else
        {
            Debug.Log("Laser Dragon: No targets found in range");
        }
    }

    void StartLaserCharge(Transform target, Vector3 targetPos)
    {
        isCharging = true;
        currentTarget = target;
        targetPosition = targetPos;
        chargeStartTime = Time.time;
        
        // Stop moving while charging
        moveSpeed = 0f;
        
        // Face the target
        Vector3 direction = (targetPos - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(direction);
        
        // Play charge effect
        if (chargeParticles != null)
        {
            chargeParticles.Play();
        }
    }

    void ContinueCharging()
    {
        if (currentTarget == null)
        {
            // Target lost, stop charging
            StopCharging();
            return;
        }

        // Check if charge time is complete
        if (Time.time - chargeStartTime >= laserChargeTime)
        {
            FireLaser();
        }
        else
        {
            // Update laser beam preview during charge
            UpdateLaserPreview();
        }
    }

    void UpdateLaserPreview()
    {
        if (laserBeam != null)
        {
            laserBeam.enabled = true;
            laserBeam.SetPosition(0, transform.position + Vector3.up * 0.5f);
            laserBeam.SetPosition(1, targetPosition);
            
            // Make beam pulse during charge
            float chargeProgress = (Time.time - chargeStartTime) / laserChargeTime;
            float alpha = Mathf.Lerp(0.3f, 1f, chargeProgress);
            Color beamColor = laserColor;
            beamColor.a = alpha;
            laserBeam.startColor = beamColor;
            laserBeam.endColor = beamColor;
        }
    }

    void FireLaser()
    {
        if (currentTarget == null) return;
        
        // Fire laser beam
        if (laserBeam != null)
        {
            laserBeam.enabled = true;
            laserBeam.SetPosition(0, transform.position + Vector3.up * 0.5f);
            laserBeam.SetPosition(1, targetPosition);
            laserBeam.startColor = laserColor;
            laserBeam.endColor = laserColor;
        }
        
        // Deal damage to target
        float distance = Vector3.Distance(transform.position, targetPosition);
        if (distance <= laserRange)
        {
            // Damage defender
            Defender defender = currentTarget.GetComponent<Defender>();
            if (defender != null)
            {
                defender.TakeDamage(laserDamage);
            }
            
            // Damage tower
            Tower tower = currentTarget.GetComponent<Tower>();
            if (tower != null)
            {
                tower.TakeDamage(laserDamage);
            }
        }
        
        // Hide laser beam after short delay
        if (laserBeam != null)
        {
            Invoke("HideLaserBeam", 0.5f);
        }
        
        // Start cooldown
        StopCharging();
        isOnCooldown = true;
        lastShotTime = Time.time;
    }

    void HideLaserBeam()
    {
        if (laserBeam != null)
        {
            laserBeam.enabled = false;
        }
    }

    void StopCharging()
    {
        isCharging = false;
        currentTarget = null;
        moveSpeed = originalSpeed;
        
        if (chargeParticles != null)
        {
            chargeParticles.Stop();
        }
        
        if (laserBeam != null)
        {
            laserBeam.enabled = false;
        }
    }

    // Override TakeDamage to ensure proper laser dragon damage handling
    public override void TakeDamage(float amount)
    {
        if (currentHealth <= 0f) return;
        
        currentHealth -= amount;

        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            Die();
        }
    }

    // Override to handle death properly
    protected void Die()
    {
        // Stop any ongoing effects
        StopCharging();
        
        // Give resources for killing it
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
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, laserRange);
        
        if (isCharging && currentTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position + Vector3.up * 0.5f, targetPosition);
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(targetPosition, 1f);
        }
    }
}
