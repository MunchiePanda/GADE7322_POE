using UnityEngine;

/// <summary>
/// Lava pool hazard that deals damage over time and slows movement.
/// Creates strategic positioning challenges for both enemies and defenders.
/// </summary>
public class LavaPoolHazard : EnvironmentalHazard
{
    [Header("Lava Settings")]
    [Tooltip("Damage per second dealt by lava")]
    public float damagePerSecond = 3f;
    
    [Tooltip("Movement speed reduction (0.5 = 50% slower)")]
    public float speedReduction = 0.5f;
    
    [Tooltip("Damage interval in seconds")]
    public float damageInterval = 1f;
    
    private float lastDamageTime = 0f;
    
    protected override void Start()
    {
        hazardType = HazardType.Lava;
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
        
        // Apply to defenders
        Defender defender = target.GetComponent<Defender>();
        if (defender != null)
        {
            // Defenders don't move, so no speed effect
            // Damage will be applied in UpdateHazardEffects
        }
        
        // Apply to tower
        Tower tower = target.GetComponent<Tower>();
        if (tower != null)
        {
            // Tower doesn't move, so no speed effect
            // Damage will be applied in UpdateHazardEffects
        }
    }
    
    protected override void UpdateHazardEffects()
    {
        // Deal damage over time to units in the hazard
        if (Time.time - lastDamageTime >= damageInterval)
        {
            lastDamageTime = Time.time;
            
            // Find all colliders in the hazard
            Collider[] colliders = Physics.OverlapSphere(transform.position, effectRadius);
            
            foreach (Collider col in colliders)
            {
                if (col == hazardCollider) continue;
                
                // Deal damage to enemies
                Enemy enemy = col.GetComponent<Enemy>();
                if (enemy != null)
                {
                    float damage = damagePerSecond * intensity;
                    enemy.TakeDamage(damage);
                }
                
                // Deal damage to defenders
                Defender defender = col.GetComponent<Defender>();
                if (defender != null)
                {
                    float damage = damagePerSecond * intensity;
                    defender.TakeDamage(damage);
                }
                
                // Deal damage to tower
                Tower tower = col.GetComponent<Tower>();
                if (tower != null)
                {
                    float damage = damagePerSecond * intensity;
                    tower.TakeDamage(damage);
                }
            }
        }
    }
    
    protected override void SetupParticleSystem()
    {
        if (ambientParticles == null) return;
        
        var main = ambientParticles.main;
        main.startLifetime = 2f;
        main.startSpeed = 2f;
        main.startSize = 0.3f;
        main.startColor = new Color(1f, 0.3f, 0f, 0.8f); // Orange-red
        main.maxParticles = 50;
        
        var emission = ambientParticles.emission;
        emission.rateOverTime = 20f;
        
        var shape = ambientParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = effectRadius;
        
        var velocityOverLifetime = ambientParticles.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(2f);
        
        ambientParticles.Play();
    }
}
