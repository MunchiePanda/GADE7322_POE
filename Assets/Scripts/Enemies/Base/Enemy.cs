using UnityEngine;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        // Debug logging disabled
    }

    void OnTriggerEnter(Collider other)
    {
        // Debug logging disabled
        Projectile projectile = other.GetComponent<Projectile>();
        if (projectile != null)
        {
            // Debug logging disabled
        }
    }
    [Header("Stats")]
    [SerializeField] protected float maxHealth = 10f;
    [SerializeField] protected float currentHealth = 0f;
    [SerializeField] protected float moveSpeed = 2.5f;
    [SerializeField] protected float attackDamage = 5f;
    [SerializeField] protected float attackIntervalSeconds = 1.0f;
    [SerializeField] protected float detectionRange = 2.0f;
    [SerializeField] protected int minResourceRewardOnDeath = 5;
    [SerializeField] protected int maxResourceRewardOnDeath = 15;

    [Header("Health Bar")]
    [Tooltip("UI Slider component for displaying health bar")]
    public UnityEngine.UI.Slider healthBarSlider;
    
    [Tooltip("Health bar controller for camera-facing health bar")]
    public HealthBarController healthBarController;

    protected List<Vector3Int> path;
    protected int currentPathIndex = 0;
    protected int terrainHeight = 1; // legacy; not used for uneven per-tile heights
    protected int finalIndex = 0;
    protected VoxelTerrainGenerator terrainGenerator;
    protected Tower targetTower;
    protected GameManager gameManager;

    // Combat state
    protected Defender currentDefenderTarget;
    protected float lastAttackTime = -999f;

    private float yOffset = 2f; // Increased yOffset to raise the enemy

    public virtual void Initialize(List<Vector3Int> pathToFollow, int terrainTopY, Tower tower, GameManager gm, float offset = 1f)
    {
        path = pathToFollow;
        terrainHeight = terrainTopY;
        targetTower = tower;
        gameManager = gm;
        terrainGenerator = gameManager.terrainGenerator;
        yOffset = offset;
        finalIndex = path != null && path.Count > 0 ? path.Count - 1 : 0;
    }

    // Public getters and setters for fields accessed by EnemySpawner and WeatherManager
    public float GetMoveSpeed() { return moveSpeed; }
    public float GetMaxHealth() { return maxHealth; }
    public void SetMaxHealth(float value) { maxHealth = value; }
    public void SetCurrentHealth(float value) { currentHealth = value; }
    public void SetMoveSpeed(float value) { moveSpeed = value; }
    public float GetAttackDamage() { return attackDamage; }
    public void SetAttackDamage(float value) { attackDamage = value; }

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        Debug.Log($"Enemy {gameObject.name} initialized with health: {currentHealth}/{maxHealth}");

        // Initialize health bar
        UpdateHealthBar();

        // Test: Force the enemy to die immediately
        // TakeDamage(currentHealth);
    }

    void Update()
    {
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

    protected void FollowPathTowardsTower()
    {
        if (path == null || path.Count == 0 || terrainGenerator == null)
            return;

        Vector3Int grid = path[currentPathIndex];
        Vector3 targetPos = terrainGenerator.GetSurfaceWorldPosition(grid);
        targetPos.y += yOffset;

        Vector3 toTarget = targetPos - transform.position;
        float step = moveSpeed * Time.deltaTime;

        if (toTarget.magnitude <= step)
        {
            transform.position = targetPos;
            // Advance along the path
            if (currentPathIndex < finalIndex)
            {
                currentPathIndex++;
                
                // Report path progression to performance tracker
                if (gameManager != null && gameManager.performanceTracker != null)
                {
                    float progressionPercentage = ((float)currentPathIndex / finalIndex) * 100f;
                    gameManager.performanceTracker.OnEnemyPathProgression(progressionPercentage);
                }
            }
            else
            {
                // Reached tower vicinity; attack tower if available
                if (targetTower != null)
                {
                    TryAttackTower();
                }
            }
        }
        else
        {
            Vector3 direction = toTarget.normalized;
            transform.position += direction * step;
        }
    }

    protected void AcquireDefenderIfAny()
    {
        // If we already have a target that is alive and nearby, keep it
        if (currentDefenderTarget != null && currentDefenderTarget.IsAlive())
        {
            float dist = Vector3.Distance(transform.position, currentDefenderTarget.transform.position);
            if (dist <= detectionRange + 0.5f)
                return;
        }

        currentDefenderTarget = null;
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRange);
        // Debug logging disabled
        
        float nearest = float.MaxValue;
        foreach (var hit in hits)
        {
            Defender defender = hit.GetComponentInParent<Defender>();
            if (defender != null && defender.IsAlive())
            {
                float d = Vector3.Distance(transform.position, defender.transform.position);
                // Debug logging disabled
                if (d < nearest)
                {
                    nearest = d;
                    currentDefenderTarget = defender;
                }
            }
        }
        
        if (currentDefenderTarget != null)
        {
            // Debug logging disabled
        }
    }

    protected void TryAttackDefender()
    {
        if (currentDefenderTarget == null || !currentDefenderTarget.IsAlive())
        {
            currentDefenderTarget = null;
            return;
        }

        float time = Time.time;
        if (time - lastAttackTime >= attackIntervalSeconds)
        {
            lastAttackTime = time;
            // Debug logging disabled
            currentDefenderTarget.TakeDamage(attackDamage);
        }
    }

    void TryAttackTower()
    {
        float time = Time.time;
        if (time - lastAttackTime >= attackIntervalSeconds)
        {
            lastAttackTime = time;
            targetTower.TakeDamage(attackDamage);
            
            // Enemy dies after first attack on tower
            // Debug logging disabled
            Die();
        }
    }

    public virtual void TakeDamage(float amount)
    {
        // Prevent damage if already dead
        if (currentHealth <= 0f)
        {
            Debug.Log($"Enemy {gameObject.name} is already dead, ignoring damage");
            return;
        }
        
        Debug.Log($"Enemy {gameObject.name} taking {amount} damage. Health: {currentHealth} -> {currentHealth - amount}");
        currentHealth -= amount;

        // Update health bar after taking damage
        UpdateHealthBar();

        if (currentHealth <= 0f)
        {
            Debug.Log($"Enemy {gameObject.name} health reached zero. Calling Die().");
            currentHealth = 0f; // Ensure health doesn't go negative
            Die();
        }
    }

    private bool isDead = false;
    
    protected virtual void Die()
    {
        // Prevent multiple death calls
        if (isDead)
        {
            Debug.Log($"Enemy {gameObject.name} already dead, ignoring duplicate death call");
            return;
        }
        
        isDead = true;
        Debug.Log($"Enemy {gameObject.name} died!");

        // Play explosion effect if available
        // Note: ExplosionEffect script was removed - add particle effects here if needed

        if (gameManager != null)
        {
            Debug.Log($"Adding resources and notifying spawner for {gameObject.name}");
            // Add a random amount of resources within the specified range
            int resourceReward = Random.Range(minResourceRewardOnDeath, maxResourceRewardOnDeath + 1);
            gameManager.AddResources(resourceReward);
            // Notify EnemySpawner that this enemy died
            EnemySpawner spawner = FindFirstObjectByType<EnemySpawner>();
            if (spawner != null)
            {
                spawner.OnEnemyDeath(gameObject);
            }
        }

        Debug.Log($"Destroying enemy object: {gameObject.name}");
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Updates the health bar slider to reflect current health
    /// </summary>
    private void UpdateHealthBar()
    {
        // Update the slider directly if available
        if (healthBarSlider != null && maxHealth > 0)
        {
            healthBarSlider.value = currentHealth / maxHealth;
        }
        
        // Update the health bar controller if available
        if (healthBarController != null)
        {
            healthBarController.UpdateHealth(currentHealth, maxHealth);
        }
    }
}


