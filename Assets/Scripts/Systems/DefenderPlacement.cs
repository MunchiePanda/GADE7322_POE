using UnityEngine;

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
        if (gameManager == null || gameManager.IsPaused() || gameManager.IsGameOver()) return;

        // Left click to place defender
        if (UnityEngine.InputSystem.Mouse.current != null && UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryPlaceAtMouse();
        }
    }

    void TryPlaceAtMouse()
    {
        if (mainCamera == null || defenderPrefab == null) return;
        Ray ray = mainCamera.ScreenPointToRay(UnityEngine.InputSystem.Mouse.current.position.ReadValue());
        RaycastHit hit;
        bool didHit = Physics.Raycast(ray, out hit, 1000f, placementPlaneMask);
        if (!didHit)
        {
            // Fallback: try without mask (in case terrain layer changed)
            didHit = Physics.Raycast(ray, out hit, 1000f);
        }
        if (didHit)
        {
            Vector3 hitPoint = hit.point;
            // Snap to integer grid
            // Convert world hit to local grid using terrain transform
            Vector3 local = terrainGenerator != null
                ? terrainGenerator.transform.InverseTransformPoint(hitPoint)
                : hitPoint;
            int gx = Mathf.RoundToInt(local.x);
            int gz = Mathf.RoundToInt(local.z);
            // Guard against clicks outside terrain
            if (gx < 0 || gz < 0 || gx >= gameManager.terrainGenerator.width || gz >= gameManager.terrainGenerator.depth)
                return;
            Vector3Int grid = new Vector3Int(gx, 0, gz);
            TryPlaceDefender(grid);
        }
    }

    /// <summary>
    /// Attempts to place a defender at the specified grid position.
    /// </summary>
    public bool TryPlaceDefender(Vector3Int gridPosition)
    {
        if (gameManager == null || defenderPrefab == null || terrainGenerator == null) return false;

        // Only allow placement on valid non-path tiles
        if (!terrainGenerator.IsValidDefenderPlacement(gridPosition)) return false;

        if (!gameManager.SpendResources(gameManager.defenderCost)) return false;

        // Clamp to terrain bounds to avoid OOB
        int gx = Mathf.Clamp(gridPosition.x, 0, terrainGenerator.width - 1);
        int gz = Mathf.Clamp(gridPosition.z, 0, terrainGenerator.depth - 1);
        Vector3 worldPos = terrainGenerator.GridToWorld(gx, gz);
        Instantiate(defenderPrefab, worldPos, Quaternion.identity);
        return true;
    }
}


