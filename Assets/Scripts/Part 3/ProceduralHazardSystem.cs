using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Main system for procedurally generating environmental hazards during waves.
/// Manages hazard spawning, lifecycle, and integration with the terrain system.
/// </summary>
public class ProceduralHazardSystem : MonoBehaviour
{
    [Header("Hazard Spawning")]
    [Tooltip("Minimum hazards per wave")]
    public int minHazardsPerWave = 2;
    
    [Tooltip("Maximum hazards per wave")]
    public int maxHazardsPerWave = 5;
    
    [Tooltip("Wave when hazards start spawning")]
    public int hazardStartWave = 3;
    
    [Header("Hazard Prefabs")]
    [Tooltip("Prefab for lava pool hazards")]
    public GameObject lavaPoolPrefab;
    
    [Tooltip("Prefab for ice patch hazards")]
    public GameObject icePatchPrefab;
    
    [Tooltip("Prefab for wind zone hazards")]
    public GameObject windZonePrefab;
    
    [Header("Hazard Materials")]
    [Tooltip("Material with HazardDistortion shader")]
    public Material hazardDistortionMaterial;
    
    [Header("Terrain Integration")]
    [Tooltip("Reference to terrain generator")]
    public VoxelTerrainGenerator terrainGenerator;
    
    [Tooltip("Minimum distance from paths")]
    public float minDistanceFromPaths = 5f;
    
    [Tooltip("Minimum distance from tower")]
    public float minDistanceFromTower = 8f;
    
    [Tooltip("Minimum distance from defender zones")]
    public float minDistanceFromDefenderZones = 3f;
    
    [Header("Hazard Scaling")]
    [Tooltip("Intensity scaling per wave")]
    public float intensityScalingPerWave = 0.1f;
    
    [Tooltip("Size scaling per wave")]
    public float sizeScalingPerWave = 0.05f;
    
    // Current active hazards
    private List<EnvironmentalHazard> activeHazards = new List<EnvironmentalHazard>();
    
    // References
    private GameManager gameManager;
    private EnemySpawner enemySpawner;
    
    void Start()
    {
        // Get references
        gameManager = FindFirstObjectByType<GameManager>();
        enemySpawner = FindFirstObjectByType<EnemySpawner>();
        
        if (terrainGenerator == null)
            terrainGenerator = FindFirstObjectByType<VoxelTerrainGenerator>();
        
        // Subscribe to wave events
        if (enemySpawner != null)
        {
            // We'll check for wave changes in Update
        }
    }
    
    void Update()
    {
        // Check if we should spawn hazards for new waves
        if (enemySpawner != null)
        {
            int currentWave = enemySpawner.currentWave;
            if (currentWave >= hazardStartWave)
            {
                // Check if we need to spawn hazards for this wave
                if (ShouldSpawnHazardsForWave(currentWave))
                {
                    SpawnHazardsForWave(currentWave);
                }
            }
        }
        
        // Clean up expired hazards
        CleanupExpiredHazards();
    }
    
    private bool ShouldSpawnHazardsForWave(int wave)
    {
        // Spawn hazards at the start of each wave
        // This is a simple implementation - in a real game you'd track wave state better
        return activeHazards.Count == 0 || Random.Range(0f, 1f) < 0.3f; // 30% chance to add more hazards
    }
    
    private void SpawnHazardsForWave(int wave)
    {
        int hazardCount = Random.Range(minHazardsPerWave, maxHazardsPerWave + 1);
        
        for (int i = 0; i < hazardCount; i++)
        {
            SpawnRandomHazard(wave);
        }
    }
    
    private void SpawnRandomHazard(int wave)
    {
        // Select random hazard type with weighted probability
        HazardType hazardType = SelectRandomHazardType();
        
        // Find valid spawn position
        Vector3 spawnPosition = FindValidSpawnPosition();
        if (spawnPosition == Vector3.zero) return; // No valid position found
        
        // Create hazard
        GameObject hazardPrefab = GetHazardPrefab(hazardType);
        if (hazardPrefab == null) return;
        
        GameObject hazardObj = Instantiate(hazardPrefab, spawnPosition, Quaternion.identity);
        EnvironmentalHazard hazard = hazardObj.GetComponent<EnvironmentalHazard>();
        
        if (hazard != null)
        {
            // Configure hazard based on wave
            ConfigureHazardForWave(hazard, wave);
            activeHazards.Add(hazard);
        }
    }
    
    private HazardType SelectRandomHazardType()
    {
        float rand = Random.Range(0f, 1f);
        
        if (rand < 0.4f) return HazardType.Lava;      // 40% chance
        else if (rand < 0.75f) return HazardType.Ice;  // 35% chance
        else return HazardType.Wind;                   // 25% chance
    }
    
