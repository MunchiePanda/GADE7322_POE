using UnityEngine;

public class Tower : MonoBehaviour
{
    [Header("Tower Stats")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    
    [Header("References")]
    [SerializeField] private GameManager gameManager;
    
    void Start()
    {
        currentHealth = maxHealth;
        
        // Get GameManager reference if not assigned
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }
        
        // Update UI with initial health
        UpdateHealthUI();
    }
    
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth); // Don't go below 0
        
        UpdateHealthUI();
        
        // Visual feedback (you can add particle effects, screen shake, etc.)
        Debug.Log($"Tower took {damage} damage! Health: {currentHealth}/{maxHealth}");
        
        // Check if tower is destroyed
        if (currentHealth <= 0)
        {
            OnTowerDestroyed();
        }
    }
    
    public void Heal(float healAmount)
    {
        currentHealth += healAmount;
        currentHealth = Mathf.Min(maxHealth, currentHealth); // Don't exceed max health
        
        UpdateHealthUI();
        
        Debug.Log($"Tower healed {healAmount}! Health: {currentHealth}/{maxHealth}");
    }
    
    private void UpdateHealthUI()
    {
        if (gameManager != null && gameManager.towerHealthBar != null)
        {
            gameManager.towerHealthBar.SetHealth(currentHealth, maxHealth);
        }
    }
    
    private void OnTowerDestroyed()
    {
        Debug.Log("Tower destroyed! Game Over!");
        
        if (gameManager != null)
        {
            gameManager.GameOver();
        }
    }
    
    // Public getters
    public float GetCurrentHealth() { return currentHealth; }
    public float GetMaxHealth() { return maxHealth; }
    public float GetHealthPercentage() { return currentHealth / maxHealth; }
    
    // Public setters
    public void SetMaxHealth(float newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        UpdateHealthUI();
    }
} 