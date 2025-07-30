using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image fillImage;
    [SerializeField] private Color fullHealthColor = Color.green;
    [SerializeField] private Color lowHealthColor = Color.red;

    public void SetHealth(float current, float max)
    {
        if (healthSlider != null)
        {
            healthSlider.value = current / max;
        }
        if (fillImage != null)
        {
            fillImage.color = Color.Lerp(lowHealthColor, fullHealthColor, current / max);
        }
    }
} 