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

public static class BarricadeTestBuilder
{
    private const string ScenePath = "Assets/Scenes/BarricadeTest.unity";
    private const string ConfigPath = "Assets/Resources/GameConfig.asset";

    [MenuItem("Ember Blade/Build Barricade Test Scene")]
    public static void BuildFromMenu() => Build();

    public static void BuildFromCommandLine() => Build();

    private static void Build()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        EditorSceneManager.SaveScene(scene, ScenePath);
        GameConfig config = AssetDatabase.LoadAssetAtPath<GameConfig>(ConfigPath);
        GameObject ground = CreateGround();
        NavMeshSurface surface = ground.AddComponent<NavMeshSurface>();
        surface.collectObjects = CollectObjects.All;
        surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
        GameObject player = CreatePlayer(config);
        ThirdPersonCamera cameraRig = CreateCamera(config, player.transform);
        GameObject scout = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Combat/SilverholdScout.prefab");
        GameObject skirmisher = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Combat/SilverholdSkirmisher.prefab");
        EnemyWaveManager enemies = new GameObject("EnemyWaveManager").AddComponent<EnemyWaveManager>();
        BarricadeManager barricades = new GameObject("BarricadeManager").AddComponent<BarricadeManager>();
        CombatHudController combatHud = CreateCombatHud();
        CreateEventSystem();
        GameObject placementRoot = new GameObject("Barricades");
        Material wallMaterial = GetMaterial("BarricadeWallMaterial", new Color(0.35f, 0.2f, 0.08f));
        Material trapMaterial = GetMaterial("SpikeTrapMaterial", Color.yellow);
        CreateBarricadeHud(
            out TMP_Text timer,
            out TMP_Text inventory,
            out TMP_Text instructions);
        BarricadeSceneBootstrap bootstrap = new GameObject("BarricadeBootstrap")
            .AddComponent<BarricadeSceneBootstrap>();

        Assign(bootstrap, "config", config);
        Assign(bootstrap, "player", player);
        Assign(bootstrap, "followCamera", cameraRig);
        Assign(bootstrap, "enemyWaveManager", enemies);
        Assign(bootstrap, "barricadeManager", barricades);
        Assign(bootstrap, "combatHud", combatHud);
        Assign(bootstrap, "scoutPrefab", scout);
        Assign(bootstrap, "skirmisherPrefab", skirmisher);
        Assign(bootstrap, "placementCamera", cameraRig.GetComponent<Camera>());
        Assign(bootstrap, "placementRoot", placementRoot.transform);
        Assign(bootstrap, "timerText", timer);
        Assign(bootstrap, "inventoryText", inventory);
        Assign(bootstrap, "instructionText", instructions);
        Assign(bootstrap, "wallMaterial", wallMaterial);
        Assign(bootstrap, "trapMaterial", trapMaterial);
        Assign(barricades, "config", config);
        Assign(barricades, "placementCamera", cameraRig.GetComponent<Camera>());
        Assign(barricades, "placementRoot", placementRoot.transform);
        Assign(barricades, "timerText", timer);
        Assign(barricades, "inventoryText", inventory);
        Assign(barricades, "instructionText", instructions);
        Assign(barricades, "wallMaterial", wallMaterial);
        Assign(barricades, "trapMaterial", trapMaterial);
        Assign(barricades, "enemyWaveManager", enemies);

        surface.BuildNavMesh();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AddBuildScene(ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Ember Blade Barricade Test scene built at {ScenePath}.");
    }

