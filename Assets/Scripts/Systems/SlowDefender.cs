using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Slow defender type that applies debuff effects to enemies.
/// Focuses on crowd control rather than raw damage.
/// </summary>
public class SlowDefender : Defender
{
    [Header("Slow Effect Settings")]
    [Tooltip("Speed reduction factor (0.5 = 50% speed reduction).")]
    public float slowEffect = 0.5f;
    
    [Tooltip("Duration of the slow effect in seconds.")]
    public float slowDuration = 3f;
    
    [Tooltip("Chance to apply slow effect per hit (0.0 to 1.0).")]
    public float slowChance = 0.8f;

    private Dictionary<Enemy, Coroutine> activeSlowEffects = new Dictionary<Enemy, Coroutine>();

    protected override void Start()
    {
        base.Start();
        
        // Slow defender specific stats
        attackDamage = 3f; // Very low damage - utility focused
        attackRange *= 1.3f; // Longer range to compensate for low damage
        attackIntervalSeconds = 0.6f; // Faster attacks for more slow applications
        
        Debug.Log($"SlowDefender initialized with damage: {attackDamage}, range: {attackRange}");
    }

    protected override void TryAttackEnemy()
    {
        if (currentEnemyTarget == null) return;
        float time = Time.time;
        if (time - lastAttackTime >= attackIntervalSeconds)
        {
            lastAttackTime = time;
            LobProjectileAtEnemy(currentEnemyTarget);
        }
    }

    protected override void LobProjectileAtEnemy(Enemy enemy)
    {
        // Call base projectile method first
        base.LobProjectileAtEnemy(enemy);
        
        // Apply slow effect based on chance
        if (Random.Range(0f, 1f) <= slowChance)
        {
            ApplySlowEffect(enemy);
        }
    }

    private void ApplySlowEffect(Enemy enemy)
    {
        if (enemy == null) return;

        // If enemy already has a slow effect, refresh it
        if (activeSlowEffects.ContainsKey(enemy))
        {
            StopCoroutine(activeSlowEffects[enemy]);
            activeSlowEffects.Remove(enemy);
        }

        // Start new slow effect
        Coroutine slowCoroutine = StartCoroutine(SlowEffectCoroutine(enemy));
        activeSlowEffects[enemy] = slowCoroutine;
    }

    private IEnumerator SlowEffectCoroutine(Enemy enemy)
    {
        if (enemy == null) yield break;

        // Store original speed
        float originalSpeed = enemy.GetMoveSpeed();
        float newSpeed = originalSpeed * slowEffect;
        
        // Apply slow
        enemy.SetMoveSpeed(newSpeed);
        Debug.Log($"Applied slow effect to {enemy.name}: {originalSpeed} -> {newSpeed}");
        
        // Wait for duration
        yield return new WaitForSeconds(slowDuration);
        
        // Restore original speed if enemy still exists
        if (enemy != null)
        {
            enemy.SetMoveSpeed(originalSpeed);
            Debug.Log($"Slow effect expired on {enemy.name}: restored to {originalSpeed}");
        }
        
        // Clean up tracking
        if (activeSlowEffects.ContainsKey(enemy))
        {
            activeSlowEffects.Remove(enemy);
        }
    }

    private void OnDestroy()
    {
        // Clean up all active slow effects
        foreach (var kvp in activeSlowEffects)
        {
            if (kvp.Value != null)
            {
                StopCoroutine(kvp.Value);
            }
        }
        activeSlowEffects.Clear();
    }
}