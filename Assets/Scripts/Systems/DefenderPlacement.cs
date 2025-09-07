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

    [Header("Defender Placement Settings")]
    [SerializeField] private int maxDefenderLocations = 3; // Number of possible defender locations per path
    [SerializeField] private GameObject placementMarkerPrefab; // Prefab to mark valid placement locations
    [SerializeField] private GameObject placementHighlightPrefab; // Prefab to highlight valid placement locations

    private List<GameObject> placementMarkers = new List<GameObject>();
    private List<GameObject> placementHighlights = new List<GameObject>();

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

    /// <summary>
    /// Shows possible defender placement locations near the selected path.
    /// </summary>
    public void ShowPlacementLocations(int pathIndex)
    {
        ClearPlacementMarkers();
        ClearPlacementHighlights();

        List<List<Vector3Int>> paths = terrainGenerator.GetPaths();
        if (paths == null || paths.Count <= pathIndex || pathIndex < 0)
        {
            Debug.LogError("Invalid path index for placement locations.");
            return;
        }

        List<Vector3Int> path = paths[pathIndex];
        if (path == null || path.Count == 0)
        {
            Debug.LogError("Selected path is empty.");
            return;
        }

        List<Vector3Int> validLocations = new List<Vector3Int>();
        for (int i = 0; i < path.Count; i++)
        {
            Vector3Int pathPos = path[i];
            int surfaceY = terrainGenerator.GetSurfaceY(pathPos.x, pathPos.z);

            // Check nearby positions for valid placement
            for (int x = -2; x <= 2; x++)
            {
                for (int z = -2; z <= 2; z++)
                {
                    Vector3Int testPosition = new Vector3Int(pathPos.x + x, surfaceY - 1, pathPos.z + z);
                    testPosition.x = Mathf.Clamp(testPosition.x, 0, terrainGenerator.width - 1);
                    testPosition.z = Mathf.Clamp(testPosition.z, 0, terrainGenerator.depth - 1);

                    if (terrainGenerator.IsValidDefenderPlacement(testPosition) && !validLocations.Contains(testPosition))
                    {
                        validLocations.Add(testPosition);
                    }
                }
            }
        }

        // Randomly select a subset of valid locations
        if (validLocations.Count > 0)
        {
            // Shuffle the list to randomize selection
            for (int i = 0; i < validLocations.Count; i++)
            {
                Vector3Int temp = validLocations[i];
                int randomIndex = Random.Range(i, validLocations.Count);
                validLocations[i] = validLocations[randomIndex];
                validLocations[randomIndex] = temp;
            }

            // Select up to maxDefenderLocations locations
            int locationsToShow = Mathf.Min(maxDefenderLocations, validLocations.Count);
            for (int i = 0; i < locationsToShow; i++)
            {
                Vector3Int location = validLocations[i];
                Vector3 worldPos = terrainGenerator.GetSurfaceWorldPosition(new Vector3Int(location.x, 0, location.z));

                // Create a highlight
                GameObject highlight = Instantiate(placementHighlightPrefab, worldPos, Quaternion.identity);
                placementHighlights.Add(highlight);

                // Add a collider to the highlight for raycast detection
                if (highlight.GetComponent<Collider>() == null)
                {
                    highlight.AddComponent<BoxCollider>();
                }
            }
        }
    }

    /// <summary>
    /// Clears all placement markers.
    /// </summary>
    public void ClearPlacementLocations()
    {
        ClearPlacementMarkers();
        ClearPlacementHighlights();
    }

    private void ClearPlacementMarkers()
    {
        foreach (GameObject marker in placementMarkers)
        {
            if (marker != null)
            {
                Destroy(marker);
            }
        }
        placementMarkers.Clear();
    }

    private void ClearPlacementHighlights()
    {
        foreach (GameObject highlight in placementHighlights)
        {
            if (highlight != null)
            {
                Destroy(highlight);
            }
        }
        placementHighlights.Clear();
    }

    /// <summary>
    /// Attempts to place a defender at the clicked placement marker.
    /// </summary>
    private void TryPlaceDefenderAtHighlight()
    {
        Ray ray = Camera.main.ScreenPointToRay(UnityEngine.InputSystem.Mouse.current.position.ReadValue());
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            // Check if the clicked object is a defender highlight quad
            if (hit.transform != null && hit.transform.name.StartsWith("Highlight_Defender"))
            {
                // Extract grid position from the highlight's transform
                Vector3 worldPos = hit.transform.position;
                Vector3 localPos = terrainGenerator.transform.InverseTransformPoint(worldPos);
                Vector3Int gridPos = new Vector3Int(Mathf.RoundToInt(localPos.x), 0, Mathf.RoundToInt(localPos.z));

                // Get the surface height at the grid position
                int surfaceY = terrainGenerator.GetSurfaceY(gridPos.x, gridPos.z);
                gridPos.y = surfaceY - 1;

                // Try to place the defender
                TryPlaceDefenderAtMarker(gridPos);
            }
        }
    }

    void Update()
    {
        if (UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame && !gameManager.IsGameOver() && !gameManager.IsPaused())
        {
            TryPlaceDefenderAtHighlight();
        }
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
        Vector3 worldPos = terrainGenerator.GetSurfaceWorldPosition(new Vector3Int(gx, 0, gz));
        Debug.Log($"Placing defender at world position: {worldPos}");
        GameObject defender = Instantiate(defenderPrefab, worldPos, Quaternion.identity);
        Debug.Log($"Defender instantiated: {defender != null}");
        return true;
    }

    public void ShowDefenderLocations(int pathIndex)
    {
        if (gameManager == null || gameManager.IsGameOver() || gameManager.IsPaused() || terrainGenerator == null) return;

        gameManager.HighlightDefenderLocations(pathIndex);
    }

    /// <summary>
    /// Attempts to place a defender at the specified grid position.
    /// </summary>
    public bool TryPlaceDefenderAtMarker(Vector3Int gridPosition)
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
        Vector3 worldPos = terrainGenerator.GetSurfaceWorldPosition(new Vector3Int(gx, 0, gz));
        Debug.Log($"Placing defender at world position: {worldPos}");
        GameObject defender = Instantiate(defenderPrefab, worldPos, Quaternion.identity);
        Debug.Log($"Defender instantiated: {defender != null}");
        ClearPlacementLocations();
        return true;
    }
}


