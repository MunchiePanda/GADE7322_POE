using UnityEngine;

/// <summary>
/// Ice patch hazard that slows movement and has a chance to freeze units.
/// Creates tactical positioning opportunities and affects projectile paths.
/// </summary>
public class IcePatchHazard : EnvironmentalHazard
{
    [Header("Ice Settings")]
    [Tooltip("Movement speed reduction (0.3 = 30% slower)")]
    public float speedReduction = 0.3f;
    
    [Tooltip("Chance per second to freeze units (0-1)")]
    public float freezeChance = 0.15f;
    
    [Tooltip("Duration of freeze effect in seconds")]
    public float freezeDuration = 1f;
    
    [Tooltip("Projectile speed reduction multiplier")]
    public float projectileSlowMultiplier = 0.7f;
    
    private float lastFreezeCheck = 0f;
    
    protected override void Start()
    {
        hazardType = HazardType.Ice;
        base.Start();
    }
    
    protected override void ApplyHazardEffect(GameObject target, bool isEntering)
    {
        if (target == null) return;
        
        // Apply to enemies
        Enemy enemy = target.GetComponent<Enemy>();
            if (enemy != null)
            {
                if (isEntering)
                {
                    // Slow down enemy
                    enemy.MoveSpeed *= (1f - speedReduction);
                }
                else
                {
                    // Restore enemy speed
                    enemy.MoveSpeed /= (1f - speedReduction);
                }
            }
        
        // Apply to defenders (they don't move, so no speed effect)
        // But we can affect their projectiles
        
        // Apply to tower (no movement effect)
    }
    
    protected override void UpdateHazardEffects()
    {
        // Check for freeze chance
        if (Time.time - lastFreezeCheck >= 1f)
        {
            lastFreezeCheck = Time.time;
            
            // Find all colliders in the hazard
            Collider[] colliders = Physics.OverlapSphere(transform.position, effectRadius);
            
            foreach (Collider col in colliders)
            {
                if (col == hazardCollider) continue;
                
                // Check freeze chance
                if (Random.Range(0f, 1f) < freezeChance * intensity)
                {
                    // Apply freeze effect
                    StartCoroutine(FreezeUnit(col.gameObject));
                }
            }
        }
    }
    
    private System.Collections.IEnumerator FreezeUnit(GameObject unit)
    {
        // Store original speed
        float originalSpeed = 0f;
        Enemy enemy = unit.GetComponent<Enemy>();
        if (enemy != null)
        {
            originalSpeed = enemy.MoveSpeed;
            enemy.MoveSpeed = 0f; // Stop movement
        }
        
        // Visual freeze effect
        Renderer renderer = unit.GetComponent<Renderer>();
        Color originalColor = Color.white;
        if (renderer != null)
        {
            originalColor = renderer.material.color;
            renderer.material.color = Color.cyan; // Freeze color
        }
        
        // Wait for freeze duration
        yield return new WaitForSeconds(freezeDuration);
        
        // Restore movement
        if (enemy != null)
        {
            enemy.MoveSpeed = originalSpeed;
        }
        
        // Restore color
        if (renderer != null)
        {
            renderer.material.color = originalColor;
        }
    }
    
    protected override void SetupParticleSystem()
    {
        if (ambientParticles == null) return;
        
        var main = ambientParticles.main;
        main.startLifetime = 3f;
        main.startSpeed = 1f;
        main.startSize = 0.2f;
        main.startColor = new Color(0.7f, 0.9f, 1f, 0.6f); // Light blue
        main.maxParticles = 30;
        
        var emission = ambientParticles.emission;
        emission.rateOverTime = 10f;
        
        var shape = ambientParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = effectRadius;
        
        var velocityOverLifetime = ambientParticles.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(1f);
        
        var sizeOverLifetime = ambientParticles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
        
        ambientParticles.Play();
    }
}
