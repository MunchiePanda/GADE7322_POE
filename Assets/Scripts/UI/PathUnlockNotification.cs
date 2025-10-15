using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Handles UI notifications for path unlocks
/// </summary>
public class PathUnlockNotification : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Text component to display notifications")]
    public TextMeshProUGUI notificationText;
    [Tooltip("Panel that contains the notification")]
    public GameObject notificationPanel;
    
    [Header("Animation Settings")]
    [Tooltip("Duration to show the notification")]
    public float displayDuration = 3f;
    [Tooltip("Animation speed for fade in/out")]
    public float animationSpeed = 2f;
    
    private Coroutine currentNotification;
    
    void Start()
    {
        // Hide notification panel initially
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Shows a path unlock notification
    /// </summary>
    /// <param name="pathNumber">The path number that was unlocked</param>
    public void ShowPathUnlockNotification(int pathNumber)
    {
        if (notificationText == null || notificationPanel == null) return;
        
        // Stop any existing notification
        if (currentNotification != null)
        {
            StopCoroutine(currentNotification);
        }
        
        // Set the notification text
        notificationText.text = $"🚀 LANE {pathNumber} UNLOCKED!\nPress {pathNumber} to select this lane";
        
        // Start the notification coroutine
        currentNotification = StartCoroutine(ShowNotificationCoroutine());
    }
    
    /// <summary>
    /// Shows a performance requirement notification
    /// </summary>
    /// <param name="pathNumber">The path number that's locked</param>
    /// <param name="currentPerformance">Current performance score</param>
    /// <param name="requiredPerformance">Required performance score</param>
    public void ShowPerformanceRequirementNotification(int pathNumber, float currentPerformance, float requiredPerformance)
    {
        if (notificationText == null || notificationPanel == null) return;
        
        // Stop any existing notification
        if (currentNotification != null)
        {
            StopCoroutine(currentNotification);
        }
        
        // Set the notification text
        notificationText.text = $"❌ LANE {pathNumber} LOCKED\nPerformance: {currentPerformance:F1}/{requiredPerformance}\nPlay better to unlock!";
        
        // Start the notification coroutine
        currentNotification = StartCoroutine(ShowNotificationCoroutine());
    }
    
    /// <summary>
    /// Coroutine to handle notification display and fade
    /// </summary>
    IEnumerator ShowNotificationCoroutine()
    {
        // Show the panel
        notificationPanel.SetActive(true);
        
        // Fade in
        CanvasGroup canvasGroup = notificationPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = notificationPanel.AddComponent<CanvasGroup>();
        }
        
        // Fade in
        float alpha = 0f;
        while (alpha < 1f)
        {
            alpha += Time.deltaTime * animationSpeed;
            canvasGroup.alpha = alpha;
            yield return null;
        }
        canvasGroup.alpha = 1f;
        
        // Wait for display duration
        yield return new WaitForSeconds(displayDuration);
        
        // Fade out
        while (alpha > 0f)
        {
            alpha -= Time.deltaTime * animationSpeed;
            canvasGroup.alpha = alpha;
            yield return null;
        }
        canvasGroup.alpha = 0f;
        
        // Hide the panel
        notificationPanel.SetActive(false);
        
        currentNotification = null;
    }
}
