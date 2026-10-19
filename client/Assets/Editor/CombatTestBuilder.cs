using System;
using System.Collections.Generic;
using System.Linq;
using EmberBlade.AI;
using EmberBlade.Combat;
using EmberBlade.Config;
using EmberBlade.Player;
using EmberBlade.UI;
using TMPro;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class CombatTestBuilder
{
    private const string ScenePath = "Assets/Scenes/CombatTest.unity";
    private const string PrefabFolder = "Assets/Prefabs/Combat";
    private const string ConfigPath = "Assets/Resources/GameConfig.asset";

    [MenuItem("Ember Blade/Build Combat Test Scene")]
    public static void BuildFromMenu()
    {
        Build();
    }

    public static void BuildFromCommandLine()
    {
        Build();
    }

    private static void Build()
    {
        EnsureFolder("Assets/Scenes");
        EnsureFolder("Assets/Prefabs");
        EnsureFolder(PrefabFolder);
        EnsureFolder("Assets/Config");
        EnsureFolder("Assets/Resources");

        GameConfig config = GetOrCreateConfig();
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        EditorSceneManager.SaveScene(scene, ScenePath);

        CreateLighting();
        GameObject ground = CreateGround();
        NavMeshSurface surface = ground.AddComponent<NavMeshSurface>();
        surface.collectObjects = CollectObjects.All;
        surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;

        GameObject player = CreatePlayer(config);
        ThirdPersonCamera followCamera = CreateFollowCamera(config, player.transform);
        AssignObject(player.GetComponent<PlayerController>(), "followCamera", followCamera);
        GameObject scoutPrefab = CreateEnemyPrefab("SilverholdScout", config, false, Color.gray);
        GameObject skirmisherPrefab = CreateEnemyPrefab("SilverholdSkirmisher", config, true, new Color(0.4f, 0.6f, 0.9f));

        surface.BuildNavMesh();
        EnemyWaveManager waveManager = CreateWaveManager();
        CombatHudController hud = CreateCombatHud();
        CreateEventSystem();
        CombatSceneBootstrap bootstrap = CreateBootstrap(
            config,
            player,
            followCamera,
            waveManager,
            hud,
            scoutPrefab,
            skirmisherPrefab);

        AssignObject(waveManager, "config", config);
        AssignObject(waveManager, "player", player.transform);
        AssignObject(waveManager, "playerCombat", player.GetComponent<PlayerCombat>());
        AssignObject(waveManager, "scoutPrefab", scoutPrefab);
        AssignObject(waveManager, "skirmisherPrefab", skirmisherPrefab);
        AssignObject(waveManager, "spawnRoot", waveManager.transform);
        AssignObject(bootstrap, "config", config);
        AssignObject(bootstrap, "player", player);
        AssignObject(bootstrap, "followCamera", followCamera);
        AssignObject(bootstrap, "enemyWaveManager", waveManager);
        AssignObject(bootstrap, "combatHud", hud);
        AssignObject(bootstrap, "scoutPrefab", scoutPrefab);
        AssignObject(bootstrap, "skirmisherPrefab", skirmisherPrefab);
        VerifyObjectField(bootstrap, "player");
        VerifyObjectField(bootstrap, "followCamera");
        VerifyObjectField(bootstrap, "enemyWaveManager");
        VerifyObjectField(bootstrap, "combatHud");
        VerifyObjectField(bootstrap, "scoutPrefab");
        VerifyObjectField(bootstrap, "skirmisherPrefab");

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AddSceneToBuildSettings(ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Ember Blade Combat Test scene built at {ScenePath}.");
    }

    private static GameConfig GetOrCreateConfig()
    {
        GameConfig config = AssetDatabase.LoadAssetAtPath<GameConfig>(ConfigPath);
        if (config == null)
        {
            string legacyPath = "Assets/Config/GameConfig.asset";
            config = AssetDatabase.LoadAssetAtPath<GameConfig>(legacyPath);
            if (config != null)
            {
                AssetDatabase.MoveAsset(legacyPath, ConfigPath);
                config = AssetDatabase.LoadAssetAtPath<GameConfig>(ConfigPath);
            }
        }
        if (config == null)
        {
            GameConfig created = ScriptableObject.CreateInstance<GameConfig>();
            AssetDatabase.CreateAsset(created, ConfigPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(ConfigPath);
            config = AssetDatabase.LoadAssetAtPath<GameConfig>(ConfigPath);
        }
        return config;
    }

    private static void CreateLighting()
    {
        GameObject lightObject = new GameObject("Directional Light");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1f;
        lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
    }

    private static GameObject CreateGround()
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.localScale = new Vector3(5f, 1f, 5f);
        ground.GetComponent<Renderer>().sharedMaterial = GetMaterial(
            "CombatGroundMaterial",
            new Color(0.18f, 0.22f, 0.18f));
        return ground;
    }

    private static GameObject CreatePlayer(GameConfig config)
    {
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.transform.position = new Vector3(0f, 1f, 0f);
        player.GetComponent<Renderer>().sharedMaterial = GetMaterial(
            "PlayerMaterial",
            new Color(0.2f, 0.45f, 0.9f));
        UnityEngine.Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());
        CharacterController controller = player.AddComponent<CharacterController>();
        controller.center = Vector3.up;
        controller.height = 2f;
        Health health = player.AddComponent<Health>();
        EmberMeter ember = player.AddComponent<EmberMeter>();
        PlayerController movement = player.AddComponent<PlayerController>();
        PlayerCombat combat = player.AddComponent<PlayerCombat>();
        AssignInt(health, "maxHealth", config.playerMaxHealth);
        AssignObject(ember, "config", config);
        AssignObject(movement, "config", config);
        AssignObject(combat, "config", config);
        AssignObject(combat, "health", health);
        AssignObject(combat, "emberMeter", ember);
        return player;
    }

    private static ThirdPersonCamera CreateFollowCamera(GameConfig config, Transform target)
    {
        GameObject cameraObject = new GameObject("Follow Camera");
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.Skybox;
        cameraObject.AddComponent<AudioListener>();
        ThirdPersonCamera followCamera = cameraObject.AddComponent<ThirdPersonCamera>();
        AssignObject(followCamera, "config", config);
        AssignObject(followCamera, "target", target);
        return followCamera;
    }

    private static GameObject CreateEnemyPrefab(
        string name,
        GameConfig config,
        bool skirmisher,
        Color color)
    {
        string path = $"{PrefabFolder}/{name}.prefab";
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null)
        {
            return existing;
        }

        GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        enemy.name = name;
        enemy.transform.position = new Vector3(0f, 1f, 0f);
        enemy.GetComponent<Renderer>().sharedMaterial = GetMaterial($"{name}Material", color);
        UnityEngine.Object.DestroyImmediate(enemy.GetComponent<CapsuleCollider>());
        enemy.AddComponent<NavMeshAgent>();
        Health health = enemy.AddComponent<Health>();
        EnemyController controller = enemy.AddComponent<EnemyController>();
        AssignInt(health, "maxHealth", skirmisher ? config.skirmisherHealth : config.scoutHealth);
        AssignObject(controller, "config", config);
        AssignBool(controller, "isSkirmisher", skirmisher);
        AssignObject(controller, "health", health);
        AssignObject(controller, "bodyRenderer", enemy.GetComponent<Renderer>());
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(enemy, path);
        UnityEngine.Object.DestroyImmediate(enemy);
        return prefab;
    }

    private static EnemyWaveManager CreateWaveManager()
    {
        GameObject managerObject = new GameObject("EnemyWaveManager");
        return managerObject.AddComponent<EnemyWaveManager>();
    }

    private static CombatSceneBootstrap CreateBootstrap(
        GameConfig config,
        GameObject player,
        ThirdPersonCamera followCamera,
        EnemyWaveManager waveManager,
        CombatHudController hud,
        GameObject scoutPrefab,
        GameObject skirmisherPrefab)
    {
        return new GameObject("CombatBootstrap").AddComponent<CombatSceneBootstrap>();
    }

    private static CombatHudController CreateCombatHud()
    {
        GameObject canvasObject = new GameObject(
            "CombatHUD",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        TMP_Text healthLabel = CreateText(canvasObject.transform, "HealthLabel", "Health", 24f);
        SetRect(healthLabel.rectTransform, new Vector2(0.03f, 0.91f), new Vector2(0.2f, 0.97f));
        Slider healthBar = CreateBar(canvasObject.transform, "HealthBar", new Color(0.8f, 0.1f, 0.1f));
        SetRect(healthBar.GetComponent<RectTransform>(), new Vector2(0.03f, 0.86f), new Vector2(0.25f, 0.91f));

        TMP_Text emberLabel = CreateText(canvasObject.transform, "EmberText", "Ember: 0/100", 24f);
        SetRect(emberLabel.rectTransform, new Vector2(0.03f, 0.78f), new Vector2(0.3f, 0.84f));
        Slider emberBar = CreateBar(canvasObject.transform, "EmberBar", new Color(0.95f, 0.2f, 0.05f));
        SetRect(emberBar.GetComponent<RectTransform>(), new Vector2(0.03f, 0.73f), new Vector2(0.25f, 0.78f));

        TMP_Text enemies = CreateText(canvasObject.transform, "EnemiesText", "Enemies remaining: 0", 24f);
        SetRect(enemies.rectTransform, new Vector2(0.72f, 0.9f), new Vector2(0.97f, 0.97f));

        CombatHudController hud = canvasObject.AddComponent<CombatHudController>();
        AssignObject(hud, "healthBar", healthBar);
        AssignObject(hud, "emberBar", emberBar);
        AssignObject(hud, "emberText", emberLabel);
        AssignObject(hud, "enemiesText", enemies);
        return hud;
    }

    private static Slider CreateBar(Transform parent, string name, Color fillColor)
    {
        GameObject barObject = new GameObject(name, typeof(RectTransform), typeof(Slider));
        barObject.transform.SetParent(parent, false);
        Image background = barObject.AddComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0.65f);
        GameObject fillObject = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fillObject.transform.SetParent(barObject.transform, false);
        Image fill = fillObject.GetComponent<Image>();
        fill.color = fillColor;
        SetRect(fillObject.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);
        Slider slider = barObject.GetComponent<Slider>();
        slider.fillRect = fillObject.GetComponent<RectTransform>();
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;
        return slider;
    }

    private static TMP_Text CreateText(Transform parent, string name, string value, float size)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform));
        textObject.transform.SetParent(parent, false);
        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = size;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Left;
        return text;
    }

    private static void SetRect(RectTransform rect, Vector2 min, Vector2 max)
    {
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void CreateEventSystem()
    {
        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<InputSystemUIInputModule>();
    }

    private static Material GetMaterial(string name, Color color)
    {
        string path = $"Assets/{name}.mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("Universal Render Pipeline/Lit"))
            {
                name = name,
                color = color
            };
            AssetDatabase.CreateAsset(material, path);
        }
        return material;
    }

    private static void AssignObject(UnityEngine.Object target, string fieldName, UnityEngine.Object value)
    {
        SerializedObject serialized = new SerializedObject(target);
        serialized.Update();
        SerializedProperty property = serialized.FindProperty(fieldName);
        if (property == null)
        {
            throw new InvalidOperationException($"Field '{fieldName}' was not found on {target.GetType().Name}.");
        }
        property.objectReferenceValue = value;
        serialized.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
    }

    private static void AssignInt(UnityEngine.Object target, string fieldName, int value)
    {
        SerializedObject serialized = new SerializedObject(target);
        serialized.Update();
        SerializedProperty property = serialized.FindProperty(fieldName);
        if (property == null)
        {
            throw new InvalidOperationException($"Field '{fieldName}' was not found on {target.GetType().Name}.");
        }
        property.intValue = value;
        serialized.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
    }

    private static void AssignBool(UnityEngine.Object target, string fieldName, bool value)
    {
        SerializedObject serialized = new SerializedObject(target);
        serialized.Update();
        SerializedProperty property = serialized.FindProperty(fieldName);
        if (property == null)
        {
            throw new InvalidOperationException($"Field '{fieldName}' was not found on {target.GetType().Name}.");
        }
        property.boolValue = value;
        serialized.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
    }

    private static void VerifyObjectField(UnityEngine.Object target, string fieldName)
    {
        SerializedObject serialized = new SerializedObject(target);
        serialized.Update();
        SerializedProperty property = serialized.FindProperty(fieldName);
        if (property == null || property.objectReferenceValue == null)
        {
            Debug.LogError($"Builder assignment check: '{fieldName}' type={property?.propertyType} " +
                           $"value={property?.objectReferenceValue} target={target.name}");
            throw new InvalidOperationException(
                $"Builder failed to assign '{fieldName}' on {target.GetType().Name}.");
        }
    }

    private static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parent = System.IO.Path.GetDirectoryName(path)?.Replace("\\", "/");
            string folder = System.IO.Path.GetFileName(path);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, folder);
        }
    }

    private static void AddSceneToBuildSettings(string path)
    {
        List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes.ToList();
        if (scenes.All(scene => scene.path != path))
        {
            scenes.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
