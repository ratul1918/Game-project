using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace TheLightHouse.Editor
{
    public static class AutomatedPlaytestQA
    {
        public sealed class QAReport
        {
            public List<string> IdentifiedBugs = new();
            public List<string> AppliedFixes = new();
            public Dictionary<string, bool> PhaseStatus = new();
            public List<string> Logs = new();
        }

        [MenuItem("TheLightHouse/Run Full Automated QA Playtest")]
        public static void RunFullPlaytestMenu()
        {
            var report = RunFullPlaytest();
            Debug.Log($"<color=#44FF88>[QA Playtest Complete]</color>\n" +
                      $"Phases Passed: {report.PhaseStatus.Count(kv => kv.Value)}/{report.PhaseStatus.Count}\n" +
                      $"Identified Bugs: {report.IdentifiedBugs.Count}\n" +
                      $"Applied Fixes: {report.AppliedFixes.Count}");
        }

        public static QAReport RunFullPlaytest()
        {
            var report = new QAReport();

            try
            {
                // Phase 1: Main Menu & Boot Flow
                TestPhase1_MainMenu(report);

                // Phase 2: Act 1 - Arrival at Harrow Rock (Dusk)
                TestPhase2_Act1(report);

                // Phase 3: Act 2 - Keeper's House & Stalker Herding
                TestPhase3_Act2(report);

                // Phase 4 & 5: Act 3 - Ascending Tower, Generator Cut, Hand-Crank, Stalker & Shallows Figures
                TestPhase4And5_Act3(report);

                // Phase 6: Act 4 - Cellar, Recording & Harbor Ledger
                TestPhase6_Act4(report);

                // Phase 7: Act 5 - Climax, Courtyard Flood, Father's Sacrifice, Morning Boat Escape
                TestPhase7_Act5(report);

                // Return to Main Menu as default editor active scene
                EditorSceneManager.OpenScene("Assets/_Project/Scenes/MainMenu.unity", OpenSceneMode.Single);
            }
            catch (Exception ex)
            {
                report.IdentifiedBugs.Add($"Global QA Exception: {ex.Message}\n{ex.StackTrace}");
            }

            return report;
        }

        public static void TestPhase1_MainMenu(QAReport report)
        {
            const string scenePath = "Assets/_Project/Scenes/MainMenu.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            bool phasePassed = true;

            // 1. Check MainMenu component
            var menu = Object.FindAnyObjectByType<MainMenu>();
            if (!menu)
            {
                report.IdentifiedBugs.Add("[MainMenu] MainMenu component missing on canvas.");
                phasePassed = false;
            }

            // 2. Event System & Input System UI Module
            var eventSystem = Object.FindAnyObjectByType<EventSystem>();
            if (!eventSystem)
            {
                report.IdentifiedBugs.Add("[MainMenu] EventSystem missing.");
                new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                report.AppliedFixes.Add("[MainMenu] Instantiated EventSystem with InputSystemUIInputModule.");
            }
            else
            {
                var inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
                if (!inputModule)
                {
                    eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
                    report.AppliedFixes.Add("[MainMenu] Added InputSystemUIInputModule to EventSystem.");
                }
            }

            // 3. UI Buttons
            string[] requiredButtons = { "New Game", "Continue", "Chapter Select", "Settings", "Extras", "Credits", "Quit" };
            var allButtons = Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var req in requiredButtons)
            {
                var btn = allButtons.FirstOrDefault(b => b.name.IndexOf(req, StringComparison.OrdinalIgnoreCase) >= 0);
                if (!btn)
                {
                    report.IdentifiedBugs.Add($"[MainMenu] Required menu button '{req}' missing.");
                    phasePassed = false;
                }
                else
                {
                    // Ensure Button has MenuButtonAudio component
                    if (!btn.GetComponent<MenuButtonAudio>())
                    {
                        btn.gameObject.AddComponent<MenuButtonAudio>();
                        report.AppliedFixes.Add($"[MainMenu] Attached MenuButtonAudio to button '{btn.name}'.");
                    }
                }
            }

            // 4. Viewport Camera, Beacon and Audio
            var camCtrl = Object.FindAnyObjectByType<MenuCameraController>();
            if (!camCtrl)
            {
                report.IdentifiedBugs.Add("[MainMenu] MenuCameraController missing on camera.");
                phasePassed = false;
            }

            var ocean = GameObject.Find("Dynamic Ocean Surface");
            if (!ocean || !ocean.GetComponent<MeshRenderer>() || !ocean.GetComponent<MeshRenderer>().sharedMaterial)
            {
                report.IdentifiedBugs.Add("[MainMenu] Dynamic Ocean Surface missing or unassigned.");
                phasePassed = false;
            }

            var rain = GameObject.Find("Viewport Driving Rain Particles");
            if (!rain || !rain.GetComponent<ParticleSystem>())
            {
                report.IdentifiedBugs.Add("[MainMenu] Viewport rain particle system missing.");
                phasePassed = false;
            }
            else
            {
                var ps = rain.GetComponent<ParticleSystem>();
                var vel = ps.velocityOverLifetime;
                // Verify rain drops downward toward ground (vel.y < 0)
                if (vel.y.constantMin >= 0 || vel.y.constantMax >= 0)
                {
                    report.IdentifiedBugs.Add("[MainMenu] Rain particles flying upward toward sky instead of dropping to ground.");
                    vel.y = new ParticleSystem.MinMaxCurve(-20f, -25f);
                    report.AppliedFixes.Add("[MainMenu] Corrected rain velocityOverLifetime to drop downwards toward ground.");
                }
            }

            // 5. Verify New Game Flow
            var flow = Object.FindAnyObjectByType<ActFlowController>();
            if (!flow)
            {
                report.IdentifiedBugs.Add("[MainMenu] ActFlowController missing.");
                phasePassed = false;
            }

            AuditSceneMaterialsAndRenderers("MainMenu", report);

            report.PhaseStatus["Phase 1: Main Menu & Boot Flow"] = phasePassed;
            if (scene.isDirty) EditorSceneManager.SaveScene(scene);
        }

        public static void TestPhase2_Act1(QAReport report)
        {
            const string scenePath = "Assets/_Project/Scenes/Act1.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            bool phasePassed = true;

            // 1. Player check
            var player = GameObject.Find("Player");
            if (!player)
            {
                report.IdentifiedBugs.Add("[Act 1] Player GameObject missing.");
                phasePassed = false;
            }
            else
            {
                var cc = player.GetComponent<CharacterController>();
                if (!cc)
                {
                    report.IdentifiedBugs.Add("[Act 1] CharacterController missing on Player.");
                    phasePassed = false;
                }
                else if (cc.stepOffset < 0.25f)
                {
                    cc.stepOffset = 0.3f;
                    report.AppliedFixes.Add("[Act 1] Increased Player CharacterController stepOffset to 0.3m for smooth stair climbing.");
                }

                if (!player.GetComponent<FirstPersonController>())
                {
                    report.IdentifiedBugs.Add("[Act 1] FirstPersonController missing on Player.");
                    phasePassed = false;
                }
                if (!player.GetComponent<Inventory>())
                {
                    player.AddComponent<Inventory>();
                    report.AppliedFixes.Add("[Act 1] Attached Inventory to Player.");
                }
            }

            // 2. Boatman narrative check
            var boatman = Object.FindAnyObjectByType<Boatman>();
            if (!boatman)
            {
                report.IdentifiedBugs.Add("[Act 1] Boatman component missing.");
                phasePassed = false;
            }

            // 3. Narrative interactables
            RequireComponentWithCollider<MealTable>("Act 1", "MealTable", report, ref phasePassed);
            RequireComponentWithCollider<Radio>("Act 1", "Radio", report, ref phasePassed);
            RequireComponentWithCollider<FuelCan>("Act 1", "FuelCan", report, ref phasePassed);
            RequireComponentWithCollider<LanternInteract>("Act 1", "LanternInteract", report, ref phasePassed);

            var door = GameObject.Find("Tower Door")?.GetComponent<Door>();
            if (!door)
            {
                report.IdentifiedBugs.Add("[Act 1] Tower Door component missing.");
                phasePassed = false;
            }

            // 4. StoryBeats validation (A1-B01 through A1-B06)
            ValidateStoryDirector("Act 1", new[] { "A1-B01", "A1-B02", "A1-B03", "A1-B04", "A1-B05", "A1-B06" }, report, ref phasePassed);

            // 5. NavMesh validation
            AuditNavMesh("Act 1", 1, report);

            // 6. Shader/Material audit
            AuditSceneMaterialsAndRenderers("Act 1", report);

            // 7. EventSystem and PauseMenu UI audit
            EnsureEventSystemInScene("Act 1", report);
            AuditPauseMenuAndButtons("Act 1", report, ref phasePassed);

            report.PhaseStatus["Phase 2: Arrival at Harrow Rock (Dusk)"] = phasePassed;
            if (scene.isDirty) EditorSceneManager.SaveScene(scene);
        }

        public static void TestPhase3_Act2(QAReport report)
        {
            const string scenePath = "Assets/_Project/Scenes/Act2.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            bool phasePassed = true;

            // 1. Night state & lamp fuel drain
            var state = Object.FindAnyObjectByType<GameStateMachine>();
            if (!state)
            {
                report.IdentifiedBugs.Add("[Act 2] GameStateMachine missing.");
                phasePassed = false;
            }

            var lamp = Object.FindAnyObjectByType<LampController>();
            if (!lamp)
            {
                report.IdentifiedBugs.Add("[Act 2] LampController missing.");
                phasePassed = false;
            }

            // 2. Wet footprints & cellar hum
            var trail = Object.FindAnyObjectByType<WetFootprints>();
            if (!trail)
            {
                report.IdentifiedBugs.Add("[Act 2] WetFootprints missing.");
                phasePassed = false;
            }

            var hum = Object.FindAnyObjectByType<CellarHum>();
            if (!hum)
            {
                report.IdentifiedBugs.Add("[Act 2] CellarHum missing.");
                phasePassed = false;
            }

            // 3. Stalker & CraneHerding
            var stalker = GameObject.Find("Stalker") ?? Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(g => g.name == "Stalker" && g.scene.isLoaded);
            if (!stalker)
            {
                report.IdentifiedBugs.Add("[Act 2] Stalker GameObject missing.");
                phasePassed = false;
            }
            else
            {
                if (!stalker.GetComponent<StalkerAI>()) report.IdentifiedBugs.Add("[Act 2] StalkerAI missing on Stalker.");
                if (!stalker.GetComponent<CraneHerding>()) report.IdentifiedBugs.Add("[Act 2] CraneHerding missing on Stalker.");
            }

            // 4. Hiding spots
            var hidingSpots = Object.FindObjectsByType<HidingSpot>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (hidingSpots.Length < 3)
            {
                report.IdentifiedBugs.Add($"[Act 2] Expected at least 3 hiding spots; found {hidingSpots.Length}.");
                phasePassed = false;
            }

            // 5. StoryBeats validation (A2-B01 through A2-B07)
            ValidateStoryDirector("Act 2", new[] { "A2-B01", "A2-B02", "A2-B03", "A2-B04", "A2-B05", "A2-B06", "A2-B07" }, report, ref phasePassed);

            AuditNavMesh("Act 2", 2, report);
            AuditSceneMaterialsAndRenderers("Act 2", report);
            EnsureEventSystemInScene("Act 2", report);
            AuditPauseMenuAndButtons("Act 2", report, ref phasePassed);

            report.PhaseStatus["Phase 3: The Keeper’s House & Radio Transmission"] = phasePassed;
            if (scene.isDirty) EditorSceneManager.SaveScene(scene);
        }

        public static void TestPhase4And5_Act3(QAReport report)
        {
            const string scenePath = "Assets/_Project/Scenes/Act3.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            bool phasePassed = true;

            // 1. Storm Controller & Storm Settings
            var storm = Object.FindAnyObjectByType<StormController>();
            if (!storm)
            {
                report.IdentifiedBugs.Add("[Act 3] StormController missing.");
                phasePassed = false;
            }

            // 2. Generator Cut Event
            var gen = Object.FindAnyObjectByType<Generator>();
            if (!gen)
            {
                report.IdentifiedBugs.Add("[Act 3] Generator missing.");
                phasePassed = false;
            }

            // 3. Hand-crank mechanic
            var crank = Object.FindAnyObjectByType<CrankStation>();
            if (!crank)
            {
                report.IdentifiedBugs.Add("[Act 3] CrankStation missing.");
                phasePassed = false;
            }

            // 4. Stalker AI in storm mode
            var stalker = GameObject.Find("Stalker") ?? Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(g => g.name == "Stalker" && g.scene.isLoaded);
            if (!stalker || !stalker.GetComponent<StalkerAI>())
            {
                report.IdentifiedBugs.Add("[Act 3] StalkerAI missing on Stalker in Act 3.");
                phasePassed = false;
            }

            // 5. Drowned Figures & Weeping Angel Manager
            var drownedMgr = Object.FindAnyObjectByType<DrownedManager>();
            if (!drownedMgr)
            {
                report.IdentifiedBugs.Add("[Act 3] DrownedManager missing.");
                phasePassed = false;
            }
            var figures = Object.FindObjectsByType<DrownedFigure>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (figures.Length < 4)
            {
                report.IdentifiedBugs.Add($"[Act 3] Expected at least 4 Drowned figures in shallows; found {figures.Length}.");
                phasePassed = false;
            }

            // 6. StoryBeats validation (A3-B01 through A3-B09)
            ValidateStoryDirector("Act 3", new[] { "A3-B01", "A3-B02", "A3-B03", "A3-B04", "A3-B05", "A3-B06", "A3-B07", "A3-B08", "A3-B09" }, report, ref phasePassed);

            AuditNavMesh("Act 3", 3, report);
            AuditSceneMaterialsAndRenderers("Act 3", report);
            EnsureEventSystemInScene("Act 3", report);
            AuditPauseMenuAndButtons("Act 3", report, ref phasePassed);

            report.PhaseStatus["Phase 4 & 5: Ascending Tower, Generator Cut, Hand-Crank & Shallows Figures"] = phasePassed;
            if (scene.isDirty) EditorSceneManager.SaveScene(scene);
        }

        public static void TestPhase6_Act4(QAReport report)
        {
            const string scenePath = "Assets/_Project/Scenes/Act4.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            bool phasePassed = true;

            // 1. Hidden cellar room
            var hiddenRoom = Object.FindAnyObjectByType<HiddenCellarRoom>();
            if (!hiddenRoom)
            {
                report.IdentifiedBugs.Add("[Act 4] HiddenCellarRoom missing.");
                phasePassed = false;
            }

            // 2. Harbor ledger
            var ledger = Object.FindAnyObjectByType<Ledger>();
            if (!ledger)
            {
                report.IdentifiedBugs.Add("[Act 4] Harbor Ledger missing.");
                phasePassed = false;
            }

            // 3. Locked room door & family photograph
            var lockedDoor = Object.FindAnyObjectByType<LockedRoomDoor>();
            if (!lockedDoor)
            {
                report.IdentifiedBugs.Add("[Act 4] LockedRoomDoor missing.");
                phasePassed = false;
            }

            var photo = Object.FindAnyObjectByType<FamilyPhoto>();
            if (!photo)
            {
                report.IdentifiedBugs.Add("[Act 4] FamilyPhoto missing.");
                phasePassed = false;
            }

            // 4. Logbook Journal & UI Reader
            var journal = Object.FindAnyObjectByType<LogbookJournal>();
            var reader = Object.FindAnyObjectByType<LogbookReaderUI>();
            if (!journal || !reader)
            {
                report.IdentifiedBugs.Add("[Act 4] LogbookJournal or LogbookReaderUI missing.");
                phasePassed = false;
            }

            // 5. StoryBeats validation (A4-B01 through A4-B06)
            ValidateStoryDirector("Act 4", new[] { "A4-B01", "A4-B02", "A4-B03", "A4-B04", "A4-B05", "A4-B06" }, report, ref phasePassed);

            AuditNavMesh("Act 4", 4, report);
            AuditSceneMaterialsAndRenderers("Act 4", report);
            EnsureEventSystemInScene("Act 4", report);
            AuditPauseMenuAndButtons("Act 4", report, ref phasePassed);

            report.PhaseStatus["Phase 6: The Cellar, The Recording & The Harbor Ledger"] = phasePassed;
            if (scene.isDirty) EditorSceneManager.SaveScene(scene);
        }

        public static void TestPhase7_Act5(QAReport report)
        {
            const string scenePath = "Assets/_Project/Scenes/Act5.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            bool phasePassed = true;

            // 1. Crane Finale AI & Doorway Block
            var craneFinale = Object.FindAnyObjectByType<CraneFinale>();
            if (!craneFinale)
            {
                report.IdentifiedBugs.Add("[Act 5] CraneFinale component missing.");
                phasePassed = false;
            }

            var doorway = Object.FindAnyObjectByType<DoorwayBlock>();
            if (!doorway)
            {
                report.IdentifiedBugs.Add("[Act 5] DoorwayBlock component missing.");
                phasePassed = false;
            }

            // 2. Shoreline Ending & Lullaby
            var shoreline = Object.FindAnyObjectByType<ShorelineEnding>();
            if (!shoreline)
            {
                report.IdentifiedBugs.Add("[Act 5] ShorelineEnding missing.");
                phasePassed = false;
            }

            // 3. Morning Boat Departure & Ending UI
            var boatEnding = Object.FindAnyObjectByType<BoatEnding>();
            if (!boatEnding)
            {
                report.IdentifiedBugs.Add("[Act 5] BoatEnding missing.");
                phasePassed = false;
            }

            var endingUI = Object.FindAnyObjectByType<EndingUI>();
            if (!endingUI)
            {
                report.IdentifiedBugs.Add("[Act 5] EndingUI missing.");
                phasePassed = false;
            }

            // 4. Finale Director & Dawn Controller
            var finaleDir = Object.FindAnyObjectByType<FinaleDirector>();
            if (!finaleDir)
            {
                report.IdentifiedBugs.Add("[Act 5] FinaleDirector missing.");
                phasePassed = false;
            }

            var dawn = Object.FindAnyObjectByType<DawnController>();
            if (!dawn)
            {
                report.IdentifiedBugs.Add("[Act 5] DawnController missing.");
                phasePassed = false;
            }

            // 5. StoryBeats validation (A5-B01 through A5-B06)
            ValidateStoryDirector("Act 5", new[] { "A5-B01", "A5-B02", "A5-B03", "A5-B04", "A5-B05", "A5-B06" }, report, ref phasePassed);

            AuditNavMesh("Act 5", 5, report);
            AuditSceneMaterialsAndRenderers("Act 5", report);
            EnsureEventSystemInScene("Act 5", report);
            AuditPauseMenuAndButtons("Act 5", report, ref phasePassed);

            report.PhaseStatus["Phase 7: Climax, Father's Sacrifice & Morning Boat Escape"] = phasePassed;
            if (scene.isDirty) EditorSceneManager.SaveScene(scene);
        }

        private static void RequireComponentWithCollider<T>(string act, string name, QAReport report, ref bool phasePassed) where T : Component
        {
            var comp = Object.FindAnyObjectByType<T>();
            if (!comp)
            {
                report.IdentifiedBugs.Add($"[{act}] {name} component missing.");
                phasePassed = false;
                return;
            }

            var col = comp.GetComponent<Collider>() ?? comp.GetComponentInChildren<Collider>();
            if (!col)
            {
                var box = comp.gameObject.AddComponent<BoxCollider>();
                box.isTrigger = true;
                box.size = new Vector3(1.5f, 1.5f, 1.5f);
                report.AppliedFixes.Add($"[{act}] Added trigger BoxCollider to {name}.");
            }
        }

        private static void ValidateStoryDirector(string act, string[] expectedIds, QAReport report, ref bool phasePassed)
        {
            var director = Object.FindAnyObjectByType<StoryDirector>();
            if (!director)
            {
                report.IdentifiedBugs.Add($"[{act}] StoryDirector missing.");
                phasePassed = false;
                return;
            }

            var so = new SerializedObject(director);
            var beatsProp = so.FindProperty("beats");
            var optProp = so.FindProperty("optionalBeats");

            var presentIds = new HashSet<string>();
            for (int i = 0; i < beatsProp.arraySize; i++)
            {
                var beat = beatsProp.GetArrayElementAtIndex(i).objectReferenceValue as StoryBeat;
                if (beat) presentIds.Add(beat.id);
            }
            if (optProp != null)
            {
                for (int i = 0; i < optProp.arraySize; i++)
                {
                    var beat = optProp.GetArrayElementAtIndex(i).objectReferenceValue as StoryBeat;
                    if (beat) presentIds.Add(beat.id);
                }
            }

            foreach (var req in expectedIds)
            {
                if (!presentIds.Contains(req))
                {
                    report.IdentifiedBugs.Add($"[{act}] Required StoryBeat '{req}' not found in StoryDirector.");
                    phasePassed = false;
                }
            }
        }

        private static void AuditNavMesh(string act, int actNum, QAReport report)
        {
            var surfaces = Object.FindObjectsByType<NavMeshSurface>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (surfaces.Length == 0)
            {
                report.IdentifiedBugs.Add($"[{act}] No NavMeshSurface found.");
                return;
            }

            foreach (var s in surfaces)
            {
                if (s.navMeshData == null)
                {
                    s.BuildNavMesh();
                    string dir = $"Assets/_Project/Scenes/Act{actNum}";
                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                    string assetPath = $"{dir}/NavMesh-{s.gameObject.name}.asset";
                    if (s.navMeshData != null)
                    {
                        AssetDatabase.CreateAsset(s.navMeshData, assetPath);
                        EditorUtility.SetDirty(s);
                        report.AppliedFixes.Add($"[{act}] Baked and saved NavMeshData for surface '{s.gameObject.name}' at {assetPath}.");
                    }
                }
            }
        }

        private static void AuditSceneMaterialsAndRenderers(string sceneName, QAReport report)
        {
            var renderers = Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var r in renderers)
            {
                if (r.sharedMaterials == null || r.sharedMaterials.Length == 0)
                {
                    report.IdentifiedBugs.Add($"[{sceneName}] {r.name} has no materials.");
                    continue;
                }

                for (int m = 0; m < r.sharedMaterials.Length; m++)
                {
                    var mat = r.sharedMaterials[m];
                    if (!mat)
                    {
                        report.IdentifiedBugs.Add($"[{sceneName}] {r.name} has null material slot {m}.");
                    }
                    else if (!mat.shader || mat.shader.name == "Hidden/InternalErrorShader")
                    {
                        report.IdentifiedBugs.Add($"[{sceneName}] {r.name} has broken/pink shader ({mat.name}).");
                        mat.shader = Shader.Find("Universal Render Pipeline/Lit");
                        report.AppliedFixes.Add($"[{sceneName}] Repaired shader on {r.name} material ({mat.name}) to URP/Lit.");
                    }
                }
            }
        }

        private static void EnsureEventSystemInScene(string act, QAReport report)
        {
            var scene = EditorSceneManager.GetActiveScene();
            var eventSystem = Object.FindAnyObjectByType<EventSystem>();
            if (!eventSystem)
            {
                report.IdentifiedBugs.Add($"[{act}] EventSystem missing.");
                var es = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                var mod = es.GetComponent<InputSystemUIInputModule>();
                mod?.AssignDefaultActions();
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                report.AppliedFixes.Add($"[{act}] Instantiated EventSystem with InputSystemUIInputModule and saved scene.");
            }
            else
            {
                var inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
                if (!inputModule)
                {
                    inputModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
                    inputModule.AssignDefaultActions();
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                    report.AppliedFixes.Add($"[{act}] Added InputSystemUIInputModule to EventSystem and saved scene.");
                }
            }
        }

        private static void AuditPauseMenuAndButtons(string act, QAReport report, ref bool phasePassed)
        {
            var pause = Object.FindAnyObjectByType<PauseMenu>();
            if (!pause)
            {
                report.IdentifiedBugs.Add($"[{act}] PauseMenu component missing.");
                phasePassed = false;
                return;
            }

            var canvas = pause.GetComponent<Canvas>() ?? pause.GetComponentInParent<Canvas>();
            if (canvas)
            {
                var gr = canvas.GetComponent<GraphicRaycaster>();
                if (!gr)
                {
                    canvas.gameObject.AddComponent<GraphicRaycaster>();
                    report.AppliedFixes.Add($"[{act}] Added GraphicRaycaster to Canvas for PauseMenu button clicks.");
                }
            }

            var buttons = pause.GetComponentsInChildren<Button>(true);
            string[] required = { "Resume", "Journal", "Options", "Save", "Quit to MainMenu" };
            foreach (var req in required)
            {
                var btn = buttons.FirstOrDefault(b => b.name.IndexOf(req, StringComparison.OrdinalIgnoreCase) >= 0);
                if (!btn)
                {
                    report.IdentifiedBugs.Add($"[{act}] PauseMenu button '{req}' missing.");
                    phasePassed = false;
                }
                else
                {
                    if (btn.onClick.GetPersistentEventCount() == 0)
                    {
                        report.IdentifiedBugs.Add($"[{act}] PauseMenu button '{req}' has no onClick event listener.");
                        phasePassed = false;
                    }
                    if (btn.targetGraphic != null && !btn.targetGraphic.raycastTarget)
                    {
                        btn.targetGraphic.raycastTarget = true;
                        report.AppliedFixes.Add($"[{act}] Enabled raycastTarget on PauseMenu button '{req}'.");
                    }
                }
            }
        }
    }
}
