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

            // Create a simple sphere projectile
            GameObject projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectile.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            projectile.transform.position = transform.position;
            Destroy(projectile, 5f); // Destroy after 5 seconds

            // Add Rigidbody to move the projectile
            Rigidbody rb = projectile.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.mass = 0.1f;

            // Move the projectile toward the enemy
            Vector3 direction = (nearestEnemy.transform.position - transform.position).normalized;
            rb.linearVelocity = direction * ProjectileSpeed;

            // Add a trigger collider for damage
            SphereCollider collider = projectile.AddComponent<SphereCollider>();
            collider.isTrigger = true;

            // Add a script to handle collision with enemies
            ProjectileDamage projectileDamage = projectile.AddComponent<ProjectileDamage>();
            projectileDamage.Damage = AttackDamage;
            projectileDamage.TargetLayerMask = EnemyLayerMask;

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
