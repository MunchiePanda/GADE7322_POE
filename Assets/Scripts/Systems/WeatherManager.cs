using UnityEngine;
using System.Collections;

/// <summary>
/// Manages dynamic weather effects (rain, snow) that impact gameplay.
/// Attach to the GameManager or a dedicated WeatherManager GameObject.
/// </summary>
public class WeatherManager : MonoBehaviour
{
    [Header("Weather Settings")]
    [Tooltip("Time between weather changes (seconds).")]
    public float weatherChangeInterval = 30f;

    [Tooltip("Duration of each weather event (seconds).")]
    public float weatherDuration = 20f;

    [Tooltip("Prefabs for weather effects (e.g., rain, snow).")]
    public GameObject rainPrefab;
    public GameObject snowPrefab;

    [Header("Gameplay Effects")]
    [Tooltip("Factor by which enemy speed is reduced during rain (0.5 = 50% slower).")]
    public float rainSlowFactor = 0.7f;

    [Tooltip("Factor by which enemy speed is reduced during snow (0.5 = 50% slower).")]
    public float snowSlowFactor = 0.6f;

    private GameObject currentWeatherEffect;
    private Coroutine weatherRoutine;
    private Enemy[] enemies;
    private float[] originalEnemySpeeds;

    void Start()
    {
        if (weatherRoutine == null)
        {
            weatherRoutine = StartCoroutine(WeatherCycle());
        }
    }

    /// <summary>
    /// Cycles through weather effects at random intervals.
    /// </summary>
    IEnumerator WeatherCycle()
    {
        while (true)
        {
            yield return new WaitForSeconds(weatherChangeInterval);
            StartRandomWeather();
            yield return new WaitForSeconds(weatherDuration);
            StopCurrentWeather();
        }
    }

    /// <summary>
    /// Starts a random weather effect.
    /// </summary>
    void StartRandomWeather()
    {
        StopCurrentWeather(); // Clear any existing weather

        int weatherType = Random.Range(0, 2); // 0: rain, 1: snow
        switch (weatherType)
        {
            case 0:
                StartRain();
                break;
            case 1:
                StartSnow();
                break;
        }
    }

    /// <summary>
    /// Starts rain weather effect.
    /// </summary>
    void StartRain()
    {
        if (rainPrefab != null)
        {
            currentWeatherEffect = Instantiate(rainPrefab, Vector3.zero, Quaternion.identity);
            ApplyWeatherEffect(rainSlowFactor);
            Debug.Log("Rain started! Enemies are slower.");
        }
    }

    /// <summary>
    /// Starts snow weather effect.
    /// </summary>
    void StartSnow()
    {
        if (snowPrefab != null)
        {
            currentWeatherEffect = Instantiate(snowPrefab, Vector3.zero, Quaternion.identity);
            ApplyWeatherEffect(snowSlowFactor);
            Debug.Log("Snow started! Enemies are slower.");
        }
    }

    /// <summary>
    /// Stops the current weather effect.
    /// </summary>
    void StopCurrentWeather()
    {
        if (currentWeatherEffect != null)
        {
            Destroy(currentWeatherEffect);
            currentWeatherEffect = null;
            RemoveWeatherEffect();
            Debug.Log("Weather cleared! Enemies return to normal speed.");
        }
    }

    /// <summary>
    /// Applies the weather effect to all enemies.
    /// </summary>
    void ApplyWeatherEffect(float slowFactor)
    {
        enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        originalEnemySpeeds = new float[enemies.Length];
        for (int i = 0; i < enemies.Length; i++)
        {
            originalEnemySpeeds[i] = enemies[i].GetMoveSpeed();
            enemies[i].SetMoveSpeed(originalEnemySpeeds[i] * slowFactor);
        }
    }

    /// <summary>
    /// Removes the weather effect from all enemies.
    /// </summary>
    void RemoveWeatherEffect()
    {
        if (enemies != null && originalEnemySpeeds != null)
        {
            for (int i = 0; i < enemies.Length; i++)
            {
                if (enemies[i] != null)
                {
                    enemies[i].SetMoveSpeed(originalEnemySpeeds[i]);
                }
            }
        }
    }

    void OnDestroy()
    {
        if (weatherRoutine != null)
        {
            StopCoroutine(weatherRoutine);
        }
        StopCurrentWeather();
    }
}
