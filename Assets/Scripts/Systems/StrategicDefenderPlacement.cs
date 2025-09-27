using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class StrategicDefenderPlacement : MonoBehaviour
{
    [Header("References")]
    public VoxelTerrainGenerator terrainGenerator;
    public GameManager gameManager;
    public Camera strategicCamera;
    
    [Header("Placement Settings")]
    public int maxDefenderLocationsPerPath = 5;
    public float placementMarkerHeight = 0.5f;
    
    [Header("UI References")]
    public GameObject tooltipPrefab;
    public Canvas worldSpaceCanvas;
    
    [Header("Placement Marker Materials")]
    public Material availableMarkerMaterial;
    public Material hoveredMarkerMaterial;
    public Material occupiedMarkerMaterial;
    
    private List<DefenderPlacementMarker> placementMarkers = new List<DefenderPlacementMarker>();
    private DefenderPlacementMarker currentHoveredMarker;
    private GameObject currentTooltip;
    
    void Start()
    {
        if (terrainGenerator == null) terrainGenerator = FindFirstObjectByType<VoxelTerrainGenerator>();
        if (gameManager == null) gameManager = FindFirstObjectByType<GameManager>();
        if (strategicCamera == null) strategicCamera = FindFirstObjectByType<StrategicCameraController>().strategicCamera;
        
        CreateWorldSpaceCanvas();
    }
    
    void CreateWorldSpaceCanvas()
    {
        if (worldSpaceCanvas == null)
        {
            GameObject canvasObj = new GameObject("Strategic UI Canvas");
            worldSpaceCanvas = canvasObj.AddComponent<Canvas>();
            worldSpaceCanvas.renderMode = RenderMode.WorldSpace;
            worldSpaceCanvas.worldCamera = strategicCamera;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            
            canvasObj.AddComponent<GraphicRaycaster>();
        }
    }
    
    public void ShowAllDefenderLocations()
    {
        ClearAllMarkers();
        
        List<Vector3Int> defenderLocations = terrainGenerator.DefenderLocations;
        
        foreach (Vector3Int location in defenderLocations)
        {
            CreatePlacementMarker(location);
        }
    }
    
    public void HideAllDefenderLocations()
    {
        ClearAllMarkers();
    }
    
    void CreatePlacementMarker(Vector3Int gridPosition)
    {
        Vector3 worldPosition = terrainGenerator.GetSurfaceWorldPosition(gridPosition);
        worldPosition.y += placementMarkerHeight;
        
        GameObject markerObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        markerObj.name = $"DefenderMarker_{gridPosition.x}_{gridPosition.z}";
        markerObj.transform.position = worldPosition;
        markerObj.transform.localScale = new Vector3(0.8f, 0.1f, 0.8f);
        
        DefenderPlacementMarker marker = markerObj.AddComponent<DefenderPlacementMarker>();
        marker.Initialize(gridPosition, this, availableMarkerMaterial, hoveredMarkerMaterial);
        
        placementMarkers.Add(marker);
    }
    
    void Update()
    {
        if (strategicCamera != null && strategicCamera.gameObject.activeInHierarchy)
        {
            HandleMouseInput();
        }
    }
    
    void HandleMouseInput()
    {
        Vector2 mousePosition = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
        Ray ray = strategicCamera.ScreenPointToRay(mousePosition);
        
        RaycastHit hit;
        DefenderPlacementMarker hitMarker = null;
        
        if (Physics.Raycast(ray, out hit))
        {
            hitMarker = hit.collider.GetComponent<DefenderPlacementMarker>();
        }
        
        if (hitMarker != currentHoveredMarker)
        {
            if (currentHoveredMarker != null)
            {
                currentHoveredMarker.OnHoverExit();
                HideTooltip();
            }
            
            currentHoveredMarker = hitMarker;
            
            if (currentHoveredMarker != null)
            {
                currentHoveredMarker.OnHoverEnter();
                ShowTooltip(currentHoveredMarker);
            }
        }
        
        if (UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame && currentHoveredMarker != null)
        {
            TryPlaceDefender(currentHoveredMarker);
        }
    }
    
    void ShowTooltip(DefenderPlacementMarker marker)
    {
        if (tooltipPrefab == null) return;
        
        Vector3 tooltipPosition = marker.transform.position + Vector3.up * 2f;
        currentTooltip = Instantiate(tooltipPrefab, tooltipPosition, Quaternion.identity, worldSpaceCanvas.transform);
        
        TextMeshProUGUI tooltipText = currentTooltip.GetComponentInChildren<TextMeshProUGUI>();
        if (tooltipText != null)
        {
            if (marker.IsOccupied)
            {
                tooltipText.text = "Defender Already Placed";
            }
            else if (gameManager.GetResources() >= gameManager.defenderCost)
            {
                tooltipText.text = $"Place Defender\nCost: {gameManager.defenderCost}";
            }
            else
            {
                tooltipText.text = $"Insufficient Resources\nNeed: {gameManager.defenderCost}";
            }
        }
    }
    
    void HideTooltip()
    {
        if (currentTooltip != null)
        {
            Destroy(currentTooltip);
            currentTooltip = null;
        }
    }
    
    void TryPlaceDefender(DefenderPlacementMarker marker)
    {
        if (marker.IsOccupied) return;
        
        if (gameManager.TryPlaceDefender(marker.GridPosition))
        {
            marker.SetOccupied(true, occupiedMarkerMaterial);
            HideTooltip();
        }
    }
    
    void ClearAllMarkers()
    {
        foreach (DefenderPlacementMarker marker in placementMarkers)
        {
            if (marker != null)
            {
                Destroy(marker.gameObject);
            }
        }
        placementMarkers.Clear();
        
        if (currentTooltip != null)
        {
            Destroy(currentTooltip);
            currentTooltip = null;
        }
    }
}