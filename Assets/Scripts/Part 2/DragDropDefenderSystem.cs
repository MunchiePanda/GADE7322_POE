using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Handles drag-and-drop functionality for defender placement.
/// Attach this to UI buttons that represent defender types.
/// </summary>
public class DragDropDefenderSystem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Defender Settings")]
    [Tooltip("Type of defender this drag-drop system represents")]
    public DefenderType defenderType;
    
    [Tooltip("Cost in resources to place this defender")]
    public int cost;
    
    [Header("Visual Feedback")]
    [Tooltip("Prefab to show as preview while dragging")]
    public GameObject previewPrefab;
    
    [Tooltip("Material for valid placement (green)")]
    public Material validPlacementMaterial;
    
    [Tooltip("Material for invalid placement (red)")]
    public Material invalidPlacementMaterial;
    
    [Header("References")]
    [Tooltip("Reference to the terrain generator")]
    public VoxelTerrainGenerator terrainGenerator;
    
    [Tooltip("Reference to the game manager")]
    public GameManager gameManager;
    
    private GameObject previewObject;
    private bool isValidPlacement = false;
    private Vector3Int currentGridPosition;
    private Camera cam;
    
    void Start()
    {
        // Find references if not assigned
        if (terrainGenerator == null)
            terrainGenerator = FindFirstObjectByType<VoxelTerrainGenerator>();
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();
        
        cam = Camera.main;
        
        // Create default materials if not assigned
        if (validPlacementMaterial == null)
        {
            validPlacementMaterial = new Material(Shader.Find("Standard"));
            validPlacementMaterial.color = new Color(0, 1, 0, 0.5f); // Green
        }
        
        if (invalidPlacementMaterial == null)
        {
            invalidPlacementMaterial = new Material(Shader.Find("Standard"));
            invalidPlacementMaterial.color = new Color(1, 0, 0, 0.5f); // Red
        }
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Check if player has enough resources
        if (gameManager.GetResources() < cost)
        {
            Debug.Log($"Not enough resources! Need {cost}, have {gameManager.GetResources()}");
            return;
        }
        
        // NEW: Check if defender limit is reached
        if (terrainGenerator.IsDefenderLimitReached())
        {
            Debug.Log($"Defender limit reached! Maximum {terrainGenerator.maxTotalDefenders} defenders allowed.");
            return;
        }
        
        // Create preview object
        if (previewPrefab != null)
        {
            Debug.Log($"Creating preview object from prefab: {previewPrefab.name}");
            previewObject = Instantiate(previewPrefab);
            previewObject.SetActive(false); // Start hidden
            Debug.Log("Preview object created with prefab");
        }
        else
        {
            Debug.Log("Creating simple cube preview object");
            // Create a simple cube preview if no prefab is assigned
            previewObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            previewObject.transform.localScale = Vector3.one * 0.8f;
            previewObject.GetComponent<Renderer>().material = validPlacementMaterial;
            Destroy(previewObject.GetComponent<Collider>()); // Remove collider
            previewObject.SetActive(false); // Start hidden
            Debug.Log("Simple cube preview object created");
        }
        
        // NEW: Show only pre-selected spots instead of all valid areas
        if (terrainGenerator != null)
        {
            Debug.Log("DragDropDefenderSystem: Calling HighlightPreSelectedDefenderSpots()");
            
            // Check if placement areas exist
            var placementAreas = terrainGenerator.GetPlacementAreas();
            Debug.Log($"Found {placementAreas.Count} placement areas");
            
            // If no placement areas exist, try to generate them
            if (placementAreas.Count == 0)
            {
                Debug.Log("No placement areas found - regenerating defender locations");
                terrainGenerator.GenerateDefenderLocations();
            }
            
            terrainGenerator.HighlightPreSelectedDefenderSpots();
        }
        else
        {
            Debug.LogError("DragDropDefenderSystem: terrainGenerator is null!");
        }
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (previewObject == null) return;
        
        // Convert screen position to world position using raycast
        Ray ray = cam.ScreenPointToRay(eventData.position);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit))
        {
            // Check if we hit a placement area
            DefenderPlacementArea placementArea = hit.collider.GetComponent<DefenderPlacementArea>();
            
            if (placementArea != null)
            {
                // Check if this placement area allows this defender type and is not occupied
                if (placementArea.CanPlaceDefender(defenderType) && !placementArea.isOccupied)
                {
                    // Check lane limit
                    int laneIndex = placementArea.laneIndex;
                    if (laneIndex >= 0 && !terrainGenerator.IsLaneAtDefenderLimit(laneIndex))
                    {
                        // Show preview on the placement area
                        Vector3 snappedWorldPos = placementArea.GetDefenderSpawnPosition();
                        previewObject.transform.position = snappedWorldPos;
                        previewObject.SetActive(true);
                        currentGridPosition = placementArea.gridPosition;
                        isValidPlacement = true;
                    }
                    else
                    {
                        // Lane at limit - hide preview
                        previewObject.SetActive(false);
                        currentGridPosition = Vector3Int.zero;
                        isValidPlacement = false;
                    }
                }
                else
                {
                    // Invalid placement - hide preview
                    previewObject.SetActive(false);
                    currentGridPosition = Vector3Int.zero;
                    isValidPlacement = false;
                }
            }
            else
            {
                // Not over a placement area - hide preview
                previewObject.SetActive(false);
                currentGridPosition = Vector3Int.zero;
                isValidPlacement = false;
            }
        }
        else
        {
            // No hit - hide preview
            previewObject.SetActive(false);
            currentGridPosition = Vector3Int.zero;
            isValidPlacement = false;
        }
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        if (previewObject == null) return;
        
        if (isValidPlacement)
        {
            // Place the defender
            if (gameManager.TryPlaceDefender(currentGridPosition, defenderType))
            {
                Debug.Log($"Successfully placed {defenderType} defender at {currentGridPosition}!");
                
                // Mark the placement area as occupied
                Ray ray = cam.ScreenPointToRay(eventData.position);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    DefenderPlacementArea placementArea = hit.collider.GetComponent<DefenderPlacementArea>();
                    if (placementArea != null)
                    {
                        placementArea.MarkOccupied();
                    }
                }
                
                // Update defender count display
                int currentDefenders = terrainGenerator.GetDefenderCount();
                int maxDefenders = terrainGenerator.maxTotalDefenders;
                Debug.Log($"Defenders: {currentDefenders}/{maxDefenders}");
                
                // Play placement effect
                PlayPlacementEffect(currentGridPosition);
            }
            else
            {
                Debug.Log("Failed to place defender - check resources and limits!");
            }
        }
        else
        {
            Debug.Log("Invalid placement location - must be on a valid placement area!");
        }
        
        // Clean up
        Destroy(previewObject);
        previewObject = null;
        
        // Clear highlights
        if (terrainGenerator != null)
        {
            terrainGenerator.ClearPlacementHighlights();
        }
    }
    
    /// <summary>
    /// Plays a visual effect when a defender is successfully placed
    /// </summary>
    void PlayPlacementEffect(Vector3Int gridPos)
    {
        Vector3 worldPos = terrainGenerator.GetSurfaceWorldPosition(gridPos);
        worldPos.y += 1f; // Above the surface
        
        // Create a simple particle effect
        GameObject effect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        effect.transform.position = worldPos;
        effect.transform.localScale = Vector3.one * 0.5f;
        effect.GetComponent<Renderer>().material.color = Color.yellow;
        Destroy(effect.GetComponent<Collider>());
        
        // Destroy the effect after a short time
        Destroy(effect, 1f);
        
        Debug.Log($"Defender placed at world position: {worldPos}");
    }
    
    /// <summary>
    /// Finds the nearest available defender spot to the given world position
    /// </summary>
    Vector3Int GetNearestAvailableDefenderSpot(Vector3 worldPos)
    {
        Vector3Int nearestSpot = Vector3Int.zero;
        float nearestDistance = float.MaxValue;
        
        foreach (var spot in terrainGenerator.GetAvailableDefenderSpots())
        {
            // Only consider spots that allow this defender type
            if (terrainGenerator.CanPlaceDefenderAt(spot, defenderType))
            {
                Vector3 spotWorldPos = terrainGenerator.GetSurfaceWorldPosition(spot);
                float distance = Vector3.Distance(worldPos, spotWorldPos);
                
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestSpot = spot;
                }
            }
        }
        
        return nearestSpot;
    }
}
