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

    // Flag to check if the projectile has been initialized.
    private bool isInitialized = false;

    /// <summary>
    /// Initializes the projectile with its target, damage, and speed.
    /// </summary>
    /// <param name="targetTransform">The target the projectile will track.</param>
    /// <param name="damageAmount">The amount of damage the projectile will deal.</param>
    /// <param name="projectileSpeed">The speed at which the projectile will move.</param>
    public void Initialize(Transform targetTransform, float damageAmount, float projectileSpeed)
    {
        target = targetTransform;
        damage = damageAmount;
        speed = projectileSpeed;
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
            Destroy(gameObject);
            return;
        }

        // Calculate the direction to the target and move the projectile towards it.
        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        // Check if the projectile has reached the target.
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        if (distanceToTarget < 0.5f)
        {
            HitTarget();
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
            Debug.Log($"Projectile hit target! Applying {damage} damage to {enemy.gameObject.name}");
            enemy.TakeDamage(damage);
        }
        else
        {
            Debug.Log("Projectile hit target, but no Enemy component found!");
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
        Debug.Log($"Projectile collided with {other.gameObject.name}");

        // Check if the collider belongs to an enemy.
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            Debug.Log($"Enemy found: {enemy.gameObject.name}, applying {damage} damage");
            enemy.TakeDamage(damage);
            Destroy(gameObject);
        }
        // Destroy the projectile if it hits the terrain.
        else if (other.CompareTag("Terrain"))
        {
            Debug.Log("Projectile hit terrain, destroying");
            Destroy(gameObject);
        }
    }
}
