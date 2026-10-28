using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TheLightHouse.Editor
{
    public static class BuildMovementTestScene
    {
        public const string SettingsPath = "Assets/_Project/Resources/PlayerMovementSettings.asset";
        const string ScenePath = "Assets/_Project/Scenes/MovementTest.unity";
        const int StairCount = 12;
        const float StairRise = .2f;
        const float StairRun = .5f;
        static readonly float[] LedgeHeights = { .4f, .8f, 1.1f };
        static readonly string[] Tags = { "Wood", "Stone", "Metal", "Water", "Sand" };

        [InitializeOnLoadMethod]
        static void InitializeSettings() => EditorApplication.delayCall += () => EnsureSettings();

        public static PlayerMovementSettings EnsureSettings()
        {
            var existing = AssetDatabase.LoadAssetAtPath<PlayerMovementSettings>(SettingsPath);
            if (existing) return existing;
            Directory.CreateDirectory("Assets/_Project/Resources");
            AssetDatabase.Refresh();
            var settings = ScriptableObject.CreateInstance<PlayerMovementSettings>();
            AssetDatabase.CreateAsset(settings, SettingsPath);
            AssetDatabase.SaveAssets();
            return settings;
        }

        [MenuItem("TheLightHouse/Build Movement Test Scene")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("Stop Play Mode before building the movement course.");
                return;
            }
            EnsureSettings();
            EnsureTags();
            // Reuse the established player, interaction, noise and debug Canvas wiring.
            BuildThinSliceScene.BuildTo(ScenePath);
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (scene.path != ScenePath) return;
            Object.DestroyImmediate(GameObject.Find("Greybox Tower"));
            Object.DestroyImmediate(GameObject.Find("Lamp"));
            Object.DestroyImmediate(GameObject.Find("Crank Station"));
            Object.DestroyImmediate(GameObject.Find("Stalker"));
            var player = GameObject.Find("Player");
            player.transform.position = new Vector3(0f, .1f, -12f);
            var serialized = new SerializedObject(player.GetComponent<FirstPersonController>());
            serialized.FindProperty("settings").objectReferenceValue = EnsureSettings();
            serialized.ApplyModifiedPropertiesWithoutUndo();

            Cube("Flat floor — run lane", new(0f, -.25f, 0f), new(40f, .5f, 36f));
            for (int i = 0; i < StairCount; i++)
                Cube("Stair " + i, new(-10f, (i + 1) * StairRise * .5f, i * StairRun),
                    new(3f, (i + 1) * StairRise, StairRun));
            Cube("Stair landing", new(-10f, 1.1f, 7f), new(3f, 2.2f, 2f));
            var ramp = Cube("Ramp 20 degrees", new(-5f, .9f, 3f), new(3f, .3f, 6f));
            ramp.transform.rotation = Quaternion.Euler(-20f, 0f, 0f);
            Cube("Low tunnel roof — 1.25m clearance", new(0f, 1.5f, 3f), new(3f, .5f, 6f));
            Cube("Tunnel left", new(-1.65f, .6f, 3f), new(.3f, 1.2f, 6f));
            Cube("Tunnel right", new(1.65f, .6f, 3f), new(.3f, 1.2f, 6f));
            for (int i = 0; i < LedgeHeights.Length; i++)
            {
                float height = LedgeHeights[i];
                Cube("Jump ledge " + height + "m", new(6f + i * 4f, height * .5f, 3f), new(3f, height, 3f));
            }
            // Separate islands sit beyond the floor edge, leaving real gaps and a kill-plane fall.
            Cube("Gap start", new(0f, -.25f, 20f), new(4f, .5f, 3f));
            Cube("Gap landing", new(0f, -.25f, 25f), new(4f, .5f, 3f));
            Cube("High landing-drop platform", new(-10f, 2.75f, 13f), new(4f, 5.5f, 4f));
            for (int i = 0; i < StairCount * 2 + 4; i++)
                Cube("High platform access stair " + i, new(-15f, (i + 1) * StairRise * .5f, i * StairRun),
                    new(3f, (i + 1) * StairRise, StairRun));
            Cube("High platform bridge", new(-12.5f, 5.35f, 13f), new(3f, .3f, 3f));
            for (int i = 0; i < Tags.Length; i++)
            {
                var patch = Cube(Tags[i] + " surface", new(-12f + i * 6f, .025f, -5f), new(4f, .05f, 4f));
                patch.tag = Tags[i];
            }
            var sun = new GameObject("Course Light").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.transform.rotation = Quaternion.Euler(45f, -30f, 0f);
            RenderSettings.ambientLight = Color.gray;
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("Movement course built. Press Play, F3 for movement diagnostics. No NavMesh bake required.");
        }

        static void EnsureTags()
        {
            var tags = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var array = tags.FindProperty("tags");
            foreach (string tag in Tags)
            {
                bool found = false;
                for (int i = 0; i < array.arraySize; i++)
                    if (array.GetArrayElementAtIndex(i).stringValue == tag) { found = true; break; }
                if (found) continue;
                int index = array.arraySize;
                array.InsertArrayElementAtIndex(index);
                array.GetArrayElementAtIndex(index).stringValue = tag;
            }
            tags.ApplyModifiedPropertiesWithoutUndo();
        }

        static GameObject Cube(string name, Vector3 position, Vector3 scale)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.position = position;
            cube.transform.localScale = scale;
            return cube;
        }
    }
}
