using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class WeatherController : MonoBehaviour
{
    [Header("References")]
    public WeatherSystem weatherSystem;
    public ParticleSystem rainParticles;
    public Image screenFlashImage;
    public WeatherWarningUI warningUI;
    public VoxelTerrainGenerator terrainGenerator;

    [Header("Procedural Weather Settings")]
    [Range(0f, 1f)] public float baseRainChance = 0.45f;
    public Vector2 rainIntensityRange = new Vector2(0.4f, 0.9f);
    public int minClearGapWaves = 1;

    [Header("Rain Effects")]
    public float rainSlowFactor = 0.8f;

    [Header("Screen Flash Settings")]
    public Color flashColor = new Color(1f, 1f, 1f, 0.45f);
    public float flashFadeSpeed = 2.5f;

    [Header("Procedural Hazard System")]
    [Tooltip("Enable meteor strikes")]
    public bool enableMeteors = true;
    [Tooltip("Enable earthquakes")]
    public bool enableEarthquakes = true;
    [Tooltip("Number of meteors per strike")]
    [Range(1, 5)] public int meteorCount = 3;
    [Tooltip("Perlin noise seed offset for procedural generation")]
    public float noiseOffset = 137.42f;
    [Tooltip("Noise scale for hazard timing")]
    public float noiseScale = 0.15f;

    [Header("Meteor Settings")]
    public float meteorWarningDuration = 3f;
    public float meteorSpeed = 15f;
    public float meteorImpactRadius = 3f;
    public Color meteorWarningColor = new Color(1f, 0.3f, 0f, 0.6f);

    [Header("Earthquake Settings")]
    public float earthquakeWarningDuration = 2f;
    public float earthquakeDuration = 4f;
    public Vector2 earthquakeMagnitudeRange = new Vector2(1.0f, 5.0f);
    public float earthquakeBaseDamagePercent = 0.15f;
    public float earthquakeCameraShakeMultiplier = 0.4f;
    [Tooltip("Optional: Custom prefab for earthquake dust particles")]
    public GameObject earthquakeDustPrefab;

    private int _lastRainWave = -1000;
    private bool _isRaining;
    private int _currentWave = 0;
    private List<GameObject> _activeHazardObjects = new List<GameObject>();
    private Coroutine _flashCoroutine;

    void Awake()
    {
        if (weatherSystem == null)
            weatherSystem = FindFirstObjectByType<WeatherSystem>();
        
        if (terrainGenerator == null)
            terrainGenerator = FindFirstObjectByType<VoxelTerrainGenerator>();
    }

    public void OnPreWave(int upcomingWave)
    {
        _currentWave = upcomingWave;
        string hazardType = SelectProceduralHazardType(upcomingWave);
        
        if (warningUI != null)
        {
            string warningMessage = GetWarningMessage(hazardType);
            warningUI.Show(warningMessage);
        }
    }

    public void OnWaveStart(int wave)
    {
        _currentWave = wave;
        string hazardType = SelectProceduralHazardType(wave);
        
        switch (hazardType)
        {
            case "RAIN":
                float intensity = SamplePerlinNoise(wave, 0f) * (rainIntensityRange.y - rainIntensityRange.x) + rainIntensityRange.x;
                StartRain(intensity);
                _lastRainWave = wave;
                break;
                
            case "METEOR":
                if (enableMeteors)
                {
                    StartCoroutine(TriggerMeteorStrike(wave));
                }
                break;
                
            case "EARTHQUAKE":
                if (enableEarthquakes)
                {
                    StartCoroutine(TriggerEarthquake(wave));
                }
                break;
                
            default:
                StopRain();
                break;
        }
    }

    public void OnWaveEnd(int wave)
    {
        StopRain();
        CleanupHazardObjects();
    }

    string SelectProceduralHazardType(int wave)
    {
        if (wave - _lastRainWave <= minClearGapWaves)
        {
            return "CLEAR";
        }
        
        float noise = SamplePerlinNoise(wave, 1f);
        float secondNoise = SamplePerlinNoise(wave, 2.5f);
        float combinedNoise = (noise + secondNoise * 0.5f) / 1.5f;
        
        float rainThreshold = baseRainChance;
        float meteorThreshold = rainThreshold + (enableMeteors ? 0.25f : 0f);
        float earthquakeThreshold = meteorThreshold + (enableEarthquakes ? 0.25f : 0f);
        
        if (combinedNoise < rainThreshold)
            return "RAIN";
        else if (combinedNoise < meteorThreshold)
            return "METEOR";
        else if (combinedNoise < earthquakeThreshold)
            return "EARTHQUAKE";
        else
            return "CLEAR";
    }
    
    float SamplePerlinNoise(int wave, float offsetMultiplier)
    {
        float x = wave * noiseScale + noiseOffset * offsetMultiplier;
        float y = Time.time * 0.01f + noiseOffset * offsetMultiplier;
        return Mathf.PerlinNoise(x, y);
    }
    
    string GetWarningMessage(string hazardType)
    {
        switch (hazardType)
        {
            case "RAIN": return "⚠️ HEAVY RAIN INCOMING";
            case "METEOR": return "☄️ METEOR STRIKE DETECTED";
            case "EARTHQUAKE": return "🌍 EARTHQUAKE WARNING";
            default: return "CLEAR SKIES AHEAD";
        }
    }

    void StartRain(float intensity)
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
            rainParticles.Play();
        }

        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (Enemy enemy in enemies)
        {
            enemy.SetMoveSpeed(enemy.GetMoveSpeed() * rainSlowFactor);
        }

        TriggerFlash();
    }

    void StopRain()
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
            rainParticles.Stop();
        }

        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (Enemy enemy in enemies)
        {
            enemy.SetMoveSpeed(enemy.GetMoveSpeed() / rainSlowFactor);
        }

        TriggerFlash();
    }

    void TriggerFlash()
    {
        if (screenFlashImage == null) return;
        if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
        screenFlashImage.color = flashColor;
        screenFlashImage.gameObject.SetActive(true);
        _flashCoroutine = StartCoroutine(FadeFlash());
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
    
    IEnumerator TriggerMeteorStrike(int wave)
    {
        Debug.Log($"☄️ METEOR STRIKE: Wave {wave} - Targeting {meteorCount} defenders");
        
        List<Vector3> targetPositions = SelectMeteorTargets(wave);
        
        List<GameObject> warnings = ShowMeteorWarnings(targetPositions);
        
        yield return new WaitForSeconds(meteorWarningDuration);
        
        foreach (GameObject warning in warnings)
        {
            if (warning != null) Destroy(warning);
        }
        
        SpawnMeteors(targetPositions, wave);
    }
    
    List<Vector3> SelectMeteorTargets(int wave)
    {
        List<Vector3> targets = new List<Vector3>();
        
        Defender[] allDefenders = FindObjectsByType<Defender>(FindObjectsSortMode.None);
        
        if (allDefenders.Length > 0)
        {
            List<Defender> weightedDefenders = new List<Defender>();
            
            foreach (Defender defender in allDefenders)
            {
                if (defender != null && defender.IsAlive())
                {
                    int weight = 1;
                    
                    if (defender is FrostTowerDefender || defender is LightningTowerDefender)
                    {
                        weight = 3;
                    }
                    
                    for (int i = 0; i < weight; i++)
                    {
                        weightedDefenders.Add(defender);
                    }
                }
            }
            
            int targetsToSelect = Mathf.Min(meteorCount, allDefenders.Length);
            HashSet<Defender> selectedDefenders = new HashSet<Defender>();
            
            for (int i = 0; i < targetsToSelect; i++)
            {
                if (weightedDefenders.Count == 0) break;
                
                int randomIndex = Mathf.FloorToInt(SamplePerlinNoise(wave, i * 0.7f) * weightedDefenders.Count);
                randomIndex = Mathf.Clamp(randomIndex, 0, weightedDefenders.Count - 1);
                
                Defender selected = weightedDefenders[randomIndex];
                
                if (!selectedDefenders.Contains(selected))
                {
                    selectedDefenders.Add(selected);
                    targets.Add(selected.GetVisualCenter());
                }
                
                weightedDefenders.RemoveAll(d => d == selected);
            }
        }
        else if (terrainGenerator != null)
        {
            for (int i = 0; i < meteorCount; i++)
            {
                float noise1 = SamplePerlinNoise(wave, i * 1.3f);
                float noise2 = SamplePerlinNoise(wave, i * 2.1f + 10f);
                
                int x = Mathf.FloorToInt(noise1 * terrainGenerator.width);
                int z = Mathf.FloorToInt(noise2 * terrainGenerator.depth);
                
                Vector3 pos = terrainGenerator.GetSurfaceWorldPosition(new Vector3Int(x, 0, z));
                targets.Add(pos);
            }
        }
        
        return targets;
    }
    
    List<GameObject> ShowMeteorWarnings(List<Vector3> positions)
    {
        List<GameObject> warnings = new List<GameObject>();
        
        foreach (Vector3 pos in positions)
        {
            GameObject warning = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            warning.transform.position = pos + Vector3.up * 0.1f;
            warning.transform.localScale = new Vector3(meteorImpactRadius * 2f, 0.1f, meteorImpactRadius * 2f);
            
            Renderer rend = warning.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = meteorWarningColor;
            rend.material = mat;
            
            Destroy(warning.GetComponent<Collider>());
            
            warning.AddComponent<MeteorWarningPulse>();
            
            warnings.Add(warning);
            _activeHazardObjects.Add(warning);
        }
        
        return warnings;
    }
    
    void SpawnMeteors(List<Vector3> targetPositions, int wave)
    {
        foreach (Vector3 target in targetPositions)
        {
            float heightNoise = SamplePerlinNoise(wave, target.x * 0.1f);
            float spawnHeight = 40f + heightNoise * 20f;
            
            Vector3 spawnPos = target + Vector3.up * spawnHeight;
            
            GameObject meteor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            meteor.transform.position = spawnPos;
            meteor.transform.localScale = Vector3.one * 2f;
            
            Renderer rend = meteor.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(1f, 0.5f, 0.1f);
            mat.SetFloat("_Smoothness", 0.8f);
            rend.material = mat;
            
            TrailRenderer trail = meteor.AddComponent<TrailRenderer>();
            trail.time = 0.5f;
            trail.startWidth = 1.5f;
            trail.endWidth = 0.3f;
            trail.material = mat;
            
            MeteorProjectile projectile = meteor.AddComponent<MeteorProjectile>();
            projectile.Initialize(target, meteorSpeed, meteorImpactRadius, this);
            
            _activeHazardObjects.Add(meteor);
        }
    }
    
    public void OnMeteorImpact(Vector3 position, float radius)
    {
        SpawnExplosionEffect(position);
        
        CameraShake(0.5f, 0.4f);
        
        Collider[] hits = Physics.OverlapSphere(position, radius);
        
        foreach (Collider hit in hits)
        {
            Defender defender = hit.GetComponentInParent<Defender>();
            if (defender != null && defender.IsAlive())
            {
                float distance = Vector3.Distance(position, defender.transform.position);
                
                if (distance < 1.5f)
                {
                    Debug.Log($"☄️ Meteor DESTROYED defender: {defender.name}");
                    defender.TakeDamage(99999f);
                }
                else if (distance < radius)
                {
                    float damagePercent = 1f - (distance / radius);
                    float damage = 50f * damagePercent;
                    Debug.Log($"☄️ Meteor damaged defender: {defender.name} for {damage:F1} HP");
                    defender.TakeDamage(damage);
                }
            }
        }
    }
    
    void SpawnExplosionEffect(Vector3 position)
    {
        GameObject explosion = new GameObject("MeteorExplosion");
        explosion.transform.position = position;
        
        ParticleSystem ps = explosion.AddComponent<ParticleSystem>();
        
        var main = ps.main;
        main.duration = 1f;
        main.startLifetime = 0.8f;
        main.startSpeed = 8f;
        main.startSize = 1.5f;
        main.startColor = new Color(1f, 0.6f, 0.1f, 1f);
        main.gravityModifier = 0.2f;
        
        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 50) });
        
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.5f;
        
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        
        ps.Play();
        
        Destroy(explosion, 2f);
        _activeHazardObjects.Add(explosion);
    }
    
    IEnumerator TriggerEarthquake(int wave)
    {
        float magnitude = SamplePerlinNoise(wave, 3.7f) * (earthquakeMagnitudeRange.y - earthquakeMagnitudeRange.x) + earthquakeMagnitudeRange.x;
        
        Debug.Log($"🌍 EARTHQUAKE: Wave {wave} - Magnitude {magnitude:F1}");
        
        yield return new WaitForSeconds(earthquakeWarningDuration);
        
        float earthquakeStartTime = Time.time;
        
        StartCoroutine(EarthquakeCameraShake(magnitude));
        
        SpawnEarthquakeDust(wave, magnitude);
        
        DamageUnitsFromEarthquake(magnitude);
        
        while (Time.time - earthquakeStartTime < earthquakeDuration)
        {
            yield return null;
        }
        
        Debug.Log("🌍 Earthquake ended");
    }
    
    IEnumerator EarthquakeCameraShake(float magnitude)
    {
        Camera mainCam = Camera.main;
        if (mainCam == null) yield break;
        
        Vector3 originalPos = mainCam.transform.localPosition;
        float intensity = magnitude * earthquakeCameraShakeMultiplier * 0.1f;
        float elapsed = 0f;
        
        while (elapsed < earthquakeDuration)
        {
            float shakeAmount = intensity * (1f - elapsed / earthquakeDuration);
            
            float noiseX = SamplePerlinNoise(_currentWave, elapsed * 10f);
            float noiseY = SamplePerlinNoise(_currentWave, elapsed * 10f + 50f);
            
            float x = (noiseX * 2f - 1f) * shakeAmount;
            float y = (noiseY * 2f - 1f) * shakeAmount;
            
            mainCam.transform.localPosition = originalPos + new Vector3(x, y, 0f);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        mainCam.transform.localPosition = originalPos;
    }
    
    void SpawnEarthquakeDust(int wave, float magnitude)
    {
        if (terrainGenerator == null) return;
        
        int dustCount = Mathf.RoundToInt(magnitude * 8f);
        
        for (int i = 0; i < dustCount; i++)
        {
            float noise1 = SamplePerlinNoise(wave, i * 1.7f + 5f);
            float noise2 = SamplePerlinNoise(wave, i * 2.3f + 15f);
            
            int x = Mathf.FloorToInt(noise1 * terrainGenerator.width);
            int z = Mathf.FloorToInt(noise2 * terrainGenerator.depth);
            
            Vector3 pos = terrainGenerator.GetSurfaceWorldPosition(new Vector3Int(x, 0, z));
            
            GameObject dust;
            
            if (earthquakeDustPrefab != null)
            {
                dust = Instantiate(earthquakeDustPrefab, pos, Quaternion.identity);
            }
            else
            {
                dust = new GameObject("EarthquakeDust");
                dust.transform.position = pos;
                
                ParticleSystem ps = dust.AddComponent<ParticleSystem>();
                
                var main = ps.main;
                main.duration = 2f;
                main.startLifetime = 1.5f;
                main.startSpeed = 2f;
                main.startSize = 0.8f;
                main.startColor = new Color(0.6f, 0.5f, 0.4f, 0.7f);
                main.gravityModifier = -0.1f;
                
                var emission = ps.emission;
                emission.rateOverTime = 20f;
                
                var shape = ps.shape;
                shape.shapeType = ParticleSystemShapeType.Sphere;
                shape.radius = 1f;
                
                ps.Play();
            }
            
            Destroy(dust, 3f);
            _activeHazardObjects.Add(dust);
        }
    }
    
    void DamageUnitsFromEarthquake(float magnitude)
    {
        float damagePercent = earthquakeBaseDamagePercent + (magnitude / earthquakeMagnitudeRange.y) * 0.15f;
        damagePercent = Mathf.Clamp(damagePercent, 0.1f, 0.35f);
        
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        int enemiesDamaged = 0;
        float totalEnemyDamage = 0f;
        
        foreach (Enemy enemy in enemies)
        {
            if (enemy != null)
            {
                float damage = enemy.GetMaxHealth() * damagePercent;
                enemy.TakeDamage(damage);
                enemiesDamaged++;
                totalEnemyDamage += damage;
            }
        }
        
        Defender[] defenders = FindObjectsByType<Defender>(FindObjectsSortMode.None);
        int defendersDamaged = 0;
        float totalDefenderDamage = 0f;
        
        foreach (Defender defender in defenders)
        {
            if (defender != null && defender.IsAlive())
            {
                float noiseValue = SamplePerlinNoise(_currentWave, defender.transform.position.x * 0.1f);
                
                if (noiseValue < damagePercent * 1.5f)
                {
                    float damage = 30f + magnitude * 10f;
                    defender.TakeDamage(damage);
                    defendersDamaged++;
                    totalDefenderDamage += damage;
                }
            }
        }
        
        if (enemiesDamaged > 0)
        {
            Debug.Log($"🌍 Earthquake damaged {enemiesDamaged} enemies ({damagePercent * 100f:F1}% of health, Total: {totalEnemyDamage:F0} damage)");
        }
        
        if (defendersDamaged > 0)
        {
            Debug.Log($"🌍 Earthquake damaged {defendersDamaged} defenders (Total: {totalDefenderDamage:F0} damage)");
        }
    }
    
    void CameraShake(float duration, float intensity)
    {
        StartCoroutine(CameraShakeCoroutine(duration, intensity));
    }
    
    IEnumerator CameraShakeCoroutine(float duration, float intensity)
    {
        Camera mainCam = Camera.main;
        if (mainCam == null) yield break;
        
        Vector3 originalPos = mainCam.transform.localPosition;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * intensity;
            float y = Random.Range(-1f, 1f) * intensity;
            
            mainCam.transform.localPosition = originalPos + new Vector3(x, y, 0f);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        mainCam.transform.localPosition = originalPos;
    }
    
    void CleanupHazardObjects()
    {
        foreach (GameObject obj in _activeHazardObjects)
        {
            if (obj != null) Destroy(obj);
        }
        _activeHazardObjects.Clear();
    }
}

