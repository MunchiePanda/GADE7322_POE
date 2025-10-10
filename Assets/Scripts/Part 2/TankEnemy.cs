using UnityEngine;

/// <summary>
/// Tank enemy type with low speed and high health.
/// Inherits from Enemy and overrides movement speed and health.
/// </summary>
public class TankEnemy : Enemy
{
    protected override void Start()
    {
        base.Start();
        moveSpeed *= 0.5f; // Half the movement speed
        maxHealth = 30;   // Higher health
        currentHealth = maxHealth;
    }
}
