using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles path selection for enemy spawning
/// </summary>
public class PathSelector : MonoBehaviour
{
    [Header("Path Selection")]
    [Tooltip("Currently selected path index (0-based)")]
    public int selectedPathIndex = 0;
    [Tooltip("Maximum number of paths available")]
    public int maxPaths = 5;
    
    [Header("References")]
    [Tooltip("Reference to the terrain generator")]
    public VoxelTerrainGenerator terrainGenerator;
    [Tooltip("Reference to the enemy spawner")]
    public EnemySpawner enemySpawner;
    
    [Header("Visual Feedback")]
    [Tooltip("Material to highlight selected path")]
    public Material selectedPathMaterial;
    [Tooltip("Material to highlight available paths")]
    public Material availablePathMaterial;
    
    private Material[] originalMaterials;
    private bool[] pathAvailability;
    
    void Start()
    {
        // Find references if not assigned
        if (terrainGenerator == null)
            terrainGenerator = FindFirstObjectByType<VoxelTerrainGenerator>();
        if (enemySpawner == null)
            enemySpawner = FindFirstObjectByType<EnemySpawner>();
            
        // Initialize path availability
        pathAvailability = new bool[maxPaths];
        UpdatePathAvailability();
    }
    
    void Update()
    {
        // Handle number key input for path selection
        HandlePathSelectionInput();
    }
    
    /// <summary>
    /// Handles input for path selection
    /// </summary>
    void HandlePathSelectionInput()
    {
        if (Keyboard.current == null) return;
        
        // Check for number keys 1-5
        for (int i = 1; i <= maxPaths; i++)
        {
            Key key = (Key)((int)Key.Digit1 + i - 1);
            if (Keyboard.current[key].wasPressedThisFrame)
            {
                SelectPath(i - 1); // Convert to 0-based index
            }
        }
    }
    
    /// <summary>
    /// Selects a path for enemy spawning
    /// </summary>
    /// <param name="pathIndex">Path index to select (0-based)</param>
    public void SelectPath(int pathIndex)
    {
        if (pathIndex < 0 || pathIndex >= maxPaths)
        {
            Debug.LogWarning($"Invalid path index: {pathIndex}");
            return;
        }
        
        if (!pathAvailability[pathIndex])
        {
            Debug.Log($"Path {pathIndex + 1} is not yet unlocked!");
            ShowPathLockedMessage(pathIndex + 1);
            return;
        }
        
        selectedPathIndex = pathIndex;
        Debug.Log($"🎯 Selected Path {pathIndex + 1}");
        
        // Update visual feedback
        UpdatePathVisuals();
        
        // Notify enemy spawner of path change
        if (enemySpawner != null)
        {
            enemySpawner.SetSelectedPath(selectedPathIndex);
        }
    }
    
    /// <summary>
    /// Updates which paths are available based on current wave
    /// </summary>
    public void UpdatePathAvailability()
    {
        if (terrainGenerator == null) return;
        
        var paths = terrainGenerator.GetPaths();
        int availablePaths = paths.Count;
        
        // Update availability array
        for (int i = 0; i < maxPaths; i++)
        {
            pathAvailability[i] = i < availablePaths;
        }
        
        // Ensure selected path is valid
        if (selectedPathIndex >= availablePaths)
        {
            selectedPathIndex = Mathf.Max(0, availablePaths - 1);
        }
        
        Debug.Log($"🛤️ Available paths: {availablePaths}/{maxPaths}");
    }
    
    /// <summary>
    /// Updates visual feedback for path selection
    /// </summary>
    void UpdatePathVisuals()
    {
        // This would be implemented based on your visual system
        // For now, just log the selection
        Debug.Log($"🎨 Visual feedback: Path {selectedPathIndex + 1} selected");
    }
    
    /// <summary>
    /// Gets the currently selected path
    /// </summary>
    /// <returns>List of Vector3Int positions for the selected path</returns>
    public System.Collections.Generic.List<Vector3Int> GetSelectedPath()
    {
        if (terrainGenerator == null) return null;
        
        var paths = terrainGenerator.GetPaths();
        if (selectedPathIndex >= 0 && selectedPathIndex < paths.Count)
        {
            return paths[selectedPathIndex];
        }
        
        return null;
    }
    
    /// <summary>
    /// Gets the number of available paths
    /// </summary>
    /// <returns>Number of unlocked paths</returns>
    public int GetAvailablePathCount()
    {
        if (terrainGenerator == null) return 0;
        return terrainGenerator.GetPaths().Count;
    }
    
    /// <summary>
    /// Shows a message when trying to select a locked path
    /// </summary>
    /// <param name="pathNumber">The path number that's locked</param>
    void ShowPathLockedMessage(int pathNumber)
    {
        // Get performance requirements from terrain generator
        if (terrainGenerator == null) return;
        
        float requiredPerformance = 0f;
        if (pathNumber == 4)
        {
            requiredPerformance = terrainGenerator.path4PerformanceRequirement;
        }
        else if (pathNumber == 5)
        {
            requiredPerformance = terrainGenerator.path5PerformanceRequirement;
        }
        
        // Get current performance
        PlayerPerformanceTracker performanceTracker = FindFirstObjectByType<PlayerPerformanceTracker>();
        if (performanceTracker != null)
        {
            float currentPerformance = performanceTracker.performanceScore;
            Debug.Log($"❌ Lane {pathNumber} locked! Performance: {currentPerformance:F1}/{requiredPerformance}");
        }
        else
        {
            Debug.Log($"❌ Lane {pathNumber} locked! Performance tracking not available.");
        }
    }
}
