using System.Collections.Generic;
using System.IO;
using UnityEditor;
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

        [MenuItem("TheLightHouse/Build Full Game Scenes")]
        public static void Build()
        {
            BuildAct1Scene.BuildTo("Assets/_Project/Scenes/Act1.unity");
            BuildAct2Scene.BuildTo("Assets/_Project/Scenes/Act2.unity");
            BuildAct3Scene.BuildTo("Assets/_Project/Scenes/Act3.unity");
            BuildAct4Scene.BuildTo("Assets/_Project/Scenes/Act4.unity");
            BuildAct5Scene.BuildTo("Assets/_Project/Scenes/Act5.unity");
            ApplyGraphics1.Apply();
            ReferenceVisualPass.ApplyAll();
            IslandArrivalPass.ApplyAll();
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

        [MenuItem("TheLightHouse/Build Main Menu")]
        public static void BuildMenu()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Build 3D Cinematic Environment
            var envRoot = new GameObject("Environment 3D");

            var matPlaster = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Reference/Salt Plaster.mat");
            var matShale = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Reference/Wet Shale.mat");
            var matSea = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Reference/Harrow Sea.mat") ?? matShale;
            var matIron = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Reference/Oxidised Iron.mat");
            var matBeaconGlass = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Reference/Beacon Glass.mat");
            var matCottageGlass = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Reference/Warm Cottage Glass.mat");
            var matSlate = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Reference/Slate.mat");
            var matRain = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Reference/Rain.mat");
            var matGlow = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Reference/Dock Lantern Glow.mat");
            var matFoam = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Reference/Sea Spray Foam.mat");

            // Sea Surface
            var sea = GameObject.CreatePrimitive(PrimitiveType.Plane);
            sea.name = "Ocean Surface";
            sea.transform.SetParent(envRoot.transform);
            sea.transform.position = new Vector3(0, -1f, 15f);
            sea.transform.localScale = new Vector3(30f, 1f, 30f);
            if (matSea) sea.GetComponent<Renderer>().sharedMaterial = matSea;

            // Harrow Rock Island Bluff (Wavy Organic Coastal Rock Promontory)
            CreateOrganicWavyBluff(envRoot.transform, matShale, matFoam);

            // Lighthouse Masonry Tower
            var tower = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tower.name = "Lighthouse Tower";
            tower.transform.SetParent(envRoot.transform);
            tower.transform.position = new Vector3(10f, 15f, 22f);
            tower.transform.localScale = new Vector3(5.5f, 22f, 5.5f);
            if (matPlaster) tower.GetComponent<Renderer>().sharedMaterial = matPlaster;

            // Lantern Gallery Platform
            var gallery = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            gallery.name = "Lantern Gallery";
            gallery.transform.SetParent(envRoot.transform);
            gallery.transform.position = new Vector3(10f, 26.2f, 22f);
            gallery.transform.localScale = new Vector3(7.5f, 0.4f, 7.5f);
            if (matIron) gallery.GetComponent<Renderer>().sharedMaterial = matIron;

            // Lantern Glass Chamber
            var chamber = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            chamber.name = "Beacon Chamber";
            chamber.transform.SetParent(envRoot.transform);
            chamber.transform.position = new Vector3(10f, 28.2f, 22f);
            chamber.transform.localScale = new Vector3(4.8f, 3.5f, 4.8f);
            if (matBeaconGlass) chamber.GetComponent<Renderer>().sharedMaterial = matBeaconGlass;

            // Lantern Cap Dome
            var dome = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            dome.name = "Lantern Dome";
            dome.transform.SetParent(envRoot.transform);
            dome.transform.position = new Vector3(10f, 30.2f, 22f);
            dome.transform.localScale = new Vector3(5.2f, 0.8f, 5.2f);
            if (matSlate) dome.GetComponent<Renderer>().sharedMaterial = matSlate;

            // Connected Keeper's Cottage
            var cottage = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cottage.name = "Keeper Cottage";
            cottage.transform.SetParent(envRoot.transform);
            cottage.transform.position = new Vector3(2f, 6f, 20f);
            cottage.transform.localScale = new Vector3(9f, 4.8f, 7.5f);
            if (matPlaster) cottage.GetComponent<Renderer>().sharedMaterial = matPlaster;

            var cottageRoof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cottageRoof.name = "Cottage Slate Roof";
            cottageRoof.transform.SetParent(envRoot.transform);
            cottageRoof.transform.position = new Vector3(2f, 8.8f, 20f);
            cottageRoof.transform.localScale = new Vector3(10f, 1.6f, 8.5f);
            if (matSlate) cottageRoof.GetComponent<Renderer>().sharedMaterial = matSlate;

            // Warm Glowing Cottage Window
            var window = GameObject.CreatePrimitive(PrimitiveType.Quad);
            window.name = "Warm Window";
            window.transform.SetParent(envRoot.transform);
            window.transform.position = new Vector3(2f, 6.2f, 16.2f);
            window.transform.localScale = new Vector3(1.8f, 1.8f, 1f);
            if (matCottageGlass) window.GetComponent<Renderer>().sharedMaterial = matCottageGlass;

            var windowLight = new GameObject("Cottage Warm Glow").AddComponent<Light>();
            windowLight.transform.SetParent(envRoot.transform);
            windowLight.transform.position = new Vector3(2f, 6.2f, 15f);
            windowLight.type = LightType.Point;
            windowLight.color = new Color(1f, 0.72f, 0.35f);
            windowLight.range = 14f;
            windowLight.intensity = 2.5f;

            // Rotating Lighthouse Beacon Beam
            var beaconPivot = new GameObject("Lighthouse Beacon Pivot");
            beaconPivot.transform.SetParent(envRoot.transform);
            beaconPivot.transform.position = new Vector3(10f, 28.2f, 22f);

            var beamLight = new GameObject("Beacon Spotlight").AddComponent<Light>();
            beamLight.transform.SetParent(beaconPivot.transform);
            beamLight.transform.localPosition = Vector3.zero;
            beamLight.transform.localRotation = Quaternion.Euler(14f, 0f, 0f);
            beamLight.type = LightType.Spot;
            beamLight.color = new Color(1f, 0.92f, 0.65f);
            beamLight.range = 140f;
            beamLight.spotAngle = 45f;
            beamLight.innerSpotAngle = 20f;
            beamLight.intensity = 15f;
            beamLight.shadows = LightShadows.Soft;

            // Ambient Moonlight
            var moon = new GameObject("Dusk Moonlight").AddComponent<Light>();
            moon.transform.SetParent(envRoot.transform);
            moon.type = LightType.Directional;
            moon.transform.rotation = Quaternion.Euler(35f, -60f, 0f);
            moon.color = new Color(0.18f, 0.28f, 0.38f);
            moon.intensity = 0.5f;
            moon.shadows = LightShadows.Soft;

            // Distant Lightning Light
            var lightning = new GameObject("Distant Lightning").AddComponent<Light>();
            lightning.transform.SetParent(envRoot.transform);
            lightning.type = LightType.Directional;
            lightning.transform.rotation = Quaternion.Euler(25f, 130f, 0f);
            lightning.color = new Color(0.75f, 0.88f, 1.0f);
            lightning.intensity = 0f;

            // Rain Particle System
            var rainGo = new GameObject("Menu Rain Particles");
            rainGo.transform.SetParent(envRoot.transform);
            rainGo.transform.position = new Vector3(0, 25f, 10f);
            var ps = rainGo.AddComponent<ParticleSystem>();
            var psRenderer = rainGo.GetComponent<ParticleSystemRenderer>();
            if (matRain) psRenderer.sharedMaterial = matRain;
            var main = ps.main;
            main.startSpeed = 22f;
            main.startLifetime = 1.8f;
            main.startSize = 0.08f;
            main.maxParticles = 800;
            main.startColor = new Color(1f, 1f, 1f, 0.45f);
            var emission = ps.emission;
            emission.rateOverTime = 350f;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(40f, 1f, 35f);

            // Post-Processing Volume
            var volProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>("Assets/_Project/Settings/Volume_Night.asset");
            if (volProfile)
            {
                var volGo = new GameObject("Global PostProcess Volume").AddComponent<Volume>();
                volGo.transform.SetParent(envRoot.transform);
                volGo.isGlobal = true;
                volGo.profile = volProfile;
            }

            // 2. Camera Setup
            var cameraObject = new GameObject("Menu Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(-3f, 4.5f, -10f);
            cameraObject.transform.rotation = Quaternion.Euler(-8f, 24f, 0f);

            var camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.015f, 0.022f, 0.032f, 1f);
            camera.cullingMask = ~0;
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 180f;

            var camSway = cameraObject.AddComponent<MenuCameraController>();
            Set(camSway, "swayAmount", 0.3f);
            Set(camSway, "swaySpeed", 0.35f);
            Set(camSway, "lighthouseBeam", beaconPivot.transform);
            Set(camSway, "lightningLight", lightning);

            // 3. Act Flow & Save System
            var flowObject = new GameObject("Act Flow");
            var flow = flowObject.AddComponent<ActFlowController>();
            Set(flow, "enableSceneChaining", true);
            var saves = flowObject.AddComponent<SaveSystem>();
            var debug = flowObject.AddComponent<GameDebugMenu>();
            Set(debug, "flow", flow);
            Set(debug, "inputActions", AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions"));

            // 4. UI Canvas
            var canvasObject = new GameObject("Main Menu Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = .5f;

            var menu = canvasObject.AddComponent<MainMenu>();
            Set(menu, "flow", flow);
            Set(menu, "root", canvasObject);

            // Left-docked Semi-Transparent Navigation Panel
            var leftVignette = Image("Left Navigation Panel", canvasObject.transform, new Vector2(-480f, 0f), new Vector2(580f, 1080f), GlassPanelColor);
            StretchVerticalLeft(leftVignette.rectTransform, 580f);

            // Title & Subtitle
            Text title = Label("THE LIGHTHOUSE", leftVignette.transform, new Vector2(0f, 380f), new Vector2(500f, 68f), 48, TextAnchor.MiddleLeft);
            title.fontStyle = FontStyle.Bold;
            title.color = TextColor;
            title.gameObject.AddComponent<Shadow>().effectColor = new Color(0f, 0f, 0f, 0.9f);

            Text subtitle = Label("HARROW ROCK", leftVignette.transform, new Vector2(0f, 335f), new Vector2(500f, 34f), 18, TextAnchor.MiddleLeft);
            subtitle.color = AccentAmber;

            Text tagline = Label("Keep the light. Never answer the singing.", leftVignette.transform, new Vector2(0f, 300f), new Vector2(500f, 28f), 15, TextAnchor.MiddleLeft);
            tagline.fontStyle = FontStyle.Italic;
            tagline.color = TextMuted;

            // Main Menu 8-Option Buttons
            float btnY = 220f;
            float btnSpacing = 58f;

            var btnNew = Button("NEW GAME", leftVignette.transform, new Vector2(0f, btnY), menu.StartGame);
            var btnContinue = Button("CONTINUE", leftVignette.transform, new Vector2(0f, btnY - btnSpacing), menu.ContinueGame);
            var btnChapters = Button("CHAPTER SELECT", leftVignette.transform, new Vector2(0f, btnY - btnSpacing * 2), menu.ShowChapterSelect);
            var btnLoad = Button("LOAD GAME", leftVignette.transform, new Vector2(0f, btnY - btnSpacing * 3), menu.ShowLoadGame);
            var btnSettings = Button("SETTINGS", leftVignette.transform, new Vector2(0f, btnY - btnSpacing * 4), menu.ShowSettings);
            var btnExtras = Button("EXTRAS", leftVignette.transform, new Vector2(0f, btnY - btnSpacing * 5), menu.ShowExtras);
            var btnCredits = Button("CREDITS", leftVignette.transform, new Vector2(0f, btnY - btnSpacing * 6), menu.ShowCredits);
            var btnQuit = Button("QUIT", leftVignette.transform, new Vector2(0f, btnY - btnSpacing * 7), menu.Quit);

            Set(menu, "continueButton", btnContinue);

            Label("F1 debug tools • F4 performance overlay", leftVignette.transform, new Vector2(0f, -480f), new Vector2(500f, 24f), 12, TextAnchor.MiddleLeft).color = new Color(TextColor.r, TextColor.g, TextColor.b, 0.45f);

            // 5. Submenus
            // Chapter Select Modal
            var chapterPanel = Image("Chapter Select Panel", canvasObject.transform, Vector2.zero, new Vector2(760f, 620f), SubmenuPanelColor);
            var chapterUI = chapterPanel.gameObject.AddComponent<ChapterSelectUI>();
            Set(chapterUI, "panel", chapterPanel.gameObject);
            Set(chapterUI, "flow", flow);
            Label("CHAPTER SELECT", chapterPanel.transform, new Vector2(0f, 250f), new Vector2(650f, 50f), 28, TextAnchor.MiddleCenter).color = AccentAmber;

            var chapterButtons = new Button[5];
            var chapterStatus = new Text[5];
            for (int i = 0; i < 5; i++)
            {
                var chBtn = Button($"ACT {i + 1}", chapterPanel.transform, new Vector2(0f, 170f - i * 65f), null, new Vector2(620f, 52f));
                chapterButtons[i] = chBtn;
                chapterStatus[i] = chBtn.GetComponentInChildren<Text>();
            }
            var chBack = Button("BACK", chapterPanel.transform, new Vector2(0f, -240f), chapterUI.Close, new Vector2(240f, 46f));
            Set(chapterUI, "backButton", chBack);
            SetArray(chapterUI, "chapterButtons", chapterButtons);
            SetArray(chapterUI, "chapterStatusLabels", chapterStatus);
            chapterPanel.gameObject.SetActive(false);
            Set(menu, "chapterSelect", chapterUI);

            // Save / Load Modal
            var saveLoadPanel = Image("Save Load Panel", canvasObject.transform, Vector2.zero, new Vector2(760f, 580f), SubmenuPanelColor);
            var saveLoadUI = saveLoadPanel.gameObject.AddComponent<SaveLoadUI>();
            Set(saveLoadUI, "panel", saveLoadPanel.gameObject);
            Set(saveLoadUI, "flow", flow);
            var slTitle = Label("LOAD GAME", saveLoadPanel.transform, new Vector2(0f, 230f), new Vector2(650f, 50f), 28, TextAnchor.MiddleCenter);
            slTitle.color = AccentAmber;
            Set(saveLoadUI, "headerTitle", slTitle);

            var slotButtons = new Button[3];
            var slotInfo = new Text[3];
            for (int i = 0; i < 3; i++)
            {
                var sBtn = Button($"Slot {i + 1}", saveLoadPanel.transform, new Vector2(0f, 130f - i * 85f), null, new Vector2(620f, 70f));
                slotButtons[i] = sBtn;
                slotInfo[i] = sBtn.GetComponentInChildren<Text>();
            }
            var slBack = Button("BACK", saveLoadPanel.transform, new Vector2(0f, -220f), saveLoadUI.Close, new Vector2(240f, 46f));
            Set(saveLoadUI, "backButton", slBack);
            SetArray(saveLoadUI, "slotButtons", slotButtons);
            SetArray(saveLoadUI, "slotInfoTexts", slotInfo);
            saveLoadPanel.gameObject.SetActive(false);
            Set(menu, "saveLoad", saveLoadUI);

            // Complete Settings Menu Modal
            var settingsPanel = Image("Settings Panel", canvasObject.transform, Vector2.zero, new Vector2(980f, 740f), SubmenuPanelColor);
            var settingsUI = settingsPanel.gameObject.AddComponent<SettingsMenuUI>();
            Set(settingsUI, "rootPanel", settingsPanel.gameObject);
            Label("SETTINGS", settingsPanel.transform, new Vector2(0f, 320f), new Vector2(850f, 45f), 30, TextAnchor.MiddleCenter).color = AccentAmber;

            // Tab Buttons
            var tabDisplay = Button("Display", settingsPanel.transform, new Vector2(-340f, 260f), null, new Vector2(160f, 42f));
            var tabAudio = Button("Audio", settingsPanel.transform, new Vector2(-170f, 260f), null, new Vector2(160f, 42f));
            var tabGameplay = Button("Gameplay", settingsPanel.transform, new Vector2(0f, 260f), null, new Vector2(160f, 42f));
            var tabAccess = Button("Accessibility", settingsPanel.transform, new Vector2(170f, 260f), null, new Vector2(160f, 42f));
            var tabControls = Button("Controls", settingsPanel.transform, new Vector2(340f, 260f), null, new Vector2(160f, 42f));

            Set(settingsUI, "displayTabButton", tabDisplay);
            Set(settingsUI, "audioTabButton", tabAudio);
            Set(settingsUI, "gameplayTabButton", tabGameplay);
            Set(settingsUI, "accessibilityTabButton", tabAccess);
            Set(settingsUI, "controlsTabButton", tabControls);

            // Tab Panels
            var pDisplay = RectObject("Tab_Display", settingsPanel.transform, new Vector2(0, -10f), new Vector2(880f, 460f));
            var pAudio = RectObject("Tab_Audio", settingsPanel.transform, new Vector2(0, -10f), new Vector2(880f, 460f));
            var pGameplay = RectObject("Tab_Gameplay", settingsPanel.transform, new Vector2(0, -10f), new Vector2(880f, 460f));
            var pAccess = RectObject("Tab_Access", settingsPanel.transform, new Vector2(0, -10f), new Vector2(880f, 460f));
            var pControls = RectObject("Tab_Controls", settingsPanel.transform, new Vector2(0, -10f), new Vector2(880f, 460f));

            Set(settingsUI, "displayTab", pDisplay);
            Set(settingsUI, "audioTab", pAudio);
            Set(settingsUI, "gameplayTab", pGameplay);
            Set(settingsUI, "accessibilityTab", pAccess);
            Set(settingsUI, "controlsTab", pControls);

            // Display Controls
            var sRenderScale = Slider("Render Scale (Mac M3: 100%)", pDisplay.transform, new Vector2(-220f, 150f), null, 0.7f, 1.0f, 1.0f);
            Set(settingsUI, "renderScaleSlider", sRenderScale);
            var sFov = Slider("Field of View (FOV)", pDisplay.transform, new Vector2(-220f, 60f), null, 55f, 90f, 65f);
            Set(settingsUI, "fovSlider", sFov);
            var sBrightness = Slider("Brightness", pDisplay.transform, new Vector2(-220f, -30f), null, 0.5f, 1.5f, 1.0f);
            Set(settingsUI, "brightnessSlider", sBrightness);
            var tVSync = Toggle("Vertical Sync (VSync)", pDisplay.transform, new Vector2(220f, 150f), null, true);
            Set(settingsUI, "vSyncToggle", tVSync);
            var tFullscreen = Toggle("Fullscreen Mode", pDisplay.transform, new Vector2(220f, 70f), null, true);
            Set(settingsUI, "fullscreenToggle", tFullscreen);

            // Audio Controls
            var sMaster = Slider("Master Volume", pAudio.transform, new Vector2(0f, 160f), null, 0f, 1f, 1f);
            Set(settingsUI, "masterVolumeSlider", sMaster);
            var sMusic = Slider("Music Volume", pAudio.transform, new Vector2(0f, 90f), null, 0f, 1f, 0.8f);
            Set(settingsUI, "musicVolumeSlider", sMusic);
            var sAmbience = Slider("Ambience Volume", pAudio.transform, new Vector2(0f, 20f), null, 0f, 1f, 0.9f);
            Set(settingsUI, "ambienceVolumeSlider", sAmbience);
            var sSfx = Slider("SFX Volume", pAudio.transform, new Vector2(0f, -50f), null, 0f, 1f, 1f);
            Set(settingsUI, "sfxVolumeSlider", sSfx);
            var sDialogue = Slider("Dialogue Volume", pAudio.transform, new Vector2(0f, -120f), null, 0f, 1f, 1f);
            Set(settingsUI, "dialogueVolumeSlider", sDialogue);

            // Gameplay Controls
            var sSens = Slider("Look Sensitivity", pGameplay.transform, new Vector2(0f, 150f), null, 0.1f, 3.0f, 1.0f);
            Set(settingsUI, "sensitivitySlider", sSens);
            var tInvertY = Toggle("Invert Y Axis", pGameplay.transform, new Vector2(0f, 60f), null, false);
            Set(settingsUI, "invertYToggle", tInvertY);
            var tBob = Toggle("Camera Bobbing", pGameplay.transform, new Vector2(0f, 0f), null, true);
            Set(settingsUI, "cameraBobToggle", tBob);
            var tMotion = Toggle("Motion Sickness Reduction", pGameplay.transform, new Vector2(0f, -60f), null, false);
            Set(settingsUI, "motionSicknessToggle", tMotion);
            var tObj = Toggle("Objective Navigation Hints", pGameplay.transform, new Vector2(0f, -120f), null, true);
            Set(settingsUI, "objectiveAssistanceToggle", tObj);

            // Accessibility Controls
            var tSub = Toggle("Subtitles Enabled", pAccess.transform, new Vector2(0f, 150f), null, true);
            Set(settingsUI, "subtitlesToggle", tSub);
            var sSubSize = Slider("Subtitle Size", pAccess.transform, new Vector2(0f, 75f), null, 0.8f, 1.5f, 1.0f);
            Set(settingsUI, "subtitleSizeSlider", sSubSize);
            var sSubBg = Slider("Subtitle Background Opacity", pAccess.transform, new Vector2(0f, 0f), null, 0f, 1f, 0.75f);
            Set(settingsUI, "subtitleBgOpacitySlider", sSubBg);
            var tStorm = Toggle("Reduced Storm Flashes", pAccess.transform, new Vector2(0f, -75f), null, true);
            Set(settingsUI, "reducedStormToggle", tStorm);
            var tShake = Toggle("Reduced Camera Shake", pAccess.transform, new Vector2(0f, -135f), null, false);
            Set(settingsUI, "reducedCameraShakeToggle", tShake);

            // Controls Keybindings Text
            var kbText = Label("Keybindings", pControls.transform, Vector2.zero, new Vector2(800f, 380f), 18, TextAnchor.MiddleCenter);
            kbText.color = TextColor;
            Set(settingsUI, "keybindingsText", kbText);

            var sBack = Button("BACK", settingsPanel.transform, new Vector2(-150f, -315f), settingsUI.Close, new Vector2(220f, 46f));
            var sReset = Button("RESET DEFAULTS", settingsPanel.transform, new Vector2(150f, -315f), null, new Vector2(220f, 46f));
            Set(settingsUI, "backButton", sBack);
            Set(settingsUI, "resetDefaultsButton", sReset);

            settingsPanel.gameObject.SetActive(false);
            Set(menu, "settingsMenu", settingsUI);

            // Extras Modal
            var extrasPanel = Image("Extras Panel", canvasObject.transform, Vector2.zero, new Vector2(760f, 540f), SubmenuPanelColor);
            Label("EXTRAS & LORE", extrasPanel.transform, new Vector2(0f, 210f), new Vector2(650f, 50f), 28, TextAnchor.MiddleCenter).color = AccentAmber;
            Label("HARROW ROCK ARCHIVE:\n\n• The 'Marguerite': Sunk deliberate by island wreckers in 1888.\n• The Light: A supernatural boundary leashing the drowned souls.\n• Marta Relay: Recorded VHF loop synthesized by harbor board.\n• The Ledger: Daniel Hale selected weeks before arrival to atone.", extrasPanel.transform, new Vector2(0f, 30f), new Vector2(680f, 260f), 17, TextAnchor.MiddleLeft).color = TextColor;
            Button("BACK", extrasPanel.transform, new Vector2(0f, -210f), menu.CloseExtras, new Vector2(220f, 46f));
            extrasPanel.gameObject.SetActive(false);
            Set(menu, "extrasPanel", extrasPanel.gameObject);

            // Credits Modal
            var creditsPanel = Image("Credits Panel", canvasObject.transform, Vector2.zero, new Vector2(760f, 540f), SubmenuPanelColor);
            Label("CREDITS", creditsPanel.transform, new Vector2(0f, 210f), new Vector2(650f, 50f), 28, TextAnchor.MiddleCenter).color = AccentAmber;
            Label("THE LIGHTHOUSE — HARROW ROCK\n\nDirected & Developed with Google DeepMind Advanced Agentic Coding\nAudio & Visuals: Kenney (CC0 1.0), Maritime Historical Records\nEngine: Unity 6 (Universal Render Pipeline)\nBackend: Java 21, Spring Boot 3.4.5, PostgreSQL", creditsPanel.transform, new Vector2(0f, 30f), new Vector2(680f, 260f), 17, TextAnchor.MiddleCenter).color = TextColor;
            Button("BACK", creditsPanel.transform, new Vector2(0f, -210f), menu.CloseCredits, new Vector2(220f, 46f));
            creditsPanel.gameObject.SetActive(false);
            Set(menu, "creditsPanel", creditsPanel.gameObject);

            // New Game Confirmation Modal
            var confirmModal = Image("New Game Confirm Modal", canvasObject.transform, Vector2.zero, new Vector2(640f, 320f), SubmenuPanelColor);
            Label("START NEW GAME?", confirmModal.transform, new Vector2(0f, 90f), new Vector2(580f, 45f), 24, TextAnchor.MiddleCenter).color = AccentAmber;
            Label("Starting a new game will reset your current checkpoint.\nDo you wish to proceed?", confirmModal.transform, new Vector2(0f, 20f), new Vector2(580f, 60f), 16, TextAnchor.MiddleCenter).color = TextColor;
            Button("CANCEL", confirmModal.transform, new Vector2(-130f, -80f), menu.CancelNewGame, new Vector2(200f, 46f));
            Button("PROCEED", confirmModal.transform, new Vector2(130f, -80f), menu.ConfirmNewGame, new Vector2(200f, 46f));
            confirmModal.gameObject.SetActive(false);
            Set(menu, "confirmNewGameModal", confirmModal.gameObject);

            new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));

            EditorSceneManager.SaveScene(scene, MenuPath);
            AssetDatabase.SaveAssets();
            Debug.Log("Commercial Main Menu rebuilt with real-time 3D environment, rotating beacon, and full submenu suite.");
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
