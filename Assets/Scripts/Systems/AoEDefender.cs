using UnityEngine;

/// <summary>
/// AoE (Area of Effect) defender type with short range and area damage.
/// Inherits from Defender and overrides attack logic to hit multiple enemies.
/// </summary>
public class AoEDefender : Defender
{
    [Header("AoE Settings")]
    [Tooltip("Radius of the AoE attack.")]
    public float aoeRadius = 3f;

    protected override void Start()
    {
        base.Start();
        attackRange *= 0.7f; // Shorter range
        attackDamage *= 0.8f; // Slightly lower single-target damage
    }

    protected void Attack()
    {
        if (currentEnemyTarget == null) return;

        // Find all enemies in AoE radius
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, aoeRadius);
        foreach (Collider collider in hitColliders)
        {
            Enemy enemy = collider.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(attackDamage);
            }
        }
    }
}
