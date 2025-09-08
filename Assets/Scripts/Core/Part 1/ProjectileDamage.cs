using UnityEngine;

namespace GADE7322_POE.Core
{
    /// <summary>
    /// Handles projectile damage on collision with enemies.
    /// </summary>
    public class ProjectileDamage : MonoBehaviour
    {
        [Tooltip("Damage dealt to enemies.")]
        public float Damage = 10.0f;
        [Tooltip("Layer mask for detecting enemies.")]
        public LayerMask TargetLayerMask;

        private void OnTriggerEnter(Collider other)
        {
            // Check if the collider is on the target layer (e.g., enemies)
            if (((1 << other.gameObject.layer) & TargetLayerMask) != 0)
            {
                Health enemyHealth = other.GetComponent<Health>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(Damage);
                    Debug.Log($"{gameObject.name} hit {other.name} for {Damage} damage!");
                }
                Destroy(gameObject); // Destroy the projectile on hit
            }
        }
    }
}
