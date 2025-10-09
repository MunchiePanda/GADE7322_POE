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

    protected override void Start()
    {
        base.Start();
        // Lightning tower characteristics
        attackRange = 8f;           // Medium range
        attackDamage = 15f;         // High initial damage
        attackIntervalSeconds = 1.2f; // Fast attack speed
        
        // Visual setup
        GetComponent<Renderer>().material.color = Color.yellow;
    }

    void Update()
    {
        if (!IsAlive()) return;

        AcquireEnemyIfAny();
        TryLightningAttack();
    }

    void TryLightningAttack()
    {
        if (currentEnemyTarget == null) return;
        
        float time = Time.time;
        if (time - lastAttackTime >= attackIntervalSeconds)
        {
            lastAttackTime = time;
            PerformChainLightning();
        }
    }

    void PerformChainLightning()
    {
        List<Enemy> chainedEnemies = new List<Enemy>();
        List<Vector3> lightningPoints = new List<Vector3>();
        
        // Start with the primary target
        Enemy currentTarget = currentEnemyTarget;
        float currentDamage = attackDamage;
        
        for (int i = 0; i < maxChainTargets && currentTarget != null; i++)
        {
            // Deal damage to current target
            currentTarget.TakeDamage(currentDamage);
            chainedEnemies.Add(currentTarget);
            lightningPoints.Add(currentTarget.transform.position);
            
            // Find next target for chain
            currentTarget = FindNextChainTarget(currentTarget, chainedEnemies);
            
            // Reduce damage for next chain
            currentDamage *= chainDamageReduction;
        }
        
        // Play visual effects
        PlayLightningEffects(lightningPoints);
    }

    Enemy FindNextChainTarget(Enemy fromEnemy, List<Enemy> alreadyHit)
    {
        Collider[] nearbyEnemies = Physics.OverlapSphere(fromEnemy.transform.position, chainRange);
        float closestDistance = float.MaxValue;
        Enemy closestEnemy = null;
        
        foreach (Collider enemyCollider in nearbyEnemies)
        {
            Enemy enemy = enemyCollider.GetComponent<Enemy>();
            if (enemy != null && !alreadyHit.Contains(enemy))
            {
                float distance = Vector3.Distance(fromEnemy.transform.position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy;
                }
            }
        }
        
        return closestEnemy;
    }

    void PlayLightningEffects(List<Vector3> lightningPoints)
    {
        if (lightningLine != null && lightningPoints.Count > 1)
        {
            lightningLine.positionCount = lightningPoints.Count;
            lightningLine.SetPositions(lightningPoints.ToArray());
            
            // Animate the lightning
            StartCoroutine(AnimateLightning());
        }
    }

    System.Collections.IEnumerator AnimateLightning()
    {
        lightningLine.enabled = true;
        yield return new WaitForSeconds(0.2f);
        lightningLine.enabled = false;
    }
}
