using UnityEngine;
using UnityEditor;

/// <summary>
/// Custom inspector for the ProceduralMeteorSystem component.
/// Provides quick configuration presets and displays live stats in the Unity Editor.
/// </summary>
[CustomEditor(typeof(ProceduralMeteorSystem))]
public class ProceduralMeteorSystemEditor : Editor
{
    // Serialized properties for all configurable fields in ProceduralMeteorSystem
    private SerializedProperty warningUI;
    private SerializedProperty terrainGenerator;
    private SerializedProperty minMeteorCount;
    private SerializedProperty maxMeteorCount;
    private SerializedProperty baseDamage;
    private SerializedProperty specialDefenderDamageMultiplier;
    private SerializedProperty enableHealingMeteors;
    private SerializedProperty healingMeteorMinSize;
    private SerializedProperty healingMeteorMaxSize;
    private SerializedProperty healingAmount;
    private SerializedProperty maxDisplacement;
    private SerializedProperty displacementFrequency;

    // Controls whether the preset section is expanded in the inspector
    private bool showPresets = true;

    /// <summary>
    /// Cache serialized properties when the inspector is enabled.
    /// </summary>
    void OnEnable()
    {
        warningUI = serializedObject.FindProperty("warningUI");
        terrainGenerator = serializedObject.FindProperty("terrainGenerator");
        minMeteorCount = serializedObject.FindProperty("minMeteorCount");
        maxMeteorCount = serializedObject.FindProperty("maxMeteorCount");
        baseDamage = serializedObject.FindProperty("baseDamage");
        specialDefenderDamageMultiplier = serializedObject.FindProperty("specialDefenderDamageMultiplier");
        enableHealingMeteors = serializedObject.FindProperty("enableHealingMeteors");
        healingMeteorMinSize = serializedObject.FindProperty("healingMeteorMinSize");
        healingMeteorMaxSize = serializedObject.FindProperty("healingMeteorMaxSize");
        healingAmount = serializedObject.FindProperty("healingAmount");
        maxDisplacement = serializedObject.FindProperty("maxDisplacement");
        displacementFrequency = serializedObject.FindProperty("displacementFrequency");
    }

    /// <summary>
    /// Draws the custom inspector GUI, including presets and live stats.
    /// </summary>
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Reference to the target ProceduralMeteorSystem
        ProceduralMeteorSystem system = (ProceduralMeteorSystem)target;

        // Info box describing the system
        EditorGUILayout.Space(10);
        EditorGUILayout.HelpBox("Procedural Meteor System - Creates varied meteor shapes with size-based danger correlation. Special defenders are protected from instant-kill.", MessageType.Info);

        EditorGUILayout.Space(10);

        // Foldout for quick preset buttons
        showPresets = EditorGUILayout.Foldout(showPresets, "Quick Presets", true, EditorStyles.foldoutHeader);
        if (showPresets)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Apply preset configurations:", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // First row of preset buttons
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Balanced (Default)", GUILayout.Height(30)))
            {
                ApplyBalancedPreset();
            }
            if (GUILayout.Button("Intense Danger", GUILayout.Height(30)))
            {
                ApplyIntensePreset();
            }
            EditorGUILayout.EndHorizontal();