public class MeteorWarningPulse : MonoBehaviour
{
    public float pulseSpeed = 3f;
    private Vector3 originalScale;
    
    void Start()
    {
        originalScale = transform.localScale;
    }
    
    void Update()
    {
        float scale = 1f + Mathf.Sin(Time.time * pulseSpeed) * 0.25f;
        transform.localScale = originalScale * scale;
    }
}

public class MeteorProjectile : MonoBehaviour
{
    private Vector3 targetPosition;
    private float speed;
    private float impactRadius;
    private WeatherController weatherController;
    private bool hasImpacted = false;
    
    public void Initialize(Vector3 target, float meteorSpeed, float radius, WeatherController controller)
    {
        targetPosition = target;
        speed = meteorSpeed;
        impactRadius = radius;
        weatherController = controller;
    }
    
    void Update()
    {
        if (hasImpacted) return;
        
        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
        
        transform.rotation = Quaternion.LookRotation(direction);
        
        if (Vector3.Distance(transform.position, targetPosition) < 1f)
        {
            Impact();
        }
    }
    
    void Impact()
    {
        if (hasImpacted) return;
        hasImpacted = true;
        
        if (weatherController != null)
        {
            weatherController.OnMeteorImpact(transform.position, impactRadius);
        }
        
        Destroy(gameObject);
    }
}



