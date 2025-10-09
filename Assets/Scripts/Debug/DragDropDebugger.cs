using UnityEngine;

/// <summary>
/// Debug script to help diagnose drag-drop defender placement issues.
/// Attach this to any GameObject to enable debugging.
/// </summary>
public class DragDropDebugger : MonoBehaviour
{
    [Header("Debug Settings")]
    [Tooltip("Enable debug logging")]
    public bool enableDebugLogs = true;
    
    [Tooltip("Show valid placement areas on start")]
    public bool showValidAreasOnStart = true;
    
    [Header("References")]
    [Tooltip("Reference to terrain generator")]
    public VoxelTerrainGenerator terrainGenerator;
    
    [Tooltip("Reference to game manager")]
    public GameManager gameManager;
    
    void Start()
    {
        // Find references if not assigned
        if (terrainGenerator == null)
            terrainGenerator = FindFirstObjectByType<VoxelTerrainGenerator>();
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();
        
        if (enableDebugLogs)
        {
            Debug.Log("=== DragDrop Debugger Started ===");
            Debug.Log($"Terrain Generator: {(terrainGenerator != null ? "Found" : "NOT FOUND")}");
            Debug.Log($"Game Manager: {(gameManager != null ? "Found" : "NOT FOUND")}");
            
            if (terrainGenerator != null)
            {
                Debug.Log($"Terrain Generated: {terrainGenerator.IsReady}");
                Debug.Log($"Terrain Size: {terrainGenerator.width}x{terrainGenerator.depth}");
                
                // Test valid placement positions
                var validPositions = terrainGenerator.GetAllValidDefenderPositions();
                Debug.Log($"Valid Defender Positions Found: {validPositions.Count}");
                
                if (validPositions.Count == 0)
                {
                    Debug.LogWarning("NO VALID DEFENDER POSITIONS FOUND! This is likely the problem.");
                    Debug.Log("Check that:");
                    Debug.Log("1. Terrain is fully generated");
                    Debug.Log("2. Paths are created");
                    Debug.Log("3. IsValidDefenderPlacement() is working correctly");
                }
            }
        }
        
        if (showValidAreasOnStart && terrainGenerator != null)
        {
            terrainGenerator.HighlightAllValidDefenderAreas();
        }
    }
    
    void Update()
    {
        // Debug input
        if (Input.GetKeyDown(KeyCode.H))
        {
            if (terrainGenerator != null)
            {
                terrainGenerator.HighlightAllValidDefenderAreas();
                Debug.Log("Highlighted valid defender areas (Press H)");
            }
        }
        
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (terrainGenerator != null)
            {
                terrainGenerator.ClearPlacementHighlights();
                Debug.Log("Cleared placement highlights (Press C)");
            }
        }
        
        if (Input.GetKeyDown(KeyCode.T))
        {
            TestTerrainGeneration();
        }
    }
    
    void TestTerrainGeneration()
    {
        if (terrainGenerator == null) return;
        
        Debug.Log("=== Terrain Generation Test ===");
        Debug.Log($"Is Generated: {terrainGenerator.IsReady}");
        Debug.Log($"Width: {terrainGenerator.width}, Depth: {terrainGenerator.depth}");
        
        // Test a few positions
        for (int x = 0; x < Mathf.Min(10, terrainGenerator.width); x += 2)
        {
            for (int z = 0; z < Mathf.Min(10, terrainGenerator.depth); z += 2)
            {
                int surfaceY = terrainGenerator.GetSurfaceY(x, z);
                Vector3Int testPos = new Vector3Int(x, surfaceY - 1, z);
                bool isValid = terrainGenerator.IsValidDefenderPlacement(testPos);
                
                if (isValid)
                {
                    Debug.Log($"Valid position found at: {testPos}");
                }
            }
        }
    }
    
    /// <summary>
    /// Test drag-drop system components
    /// </summary>
    [ContextMenu("Test Drag-Drop Components")]
    public void TestDragDropComponents()
    {
        Debug.Log("=== Testing Drag-Drop Components ===");
        
        // Find all drag-drop components
        DragDropDefenderSystem[] dragDropSystems = FindObjectsOfType<DragDropDefenderSystem>();
        Debug.Log($"Found {dragDropSystems.Length} DragDropDefenderSystem components");
        
        foreach (var system in dragDropSystems)
        {
            Debug.Log($"- {system.gameObject.name}: Type={system.defenderType}, Cost={system.cost}");
            Debug.Log($"  Preview Prefab: {(system.previewPrefab != null ? "Assigned" : "NOT ASSIGNED")}");
            Debug.Log($"  Valid Material: {(system.validPlacementMaterial != null ? "Assigned" : "NOT ASSIGNED")}");
            Debug.Log($"  Invalid Material: {(system.invalidPlacementMaterial != null ? "Assigned" : "NOT ASSIGNED")}");
        }
        
        // Find shop manager
        DefenderShopManager shopManager = FindFirstObjectByType<DefenderShopManager>();
        Debug.Log($"Shop Manager: {(shopManager != null ? "Found" : "NOT FOUND")}");
        
        if (shopManager != null)
        {
            Debug.Log($"Basic Defender Button: {(shopManager.basicDefenderButton != null ? "Assigned" : "NOT ASSIGNED")}");
            Debug.Log($"Frost Tower Button: {(shopManager.frostTowerButton != null ? "Assigned" : "NOT ASSIGNED")}");
            Debug.Log($"Lightning Tower Button: {(shopManager.lightningTowerButton != null ? "Assigned" : "NOT ASSIGNED")}");
        }
    }
}
