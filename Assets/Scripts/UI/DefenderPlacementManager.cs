using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DefenderPlacementManager : MonoBehaviour
{
    public Button showLocationsButton;
    public VoxelTerrainGenerator terrainGenerator;
    public GameManager gameManager;
    public int pathIndex = 0;
    public int numLocations = 3;
    public int range = 2;

    private List<Vector3Int> defenderLocations;

    void Start()
    {
        if (terrainGenerator == null)
            terrainGenerator = FindObjectOfType<VoxelTerrainGenerator>();

        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();

        if (showLocationsButton != null)
            showLocationsButton.onClick.AddListener(ShowDefenderLocations);
    }

    void ShowDefenderLocations()
    {
        defenderLocations = terrainGenerator.GetDefenderLocationsNearPath(pathIndex, numLocations, range);
        terrainGenerator.HighlightDefenderLocations(pathIndex, numLocations, range);
    }

    void Update()
    {
        if (UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryPlaceDefenderAtHighlight();
        }
    }

    void TryPlaceDefenderAtHighlight()
    {
        Ray ray = Camera.main.ScreenPointToRay(UnityEngine.InputSystem.Mouse.current.position.ReadValue());
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform != null && hit.transform.name.StartsWith("Highlight_Defender"))
            {
                string[] parts = hit.transform.name.Split('_');
                int x = int.Parse(parts[2]);
                int z = int.Parse(parts[3]);

                Vector3Int location = new Vector3Int(x, 0, z);
                int surfaceY = terrainGenerator.GetSurfaceY(location.x, location.z);
                location.y = surfaceY - 1;

                gameManager.TryPlaceDefender(location);
            }
        }
    }
}
