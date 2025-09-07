using UnityEngine;

public class Tower : MonoBehaviour
{
    [Header("Tower Stats")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [Header("Combat")]
    [SerializeField] private float baseAttackRange = 15f;
    [SerializeField] private float extendedAttackRange = 35f;
    private float currentAttackRange;
    public float attackDamage = 8f;
    [SerializeField] private float attackIntervalSeconds = 0.7f;
    [SerializeField] private LayerMask enemyMask = ~0;
    public GameObject projectilePrefab;
    public float projectileSpeed = 10f;
    public Transform projectileSpawnPoint;
    private float lastAttackTime = -999f;

    [Header("Upgrade Settings")]
    [SerializeField] public int healthUpgradeCost = 50;
    [SerializeField] public int damageUpgradeCost = 75;
    [SerializeField] private float healthUpgradeAmount = 20f;
    [SerializeField] private float damageUpgradeAmount = 3f;

    [Header("References")]
    [SerializeField] private GameManager gameManager;

    void Start()
    {
        currentHealth = maxHealth;
        currentAttackRange = baseAttackRange;

        // Get GameManager reference if not assigned
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }

        // Update UI with initial health
        UpdateHealthUI();
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth); // Don't go below 0

        UpdateHealthUI();

        // Visual feedback (you can add particle effects, screen shake, etc.)
        Debug.Log($"Tower took {damage} damage! Health: {currentHealth}/{maxHealth}");

        // Check if tower is destroyed
        if (currentHealth <= 0)
        {
            OnTowerDestroyed();
        }
    }

    public void Heal(float healAmount)
    {
        currentHealth += healAmount;
        currentHealth = Mathf.Min(maxHealth, currentHealth); // Don't exceed max health

        UpdateHealthUI();

        Debug.Log($"Tower healed {healAmount}! Health: {currentHealth}/{maxHealth}");
    }

    private void UpdateHealthUI()
    {
        if (gameManager != null && gameManager.towerHealthBar != null)
        {
            gameManager.towerHealthBar.SetHealth(currentHealth, maxHealth);
        }
    }

    private void OnTowerDestroyed()
    {
        Debug.Log("Tower destroyed! Game Over!");

        if (gameManager != null)
        {
            gameManager.GameOver();
        }
    }

    void Update()
    {
        AutoAttackNearestEnemy();
    }

    void AutoAttackNearestEnemy()
    {
        float time = Time.time;
        if (time - lastAttackTime < attackIntervalSeconds) return;
        Enemy nearest = null;
        float nearestDist = float.MaxValue;
        Collider[] hits = Physics.OverlapSphere(transform.position, currentAttackRange, enemyMask);
        foreach (var hit in hits)
        {
            Enemy e = hit.GetComponentInParent<Enemy>();
            if (e != null)
            {
                float d = Vector3.Distance(transform.position, e.transform.position);
                if (d < nearestDist)
                {
                    nearestDist = d;
                    nearest = e;
                }
            }
        }
        if (nearest != null)
        {
            lastAttackTime = time;
            LobProjectileAtEnemy(nearest);
        }
    }

    public void LobProjectileAtEnemy(Enemy enemy)
    {
        if (projectilePrefab == null)
        {
            Debug.LogError("Projectile prefab is not assigned!");
            return;
        }

        Vector3 spawnPosition = projectileSpawnPoint != null ? projectileSpawnPoint.position : transform.position;
        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
        Projectile projectileComponent = projectile.GetComponent<Projectile>();
        if (projectileComponent == null)
        {
            Debug.LogError("Projectile prefab does not have a Projectile component!");
            return;
        }

        projectileComponent.Initialize(enemy.transform, attackDamage, projectileSpeed);
    }

    // Public getters
    public float GetCurrentHealth() { return currentHealth; }
    public float GetMaxHealth() { return maxHealth; }
    public float GetHealthPercentage() { return currentHealth / maxHealth; }

    // Public setters
    public void SetMaxHealth(float newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        UpdateHealthUI();
    }

    // Upgrade methods
    public bool UpgradeHealth()
    {
        if (gameManager == null) return false;

        if (gameManager.SpendResources(healthUpgradeCost))
        {
            maxHealth += healthUpgradeAmount;
            currentHealth = maxHealth; // Fully heal on upgrade
            UpdateHealthUI();
            Debug.Log($"Tower health upgraded! New max health: {maxHealth}");
            transform.localScale *= 1.1f; // Visual feedback
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
            Debug.Log($"Tower damage upgraded! New damage: {attackDamage}");
            // Visual feedback (e.g., change color or add particles)
            return true;
        }
        return false;
    }

    public void ExtendAttackRange(bool extend)
    {
        currentAttackRange = extend ? extendedAttackRange : baseAttackRange;
        Debug.Log($"Tower attack range set to: {currentAttackRange}");
    }
}