using UnityEngine;

public class Defender : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float maxHealth = 60f;
    [SerializeField] private float currentHealth = 0f;

    [Header("Combat")]
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackIntervalSeconds = 0.8f;
    [SerializeField] private float attackRange = 5f;
    [SerializeField] private LayerMask enemyMask = ~0;

    private float lastAttackTime = -999f;
    private Enemy currentEnemyTarget;

    void Start()
    {
        currentHealth = maxHealth;
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
            Destroy(gameObject);
        }
    }

    public bool IsAlive()
    {
        return currentHealth > 0f;
    }
}


