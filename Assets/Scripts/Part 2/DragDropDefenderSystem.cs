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
        
        // Create preview object
        if (previewPrefab != null)
        {
            previewObject = Instantiate(previewPrefab);
            // Check if renderer exists before accessing it
            Renderer renderer = previewObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = invalidPlacementMaterial;
            }
            else
            {
                Debug.LogWarning($"Preview prefab {previewPrefab.name} has no Renderer component!");
            }
        }
        else
        {
            // Create a simple cube preview if no prefab is assigned
            previewObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            previewObject.transform.localScale = Vector3.one * 0.8f;
            previewObject.GetComponent<Renderer>().material = invalidPlacementMaterial;
            Destroy(previewObject.GetComponent<Collider>()); // Remove collider
        }
        
        // Show all valid placement areas
        if (terrainGenerator != null)
        {
            terrainGenerator.HighlightAllValidDefenderAreas();
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
            // Check if we hit a placement point
            if (hit.collider.CompareTag("PlacementPoint"))
            {
                // Get the placement point data
                PlacementPointData pointData = hit.collider.GetComponent<PlacementPointData>();
                
                if (pointData != null && pointData.IsAvailable())
                {
                    // This is a valid placement point - show preview
                    Vector3 snappedWorldPos = hit.collider.transform.position;
                    snappedWorldPos.y += 0.1f; // Slightly above the plane
                    previewObject.transform.position = snappedWorldPos;
                    previewObject.SetActive(true);
                    
                    // Use the valid grid position from the placement point
                    currentGridPosition = pointData.GetValidGridPosition();
                    isValidPlacement = true;
                    
                    // Update visual feedback
                    Renderer renderer = previewObject.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.material = validPlacementMaterial;
                    }
                    
                    Debug.Log($"Hovering over placement point at {snappedWorldPos}, valid grid position: {currentGridPosition}");
                }
                else
                {
                    // Placement point is occupied or invalid
                    previewObject.SetActive(false);
                    currentGridPosition = Vector3Int.zero;
                    isValidPlacement = false;
                }
            }
            else
            {
                // Not over a placement point - hide preview
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
            Debug.Log($"Attempting to place {defenderType} at {currentGridPosition}");
            
            // Place the defender
            if (gameManager.TryPlaceDefender(currentGridPosition, defenderType))
            {
                Debug.Log($"Successfully placed {defenderType} defender at {currentGridPosition}!");
                
                // Mark the placement point as occupied
                Ray ray = cam.ScreenPointToRay(eventData.position);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit) && hit.collider.CompareTag("PlacementPoint"))
                {
                    PlacementPointData pointData = hit.collider.GetComponent<PlacementPointData>();
                    if (pointData != null)
                    {
                        pointData.MarkAsOccupied();
                        Debug.Log($"Placement point {hit.collider.name} is now occupied");
                    }
                }
                
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
            Debug.Log("Invalid placement location - must be on a valid placement point!");
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
}
