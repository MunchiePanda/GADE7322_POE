using UnityEngine;
using UnityEditor;

/// <summary>
/// Custom editor for HealthBarSetupGuide to provide a safe setup button
/// </summary>
[CustomEditor(typeof(HealthBarSetupGuide))]
public class HealthBarSetupGuideEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        HealthBarSetupGuide setupGuide = (HealthBarSetupGuide)target;
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Health Bar Setup", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Create Health Bar for This Unit"))
        {
            setupGuide.CreateHealthBarForUnit();
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("This will create a health bar that faces the camera and follows this unit.", MessageType.Info);
    }
}
