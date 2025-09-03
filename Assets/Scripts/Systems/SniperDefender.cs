using UnityEngine;

/// <summary>
/// Sniper defender type with long range and high damage.
/// Inherits from Defender and overrides attack range and damage.
/// </summary>
public class SniperDefender : Defender
{
    protected override void Start()
    {
        base.Start();
        attackRange *= 2f;    // Double the attack range
        attackDamage *= 1.5f; // 50% more damage
        attackIntervalSeconds *= 1.5f; // Slower attack speed
    }
}
