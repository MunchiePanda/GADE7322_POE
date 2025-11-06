using UnityEngine;
using UnityEngine.UI;
using GADE7322_POE.Core;
using System.Collections.Generic;

public class Defender : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] protected int hitPoints = 15;
    
    private Health healthComponent;

    [Header("UI")]
    [Tooltip("World space health bar slider (assign your UI slider here)")]
    [SerializeField] protected Slider healthBarSlider;


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

        healthComponent = GetComponent<Health>();
        if (healthComponent == null)
        {
        }
        else
        {
            healthComponent.OnDeath.AddListener(OnDefenderDeath);
        }

        AdjustColliderSize(1.5f);
        
        InitializeHealthBar();
    }

    /// <summary>
    /// Adjusts the collider size of the defender for better tracking.
    /// </summary>
    /// <param name="scaleFactor">The factor by which to scale the collider.</param>
    protected void AdjustColliderSize(float scaleFactor)
    {
        Collider collider = GetComponent<Collider>();
        if (collider == null)
        {
            // If no collider, try to find one in children
            collider = GetComponentInChildren<Collider>();
        }

        if (collider != null)
        {
            // Scale the collider size
            if (collider is BoxCollider boxCollider)
            {
                boxCollider.size *= scaleFactor;
            }
            else if (collider is SphereCollider sphereCollider)
            {
                sphereCollider.radius *= scaleFactor;
            }
            else if (collider is CapsuleCollider capsuleCollider)
            {
                capsuleCollider.radius *= scaleFactor;
                capsuleCollider.height *= scaleFactor;
            }
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
            Vector3 direction = (currentEnemyTarget.transform.position - transform.position).normalized;
            if (this is FrostTowerDefender)
            {
                FrostTowerDefender frostDefender = (FrostTowerDefender)this;
                if (Physics.SphereCast(transform.position, frostDefender.frostRadius, direction, out RaycastHit hit, frostDefender.frostRadius))
                {
                    frostDefender.PlayFrostParticleEffect(hit.point);
                }
            }
            else if (this is LightningTowerDefender)
            {
                LightningTowerDefender lightningDefender = (LightningTowerDefender)this;
                if (Physics.SphereCast(transform.position, lightningDefender.chainRange, direction, out RaycastHit hit, lightningDefender.chainRange))
                {
                    lightningDefender.PlayLightningParticleEffect(hit.point);
                }
            }
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

        // Special handling for Frost and Lightning defenders
        if (this is FrostTowerDefender)
        {
            // Use particle effects instead of projectiles for Frost Tower
            FrostTowerDefender frostDefender = (FrostTowerDefender)this;
            frostDefender.PlayFrostEffects();
            enemy.TakeDamage(finalDamage);
        }
        else if (this is LightningTowerDefender)
        {
            // Use particle effects instead of projectiles for Lightning Tower
            LightningTowerDefender lightningDefender = (LightningTowerDefender)this;
            lightningDefender.PlayLightningEffects(new List<Vector3> { enemy.transform.position });
            enemy.TakeDamage(finalDamage);
        }
        else
        {
            // Default projectile behavior for other defenders
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
    }

    public void TakeDamage(float amount)
    {
        if (!IsAlive()) return;
        
        if (healthComponent != null)
        {
            healthComponent.TakeDamage(amount);
            UpdateHealthBarFromHealthComponent();
        }
        else
        {
            hitPoints -= Mathf.RoundToInt(amount);
            UpdateHealthBar();
            
            if (hitPoints <= 0)
            {
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

    public Vector3 GetVisualCenter()
    {
        Renderer renderer = GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            return renderer.bounds.center;
        }
        return transform.position;
    }

    public bool UpgradeHealth()
    {
        if (gameManager == null) return false;

        if (gameManager.SpendResources(healthUpgradeCost))
        {
            hitPoints += Mathf.RoundToInt(healthUpgradeAmount);
            transform.localScale *= 1.1f;
            
            if (healthComponent != null)
            {
                healthComponent.MaxHealth += healthUpgradeAmount;
                healthComponent.Heal(healthUpgradeAmount);
            }
            
            InitializeHealthBar();
            
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
            if (gameManager.performanceTracker != null)
            {
                gameManager.performanceTracker.OnDefenderLost();
            }
            
            gameManager.OnDefenderDestroyed();
        }
    }
    
    void InitializeHealthBar()
    {
        if (healthBarSlider != null)
        {
            if (healthComponent != null)
            {
                healthBarSlider.maxValue = healthComponent.MaxHealth;
                healthBarSlider.value = healthComponent.CurrentHealth;
            }
            else
            {
                healthBarSlider.maxValue = hitPoints;
                healthBarSlider.value = hitPoints;
            }
        }
    }
    
    void UpdateHealthBar()
    {
        if (healthBarSlider != null)
        {
            healthBarSlider.value = hitPoints;
        }
    }
    
    void UpdateHealthBarFromHealthComponent()
    {
        if (healthBarSlider != null && healthComponent != null)
        {
            healthBarSlider.value = healthComponent.CurrentHealth;
        }
    }
    
}