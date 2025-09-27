using UnityEngine;
using System.Collections;

public class StrategicWaveManager : MonoBehaviour
{
    [Header("References")]
    public GameManager gameManager;
    public EnemySpawner enemySpawner;
    public StrategicCameraController cameraController;
    public StrategicDefenderPlacement defenderPlacement;
    
    [Header("Strategic Phase Settings")]
    public float strategicPhaseDuration = 30f;
    public float preparationTime = 5f;
    
    private bool isInStrategicPhase = false;
    private bool hasStartedFirstWave = false;
    
    void Start()
    {
        if (gameManager == null) gameManager = FindFirstObjectByType<GameManager>();
        if (enemySpawner == null) enemySpawner = FindFirstObjectByType<EnemySpawner>();
        if (cameraController == null) cameraController = FindFirstObjectByType<StrategicCameraController>();
        if (defenderPlacement == null) defenderPlacement = FindFirstObjectByType<StrategicDefenderPlacement>();
        
        StartCoroutine(StartInitialStrategicPhase());
    }
    
    IEnumerator StartInitialStrategicPhase()
    {
        yield return new WaitForSeconds(1f);
        
        if (!hasStartedFirstWave)
        {
            StartStrategicPhase();
            hasStartedFirstWave = true;
        }
    }
    
    public void StartStrategicPhase()
    {
        if (isInStrategicPhase) return;
        
        isInStrategicPhase = true;
        
        cameraController.EnterStrategicMode();
        defenderPlacement.ShowAllDefenderLocations();
        
        if (enemySpawner != null)
        {
            enemySpawner.enabled = false;
        }
        
        StartCoroutine(StrategicPhaseTimer());
    }
    
    public void EndStrategicPhase()
    {
        if (!isInStrategicPhase) return;
        
        isInStrategicPhase = false;
        
        cameraController.ExitStrategicMode();
        defenderPlacement.HideAllDefenderLocations();
        
        if (enemySpawner != null)
        {
            enemySpawner.enabled = true;
        }
    }
    
    IEnumerator StrategicPhaseTimer()
    {
        yield return new WaitForSeconds(strategicPhaseDuration);
        
        if (isInStrategicPhase)
        {
            EndStrategicPhase();
        }
    }
    
    public bool IsInStrategicPhase => isInStrategicPhase;
    
    public void OnWaveComplete()
    {
        StartCoroutine(StartInterWaveStrategicPhase());
    }
    
    IEnumerator StartInterWaveStrategicPhase()
    {
        yield return new WaitForSeconds(preparationTime);
        StartStrategicPhase();
    }
}