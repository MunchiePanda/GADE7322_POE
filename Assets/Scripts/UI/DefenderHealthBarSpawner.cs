using UnityEngine;
using GADE7322_POE.Core;
using GADE7322_POE.UI;

namespace GADE7322_POE.UI
{
    /// <summary>
    /// Spawns and links a health bar UI to a defender.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class DefenderHealthBarSpawner : MonoBehaviour
    {
        [Header("Health Bar Settings")]
        [Tooltip("Prefab for the defender health bar UI.")]
        public GameObject HealthBarPrefab;
        [Tooltip("Reference to the world-space canvas (optional).")]
        public Transform WorldSpaceCanvas;

        private Health defenderHealth;
        private GameObject healthBarInstance;

        private void Awake()
        {
            defenderHealth = GetComponent<Health>();
            if (defenderHealth == null)
            {
                Debug.LogError("DefenderHealthBarSpawner requires a Health component!");
                return;
            }
        }

        private void Start()
        {
            SpawnHealthBar();
        }

        /// <summary>
        /// Instantiates and initializes the health bar UI.
        /// </summary>
        private void SpawnHealthBar()
        {
            if (HealthBarPrefab == null)
            {
                Debug.LogError("HealthBarPrefab not assigned!");
                return;
            }

            // Instantiate the health bar under the world-space canvas
            Transform canvasTransform = WorldSpaceCanvas != null ? WorldSpaceCanvas : FindFirstObjectByType<Canvas>().transform;
            healthBarInstance = Instantiate(HealthBarPrefab, canvasTransform);

            // Initialize the health bar
            HealthBarUI healthBarUI = healthBarInstance.GetComponent<HealthBarUI>();
            if (healthBarUI != null)
            {
                healthBarUI.Initialize(defenderHealth, transform);
            }
            else
            {
                Debug.LogError("HealthBarUI component not found on prefab!");
            }
        }

        private void OnDestroy()
        {
            // Clean up the health bar if the defender is destroyed
            if (healthBarInstance != null)
            {
                Destroy(healthBarInstance);
            }
        }
    }
}
