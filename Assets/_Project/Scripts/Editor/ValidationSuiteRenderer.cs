using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace TheLightHouse.Editor
{
    public static class ValidationSuiteRenderer
    {
        private const string RepoDir = "docs/screenshots";
        private const string ArtifactDir = "/Users/rafiurrahman/.gemini/antigravity/brain/58bc2ebc-cf93-4bf5-9fdf-0c1664283953/validation_shots";

        private struct ShotDef
        {
            public string Filename;
            public Vector3 Pos;
            public Vector3 Target;
            public float Fov;

            public ShotDef(string filename, Vector3 pos, Vector3 target, float fov)
            {
                Filename = filename;
                Pos = pos;
                Target = target;
                Fov = fov;
            }
        }

        public static void CaptureAll16()
        {
            Directory.CreateDirectory(RepoDir);
            Directory.CreateDirectory(ArtifactDir);

            // Ensure current scene is Act1
            if (EditorSceneManager.GetActiveScene().name != "Act1")
            {
                EditorSceneManager.OpenScene("Assets/_Project/Scenes/Act1.unity");
            }

            // Ensure post-processing profiles are assigned and active
            CinematicVolumeSetup.AssignProfilesToOpenScene();

            // Simulate rain particle system if present
            foreach (var ps in Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None))
            {
                if (ps.name == "Coastal Rain" || ps.name.Contains("Rain"))
                {
                    ps.Simulate(3.5f, true, true);
                }
            }

            var shotDefs = new[]
            {
                new ShotDef("01_full_lighthouse_exterior", new Vector3(-18f, 3.5f, -38f), new Vector3(0f, 14f, 0f), 52f),
                new ShotDef("02_storm_exterior_atmosphere", new Vector3(-14f, 2.2f, -28f), new Vector3(-3f, 8f, -6f), 60f),
                new ShotDef("03_volumetric_beam_fog", new Vector3(-12f, 18f, -22f), new Vector3(0f, 26f, 0f), 55f),
                new ShotDef("04_grand_entrance_portal", new Vector3(0f, 1.6f, -11.5f), new Vector3(0f, 1.8f, -6.5f), 55f),
                new ShotDef("05_dock_and_boatman", new Vector3(-3.0f, 1.2f, -31.5f), new Vector3(-5.8f, 0.35f, -29.8f), 62f),
                new ShotDef("06_lantern_gallery_underside", new Vector3(0f, 17.5f, -8.5f), new Vector3(0f, 26.0f, -4.8f), 65f),
                new ShotDef("07_spiral_staircase_spacious", new Vector3(-2.2f, 1.8f, -2.0f), new Vector3(4.2f, 2.2f, 0.8f), 70f),
                new ShotDef("08_ground_floor_vestibule", new Vector3(0f, 2.2f, -3.8f), new Vector3(0f, 0.2f, 0f), 72f),
                new ShotDef("09_first_floor_archive_desk", new Vector3(0f, 4.8f, -0.8f), new Vector3(0f, 4.1f, 2.5f), 72f),
                new ShotDef("10_second_floor_quarters", new Vector3(-1.8f, 7.8f, -1.8f), new Vector3(1.8f, 7.2f, 1.5f), 72f),
                new ShotDef("11_lantern_room_fresnel", new Vector3(0f, 27.0f, -3.2f), new Vector3(0f, 27.5f, 0f), 72f),
                new ShotDef("12_cottage_kitchen_stove", new Vector3(-8.0f, 1.8f, -5.8f), new Vector3(-12.0f, 1.2f, -2.5f), 70f),
                new ShotDef("13_cottage_dining_meal", new Vector3(-7.2f, 1.8f, -7.8f), new Vector3(-9.5f, 1.0f, -7.0f), 68f),
                new ShotDef("14_lantern_exterior_walkway", new Vector3(4.8f, 26.8f, 0f), new Vector3(3.2f, 26.8f, 3.5f), 70f),
                new ShotDef("15_crank_station_mechanism", new Vector3(-0.6f, 26.8f, 1.2f), new Vector3(-1.8f, 26.6f, 1.2f), 65f),
                new ShotDef("16_before_after_firstperson", new Vector3(-1.1f, 1.8f, -33.5f), new Vector3(-1.1f, 1.8f, -20.0f), 68f),
                new ShotDef("17_terrain_closeup", new Vector3(-1.0f, 1.2f, -16.0f), new Vector3(-1.0f, 0.0f, -14.0f), 50f),
                new ShotDef("18_wet_mud_sand_transition", new Vector3(-2.0f, 1.0f, -28.0f), new Vector3(-3.0f, 0.0f, -26.0f), 55f),
                new ShotDef("19_coastal_rocks_closeup", new Vector3(-3.5f, 0.8f, -27.0f), new Vector3(-5.5f, 0.0f, -25.5f), 55f),
                new ShotDef("20_ocean_shoreline_water", new Vector3(-5.0f, 1.2f, -32.0f), new Vector3(-12.0f, -0.4f, -34.0f), 60f)
            };

            var camGo = new GameObject("Validation Capture Camera", typeof(Camera));
            var cam = camGo.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.Skybox;
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 350f;

            var camData = cam.GetUniversalAdditionalCameraData();
            camData.renderPostProcessing = true;
            camData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            camData.antialiasingQuality = AntialiasingQuality.High;
            camData.volumeLayerMask = ~0;
            camData.volumeTrigger = cam.transform;

            const int width = 1920;
            const int height = 1080;
            var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32)
            {
                antiAliasing = 4,
                useMipMap = false
            };
            var tex = new Texture2D(width, height, TextureFormat.RGB24, false);

            var prevActive = RenderTexture.active;
            cam.targetTexture = rt;

            try
            {
                foreach (var def in shotDefs)
                {
                    cam.transform.position = def.Pos;
                    cam.transform.LookAt(def.Target);
                    cam.fieldOfView = def.Fov;

                    GameObject frontDoorObj = null;
                    Quaternion prevDoorRot = Quaternion.identity;
                    if (def.Filename == "43_cottage_widened_front_door")
                    {
                        frontDoorObj = GameObject.Find("Keeper House Front Door");
                        if (frontDoorObj)
                        {
                            prevDoorRot = frontDoorObj.transform.rotation;
                            frontDoorObj.transform.rotation = Quaternion.Euler(0, -95f, 0);
                        }
                    }

                    GameObject flashlightModel = null;
                    Light tacticalBeam = null;
                    if (def.Filename == "35_flooded_courtyard_boatshed_path" || def.Filename == "37_drowned_in_flooded_courtyard")
                    {
                        flashlightModel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        flashlightModel.name = "FirstPersonFlashlight";
                        flashlightModel.transform.SetParent(cam.transform, false);
                        flashlightModel.transform.localPosition = new Vector3(0.28f, -0.22f, 0.48f);
                        flashlightModel.transform.localRotation = Quaternion.Euler(75f, -15f, 0f);
                        flashlightModel.transform.localScale = new Vector3(0.046f, 0.16f, 0.046f);
                        var rend = flashlightModel.GetComponent<Renderer>();
                        if (rend)
                        {
                            var ironMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/PBR/PBR_RustedIron.mat");
                            if (ironMat) rend.sharedMaterial = ironMat;
                        }
                        var col = flashlightModel.GetComponent<Collider>();
                        if (col) Object.DestroyImmediate(col);

                        var beamGo = new GameObject("FlashlightTacticalBeam");
                        beamGo.transform.SetParent(flashlightModel.transform, false);
                        beamGo.transform.localPosition = new Vector3(0, 0.15f, 0);
                        tacticalBeam = beamGo.AddComponent<Light>();
                        tacticalBeam.type = LightType.Spot;
                        tacticalBeam.spotAngle = def.Filename == "37_drowned_in_flooded_courtyard" ? 42f : 54f;
                        tacticalBeam.innerSpotAngle = 24f;
                        tacticalBeam.range = 38f;
                        tacticalBeam.intensity = 26.0f;
                        tacticalBeam.color = new Color(0.92f, 0.96f, 1.0f);
                        tacticalBeam.shadows = LightShadows.Soft;
                        beamGo.transform.rotation = Quaternion.LookRotation(def.Target - beamGo.transform.position);
                    }

                    cam.Render();

                    if (flashlightModel) Object.DestroyImmediate(flashlightModel);

                    RenderTexture.active = rt;
                    tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                    tex.Apply();

                    byte[] pngBytes = tex.EncodeToPNG();

                    string repoPath = Path.Combine(RepoDir, $"{def.Filename}.png");
                    string artifactPath = Path.Combine(ArtifactDir, $"{def.Filename}.png");

                    File.WriteAllBytes(repoPath, pngBytes);
                    File.WriteAllBytes(artifactPath, pngBytes);

                    Debug.Log($"[ValidationSuiteRenderer] Rendered {def.Filename} to repo and artifact dirs.");
                }
            }
            finally
            {
                cam.targetTexture = null;
                RenderTexture.active = prevActive;
                Object.DestroyImmediate(tex);
                Object.DestroyImmediate(rt);
                Object.DestroyImmediate(camGo);
            }

            AssetDatabase.Refresh();
            Debug.Log("[ValidationSuiteRenderer] All 16 validation screenshots successfully generated.");
        }

        public static void CaptureTowerStudyValidationShots()
        {
            string repoDir = "docs/screenshots";
            string artifactDir = "/Users/rafiurrahman/.gemini/antigravity/brain/58bc2ebc-cf93-4bf5-9fdf-0c1664283953/screenshots";
            Directory.CreateDirectory(repoDir);
            Directory.CreateDirectory(artifactDir);

            if (EditorSceneManager.GetActiveScene().name != "Act1")
            {
                EditorSceneManager.OpenScene("Assets/_Project/Scenes/Act1.unity");
            }

            HarrowRockEnvironmentMasterPass.ApplyToCurrentScene();
            CinematicVolumeSetup.AssignProfilesToOpenScene();

            var shotDefs = new[]
            {
                new ShotDef("31_vertical_tower_spiral_stairs", new Vector3(-1.6f, 1.2f, -1.6f), new Vector3(1.2f, 5.5f, 1.8f), 75f),
                new ShotDef("32_midlevel_study_desk_overview", new Vector3(-1.15f, 4.30f, 0.35f), new Vector3(0.12f, 4.05f, 1.95f), 68f),
                new ShotDef("33_study_calipers_and_charts_closeup", new Vector3(0.0f, 4.60f, 1.05f), new Vector3(0.08f, 4.01f, 1.55f), 45f),
                new ShotDef("34_tower_study_stairs_framing", new Vector3(2.5f, 3.85f, -0.2f), new Vector3(-0.2f, 4.02f, 1.7f), 65f)
            };

            var camGo = new GameObject("Validation Capture Camera", typeof(Camera));
            var cam = camGo.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.Skybox;
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 350f;

            var camData = cam.GetUniversalAdditionalCameraData();
            camData.renderPostProcessing = true;
            camData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            camData.antialiasingQuality = AntialiasingQuality.High;
            camData.volumeLayerMask = ~0;
            camData.volumeTrigger = cam.transform;

            const int width = 1920;
            const int height = 1080;
            var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32)
            {
                antiAliasing = 4,
                useMipMap = false
            };
            var tex = new Texture2D(width, height, TextureFormat.RGB24, false);

            var prevActive = RenderTexture.active;
            cam.targetTexture = rt;

            try
            {
                foreach (var def in shotDefs)
                {
                    cam.transform.position = def.Pos;
                    cam.transform.LookAt(def.Target);
                    cam.fieldOfView = def.Fov;

                    GameObject flashlightModel = null;
                    Light tacticalBeam = null;
                    if (def.Filename == "35_flooded_courtyard_boatshed_path" || def.Filename == "37_drowned_in_flooded_courtyard")
                    {
                        flashlightModel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        flashlightModel.name = "FirstPersonFlashlight";
                        flashlightModel.transform.SetParent(cam.transform, false);
                        flashlightModel.transform.localPosition = new Vector3(0.28f, -0.22f, 0.48f);
                        flashlightModel.transform.localRotation = Quaternion.Euler(75f, -15f, 0f);
                        flashlightModel.transform.localScale = new Vector3(0.046f, 0.16f, 0.046f);
                        var rend = flashlightModel.GetComponent<Renderer>();
                        if (rend)
                        {
                            var ironMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/PBR/PBR_RustedIron.mat");
                            if (ironMat) rend.sharedMaterial = ironMat;
                        }
                        var col = flashlightModel.GetComponent<Collider>();
                        if (col) Object.DestroyImmediate(col);

                        var beamGo = new GameObject("FlashlightTacticalBeam");
                        beamGo.transform.SetParent(flashlightModel.transform, false);
                        beamGo.transform.localPosition = new Vector3(0, 0.15f, 0);
                        tacticalBeam = beamGo.AddComponent<Light>();
                        tacticalBeam.type = LightType.Spot;
                        tacticalBeam.spotAngle = def.Filename == "37_drowned_in_flooded_courtyard" ? 42f : 54f;
                        tacticalBeam.innerSpotAngle = 22f;
                        tacticalBeam.range = 38f;
                        tacticalBeam.intensity = 26.0f;
                        tacticalBeam.color = new Color(0.92f, 0.96f, 1.0f);
                        tacticalBeam.shadows = LightShadows.Soft;
                        beamGo.transform.rotation = Quaternion.LookRotation(def.Target - beamGo.transform.position);
                    }

                    cam.Render();

                    if (flashlightModel) Object.DestroyImmediate(flashlightModel);

                    RenderTexture.active = rt;
                    tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                    tex.Apply();

                    byte[] pngBytes = tex.EncodeToPNG();

                    string repoPath = Path.Combine(repoDir, $"{def.Filename}.png");
                    string artifactPath = Path.Combine(artifactDir, $"{def.Filename}.png");

                    File.WriteAllBytes(repoPath, pngBytes);
                    File.WriteAllBytes(artifactPath, pngBytes);

                    Debug.Log($"[ValidationSuiteRenderer] Rendered {def.Filename} to repo and artifact dirs.");
                }
            }
            finally
            {
                cam.targetTexture = null;
                RenderTexture.active = prevActive;
                Object.DestroyImmediate(tex);
                Object.DestroyImmediate(rt);
                Object.DestroyImmediate(camGo);
            }

            AssetDatabase.Refresh();
            Debug.Log("[ValidationSuiteRenderer] Tower & Study validation screenshots successfully generated.");
        }

        public static void CaptureFloodedCourtyardValidationShots()
        {
            string repoDir = "docs/screenshots";
            string artifactDir = "/Users/rafiurrahman/.gemini/antigravity/brain/58bc2ebc-cf93-4bf5-9fdf-0c1664283953/screenshots";
            Directory.CreateDirectory(repoDir);
            Directory.CreateDirectory(artifactDir);

            if (EditorSceneManager.GetActiveScene().name != "Act1")
            {
                EditorSceneManager.OpenScene("Assets/_Project/Scenes/Act1.unity");
            }

            HarrowRockEnvironmentMasterPass.ApplyToCurrentScene();
            CinematicVolumeSetup.AssignProfilesToOpenScene();

            var shotDefs = new[]
            {
                new ShotDef("35_flooded_courtyard_boatshed_path", new Vector3(0.6f, 1.75f, -11.2f), new Vector3(8.5f, 1.4f, -14.0f), 62f),
                new ShotDef("36_boatshed_exterior_closeup", new Vector3(4.5f, 1.75f, -13.8f), new Vector3(9.8f, 2.2f, -14.0f), 58f),
                new ShotDef("37_drowned_in_flooded_courtyard", new Vector3(1.8f, 1.55f, -12.5f), new Vector3(3.2f, 0.70f, -13.8f), 58f),
                new ShotDef("38_courtyard_boardwalk_overview", new Vector3(-2.5f, 5.2f, -8.5f), new Vector3(6.5f, 0.5f, -14.0f), 62f)
            };

            var camGo = new GameObject("Validation Capture Camera", typeof(Camera));
            var cam = camGo.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.Skybox;
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 350f;

            var camData = cam.GetUniversalAdditionalCameraData();
            camData.renderPostProcessing = true;
            camData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            camData.antialiasingQuality = AntialiasingQuality.High;
            camData.volumeLayerMask = ~0;
            camData.volumeTrigger = cam.transform;

            const int width = 1920;
            const int height = 1080;
            var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32)
            {
                antiAliasing = 4,
                useMipMap = false
            };
            var tex = new Texture2D(width, height, TextureFormat.RGB24, false);

            var prevActive = RenderTexture.active;
            cam.targetTexture = rt;

            try
            {
                foreach (var def in shotDefs)
                {
                    cam.transform.position = def.Pos;
                    cam.transform.LookAt(def.Target);
                    cam.fieldOfView = def.Fov;

                    GameObject flashlightModel = null;
                    Light tacticalBeam = null;
                    if (def.Filename == "35_flooded_courtyard_boatshed_path" || def.Filename == "37_drowned_in_flooded_courtyard")
                    {
                        flashlightModel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        flashlightModel.name = "FirstPersonFlashlight";
                        flashlightModel.transform.SetParent(cam.transform, false);
                        flashlightModel.transform.localPosition = new Vector3(0.28f, -0.22f, 0.48f);
                        flashlightModel.transform.localRotation = Quaternion.Euler(75f, -15f, 0f);
                        flashlightModel.transform.localScale = new Vector3(0.046f, 0.16f, 0.046f);
                        var rend = flashlightModel.GetComponent<Renderer>();
                        if (rend)
                        {
                            var ironMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/PBR/PBR_RustedIron.mat");
                            if (ironMat) rend.sharedMaterial = ironMat;
                        }
                        var col = flashlightModel.GetComponent<Collider>();
                        if (col) Object.DestroyImmediate(col);

                        var beamGo = new GameObject("FlashlightTacticalBeam");
                        beamGo.transform.SetParent(flashlightModel.transform, false);
                        beamGo.transform.localPosition = new Vector3(0, 0.15f, 0);
                        tacticalBeam = beamGo.AddComponent<Light>();
                        tacticalBeam.type = LightType.Spot;
                        tacticalBeam.spotAngle = def.Filename == "37_drowned_in_flooded_courtyard" ? 42f : 54f;
                        tacticalBeam.innerSpotAngle = 22f;
                        tacticalBeam.range = 38f;
                        tacticalBeam.intensity = 26.0f;
                        tacticalBeam.color = new Color(0.92f, 0.96f, 1.0f);
                        tacticalBeam.shadows = LightShadows.Soft;
                        beamGo.transform.rotation = Quaternion.LookRotation(def.Target - beamGo.transform.position);
                    }

                    cam.Render();

                    if (flashlightModel) Object.DestroyImmediate(flashlightModel);

                    RenderTexture.active = rt;
                    tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                    tex.Apply();

                    byte[] pngBytes = tex.EncodeToPNG();

                    string repoPath = Path.Combine(repoDir, $"{def.Filename}.png");
                    string artifactPath = Path.Combine(artifactDir, $"{def.Filename}.png");

                    File.WriteAllBytes(repoPath, pngBytes);
                    File.WriteAllBytes(artifactPath, pngBytes);

                    Debug.Log($"[ValidationSuiteRenderer] Rendered {def.Filename} to repo and artifact dirs.");
                }
            }
            finally
            {
                cam.targetTexture = null;
                RenderTexture.active = prevActive;
                Object.DestroyImmediate(tex);
                Object.DestroyImmediate(rt);
                Object.DestroyImmediate(camGo);
            }

            AssetDatabase.Refresh();
            Debug.Log("[ValidationSuiteRenderer] Flooded Courtyard & Boat Shed validation screenshots successfully generated.");
        }

        public static void CaptureSecretRadioRoomAndLedgerValidationShots()
        {
            string repoDir = "docs/screenshots";
            string artifactDir = "/Users/rafiurrahman/.gemini/antigravity/brain/58bc2ebc-cf93-4bf5-9fdf-0c1664283953/screenshots";
            Directory.CreateDirectory(repoDir);
            Directory.CreateDirectory(artifactDir);

            if (EditorSceneManager.GetActiveScene().name != "Act1")
            {
                EditorSceneManager.OpenScene("Assets/_Project/Scenes/Act1.unity");
            }

            HarrowRockEnvironmentMasterPass.ApplyToCurrentScene();
            CinematicVolumeSetup.AssignProfilesToOpenScene();

            Vector3 deskPos = new Vector3(-12.0f, -3.2f, -4.05f);

            var shotDefs = new[]
            {
                new ShotDef("39_secret_radio_room_overview", new Vector3(-12.0f, -1.65f, -6.60f), new Vector3(-12.0f, -2.15f, -4.05f), 68f),
                new ShotDef("40_finding_the_ledger_closeup", new Vector3(-11.95f, -1.80f, -4.60f), new Vector3(-11.92f, -2.31f, -4.08f), 44f),
                new ShotDef("41_marguerite_wall_chart_closeup", new Vector3(-12.0f, -1.60f, -4.50f), new Vector3(-12.0f, -1.60f, -3.33f), 48f),
                new ShotDef("42_radio_gear_and_vacuum_tubes", new Vector3(-11.30f, -1.82f, -4.55f), new Vector3(-11.28f, -2.20f, -3.89f), 44f),
                new ShotDef("43_cottage_widened_front_door", new Vector3(-8.5f, 1.45f, -11.5f), new Vector3(-8.5f, 1.35f, -6.5f), 65f)
            };

            var camGo = new GameObject("Validation Capture Camera", typeof(Camera));
            var cam = camGo.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.Skybox;
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 150f;

            var camData = cam.GetUniversalAdditionalCameraData();
            camData.renderPostProcessing = true;
            camData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            camData.antialiasingQuality = AntialiasingQuality.High;
            camData.volumeLayerMask = ~0;
            camData.volumeTrigger = cam.transform;

            const int width = 1920;
            const int height = 1080;
            var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32)
            {
                antiAliasing = 4,
                useMipMap = false
            };
            var tex = new Texture2D(width, height, TextureFormat.RGB24, false);

            var prevActive = RenderTexture.active;
            cam.targetTexture = rt;

            try
            {
                foreach (var def in shotDefs)
                {
                    cam.transform.position = def.Pos;
                    cam.transform.LookAt(def.Target);
                    cam.fieldOfView = def.Fov;

                    GameObject frontDoorObj = null;
                    Quaternion prevDoorRot = Quaternion.identity;
                    if (def.Filename == "43_cottage_widened_front_door")
                    {
                        frontDoorObj = GameObject.Find("Keeper House Front Door");
                        if (frontDoorObj)
                        {
                            prevDoorRot = frontDoorObj.transform.rotation;
                            frontDoorObj.transform.rotation = Quaternion.Euler(0, -95f, 0);
                        }
                    }

                    GameObject fpFlashlight = null;
                    if (def.Filename == "39_secret_radio_room_overview" || def.Filename == "40_finding_the_ledger_closeup")
                    {
                        fpFlashlight = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        fpFlashlight.name = "FirstPersonFlashlight";
                        fpFlashlight.transform.SetParent(cam.transform, false);
                        fpFlashlight.transform.localPosition = new Vector3(-0.22f, -0.18f, 0.45f);
                        fpFlashlight.transform.localRotation = Quaternion.Euler(75f, 15f, 0);
                        fpFlashlight.transform.localScale = new Vector3(0.04f, 0.14f, 0.04f);
                        var rend = fpFlashlight.GetComponent<Renderer>();
                        if (rend)
                        {
                            var ironMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/PBR/PBR_RustedIron.mat");
                            if (ironMat) rend.sharedMaterial = ironMat;
                        }
                        var col = fpFlashlight.GetComponent<Collider>();
                        if (col) Object.DestroyImmediate(col);

                        var beamGo = new GameObject("FlashlightBeam");
                        beamGo.transform.SetParent(fpFlashlight.transform, false);
                        beamGo.transform.localPosition = new Vector3(0, 0.12f, 0);
                        var beam = beamGo.AddComponent<Light>();
                        beam.type = LightType.Spot;
                        beam.spotAngle = def.Filename == "40_finding_the_ledger_closeup" ? 38f : 48f;
                        beam.innerSpotAngle = 20f;
                        beam.range = 25f;
                        beam.intensity = def.Filename == "40_finding_the_ledger_closeup" ? 1.8f : 12f;
                        beam.color = new Color(0.92f, 0.96f, 1.0f);
                        beam.shadows = LightShadows.Soft;
                        beamGo.transform.rotation = Quaternion.LookRotation(def.Target - beamGo.transform.position);
                    }

                    cam.Render();

                    if (frontDoorObj) frontDoorObj.transform.rotation = prevDoorRot;
                    if (fpFlashlight) Object.DestroyImmediate(fpFlashlight);

                    RenderTexture.active = rt;
                    tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                    tex.Apply();

                    byte[] pngBytes = tex.EncodeToPNG();

                    string repoPath = Path.Combine(repoDir, $"{def.Filename}.png");
                    string artifactPath = Path.Combine(artifactDir, $"{def.Filename}.png");

                    File.WriteAllBytes(repoPath, pngBytes);
                    File.WriteAllBytes(artifactPath, pngBytes);

                    Debug.Log($"[ValidationSuiteRenderer] Rendered {def.Filename} to repo and artifact dirs.");
                }
            }
            finally
            {
                cam.targetTexture = null;
                RenderTexture.active = prevActive;
                Object.DestroyImmediate(tex);
                Object.DestroyImmediate(rt);
                Object.DestroyImmediate(camGo);
            }

            AssetDatabase.Refresh();
            Debug.Log("[ValidationSuiteRenderer] Secret Radio Room & Ledger validation screenshots successfully generated.");
        }
    }
}
