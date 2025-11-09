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
    [Tooltip("Base probability of rain occurring on any given wave")]
    [Range(0f, 1f)] public float baseRainChance = 0.45f;
    [Tooltip("Minimum and maximum intensity values for rain when it occurs")]
    public Vector2 rainIntensityRange = new Vector2(0.4f, 0.9f);
    [Tooltip("Minimum number of clear waves required between rain events")]
    public int minClearGapWaves = 1;

    [Header("Rain Effects")]
    [Tooltip("Multiplier applied to enemy movement speed during rain")]
    public float rainSlowFactor = 0.8f;

    [Header("Screen Flash Settings")]
    public Color flashColor = new Color(1f, 1f, 1f, 0.45f);
    public float flashFadeSpeed = 2.5f;

    [Header("Procedural Meteor System")]
    public ProceduralMeteorSystem proceduralMeteorSystem;

    [Header("Legacy Meteor System Settings")]
    [Tooltip("Enable meteor strikes in legacy system")]
    public bool enableMeteors = true;
    [Tooltip("Enable earthquakes in legacy system")]
    public bool enableEarthquakes = false;
    [Tooltip("Minimum meteors per strike in legacy system")]
    [Range(1, 3)] public int minMeteorCount = 2;
    [Tooltip("Maximum meteors per strike in legacy system")]
    [Range(2, 8)] public int maxMeteorCount = 5;
    [Tooltip("Perlin noise seed offset for procedural generation")]
    public float noiseOffset = 137.42f;
    [Tooltip("Noise scale for hazard timing")]
    public float noiseScale = 0.15f;

    [Header("Meteor Settings")]
    [Tooltip("Base warning duration for early waves")]
    public float baseWarningDuration = 3f;
    [Tooltip("Minimum warning duration for later waves")]
    public float minWarningDuration = 1.5f;
    [Tooltip("Speed at which meteors travel toward their targets")]
    public float meteorSpeed = 15f;
    [Tooltip("Radius in world units for meteor damage and area of effect")]
    public float meteorImpactRadius = 3f;
    [Tooltip("Color of the warning cylinder displayed at target locations")]
    public Color meteorWarningColor = new Color(1f, 0.3f, 0f, 0.6f);
    [Tooltip("Minimum meteor size multiplier")]
    [Range(0.5f, 1f)] public float minMeteorSize = 0.7f;
    [Tooltip("Maximum meteor size multiplier")]
    [Range(1f, 3f)] public float maxMeteorSize = 2.5f;

    [Header("Earthquake Settings")]
    [Tooltip("Duration in seconds that warning displays before earthquake begins")]
    public float earthquakeWarningDuration = 2f;
    [Tooltip("Total duration in seconds that the earthquake lasts")]
    public float earthquakeDuration = 4f;
    [Tooltip("Minimum and maximum magnitude values for procedurally generated earthquakes")]
    public Vector2 earthquakeMagnitudeRange = new Vector2(1.0f, 5.0f);
    [Tooltip("Base percentage of max health dealt as damage to all enemies")]
    public float earthquakeBaseDamagePercent = 0.15f;
    [Tooltip("Multiplier for camera shake intensity based on earthquake magnitude")]
    public float earthquakeCameraShakeMultiplier = 0.4f;
    [Tooltip("Optional custom prefab for earthquake dust particles")]
    public GameObject earthquakeDustPrefab;

    private int _lastRainWave = -1000;
    private bool _isRaining;
    private int _currentWave = 0;
    private List<GameObject> _activeHazardObjects = new List<GameObject>();
    private Coroutine _flashCoroutine;
    private string _selectedHazardType = "CLEAR";

    void Awake()
    {
        if (weatherSystem == null)
            weatherSystem = FindFirstObjectByType<WeatherSystem>();
        
        if (terrainGenerator == null)
            terrainGenerator = FindFirstObjectByType<VoxelTerrainGenerator>();
        
        if (proceduralMeteorSystem == null)
            proceduralMeteorSystem = FindFirstObjectByType<ProceduralMeteorSystem>();
    }

    // Called before a wave starts to determine and display the upcoming weather hazard.
    // This gives players time to prepare for the incoming environmental challenge.
    // Caches the selected hazard to ensure consistency between warning and actual activation.
    public void OnPreWave(int upcomingWave)
    {
        _currentWave = upcomingWave;
        
        if (proceduralMeteorSystem != null)
        {
            proceduralMeteorSystem.OnPreWave(upcomingWave);
        }
        else if (warningUI != null)
        {
            warningUI.Show("METEOR STRIKE DETECTED");
        }
    }

    // Activates the selected weather hazard when the wave begins.
    // Each hazard type has unique mechanics that affect gameplay differently.
    // Uses the cached hazard type from OnPreWave to ensure the warning matches the actual event.
    public void OnWaveStart(int wave)
    {
        _currentWave = wave;
        
        if (proceduralMeteorSystem != null)
        {
            proceduralMeteorSystem.OnWaveStart(wave);
        }
        else
        {
            StartCoroutine(TriggerMeteorStrike(wave));
        }
    }

    public void OnWaveEnd(int wave)
    {
        if (proceduralMeteorSystem != null)
        {
            proceduralMeteorSystem.OnWaveEnd(wave);
        }
        
        CleanupHazardObjects();
    }

    // Uses Perlin noise to procedurally select which hazard will occur for the given wave.
    // This creates varied but consistent weather patterns that feel natural rather than purely random.
    // Advanced defenders have higher targeting priority for meteor strikes to increase challenge.
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
    
    // Samples 2D Perlin noise to generate procedural values for hazards.
    // The offset multiplier allows multiple independent noise channels from the same wave number.
    // Uses deterministic noise based on wave number to ensure consistent results.
    float SamplePerlinNoise(int wave, float offsetMultiplier)
    {
        float x = wave * noiseScale + noiseOffset * offsetMultiplier;
        float y = noiseOffset * offsetMultiplier + 123.456f;
        return Mathf.PerlinNoise(x, y);
    }
    
    string GetWarningMessage(string hazardType)
    {
        switch (hazardType)
        {
            case "RAIN": return "HEAVY RAIN INCOMING";
            case "METEOR": return "METEOR STRIKE DETECTED";
            case "EARTHQUAKE": return "EARTHQUAKE WARNING";
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
        // Procedurally determine meteor count based on wave number and noise.
        int meteorCount = CalculateMeteorCount(wave);
        
        // Procedurally determine warning duration that decreases with wave difficulty.
        float warningDuration = CalculateWarningDuration(wave);
        
        Debug.Log($"METEOR STRIKE: Wave {wave} - Targeting {meteorCount} defenders (Warning: {warningDuration:F1}s)");
        
        List<Vector3> targetPositions = SelectMeteorTargets(wave, meteorCount);
        
        List<GameObject> warnings = ShowMeteorWarnings(targetPositions);
        
        yield return new WaitForSeconds(warningDuration);
        
        foreach (GameObject warning in warnings)
        {
            if (warning != null) Destroy(warning);
        }
        
        SpawnMeteors(targetPositions, wave);
    }
    
    // Procedurally calculates the number of meteors for this wave.
    // Increases with wave difficulty and adds noise-based variation.
    int CalculateMeteorCount(int wave)
    {
        float noiseValue = SamplePerlinNoise(wave, 4.2f);
        int count = minMeteorCount + Mathf.FloorToInt(noiseValue * (maxMeteorCount - minMeteorCount + 1));
        
        // Scale up slightly with wave number for increasing difficulty
        int waveBonus = Mathf.Min(wave / 10, 2);
        count = Mathf.Min(count + waveBonus, maxMeteorCount);
        
        return count;
    }
    
    // Procedurally calculates warning duration that decreases as waves progress.
    // Later waves have shorter warnings for increased challenge.
    float CalculateWarningDuration(int wave)
    {
        float waveFactor = Mathf.Clamp01(wave / 20f);
        float duration = Mathf.Lerp(baseWarningDuration, minWarningDuration, waveFactor);
        
        // Add slight noise variation
        float noiseVariation = SamplePerlinNoise(wave, 6.8f) * 0.5f - 0.25f;
        duration += noiseVariation;
        
        return Mathf.Max(duration, minWarningDuration);
    }
    
    // Selects target positions for meteor strikes using weighted defender priority.
    // FrostTower and LightningTower defenders have 3x targeting weight to increase difficulty.
    // Falls back to random terrain positions if no defenders exist.
    List<Vector3> SelectMeteorTargets(int wave, int meteorCount)
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
            // Vary spawn height using procedural noise to create visual variety in meteor trajectories.
            float heightNoise = SamplePerlinNoise(wave, target.x * 0.1f);
            float spawnHeight = 40f + heightNoise * 20f;
            
            Vector3 spawnPos = target + Vector3.up * spawnHeight;
            
            // Procedurally vary meteor size based on wave difficulty and noise.
            float sizeNoise = SamplePerlinNoise(wave, target.z * 0.15f);
            float waveSizeBonus = 1f + (wave * 0.02f);
            float meteorSize = Mathf.Lerp(minMeteorSize, maxMeteorSize, sizeNoise) * waveSizeBonus;
            meteorSize = Mathf.Clamp(meteorSize, minMeteorSize, maxMeteorSize * 1.5f);
            
            GameObject meteor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            meteor.transform.position = spawnPos;
            meteor.transform.localScale = Vector3.one * meteorSize;
            
            // Color intensity varies with size for visual feedback.
            float colorIntensity = Mathf.Lerp(0.8f, 1.2f, (meteorSize - minMeteorSize) / (maxMeteorSize - minMeteorSize));
            
            Renderer rend = meteor.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(1f * colorIntensity, 0.5f * colorIntensity, 0.1f);
            mat.SetFloat("_Smoothness", 0.8f);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(1f, 0.4f, 0f) * (2f * colorIntensity));
            rend.material = mat;
            
            // Add a trail to show the meteor path through the sky.
            TrailRenderer trail = meteor.AddComponent<TrailRenderer>();
            trail.time = 0.5f;
            trail.startWidth = 1.5f * meteorSize;
            trail.endWidth = 0.3f * meteorSize;
            Material trailMat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            trailMat.color = new Color(1f, 0.5f, 0.1f, 0.8f);
            trail.material = trailMat;
            
            // Calculate impact damage based on meteor size.
            float baseDamage = 50f * (meteorSize / 2f);
            
            MeteorProjectile projectile = meteor.AddComponent<MeteorProjectile>();
            projectile.Initialize(target, meteorSpeed, meteorImpactRadius * (meteorSize / 2f), baseDamage, this);
            
            _activeHazardObjects.Add(meteor);
        }
    }
    
    public void OnMeteorImpact(Vector3 position, float radius, float baseDamage)
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
                    Debug.Log($"Meteor DESTROYED defender: {defender.name}");
                    defender.TakeDamage(99999f);
                }
                else if (distance < radius)
                {
                    float damagePercent = 1f - (distance / radius);
                    float damage = baseDamage * damagePercent;
                    Debug.Log($"Meteor damaged defender: {defender.name} for {damage:F1} HP");
                    defender.TakeDamage(damage);
                }
            }
        }
    }
    
    // Creates a procedural particle explosion effect at the meteor impact location.
    // The particles use a burst emission pattern with warm colors to simulate fire and debris.
    void SpawnExplosionEffect(Vector3 position)
    {
        GameObject explosion = new GameObject("MeteorExplosion");
        explosion.transform.position = position;
        
        ParticleSystem ps = explosion.AddComponent<ParticleSystem>();
        ParticleSystemRenderer psRenderer = explosion.GetComponent<ParticleSystemRenderer>();
        
        // Use URP compatible shader for particles to avoid purple/pink material issues.
        Material particleMat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        particleMat.color = new Color(1f, 0.6f, 0.1f, 1f);
        psRenderer.material = particleMat;
        
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
        
        Debug.Log($"EARTHQUAKE: Wave {wave} - Magnitude {magnitude:F1}");
        
        yield return new WaitForSeconds(earthquakeWarningDuration);
        
        float earthquakeStartTime = Time.time;
        
        StartCoroutine(EarthquakeCameraShake(magnitude));
        
        SpawnEarthquakeDust(wave, magnitude);
        
        DamageUnitsFromEarthquake(magnitude);
        
        while (Time.time - earthquakeStartTime < earthquakeDuration)
        {
            yield return null;
        }
        
        Debug.Log("Earthquake ended");
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
            Debug.Log($"Earthquake damaged {enemiesDamaged} enemies ({damagePercent * 100f:F1}% of health, Total: {totalEnemyDamage:F0} damage)");
        }
        
        if (defendersDamaged > 0)
        {
            Debug.Log($"Earthquake damaged {defendersDamaged} defenders (Total: {totalDefenderDamage:F0} damage)");
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
    private float baseDamage;
    private WeatherController weatherController;
    private bool hasImpacted = false;
    
    public void Initialize(Vector3 target, float meteorSpeed, float radius, float damage, WeatherController controller)
    {
        targetPosition = target;
        speed = meteorSpeed;
        impactRadius = radius;
        baseDamage = damage;
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
            weatherController.OnMeteorImpact(transform.position, impactRadius, baseDamage);
        }
        
        Destroy(gameObject);
    }
}



