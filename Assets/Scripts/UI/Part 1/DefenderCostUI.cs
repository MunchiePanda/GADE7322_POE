using UnityEngine;
using TMPro;

public class DefenderCostUI : MonoBehaviour
{
    [SerializeField] private TMP_Text costText;

    public void SetCost(int value)
    {
        if (costText != null)
        {
            costText.text = $"Defender Cost: {value}";
        }
    }
} 