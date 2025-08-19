using UnityEngine;

public class DefenderPlacement : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private LayerMask placementPlaneMask;
    [SerializeField] private VoxelTerrainGenerator terrainGenerator;

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }
        if (terrainGenerator == null)
        {
            terrainGenerator = FindObjectOfType<VoxelTerrainGenerator>();
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
        if (mainCamera == null) return;
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
            gameManager.TryPlaceDefender(grid);
        }
    }
}


