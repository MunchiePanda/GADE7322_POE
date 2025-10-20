using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class HealthBarSetupHelper : EditorWindow
{
    private Vector2 scrollPosition;
    private Color healthBarGreenColor = Color.green;
    private Color healthBarYellowColor = Color.yellow;
    private Color healthBarRedColor = Color.red;
    private Vector3 enemyOffset = new Vector3(0, 2.5f, 0);
    private Vector3 defenderOffset = new Vector3(0, 2f, 0);
    private bool hideWhenFull = false;

    [MenuItem("Tools/Health Bar Setup Helper")]
    public static void ShowWindow()
    {
        HealthBarSetupHelper window = GetWindow<HealthBarSetupHelper>("Health Bar Setup");
        window.minSize = new Vector2(400, 600);
    }

    void OnGUI()
    {
        GUILayout.Label("Health Bar Setup Helper", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);

        EditorGUILayout.HelpBox("This tool creates health bar prefabs and automatically adds them to enemies and defenders in your scene.", MessageType.Info);
        EditorGUILayout.Space(10);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        GUILayout.Label("Quick Setup", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        if (GUILayout.Button("SETUP EVERYTHING", GUILayout.Height(50)))
        {
            if (EditorUtility.DisplayDialog("Setup Health Bars", 
                "This will:\n\n" +
                "1. Create health bar prefab\n" +
                "2. Add health bars to all enemy prefabs\n" +
                "3. Add health bars to all defender prefabs\n\n" +
                "Continue?", 
                "Yes", "Cancel"))
            {
                SetupEverything();
            }
        }

        EditorGUILayout.Space(15);
        GUILayout.Label("Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        healthBarGreenColor = EditorGUILayout.ColorField("Full Health Color", healthBarGreenColor);
        healthBarYellowColor = EditorGUILayout.ColorField("Mid Health Color", healthBarYellowColor);
        healthBarRedColor = EditorGUILayout.ColorField("Low Health Color", healthBarRedColor);
        
        EditorGUILayout.Space(5);
        enemyOffset = EditorGUILayout.Vector3Field("Enemy Health Bar Offset", enemyOffset);
        defenderOffset = EditorGUILayout.Vector3Field("Defender Health Bar Offset", defenderOffset);
        hideWhenFull = EditorGUILayout.Toggle("Hide When Full Health", hideWhenFull);

        EditorGUILayout.Space(15);
        GUILayout.Label("Individual Setup Steps", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        if (GUILayout.Button("1. Create Health Bar Prefab", GUILayout.Height(35)))
        {
            CreateHealthBarPrefab();
        }

        if (GUILayout.Button("2. Add Health Bars to Enemy Prefabs", GUILayout.Height(35)))
        {
            AddHealthBarsToEnemyPrefabs();
        }

        if (GUILayout.Button("3. Add Health Bars to Defender Prefabs", GUILayout.Height(35)))
        {
            AddHealthBarsToDefenderPrefabs();
        }

        EditorGUILayout.Space(10);
        GUILayout.Label("Scene Setup", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        if (GUILayout.Button("Add Health Bars to Enemies in Scene", GUILayout.Height(35)))
        {
            AddHealthBarsToSceneEnemies();
        }

        if (GUILayout.Button("Add Health Bars to Defenders in Scene", GUILayout.Height(35)))
        {
            AddHealthBarsToSceneDefenders();
        }

        EditorGUILayout.EndScrollView();
    }

    private void SetupEverything()
    {
        CreateHealthBarPrefab();
        AddHealthBarsToEnemyPrefabs();
        AddHealthBarsToDefenderPrefabs();
        
        Debug.Log("✓ Health Bar setup complete!");
        EditorUtility.DisplayDialog("Success", 
            "Health bar system setup complete!\n\n" +
            "- Health bar prefab created\n" +
            "- Enemy prefabs updated\n" +
            "- Defender prefabs updated", "OK");
    }

    private GameObject CreateHealthBarPrefab()
    {
        GameObject healthBarObj = new GameObject("WorldHealthBar");
        
        WorldHealthBar healthBarScript = healthBarObj.AddComponent<WorldHealthBar>();
        
        GameObject canvasObj = new GameObject("Canvas");
        canvasObj.transform.SetParent(healthBarObj.transform);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10;
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(100, 20);
        canvasRect.localScale = Vector3.one * 0.01f;
        
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(canvasObj.transform);
        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.anchoredPosition = Vector2.zero;
        
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        GameObject sliderObj = new GameObject("HealthSlider");
        sliderObj.transform.SetParent(canvasObj.transform);
        RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
        sliderRect.anchorMin = Vector2.zero;
        sliderRect.anchorMax = Vector2.one;
        sliderRect.sizeDelta = new Vector2(-10, -4);
        sliderRect.anchoredPosition = Vector2.zero;
        
        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;
        slider.transition = Selectable.Transition.None;
        
        GameObject fillAreaObj = new GameObject("Fill Area");
        fillAreaObj.transform.SetParent(sliderObj.transform);
        RectTransform fillAreaRect = fillAreaObj.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = Vector2.zero;
        fillAreaRect.anchoredPosition = Vector2.zero;
        
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillAreaObj.transform);
        RectTransform fillRect = fillObj.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        fillRect.anchoredPosition = Vector2.zero;
        
        Image fillImage = fillObj.AddComponent<Image>();
        fillImage.color = healthBarGreenColor;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        
        slider.fillRect = fillRect;
        
        SerializedObject serializedHealthBar = new SerializedObject(healthBarScript);
        serializedHealthBar.FindProperty("canvas").objectReferenceValue = canvas;
        serializedHealthBar.FindProperty("healthSlider").objectReferenceValue = slider;
        serializedHealthBar.FindProperty("fillImage").objectReferenceValue = fillImage;
        serializedHealthBar.FindProperty("fullHealthColor").colorValue = healthBarGreenColor;
        serializedHealthBar.FindProperty("midHealthColor").colorValue = healthBarYellowColor;
        serializedHealthBar.FindProperty("lowHealthColor").colorValue = healthBarRedColor;
        serializedHealthBar.FindProperty("hideWhenFull").boolValue = hideWhenFull;
        serializedHealthBar.ApplyModifiedProperties();
        
        string prefabPath = "Assets/Prefabs/WorldHealthBar.prefab";
        
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }
        
        GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (existingPrefab != null)
        {
            AssetDatabase.DeleteAsset(prefabPath);
        }
        
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(healthBarObj, prefabPath);
        DestroyImmediate(healthBarObj);
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"✓ Health bar prefab created at {prefabPath}");
        Selection.activeObject = prefab;
        
        return prefab;
    }

    private void AddHealthBarsToEnemyPrefabs()
    {
        GameObject healthBarPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/WorldHealthBar.prefab");
        if (healthBarPrefab == null)
        {
            Debug.LogWarning("Health bar prefab not found. Creating it first.");
            healthBarPrefab = CreateHealthBarPrefab();
        }
        
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" });
        int updatedCount = 0;
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            
            if (prefab == null) continue;
            
            Enemy enemy = prefab.GetComponent<Enemy>();
            if (enemy == null) continue;
            
            GameObject prefabInstance = PrefabUtility.LoadPrefabContents(path);
            
            WorldHealthBar existingHealthBar = prefabInstance.GetComponentInChildren<WorldHealthBar>();
            if (existingHealthBar != null)
            {
                DestroyImmediate(existingHealthBar.gameObject);
            }
            
            GameObject healthBarInstance = (GameObject)PrefabUtility.InstantiatePrefab(healthBarPrefab, prefabInstance.transform);
            WorldHealthBar healthBar = healthBarInstance.GetComponent<WorldHealthBar>();
            healthBar.SetOffset(enemyOffset);
            healthBar.SetHideWhenFull(hideWhenFull);
            
            PrefabUtility.SaveAsPrefabAsset(prefabInstance, path);
            PrefabUtility.UnloadPrefabContents(prefabInstance);
            
            updatedCount++;
            Debug.Log($"✓ Added health bar to enemy prefab: {prefab.name}");
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"✓ Updated {updatedCount} enemy prefabs with health bars");
        
        if (updatedCount > 0)
        {
            EditorUtility.DisplayDialog("Success", $"Added health bars to {updatedCount} enemy prefabs!", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("No Enemies Found", "No enemy prefabs found in Assets/Prefabs folder.", "OK");
        }
    }

    private void AddHealthBarsToDefenderPrefabs()
    {
        GameObject healthBarPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/WorldHealthBar.prefab");
        if (healthBarPrefab == null)
        {
            Debug.LogWarning("Health bar prefab not found. Creating it first.");
            healthBarPrefab = CreateHealthBarPrefab();
        }
        
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" });
        int updatedCount = 0;
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            
            if (prefab == null) continue;
            
            Defender defender = prefab.GetComponent<Defender>();
            if (defender == null) continue;
            
            GameObject prefabInstance = PrefabUtility.LoadPrefabContents(path);
            
            WorldHealthBar existingHealthBar = prefabInstance.GetComponentInChildren<WorldHealthBar>();
            if (existingHealthBar != null)
            {
                DestroyImmediate(existingHealthBar.gameObject);
            }
            
            GameObject healthBarInstance = (GameObject)PrefabUtility.InstantiatePrefab(healthBarPrefab, prefabInstance.transform);
            WorldHealthBar healthBar = healthBarInstance.GetComponent<WorldHealthBar>();
            healthBar.SetOffset(defenderOffset);
            healthBar.SetHideWhenFull(hideWhenFull);
            
            PrefabUtility.SaveAsPrefabAsset(prefabInstance, path);
            PrefabUtility.UnloadPrefabContents(prefabInstance);
            
            updatedCount++;
            Debug.Log($"✓ Added health bar to defender prefab: {prefab.name}");
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"✓ Updated {updatedCount} defender prefabs with health bars");
        
        if (updatedCount > 0)
        {
            EditorUtility.DisplayDialog("Success", $"Added health bars to {updatedCount} defender prefabs!", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("No Defenders Found", "No defender prefabs found in Assets/Prefabs folder.", "OK");
        }
    }

    private void AddHealthBarsToSceneEnemies()
    {
        GameObject healthBarPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/WorldHealthBar.prefab");
        if (healthBarPrefab == null)
        {
            EditorUtility.DisplayDialog("Error", "Health bar prefab not found! Create it first using 'Create Health Bar Prefab' button.", "OK");
            return;
        }
        
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        int addedCount = 0;
        
        foreach (Enemy enemy in enemies)
        {
            WorldHealthBar existingHealthBar = enemy.GetComponentInChildren<WorldHealthBar>();
            if (existingHealthBar != null) continue;
            
            GameObject healthBarInstance = (GameObject)PrefabUtility.InstantiatePrefab(healthBarPrefab, enemy.transform);
            WorldHealthBar healthBar = healthBarInstance.GetComponent<WorldHealthBar>();
            healthBar.Initialize(enemy.transform);
            healthBar.SetOffset(enemyOffset);
            healthBar.SetHideWhenFull(hideWhenFull);
            
            addedCount++;
        }
        
        Debug.Log($"✓ Added health bars to {addedCount} enemies in the scene");
        EditorUtility.DisplayDialog("Success", $"Added health bars to {addedCount} enemies!", "OK");
    }

    private void AddHealthBarsToSceneDefenders()
    {
        GameObject healthBarPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/WorldHealthBar.prefab");
        if (healthBarPrefab == null)
        {
            EditorUtility.DisplayDialog("Error", "Health bar prefab not found! Create it first using 'Create Health Bar Prefab' button.", "OK");
            return;
        }
        
        Defender[] defenders = FindObjectsByType<Defender>(FindObjectsSortMode.None);
        int addedCount = 0;
        
        foreach (Defender defender in defenders)
        {
            WorldHealthBar existingHealthBar = defender.GetComponentInChildren<WorldHealthBar>();
            if (existingHealthBar != null) continue;
            
            GameObject healthBarInstance = (GameObject)PrefabUtility.InstantiatePrefab(healthBarPrefab, defender.transform);
            WorldHealthBar healthBar = healthBarInstance.GetComponent<WorldHealthBar>();
            healthBar.Initialize(defender.transform);
            healthBar.SetOffset(defenderOffset);
            healthBar.SetHideWhenFull(hideWhenFull);
            
            addedCount++;
        }
        
        Debug.Log($"✓ Added health bars to {addedCount} defenders in the scene");
        EditorUtility.DisplayDialog("Success", $"Added health bars to {addedCount} defenders!", "OK");
    }
}
