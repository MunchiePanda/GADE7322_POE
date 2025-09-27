using UnityEngine;
using System.Collections;

public class StrategicCameraController : MonoBehaviour
{
    [Header("Camera References")]
    public Camera strategicCamera;
    private Camera gameplayCamera;
    public string gameplayCameraTag = "MainCamera";
    
    [Header("Strategic View Settings")]
    public float strategicHeight = 50f;
    public float strategicSize = 30f;
    public Vector3 strategicOffset = new Vector3(0, 0, 0);
    
    [Header("Transition Settings")]
    public float transitionDuration = 2f;
    public AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    private VoxelTerrainGenerator terrainGenerator;
    private bool isInStrategicMode = false;
    
    void Start()
    {
        terrainGenerator = FindFirstObjectByType<VoxelTerrainGenerator>();
        
        if (strategicCamera == null)
        {
            SetupStrategicCamera();
        }
        
        strategicCamera.gameObject.SetActive(false);
    }
    
    void SetupStrategicCamera()
    {
        GameObject strategicCamObj = new GameObject("Strategic Camera");
        strategicCamera = strategicCamObj.AddComponent<Camera>();
        strategicCamera.CopyFrom(gameplayCamera);
        strategicCamera.orthographic = true;
        strategicCamera.orthographicSize = strategicSize;
        strategicCamera.gameObject.SetActive(false);
    }
    
    public void EnterStrategicMode()
    {
        if (isInStrategicMode) return;
        
        StartCoroutine(TransitionToStrategicView());
    }
    
    public void ExitStrategicMode()
    {
        if (!isInStrategicMode) return;
        
        StartCoroutine(TransitionToGameplayView());
    }
    
    IEnumerator TransitionToStrategicView()
    {
        isInStrategicMode = true;

        if (gameplayCamera == null)
        {
            gameplayCamera = GameObject.FindGameObjectWithTag(gameplayCameraTag).GetComponent<Camera>();
            if (gameplayCamera == null)
            {
                Debug.LogError("Gameplay Camera not found. Make sure the camera has the tag '" + gameplayCameraTag + "'.");
                yield break;
            }
        }
        
        Vector3Int center = terrainGenerator.GetCenterGrid();
        Vector3 centerWorld = terrainGenerator.GridToWorld(center.x, center.z);
        Vector3 strategicPosition = centerWorld + new Vector3(strategicOffset.x, strategicHeight, strategicOffset.z);
        
        strategicCamera.transform.position = strategicPosition;
        strategicCamera.transform.rotation = Quaternion.Euler(90, 0, 0);
        
        strategicCamera.gameObject.SetActive(true);
        
        yield return new WaitForSeconds(transitionDuration);
        
        gameplayCamera.gameObject.SetActive(false);
    }

    IEnumerator TransitionToGameplayView()
    {
        isInStrategicMode = false;

        if (gameplayCamera == null)
        {
            gameplayCamera = GameObject.FindGameObjectWithTag(gameplayCameraTag).GetComponent<Camera>();
            if (gameplayCamera == null)
            {
                Debug.LogError("Gameplay Camera not found. Make sure the camera has the tag '" + gameplayCameraTag + "'.");
                yield break;
            }
        }
        
        gameplayCamera.gameObject.SetActive(true);
        
        yield return new WaitForSeconds(transitionDuration);
        
        strategicCamera.gameObject.SetActive(false);
    }
    
    public bool IsInStrategicMode => isInStrategicMode;
}