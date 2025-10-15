using UnityEngine;
using GADE7322_POE.Core;

public class Defender : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] protected int hitPoints = 15;
    
    // Health component reference
    private Health healthComponent;


    [Header("Combat")]
    [SerializeField] protected float attackDamage = 5f;
    [SerializeField] protected float attackIntervalSeconds = 0.8f;
    [SerializeField] protected float attackRange = 5f;
    [SerializeField] protected LayerMask enemyMask = ~0;
    [SerializeField] protected GameObject projectilePrefab;
    [SerializeField] protected Transform projectileSpawnPoint;
    [SerializeField] protected float projectileSpeed = 10f;

    [Header("Upgrade Settings")]
    [SerializeField] private int healthUpgradeCost = 30;
    [SerializeField] private int damageUpgradeCost = 40;
    [SerializeField] private float healthUpgradeAmount = 15f;
    [SerializeField] private float damageUpgradeAmount = 2f;

    public float lastAttackTime = -999f;
    protected Enemy currentEnemyTarget;
    private GameManager gameManager;
    private CriticalHitSystem criticalHitSystem;

    protected virtual void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        criticalHitSystem = FindFirstObjectByType<CriticalHitSystem>();
        
        // Get the Health component
        healthComponent = GetComponent<Health>();
        if (healthComponent == null)
        {
            // Debug logging disabled
        }
        else
        {
            // Set up the OnDeath event to notify performance tracker
            healthComponent.OnDeath.AddListener(OnDefenderDeath);
        }
        
    }

    void Update()
    {
        if (!IsAlive()) return;

        AcquireEnemyIfAny();
        TryAttackEnemy();
    }

    protected void AcquireEnemyIfAny()
    {
        if (currentEnemyTarget != null)
        {
            float dist = Vector3.Distance(transform.position, currentEnemyTarget.transform.position);
            if (dist <= attackRange && currentEnemyTarget != null)
                return;
        }
        currentEnemyTarget = null;
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange, enemyMask);
        // Debug.Log($"Defender {gameObject.name} looking for enemies in range {attackRange}, found {hits.Length} colliders");
        float nearest = float.MaxValue;
        foreach (var hit in hits)
        {
            Enemy enemy = hit.GetComponentInParent<Enemy>();
            if (enemy != null)
            {
                float d = Vector3.Distance(transform.position, enemy.transform.position);
                // Debug.Log($"Defender found enemy {enemy.name} at distance {d:F2}");
                if (d < nearest)
                {
                    nearest = d;
                    currentEnemyTarget = enemy;
                }
            }
        }
        
        if (currentEnemyTarget != null)
        {
            // Debug.Log($"Defender {gameObject.name} acquired target: {currentEnemyTarget.name}");
        }
    }

    void TryAttackEnemy()
    {
        if (currentEnemyTarget == null) return;
        float time = Time.time;
        if (time - lastAttackTime >= attackIntervalSeconds)
        {
            lastAttackTime = time;
            // Debug.Log($"Defender {gameObject.name} attacking enemy {currentEnemyTarget.name}!");
            LobProjectileAtEnemy(currentEnemyTarget);
        }
    }

    void LobProjectileAtEnemy(Enemy enemy)
    {
        if (projectilePrefab == null)
        {
            // Debug logging disabled
            return;
        }

        // Calculate critical hit
        bool isCritical = false;
        float finalDamage = attackDamage;
        
        if (criticalHitSystem != null)
        {
            isCritical = criticalHitSystem.RollCriticalHit();
            finalDamage = criticalHitSystem.CalculateDamage(attackDamage, isCritical);
        }

        Vector3 spawnPosition = projectileSpawnPoint != null ? projectileSpawnPoint.position : transform.position;
        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
        Projectile projectileComponent = projectile.GetComponent<Projectile>();
        if (projectileComponent == null)
        {
            // Debug logging disabled
            Destroy(projectile);
            return;
        }

        // Initialize projectile with critical hit info
        projectileComponent.Initialize(enemy.transform, finalDamage, projectileSpeed, isCritical);
        
        if (isCritical)
        {
            // Debug logging disabled
        }
        else
        {
            // Debug logging disabled
        }
    }

    public void TakeDamage(float amount)
    {
        if (!IsAlive()) return;
        
        // Debug logging disabled
        
        // Use Health component if available, otherwise use hit points system
        if (healthComponent != null)
        {
            healthComponent.TakeDamage(amount);
        }
        else
        {
            // Fallback to hit points system
            hitPoints -= Mathf.RoundToInt(amount);
            // Debug logging disabled
            
            if (hitPoints <= 0)
            {
                // Debug logging disabled
                NotifyDefenderLoss();
                Destroy(gameObject);
            }
        }
        
    }

    public bool IsAlive()
    {
        if (healthComponent != null)
        {
            return healthComponent.CurrentHealth > 0;
        }
        return hitPoints > 0;
    }

    public bool UpgradeHealth()
    {
        if (gameManager == null) return false;

        if (gameManager.SpendResources(healthUpgradeCost))
        {
            hitPoints += Mathf.RoundToInt(healthUpgradeAmount);
            // Debug logging disabled
            transform.localScale *= 1.1f;
            
            return true;
        }
        return false;
    }

    public bool UpgradeDamage()
    {
        if (gameManager == null) return false;

        if (gameManager.SpendResources(damageUpgradeCost))
        {
            attackDamage += damageUpgradeAmount;
            // Debug logging disabled
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Called when the defender dies (via Health component)
    /// </summary>
    private void OnDefenderDeath()
    {
        // Debug logging disabled
        NotifyDefenderLoss();
    }
    
    /// <summary>
    /// Notifies the performance tracker of defender loss
    /// </summary>
    private void NotifyDefenderLoss()
    {
        if (gameManager != null)
        {
            // Notify performance tracker
            if (gameManager.performanceTracker != null)
            {
                gameManager.performanceTracker.OnDefenderLost();
            }
            
            // Notify GameManager of defender destruction for count tracking
            gameManager.OnDefenderDestroyed();
        }
    }
    
}