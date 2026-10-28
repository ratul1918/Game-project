using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TheLightHouse.Editor
{
    public static class BuildThinSliceScene
    {
        private const string SceneDirectory = "Assets/_Project/Scenes";
        private const string ScenePath = SceneDirectory + "/ThinSlice.unity";
        private const string InputActionsPath = "Assets/InputSystem_Actions.inputactions";
        private const string NavMeshSurfaceTypeName = "Unity.AI.Navigation.NavMeshSurface";
        private const string NavigationPackageId = "com.unity.ai.navigation@2.0.9";

        private const float TowerWidth = 20f;
        private const float TowerDepth = 20f;
        private const float FloorThickness = 0.5f;
        private const float WallThickness = 0.5f;
        private const float WallHeight = 6f;
        private const float PlayerHeight = 1.8f;
        private const float PlayerRadius = 0.35f;
        private const float CameraHeight = 1.65f;
        private const float StalkerHeight = 2f;
        private const float StalkerRadius = 0.5f;
        private const float LampLightRange = 24f;

        private static readonly Vector3 PlayerPosition = new(0f, 0.05f, -6f);
        private static readonly Vector3 LampPosition = new(0f, 1.5f, 4f);
        private static readonly Vector3 CrankPosition = new(2.5f, 0.75f, 4f);
        private static readonly Vector3 StalkerPosition = new(-6f, 0f, 5f);

        [MenuItem("TheLightHouse/Build Thin Slice Scene")]
        public static void Build()
        {
            BuildTo(ScenePath);
        }

        public static void BuildTo(string outputPath)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            InputActionAsset inputActions =
                AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
            if (inputActions == null)
            {
                Debug.LogError($"Could not load the Input Actions asset at {InputActionsPath}.");
                return;
            }

            Directory.CreateDirectory(SceneDirectory);
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject environment = new("Greybox Tower");
            CreateGreybox(environment.transform);
            AddNavigationSurface();

            NoiseManager noiseManager = new GameObject("Noise Manager").AddComponent<NoiseManager>();
            LampController lamp = CreateLamp();
            CreateCrankStation(lamp);
            StalkerAI stalker = CreateStalker();
            InteractionPrompt prompt = CreateCanvas(lamp, stalker);
            GameObject player = CreatePlayer(inputActions, prompt);

            EditorSceneManager.MarkSceneDirty(scene);
            bool saved = EditorSceneManager.SaveScene(scene, outputPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeGameObject = player;
            Debug.Log(
                saved
                    ? $"Built thin slice scene at {ScenePath}. Bake the NavMeshSurface before Play."
                    : $"Failed to save thin slice scene at {ScenePath}.",
                noiseManager);
        }

        private static void CreateGreybox(Transform parent)
        {
            CreateCube(
                "Floor",
                new Vector3(0f, -FloorThickness * 0.5f, 0f),
                new Vector3(TowerWidth, FloorThickness, TowerDepth),
                parent);

            float halfWidth = TowerWidth * 0.5f;
            float halfDepth = TowerDepth * 0.5f;
            float wallY = WallHeight * 0.5f;

            CreateCube(
                "North Wall",
                new Vector3(0f, wallY, halfDepth),
                new Vector3(TowerWidth, WallHeight, WallThickness),
                parent);
            CreateCube(
                "South Wall",
                new Vector3(0f, wallY, -halfDepth),
                new Vector3(TowerWidth, WallHeight, WallThickness),
                parent);
            CreateCube(
                "East Wall",
                new Vector3(halfWidth, wallY, 0f),
                new Vector3(WallThickness, WallHeight, TowerDepth),
                parent);
            CreateCube(
                "West Wall",
                new Vector3(-halfWidth, wallY, 0f),
                new Vector3(WallThickness, WallHeight, TowerDepth),
                parent);
        }

        private static GameObject CreatePlayer(
            InputActionAsset inputActions,
            InteractionPrompt prompt)
        {
            GameObject player = new("Player");
            player.transform.position = PlayerPosition;

            CharacterController characterController = player.AddComponent<CharacterController>();
            characterController.height = PlayerHeight;
            characterController.radius = PlayerRadius;
            characterController.center = Vector3.up * (PlayerHeight * 0.5f);

            NoiseEmitter noiseEmitter = player.AddComponent<NoiseEmitter>();
            FirstPersonController controller = player.AddComponent<FirstPersonController>();

            GameObject cameraObject = new("Player Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(player.transform, false);
            cameraObject.transform.localPosition = Vector3.up * CameraHeight;
            Camera playerCamera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();

            Interactor interactor = player.AddComponent<Interactor>();

            SetObjectReference(controller, "inputActions", inputActions);
            SetObjectReference(controller, "cameraTransform", cameraObject.transform);
            SetObjectReference(controller, "noiseEmitter", noiseEmitter);
            SetObjectReference(interactor, "inputActions", inputActions);
            SetObjectReference(interactor, "viewCamera", playerCamera);
            SetObjectReference(interactor, "interactionPrompt", prompt);

            return player;
        }

        private static LampController CreateLamp()
        {
            GameObject lampObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            lampObject.name = "Lamp";
            lampObject.transform.position = LampPosition;
            lampObject.transform.localScale = new Vector3(1.25f, 1.5f, 1.25f);

            LampController lamp = lampObject.AddComponent<LampController>();

            GameObject lightObject = new("Lamp Light");
            lightObject.transform.SetParent(lampObject.transform, false);
            lightObject.transform.localPosition = Vector3.up * 1.5f;
            Light lampLight = lightObject.AddComponent<Light>();
            lampLight.type = LightType.Point;
            lampLight.range = LampLightRange;
            lampLight.shadows = LightShadows.Soft;

            SetObjectReference(lamp, "lampLight", lampLight);
            return lamp;
        }

        private static void CreateCrankStation(LampController lamp)
        {
            GameObject crankObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            crankObject.name = "Crank Station";
            crankObject.transform.position = CrankPosition;
            crankObject.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);

            NoiseEmitter noiseEmitter = crankObject.AddComponent<NoiseEmitter>();
            CrankStation crankStation = crankObject.AddComponent<CrankStation>();
            SetObjectReference(crankStation, "lamp", lamp);
            SetObjectReference(crankStation, "noiseEmitter", noiseEmitter);
        }

        private static StalkerAI CreateStalker()
        {
            GameObject stalker = new("Stalker");
            stalker.transform.position = StalkerPosition;

            NavMeshAgent agent = stalker.AddComponent<NavMeshAgent>();
            agent.height = StalkerHeight;
            agent.radius = StalkerRadius;
            agent.stoppingDistance = StalkerRadius;

            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "Placeholder Cube";
            visual.transform.SetParent(stalker.transform, false);
            visual.transform.localPosition = Vector3.up * (StalkerHeight * 0.5f);
            visual.transform.localScale = new Vector3(1f, StalkerHeight, 1f);
            UnityEngine.Object.DestroyImmediate(visual.GetComponent<Collider>());

            return stalker.AddComponent<StalkerAI>();
        }

        private static InteractionPrompt CreateCanvas(LampController lamp, StalkerAI stalker)
        {
            GameObject canvasObject = new(
                "Debug Canvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            Text promptText = CreateText(
                "Interaction Prompt",
                canvasObject.transform,
                new Vector2(0.5f, 0.2f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(700f, 60f),
                TextAnchor.MiddleCenter);
            InteractionPrompt prompt = promptText.gameObject.AddComponent<InteractionPrompt>();
            SetObjectReference(prompt, "label", promptText);

            Text fuelText = CreateText(
                "Fuel Text",
                canvasObject.transform,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(24f, -24f),
                new Vector2(600f, 44f),
                TextAnchor.UpperLeft);
            Text stalkerText = CreateText(
                "Stalker State Text",
                canvasObject.transform,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(24f, -72f),
                new Vector2(600f, 44f),
                TextAnchor.UpperLeft);

            DebugHud hud = canvasObject.AddComponent<DebugHud>();
            SetObjectReference(hud, "lamp", lamp);
            SetObjectReference(hud, "stalker", stalker);
            SetObjectReference(hud, "fuelText", fuelText);
            SetObjectReference(hud, "stalkerStateText", stalkerText);

            return prompt;
        }

        private static Text CreateText(
            string name,
            Transform parent,
            Vector2 anchor,
            Vector2 pivot,
            Vector2 anchoredPosition,
            Vector2 size,
            TextAnchor alignment)
        {
            GameObject textObject = new(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Text));
            textObject.transform.SetParent(parent, false);

            RectTransform rectTransform = textObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = anchor;
            rectTransform.anchorMax = anchor;
            rectTransform.pivot = pivot;
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = size;

            Text text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 28;
            text.color = Color.white;
            text.alignment = alignment;
            text.raycastTarget = false;
            return text;
        }

        private static GameObject CreateCube(
            string name,
            Vector3 position,
            Vector3 scale,
            Transform parent)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent, false);
            cube.transform.position = position;
            cube.transform.localScale = scale;
            return cube;
        }

        private static void AddNavigationSurface()
        {
            Type surfaceType = null;
            foreach (Type componentType in TypeCache.GetTypesDerivedFrom<Component>())
            {
                if (componentType.FullName == NavMeshSurfaceTypeName)
                {
                    surfaceType = componentType;
                    break;
                }
            }

            GameObject navigation = new("Navigation");
            if (surfaceType != null)
            {
                navigation.AddComponent(surfaceType);
                return;
            }

            Debug.LogWarning(
                $"AI Navigation is not installed. Install {NavigationPackageId}, then rebuild the scene.");
        }

        private static void SetObjectReference(
            UnityEngine.Object target,
            string propertyName,
            UnityEngine.Object value)
        {
            SerializedObject serializedObject = new(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                throw new InvalidOperationException(
                    $"Could not find serialized property '{propertyName}' on {target.GetType().Name}.");
            }

            property.objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