    private static GameObject CreateGround()
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.localScale = new Vector3(6f, 1f, 6f);
        ground.GetComponent<Renderer>().sharedMaterial =
            GetMaterial("BarricadeGroundMaterial", new Color(0.18f, 0.22f, 0.18f));
        return ground;
    }

    private static GameObject CreatePlayer(GameConfig config)
    {
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.transform.position = new Vector3(0f, 1f, -4f);
        player.GetComponent<Renderer>().sharedMaterial =
            GetMaterial("BarricadePlayerMaterial", new Color(0.2f, 0.45f, 0.9f));
        UnityEngine.Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());
        player.AddComponent<CharacterController>();
        Health health = player.AddComponent<Health>();
        EmberMeter ember = player.AddComponent<EmberMeter>();
        PlayerController movement = player.AddComponent<PlayerController>();
        PlayerCombat combat = player.AddComponent<PlayerCombat>();
        AssignInt(health, "maxHealth", config.playerMaxHealth);
        Assign(ember, "config", config);
        Assign(movement, "config", config);
        Assign(combat, "config", config);
        Assign(combat, "health", health);
        Assign(combat, "emberMeter", ember);
        return player;
    }

    private static ThirdPersonCamera CreateCamera(GameConfig config, Transform target)
    {
        GameObject cameraObject = new GameObject("Follow Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.AddComponent<Camera>();
        cameraObject.AddComponent<AudioListener>();
        ThirdPersonCamera camera = cameraObject.AddComponent<ThirdPersonCamera>();
        Assign(camera, "config", config);
        Assign(camera, "target", target);
        Assign(target.GetComponent<PlayerController>(), "followCamera", camera);
        return camera;
    }

    private static CombatHudController CreateCombatHud()
    {
        GameObject canvas = new GameObject("CombatHUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvas.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920f, 1080f);
        Slider health = CreateBar(canvas.transform, "HealthBar", Color.red, new Vector2(0.03f, 0.9f), new Vector2(0.25f, 0.95f));
        Slider ember = CreateBar(canvas.transform, "EmberBar", new Color(1f, 0.25f, 0.05f), new Vector2(0.03f, 0.82f), new Vector2(0.25f, 0.87f));
        TMP_Text emberText = CreateText(canvas.transform, "EmberText", "Ember: 0/100", new Vector2(0.03f, 0.75f), new Vector2(0.3f, 0.81f));
        TMP_Text enemies = CreateText(canvas.transform, "EnemiesText", "Enemies remaining: 0", new Vector2(0.7f, 0.9f), new Vector2(0.98f, 0.97f));
        CombatHudController controller = canvas.AddComponent<CombatHudController>();
        Assign(controller, "healthBar", health);
        Assign(controller, "emberBar", ember);
        Assign(controller, "emberText", emberText);
        Assign(controller, "enemiesText", enemies);
        return controller;
    }

    private static void CreateBarricadeHud(out TMP_Text timer, out TMP_Text inventory, out TMP_Text instructions)
    {
        GameObject canvas = GameObject.Find("CombatHUD");
        timer = CreateText(canvas.transform, "BuildTimer", "Build time: 30", new Vector2(0.35f, 0.9f), new Vector2(0.65f, 0.96f));
        inventory = CreateText(canvas.transform, "BarricadeInventory", "Walls: 8 Traps: 3", new Vector2(0.35f, 0.83f), new Vector2(0.65f, 0.89f));
        instructions = CreateText(canvas.transform, "BuildInstructions", "Click: place wall | T: select trap | R: rotate | Right-click: remove | Enter: raid", new Vector2(0.2f, 0.02f), new Vector2(0.8f, 0.08f));
    }

    private static Slider CreateBar(Transform parent, string name, Color color, Vector2 min, Vector2 max)
    {
        GameObject bar = new GameObject(name, typeof(RectTransform), typeof(Slider), typeof(Image));
        bar.transform.SetParent(parent, false);
        SetRect(bar.GetComponent<RectTransform>(), min, max);
        GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fill.transform.SetParent(bar.transform, false);
        fill.GetComponent<Image>().color = color;
        SetRect(fill.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);
        Slider slider = bar.GetComponent<Slider>();
        slider.fillRect = fill.GetComponent<RectTransform>();
        slider.value = 1f;
        return slider;
    }

    private static TMP_Text CreateText(Transform parent, string name, string value, Vector2 min, Vector2 max)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform));
        textObject.transform.SetParent(parent, false);
        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = 22f;
        text.color = Color.white;
        SetRect(text.rectTransform, min, max);
        return text;
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
            material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = name, color = color };
            AssetDatabase.CreateAsset(material, path);
        }
        return material;
    }

    private static void SetRect(RectTransform rect, Vector2 min, Vector2 max)
    {
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void Assign(UnityEngine.Object target, string field, UnityEngine.Object value)
    {
        SerializedObject serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(field);
        if (property == null)
        {
            throw new InvalidOperationException($"Field '{field}' was not found on {target.GetType().Name}.");
        }
        property.objectReferenceValue = value;
        serialized.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
    }

    private static void AssignInt(UnityEngine.Object target, string field, int value)
    {
        SerializedObject serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(field);
        if (property == null)
        {
            throw new InvalidOperationException($"Field '{field}' was not found on {target.GetType().Name}.");
        }
        property.intValue = value;
        serialized.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
    }

    private static void AddBuildScene(string path)
    {
        List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes.ToList();
        if (scenes.All(scene => scene.path != path))
        {
            scenes.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
