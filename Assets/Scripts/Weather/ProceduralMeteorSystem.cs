using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

// Handles all meteor strike logic, including warnings, spawning, and effects
public class ProceduralMeteorSystem : MonoBehaviour
{
   
    [Header("References")]
    public WeatherWarningUI warningUI;
    public VoxelTerrainGenerator terrainGenerator;
    public Image screenFlashImage;

    [Header("Meteor Spawn Settings")]
    [Tooltip("Minimum meteors per strike")]
    [Range(1, 5)] public int minMeteorCount = 2;
    [Tooltip("Maximum meteors per strike")]
    [Range(3, 12)] public int maxMeteorCount = 8;
    [Tooltip("Perlin noise seed offset")]
    public float noiseOffset = 137.42f;
    [Tooltip("Noise scale for procedural generation")]
    public float noiseScale = 0.15f;

    [Header("Warning Settings")]
    [Tooltip("Base warning duration for early waves")]
    public float baseWarningDuration = 3f;
    [Tooltip("Minimum warning duration for later waves")]
    public float minWarningDuration = 1.5f;
    [Tooltip("Color of warning cylinders")]
    public Color meteorWarningColor = new Color(1f, 0.3f, 0f, 0.6f);

    [Header("Meteor Properties")]
    [Tooltip("Speed of meteor travel")]
    public float meteorSpeed = 15f;
    [Tooltip("Base impact radius")]
    public float baseImpactRadius = 3f;
    [Tooltip("Minimum meteor size")]
    [Range(0.3f, 1f)] public float minMeteorSize = 0.5f;
    [Tooltip("Maximum meteor size")]
    [Range(1f, 4f)] public float maxMeteorSize = 3f;

    [Header("Damage Settings")]
    [Tooltip("Base damage for medium-sized meteors")]
    public float baseDamage = 40f;
    [Tooltip("Damage multiplier for special defenders")]
    public float specialDefenderDamageMultiplier = 0.6f;

    [Header("Healing Meteor Settings")]
    [Tooltip("Enable healing meteors")]
    public bool enableHealingMeteors = true;
    [Tooltip("Size range for healing meteors (min)")]
    [Range(0.3f, 1.5f)] public float healingMeteorMinSize = 0.3f;
    [Tooltip("Size range for healing meteors (max)")]
    [Range(0.5f, 2f)] public float healingMeteorMaxSize = 0.8f;
    [Tooltip("Healing amount")]
    public float healingAmount = 25f;
    [Tooltip("Healing meteor color")]
    public Color healingMeteorColor = new Color(0.3f, 1f, 0.5f);

    [Header("Procedural Shape Settings")]
    public Shader proceduralMeteorShader;
    [Tooltip("Displacement amount correlates with danger")]
    [Range(0f, 1f)] public float maxDisplacement = 0.5f;
    [Tooltip("Frequency of displacement noise")]
    [Range(1f, 20f)] public float displacementFrequency = 8f;

    [Header("Screen Flash")]
    public Color flashColor = new Color(1f, 1f, 1f, 0.45f);
    public float flashFadeSpeed = 2.5f;

    // --- Private state ---
    private int _currentWave = 0;
    private List<GameObject> _activeHazardObjects = new List<GameObject>();
    private Coroutine _flashCoroutine;

    // Make sure we have references to terrain and shader
    void Awake()
    {
        if (terrainGenerator == null)
            terrainGenerator = FindFirstObjectByType<VoxelTerrainGenerator>();

        if (proceduralMeteorShader == null)
            proceduralMeteorShader = Shader.Find("Custom/ProceduralMeteorShader");
    }

    // Called before a wave starts, just to show the warning
    public void OnPreWave(int upcomingWave)
    {
        _currentWave = upcomingWave;
        if (warningUI != null)
        {
            warningUI.Show("METEOR STRIKE DETECTED");
        }
    }

    // Called when the wave actually starts, triggers the meteor strike
    public void OnWaveStart(int wave)
    {
        _currentWave = wave;
        StartCoroutine(TriggerMeteorStrike(wave));
    }

    // Clean up any leftover objects at the end of the wave
    public void OnWaveEnd(int wave)
    {
        CleanupHazardObjects();
    }

