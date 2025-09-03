using UnityEngine;

public class Defender : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] protected float maxHealth = 60f;
    [SerializeField] protected float currentHealth = 0f;

    [Header("Combat")]
    [SerializeField] protected float attackDamage = 10f;
    [SerializeField] protected float attackIntervalSeconds = 0.8f;
    [SerializeField] protected float attackRange = 5f;
    [SerializeField] protected LayerMask enemyMask = ~0;

    [Header("Upgrade Settings")]
    [SerializeField] private int healthUpgradeCost = 30;
    [SerializeField] private int damageUpgradeCost = 40;
    [SerializeField] private float healthUpgradeAmount = 15f;
    [SerializeField] private float damageUpgradeAmount = 2f;

    private float lastAttackTime = -999f;
    protected Enemy currentEnemyTarget;
    private GameManager gameManager;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        gameManager = FindFirstObjectByType<GameManager>();
    }

    void Update()
    {
        if (!IsAlive()) return;

        AcquireEnemyIfAny();
        TryAttackEnemy();
    }

    void AcquireEnemyIfAny()
    {
        if (currentEnemyTarget != null)
        {
            float dist = Vector3.Distance(transform.position, currentEnemyTarget.transform.position);
            if (dist <= attackRange && currentEnemyTarget != null)
                return;
        }
        currentEnemyTarget = null;
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange, enemyMask);
        float nearest = float.MaxValue;
        foreach (var hit in hits)
        {
            Enemy enemy = hit.GetComponentInParent<Enemy>();
            if (enemy != null)
            {
                float d = Vector3.Distance(transform.position, enemy.transform.position);
                if (d < nearest)
                {
                    nearest = d;
                    currentEnemyTarget = enemy;
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
            currentEnemyTarget.TakeDamage(attackDamage);
        }
    }

    public void TakeDamage(float amount)
    {
        if (!IsAlive()) return;
        currentHealth -= amount;
        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            // Play explosion effect
            ExplosionEffect explosionEffect = GetComponent<ExplosionEffect>();
            if (explosionEffect != null)
            {
                explosionEffect.PlayExplosion();
            }
            Destroy(gameObject);
        }
    }

    public bool IsAlive()
    {
        return currentHealth > 0f;
    }

    // Upgrade methods
    public bool UpgradeHealth()
    {
        if (gameManager == null) return false;

        if (gameManager.SpendResources(healthUpgradeCost))
        {
            maxHealth += healthUpgradeAmount;
            currentHealth = maxHealth; // Fully heal on upgrade
            Debug.Log($"Defender health upgraded! New max health: {maxHealth}");
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
            Debug.Log($"Defender damage upgraded! New damage: {attackDamage}");
            // Visual feedback (e.g., change color or add particles)
            return true;
        }
        return false;
    }
}


