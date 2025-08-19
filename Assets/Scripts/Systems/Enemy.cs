using UnityEngine;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float maxHealth = 50f;
    [SerializeField] private float currentHealth = 0f;
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float attackDamage = 5f;
    [SerializeField] private float attackIntervalSeconds = 1.0f;
    [SerializeField] private float detectionRange = 2.0f;
    [SerializeField] private int resourceRewardOnDeath = 10;

    private List<Vector3Int> path;
    private int currentPathIndex = 0;
    private int terrainHeight = 1; // legacy; not used for uneven per-tile heights
    private int finalIndex = 0;
    private VoxelTerrainGenerator terrainGenerator;
    private Tower targetTower;
    private GameManager gameManager;

    // Combat state
    private Defender currentDefenderTarget;
    private float lastAttackTime = -999f;

    public void Initialize(List<Vector3Int> pathToFollow, int terrainTopY, Tower tower, GameManager gm, VoxelTerrainGenerator generator)
    {
        path = pathToFollow;
        terrainHeight = terrainTopY;
        targetTower = tower;
        gameManager = gm;
        terrainGenerator = generator;
        finalIndex = path != null && path.Count > 0 ? path.Count - 1 : 0;
    }

    void Start()
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
        if (path == null || path.Count == 0)
            return;

        Vector3Int grid = path[currentPathIndex];
        Vector3 targetPos = terrainGenerator != null
            ? terrainGenerator.GetSurfaceWorldPosition(grid)
            : new Vector3(grid.x, terrainHeight, grid.z);
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
            transform.position += toTarget.normalized * step;
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

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        if (gameManager != null)
        {
            gameManager.AddResources(resourceRewardOnDeath);
        }
        Destroy(gameObject);
    }
}


