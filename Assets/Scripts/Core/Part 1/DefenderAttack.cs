using UnityEngine;

namespace GADE7322_POE.Core
{
    /// <summary>
    /// This script manages the attacking behavior of defenders in the tower defense game.
    /// Defenders automatically detect and attack the nearest enemy within their range.
    /// </summary>
    public class DefenderAttack : MonoBehaviour
    {
        [Header("Attack Settings")]
        [Tooltip("The maximum distance at which the defender can detect and attack enemies.")]
        public float AttackRange = 5.0f;
        [Tooltip("The amount of damage the defender deals to enemies with each attack.")]
        public float AttackDamage = 10.0f;
        [Tooltip("The time delay (in seconds) between consecutive attacks.")]
        public float AttackCooldown = 1.0f;
        [Tooltip("The layer mask used to identify enemies in the scene.")]
        public LayerMask EnemyLayerMask;
        [Tooltip("The prefab of the projectile that the defender shoots at enemies.")]
        public GameObject ProjectilePrefab;
        [Tooltip("The speed at which the projectile travels towards the enemy.")]
        public float ProjectileSpeed = 10.0f;

        // Tracks the remaining time until the next attack can occur.
        private float cooldownTimer = 0f;

        private void Update()
        {
            // Decrease the cooldown timer by the time since the last frame.
            cooldownTimer -= Time.deltaTime;

            // If the cooldown timer has reached zero, attack the nearest enemy and reset the timer.
            if (cooldownTimer <= 0f)
            {
                AttackNearestEnemy();
                cooldownTimer = AttackCooldown;
            }
        }

        /// <summary>
        /// Detects the nearest enemy within range and shoots a projectile at it.
        /// </summary>
        private void AttackNearestEnemy()
        {
            // Detect all enemies within the attack range.
            Collider[] enemies = Physics.OverlapSphere(transform.position, AttackRange, EnemyLayerMask);
            if (enemies.Length == 0) return; // Exit if no enemies are found.

            // Find the closest enemy by comparing distances.
            Collider nearestEnemy = enemies[0];
            float minDistance = Vector3.Distance(transform.position, nearestEnemy.transform.position);
            foreach (Collider enemy in enemies)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestEnemy = enemy;
                }
            }

            // Instantiate a projectile and initialize it to target the nearest enemy.
            if (ProjectilePrefab != null)
            {
                GameObject projectile = Instantiate(ProjectilePrefab, transform.position, Quaternion.identity);
                Projectile projectileComponent = projectile.GetComponent<Projectile>();
                if (projectileComponent != null)
                {
                    projectileComponent.Initialize(nearestEnemy.transform, AttackDamage, ProjectileSpeed);
                }
                else
                {
                    Debug.LogError("ProjectilePrefab does not have a Projectile component!");
                }
            }
            else
            {
                Debug.LogError("ProjectilePrefab is not assigned!");
            }

            Debug.Log($"{gameObject.name} shot a projectile at {nearestEnemy.name}!");
        }

        /// <summary>
        /// Draws a red wire sphere in the Unity Editor to visualize the defender's attack range.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, AttackRange);
        }
    }
}
