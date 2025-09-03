using UnityEngine;
using UnityEngine.UI;
using GADE7322_POE.Systems;

namespace GADE7322_POE.UI
{
    /// <summary>
    /// Simplified defender placement: place defenders near paths using a button.
    /// </summary>
    public class SimpleDefenderPlacer : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The player GameObject.")]
        public GameObject Player;
        [Tooltip("The UI button for placing defenders.")]
        public Button PlaceDefenderButton;
        [Tooltip("The defender prefab to spawn.")]
        public GameObject DefenderPrefab;
        [Tooltip("The GameState manager.")]
        public GameState GameState;

        [Header("Placement Settings")]
        [Tooltip("The layer mask for path detection.")]
        public LayerMask PathLayerMask;
        [Tooltip("The maximum distance from the path to allow placement.")]
        public float MaxPathDistance = 2.0f;
        [Tooltip("The offset distance from the path for defender placement.")]
        public float PlacementOffset = 1.5f;

        private void Start()
        {
            // Disable the button until the player is near a path
            PlaceDefenderButton.interactable = false;
            PlaceDefenderButton.onClick.AddListener(AttemptPlaceDefender);
        }

        private void Update()
        {
            // Check if the player is near a path
            bool nearPath = IsNearPath();
            PlaceDefenderButton.interactable = nearPath;
        }

        /// <summary>
        /// Checks if the player is near a path.
        /// </summary>
        private bool IsNearPath()
        {
            if (Player == null) return false;
            return Physics.CheckSphere(Player.transform.position, MaxPathDistance, PathLayerMask);
        }

        /// <summary>
        /// Attempts to place a defender if the player has enough resources.
        /// </summary>
        private void AttemptPlaceDefender()
        {
            if (GameState == null || !GameState.TryBuyDefender())
            {
                Debug.Log("Cannot place defender: not enough resources.");
                return;
            }

            // Find the nearest path
            Collider[] colliders = Physics.OverlapSphere(Player.transform.position, MaxPathDistance, PathLayerMask);
            if (colliders.Length == 0)
            {
                Debug.LogError("No path found nearby!");
                return;
            }

            // Get the closest path collider
            Collider nearestPath = colliders[0];
            float minDistance = Vector3.Distance(Player.transform.position, nearestPath.transform.position);
            foreach (Collider collider in colliders)
            {
                float distance = Vector3.Distance(Player.transform.position, collider.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestPath = collider;
                }
            }

            // Get the closest point on the path
            Vector3 pathPoint = nearestPath.ClosestPoint(Player.transform.position);

            // Calculate an offset position perpendicular to the path
            Vector3 pathForward = nearestPath.transform.forward;
            Vector3 offsetDirection = Vector3.Cross(Vector3.up, pathForward).normalized;
            Vector3 spawnPosition = pathPoint + offsetDirection * PlacementOffset;

            // Spawn the defender at the offset position
            GameObject defender = Instantiate(DefenderPrefab, spawnPosition, Quaternion.identity);

            // Rotate the defender to face the path (or enemies)
            defender.transform.LookAt(pathPoint);

            Debug.Log($"Defender placed at: {spawnPosition} (offset from path)");
        }
    }
}
