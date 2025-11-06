using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Lightning Tower defender with chain lightning attacks.
/// Fast attack speed with chain damage to multiple enemies.
/// </summary>
public class LightningTowerDefender : Defender
{
    [Header("Lightning Tower Settings")]
    [Tooltip("Maximum number of enemies the lightning can chain to")]
    public int maxChainTargets = 3;

    [Tooltip("Maximum distance for chain lightning to jump")]
    public float chainRange = 4f;

    [Tooltip("Damage reduction per chain (0.8 = 20% damage reduction per jump)")]
    [Range(0.1f, 1f)]
    public float chainDamageReduction = 0.8f;

    [Tooltip("Visual effect for lightning")]
    public GameObject lightningEffectPrefab;

    [Tooltip("Particle system for lightning visual")]
    public ParticleSystem lightningParticles;

    [Tooltip("Line renderer for lightning visual")]
    public LineRenderer lightningLine;
    
    [Header("Visual Feedback")]
    [Tooltip("Color of lightning effects")]
    public Color lightningColor = Color.yellow;
    
    [Tooltip("Lightning projectile visual")]
    public GameObject lightningProjectilePrefab;

    protected override void Start()
    {
        base.Start();
        // Lightning tower characteristics
        attackRange = 8f;           // Medium range
        attackDamage = 15f;         // High initial damage
        attackIntervalSeconds = 1.2f; // Fast attack speed
        
        // Visual setup
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.yellow;
        }
        else
        {
            // Add a renderer if none exists
            renderer = gameObject.AddComponent<MeshRenderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = Color.yellow;
            renderer.material = mat;
        }
    }

    void Update()
    {
        if (!IsAlive()) return;

        AcquireEnemyIfAny();
        TryLightningAttack();
        
        // Debug current state
        if (currentEnemyTarget != null)
        {
            // Debug.Log($"Lightning Tower: Aiming at {currentEnemyTarget.name} (Distance: {Vector3.Distance(transform.position, currentEnemyTarget.transform.position):F1})");
        }
    }

    void TryLightningAttack()
    {
        if (currentEnemyTarget == null) 
        {
            // Debug.Log("Lightning Tower: No target in range");
            return;
        }
        
        float time = Time.time;
        float timeSinceLastAttack = time - lastAttackTime;
        
        if (timeSinceLastAttack >= attackIntervalSeconds)
        {
            // Debug.Log($"Lightning Tower: ATTACKING! Target: {currentEnemyTarget.name}, Cooldown: {timeSinceLastAttack:F1}s");
            lastAttackTime = time;
            PerformChainLightning();
        }
        else
        {
            // Debug.Log($"Lightning Tower: On cooldown ({timeSinceLastAttack:F1}s / {attackIntervalSeconds}s)");
        }
    }

     void PerformChainLightning()
     {
         List<Enemy> chainedEnemies = new List<Enemy>();
         List<Vector3> lightningPoints = new List<Vector3>();
         
         lightningPoints.Add(GetVisualCenter());
         
         Enemy currentTarget = currentEnemyTarget;
         float currentDamage = attackDamage;
         
         for (int i = 0; i < maxChainTargets && currentTarget != null; i++)
         {
             currentTarget.TakeDamage(currentDamage);
             chainedEnemies.Add(currentTarget);
             lightningPoints.Add(currentTarget.transform.position);
             
             ApplyLightningVisualEffect(currentTarget);
             SpawnLightningImpactEffect(currentTarget.transform.position);
             
             Enemy nextTarget = FindNextChainTarget(currentTarget, chainedEnemies);
             currentTarget = nextTarget;
             currentDamage *= chainDamageReduction;
         }
         
         PlayLightningEffects(lightningPoints);
         SpawnChainLightningBolt(lightningPoints);
     }

    Enemy FindNextChainTarget(Enemy fromEnemy, List<Enemy> alreadyHit)
    {
        Collider[] nearbyEnemies = Physics.OverlapSphere(fromEnemy.transform.position, chainRange);
        // Debug.Log($"Lightning Tower: Searching for chain targets around {fromEnemy.name} (Range: {chainRange}, Found: {nearbyEnemies.Length} colliders)");
        
        float closestDistance = float.MaxValue;
        Enemy closestEnemy = null;
        
        foreach (Collider enemyCollider in nearbyEnemies)
        {
            Enemy enemy = enemyCollider.GetComponent<Enemy>();
            if (enemy != null && !alreadyHit.Contains(enemy))
            {
                float distance = Vector3.Distance(fromEnemy.transform.position, enemy.transform.position);
                // Debug.Log($"Lightning Tower: Found potential target {enemy.name} at distance {distance:F1}");
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy;
                }
            }
        }
        
        if (closestEnemy != null)
        {
            // Debug.Log($"Lightning Tower: Selected next target: {closestEnemy.name} at distance {closestDistance:F1}");
        }
        else
        {
            // Debug.Log("Lightning Tower: No valid chain targets found");
        }
        
        return closestEnemy;
    }

    public void PlayLightningEffects(List<Vector3> lightningPoints)
    {
        if (lightningLine != null && lightningPoints.Count > 1)
        {
            lightningLine.positionCount = lightningPoints.Count;
            lightningLine.SetPositions(lightningPoints.ToArray());
            lightningLine.material.color = lightningColor;

            // Animate the lightning
            StartCoroutine(AnimateLightning());
        }

        // Create lightning projectile visual
        if (lightningProjectilePrefab != null)
        {
            GameObject projectile = Instantiate(lightningProjectilePrefab, transform.position, Quaternion.identity);
            Destroy(projectile, 1f);
        }

        // Simple visual feedback without prefabs
        CreateSimpleLightningEffect();

        // Debug visual feedback
        // Debug.Log($"Lightning Tower attacking! Chain targets: {lightningPoints.Count}, Target: {currentEnemyTarget?.name}");
    }

    public void PlayLightningParticleEffect(Vector3 targetPosition)
    {
        if (lightningParticles != null)
        {
            lightningParticles.transform.position = transform.position;
            lightningParticles.transform.LookAt(targetPosition);
            lightningParticles.Play();
        }

        if (lightningLine != null)
        {
            lightningLine.SetPosition(0, transform.position);
            lightningLine.SetPosition(1, targetPosition);
            lightningLine.enabled = true;
            StartCoroutine(AnimateLightning());
        }
    }

    System.Collections.IEnumerator AnimateLightning()
    {
        lightningLine.enabled = true;
        yield return new WaitForSeconds(0.2f);
        lightningLine.enabled = false;
    }
    
     void CreateSimpleLightningEffect()
     {
         // Create a simple visual effect without requiring prefabs
         GameObject effect = GameObject.CreatePrimitive(PrimitiveType.Cube);
         effect.transform.position = transform.position;
         effect.transform.localScale = Vector3.one * 0.3f;
         effect.name = "LightningEffect";
         
         // Make it yellow and bright
         Renderer renderer = effect.GetComponent<Renderer>();
         Material mat = new Material(Shader.Find("Standard"));
         mat.color = lightningColor;
         mat.SetFloat("_Emission", 1f); // Make it glow
         renderer.material = mat;
         
         // Remove collider
         Destroy(effect.GetComponent<Collider>());
         
         // Destroy after short time
         Destroy(effect, 0.3f);
     }
     
     void ApplyLightningVisualEffect(Enemy enemy)
     {
         StartCoroutine(FlashEnemyLightning(enemy));
     }
     
     void SpawnChainLightningBolt(List<Vector3> points)
     {
         if (points.Count < 2) return;
         
         GameObject bolt = new GameObject("LightningBolt");
         LineRenderer line = bolt.AddComponent<LineRenderer>();
         
         List<Vector3> zigzagPoints = new List<Vector3>();
         
         for (int i = 0; i < points.Count - 1; i++)
         {
             Vector3 start = points[i];
             Vector3 end = points[i + 1];
             
             zigzagPoints.Add(start);
             
             int segments = 8;
             for (int j = 1; j < segments; j++)
             {
                 float t = j / (float)segments;
                 Vector3 midPoint = Vector3.Lerp(start, end, t);
                 
                 Vector3 perpendicular = Vector3.Cross((end - start).normalized, Vector3.up);
                 if (perpendicular.magnitude < 0.1f)
                     perpendicular = Vector3.Cross((end - start).normalized, Vector3.right);
                 
                 float offset = Random.Range(-0.3f, 0.3f);
                 midPoint += perpendicular.normalized * offset;
                 midPoint.y += Random.Range(-0.2f, 0.2f);
                 
                 zigzagPoints.Add(midPoint);
             }
         }
         
         zigzagPoints.Add(points[points.Count - 1]);
         
         line.positionCount = zigzagPoints.Count;
         line.SetPositions(zigzagPoints.ToArray());
         line.startWidth = 0.15f;
         line.endWidth = 0.1f;
         
         line.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
         line.material.color = lightningColor;
         
         Gradient gradient = new Gradient();
         gradient.SetKeys(
             new GradientColorKey[] { 
                 new GradientColorKey(Color.white, 0.0f), 
                 new GradientColorKey(lightningColor, 0.5f),
                 new GradientColorKey(new Color(1f, 1f, 0.5f), 1.0f)
             },
             new GradientAlphaKey[] { 
                 new GradientAlphaKey(1.0f, 0.0f), 
                 new GradientAlphaKey(0.8f, 1.0f) 
             }
         );
         line.colorGradient = gradient;
         
         Destroy(bolt, 0.3f);
     }
     
     void SpawnLightningImpactEffect(Vector3 position)
     {
         GameObject impact = new GameObject("LightningImpact");
         impact.transform.position = position;
         
         ParticleSystem ps = impact.AddComponent<ParticleSystem>();
         var main = ps.main;
         main.duration = 0.3f;
         main.startLifetime = 0.5f;
         main.startSpeed = 4f;
         main.startSize = 0.25f;
         main.startColor = new Color(1f, 1f, 0.7f, 1f);
         main.gravityModifier = -0.2f;
         
         var emission = ps.emission;
         emission.rateOverTime = 0;
         emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 20) });
         
         var shape = ps.shape;
         shape.shapeType = ParticleSystemShapeType.Sphere;
         shape.radius = 0.3f;
         
         ps.Play();
         
         GameObject flash = GameObject.CreatePrimitive(PrimitiveType.Sphere);
         flash.transform.position = position;
         flash.transform.localScale = Vector3.one * 0.6f;
         flash.transform.parent = impact.transform;
         
         Renderer flashRenderer = flash.GetComponent<Renderer>();
         Material flashMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
         flashMat.color = new Color(1f, 1f, 0.5f, 0.8f);
         flashRenderer.material = flashMat;
         
         Destroy(flash.GetComponent<Collider>());
         
         Destroy(impact, 0.6f);
     }
     
     System.Collections.IEnumerator FlashEnemyLightning(Enemy enemy)
     {
         if (enemy == null) yield break;
         
         // Get the enemy's renderer
         Renderer enemyRenderer = enemy.GetComponent<Renderer>();
         if (enemyRenderer == null) yield break;
         
         // Store original material
         Material originalMaterial = enemyRenderer.material;
         Color originalColor = originalMaterial.color;
         
         // Create lightning material
         Material lightningMaterial = new Material(Shader.Find("Standard"));
         lightningMaterial.color = lightningColor;
         lightningMaterial.SetFloat("_Emission", 2f); // Bright yellow glow
         
         // Flash the enemy 3 times
         for (int i = 0; i < 3; i++)
         {
             // Flash yellow
             enemyRenderer.material = lightningMaterial;
             yield return new WaitForSeconds(0.1f);
             
             // Flash back to original
             enemyRenderer.material = originalMaterial;
             yield return new WaitForSeconds(0.1f);
         }
         
         // Ensure we end with the original material
         if (enemyRenderer != null)
         {
             enemyRenderer.material = originalMaterial;
         }
     }
}
