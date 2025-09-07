using UnityEngine;
using UnityEngine.UI;

public class ShowDefenderLocationsButton : MonoBehaviour
{
    public Button showLocationsButton;
    public DefenderPlacement defenderPlacement;
    public int pathIndex = 0;

    void Start()
    {
        if (showLocationsButton != null)
        {
            showLocationsButton.onClick.AddListener(OnShowLocationsButtonClicked);
        }
    }

    void OnShowLocationsButtonClicked()
    {
        if (defenderPlacement != null)
        {
            defenderPlacement.ShowPlacementLocations(pathIndex);
        }
    }
}