    private Vector3 FindValidSpawnPosition()
    {
        if (terrainGenerator == null) return Vector3.zero;
        
        int attempts = 0;
        int maxAttempts = 50;
        
        while (attempts < maxAttempts)
        {
            // Generate random position within terrain bounds
            Vector3 randomPos = new Vector3(
                Random.Range(-terrainGenerator.width / 2f, terrainGenerator.width / 2f),
                0f,
                Random.Range(-terrainGenerator.depth / 2f, terrainGenerator.depth / 2f)
            );
            
            // Get surface position
            Vector3 surfacePos = terrainGenerator.GetSurfaceWorldPosition(
                terrainGenerator.WorldToGridPosition(randomPos)
            );
            
            // Check if position is valid
            if (IsValidHazardPosition(surfacePos))
            {
                return surfacePos;
            }
            
            attempts++;
        }
        
        return Vector3.zero; // No valid position found
    }
    
    private bool IsValidHazardPosition(Vector3 position)
    {
        // Check distance from tower
        Tower tower = FindFirstObjectByType<Tower>();
        if (tower != null)
        {
            float distanceFromTower = Vector3.Distance(position, tower.transform.position);
            if (distanceFromTower < minDistanceFromTower) return false;
        }
        
        // Check distance from paths (simplified - would need path data from terrain generator)
        // For now, we'll use a simple distance check from center
        float distanceFromCenter = Vector3.Distance(position, Vector3.zero);
        if (distanceFromCenter < minDistanceFromPaths) return false;
        
        // Check distance from existing hazards
        foreach (EnvironmentalHazard hazard in activeHazards)
        {
            if (hazard != null)
            {
                float distance = Vector3.Distance(position, hazard.transform.position);
                if (distance < hazard.effectRadius + 3f) return false; // Minimum spacing
            }
        }
        
        return true;
    }
    
    private GameObject GetHazardPrefab(HazardType hazardType)
    {
        switch (hazardType)
        {
            case HazardType.Lava:
                return lavaPoolPrefab;
            case HazardType.Ice:
                return icePatchPrefab;
            case HazardType.Wind:
                return windZonePrefab;
            default:
                return lavaPoolPrefab;
        }
    }
    
    private void ConfigureHazardForWave(EnvironmentalHazard hazard, int wave)
    {
        // Scale intensity based on wave
        float intensity = 1f + (wave - hazardStartWave) * intensityScalingPerWave;
        hazard.SetIntensity(intensity);
        
        // Scale size based on wave
        float sizeMultiplier = 1f + (wave - hazardStartWave) * sizeScalingPerWave;
        hazard.effectRadius *= sizeMultiplier;
        
        // Set duration (hazards last 2-4 waves)
        int durationWaves = Random.Range(2, 5);
        hazard.duration = durationWaves * 30f; // Assuming 30 seconds per wave
        
        // Apply hazard material
        if (hazardDistortionMaterial != null)
        {
            hazard.hazardMaterial = hazardDistortionMaterial;
        }
    }
    
    private void CleanupExpiredHazards()
    {
        for (int i = activeHazards.Count - 1; i >= 0; i--)
        {
            if (activeHazards[i] == null)
            {
                activeHazards.RemoveAt(i);
            }
        }
    }
    
    /// <summary>
    /// Gets all active hazards of a specific type
    /// </summary>
    public List<EnvironmentalHazard> GetHazardsOfType(HazardType type)
    {
        List<EnvironmentalHazard> hazardsOfType = new List<EnvironmentalHazard>();
        
        foreach (EnvironmentalHazard hazard in activeHazards)
        {
            if (hazard != null && hazard.hazardType == type)
            {
                hazardsOfType.Add(hazard);
            }
        }
        
        return hazardsOfType;
    }
    
    /// <summary>
    /// Gets all active hazards
    /// </summary>
    public List<EnvironmentalHazard> GetAllActiveHazards()
    {
        return new List<EnvironmentalHazard>(activeHazards);
    }
    
    /// <summary>
    /// Clears all active hazards
    /// </summary>
    public void ClearAllHazards()
    {
        foreach (EnvironmentalHazard hazard in activeHazards)
        {
            if (hazard != null)
            {
                Destroy(hazard.gameObject);
            }
        }
        activeHazards.Clear();
    }
    
    /// <summary>
    /// Manually spawns a hazard at a specific position
    /// </summary>
    public void SpawnHazardAtPosition(HazardType type, Vector3 position, int wave)
    {
        GameObject hazardPrefab = GetHazardPrefab(type);
        if (hazardPrefab == null) return;
        
        GameObject hazardObj = Instantiate(hazardPrefab, position, Quaternion.identity);
        EnvironmentalHazard hazard = hazardObj.GetComponent<EnvironmentalHazard>();
        
        if (hazard != null)
        {
            ConfigureHazardForWave(hazard, wave);
            activeHazards.Add(hazard);
        }
    }
}
