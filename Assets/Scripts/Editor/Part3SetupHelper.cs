using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

/// <summary>
/// All-in-one setup helper for Part 3 requirements.
/// Automatically configures upgrade system, hazard system, VFX, and post-processing.
/// </summary>
public class Part3SetupHelper : EditorWindow
{
    private Vector2 scrollPosition;

    [MenuItem("Tools/Part 3 Setup Helper")]
    public static void ShowWindow()
    {
        Part3SetupHelper window = GetWindow<Part3SetupHelper>("Part 3 Setup");
        window.minSize = new Vector2(400, 500);
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        GUILayout.Label("Part 3 - Complete Scene Setup", EditorStyles.boldLabel);
        GUILayout.Label("Automatically setup all Part 3 requirements", EditorStyles.miniLabel);
        EditorGUILayout.Space(10);

        EditorGUILayout.HelpBox("This will setup:\n• Upgrade System\n• Procedural Hazard System\n• Hazard Prefabs\n• Post-Processing VFX\n• Upgrade UI", MessageType.Info);
        EditorGUILayout.Space(10);

        if (GUILayout.Button("SETUP EVERYTHING", GUILayout.Height(50)))
        {
            if (EditorUtility.DisplayDialog("Setup Part 3", 
                "This will create/modify systems in your scene. Continue?", "Yes", "Cancel"))
            {
                SetupEverything();
            }
        }

        EditorGUILayout.Space(20);
        GUILayout.Label("Individual Setup Options", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        if (GUILayout.Button("1. Setup Upgrade System", GUILayout.Height(35)))
        {
            SetupUpgradeSystem();
        }

        if (GUILayout.Button("2. Create Hazard Prefabs", GUILayout.Height(35)))
        {
            CreateHazardPrefabs();
        }

        if (GUILayout.Button("3. Setup Procedural Hazard System", GUILayout.Height(35)))
        {
            SetupProceduralHazardSystem();
        }

        if (GUILayout.Button("4. Setup Post-Processing", GUILayout.Height(35)))
        {
            SetupPostProcessing();
        }

        if (GUILayout.Button("5. Create Upgrade VFX Prefab", GUILayout.Height(35)))
        {
            CreateUpgradeVFXPrefab();
        }

        EditorGUILayout.Space(10);
        GUILayout.Label("UI Only Setup", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        if (GUILayout.Button("Create Upgrade UI Panel Only", GUILayout.Height(35)))
        {
            CreateUpgradeUIPanelOnly();
        }

        EditorGUILayout.EndScrollView();
    }

    private void CreateUpgradeUIPanelOnly()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No Canvas found! Create a Canvas first: GameObject > UI > Canvas");
            EditorUtility.DisplayDialog("Error", "No Canvas found in the scene!\n\nPlease create a Canvas first:\nGameObject > UI > Canvas", "OK");
            return;
        }

        GameObject existingUI = GameObject.Find("UpgradeUI");
        if (existingUI != null)
        {
            if (!EditorUtility.DisplayDialog("UI Already Exists", 
                "An UpgradeUI already exists. Do you want to delete it and create a new one?", 
                "Yes, Replace", "Cancel"))
            {
                return;
            }
            DestroyImmediate(existingUI);
        }

        UpgradeUI upgradeUI = CreateUpgradeUIComplete();
        
        if (upgradeUI != null)
        {
            UpgradeSystem upgradeSystem = FindFirstObjectByType<UpgradeSystem>();
            GameManager gameManager = FindFirstObjectByType<GameManager>();
            
            if (upgradeSystem != null)
            {
                upgradeUI.upgradeSystem = upgradeSystem;
                upgradeSystem.upgradeUI = upgradeUI;
                EditorUtility.SetDirty(upgradeSystem);
            }
            
            if (gameManager != null)
            {
                upgradeUI.gameManager = gameManager;
            }
            
            EditorUtility.SetDirty(upgradeUI);
            Selection.activeGameObject = upgradeUI.gameObject;
            
            Debug.Log("✓ Upgrade UI Panel created successfully!");
            EditorUtility.DisplayDialog("Success", 
                "Upgrade UI Panel created!\n\nLocation: Canvas > UpgradeUI\n\nThe panel is on the right side of the screen.\nClick the UPGRADES button in Play mode to toggle it.", "OK");
        };
    }