    // Main coroutine for meteor strike: warnings, delay, then spawn
    IEnumerator TriggerMeteorStrike(int wave)
    {
        int meteorCount = CalculateMeteorCount(wave);
        float warningDuration = CalculateWarningDuration(wave);

        Debug.Log($"METEOR STRIKE: Wave {wave} - Spawning {meteorCount} meteors (Warning: {warningDuration:F1}s)");

        // Figure out where meteors will land
        List<MeteorData> meteorDataList = GenerateMeteorData(wave, meteorCount);
        // Show warning markers
        List<GameObject> warnings = ShowMeteorWarnings(meteorDataList);

        // Wait for warning duration
        yield return new WaitForSeconds(warningDuration);

        // Remove warning markers
        foreach (GameObject warning in warnings)
        {
            if (warning != null) Destroy(warning);
        }

        // Actually spawn the meteors
        SpawnMeteors(meteorDataList, wave);
    }

    // Decide how many meteors to spawn, with some randomness and scaling
    int CalculateMeteorCount(int wave)
    {
        float noiseValue = SamplePerlinNoise(wave, 4.2f);
        int count = minMeteorCount + Mathf.FloorToInt(noiseValue * (maxMeteorCount - minMeteorCount + 1));
        int waveBonus = Mathf.Min(wave / 10, 3);
        count = Mathf.Min(count + waveBonus, maxMeteorCount);
        return count;
    }

    // Warning time gets shorter as waves go up, with a bit of noise
    float CalculateWarningDuration(int wave)
    {
        float waveFactor = Mathf.Clamp01(wave / 20f);
        float duration = Mathf.Lerp(baseWarningDuration, minWarningDuration, waveFactor);
        float noiseVariation = SamplePerlinNoise(wave, 6.8f) * 0.5f - 0.25f;
        duration += noiseVariation;
        return Mathf.Max(duration, minWarningDuration);
    }

    // Pick meteor targets: defenders get priority, rest are random terrain spots
    List<MeteorData> GenerateMeteorData(int wave, int meteorCount)
    {
        List<MeteorData> meteorDataList = new List<MeteorData>();
        Defender[] allDefenders = FindObjectsByType<Defender>(FindObjectsSortMode.None);

        if (allDefenders.Length > 0)
        {
            // Weighted defender selection: special towers are more likely targets
            List<Defender> weightedDefenders = new List<Defender>();
            foreach (Defender defender in allDefenders)
            {
                if (defender != null && defender.IsAlive())
                {
                    int weight = 1;
                    if (defender is FrostTowerDefender || defender is LightningTowerDefender)
                        weight = 2;
                    for (int i = 0; i < weight; i++)
                        weightedDefenders.Add(defender);
                }
            }

            int targetsToSelect = Mathf.Min(meteorCount, allDefenders.Length);
            HashSet<Defender> selectedDefenders = new HashSet<Defender>();

            // Pick unique defenders to target
            for (int i = 0; i < targetsToSelect; i++)
            {
                if (weightedDefenders.Count == 0) break;
                int randomIndex = Mathf.FloorToInt(SamplePerlinNoise(wave, i * 0.7f) * weightedDefenders.Count);
                randomIndex = Mathf.Clamp(randomIndex, 0, weightedDefenders.Count - 1);
                Defender selected = weightedDefenders[randomIndex];
                if (!selectedDefenders.Contains(selected))
                {
                    selectedDefenders.Add(selected);
                    MeteorData data = CreateMeteorData(wave, i, selected.GetVisualCenter());
                    meteorDataList.Add(data);
                }
                weightedDefenders.RemoveAll(d => d == selected);
            }

            // Fill in any extra meteors with random terrain positions
            int remainingMeteors = meteorCount - meteorDataList.Count;
            for (int i = 0; i < remainingMeteors; i++)
            {
                Vector3 randomPos = GetRandomTerrainPosition(wave, meteorDataList.Count + i);
                MeteorData data = CreateMeteorData(wave, meteorDataList.Count + i, randomPos);
                meteorDataList.Add(data);
            }
        }
        else if (terrainGenerator != null)
        {
            // No defenders? Just hit random terrain spots
            for (int i = 0; i < meteorCount; i++)
            {
                Vector3 pos = GetRandomTerrainPosition(wave, i);
                MeteorData data = CreateMeteorData(wave, i, pos);
                meteorDataList.Add(data);
            }
        }
        return meteorDataList;
    }

