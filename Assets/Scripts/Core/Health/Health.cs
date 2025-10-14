using UnityEngine;
using UnityEngine.Events;

namespace GADE7322_POE.Core
{
    /*
     * Health.cs
     * ----------
     * This script manages the health system for entities in the game, such as defenders, the tower, and enemies.
     * It handles taking damage, healing, and death events.
     *
     * Attach this script to any GameObject that requires health management.
     */
    public class Health : MonoBehaviour
    {
        [Header("Health Settings")]
        [Tooltip("The maximum health value for this entity.")]
        public float MaxHealth = 100.0f;
        [Tooltip("The current health value for this entity.")]
        public float CurrentHealth;

        [Header("Events")]
        [Tooltip("Event triggered when this entity takes damage.")]
        public UnityEvent OnTakeDamage;
        [Tooltip("Event triggered when this entity's health reaches zero.")]
        public UnityEvent OnDeath;

        private void Awake()
        {
            // Initialize current health to the maximum health value.
            CurrentHealth = MaxHealth;
        }

        /// <summary>
        /// Reduces the entity's health by the specified damage amount.
        /// </summary>
        /// <param name="damage">Amount of damage to apply.</param>
        public void TakeDamage(float damage)
        {
            // Reduce current health by the damage amount.
            CurrentHealth -= damage;

            // Trigger the OnTakeDamage event.
            OnTakeDamage?.Invoke();

            // If health drops to zero or below, trigger death.
            if (CurrentHealth <= 0.0f)
            {
                Die();
            }
        }

        /// <summary>
        /// Destroys the entity when its health reaches zero.
        /// </summary>
        public void Die()
        {
            // Trigger the OnDeath event.
            OnDeath?.Invoke();

            // Destroy the GameObject this script is attached to.
            Destroy(gameObject);
        }

        /// <summary>
        /// Increases the entity's health by the specified amount, up to the maximum health.
        /// </summary>
        /// <param name="amount">Amount of health to restore.</param>
        public void Heal(float amount)
        {
            // Increase current health by the specified amount, without exceeding max health.
            CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);
        }
    }
}
