using UnityEngine;

/*
 * Tower.cs
 * --------
 * This script manages the behavior of the central tower in the game.
 * The tower has health, can attack enemies, and can be upgraded.
 *
 * Attach this script to the tower prefab in the game.
 */
public class Tower : MonoBehaviour
{
    [Header("Tower Stats")]
    [Tooltip("The maximum health of the tower.")]
    [SerializeField] private float maxHealth = 200f;
    [Tooltip("The current health of the tower.")]
    [SerializeField] private float currentHealth;

    [Header("Combat")]
    [Tooltip("The base attack range of the tower.")]
    [SerializeField] private float baseAttackRange = 8f;
    [Tooltip("The extended attack range of the tower (used when upgraded).")]
    [SerializeField] private float extendedAttackRange = 12f;
    [Tooltip("The current attack range of the tower.")]
    private float currentAttackRange;
    [Tooltip("The amount of damage the tower deals per attack.")]
    public float attackDamage = 4f;
    [Tooltip("The time interval (in seconds) between attacks.")]
    [SerializeField] private float attackIntervalSeconds = 0.7f;
    [Tooltip("The layer mask used to detect enemies.")]
    [SerializeField] private LayerMask enemyMask = ~0;
    [Tooltip("The prefab of the projectile the tower shoots.")]
    public GameObject projectilePrefab;
    [Tooltip("The speed at which the projectile travels.")]
    public float projectileSpeed = 10f;
    [Tooltip("The transform from which projectiles are spawned.")]
    public Transform projectileSpawnPoint;
    [Tooltip("The time at which the tower last attacked.")]
    private float lastAttackTime = -999f;

    [Header("Upgrade Settings")]
    [Tooltip("The cost in resources to upgrade the tower's health.")]
    [SerializeField] public int healthUpgradeCost = 50;
    [Tooltip("The cost in resources to upgrade the tower's damage.")]
    [SerializeField] public int damageUpgradeCost = 75;
    [Tooltip("The amount of health added when upgrading the tower's health.")]
    [SerializeField] private float healthUpgradeAmount = 20f;
    [Tooltip("The amount of damage added when upgrading the tower's damage.")]
    [SerializeField] private float damageUpgradeAmount = 3f;

    [Header("References")]
    [Tooltip("Reference to the GameManager for resource management and game state.")]
    [SerializeField] private GameManager gameManager;

    void Start()
    {
        // Initialize the tower's current health to its maximum health.
        currentHealth = maxHealth;
        currentAttackRange = baseAttackRange;

        // Get a reference to the GameManager if not already assigned.
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }

