using UnityEngine;

/// <summary>
/// Rapid defender type that fires many low-damage projectiles quickly.
/// Excels against swarm enemies and light armor targets.
/// </summary>
public class RapidDefender : Defender
{
    [Header("Rapid Fire Settings")]
    [Tooltip("Damage reduction factor for rapid fire balance.")]
    public float damageMultiplier = 0.4f;
    
    [Tooltip("Attack speed increase factor.")]
    public float attackSpeedMultiplier = 0.25f;
    
    [Tooltip("Range reduction factor.")]
    public float rangeMultiplier = 0.85f;

    protected override void Start()
    {
        base.Start();
        
        // Rapid defender specific stats
        attackDamage *= damageMultiplier; // Much lower damage per shot
        attackIntervalSeconds *= attackSpeedMultiplier; // Much faster attacks
        attackRange *= rangeMultiplier; // Slightly shorter range
        
        // Higher projectile speed for rapid fire feel
        projectileSpeed *= 1.3f;
        
        Debug.Log($"RapidDefender initialized - Damage: {attackDamage}, " +
                 $"Interval: {attackIntervalSeconds}, Range: {attackRange}");
    }

    protected override void TryAttackEnemy()
    {
        // Use base attack logic but with modified stats
        base.TryAttackEnemy();
    }

    protected override void LobProjectileAtEnemy(Enemy enemy)
    {
        // Rapid defenders might fire in short bursts
        if (ShouldFireBurst())
        {
            StartCoroutine(FireBurst(enemy));
        }
        else
        {
            base.LobProjectileAtEnemy(enemy);
        }
    }

    private bool ShouldFireBurst()
    {
        // 30% chance to fire a 3-shot burst instead of single shot
        return Random.Range(0f, 1f) <= 0.3f;
    }

    private System.Collections.IEnumerator FireBurst(Enemy enemy)
    {
        int burstSize = 3;
        float burstDelay = 0.1f;
        
        for (int i = 0; i < burstSize && enemy != null; i++)
        {
            base.LobProjectileAtEnemy(enemy);
            if (i < burstSize - 1) // Don't wait after the last shot
            {
                yield return new WaitForSeconds(burstDelay);
            }
        }
    }
}