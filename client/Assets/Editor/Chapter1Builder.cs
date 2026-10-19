using System;
using System.Collections.Generic;
using System.Linq;
using EmberBlade.AI;
using EmberBlade.Combat;
using EmberBlade.Config;
using EmberBlade.Missions;
using EmberBlade.Player;
using EmberBlade.UI;
using TMPro;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class Chapter1Builder
{
    private const string ScenePath = "Assets/Scenes/Chapter1.unity";
    private const string ConfigPath = "Assets/Resources/GameConfig.asset";

    [MenuItem("Ember Blade/Build Chapter 1 Scene")]
    public static void BuildFromMenu() => Build();

    public static void BuildFromCommandLine() => Build();

    private static void Build()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        EditorSceneManager.SaveScene(scene, ScenePath);
        GameConfig config = AssetDatabase.LoadAssetAtPath<GameConfig>(ConfigPath);
        if (config == null)
        {
            throw new InvalidOperationException($"Missing {ConfigPath}. Build Combat Test Scene first.");
        }

        GameObject ground = CreateGround();
        CreateVillage();
        NavMeshSurface surface = ground.AddComponent<NavMeshSurface>();
        surface.collectObjects = CollectObjects.All;
        GameObject player = CreatePlayer(config);
        ThirdPersonCamera camera = CreateCamera(config, player.transform);
        GameObject scout = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Prefabs/Combat/SilverholdScout.prefab");
        GameObject skirmisher = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Prefabs/Combat/SilverholdSkirmisher.prefab");
        EnemyWaveManager enemies = new GameObject("EnemyWaveManager").AddComponent<EnemyWaveManager>();
        BarricadeManager barricades = new GameObject("BarricadeManager").AddComponent<BarricadeManager>();
        CombatHudController combatHud = CreateCombatHud();
        CreateEventSystem();
        GameObject placementRoot = new GameObject("Barricades");
        Material wallMaterial = GetMaterial("BarricadeWallMaterial", new Color(0.35f, 0.2f, 0.08f));
        Material trapMaterial = GetMaterial("SpikeTrapMaterial", Color.yellow);
        CreateBarricadeHud(out TMP_Text timer, out TMP_Text inventory, out TMP_Text instructions);
        CreateChapterHud(
            out TMP_Text speaker,
            out TMP_Text dialogue,
            out TMP_Text prompt,
            out TMP_Text objective,
            out TMP_Text result);

        List<DialogueInteractable> targets = new();
        targets.Add(CreateNpc("Tomas", new Vector3(-6f, 1f, 0f), Color.blue,
            CreateDialogue("TomasDialogue", "tomas", "Tomas", "I want to be a soldier like you.")));
        targets.Add(CreateNpc("Mara", new Vector3(-3f, 1f, 2f), Color.magenta,
            CreateDialogue("MaraDialogue", "mara", "Mara", "The smoke on the horizon worries me.")));
        targets.Add(CreateNpc("VillagerOne", new Vector3(3f, 1f, 1f), Color.gray,
            CreateDialogue("VillagerOneDialogue", "villager-one", "Villager", "The border road is quiet for now.")));
        targets.Add(CreateNpc("VillagerTwo", new Vector3(6f, 1f, -1f), Color.gray,
            CreateDialogue("VillagerTwoDialogue", "villager-two", "Villager", "We will help protect Emberfield.")));
        targets.Add(CreateNpc("Lira", new Vector3(0f, 1f, 5f), Color.yellow,
            CreateDialogue("LiraDialogue", "lira", "Lira", "Take this ember stone. The fire remembers you are coming home.")));
        GameObject rider = CreateNpc("RoyalRider", new Vector3(0f, 1f, 9f), Color.white,
            CreateDialogue("RiderDialogue", "royal-rider", "Royal Rider", "The crown summons you. Emberfield needs you."));
        DialogueInteractable riderInteractable = rider.GetComponent<DialogueInteractable>();
        targets.Add(riderInteractable);

        GameObject smoke = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        smoke.name = "HorizonSmoke";
        smoke.transform.position = new Vector3(0f, 4f, 18f);
        smoke.transform.localScale = new Vector3(1.5f, 4f, 1.5f);
        smoke.GetComponent<Renderer>().sharedMaterial = GetMaterial("SmokeMaterial", Color.gray);
        smoke.SetActive(false);

        MissionManager missions = new GameObject("MissionManager").AddComponent<MissionManager>();
        DialogueUI dialogueUI = new GameObject("DialogueUI").AddComponent<DialogueUI>();
        Chapter1SceneBootstrap bootstrap =
            new GameObject("Chapter1Bootstrap").AddComponent<Chapter1SceneBootstrap>();

        Assign(bootstrap, "config", config);
        Assign(bootstrap, "player", player);
        Assign(bootstrap, "followCamera", camera);
        Assign(bootstrap, "dialogueUI", dialogueUI);
        Assign(bootstrap, "missionManager", missions);
        Assign(bootstrap, "barricadeManager", barricades);
        Assign(bootstrap, "enemyWaveManager", enemies);
        Assign(bootstrap, "combatHud", combatHud);
        Assign(bootstrap, "scoutPrefab", scout);
        Assign(bootstrap, "skirmisherPrefab", skirmisher);
        Assign(bootstrap, "smokeObject", smoke);
        Assign(bootstrap, "royalRider", rider);
        AssignArray(bootstrap, "dialogueTargets", targets.Cast<UnityEngine.Object>().ToArray());
        Assign(bootstrap, "speakerText", speaker);
        Assign(bootstrap, "dialogueText", dialogue);
        Assign(bootstrap, "promptText", prompt);
        Assign(bootstrap, "objectiveText", objective);
        Assign(bootstrap, "resultText", result);
        Assign(bootstrap, "timerText", timer);
        Assign(bootstrap, "inventoryText", inventory);
        Assign(bootstrap, "instructionText", instructions);
        Assign(bootstrap, "placementCamera", camera.GetComponent<Camera>());
        Assign(bootstrap, "placementRoot", placementRoot.transform);
        Assign(bootstrap, "wallMaterial", wallMaterial);
        Assign(bootstrap, "trapMaterial", trapMaterial);
        Assign(missions, "dialogueUI", dialogueUI);
        Assign(missions, "barricadeManager", barricades);
        Assign(missions, "enemyWaveManager", enemies);
        Assign(missions, "playerHealth", player.GetComponent<Health>());
        Assign(missions, "smokeObject", smoke);
        Assign(missions, "royalRider", rider);
        Assign(missions, "objectiveText", objective);
        Assign(missions, "resultText", result);
        Assign(dialogueUI, "player", player.transform);
        AssignArray(dialogueUI, "interactables", targets.Cast<UnityEngine.Object>().ToArray());
        Assign(dialogueUI, "speakerText", speaker);
        Assign(dialogueUI, "dialogueText", dialogue);
        Assign(dialogueUI, "promptText", prompt);
        Assign(barricades, "config", config);
        Assign(barricades, "placementCamera", camera.GetComponent<Camera>());
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
        Debug.Log($"Ember Blade Chapter 1 scene built at {ScenePath}.");
    }

    private static GameObject CreateGround()
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.localScale = new Vector3(8f, 1f, 8f);
        ground.GetComponent<Renderer>().sharedMaterial =
            GetMaterial("ChapterGroundMaterial", new Color(0.18f, 0.22f, 0.18f));
        return ground;
    }

    private static void CreateVillage()
    {
        for (int index = 0; index < 4; index++)
        {
            GameObject hut = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hut.name = $"VillageHut{index + 1}";
            hut.transform.position = new Vector3(-7f + index * 4.5f, 1f, 7f);
            hut.transform.localScale = new Vector3(3f, 2f, 3f);
            hut.GetComponent<Renderer>().sharedMaterial =
                GetMaterial("VillageHutMaterial", new Color(0.45f, 0.25f, 0.12f));
        }
    }

    private static GameObject CreatePlayer(GameConfig config)
    {
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.transform.position = new Vector3(0f, 1f, -5f);
        player.GetComponent<Renderer>().sharedMaterial =
            GetMaterial("ChapterPlayerMaterial", new Color(0.2f, 0.45f, 0.9f));
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

    private static GameObject CreateNpc(string name, Vector3 position, Color color, DialogueData data)
    {
        GameObject npc = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        npc.name = name;
        npc.transform.position = position;
        npc.GetComponent<Renderer>().sharedMaterial = GetMaterial($"{name}Material", color);
        DialogueInteractable interactable = npc.AddComponent<DialogueInteractable>();
        Assign(interactable, "dialogueId", data.dialogueId);
        Assign(interactable, "dialogue", data);
        return npc;
    }

    private static DialogueData CreateDialogue(
        string assetName, string id, string speaker, string text)
    {
        string folder = "Assets/Resources/Dialogue";
        EnsureFolder("Assets/Resources", "Dialogue");
        string path = $"{folder}/{assetName}.asset";
        DialogueData data = AssetDatabase.LoadAssetAtPath<DialogueData>(path);
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<DialogueData>();
            AssetDatabase.CreateAsset(data, path);
        }
        data.dialogueId = id;
        data.lines = new[] { new DialogueLine { speaker = speaker, text = text } };
        EditorUtility.SetDirty(data);
        return data;
    }

    private static CombatHudController CreateCombatHud()
    {
        GameObject canvas = CreateCanvas("CombatHUD");
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
        timer = CreateText(canvas.transform, "BuildTimer", string.Empty, new Vector2(0.35f, 0.9f), new Vector2(0.65f, 0.96f));
        inventory = CreateText(canvas.transform, "BarricadeInventory", string.Empty, new Vector2(0.35f, 0.83f), new Vector2(0.65f, 0.89f));
        instructions = CreateText(canvas.transform, "BuildInstructions", string.Empty, new Vector2(0.15f, 0.02f), new Vector2(0.85f, 0.08f));
    }

    private static void CreateChapterHud(
        out TMP_Text speaker,
        out TMP_Text dialogue,
        out TMP_Text prompt,
        out TMP_Text objective,
        out TMP_Text result)
    {
        GameObject canvas = GameObject.Find("CombatHUD");
        objective = CreateText(canvas.transform, "ObjectiveText", string.Empty, new Vector2(0.28f, 0.93f), new Vector2(0.72f, 0.99f));
        result = CreateText(canvas.transform, "ResultText", string.Empty, new Vector2(0.3f, 0.45f), new Vector2(0.7f, 0.58f));
        speaker = CreateText(canvas.transform, "DialogueSpeaker", string.Empty, new Vector2(0.2f, 0.2f), new Vector2(0.8f, 0.25f));
        dialogue = CreateText(canvas.transform, "DialogueText", string.Empty, new Vector2(0.2f, 0.1f), new Vector2(0.8f, 0.2f));
        prompt = CreateText(canvas.transform, "DialoguePrompt", string.Empty, new Vector2(0.2f, 0.03f), new Vector2(0.8f, 0.08f));
    }

    private static GameObject CreateCanvas(string name)
    {
        GameObject canvas = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        return canvas;
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
            material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = name };
            material.color = color;
            AssetDatabase.CreateAsset(material, path);
        }
        return material;
    }

    private static void EnsureFolder(string parent, string child)
    {
        if (!AssetDatabase.IsValidFolder($"{parent}/{child}"))
        {
            AssetDatabase.CreateFolder(parent, child);
        }
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
        if (value is UnityEngine.Object)
        {
            property.objectReferenceValue = value;
        }
        serialized.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
    }

    private static void Assign(UnityEngine.Object target, string field, string value)
    {
        SerializedObject serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(field);
        if (property == null)
        {
            throw new InvalidOperationException($"Field '{field}' was not found on {target.GetType().Name}.");
        }
        property.stringValue = value;
        serialized.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
    }

    private static void AssignArray(UnityEngine.Object target, string field, UnityEngine.Object[] values)
    {
        SerializedObject serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(field);
        if (property == null)
        {
            throw new InvalidOperationException($"Field '{field}' was not found on {target.GetType().Name}.");
        }
        property.arraySize = values.Length;
        for (int index = 0; index < values.Length; index++)
        {
            property.GetArrayElementAtIndex(index).objectReferenceValue = values[index];
        }
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
