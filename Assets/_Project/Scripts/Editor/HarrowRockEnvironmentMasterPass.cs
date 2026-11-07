using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace TheLightHouse.Editor
{
    public static class HarrowRockEnvironmentMasterPass
    {
        private const string RootName = "Harrow Rock Master Environment";
        private const string TexEnvRoot = "Assets/_Project/Art/Textures/Environment";
        private const string TexPropsRoot = "Assets/_Project/Art/Textures/Props";
        private const string MatRoot = "Assets/_Project/Materials/Reference";

        private static Material matPeelingBrick;
        private static Material matWetFootprint;
        private static Material matLedger;
        private static Material matNauticalChart;
        private static Material matWarningSign;
        private static Material matReelToReel;
        private static Material matCopperKettle;
        private static Material matVintageFridge;
        private static Material matDarkBread;
        private static Material matWood;
        private static Material matIron;
        private static Material matStone;
        private static Material matSlate;
        private static Material matLanternGlow;
        private static Material matCeramic;
        private static Material matWarmWindow;
        private static Material matPuddle;
        private static Material matSlickCobble;
        private static Material matMahogany;
        private static Material matBrass;
        private static Material matSpecimenJar;
        private static Material matRug;
        private static Material matBookRed;
        private static Material matBookGreen;
        private static Material matBookBlue;
        private static Material matBookGold;
        private static Material matParchment;
        private static Material matFloodWater;
        private static Material matSeaFoam;
        private static Material matBoatShedPlanks;
        private static Material matSlateRoof;
        private static Material matPhoto;
        private static Material matVacuumTubeGlass;
        private static Material matTubeFilament;
        private static Material matIndicatorGreen;
        private static Material matIndicatorRed;
        private static Material matBakelite;
        private static Material matDeskLampShade;
        private static Material matRadioMetal;

        [MenuItem("TheLightHouse/Apply Complete Harrow Rock Environments")]
        public static void ApplyToAllScenes()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Play Mode Active", "Please exit Play Mode before applying environments.", "OK");
                return;
            }

            PrepareMaterials();

            string[] scenes = new[]
            {
                "Assets/_Project/Scenes/Act1.unity",
                "Assets/_Project/Scenes/Act2.unity",
                "Assets/_Project/Scenes/Act3.unity",
                "Assets/_Project/Scenes/Act4.unity",
                "Assets/_Project/Scenes/Act5.unity"
            };

            foreach (string scenePath in scenes)
            {
                if (!File.Exists(scenePath)) continue;
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                ApplyToActiveScene();
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log($"<b>[HarrowRockEnvironmentMasterPass] Successfully built complete environment for {scene.name}.</b>");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("TheLightHouse/Apply Complete Environment (Current Scene)")]
        public static void ApplyToCurrentScene()
        {
            PrepareMaterials();
            ApplyToActiveScene();
            var scene = SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"<b>[HarrowRockEnvironmentMasterPass] Applied complete environment to active scene: {scene.name}</b>");
        }

        public static void PrepareMaterials()
        {
            ConfigureTextureImporters();

            matWood = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/PBR/PBR_WeatheredPlanks.mat")
                     ?? AssetDatabase.LoadAssetAtPath<Material>($"{MatRoot}/Old Timber.mat")
                     ?? EnsureMaterial("Fallback_Wood", "Universal Render Pipeline/Lit");
            matIron = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/PBR/PBR_RustedIron.mat")
                     ?? AssetDatabase.LoadAssetAtPath<Material>($"{MatRoot}/Oxidised Iron.mat")
                     ?? EnsureMaterial("Fallback_Iron", "Universal Render Pipeline/Lit");
            matStone = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/PBR/PBR_LighthouseStone.mat")
                      ?? AssetDatabase.LoadAssetAtPath<Material>($"{MatRoot}/Wet Shale.mat")
                      ?? EnsureMaterial("Fallback_Stone", "Universal Render Pipeline/Lit");
            matSlate = AssetDatabase.LoadAssetAtPath<Material>($"{MatRoot}/Slate.mat")
                      ?? EnsureMaterial("Fallback_Slate", "Universal Render Pipeline/Lit");
            matCeramic = AssetDatabase.LoadAssetAtPath<Material>($"{MatRoot}/Cottage Ceramic.mat")
                        ?? EnsureMaterial("CottageCeramic", "Universal Render Pipeline/Lit");
            matCeramic.SetColor("_BaseColor", new Color(0.92f, 0.90f, 0.86f));

            matLanternGlow = AssetDatabase.LoadAssetAtPath<Material>($"{MatRoot}/Dock Lantern Glow.mat")
                            ?? EnsureMaterial("DockLanternGlow", "Universal Render Pipeline/Lit");
            matLanternGlow.SetColor("_BaseColor", new Color(1.0f, 0.82f, 0.55f));
            matLanternGlow.EnableKeyword("_EMISSION");
            matLanternGlow.SetColor("_EmissionColor", new Color(1.0f, 0.82f, 0.55f) * 2.0f);

            matWarmWindow = AssetDatabase.LoadAssetAtPath<Material>($"{MatRoot}/Warm Cottage Glass.mat")
                           ?? EnsureMaterial("WarmCottageGlass", "Universal Render Pipeline/Lit");
            matWarmWindow.SetColor("_BaseColor", new Color(1.0f, 0.78f, 0.38f));
            matWarmWindow.EnableKeyword("_EMISSION");
            matWarmWindow.SetColor("_EmissionColor", new Color(1.0f, 0.78f, 0.38f) * 2.0f);

            // 1. Peeling Brick Material
            matPeelingBrick = EnsureMaterial("LighthousePeelingBrick", "Universal Render Pipeline/Lit");
            var brickDiff = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TexEnvRoot}/LighthousePeelingBrick_diff.png");
            var brickNor = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Project/Art/Textures/PBR/LighthouseStone/LighthouseStone_nor.jpg");
            if (brickDiff) { matPeelingBrick.SetTexture("_BaseMap", brickDiff); matPeelingBrick.SetTextureScale("_BaseMap", new Vector2(3f, 12f)); }
            if (brickNor) { matPeelingBrick.SetTexture("_BumpMap", brickNor); matPeelingBrick.EnableKeyword("_NORMALMAP"); matPeelingBrick.SetTextureScale("_BumpMap", new Vector2(3f, 12f)); }
            matPeelingBrick.SetFloat("_Smoothness", 0.35f);
            matPeelingBrick.SetFloat("_Metallic", 0.05f);

            // 2. Wet Footprint Decal Material
            matWetFootprint = EnsureMaterial("WetFootprintDecal", "Universal Render Pipeline/Lit");
            var fpDiff = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TexEnvRoot}/WetFootprint_diff.png");
            var fpNor = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TexEnvRoot}/WetFootprint_nor.png");
            if (fpDiff) matWetFootprint.SetTexture("_BaseMap", fpDiff);
            if (fpNor) { matWetFootprint.SetTexture("_BumpMap", fpNor); matWetFootprint.EnableKeyword("_NORMALMAP"); }
            matWetFootprint.SetFloat("_Surface", 1); // Transparent
            matWetFootprint.SetFloat("_Blend", 0); // Alpha
            matWetFootprint.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            matWetFootprint.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            matWetFootprint.SetInt("_ZWrite", 0);
            matWetFootprint.renderQueue = (int)RenderQueue.Transparent + 10;
            matWetFootprint.SetFloat("_Smoothness", 0.95f); // Wet reflective water puddle
            matWetFootprint.SetFloat("_Metallic", 0.0f);

            // 3. Harbor Board Ledger Material
            matLedger = EnsureMaterial("HarborBoardLedger", "Universal Render Pipeline/Lit");
            var ledgerTex = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TexPropsRoot}/HarborBoardLedger_page.png");
            if (ledgerTex) matLedger.SetTexture("_BaseMap", ledgerTex);
            matLedger.SetFloat("_Smoothness", 0.15f);

            // 4. Nautical Chart Material
            matNauticalChart = EnsureMaterial("NauticalChart_Harrow", "Universal Render Pipeline/Lit");
            var chartTex = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TexPropsRoot}/NauticalChart_HarrowRock.png");
            if (chartTex) matNauticalChart.SetTexture("_BaseMap", chartTex);
            matNauticalChart.SetFloat("_Smoothness", 0.18f);

            // 5. Cellar Warning Sign Material
            matWarningSign = EnsureMaterial("CellarWarningSign", "Universal Render Pipeline/Lit");
            var signTex = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TexPropsRoot}/CellarWarningSign.png");
            if (signTex) matWarningSign.SetTexture("_BaseMap", signTex);
            matWarningSign.SetFloat("_Smoothness", 0.70f);
            matWarningSign.SetFloat("_Metallic", 0.40f);

            // 6. Reel To Reel Tape Deck Material
            matReelToReel = EnsureMaterial("ReelToReelFaceplate", "Universal Render Pipeline/Lit");
            var rtrTex = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TexPropsRoot}/ReelToReel_faceplate.png");
            if (rtrTex) matReelToReel.SetTexture("_BaseMap", rtrTex);
            matReelToReel.SetFloat("_Smoothness", 0.72f);
            matReelToReel.SetFloat("_Metallic", 0.65f);

            // 7. Tarnished Copper Kettle
            matCopperKettle = EnsureMaterial("TarnishedCopperKettle", "Universal Render Pipeline/Lit");
            matCopperKettle.SetColor("_BaseColor", new Color(0.72f, 0.44f, 0.28f));
            matCopperKettle.SetFloat("_Metallic", 0.82f);
            matCopperKettle.SetFloat("_Smoothness", 0.68f);

            // 8. Vintage Refrigerator Material
            matVintageFridge = EnsureMaterial("VintageRefrigeratorEnamel", "Universal Render Pipeline/Lit");
            matVintageFridge.SetColor("_BaseColor", new Color(0.86f, 0.85f, 0.80f));
            matVintageFridge.SetFloat("_Metallic", 0.15f);
            matVintageFridge.SetFloat("_Smoothness", 0.75f);

            // 9. Dark Bread Loaf Material
            matDarkBread = EnsureMaterial("DarkBreadCrust", "Universal Render Pipeline/Lit");
            matDarkBread.SetColor("_BaseColor", new Color(0.28f, 0.18f, 0.11f));
            matDarkBread.SetFloat("_Smoothness", 0.12f);

            // 10. Water Puddle Material
            matPuddle = EnsureMaterial("FloorWaterPuddle", "Universal Render Pipeline/Lit");
            matPuddle.SetColor("_BaseColor", new Color(0.04f, 0.05f, 0.06f, 0.82f));
            matPuddle.SetFloat("_Surface", 1);
            matPuddle.SetFloat("_Smoothness", 0.98f);
            matPuddle.SetFloat("_Metallic", 0.0f);

            // 11. Slick Sea Cobblestone
            matSlickCobble = EnsureMaterial("SlickSeaCobble", "Universal Render Pipeline/Lit");
            var cobbleDiff = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Project/Art/Textures/PBR/LighthouseStone/LighthouseStone_diff.jpg");
            var cobbleNor = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Project/Art/Textures/PBR/LighthouseStone/LighthouseStone_nor.jpg");
            if (cobbleDiff) { matSlickCobble.SetTexture("_BaseMap", cobbleDiff); matSlickCobble.SetTextureScale("_BaseMap", new Vector2(4f, 4f)); }
            if (cobbleNor) { matSlickCobble.SetTexture("_BumpMap", cobbleNor); matSlickCobble.EnableKeyword("_NORMALMAP"); matSlickCobble.SetTextureScale("_BumpMap", new Vector2(4f, 4f)); }
            matSlickCobble.SetColor("_BaseColor", new Color(0.18f, 0.20f, 0.22f));
            matSlickCobble.SetFloat("_Smoothness", 0.85f); // High specular wet reflection
            matSlickCobble.SetFloat("_Metallic", 0.05f);

            // 12. Polished Mahogany Writing Desk Material
            matMahogany = EnsureMaterial("PolishedMahogany", "Universal Render Pipeline/Lit");
            matMahogany.SetColor("_BaseColor", new Color(0.24f, 0.12f, 0.08f));
            matMahogany.SetFloat("_Smoothness", 0.72f);
            matMahogany.SetFloat("_Metallic", 0.02f);

            // 13. Maritime Brass Navigational Calipers & Instruments
            matBrass = EnsureMaterial("PolishedMaritimeBrass", "Universal Render Pipeline/Lit");
            matBrass.SetColor("_BaseColor", new Color(0.92f, 0.78f, 0.44f));
            matBrass.SetFloat("_Metallic", 0.94f);
            matBrass.SetFloat("_Smoothness", 0.88f);
            matBrass.SetTexture("_BaseMap", null);
            matBrass.SetTexture("_BumpMap", null);
            matBrass.DisableKeyword("_NORMALMAP");

            // 14. Specimen Jar Translucent Glass
            matSpecimenJar = EnsureMaterial("SpecimenJarGlass", "Universal Render Pipeline/Lit");
            matSpecimenJar.SetColor("_BaseColor", new Color(0.72f, 0.86f, 0.80f, 0.40f));
            matSpecimenJar.SetFloat("_Surface", 1);
            matSpecimenJar.SetFloat("_Blend", 0);
            matSpecimenJar.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            matSpecimenJar.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            matSpecimenJar.SetInt("_ZWrite", 0);
            matSpecimenJar.renderQueue = (int)RenderQueue.Transparent + 5;
            matSpecimenJar.SetFloat("_Smoothness", 0.94f);

            // 15. Persian Study Carpet
            matRug = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Reference/Persian Carpet.mat")
                  ?? EnsureMaterial("LibraryCarpet", "Universal Render Pipeline/Lit");

            // 16. Study Book Bindings & Parchment
            matBookRed = ReferenceSurfaceLibrary.Solid("Study Book Red", new Color(0.38f, 0.08f, 0.08f));
            matBookGreen = ReferenceSurfaceLibrary.Solid("Study Book Green", new Color(0.08f, 0.28f, 0.14f));
            matBookBlue = ReferenceSurfaceLibrary.Solid("Study Book Blue", new Color(0.08f, 0.14f, 0.32f));
            matBookGold = ReferenceSurfaceLibrary.Solid("Study Book Gold", new Color(0.80f, 0.65f, 0.25f));
            matParchment = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Reference/Aged Parchment.mat")
                        ?? EnsureMaterial("AgedParchment", "Universal Render Pipeline/Lit");

            // 17. Flooded Courtyard Water & Swirling Foam
            matFloodWater = EnsureMaterial("FloodedCourtyardWater", "Universal Render Pipeline/Lit");
            matFloodWater.SetFloat("_Surface", 1); // Transparent
            matFloodWater.SetFloat("_Blend", 0);
            matFloodWater.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            matFloodWater.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            matFloodWater.SetInt("_ZWrite", 0);
            matFloodWater.renderQueue = (int)RenderQueue.Transparent + 15;
            matFloodWater.SetColor("_BaseColor", new Color(0.05f, 0.08f, 0.11f, 0.90f));
            matFloodWater.SetFloat("_Smoothness", 0.98f);
            matFloodWater.SetFloat("_Metallic", 0.05f);

            matSeaFoam = EnsureMaterial("CourtyardSeaFoam", "Universal Render Pipeline/Lit");
            matSeaFoam.SetFloat("_Surface", 1); // Transparent
            matSeaFoam.SetFloat("_Blend", 0);
            matSeaFoam.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            matSeaFoam.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            matSeaFoam.SetInt("_ZWrite", 0);
            matSeaFoam.renderQueue = (int)RenderQueue.Transparent + 20;
            var foamTex = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TexEnvRoot}/Seafoam_diff.png");
            if (foamTex) matSeaFoam.SetTexture("_BaseMap", foamTex);
            matSeaFoam.SetColor("_BaseColor", new Color(0.92f, 0.96f, 0.98f, 0.85f));
            matSeaFoam.EnableKeyword("_EMISSION");
            matSeaFoam.SetColor("_EmissionColor", new Color(0.85f, 0.92f, 0.95f) * 0.45f);
            matSeaFoam.SetFloat("_Smoothness", 0.45f);

            // 18. Weathered Boat Shed Planks & Slate Roof
            matBoatShedPlanks = EnsureMaterial("BoatShedWeatheredPlanks", "Universal Render Pipeline/Lit");
            var plankDiff = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Project/Art/Textures/PBR/WeatheredPlanks/WeatheredPlanks_diff.jpg");
            var plankNor = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Project/Art/Textures/PBR/WeatheredPlanks/WeatheredPlanks_nor.jpg");
            if (plankDiff) { matBoatShedPlanks.SetTexture("_BaseMap", plankDiff); matBoatShedPlanks.SetTextureScale("_BaseMap", new Vector2(3f, 6f)); }
            if (plankNor) { matBoatShedPlanks.SetTexture("_BumpMap", plankNor); matBoatShedPlanks.EnableKeyword("_NORMALMAP"); matBoatShedPlanks.SetTextureScale("_BumpMap", new Vector2(3f, 6f)); }
            matBoatShedPlanks.SetColor("_BaseColor", new Color(0.24f, 0.20f, 0.17f));
            matBoatShedPlanks.SetFloat("_Smoothness", 0.52f);
            matBoatShedPlanks.SetFloat("_Metallic", 0.02f);

            matSlateRoof = EnsureMaterial("BoatShedSlateRoof", "Universal Render Pipeline/Lit");
            var stoneDiff = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Project/Art/Textures/PBR/LighthouseStone/LighthouseStone_diff.jpg");
            var stoneNor = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Project/Art/Textures/PBR/LighthouseStone/LighthouseStone_nor.jpg");
            if (stoneDiff) { matSlateRoof.SetTexture("_BaseMap", stoneDiff); matSlateRoof.SetTextureScale("_BaseMap", new Vector2(4f, 6f)); }
            if (stoneNor) { matSlateRoof.SetTexture("_BumpMap", stoneNor); matSlateRoof.EnableKeyword("_NORMALMAP"); matSlateRoof.SetTextureScale("_BumpMap", new Vector2(4f, 6f)); }
            matSlateRoof.SetColor("_BaseColor", new Color(0.16f, 0.18f, 0.21f));
            matSlateRoof.SetFloat("_Smoothness", 0.65f);

            // 19. Framed Family Photograph Material
            matPhoto = EnsureMaterial("FramedFamilyPhoto", "Universal Render Pipeline/Lit");
            var photoTex = AssetDatabase.LoadAssetAtPath<Texture2D>($"{TexPropsRoot}/Crane_Daniel_Photo.png");
            if (photoTex) matPhoto.SetTexture("_BaseMap", photoTex);
            matPhoto.SetFloat("_Smoothness", 0.88f);
            matPhoto.SetFloat("_Metallic", 0.05f);

            // 20. Vacuum Tube Glass & Glowing Tungsten Filament
            matVacuumTubeGlass = EnsureMaterial("VacuumTubeGlass", "Universal Render Pipeline/Lit");
            matVacuumTubeGlass.SetFloat("_Surface", 1); // Transparent
            matVacuumTubeGlass.SetFloat("_Blend", 0);
            matVacuumTubeGlass.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            matVacuumTubeGlass.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            matVacuumTubeGlass.SetInt("_ZWrite", 0);
            matVacuumTubeGlass.renderQueue = (int)RenderQueue.Transparent + 10;
            matVacuumTubeGlass.SetColor("_BaseColor", new Color(0.9f, 0.95f, 1.0f, 0.35f));
            matVacuumTubeGlass.SetFloat("_Smoothness", 0.96f);
            matVacuumTubeGlass.SetFloat("_Metallic", 0.1f);

            matTubeFilament = EnsureMaterial("VacuumTubeFilament", "Universal Render Pipeline/Lit");
            matTubeFilament.SetColor("_BaseColor", new Color(1.0f, 0.42f, 0.12f));
            matTubeFilament.EnableKeyword("_EMISSION");
            matTubeFilament.SetColor("_EmissionColor", new Color(1.0f, 0.45f, 0.15f) * 3.5f);

            matIndicatorGreen = EnsureMaterial("RadioIndicatorGreen", "Universal Render Pipeline/Lit");
            matIndicatorGreen.SetColor("_BaseColor", new Color(0.2f, 0.9f, 0.3f));
            matIndicatorGreen.EnableKeyword("_EMISSION");
            matIndicatorGreen.SetColor("_EmissionColor", new Color(0.2f, 1.0f, 0.35f) * 3.0f);

            matIndicatorRed = EnsureMaterial("RadioIndicatorRed", "Universal Render Pipeline/Lit");
            matIndicatorRed.SetColor("_BaseColor", new Color(0.95f, 0.2f, 0.15f));
            matIndicatorRed.EnableKeyword("_EMISSION");
            matIndicatorRed.SetColor("_EmissionColor", new Color(1.0f, 0.25f, 0.15f) * 3.0f);

            matBakelite = EnsureMaterial("BakeliteBlack", "Universal Render Pipeline/Lit");
            matBakelite.SetColor("_BaseColor", new Color(0.12f, 0.11f, 0.11f));
            matBakelite.SetFloat("_Smoothness", 0.78f);
            matBakelite.SetFloat("_Metallic", 0.05f);

            matDeskLampShade = EnsureMaterial("BankersLampGreen", "Universal Render Pipeline/Lit");
            matDeskLampShade.SetColor("_BaseColor", new Color(0.06f, 0.32f, 0.16f));
            matDeskLampShade.SetFloat("_Smoothness", 0.88f);
            matDeskLampShade.SetFloat("_Metallic", 0.2f);

            matRadioMetal = EnsureMaterial("VintageRadioMetal", "Universal Render Pipeline/Lit");
            matRadioMetal.SetColor("_BaseColor", new Color(0.20f, 0.22f, 0.25f));
            matRadioMetal.SetFloat("_Smoothness", 0.65f);
            matRadioMetal.SetFloat("_Metallic", 0.70f);

            AssetDatabase.SaveAssets();
        }

        private static Material EnsureMaterial(string name, string shaderName)
        {
            string path = $"{MatRoot}/{name}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!mat)
            {
                var shader = Shader.Find(shaderName) ?? Shader.Find("Universal Render Pipeline/Lit");
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
            }
            return mat;
        }

        private static void ConfigureTextureImporters()
        {
            string[] normalMaps = new[]
            {
                $"{TexEnvRoot}/WetFootprint_nor.png"
            };

            foreach (var path in normalMaps)
            {
                if (!File.Exists(path)) continue;
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer && importer.textureType != TextureImporterType.NormalMap)
                {
                    importer.textureType = TextureImporterType.NormalMap;
                    importer.SaveAndReimport();
                }
            }

            string[] alphaTextures = new[]
            {
                $"{TexEnvRoot}/WetFootprint_diff.png",
                $"{TexEnvRoot}/Seafoam_diff.png"
            };
            foreach (var path in alphaTextures)
            {
                if (!File.Exists(path)) continue;
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer && !importer.alphaIsTransparency)
                {
                    importer.alphaIsTransparency = true;
                    importer.SaveAndReimport();
                }
            }
        }

        public static void ApplyToActiveScene()
        {
            PrepareMaterials();

            var existing = GameObject.Find(RootName);
            if (existing) Object.DestroyImmediate(existing);

            var root = new GameObject(RootName).transform;

            BuildExteriorIsland(root);
            BuildKeepersHouseInterior(root);
            BuildTowerInteriorAndStudy(root);
            BuildUndergroundCellar(root);
            BuildFloodedCourtyardAndBoatShed(root);
        }

        // =========================================================================
        // 1. HARROW ROCK EXTERIOR ISLAND SCENE
        // =========================================================================
        private static void BuildExteriorIsland(Transform parent)
        {
            var exteriorRoot = new GameObject("Exterior Island & Architecture").transform;
            exteriorRoot.SetParent(parent, false);

            // A. 35-Meter Weathered Circular Brick Lighthouse Tower Enhancements
            var towerGroup = new GameObject("35m Weathered Tower Dressing").transform;
            towerGroup.SetParent(exteriorRoot, false);

            // Update existing tapered masonry renderer with peeling brick material
            var taperedMasonry = GameObject.Find("Tapered Masonry");
            if (taperedMasonry && matPeelingBrick)
            {
                var rend = taperedMasonry.GetComponent<Renderer>() ?? taperedMasonry.GetComponentInChildren<Renderer>();
                if (rend) rend.sharedMaterial = matPeelingBrick;
            }

            // Tower Structural Iron Rivet Rings & Downward Rust Streaks
            float[] ringHeights = new[] { 7.5f, 15.0f, 22.5f, 30.0f };
            foreach (float y in ringHeights)
            {
                float radius = Mathf.Lerp(6.5f, 4.8f, y / 25.6f) + 0.04f;
                var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                ring.name = $"Tower Rivet Ring {y:F0}m";
                ring.transform.SetParent(towerGroup, false);
                ring.transform.position = new Vector3(0, y, 0);
                ring.transform.localScale = new Vector3(radius * 2f, 0.16f, radius * 2f);
                ring.GetComponent<Renderer>().sharedMaterial = matIron;
                Object.DestroyImmediate(ring.GetComponent<Collider>());
            }

            // Top Lantern Volumetric Sweeping Spotlight cutting through driving rain
            var sweepingLightGo = new GameObject("Top Lantern Sweeping Spotlight");
            sweepingLightGo.transform.SetParent(towerGroup, false);
            sweepingLightGo.transform.position = new Vector3(0, 34.5f, 0);
            sweepingLightGo.transform.rotation = Quaternion.Euler(24f, 0, 0);

            var spot = sweepingLightGo.AddComponent<Light>();
            spot.type = LightType.Spot;
            spot.spotAngle = 38f;
            spot.innerSpotAngle = 22f;
            spot.range = 75f;
            spot.intensity = 18f;
            spot.color = new Color(1.0f, 0.95f, 0.85f);
            spot.shadows = LightShadows.Soft;

            var beamPivot = new GameObject("Sweeping Pivot").transform;
            beamPivot.SetParent(sweepingLightGo.transform, false);
            sweepingLightGo.AddComponent<VolumetricSweepLight>();

            // B. Dock Gallows Post & Warm Swinging Oil Lantern
            var dockPost = GameObject.Find("Gallows Lamp Post");
            if (dockPost)
            {
                var lightChild = dockPost.transform.Find("Dock Lantern Light");
                Light dockLight;
                if (!lightChild)
                {
                    var lg = new GameObject("Dock Lantern Light");
                    lg.transform.SetParent(dockPost.transform, false);
                    lg.transform.position = new Vector3(-1.5f, 2.58f, -28.5f);
                    dockLight = lg.AddComponent<Light>();
                }
                else
                {
                    dockLight = lightChild.GetComponent<Light>();
                }

                if (dockLight)
                {
                    dockLight.type = LightType.Point;
                    // Exact specification: Color: #FFAA44, intensity: 350 lumens (approx 3.5 cd)
                    dockLight.color = new Color(1.0f, 0.667f, 0.267f);
                    dockLight.intensity = 3.5f;
                    dockLight.range = 9.5f;
                    dockLight.shadows = LightShadows.Soft;

                    if (!dockLight.GetComponent<FlickerLight>())
                    {
                        var fl = dockLight.gameObject.AddComponent<FlickerLight>();
                        var so = new SerializedObject(fl);
                        so.FindProperty("baseIntensity").floatValue = 3.5f;
                        so.FindProperty("amplitude").floatValue = 0.35f;
                        so.FindProperty("frequency").floatValue = 8.5f;
                        so.ApplyModifiedProperties();
                    }
                }

                // Attach subtle wind pendulum sway to the hanging lantern
                var hood = dockPost.transform.Find("Lantern Glass Chimney");
                if (hood && !hood.GetComponent<LanternSway>())
                {
                    hood.gameObject.AddComponent<LanternSway>();
                }
            }

            // C. Cottage Window Frosted Glass Glow
            var cottageWindowLight = new GameObject("Cottage Frosted Window Light");
            cottageWindowLight.transform.SetParent(exteriorRoot, false);
            cottageWindowLight.transform.position = new Vector3(-8.5f, 1.65f, -9.5f);
            var cwl = cottageWindowLight.AddComponent<Light>();
            cwl.type = LightType.Point;
            cwl.color = new Color(1.0f, 0.72f, 0.38f);
            cwl.intensity = 1.8f;
            cwl.range = 4.5f;
            cwl.shadows = LightShadows.None;

            // D. Slick Sea-Weathered Cobblestones along Courtyard Approach
            var cobbleGroup = new GameObject("Slick Sea Cobblestone Approach").transform;
            cobbleGroup.SetParent(exteriorRoot, false);
            for (int i = 0; i < 6; i++)
            {
                float cz = -10.5f - i * 1.6f;
                var cobbleSlab = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cobbleSlab.name = $"Cobblestone Slab {i}";
                cobbleSlab.transform.SetParent(cobbleGroup, false);
                cobbleSlab.transform.position = new Vector3(-0.8f, 0.045f, cz);
                cobbleSlab.transform.localScale = new Vector3(2.8f, 0.08f, 1.4f);
                cobbleSlab.GetComponent<Renderer>().sharedMaterial = matSlickCobble;
                Object.DestroyImmediate(cobbleSlab.GetComponent<Collider>());
            }
        }

        // =========================================================================
        // 2. KEEPER'S HOUSE LIVING ROOM AND KITCHEN INTERIOR
        // =========================================================================
        private static void BuildKeepersHouseInterior(Transform parent)
        {
            var interiorRoot = new GameObject("Keeper House Living & Kitchen Master").transform;
            interiorRoot.SetParent(parent, false);

            Vector3 tablePos = new Vector3(-9.5f, 0f, -7.0f);

            // A. The Scarred Oak Dining Table in Room Center (Matching Image 10 Panel 2)
            var tableGroup = new GameObject("Scarred Oak Dining Table Set").transform;
            tableGroup.SetParent(interiorRoot, false);

            // Sturdy rectangular oak table
            var tableTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tableTop.name = "Scarred Oak Tabletop";
            tableTop.transform.SetParent(tableGroup, false);
            tableTop.transform.position = tablePos + new Vector3(0, 0.82f, 0);
            tableTop.transform.localScale = new Vector3(1.35f, 0.07f, 2.10f);
            tableTop.GetComponent<Renderer>().sharedMaterial = matWood;

            // 4 turned legs
            for (int x = -1; x <= 1; x += 2)
            {
                for (int z = -1; z <= 1; z += 2)
                {
                    var leg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    leg.name = $"Table Leg ({x},{z})";
                    leg.transform.SetParent(tableGroup, false);
                    leg.transform.position = tablePos + new Vector3(x * 0.58f, 0.40f, z * 0.95f);
                    leg.transform.localScale = new Vector3(0.09f, 0.80f, 0.09f);
                    leg.GetComponent<Renderer>().sharedMaterial = matWood;
                }
            }

            // Steaming Hot Bowl of Stew
            Vector3 stewPos = tablePos + new Vector3(0.12f, 0.86f, -0.25f);
            var stewBowl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stewBowl.name = "Steaming Stew Bowl";
            stewBowl.transform.SetParent(tableGroup, false);
            stewBowl.transform.position = stewPos;
            stewBowl.transform.localScale = new Vector3(0.28f, 0.09f, 0.28f);
            stewBowl.GetComponent<Renderer>().sharedMaterial = matCeramic;
            Object.DestroyImmediate(stewBowl.GetComponent<Collider>());

            var stewSurface = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stewSurface.name = "Stew Surface";
            stewSurface.transform.SetParent(stewBowl.transform, false);
            stewSurface.transform.localPosition = new Vector3(0, 0.45f, 0);
            stewSurface.transform.localScale = new Vector3(0.88f, 0.12f, 0.88f);
            stewSurface.GetComponent<Renderer>().sharedMaterial = matDarkBread;
            Object.DestroyImmediate(stewSurface.GetComponent<Collider>());

            // Stew steam emitter
            var stewSteam = new GameObject("Stew Steam Emitter");
            stewSteam.transform.SetParent(stewBowl.transform, false);
            stewSteam.transform.localPosition = new Vector3(0, 0.6f, 0);
            stewSteam.AddComponent<SteamEmitter>();

            // Wooden Spoon
            var spoon = GameObject.CreatePrimitive(PrimitiveType.Cube);
            spoon.name = "Wooden Spoon";
            spoon.transform.SetParent(tableGroup, false);
            spoon.transform.position = stewPos + new Vector3(0.18f, 0.03f, 0.08f);
            spoon.transform.localScale = new Vector3(0.03f, 0.015f, 0.22f);
            spoon.transform.rotation = Quaternion.Euler(0, 25f, 0);
            spoon.GetComponent<Renderer>().sharedMaterial = matWood;
            Object.DestroyImmediate(spoon.GetComponent<Collider>());

            // Chipped Ceramic Mug
            var mug = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mug.name = "Chipped Ceramic Mug";
            mug.transform.SetParent(tableGroup, false);
            mug.transform.position = tablePos + new Vector3(-0.35f, 0.94f, -0.45f);
            mug.transform.localScale = new Vector3(0.12f, 0.14f, 0.12f);
            mug.GetComponent<Renderer>().sharedMaterial = matCeramic;
            Object.DestroyImmediate(mug.GetComponent<Collider>());

            // Half-eaten Loaf of Dark Bread
            var breadLoaf = GameObject.CreatePrimitive(PrimitiveType.Cube);
            breadLoaf.name = "Loaf of Dark Bread";
            breadLoaf.transform.SetParent(tableGroup, false);
            breadLoaf.transform.position = tablePos + new Vector3(-0.25f, 0.89f, 0.35f);
            breadLoaf.transform.localScale = new Vector3(0.24f, 0.12f, 0.38f);
            breadLoaf.transform.rotation = Quaternion.Euler(0, -18f, 0);
            breadLoaf.GetComponent<Renderer>().sharedMaterial = matDarkBread;
            Object.DestroyImmediate(breadLoaf.GetComponent<Collider>());

            // B. Kitchenette Enhancements: Tarnished Copper Kettle emitting steam
            Vector3 stovePos = new Vector3(-12.05f, 0.46f, -1.80f);
            var kettleGroup = new GameObject("Tarnished Copper Kettle").transform;
            kettleGroup.SetParent(interiorRoot, false);
            Vector3 kettlePos = stovePos + new Vector3(0.18f, 0.58f, 0.22f);

            var kettleBody = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            kettleBody.name = "Kettle Body";
            kettleBody.transform.SetParent(kettleGroup, false);
            kettleBody.transform.position = kettlePos;
            kettleBody.transform.localScale = new Vector3(0.26f, 0.18f, 0.26f);
            kettleBody.GetComponent<Renderer>().sharedMaterial = matCopperKettle;
            Object.DestroyImmediate(kettleBody.GetComponent<Collider>());

            var kettleSpout = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            kettleSpout.name = "Kettle Spout";
            kettleSpout.transform.SetParent(kettleBody.transform, false);
            kettleSpout.transform.localPosition = new Vector3(0.55f, 0.25f, 0);
            kettleSpout.transform.localScale = new Vector3(0.18f, 0.45f, 0.18f);
            kettleSpout.transform.localRotation = Quaternion.Euler(0, 0, -42f);
            kettleSpout.GetComponent<Renderer>().sharedMaterial = matCopperKettle;
            Object.DestroyImmediate(kettleSpout.GetComponent<Collider>());

            var kettleSteam = new GameObject("Kettle Steam Emitter");
            kettleSteam.transform.SetParent(kettleSpout.transform, false);
            kettleSteam.transform.localPosition = new Vector3(0, 0.5f, 0);
            kettleSteam.AddComponent<SteamEmitter>();

            // C. Old Enameled Refrigerator in Corner (Matching Image 10 Panel 2)
            Vector3 fridgePos = new Vector3(-6.2f, 0.95f, -1.8f);
            var fridgeGroup = new GameObject("Vintage Enameled Refrigerator").transform;
            fridgeGroup.SetParent(interiorRoot, false);

            var fridgeBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fridgeBody.name = "Fridge Cabinet";
            fridgeBody.transform.SetParent(fridgeGroup, false);
            fridgeBody.transform.position = fridgePos;
            fridgeBody.transform.localScale = new Vector3(0.85f, 1.80f, 0.85f);
            fridgeBody.GetComponent<Renderer>().sharedMaterial = matVintageFridge;

            var fridgeHandle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fridgeHandle.name = "Corroded Latch Handle";
            fridgeHandle.transform.SetParent(fridgeGroup, false);
            fridgeHandle.transform.position = fridgePos + new Vector3(-0.35f, 0.15f, -0.45f);
            fridgeHandle.transform.localScale = new Vector3(0.04f, 0.26f, 0.05f);
            fridgeHandle.GetComponent<Renderer>().sharedMaterial = matIron;
            Object.DestroyImmediate(fridgeHandle.GetComponent<Collider>());

            // D. Primary Overhead Pendant Lamp over Dining Table (3000K, Volumetric Haze)
            var pendantLightGo = new GameObject("Dining Overhead Pendant Light");
            pendantLightGo.transform.SetParent(interiorRoot, false);
            pendantLightGo.transform.position = tablePos + new Vector3(0, 2.2f, 0);
            var pendantLight = pendantLightGo.AddComponent<Light>();
            pendantLight.type = LightType.Spot;
            pendantLight.spotAngle = 78f;
            pendantLight.innerSpotAngle = 48f;
            pendantLight.range = 5.5f;
            pendantLight.intensity = 3.2f;
            pendantLight.color = new Color(1.0f, 0.77f, 0.45f); // 3000K Warm Incandescent
            pendantLight.shadows = LightShadows.Soft;

            var pendantFill = new GameObject("Dining Table Ambient Fill");
            pendantFill.transform.SetParent(pendantLightGo.transform, false);
            var pFill = pendantFill.AddComponent<Light>();
            pFill.type = LightType.Point;
            pFill.range = 4.2f;
            pFill.intensity = 1.1f;
            pFill.color = new Color(1.0f, 0.72f, 0.38f);
            pFill.shadows = LightShadows.None;

            // E. Cool Blue Moonlight Bleeding Through Window Panes
            var moonBleed = new GameObject("Cool Moonlight Window Bleed");
            moonBleed.transform.SetParent(interiorRoot, false);
            moonBleed.transform.position = new Vector3(-8.8f, 2.1f, -1.6f);
            var mb = moonBleed.AddComponent<Light>();
            mb.type = LightType.Spot;
            mb.spotAngle = 65f;
            mb.range = 6.0f;
            mb.intensity = 1.6f;
            mb.color = new Color(0.42f, 0.58f, 0.85f); // Soft cool blue moonlight
            mb.transform.rotation = Quaternion.Euler(35f, 180f, 0);
            mb.shadows = LightShadows.Soft;

            // F. Stamped Wet Footprint Decals leading from doorway across planks to Cellar Door
            var footprintTrail = new GameObject("Wet Footprint Trail (To Cellar)").transform;
            footprintTrail.SetParent(interiorRoot, false);

            Vector3[] trailWaypoints = new[]
            {
                new Vector3(-8.5f, 0.08f, -8.6f), // Cottage entrance threshold
                new Vector3(-8.7f, 0.08f, -7.8f),
                new Vector3(-8.3f, 0.08f, -7.0f),
                new Vector3(-8.6f, 0.08f, -6.2f),
                new Vector3(-8.2f, 0.08f, -5.5f),
                new Vector3(-8.0f, 0.08f, -5.0f), // Cellar access door threshold
            };

            for (int i = 0; i < trailWaypoints.Length; i++)
            {
                var step = GameObject.CreatePrimitive(PrimitiveType.Quad);
                step.name = $"Wet Footprint Step {i + 1}";
                step.transform.SetParent(footprintTrail, false);
                step.transform.position = trailWaypoints[i];
                step.transform.rotation = Quaternion.Euler(90f, (i % 2 == 0 ? 5f : -8f), 0);
                step.transform.localScale = new Vector3(0.24f, 0.55f, 1f);
                step.GetComponent<Renderer>().sharedMaterial = matWetFootprint;
                Object.DestroyImmediate(step.GetComponent<Collider>());
            }
        }

        // =========================================================================
        // 2. TOWER VERTICAL INTERIOR & MID-LEVEL STUDY MASTER CONSTRUCTION
        // =========================================================================
        private static void BuildTowerInteriorAndStudy(Transform parent)
        {
            // 1. Deactivate legacy primitive blockout from ReferenceVisualPass and hiding spots if present
            foreach (string oldName in new[] { "Reference Visuals/Lighthouse/FirstFloor/ArchiveRoom", "Reference Visuals/Lighthouse/FirstFloor/Architecture", "ArchiveRoom" })
            {
                var go = GameObject.Find(oldName);
                if (go)
                {
                    foreach (var r in go.GetComponentsInChildren<Renderer>(true))
                        r.enabled = false;
                }
            }

            var lvl2Desk = GameObject.Find("level2-desk");
            if (lvl2Desk)
            {
                var r = lvl2Desk.GetComponent<Renderer>();
                if (r) r.enabled = false;
            }

            var towerRoot = new GameObject("Tower Interior & Mid-Level Study Master").transform;
            towerRoot.SetParent(parent, false);

            // A. Exposed Masonry Curving Walls (y = 3.2m to 7.2m, Radius approx 5.35m)
            var wallGroup = new GameObject("Curving Masonry Walls & Ceiling Joists").transform;
            wallGroup.SetParent(towerRoot, false);

            int wallSegments = 32;
            float towerRadius = 5.35f;
            for (int i = 0; i < wallSegments; i++)
            {
                float angle = i * (360f / wallSegments);
                float rad = angle * Mathf.Deg2Rad;
                Vector3 segPos = new Vector3(Mathf.Sin(rad) * towerRadius, 5.2f, Mathf.Cos(rad) * towerRadius);
                Quaternion segRot = Quaternion.Euler(0, angle, 0);

                CreatePrimitiveMesh($"Tower Wall Stone Arc {i}", PrimitiveType.Cube, wallGroup,
                    segPos, new Vector3(1.15f, 4.2f, 0.35f), segRot, matStone, false);

                // Heavy timber wall corbel supporting mezzanine deck above (only outside the stair wellhole!)
                if (i % 2 == 0)
                {
                    // Stair Flight 0 ascends along the North arc from angle 50° through 0° down to -110° (approx 40° to 250°)
                    // Omit corbels in this sector to ensure >= 2.4m vertical headroom over every step (Ref: Screenshot 19)
                    if (angle < 45f || angle > 250f)
                    {
                        Vector3 corbelPos = new Vector3(Mathf.Sin(rad) * (towerRadius - 0.28f), 3.12f, Mathf.Cos(rad) * (towerRadius - 0.28f));
                        CreatePrimitiveMesh($"Deck Corbel {i}", PrimitiveType.Cube, wallGroup,
                            corbelPos, new Vector3(0.24f, 0.36f, 0.55f), segRot, matWood, false);
                    }
                }
            }

            // Overhead Mezzanine Ring Structure & Framing (Framing opening to lantern deck above)
            float upperRingY = 6.85f;
            CreatePrimitiveMesh("Upper Lantern Access Ring", PrimitiveType.Cylinder, wallGroup,
                new Vector3(0, upperRingY, 0), new Vector3(5.2f, 0.12f, 5.2f), Quaternion.identity, matIron, false);

            // Circular inner aperture cutout framing
            for (int spoke = 0; spoke < 8; spoke++)
            {
                float sAngle = spoke * 45f;
                float sRad = sAngle * Mathf.Deg2Rad;
                Vector3 spokePos = new Vector3(Mathf.Sin(sRad) * 3.8f, upperRingY - 0.05f, Mathf.Cos(sRad) * 3.8f);
                CreatePrimitiveMesh($"Radial Ceiling Girder {spoke}", PrimitiveType.Cube, wallGroup,
                    spokePos, new Vector3(0.18f, 0.25f, 2.6f), Quaternion.Euler(0, sAngle, 0), matIron, false);
            }

            // B. Wooden Mezzanine Study Floor Deck & Wrought-Iron Curved Balustrade
            var deckGroup = new GameObject("Mezzanine Study Deck & Balustrade").transform;
            deckGroup.SetParent(towerRoot, false);

            // Hardwood floor platform with generous curved stairwell wellhole opening (Ref: Screenshot 19)
            // Covers the study zone (x in [-3.2f, 3.2f], z in [0.2f, 3.8f]) leaving the outer stairwell perimeter (r > 3.85m on North arc) 100% OPEN AIR
            CreatePrimitiveMesh("Mezzanine Study Floor Core", PrimitiveType.Cube, deckGroup,
                new Vector3(0f, 3.20f, 2.00f), new Vector3(6.4f, 0.12f, 3.6f), Quaternion.identity, matWood, true);

            // Arrival Landing Platform connecting stair top (angle -110°) smoothly into study floor
            CreatePrimitiveMesh("Stair Arrival Landing Deck", PrimitiveType.Cube, deckGroup,
                new Vector3(-3.85f, 3.20f, -0.60f), new Vector3(2.4f, 0.12f, 2.4f), Quaternion.identity, matWood, true);

            // Inner Curved Wrought-Iron Guardrail along void edge overlooking staircase (South perimeter of mezzanine)
            int railPosts = 24;
            for (int p = 0; p <= railPosts; p++)
            {
                float t = p / (float)railPosts;
                float px = Mathf.Lerp(-3.8f, 1.2f, t);
                float pz = 0.20f - 0.25f * Mathf.Sin(t * Mathf.PI);
                Vector3 postPos = new Vector3(px, 3.21f, pz);

                // Vertical baluster rod
                CreatePrimitiveMesh($"Baluster {p}", PrimitiveType.Cylinder, deckGroup,
                    postPos + Vector3.up * 0.52f, new Vector3(0.038f, 0.52f, 0.038f), Quaternion.identity, matIron, false);

                // Main posts with ornamental cap
                if (p == 0 || p == railPosts || p % 6 == 0)
                {
                    CreatePrimitiveMesh($"Main Post {p}", PrimitiveType.Cylinder, deckGroup,
                        postPos + Vector3.up * 0.55f, new Vector3(0.08f, 0.55f, 0.08f), Quaternion.identity, matIron, false);
                }

                // Curved top handrail segment
                if (p < railPosts)
                {
                    float nextT = (p + 1) / (float)railPosts;
                    float npx = Mathf.Lerp(-3.8f, 1.2f, nextT);
                    float npz = 0.20f - 0.25f * Mathf.Sin(nextT * Mathf.PI);
                    Vector3 nextPos = new Vector3(npx, 3.21f, npz);
                    Vector3 mid = (postPos + nextPos) * 0.5f + Vector3.up * 1.05f;
                    Vector3 diff = nextPos - postPos;
                    float len = diff.magnitude;
                    Quaternion rot = Quaternion.LookRotation(diff);
                    CreatePrimitiveMesh($"Handrail Seg {p}", PrimitiveType.Cylinder, deckGroup,
                        mid, new Vector3(0.055f, len * 0.5f, 0.055f), rot * Quaternion.Euler(90f, 0, 0), matIron, false);
                }
            }

            // C. Mid-Level Study Platform: Persian Rug & Solid Mahogany Writing Desk
            var studyGroup = new GameObject("Study Writing Desk & Instruments").transform;
            studyGroup.SetParent(towerRoot, false);

            // Persian Carpet / Area Rug (sitting on top of wooden floor)
            CreatePrimitiveMesh("Persian Study Rug", PrimitiveType.Cube, studyGroup,
                new Vector3(0f, 3.27f, 1.75f), new Vector3(4.2f, 0.015f, 3.2f), Quaternion.identity, matRug, false);

            // Mahogany Desk Center Coordinates
            Vector3 deskPos = new Vector3(0f, 3.21f, 1.60f);

            // Solid Mahogany Desktop
            CreatePrimitiveMesh("Mahogany Desk Top", PrimitiveType.Cube, studyGroup,
                deskPos + new Vector3(0f, 0.75f, 0f), new Vector3(2.0f, 0.08f, 1.05f), Quaternion.identity, matMahogany, true);

            // Left & Right Drawered Pedestals
            CreatePrimitiveMesh("Desk Pedestal Left", PrimitiveType.Cube, studyGroup,
                deskPos + new Vector3(-0.74f, 0.36f, 0f), new Vector3(0.48f, 0.70f, 0.92f), Quaternion.identity, matMahogany, true);
            CreatePrimitiveMesh("Desk Pedestal Right", PrimitiveType.Cube, studyGroup,
                deskPos + new Vector3(0.74f, 0.36f, 0f), new Vector3(0.48f, 0.70f, 0.92f), Quaternion.identity, matMahogany, true);
            CreatePrimitiveMesh("Desk Modesty Panel", PrimitiveType.Cube, studyGroup,
                deskPos + new Vector3(0f, 0.38f, 0.38f), new Vector3(1.0f, 0.66f, 0.04f), Quaternion.identity, matMahogany, false);

            // Solid Brass Drawer Pulls
            for (int d = 0; d < 3; d++)
            {
                float dy = 0.58f - d * 0.22f;
                CreatePrimitiveMesh($"Brass Pull L{d}", PrimitiveType.Cube, studyGroup,
                    deskPos + new Vector3(-0.74f, dy, -0.47f), new Vector3(0.14f, 0.035f, 0.025f), Quaternion.identity, matBrass, false);
                CreatePrimitiveMesh($"Brass Pull R{d}", PrimitiveType.Cube, studyGroup,
                    deskPos + new Vector3(0.74f, dy, -0.47f), new Vector3(0.14f, 0.035f, 0.025f), Quaternion.identity, matBrass, false);
            }

            // D. Desk Clues & High-Detail Props
            // 1. Sea Charts: Open Navigation Chart of Harrow Rock & Shoals
            CreatePrimitiveMesh("Nautical Chart Spread", PrimitiveType.Quad, studyGroup,
                deskPos + new Vector3(0.08f, 0.795f, -0.05f), new Vector3(0.78f, 0.55f, 1f), Quaternion.Euler(90f, 4f, 0f), matNauticalChart, false);

            // Rolled Parchment Charts in brass retention rings
            CreatePrimitiveMesh("Rolled Chart 1", PrimitiveType.Cylinder, studyGroup,
                deskPos + new Vector3(-0.72f, 0.83f, 0.22f), new Vector3(0.07f, 0.42f, 0.07f), Quaternion.Euler(0, 32f, 90f), matParchment, false);
            CreatePrimitiveMesh("Rolled Chart 2", PrimitiveType.Cylinder, studyGroup,
                deskPos + new Vector3(-0.75f, 0.88f, 0.18f), new Vector3(0.06f, 0.38f, 0.06f), Quaternion.Euler(0, -15f, 90f), matParchment, false);
            CreatePrimitiveMesh("Brass Chart Cylinder", PrimitiveType.Cylinder, studyGroup,
                deskPos + new Vector3(-0.80f, 0.82f, -0.20f), new Vector3(0.08f, 0.45f, 0.08f), Quaternion.Euler(0, 78f, 90f), matBrass, false);

            // 2. Brass Navigational Calipers / Drafting Dividers Resting on the Chart
            Vector3 caliperPos = deskPos + new Vector3(0.16f, 0.803f, -0.06f);
            CreatePrimitiveMesh("Calipers Hinge Disk", PrimitiveType.Cylinder, studyGroup,
                caliperPos, new Vector3(0.045f, 0.008f, 0.045f), Quaternion.identity, matBrass, false);
            CreatePrimitiveMesh("Calipers Leg Left", PrimitiveType.Cylinder, studyGroup,
                caliperPos + new Vector3(-0.045f, 0.003f, 0.09f), new Vector3(0.010f, 0.10f, 0.010f), Quaternion.Euler(0, -18f, 90f), matBrass, false);
            CreatePrimitiveMesh("Calipers Leg Right", PrimitiveType.Cylinder, studyGroup,
                caliperPos + new Vector3(0.045f, 0.003f, 0.09f), new Vector3(0.010f, 0.10f, 0.010f), Quaternion.Euler(0, 18f, 90f), matBrass, false);
            CreatePrimitiveMesh("Brass Parallel Ruler", PrimitiveType.Cube, studyGroup,
                deskPos + new Vector3(-0.10f, 0.801f, -0.28f), new Vector3(0.42f, 0.012f, 0.065f), Quaternion.Euler(0, 6f, 0), matBrass, false);

            // 3. Open Keeper's Logbook with handwritten entries
            Vector3 logbookPos = deskPos + new Vector3(-0.38f, 0.801f, -0.08f);
            CreatePrimitiveMesh("Logbook Cover", PrimitiveType.Cube, studyGroup,
                logbookPos, new Vector3(0.55f, 0.018f, 0.38f), Quaternion.Euler(0, 12f, 0), matWood, false);
            CreatePrimitiveMesh("Logbook Left Page", PrimitiveType.Cube, studyGroup,
                logbookPos + new Vector3(-0.13f, 0.012f, 0f), new Vector3(0.25f, 0.012f, 0.34f), Quaternion.Euler(0, 12f, 0), matLedger, false);
            CreatePrimitiveMesh("Logbook Right Page", PrimitiveType.Cube, studyGroup,
                logbookPos + new Vector3(0.13f, 0.012f, 0f), new Vector3(0.25f, 0.012f, 0.34f), Quaternion.Euler(0, 12f, 0), matLedger, false);

            // 4. Unlit Vintage Brass Oil Lamp with Clear Glass Chimney
            Vector3 lampPos = deskPos + new Vector3(0.68f, 0.795f, 0.22f);
            CreatePrimitiveMesh("Oil Lamp Brass Base", PrimitiveType.Cylinder, studyGroup,
                lampPos + Vector3.up * 0.04f, new Vector3(0.18f, 0.08f, 0.18f), Quaternion.identity, matBrass, false);
            CreatePrimitiveMesh("Oil Lamp Font Reservoir", PrimitiveType.Sphere, studyGroup,
                lampPos + Vector3.up * 0.14f, new Vector3(0.16f, 0.12f, 0.16f), Quaternion.identity, matBrass, false);
            CreatePrimitiveMesh("Oil Lamp Burner Collar", PrimitiveType.Cylinder, studyGroup,
                lampPos + Vector3.up * 0.22f, new Vector3(0.08f, 0.05f, 0.08f), Quaternion.identity, matBrass, false);
            CreatePrimitiveMesh("Oil Lamp Glass Chimney", PrimitiveType.Cylinder, studyGroup,
                lampPos + Vector3.up * 0.36f, new Vector3(0.10f, 0.25f, 0.10f), Quaternion.identity, matSpecimenJar, false);
            CreatePrimitiveMesh("Oil Lamp Cotton Wick", PrimitiveType.Cylinder, studyGroup,
                lampPos + Vector3.up * 0.25f, new Vector3(0.02f, 0.06f, 0.02f), Quaternion.identity, matWood, false);

            // 5. Terrestrial Globe on Brass Armillary Stand
            Vector3 globePos = deskPos + new Vector3(-0.65f, 0.795f, -0.22f);
            CreatePrimitiveMesh("Globe Wooden Base", PrimitiveType.Cylinder, studyGroup,
                globePos + Vector3.up * 0.025f, new Vector3(0.18f, 0.04f, 0.18f), Quaternion.identity, matMahogany, false);
            CreatePrimitiveMesh("Globe Brass Axis Stem", PrimitiveType.Cylinder, studyGroup,
                globePos + Vector3.up * 0.12f, new Vector3(0.03f, 0.15f, 0.03f), Quaternion.identity, matBrass, false);
            CreatePrimitiveMesh("Globe Sphere", PrimitiveType.Sphere, studyGroup,
                globePos + Vector3.up * 0.25f, new Vector3(0.24f, 0.24f, 0.24f), Quaternion.Euler(23.5f, 35f, 0f), matParchment, false);
            CreatePrimitiveMesh("Globe Meridian Ring", PrimitiveType.Cylinder, studyGroup,
                globePos + Vector3.up * 0.25f, new Vector3(0.28f, 0.015f, 0.28f), Quaternion.Euler(0, 0, 90f), matBrass, false);

            // 6. Glazed Dark Stoneware Inkwell & Feather Quill
            var matInkwell = EnsureMaterial("MaritimeInkwellGlaze", "Universal Render Pipeline/Lit");
            matInkwell.SetColor("_BaseColor", new Color(0.12f, 0.13f, 0.15f));
            matInkwell.SetFloat("_Smoothness", 0.92f);
            matInkwell.SetFloat("_Metallic", 0.15f);

            Vector3 inkPos = deskPos + new Vector3(0.36f, 0.795f, -0.25f);
            CreatePrimitiveMesh("Ceramic Inkwell", PrimitiveType.Cylinder, studyGroup,
                inkPos + Vector3.up * 0.035f, new Vector3(0.075f, 0.07f, 0.075f), Quaternion.identity, matInkwell, false);
            CreatePrimitiveMesh("Goose Quill", PrimitiveType.Cylinder, studyGroup,
                inkPos + new Vector3(-0.02f, 0.15f, 0.02f), new Vector3(0.012f, 0.24f, 0.012f), Quaternion.Euler(24f, 0, 15f), matParchment, false);

            // 7. Stack of Maritime Volumes on Corner of Desk
            Vector3 bookStackPos = deskPos + new Vector3(0.66f, 0.795f, -0.22f);
            CreatePrimitiveMesh("Port Almanac 1", PrimitiveType.Cube, studyGroup,
                bookStackPos + Vector3.up * 0.025f, new Vector3(0.28f, 0.05f, 0.36f), Quaternion.Euler(0, 8f, 0), matBookRed, false);
            CreatePrimitiveMesh("Port Almanac 2", PrimitiveType.Cube, studyGroup,
                bookStackPos + Vector3.up * 0.075f, new Vector3(0.26f, 0.045f, 0.34f), Quaternion.Euler(0, -6f, 0), matBookBlue, false);
            CreatePrimitiveMesh("Admiralty Ledger 3", PrimitiveType.Cube, studyGroup,
                bookStackPos + Vector3.up * 0.12f, new Vector3(0.24f, 0.04f, 0.32f), Quaternion.Euler(0, 14f, 0), matParchment, false);

            // 8. Carved Maritime Desk Armchair (Facing Desk, slightly turned)
            Vector3 chairPos = deskPos + new Vector3(0.18f, 0.06f, -0.68f);
            Quaternion chairRot = Quaternion.Euler(0, 15f, 0);
            CreatePrimitiveMesh("Armchair Seat", PrimitiveType.Cube, studyGroup,
                chairPos + new Vector3(0f, 0.44f, 0f), new Vector3(0.62f, 0.08f, 0.58f), chairRot, matMahogany, true);
            // 4 Turned Legs
            CreatePrimitiveMesh("Chair Leg FL", PrimitiveType.Cylinder, studyGroup,
                chairPos + chairRot * new Vector3(-0.25f, 0.22f, 0.23f), new Vector3(0.06f, 0.42f, 0.06f), chairRot, matMahogany, false);
            CreatePrimitiveMesh("Chair Leg FR", PrimitiveType.Cylinder, studyGroup,
                chairPos + chairRot * new Vector3(0.25f, 0.22f, 0.23f), new Vector3(0.06f, 0.42f, 0.06f), chairRot, matMahogany, false);
            CreatePrimitiveMesh("Chair Leg BL", PrimitiveType.Cylinder, studyGroup,
                chairPos + chairRot * new Vector3(-0.25f, 0.22f, -0.23f), new Vector3(0.06f, 0.42f, 0.06f), chairRot, matMahogany, false);
            CreatePrimitiveMesh("Chair Leg BR", PrimitiveType.Cylinder, studyGroup,
                chairPos + chairRot * new Vector3(0.25f, 0.22f, -0.23f), new Vector3(0.06f, 0.42f, 0.06f), chairRot, matMahogany, false);
            // Backrest & Armrests
            CreatePrimitiveMesh("Chair Back Top Rail", PrimitiveType.Cube, studyGroup,
                chairPos + chairRot * new Vector3(0f, 1.15f, -0.24f), new Vector3(0.58f, 0.12f, 0.06f), chairRot, matMahogany, false);
            for (int s = -2; s <= 2; s++)
            {
                CreatePrimitiveMesh($"Chair Slat {s}", PrimitiveType.Cube, studyGroup,
                    chairPos + chairRot * new Vector3(s * 0.11f, 0.80f, -0.24f), new Vector3(0.045f, 0.65f, 0.025f), chairRot, matMahogany, false);
            }
            CreatePrimitiveMesh("Chair Arm L", PrimitiveType.Cube, studyGroup,
                chairPos + chairRot * new Vector3(-0.28f, 0.72f, 0.02f), new Vector3(0.06f, 0.04f, 0.48f), chairRot, matMahogany, false);
            CreatePrimitiveMesh("Chair Arm R", PrimitiveType.Cube, studyGroup,
                chairPos + chairRot * new Vector3(0.28f, 0.72f, 0.02f), new Vector3(0.06f, 0.04f, 0.48f), chairRot, matMahogany, false);

            // E. Tall Curved Wooden Bookcases Packed with Maritime Volumes & Specimen Jars (North Wall)
            var shelfGroup = new GameObject("Curved North Wall Bookcases & Library").transform;
            shelfGroup.SetParent(towerRoot, false);

            float bz = 4.35f;
            // Uprights
            CreatePrimitiveMesh("Bookcase Upright L", PrimitiveType.Cube, shelfGroup,
                new Vector3(-1.85f, 4.55f, bz), new Vector3(0.12f, 2.70f, 0.48f), Quaternion.identity, matWood, true);
            CreatePrimitiveMesh("Bookcase Upright R", PrimitiveType.Cube, shelfGroup,
                new Vector3(1.85f, 4.55f, bz), new Vector3(0.12f, 2.70f, 0.48f), Quaternion.identity, matWood, true);
            CreatePrimitiveMesh("Bookcase Divider 1", PrimitiveType.Cube, shelfGroup,
                new Vector3(-0.62f, 4.55f, bz), new Vector3(0.09f, 2.70f, 0.45f), Quaternion.identity, matWood, false);
            CreatePrimitiveMesh("Bookcase Divider 2", PrimitiveType.Cube, shelfGroup,
                new Vector3(0.62f, 4.55f, bz), new Vector3(0.09f, 2.70f, 0.45f), Quaternion.identity, matWood, false);
            CreatePrimitiveMesh("Bookcase Crown", PrimitiveType.Cube, shelfGroup,
                new Vector3(0f, 5.92f, bz), new Vector3(3.95f, 0.14f, 0.55f), Quaternion.identity, matWood, false);

            // Shelves 1 to 4
            float[] shelfHeights = new[] { 3.32f, 3.95f, 4.60f, 5.25f };
            for (int s = 0; s < shelfHeights.Length; s++)
            {
                CreatePrimitiveMesh($"Bookcase Shelf {s + 1}", PrimitiveType.Cube, shelfGroup,
                    new Vector3(0f, shelfHeights[s], bz), new Vector3(3.68f, 0.06f, 0.45f), Quaternion.identity, matWood, false);
            }

            // Books on Shelf 2 (y = 3.95m): 28 dense leatherbound volumes
            Material[] bookMats = new[] { matBookRed, matBookGreen, matBookBlue, matBookGold };
            for (int b = 0; b < 28; b++)
            {
                float bx = -1.70f + b * 0.125f;
                if (Mathf.Abs(bx - (-0.62f)) < 0.08f || Mathf.Abs(bx - 0.62f) < 0.08f) continue;
                float bh = 0.25f + (b % 5) * 0.035f;
                float bd = 0.24f + (b % 3) * 0.02f;
                Material bm = bookMats[b % bookMats.Length];
                CreatePrimitiveMesh($"Book S2_{b}", PrimitiveType.Cube, shelfGroup,
                    new Vector3(bx, 3.98f + bh * 0.5f, bz - 0.02f), new Vector3(0.085f, bh, bd), Quaternion.identity, bm, false);
                CreatePrimitiveMesh($"Spine S2_{b}", PrimitiveType.Cube, shelfGroup,
                    new Vector3(bx, 3.98f + bh * 0.5f, bz - 0.14f), new Vector3(0.08f, bh * 0.88f, 0.02f), Quaternion.identity, matBookGold, false);
            }

            // Books on Shelf 3 (y = 4.60m): 26 dense maritime almanacs
            for (int b = 0; b < 26; b++)
            {
                float bx = -1.65f + b * 0.13f;
                if (Mathf.Abs(bx - (-0.62f)) < 0.08f || Mathf.Abs(bx - 0.62f) < 0.08f) continue;
                float bh = 0.22f + ((b * 2) % 4) * 0.03f;
                Material bm = bookMats[(b * 3) % bookMats.Length];
                CreatePrimitiveMesh($"Book S3_{b}", PrimitiveType.Cube, shelfGroup,
                    new Vector3(bx, 4.63f + bh * 0.5f, bz - 0.02f), new Vector3(0.09f, bh, 0.22f), Quaternion.identity, bm, false);
            }

            // Shelf 1 (y = 3.32m): Heavy port ledgers & chart cylinders
            CreatePrimitiveMesh("Harbor Port Ledgers", PrimitiveType.Cube, shelfGroup,
                new Vector3(-1.15f, 3.52f, bz - 0.02f), new Vector3(0.72f, 0.32f, 0.32f), Quaternion.identity, matParchment, false);
            CreatePrimitiveMesh("Logbook Stacks", PrimitiveType.Cube, shelfGroup,
                new Vector3(1.15f, 3.55f, bz - 0.02f), new Vector3(0.80f, 0.36f, 0.30f), Quaternion.identity, matWood, false);

            // Shelf 4 (y = 5.25m): 6 Glass Specimen Jars with preserved marine specimens
            float[] jarX = new[] { -1.35f, -0.92f, -0.32f, 0.32f, 0.92f, 1.35f };
            float[] jarH = new[] { 0.34f, 0.28f, 0.36f, 0.30f, 0.35f, 0.29f };
            float[] jarR = new[] { 0.12f, 0.09f, 0.13f, 0.10f, 0.12f, 0.09f };
            for (int j = 0; j < jarX.Length; j++)
            {
                Vector3 jPos = new Vector3(jarX[j], 5.28f, bz - 0.05f);
                CreatePrimitiveMesh($"Specimen Jar {j}", PrimitiveType.Cylinder, shelfGroup,
                    jPos + Vector3.up * jarH[j] * 0.5f, new Vector3(jarR[j] * 2f, jarH[j] * 0.5f, jarR[j] * 2f), Quaternion.identity, matSpecimenJar, false);
                CreatePrimitiveMesh($"Jar Cork {j}", PrimitiveType.Cylinder, shelfGroup,
                    jPos + Vector3.up * (jarH[j] + 0.03f), new Vector3(jarR[j] * 1.5f, 0.03f, jarR[j] * 1.5f), Quaternion.identity, matWood, false);
                Material specMat = (j % 2 == 0) ? matBookGreen : matBookGold;
                CreatePrimitiveMesh($"Preserved Specimen {j}", PrimitiveType.Sphere, shelfGroup,
                    jPos + Vector3.up * (jarH[j] * 0.45f), new Vector3(jarR[j] * 1.1f, jarH[j] * 0.4f, jarR[j] * 1.1f), Quaternion.Euler(15f, 30f * j, 0), specMat, false);
            }

            // F. Overhead Lighting & Atmospheric Dust Motes
            var pendantLightGo = new GameObject("Study Low-Hanging Industrial Pendant Bulb");
            pendantLightGo.transform.SetParent(towerRoot, false);
            pendantLightGo.transform.position = deskPos + new Vector3(0f, 1.70f, 0f); // y = 4.91m

            // Chain connecting fixture to upper girder
            CreatePrimitiveMesh("Lamp Suspension Chain", PrimitiveType.Cylinder, pendantLightGo.transform,
                new Vector3(0f, 0.95f, 0f), new Vector3(0.035f, 0.95f, 0.035f), Quaternion.identity, matIron, false);

            // Instantiate Industrial Hanging Lamp FBX model
            var lampPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Art/Models/IndustrialHangingLamp/IndustrialHangingLamp.fbx");
            if (lampPrefab)
            {
                var lampInstance = Object.Instantiate(lampPrefab, pendantLightGo.transform);
                lampInstance.name = "IndustrialHangingLamp_Mesh";
                lampInstance.transform.localPosition = new Vector3(0, -0.32f, 0);
                lampInstance.transform.localRotation = Quaternion.Euler(90f, 0, 0);
                lampInstance.transform.localScale = Vector3.one * 55.0f; // Scale from FBX units to realistic 0.55m fixture

                var lampMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Models/IndustrialHangingLamp/M_IndustrialLamp.mat");
                if (lampMat)
                {
                    foreach (var rend in lampInstance.GetComponentsInChildren<Renderer>())
                        rend.sharedMaterial = lampMat;
                }
            }
            else
            {
                CreatePrimitiveMesh("Lamp Metal Hood", PrimitiveType.Cylinder, pendantLightGo.transform,
                    new Vector3(0f, 0.12f, 0f), new Vector3(0.55f, 0.12f, 0.55f), Quaternion.identity, matIron, false);
                CreatePrimitiveMesh("Lamp Glowing Bulb", PrimitiveType.Sphere, pendantLightGo.transform,
                    new Vector3(0f, -0.04f, 0f), new Vector3(0.18f, 0.22f, 0.18f), Quaternion.identity, matLanternGlow, false);
            }

            // Warm Amber Spotlight Illuminating Study Writing Desk
            var pendantSpot = pendantLightGo.AddComponent<Light>();
            pendantSpot.type = LightType.Spot;
            pendantSpot.spotAngle = 68f;
            pendantSpot.innerSpotAngle = 32f;
            pendantSpot.range = 6.8f;
            pendantSpot.intensity = 3.8f;
            pendantSpot.color = new Color(1.0f, 0.74f, 0.40f); // 2300K Rich Amber Gold
            pendantSpot.shadows = LightShadows.Soft;

            // Ambient Fill Point Light
            var pendantFill = new GameObject("Study Ambient Fill Light");
            pendantFill.transform.SetParent(pendantLightGo.transform, false);
            var pFill = pendantFill.AddComponent<Light>();
            pFill.type = LightType.Point;
            pFill.range = 4.8f;
            pFill.intensity = 1.25f;
            pFill.color = new Color(1.0f, 0.72f, 0.38f);
            pFill.shadows = LightShadows.None;

            // Atmospheric Dust Motes Emitter attached to light fixture
            var motesEmitter = pendantLightGo.AddComponent<DustMotesEmitter>();
            motesEmitter.SetupParticles();

            // G. Caged Bulkhead Sconces along Spiral Stair Steps (Casting raking vertical shadows on stone)
            Vector3[] bulkheadPositions = new[]
            {
                new Vector3(4.35f, 2.10f, 1.10f),
                new Vector3(3.20f, 4.40f, 3.80f),
                new Vector3(-2.60f, 6.70f, 4.10f)
            };

            for (int b = 0; b < bulkheadPositions.Length; b++)
            {
                var bhGroup = new GameObject($"Caged Bulkhead Sconce {b + 1}");
                bhGroup.transform.SetParent(towerRoot, false);
                bhGroup.transform.position = bulkheadPositions[b];

                CreatePrimitiveMesh("Bulkhead Baseplate", PrimitiveType.Cube, bhGroup.transform,
                    Vector3.zero, new Vector3(0.24f, 0.32f, 0.08f), Quaternion.identity, matIron, false);

                CreatePrimitiveMesh("Bulkhead Cage Guard", PrimitiveType.Cylinder, bhGroup.transform,
                    new Vector3(0f, 0f, 0.12f), new Vector3(0.18f, 0.14f, 0.18f), Quaternion.Euler(90f, 0, 0), matIron, false);

                CreatePrimitiveMesh("Bulkhead Bulb", PrimitiveType.Sphere, bhGroup.transform,
                    new Vector3(0f, 0f, 0.10f), new Vector3(0.12f, 0.14f, 0.12f), Quaternion.identity, matLanternGlow, false);

                var bhLight = bhGroup.AddComponent<Light>();
                bhLight.type = LightType.Point;
                bhLight.color = new Color(1.0f, 0.68f, 0.32f);
                bhLight.intensity = 1.6f;
                bhLight.range = 5.2f;
                bhLight.shadows = LightShadows.Soft;
            }
        }

        private static GameObject CreatePrimitiveMesh(string name, PrimitiveType type, Transform parent, Vector3 localPos, Vector3 localScale, Quaternion localRot, Material mat, bool collider = false)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = localRot;
            go.transform.localScale = localScale;
            if (mat != null)
            {
                var rend = go.GetComponent<Renderer>();
                if (rend) rend.sharedMaterial = mat;
            }
            if (!collider)
            {
                var col = go.GetComponent<Collider>();
                if (col) Object.DestroyImmediate(col);
            }
            return go;
        }

        // =========================================================================
        // 3. THE UNDERGROUND FLOOR / CELLAR MASTER CONSTRUCTION
        // =========================================================================
        private static void BuildUndergroundCellar(Transform parent)
        {
            // Hide legacy greybox cellar primitives
            foreach (string gbName in new[] { "Cellar Floor", "Hidden Room Floor", "Flooded Corridor", "Drain" })
            {
                var go = GameObject.Find(gbName);
                if (go)
                {
                    var r = go.GetComponent<Renderer>();
                    if (r) r.enabled = false;
                }
            }
            for (int i = 0; i < 6; i++)
            {
                var archL = GameObject.Find($"Cellar Arch Left {i}");
                if (archL) { var r = archL.GetComponent<Renderer>(); if (r) r.enabled = false; }
                var archT = GameObject.Find($"Cellar Arch Top {i}");
                if (archT) { var r = archT.GetComponent<Renderer>(); if (r) r.enabled = false; }
            }

            var cellarRoot = new GameObject("Underground Cellar & Secret Room Master").transform;
            cellarRoot.SetParent(parent, false);

            // A. Entrance from 1st Floor / Ground Floor (Door, Plaque, Coats, Boots, Lantern)
            Vector3 cellarDoorPos = new Vector3(-8.0f, 1.4f, -5.0f);
            var entranceGroup = new GameObject("Cellar Entrance Vestibule").transform;
            entranceGroup.SetParent(cellarRoot, false);

            // "WARNING: Cellar Access" enamel plaque mounted beside door
            var warningPlaque = GameObject.CreatePrimitive(PrimitiveType.Quad);
            warningPlaque.name = "Warning Cellar Access Plaque";
            warningPlaque.transform.SetParent(entranceGroup, false);
            warningPlaque.transform.position = cellarDoorPos + new Vector3(-1.15f, 0.15f, -0.15f);
            warningPlaque.transform.localScale = new Vector3(0.38f, 0.48f, 1f);
            warningPlaque.transform.rotation = Quaternion.Euler(0, 0, 0);
            warningPlaque.GetComponent<Renderer>().sharedMaterial = matWarningSign;
            Object.DestroyImmediate(warningPlaque.GetComponent<Collider>());

            // Wooden Coat Pegs with Hanging Oilskin Keeper's Slickers
            var coatRack = GameObject.CreatePrimitive(PrimitiveType.Cube);
            coatRack.name = "Wooden Coat Rack Bar";
            coatRack.transform.SetParent(entranceGroup, false);
            coatRack.transform.position = cellarDoorPos + new Vector3(1.35f, 0.35f, -0.12f);
            coatRack.transform.localScale = new Vector3(1.10f, 0.12f, 0.05f);
            coatRack.GetComponent<Renderer>().sharedMaterial = matWood;
            Object.DestroyImmediate(coatRack.GetComponent<Collider>());

            // 3 Hanging Coats
            var yellowSlickerMat = AssetDatabase.LoadAssetAtPath<Material>($"{MatRoot}/Yellow Oilskin.mat") ?? matWood;
            var darkCoatMat = AssetDatabase.LoadAssetAtPath<Material>($"{MatRoot}/Dark Wool Navy.mat") ?? matIron;

            var c1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            c1.name = "Hanging Keeper Slicker 1";
            c1.transform.SetParent(entranceGroup, false);
            c1.transform.position = coatRack.transform.position + new Vector3(-0.35f, -0.55f, 0.08f);
            c1.transform.localScale = new Vector3(0.28f, 0.95f, 0.18f);
            c1.GetComponent<Renderer>().sharedMaterial = yellowSlickerMat;
            Object.DestroyImmediate(c1.GetComponent<Collider>());

            var c2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            c2.name = "Hanging Keeper Slicker 2";
            c2.transform.SetParent(entranceGroup, false);
            c2.transform.position = coatRack.transform.position + new Vector3(0.05f, -0.58f, 0.09f);
            c2.transform.localScale = new Vector3(0.30f, 1.02f, 0.20f);
            c2.GetComponent<Renderer>().sharedMaterial = darkCoatMat;
            Object.DestroyImmediate(c2.GetComponent<Collider>());

            // Pair of wet boots on floor
            for (int b = -1; b <= 1; b += 2)
            {
                var boot = GameObject.CreatePrimitive(PrimitiveType.Cube);
                boot.name = $"Keeper Muddy Boot {b}";
                boot.transform.SetParent(entranceGroup, false);
                boot.transform.position = cellarDoorPos + new Vector3(1.35f + b * 0.14f, -1.35f, 0.18f);
                boot.transform.localScale = new Vector3(0.12f, 0.30f, 0.24f);
                boot.GetComponent<Renderer>().sharedMaterial = matIron;
                Object.DestroyImmediate(boot.GetComponent<Collider>());
            }

            // Storm Lantern resting on floor beside doorway
            var floorLantern = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            floorLantern.name = "Vestibule Floor Storm Lantern";
            floorLantern.transform.SetParent(entranceGroup, false);
            floorLantern.transform.position = cellarDoorPos + new Vector3(-0.85f, -1.25f, 0.25f);
            floorLantern.transform.localScale = new Vector3(0.18f, 0.35f, 0.18f);
            floorLantern.GetComponent<Renderer>().sharedMaterial = matLanternGlow;
            Object.DestroyImmediate(floorLantern.GetComponent<Collider>());

            var floorLanternLight = new GameObject("Floor Lantern Light");
            floorLanternLight.transform.SetParent(floorLantern.transform, false);
            var fll = floorLanternLight.AddComponent<Light>();
            fll.type = LightType.Point;
            fll.color = new Color(1.0f, 0.65f, 0.28f);
            fll.intensity = 1.4f;
            fll.range = 3.5f;

            // B. Deep Stone Stairwell Spiraling into Bedrock down to y = -4.95m
            var stairGroup = new GameObject("Deep Stone Stairwell").transform;
            stairGroup.SetParent(cellarRoot, false);

            const int steps = 24;
            for (int s = 0; s < steps; s++)
            {
                float t = s / (float)steps;
                float sy = Mathf.Lerp(0f, -4.95f, t);
                float angle = s * 13.5f * Mathf.Deg2Rad;
                float sx = -8.0f + Mathf.Sin(angle) * 1.5f;
                float sz = -4.8f - Mathf.Cos(angle) * 1.5f;

                var step = GameObject.CreatePrimitive(PrimitiveType.Cube);
                step.name = $"Deep Stone Step {s}";
                step.transform.SetParent(stairGroup, false);
                step.transform.position = new Vector3(sx, sy, sz);
                step.transform.localScale = new Vector3(1.15f, 0.20f, 0.45f);
                step.transform.rotation = Quaternion.Euler(0, -angle * Mathf.Rad2Deg, 0);
                step.GetComponent<Renderer>().sharedMaterial = matStone;

                // Wet glistening footprint on step
                if (s % 2 == 0)
                {
                    var fp = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    fp.name = $"Stair Footprint {s}";
                    fp.transform.SetParent(step.transform, false);
                    fp.transform.localPosition = new Vector3(0, 0.52f, 0);
                    fp.transform.rotation = Quaternion.Euler(90f, 0, 0);
                    fp.transform.localScale = new Vector3(0.35f, 0.75f, 1f);
                    fp.GetComponent<Renderer>().sharedMaterial = matWetFootprint;
                    Object.DestroyImmediate(fp.GetComponent<Collider>());
                }
            }

            // Bare Hanging Incandescent Bulb in Deep Stairwell
            var bulbGroup = new GameObject("Dangling Stairwell Bulb").transform;
            bulbGroup.SetParent(stairGroup, false);
            bulbGroup.position = new Vector3(-8.0f, -2.2f, -4.8f);

            var cord = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cord.name = "Bulb Cord";
            cord.transform.SetParent(bulbGroup, false);
            cord.transform.localPosition = new Vector3(0, 0.65f, 0);
            cord.transform.localScale = new Vector3(0.02f, 0.65f, 0.02f);
            cord.GetComponent<Renderer>().sharedMaterial = matIron;
            Object.DestroyImmediate(cord.GetComponent<Collider>());

            var bulb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bulb.name = "Bare Filament Bulb";
            bulb.transform.SetParent(bulbGroup, false);
            bulb.transform.localPosition = Vector3.zero;
            bulb.transform.localScale = Vector3.one * 0.16f;
            bulb.GetComponent<Renderer>().sharedMaterial = matLanternGlow;
            Object.DestroyImmediate(bulb.GetComponent<Collider>());

            var bulbLight = new GameObject("Bulb Light");
            bulbLight.transform.SetParent(bulbGroup, false);
            var bl = bulbLight.AddComponent<Light>();
            bl.type = LightType.Point;
            bl.color = new Color(1.0f, 0.72f, 0.35f);
            bl.intensity = 2.2f;
            bl.range = 6.5f;
            bl.shadows = LightShadows.Soft;

            // C. Low Subterranean Corridor (Romanesque Stone Arches, Enclosed Walls, Puddles)
            var corridorGroup = new GameObject("Low Subterranean Corridor").transform;
            corridorGroup.SetParent(cellarRoot, false);

            Vector3 corridorCenter = new Vector3(-7.5f, -3.2f, -5.0f);

            // Flagstone floor
            var cellarFloor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cellarFloor.name = "Cellar Flagstone Floor";
            cellarFloor.transform.SetParent(corridorGroup, false);
            cellarFloor.transform.position = corridorCenter;
            cellarFloor.transform.localScale = new Vector3(10.0f, 0.15f, 4.2f);
            cellarFloor.GetComponent<Renderer>().sharedMaterial = matStone;

            // Arched stone ceiling vault
            var ceilingVault = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ceilingVault.name = "Cellar Stone Vault Ceiling";
            ceilingVault.transform.SetParent(corridorGroup, false);
            ceilingVault.transform.position = corridorCenter + new Vector3(0, 2.6f, 0);
            ceilingVault.transform.localScale = new Vector3(10.0f, 0.20f, 4.2f);
            ceilingVault.GetComponent<Renderer>().sharedMaterial = matStone;

            // Enclosing corridor side walls
            var wallN = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallN.name = "Corridor Wall North";
            wallN.transform.SetParent(corridorGroup, false);
            wallN.transform.position = corridorCenter + new Vector3(0, 1.3f, 2.1f);
            wallN.transform.localScale = new Vector3(10.0f, 2.6f, 0.2f);
            wallN.GetComponent<Renderer>().sharedMaterial = matStone;

            var wallS = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallS.name = "Corridor Wall South";
            wallS.transform.SetParent(corridorGroup, false);
            wallS.transform.position = corridorCenter + new Vector3(0, 1.3f, -2.1f);
            wallS.transform.localScale = new Vector3(10.0f, 2.6f, 0.2f);
            wallS.GetComponent<Renderer>().sharedMaterial = matStone;

            // Romanesque Arched Vaults along corridor
            for (int a = 0; a < 4; a++)
            {
                float ax = -4.5f - a * 2.0f;
                var arch = new GameObject($"Romanesque Arch {a}").transform;
                arch.SetParent(corridorGroup, false);
                arch.position = new Vector3(ax, -3.2f, -5.0f);

                // Left pillar
                var pilL = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pilL.transform.SetParent(arch, false);
                pilL.transform.localPosition = new Vector3(0, 1.25f, -1.95f);
                pilL.transform.localScale = new Vector3(0.35f, 2.5f, 0.35f);
                pilL.GetComponent<Renderer>().sharedMaterial = matStone;

                // Right pillar
                var pilR = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pilR.transform.SetParent(arch, false);
                pilR.transform.localPosition = new Vector3(0, 1.25f, 1.95f);
                pilR.transform.localScale = new Vector3(0.35f, 2.5f, 0.35f);
                pilR.GetComponent<Renderer>().sharedMaterial = matStone;

                // Transverse Arch Rib
                var archRib = GameObject.CreatePrimitive(PrimitiveType.Cube);
                archRib.transform.SetParent(arch, false);
                archRib.transform.localPosition = new Vector3(0, 2.45f, 0);
                archRib.transform.localScale = new Vector3(0.38f, 0.32f, 4.0f);
                archRib.GetComponent<Renderer>().sharedMaterial = matStone;
            }

            // Iron water pipes and electrical conduits along walls
            var pipe = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pipe.name = "Cellar Iron Conduit Pipe";
            pipe.transform.SetParent(corridorGroup, false);
            pipe.transform.position = corridorCenter + new Vector3(0, 1.6f, -1.98f);
            pipe.transform.localScale = new Vector3(0.06f, 5.0f, 0.06f);
            pipe.transform.rotation = Quaternion.Euler(0, 0, 90f);
            pipe.GetComponent<Renderer>().sharedMaterial = matIron;
            Object.DestroyImmediate(pipe.GetComponent<Collider>());

            // Puddle reflections on corridor floor
            for (int p = 0; p < 4; p++)
            {
                var puddle = GameObject.CreatePrimitive(PrimitiveType.Quad);
                puddle.name = $"Corridor Wet Puddle {p}";
                puddle.transform.SetParent(corridorGroup, false);
                puddle.transform.position = new Vector3(-5.0f - p * 1.8f, -3.12f, -5.0f + (p % 2 == 0 ? 0.35f : -0.35f));
                puddle.transform.rotation = Quaternion.Euler(90f, 0, 0);
                puddle.transform.localScale = new Vector3(0.85f, 1.25f, 1f);
                puddle.GetComponent<Renderer>().sharedMaterial = matPuddle;
                Object.DestroyImmediate(puddle.GetComponent<Collider>());
            }

            // D. Hidden Cellar Entryway & Recorded Room (Reel-to-Reel, Ledger, Maps, Radio)
            // Layout reference: Image 11 Panel 4 & Image 12 Panels 2-3
            Vector3 secretRoomPos = new Vector3(-12.0f, -3.2f, -5.0f);
            var secretRoomGroup = new GameObject("Secret Recorded Radio Room").transform;
            secretRoomGroup.SetParent(cellarRoot, false);

            // Secret room enclosing walls & ceiling (Damp cut stone with lime plaster bloom)
            var srFloor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            srFloor.name = "Secret Room Floor";
            srFloor.transform.SetParent(secretRoomGroup, false);
            srFloor.transform.position = secretRoomPos;
            srFloor.transform.localScale = new Vector3(4.8f, 0.15f, 4.8f);
            srFloor.GetComponent<Renderer>().sharedMaterial = matStone;

            var srCeil = GameObject.CreatePrimitive(PrimitiveType.Cube);
            srCeil.name = "Secret Room Ceiling";
            srCeil.transform.SetParent(secretRoomGroup, false);
            srCeil.transform.position = secretRoomPos + new Vector3(0, 2.6f, 0);
            srCeil.transform.localScale = new Vector3(4.8f, 0.20f, 4.8f);
            srCeil.GetComponent<Renderer>().sharedMaterial = matStone;

            // North Wall (Behind radio workbench)
            var srWallN = GameObject.CreatePrimitive(PrimitiveType.Cube);
            srWallN.name = "Secret Room Wall North";
            srWallN.transform.SetParent(secretRoomGroup, false);
            srWallN.transform.position = secretRoomPos + new Vector3(0, 1.3f, 2.35f);
            srWallN.transform.localScale = new Vector3(4.8f, 2.6f, 0.25f);
            srWallN.GetComponent<Renderer>().sharedMaterial = matStone;

            // West Wall (Left flank)
            var srWallW = GameObject.CreatePrimitive(PrimitiveType.Cube);
            srWallW.name = "Secret Room Wall West";
            srWallW.transform.SetParent(secretRoomGroup, false);
            srWallW.transform.position = secretRoomPos + new Vector3(-2.35f, 1.3f, 0);
            srWallW.transform.localScale = new Vector3(0.25f, 2.6f, 4.8f);
            srWallW.GetComponent<Renderer>().sharedMaterial = matStone;

            // South Wall (Rear flank)
            var srWallS = GameObject.CreatePrimitive(PrimitiveType.Cube);
            srWallS.name = "Secret Room Wall South";
            srWallS.transform.SetParent(secretRoomGroup, false);
            srWallS.transform.position = secretRoomPos + new Vector3(0, 1.3f, -2.35f);
            srWallS.transform.localScale = new Vector3(4.8f, 2.6f, 0.25f);
            srWallS.GetComponent<Renderer>().sharedMaterial = matStone;

            // East Wall with Arched Secret Entryway connecting to corridor
            var srWallEJambL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            srWallEJambL.name = "Secret Room Doorway Jamb L";
            srWallEJambL.transform.SetParent(secretRoomGroup, false);
            srWallEJambL.transform.position = secretRoomPos + new Vector3(2.35f, 1.3f, 1.45f);
            srWallEJambL.transform.localScale = new Vector3(0.25f, 2.6f, 1.8f);
            srWallEJambL.GetComponent<Renderer>().sharedMaterial = matStone;

            var srWallEJambR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            srWallEJambR.name = "Secret Room Doorway Jamb R";
            srWallEJambR.transform.SetParent(secretRoomGroup, false);
            srWallEJambR.transform.position = secretRoomPos + new Vector3(2.35f, 1.3f, -1.45f);
            srWallEJambR.transform.localScale = new Vector3(0.25f, 2.6f, 1.8f);
            srWallEJambR.GetComponent<Renderer>().sharedMaterial = matStone;

            var srWallEArch = GameObject.CreatePrimitive(PrimitiveType.Cube);
            srWallEArch.name = "Secret Room Doorway Arched Header";
            srWallEArch.transform.SetParent(secretRoomGroup, false);
            srWallEArch.transform.position = secretRoomPos + new Vector3(2.35f, 2.35f, 0);
            srWallEArch.transform.localScale = new Vector3(0.28f, 0.50f, 1.40f);
            srWallEArch.GetComponent<Renderer>().sharedMaterial = matStone;

            // Heavy Timber Workbench Desk
            Vector3 deskPos = secretRoomPos + new Vector3(0, 0, 0.95f);
            var desk = GameObject.CreatePrimitive(PrimitiveType.Cube);
            desk.name = "Radio & Ledger Workbench";
            desk.transform.SetParent(secretRoomGroup, false);
            desk.transform.position = deskPos + new Vector3(0, 0.42f, 0);
            desk.transform.localScale = new Vector3(2.50f, 0.84f, 1.05f);
            desk.GetComponent<Renderer>().sharedMaterial = matWood;

            // 1. Marta's Reel-to-Reel Magnetic Tape Recorder (Left Flank of Desk)
            Vector3 rtrPos = deskPos + new Vector3(-0.75f, 0.95f, 0.16f);
            var rtrObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rtrObj.name = "Marta reel-to-reel recorder";
            rtrObj.transform.SetParent(secretRoomGroup, false);
            rtrObj.transform.position = rtrPos;
            rtrObj.transform.localScale = new Vector3(0.65f, 0.36f, 0.46f);
            rtrObj.transform.rotation = Quaternion.Euler(15f, 0, 0);
            rtrObj.GetComponent<Renderer>().sharedMaterial = matReelToReel;

            // Left Supply Reel & Right Take-up Reel
            for (int r = -1; r <= 1; r += 2)
            {
                var reel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                reel.name = $"Magnetic Tape Reel {(r < 0 ? "Left" : "Right")}";
                reel.transform.SetParent(rtrObj.transform, false);
                reel.transform.localPosition = new Vector3(r * 0.20f, 0.22f, 0.02f);
                reel.transform.localRotation = Quaternion.Euler(0, 0, 0);
                reel.transform.localScale = new Vector3(0.24f, 0.02f, 0.24f);
                reel.GetComponent<Renderer>().sharedMaterial = matIron;
                Object.DestroyImmediate(reel.GetComponent<Collider>());
            }

            // 2. 1940s-1950s Vacuum Tube Shortwave Radio Console (Right Flank of Desk)
            Vector3 radioPos = deskPos + new Vector3(0.75f, 0.96f, 0.16f);
            var radioConsole = GameObject.CreatePrimitive(PrimitiveType.Cube);
            radioConsole.name = "Military Radio Transceiver";
            radioConsole.transform.SetParent(secretRoomGroup, false);
            radioConsole.transform.position = radioPos;
            radioConsole.transform.localScale = new Vector3(0.68f, 0.38f, 0.44f);
            radioConsole.GetComponent<Renderer>().sharedMaterial = matRadioMetal ?? matIron;

            // Tuning dial window on radio front
            var dial = GameObject.CreatePrimitive(PrimitiveType.Quad);
            dial.name = "Radio Tuning Dial Window";
            dial.transform.SetParent(radioConsole.transform, false);
            dial.transform.localPosition = new Vector3(0, 0.10f, -0.51f);
            dial.transform.localScale = new Vector3(0.55f, 0.16f, 1f);
            dial.GetComponent<Renderer>().sharedMaterial = matParchment;
            Object.DestroyImmediate(dial.GetComponent<Collider>());

            // Row of 4 Vacuum Tubes on top deck of the radio
            for (int v = 0; v < 4; v++)
            {
                float vx = -0.21f + v * 0.14f;
                var tubeBase = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                tubeBase.name = $"Vacuum Tube Base {v + 1}";
                tubeBase.transform.SetParent(radioConsole.transform, false);
                tubeBase.transform.localPosition = new Vector3(vx, 0.52f, 0.05f);
                tubeBase.transform.localScale = new Vector3(0.07f, 0.02f, 0.07f);
                tubeBase.GetComponent<Renderer>().sharedMaterial = matBakelite;
                Object.DestroyImmediate(tubeBase.GetComponent<Collider>());

                var tubeGlass = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                tubeGlass.name = $"Vacuum Tube Glass Envelope {v + 1}";
                tubeGlass.transform.SetParent(tubeBase.transform, false);
                tubeGlass.transform.localPosition = new Vector3(0, 3.2f, 0);
                tubeGlass.transform.localScale = new Vector3(0.85f, 2.6f, 0.85f);
                tubeGlass.GetComponent<Renderer>().sharedMaterial = matVacuumTubeGlass;
                Object.DestroyImmediate(tubeGlass.GetComponent<Collider>());

                var filament = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                filament.name = $"Tungsten Glowing Filament {v + 1}";
                filament.transform.SetParent(tubeGlass.transform, false);
                filament.transform.localPosition = Vector3.zero;
                filament.transform.localScale = new Vector3(0.35f, 0.65f, 0.35f);
                filament.GetComponent<Renderer>().sharedMaterial = matTubeFilament;
                Object.DestroyImmediate(filament.GetComponent<Collider>());
            }

            // Warm amber tube filament point light
            var tubeGlowLight = new GameObject("Vacuum Tube Filament Glow");
            tubeGlowLight.transform.SetParent(radioConsole.transform, false);
            tubeGlowLight.transform.localPosition = new Vector3(0, 0.72f, 0.05f);
            var tgl = tubeGlowLight.AddComponent<Light>();
            tgl.type = LightType.Point;
            tgl.color = new Color(1.0f, 0.45f, 0.15f);
            tgl.intensity = 1.4f;
            tgl.range = 2.0f;
            tgl.shadows = LightShadows.None;

            // Green & Red Indicator Pilot Lamps on radio panel
            var indGreen = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            indGreen.name = "Radio Pilot Lamp (Green)";
            indGreen.transform.SetParent(radioConsole.transform, false);
            indGreen.transform.localPosition = new Vector3(-0.28f, -0.15f, -0.52f);
            indGreen.transform.localScale = Vector3.one * 0.040f;
            indGreen.GetComponent<Renderer>().sharedMaterial = matIndicatorGreen;
            Object.DestroyImmediate(indGreen.GetComponent<Collider>());

            var indRed = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            indRed.name = "Radio Power Lamp (Red)";
            indRed.transform.SetParent(radioConsole.transform, false);
            indRed.transform.localPosition = new Vector3(-0.21f, -0.15f, -0.52f);
            indRed.transform.localScale = Vector3.one * 0.040f;
            indRed.GetComponent<Renderer>().sharedMaterial = matIndicatorRed;
            Object.DestroyImmediate(indRed.GetComponent<Collider>());

            // 3. The Harbor Board Ledger Book (Center-Left on Desk, directly under Lamp)
            Vector3 ledgerPos = deskPos + new Vector3(-0.15f, 0.885f, -0.12f);
            var ledgerObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ledgerObj.name = "Harbor board ledger";
            ledgerObj.transform.SetParent(secretRoomGroup, false);
            ledgerObj.transform.position = ledgerPos;
            ledgerObj.transform.localScale = new Vector3(0.64f, 0.035f, 0.44f);
            ledgerObj.transform.rotation = Quaternion.Euler(0, 180f, 0);
            ledgerObj.GetComponent<Renderer>().sharedMaterial = matLedger;

            if (!ledgerObj.GetComponent<TheLightHouse.Ledger>())
            {
                ledgerObj.AddComponent<TheLightHouse.Ledger>();
            }

            // Brass inkwell & quill
            var inkwell = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            inkwell.name = "Brass Inkwell";
            inkwell.transform.SetParent(secretRoomGroup, false);
            inkwell.transform.position = deskPos + new Vector3(-0.02f, 0.885f, 0.18f);
            inkwell.transform.localScale = new Vector3(0.08f, 0.06f, 0.08f);
            inkwell.GetComponent<Renderer>().sharedMaterial = matCopperKettle;
            Object.DestroyImmediate(inkwell.GetComponent<Collider>());

            // 4. Framed Cracked B&W Family Photograph (Positioned beside Ledger)
            Vector3 photoPos = deskPos + new Vector3(0.28f, 0.91f, -0.06f);
            var photoFrame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            photoFrame.name = "Framed Family Photograph (Crane & Daniel)";
            photoFrame.transform.SetParent(secretRoomGroup, false);
            photoFrame.transform.position = photoPos;
            photoFrame.transform.localScale = new Vector3(0.26f, 0.34f, 0.035f);
            photoFrame.transform.rotation = Quaternion.Euler(-20f, 15f, 0);
            photoFrame.GetComponent<Renderer>().sharedMaterial = matWood;

            var photoQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            photoQuad.name = "Photograph Portrait";
            photoQuad.transform.SetParent(photoFrame.transform, false);
            photoQuad.transform.localPosition = new Vector3(0, 0, -0.52f);
            photoQuad.transform.localRotation = Quaternion.identity;
            photoQuad.transform.localScale = new Vector3(0.88f, 0.90f, 1f);
            photoQuad.GetComponent<Renderer>().sharedMaterial = matPhoto;
            Object.DestroyImmediate(photoQuad.GetComponent<Collider>());

            if (!photoFrame.GetComponent<TheLightHouse.FamilyPhoto>())
            {
                photoFrame.AddComponent<TheLightHouse.FamilyPhoto>();
            }

            // 5. Heavy Morse Telegraph Key (Positioned in front of photograph near desk edge)
            Vector3 morsePos = deskPos + new Vector3(0.30f, 0.885f, -0.32f);
            var morseBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
            morseBase.name = "Morse Telegraph Key Base";
            morseBase.transform.SetParent(secretRoomGroup, false);
            morseBase.transform.position = morsePos;
            morseBase.transform.localScale = new Vector3(0.16f, 0.024f, 0.09f);
            morseBase.GetComponent<Renderer>().sharedMaterial = matBrass;

            var morseArm = GameObject.CreatePrimitive(PrimitiveType.Cube);
            morseArm.name = "Morse Key Lever Arm";
            morseArm.transform.SetParent(morseBase.transform, false);
            morseArm.transform.localPosition = new Vector3(0, 0.8f, 0);
            morseArm.transform.localScale = new Vector3(0.75f, 0.35f, 0.22f);
            morseArm.GetComponent<Renderer>().sharedMaterial = matBrass;
            Object.DestroyImmediate(morseArm.GetComponent<Collider>());

            var morseKnob = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            morseKnob.name = "Morse Key Bakelite Knob";
            morseKnob.transform.SetParent(morseBase.transform, false);
            morseKnob.transform.localPosition = new Vector3(-0.28f, 1.2f, 0);
            morseKnob.transform.localScale = new Vector3(0.24f, 0.35f, 0.24f);
            morseKnob.GetComponent<Renderer>().sharedMaterial = matBakelite;
            Object.DestroyImmediate(morseKnob.GetComponent<Collider>());

            // 6. Vintage Bakelite Microphone (To the left of the ledger)
            Vector3 micPos = deskPos + new Vector3(-0.52f, 0.885f, -0.18f);
            var micBase = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            micBase.name = "Vintage Bakelite Microphone Base";
            micBase.transform.SetParent(secretRoomGroup, false);
            micBase.transform.position = micPos;
            micBase.transform.localScale = new Vector3(0.14f, 0.025f, 0.14f);
            micBase.GetComponent<Renderer>().sharedMaterial = matBakelite;

            var micStem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            micStem.name = "Microphone Brass Neck";
            micStem.transform.SetParent(micBase.transform, false);
            micStem.transform.localPosition = new Vector3(0, 4.5f, 0);
            micStem.transform.localScale = new Vector3(0.18f, 4.2f, 0.18f);
            micStem.GetComponent<Renderer>().sharedMaterial = matBrass;
            Object.DestroyImmediate(micStem.GetComponent<Collider>());

            var micHead = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            micHead.name = "Microphone Perforated Head";
            micHead.transform.SetParent(micBase.transform, false);
            micHead.transform.localPosition = new Vector3(0, 9.5f, 0);
            micHead.transform.localScale = new Vector3(0.65f, 0.75f, 0.55f);
            micHead.GetComponent<Renderer>().sharedMaterial = matBakelite;
            Object.DestroyImmediate(micHead.GetComponent<Collider>());

            // 7. Green-Shaded Banker's Desk Lamp
            Vector3 lampPos = deskPos + new Vector3(-0.48f, 0.885f, 0.16f);
            var lampBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lampBase.name = "Bankers Desk Lamp Base";
            lampBase.transform.SetParent(secretRoomGroup, false);
            lampBase.transform.position = lampPos;
            lampBase.transform.localScale = new Vector3(0.16f, 0.024f, 0.12f);
            lampBase.GetComponent<Renderer>().sharedMaterial = matBrass;

            var lampStem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            lampStem.name = "Bankers Lamp Brass Neck";
            lampStem.transform.SetParent(lampBase.transform, false);
            lampStem.transform.localPosition = new Vector3(0, 5.0f, 0);
            lampStem.transform.localScale = new Vector3(0.15f, 4.5f, 0.15f);
            lampStem.GetComponent<Renderer>().sharedMaterial = matBrass;
            Object.DestroyImmediate(lampStem.GetComponent<Collider>());

            var lampShade = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lampShade.name = "Bankers Lamp Emerald Shade";
            lampShade.transform.SetParent(lampBase.transform, false);
            lampShade.transform.localPosition = new Vector3(0.08f, 9.8f, -0.05f);
            lampShade.transform.localRotation = Quaternion.Euler(20f, 0, 0);
            lampShade.transform.localScale = new Vector3(1.6f, 0.65f, 0.95f);
            lampShade.GetComponent<Renderer>().sharedMaterial = matDeskLampShade;
            Object.DestroyImmediate(lampShade.GetComponent<Collider>());

            // Dedicated desk lamp incandescent spotlight directed at Ledger & Family Photo
            var deskLampLight = new GameObject("Bankers Lamp Illuminator");
            deskLampLight.transform.SetParent(lampShade.transform, false);
            deskLampLight.transform.localPosition = new Vector3(0, -0.2f, 0);
            deskLampLight.transform.localRotation = Quaternion.Euler(65f, 25f, 0);
            var dll = deskLampLight.AddComponent<Light>();
            dll.type = LightType.Spot;
            dll.spotAngle = 75f;
            dll.innerSpotAngle = 48f;
            dll.range = 3.2f;
            dll.intensity = 3.2f;
            dll.color = new Color(1.0f, 0.88f, 0.65f);
            dll.shadows = LightShadows.Soft;

            // 8. Nautical Navigation Wall Charts Pinned to Damp Brick Wall (Marguerite Wreck Site)
            var chartBacking = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chartBacking.name = "Chart Mounting Board";
            chartBacking.transform.SetParent(secretRoomGroup, false);
            chartBacking.transform.position = deskPos + new Vector3(0, 1.55f, 0.72f);
            chartBacking.transform.localScale = new Vector3(1.85f, 1.25f, 0.04f);
            chartBacking.GetComponent<Renderer>().sharedMaterial = matWood;

            var chartWall = GameObject.CreatePrimitive(PrimitiveType.Quad);
            chartWall.name = "Harrow Rock Navigation Wall Chart";
            chartWall.transform.SetParent(chartBacking.transform, false);
            chartWall.transform.localPosition = new Vector3(0, 0, -0.52f);
            chartWall.transform.localScale = new Vector3(0.92f, 0.90f, 1f);
            chartWall.GetComponent<Renderer>().sharedMaterial = matNauticalChart;
            Object.DestroyImmediate(chartWall.GetComponent<Collider>());

            // Corner pushpins for the chart
            for (int px = -1; px <= 1; px += 2)
            {
                for (int py = -1; py <= 1; py += 2)
                {
                    var pin = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    pin.name = "Chart Brass Pushpin";
                    pin.transform.SetParent(chartBacking.transform, false);
                    pin.transform.localPosition = new Vector3(px * 0.44f, py * 0.42f, -0.55f);
                    pin.transform.localScale = Vector3.one * 0.035f;
                    pin.GetComponent<Renderer>().sharedMaterial = matBrass;
                    Object.DestroyImmediate(pin.GetComponent<Collider>());
                }
            }

            // 9. Atmospheric Overhead Caged Bulkhead Pendant
            var secretRoomLight = new GameObject("Secret Room Overhead Light");
            secretRoomLight.transform.SetParent(secretRoomGroup, false);
            secretRoomLight.transform.position = secretRoomPos + new Vector3(0, 2.35f, 0);
            var srl = secretRoomLight.AddComponent<Light>();
            srl.type = LightType.Point;
            srl.range = 5.5f;
            srl.intensity = 1.4f;
            srl.color = new Color(1.0f, 0.75f, 0.45f);
            srl.shadows = LightShadows.Soft;

            // Flashlight inspection fill spot
            var ledgerHighlight = new GameObject("Ledger Inspection Spotlight");
            ledgerHighlight.transform.SetParent(secretRoomGroup, false);
            ledgerHighlight.transform.position = deskPos + new Vector3(0, 1.4f, -0.4f);
            ledgerHighlight.transform.rotation = Quaternion.Euler(65f, 0, 0);
            var lhl = ledgerHighlight.AddComponent<Light>();
            lhl.type = LightType.Spot;
            lhl.spotAngle = 48f;
            lhl.innerSpotAngle = 36f;
            lhl.range = 2.5f;
            lhl.intensity = 0.55f;
            lhl.color = new Color(0.95f, 0.98f, 1.0f);
            lhl.shadows = LightShadows.None;
        }

        // =========================================================================
        // 4. FLOODED COURTYARD & WEATHERED BOAT SHED MASTER CONSTRUCTION (Image 12 Panel 4)
        // =========================================================================
        private static void BuildFloodedCourtyardAndBoatShed(Transform parent)
        {
            // Deactivate legacy blockout primitives while preserving colliders/triggers
            foreach (string oldName in new[] { "Boat Shed Floor", "Boat Shed Back", "Boat Shed Roof", "Flooded courtyard" })
            {
                var go = GameObject.Find(oldName);
                if (go)
                {
                    var r = go.GetComponent<Renderer>();
                    if (r) r.enabled = false;
                }
            }

            var courtyardMaster = new GameObject("Flooded Courtyard & Boat Shed Master").transform;
            courtyardMaster.SetParent(parent, false);

            // A. Flooded Courtyard Water Basin & Swirling Wave Foam (Phase 7 / Act 5 Climax ONLY)
            var waterGroup = new GameObject("Flooded Courtyard Water & Wave Churn").transform;
            waterGroup.SetParent(courtyardMaster, false);

            bool isAct5 = SceneManager.GetActiveScene().name == "Act5";

            // Water surface plane at y = 0.50m (representing storm surge during Act 5 finale, disabled during Acts 1-4)
            CreatePrimitiveMesh("Flooded Courtyard Water Surface", PrimitiveType.Cube, waterGroup,
                new Vector3(3.5f, 0.50f, -14.0f), new Vector3(24.0f, 0.04f, 12.5f), Quaternion.identity, matFloodWater, false);

            // Swirling wave foam ribbons and churned froth patches across flooded basin at y = 0.51m
            for (int f = 0; f < 18; f++)
            {
                float fx = -2.5f + (f % 6) * 2.2f;
                float fz = -10.5f - (f / 6) * 2.4f;
                float rotY = (f * 37f) % 360f;
                float sx = 1.4f + (f % 3) * 0.4f;
                float sz = 0.5f + ((f * 2) % 3) * 0.3f;
                CreatePrimitiveMesh($"Churned Seafoam Patch {f}", PrimitiveType.Quad, waterGroup,
                    new Vector3(fx, 0.51f + (f * 0.001f), fz), new Vector3(sx, sz, 1f), Quaternion.Euler(90f, rotY, 0), matSeaFoam, false);
            }

            // Confine courtyard flooding strictly to Act 5 climax
            waterGroup.gameObject.SetActive(isAct5);

            // B. Raised Wooden Boardwalk Approach (Partially Broken Dock Walkway elevated just above water)
            var boardwalkGroup = new GameObject("Raised Wooden Boardwalk Approach").transform;
            boardwalkGroup.SetParent(courtyardMaster, false);

            // Longitudinal Heavy Stringer Beams (partially submerged in wave surge at y = 0.46m)
            CreatePrimitiveMesh("Stringer North", PrimitiveType.Cube, boardwalkGroup,
                new Vector3(2.6f, 0.46f, -12.9f), new Vector3(5.6f, 0.16f, 0.18f), Quaternion.identity, matWood, true);
            CreatePrimitiveMesh("Stringer South", PrimitiveType.Cube, boardwalkGroup,
                new Vector3(2.6f, 0.46f, -14.1f), new Vector3(5.6f, 0.16f, 0.18f), Quaternion.identity, matWood, true);

            // 24 Planks across boardwalk with missing & tilted broken planks (Image 12 Panel 4)
            for (int p = 0; p < 24; p++)
            {
                // Planks 6, 13, and 18 are completely swept away / missing by storm surge!
                if (p == 6 || p == 13 || p == 18) continue;

                float px = 0.1f + p * 0.22f;
                float pz = Mathf.Lerp(-12.8f, -14.0f, p / 23f);
                float py = 0.58f;
                Quaternion rot = Quaternion.Euler(0, -12f, 0);

                // Tilted / loosened broken planks
                if (p == 5 || p == 7 || p == 12 || p == 14 || p == 19 || p == 21)
                {
                    float tiltAngle = (p % 2 == 0 ? 12f : -15f);
                    float rollAngle = (p % 3 == 0 ? 8f : -10f);
                    rot = Quaternion.Euler(rollAngle, -12f, tiltAngle);
                    py += 0.03f;
                    pz += (p % 2 == 0 ? 0.06f : -0.06f);
                }

                CreatePrimitiveMesh($"Boardwalk Plank {p}", PrimitiveType.Cube, boardwalkGroup,
                    new Vector3(px, py, pz), new Vector3(0.20f, 0.05f, 1.80f), rot, matBoatShedPlanks, true);
            }

            // Rustic Mooring Piles & Post-and-Rail Guide Balustrades
            for (int m = 0; m <= 4; m++)
            {
                float mx = 0.2f + m * 1.25f;
                float mzN = Mathf.Lerp(-12.0f, -13.2f, m / 4f);
                float mzS = Mathf.Lerp(-13.6f, -14.8f, m / 4f);

                // North pile
                CreatePrimitiveMesh($"Mooring Pile N_{m}", PrimitiveType.Cylinder, boardwalkGroup,
                    new Vector3(mx, 0.85f, mzN), new Vector3(0.18f, 0.75f, 0.18f), Quaternion.identity, matWood, true);
                // Foam ring at base of submerged pile
                CreatePrimitiveMesh($"Foam Ring N_{m}", PrimitiveType.Quad, waterGroup,
                    new Vector3(mx, 0.51f, mzN), new Vector3(0.65f, 0.65f, 1f), Quaternion.Euler(90f, 0, 0), matSeaFoam, false);

                // South pile
                CreatePrimitiveMesh($"Mooring Pile S_{m}", PrimitiveType.Cylinder, boardwalkGroup,
                    new Vector3(mx, 0.85f, mzS), new Vector3(0.18f, 0.75f, 0.18f), Quaternion.identity, matWood, true);
                CreatePrimitiveMesh($"Foam Ring S_{m}", PrimitiveType.Quad, waterGroup,
                    new Vector3(mx, 0.51f, mzS), new Vector3(0.65f, 0.65f, 1f), Quaternion.Euler(90f, 0, 0), matSeaFoam, false);

                // Guide Rails between piles
                if (m < 4)
                {
                    float nmx = 0.2f + (m + 1) * 1.25f;
                    float nmzN = Mathf.Lerp(-12.0f, -13.2f, (m + 1) / 4f);
                    float nmzS = Mathf.Lerp(-13.6f, -14.8f, (m + 1) / 4f);

                    Vector3 midN = new Vector3((mx + nmx) * 0.5f, 1.15f, (mzN + nmzN) * 0.5f);
                    Vector3 diffN = new Vector3(nmx - mx, 0, nmzN - mzN);
                    CreatePrimitiveMesh($"Rail N_{m}", PrimitiveType.Cube, boardwalkGroup,
                        midN, new Vector3(diffN.magnitude, 0.08f, 0.08f), Quaternion.LookRotation(diffN) * Quaternion.Euler(0, 90f, 0), matWood, false);

                    Vector3 midS = new Vector3((mx + nmx) * 0.5f, 1.15f, (mzS + nmzS) * 0.5f);
                    Vector3 diffS = new Vector3(nmx - mx, 0, nmzS - mzS);
                    CreatePrimitiveMesh($"Rail S_{m}", PrimitiveType.Cube, boardwalkGroup,
                        midS, new Vector3(diffS.magnitude, 0.08f, 0.08f), Quaternion.LookRotation(diffS) * Quaternion.Euler(0, 90f, 0), matWood, false);
                }
            }

            // C. Weathered Coastal Boat Shed Architecture
            var shedGroup = new GameObject("Weathered Coastal Boat Shed").transform;
            shedGroup.SetParent(courtyardMaster, false);

            Vector3 shedPos = new Vector3(9.8f, 0.0f, -14.0f);

            // Foundation Plinths & Heavy Sill Beams
            CreatePrimitiveMesh("Boat Shed Foundation", PrimitiveType.Cube, shedGroup,
                shedPos + new Vector3(0f, 0.35f, 0f), new Vector3(7.2f, 0.45f, 6.2f), Quaternion.identity, matWood, true);
            CreatePrimitiveMesh("Boat Shed Plank Floor", PrimitiveType.Cube, shedGroup,
                shedPos + new Vector3(0f, 0.58f, 0f), new Vector3(7.0f, 0.06f, 6.0f), Quaternion.identity, matBoatShedPlanks, true);

            // East Wall (Back)
            CreatePrimitiveMesh("Wall East (Back)", PrimitiveType.Cube, shedGroup,
                shedPos + new Vector3(3.55f, 1.95f, 0f), new Vector3(0.22f, 3.1f, 6.0f), Quaternion.identity, matBoatShedPlanks, true);

            // North Wall
            CreatePrimitiveMesh("Wall North", PrimitiveType.Cube, shedGroup,
                shedPos + new Vector3(0f, 1.95f, 2.95f), new Vector3(7.1f, 3.1f, 0.22f), Quaternion.identity, matBoatShedPlanks, true);

            // South Wall
            CreatePrimitiveMesh("Wall South", PrimitiveType.Cube, shedGroup,
                shedPos + new Vector3(0f, 1.95f, -2.95f), new Vector3(7.1f, 3.1f, 0.22f), Quaternion.identity, matBoatShedPlanks, true);

            // West Front Wall with Open Boat Bay Lintel & Framed Doors
            CreatePrimitiveMesh("Wall West Lintel", PrimitiveType.Cube, shedGroup,
                shedPos + new Vector3(-3.55f, 3.05f, 0f), new Vector3(0.24f, 0.90f, 6.0f), Quaternion.identity, matBoatShedPlanks, true);
            CreatePrimitiveMesh("Wall West Jamb N", PrimitiveType.Cube, shedGroup,
                shedPos + new Vector3(-3.55f, 1.55f, 2.15f), new Vector3(0.24f, 2.2f, 1.6f), Quaternion.identity, matBoatShedPlanks, true);
            CreatePrimitiveMesh("Wall West Jamb S", PrimitiveType.Cube, shedGroup,
                shedPos + new Vector3(-3.55f, 1.55f, -2.15f), new Vector3(0.24f, 2.2f, 1.6f), Quaternion.identity, matBoatShedPlanks, true);

            // Open Boat Bay Doors (Ajar)
            CreatePrimitiveMesh("Door Leaf Left (Ajar)", PrimitiveType.Cube, shedGroup,
                shedPos + new Vector3(-3.45f, 1.55f, 1.15f), new Vector3(0.08f, 2.15f, 1.35f), Quaternion.Euler(0, 35f, 0), matBoatShedPlanks, true);
            CreatePrimitiveMesh("Door Leaf Right (Ajar)", PrimitiveType.Cube, shedGroup,
                shedPos + new Vector3(-3.45f, 1.55f, -1.15f), new Vector3(0.08f, 2.15f, 1.35f), Quaternion.Euler(0, -28f, 0), matBoatShedPlanks, true);

            // Pitched Slate Gable Roof
            float roofPitch = 28f;
            CreatePrimitiveMesh("Roof Slate South", PrimitiveType.Cube, shedGroup,
                shedPos + new Vector3(0f, 3.85f, -1.65f), new Vector3(7.5f, 0.18f, 3.8f), Quaternion.Euler(-roofPitch, 0, 0), matSlateRoof, true);
            CreatePrimitiveMesh("Roof Slate North", PrimitiveType.Cube, shedGroup,
                shedPos + new Vector3(0f, 3.85f, 1.65f), new Vector3(7.5f, 0.18f, 3.8f), Quaternion.Euler(roofPitch, 0, 0), matSlateRoof, true);
            CreatePrimitiveMesh("Roof Ridge Cap", PrimitiveType.Cube, shedGroup,
                shedPos + new Vector3(0f, 4.75f, 0f), new Vector3(7.6f, 0.14f, 0.22f), Quaternion.identity, matIron, false);

            // Front Porch Overhang Support Pillars
            CreatePrimitiveMesh("Porch Pillar North", PrimitiveType.Cube, shedGroup,
                shedPos + new Vector3(-4.4f, 1.55f, 2.5f), new Vector3(0.18f, 2.7f, 0.18f), Quaternion.identity, matWood, true);
            CreatePrimitiveMesh("Porch Pillar South", PrimitiveType.Cube, shedGroup,
                shedPos + new Vector3(-4.4f, 1.55f, -2.5f), new Vector3(0.18f, 2.7f, 0.18f), Quaternion.identity, matWood, true);

            // Hanging Hurricane Storm Lantern on Boat Shed Porch
            var porchLampGo = new GameObject("Boat Shed Porch Hurricane Lantern");
            porchLampGo.transform.SetParent(shedGroup, false);
            porchLampGo.transform.position = shedPos + new Vector3(-4.35f, 2.75f, 0f);

            CreatePrimitiveMesh("Lantern Hood", PrimitiveType.Cylinder, porchLampGo.transform,
                new Vector3(0, 0.18f, 0), new Vector3(0.24f, 0.05f, 0.24f), Quaternion.identity, matIron, false);
            CreatePrimitiveMesh("Lantern Glass", PrimitiveType.Cylinder, porchLampGo.transform,
                Vector3.zero, new Vector3(0.16f, 0.22f, 0.16f), Quaternion.identity, matLanternGlow, false);

            var porchLight = porchLampGo.AddComponent<Light>();
            porchLight.type = LightType.Point;
            porchLight.color = new Color(1.0f, 0.65f, 0.28f);
            porchLight.intensity = 2.4f;
            porchLight.range = 7.5f;
            porchLight.shadows = LightShadows.Soft;

            // Boat Shed Interior Dressing Props
            Vector3 cradlePos = shedPos + new Vector3(0.5f, 0.55f, 0f);
            CreatePrimitiveMesh("Boat Cradle Beam L", PrimitiveType.Cube, shedGroup,
                cradlePos + new Vector3(-1.2f, 0.15f, 0f), new Vector3(0.18f, 0.35f, 1.8f), Quaternion.identity, matWood, true);
            CreatePrimitiveMesh("Boat Cradle Beam R", PrimitiveType.Cube, shedGroup,
                cradlePos + new Vector3(1.2f, 0.15f, 0f), new Vector3(0.18f, 0.35f, 1.8f), Quaternion.identity, matWood, true);

            // Overturned Rowboat Hull resting in cradle
            CreatePrimitiveMesh("Rowboat Hull Keel", PrimitiveType.Cube, shedGroup,
                cradlePos + new Vector3(0f, 0.70f, 0f), new Vector3(3.8f, 0.45f, 1.25f), Quaternion.Euler(0, 0, 180f), matWood, true);

            // Coiled Hemp Ropes on wall
            CreatePrimitiveMesh("Coiled Wall Rope", PrimitiveType.Cylinder, shedGroup,
                shedPos + new Vector3(1.8f, 1.95f, 2.82f), new Vector3(0.38f, 0.06f, 0.38f), Quaternion.Euler(90f, 0, 0), matBookGold, false);

            // Pair of Wooden Ash Oars
            CreatePrimitiveMesh("Boat Oar 1", PrimitiveType.Cylinder, shedGroup,
                shedPos + new Vector3(2.6f, 1.75f, 2.75f), new Vector3(0.06f, 1.35f, 0.06f), Quaternion.Euler(-18f, 0, 8f), matWood, false);
            CreatePrimitiveMesh("Boat Oar 2", PrimitiveType.Cylinder, shedGroup,
                shedPos + new Vector3(2.8f, 1.75f, 2.75f), new Vector3(0.06f, 1.35f, 0.06f), Quaternion.Euler(-16f, 0, -5f), matWood, false);

            // Stacked Cargo Crates & Oak Barrels
            var barrelMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Reference/Oak Barrel.mat") ?? matWood;
            CreatePrimitiveMesh("Storage Barrel 1", PrimitiveType.Cylinder, shedGroup,
                shedPos + new Vector3(2.8f, 0.92f, -1.8f), new Vector3(0.65f, 0.88f, 0.65f), Quaternion.identity, barrelMat, true);
            CreatePrimitiveMesh("Storage Barrel 2", PrimitiveType.Cylinder, shedGroup,
                shedPos + new Vector3(2.2f, 0.92f, -2.1f), new Vector3(0.62f, 0.84f, 0.62f), Quaternion.identity, barrelMat, true);
            CreatePrimitiveMesh("Storage Crate 1", PrimitiveType.Cube, shedGroup,
                shedPos + new Vector3(1.2f, 0.85f, -2.1f), new Vector3(0.95f, 0.75f, 0.95f), Quaternion.Euler(0, 8f, 0), matWood, true);

            // D. Tactical Flashlight Beam & Drowned Figures in Flood Waters (Matching Image 12 Panel 4)
            var flashlightGo = new GameObject("Tactical Flashlight Beam (Courtyard Inspection)");
            flashlightGo.transform.SetParent(courtyardMaster, false);
            flashlightGo.transform.position = new Vector3(-0.4f, 1.55f, -10.5f);
            flashlightGo.transform.LookAt(shedPos + new Vector3(-2.0f, 0.8f, 0f));

            var flashSpot = flashlightGo.AddComponent<Light>();
            flashSpot.type = LightType.Spot;
            flashSpot.spotAngle = 36f;
            flashSpot.innerSpotAngle = 20f;
            flashSpot.range = 28f;
            flashSpot.intensity = 5.2f;
            flashSpot.color = new Color(0.90f, 0.95f, 1.0f); // Cold piercing flashlight beam
            flashSpot.shadows = LightShadows.Soft;

            // Atmospheric Drowned Figures in Churned Flood Waters
            var drownedGroup = new GameObject("Drowned Figures in Flooded Water").transform;
            drownedGroup.SetParent(courtyardMaster, false);

            Vector3[] drownedPositions = new[]
            {
                new Vector3(3.2f, -0.15f, -11.2f),
                new Vector3(4.8f, -0.15f, -16.5f),
                new Vector3(1.5f, -0.15f, -15.8f)
            };

            var ragMat = EnsureMaterial("DrownedWaterloggedRags", "Universal Render Pipeline/Lit");
            ragMat.SetColor("_BaseColor", new Color(0.18f, 0.22f, 0.22f));
            ragMat.SetFloat("_Smoothness", 0.85f); // Wet dripping cloth

            var paleSkinMat = EnsureMaterial("DrownedPaleSkin", "Universal Render Pipeline/Lit");
            paleSkinMat.SetColor("_BaseColor", new Color(0.68f, 0.76f, 0.74f)); // Sickly pale maritime green-grey
            paleSkinMat.SetFloat("_Smoothness", 0.70f);

            for (int df = 0; df < drownedPositions.Length; df++)
            {
                Vector3 dfPos = drownedPositions[df];
                var fig = new GameObject($"Atmospheric Drowned Figure {df + 1}").transform;
                fig.SetParent(drownedGroup, false);
                fig.position = dfPos;
                fig.LookAt(new Vector3(-0.4f, 0.0f, -10.5f)); // Staring silently at the player

                // Organic Humanoid Silhouette
                // Lower submerged body / legs
                CreatePrimitiveMesh("Submerged Legs", PrimitiveType.Cylinder, fig,
                    new Vector3(0, 0.35f, 0), new Vector3(0.28f, 0.45f, 0.24f), Quaternion.identity, ragMat, false);
                // Slender waterlogged torso
                CreatePrimitiveMesh("Torso", PrimitiveType.Cylinder, fig,
                    new Vector3(0, 0.95f, 0), new Vector3(0.36f, 0.42f, 0.26f), Quaternion.identity, ragMat, false);
                // Chest rags & tattered shawl
                CreatePrimitiveMesh("Chest Shawl", PrimitiveType.Cube, fig,
                    new Vector3(0, 1.05f, 0.02f), new Vector3(0.42f, 0.38f, 0.28f), Quaternion.Euler(5f, 0, 0), ragMat, false);
                // Head with wet dripping hair
                CreatePrimitiveMesh("Head", PrimitiveType.Sphere, fig,
                    new Vector3(0, 1.48f, 0.04f), new Vector3(0.22f, 0.26f, 0.22f), Quaternion.Euler(8f, 0, 0), paleSkinMat, false);
                CreatePrimitiveMesh("Hair", PrimitiveType.Sphere, fig,
                    new Vector3(0, 1.54f, 0.01f), new Vector3(0.24f, 0.18f, 0.24f), Quaternion.identity, ragMat, false);
                // Pale dangling arms reaching into water
                CreatePrimitiveMesh("Arm Left", PrimitiveType.Cylinder, fig,
                    new Vector3(-0.26f, 0.88f, 0.08f), new Vector3(0.08f, 0.38f, 0.08f), Quaternion.Euler(22f, -10f, 12f), paleSkinMat, false);
                CreatePrimitiveMesh("Arm Right", PrimitiveType.Cylinder, fig,
                    new Vector3(0.26f, 0.88f, 0.08f), new Vector3(0.08f, 0.38f, 0.08f), Quaternion.Euler(22f, 10f, -12f), paleSkinMat, false);

                // Swirling churned foam ring at waist where body breaches water (y = 0.51m in world space)
                CreatePrimitiveMesh($"Water Foam Ring", PrimitiveType.Quad, fig,
                    new Vector3(0, 0.65f, 0), new Vector3(1.35f, 1.35f, 1f), Quaternion.Euler(90f, 0, 0), matSeaFoam, false);
            }
        }
    }
}
