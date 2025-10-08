using UnityEngine;

/// <summary>
/// Berserker enemy type that gets faster as it takes damage.
/// Becomes more dangerous when wounded, encouraging quick elimination.
/// </summary>
public class BerserkerEnemy : Enemy
{
    [Header("Berserker Settings")]
    [Tooltip("Base movement speed multiplier when at full health.")]
    public float baseSpeedMultiplier = 0.8f;
    
    [Tooltip("Maximum speed multiplier when near death.")]
    public float maxSpeedMultiplier = 3f;
    
    private float originalMoveSpeed;

    protected override void Start()
    {
        base.Start();
        
        // Berserker specific stats
        maxHealth = 25f;
        currentHealth = maxHealth;
        attackDamage = 8f; // Higher attack damage
        originalMoveSpeed = moveSpeed;
        moveSpeed = originalMoveSpeed * baseSpeedMultiplier;
        
        Debug.Log($"BerserkerEnemy initialized with health: {currentHealth}, speed: {moveSpeed}");
    }

    public override void TakeDamage(float amount)
    {
        base.TakeDamage(amount);
        
        // Only update speed if still alive
        if (currentHealth > 0)
        {
            UpdateSpeedBasedOnHealth();
        }
    }

    private void UpdateSpeedBasedOnHealth()
    {
        // Calculate health percentage (1.0 = full health, 0.0 = no health)
        float healthPercent = currentHealth / maxHealth;
        
        // Invert so lower health = higher speed multiplier
        float rageMultiplier = Mathf.Lerp(maxSpeedMultiplier, baseSpeedMultiplier, healthPercent);
        
        // Apply the new speed
        moveSpeed = originalMoveSpeed * rageMultiplier;
        
        Debug.Log($"Berserker rage activated! Health: {healthPercent:F2}, Speed multiplier: {rageMultiplier:F2}");
    }
}