    // Build up all the data for a single meteor: size, type, damage, etc.
    MeteorData CreateMeteorData(int wave, int index, Vector3 targetPosition)
    {
        MeteorData data = new MeteorData();
        data.targetPosition = targetPosition;

        // Size is based on noise and wave number
        float sizeNoise = SamplePerlinNoise(wave, index * 0.95f + 20f);
        float waveSizeBonus = 1f + (wave * 0.015f);
        data.size = Mathf.Lerp(minMeteorSize, maxMeteorSize, sizeNoise) * waveSizeBonus;
        data.size = Mathf.Clamp(data.size, minMeteorSize, maxMeteorSize);

        // Decide if this is a healing meteor
        if (enableHealingMeteors && data.size >= healingMeteorMinSize && data.size <= healingMeteorMaxSize)
        {
            float healingChance = SamplePerlinNoise(wave, index * 1.5f + 50f);
            data.isHealing = healingChance < 0.2f;
        }

        // Shape and impact radius scale with size
        float dangerLevel = (data.size - minMeteorSize) / (maxMeteorSize - minMeteorSize);
        data.displacement = Mathf.Lerp(0.05f, maxDisplacement, dangerLevel);
        data.frequency = Mathf.Lerp(displacementFrequency * 0.5f, displacementFrequency * 1.5f, dangerLevel);
        data.seed = SamplePerlinNoise(wave, index * 2.3f + 100f) * 1000f;
        data.impactRadius = baseImpactRadius * (data.size / maxMeteorSize);

        // Set damage or healing
        if (data.isHealing)
        {
            data.damage = 0f;
            data.healing = healingAmount;
        }
        else
        {
            data.damage = baseDamage * (data.size / maxMeteorSize);
            data.healing = 0f;
        }
        return data;
    }

    // Pick a random spot on the terrain for a meteor to hit
    Vector3 GetRandomTerrainPosition(int wave, int index)
    {
        if (terrainGenerator == null) return Vector3.zero;
        float noise1 = SamplePerlinNoise(wave, index * 1.3f);
        float noise2 = SamplePerlinNoise(wave, index * 2.1f + 10f);
        int x = Mathf.FloorToInt(noise1 * terrainGenerator.width);
        int z = Mathf.FloorToInt(noise2 * terrainGenerator.depth);
        return terrainGenerator.GetSurfaceWorldPosition(new Vector3Int(x, 0, z));
    }

    // Show warning cylinders at all meteor target locations
    List<GameObject> ShowMeteorWarnings(List<MeteorData> meteorDataList)
    {
        List<GameObject> warnings = new List<GameObject>();
        foreach (MeteorData data in meteorDataList)
        {
            GameObject warning = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            warning.transform.position = data.targetPosition + Vector3.up * 0.1f;
            warning.transform.localScale = new Vector3(data.impactRadius * 2f, 0.1f, data.impactRadius * 2f);

            Renderer rend = warning.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            Color warningColor = data.isHealing ? new Color(0.3f, 1f, 0.5f, 0.6f) : meteorWarningColor;
            mat.color = warningColor;
            rend.material = mat;

            Destroy(warning.GetComponent<Collider>());
            warning.AddComponent<MeteorWarningPulse>();

            warnings.Add(warning);
            _activeHazardObjects.Add(warning);
        }
        return warnings;
    }

