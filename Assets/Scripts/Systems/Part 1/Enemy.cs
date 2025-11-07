using UnityEngine;
using UnityEngine.UI;
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

    [Header("UI")]
    [Tooltip("Optional world space health bar slider. Assign a UI slider to display current health visually above the enemy.")]
    [SerializeField] protected Slider healthBarSlider;

    protected ParticleSystem buffParticleEffect;
    protected float buffSpeedMultiplier = 1.5f;
    protected float buffDamageMultiplier = 1.2f;
    protected bool _isBuffed = false;
    protected float _originalSpeed;
    protected float _originalDamage;


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
    public float GetCurrentHealth() { return currentHealth; }
    public void SetMaxHealth(float value) { maxHealth = value; }
    public void SetCurrentHealth(float value) { currentHealth = value; }
    public void SetMoveSpeed(float value) { moveSpeed = value; }
    public float GetAttackDamage() { return attackDamage; }
    public void SetAttackDamage(float value) { attackDamage = value; }

    protected virtual void Start()
    {
        currentHealth = maxHealth;

        _originalSpeed = moveSpeed;
        _originalDamage = attackDamage;
        
        InitializeHealthBar();
    }

    public void ApplyBuff()
    {
        if (_isBuffed) return;

        _isBuffed = true;
        moveSpeed *= buffSpeedMultiplier;
        attackDamage *= buffDamageMultiplier;

        if (buffParticleEffect != null)
        {
            buffParticleEffect.Play();
        }
    }

    public void RemoveBuff()
    {
        if (!_isBuffed) return;

        _isBuffed = false;
        moveSpeed = _originalSpeed;
        attackDamage = _originalDamage;

        if (buffParticleEffect != null)
        {
            buffParticleEffect.Stop();
        }
    }

    public bool IsBuffed()
    {
        return _isBuffed;
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
            if (currentPathIndex < finalIndex)
            {
                currentPathIndex++;
                
                if (gameManager != null && gameManager.performanceTracker != null)
                {
                    float progressionPercentage = ((float)currentPathIndex / finalIndex) * 100f;
                    gameManager.performanceTracker.OnEnemyPathProgression(progressionPercentage);
                }
            }
            else
            {
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
            
            // Rotate the enemy to face the direction it is moving.
            // This provides visual feedback showing which waypoint the enemy is heading toward.
            // The rotation is smoothly interpolated to avoid jarring snaps during path following.
            if (direction.magnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
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

        // Face the defender being attacked to make combat encounters more visually clear.
        // The enemy will continuously track the defender's position during combat.
        Vector3 directionToDefender = (currentDefenderTarget.transform.position - transform.position).normalized;
        if (directionToDefender.magnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToDefender);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        float time = Time.time;
        if (time - lastAttackTime >= attackIntervalSeconds)
        {
            lastAttackTime = time;
            currentDefenderTarget.TakeDamage(attackDamage);
        }
    }

    void TryAttackTower()
    {
        // Rotate to face the tower when the enemy arrives at its final destination.
        // This ensures the attack visually targets the tower properly.
        if (targetTower != null)
        {
            Vector3 directionToTower = (targetTower.transform.position - transform.position).normalized;
            if (directionToTower.magnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToTower);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
        }

        float time = Time.time;
        if (time - lastAttackTime >= attackIntervalSeconds)
        {
            lastAttackTime = time;
            targetTower.TakeDamage(attackDamage);
            
            Die();
        }
    }

    public virtual void TakeDamage(float amount)
    {
        if (currentHealth <= 0f)
        {
            return;
        }
        
        currentHealth -= amount;
        
        UpdateHealthBar();

        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            UpdateHealthBar();
            Die();
        }
    }

    private bool isDead = false;
    
    protected virtual void Die()
    {
        // Prevent multiple death calls
        if (isDead)
        {
            // Debug.Log($"💀 Enemy {gameObject.name} already dead, ignoring duplicate death call");
            return;
        }
        
        isDead = true;
        // Debug.Log($"💀 Enemy {gameObject.name} died! Health: {currentHealth}/{maxHealth}");

        // Play explosion effect if available
        // Note: ExplosionEffect script was removed - add particle effects here if needed

        if (gameManager != null)
        {
            Debug.Log($"💰 Adding resources and notifying spawner for {gameObject.name}");
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

        Debug.Log($"💀 Destroying enemy object: {gameObject.name}");
        Destroy(gameObject);
    }
    
    // Initializes the health bar UI element when the enemy spawns.
    // Sets the slider maximum to match enemy max health and current value to current health.
    // The slider will remain inactive if no health bar is assigned in the inspector.
    void InitializeHealthBar()
    {
        if (healthBarSlider != null)
        {
            healthBarSlider.maxValue = maxHealth;
            healthBarSlider.value = currentHealth;
        }
    }
    
    // Updates the health bar slider to reflect the current health value.
    // Called automatically when the enemy takes damage or heals.
    // Safe to call even when no health bar is assigned.
    void UpdateHealthBar()
    {
        if (healthBarSlider != null)
        {
            healthBarSlider.value = currentHealth;
        }
    }
    
}


