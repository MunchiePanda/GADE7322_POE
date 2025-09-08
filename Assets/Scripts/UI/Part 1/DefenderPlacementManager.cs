using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class DefenderPlacementManager : MonoBehaviour
{
    public Button placeDefenderButton;
    public VoxelTerrainGenerator terrainGenerator;
    public GameManager gameManager;
    public int numLocations = 3;
    public int range = 2;

    private int selectedPathIndex = 0;
    private List<Vector3Int> selectedDefenderLocations = new List<Vector3Int>();
    private int selectedLocationIndex = 0;

    void Start()
    {
        if (terrainGenerator == null)
            terrainGenerator = FindObjectOfType<VoxelTerrainGenerator>();

        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();

        if (placeDefenderButton != null)
            placeDefenderButton.onClick.AddListener(PlaceDefenderAtSelectedLocation);

        // Highlight the initial path and defender locations
        SelectPath(selectedPathIndex);
    }

    void Update()
    {
        // Check for key presses to switch paths
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
            SelectPath(0);
        else if (Keyboard.current.digit2Key.wasPressedThisFrame)
            SelectPath(1);
        else if (Keyboard.current.digit3Key.wasPressedThisFrame)
            SelectPath(2);
        else if (Keyboard.current.digit4Key.wasPressedThisFrame)
            SelectPath(3);
        else if (Keyboard.current.digit5Key.wasPressedThisFrame)
            SelectPath(4);
    }

    void SelectPath(int pathIndex)
    {
        if (pathIndex < 0 || pathIndex >= terrainGenerator.GetPaths().Count)
            return;

        // Update the path index
        selectedPathIndex = pathIndex;

        // Highlight the selected path in pink
        terrainGenerator.HighlightPath(selectedPathIndex);

        // Highlight defender locations in blue
        selectedDefenderLocations = terrainGenerator.GetDefenderLocationsNearPath(selectedPathIndex, numLocations, range);
        terrainGenerator.HighlightDefenderLocations(selectedPathIndex, numLocations, range);

        // Reset the location index
        selectedLocationIndex = 0;
    }

    void PlaceDefenderAtSelectedLocation()
    {
        if (selectedDefenderLocations.Count == 0 || selectedLocationIndex >= selectedDefenderLocations.Count)
            return;

        Vector3Int location = selectedDefenderLocations[selectedLocationIndex];
        int surfaceY = terrainGenerator.GetSurfaceY(location.x, location.z);
        location.y = surfaceY - 1;

        gameManager.TryPlaceDefender(location);

        // Move to the next location
        selectedLocationIndex++;
    }
}
