using UnityEngine;

/*
 * Projectile.cs
 * --------------
 * This script manages the behavior of projectiles shot by defenders.
 * Projectiles track their target, move towards it, and deal damage on impact.
 *
 * Attach this script to any projectile prefab in the game.
 */
public class Projectile : MonoBehaviour
{
    // Reference to the target the projectile is tracking.
    private Transform target;

    // Amount of damage the projectile deals on impact.
    private float damage;

    // Speed at which the projectile moves.
    private float speed;
    
    // Whether this projectile is a critical hit.
    private bool isCriticalHit;

    // Flag to check if the projectile has been initialized.
    private bool isInitialized = false;

    /// <summary>
    /// Initializes the projectile with its target, damage, and speed.
    /// </summary>
    /// <param name="targetTransform">The target the projectile will track.</param>
    /// <param name="damageAmount">The amount of damage the projectile will deal.</param>
    /// <param name="projectileSpeed">The speed at which the projectile will move.</param>
    /// <param name="criticalHit">Whether this is a critical hit.</param>
    public void Initialize(Transform targetTransform, float damageAmount, float projectileSpeed, bool criticalHit = false)
    {
        target = targetTransform;
        damage = damageAmount;
        speed = projectileSpeed;
        isCriticalHit = criticalHit;
        isInitialized = true;
    }

    void Start()
    {
        // If the projectile is initialized and has a target, set its forward direction to face the target.
        if (isInitialized && target != null)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            transform.forward = direction;
        }
    }

    void Update()
    {
        // Destroy the projectile if it is not initialized or has no target.
        if (!isInitialized || target == null)
        {
            Debug.Log("❌ Projectile not initialized or no target, destroying");
            Destroy(gameObject);
            return;
        }

        // Calculate the direction to the target and move the projectile towards it.
        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        // Check if the projectile has reached the target.
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        
        // Debug logging every few frames to track movement
        if (Time.frameCount % 10 == 0) // Log every 10th frame to avoid spam
        {
            Debug.Log($"🎯 Projectile distance to target: {distanceToTarget:F2}");
        }
        
        if (distanceToTarget < 1.5f) // Increased threshold from 0.5f to 1.5f
        {
            Debug.Log($"🎯 Projectile reached target! Distance: {distanceToTarget:F2}");
            HitTarget();
        }
        else if (distanceToTarget > 50f) // If projectile is too far from target, destroy it
        {
            Debug.Log($"❌ Projectile too far from target ({distanceToTarget:F2}), destroying");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Called when the projectile reaches its target.
    /// </summary>
    void HitTarget()
    {
        // Check if the target has an Enemy component.
        Enemy enemy = target.GetComponent<Enemy>();
        if (enemy != null)
        {
            // Debug.Log($"🎯 PROJECTILE HIT: Applying {damage} damage to {enemy.gameObject.name} (Critical: {isCriticalHit})");
            // Debug.Log($"🎯 ENEMY HEALTH BEFORE: {enemy.GetCurrentHealth()}/{enemy.GetMaxHealth()}");
            
            enemy.TakeDamage(damage);
            
            // Debug.Log($"🎯 ENEMY HEALTH AFTER: {enemy.GetCurrentHealth()}/{enemy.GetMaxHealth()}");
            
            // Show damage number and screen shake for critical hits
            CriticalHitSystem criticalSystem = FindFirstObjectByType<CriticalHitSystem>();
            if (criticalSystem != null)
            {
                criticalSystem.ShowDamageNumber(damage, enemy.transform.position, isCriticalHit);
                criticalSystem.TriggerScreenShake(isCriticalHit);
            }
        }
        else
        {
            // Debug.Log("❌ Projectile hit target, but no Enemy component found!");
        }

        // Destroy the projectile after hitting the target.
        Destroy(gameObject);
    }

    /// <summary>
    /// Called when the projectile collides with another object.
    /// </summary>
    /// <param name="other">The collider the projectile hit.</param>
    void OnTriggerEnter(Collider other)
    {
        // Debug logging disabled

        // Only handle collision if we haven't already hit our target
        if (target != null && other.transform == target)
        {
            // Debug logging disabled
            HitTarget();
        }
        // Destroy the projectile if it hits the terrain.
        else if (other.CompareTag("Terrain"))
        {
            // Debug logging disabled
            Destroy(gameObject);
        }
    }
}
