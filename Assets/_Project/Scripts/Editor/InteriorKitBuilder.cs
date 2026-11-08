using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace TheLightHouse.Editor
{
    public static class InteriorKitBuilder
    {
        public const string PrefabDir = "Assets/_Project/Prefabs/Interior";
        public const string SettingsPath = "Assets/_Project/Settings/InteriorSettings.asset";
        public const string MatDir = "Assets/_Project/Materials/Interior";

        [MenuItem("TheLightHouse/Build Modular Interior Kit")]
        public static void BuildKit()
        {
            Directory.CreateDirectory(PrefabDir);
            Directory.CreateDirectory(MatDir);
            AssetDatabase.Refresh();

            var settings = EnsureSettings();
            BuildMaterials(settings);
            BuildPrefabs(settings);

            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[InteriorKitBuilder] Modular interior kit prefabs and materials successfully built in " + PrefabDir);
        }

        public static InteriorSettings EnsureSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<InteriorSettings>(SettingsPath);
            if (!settings)
            {
                settings = ScriptableObject.CreateInstance<InteriorSettings>();
                AssetDatabase.CreateAsset(settings, SettingsPath);
            }

            settings.stepsPerFlight = 9;
            settings.stepRise = 0.178f;
            settings.stepRun = 0.28f;
            settings.stairWidth = 1.2f;
            settings.towerShaftSize = 5.0f;
            settings.floorHeight = 3.2f;
            settings.minHeadroom = 2.4f;
            EditorUtility.SetDirty(settings);
            return settings;
        }

        public static void BuildMaterials(InteriorSettings settings)
        {
            Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (!litShader) litShader = Shader.Find("Standard");

            // 1. StoneWall (LighthouseStone)
            settings.stoneWallMaterial = CreatePbrMaterial("StoneWall",
                "Assets/_Project/Art/Textures/PBR/LighthouseStone/LighthouseStone_diff.jpg",
                "Assets/_Project/Art/Textures/PBR/LighthouseStone/LighthouseStone_nor.jpg",
                "Assets/_Project/Art/Textures/PBR/LighthouseStone/LighthouseStone_rough.jpg",
                "Assets/_Project/Art/Textures/PBR/LighthouseStone/LighthouseStone_ao.jpg",
                0.0f, new Color(0.65f, 0.65f, 0.65f));

            // 2. Plaster (WeatheredPlaster)
            settings.plasterMaterial = CreatePbrMaterial("Plaster",
                "Assets/_Project/Art/Textures/PBR/WeatheredPlaster/WeatheredPlaster_diff.jpg",
                "Assets/_Project/Art/Textures/PBR/WeatheredPlaster/WeatheredPlaster_nor.jpg",
                "Assets/_Project/Art/Textures/PBR/WeatheredPlaster/WeatheredPlaster_rough.jpg",
                "Assets/_Project/Art/Textures/PBR/WeatheredPlaster/WeatheredPlaster_ao.jpg",
                0.0f, new Color(0.82f, 0.82f, 0.78f));

            // 3. Wood (WeatheredPlanks)
            settings.woodMaterial = CreatePbrMaterial("Wood",
                "Assets/_Project/Art/Textures/PBR/WeatheredPlanks/WeatheredPlanks_diff.jpg",
                "Assets/_Project/Art/Textures/PBR/WeatheredPlanks/WeatheredPlanks_nor.jpg",
                "Assets/_Project/Art/Textures/PBR/WeatheredPlanks/WeatheredPlanks_rough.jpg",
                "Assets/_Project/Art/Textures/PBR/WeatheredPlanks/WeatheredPlanks_ao.jpg",
                0.0f, new Color(0.55f, 0.45f, 0.35f));

            // 4. RustMetal (RustedIron)
            settings.rustMetalMaterial = CreatePbrMaterial("RustMetal",
                "Assets/_Project/Art/Textures/PBR/RustedIron/RustedIron_diff.jpg",
                "Assets/_Project/Art/Textures/PBR/RustedIron/RustedIron_nor.jpg",
                "Assets/_Project/Art/Textures/PBR/RustedIron/RustedIron_rough.jpg",
                "Assets/_Project/Art/Textures/PBR/RustedIron/RustedIron_ao.jpg",
                0.85f, new Color(0.48f, 0.44f, 0.42f));

            // 5. WetRock (CoastalRock)
            settings.wetRockMaterial = CreatePbrMaterial("WetRock",
                "Assets/_Project/Art/Textures/PBR/CoastalRock/CoastalRock_diff.jpg",
                "Assets/_Project/Art/Textures/PBR/CoastalRock/CoastalRock_nor.jpg",
                "Assets/_Project/Art/Textures/PBR/CoastalRock/CoastalRock_rough.jpg",
                "Assets/_Project/Art/Textures/PBR/CoastalRock/CoastalRock_ao.jpg",
                0.0f, new Color(0.32f, 0.35f, 0.36f));

            // Missing folders: Flagstone, Sand, Paper -> create placeholder materials and list them explicitly
            settings.flagstoneMaterial = CreatePlaceholderMaterial("Flagstone", new Color(0.28f, 0.28f, 0.27f), 0.25f, 0.0f);
            settings.sandMaterial = CreatePlaceholderMaterial("Sand", new Color(0.42f, 0.38f, 0.30f), 0.15f, 0.0f);
            settings.paperMaterial = CreatePlaceholderMaterial("Paper", new Color(0.90f, 0.88f, 0.82f), 0.10f, 0.0f);

            EnsureSoftParticleAssets();

            Debug.Log("[InteriorKitBuilder] Missing texture folders handled with flat placeholder materials: 'Flagstone', 'Sand', 'Paper'.");
        }

        private static Material CreatePbrMaterial(string name, string diffPath, string norPath, string roughPath, string aoPath, float metallicVal, Color tint)
        {
            string matPath = $"{MatDir}/{name}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (!mat)
            {
                mat = new Material(litShader) { name = name };
                AssetDatabase.CreateAsset(mat, matPath);
            }
            else
            {
                mat.shader = litShader;
            }

            ConfigureTextureImporter(diffPath, true, false);
            ConfigureTextureImporter(norPath, false, true);
            ConfigureTextureImporter(roughPath, false, false);
            ConfigureTextureImporter(aoPath, false, false);

            var diff = AssetDatabase.LoadAssetAtPath<Texture2D>(diffPath);
            var nor = AssetDatabase.LoadAssetAtPath<Texture2D>(norPath);
            var rough = AssetDatabase.LoadAssetAtPath<Texture2D>(roughPath);
            var ao = AssetDatabase.LoadAssetAtPath<Texture2D>(aoPath);

            mat.SetColor("_BaseColor", tint);
            if (diff) mat.SetTexture("_BaseColorMap", diff);
            if (nor)
            {
                mat.EnableKeyword("_NORMALMAP");
                mat.SetTexture("_BumpMap", nor);
                mat.SetFloat("_BumpScale", 1.0f);
            }

            // Pack Metallic-Smoothness map from roughness if present
            string packedPath = $"{MatDir}/{name}_metallic_smoothness.png";
            Texture2D packedMap = PackMetallicSmoothness(packedPath, rough, ao, metallicVal);
            if (packedMap)
            {
                mat.EnableKeyword("_METALLICSPECGLOSSMAP");
                mat.SetTexture("_MetallicGlossMap", packedMap);
                mat.SetFloat("_Smoothness", 1.0f);
                mat.SetFloat("_Metallic", 1.0f);
            }
            else
            {
                mat.SetFloat("_Smoothness", 0.4f);
                mat.SetFloat("_Metallic", metallicVal);
            }

            if (ao)
            {
                mat.EnableKeyword("_OCCLUSIONMAP");
                mat.SetTexture("_OcclusionMap", ao);
                mat.SetFloat("_OcclusionStrength", 1.0f);
            }

            mat.SetFloat("_Surface", 0.0f); // Opaque
            mat.renderQueue = (int)RenderQueue.Geometry;
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static Material CreatePlaceholderMaterial(string name, Color color, float smoothness, float metallic)
        {
            string matPath = $"{MatDir}/{name}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (!mat)
            {
                mat = new Material(litShader) { name = name };
                AssetDatabase.CreateAsset(mat, matPath);
            }
            else
            {
                mat.shader = litShader;
            }
            mat.SetColor("_BaseColor", color);
            mat.SetFloat("_Smoothness", smoothness);
            mat.SetFloat("_Metallic", metallic);
            mat.SetFloat("_Surface", 0.0f);
            mat.renderQueue = (int)RenderQueue.Geometry;
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static void ConfigureTextureImporter(string path, bool isSrgb, bool isNormal)
        {
            if (string.IsNullOrEmpty(path)) return;
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (!importer) return;

            bool changed = false;
            if (isNormal)
            {
                if (importer.textureType != TextureImporterType.NormalMap)
                {
                    importer.textureType = TextureImporterType.NormalMap;
                    changed = true;
                }
            }
            else
            {
                if (importer.textureType != TextureImporterType.Default)
                {
                    importer.textureType = TextureImporterType.Default;
                    changed = true;
                }
                if (importer.sRGBTexture != isSrgb)
                {
                    importer.sRGBTexture = isSrgb;
                    changed = true;
                }
            }

            if (importer.maxTextureSize > 1024)
            {
                importer.maxTextureSize = 1024;
                changed = true;
            }

            if (!importer.mipmapEnabled)
            {
                importer.mipmapEnabled = true;
                changed = true;
            }

            if (importer.anisoLevel < 4)
            {
                importer.anisoLevel = 4;
                changed = true;
            }

            if (changed)
            {
                importer.SaveAndReimport();
            }
        }

        public static void EnsureSoftParticleAssets()
        {
            string dir = "Assets/_Project/Art/Textures/Particles";
            Directory.CreateDirectory(dir);
            string texPath = $"{dir}/soft_particle_smoke.png";
            if (!File.Exists(texPath))
            {
                int size = 128;
                var tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
                float center = (size - 1) * 0.5f;
                float radius = center;
                Color[] pixels = new Color[size * size];
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float dx = (x - center) / radius;
                        float dy = (y - center) / radius;
                        float dist = Mathf.Sqrt(dx * dx + dy * dy);
                        float alpha = Mathf.Clamp01(1f - dist);
                        alpha = Mathf.SmoothStep(0f, 1f, alpha);
                        alpha = Mathf.Pow(alpha, 1.8f);
                        pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                    }
                }
                tex.SetPixels(pixels);
                tex.Apply(true);
                File.WriteAllBytes(texPath, tex.EncodeToPNG());
                Object.DestroyImmediate(tex);
                AssetDatabase.ImportAsset(texPath, ImportAssetOptions.ForceUpdate);
                ConfigureTextureImporter(texPath, false, false);
            }

            string matPath = "Assets/_Project/Materials/Level/Particle.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat)
            {
                var softTex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
                if (softTex)
                {
                    mat.SetTexture("_BaseMap", softTex);
                    mat.mainTexture = softTex;
                }
                mat.SetColor("_BaseColor", new Color(0.75f, 0.78f, 0.80f, 0.12f));
                EditorUtility.SetDirty(mat);
            }
        }

        private static Texture2D PackMetallicSmoothness(string targetPath, Texture2D roughMap, Texture2D aoMap, float metallic)
        {
            if (!roughMap) return null;
            if (File.Exists(targetPath))
            {
                return AssetDatabase.LoadAssetAtPath<Texture2D>(targetPath);
            }

            // Read roughness
            string roughAssetPath = AssetDatabase.GetAssetPath(roughMap);
            var importer = AssetImporter.GetAtPath(roughAssetPath) as TextureImporter;
            bool wasReadable = importer && importer.isReadable;
            if (importer && !importer.isReadable)
            {
                importer.isReadable = true;
                importer.SaveAndReimport();
            }

            int w = Mathf.Min(roughMap.width, 1024);
            int h = Mathf.Min(roughMap.height, 1024);
            var packed = new Texture2D(w, h, TextureFormat.RGBA32, true, true);
            var roughPixels = roughMap.GetPixels(0);

            Color[] aoPixels = null;
            if (aoMap)
            {
                string aoAssetPath = AssetDatabase.GetAssetPath(aoMap);
                var aoImporter = AssetImporter.GetAtPath(aoAssetPath) as TextureImporter;
                if (aoImporter && !aoImporter.isReadable)
                {
                    aoImporter.isReadable = true;
                    aoImporter.SaveAndReimport();
                }
                if (aoMap.width == roughMap.width && aoMap.height == roughMap.height)
                {
                    aoPixels = aoMap.GetPixels(0);
                }
            }

            var packedPixels = new Color[roughPixels.Length];
            byte metByte = (byte)(Mathf.Clamp01(metallic) * 255f);

            for (int i = 0; i < roughPixels.Length; i++)
            {
                float r = roughPixels[i].r;
                float smoothness = Mathf.Clamp01(1.0f - r);
                float ao = aoPixels != null ? aoPixels[i].r : 1.0f;
                packedPixels[i] = new Color(metallic, ao, 0.0f, smoothness);
            }

            packed.SetPixels(packedPixels);
            packed.Apply(true);

            byte[] png = packed.EncodeToPNG();
            File.WriteAllBytes(targetPath, png);
            Object.DestroyImmediate(packed);

            AssetDatabase.ImportAsset(targetPath, ImportAssetOptions.ForceUpdate);
            ConfigureTextureImporter(targetPath, false, false);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(targetPath);
        }

        public static void BuildPrefabs(InteriorSettings settings)
        {
            // 1. Wall Prefab (2.0m wide, 3.2m high, 0.25m thick, with modeled bevel trim)
            settings.wallPrefab = BuildWallPrefab(settings);

            // 2. Floor Slab Prefab (with stairwell hole)
            settings.floorSlabPrefab = BuildFloorSlabPrefab(settings);

            // 3. Door Frame Prefab (with working Door component)
            settings.doorFramePrefab = BuildDoorFramePrefab(settings);

            // 4. Straight Stair Flight Prefab (18 steps, rise 0.178, run 0.28, width 1.2, ramp collider)
            settings.stairFlightPrefab = BuildStairFlightPrefab(settings);

            // 5. Landing Prefab (1.6 x 1.6, flush collider)
            settings.landingPrefab = BuildLandingPrefab(settings);

            // 6. Handrail Prefab (sloped and horizontal)
            settings.handrailPrefab = BuildHandrailPrefab(settings);

            // 7. Window Frame Prefab (with stone reveal)
            settings.windowFramePrefab = BuildWindowFramePrefab(settings);

            // 8. Vault Segment Prefab (2.8m wide, 2.6m high, box with half-cylinder ceiling)
            settings.vaultSegmentPrefab = BuildVaultSegmentPrefab(settings);
        }

        private static GameObject BuildWallPrefab(InteriorSettings settings)
        {
            string path = $"{PrefabDir}/Interior_Wall.prefab";
            var root = new GameObject("Interior_Wall");
            root.AddComponent<InteriorKit>().Configure(InteriorKit.PieceType.Wall, settings);

            // Main wall body: 2.0m x 3.2m x 0.25m
            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "WallBody";
            body.transform.SetParent(root.transform, false);
            body.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            body.transform.localScale = new Vector3(2.0f, 3.2f, 0.25f);
            body.GetComponent<Renderer>().sharedMaterial = settings.plasterMaterial;

            // Modeled baseboard / stone plinth: 2.0m x 0.24m x 0.32m (B3 bevel/chunkier edges)
            var plinth = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plinth.name = "StonePlinth";
            plinth.transform.SetParent(root.transform, false);
            plinth.transform.localPosition = new Vector3(0f, 0.12f, 0f);
            plinth.transform.localScale = new Vector3(2.0f, 0.24f, 0.32f);
            plinth.GetComponent<Renderer>().sharedMaterial = settings.stoneWallMaterial;
            var plinthCol = plinth.GetComponent<Collider>();
            if (plinthCol) Object.DestroyImmediate(plinthCol);

            // Modeled top cornice: 2.0m x 0.16m x 0.32m
            var cornice = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cornice.name = "TopCornice";
            cornice.transform.SetParent(root.transform, false);
            cornice.transform.localPosition = new Vector3(0f, 3.12f, 0f);
            cornice.transform.localScale = new Vector3(2.0f, 0.16f, 0.32f);
            cornice.GetComponent<Renderer>().sharedMaterial = settings.stoneWallMaterial;
            var corniceCol = cornice.GetComponent<Collider>();
            if (corniceCol) Object.DestroyImmediate(corniceCol);

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject BuildFloorSlabPrefab(InteriorSettings settings)
        {
            string path = $"{PrefabDir}/Interior_FloorSlab.prefab";
            var root = new GameObject("Interior_FloorSlab");
            root.AddComponent<InteriorKit>().Configure(InteriorKit.PieceType.FloorSlab, settings);

            // 5.0m x 5.0m Square Shaft floor slab with switchback stairwell slots
            // South landing slab (where each floor arrives from Flight B and departs to Flight A):
            var south = GameObject.CreatePrimitive(PrimitiveType.Cube);
            south.name = "Slab_SouthLanding";
            south.transform.SetParent(root.transform, false);
            south.transform.localPosition = new Vector3(0f, -0.125f, -1.88f);
            south.transform.localScale = new Vector3(5.0f, 0.25f, 1.24f);
            south.GetComponent<Renderer>().sharedMaterial = settings.woodMaterial;

            // Central well spine / divider:
            var spine = GameObject.CreatePrimitive(PrimitiveType.Cube);
            spine.name = "Slab_CenterDivider";
            spine.transform.SetParent(root.transform, false);
            spine.transform.localPosition = new Vector3(0f, -0.125f, 0f);
            spine.transform.localScale = new Vector3(0.2f, 0.25f, 2.52f);
            spine.GetComponent<Renderer>().sharedMaterial = settings.woodMaterial;

            // North [1.26, 2.50] remains OPEN on Floor Slabs so that North Turn Landings at mid-height (y = 1.602, 4.806, etc.) have full 3.2m headroom!

            // East slot [1.3, 2.5] and West slot [-2.5, -1.3] between Z in [-1.26, 1.26] remain open for stairs!

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject BuildDoorFramePrefab(InteriorSettings settings)
        {
            string path = $"{PrefabDir}/Interior_DoorFrame.prefab";
            var root = new GameObject("Interior_DoorFrame");
            root.AddComponent<InteriorKit>().Configure(InteriorKit.PieceType.DoorFrame, settings);

            float w = 1.2f;
            float h = 2.4f;
            float d = 0.35f;
            float jambWidth = 0.12f;

            // Left Jamb
            var leftJamb = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftJamb.name = "LeftJamb";
            leftJamb.transform.SetParent(root.transform, false);
            leftJamb.transform.localPosition = new Vector3(-w * 0.5f + jambWidth * 0.5f, h * 0.5f, 0f);
            leftJamb.transform.localScale = new Vector3(jambWidth, h, d);
            leftJamb.GetComponent<Renderer>().sharedMaterial = settings.stoneWallMaterial;

            // Right Jamb
            var rightJamb = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightJamb.name = "RightJamb";
            rightJamb.transform.SetParent(root.transform, false);
            rightJamb.transform.localPosition = new Vector3(w * 0.5f - jambWidth * 0.5f, h * 0.5f, 0f);
            rightJamb.transform.localScale = new Vector3(jambWidth, h, d);
            rightJamb.GetComponent<Renderer>().sharedMaterial = settings.stoneWallMaterial;

            // Lintel
            var lintel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lintel.name = "Lintel";
            lintel.transform.SetParent(root.transform, false);
            lintel.transform.localPosition = new Vector3(0f, h - 0.1f, 0f);
            lintel.transform.localScale = new Vector3(w, 0.2f, d + 0.05f);
            lintel.GetComponent<Renderer>().sharedMaterial = settings.stoneWallMaterial;

            // Threshold plate (flush with floor, eliminates gap)
            var threshold = GameObject.CreatePrimitive(PrimitiveType.Cube);
            threshold.name = "Threshold";
            threshold.transform.SetParent(root.transform, false);
            threshold.transform.localPosition = new Vector3(0f, 0.02f, 0f);
            threshold.transform.localScale = new Vector3(w, 0.04f, d);
            threshold.GetComponent<Renderer>().sharedMaterial = settings.rustMetalMaterial;

            // Working Door Leaf (hinged)
            var doorPivot = new GameObject("DoorPivot");
            doorPivot.transform.SetParent(root.transform, false);
            doorPivot.transform.localPosition = new Vector3(-w * 0.5f + jambWidth, 0f, 0f);
            var pivotMod = doorPivot.AddComponent<Unity.AI.Navigation.NavMeshModifier>();
            pivotMod.ignoreFromBuild = true;

            var doorPanel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            doorPanel.name = "DoorPanel";
            doorPanel.transform.SetParent(doorPivot.transform, false);
            float leafWidth = w - jambWidth * 2f;
            float leafHeight = h - 0.2f;
            doorPanel.transform.localPosition = new Vector3(leafWidth * 0.5f, leafHeight * 0.5f, 0f);
            doorPanel.transform.localScale = new Vector3(leafWidth, leafHeight, 0.06f);
            doorPanel.GetComponent<Renderer>().sharedMaterial = settings.woodMaterial;
            var panelMod = doorPanel.AddComponent<Unity.AI.Navigation.NavMeshModifier>();
            panelMod.ignoreFromBuild = true;

            // Hardware: Iron strap hinges
            for (int i = 0; i < 2; i++)
            {
                float yHinge = i == 0 ? 0.45f : leafHeight - 0.45f;
                var hinge = GameObject.CreatePrimitive(PrimitiveType.Cube);
                hinge.name = $"Hinge_{i}";
                hinge.transform.SetParent(doorPanel.transform, false);
                hinge.transform.localPosition = new Vector3(-leafWidth * 0.5f + 0.18f, yHinge - leafHeight * 0.5f, 0.035f);
                hinge.transform.localScale = new Vector3(0.36f, 0.08f, 0.02f);
                hinge.GetComponent<Renderer>().sharedMaterial = settings.rustMetalMaterial;
                var col = hinge.GetComponent<Collider>();
                if (col) Object.DestroyImmediate(col);
            }

            // Door handle
            var handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            handle.name = "Handle";
            handle.transform.SetParent(doorPanel.transform, false);
            handle.transform.localPosition = new Vector3(leafWidth * 0.5f - 0.12f, 1.0f - leafHeight * 0.5f, 0.04f);
            handle.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            handle.transform.localScale = new Vector3(0.04f, 0.06f, 0.04f);
            handle.GetComponent<Renderer>().sharedMaterial = settings.rustMetalMaterial;
            var hCol = handle.GetComponent<Collider>();
            if (hCol) Object.DestroyImmediate(hCol);

            // Add Door component and NoiseEmitter
            doorPivot.AddComponent<NoiseEmitter>();
            var doorComp = doorPivot.AddComponent<Door>();
            var doorSo = new SerializedObject(doorComp);
            doorSo.FindProperty("doorId").stringValue = "interior-door";
            doorSo.FindProperty("openAngle").floatValue = 90f;
            doorSo.FindProperty("turnSpeed").floatValue = 120f;
            doorSo.FindProperty("loudness").floatValue = 5f;
            doorSo.ApplyModifiedPropertiesWithoutUndo();

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject BuildStairFlightPrefab(InteriorSettings settings)
        {
            string path = $"{PrefabDir}/Interior_StairFlight.prefab";
            var root = new GameObject("Interior_StairFlight");
            root.AddComponent<InteriorKit>().Configure(InteriorKit.PieceType.StairFlight, settings);

            int count = settings.stepsPerFlight; // 18
            float rise = settings.stepRise;      // 0.178
            float run = settings.stepRun;        // 0.28
            float width = settings.stairWidth;   // 1.2
            float totalRise = count * rise;      // 3.204
            float totalRun = count * run;        // 5.04

            var visualRoot = new GameObject("TreadsVisual");
            visualRoot.transform.SetParent(root.transform, false);

            // 18 modeled treads with bullnose nosing and NO individual colliders
            for (int i = 0; i < count; i++)
            {
                float y = (i + 0.5f) * rise;
                float z = (i + 0.5f) * run;

                var step = GameObject.CreatePrimitive(PrimitiveType.Cube);
                step.name = $"Step_{i:00}";
                step.transform.SetParent(visualRoot.transform, false);
                step.transform.localPosition = new Vector3(0f, y, z);
                step.transform.localScale = new Vector3(width, rise, run);
                step.GetComponent<Renderer>().sharedMaterial = settings.woodMaterial;

                // Modeled nosing overhang (B3)
                var nosing = GameObject.CreatePrimitive(PrimitiveType.Cube);
                nosing.name = "Nosing";
                nosing.transform.SetParent(step.transform, false);
                nosing.transform.localPosition = new Vector3(0f, 0.45f, -0.48f);
                nosing.transform.localScale = new Vector3(1.02f, 0.15f, 0.18f);
                nosing.GetComponent<Renderer>().sharedMaterial = settings.woodMaterial;

                // Strip individual colliders (A3 requirement: no per-step colliders!)
                var sc = step.GetComponent<Collider>();
                if (sc) Object.DestroyImmediate(sc);
                var nc = nosing.GetComponent<Collider>();
                if (nc) Object.DestroyImmediate(nc);
            }

            // Side stringers (structural timber beams flanking the treads)
            float diagLen = Mathf.Sqrt(totalRise * totalRise + totalRun * totalRun);
            float angleDeg = Mathf.Atan2(totalRise, totalRun) * Mathf.Rad2Deg; // ~32.44 degrees
            var rampRot = Quaternion.Euler(-angleDeg, 0f, 0f);
            var rampNormal = rampRot * Vector3.up;

            for (int side = -1; side <= 1; side += 2)
            {
                var stringer = GameObject.CreatePrimitive(PrimitiveType.Cube);
                stringer.name = side < 0 ? "StringerLeft" : "StringerRight";
                stringer.transform.SetParent(visualRoot.transform, false);
                stringer.transform.localPosition = new Vector3(side * (width * 0.5f + 0.04f), totalRise * 0.5f - 0.06f, totalRun * 0.5f);
                stringer.transform.localRotation = rampRot;
                stringer.transform.localScale = new Vector3(0.08f, 0.28f, diagLen + 0.35f);
                stringer.GetComponent<Renderer>().sharedMaterial = settings.woodMaterial;
                var strCol = stringer.GetComponent<Collider>();
                if (strCol) Object.DestroyImmediate(strCol);
            }

            // Continuous ramp collider (A3 requirement: one continuous ramp collider per flight, about 32 degrees)
            float rampThick = 0.18f;
            Vector3 rampOffset = -rampNormal * (rampThick * 0.5f);
            var rampObj = new GameObject("RampCollider");
            rampObj.transform.SetParent(root.transform, false);
            // Mathematically flush with lower landing at (0, 0, 0) and upper landing at (0, totalRise, totalRun)
            rampObj.transform.localPosition = new Vector3(0f, totalRise * 0.5f, totalRun * 0.5f) + rampOffset;
            rampObj.transform.localRotation = rampRot;
            var rampBox = rampObj.AddComponent<BoxCollider>();
            rampBox.size = new Vector3(width, rampThick, diagLen);

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject BuildLandingPrefab(InteriorSettings settings)
        {
            string path = $"{PrefabDir}/Interior_Landing.prefab";
            var root = new GameObject("Interior_Landing");
            root.AddComponent<InteriorKit>().Configure(InteriorKit.PieceType.Landing, settings);

            float sizeX = settings.landingDimensions.x; // 1.6m
            float sizeZ = settings.landingDimensions.y; // 1.6m
            float thick = 0.25f;

            var slab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            slab.name = "LandingSlab";
            slab.transform.SetParent(root.transform, false);
            slab.transform.localPosition = Vector3.zero;
            slab.transform.localScale = Vector3.one;
            slab.GetComponent<Renderer>().sharedMaterial = settings.woodMaterial;

            // Flush collider is already present on the cube
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject BuildHandrailPrefab(InteriorSettings settings)
        {
            string path = $"{PrefabDir}/Interior_Handrail.prefab";
            var root = new GameObject("Interior_Handrail");
            root.AddComponent<InteriorKit>().Configure(InteriorKit.PieceType.Handrail, settings);

            float totalRise = settings.stepsPerFlight * settings.stepRise;
            float totalRun = settings.stepsPerFlight * settings.stepRun;
            float diagLen = Mathf.Sqrt(totalRise * totalRise + totalRun * totalRun);
            float angleDeg = Mathf.Atan2(totalRise, totalRun) * Mathf.Rad2Deg;

            // Top rail (sloped at 32.44 degrees)
            var topRail = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            topRail.name = "TopRail";
            topRail.transform.SetParent(root.transform, false);
            topRail.transform.localPosition = new Vector3(0f, totalRise * 0.5f + 0.95f, totalRun * 0.5f);
            topRail.transform.localRotation = Quaternion.Euler(90f - angleDeg, 0f, 0f);
            topRail.transform.localScale = new Vector3(0.05f, (diagLen + 0.4f) * 0.5f, 0.05f);
            topRail.GetComponent<Renderer>().sharedMaterial = settings.rustMetalMaterial;
            var rc = topRail.GetComponent<Collider>();
            if (rc) Object.DestroyImmediate(rc);

            // Intermediate rail
            var midRail = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            midRail.name = "MidRail";
            midRail.transform.SetParent(root.transform, false);
            midRail.transform.localPosition = new Vector3(0f, totalRise * 0.5f + 0.45f, totalRun * 0.5f);
            midRail.transform.localRotation = Quaternion.Euler(90f - angleDeg, 0f, 0f);
            midRail.transform.localScale = new Vector3(0.04f, (diagLen + 0.4f) * 0.5f, 0.04f);
            midRail.GetComponent<Renderer>().sharedMaterial = settings.rustMetalMaterial;
            var mc = midRail.GetComponent<Collider>();
            if (mc) Object.DestroyImmediate(mc);

            // Vertical iron balusters
            int postCount = 6;
            for (int i = 0; i <= postCount; i++)
            {
                float t = i / (float)postCount;
                float y = t * totalRise;
                float z = t * totalRun;

                var post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                post.name = $"Baluster_{i}";
                post.transform.SetParent(root.transform, false);
                post.transform.localPosition = new Vector3(0f, y + 0.5f, z);
                post.transform.localScale = new Vector3(0.04f, 0.5f, 0.04f);
                post.GetComponent<Renderer>().sharedMaterial = settings.rustMetalMaterial;
                var pc = post.GetComponent<Collider>();
                if (pc) Object.DestroyImmediate(pc);
            }

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject BuildWindowFramePrefab(InteriorSettings settings)
        {
            string path = $"{PrefabDir}/Interior_WindowFrame.prefab";
            var root = new GameObject("Interior_WindowFrame");
            root.tag = "Window";
            root.AddComponent<InteriorKit>().Configure(InteriorKit.PieceType.WindowFrame, settings);

            float w = 1.0f;
            float h = 1.6f;
            float d = 0.35f;

            // Deep stone reveal surround (B3)
            // Left reveal
            var left = GameObject.CreatePrimitive(PrimitiveType.Cube);
            left.name = "RevealLeft";
            left.transform.SetParent(root.transform, false);
            left.transform.localPosition = new Vector3(-w * 0.5f - 0.06f, 0f, 0f);
            left.transform.localScale = new Vector3(0.12f, h + 0.2f, d);
            left.GetComponent<Renderer>().sharedMaterial = settings.stoneWallMaterial;

            // Right reveal
            var right = GameObject.CreatePrimitive(PrimitiveType.Cube);
            right.name = "RevealRight";
            right.transform.SetParent(root.transform, false);
            right.transform.localPosition = new Vector3(w * 0.5f + 0.06f, 0f, 0f);
            right.transform.localScale = new Vector3(0.12f, h + 0.2f, d);
            right.GetComponent<Renderer>().sharedMaterial = settings.stoneWallMaterial;

            // Top arch / lintel
            var top = GameObject.CreatePrimitive(PrimitiveType.Cube);
            top.name = "RevealTop";
            top.transform.SetParent(root.transform, false);
            top.transform.localPosition = new Vector3(0f, h * 0.5f + 0.06f, 0f);
            top.transform.localScale = new Vector3(w + 0.24f, 0.12f, d + 0.04f);
            top.GetComponent<Renderer>().sharedMaterial = settings.stoneWallMaterial;

            // Stone sill with drip groove
            var sill = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sill.name = "StoneSill";
            sill.transform.SetParent(root.transform, false);
            sill.transform.localPosition = new Vector3(0f, -h * 0.5f - 0.06f, 0.04f);
            sill.transform.localScale = new Vector3(w + 0.3f, 0.14f, d + 0.12f);
            sill.GetComponent<Renderer>().sharedMaterial = settings.stoneWallMaterial;

            // Iron mullion cross
            var vertMullion = GameObject.CreatePrimitive(PrimitiveType.Cube);
            vertMullion.name = "MullionV";
            vertMullion.transform.SetParent(root.transform, false);
            vertMullion.transform.localPosition = Vector3.zero;
            vertMullion.transform.localScale = new Vector3(0.04f, h, 0.05f);
            vertMullion.GetComponent<Renderer>().sharedMaterial = settings.rustMetalMaterial;
            var vc = vertMullion.GetComponent<Collider>();
            if (vc) Object.DestroyImmediate(vc);

            var horizMullion = GameObject.CreatePrimitive(PrimitiveType.Cube);
            horizMullion.name = "MullionH";
            horizMullion.transform.SetParent(root.transform, false);
            horizMullion.transform.localPosition = Vector3.zero;
            horizMullion.transform.localScale = new Vector3(w, 0.04f, 0.05f);
            horizMullion.GetComponent<Renderer>().sharedMaterial = settings.rustMetalMaterial;
            var hc = horizMullion.GetComponent<Collider>();
            if (hc) Object.DestroyImmediate(hc);

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject BuildVaultSegmentPrefab(InteriorSettings settings)
        {
            string path = $"{PrefabDir}/Interior_VaultSegment.prefab";
            var root = new GameObject("Interior_VaultSegment");
            root.AddComponent<InteriorKit>().Configure(InteriorKit.PieceType.VaultSegment, settings);

            float w = settings.vaultWidth;    // 2.8m internal width
            float h = settings.vaultHeight;   // 2.6m internal height
            float len = 2.0f;                 // 2.0m on 0.5m grid
            float thick = 0.28f;
            float vertWallH = 1.2f;

            // Floor slab
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "VaultFloor";
            floor.transform.SetParent(root.transform, false);
            floor.transform.localPosition = new Vector3(0f, -0.125f, 0f);
            floor.transform.localScale = new Vector3(w + thick * 2f, 0.25f, len);
            floor.GetComponent<Renderer>().sharedMaterial = settings.flagstoneMaterial;

            // Left vertical wall: x = -1.4m - thick/2
            var leftWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftWall.name = "VaultWallLeft";
            leftWall.transform.SetParent(root.transform, false);
            leftWall.transform.localPosition = new Vector3(-w * 0.5f - thick * 0.5f, vertWallH * 0.5f, 0f);
            leftWall.transform.localScale = new Vector3(thick, vertWallH, len);
            leftWall.GetComponent<Renderer>().sharedMaterial = settings.stoneWallMaterial;

            // Right vertical wall: x = +1.4m + thick/2
            var rightWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightWall.name = "VaultWallRight";
            rightWall.transform.SetParent(root.transform, false);
            rightWall.transform.localPosition = new Vector3(w * 0.5f + thick * 0.5f, vertWallH * 0.5f, 0f);
            rightWall.transform.localScale = new Vector3(thick, vertWallH, len);
            rightWall.GetComponent<Renderer>().sharedMaterial = settings.stoneWallMaterial;

            // Vault ceiling (half-cylinder arch, radius 1.4m from y=1.2 to 2.6m)
            // Modeled using 8 curved facets for smooth arch
            int facets = 8;
            float radius = w * 0.5f;
            float springY = vertWallH;

            for (int i = 0; i < facets; i++)
            {
                float a0 = (i / (float)facets) * Mathf.PI;
                float a1 = ((i + 1) / (float)facets) * Mathf.PI;
                float aMid = (a0 + a1) * 0.5f;

                float xMid = Mathf.Cos(aMid) * (radius + thick * 0.5f);
                float yMid = springY + Mathf.Sin(aMid) * (radius + thick * 0.5f);

                float facetChord = 2f * radius * Mathf.Sin(Mathf.PI / (facets * 2f)) + 0.05f;
                float angleDeg = (aMid * Mathf.Rad2Deg) - 90f;

                var facet = GameObject.CreatePrimitive(PrimitiveType.Cube);
                facet.name = $"CeilingFacet_{i}";
                facet.transform.SetParent(root.transform, false);
                facet.transform.localPosition = new Vector3(xMid, yMid, 0f);
                facet.transform.localRotation = Quaternion.Euler(0f, 0f, angleDeg);
                facet.transform.localScale = new Vector3(facetChord, thick, len);
                facet.GetComponent<Renderer>().sharedMaterial = settings.stoneWallMaterial;
            }

            // Modeled wooden arch rib across the segment (B3)
            var rib = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rib.name = "TimberRib";
            rib.transform.SetParent(root.transform, false);
            rib.transform.localPosition = new Vector3(0f, h - 0.08f, 0f);
            rib.transform.localScale = new Vector3(w + 0.1f, 0.16f, 0.18f);
            rib.GetComponent<Renderer>().sharedMaterial = settings.woodMaterial;
            var ribCol = rib.GetComponent<Collider>();
            if (ribCol) Object.DestroyImmediate(ribCol);

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }
    }
}
