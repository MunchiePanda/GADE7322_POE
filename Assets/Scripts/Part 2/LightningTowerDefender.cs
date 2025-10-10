using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Lightning Tower defender with chain lightning attacks.
/// Fast attack speed with chain damage to multiple enemies.
/// </summary>
public class LightningTowerDefender : Defender
{
    [Header("Lightning Tower Settings")]
    [Tooltip("Maximum number of enemies the lightning can chain to")]
    public int maxChainTargets = 3;
    
    [Tooltip("Maximum distance for chain lightning to jump")]
    public float chainRange = 4f;
    
    [Tooltip("Damage reduction per chain (0.8 = 20% damage reduction per jump)")]
    [Range(0.1f, 1f)]
    public float chainDamageReduction = 0.8f;
    
    [Tooltip("Visual effect for lightning")]
    public GameObject lightningEffectPrefab;
    
    [Tooltip("Line renderer for lightning visual")]
    public LineRenderer lightningLine;
    
    [Header("Visual Feedback")]
    [Tooltip("Color of lightning effects")]
    public Color lightningColor = Color.yellow;
    
    [Tooltip("Lightning projectile visual")]
    public GameObject lightningProjectilePrefab;

    protected override void Start()
    {
        base.Start();
        // Lightning tower characteristics
        attackRange = 8f;           // Medium range
        attackDamage = 15f;         // High initial damage
        attackIntervalSeconds = 1.2f; // Fast attack speed
        
        // Visual setup
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.yellow;
        }
        else
        {
            // Add a renderer if none exists
            renderer = gameObject.AddComponent<MeshRenderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = Color.yellow;
            renderer.material = mat;
        }
    }

    void Update()
    {
        if (!IsAlive()) return;

        AcquireEnemyIfAny();
        TryLightningAttack();
        
        // Debug current state
        if (currentEnemyTarget != null)
        {
            Debug.Log($"Lightning Tower: Aiming at {currentEnemyTarget.name} (Distance: {Vector3.Distance(transform.position, currentEnemyTarget.transform.position):F1})");
        }
    }

    void TryLightningAttack()
    {
        if (currentEnemyTarget == null) 
        {
            Debug.Log("Lightning Tower: No target in range");
            return;
        }
        
        float time = Time.time;
        float timeSinceLastAttack = time - lastAttackTime;
        
        if (timeSinceLastAttack >= attackIntervalSeconds)
        {
            Debug.Log($"Lightning Tower: ATTACKING! Target: {currentEnemyTarget.name}, Cooldown: {timeSinceLastAttack:F1}s");
            lastAttackTime = time;
            PerformChainLightning();
        }
        else
        {
            Debug.Log($"Lightning Tower: On cooldown ({timeSinceLastAttack:F1}s / {attackIntervalSeconds}s)");
        }
    }

    void PerformChainLightning()
    {
        Debug.Log($"Lightning Tower: Starting chain lightning attack (Max targets: {maxChainTargets})");
        
        List<Enemy> chainedEnemies = new List<Enemy>();
        List<Vector3> lightningPoints = new List<Vector3>();
        
        // Start with the primary target
        Enemy currentTarget = currentEnemyTarget;
        float currentDamage = attackDamage;
        
        for (int i = 0; i < maxChainTargets && currentTarget != null; i++)
        {
            float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
            Debug.Log($"Lightning Tower: Chain {i + 1} - HIT {currentTarget.name} at distance {distance:F1} for {currentDamage:F1} damage");
            
            // Deal damage to current target
            currentTarget.TakeDamage(currentDamage);
            chainedEnemies.Add(currentTarget);
            lightningPoints.Add(currentTarget.transform.position);
            
            // Find next target for chain
            Enemy nextTarget = FindNextChainTarget(currentTarget, chainedEnemies);
            if (nextTarget != null)
            {
                float chainDistance = Vector3.Distance(currentTarget.transform.position, nextTarget.transform.position);
                Debug.Log($"Lightning Tower: Chain {i + 1} -> {i + 2}: Jumping to {nextTarget.name} at distance {chainDistance:F1}");
            }
            else
            {
                Debug.Log($"Lightning Tower: Chain {i + 1} -> No more targets in range");
            }
            
            currentTarget = nextTarget;
            
            // Reduce damage for next chain
            currentDamage *= chainDamageReduction;
        }
        
        Debug.Log($"Lightning Tower: Chain attack complete! Hit {chainedEnemies.Count} enemies");
        
        // Play visual effects
        PlayLightningEffects(lightningPoints);
    }

    Enemy FindNextChainTarget(Enemy fromEnemy, List<Enemy> alreadyHit)
    {
        Collider[] nearbyEnemies = Physics.OverlapSphere(fromEnemy.transform.position, chainRange);
        Debug.Log($"Lightning Tower: Searching for chain targets around {fromEnemy.name} (Range: {chainRange}, Found: {nearbyEnemies.Length} colliders)");
        
        float closestDistance = float.MaxValue;
        Enemy closestEnemy = null;
        
        foreach (Collider enemyCollider in nearbyEnemies)
        {
            Enemy enemy = enemyCollider.GetComponent<Enemy>();
            if (enemy != null && !alreadyHit.Contains(enemy))
            {
                float distance = Vector3.Distance(fromEnemy.transform.position, enemy.transform.position);
                Debug.Log($"Lightning Tower: Found potential target {enemy.name} at distance {distance:F1}");
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy;
                }
            }
        }
        
        if (closestEnemy != null)
        {
            Debug.Log($"Lightning Tower: Selected next target: {closestEnemy.name} at distance {closestDistance:F1}");
        }
        else
        {
            Debug.Log("Lightning Tower: No valid chain targets found");
        }
        
        return closestEnemy;
    }

    void PlayLightningEffects(List<Vector3> lightningPoints)
    {
        if (lightningLine != null && lightningPoints.Count > 1)
        {
            lightningLine.positionCount = lightningPoints.Count;
            lightningLine.SetPositions(lightningPoints.ToArray());
            lightningLine.material.color = lightningColor;
            
            // Animate the lightning
            StartCoroutine(AnimateLightning());
        }
        
        // Create lightning projectile visual
        if (lightningProjectilePrefab != null)
        {
            GameObject projectile = Instantiate(lightningProjectilePrefab, transform.position, Quaternion.identity);
            Destroy(projectile, 1f);
        }
        
        // Simple visual feedback without prefabs
        CreateSimpleLightningEffect();
        
        // Debug visual feedback
        Debug.Log($"Lightning Tower attacking! Chain targets: {lightningPoints.Count}, Target: {currentEnemyTarget?.name}");
    }

    System.Collections.IEnumerator AnimateLightning()
    {
        lightningLine.enabled = true;
        yield return new WaitForSeconds(0.2f);
        lightningLine.enabled = false;
    }
    
    void CreateSimpleLightningEffect()
    {
        // Create a simple visual effect without requiring prefabs
        GameObject effect = GameObject.CreatePrimitive(PrimitiveType.Cube);
        effect.transform.position = transform.position;
        effect.transform.localScale = Vector3.one * 0.3f;
        effect.name = "LightningEffect";
        
        // Make it yellow and bright
        Renderer renderer = effect.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = lightningColor;
        mat.SetFloat("_Emission", 1f); // Make it glow
        renderer.material = mat;
        
        // Remove collider
        Destroy(effect.GetComponent<Collider>());
        
        // Destroy after short time
        Destroy(effect, 0.3f);
    }
}
