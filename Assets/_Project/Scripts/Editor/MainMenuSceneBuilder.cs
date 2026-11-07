using System;
using System.Collections.Generic;
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
    public static class MainMenuSceneBuilder
    {
        private const string MenuPath = "Assets/_Project/Scenes/MainMenu.unity";
        private static readonly Vector2 ReferenceResolution = new(1920f, 1080f);

        // UI Color Palette matching specification
        private static readonly Color ColorTextNormal = new(0.545f, 0.580f, 0.620f, 1f);      // #8B949E Muted Gray
        private static readonly Color ColorTextHover = new(0.945f, 0.769f, 0.059f, 1f);       // #F1C40F Warm Lantern Gold
        private static readonly Color ColorTextPressed = new(1.0f, 0.96f, 0.88f, 1f);         // Warm White
        private static readonly Color ColorTextDisabled = new(0.35f, 0.38f, 0.42f, 0.45f);
        private static readonly Color ColorTitle = new(0.92f, 0.94f, 0.96f, 1f);             // Sharp Off-White
        private static readonly Color ColorSubtitle = new(0.831f, 0.639f, 0.349f, 1f);        // #D4A359 Muted Amber
        private static readonly Color ColorTagline = new(0.640f, 0.660f, 0.680f, 1f);         // #A0A5AA Off-White
        private static readonly Color SubmenuPanelColor = new(0.010f, 0.015f, 0.022f, 0.96f);
        private static readonly Color ControlColor = new(0.07f, 0.10f, 0.13f, 1f);
        private static readonly Color HoverColor = new(0.14f, 0.19f, 0.24f, 1f);

        // Loaded Assets
        private static Material matSky;
        private static Material matSea;
        private static Material matStone;
        private static Material matRock;
        private static Material matSlate;
        private static Material matWood;
        private static Material matIron;
        private static Material matCopper;
        private static Material matWindowGlass;
        private static Material matLanternGlass;
        private static Material matBeam;
        private static Material matRain;
        private static Sprite spriteGradient;
        private static Font fontCinzel;
        private static Font fontCormorant;
        private static AudioClip audioCello;
        private static AudioClip audioClick;
        private static AudioClip audioWaves;
        private static AudioClip audioRain;
        private static Mesh rockMesh;

        public static void BuildMenuScene()
        {
            LoadAssets();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Atmosphere & Lighting
            SetupAtmosphereAndLighting();

            // 2. 3D Cinematic Environment
            var envRoot = new GameObject("Environment 3D");

            // Dynamic Subdivided Ocean
            BuildDynamicOcean(envRoot.transform);

            // Jagged Coastal Rock Cliffs
            BuildJaggedCoastalCliffs(envRoot.transform);

            // Detailed Stone Keeper's Lodge
            BuildStoneKeepersLodge(envRoot.transform);

            // Upgraded Tapered Lighthouse Tower with Sweeping Volumetric Beam
            var beaconPivot = BuildLighthouseTower(envRoot.transform);

            // 3. Camera Setup & Viewport Atmospheric Particles
            var cameraObj = BuildMenuCamera(beaconPivot);

            // 4. Act Flow & Save System
            var flowObject = new GameObject("Act Flow");
            var flow = flowObject.AddComponent<ActFlowController>();
            Set(flow, "enableSceneChaining", true);
            flowObject.AddComponent<SaveSystem>();
            var debug = flowObject.AddComponent<GameDebugMenu>();
            Set(debug, "flow", flow);
            Set(debug, "inputActions", AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions"));

            // 5. Polished UI Canvas
            BuildMenuCanvas(flow, cameraObj.GetComponent<MenuCameraController>());

            // Event System
            new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));

            EditorSceneManager.SaveScene(scene, MenuPath);
            AssetDatabase.SaveAssets();
            Debug.Log("<color=#44FF88>[TheLightHouse]</color> Main Menu scene rebuilt with complete cinematic psychological horror polish!");
        }

        private static void LoadAssets()
        {
            matSky = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Reference/Harrow Sky.mat");
            matSea = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Reference/Harrow Sea.mat");
            matStone = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/PBR/PBR_LighthouseStone.mat")
                ?? AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Level/CoastalIsland.mat");
            matRock = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Models/CoastalRock_01/M_CoastalRock_01.mat")
                ?? AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/PBR/PBR_CoastalRock.mat");
            matSlate = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Reference/Slate.mat");
            matWood = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/PBR/PBR_WeatheredPlanks.mat")
                ?? AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Reference/Old Timber.mat");
            matIron = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/PBR/PBR_RustedIron.mat")
                ?? AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Reference/Oxidised Iron.mat");
            matCopper = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Reference/TarnishedCopperKettle.mat")
                ?? AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Reference/PolishedMaritimeBrass.mat")
                ?? matIron;
            matWindowGlass = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Reference/Warm Cottage Glass.mat");
            matLanternGlass = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Reference/VacuumTubeGlass.mat")
                ?? matWindowGlass;
            matBeam = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Graphics1/LampBeam.mat");
            matRain = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Reference/Rain.mat");

            spriteGradient = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Textures/UI/Menu_LeftGradientFade.png");
            fontCinzel = AssetDatabase.LoadAssetAtPath<Font>("Assets/_Project/Fonts/Cinzel.ttf");
            fontCormorant = fontCinzel; // Cinzel supports Italic, Bold and Normal with flawless metrics

            audioCello = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/Menu_Hover_CelloHum.wav");
            audioClick = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/Menu_Select_Click.wav");
            audioWaves = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/Ambience_Ocean_Waves.ogg");
            audioRain = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/Ambience_Rain_Storm.ogg");

            var allRockAssets = AssetDatabase.LoadAllAssetsAtPath("Assets/_Project/Art/Models/CoastalRock_01/CoastalRock_01.fbx");
            foreach (var a in allRockAssets)
            {
                if (a is Mesh m && (a.name == "coast_rocks_01_LOD0" || a.name == "coast_rocks_01_LOD1" || a.name == "coast_rocks_01_LOD2"))
                {
                    rockMesh = m;
                    if (a.name == "coast_rocks_01_LOD0") break;
                }
            }
        }

        private static void SetupAtmosphereAndLighting()
        {
            // Active Volumetric Night Storm Skybox
            if (matSky)
            {
                RenderSettings.skybox = matSky;
            }

            // Directional / Height Fog hugging the ocean surface
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.045f, 0.068f, 0.092f, 1f);
            RenderSettings.fogDensity = 0.009f;

            // Ambient Dusk Lighting
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.040f, 0.058f, 0.080f, 1f);

            // Cold Directional Dusk Moon Light
            var moonObj = new GameObject("Dusk Moonlight");
            var moon = moonObj.AddComponent<Light>();
            moon.type = LightType.Directional;
            moon.transform.rotation = Quaternion.Euler(32f, -55f, 0f);
            moon.color = new Color(0.24f, 0.35f, 0.48f);
            moon.intensity = 0.85f;
            moon.shadows = LightShadows.Soft;

            // Distant Lightning Light
            var lightningObj = new GameObject("Distant Lightning");
            var lightning = lightningObj.AddComponent<Light>();
            lightning.type = LightType.Directional;
            lightning.transform.rotation = Quaternion.Euler(22f, 125f, 0f);
            lightning.color = new Color(0.70f, 0.84f, 1.0f);
            lightning.intensity = 0f;

            // Global Post-Processing Volume
            var volProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>("Assets/_Project/Settings/Volume_Storm.asset")
                ?? AssetDatabase.LoadAssetAtPath<VolumeProfile>("Assets/_Project/Settings/Volume_Night.asset");
            if (volProfile)
            {
                var volGo = new GameObject("Global PostProcess Volume").AddComponent<Volume>();
                volGo.isGlobal = true;
                volGo.profile = volProfile;
            }
        }

        private static void BuildDynamicOcean(Transform parent)
        {
            var oceanObj = new GameObject("Dynamic Ocean Surface");
            oceanObj.transform.SetParent(parent, false);
            oceanObj.transform.position = new Vector3(0f, -0.55f, 15f);

            // Subdivided ocean grid (80x80 quads = 6,561 vertices)
            const int resolution = 80;
            const float size = 260f;
            var vertices = new Vector3[(resolution + 1) * (resolution + 1)];
            var uvs = new Vector2[vertices.Length];
            var triangles = new int[resolution * resolution * 6];

            float step = size / resolution;
            float halfSize = size * 0.5f;

            int vertIndex = 0;
            for (int z = 0; z <= resolution; z++)
            {
                float posZ = -halfSize + z * step;
                for (int x = 0; x <= resolution; x++)
                {
                    float posX = -halfSize + x * step;
                    vertices[vertIndex] = new Vector3(posX, 0f, posZ);
                    uvs[vertIndex] = new Vector2(posX * 0.08f, posZ * 0.08f);
                    vertIndex++;
                }
            }

            int triIndex = 0;
            for (int z = 0; z < resolution; z++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    int row1 = z * (resolution + 1);
                    int row2 = (z + 1) * (resolution + 1);

                    triangles[triIndex++] = row1 + x;
                    triangles[triIndex++] = row2 + x;
                    triangles[triIndex++] = row1 + x + 1;

                    triangles[triIndex++] = row1 + x + 1;
                    triangles[triIndex++] = row2 + x;
                    triangles[triIndex++] = row2 + x + 1;
                }
            }

            var mesh = new Mesh
            {
                name = "MenuDynamicOceanMesh",
                vertices = vertices,
                uv = uvs,
                triangles = triangles
            };
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();

            var mf = oceanObj.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;
            var mr = oceanObj.AddComponent<MeshRenderer>();
            if (matSea) mr.sharedMaterial = matSea;
        }

        private static void BuildJaggedCoastalCliffs(Transform parent)
        {
            var cliffRoot = new GameObject("Jagged Coastal Rock Cliffs");
            cliffRoot.transform.SetParent(parent, false);

            // Layered angular promontory foundation
            var foundationMesh = CreatePromontoryBaseMesh();
            var foundationObj = new GameObject("Cliff Promontory Bedrock");
            foundationObj.transform.SetParent(cliffRoot.transform, false);
            foundationObj.AddComponent<MeshFilter>().sharedMesh = foundationMesh;
            foundationObj.AddComponent<MeshRenderer>().sharedMaterial = matRock ?? matStone;

            // Clustered Photogrammetry Rocks along cliff faces, ledges, and surf
            if (rockMesh)
            {
                var random = new System.Random(1337);

                // Sea-level crashing surf rocks
                for (int i = 0; i < 24; i++)
                {
                    float angle = (i / 24f) * Mathf.PI * 2f;
                    float radius = 17f + (float)random.NextDouble() * 7.5f;
                    float rx = 6f + Mathf.Sin(angle) * (radius * 1.15f);
                    float rz = 18f + Mathf.Cos(angle) * radius;
                    float ry = -0.55f + (float)random.NextDouble() * 0.45f;

                    var rock = new GameObject($"Surf Coastal Rock {i}");
                    rock.transform.SetParent(cliffRoot.transform, false);
                    rock.transform.position = new Vector3(rx, ry, rz);
                    rock.transform.rotation = Quaternion.Euler((float)random.NextDouble() * 25f, (float)random.NextDouble() * 360f, (float)random.NextDouble() * 25f);
                    float scale = 4.5f + (float)random.NextDouble() * 4.0f;
                    rock.transform.localScale = new Vector3(scale * 1.3f, scale * 1.0f, scale * 1.3f);
                    rock.AddComponent<MeshFilter>().sharedMesh = rockMesh;
                    rock.AddComponent<MeshRenderer>().sharedMaterial = matRock;
                }

                // Mid-cliff jagged rock shelves
                for (int i = 0; i < 18; i++)
                {
                    float angle = (i / 18f) * Mathf.PI * 2f + 0.25f;
                    float radius = 11.5f + (float)random.NextDouble() * 5.0f;
                    float rx = 6f + Mathf.Sin(angle) * (radius * 1.1f);
                    float rz = 18f + Mathf.Cos(angle) * radius;
                    float ry = 1.0f + (float)random.NextDouble() * 2.2f;

                    var rock = new GameObject($"Mid Shelf Rock {i}");
                    rock.transform.SetParent(cliffRoot.transform, false);
                    rock.transform.position = new Vector3(rx, ry, rz);
                    rock.transform.rotation = Quaternion.Euler((float)random.NextDouble() * 35f, (float)random.NextDouble() * 360f, (float)random.NextDouble() * 35f);
                    float scale = 3.5f + (float)random.NextDouble() * 3.5f;
                    rock.transform.localScale = Vector3.one * scale;
                    rock.AddComponent<MeshFilter>().sharedMesh = rockMesh;
                    rock.AddComponent<MeshRenderer>().sharedMaterial = matRock;
                }

                // Foreground coastal sea crags breaking water
                for (int i = 0; i < 7; i++)
                {
                    float angle = -0.8f + (i / 7f) * 1.6f;
                    float rx = -4.5f + Mathf.Sin(angle) * 12.0f;
                    float rz = 2.0f + Mathf.Cos(angle) * 10.0f;
                    float ry = -0.45f + (float)random.NextDouble() * 0.6f;

                    var rock = new GameObject($"Foreground Crag {i}");
                    rock.transform.SetParent(cliffRoot.transform, false);
                    rock.transform.position = new Vector3(rx, ry, rz);
                    rock.transform.rotation = Quaternion.Euler((float)random.NextDouble() * 20f, (float)random.NextDouble() * 360f, (float)random.NextDouble() * 20f);
                    float scale = 4.0f + (float)random.NextDouble() * 3.5f;
                    rock.transform.localScale = Vector3.one * scale;
                    rock.AddComponent<MeshFilter>().sharedMesh = rockMesh;
                    rock.AddComponent<MeshRenderer>().sharedMaterial = matRock;
                }
            }

            // Decaying shoreline dock timbers and driftwood logs wedged into rock crevices
            var driftwoodGroup = new GameObject("Decaying Shoreline Timbers").transform;
            driftwoodGroup.SetParent(cliffRoot.transform, false);

            var rnd = new System.Random(42);
            for (int i = 0; i < 8; i++)
            {
                float t = i / 8f;
                float ang = t * Mathf.PI * 1.2f - 0.6f;
                float tx = 6f + Mathf.Sin(ang) * 16.5f + (float)rnd.NextDouble() * 2f;
                float tz = 18f + Mathf.Cos(ang) * 13.5f + (float)rnd.NextDouble() * 2f;
                float ty = -0.3f + (float)rnd.NextDouble() * 0.5f;

                var timber = GameObject.CreatePrimitive(PrimitiveType.Cube);
                timber.name = $"Decaying Timber {i}";
                timber.transform.SetParent(driftwoodGroup, false);
                timber.transform.position = new Vector3(tx, ty, tz);
                timber.transform.rotation = Quaternion.Euler((float)rnd.NextDouble() * 35f, (float)rnd.NextDouble() * 360f, (float)rnd.NextDouble() * 45f);
                timber.transform.localScale = new Vector3(0.32f, 2.2f + (float)rnd.NextDouble() * 1.4f, 0.32f);
                timber.GetComponent<Renderer>().sharedMaterial = matWood;
                UnityEngine.Object.DestroyImmediate(timber.GetComponent<Collider>());
            }
        }

        private static Mesh CreatePromontoryBaseMesh()
        {
            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();

            const int rings = 12;
            const int angularSegments = 48;
            Vector3 center = new Vector3(6f, 0, 18f);
            float radiusX = 18f;
            float radiusZ = 14f;
            float plateauHeight = 3.8f;

            for (int r = 0; r <= rings; r++)
            {
                float t = r / (float)rings;
                for (int a = 0; a < angularSegments; a++)
                {
                    float angle = a * Mathf.PI * 2f / angularSegments;
                    float sinA = Mathf.Sin(angle);
                    float cosA = Mathf.Cos(angle);

                    // Jagged harmonic angular noise
                    float harmonic = 1f
                        + 0.16f * Mathf.Sin(angle * 4f + 0.3f)
                        + 0.11f * Mathf.Cos(angle * 7f - 0.5f)
                        + 0.06f * Mathf.Sin(angle * 11f);

                    float x = center.x + sinA * radiusX * t * harmonic;
                    float z = center.z + cosA * radiusZ * t * harmonic;

                    float y;
                    if (t < 0.45f)
                    {
                        // High plateau with subtle undulation
                        float undulation = 0.15f * Mathf.Sin(x * 0.5f) * Mathf.Cos(z * 0.5f);
                        y = plateauHeight + undulation;
                    }
                    else if (t < 0.85f)
                    {
                        // Sharp stepped cliff terraces
                        float cliffT = (t - 0.45f) / 0.40f;
                        float stepped = Mathf.Floor(cliffT * 4f) / 4f;
                        float ledge = Mathf.Lerp(cliffT, stepped, 0.65f);
                        float cliffHeight = Mathf.Lerp(plateauHeight, 0.2f, ledge);
                        float cragNoise = 0.35f * Mathf.Sin(angle * 9f) * Mathf.Cos(x * 0.4f);
                        y = cliffHeight + cragNoise;
                    }
                    else
                    {
                        // Shoreline intertidal slope dipping below sea level
                        float seaT = (t - 0.85f) / 0.15f;
                        y = Mathf.Lerp(0.2f, -1.8f, seaT);
                    }

                    vertices.Add(new Vector3(x, y, z));
                    uvs.Add(new Vector2(x * 0.12f, z * 0.12f));
                }
            }

            for (int r = 0; r < rings; r++)
            {
                int row0 = r * angularSegments;
                int row1 = (r + 1) * angularSegments;
                for (int a = 0; a < angularSegments; a++)
                {
                    int nextA = (a + 1) % angularSegments;
                    triangles.Add(row0 + a);
                    triangles.Add(row1 + a);
                    triangles.Add(row0 + nextA);

                    triangles.Add(row0 + nextA);
                    triangles.Add(row1 + a);
                    triangles.Add(row1 + nextA);
                }
            }

            var mesh = new Mesh { name = "PromontoryBedrockMesh" };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void BuildStoneKeepersLodge(Transform parent)
        {
            var lodgeRoot = new GameObject("Stone Keeper Lodge");
            lodgeRoot.transform.SetParent(parent, false);
            // Angled slightly toward the camera for a dramatic 3/4 perspective
            lodgeRoot.transform.position = new Vector3(0.5f, 3.8f, 16.5f);
            lodgeRoot.transform.rotation = Quaternion.Euler(0f, 14f, 0f);

            // 1. Stone Masonry Lodge Main Hall Walls
            var lodgeBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lodgeBody.name = "Lodge Main Hall Walls";
            lodgeBody.transform.SetParent(lodgeRoot.transform, false);
            lodgeBody.transform.localPosition = new Vector3(0f, 2.0f, 0f);
            lodgeBody.transform.localScale = new Vector3(8.2f, 4.0f, 6.0f);
            if (matStone) lodgeBody.GetComponent<Renderer>().sharedMaterial = matStone;
            UnityEngine.Object.DestroyImmediate(lodgeBody.GetComponent<Collider>());

            // Attic Ceiling Slab (Seals interior light from shining upward under roof)
            var ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ceiling.name = "Attic Ceiling Slab";
            ceiling.transform.SetParent(lodgeRoot.transform, false);
            ceiling.transform.localPosition = new Vector3(0f, 4.05f, 0f);
            ceiling.transform.localScale = new Vector3(8.4f, 0.2f, 6.2f);
            if (matStone) ceiling.GetComponent<Renderer>().sharedMaterial = matStone;
            UnityEngine.Object.DestroyImmediate(ceiling.GetComponent<Collider>());

            // 2. Steep Pitched Slate Gable Roof
            var roofPitch = new GameObject("Pitched Slate Roof");
            roofPitch.transform.SetParent(lodgeRoot.transform, false);
            roofPitch.transform.localPosition = new Vector3(0f, 4.0f, 0f);

            // Left & Right Triangular Gable Stone Wall Ends
            var gableL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gableL.name = "Stone Gable West";
            gableL.transform.SetParent(roofPitch.transform, false);
            gableL.transform.localPosition = new Vector3(-4.0f, 0.95f, 0f);
            gableL.transform.localScale = new Vector3(0.35f, 1.9f, 5.8f);
            if (matStone) gableL.GetComponent<Renderer>().sharedMaterial = matStone;
            UnityEngine.Object.DestroyImmediate(gableL.GetComponent<Collider>());

            var gableR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gableR.name = "Stone Gable East";
            gableR.transform.SetParent(roofPitch.transform, false);
            gableR.transform.localPosition = new Vector3(4.0f, 0.95f, 0f);
            gableR.transform.localScale = new Vector3(0.35f, 1.9f, 5.8f);
            if (matStone) gableR.GetComponent<Renderer>().sharedMaterial = matStone;
            UnityEngine.Object.DestroyImmediate(gableR.GetComponent<Collider>());

            // Front (South) Roof Slope - properly pitched down toward front facade
            var slopeFront = GameObject.CreatePrimitive(PrimitiveType.Cube);
            slopeFront.name = "Roof Slope Front";
            slopeFront.transform.SetParent(roofPitch.transform, false);
            slopeFront.transform.localPosition = new Vector3(0f, 1.05f, -1.60f);
            slopeFront.transform.localRotation = Quaternion.Euler(32f, 0f, 0f);
            slopeFront.transform.localScale = new Vector3(8.8f, 0.18f, 3.8f);
            if (matSlate) slopeFront.GetComponent<Renderer>().sharedMaterial = matSlate;
            UnityEngine.Object.DestroyImmediate(slopeFront.GetComponent<Collider>());

            // Back (North) Roof Slope
            var slopeBack = GameObject.CreatePrimitive(PrimitiveType.Cube);
            slopeBack.name = "Roof Slope Back";
            slopeBack.transform.SetParent(roofPitch.transform, false);
            slopeBack.transform.localPosition = new Vector3(0f, 1.05f, 1.60f);
            slopeBack.transform.localRotation = Quaternion.Euler(-32f, 0f, 0f);
            slopeBack.transform.localScale = new Vector3(8.8f, 0.18f, 3.8f);
            if (matSlate) slopeBack.GetComponent<Renderer>().sharedMaterial = matSlate;
            UnityEngine.Object.DestroyImmediate(slopeBack.GetComponent<Collider>());

            // Roof Ridge Cap
            var ridge = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ridge.name = "Roof Ridge Cap";
            ridge.transform.SetParent(roofPitch.transform, false);
            ridge.transform.localPosition = new Vector3(0f, 2.15f, 0f);
            ridge.transform.localScale = new Vector3(9.0f, 0.16f, 0.35f);
            if (matSlate) ridge.GetComponent<Renderer>().sharedMaterial = matSlate;
            UnityEngine.Object.DestroyImmediate(ridge.GetComponent<Collider>());

            // 3. Stone Chimney Stack on West Gable with Smoke Particles
            var chimney = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chimney.name = "Stone Chimney Stack";
            chimney.transform.SetParent(lodgeRoot.transform, false);
            chimney.transform.localPosition = new Vector3(-3.8f, 4.6f, 0f);
            chimney.transform.localScale = new Vector3(1.15f, 4.8f, 1.15f);
            if (matStone) chimney.GetComponent<Renderer>().sharedMaterial = matStone;
            UnityEngine.Object.DestroyImmediate(chimney.GetComponent<Collider>());

            var chimneyPot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            chimneyPot.name = "Terracotta Chimney Pot";
            chimneyPot.transform.SetParent(chimney.transform, false);
            chimneyPot.transform.localPosition = new Vector3(0f, 0.58f, 0f);
            chimneyPot.transform.localScale = new Vector3(0.52f, 0.22f, 0.52f);
            if (matSlate) chimneyPot.GetComponent<Renderer>().sharedMaterial = matSlate;
            UnityEngine.Object.DestroyImmediate(chimneyPot.GetComponent<Collider>());

            // Chimney Smoke Particle System
            var smokeObj = new GameObject("Chimney Smoke Particles");
            smokeObj.transform.SetParent(chimney.transform, false);
            smokeObj.transform.localPosition = new Vector3(0f, 0.8f, 0f);
            var smokePS = smokeObj.AddComponent<ParticleSystem>();
            var smokeRend = smokeObj.GetComponent<ParticleSystemRenderer>();
            if (matRain) smokeRend.sharedMaterial = matRain;

            var sMain = smokePS.main;
            sMain.startSpeed = 1.2f;
            sMain.startLifetime = 3.5f;
            sMain.startSize = 0.65f;
            sMain.startColor = new Color(0.12f, 0.15f, 0.18f, 0.25f);
            sMain.gravityModifier = -0.05f;
            var sEmit = smokePS.emission;
            sEmit.rateOverTime = 8f;
            var sShape = smokePS.shape;
            sShape.shapeType = ParticleSystemShapeType.Cone;
            sShape.angle = 12f;
            sShape.radius = 0.15f;
            var sVel = smokePS.velocityOverLifetime;
            sVel.enabled = true;
            sVel.x = new ParticleSystem.MinMaxCurve(1.5f, 2.8f);
            sVel.y = new ParticleSystem.MinMaxCurve(0.4f, 0.8f);
            sVel.z = new ParticleSystem.MinMaxCurve(-0.8f, -1.5f);

            // 4. Double-Hung Paned Window on Front South Facade facing Camera
            BuildLodgeWindow(lodgeRoot.transform, new Vector3(1.3f, 2.0f, -3.05f));

            // 5. Timber Front Storm Porch & Heavy Plank Door
            var porch = new GameObject("Front Storm Porch");
            porch.transform.SetParent(lodgeRoot.transform, false);
            porch.transform.localPosition = new Vector3(-1.8f, 0f, -3.05f);

            var door = GameObject.CreatePrimitive(PrimitiveType.Cube);
            door.name = "Keeper Weathered Door";
            door.transform.SetParent(porch.transform, false);
            door.transform.localPosition = new Vector3(0f, 1.35f, 0.05f);
            door.transform.localScale = new Vector3(1.35f, 2.5f, 0.12f);
            if (matWood) door.GetComponent<Renderer>().sharedMaterial = matWood;
            UnityEngine.Object.DestroyImmediate(door.GetComponent<Collider>());

            var porchRoof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            porchRoof.name = "Porch Hood";
            porchRoof.transform.SetParent(porch.transform, false);
            porchRoof.transform.localPosition = new Vector3(0f, 2.75f, -0.55f);
            porchRoof.transform.localRotation = Quaternion.Euler(18f, 0f, 0f);
            porchRoof.transform.localScale = new Vector3(1.9f, 0.12f, 1.35f);
            if (matSlate) porchRoof.GetComponent<Renderer>().sharedMaterial = matSlate;
            UnityEngine.Object.DestroyImmediate(porchRoof.GetComponent<Collider>());
        }

        private static void BuildLodgeWindow(Transform parent, Vector3 windowCenter)
        {
            var windowRoot = new GameObject("Cottage Double-Hung Paned Window");
            windowRoot.transform.SetParent(parent, false);
            windowRoot.transform.localPosition = windowCenter;

            // Outer Wooden Sill and Stone Surround
            var sill = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sill.name = "Window Sill";
            sill.transform.SetParent(windowRoot.transform, false);
            sill.transform.localPosition = new Vector3(0f, -1.15f, -0.10f);
            sill.transform.localScale = new Vector3(2.5f, 0.18f, 0.42f);
            if (matStone) sill.GetComponent<Renderer>().sharedMaterial = matStone;
            UnityEngine.Object.DestroyImmediate(sill.GetComponent<Collider>());

            var lintel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lintel.name = "Window Lintel";
            lintel.transform.SetParent(windowRoot.transform, false);
            lintel.transform.localPosition = new Vector3(0f, 1.15f, -0.06f);
            lintel.transform.localScale = new Vector3(2.5f, 0.22f, 0.35f);
            if (matStone) lintel.GetComponent<Renderer>().sharedMaterial = matStone;
            UnityEngine.Object.DestroyImmediate(lintel.GetComponent<Collider>());

            // Timber Window Outer Casing Frame
            var frameTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frameTop.name = "Frame Outer Top";
            frameTop.transform.SetParent(windowRoot.transform, false);
            frameTop.transform.localPosition = new Vector3(0f, 1.02f, 0f);
            frameTop.transform.localScale = new Vector3(2.05f, 0.10f, 0.18f);
            if (matWood) frameTop.GetComponent<Renderer>().sharedMaterial = matWood;
            UnityEngine.Object.DestroyImmediate(frameTop.GetComponent<Collider>());

            var frameLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frameLeft.name = "Frame Outer Left";
            frameLeft.transform.SetParent(windowRoot.transform, false);
            frameLeft.transform.localPosition = new Vector3(-1.0f, 0f, 0f);
            frameLeft.transform.localScale = new Vector3(0.12f, 2.05f, 0.18f);
            if (matWood) frameLeft.GetComponent<Renderer>().sharedMaterial = matWood;
            UnityEngine.Object.DestroyImmediate(frameLeft.GetComponent<Collider>());

            var frameRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frameRight.name = "Frame Outer Right";
            frameRight.transform.SetParent(windowRoot.transform, false);
            frameRight.transform.localPosition = new Vector3(1.0f, 0f, 0f);
            frameRight.transform.localScale = new Vector3(0.12f, 2.05f, 0.18f);
            if (matWood) frameRight.GetComponent<Renderer>().sharedMaterial = matWood;
            UnityEngine.Object.DestroyImmediate(frameRight.GetComponent<Collider>());

            // Cross-Pane Wooden Mullion (Vertical Center)
            var mullion = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mullion.name = "Mullion Center";
            mullion.transform.SetParent(windowRoot.transform, false);
            mullion.transform.localPosition = new Vector3(0f, 0f, -0.02f);
            mullion.transform.localScale = new Vector3(0.09f, 1.95f, 0.12f);
            if (matWood) mullion.GetComponent<Renderer>().sharedMaterial = matWood;
            UnityEngine.Object.DestroyImmediate(mullion.GetComponent<Collider>());

            // Horizontal Muntin Bars (Creating 6 Panes)
            var muntin1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            muntin1.name = "Muntin Upper";
            muntin1.transform.SetParent(windowRoot.transform, false);
            muntin1.transform.localPosition = new Vector3(0f, 0.35f, -0.02f);
            muntin1.transform.localScale = new Vector3(1.95f, 0.07f, 0.10f);
            if (matWood) muntin1.GetComponent<Renderer>().sharedMaterial = matWood;
            UnityEngine.Object.DestroyImmediate(muntin1.GetComponent<Collider>());

            var muntin2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            muntin2.name = "Muntin Lower";
            muntin2.transform.SetParent(windowRoot.transform, false);
            muntin2.transform.localPosition = new Vector3(0f, -0.35f, -0.02f);
            muntin2.transform.localScale = new Vector3(1.95f, 0.07f, 0.10f);
            if (matWood) muntin2.GetComponent<Renderer>().sharedMaterial = matWood;
            UnityEngine.Object.DestroyImmediate(muntin2.GetComponent<Collider>());

            // Frosted / Translucent Rain Glass Quad
            var glass = GameObject.CreatePrimitive(PrimitiveType.Quad);
            glass.name = "Frosted Rain Glass";
            glass.transform.SetParent(windowRoot.transform, false);
            glass.transform.localPosition = new Vector3(0f, 0f, -0.01f);
            glass.transform.localScale = new Vector3(1.95f, 1.95f, 1f);
            if (matWindowGlass) glass.GetComponent<Renderer>().sharedMaterial = matWindowGlass;
            UnityEngine.Object.DestroyImmediate(glass.GetComponent<Collider>());

            // Interior Room Silhouettes (Placed 0.25m behind glass)
            var silRoot = new GameObject("Interior Room Silhouettes");
            silRoot.transform.SetParent(windowRoot.transform, false);
            silRoot.transform.localPosition = new Vector3(0f, 0f, 0.25f);

            // Left Curtain Silhouette
            var curtL = GameObject.CreatePrimitive(PrimitiveType.Quad);
            curtL.name = "Curtain Left Silhouette";
            curtL.transform.SetParent(silRoot.transform, false);
            curtL.transform.localPosition = new Vector3(-0.72f, 0.1f, 0f);
            curtL.transform.localScale = new Vector3(0.55f, 1.8f, 1f);
            if (matIron) curtL.GetComponent<Renderer>().sharedMaterial = matIron;
            UnityEngine.Object.DestroyImmediate(curtL.GetComponent<Collider>());

            // Right Curtain Silhouette
            var curtR = GameObject.CreatePrimitive(PrimitiveType.Quad);
            curtR.name = "Curtain Right Silhouette";
            curtR.transform.SetParent(silRoot.transform, false);
            curtR.transform.localPosition = new Vector3(0.72f, 0.1f, 0f);
            curtR.transform.localScale = new Vector3(0.55f, 1.8f, 1f);
            if (matIron) curtR.GetComponent<Renderer>().sharedMaterial = matIron;
            UnityEngine.Object.DestroyImmediate(curtR.GetComponent<Collider>());

            // Tabletop Candlestick / Dish Rack Silhouette
            var tableSil = GameObject.CreatePrimitive(PrimitiveType.Quad);
            tableSil.name = "Tabletop Silhouette";
            tableSil.transform.SetParent(silRoot.transform, false);
            tableSil.transform.localPosition = new Vector3(0.12f, -0.55f, -0.05f);
            tableSil.transform.localScale = new Vector3(0.70f, 0.60f, 1f);
            if (matIron) tableSil.GetComponent<Renderer>().sharedMaterial = matIron;
            UnityEngine.Object.DestroyImmediate(tableSil.GetComponent<Collider>());

            // Warm Incandescent Interior Point Light (2700K #FFAE56)
            var lightGo = new GameObject("Cottage Warm Incandescent Glow");
            lightGo.transform.SetParent(windowRoot.transform, false);
            lightGo.transform.localPosition = new Vector3(0f, 0.05f, 0.85f);

            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1.0f, 0.68f, 0.35f); // 2700K incandescent
            light.intensity = 3.6f;
            light.range = 8.5f;
            light.shadows = LightShadows.None;

            // Project warm window glow onto the rock ledge outside
            var spotGo = new GameObject("Window Sill Ground Light");
            spotGo.transform.SetParent(windowRoot.transform, false);
            spotGo.transform.localPosition = new Vector3(0f, -0.1f, -0.15f);
            spotGo.transform.localRotation = Quaternion.Euler(50f, 0f, 0f);

            var spotLight = spotGo.AddComponent<Light>();
            spotLight.type = LightType.Spot;
            spotLight.color = new Color(1.0f, 0.68f, 0.35f);
            spotLight.intensity = 4.2f;
            spotLight.range = 7.5f;
            spotLight.spotAngle = 60f;
            spotLight.innerSpotAngle = 30f;
            spotLight.shadows = LightShadows.Soft;
        }

        private static Transform BuildLighthouseTower(Transform parent)
        {
            var towerRoot = new GameObject("Upgraded Lighthouse Tower");
            towerRoot.transform.SetParent(parent, false);
            Vector3 towerBasePos = new Vector3(8.5f, 3.8f, 19.5f);

            // 1. Tapered 32-Sided Masonry Tower Shaft
            // Base at y = 3.8m, height 26.5m, tapering from r = 3.8m to r = 2.7m
            var towerMesh = CreateTaperedTowerMesh(3.8f, 2.7f, 26.5f, 32);
            var shaft = new GameObject("Tapered Masonry Shaft");
            shaft.transform.SetParent(towerRoot.transform, false);
            shaft.transform.position = towerBasePos;
            shaft.AddComponent<MeshFilter>().sharedMesh = towerMesh;
            shaft.AddComponent<MeshRenderer>().sharedMaterial = matStone;

            // 2. Structural Iron Rivet Open Tubular Bands (NO solid discs)
            float[] ringHeights = { 7.0f, 13.5f, 20.0f, 25.5f };
            foreach (float y in ringHeights)
            {
                float t = y / 26.5f;
                float r = Mathf.Lerp(3.8f, 2.7f, t) + 0.04f;
                var bandMesh = CreateTubularBandMesh(r, 0.22f, 32);
                var band = new GameObject($"Tower Iron Rivet Band {y:F0}m");
                band.transform.SetParent(towerRoot.transform, false);
                band.transform.position = towerBasePos + new Vector3(0f, y, 0f);
                band.AddComponent<MeshFilter>().sharedMesh = bandMesh;
                band.AddComponent<MeshRenderer>().sharedMaterial = matIron;
            }

            // 3. Recessed Vertical Watch Windows
            float[] winHeights = { 9.5f, 16.5f, 23.0f };
            foreach (float y in winHeights)
            {
                float t = y / 26.5f;
                float r = Mathf.Lerp(3.8f, 2.7f, t);
                var win = GameObject.CreatePrimitive(PrimitiveType.Cube);
                win.name = $"Tower Slit Window {y:F0}m";
                win.transform.SetParent(towerRoot.transform, false);
                win.transform.position = towerBasePos + new Vector3(-r * 0.96f, y, -r * 0.25f);
                win.transform.rotation = Quaternion.Euler(0f, 15f, 0f);
                win.transform.localScale = new Vector3(0.42f, 1.4f, 0.85f);
                if (matWood) win.GetComponent<Renderer>().sharedMaterial = matWood;
                UnityEngine.Object.DestroyImmediate(win.GetComponent<Collider>());
            }

            // 4. Cantilevered Watch Gallery Balcony Platform & Metal Railings
            float galleryY = 26.5f;
            float galleryRadius = 3.6f;

            // Flared Masonry Capital Corbel beneath gallery (tapers out from tower shaft r=2.7m to 3.5m)
            var corbelMesh = CreateTaperedTowerMesh(2.7f, 3.5f, 1.2f, 32);
            var corbelCapital = new GameObject("Gallery Masonry Corbel Capital");
            corbelCapital.transform.SetParent(towerRoot.transform, false);
            corbelCapital.transform.position = towerBasePos + new Vector3(0f, galleryY - 1.2f, 0f);
            corbelCapital.AddComponent<MeshFilter>().sharedMesh = corbelMesh;
            corbelCapital.AddComponent<MeshRenderer>().sharedMaterial = matStone;

            // Gallery stone masonry floor slab
            var galleryFloorMesh = CreateCylinderPlateMesh(galleryRadius, 0.35f, 32);
            var galleryFloor = new GameObject("Watch Gallery Platform Floor");
            galleryFloor.transform.SetParent(towerRoot.transform, false);
            galleryFloor.transform.position = towerBasePos + new Vector3(0f, galleryY, 0f);
            galleryFloor.AddComponent<MeshFilter>().sharedMesh = galleryFloorMesh;
            galleryFloor.AddComponent<MeshRenderer>().sharedMaterial = matStone;

            // Outer Structural Iron Curb Band
            var curbMesh = CreateTubularBandMesh(galleryRadius + 0.02f, 0.36f, 32);
            var curbBand = new GameObject("Gallery Outer Iron Curb");
            curbBand.transform.SetParent(towerRoot.transform, false);
            curbBand.transform.position = towerBasePos + new Vector3(0f, galleryY, 0f);
            curbBand.AddComponent<MeshFilter>().sharedMesh = curbMesh;
            curbBand.AddComponent<MeshRenderer>().sharedMaterial = matIron;

            // Metal Balcony Railings
            var railingGroup = new GameObject("Metal Balcony Railings").transform;
            railingGroup.SetParent(towerRoot.transform, false);
            railingGroup.position = towerBasePos + new Vector3(0f, galleryY + 0.20f, 0f);

            const int balusters = 20;
            for (int i = 0; i < balusters; i++)
            {
                float angle = i * Mathf.PI * 2f / balusters;
                float bx = Mathf.Sin(angle) * (galleryRadius - 0.15f);
                float bz = Mathf.Cos(angle) * (galleryRadius - 0.15f);

                var post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                post.name = $"Balcony Stanchion {i}";
                post.transform.SetParent(railingGroup, false);
                post.transform.localPosition = new Vector3(bx, 0.55f, bz);
                post.transform.localScale = new Vector3(0.08f, 0.55f, 0.08f);
                if (matIron) post.GetComponent<Renderer>().sharedMaterial = matIron;
                UnityEngine.Object.DestroyImmediate(post.GetComponent<Collider>());
            }

            // Top Handrail Ring (Tubular)
            var handrailMesh = CreateTubularBandMesh(galleryRadius - 0.15f, 0.10f, 32);
            var handrail = new GameObject("Balcony Top Handrail");
            handrail.transform.SetParent(railingGroup, false);
            handrail.transform.localPosition = new Vector3(0f, 1.05f, 0f);
            handrail.AddComponent<MeshFilter>().sharedMesh = handrailMesh;
            handrail.AddComponent<MeshRenderer>().sharedMaterial = matIron;

            // 5. Lantern Room Glass Chamber & Glazing Structure
            float lanternY = galleryY + 0.45f;
            var chamber = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            chamber.name = "Lantern Glass Chamber";
            chamber.transform.SetParent(towerRoot.transform, false);
            chamber.transform.position = towerBasePos + new Vector3(0f, lanternY + 1.8f, 0f);
            chamber.transform.localScale = new Vector3(4.8f, 1.8f, 4.8f);
            if (matLanternGlass) chamber.GetComponent<Renderer>().sharedMaterial = matLanternGlass;
            UnityEngine.Object.DestroyImmediate(chamber.GetComponent<Collider>());

            // Vertical Glazing Astragals (Iron Struts)
            var astragalsRoot = new GameObject("Lantern Glazing Astragals").transform;
            astragalsRoot.SetParent(towerRoot.transform, false);
            astragalsRoot.position = towerBasePos + new Vector3(0f, lanternY + 1.8f, 0f);
            const int numAstragals = 16;
            for (int i = 0; i < numAstragals; i++)
            {
                float angle = i * Mathf.PI * 2f / numAstragals;
                float ax = Mathf.Sin(angle) * 2.42f;
                float az = Mathf.Cos(angle) * 2.42f;
                var strut = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                strut.name = $"Astragal {i}";
                strut.transform.SetParent(astragalsRoot, false);
                strut.transform.localPosition = new Vector3(ax, 0f, az);
                strut.transform.localScale = new Vector3(0.09f, 1.8f, 0.09f);
                if (matIron) strut.GetComponent<Renderer>().sharedMaterial = matIron;
                UnityEngine.Object.DestroyImmediate(strut.GetComponent<Collider>());
            }

            // Central Pedestal Base for Lantern Beacon
            var pedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pedestal.name = "Fresnel Pedestal Base";
            pedestal.transform.SetParent(towerRoot.transform, false);
            pedestal.transform.position = towerBasePos + new Vector3(0f, lanternY + 0.6f, 0f);
            pedestal.transform.localScale = new Vector3(1.6f, 0.6f, 1.6f);
            if (matIron) pedestal.GetComponent<Renderer>().sharedMaterial = matIron;
            UnityEngine.Object.DestroyImmediate(pedestal.GetComponent<Collider>());

            // 6. Copper Lantern Dome Roof
            float domeY = lanternY + 3.6f;
            var dome = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dome.name = "Copper Lantern Dome";
            dome.transform.SetParent(towerRoot.transform, false);
            dome.transform.position = towerBasePos + new Vector3(0f, domeY + 0.8f, 0f);
            dome.transform.localScale = new Vector3(5.4f, 2.2f, 5.4f);
            if (matCopper) dome.GetComponent<Renderer>().sharedMaterial = matCopper;
            UnityEngine.Object.DestroyImmediate(dome.GetComponent<Collider>());

            // Copper Ball Finial & Lightning Rod
            var finial = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            finial.name = "Lightning Rod Spire";
            finial.transform.SetParent(towerRoot.transform, false);
            finial.transform.position = towerBasePos + new Vector3(0f, domeY + 3.0f, 0f);
            finial.transform.localScale = new Vector3(0.08f, 1.8f, 0.08f);
            if (matIron) finial.GetComponent<Renderer>().sharedMaterial = matIron;
            UnityEngine.Object.DestroyImmediate(finial.GetComponent<Collider>());

            // 7. Sweeping High-Intensity Volumetric Beacon Spotlight
            var beaconPivot = new GameObject("Lighthouse Beacon Pivot").transform;
            beaconPivot.SetParent(towerRoot.transform, false);
            beaconPivot.position = towerBasePos + new Vector3(0f, lanternY + 1.8f, 0f);

            var spotObj = new GameObject("Beacon Sweeping Spotlight");
            spotObj.transform.SetParent(beaconPivot, false);
            spotObj.transform.localPosition = Vector3.zero;
            spotObj.transform.localRotation = Quaternion.Euler(14f, 0f, 0f);

            var spot = spotObj.AddComponent<Light>();
            spot.type = LightType.Spot;
            spot.color = new Color(1.0f, 0.94f, 0.82f); // Warm halogen / kerosene white
            spot.intensity = 22.0f;
            spot.range = 175.0f;
            spot.spotAngle = 36.0f;
            spot.innerSpotAngle = 18.0f;
            spot.shadows = LightShadows.Soft;

            // Volumetric Cone Mesh
            var coneMesh = CreateVolumetricConeMesh(18.0f, 140.0f, 32);
            var coneObj = new GameObject("Beacon Volumetric Light Cone");
            coneObj.transform.SetParent(spotObj.transform, false);
            coneObj.transform.localPosition = Vector3.zero;
            coneObj.transform.localRotation = Quaternion.identity;
            coneObj.AddComponent<MeshFilter>().sharedMesh = coneMesh;
            coneObj.AddComponent<MeshRenderer>().sharedMaterial = matBeam;

            return beaconPivot;
        }

        private static Mesh CreateTaperedTowerMesh(float bottomRadius, float topRadius, float height, int segments)
        {
            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();

            for (int yStep = 0; yStep <= 1; yStep++)
            {
                float y = yStep * height;
                float r = Mathf.Lerp(bottomRadius, topRadius, (float)yStep);
                for (int i = 0; i < segments; i++)
                {
                    float angle = i * Mathf.PI * 2f / segments;
                    float x = Mathf.Sin(angle) * r;
                    float z = Mathf.Cos(angle) * r;

                    vertices.Add(new Vector3(x, y, z));
                    uvs.Add(new Vector2((float)i / segments * 3f, (float)yStep * 6f));
                }
            }

            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                int b0 = i;
                int b1 = next;
                int t0 = segments + i;
                int t1 = segments + next;

                triangles.Add(b0);
                triangles.Add(t0);
                triangles.Add(b1);

                triangles.Add(b1);
                triangles.Add(t0);
                triangles.Add(t1);
            }

            var mesh = new Mesh { name = "TaperedTowerMesh" };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh CreateTubularBandMesh(float radius, float height, int segments)
        {
            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();

            float halfH = height * 0.5f;
            for (int step = 0; step <= 1; step++)
            {
                float y = (step == 0) ? -halfH : halfH;
                for (int i = 0; i < segments; i++)
                {
                    float angle = i * Mathf.PI * 2f / segments;
                    float x = Mathf.Sin(angle) * radius;
                    float z = Mathf.Cos(angle) * radius;
                    vertices.Add(new Vector3(x, y, z));
                    uvs.Add(new Vector2((float)i / segments * 4f, step));
                }
            }

            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                int b0 = i;
                int b1 = next;
                int t0 = segments + i;
                int t1 = segments + next;

                triangles.Add(b0);
                triangles.Add(t0);
                triangles.Add(b1);

                triangles.Add(b1);
                triangles.Add(t0);
                triangles.Add(t1);
            }

            var mesh = new Mesh { name = "TubularBandMesh" };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh CreateCylinderPlateMesh(float radius, float height, int segments)
        {
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();

            float halfH = height * 0.5f;

            // 1. Bottom Cap (Facing DOWN - Vector3.down)
            int bottomCenterIndex = vertices.Count;
            vertices.Add(new Vector3(0, -halfH, 0));
            normals.Add(Vector3.down);
            uvs.Add(new Vector2(0.5f, 0.5f));

            int bottomRimStart = vertices.Count;
            for (int i = 0; i < segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                float x = Mathf.Sin(angle) * radius;
                float z = Mathf.Cos(angle) * radius;
                vertices.Add(new Vector3(x, -halfH, z));
                normals.Add(Vector3.down);
                uvs.Add(new Vector2(x / (radius * 2f) + 0.5f, z / (radius * 2f) + 0.5f));
            }
            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                triangles.Add(bottomCenterIndex);
                triangles.Add(bottomRimStart + next);
                triangles.Add(bottomRimStart + i);
            }

            // 2. Top Cap (Facing UP - Vector3.up)
            int topCenterIndex = vertices.Count;
            vertices.Add(new Vector3(0, halfH, 0));
            normals.Add(Vector3.up);
            uvs.Add(new Vector2(0.5f, 0.5f));

            int topRimStart = vertices.Count;
            for (int i = 0; i < segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                float x = Mathf.Sin(angle) * radius;
                float z = Mathf.Cos(angle) * radius;
                vertices.Add(new Vector3(x, halfH, z));
                normals.Add(Vector3.up);
                uvs.Add(new Vector2(x / (radius * 2f) + 0.5f, z / (radius * 2f) + 0.5f));
            }
            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                triangles.Add(topCenterIndex);
                triangles.Add(topRimStart + i);
                triangles.Add(topRimStart + next);
            }

            // 3. Cylinder Sides (Facing OUTWARD with sharp horizontal normals)
            int sideStart = vertices.Count;
            for (int i = 0; i < segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                float x = Mathf.Sin(angle);
                float z = Mathf.Cos(angle);
                Vector3 n = new Vector3(x, 0, z);

                // Bottom side vertex
                vertices.Add(new Vector3(x * radius, -halfH, z * radius));
                normals.Add(n);
                uvs.Add(new Vector2((float)i / segments * 4f, 0f));

                // Top side vertex
                vertices.Add(new Vector3(x * radius, halfH, z * radius));
                normals.Add(n);
                uvs.Add(new Vector2((float)i / segments * 4f, 1f));
            }
            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                int b0 = sideStart + i * 2;
                int t0 = sideStart + i * 2 + 1;
                int b1 = sideStart + next * 2;
                int t1 = sideStart + next * 2 + 1;

                triangles.Add(b0);
                triangles.Add(t0);
                triangles.Add(b1);

                triangles.Add(b1);
                triangles.Add(t0);
                triangles.Add(t1);
            }

            var mesh = new Mesh { name = "CylinderPlateMesh" };
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh CreateVolumetricConeMesh(float baseRadius, float length, int segments)
        {
            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var normals = new List<Vector3>();
            var triangles = new List<int>();

            // Apex at (0,0,0)
            vertices.Add(Vector3.zero);
            uvs.Add(new Vector2(0.5f, 0f));
            normals.Add(-Vector3.forward);

            for (int i = 0; i <= segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                float x = Mathf.Sin(angle) * baseRadius;
                float y = Mathf.Cos(angle) * baseRadius;
                Vector3 basePt = new Vector3(x, y, length);

                vertices.Add(basePt);
                uvs.Add(new Vector2((float)i / segments, 1.0f));
                normals.Add(new Vector3(Mathf.Sin(angle), Mathf.Cos(angle), 0f));
            }

            for (int i = 1; i <= segments; i++)
            {
                triangles.Add(0);
                triangles.Add(i);
                triangles.Add(i + 1);
            }

            var mesh = new Mesh { name = "VolumetricLightConeMesh" };
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        private static GameObject BuildMenuCamera(Transform beaconPivot)
        {
            var cameraObj = new GameObject("Menu Camera", typeof(Camera), typeof(AudioListener));
            cameraObj.tag = "MainCamera";

            // Low-angle camera tilted upward toward the looming 35m tower
            cameraObj.transform.position = new Vector3(-16.0f, 3.2f, -21.0f);
            cameraObj.transform.rotation = Quaternion.Euler(-13.0f, 32.0f, 0f);

            var cam = cameraObj.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.Skybox;
            cam.fieldOfView = 50f;
            cam.nearClipPlane = 0.25f;
            cam.farClipPlane = 350f;

            var uCamData = cameraObj.AddComponent<UniversalAdditionalCameraData>();
            uCamData.requiresDepthTexture = true;
            uCamData.renderPostProcessing = true;

            // Ambient Sea Surf and Storm Rain Audio
            var audioAmb = cameraObj.AddComponent<AudioSource>();
            audioAmb.clip = audioWaves;
            audioAmb.loop = true;
            audioAmb.volume = 0.40f;
            audioAmb.playOnAwake = true;
            audioAmb.spatialBlend = 0f;
            audioAmb.Play();

            // Menu Cinematic Drift, Sway, Beacon Sweeping & Audio Manager
            var camSway = cameraObj.AddComponent<MenuCameraController>();
            Set(camSway, "swayAmount", 0.22f);
            Set(camSway, "swaySpeed", 0.26f);
            Set(camSway, "pitchSway", 0.50f);
            Set(camSway, "yawSway", 0.75f);
            Set(camSway, "dollySpeed", 0.016f);
            Set(camSway, "dollyMaxDistance", 1.15f);
            Set(camSway, "lighthouseBeam", beaconPivot);
            Set(camSway, "beamRotationSpeed", 16.0f);
            Set(camSway, "hoverSound", audioCello);
            Set(camSway, "clickSound", audioClick);

            var lightningLight = GameObject.Find("Distant Lightning")?.GetComponent<Light>();
            if (lightningLight) Set(camSway, "lightningLight", lightningLight);
            if (matSky) Set(camSway, "skyboxMaterial", matSky);

            // Driving Diagonal Rain Streaks across viewport (World-space downward fall to ground/ocean)
            var rainObj = new GameObject("Viewport Driving Rain Particles");
            rainObj.transform.SetParent(cameraObj.transform, false);
            rainObj.transform.localPosition = new Vector3(2f, 14f, 16f);

            var rainPS = rainObj.AddComponent<ParticleSystem>();
            var rainRend = rainObj.GetComponent<ParticleSystemRenderer>();
            rainRend.renderMode = ParticleSystemRenderMode.Stretch;
            rainRend.cameraVelocityScale = 0f;
            rainRend.velocityScale = 0.016f;
            rainRend.lengthScale = 1.0f;
            if (matRain) rainRend.sharedMaterial = matRain;

            var rMain = rainPS.main;
            rMain.startSpeed = 0f;
            rMain.startLifetime = 1.05f;
            rMain.startSize = 0.012f; // Fine atmospheric threads
            rMain.maxParticles = 1400;
            rMain.simulationSpace = ParticleSystemSimulationSpace.World;
            rMain.startColor = new Color(0.72f, 0.84f, 0.94f, 0.18f); // Soft alpha
            var rEmit = rainPS.emission;
            rEmit.rateOverTime = 650f;
            var rShape = rainPS.shape;
            rShape.shapeType = ParticleSystemShapeType.Box;
            rShape.scale = new Vector3(38f, 2f, 28f);
            var rVel = rainPS.velocityOverLifetime;
            rVel.enabled = true;
            rVel.space = ParticleSystemSimulationSpace.World;
            rVel.x = new ParticleSystem.MinMaxCurve(-4.0f, -5.5f);
            rVel.y = new ParticleSystem.MinMaxCurve(-20.0f, -24.0f);
            rVel.z = new ParticleSystem.MinMaxCurve(1.0f, 2.0f);

            // Low-altitude wind-blown mist rolling over ocean
            var mistObj = new GameObject("Ocean Shore Mist Particles");
            mistObj.transform.SetParent(cameraObj.transform, false);
            mistObj.transform.localPosition = new Vector3(4f, -1.8f, 12f);

            var mistPS = mistObj.AddComponent<ParticleSystem>();
            var mistRend = mistObj.GetComponent<ParticleSystemRenderer>();
            if (matRain) mistRend.sharedMaterial = matRain;

            var mMain = mistPS.main;
            mMain.startSpeed = 1.4f;
            mMain.startLifetime = 4.2f;
            mMain.startSize = 4.5f;
            mMain.startColor = new Color(0.18f, 0.24f, 0.30f, 0.14f);
            var mEmit = mistPS.emission;
            mEmit.rateOverTime = 14f;
            var mShape = mistPS.shape;
            mShape.shapeType = ParticleSystemShapeType.Box;
            mShape.scale = new Vector3(32f, 2f, 24f);

            return cameraObj;
        }

        private static void BuildMenuCanvas(ActFlowController flow, MenuCameraController camController)
        {
            var canvasObject = new GameObject("Main Menu Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            var menu = canvasObject.AddComponent<MainMenu>();
            Set(menu, "flow", flow);
            Set(menu, "root", canvasObject);

            // Left-docked Soft Horizontal Gradient Fade (no harsh black box)
            var gradientObj = new GameObject("Left Horizontal Gradient Panel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            gradientObj.transform.SetParent(canvasObject.transform, false);

            var gRect = gradientObj.GetComponent<RectTransform>();
            gRect.anchorMin = new Vector2(0f, 0f);
            gRect.anchorMax = new Vector2(0f, 1f);
            gRect.pivot = new Vector2(0f, 0.5f);
            gRect.anchoredPosition = Vector2.zero;
            gRect.sizeDelta = new Vector2(750f, 0f);

            var gImg = gradientObj.GetComponent<Image>();
            if (spriteGradient) gImg.sprite = spriteGradient;
            gImg.color = Color.white;
            gImg.raycastTarget = false;

            // Title & Subtitle Container (Generous width to prevent truncation)
            var titleRoot = RectObject("Title Block", gradientObj.transform, new Vector2(85f, 375f), new Vector2(820f, 170f));
            SetAnchorLeft(titleRoot.GetComponent<RectTransform>());

            // "THE LIGHTHOUSE" (Cinzel Roman Serif with wide tracking)
            Text title = Label("T  H  E    L  I  G  H  T  H  O  U  S  E", titleRoot.transform, new Vector2(0f, 50f), new Vector2(820f, 62f), 38, TextAnchor.MiddleLeft);
            title.font = fontCinzel ?? title.font;
            title.fontStyle = FontStyle.Normal;
            title.color = ColorTitle;
            var shadow = title.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0.02f, 0.03f, 0.04f, 0.95f);
            shadow.effectDistance = new Vector2(2f, -2f);

            // Subtitle "HARROW ROCK" (Small-caps Muted Amber #D4A359)
            Text subtitle = Label("H A R R O W   R O C K", titleRoot.transform, new Vector2(0f, 6f), new Vector2(820f, 28f), 16, TextAnchor.MiddleLeft);
            subtitle.font = fontCinzel ?? subtitle.font;
            subtitle.color = ColorSubtitle;

            // Tagline "Keep the light. Never answer the singing." (Italicized #A0A5AA)
            Text tagline = Label("Keep the light. Never answer the singing.", titleRoot.transform, new Vector2(0f, -26f), new Vector2(820f, 26f), 15, TextAnchor.MiddleLeft);
            tagline.font = fontCinzel ?? tagline.font;
            tagline.fontStyle = FontStyle.Italic;
            tagline.color = ColorTagline;

            // Clean Text Menu Buttons (No button boxes, gold hover state, cello audio)
            float btnStartY = 190f;
            float btnSpacing = 52f;

            var btnNew = TextButton("NEW GAME", gradientObj.transform, new Vector2(85f, btnStartY), menu.StartGame, camController);
            var btnContinue = TextButton("CONTINUE", gradientObj.transform, new Vector2(85f, btnStartY - btnSpacing), menu.ContinueGame, camController);
            var btnChapters = TextButton("CHAPTER SELECT", gradientObj.transform, new Vector2(85f, btnStartY - btnSpacing * 2), menu.ShowChapterSelect, camController);
            var btnLoad = TextButton("LOAD GAME", gradientObj.transform, new Vector2(85f, btnStartY - btnSpacing * 3), menu.ShowLoadGame, camController);
            var btnSettings = TextButton("SETTINGS", gradientObj.transform, new Vector2(85f, btnStartY - btnSpacing * 4), menu.ShowSettings, camController);
            var btnExtras = TextButton("EXTRAS", gradientObj.transform, new Vector2(85f, btnStartY - btnSpacing * 5), menu.ShowExtras, camController);
            var btnCredits = TextButton("CREDITS", gradientObj.transform, new Vector2(85f, btnStartY - btnSpacing * 6), menu.ShowCredits, camController);
            var btnQuit = TextButton("QUIT", gradientObj.transform, new Vector2(85f, btnStartY - btnSpacing * 7), menu.Quit, camController);

            Set(menu, "continueButton", btnContinue);

            // Subtle Footer Hotkey Hint
            var hint = Label("F1 Debug Suite  •  F4 Engine Overlay", gradientObj.transform, new Vector2(85f, -485f), new Vector2(500f, 24f), 12, TextAnchor.MiddleLeft);
            SetAnchorLeft(hint.GetComponent<RectTransform>());
            hint.font = fontCinzel ?? hint.font;
            hint.color = new Color(ColorTagline.r, ColorTagline.g, ColorTagline.b, 0.40f);

            // Submenus
            BuildSubmenus(canvasObject.transform, menu, flow);
        }

        private static Button TextButton(string label, Transform parent, Vector2 position, UnityEngine.Events.UnityAction action, MenuCameraController camController)
        {
            var root = RectObject(label, parent, position, new Vector2(360f, 44f));
            SetAnchorLeft(root.GetComponent<RectTransform>());

            var image = root.AddComponent<Image>();
            image.color = Color.clear; // Transparent hit-area

            var button = root.AddComponent<Button>();
            if (action != null) UnityEventTools.AddPersistentListener(button.onClick, action);

            Text text = Label(label, root.transform, Vector2.zero, new Vector2(360f, 44f), 18, TextAnchor.MiddleLeft);
            text.font = fontCinzel ?? text.font;
            text.raycastTarget = false;

            button.targetGraphic = text;
            button.transition = Selectable.Transition.ColorTint;
            button.colors = new ColorBlock
            {
                normalColor = ColorTextNormal,
                highlightedColor = ColorTextHover,
                pressedColor = ColorTextPressed,
                selectedColor = ColorTextHover,
                disabledColor = ColorTextDisabled,
                colorMultiplier = 1f,
                fadeDuration = 0.12f
            };

            // Audio feedback component
            var btnAudio = root.AddComponent<MenuButtonAudio>();
            btnAudio.SetController(camController);

            return button;
        }

        private static void BuildSubmenus(Transform canvasRoot, MainMenu menu, ActFlowController flow)
        {
            // 1. Chapter Select Modal
            var chapterPanel = Image("Chapter Select Panel", canvasRoot, Vector2.zero, new Vector2(760f, 620f), SubmenuPanelColor);
            var chapterUI = chapterPanel.gameObject.AddComponent<ChapterSelectUI>();
            Set(chapterUI, "panel", chapterPanel.gameObject);
            Set(chapterUI, "flow", flow);
            var chTitle = Label("CHAPTER SELECT", chapterPanel.transform, new Vector2(0f, 250f), new Vector2(650f, 50f), 26, TextAnchor.MiddleCenter);
            chTitle.font = fontCinzel ?? chTitle.font;
            chTitle.color = ColorSubtitle;

            var chapterButtons = new Button[5];
            var chapterStatus = new Text[5];
            for (int i = 0; i < 5; i++)
            {
                var chBtn = ModalButton($"ACT {i + 1}", chapterPanel.transform, new Vector2(0f, 170f - i * 65f), null, new Vector2(620f, 52f));
                chapterButtons[i] = chBtn;
                chapterStatus[i] = chBtn.GetComponentInChildren<Text>();
            }
            var chBack = ModalButton("BACK", chapterPanel.transform, new Vector2(0f, -240f), chapterUI.Close, new Vector2(240f, 46f));
            Set(chapterUI, "backButton", chBack);
            SetArray(chapterUI, "chapterButtons", chapterButtons);
            SetArray(chapterUI, "chapterStatusLabels", chapterStatus);
            chapterPanel.gameObject.SetActive(false);
            Set(menu, "chapterSelect", chapterUI);

            // 2. Save / Load Modal
            var saveLoadPanel = Image("Save Load Panel", canvasRoot, Vector2.zero, new Vector2(760f, 580f), SubmenuPanelColor);
            var saveLoadUI = saveLoadPanel.gameObject.AddComponent<SaveLoadUI>();
            Set(saveLoadUI, "panel", saveLoadPanel.gameObject);
            Set(saveLoadUI, "flow", flow);
            var slTitle = Label("LOAD GAME", saveLoadPanel.transform, new Vector2(0f, 230f), new Vector2(650f, 50f), 26, TextAnchor.MiddleCenter);
            slTitle.font = fontCinzel ?? slTitle.font;
            slTitle.color = ColorSubtitle;
            Set(saveLoadUI, "headerTitle", slTitle);

            var slotButtons = new Button[3];
            var slotInfo = new Text[3];
            for (int i = 0; i < 3; i++)
            {
                var sBtn = ModalButton($"Slot {i + 1}", saveLoadPanel.transform, new Vector2(0f, 130f - i * 85f), null, new Vector2(620f, 70f));
                slotButtons[i] = sBtn;
                slotInfo[i] = sBtn.GetComponentInChildren<Text>();
            }
            var slBack = ModalButton("BACK", saveLoadPanel.transform, new Vector2(0f, -220f), saveLoadUI.Close, new Vector2(240f, 46f));
            Set(saveLoadUI, "backButton", slBack);
            SetArray(saveLoadUI, "slotButtons", slotButtons);
            SetArray(saveLoadUI, "slotInfoTexts", slotInfo);
            saveLoadPanel.gameObject.SetActive(false);
            Set(menu, "saveLoad", saveLoadUI);

            // 3. Settings Menu Modal
            var settingsPanel = Image("Settings Panel", canvasRoot, Vector2.zero, new Vector2(980f, 740f), SubmenuPanelColor);
            var settingsUI = settingsPanel.gameObject.AddComponent<SettingsMenuUI>();
            Set(settingsUI, "rootPanel", settingsPanel.gameObject);
            var setHeader = Label("SETTINGS", settingsPanel.transform, new Vector2(0f, 320f), new Vector2(850f, 45f), 28, TextAnchor.MiddleCenter);
            setHeader.font = fontCinzel ?? setHeader.font;
            setHeader.color = ColorSubtitle;

            var tabDisplay = ModalButton("Display", settingsPanel.transform, new Vector2(-340f, 260f), null, new Vector2(160f, 42f));
            var tabAudio = ModalButton("Audio", settingsPanel.transform, new Vector2(-170f, 260f), null, new Vector2(160f, 42f));
            var tabGameplay = ModalButton("Gameplay", settingsPanel.transform, new Vector2(0f, 260f), null, new Vector2(160f, 42f));
            var tabAccess = ModalButton("Accessibility", settingsPanel.transform, new Vector2(170f, 260f), null, new Vector2(160f, 42f));
            var tabControls = ModalButton("Controls", settingsPanel.transform, new Vector2(340f, 260f), null, new Vector2(160f, 42f));

            Set(settingsUI, "displayTabButton", tabDisplay);
            Set(settingsUI, "audioTabButton", tabAudio);
            Set(settingsUI, "gameplayTabButton", tabGameplay);
            Set(settingsUI, "accessibilityTabButton", tabAccess);
            Set(settingsUI, "controlsTabButton", tabControls);

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

            // Populate Setting Tabs
            var tFullscreen = Toggle("Fullscreen", pDisplay.transform, new Vector2(0f, 160f), null, true);
            var sRenderScale = Slider("Render Scale (0.5x - 1.5x)", pDisplay.transform, new Vector2(0f, 90f), null, 0.5f, 1.5f, 1.0f);
            var tVsync = Toggle("V-Sync", pDisplay.transform, new Vector2(0f, 20f), null, true);
            Set(settingsUI, "fullscreenToggle", tFullscreen);
            Set(settingsUI, "renderScaleSlider", sRenderScale);
            Set(settingsUI, "vsyncToggle", tVsync);

            var sMaster = Slider("Master Volume", pAudio.transform, new Vector2(0f, 140f), null, 0f, 1f, 1f);
            var sAmbience = Slider("Ambience Volume", pAudio.transform, new Vector2(0f, 60f), null, 0f, 1f, 0.85f);
            var sSfx = Slider("SFX Volume", pAudio.transform, new Vector2(0f, -20f), null, 0f, 1f, 0.90f);
            var sDialogue = Slider("Dialogue Volume", pAudio.transform, new Vector2(0f, -100f), null, 0f, 1f, 1.0f);
            Set(settingsUI, "masterVolumeSlider", sMaster);
            Set(settingsUI, "ambienceVolumeSlider", sAmbience);
            Set(settingsUI, "sfxVolumeSlider", sSfx);
            Set(settingsUI, "dialogueVolumeSlider", sDialogue);

            var sSens = Slider("Look Sensitivity", pGameplay.transform, new Vector2(0f, 120f), null, 0.02f, 0.3f, 0.08f);
            var tInvert = Toggle("Invert Y-Axis", pGameplay.transform, new Vector2(0f, 40f), null, false);
            var tHeadBob = Toggle("Head Bob & Camera Breathing", pGameplay.transform, new Vector2(0f, -40f), null, true);
            Set(settingsUI, "sensitivitySlider", sSens);
            Set(settingsUI, "invertYToggle", tInvert);
            Set(settingsUI, "headBobToggle", tHeadBob);

            var tSubtitles = Toggle("Display Subtitles", pAccess.transform, new Vector2(0f, 140f), null, true);
            var tLargeText = Toggle("Large Subtitle Typography", pAccess.transform, new Vector2(0f, 60f), null, false);
            var tHighContrast = Toggle("High Contrast Dialogue Box", pAccess.transform, new Vector2(0f, -20f), null, false);
            Set(settingsUI, "subtitlesToggle", tSubtitles);
            Set(settingsUI, "largeTextToggle", tLargeText);
            Set(settingsUI, "highContrastToggle", tHighContrast);

            Label("W A S D — Movement\nMouse — Look / Aim\nE / Left Click — Interact & Examine\nShift — Sprint\nC / Ctrl — Crouch\nF — Lantern\nTab — Nautical Journal\nEsc — Pause Menu",
                pControls.transform, new Vector2(0f, 20f), new Vector2(700f, 260f), 17, TextAnchor.MiddleCenter).color = ColorTitle;

            var setBack = ModalButton("BACK", settingsPanel.transform, new Vector2(0f, -300f), settingsUI.Close, new Vector2(240f, 46f));
            Set(settingsUI, "backButton", setBack);
            settingsPanel.gameObject.SetActive(false);
            Set(menu, "settingsMenu", settingsUI);

            // 4. Extras Modal
            var extrasPanel = Image("Extras Panel", canvasRoot, Vector2.zero, new Vector2(760f, 540f), SubmenuPanelColor);
            var exTitle = Label("EXTRAS", extrasPanel.transform, new Vector2(0f, 210f), new Vector2(650f, 50f), 26, TextAnchor.MiddleCenter);
            exTitle.font = fontCinzel ?? exTitle.font;
            exTitle.color = ColorSubtitle;
            Label("1952 Harrow Rock Logbook Recordings\nHistorical Nautical Cartography Archive\nConcept Art & Audio Atmosphere Suite", extrasPanel.transform, new Vector2(0f, 40f), new Vector2(650f, 220f), 18, TextAnchor.MiddleCenter).color = ColorTitle;
            ModalButton("BACK", extrasPanel.transform, new Vector2(0f, -210f), menu.CloseExtras, new Vector2(220f, 46f));
            extrasPanel.gameObject.SetActive(false);
            Set(menu, "extrasPanel", extrasPanel.gameObject);

            // 5. Credits Modal
            var creditsPanel = Image("Credits Panel", canvasRoot, Vector2.zero, new Vector2(760f, 540f), SubmenuPanelColor);
            var crTitle = Label("CREDITS", creditsPanel.transform, new Vector2(0f, 210f), new Vector2(650f, 50f), 26, TextAnchor.MiddleCenter);
            crTitle.font = fontCinzel ?? crTitle.font;
            crTitle.color = ColorSubtitle;
            Label("THE LIGHTHOUSE — HARROW ROCK\n\nDirected & Developed with Google DeepMind Advanced Agentic Coding\nAudio & Visuals: Historical Maritime Records & Photogrammetry Library\nEngine: Unity 6 (Universal Render Pipeline)\nBackend: Java 21, Spring Boot 3.4.5, PostgreSQL", creditsPanel.transform, new Vector2(0f, 30f), new Vector2(680f, 260f), 16, TextAnchor.MiddleCenter).color = ColorTitle;
            ModalButton("BACK", creditsPanel.transform, new Vector2(0f, -210f), menu.CloseCredits, new Vector2(220f, 46f));
            creditsPanel.gameObject.SetActive(false);
            Set(menu, "creditsPanel", creditsPanel.gameObject);

            // 6. Confirm New Game Modal
            var confirmModal = Image("New Game Confirm Modal", canvasRoot, Vector2.zero, new Vector2(640f, 320f), SubmenuPanelColor);
            var confTitle = Label("START NEW GAME?", confirmModal.transform, new Vector2(0f, 90f), new Vector2(580f, 45f), 22, TextAnchor.MiddleCenter);
            confTitle.font = fontCinzel ?? confTitle.font;
            confTitle.color = ColorSubtitle;
            Label("Starting a new game will overwrite your active save file.\nAre you prepared to face Harrow Rock?", confirmModal.transform, new Vector2(0f, 20f), new Vector2(580f, 60f), 15, TextAnchor.MiddleCenter).color = ColorTitle;
            ModalButton("CANCEL", confirmModal.transform, new Vector2(-130f, -80f), menu.CancelNewGame, new Vector2(200f, 46f));
            ModalButton("PROCEED", confirmModal.transform, new Vector2(130f, -80f), menu.ConfirmNewGame, new Vector2(200f, 46f));
            confirmModal.gameObject.SetActive(false);
            Set(menu, "confirmNewGameModal", confirmModal.gameObject);
        }

        private static Button ModalButton(string label, Transform parent, Vector2 position, UnityEngine.Events.UnityAction action, Vector2 size)
        {
            Image image = Image(label, parent, position, size, ControlColor);
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.ColorTint;
            button.colors = new ColorBlock
            {
                normalColor = ControlColor,
                highlightedColor = HoverColor,
                pressedColor = ColorSubtitle,
                selectedColor = HoverColor,
                disabledColor = new Color(0.04f, 0.05f, 0.06f, 0.45f),
                colorMultiplier = 1f,
                fadeDuration = 0.1f
            };
            if (action != null) UnityEventTools.AddPersistentListener(button.onClick, action);
            Text text = Label(label, image.transform, Vector2.zero, size, 16, TextAnchor.MiddleCenter);
            text.font = fontCinzel ?? text.font;
            text.color = ColorTitle;
            text.raycastTarget = false;
            return button;
        }

        private static Toggle Toggle(string label, Transform parent, Vector2 position, UnityEngine.Events.UnityAction<bool> action, bool initial)
        {
            var root = RectObject(label, parent, position, new Vector2(440f, 38f));
            Image box = Image("Box", root.transform, new Vector2(-195f, 0f), new Vector2(26f, 26f), ControlColor);
            Image check = Image("Checkmark", box.transform, Vector2.zero, new Vector2(16f, 16f), ColorSubtitle);
            Text text = Label(label, root.transform, new Vector2(18f, 0f), new Vector2(380f, 36f), 15, TextAnchor.MiddleLeft);
            text.font = fontCinzel ?? text.font;
            text.color = ColorTextNormal;
            text.raycastTarget = false;
            var toggle = root.AddComponent<Toggle>();
            toggle.targetGraphic = box;
            toggle.graphic = check;
            toggle.isOn = initial;
            if (action != null) UnityEventTools.AddPersistentListener(toggle.onValueChanged, action);
            return toggle;
        }

        private static Slider Slider(string label, Transform parent, Vector2 position, UnityEngine.Events.UnityAction<float> action, float low, float high, float initial)
        {
            var root = RectObject(label, parent, position, new Vector2(440f, 54f));
            Text text = Label(label, root.transform, new Vector2(0f, 16f), new Vector2(440f, 22f), 14, TextAnchor.MiddleLeft);
            text.font = fontCinzel ?? text.font;
            text.color = ColorTextNormal;
            text.raycastTarget = false;

            Image track = Image("Track", root.transform, new Vector2(0f, -10f), new Vector2(440f, 10f), ControlColor);
            var fillArea = RectObject("Fill Area", track.transform, Vector2.zero, Vector2.zero);
            Stretch(fillArea.GetComponent<RectTransform>());
            fillArea.GetComponent<RectTransform>().offsetMin = new Vector2(4f, 2f);
            fillArea.GetComponent<RectTransform>().offsetMax = new Vector2(-4f, -2f);
            Image fill = Image("Fill", fillArea.transform, Vector2.zero, Vector2.zero, ColorSubtitle);
            Stretch(fill.rectTransform);

            var handleArea = RectObject("Handle Slide Area", track.transform, Vector2.zero, Vector2.zero);
            Stretch(handleArea.GetComponent<RectTransform>());
            handleArea.GetComponent<RectTransform>().offsetMin = new Vector2(8f, -6f);
            handleArea.GetComponent<RectTransform>().offsetMax = new Vector2(-8f, 6f);
            Image handle = Image("Handle", handleArea.transform, Vector2.zero, new Vector2(16f, 24f), ColorTitle);

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

        private static GameObject RectObject(string name, Transform parent, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return go;
        }

        private static Image Image(string name, Transform parent, Vector2 position, Vector2 size, Color color)
        {
            var go = RectObject(name, parent, position, size);
            go.AddComponent<CanvasRenderer>();
            var image = go.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private static Text Label(string value, Transform parent, Vector2 position, Vector2 size, int fontSize, TextAnchor alignment)
        {
            var go = RectObject(value + " Text", parent, position, size);
            go.AddComponent<CanvasRenderer>();
            var text = go.AddComponent<Text>();
            text.text = value;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = ColorTitle;
            return text;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
        }

        private static void SetAnchorLeft(RectTransform rect)
        {
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
        }

        private static void Set(UnityEngine.Object target, string name, UnityEngine.Object value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(name);
            if (prop != null)
            {
                prop.objectReferenceValue = value;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void Set(UnityEngine.Object target, string name, float value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(name);
            if (prop != null)
            {
                prop.floatValue = value;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void Set(UnityEngine.Object target, string name, bool value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(name);
            if (prop != null)
            {
                prop.boolValue = value;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void SetArray<T>(UnityEngine.Object target, string name, T[] array) where T : UnityEngine.Object
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
    }
}
