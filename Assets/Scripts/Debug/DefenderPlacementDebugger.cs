using UnityEngine;

/// <summary>
/// Debug script to help diagnose defender placement issues.
/// This will check if all defender prefabs are properly assigned and configured.
/// </summary>
public class DefenderPlacementDebugger : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the game manager")]
    public GameManager gameManager;
    
    [Tooltip("Reference to the terrain generator")]
    public VoxelTerrainGenerator terrainGenerator;
    
    void Start()
    {
        // Find references if not assigned
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();
        if (terrainGenerator == null)
            terrainGenerator = FindFirstObjectByType<VoxelTerrainGenerator>();
        
        Debug.Log("=== Defender Placement Debugger Started ===");
        CheckDefenderPrefabs();
        CheckDefenderPlacementSystem();
    }
    
    void CheckDefenderPrefabs()
    {
        Debug.Log("=== Checking Defender Prefabs ===");
        
        if (gameManager == null)
        {
            Debug.LogError("GameManager not found!");
            return;
        }
        
        // Check basic defender
        Debug.Log($"Basic Defender Prefab: {(gameManager.defenderPrefab != null ? "ASSIGNED" : "NOT ASSIGNED")}");
        if (gameManager.defenderPrefab != null)
        {
            Debug.Log($"  - Name: {gameManager.defenderPrefab.name}");
            Debug.Log($"  - Has Defender Script: {gameManager.defenderPrefab.GetComponent<Defender>() != null}");
        }
        
        // Check frost tower
        Debug.Log($"Frost Tower Prefab: {(gameManager.frostTowerPrefab != null ? "ASSIGNED" : "NOT ASSIGNED")}");
        if (gameManager.frostTowerPrefab != null)
        {
            Debug.Log($"  - Name: {gameManager.frostTowerPrefab.name}");
            Debug.Log($"  - Has FrostTowerDefender Script: {gameManager.frostTowerPrefab.GetComponent<FrostTowerDefender>() != null}");
        }
        else
        {
            Debug.LogError("FROST TOWER PREFAB NOT ASSIGNED! This is why you can't place frost towers.");
        }
        
        // Check lightning tower
        Debug.Log($"Lightning Tower Prefab: {(gameManager.lightningTowerPrefab != null ? "ASSIGNED" : "NOT ASSIGNED")}");
        if (gameManager.lightningTowerPrefab != null)
        {
            Debug.Log($"  - Name: {gameManager.lightningTowerPrefab.name}");
            Debug.Log($"  - Has LightningTowerDefender Script: {gameManager.lightningTowerPrefab.GetComponent<LightningTowerDefender>() != null}");
        }
        else
        {
            Debug.LogError("LIGHTNING TOWER PREFAB NOT ASSIGNED! This is why you can't place lightning towers.");
        }
    }
    
    void CheckDefenderPlacementSystem()
    {
        Debug.Log("=== Checking Drag-Drop Systems ===");
        
        // Find all drag-drop systems
        DragDropDefenderSystem[] dragDropSystems = FindObjectsOfType<DragDropDefenderSystem>();
        Debug.Log($"Found {dragDropSystems.Length} DragDropDefenderSystem components");
        
        foreach (var system in dragDropSystems)
        {
            Debug.Log($"- {system.gameObject.name}:");
            Debug.Log($"  - Defender Type: {system.defenderType}");
            Debug.Log($"  - Cost: {system.cost}");
            Debug.Log($"  - Preview Prefab: {(system.previewPrefab != null ? "ASSIGNED" : "NOT ASSIGNED")}");
            Debug.Log($"  - Game Manager: {(system.gameManager != null ? "ASSIGNED" : "NOT ASSIGNED")}");
            Debug.Log($"  - Terrain Generator: {(system.terrainGenerator != null ? "ASSIGNED" : "NOT ASSIGNED")}");
        }
    }
    
    [ContextMenu("Test Defender Placement")]
    public void TestDefenderPlacement()
    {
        if (gameManager == null)
        {
            Debug.LogError("GameManager not found!");
            return;
        }
        
        Debug.Log("=== Testing Defender Placement ===");
        
        // Test basic defender placement
        Vector3Int testPos = new Vector3Int(10, 0, 10);
        Debug.Log($"Testing Basic Defender placement at {testPos}");
        bool basicResult = gameManager.TryPlaceDefender(testPos, DefenderType.Basic);
        Debug.Log($"Basic Defender placement: {(basicResult ? "SUCCESS" : "FAILED")}");
        
        // Test frost tower placement
        Debug.Log($"Testing Frost Tower placement at {testPos}");
        bool frostResult = gameManager.TryPlaceDefender(testPos, DefenderType.FrostTower);
        Debug.Log($"Frost Tower placement: {(frostResult ? "SUCCESS" : "FAILED")}");
        
        // Test lightning tower placement
        Debug.Log($"Testing Lightning Tower placement at {testPos}");
        bool lightningResult = gameManager.TryPlaceDefender(testPos, DefenderType.LightningTower);
        Debug.Log($"Lightning Tower placement: {(lightningResult ? "SUCCESS" : "FAILED")}");
    }
    
    [ContextMenu("Create Missing Prefabs")]
    public void CreateMissingPrefabs()
    {
        Debug.Log("=== Creating Missing Prefabs ===");
        
        if (gameManager == null)
        {
            Debug.LogError("GameManager not found!");
            return;
        }
        
        // Create frost tower prefab if missing
        if (gameManager.frostTowerPrefab == null)
        {
            Debug.Log("Creating Frost Tower Prefab...");
            GameObject frostTower = CreateDefenderPrefab("FrostTower", DefenderType.FrostTower);
            gameManager.frostTowerPrefab = frostTower;
            Debug.Log("Frost Tower Prefab created and assigned!");
        }
        
        // Create lightning tower prefab if missing
        if (gameManager.lightningTowerPrefab == null)
        {
            Debug.Log("Creating Lightning Tower Prefab...");
            GameObject lightningTower = CreateDefenderPrefab("LightningTower", DefenderType.LightningTower);
            gameManager.lightningTowerPrefab = lightningTower;
            Debug.Log("Lightning Tower Prefab created and assigned!");
        }
    }
    
    GameObject CreateDefenderPrefab(string name, DefenderType type)
    {
        // Create a simple defender prefab
        GameObject defender = GameObject.CreatePrimitive(PrimitiveType.Cube);
        defender.name = name;
        defender.transform.localScale = Vector3.one * 1.5f;
        
        // Add the appropriate script
        switch (type)
        {
            case DefenderType.FrostTower:
                defender.AddComponent<FrostTowerDefender>();
                defender.GetComponent<Renderer>().material.color = Color.cyan;
                break;
            case DefenderType.LightningTower:
                defender.AddComponent<LightningTowerDefender>();
                defender.GetComponent<Renderer>().material.color = Color.yellow;
                break;
            default:
                defender.AddComponent<Defender>();
                break;
        }
        
        // Add a collider for placement detection
        defender.AddComponent<BoxCollider>();
        
        return defender;
    }
}
