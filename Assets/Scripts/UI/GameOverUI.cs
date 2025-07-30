using UnityEngine;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text messageText;

    public void Show(string message)
    {
        if (panel != null) panel.SetActive(true);
        if (messageText != null) messageText.text = message;
    }

    public void Hide()
    {
        if (panel != null) panel.SetActive(false);
    }
} 