    private void SetupEverything()
    {
        Debug.Log("=== Starting Part 3 Complete Setup ===");
        
        SetupUpgradeSystem();
        CreateHazardPrefabs();
        SetupProceduralHazardSystem();
        SetupPostProcessing();
        CreateUpgradeVFXPrefab();
        
        Debug.Log("=== Part 3 Setup Complete! ===");
        EditorUtility.DisplayDialog("Setup Complete", 
            "All systems have been setup!\n\nNext steps:\n1. Assign UI elements in UpgradeUI inspector\n2. Customize post-processing settings\n3. Test in Play mode", "OK");
    }

    private void SetupUpgradeSystem()
    {
        GameObject upgradeSystemObj = GameObject.Find("UpgradeSystem");
        if (upgradeSystemObj == null)
        {
            upgradeSystemObj = new GameObject("UpgradeSystem");
            Undo.RegisterCreatedObjectUndo(upgradeSystemObj, "Create Upgrade System");
        }

        UpgradeSystem upgradeSystem = upgradeSystemObj.GetComponent<UpgradeSystem>();
        if (upgradeSystem == null)
        {
            upgradeSystem = upgradeSystemObj.AddComponent<UpgradeSystem>();
        }

        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            upgradeSystem.gameManager = gameManager;
        }

        UpgradeUI upgradeUI = FindFirstObjectByType<UpgradeUI>();
        if (upgradeUI == null)
        {
            upgradeUI = CreateUpgradeUIComplete();
        }

        if (upgradeUI != null)
        {
            upgradeSystem.upgradeUI = upgradeUI;
            upgradeUI.upgradeSystem = upgradeSystem;
            upgradeUI.gameManager = gameManager;
        }

