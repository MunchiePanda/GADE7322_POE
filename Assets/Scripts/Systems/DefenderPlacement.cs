using UnityEngine;
using System.Collections.Generic;

public class DefenderPlacement : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private LayerMask placementPlaneMask;
    [SerializeField] private VoxelTerrainGenerator terrainGenerator;

    // Public property for defender prefab (can be set by DefenderSelectionUI)
    public GameObject defenderPrefab;

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }
        if (terrainGenerator == null)
        {
            terrainGenerator = FindFirstObjectByType<VoxelTerrainGenerator>();
        }

        // Default to the GameManager's defender prefab
        if (defenderPrefab == null && gameManager != null)
        {
            defenderPrefab = gameManager.defenderPrefab;
        }
    }

    void Update()
    {
        // Defender placement is now handled by a button, so no mouse input is needed here.
    }

    void TryPlaceAtMouse()
    {
        Debug.Log("TryPlaceAtMouse called");
        if (mainCamera == null)
        {
            Debug.LogError("Main camera is null in TryPlaceAtMouse");
            return;
        }
        if (defenderPrefab == null)
        {
            Debug.LogError("Defender prefab is null in TryPlaceAtMouse");
            return;
        }
        if (terrainGenerator == null)
        {
            Debug.LogError("Terrain generator is null in TryPlaceAtMouse");
            return;
        }

        Vector2 mousePosition = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);

        // Position the plane at the terrain's height
        float terrainHeight = terrainGenerator.height / 2f;
        Plane plane = new Plane(Vector3.up, new Vector3(0, terrainHeight, 0));
        float distance;

        if (plane.Raycast(ray, out distance))
        {
            Vector3 worldPosition = ray.GetPoint(distance);
            Vector3 local = terrainGenerator.transform.InverseTransformPoint(worldPosition);
            int gx = Mathf.RoundToInt(local.x);
            int gz = Mathf.RoundToInt(local.z);

            // Clamp grid position to terrain bounds
            gx = Mathf.Clamp(gx, 0, terrainGenerator.width - 1);
            gz = Mathf.Clamp(gz, 0, terrainGenerator.depth - 1);

            Debug.Log($"Grid position: {gx}, {gz}");

            int surfaceY = terrainGenerator.GetSurfaceY(gx, gz);
            Vector3Int grid = new Vector3Int(gx, surfaceY - 1, gz);
            Debug.Log($"Surface Y: {surfaceY}, Grid: {grid}");

            if (TryPlaceDefender(grid))
            {
                Debug.Log($"Defender placed at {grid}");
            }
            else
            {
                Debug.LogWarning("Cannot place defender: Invalid position or insufficient resources");
            }
        }
        else
        {
            Debug.LogWarning("Plane raycast did not hit anything");
        }
    }

    /// <summary>
    /// Attempts to place a defender at the specified grid position.
    /// </summary>
    public bool TryPlaceDefender(Vector3Int gridPosition)
    {
        if (gameManager == null)
        {
            Debug.LogError("GameManager is not assigned in DefenderPlacement.");
            return false;
        }
        if (defenderPrefab == null)
        {
            Debug.LogError("Defender prefab is not assigned in DefenderPlacement.");
            return false;
        }
        if (terrainGenerator == null)
        {
            Debug.LogError("Terrain generator is not assigned in DefenderPlacement.");
            return false;
        }

        Debug.Log($"Checking placement at {gridPosition}");

        if (!terrainGenerator.IsValidDefenderPlacement(gridPosition))
        {
            Debug.LogWarning($"Invalid placement at {gridPosition}: Not on surface or too far from path");
            return false;
        }

        if (!gameManager.SpendResources(gameManager.defenderCost))
        {
            Debug.LogWarning("Not enough resources to place defender");
            return false;
        }

        int gx = Mathf.Clamp(gridPosition.x, 0, terrainGenerator.width - 1);
        int gz = Mathf.Clamp(gridPosition.z, 0, terrainGenerator.depth - 1);
        Vector3 worldPos = terrainGenerator.GridToWorld(gx, gz);
        Debug.Log($"Placing defender at world position: {worldPos}");
        GameObject defender = Instantiate(defenderPrefab, worldPos, Quaternion.identity);
        Debug.Log($"Defender instantiated: {defender != null}");
        return true;
    }

    public void PlaceDefenderAtMouse()
    {
        if (gameManager == null || gameManager.IsGameOver() || gameManager.IsPaused() || terrainGenerator == null) return;

        // Get a random path
        List<List<Vector3Int>> paths = terrainGenerator.GetPaths();
        if (paths == null || paths.Count == 0)
        {
            Debug.LogError("No paths available for defender placement.");
            return;
        }

        // Select the first path for simplicity
        List<Vector3Int> path = paths[0];
        if (path == null || path.Count == 0)
        {
            Debug.LogError("Selected path is empty.");
            return;
        }

        // Select the first position on the path
        Vector3Int pathPos = path[0];

        // Get the surface height at the path position
        int surfaceY = terrainGenerator.GetSurfaceY(pathPos.x, pathPos.z);

        // Try to find a valid position near the path
        Vector3Int gridPosition = new Vector3Int(pathPos.x + 1, surfaceY - 1, pathPos.z + 1);

        // Ensure the position is within bounds
        gridPosition.x = Mathf.Clamp(gridPosition.x, 0, terrainGenerator.width - 1);
        gridPosition.z = Mathf.Clamp(gridPosition.z, 0, terrainGenerator.depth - 1);

        // Check if the position is valid
        if (!terrainGenerator.IsValidDefenderPlacement(gridPosition))
        {
            // Try to find a valid position near the path
            bool foundValidPosition = false;
            for (int x = -2; x <= 2; x++)
            {
                for (int z = -2; z <= 2; z++)
                {
                    Vector3Int testPosition = new Vector3Int(pathPos.x + x, surfaceY - 1, pathPos.z + z);
                    testPosition.x = Mathf.Clamp(testPosition.x, 0, terrainGenerator.width - 1);
                    testPosition.z = Mathf.Clamp(testPosition.z, 0, terrainGenerator.depth - 1);

                    if (terrainGenerator.IsValidDefenderPlacement(testPosition))
                    {
                        gridPosition = testPosition;
                        foundValidPosition = true;
                        break;
                    }
                }
                if (foundValidPosition) break;
            }

            if (!foundValidPosition)
            {
                Debug.LogWarning("No valid position found near the path for defender placement.");
                return;
            }
        }

        TryPlaceDefender(gridPosition);
    }
}


