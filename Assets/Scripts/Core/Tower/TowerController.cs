using UnityEngine;
using UnityEngine.InputSystem;

/*
 * TowerController.cs
 * -------------------
 * This script manages the tower's rotation, path selection, and shooting mechanics.
 * It allows the player to control the tower's turret, select paths, and shoot projectiles.
 *
 * Attach this script to the tower GameObject.
 */
public class TowerController : MonoBehaviour
{
    [Header("Tower Settings")]
    [Tooltip("The speed at which the tower turret rotates (degrees per second).")]
    [SerializeField] private float rotationSpeed = 90f;
    [Tooltip("The transform of the tower turret that rotates (optional).")]
    [SerializeField] private Transform towerTurret;

    [Header("Path Selection")]
    [Tooltip("Reference to the VoxelTerrainGenerator for path data.")]
    [SerializeField] private VoxelTerrainGenerator terrainGenerator;
    [Tooltip("The index of the currently selected path.")]
    [SerializeField] private int currentPathIndex = 0;

    // Reference to the main camera.
    private Camera mainCamera;

    // Reference to the transform that will rotate (turret or tower).
    private Transform targetTransform;

    // Flag to track if the tower is currently rotating.
    private bool isRotating = false;

    // The direction the tower is rotating towards.
    private Vector3 targetDirection;
    
    void Start()
    {
        // Set the target transform to the turret if available, otherwise use the tower's transform.
        targetTransform = towerTurret != null ? towerTurret : transform;

        // Get a reference to the VoxelTerrainGenerator if not already assigned.
        if (terrainGenerator == null)
        {
            terrainGenerator = FindFirstObjectByType<VoxelTerrainGenerator>();
        }

        // Get a reference to the main camera.
        mainCamera = Camera.main;
    }
    
    void Update()
    {
        // Handle player input for path selection, rotation, and shooting.
        HandleInput();

        // Handle the tower's rotation towards the target direction.
        HandleRotation();
    }
    
    /// <summary>
    /// Handles player input for path selection, rotation, and shooting.
    /// </summary>
    void HandleInput()
    {
        // Use number keys 1-9 to select different paths.
        for (int i = 0; i < 9; i++)
        {
            if (Keyboard.current != null && Keyboard.current[Key.Digit1 + i].wasPressedThisFrame)
            {
                SelectPath(i);
            }
        }

        // Use arrow keys to manually rotate the tower.
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

        // Use the E key to shoot a projectile.
        if (Keyboard.current != null && Keyboard.current[Key.E].wasPressedThisFrame)
        {
            ShootAtMousePosition();
        }
    }

    /// <summary>
    /// Handles shooting input from the mouse.
    /// </summary>
    void HandleShooting()
    {
        // Shoot a projectile when the left mouse button is pressed.
        if (UnityEngine.InputSystem.Mouse.current != null && UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame)
        {
            ShootAtMousePosition();
        }
    }
    
    /// <summary>
    /// Handles the tower's rotation towards the target direction.
    /// </summary>
    void HandleRotation()
    {
        // If the tower is currently rotating, smoothly rotate towards the target direction.
        if (isRotating)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            targetTransform.rotation = Quaternion.RotateTowards(
                targetTransform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );

            // Stop rotating if the target rotation is reached.
            if (Quaternion.Angle(targetTransform.rotation, targetRotation) < 1f)
            {
                isRotating = false;
            }
        }
    }
    
    /// <summary>
    /// Selects a path and rotates the tower to face its entrance.
    /// </summary>
    /// <param name="pathIndex">The index of the path to select.</param>
    public void SelectPath(int pathIndex)
    {
        // Exit if the terrain generator is not available.
        if (terrainGenerator == null) return;

        // Get the list of paths from the terrain generator.
        var paths = terrainGenerator.GetPaths();
        if (paths == null || pathIndex >= paths.Count || pathIndex < 0) return;

        // Update the current path index.
        currentPathIndex = pathIndex;

        // If the selected path has an entrance, rotate the tower to face it.
        if (paths[pathIndex].Count > 0)
        {
            Vector3Int entrance = paths[pathIndex][0];
            Vector3 entranceWorldPos = terrainGenerator.GetSurfaceWorldPosition(entrance);

            // Calculate the direction to the path entrance.
            targetDirection = (entranceWorldPos - transform.position).normalized;
            isRotating = true;

            // Highlight the selected path.
            terrainGenerator.HighlightPath(pathIndex);
        }
    }
    
    /// <summary>
    /// Rotates the tower by the specified angle.
    /// </summary>
    /// <param name="angle">The angle (in degrees) to rotate the tower.</param>
    public void RotateTower(float angle)
    {
        // Rotate the tower around the Y-axis.
        targetTransform.Rotate(0, angle, 0);
    }
    
    /// <summary>
    /// Sets the tower to face the specified direction.
    /// </summary>
    /// <param name="direction">The direction the tower should face.</param>
    public void FaceDirection(Vector3 direction)
    {
        // Normalize the direction and start rotating towards it.
        targetDirection = direction.normalized;
        isRotating = true;
    }

    /// <summary>
    /// Gets the index of the currently selected path.
    /// </summary>
    /// <returns>The index of the current path.</returns>
    public int GetCurrentPathIndex()
    {
        return currentPathIndex;
    }

    /// <summary>
    /// Shoots a projectile in the direction the tower is facing.
    /// </summary>
    private void ShootAtMousePosition()
    {
        // Get the Tower component and check if the projectile prefab is assigned.
        Tower tower = GetComponent<Tower>();
        if (tower != null && tower.projectilePrefab != null)
        {
            // Determine the spawn position for the projectile.
            Vector3 spawnPosition = tower.projectileSpawnPoint != null ? tower.projectileSpawnPoint.position : targetTransform.position;

            // Instantiate the projectile with the turret's rotation.
            GameObject projectile = Instantiate(tower.projectilePrefab, spawnPosition, targetTransform.rotation);
            Projectile projectileComponent = projectile.GetComponent<Projectile>();

            // Initialize the projectile to target a temporary dummy enemy.
            if (projectileComponent != null)
            {
                // Create a temporary target in the direction the turret is facing.
                GameObject tempTarget = new GameObject("TempTarget");
                tempTarget.transform.position = spawnPosition + targetTransform.forward * 50f;
                DummyEnemy dummyEnemy = tempTarget.AddComponent<DummyEnemy>();
                projectileComponent.Initialize(dummyEnemy.transform, tower.attackDamage, tower.projectileSpeed);
                Destroy(tempTarget, 5f);
            }
        }
    }
}

// DummyEnemy class to act as a temporary target for the projectile.
public class DummyEnemy : Enemy
{
    protected override void Start() { }
}