        EditorUtility.SetDirty(upgradeSystem);
        Debug.Log("✓ Upgrade System setup complete!");
    }

    private UpgradeUI CreateUpgradeUIComplete()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No Canvas found in scene! Please add a Canvas first.");
            return null;
        }

        GameObject upgradeUIRoot = new GameObject("UpgradeUI");
        upgradeUIRoot.transform.SetParent(canvas.transform);
        RectTransform rootRect = upgradeUIRoot.AddComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(1, 0.5f);
        rootRect.anchorMax = new Vector2(1, 0.5f);
        rootRect.pivot = new Vector2(1, 0.5f);
        rootRect.anchoredPosition = new Vector2(-20, 0);

        GameObject toggleButton = CreateButton("ToggleUpgradeButton", upgradeUIRoot.transform, "UPGRADES", new Vector2(150, 50), new Vector2(0, 0));

        GameObject upgradePanel = new GameObject("UpgradePanel");
        upgradePanel.transform.SetParent(upgradeUIRoot.transform);
        RectTransform panelRect = upgradePanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1, 0.5f);
        panelRect.anchorMax = new Vector2(1, 0.5f);
        panelRect.pivot = new Vector2(1, 0.5f);
        panelRect.anchoredPosition = new Vector2(-170, 0);
        panelRect.sizeDelta = new Vector2(350, 500);
        
        Image panelImage = upgradePanel.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);

        GameObject titleText = CreateText("TitleText", upgradePanel.transform, "UPGRADE SYSTEM", 20, new Vector2(0, 230), new Vector2(300, 40));
        titleText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        titleText.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        GameObject globalLabel = CreateText("GlobalLabel", upgradePanel.transform, "Global Upgrades", 16, new Vector2(0, 190), new Vector2(300, 30));
        globalLabel.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        globalLabel.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        GameObject healthBtn = CreateUpgradeButton("GlobalHealthButton", upgradePanel.transform, "Health", new Vector2(0, 150));
        GameObject damageBtn = CreateUpgradeButton("GlobalDamageButton", upgradePanel.transform, "Damage", new Vector2(0, 100));
        GameObject attackBtn = CreateUpgradeButton("GlobalAttackSpeedButton", upgradePanel.transform, "Attack Speed", new Vector2(0, 50));

        GameObject healthCost = CreateText("HealthCostText", healthBtn.transform, "Cost: 100", 12, new Vector2(100, 0), new Vector2(80, 30));
        GameObject damageCost = CreateText("DamageCostText", damageBtn.transform, "Cost: 100", 12, new Vector2(100, 0), new Vector2(80, 30));
        GameObject attackCost = CreateText("AttackSpeedCostText", attackBtn.transform, "Cost: 100", 12, new Vector2(100, 0), new Vector2(80, 30));

        GameObject healthLevel = CreateText("HealthLevelText", healthBtn.transform, "Lv: 0", 12, new Vector2(-100, 0), new Vector2(60, 30));
        GameObject damageLevel = CreateText("DamageLevelText", damageBtn.transform, "Lv: 0", 12, new Vector2(-100, 0), new Vector2(60, 30));
        GameObject attackLevel = CreateText("AttackSpeedLevelText", attackBtn.transform, "Lv: 0", 12, new Vector2(-100, 0), new Vector2(60, 30));

        GameObject individualLabel = CreateText("IndividualLabel", upgradePanel.transform, "Individual Upgrades", 16, new Vector2(0, 0), new Vector2(300, 30));
        individualLabel.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        individualLabel.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;

        GameObject selectedText = CreateText("SelectedUnitText", upgradePanel.transform, "Select a unit...", 14, new Vector2(0, -30), new Vector2(300, 30));
        selectedText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
        selectedText.GetComponent<TextMeshProUGUI>().color = Color.yellow;

        GameObject indHealthBtn = CreateUpgradeButton("IndividualHealthButton", upgradePanel.transform, "Health+", new Vector2(0, -70));
        GameObject indDamageBtn = CreateUpgradeButton("IndividualDamageButton", upgradePanel.transform, "Damage+", new Vector2(0, -120));
        GameObject indAttackBtn = CreateUpgradeButton("IndividualAttackSpeedButton", upgradePanel.transform, "Attack+", new Vector2(0, -170));

        GameObject indHealthCost = CreateText("IndHealthCostText", indHealthBtn.transform, "Cost: 50", 12, new Vector2(100, 0), new Vector2(80, 30));
        GameObject indDamageCost = CreateText("IndDamageCostText", indDamageBtn.transform, "Cost: 50", 12, new Vector2(100, 0), new Vector2(80, 30));
        GameObject indAttackCost = CreateText("IndAttackCostText", indAttackBtn.transform, "Cost: 50", 12, new Vector2(100, 0), new Vector2(80, 30));

        upgradePanel.SetActive(false);

        UpgradeUI upgradeUI = upgradeUIRoot.AddComponent<UpgradeUI>();
        upgradeUI.toggleUpgradeButton = toggleButton.GetComponent<Button>();
        upgradeUI.upgradePanel = upgradePanel;
        upgradeUI.globalHealthButton = healthBtn.GetComponent<Button>();
        upgradeUI.globalDamageButton = damageBtn.GetComponent<Button>();
        upgradeUI.globalAttackSpeedButton = attackBtn.GetComponent<Button>();
        upgradeUI.individualHealthButton = indHealthBtn.GetComponent<Button>();
        upgradeUI.individualDamageButton = indDamageBtn.GetComponent<Button>();
        upgradeUI.individualAttackSpeedButton = indAttackBtn.GetComponent<Button>();
        upgradeUI.globalHealthCostText = healthCost.GetComponent<TextMeshProUGUI>();
        upgradeUI.globalDamageCostText = damageCost.GetComponent<TextMeshProUGUI>();
        upgradeUI.globalAttackSpeedCostText = attackCost.GetComponent<TextMeshProUGUI>();
        upgradeUI.globalHealthLevelText = healthLevel.GetComponent<TextMeshProUGUI>();
        upgradeUI.globalDamageLevelText = damageLevel.GetComponent<TextMeshProUGUI>();
        upgradeUI.globalAttackSpeedLevelText = attackLevel.GetComponent<TextMeshProUGUI>();
        upgradeUI.individualHealthCostText = indHealthCost.GetComponent<TextMeshProUGUI>();
        upgradeUI.individualDamageCostText = indDamageCost.GetComponent<TextMeshProUGUI>();
        upgradeUI.individualAttackSpeedCostText = indAttackCost.GetComponent<TextMeshProUGUI>();
        upgradeUI.selectedUnitInfoText = selectedText.GetComponent<TextMeshProUGUI>();

        Debug.Log("✓ Upgrade UI created with all elements!");
        return upgradeUI;
    }

    private GameObject CreateButton(string name, Transform parent, string buttonText, Vector2 size, Vector2 position)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent);
        RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
        rectTransform.sizeDelta = size;
        rectTransform.anchoredPosition = position;

        Image image = buttonObj.AddComponent<Image>();
        image.color = new Color(0.2f, 0.6f, 0.8f, 1f);

        Button button = buttonObj.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.2f, 0.6f, 0.8f, 1f);
        colors.highlightedColor = new Color(0.3f, 0.7f, 0.9f, 1f);
        colors.pressedColor = new Color(0.15f, 0.5f, 0.7f, 1f);
        colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        button.colors = colors;

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;

        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = buttonText;
        text.fontSize = 16;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.fontStyle = FontStyles.Bold;

        return buttonObj;
    }

    private GameObject CreateUpgradeButton(string name, Transform parent, string buttonText, Vector2 position)
    {
        return CreateButton(name, parent, buttonText, new Vector2(280, 40), position);
    }

    private GameObject CreateText(string name, Transform parent, string textContent, int fontSize, Vector2 position, Vector2 size)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent);
        RectTransform rectTransform = textObj.AddComponent<RectTransform>();
        rectTransform.sizeDelta = size;
        rectTransform.anchoredPosition = position;

        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = textContent;
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Left;

        return textObj;
    }

    private void CreateHazardPrefabs()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs/Hazards"))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }
            AssetDatabase.CreateFolder("Assets/Prefabs", "Hazards");
        }

        CreateLavaPoolPrefab();
        CreateIcePatchPrefab();
        CreateWindZonePrefab();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("✓ All hazard prefabs created!");
    }

    private void CreateLavaPoolPrefab()
    {
        GameObject lavaPool = new GameObject("LavaPoolHazard");
        
        LavaPoolHazard hazard = lavaPool.AddComponent<LavaPoolHazard>();
        hazard.hazardType = HazardType.Lava;
        hazard.damagePerSecond = 10f;
        hazard.effectRadius = 3f;
        hazard.duration = 60f;
        
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        visual.name = "Visual";
        visual.transform.SetParent(lavaPool.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = new Vector3(3f, 0.1f, 3f);
        
        Shader hazardShader = Shader.Find("Custom/HazardDistortion");
        if (hazardShader != null)
        {
            Material lavaMat = new Material(hazardShader);
            lavaMat.name = "LavaHazardMat";
            lavaMat.SetColor("_Color", new Color(1f, 0.3f, 0f));
            lavaMat.SetFloat("_HazardType", 0f);
            lavaMat.SetFloat("_DistortionAmount", 0.5f);
            lavaMat.SetFloat("_DistortionSpeed", 2f);
            visual.GetComponent<Renderer>().material = lavaMat;
            
            AssetDatabase.CreateAsset(lavaMat, "Assets/Materials/LavaHazardMat.mat");
            hazard.hazardMaterial = lavaMat;
        }
        
        SphereCollider trigger = lavaPool.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = 1.5f;
        
        ParticleSystem particles = lavaPool.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.startColor = new Color(1f, 0.5f, 0f);
        main.startSize = 0.3f;
        main.startLifetime = 2f;
        
        PrefabUtility.SaveAsPrefabAsset(lavaPool, "Assets/Prefabs/Hazards/LavaPoolHazard.prefab");
        DestroyImmediate(lavaPool);
    }

    private void CreateIcePatchPrefab()
    {
        GameObject icePool = new GameObject("IcePatchHazard");
        
        IcePatchHazard hazard = icePool.AddComponent<IcePatchHazard>();
        hazard.hazardType = HazardType.Ice;
        hazard.speedReduction = 0.5f;
        hazard.freezeChance = 0.15f;
        hazard.freezeDuration = 1f;
        hazard.effectRadius = 3f;
        hazard.duration = 60f;
        
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        visual.name = "Visual";
        visual.transform.SetParent(icePool.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = new Vector3(3f, 0.1f, 3f);
        
        Shader hazardShader = Shader.Find("Custom/HazardDistortion");
        if (hazardShader != null)
        {
            Material iceMat = new Material(hazardShader);
            iceMat.name = "IceHazardMat";
            iceMat.SetColor("_Color", new Color(0.3f, 0.7f, 1f));
            iceMat.SetFloat("_HazardType", 1f);
            iceMat.SetFloat("_DistortionAmount", 0.3f);
            iceMat.SetFloat("_DistortionSpeed", 0.5f);
            visual.GetComponent<Renderer>().material = iceMat;
            
            AssetDatabase.CreateAsset(iceMat, "Assets/Materials/IceHazardMat.mat");
            hazard.hazardMaterial = iceMat;
        }
        
        SphereCollider trigger = icePool.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = 1.5f;
        
        PrefabUtility.SaveAsPrefabAsset(icePool, "Assets/Prefabs/Hazards/IcePatchHazard.prefab");
        DestroyImmediate(icePool);
    }

    private void CreateWindZonePrefab()
    {
        GameObject windZone = new GameObject("WindZoneHazard");
        
        WindZoneHazard hazard = windZone.AddComponent<WindZoneHazard>();
        hazard.hazardType = HazardType.Wind;
        hazard.windForce = 15f;
        hazard.directionChangeInterval = 3f;
        hazard.projectileDeflection = 2f;
        hazard.effectRadius = 4f;
        hazard.duration = 60f;
        
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.name = "Visual";
        visual.transform.SetParent(windZone.transform);
        visual.transform.localPosition = new Vector3(0, 1f, 0);
        visual.transform.localScale = new Vector3(2f, 2f, 2f);
        
        Shader hazardShader = Shader.Find("Custom/HazardDistortion");
        if (hazardShader != null)
        {
            Material windMat = new Material(hazardShader);
            windMat.name = "WindHazardMat";
            windMat.SetColor("_Color", new Color(0.8f, 0.8f, 0.9f, 0.5f));
            windMat.SetFloat("_HazardType", 2f);
            windMat.SetFloat("_DistortionAmount", 0.7f);
            windMat.SetFloat("_DistortionSpeed", 3f);
            visual.GetComponent<Renderer>().material = windMat;
            
            AssetDatabase.CreateAsset(windMat, "Assets/Materials/WindHazardMat.mat");
            hazard.hazardMaterial = windMat;
        }
        
        BoxCollider trigger = windZone.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.center = new Vector3(0, 1f, 0);
        trigger.size = new Vector3(2f, 2f, 2f);
        
        PrefabUtility.SaveAsPrefabAsset(windZone, "Assets/Prefabs/Hazards/WindZoneHazard.prefab");
        DestroyImmediate(windZone);
    }

    private void SetupProceduralHazardSystem()
    {
        GameObject hazardSystemObj = GameObject.Find("ProceduralHazardSystem");
        if (hazardSystemObj == null)
        {
            hazardSystemObj = new GameObject("ProceduralHazardSystem");
            Undo.RegisterCreatedObjectUndo(hazardSystemObj, "Create Procedural Hazard System");
        }

        ProceduralHazardSystem hazardSystem = hazardSystemObj.GetComponent<ProceduralHazardSystem>();
        if (hazardSystem == null)
        {
            hazardSystem = hazardSystemObj.AddComponent<ProceduralHazardSystem>();
        }

        VoxelTerrainGenerator terrainGen = FindFirstObjectByType<VoxelTerrainGenerator>();
        if (terrainGen != null)
        {
            hazardSystem.terrainGenerator = terrainGen;
        }

        GameObject lavaPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Hazards/LavaPoolHazard.prefab");
        GameObject icePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Hazards/IcePatchHazard.prefab");
        GameObject windPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Hazards/WindZoneHazard.prefab");

        if (lavaPrefab != null) hazardSystem.lavaPoolPrefab = lavaPrefab;
        if (icePrefab != null) hazardSystem.icePatchPrefab = icePrefab;
        if (windPrefab != null) hazardSystem.windZonePrefab = windPrefab;

        Material hazardMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/LavaHazardMat.mat");
        if (hazardMat != null)
        {
            hazardSystem.hazardDistortionMaterial = hazardMat;
        }

        EditorUtility.SetDirty(hazardSystem);
        Debug.Log("✓ Procedural Hazard System setup complete!");
    }

    private void SetupPostProcessing()
    {
        GameObject volumeObj = GameObject.Find("Global Volume");
        if (volumeObj == null)
        {
            volumeObj = new GameObject("Global Volume");
            Undo.RegisterCreatedObjectUndo(volumeObj, "Create Global Volume");
        }

        Volume volume = volumeObj.GetComponent<Volume>();
        if (volume == null)
        {
            volume = volumeObj.AddComponent<Volume>();
        }

        volume.isGlobal = true;
        volume.priority = 1;

        if (!AssetDatabase.IsValidFolder("Assets/Settings"))
        {
            AssetDatabase.CreateFolder("Assets", "Settings");
        }

        string profilePath = "Assets/Settings/CustomPostProcessProfile.asset";
        VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);
        
        if (profile == null)
        {
            profile = ScriptableObject.CreateInstance<VolumeProfile>();
            
            Bloom bloom = profile.Add<Bloom>();
            bloom.intensity.value = 0.3f;
            bloom.threshold.value = 0.9f;
            bloom.active = true;
            
            ColorAdjustments colorAdj = profile.Add<ColorAdjustments>();
            colorAdj.saturation.value = 10f;
            colorAdj.active = true;
            
            Vignette vignette = profile.Add<Vignette>();
            vignette.intensity.value = 0.25f;
            vignette.smoothness.value = 0.4f;
            vignette.active = true;
            
            AssetDatabase.CreateAsset(profile, profilePath);
            AssetDatabase.SaveAssets();
            Debug.Log("✓ Created new custom post-processing profile at: " + profilePath);
        }

        volume.profile = profile;
        EditorUtility.SetDirty(volume);
        Debug.Log("✓ Post-Processing setup complete!");
    }

    private void CreateUpgradeVFXPrefab()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        GameObject vfxObj = new GameObject("UpgradeVFX");
        
        ParticleSystem ps = vfxObj.AddComponent<ParticleSystem>();
        
        var main = ps.main;
        main.duration = 1.5f;
        main.startLifetime = 1.5f;
        main.startSpeed = 5f;
        main.startSize = 0.5f;
        main.startColor = new Color(0f, 1f, 1f, 1f);
        main.maxParticles = 50;
        main.loop = false;
        main.stopAction = ParticleSystemStopAction.Destroy;
        
        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0.0f, 30)
        });
        
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 1f;
        
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(Color.cyan, 0.0f), 
                new GradientColorKey(Color.blue, 1.0f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1.0f, 0.0f), 
                new GradientAlphaKey(0.0f, 1.0f) 
            }
        );
        colorOverLifetime.color = gradient;
        
        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
        
        var renderer = vfxObj.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        
        PrefabUtility.SaveAsPrefabAsset(vfxObj, "Assets/Prefabs/UpgradeVFX.prefab");
        DestroyImmediate(vfxObj);
        
        Debug.Log("✓ Upgrade VFX prefab created!");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
