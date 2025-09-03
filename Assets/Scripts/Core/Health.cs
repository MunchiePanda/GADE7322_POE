using UnityEngine;
using UnityEngine.Events;

namespace GADE7322_POE.Core
{
    /// <summary>
    /// Manages health for defenders, tower, and other entities.
    /// </summary>
    public class Health : MonoBehaviour
    {
        [Header("Health Settings")]
        [Tooltip("Maximum health for this entity.")]
        public float MaxHealth = 100.0f;
        [Tooltip("Current health for this entity.")]
        public float CurrentHealth;

        [Header("Events")]
        public UnityEvent OnTakeDamage;  // Triggered when this entity takes damage
        public UnityEvent OnDeath;       // Triggered when this entity dies

        private void Awake()
        {
            CurrentHealth = MaxHealth;
        }

        /// <summary>
        /// Applies damage to this entity.
        /// </summary>
        public void TakeDamage(float damage)
        {
            CurrentHealth -= damage;
            OnTakeDamage?.Invoke();

            if (CurrentHealth <= 0.0f)
            {
                Die();
            }
        }

        /// <summary>
        /// Kills this entity (e.g., when health reaches 0).
        /// </summary>
        public void Die()
        {
            OnDeath?.Invoke();
            Destroy(gameObject);
        }

        /// <summary>
        /// Heals this entity by the specified amount.
        /// </summary>
        public void Heal(float amount)
        {
            CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);
        }
    }
}
