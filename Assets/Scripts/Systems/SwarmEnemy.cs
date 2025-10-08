using UnityEngine;

/// <summary>
/// Swarm enemy type that is weak individually but spawns in groups.
/// Fast, low health enemies that overwhelm through numbers.
/// </summary>
public class SwarmEnemy : Enemy
{
    [Header("Swarm Settings")]
    [Tooltip("How many swarm enemies spawn together.")]
    public static int swarmSize = 3;
    
    [Tooltip("Random offset range for swarm positioning.")]
    public float swarmSpread = 2f;

    protected override void Start()
    {
        base.Start();
        
        // Swarm specific stats - weak but fast
        maxHealth = 8f;
        currentHealth = maxHealth;
        moveSpeed = 3.5f; // Faster than default
        attackDamage = 3f; // Lower damage
        attackIntervalSeconds = 0.8f; // Slightly faster attacks
        
        // Higher resource reward to balance the numbers
        minResourceRewardOnDeath = 3;
        maxResourceRewardOnDeath = 6;
        
        Debug.Log($"SwarmEnemy initialized with health: {currentHealth}, speed: {moveSpeed}");
    }

    public override void TakeDamage(float amount)
    {
        // Swarm enemies take normal damage but die quickly
        base.TakeDamage(amount);
    }

    /// <summary>
    /// Helper method for EnemySpawner to spawn multiple swarm enemies at once.
    /// </summary>
    public static void SpawnSwarmAt(Vector3 basePosition, GameObject swarmPrefab, 
        System.Collections.Generic.List<Vector3Int> path, int terrainHeight, 
        Tower tower, GameManager gameManager)
    {
        for (int i = 0; i < swarmSize; i++)
        {
            // Create random offset for each swarm member
            Vector3 offset = new Vector3(
                Random.Range(-2f, 2f), 
                0, 
                Random.Range(-2f, 2f)
            );
            
            Vector3 spawnPosition = basePosition + offset;
            
            GameObject swarmEnemy = Instantiate(swarmPrefab, spawnPosition, Quaternion.identity);
            SwarmEnemy enemyComponent = swarmEnemy.GetComponent<SwarmEnemy>();
            
            if (enemyComponent != null)
            {
                enemyComponent.Initialize(path, terrainHeight, tower, gameManager);
            }
        }
    }
}