    // Actually spawn the meteors and set up their visuals and logic
    void SpawnMeteors(List<MeteorData> meteorDataList, int wave)
    {
        foreach (MeteorData data in meteorDataList)
        {
            // Drop meteors from a random height above the target
            float heightNoise = SamplePerlinNoise(wave, data.targetPosition.x * 0.1f);
            float spawnHeight = 40f + heightNoise * 20f;
            Vector3 spawnPos = data.targetPosition + Vector3.up * spawnHeight;

            GameObject meteor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            meteor.transform.position = spawnPos;
            meteor.transform.localScale = Vector3.one * data.size;

            Renderer rend = meteor.GetComponent<Renderer>();
            Material mat;

            // Use procedural shader if available, otherwise fallback
            if (proceduralMeteorShader != null)
            {
                mat = new Material(proceduralMeteorShader);
                mat.SetFloat("_DisplacementAmount", data.displacement);
                mat.SetFloat("_DisplacementFrequency", data.frequency);
                mat.SetFloat("_DisplacementSeed", data.seed);
            }
            else
            {
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            }

            // Set color and emission based on meteor type
            if (data.isHealing)
            {
                mat.SetColor("_Color", healingMeteorColor);
                mat.SetColor("_EmissionColor", healingMeteorColor * 2f);
                mat.SetFloat("_EmissionIntensity", 3f);
                mat.EnableKeyword("_EMISSION");
            }
            else
            {
                float colorIntensity = Mathf.Lerp(0.8f, 1.3f, (data.size - minMeteorSize) / (maxMeteorSize - minMeteorSize));
                Color meteorColor = new Color(1f * colorIntensity, 0.5f * colorIntensity, 0.1f);
                mat.SetColor("_Color", meteorColor);
                mat.SetColor("_EmissionColor", new Color(1f, 0.4f, 0f) * (2f * colorIntensity));
                mat.SetFloat("_EmissionIntensity", 2.5f);
                mat.EnableKeyword("_EMISSION");
            }
            mat.SetFloat("_Smoothness", 0.2f);
            rend.material = mat;

            // Add a trail for visual flair
            TrailRenderer trail = meteor.AddComponent<TrailRenderer>();
            trail.time = 0.5f;
            trail.startWidth = 1.5f * data.size;
            trail.endWidth = 0.3f * data.size;
            Material trailMat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            Color trailColor = data.isHealing ? new Color(0.3f, 1f, 0.5f, 0.8f) : new Color(1f, 0.5f, 0.1f, 0.8f);
            trailMat.color = trailColor;
            trail.material = trailMat;

            // Attach the projectile logic
            ProceduralMeteorProjectile projectile = meteor.AddComponent<ProceduralMeteorProjectile>();
            projectile.Initialize(data, meteorSpeed, this);

            _activeHazardObjects.Add(meteor);
        }
    }

    // Called when a meteor hits: spawn effects and apply damage or healing
    public void OnMeteorImpact(MeteorData data, Vector3 position)
    {
        if (data.isHealing)
        {
            SpawnHealingEffect(position);
            ApplyHealing(position, data.impactRadius, data.healing);
        }
        else
        {
            SpawnExplosionEffect(position);
            ApplyDamage(position, data.impactRadius, data.damage, data.size);
        }
        CameraShake(0.5f, 0.3f * (data.size / maxMeteorSize));
    }

    // Damage all defenders in the impact radius, with falloff and special defender scaling
    void ApplyDamage(Vector3 position, float radius, float damage, float meteorSize)
    {
        Collider[] hits = Physics.OverlapSphere(position, radius);
        foreach (Collider hit in hits)
        {
            Defender defender = hit.GetComponentInParent<Defender>();
            if (defender != null && defender.IsAlive())
            {
                float distance = Vector3.Distance(position, defender.transform.position);
                float damagePercent = 1f - (distance / radius);
                float finalDamage = damage * damagePercent;
                bool isSpecialDefender = defender is FrostTowerDefender || defender is LightningTowerDefender;
                if (isSpecialDefender)
                {
                    finalDamage *= specialDefenderDamageMultiplier;
                }
                Debug.Log($"Meteor damaged {defender.name}: {finalDamage:F1} HP (Size: {meteorSize:F2}, Special: {isSpecialDefender})");
                defender.TakeDamage(finalDamage);
            }
        }
    }

    // Heal all defenders in the impact radius, with falloff
    void ApplyHealing(Vector3 position, float radius, float healing)
    {
        Collider[] hits = Physics.OverlapSphere(position, radius);
        foreach (Collider hit in hits)
        {
            Defender defender = hit.GetComponentInParent<Defender>();
            if (defender != null && defender.IsAlive())
            {
                float distance = Vector3.Distance(position, defender.transform.position);
                float healPercent = 1f - (distance / radius);
                float finalHealing = healing * healPercent;
                defender.TakeDamage(-finalHealing);
                Debug.Log($"Healing meteor restored {finalHealing:F1} HP to {defender.name}");
                SpawnHealingParticles(defender.transform.position);
            }
        }
    }

