using UnityEngine;

/// <summary>
/// Wind zone hazard that pushes units and affects projectile trajectories.
/// Creates dynamic battlefield conditions requiring adaptive strategies.
/// </summary>
public class WindZoneHazard : EnvironmentalHazard
{
    [Header("Wind Settings")]
    [Tooltip("Force applied to units in wind zone")]
    public float windForce = 5f;
    
    [Tooltip("Direction change interval in seconds")]
    public float directionChangeInterval = 3f;
    
    [Tooltip("Projectile deflection strength")]
    public float projectileDeflection = 2f;
    
    private Vector3 currentWindDirection;
    private float lastDirectionChange = 0f;
    
    protected override void Start()
    {
        hazardType = HazardType.Wind;
        base.Start();
        
        // Initialize random wind direction
        ChangeWindDirection();
    }
    
    protected override void ApplyHazardEffect(GameObject target, bool isEntering)
    {
        if (target == null) return;
        
        // Wind affects all units equally - no special entering/exiting logic
        // The wind force is applied continuously in UpdateHazardEffects
    }
    
    protected override void UpdateHazardEffects()
    {
        // Change wind direction periodically
        if (Time.time - lastDirectionChange >= directionChangeInterval)
        {
            ChangeWindDirection();
            lastDirectionChange = Time.time;
        }
        
        // Apply wind force to units in the zone
        Collider[] colliders = Physics.OverlapSphere(transform.position, effectRadius);
        
        foreach (Collider col in colliders)
        {
            if (col == hazardCollider) continue;
            
            // Apply wind force to enemies
            Enemy enemy = col.GetComponent<Enemy>();
            if (enemy != null)
            {
                ApplyWindForce(enemy.gameObject);
            }
            
            // Apply wind force to defenders (they don't move, but projectiles are affected)
            Defender defender = col.GetComponent<Defender>();
            if (defender != null)
            {
                // Wind affects projectiles, not the defender itself
                // This is handled in the projectile deflection system
            }
            
            // Apply wind force to tower (minimal effect)
            Tower tower = col.GetComponent<Tower>();
            if (tower != null)
            {
                // Tower is stationary, but projectiles are affected
            }
        }
    }
    
    private void ChangeWindDirection()
    {
        // Generate random wind direction
        float angle = Random.Range(0f, 360f);
        currentWindDirection = new Vector3(
            Mathf.Cos(angle * Mathf.Deg2Rad),
            0f,
            Mathf.Sin(angle * Mathf.Deg2Rad)
        );
        
        // Update particle system direction
        if (ambientParticles != null)
        {
            var velocityOverLifetime = ambientParticles.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
            velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(currentWindDirection.x * windForce);
            velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(currentWindDirection.z * windForce);
        }
    }
    
    private void ApplyWindForce(GameObject target)
    {
        // Apply wind force to rigidbody if present
        Rigidbody rb = target.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 windForceVector = currentWindDirection * windForce * intensity;
            rb.AddForce(windForceVector, ForceMode.Force);
        }
        else
        {
            // Apply direct position modification for non-rigidbody objects
            Vector3 windOffset = currentWindDirection * windForce * intensity * Time.deltaTime;
            target.transform.position += windOffset;
        }
    }
    
    /// <summary>
    /// Gets the current wind direction for projectile deflection
    /// </summary>
    public Vector3 GetWindDirection()
    {
        return currentWindDirection;
    }
    
    /// <summary>
    /// Gets the wind force strength for projectile deflection
    /// </summary>
    public float GetWindForce()
    {
        return windForce * intensity;
    }
    
    protected override void SetupParticleSystem()
    {
        if (ambientParticles == null) return;
        
        var main = ambientParticles.main;
        main.startLifetime = 2f;
        main.startSpeed = 3f;
        main.startSize = 0.1f;
        main.startColor = new Color(0.8f, 0.8f, 0.9f, 0.4f); // Light gray
        main.maxParticles = 40;
        
        var emission = ambientParticles.emission;
        emission.rateOverTime = 25f;
        
        var shape = ambientParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = effectRadius;
        
        var velocityOverLifetime = ambientParticles.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(currentWindDirection.x * windForce);
        velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(currentWindDirection.z * windForce);
        
        var sizeOverLifetime = ambientParticles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(0.05f, 0.2f);
        
        ambientParticles.Play();
    }
}
