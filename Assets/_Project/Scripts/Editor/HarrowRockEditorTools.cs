#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TheLightHouse.Editor
{
    public static class HarrowRockEditorTools
    {
        private const string MainMenuScenePath = "Assets/_Project/Scenes/MainMenu.unity";
        private const string MainMenuSceneFallback = "Assets/Scenes/MainMenu.unity";
        private const string IslandScenePath = "Assets/_Project/Scenes/Act1.unity";
        private const string IslandSceneFallback = "Assets/Scenes/HarrowRock.unity";

        // --- PLAY GAME ---
        [MenuItem("TheLightHouse/Play Game (MainMenu)", priority = 0)]
        [MenuItem("TheLightHouse/Build & Play/Play Game (MainMenu)", priority = 0)]
        public static void PlayGameFromMainMenu()
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
                Debug.Log("[HarrowRock] Stopping Play Mode.");
                return;
            }

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                string path = File.Exists(MainMenuScenePath) ? MainMenuScenePath : MainMenuSceneFallback;
                if (File.Exists(path))
                {
                    EditorSceneManager.OpenScene(path);
                    EditorApplication.isPlaying = true;
                    Debug.Log($"[HarrowRock] Opened {path} and launched Play Mode.");
                }
                else
                {
                    Debug.LogError($"[HarrowRock] Main Menu scene not found at {path}");
                }
            }
        }

        // --- QUICK SCENES ---
        [MenuItem("TheLightHouse/Quick Scenes/Open Main Menu", priority = 1)]
        public static void OpenMainMenu()
        {
            string path = File.Exists(MainMenuScenePath) ? MainMenuScenePath : MainMenuSceneFallback;
            if (File.Exists(path))
            {
                EditorSceneManager.OpenScene(path);
                Debug.Log($"[HarrowRock] Opened Main Menu scene: {path}");
            }
            else
            {
                Debug.LogError($"[HarrowRock] Main Menu scene not found at {MainMenuScenePath} or {MainMenuSceneFallback}");
            }
        }

        [MenuItem("TheLightHouse/Quick Scenes/Open Harrow Rock Island", priority = 2)]
        public static void OpenHarrowRock()
        {
            string path = File.Exists(IslandSceneFallback)
                ? IslandSceneFallback
                : (File.Exists(IslandScenePath) ? IslandScenePath : "Assets/_Project/Scenes/HarrowRock.unity");
            if (File.Exists(path))
            {
                EditorSceneManager.OpenScene(path);
                Debug.Log($"[HarrowRock] Opened Harrow Rock Island scene: {path}");
            }
            else
            {
                Debug.LogError($"[HarrowRock] Island scene not found at {path}");
            }
        }

        // --- BUILD SYSTEM ---
        [MenuItem("TheLightHouse/Build & Play/Build Full Game", priority = 2)]
        [MenuItem("TheLightHouse/Build/Build Full Game", priority = 20)]
        public static void BuildFullGame()
        {
            Debug.Log("[HarrowRock Build] Building full game scenes...");
            BuildFullGameScenes.Build();
        }

        [MenuItem("TheLightHouse/Build & Play/Build & Run Standalone (macOS)", priority = 1)]
        [MenuItem("TheLightHouse/Build/Build & Run Standalone (macOS)", priority = 21)]
        public static void BuildStandalone()
        {
            string[] preferredScenes = {
                MainMenuScenePath,
                "Assets/_Project/Scenes/Act1.unity",
                "Assets/_Project/Scenes/Act2.unity",
                "Assets/_Project/Scenes/Act3.unity",
                "Assets/_Project/Scenes/Act4.unity",
                "Assets/_Project/Scenes/Act5.unity"
            };

            string[] fallbackScenes = {
                MainMenuSceneFallback,
                IslandSceneFallback
            };

            string[] scenesToBuild = preferredScenes.Where(File.Exists).ToArray();
            if (scenesToBuild.Length == 0)
            {
                scenesToBuild = fallbackScenes.Where(File.Exists).ToArray();
            }

            Directory.CreateDirectory("Builds/macOS");
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenesToBuild,
                locationPathName = "Builds/macOS/TheLighthouse.app",
                target = BuildTarget.StandaloneOSX,
                options = BuildOptions.AutoRunPlayer
            };

            Debug.Log($"[HarrowRock Build] Compiling standalone macOS build with {scenesToBuild.Length} scenes...");
            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            Debug.Log($"[HarrowRock Build] Standalone build finished with result: {report.summary.result}");
        }

        // --- QA & VALIDATION ---
        [MenuItem("TheLightHouse/QA & Validation/Run Full Automated QA Playtest", priority = 40)]
        public static void RunQAPlaytest()
        {
            Debug.Log("[HarrowRock QA] Starting automated sequence playtest...");
            var report = AutomatedPlaytestQA.RunFullPlaytest();
            int passed = report.PhaseStatus.Count(kv => kv.Value);
            int total = report.PhaseStatus.Count;
            Debug.Log($"[HarrowRock QA] Playtest Complete: {passed}/{total} phases passed. Identified bugs: {report.IdentifiedBugs.Count}");
        }

        [MenuItem("TheLightHouse/QA & Validation/Verify Collisions & Doorways", priority = 41)]
        public static void VerifyCollisions()
        {
            Debug.Log("[HarrowRock QA] Auditing door clearances, stair ramp colliders, and water boundaries.");
            string result = ArchitectureTraversalValidation.Inspect();
            Debug.Log($"[HarrowRock QA] Collision & Traversal Audit Result:\n{result}");
        }

        [MenuItem("TheLightHouse/QA & Validation/Capture Validation Screenshots", priority = 42)]
        public static void CaptureValidationScreenshots()
        {
            Debug.Log("[HarrowRock QA] Capturing validation screenshot suite...");
            LevelPreviewCapture.CaptureAll();
            Debug.Log("[HarrowRock QA] Validation screenshots captured to docs/level-previews/");
        }

        // --- ENVIRONMENT & QA ---
        [MenuItem("TheLightHouse/Environment/Setup Active Scene Environment", priority = 30)]
        public static void SetupActiveSceneEnvironment()
        {
            Debug.Log("[HarrowRock] Setting up active scene environment & collision overhaul...");
            ArchitectureCollisionOverhaul.ApplyCurrent();
            Debug.Log("[HarrowRock] Active scene environment setup complete.");
        }

        [MenuItem("TheLightHouse/QA & Validation/Capture QA Validation Suite", priority = 43)]
        public static void CaptureQAValidationSuite()
        {
            Debug.Log("[HarrowRock QA] Capturing complete QA validation suite...");
            LevelPreviewCapture.CaptureAll();
            Debug.Log("[HarrowRock QA] QA validation suite captured to docs/level-previews/");
        }

        // --- QUICK TELEPORT (DEBUG IN PLAY MODE) ---
        [MenuItem("TheLightHouse/Quick Teleport/Dock (Arrival)", priority = 60)]
        public static void TeleportToDock() => TeleportPlayer(new Vector3(0f, 1.4f, -25f));

        [MenuItem("TheLightHouse/Quick Teleport/Keeper's Cottage", priority = 61)]
        public static void TeleportToCottage() => TeleportPlayer(new Vector3(-9.5f, 1.0f, -6.8f));

        [MenuItem("TheLightHouse/Quick Teleport/Tower Study (Floor 2)", priority = 62)]
        public static void TeleportToStudy() => TeleportPlayer(new Vector3(0f, 4.2f, 0.5f));

        [MenuItem("TheLightHouse/Quick Teleport/Subterranean Cellar", priority = 63)]
        public static void TeleportToCellar() => TeleportPlayer(new Vector3(-8.0f, -3.2f, 4.0f));

        [MenuItem("TheLightHouse/Quick Teleport/Lantern Room (Beacon)", priority = 64)]
        public static void TeleportToLantern() => TeleportPlayer(new Vector3(0f, 11.2f, 0f));

        private static void TeleportPlayer(Vector3 targetPosition)
        {
            if (!EditorApplication.isPlaying)
            {
                Debug.LogWarning("Teleportation only works during Play Mode.");
                return;
            }

            GameObject player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                var fpc = Object.FindAnyObjectByType<FirstPersonController>();
                if (fpc != null) player = fpc.gameObject;
            }

            if (player != null)
            {
                CharacterController cc = player.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;
                player.transform.position = targetPosition;
                if (cc != null) cc.enabled = true;
                Debug.Log($"Teleported player to {targetPosition}");
            }
            else
            {
                Debug.LogWarning("Could not find Player in the scene.");
            }
        }
    }
}
#endif
