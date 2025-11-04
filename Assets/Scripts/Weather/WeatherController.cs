using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WeatherController : MonoBehaviour
{
	[Header("References")]
	public WeatherSystem weatherSystem;
	[Tooltip("Assign a ParticleSystem for rain visual. Will be toggled on rain start/stop.")]
	public ParticleSystem rainParticles;
	[Tooltip("Optional full-screen Image used to flash the screen when weather changes.")]
	public Image screenFlashImage;
	[Tooltip("Optional: UI warning to show weather messages.")]
	public WeatherWarningUI warningUI;

	[Header("Procedural Weather Settings")]
	[Range(0f,1f)] public float baseRainChance = 0.45f;
	public Vector2 rainIntensityRange = new Vector2(0.4f, 0.9f);
	[Tooltip("Minimum waves gap between forced rain off states to avoid spam.")]
	public int minClearGapWaves = 1;

	[Header("Screen Flash Settings")]
	public Color flashColor = new Color(1f, 1f, 1f, 0.45f);
	public float flashFadeSpeed = 2.5f;

	int _lastRainWave = -1000;
	bool _isRaining;

	void Awake()
	{
		if (weatherSystem == null)
			weatherSystem = FindFirstObjectByType<WeatherSystem>();
	}

	public void OnPreWave(int upcomingWave)
	{
		bool shouldRain = DecideRainForWave(upcomingWave);
		if (shouldRain)
		{
			if (warningUI != null)
				warningUI.Show("RAIN INCOMING");
		}
	}

	public void OnWaveStart(int wave)
	{
		bool shouldRain = DecideRainForWave(wave);
		if (shouldRain)
		{
			float intensity = Random.Range(rainIntensityRange.x, rainIntensityRange.y);
			StartRain(intensity);
			_lastRainWave = wave;
		}
		else
		{
			StopRain();
		}
	}

	public void OnWaveEnd(int wave)
	{
		// Stop rain at wave end; you can change this to have persistent storms
		StopRain();
	}

	bool DecideRainForWave(int wave)
	{
		float chance = baseRainChance;
		// Simple pacing: avoid back-to-back rain if too frequent
		if (wave - _lastRainWave <= minClearGapWaves)
		{
			chance *= 0.5f;
		}
		return Random.value < chance;
	}

	public void StartRain(float intensity)
	{
		_isRaining = true;
		if (weatherSystem != null)
		{
			weatherSystem.SetWeather(WeatherSystem.WeatherType.Rain, intensity);
		}
		if (rainParticles != null)
		{
			var emission = rainParticles.emission;
			emission.enabled = true;
			if (!rainParticles.isPlaying) rainParticles.Play();
		}
		TriggerFlash();
	}

	public void StopRain()
	{
		if (!_isRaining) return;
		_isRaining = false;
		if (weatherSystem != null)
		{
			weatherSystem.SetWeather(WeatherSystem.WeatherType.Clear, 0f);
		}
		if (rainParticles != null)
		{
			var emission = rainParticles.emission;
			emission.enabled = false;
			rainParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
		}
		TriggerFlash();
	}

	void TriggerFlash()
	{
		if (screenFlashImage == null) return;
		StopAllCoroutines();
		screenFlashImage.color = flashColor;
		screenFlashImage.gameObject.SetActive(true);
		StartCoroutine(FadeFlash());
	}

	IEnumerator FadeFlash()
	{
		Color c = screenFlashImage.color;
		while (c.a > 0.01f)
		{
			c.a = Mathf.MoveTowards(c.a, 0f, Time.deltaTime * flashFadeSpeed);
			screenFlashImage.color = c;
			yield return null;
		}
		screenFlashImage.gameObject.SetActive(false);
	}
}

