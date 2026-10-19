using System;
using System.Collections.Generic;
using System.Linq;
using EmberBlade.Network;
using EmberBlade.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class Phase1SceneBuilder
{
    private const string ScenePath = "Assets/Scenes/Phase1.unity";

    [MenuItem("Ember Blade/Build Phase 1 Scene")]
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
        Scene scene = OpenOrCreateScene();

        CreateEnvironment();
        NetworkManager networkManager = GetOrCreateRoot<NetworkManager>("NetworkManager");
        LoginManager loginManager = GetOrCreateRoot<LoginManager>("LoginManager");
        Canvas canvas = GetOrCreateRoot<Canvas>("Canvas");
        ConfigureCanvas(canvas);
        EnsureEventSystem();

        Transform loginPanel = GetOrCreateChild(canvas.transform, "LoginPanel");
        ConfigurePanel(loginPanel.gameObject, new Vector2(420f, 430f), new Vector2(0.5f, 0.5f));
        BuildLoginPanel(loginPanel, loginManager, networkManager);

        Transform hud = GetOrCreateChild(canvas.transform, "Hud");
        BuildHud(hud, networkManager);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AddSceneToBuildSettings(ScenePath);
        AssetDatabase.SaveAssets();
        Debug.Log($"Ember Blade Phase 1 scene built at {ScenePath}.");
    }

    private static Scene OpenOrCreateScene()
    {
        Scene existing = SceneManager.GetSceneByPath(ScenePath);
        if (existing.IsValid())
        {
            if (!existing.isLoaded)
            {
                existing = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }
            else
            {
                SceneManager.SetActiveScene(existing);
            }

            return existing;
        }

        Scene created = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        EditorSceneManager.SaveScene(created, ScenePath);
        return created;
    }

    private static void CreateEnvironment()
    {
        GameObject light = GetOrCreateRootObject("Directional Light");
        Light directionalLight = GetOrAddComponent<Light>(light);
        directionalLight.type = LightType.Directional;
        directionalLight.intensity = 1f;
        light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        GameObject ground = GetOrCreateRootObject("Ground");
        MeshFilter groundMesh = GetOrAddComponent<MeshFilter>(ground);
        MeshRenderer groundRenderer = GetOrAddComponent<MeshRenderer>(ground);
        if (groundMesh.sharedMesh == null)
        {
            GameObject temporaryPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            groundMesh.sharedMesh = temporaryPlane.GetComponent<MeshFilter>().sharedMesh;
            UnityEngine.Object.DestroyImmediate(temporaryPlane);
        }

        ground.transform.localScale = new Vector3(5f, 1f, 5f);
        GetOrAddComponent<MeshCollider>(ground);
        if (groundRenderer.sharedMaterial == null)
        {
            groundRenderer.sharedMaterial = CreateDefaultMaterial("GroundMaterial", new Color(0.18f, 0.22f, 0.18f));
        }

        GameObject cameraObject = GetOrCreateRootObject("Main Camera");
        Camera camera = GetOrAddComponent<Camera>(cameraObject);
        cameraObject.tag = "MainCamera";
        camera.transform.position = new Vector3(0f, 8f, -10f);
        camera.transform.rotation = Quaternion.Euler(35f, 0f, 0f);
        camera.clearFlags = CameraClearFlags.Skybox;
        GetOrAddComponent<AudioListener>(cameraObject);
    }

    private static void BuildLoginPanel(Transform panel, LoginManager loginManager, NetworkManager networkManager)
    {
        TMP_Text title = GetOrCreateText(panel, "Title", "Ember Blade", 32f, TextAlignmentOptions.Center);
        SetAnchoredRect(title.rectTransform, new Vector2(0.05f, 0.78f), new Vector2(0.95f, 0.98f), Vector2.zero);

        TMP_InputField username = GetOrCreateInputField(panel, "UsernameInput", "Username");
        SetAnchoredRect(username.GetComponent<RectTransform>(), new Vector2(0.1f, 0.56f), new Vector2(0.9f, 0.7f), Vector2.zero);

        TMP_InputField password = GetOrCreateInputField(panel, "PasswordInput", "Password");
        password.contentType = TMP_InputField.ContentType.Password;
        password.ForceLabelUpdate();
        SetAnchoredRect(password.GetComponent<RectTransform>(), new Vector2(0.1f, 0.36f), new Vector2(0.9f, 0.5f), Vector2.zero);

        Button loginButton = GetOrCreateButton(panel, "LoginButton", "Login");
        SetAnchoredRect(loginButton.GetComponent<RectTransform>(), new Vector2(0.25f, 0.18f), new Vector2(0.75f, 0.32f), Vector2.zero);

        TMP_Text status = GetOrCreateText(panel, "LoginStatus", "", 18f, TextAlignmentOptions.Center);
        SetAnchoredRect(status.rectTransform, new Vector2(0.05f, 0.03f), new Vector2(0.95f, 0.14f), Vector2.zero);

        LoginUI loginUI = GetOrAddComponent<LoginUI>(panel.gameObject);
        AssignObject(loginUI, "usernameInput", username);
        AssignObject(loginUI, "passwordInput", password);
        AssignObject(loginUI, "loginButton", loginButton);
        AssignObject(loginUI, "statusText", status);
        AssignObject(loginUI, "loginPanel", panel.gameObject);
        AssignObject(loginUI, "loginManager", loginManager);
        AssignObject(loginUI, "networkManager", networkManager);
    }

    private static void BuildHud(Transform hud, NetworkManager networkManager)
    {
        RectTransform hudRect = GetOrAddComponent<RectTransform>(hud.gameObject);
        hudRect.anchorMin = Vector2.zero;
        hudRect.anchorMax = Vector2.one;
        hudRect.offsetMin = Vector2.zero;
        hudRect.offsetMax = Vector2.zero;

        TMP_Text threat = GetOrCreateText(hud, "ThreatText", "Emberfield threat: 0", 26f, TextAlignmentOptions.TopLeft);
        SetAnchoredRect(threat.rectTransform, new Vector2(0.03f, 0.86f), new Vector2(0.35f, 0.98f), Vector2.zero);

        GameObject bannerObject = GetOrCreateChild(hud, "AlertBanner").gameObject;
        Image banner = GetOrAddComponent<Image>(bannerObject);
        banner.color = new Color(0.75f, 0.02f, 0.02f, 0.95f);
        SetAnchoredRect(banner.rectTransform, new Vector2(0.25f, 0.78f), new Vector2(0.75f, 0.88f), Vector2.zero);

        TMP_Text alert = GetOrCreateText(bannerObject.transform, "AlertText", "Raiders near Emberfield", 24f, TextAlignmentOptions.Center);
        alert.color = Color.white;
        SetAnchoredRect(alert.rectTransform, Vector2.zero, Vector2.one, Vector2.zero);

        HudController hudController = GetOrAddComponent<HudController>(hud.gameObject);
        AssignObject(hudController, "threatText", threat);
        AssignObject(hudController, "alertBanner", bannerObject);
        AssignObject(hudController, "alertText", alert);
        bannerObject.SetActive(false);
    }

    private static Canvas ConfigureCanvas(Canvas canvas)
    {
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = GetOrAddComponent<CanvasScaler>(canvas.gameObject);
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        GetOrAddComponent<GraphicRaycaster>(canvas.gameObject);
        return canvas;
    }

    private static void EnsureEventSystem()
    {
        GameObject eventSystem = GetOrCreateRootObject("EventSystem");
        GetOrAddComponent<EventSystem>(eventSystem);
        StandaloneInputModule oldModule = eventSystem.GetComponent<StandaloneInputModule>();
        if (oldModule != null)
        {
            UnityEngine.Object.DestroyImmediate(oldModule);
        }

        GetOrAddComponent<InputSystemUIInputModule>(eventSystem);
    }

    private static void ConfigurePanel(GameObject panel, Vector2 size, Vector2 pivot)
    {
        Image image = GetOrAddComponent<Image>(panel);
        image.color = new Color(0.04f, 0.04f, 0.06f, 0.94f);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = pivot;
        rect.anchorMax = pivot;
        rect.pivot = pivot;
        rect.sizeDelta = size;
        rect.anchoredPosition = Vector2.zero;
    }

    private static TMP_Text GetOrCreateText(Transform parent, string name, string value, float fontSize, TextAlignmentOptions alignment)
    {
        Transform existing = FindDirectChild(parent, name);
        GameObject textObject = existing != null ? existing.gameObject : new GameObject(name);
        if (existing == null)
        {
            textObject.transform.SetParent(parent, false);
        }

        TMP_Text text = GetOrAddComponent<TextMeshProUGUI>(textObject);
        text.text = value;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        return text;
    }

    private static TMP_InputField GetOrCreateInputField(Transform parent, string name, string placeholder)
    {
        Transform existing = FindDirectChild(parent, name);
        GameObject inputObject = existing != null ? existing.gameObject : new GameObject(name);
        if (existing == null)
        {
            inputObject.transform.SetParent(parent, false);
        }

        Image background = GetOrAddComponent<Image>(inputObject);
        background.color = new Color(1f, 1f, 1f, 0.12f);
        TMP_InputField input = GetOrAddComponent<TMP_InputField>(inputObject);

        TMP_Text text = GetOrCreateText(inputObject.transform, "Text", "", 22f, TextAlignmentOptions.Left);
        text.margin = new Vector4(12f, 0f, 12f, 0f);
        TMP_Text placeholderText = GetOrCreateText(inputObject.transform, "Placeholder", placeholder, 22f, TextAlignmentOptions.Left);
        placeholderText.color = new Color(1f, 1f, 1f, 0.5f);
        placeholderText.margin = new Vector4(12f, 0f, 12f, 0f);
        input.textComponent = text;
        input.placeholder = placeholderText;
        return input;
    }

    private static Button GetOrCreateButton(Transform parent, string name, string label)
    {
        Transform existing = FindDirectChild(parent, name);
        GameObject buttonObject = existing != null ? existing.gameObject : new GameObject(name);
        if (existing == null)
        {
            buttonObject.transform.SetParent(parent, false);
        }

        Image image = GetOrAddComponent<Image>(buttonObject);
        image.color = new Color(0.9f, 0.25f, 0.05f, 1f);
        Button button = GetOrAddComponent<Button>(buttonObject);
        TMP_Text labelText = GetOrCreateText(buttonObject.transform, "Label", label, 24f, TextAlignmentOptions.Center);
        SetAnchoredRect(labelText.rectTransform, Vector2.zero, Vector2.one, Vector2.zero);
        return button;
    }

    private static Transform GetOrCreateChild(Transform parent, string name)
    {
        Transform child = FindDirectChild(parent, name);
        if (child != null)
        {
            return child;
        }

        GameObject created = new GameObject(name);
        created.transform.SetParent(parent, false);
        return created.transform;
    }

    private static T GetOrCreateRoot<T>(string name) where T : Component
    {
        GameObject objectToUse = GetOrCreateRootObject(name);
        return GetOrAddComponent<T>(objectToUse);
    }

    private static GameObject GetOrCreateRootObject(string name)
    {
        GameObject existing = GameObject.Find(name);
        return existing != null ? existing : new GameObject(name);
    }

    private static T GetOrAddComponent<T>(GameObject target) where T : Component
    {
        T component = target.GetComponent<T>();
        return component != null ? component : target.AddComponent<T>();
    }

    private static Transform FindDirectChild(Transform parent, string name)
    {
        return parent.Cast<Transform>().FirstOrDefault(child => child.name == name);
    }

    private static void SetAnchoredRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 position)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.anchoredPosition = position;
    }

    private static void AssignObject(UnityEngine.Object target, string fieldName, UnityEngine.Object value)
    {
        SerializedObject serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(fieldName);
        if (property == null)
        {
            throw new InvalidOperationException($"Serialized field '{fieldName}' was not found on {target.GetType().Name}.");
        }

        property.objectReferenceValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static Material CreateDefaultMaterial(string name, Color color)
    {
        Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"))
        {
            name = name,
            color = color
        };
        AssetDatabase.CreateAsset(material, $"Assets/{name}.mat");
        return material;
    }

    private static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parent = System.IO.Path.GetDirectoryName(path)?.Replace("\\", "/");
            string folder = System.IO.Path.GetFileName(path);
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
