using UnityEngine;

/// <summary>
/// Fast enemy type with high speed and low health.
/// Inherits from Enemy and overrides movement speed and health.
/// </summary>
public class FastEnemy : Enemy
{
    protected override void Start()
    {
        base.Start();
        moveSpeed *= 2f; // Double the movement speed
        maxHealth = 5;   // Lower health
        currentHealth = maxHealth;
    }
}