            // Second row of preset buttons
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Defender Friendly", GUILayout.Height(30)))
            {
                ApplyFriendlyPreset();
            }
            if (GUILayout.Button("Chaotic Mayhem", GUILayout.Height(30)))
            {
                ApplyChaoticPreset();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.Space(10);

        // Draws the default inspector for all fields
        DrawDefaultInspector();

        EditorGUILayout.Space(10);

        // Display quick stats about the current configuration
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Quick Stats:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"Meteors per wave: {minMeteorCount.intValue} - {maxMeteorCount.intValue}");
        EditorGUILayout.LabelField($"Base damage: {baseDamage.floatValue:F0} HP");
        EditorGUILayout.LabelField($"Special defender protection: {(1f - specialDefenderDamageMultiplier.floatValue) * 100f:F0}% damage reduction");
        EditorGUILayout.LabelField($"Healing meteors: {(enableHealingMeteors.boolValue ? "Enabled" : "Disabled")}");
        if (enableHealingMeteors.boolValue)
        {
            EditorGUILayout.LabelField($"  └─ Healing amount: {healingAmount.floatValue:F0} HP");
        }
        EditorGUILayout.LabelField($"Max surface roughness: {maxDisplacement.floatValue * 100f:F0}%");
        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();
    }

    /// <summary>
    /// Applies a balanced preset configuration to the system.
    /// </summary>
    void ApplyBalancedPreset()
    {
        minMeteorCount.intValue = 2;
        maxMeteorCount.intValue = 8;
        baseDamage.floatValue = 40f;
        specialDefenderDamageMultiplier.floatValue = 0.6f;
        enableHealingMeteors.boolValue = true;
        healingMeteorMinSize.floatValue = 0.3f;
        healingMeteorMaxSize.floatValue = 0.8f;
        healingAmount.floatValue = 25f;
        maxDisplacement.floatValue = 0.5f;
        displacementFrequency.floatValue = 8f;

        serializedObject.ApplyModifiedProperties();
        EditorUtility.DisplayDialog("Preset Applied", "Balanced preset applied successfully!\n\nModerate danger with healing support and special defender protection.", "OK");
    }

    /// <summary>
    /// Applies an intense danger preset configuration to the system.
    /// </summary>
    void ApplyIntensePreset()
    {
        minMeteorCount.intValue = 4;
        maxMeteorCount.intValue = 12;
        baseDamage.floatValue = 60f;
        specialDefenderDamageMultiplier.floatValue = 0.8f;
        enableHealingMeteors.boolValue = false;
        maxDisplacement.floatValue = 0.7f;
        displacementFrequency.floatValue = 12f;

        serializedObject.ApplyModifiedProperties();
        EditorUtility.DisplayDialog("Preset Applied", "Intense Danger preset applied!\n\nMore meteors, higher damage, no healing. Special defenders still protected but less.", "OK");
    }

    /// <summary>
    /// Applies a defender-friendly preset configuration to the system.
    /// </summary>
    void ApplyFriendlyPreset()
    {
        minMeteorCount.intValue = 1;
        maxMeteorCount.intValue = 5;
        baseDamage.floatValue = 25f;
        specialDefenderDamageMultiplier.floatValue = 0.4f;
        enableHealingMeteors.boolValue = true;
        healingMeteorMinSize.floatValue = 0.3f;
        healingMeteorMaxSize.floatValue = 1.2f;
        healingAmount.floatValue = 40f;
        maxDisplacement.floatValue = 0.3f;
        displacementFrequency.floatValue = 6f;

        serializedObject.ApplyModifiedProperties();
        EditorUtility.DisplayDialog("Preset Applied", "Defender Friendly preset applied!\n\nLower damage, more healing, strong special defender protection. Great for testing or casual play.", "OK");
    }

    /// <summary>
    /// Applies a chaotic mayhem preset configuration to the system.
    /// </summary>
    void ApplyChaoticPreset()
    {
        minMeteorCount.intValue = 3;
        maxMeteorCount.intValue = 10;
        baseDamage.floatValue = 50f;
        specialDefenderDamageMultiplier.floatValue = 0.7f;
        enableHealingMeteors.boolValue = true;
        healingMeteorMinSize.floatValue = 0.3f;
        healingMeteorMaxSize.floatValue = 0.8f;
        healingAmount.floatValue = 30f;
        maxDisplacement.floatValue = 0.8f;
        displacementFrequency.floatValue = 15f;

        serializedObject.ApplyModifiedProperties();
        EditorUtility.DisplayDialog("Preset Applied", "Chaotic Mayhem preset applied!\n\nExtremely rough meteors, unpredictable healing, high visual variety. Maximum chaos!", "OK");
    }
}