    // Spawn a big explosion effect at the impact point
    void SpawnExplosionEffect(Vector3 position)
    {
        GameObject explosion = new GameObject("MeteorExplosion");
        explosion.transform.position = position;
        ParticleSystem ps = explosion.AddComponent<ParticleSystem>();
        ParticleSystemRenderer psRenderer = explosion.GetComponent<ParticleSystemRenderer>();
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

        ps.Play();
        Destroy(explosion, 2f);
        _activeHazardObjects.Add(explosion);
    }

    // Spawn a healing effect at the impact point
    void SpawnHealingEffect(Vector3 position)
    {
        GameObject healEffect = new GameObject("HealingMeteorEffect");
        healEffect.transform.position = position;
        ParticleSystem ps = healEffect.AddComponent<ParticleSystem>();
        ParticleSystemRenderer psRenderer = healEffect.GetComponent<ParticleSystemRenderer>();
        Material particleMat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        particleMat.color = healingMeteorColor;
        psRenderer.material = particleMat;

        var main = ps.main;
        main.duration = 1f;
        main.startLifetime = 1.2f;
        main.startSpeed = 5f;
        main.startSize = 0.8f;
        main.startColor = healingMeteorColor;
        main.gravityModifier = -0.5f;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 30) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 1f;

        ps.Play();
        Destroy(healEffect, 2f);
        _activeHazardObjects.Add(healEffect);
    }

    // Spawn a small healing particle burst on each healed defender
    void SpawnHealingParticles(Vector3 position)
    {
        GameObject heal = new GameObject("HealParticles");
        heal.transform.position = position;
        ParticleSystem ps = heal.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 0.5f;
        main.startSpeed = 3f;
        main.startSize = 0.3f;
        main.startColor = healingMeteorColor;
        main.gravityModifier = -1f;

        var emission = ps.emission;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 10) });

        ps.Play();
        Destroy(heal, 1f);
    }

    // Quick camera shake for impact feedback
    void CameraShake(float duration, float intensity)
    {
        StartCoroutine(CameraShakeCoroutine(duration, intensity));
    }

    // Coroutine for camera shake effect
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

    // Utility: sample Perlin noise for procedural randomness
    float SamplePerlinNoise(int wave, float offsetMultiplier)
    {
        float x = wave * noiseScale + noiseOffset * offsetMultiplier;
        float y = noiseOffset * offsetMultiplier + 123.456f;
        return Mathf.PerlinNoise(x, y);
    }

    // Destroy all hazard objects (meteors, warnings, effects) at wave end
    void CleanupHazardObjects()
    {
        foreach (GameObject obj in _activeHazardObjects)
        {
            if (obj != null) Destroy(obj);
        }
        _activeHazardObjects.Clear();
    }
}

// Holds all the info for a single meteor
[System.Serializable]
public class MeteorData
{
    public Vector3 targetPosition;
    public float size;
    public float damage;
    public float healing;
    public float impactRadius;
    public float displacement;
    public float frequency;
    public float seed;
    public bool isHealing;
}

// Handles meteor movement and impact logic
public class ProceduralMeteorProjectile : MonoBehaviour
{
    private MeteorData meteorData;
    private float speed;
    private ProceduralMeteorSystem meteorSystem;
    private bool hasImpacted = false;

    // Set up the projectile with its data and system reference
    public void Initialize(MeteorData data, float meteorSpeed, ProceduralMeteorSystem system)
    {
        meteorData = data;
        speed = meteorSpeed;
        meteorSystem = system;
    }

    void Update()
    {
        if (hasImpacted) return;

        // Move toward the target
        Vector3 direction = (meteorData.targetPosition - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        // Spin for effect
        transform.rotation = Quaternion.LookRotation(direction);
        transform.Rotate(Vector3.up * Time.time * 100f, Space.Self);

        // Check for impact
        if (Vector3.Distance(transform.position, meteorData.targetPosition) < 1f)
        {
            Impact();
        }
    }

    // Handle impact: notify system and destroy self
    void Impact()
    {
        if (hasImpacted) return;
        hasImpacted = true;
        if (meteorSystem != null)
        {
            meteorSystem.OnMeteorImpact(meteorData, transform.position);
        }
        Destroy(gameObject);
    }
}
