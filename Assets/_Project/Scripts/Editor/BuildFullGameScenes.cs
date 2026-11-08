using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace TheLightHouse.Editor
{
    public static class BuildFullGameScenes
    {
        const string MenuPath = "Assets/_Project/Scenes/MainMenu.unity";
        static readonly Color BackgroundColor = new(.012f, .018f, .025f, 1f);
        static readonly Color GlassPanelColor = new(.015f, .022f, .030f, .88f);
        static readonly Color SubmenuPanelColor = new(.010f, .015f, .022f, .96f);
        static readonly Color ControlColor = new(.07f, .10f, .13f, 1f);
        static readonly Color HoverColor = new(.14f, .19f, .24f, 1f);
        static readonly Color AccentAmber = new(.88f, .62f, .28f, 1f);
        static readonly Color TextColor = new(.92f, .94f, .93f, 1f);
        static readonly Color TextMuted = new(.62f, .68f, .72f, 1f);
        static readonly Vector2 ReferenceResolution = new(1920f, 1080f);

        public static void Build()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.isDirty) EditorSceneManager.SaveScene(activeScene);

            BuildAct1Scene.BuildTo("Assets/_Project/Scenes/Act1.unity");
            BuildAct2Scene.BuildTo("Assets/_Project/Scenes/Act2.unity");
            BuildAct3Scene.BuildTo("Assets/_Project/Scenes/Act3.unity");
            BuildAct4Scene.BuildTo("Assets/_Project/Scenes/Act4.unity");
            BuildAct5Scene.BuildTo("Assets/_Project/Scenes/Act5.unity");
            ReferenceVisualPass.ApplyAll();
            IslandArrivalPass.ApplyAll();
            ApplyGraphics1.Apply();
            AssetIntegrationPass.ApplyToAllScenes();
            HarrowRockEnvironmentMasterPass.ApplyToAllScenes();
            BuildMenu();

            string[] paths = {
                MenuPath,
                "Assets/_Project/Scenes/Act1.unity",
                "Assets/_Project/Scenes/Act2.unity",
                "Assets/_Project/Scenes/Act3.unity",
                "Assets/_Project/Scenes/Act4.unity",
                "Assets/_Project/Scenes/Act5.unity"
            };
            var scenes = new EditorBuildSettingsScene[paths.Length];
            for (int i = 0; i < paths.Length; i++) scenes[i] = new EditorBuildSettingsScene(paths[i], true);
            EditorBuildSettings.scenes = scenes;
            AssetDatabase.SaveAssets();
            Debug.Log("Full game scenes built and added to Build Settings. MainMenu is scene zero.");
        }

        public static void BuildMenu()
        {
            MainMenuSceneBuilder.BuildMenuScene();
        }

        public static void BuildStandalonePlayer()
        {
            BuildStandalone(false);
        }

        public static void BuildAndRunStandalonePlayer()
        {
            BuildStandalone(true);
        }

        private static void BuildStandalone(bool autoRun)
        {
            string buildFolder = Path.Combine(Directory.GetCurrentDirectory(), "Builds", "macOS");
            if (!Directory.Exists(buildFolder)) Directory.CreateDirectory(buildFolder);
            string buildPath = Path.Combine(buildFolder, "TheLightHouse.app");

            string[] scenes = {
                MenuPath,
                "Assets/_Project/Scenes/Act1.unity",
                "Assets/_Project/Scenes/Act2.unity",
                "Assets/_Project/Scenes/Act3.unity",
                "Assets/_Project/Scenes/Act4.unity",
                "Assets/_Project/Scenes/Act5.unity"
            };

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = buildPath,
                target = BuildTarget.StandaloneOSX,
                options = autoRun ? BuildOptions.AutoRunPlayer : BuildOptions.None
            };

            Debug.Log($"<color=#44AAFF>[TheLightHouse]</color> Starting Standalone macOS build to {buildPath}...");
            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"<color=#44FF88>[TheLightHouse]</color> Standalone macOS build succeeded! Size: {summary.totalSize / (1024 * 1024):F1} MB");
                EditorUtility.RevealInFinder(buildPath);
            }
            else
            {
                Debug.LogError($"<color=#FF4444>[TheLightHouse]</color> Standalone build failed with {summary.totalErrors} errors.");
            }
        }

        static Button Button(string label, Transform parent, Vector2 position, UnityEngine.Events.UnityAction action, Vector2? size = null)
        {
            Vector2 dimensions = size ?? new Vector2(440f, 50f);
            Image image = Image(label, parent, position, dimensions, ControlColor);
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.ColorTint;
            button.colors = new ColorBlock
            {
                normalColor = ControlColor,
                highlightedColor = HoverColor,
                pressedColor = AccentAmber,
                selectedColor = HoverColor,
                disabledColor = new Color(.04f, .05f, .06f, .45f),
                colorMultiplier = 1f,
                fadeDuration = .1f
            };
            if (action != null) UnityEventTools.AddPersistentListener(button.onClick, action);
            Text text = Label(label, image.transform, Vector2.zero, dimensions, 18, TextAnchor.MiddleCenter);
            text.fontStyle = FontStyle.Bold;
            text.raycastTarget = false;
            return button;
        }

        static Toggle Toggle(string label, Transform parent, Vector2 position, UnityEngine.Events.UnityAction<bool> action, bool initial)
        {
            var root = RectObject(label, parent, position, new Vector2(440f, 38f));
            Image box = Image("Box", root.transform, new Vector2(-195f, 0f), new Vector2(26f, 26f), ControlColor);
            Image check = Image("Checkmark", box.transform, Vector2.zero, new Vector2(16f, 16f), AccentAmber);
            Text text = Label(label, root.transform, new Vector2(18f, 0f), new Vector2(380f, 36f), 16, TextAnchor.MiddleLeft);
            text.raycastTarget = false;
            var toggle = root.AddComponent<Toggle>();
            toggle.targetGraphic = box;
            toggle.graphic = check;
            toggle.isOn = initial;
            if (action != null) UnityEventTools.AddPersistentListener(toggle.onValueChanged, action);
            return toggle;
        }

        static Slider Slider(string label, Transform parent, Vector2 position, UnityEngine.Events.UnityAction<float> action, float low, float high, float initial)
        {
            var root = RectObject(label, parent, position, new Vector2(440f, 54f));
            Text text = Label(label, root.transform, new Vector2(0f, 16f), new Vector2(440f, 22f), 14, TextAnchor.MiddleLeft);
            text.color = TextMuted;
            text.raycastTarget = false;

            Image track = Image("Track", root.transform, new Vector2(0f, -10f), new Vector2(440f, 10f), ControlColor);
            var fillArea = RectObject("Fill Area", track.transform, Vector2.zero, Vector2.zero);
            Stretch(fillArea.GetComponent<RectTransform>());
            fillArea.GetComponent<RectTransform>().offsetMin = new Vector2(4f, 2f);
            fillArea.GetComponent<RectTransform>().offsetMax = new Vector2(-4f, -2f);
            Image fill = Image("Fill", fillArea.transform, Vector2.zero, Vector2.zero, AccentAmber);
            Stretch(fill.rectTransform);

            var handleArea = RectObject("Handle Slide Area", track.transform, Vector2.zero, Vector2.zero);
            Stretch(handleArea.GetComponent<RectTransform>());
            handleArea.GetComponent<RectTransform>().offsetMin = new Vector2(8f, -6f);
            handleArea.GetComponent<RectTransform>().offsetMax = new Vector2(-8f, 6f);
            Image handle = Image("Handle", handleArea.transform, Vector2.zero, new Vector2(16f, 24f), TextColor);

            var slider = root.AddComponent<Slider>();
            slider.minValue = low;
            slider.maxValue = high;
            slider.value = initial;
            slider.fillRect = fill.rectTransform;
            slider.handleRect = handle.rectTransform;
            slider.targetGraphic = handle;
            slider.direction = UnityEngine.UI.Slider.Direction.LeftToRight;
            if (action != null) UnityEventTools.AddPersistentListener(slider.onValueChanged, action);
            return slider;
        }

        static GameObject RectObject(string name, Transform parent, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
            rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return go;
        }

        static Image Image(string name, Transform parent, Vector2 position, Vector2 size, Color color)
        {
            var go = RectObject(name, parent, position, size);
            go.AddComponent<CanvasRenderer>();
            var image = go.AddComponent<Image>();
            image.color = color;
            return image;
        }

        static Text Label(string value, Transform parent, Vector2 position, Vector2 size, int fontSize, TextAnchor alignment)
        {
            var go = RectObject(value + " Text", parent, position, size);
            go.AddComponent<CanvasRenderer>();
            var text = go.AddComponent<Text>();
            text.text = value;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = TextColor;
            return text;
        }

        static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        static void StretchVerticalLeft(RectTransform rect, float width)
        {
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = new Vector2(width, 0f);
        }

        static void Set(Object target, string name, Object value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(name);
            if (prop != null)
            {
                prop.objectReferenceValue = value;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        static void Set(Object target, string name, float value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(name);
            if (prop != null)
            {
                prop.floatValue = value;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        static void Set(Object target, string name, bool value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(name);
            if (prop != null)
            {
                prop.boolValue = value;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        static void SetArray<T>(Object target, string name, T[] array) where T : Object
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(name);
            if (prop != null && array != null)
            {
                prop.arraySize = array.Length;
                for (int i = 0; i < array.Length; i++)
                {
                    prop.GetArrayElementAtIndex(i).objectReferenceValue = array[i];
                }
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        static GameObject CreateOrganicWavyBluff(Transform parent, Material matShale, Material matFoam)
        {
            var root = new GameObject("Harrow Rock Island Bluff");
            root.transform.SetParent(parent, false);

            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();

            const int rings = 10;
            const int angularSegments = 64;
            Vector3 center = new Vector3(6f, 0, 22f);
            float radiusX = 22f;
            float radiusZ = 16f;
            float plateauHeight = 3.6f;

            for (int r = 0; r <= rings; r++)
            {
                float t = r / (float)rings;
                float ringR = t;

                for (int a = 0; a < angularSegments; a++)
                {
                    float angle = a * Mathf.PI * 2f / angularSegments;
                    float sinA = Mathf.Sin(angle);
                    float cosA = Mathf.Cos(angle);

                    float harmonic = 1f 
                        + 0.14f * Mathf.Sin(angle * 3f + 0.4f) 
                        + 0.09f * Mathf.Cos(angle * 5f - 0.2f) 
                        + 0.05f * Mathf.Sin(angle * 8f);

                    float x = center.x + sinA * radiusX * ringR * harmonic;
                    float z = center.z + cosA * radiusZ * ringR * harmonic;

                    float y;
                    if (t < 0.40f)
                    {
                        float plateauUndulation = 0.12f * Mathf.Sin(x * 0.4f) * Mathf.Cos(z * 0.35f);
                        y = plateauHeight + plateauUndulation;
                    }
                    else if (t < 0.85f)
                    {
                        float cliffT = (t - 0.40f) / 0.45f;
                        float stepped = Mathf.Floor(cliffT * 3f) / 3f;
                        float ledge = Mathf.Lerp(cliffT, stepped, 0.45f);
                        float cliffHeight = Mathf.Lerp(plateauHeight, 0.4f, ledge);
                        float cragNoise = 0.28f * Mathf.Sin(angle * 7f) * Mathf.Cos(x * 0.4f) + 0.15f * Mathf.Sin(cliffHeight * 3.5f);
                        y = cliffHeight + cragNoise;
                    }
                    else
                    {
                        float seaT = (t - 0.85f) / 0.15f;
                        y = Mathf.Lerp(0.4f, -1.6f, seaT);
                    }

                    vertices.Add(new Vector3(x, y, z));
                    uvs.Add(new Vector2(x * 0.15f, z * 0.15f));
                }
            }

            for (int r = 0; r < rings; r++)
            {
                int row0 = r * angularSegments;
                int row1 = (r + 1) * angularSegments;
                for (int a = 0; a < angularSegments; a++)
                {
                    int nextA = (a + 1) % angularSegments;
                    int i00 = row0 + a;
                    int i01 = row0 + nextA;
                    int i10 = row1 + a;
                    int i11 = row1 + nextA;

                    triangles.Add(i00);
                    triangles.Add(i10);
                    triangles.Add(i01);

                    triangles.Add(i01);
                    triangles.Add(i10);
                    triangles.Add(i11);
                }
            }

            var mesh = new Mesh { name = "MenuWavyBluff" };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();

            var mf = root.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;
            var mr = root.AddComponent<MeshRenderer>();
            mr.sharedMaterial = matShale;

            var rnd = new System.Random(101);
            for (int i = 0; i < 14; i++)
            {
                float ang = i * Mathf.PI * 2f / 14f;
                float dist = 1f + 0.14f * Mathf.Sin(ang * 3f) + 0.09f * Mathf.Cos(ang * 5f);
                float bx = center.x + Mathf.Sin(ang) * (radiusX * dist * 0.94f);
                float bz = center.z + Mathf.Cos(ang) * (radiusZ * dist * 0.94f);
                var boulder = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                boulder.name = "Menu Shore Boulder";
                boulder.transform.SetParent(root.transform, false);
                boulder.transform.position = new Vector3(bx, -0.65f, bz);
                boulder.transform.localScale = new Vector3(
                    2.4f + (float)rnd.NextDouble() * 1.6f,
                    1.4f + (float)rnd.NextDouble() * 0.8f,
                    2.4f + (float)rnd.NextDouble() * 1.6f
                );
                if (matShale) boulder.GetComponent<Renderer>().sharedMaterial = matShale;
                Object.DestroyImmediate(boulder.GetComponent<Collider>());
            }

            return root;
        }
    }
}
