using UnityEngine;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] protected float maxHealth = 50f;
    [SerializeField] protected float currentHealth = 0f;
    [SerializeField] protected float moveSpeed = 2.5f;
    [SerializeField] protected float attackDamage = 5f;
    [SerializeField] protected float attackIntervalSeconds = 1.0f;
    [SerializeField] protected float detectionRange = 2.0f;
    [SerializeField] protected int minResourceRewardOnDeath = 5;
    [SerializeField] protected int maxResourceRewardOnDeath = 15;

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

    private float yOffset = 1f;

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

    protected virtual void Start()
    {
        currentHealth = maxHealth;
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

    void FollowPathTowardsTower()
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

    void AcquireDefenderIfAny()
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
        float nearest = float.MaxValue;
        foreach (var hit in hits)
        {
            Defender defender = hit.GetComponentInParent<Defender>();
            if (defender != null && defender.IsAlive())
            {
                float d = Vector3.Distance(transform.position, defender.transform.position);
                if (d < nearest)
                {
                    nearest = d;
                    currentDefenderTarget = defender;
                }
            }
        }
    }

    void TryAttackDefender()
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
        }
    }

    public virtual void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        // Play explosion effect
        ExplosionEffect explosionEffect = GetComponent<ExplosionEffect>();
        if (explosionEffect != null)
        {
            explosionEffect.PlayExplosion();
        }

        if (gameManager != null)
        {
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
        Destroy(gameObject);
    }
}


