using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Simple test script to verify drag-drop functionality.
/// Attach this to a UI button to test basic drag-drop behavior.
/// </summary>
public class SimpleDragDropTest : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Test Settings")]
    public GameObject testPreviewPrefab;
    public Material testMaterial;
    
    private GameObject previewObject;
    private Camera cam;
    
    void Start()
    {
        cam = Camera.main;
        
        // Create default material if not assigned
        if (testMaterial == null)
        {
            testMaterial = new Material(Shader.Find("Standard"));
            testMaterial.color = Color.yellow;
        }
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("SimpleDragDropTest: OnBeginDrag called!");
        
        // Create a simple preview object
        if (testPreviewPrefab != null)
        {
            previewObject = Instantiate(testPreviewPrefab);
        }
        else
        {
            // Create a simple cube
            previewObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            previewObject.transform.localScale = Vector3.one * 0.5f;
            previewObject.GetComponent<Renderer>().material = testMaterial;
            Destroy(previewObject.GetComponent<Collider>());
        }
        
        Debug.Log("Preview object created!");
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (previewObject == null) return;
        
        Debug.Log("SimpleDragDropTest: OnDrag called!");
        
        // Convert screen position to world position
        Ray ray = cam.ScreenPointToRay(eventData.position);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit))
        {
            Vector3 worldPos = hit.point;
            previewObject.transform.position = worldPos;
            Debug.Log($"Preview moved to: {worldPos}");
        }
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("SimpleDragDropTest: OnEndDrag called!");
        
        if (previewObject != null)
        {
            Debug.Log($"Final position: {previewObject.transform.position}");
            Destroy(previewObject);
            previewObject = null;
        }
    }
}
