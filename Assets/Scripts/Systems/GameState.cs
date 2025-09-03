using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GADE7322_POE.Systems
{
    /// <summary>
    /// Simplified game state manager: tracks waves, resources, and win/lose conditions.
    /// </summary>
    public class GameState : MonoBehaviour
    {
        [Header("Game Settings")]
        [Tooltip("Total number of waves to win the game.")]
        public int TotalWaves = 3;
        [Tooltip("Current wave (1-based).")]
        public int CurrentWave = 1;
        [Tooltip("Player's current resources.")]
        public int Resources = 0;
        [Tooltip("Resources needed to buy a defender.")]
        public int DefenderCost = 50;

        [Header("UI References")]
        [Tooltip("Text to display the current wave.")]
        public Text WaveText;
        [Tooltip("Text to display the player's resources.")]
        public Text ResourceText;
        [Tooltip("Panel to show when the game is won.")]
        public GameObject WinPanel;
        [Tooltip("Panel to show when the game is lost.")]
        public GameObject LosePanel;

        [Header("Events")]
        public UnityEvent OnWaveComplete;  // Triggered when a wave is completed
        public UnityEvent OnGameWin;       // Triggered when the player wins
        public UnityEvent OnGameLose;      // Triggered when the player loses

        [Header("Enemy Settings")]
        [Tooltip("Number of enemies to spawn per wave.")]
        public int EnemiesPerWave = 10;
        [Tooltip("Current enemies remaining in the wave.")]
        public int EnemiesRemaining;

        private void Start()
        {
            // Initialize UI
            UpdateWaveUI();
            UpdateResourceUI();

            // Hide win/lose panels
            WinPanel.SetActive(false);
            LosePanel.SetActive(false);

            // Start the first wave
            StartWave();
        }

        /// <summary>
        /// Starts a new wave.
        /// </summary>
        public void StartWave()
        {
            EnemiesRemaining = EnemiesPerWave * CurrentWave; // Scale enemies per wave
            Debug.Log($"Wave {CurrentWave} started! Enemies: {EnemiesRemaining}");
        }

        /// <summary>
        /// Called when an enemy is killed.
        /// </summary>
        public void OnEnemyKilled(int resourcesDropped)
        {
            Resources += resourcesDropped;
            EnemiesRemaining--;
            UpdateResourceUI();

            if (EnemiesRemaining <= 0)
            {
                CompleteWave();
            }
        }

        /// <summary>
        /// Completes the current wave and starts the next one (or ends the game).
        /// </summary>
        public void CompleteWave()
        {
            Debug.Log($"Wave {CurrentWave} completed!");

            if (CurrentWave >= TotalWaves)
            {
                GameWin();
                return;
            }

            CurrentWave++;
            UpdateWaveUI();
            OnWaveComplete?.Invoke();
            StartWave();
        }

        /// <summary>
        /// Called when the tower is destroyed (game over).
        /// </summary>
        public void GameLose()
        {
            Debug.Log("Game Over! Tower destroyed.");
            LosePanel.SetActive(true);
            OnGameLose?.Invoke();
            Time.timeScale = 0; // Pause the game
        }

        /// <summary>
        /// Called when all waves are completed (player wins).
        /// </summary>
        public void GameWin()
        {
            Debug.Log("You Win! All waves completed.");
            WinPanel.SetActive(true);
            OnGameWin?.Invoke();
            Time.timeScale = 0; // Pause the game
        }

        /// <summary>
        /// Updates the wave UI text.
        /// </summary>
        private void UpdateWaveUI()
        {
            if (WaveText != null)
            {
                WaveText.text = $"Wave: {CurrentWave}/{TotalWaves}";
            }
        }

        /// <summary>
        /// Updates the resource UI text.
        /// </summary>
        private void UpdateResourceUI()
        {
            if (ResourceText != null)
            {
                ResourceText.text = $"Resources: {Resources}";
            }
        }

        /// <summary>
        /// Attempts to buy a defender (if enough resources).
        /// </summary>
        public bool TryBuyDefender()
        {
            if (Resources >= DefenderCost)
            {
                Resources -= DefenderCost;
                UpdateResourceUI();
                return true;
            }
            Debug.Log("Not enough resources!");
            return false;
        }
    }
}
