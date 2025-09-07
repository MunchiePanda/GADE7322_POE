using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Transform target;
    private float damage;
    private float speed;
    private bool isInitialized = false;

    public void Initialize(Transform targetTransform, float damageAmount, float projectileSpeed)
    {
        target = targetTransform;
        damage = damageAmount;
        speed = projectileSpeed;
        isInitialized = true;
    }

    void Start()
    {
        if (isInitialized && target != null)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            transform.forward = direction;
        }
    }

    void Update()
    {
        if (!isInitialized || target == null)
        {
            Destroy(gameObject);
            return;
        }

        // Dynamically track the target
        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        // Check if the projectile has reached the target
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        if (distanceToTarget < 0.5f)
        {
            HitTarget();
        }
    }

    void HitTarget()
    {
        Enemy enemy = target.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }

        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Projectile collided with {other.gameObject.name}");

        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            Debug.Log($"Enemy found: {enemy.gameObject.name}, applying {damage} damage");
            enemy.TakeDamage(damage);
            Destroy(gameObject);
        }
        else if (other.CompareTag("Terrain"))
        {
            Debug.Log("Projectile hit terrain, destroying");
            Destroy(gameObject);
        }
    }
}
