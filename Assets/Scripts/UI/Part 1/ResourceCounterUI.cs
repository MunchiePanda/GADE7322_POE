using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResourceCounterUI : MonoBehaviour
{
    [SerializeField] private TMP_Text resourceText;

    public void SetResource(int value)
    {
        if (resourceText != null)
        {
            resourceText.text = $"Resources: {value}";
        }
    }
} 