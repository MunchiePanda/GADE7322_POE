using UnityEngine;
using UnityEngine.InputSystem;

public class TowerController : MonoBehaviour
{
    [Header("Tower Settings")]
    [SerializeField] private float rotationSpeed = 90f; // Degrees per second
    [SerializeField] private Transform towerTurret; // The part of the tower that rotates (optional)
    
    [Header("Path Selection")]
    [SerializeField] private VoxelTerrainGenerator terrainGenerator;
    [SerializeField] private int currentPathIndex = 0;

    private Camera mainCamera;
    private Transform targetTransform;
    private bool isRotating = false;
    private Vector3 targetDirection;
    
    void Start()
    {
        targetTransform = towerTurret != null ? towerTurret : transform;

        // Get terrain generator reference if not assigned
        if (terrainGenerator == null)
        {
            terrainGenerator = FindFirstObjectByType<VoxelTerrainGenerator>();
        }

        // Get main camera reference
        mainCamera = Camera.main;
    }
    
    void Update()
    {
        HandleInput();
        HandleRotation();
    }
    
    void HandleInput()
    {
        // Number keys 1-9 to select paths
        for (int i = 0; i < 9; i++)
        {
            if (Keyboard.current != null && Keyboard.current[Key.Digit1 + i].wasPressedThisFrame)
            {
                SelectPath(i);
            }
        }

        // Arrow keys for manual rotation
        if (Keyboard.current != null)
        {
            if (Keyboard.current[Key.LeftArrow].isPressed)
            {
                RotateTower(-rotationSpeed * Time.deltaTime);
            }
            if (Keyboard.current[Key.RightArrow].isPressed)
            {
                RotateTower(rotationSpeed * Time.deltaTime);
            }
        }

        // E key to shoot
        if (Keyboard.current != null && Keyboard.current[Key.E].wasPressedThisFrame)
        {
            ShootAtMousePosition();
        }
    }

    void HandleShooting()
    {
        if (UnityEngine.InputSystem.Mouse.current != null && UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame)
        {
            ShootAtMousePosition();
        }
    }
    
    void HandleRotation()
    {
        if (isRotating)
        {
            // Smoothly rotate towards target direction
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            targetTransform.rotation = Quaternion.RotateTowards(
                targetTransform.rotation, 
                targetRotation, 
                rotationSpeed * Time.deltaTime
            );
            
            // Check if we've reached the target rotation
            if (Quaternion.Angle(targetTransform.rotation, targetRotation) < 1f)
            {
                isRotating = false;
            }
        }
    }
    
    public void SelectPath(int pathIndex)
    {
        if (terrainGenerator == null) return;
        
        // Get the paths from terrain generator (you'll need to make this public)
        var paths = terrainGenerator.GetPaths();
        if (paths == null || pathIndex >= paths.Count || pathIndex < 0) return;
        
        currentPathIndex = pathIndex;
        
        // Get the entrance position of the selected path
        if (paths[pathIndex].Count > 0)
        {
            Vector3Int entrance = paths[pathIndex][0];
            Vector3 entranceWorldPos = new Vector3(entrance.x, transform.position.y, entrance.z);
            
            // Calculate direction to face the path entrance
            targetDirection = (entranceWorldPos - transform.position).normalized;
            isRotating = true;
            
            // Notify terrain generator to highlight this path
            terrainGenerator.HighlightPath(pathIndex);
        }
    }
    
    public void RotateTower(float angle)
    {
        targetTransform.Rotate(0, angle, 0);
    }
    
    public void FaceDirection(Vector3 direction)
    {
        targetDirection = direction.normalized;
        isRotating = true;
    }

    public int GetCurrentPathIndex()
    {
        return currentPathIndex;
    }

    private void ShootAtMousePosition()
    {
        Tower tower = GetComponent<Tower>();
        if (tower != null && tower.projectilePrefab != null)
        {
            Vector3 spawnPosition = tower.projectileSpawnPoint != null ? tower.projectileSpawnPoint.position : targetTransform.position;  // Use targetTransform if no spawn point

            // Instantiate with the turret's rotation
            GameObject projectile = Instantiate(tower.projectilePrefab, spawnPosition, targetTransform.rotation);
            Projectile projectileComponent = projectile.GetComponent<Projectile>();

            if (projectileComponent != null)
            {
                // Create a temporary target in the direction the turret/tower is facing
                GameObject tempTarget = new GameObject("TempTarget");
                tempTarget.transform.position = spawnPosition + targetTransform.forward * 50f;  // Use targetTransform.forward
                DummyEnemy dummyEnemy = tempTarget.AddComponent<DummyEnemy>();
                projectileComponent.Initialize(dummyEnemy.transform, tower.attackDamage, tower.projectileSpeed);
                Destroy(tempTarget, 5f);
            }
        }
    }
}

// DummyEnemy class to act as a target for the projectile
public class DummyEnemy : Enemy
{
    protected override void Start() { }
}