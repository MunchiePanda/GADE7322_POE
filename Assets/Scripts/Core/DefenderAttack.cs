using UnityEngine;

namespace GADE7322_POE.Core
{
    /// <summary>
    /// Handles defender attacks on enemies.
    /// </summary>
    public class DefenderAttack : MonoBehaviour
    {
        [Header("Attack Settings")]
        [Tooltip("Range within which the defender can attack enemies.")]
        public float AttackRange = 5.0f;
        [Tooltip("Damage dealt to enemies per attack.")]
        public float AttackDamage = 10.0f;
        [Tooltip("Cooldown between attacks (in seconds).")]
        public float AttackCooldown = 1.0f;
        [Tooltip("Layer mask for detecting enemies.")]
        public LayerMask EnemyLayerMask;
        [Tooltip("Projectile prefab to spawn when attacking.")]
        public GameObject ProjectilePrefab;
        [Tooltip("Speed of the projectile.")]
        public float ProjectileSpeed = 10.0f;

        private float cooldownTimer = 0f;

        private void Update()
        {
            cooldownTimer -= Time.deltaTime;

            if (cooldownTimer <= 0f)
            {
                AttackNearestEnemy();
                cooldownTimer = AttackCooldown;
            }
        }

        /// <summary>
        /// Finds the nearest enemy and shoots a projectile at it.
        /// </summary>
        private void AttackNearestEnemy()
        {
            Collider[] enemies = Physics.OverlapSphere(transform.position, AttackRange, EnemyLayerMask);
            if (enemies.Length == 0) return;

            // Find the closest enemy
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

            // Use the ProjectilePrefab
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
        /// Visualizes the attack range in the editor.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, AttackRange);
        }
    }
}
