using System.Collections.Generic;
using UnityEngine;

public class WeatherSystem : MonoBehaviour
{
	public enum WeatherType { Clear, Rain }

	[Header("References")]
	public VoxelTerrainGenerator terrainGenerator;

	[Header("State")]
	public WeatherType current = WeatherType.Clear;
	[Range(0f, 1f)] public float rainIntensity = 0.5f;

	[Header("Water Shader Params")] 
	public string amplitudeProperty = "_Amplitude";
	public string frequencyProperty = "_Frequency";
	public string speedProperty = "_Speed";
	public float baseAmplitude = 0.05f;
	public float baseFrequency = 1.5f;
	public float baseSpeed = 1.0f;

	readonly List<Renderer> _overlayRenderers = new List<Renderer>();

	void Start()
	{
		if (terrainGenerator == null)
			terrainGenerator = FindFirstObjectByType<VoxelTerrainGenerator>();
		CacheOverlayRenderers();
		ApplyWeather();
	}

	void CacheOverlayRenderers()
	{
		_overlayRenderers.Clear();
		var overlays = terrainGenerator != null ? terrainGenerator.GetWaterOverlays() : null;
		if (overlays == null) return;
		foreach (var go in overlays)
		{
			if (go == null) continue;
			var r = go.GetComponentInChildren<Renderer>();
			if (r != null) _overlayRenderers.Add(r);
		}
	}

	public void SetWeather(WeatherType type, float intensity = 0.5f)
	{
		current = type;
		rainIntensity = Mathf.Clamp01(intensity);
		ApplyWeather();
	}

	void ApplyWeather()
	{
		bool raining = current == WeatherType.Rain && rainIntensity > 0.01f;

		// Toggle overlay visuals if present
		foreach (var r in _overlayRenderers)
		{
			if (r == null) continue;
			r.enabled = raining;
			if (raining)
			{
				var mpb = new MaterialPropertyBlock();
				r.GetPropertyBlock(mpb);
				mpb.SetFloat(amplitudeProperty, baseAmplitude * rainIntensity);
				mpb.SetFloat(frequencyProperty, baseFrequency);
				mpb.SetFloat(speedProperty, baseSpeed);
				r.SetPropertyBlock(mpb);
			}
		}

		// Spawn/clear real water fill voxels
		if (terrainGenerator != null)
		{
			if (raining) terrainGenerator.GenerateWaterFill();
			else terrainGenerator.ClearWaterFill();
		}
	}
}