        // Update the UI to display the tower's initial health.
        UpdateHealthUI();
    }

    /// <summary>
    /// Reduces the tower's health by the specified damage amount.
    /// </summary>
    /// <param name="damage">The amount of damage to apply.</param>
    public void TakeDamage(float damage)
    {
        // Reduce the tower's health by the damage amount.
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth); // Ensure health doesn't go below zero.

        // Update the UI to reflect the new health.
        UpdateHealthUI();

        // Log the damage taken for debugging.
        // Debug.Log($"Tower took {damage} damage! Health: {currentHealth}/{maxHealth}");

        // Check if the tower's health has reached zero.
        if (currentHealth <= 0)
        {
            OnTowerDestroyed();
        }
    }

    /// <summary>
    /// Increases the tower's health by the specified amount.
    /// </summary>
    /// <param name="healAmount">The amount of health to restore.</param>
    public void Heal(float healAmount)
    {
        // Increase the tower's health by the heal amount, without exceeding max health.
        currentHealth += healAmount;
        currentHealth = Mathf.Min(maxHealth, currentHealth);

        // Update the UI to reflect the new health.
        UpdateHealthUI();

        // Log the healing for debugging.
        // Debug.Log($"Tower healed {healAmount}! Health: {currentHealth}/{maxHealth}");
    }

    /// <summary>
    /// Updates the UI to display the tower's current health.
    /// </summary>
    private void UpdateHealthUI()
    {
        // Update the health bar UI if the GameManager and health bar are available.
        if (gameManager != null && gameManager.towerHealthBar != null)
        {
            gameManager.towerHealthBar.SetHealth(currentHealth, maxHealth);
        }
    }

    /// <summary>
    /// Called when the tower's health reaches zero.
    /// </summary>
    private void OnTowerDestroyed()
    {
        // Log the tower's destruction.
        // Debug.Log("Tower destroyed! Game Over!");

        // Notify the GameManager that the game is over.
        if (gameManager != null)
        {
            gameManager.GameOver();
        }
    }

    void Update()
    {
        // Automatically attack the nearest enemy within range.
        AutoAttackNearestEnemy();
    }

    /// <summary>
    /// Automatically attacks the nearest enemy within the tower's attack range.
    /// </summary>
    void AutoAttackNearestEnemy()
    {
        // Check if enough time has passed since the last attack.
        float time = Time.time;
        if (time - lastAttackTime < attackIntervalSeconds) return;

        // Find the nearest enemy within the tower's attack range.
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

        // If an enemy is found, attack it.
        if (nearest != null)
        {
            lastAttackTime = time;
            LobProjectileAtEnemy(nearest);
        }
    }

    /// <summary>
    /// Creates and launches a projectile at the specified enemy.
    /// </summary>
    /// <param name="enemy">The enemy to target with the projectile.</param>
    public void LobProjectileAtEnemy(Enemy enemy)
    {
        // Exit if the projectile prefab is not assigned.
        if (projectilePrefab == null)
        {
            // Debug.LogError("Projectile prefab is not assigned!");
            return;
        }

        // Determine the spawn position for the projectile.
        Vector3 spawnPosition = projectileSpawnPoint != null ? projectileSpawnPoint.position : transform.position;

        // Instantiate the projectile and initialize it.
        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
        Projectile projectileComponent = projectile.GetComponent<Projectile>();
        if (projectileComponent == null)
        {
            // Debug.LogError("Projectile prefab does not have a Projectile component!");
            return;
        }

        // Initialize the projectile to target the enemy.
        projectileComponent.Initialize(enemy.transform, attackDamage, projectileSpeed);
    }

    // Public getters
    /// <summary>
    /// Gets the tower's current health.
    /// </summary>
    /// <returns>The current health of the tower.</returns>
    public float GetCurrentHealth() { return currentHealth; }

    /// <summary>
    /// Gets the tower's maximum health.
    /// </summary>
    /// <returns>The maximum health of the tower.</returns>
    public float GetMaxHealth() { return maxHealth; }

    /// <summary>
    /// Gets the tower's health as a percentage of its maximum health.
    /// </summary>
    /// <returns>The health percentage of the tower.</returns>
    public float GetHealthPercentage() { return currentHealth / maxHealth; }

    // Public setters
    /// <summary>
    /// Sets the tower's maximum health.
    /// </summary>
    /// <param name="newMaxHealth">The new maximum health value.</param>
    public void SetMaxHealth(float newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        UpdateHealthUI();
    }

    // Upgrade methods
    /// <summary>
    /// Upgrades the tower's health if the player has enough resources.
    /// </summary>
    /// <returns>True if the upgrade was successful, false otherwise.</returns>
    public bool UpgradeHealth()
    {
        // Exit if the GameManager is not available.
        if (gameManager == null) return false;

        // Attempt to spend resources for the upgrade.
        if (gameManager.SpendResources(healthUpgradeCost))
        {
            // Increase the tower's maximum health and fully heal it.
            maxHealth += healthUpgradeAmount;
            currentHealth = maxHealth;
            UpdateHealthUI();

            // Log the upgrade for debugging.
            // Debug.Log($"Tower health upgraded! New max health: {maxHealth}");

            // Provide visual feedback by scaling the tower.
            transform.localScale *= 1.1f;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Upgrades the tower's damage if the player has enough resources.
    /// </summary>
    /// <returns>True if the upgrade was successful, false otherwise.</returns>
    public bool UpgradeDamage()
    {
        // Exit if the GameManager is not available.
        if (gameManager == null) return false;

        // Attempt to spend resources for the upgrade.
        if (gameManager.SpendResources(damageUpgradeCost))
        {
            // Increase the tower's attack damage.
            attackDamage += damageUpgradeAmount;

            // Log the upgrade for debugging.
            // Debug.Log($"Tower damage upgraded! New damage: {attackDamage}");

            // Visual feedback can be added here (e.g., change color or add particles).
            return true;
        }
        return false;
    }

    /// <summary>
    /// Extends or resets the tower's attack range.
    /// </summary>
    /// <param name="extend">If true, sets the attack range to the extended range; otherwise, resets to the base range.</param>
    public void ExtendAttackRange(bool extend)
    {
        // Set the current attack range based on the extend parameter.
        currentAttackRange = extend ? extendedAttackRange : baseAttackRange;

        // Log the new attack range for debugging.
        // Debug.Log($"Tower attack range set to: {currentAttackRange}");
    }
    
    /// <summary>
    /// Upgrades the tower's attack speed by reducing attack interval.
    /// </summary>
    /// <returns>True if upgrade was successful, false otherwise.</returns>
    public bool UpgradeAttackSpeed()
    {
        if (gameManager == null) return false;

        if (gameManager.SpendResources(damageUpgradeCost)) // Using damage cost for now
        {
            attackIntervalSeconds *= 0.8f; // 20% faster attack speed
            attackIntervalSeconds = Mathf.Max(0.1f, attackIntervalSeconds); // Minimum attack speed
            return true;
        }
        return false;
    }
}