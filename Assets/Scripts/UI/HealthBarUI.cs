using UnityEngine;
using UnityEngine.UI;
using GADE7322_POE.Core;

namespace GADE7322_POE.UI
{
    /// <summary>
    /// Dynamic health bar UI for defenders, following them in world space.
    /// </summary>
    public class HealthBarUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Image fillImage;
        [SerializeField] private Color fullHealthColor = Color.green;
        [SerializeField] private Color lowHealthColor = Color.red;

        [Header("World Space Settings")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Vector3 offset = new Vector3(0.0f, 1.5f, 0.0f);

        // Reference to the defender this health bar is attached to
        private Health defenderHealth;
        private Transform defenderTransform;

        /// <summary>
        /// Initializes the health bar for a defender.
        /// </summary>
        public void Initialize(Health health, Transform targetTransform)
        {
            defenderHealth = health;
            defenderTransform = targetTransform;

            // Subscribe to health events
            defenderHealth.OnTakeDamage.AddListener(UpdateHealthBar);
            defenderHealth.OnDeath.AddListener(DestroyHealthBar);

            // Initial update
            UpdateHealthBar();
        }

        private void Update()
        {
            if (defenderTransform != null && mainCamera != null)
            {
                // Follow the defender in world space
                Vector3 worldPosition = defenderTransform.position + offset;
                Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
                transform.position = screenPosition;
            }
        }

        /// <summary>
        /// Updates the health bar UI based on the defender's current health.
        /// </summary>
        private void UpdateHealthBar()
        {
            if (healthSlider != null && defenderHealth != null)
            {
                healthSlider.value = defenderHealth.CurrentHealth / defenderHealth.MaxHealth;
            }
            if (fillImage != null && defenderHealth != null)
            {
                fillImage.color = Color.Lerp(
                    lowHealthColor,
                    fullHealthColor,
                    defenderHealth.CurrentHealth / defenderHealth.MaxHealth
                );
            }
        }

        /// <summary>
        /// Destroys this health bar when the defender dies.
        /// </summary>
        private void DestroyHealthBar()
        {
            Destroy(gameObject);
        }

        /// <summary>
        /// Manually set health (for external updates).
        /// </summary>
        public void SetHealth(float current, float max)
        {
            if (healthSlider != null)
            {
                healthSlider.value = current / max;
            }
            if (fillImage != null)
            {
                fillImage.color = Color.Lerp(lowHealthColor, fullHealthColor, current / max);
            }
        }
    }
}