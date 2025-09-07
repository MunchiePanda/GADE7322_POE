using UnityEngine;
using UnityEngine.UI;

public class DefenderPlacementButton : MonoBehaviour
{
    public Button showDefenderLocationsButton;
    public DefenderPlacement defenderPlacement;
    public int pathIndex = 0;

    void Start()
    {
        if (showDefenderLocationsButton != null)
        {
            showDefenderLocationsButton.onClick.AddListener(OnShowDefenderLocationsButtonClicked);
        }
    }

    void OnShowDefenderLocationsButtonClicked()
    {
        if (defenderPlacement != null)
        {
            defenderPlacement.ShowDefenderLocations(pathIndex);
        }
    }
}
