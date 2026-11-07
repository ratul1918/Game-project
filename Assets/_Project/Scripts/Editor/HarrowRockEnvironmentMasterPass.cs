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
        private static Material matColdWindow;
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

            matColdWindow = EnsureMaterial("ColdWindowGlass", "Universal Render Pipeline/Lit");
            matColdWindow.SetColor("_BaseColor", new Color(0.45f, 0.55f, 0.65f, 0.35f));
            matColdWindow.SetFloat("_Surface", 1);
            matColdWindow.SetFloat("_Blend", 0);
            matColdWindow.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            matColdWindow.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            matColdWindow.SetFloat("_Smoothness", 0.95f);
            matColdWindow.SetFloat("_Metallic", 0.1f);

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

            var radioObj = GameObject.Find("Radio");
            if (radioObj && !radioObj.GetComponent<AudioSource>()) radioObj.AddComponent<AudioSource>();

            var boatmanObj = GameObject.Find("Boatman");
            if (boatmanObj && !boatmanObj.GetComponent<AudioSource>()) boatmanObj.AddComponent<AudioSource>();

            var existing = GameObject.Find(RootName);
            if (existing) Object.DestroyImmediate(existing);

            var root = new GameObject(RootName).transform;

            BuildExteriorIsland(root);
            BuildKeepersHouseInterior(root);
            BuildTowerInteriorAndStudy(root);
            BuildUndergroundCellar(root);
            BuildFloodedCourtyardAndBoatShed(root);
            SetupPlayerFlashlightAndAudio(root);
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
            float[] ringHeights = new[] { 3.2f, 6.4f, 9.6f };
            foreach (float y in ringHeights)
            {
                float radius = Mathf.Lerp(6.5f, 4.8f, y / 9.6f) + 0.04f;
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
            sweepingLightGo.transform.position = new Vector3(0, 13.5f, 0);
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
            // 1. Deactivate legacy primitive blockouts, old straight slabs, and old duplicate staircases
            foreach (string oldName in new[]
            {
                "Reference Visuals/Lighthouse/GroundFloor/Staircase_A",
                "Reference Visuals/Lighthouse/FirstFloor/Staircase_B",
                "Reference Visuals/Lighthouse/SecondFloor/Staircase_C",
                "Reference Visuals/Lighthouse/UpperTower/Stairs_Flights3_7",
                "Reference Visuals/Lighthouse/FirstFloor/ArchiveRoom",
                "Reference Visuals/Lighthouse/FirstFloor/Architecture",
                "Reference Visuals/Lighthouse/SecondFloor/Architecture",
                "Reference Visuals/Lighthouse/SecondFloor/LivingRoom",
                "Reference Visuals/Lighthouse/UpperTower/CraneQuarters_Level3",
                "Reference Visuals/Lighthouse/UpperTower/IntermediateFloors",
                "Reference Visuals/Lighthouse/GroundFloor/Storage",
                "Lighthouse Layout/Stairs",
                "ArchiveRoom",
                "level2-desk"
            })
            {
                var go = GameObject.Find(oldName);
                if (go)
                {
                    foreach (var r in go.GetComponentsInChildren<Renderer>(true)) r.enabled = false;
                    foreach (var c in go.GetComponentsInChildren<Collider>(true)) c.enabled = false;
                    go.SetActive(false);
                }
            }

            // Deactivate legacy flat floor rings that slice through the stairs
            var towerLayout = GameObject.Find("Lighthouse Layout/Tower");
            if (towerLayout)
            {
                for (int t = 0; t < towerLayout.transform.childCount; t++)
                {
                    var child = towerLayout.transform.GetChild(t);
                    if (child.name.StartsWith("Tower Floor"))
                    {
                        foreach (var r in child.GetComponentsInChildren<Renderer>(true)) r.enabled = false;
                        foreach (var c in child.GetComponentsInChildren<Collider>(true)) c.enabled = false;
                        child.gameObject.SetActive(false);
                    }
                }
            }

            var towerRoot = new GameObject("Tower Interior & Circular Rooms Master").transform;
            towerRoot.SetParent(parent, false);

            // A. Outer Curved Masonry Wall Lining (Diameter ~10.4m, radius 5.20m, y = 0 to 9.8m)
            var wallGroup = new GameObject("Curving Outer Masonry Walls & Shaft").transform;
            wallGroup.SetParent(towerRoot, false);

            int outerWallSegs = 32;
            float outerWallR = 5.20f;
            for (int i = 0; i < outerWallSegs; i++)
            {
                float ang = i * (360f / outerWallSegs);
                // South Grand Portal at 180° (z = -6.5m)
                if (Mathf.Abs(Mathf.DeltaAngle(ang, 180f)) < 16f) continue;
                // West Cottage Link Portal at ~240°
                if (Mathf.Abs(Mathf.DeltaAngle(ang, 240f)) < 16f) continue;

                float rad = ang * Mathf.Deg2Rad;
                Vector3 segPos = new Vector3(Mathf.Sin(rad) * outerWallR, 4.90f, Mathf.Cos(rad) * outerWallR);
                Quaternion segRot = Quaternion.Euler(0, ang, 0);
                CreatePrimitiveMesh($"Outer Tower Stone Arc {i}", PrimitiveType.Cube, wallGroup,
                    segPos, new Vector3(1.15f, 9.80f, 0.40f), segRot, matStone, true);
            }

            // B. Circular Room Floor Slabs (r = 3.08m, covering ONLY the central room - Leaving Annular Stairwell 100% Open Vertically)
            var slabsGroup = new GameObject("Circular Room Floor Slabs").transform;
            slabsGroup.SetParent(towerRoot, false);

            // Floor 1 Flagstone Ground Floor (y = 0.15m)
            CreatePrimitiveMesh("Floor 1 Ground Flagstone Slab", PrimitiveType.Cylinder, slabsGroup,
                new Vector3(0, 0.08f, 0), new Vector3(6.16f, 0.14f, 6.16f), Quaternion.identity, matStone, true);

            // Floor 2 Hardwood Deck Slab (y = 3.35m)
            CreatePrimitiveMesh("Floor 2 Hardwood Deck Slab", PrimitiveType.Cylinder, slabsGroup,
                new Vector3(0, 3.28f, 0), new Vector3(6.16f, 0.14f, 6.16f), Quaternion.identity, matWood, true);

            // Floor 3 Slate & Iron Deck Slab (y = 6.55m)
            CreatePrimitiveMesh("Floor 3 Slate & Iron Deck Slab", PrimitiveType.Cylinder, slabsGroup,
                new Vector3(0, 6.48f, 0), new Vector3(6.16f, 0.14f, 6.16f), Quaternion.identity, matSlate, true);

            // C. 3-TIER STACK OF FULLY ENCLOSED CIRCULAR CENTRAL ROOMS
            // Floor 1: Entrance Hall / Utility & Pantry (holds cellar trapdoor / arched cellar entrance)
            // Floor 2: Keeper’s Study / Living Quarter (solid mahogany desk, charts, logbook, bookcases, rug)
            // Floor 3: Watchroom & Battery/Fuel Staging (battery bank racks, fuel cans, barometer, telegraph)
            float[] floorHeights = new[] { 0.15f, 3.35f, 6.55f };
            float[] doorAngles = new[] { 210f, 90f, 330f }; // Floor 1 faces SW near cottage, Floor 2 faces East, Floor 3 faces NW
            string[] roomNames = new[] { "Floor 1 Entrance Hall & Pantry", "Floor 2 Keeper Study", "Floor 3 Watchroom & Battery Deck" };

            float roomWallR = 2.95f;
            float roomWallThick = 0.25f;
            int roomSegs = 32;

            for (int k = 0; k < 3; k++)
            {
                float yBase = floorHeights[k];
                float dAng = doorAngles[k];

                var roomGroup = new GameObject(roomNames[k]).transform;
                roomGroup.SetParent(towerRoot, false);

                // 1. Vertical Circular Partition Walls enclosing the room completely from floor to ceiling slab
                for (int s = 0; s < roomSegs; s++)
                {
                    float sAng = s * (360f / roomSegs);
                    // Leave doorway opening (clear width ~1.05m)
                    if (Mathf.Abs(Mathf.DeltaAngle(sAng, dAng)) < 11.5f) continue;

                    float sRad = sAng * Mathf.Deg2Rad;
                    Vector3 segPos = new Vector3(Mathf.Sin(sRad) * roomWallR, yBase + 1.60f, Mathf.Cos(sRad) * roomWallR);
                    Quaternion segRot = Quaternion.Euler(0, sAng, 0);
                    float tangW = 2f * Mathf.PI * roomWallR / roomSegs * 1.05f;

                    CreatePrimitiveMesh($"Room {k + 1} Wall Arc {s}", PrimitiveType.Cube, roomGroup,
                        segPos, new Vector3(tangW, 3.20f, roomWallThick), segRot, matStone, true);
                }

                // 2. Doorway Frame & Rustic Wooden Plank Door
                float dRad = dAng * Mathf.Deg2Rad;
                Vector3 doorCenter = new Vector3(Mathf.Sin(dRad) * roomWallR, yBase, Mathf.Cos(dRad) * roomWallR);
                Quaternion doorRot = Quaternion.Euler(0, dAng, 0);
                Vector3 rightDir = doorRot * Vector3.right;

                // Door Lintel
                CreatePrimitiveMesh($"Floor {k + 1} Door Lintel", PrimitiveType.Cube, roomGroup,
                    doorCenter + Vector3.up * 2.22f, new Vector3(1.22f, 0.22f, 0.32f), doorRot, matWood, true);
                // Left & Right Jambs (Clear width = 0.96m, Height = 2.15m)
                CreatePrimitiveMesh($"Floor {k + 1} Door Jamb L", PrimitiveType.Cube, roomGroup,
                    doorCenter + Vector3.up * 1.08f - rightDir * 0.54f, new Vector3(0.14f, 2.16f, 0.30f), doorRot, matWood, true);
                CreatePrimitiveMesh($"Floor {k + 1} Door Jamb R", PrimitiveType.Cube, roomGroup,
                    doorCenter + Vector3.up * 1.08f + rightDir * 0.54f, new Vector3(0.14f, 2.16f, 0.30f), doorRot, matWood, true);
                // Threshold Sill
                CreatePrimitiveMesh($"Floor {k + 1} Door Threshold", PrimitiveType.Cube, roomGroup,
                    doorCenter + Vector3.up * 0.04f, new Vector3(1.10f, 0.08f, 0.38f), doorRot, matStone, true);

                // Hinged Rustic Wooden Plank Door
                var doorHinge = new GameObject($"Floor {k + 1} Door");
                doorHinge.transform.SetParent(roomGroup, false);
                doorHinge.transform.position = doorCenter - rightDir * 0.46f;
                doorHinge.transform.rotation = doorRot;

                var doorLeaf = GameObject.CreatePrimitive(PrimitiveType.Cube);
                doorLeaf.name = $"Floor_{k + 1}_Door_Panel";
                doorLeaf.transform.SetParent(doorHinge.transform, false);
                doorLeaf.transform.localPosition = new Vector3(0.46f, 1.08f, 0f);
                doorLeaf.transform.localScale = new Vector3(0.92f, 2.10f, 0.055f);
                doorLeaf.GetComponent<Renderer>().sharedMaterial = matWood;
                var leafBc = doorLeaf.GetComponent<BoxCollider>();
                if (leafBc) { leafBc.center = Vector3.zero; leafBc.size = Vector3.one; }

                // Attach Door & NoiseEmitter components
                doorHinge.AddComponent<NoiseEmitter>();
                var doorScript = doorHinge.AddComponent<Door>();
                var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
                doorScript.GetType().GetField("doorId", flags)?.SetValue(doorScript, $"tower-floor-{k + 1}-door");
                doorScript.GetType().GetField("openAngle", flags)?.SetValue(doorScript, -100f);
                doorScript.GetType().GetField("turnSpeed", flags)?.SetValue(doorScript, 130f);

                // 3. Flat Stone Landing Platform (1.6m x 1.6m at entrance of each door)
                var landingGroup = new GameObject($"Floor {k + 1} Stair Landing Platform").transform;
                landingGroup.SetParent(towerRoot, false);

                int landDivs = 8;
                float halfSpan = 14f;
                for (int d = 0; d < landDivs; d++)
                {
                    float lAng = (dAng - halfSpan) + (d + 0.5f) * (2f * halfSpan / landDivs);
                    float lRad = lAng * Mathf.Deg2Rad;
                    float lMidR = (3.12f + 4.65f) * 0.5f;
                    Vector3 lPos = new Vector3(Mathf.Sin(lRad) * lMidR, yBase - 0.06f, Mathf.Cos(lRad) * lMidR);
                    Quaternion lRot = Quaternion.Euler(0, lAng, 0);
                    float lTang = 2f * Mathf.PI * lMidR * (2f * halfSpan / 360f) / landDivs * 1.08f;
                    CreatePrimitiveMesh($"Landing {k + 1} Slab {d}", PrimitiveType.Cube, landingGroup,
                        lPos, new Vector3(lTang, 0.12f, 1.53f), lRot, matStone, true);
                }

                // Landing Balustrade Guarding Outer Void Edge & Newel Post
                for (int b = 0; b <= 4; b++)
                {
                    float bAng = (dAng - halfSpan) + b * (2f * halfSpan / 4f);
                    float bRad = bAng * Mathf.Deg2Rad;
                    Vector3 bPos = new Vector3(Mathf.Sin(bRad) * 4.65f, yBase, Mathf.Cos(bRad) * 4.65f);
                    CreatePrimitiveMesh($"Landing Rail Post {b}", PrimitiveType.Cylinder, landingGroup,
                        bPos + Vector3.up * 0.55f, new Vector3(0.04f, 0.55f, 0.04f), Quaternion.identity, matIron, false);
                }
                Vector3 newelP = new Vector3(Mathf.Sin((dAng + halfSpan) * Mathf.Deg2Rad) * 4.65f, yBase, Mathf.Cos((dAng + halfSpan) * Mathf.Deg2Rad) * 4.65f);
                CreatePrimitiveMesh($"Landing {k + 1} Newel", PrimitiveType.Cylinder, landingGroup,
                    newelP + Vector3.up * 0.60f, new Vector3(0.09f, 0.60f, 0.09f), Quaternion.identity, matBrass, false);
                CreatePrimitiveMesh($"Landing {k + 1} Finial", PrimitiveType.Sphere, landingGroup,
                    newelP + Vector3.up * 1.25f, new Vector3(0.12f, 0.12f, 0.12f), Quaternion.identity, matBrass, false);
            }

            // D. SPECIFIC ROOM NARRATIVE INTERIOR FURNISHINGS
            // -------------------------------------------------------------
            // FLOOR 1: ENTRANCE HALL / UTILITY & PANTRY
            // -------------------------------------------------------------
            var f1Group = new GameObject("Floor 1 Interior Pantry & Utility Furnishings").transform;
            f1Group.SetParent(towerRoot, false);

            // Arched Cellar Entrance Portal connecting to underground tunnels (Ref: Screenshot 1 & 2)
            Vector3 cellarArchPos = new Vector3(-1.85f, 0.15f, 1.65f);
            Quaternion cellarArchRot = Quaternion.Euler(0, -42f, 0);
            CreatePrimitiveMesh("Cellar Arch Jamb Left", PrimitiveType.Cube, f1Group,
                cellarArchPos + cellarArchRot * new Vector3(-0.55f, 1.20f, 0), new Vector3(0.24f, 2.40f, 0.35f), cellarArchRot, matStone, true);
            CreatePrimitiveMesh("Cellar Arch Jamb Right", PrimitiveType.Cube, f1Group,
                cellarArchPos + cellarArchRot * new Vector3(0.55f, 1.20f, 0), new Vector3(0.24f, 2.40f, 0.35f), cellarArchRot, matStone, true);
            CreatePrimitiveMesh("Cellar Arch Lintel Beam", PrimitiveType.Cube, f1Group,
                cellarArchPos + cellarArchRot * new Vector3(0f, 2.40f, 0), new Vector3(1.40f, 0.28f, 0.40f), cellarArchRot, matWood, true);

            // Pantry Supply Shelves along Northeast curved inner wall
            float pz = 1.85f;
            for (int s = 0; s < 3; s++)
            {
                float sy = 0.70f + s * 0.65f;
                CreatePrimitiveMesh($"Pantry Shelf {s + 1}", PrimitiveType.Cube, f1Group,
                    new Vector3(1.20f, sy, pz), new Vector3(2.40f, 0.05f, 0.45f), Quaternion.Euler(0, 15f, 0), matWood, true);
                for (int c = 0; c < 5; c++)
                {
                    float cx = 0.35f + c * 0.42f;
                    CreatePrimitiveMesh($"Provisions Can {s}_{c}", PrimitiveType.Cylinder, f1Group,
                        new Vector3(cx, sy + 0.12f, pz + 0.05f), new Vector3(0.12f, 0.12f, 0.12f), Quaternion.identity, matBrass, false);
                }
            }

            // Heavy Utility Wooden Crates & Oak Maritime Casks
            CreatePrimitiveMesh("Maritime Supply Crate 1", PrimitiveType.Cube, f1Group,
                new Vector3(0.85f, 0.45f, -1.65f), new Vector3(0.75f, 0.60f, 0.75f), Quaternion.Euler(0, 22f, 0), matWood, true);
            CreatePrimitiveMesh("Maritime Supply Crate 2", PrimitiveType.Cube, f1Group,
                new Vector3(1.55f, 0.42f, -1.45f), new Vector3(0.65f, 0.54f, 0.65f), Quaternion.Euler(0, -12f, 0), matWood, true);
            CreatePrimitiveMesh("Utility Barrel 1", PrimitiveType.Cylinder, f1Group,
                new Vector3(-0.95f, 0.55f, -1.85f), new Vector3(0.60f, 0.45f, 0.60f), Quaternion.identity, matWood, true);
            CreatePrimitiveMesh("Utility Barrel 2", PrimitiveType.Cylinder, f1Group,
                new Vector3(-1.60f, 0.50f, -1.45f), new Vector3(0.55f, 0.40f, 0.55f), Quaternion.identity, matWood, true);

            // Wall Tool Pegboard & Coiled Hawser Rope
            CreatePrimitiveMesh("Wall Tool Board", PrimitiveType.Cube, f1Group,
                new Vector3(-2.20f, 1.65f, 0.20f), new Vector3(0.08f, 1.20f, 1.80f), Quaternion.identity, matWood, false);
            CreatePrimitiveMesh("Coiled Hawser Rope", PrimitiveType.Cylinder, f1Group,
                new Vector3(-2.15f, 1.70f, 0.45f), new Vector3(0.38f, 0.08f, 0.38f), Quaternion.Euler(0, 0, 90f), matParchment, false);

            // Ceiling Bulkhead Fixture (Floor 1)
            var f1LightGo = new GameObject("Floor 1 Bulkhead Light");
            f1LightGo.transform.SetParent(f1Group, false);
            f1LightGo.transform.position = new Vector3(0f, 3.10f, 0f);
            var f1Light = f1LightGo.AddComponent<Light>();
            f1Light.type = LightType.Point;
            f1Light.color = new Color(1.0f, 0.76f, 0.42f);
            f1Light.range = 6.5f;
            f1Light.intensity = 2.2f;
            f1Light.shadows = LightShadows.Soft;

            // -------------------------------------------------------------
            // FLOOR 2: KEEPER’S STUDY / LIVING QUARTER
            // -------------------------------------------------------------
            var f2Group = new GameObject("Floor 2 Interior Study Furnishings").transform;
            f2Group.SetParent(towerRoot, false);

            float f2Y = 3.35f;

            // Persian Carpet / Area Rug
            CreatePrimitiveMesh("Persian Study Rug", PrimitiveType.Cube, f2Group,
                new Vector3(0f, f2Y + 0.015f, 0.20f), new Vector3(3.8f, 0.012f, 3.2f), Quaternion.identity, matRug, false);

            // Solid Mahogany Writing Desk Center Coordinates
            Vector3 deskPos = new Vector3(0f, f2Y, 0.35f);

            // Solid Mahogany Desktop
            CreatePrimitiveMesh("Mahogany Desk Top", PrimitiveType.Cube, f2Group,
                deskPos + new Vector3(0f, 0.75f, 0f), new Vector3(2.0f, 0.08f, 1.05f), Quaternion.identity, matMahogany, true);
            // Pedestals Left & Right
            CreatePrimitiveMesh("Desk Pedestal Left", PrimitiveType.Cube, f2Group,
                deskPos + new Vector3(-0.74f, 0.36f, 0f), new Vector3(0.48f, 0.70f, 0.92f), Quaternion.identity, matMahogany, true);
            CreatePrimitiveMesh("Desk Pedestal Right", PrimitiveType.Cube, f2Group,
                deskPos + new Vector3(0.74f, 0.36f, 0f), new Vector3(0.48f, 0.70f, 0.92f), Quaternion.identity, matMahogany, true);
            CreatePrimitiveMesh("Desk Modesty Panel", PrimitiveType.Cube, f2Group,
                deskPos + new Vector3(0f, 0.38f, 0.38f), new Vector3(1.0f, 0.66f, 0.04f), Quaternion.identity, matMahogany, false);

            // Solid Brass Drawer Pulls
            for (int d = 0; d < 3; d++)
            {
                float dy = 0.58f - d * 0.22f;
                CreatePrimitiveMesh($"Brass Pull L{d}", PrimitiveType.Cube, f2Group,
                    deskPos + new Vector3(-0.74f, dy, -0.47f), new Vector3(0.14f, 0.035f, 0.025f), Quaternion.identity, matBrass, false);
                CreatePrimitiveMesh($"Brass Pull R{d}", PrimitiveType.Cube, f2Group,
                    deskPos + new Vector3(0.74f, dy, -0.47f), new Vector3(0.14f, 0.035f, 0.025f), Quaternion.identity, matBrass, false);
            }

            // Open Maritime Navigation Chart of Harrow Rock & Shoals
            CreatePrimitiveMesh("Nautical Chart Spread", PrimitiveType.Quad, f2Group,
                deskPos + new Vector3(0.08f, 0.795f, -0.05f), new Vector3(0.78f, 0.55f, 1f), Quaternion.Euler(90f, 4f, 0f), matNauticalChart, false);

            // Brass Navigational Calipers / Drafting Dividers
            Vector3 caliperP = deskPos + new Vector3(0.16f, 0.803f, -0.06f);
            CreatePrimitiveMesh("Calipers Hinge Disk", PrimitiveType.Cylinder, f2Group,
                caliperP, new Vector3(0.045f, 0.008f, 0.045f), Quaternion.identity, matBrass, false);
            CreatePrimitiveMesh("Calipers Leg Left", PrimitiveType.Cylinder, f2Group,
                caliperP + new Vector3(-0.045f, 0.003f, 0.09f), new Vector3(0.010f, 0.10f, 0.010f), Quaternion.Euler(0, -18f, 90f), matBrass, false);
            CreatePrimitiveMesh("Calipers Leg Right", PrimitiveType.Cylinder, f2Group,
                caliperP + new Vector3(0.045f, 0.003f, 0.09f), new Vector3(0.010f, 0.10f, 0.010f), Quaternion.Euler(0, 18f, 90f), matBrass, false);

            // Open Handwritten Keeper's Logbook
            Vector3 logP = deskPos + new Vector3(-0.38f, 0.801f, -0.08f);
            CreatePrimitiveMesh("Logbook Cover", PrimitiveType.Cube, f2Group,
                logP, new Vector3(0.55f, 0.018f, 0.38f), Quaternion.Euler(0, 12f, 0), matWood, false);
            CreatePrimitiveMesh("Logbook Left Page", PrimitiveType.Cube, f2Group,
                logP + new Vector3(-0.13f, 0.012f, 0f), new Vector3(0.25f, 0.012f, 0.34f), Quaternion.Euler(0, 12f, 0), matLedger, false);
            CreatePrimitiveMesh("Logbook Right Page", PrimitiveType.Cube, f2Group,
                logP + new Vector3(0.13f, 0.012f, 0f), new Vector3(0.25f, 0.012f, 0.34f), Quaternion.Euler(0, 12f, 0), matLedger, false);

            // Vintage Brass Oil Lamp with Clear Glass Chimney
            Vector3 oilLampP = deskPos + new Vector3(0.68f, 0.795f, 0.22f);
            CreatePrimitiveMesh("Oil Lamp Brass Base", PrimitiveType.Cylinder, f2Group,
                oilLampP + Vector3.up * 0.04f, new Vector3(0.18f, 0.08f, 0.18f), Quaternion.identity, matBrass, false);
            CreatePrimitiveMesh("Oil Lamp Font Reservoir", PrimitiveType.Sphere, f2Group,
                oilLampP + Vector3.up * 0.14f, new Vector3(0.16f, 0.12f, 0.16f), Quaternion.identity, matBrass, false);
            CreatePrimitiveMesh("Oil Lamp Glass Chimney", PrimitiveType.Cylinder, f2Group,
                oilLampP + Vector3.up * 0.36f, new Vector3(0.10f, 0.25f, 0.10f), Quaternion.identity, matSpecimenJar, false);

            // Terrestrial Globe on Brass Armillary Stand
            Vector3 globeP = deskPos + new Vector3(-0.65f, 0.795f, -0.22f);
            CreatePrimitiveMesh("Globe Wooden Base", PrimitiveType.Cylinder, f2Group,
                globeP + Vector3.up * 0.025f, new Vector3(0.18f, 0.04f, 0.18f), Quaternion.identity, matMahogany, false);
            CreatePrimitiveMesh("Globe Sphere", PrimitiveType.Sphere, f2Group,
                globeP + Vector3.up * 0.25f, new Vector3(0.24f, 0.24f, 0.24f), Quaternion.Euler(23.5f, 35f, 0f), matParchment, false);

            // Glazed Dark Stoneware Inkwell & Feather Quill
            Vector3 inkP = deskPos + new Vector3(0.36f, 0.795f, -0.25f);
            CreatePrimitiveMesh("Ceramic Inkwell", PrimitiveType.Cylinder, f2Group,
                inkP + Vector3.up * 0.035f, new Vector3(0.075f, 0.07f, 0.075f), Quaternion.identity, matCeramic, false);
            CreatePrimitiveMesh("Goose Quill", PrimitiveType.Cylinder, f2Group,
                inkP + new Vector3(-0.02f, 0.15f, 0.02f), new Vector3(0.012f, 0.24f, 0.012f), Quaternion.Euler(24f, 0, 15f), matParchment, false);

            // Carved Maritime Desk Armchair
            Vector3 chairP = deskPos + new Vector3(0.18f, 0.06f, -0.68f);
            Quaternion chairRot = Quaternion.Euler(0, 15f, 0);
            CreatePrimitiveMesh("Armchair Seat", PrimitiveType.Cube, f2Group,
                chairP + new Vector3(0f, 0.44f, 0f), new Vector3(0.62f, 0.08f, 0.58f), chairRot, matMahogany, true);
            CreatePrimitiveMesh("Chair Back Top Rail", PrimitiveType.Cube, f2Group,
                chairP + chairRot * new Vector3(0f, 1.15f, -0.24f), new Vector3(0.58f, 0.12f, 0.06f), chairRot, matMahogany, false);

            // Tall Curved Wooden Bookcases with Volumes & Specimen Jars (West wall)
            float bz = 2.15f;
            CreatePrimitiveMesh("Bookcase Upright L", PrimitiveType.Cube, f2Group,
                new Vector3(-1.85f, f2Y + 1.35f, bz), new Vector3(0.12f, 2.70f, 0.48f), Quaternion.identity, matWood, true);
            CreatePrimitiveMesh("Bookcase Upright R", PrimitiveType.Cube, f2Group,
                new Vector3(1.85f, f2Y + 1.35f, bz), new Vector3(0.12f, 2.70f, 0.48f), Quaternion.identity, matWood, true);
            for (int s = 0; s < 4; s++)
            {
                float sy = f2Y + 0.35f + s * 0.65f;
                CreatePrimitiveMesh($"Bookcase Shelf {s + 1}", PrimitiveType.Cube, f2Group,
                    new Vector3(0f, sy, bz), new Vector3(3.60f, 0.06f, 0.45f), Quaternion.identity, matWood, false);
            }
            // Dense Books & Specimen Jars
            Material[] bookMats = new[] { matBookRed, matBookGreen, matBookBlue, matBookGold };
            for (int b = 0; b < 20; b++)
            {
                float bx = -1.40f + b * 0.14f;
                Material bm = bookMats[b % bookMats.Length];
                CreatePrimitiveMesh($"Book S2_{b}", PrimitiveType.Cube, f2Group,
                    new Vector3(bx, f2Y + 1.15f, bz - 0.02f), new Vector3(0.085f, 0.28f, 0.24f), Quaternion.identity, bm, false);
            }
            for (int j = 0; j < 5; j++)
            {
                float jx = -1.20f + j * 0.60f;
                CreatePrimitiveMesh($"Specimen Jar {j + 1}", PrimitiveType.Cylinder, f2Group,
                    new Vector3(jx, f2Y + 2.45f, bz), new Vector3(0.18f, 0.18f, 0.18f), Quaternion.identity, matSpecimenJar, false);
            }

            // Low-Hanging Industrial Pendant Lamp with Warm Amber Glow
            var f2LightGo = new GameObject("Study Pendant Light");
            f2LightGo.transform.SetParent(f2Group, false);
            f2LightGo.transform.position = new Vector3(0f, f2Y + 2.30f, 0.35f);

            CreatePrimitiveMesh("Lamp Hood", PrimitiveType.Cylinder, f2LightGo.transform,
                new Vector3(0, 0.08f, 0), new Vector3(0.48f, 0.10f, 0.48f), Quaternion.identity, matIron, false);
            CreatePrimitiveMesh("Lamp Bulb", PrimitiveType.Sphere, f2LightGo.transform,
                new Vector3(0, -0.04f, 0), new Vector3(0.16f, 0.18f, 0.16f), Quaternion.identity, matLanternGlow, false);

            var f2Spot = f2LightGo.AddComponent<Light>();
            f2Spot.type = LightType.Spot;
            f2Spot.spotAngle = 72f;
            f2Spot.innerSpotAngle = 36f;
            f2Spot.range = 5.8f;
            f2Spot.intensity = 3.6f;
            f2Spot.color = new Color(1.0f, 0.74f, 0.40f);
            f2Spot.shadows = LightShadows.Soft;

            var f2Fill = new GameObject("Study Fill Light");
            f2Fill.transform.SetParent(f2LightGo.transform, false);
            var f2FillLight = f2Fill.AddComponent<Light>();
            f2FillLight.type = LightType.Point;
            f2FillLight.color = new Color(1.0f, 0.72f, 0.38f);
            f2FillLight.range = 5.2f;
            f2FillLight.intensity = 1.4f;

            // -------------------------------------------------------------
            // FLOOR 3: WATCHROOM & BATTERY / FUEL STAGING ROOM
            // -------------------------------------------------------------
            var f3Group = new GameObject("Floor 3 Interior Watchroom Furnishings").transform;
            f3Group.SetParent(towerRoot, false);

            float f3Y = 6.55f;

            // Heavy Timber Battery Bank Racks holding Vintage Glass Jar Cells
            Vector3 battPos = new Vector3(-1.20f, f3Y, 1.40f);
            Quaternion battRot = Quaternion.Euler(0, -25f, 0);
            CreatePrimitiveMesh("Battery Rack Frame", PrimitiveType.Cube, f3Group,
                battPos + battRot * new Vector3(0, 0.55f, 0), new Vector3(2.20f, 1.10f, 0.55f), battRot, matWood, true);
            for (int r = 0; r < 2; r++)
            {
                for (int c = 0; c < 4; c++)
                {
                    Vector3 cellP = battPos + battRot * new Vector3(-0.75f + c * 0.50f, 0.30f + r * 0.52f, 0f);
                    CreatePrimitiveMesh($"Battery Jar {r}_{c}", PrimitiveType.Cylinder, f3Group,
                        cellP, new Vector3(0.24f, 0.18f, 0.24f), Quaternion.identity, matSpecimenJar, false);
                    CreatePrimitiveMesh($"Lead Terminal {r}_{c}", PrimitiveType.Cylinder, f3Group,
                        cellP + Vector3.up * 0.19f, new Vector3(0.04f, 0.06f, 0.04f), Quaternion.identity, matIron, false);
                }
            }

            // Reserve Fuel Staging Plinth & Red Steel Fuel Jerry Cans
            Vector3 fuelPlinthPos = new Vector3(1.65f, f3Y + 0.12f, 0.85f);
            CreatePrimitiveMesh("Fuel Staging Plinth", PrimitiveType.Cube, f3Group,
                fuelPlinthPos, new Vector3(1.20f, 0.24f, 0.85f), Quaternion.identity, matIron, true);

            var matFuelRed = EnsureMaterial("MaritimeJerryCanRed", "Universal Render Pipeline/Lit");
            matFuelRed.SetColor("_BaseColor", new Color(0.72f, 0.16f, 0.14f));
            matFuelRed.SetFloat("_Smoothness", 0.45f);
            matFuelRed.SetFloat("_Metallic", 0.65f);

            for (int f = 0; f < 3; f++)
            {
                CreatePrimitiveMesh($"Reserve Fuel Can {f + 1}", PrimitiveType.Cube, f3Group,
                    fuelPlinthPos + new Vector3(-0.35f + f * 0.35f, 0.32f, 0f), new Vector3(0.24f, 0.42f, 0.36f), Quaternion.identity, matFuelRed, true);
            }

            // Maritime Barometer & Telegraph Station Workbench
            Vector3 benchPos = new Vector3(0f, f3Y, -1.65f);
            CreatePrimitiveMesh("Watchroom Workbench Top", PrimitiveType.Cube, f3Group,
                benchPos + new Vector3(0f, 0.75f, 0f), new Vector3(2.20f, 0.08f, 0.85f), Quaternion.identity, matWood, true);
            CreatePrimitiveMesh("Workbench Leg L", PrimitiveType.Cube, f3Group,
                benchPos + new Vector3(-0.95f, 0.36f, 0f), new Vector3(0.12f, 0.72f, 0.70f), Quaternion.identity, matWood, true);
            CreatePrimitiveMesh("Workbench Leg R", PrimitiveType.Cube, f3Group,
                benchPos + new Vector3(0.95f, 0.36f, 0f), new Vector3(0.12f, 0.72f, 0.70f), Quaternion.identity, matWood, true);

            // Brass Wall Barometer
            CreatePrimitiveMesh("Maritime Wall Barometer", PrimitiveType.Cylinder, f3Group,
                benchPos + new Vector3(0f, 1.45f, -0.40f), new Vector3(0.34f, 0.06f, 0.34f), Quaternion.Euler(90f, 0, 0), matBrass, false);

            // Telegraph Morse Instrument on bench
            CreatePrimitiveMesh("Telegraph Baseplate", PrimitiveType.Cube, f3Group,
                benchPos + new Vector3(-0.45f, 0.81f, 0f), new Vector3(0.32f, 0.04f, 0.18f), Quaternion.identity, matWood, false);
            CreatePrimitiveMesh("Telegraph Sounder Coil", PrimitiveType.Cylinder, f3Group,
                benchPos + new Vector3(-0.48f, 0.87f, 0.04f), new Vector3(0.08f, 0.09f, 0.08f), Quaternion.identity, matBrass, false);
            CreatePrimitiveMesh("Telegraph Key Lever", PrimitiveType.Cube, f3Group,
                benchPos + new Vector3(-0.38f, 0.85f, -0.02f), new Vector3(0.16f, 0.025f, 0.03f), Quaternion.Euler(0, 0, 6f), matBrass, false);

            // Warning Indicator Glow Lamps (Green & Red)
            CreatePrimitiveMesh("Watchroom Indicator Green", PrimitiveType.Sphere, f3Group,
                benchPos + new Vector3(0.65f, 1.15f, -0.38f), new Vector3(0.08f, 0.08f, 0.08f), Quaternion.identity, matIndicatorGreen, false);
            CreatePrimitiveMesh("Watchroom Indicator Red", PrimitiveType.Sphere, f3Group,
                benchPos + new Vector3(0.85f, 1.15f, -0.38f), new Vector3(0.08f, 0.08f, 0.08f), Quaternion.identity, matIndicatorRed, false);

            // Watchroom Bulkhead Lighting
            var f3LightGo = new GameObject("Watchroom Bulkhead Light");
            f3LightGo.transform.SetParent(f3Group, false);
            f3LightGo.transform.position = new Vector3(0f, f3Y + 3.05f, 0f);
            var f3Light = f3LightGo.AddComponent<Light>();
            f3Light.type = LightType.Point;
            f3Light.color = new Color(0.95f, 0.85f, 0.65f);
            f3Light.range = 6.2f;
            f3Light.intensity = 2.0f;

            // E. OUTER SPIRAL STAIRCASE (Side-Winding along outer stone wall)
            // -------------------------------------------------------------
            // Corridor Width: 1.35m (from r = 3.25m to r = 4.60m)
            // Flight 1: Floor 1 (225°) -> Floor 2 (75°), rising y = 0.15m to 3.35m
            // Flight 2: Floor 2 (105°) -> Floor 3 (315°), rising y = 3.35m to 6.55m
            // Flight 3: Floor 3 (345°) -> Lantern Gallery (165°), rising y = 6.55m to 9.75m (Final Half-Spiral!)
            BuildSpiralFlight(towerRoot, 1, 225f, 75f + 360f, 0.15f, 3.35f, 22);
            BuildSpiralFlight(towerRoot, 2, 105f, 315f, 3.35f, 6.55f, 22);
            BuildSpiralFlight(towerRoot, 3, 345f, 165f + 360f, 6.55f, 9.75f, 18);

            // F. TOP OPEN LANTERN ROOM & BEACON DECK (y = 9.75m)
            // -------------------------------------------------------------
            BuildLanternGalleryAndBeacon(towerRoot, 9.75f);

            // G. CONTINUOUS SMOOTH CURVED RAMP MESHCOLLIDER
            // -------------------------------------------------------------
            BuildContinuousSpiralRampMesh(towerRoot);

            // H. Caged Bulkhead Sconces along Spiral Stair Flight Wall
            Vector3[] bulkheadPositions = new[]
            {
                new Vector3(Mathf.Sin(330f * Mathf.Deg2Rad) * 4.90f, 1.80f, Mathf.Cos(330f * Mathf.Deg2Rad) * 4.90f),
                new Vector3(Mathf.Sin(30f * Mathf.Deg2Rad) * 4.90f, 2.70f, Mathf.Cos(30f * Mathf.Deg2Rad) * 4.90f),
                new Vector3(Mathf.Sin(210f * Mathf.Deg2Rad) * 4.90f, 5.00f, Mathf.Cos(210f * Mathf.Deg2Rad) * 4.90f),
                new Vector3(Mathf.Sin(270f * Mathf.Deg2Rad) * 4.90f, 5.90f, Mathf.Cos(270f * Mathf.Deg2Rad) * 4.90f),
                new Vector3(Mathf.Sin(90f * Mathf.Deg2Rad) * 4.90f, 8.20f, Mathf.Cos(90f * Mathf.Deg2Rad) * 4.90f)
            };

            for (int b = 0; b < bulkheadPositions.Length; b++)
            {
                var bhGroup = new GameObject($"Stair Bulkhead Sconce {b + 1}");
                bhGroup.transform.SetParent(towerRoot, false);
                bhGroup.transform.position = bulkheadPositions[b];

                CreatePrimitiveMesh("Bulkhead Baseplate", PrimitiveType.Cube, bhGroup.transform,
                    Vector3.zero, new Vector3(0.24f, 0.32f, 0.08f), Quaternion.identity, matIron, false);
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

        private static void BuildSpiralFlight(Transform parent, int flightIdx, float startDeg, float endDeg, float startY, float endY, int steps)
        {
            var flightGroup = new GameObject($"Outer Spiral Stair Flight {flightIdx}").transform;
            flightGroup.SetParent(parent, false);

            float rIn = 3.25f;
            float rOut = 4.60f;
            float stairW = rOut - rIn; // Exactly 1.35m clearance
            float rMid = (rIn + rOut) * 0.5f;

            Vector3 prevRailTop = Vector3.zero;

            for (int s = 0; s < steps; s++)
            {
                float t = s / (float)steps;
                float nextT = (s + 1) / (float)steps;

                float angA = Mathf.Lerp(startDeg, endDeg, t);
                float angB = Mathf.Lerp(startDeg, endDeg, nextT);
                float radA = angA * Mathf.Deg2Rad;
                float radB = angB * Mathf.Deg2Rad;
                float midRad = (radA + radB) * 0.5f;

                float yA = Mathf.Lerp(startY, endY, t);
                float yB = Mathf.Lerp(startY, endY, nextT);

                Vector3 stepPos = new Vector3(Mathf.Sin(midRad) * rMid, yA + 0.025f, Mathf.Cos(midRad) * rMid);
                Quaternion stepRot = Quaternion.Euler(0, (angA + angB) * 0.5f, 0);

                float tangLen = Mathf.Abs(radB - radA) * rMid * 1.05f;

                // Visual Step Tread (NO COLLIDER - Smooth character movement handled by continuous ramp MeshCollider)
                CreatePrimitiveMesh($"Tread {flightIdx}_{s}", PrimitiveType.Cube, flightGroup,
                    stepPos, new Vector3(tangLen, 0.05f, stairW), stepRot, matIron, false);

                // Visual Step Riser (NO COLLIDER)
                Vector3 riserPos = new Vector3(Mathf.Sin(radB) * rMid, (yA + yB) * 0.5f, Mathf.Cos(radB) * rMid);
                CreatePrimitiveMesh($"Riser {flightIdx}_{s}", PrimitiveType.Cube, flightGroup,
                    riserPos, new Vector3(0.04f, yB - yA, stairW), Quaternion.Euler(0, angB, 0), matIron, false);

                // Inner Baluster Rod (at r = 3.25m)
                Vector3 balPos = new Vector3(Mathf.Sin(radA) * rIn, yA, Mathf.Cos(radA) * rIn);
                Vector3 balTop = balPos + Vector3.up * 1.05f;
                CreatePrimitiveMesh($"Baluster {flightIdx}_{s}", PrimitiveType.Cylinder, flightGroup,
                    balPos + Vector3.up * 0.525f, new Vector3(0.035f, 0.525f, 0.035f), Quaternion.identity, matIron, false);

                // Continuous Following Handrail
                if (s > 0)
                {
                    Vector3 midRail = (prevRailTop + balTop) * 0.5f;
                    Vector3 diff = balTop - prevRailTop;
                    float rLen = diff.magnitude;
                    Quaternion rRot = Quaternion.LookRotation(diff);
                    CreatePrimitiveMesh($"Handrail {flightIdx}_{s}", PrimitiveType.Cylinder, flightGroup,
                        midRail, new Vector3(0.05f, rLen * 0.5f, 0.05f), rRot * Quaternion.Euler(90f, 0, 0), matIron, false);
                }
                prevRailTop = balTop;

                // Wall Cantilever Anchor Bracket into outer stone wall
                if (s % 2 == 0)
                {
                    Vector3 cbPos = new Vector3(Mathf.Sin(midRad) * (rOut + 0.18f), yA - 0.08f, Mathf.Cos(midRad) * (rOut + 0.18f));
                    CreatePrimitiveMesh($"Cantilever Bracket {flightIdx}_{s}", PrimitiveType.Cube, flightGroup,
                        cbPos, new Vector3(0.06f, 0.16f, 0.38f), stepRot, matIron, false);
                }
            }
        }

        private static void BuildLanternGalleryAndBeacon(Transform parent, float deckY)
        {
            var lanternGroup = new GameObject("Lantern Room & Beacon Deck Master").transform;
            lanternGroup.SetParent(parent, false);

            float glassR = 3.80f;
            float deckRadius = 4.85f;

            // 1. Central Circular Beacon Deck (y = 9.75m, radius 3.10m, matches central room radius)
            CreatePrimitiveMesh("Lantern Central Beacon Deck", PrimitiveType.Cylinder, lanternGroup,
                new Vector3(0, deckY - 0.08f, 0), new Vector3(3.10f * 2f, 0.16f, 3.10f * 2f), Quaternion.identity, matIron, true);

            // 2. Outer Balcony Walkway Ring (y = 9.75m, r = 3.10m to 4.85m)
            // Sector 195° to 345° has floor slabs.
            // Sector 345° to 195° is the OPEN STAIRWELL WELLHOLE where Flight 3 ascends from Floor 3 up to the Arrival Landing at 165°-195°!
            // Keeping 345° to 165° open gives complete vertical head clearance for the ascending spiral flight.
            int walkwaySteps = 12;
            float walkStart = 195f;
            float walkEnd = 345f;
            float walkSpan = walkEnd - walkStart;
            for (int w = 0; w < walkwaySteps; w++)
            {
                float angA = walkStart + (w * walkSpan / walkwaySteps);
                float angB = walkStart + ((w + 1) * walkSpan / walkwaySteps);
                float midAng = (angA + angB) * 0.5f;
                float midRad = midAng * Mathf.Deg2Rad;
                float midR = (3.10f + deckRadius) * 0.5f;
                float arcW = (midR * (angB - angA) * Mathf.Deg2Rad) * 1.05f;
                Vector3 wPos = new Vector3(Mathf.Sin(midRad) * midR, deckY - 0.08f, Mathf.Cos(midRad) * midR);
                Quaternion wRot = Quaternion.Euler(0, midAng, 0);

                CreatePrimitiveMesh($"Lantern Outer Walkway Segment {w}", PrimitiveType.Cube, lanternGroup,
                    wPos, new Vector3(arcW, 0.16f, (deckRadius - 3.10f) * 1.05f), wRot, matIron, true);
            }

            // Balustrade safety railing along the open stairwell wellhole on the inner rim (r = 3.10m, 345° to 165°)
            int wellPosts = 14;
            for (int wp = 0; wp <= wellPosts; wp++)
            {
                float ang = 345f + (wp * (180f / wellPosts));
                float rad = ang * Mathf.Deg2Rad;
                Vector3 pPos = new Vector3(Mathf.Sin(rad) * 3.10f, deckY, Mathf.Cos(rad) * 3.10f);
                CreatePrimitiveMesh($"Wellhole Baluster {wp}", PrimitiveType.Cylinder, lanternGroup,
                    pPos + Vector3.up * 0.55f, new Vector3(0.04f, 0.55f, 0.04f), Quaternion.identity, matIron, false);

                if (wp < wellPosts)
                {
                    float nxtAng = 345f + ((wp + 1) * (180f / wellPosts));
                    float midAng = (ang + nxtAng) * 0.5f;
                    float midRad = midAng * Mathf.Deg2Rad;
                    float segLen = Vector3.Distance(pPos, new Vector3(Mathf.Sin(nxtAng * Mathf.Deg2Rad) * 3.10f, deckY, Mathf.Cos(nxtAng * Mathf.Deg2Rad) * 3.10f));
                    Vector3 rMid = new Vector3(Mathf.Sin(midRad) * 3.10f, deckY + 1.05f, Mathf.Cos(midRad) * 3.10f);
                    CreatePrimitiveMesh($"Wellhole Rail {wp}", PrimitiveType.Cube, lanternGroup,
                        rMid, new Vector3(0.05f, 0.06f, segLen * 1.05f), Quaternion.Euler(0, midAng + 90f, 0), matBrass, false);
                }
            }

            // Outer Balcony Walkway Guardrail
            int railPosts = 24;
            for (int r = 0; r < railPosts; r++)
            {
                float ang = r * (360f / railPosts);
                float rad = ang * Mathf.Deg2Rad;
                Vector3 rPos = new Vector3(Mathf.Sin(rad) * (deckRadius - 0.15f), deckY, Mathf.Cos(rad) * (deckRadius - 0.15f));
                CreatePrimitiveMesh($"Balcony Rail Post {r}", PrimitiveType.Cylinder, lanternGroup,
                    rPos + Vector3.up * 0.55f, new Vector3(0.045f, 0.55f, 0.045f), Quaternion.identity, matIron, false);
            }
            CreatePrimitiveMesh("Balcony Rail Top Ring", PrimitiveType.Cylinder, lanternGroup,
                new Vector3(0, deckY + 1.10f, 0), new Vector3((deckRadius - 0.15f) * 2f, 0.05f, (deckRadius - 0.15f) * 2f), Quaternion.identity, matIron, false);

            // 3. Panoramic 360-degree Glass Lantern Windows & Bronze Mullions
            int paneCount = 16;
            for (int p = 0; p < paneCount; p++)
            {
                float ang = p * (360f / paneCount);
                float rad = ang * Mathf.Deg2Rad;
                Vector3 gPos = new Vector3(Mathf.Sin(rad) * glassR, deckY + 1.45f, Mathf.Cos(rad) * glassR);
                Quaternion gRot = Quaternion.Euler(0, ang, 0);

                // Bronze Vertical Mullion
                CreatePrimitiveMesh($"Lantern Mullion {p}", PrimitiveType.Cube, lanternGroup,
                    gPos, new Vector3(0.08f, 2.70f, 0.10f), gRot, matBrass, true);

                // Clear / Cold Window Glass Pane
                float tangW = 2f * Mathf.PI * glassR / paneCount;
                CreatePrimitiveMesh($"Lantern Glass Pane {p}", PrimitiveType.Cube, lanternGroup,
                    gPos, new Vector3(tangW, 2.65f, 0.03f), gRot, matColdWindow, true);
            }

            // 4. Copper Lantern Dome Roof Cap (y = 12.6m to 13.5m)
            CreatePrimitiveMesh("Copper Lantern Dome", PrimitiveType.Cylinder, lanternGroup,
                new Vector3(0, deckY + 2.85f, 0), new Vector3(glassR * 2.1f, 0.35f, glassR * 2.1f), Quaternion.identity, matSlate, false);
            CreatePrimitiveMesh("Copper Dome Cap", PrimitiveType.Sphere, lanternGroup,
                new Vector3(0, deckY + 3.45f, 0), new Vector3(glassR * 1.8f, 1.20f, glassR * 1.8f), Quaternion.identity, matSlate, false);

            // 5. Central Revolving Fresnel Lens Pedestal & Beacon Mechanism
            Vector3 beaconPos = new Vector3(0, deckY, 0);
            CreatePrimitiveMesh("Beacon Iron Pedestal", PrimitiveType.Cylinder, lanternGroup,
                beaconPos + Vector3.up * 0.45f, new Vector3(1.20f, 0.45f, 1.20f), Quaternion.identity, matIron, true);
            CreatePrimitiveMesh("Fresnel Brass Housing", PrimitiveType.Cylinder, lanternGroup,
                beaconPos + Vector3.up * 1.05f, new Vector3(0.95f, 0.40f, 0.95f), Quaternion.identity, matBrass, true);
            CreatePrimitiveMesh("Beacon Concentric Glass Lens", PrimitiveType.Cylinder, lanternGroup,
                beaconPos + Vector3.up * 1.25f, new Vector3(0.85f, 0.60f, 0.85f), Quaternion.identity, matSpecimenJar, false);

            // 6. Relocate Narrative Gameplay Anchors directly onto the Beacon Deck!
            var lampGo = GameObject.Find("Lamp");
            if (lampGo)
            {
                lampGo.transform.position = new Vector3(0, deckY + 1.10f, 0);
            }

            var crankGo = GameObject.Find("Crank Station");
            if (crankGo)
            {
                crankGo.transform.position = new Vector3(-1.80f, deckY + 0.15f, 1.20f);
                var bc = crankGo.GetComponent<BoxCollider>();
                if (bc)
                {
                    bc.size = new Vector3(0.85f, 1.10f, 0.85f);
                    bc.center = new Vector3(0, 0.55f, 0);
                }
            }

            // Sweeping Beacon Light atop Fresnel lens
            var beaconLightGo = new GameObject("Beacon Chamber Radiant Light");
            beaconLightGo.transform.SetParent(lanternGroup, false);
            beaconLightGo.transform.position = new Vector3(0, deckY + 1.25f, 0);
            var bLight = beaconLightGo.AddComponent<Light>();
            bLight.type = LightType.Point;
            bLight.color = new Color(1.0f, 0.92f, 0.75f);
            bLight.range = 14f;
            bLight.intensity = 5.5f;
            bLight.shadows = LightShadows.Soft;
        }

        private static void BuildContinuousSpiralRampMesh(Transform parent)
        {
            var rampGo = new GameObject("Continuous Spiral Stair Ramp MeshCollider");
            rampGo.transform.SetParent(parent, false);
            rampGo.tag = "Stone";

            var verts = new List<Vector3>();
            var tris = new List<int>();

            float rIn = 3.20f;
            float rOut = 4.65f;

            void SampleSpiralPath(float startDeg, float endDeg, float startY, float endY, int samples)
            {
                for (int i = 0; i <= samples; i++)
                {
                    float t = i / (float)samples;
                    float deg = Mathf.Lerp(startDeg, endDeg, t);
                    float rad = deg * Mathf.Deg2Rad;
                    float y = Mathf.Lerp(startY, endY, t);

                    Vector3 pIn = new Vector3(Mathf.Sin(rad) * rIn, y, Mathf.Cos(rad) * rIn);
                    Vector3 pOut = new Vector3(Mathf.Sin(rad) * rOut, y, Mathf.Cos(rad) * rOut);

                    int baseIdx = verts.Count;
                    verts.Add(pIn);
                    verts.Add(pOut);

                    if (baseIdx >= 2)
                    {
                        int prevIn = baseIdx - 2;
                        int prevOut = baseIdx - 1;
                        int curIn = baseIdx;
                        int curOut = baseIdx + 1;

                        // Quad: double-sided faces to guarantee raycasts from above and below hit cleanly
                        // Top face (normals up)
                        tris.Add(prevIn);
                        tris.Add(prevOut);
                        tris.Add(curIn);

                        tris.Add(prevOut);
                        tris.Add(curOut);
                        tris.Add(curIn);

                        // Bottom face
                        tris.Add(prevIn);
                        tris.Add(curIn);
                        tris.Add(prevOut);

                        tris.Add(prevOut);
                        tris.Add(curIn);
                        tris.Add(curOut);
                    }
                }
            }

            // Continuous sample path:
            // 1. Floor 1 Landing (195° to 225°, y = 0.15m)
            SampleSpiralPath(195f, 225f, 0.15f, 0.15f, 6);
            // 2. Flight 1 (225° to 435° / 75°, y = 0.15m -> 3.35m)
            SampleSpiralPath(225f, 435f, 0.15f, 3.35f, 42);
            // 3. Floor 2 Landing (75° to 105°, y = 3.35m)
            SampleSpiralPath(75f, 105f, 3.35f, 3.35f, 6);
            // 4. Flight 2 (105° to 315°, y = 3.35m -> 6.55m)
            SampleSpiralPath(105f, 315f, 3.35f, 6.55f, 42);
            // 5. Floor 3 Landing (315° to 345°, y = 6.55m)
            SampleSpiralPath(315f, 345f, 6.55f, 6.55f, 6);
            // 6. Flight 3 Final Half-Spiral (345° to 525° / 165°, y = 6.55m -> 9.75m)
            SampleSpiralPath(345f, 525f, 6.55f, 9.75f, 36);
            // 7. Lantern Gallery Arrival (165° to 195°, y = 9.75m)
            SampleSpiralPath(165f, 195f, 9.75f, 9.75f, 6);

            var mesh = new Mesh();
            mesh.name = "ContinuousSpiralRampMesh";
            mesh.vertices = verts.ToArray();
            mesh.triangles = tris.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            var mc = rampGo.AddComponent<MeshCollider>();
            mc.sharedMesh = mesh;
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
            else if (type == PrimitiveType.Cylinder)
            {
                var cap = go.GetComponent<CapsuleCollider>();
                if (cap) Object.DestroyImmediate(cap);
                var mf = go.GetComponent<MeshFilter>();
                if (mf && mf.sharedMesh)
                {
                    var mc = go.AddComponent<MeshCollider>();
                    mc.sharedMesh = mf.sharedMesh;
                }
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
                if (archL)
                {
                    var r = archL.GetComponent<Renderer>(); if (r) r.enabled = false;
                    foreach (var c in archL.GetComponentsInChildren<Collider>(true)) c.enabled = false;
                }
                var archT = GameObject.Find($"Cellar Arch Top {i}");
                if (archT)
                {
                    var r = archT.GetComponent<Renderer>(); if (r) r.enabled = false;
                    foreach (var c in archT.GetComponentsInChildren<Collider>(true)) c.enabled = false;
                }
            }

            var oldTimberFloor = GameObject.Find("Timber Floor");
            if (oldTimberFloor)
            {
                foreach (var c in oldTimberFloor.GetComponentsInChildren<Collider>(true)) c.enabled = false;
            }

            var oldCellarStairs = GameObject.Find("Cellar Stair");
            if (oldCellarStairs)
            {
                foreach (var c in oldCellarStairs.GetComponentsInChildren<Collider>(true)) c.enabled = false;
            }

            var lockedCellarDoor = GameObject.Find("Locked Cellar Door");
            if (lockedCellarDoor)
            {
                var mod = lockedCellarDoor.GetComponent<Unity.AI.Navigation.NavMeshModifier>();
                if (!mod) mod = lockedCellarDoor.AddComponent<Unity.AI.Navigation.NavMeshModifier>();
                mod.ignoreFromBuild = true;
            }

            var cellarRoot = new GameObject("Underground Cellar & Secret Room Master").transform;
            cellarRoot.SetParent(parent, false);

            // A. Heavy Timber-Framed Arched Entrance Portal (Matching Panel 1 Reference)
            // Positioned at cottage North wall at x = -8.00m, z = -5.00m, ground y = 0.05m
            Vector3 cellarPortalPos = new Vector3(-8.00f, 0.05f, -5.00f);
            var entranceGroup = new GameObject("Cellar Entrance Portal & Vestibule").transform;
            entranceGroup.SetParent(cellarRoot, false);

            // Heavy rough-hewn oak vertical posts framing doorway (1.30m wide opening along X)
            float postX_L = -8.00f - 0.75f; // x = -8.75f (West)
            float postX_R = -8.00f + 0.75f; // x = -7.25f (East)

            var postL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            postL.name = "Timber Portal Post Left";
            postL.transform.SetParent(entranceGroup, false);
            postL.transform.position = new Vector3(postX_L, 1.25f, cellarPortalPos.z);
            postL.transform.localScale = new Vector3(0.28f, 2.45f, 0.28f);
            postL.GetComponent<Renderer>().sharedMaterial = matWood;

            var postR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            postR.name = "Timber Portal Post Right";
            postR.transform.SetParent(entranceGroup, false);
            postR.transform.position = new Vector3(postX_R, 1.25f, cellarPortalPos.z);
            postR.transform.localScale = new Vector3(0.28f, 2.45f, 0.28f);
            postR.GetComponent<Renderer>().sharedMaterial = matWood;

            // Heavy Timber Overhead Lintel
            var lintel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lintel.name = "Timber Portal Lintel";
            lintel.transform.SetParent(entranceGroup, false);
            lintel.transform.position = new Vector3(cellarPortalPos.x, 2.50f, cellarPortalPos.z);
            lintel.transform.localScale = new Vector3(1.80f, 0.30f, 0.32f);
            lintel.GetComponent<Renderer>().sharedMaterial = matWood;

            // Diagonal Knee Braces in upper corners (Panel 1)
            var braceL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            braceL.name = "Timber Knee Brace Left";
            braceL.transform.SetParent(entranceGroup, false);
            braceL.transform.position = new Vector3(postX_L + 0.22f, 2.25f, cellarPortalPos.z);
            braceL.transform.localScale = new Vector3(0.45f, 0.22f, 0.22f);
            braceL.transform.rotation = Quaternion.Euler(0, 0, 45f);
            braceL.GetComponent<Renderer>().sharedMaterial = matWood;
            Object.DestroyImmediate(braceL.GetComponent<Collider>());

            var braceR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            braceR.name = "Timber Knee Brace Right";
            braceR.transform.SetParent(entranceGroup, false);
            braceR.transform.position = new Vector3(postX_R - 0.22f, 2.25f, cellarPortalPos.z);
            braceR.transform.localScale = new Vector3(0.45f, 0.22f, 0.22f);
            braceR.transform.rotation = Quaternion.Euler(0, 0, -45f);
            braceR.GetComponent<Renderer>().sharedMaterial = matWood;
            Object.DestroyImmediate(braceR.GetComponent<Collider>());

            // Wrought-iron studded corner brackets on lintel
            for (int b = -1; b <= 1; b += 2)
            {
                var bracket = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bracket.name = $"Iron Stud Bracket {b}";
                bracket.transform.SetParent(entranceGroup, false);
                bracket.transform.position = new Vector3(cellarPortalPos.x + b * 0.68f, 2.48f, cellarPortalPos.z - 0.05f);
                bracket.transform.localScale = new Vector3(0.22f, 0.22f, 0.04f);
                bracket.GetComponent<Renderer>().sharedMaterial = matIron;
                Object.DestroyImmediate(bracket.GetComponent<Collider>());
            }

            // Wall-mounted Warm Hurricane Lantern Sconce beside doorway (Panel 1)
            var wallSconce = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            wallSconce.name = "Cellar Portal Warm Lantern Sconce";
            wallSconce.transform.SetParent(entranceGroup, false);
            wallSconce.transform.position = new Vector3(-7.00f, 1.85f, -5.15f);
            wallSconce.transform.localScale = new Vector3(0.18f, 0.38f, 0.18f);
            wallSconce.GetComponent<Renderer>().sharedMaterial = matLanternGlow;
            Object.DestroyImmediate(wallSconce.GetComponent<Collider>());

            var sconceArm = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sconceArm.name = "Sconce Iron Arm";
            sconceArm.transform.SetParent(wallSconce.transform, false);
            sconceArm.transform.localPosition = new Vector3(0, 0.08f, -0.10f);
            sconceArm.transform.localScale = new Vector3(0.04f, 0.04f, 0.25f);
            sconceArm.GetComponent<Renderer>().sharedMaterial = matIron;
            Object.DestroyImmediate(sconceArm.GetComponent<Collider>());

            var sconceLightGo = new GameObject("Portal Lantern Light");
            sconceLightGo.transform.SetParent(wallSconce.transform, false);
            var sconceLight = sconceLightGo.AddComponent<Light>();
            sconceLight.type = LightType.Point;
            sconceLight.color = new Color(1.0f, 0.70f, 0.32f);
            sconceLight.intensity = 2.4f;
            sconceLight.range = 5.5f;
            sconceLight.shadows = LightShadows.Soft;

            // \"WARNING: Cellar Access\" enamel plaque mounted beside door
            var warningPlaque = GameObject.CreatePrimitive(PrimitiveType.Quad);
            warningPlaque.name = "Warning Cellar Access Plaque";
            warningPlaque.transform.SetParent(entranceGroup, false);
            warningPlaque.transform.position = new Vector3(-8.95f, 1.55f, -5.15f);
            warningPlaque.transform.localScale = new Vector3(0.38f, 0.48f, 1f);
            warningPlaque.transform.rotation = Quaternion.Euler(0, 180f, 0);
            warningPlaque.GetComponent<Renderer>().sharedMaterial = matWarningSign;
            Object.DestroyImmediate(warningPlaque.GetComponent<Collider>());

            // Wooden Coat Pegs with Hanging Oilskin Keeper's Slickers (Panel 1)
            var coatRack = GameObject.CreatePrimitive(PrimitiveType.Cube);
            coatRack.name = "Wooden Coat Rack Bar";
            coatRack.transform.SetParent(entranceGroup, false);
            coatRack.transform.position = new Vector3(-6.80f, 1.70f, -5.15f);
            coatRack.transform.localScale = new Vector3(1.10f, 0.12f, 0.06f);
            coatRack.GetComponent<Renderer>().sharedMaterial = matWood;
            Object.DestroyImmediate(coatRack.GetComponent<Collider>());

            var yellowSlickerMat = AssetDatabase.LoadAssetAtPath<Material>($"{MatRoot}/Yellow Oilskin.mat") ?? matWood;
            var darkCoatMat = AssetDatabase.LoadAssetAtPath<Material>($"{MatRoot}/Dark Wool Navy.mat") ?? matIron;

            var c1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            c1.name = "Hanging Keeper Slicker 1";
            c1.transform.SetParent(entranceGroup, false);
            c1.transform.position = coatRack.transform.position + new Vector3(-0.32f, -0.55f, -0.06f);
            c1.transform.localScale = new Vector3(0.28f, 0.95f, 0.18f);
            c1.GetComponent<Renderer>().sharedMaterial = yellowSlickerMat;
            Object.DestroyImmediate(c1.GetComponent<Collider>());

            var c2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            c2.name = "Hanging Keeper Slicker 2";
            c2.transform.SetParent(entranceGroup, false);
            c2.transform.position = coatRack.transform.position + new Vector3(0.12f, -0.58f, -0.06f);
            c2.transform.localScale = new Vector3(0.30f, 1.02f, 0.20f);
            c2.GetComponent<Renderer>().sharedMaterial = darkCoatMat;
            Object.DestroyImmediate(c2.GetComponent<Collider>());

            // Pair of muddy boots on floor beside doorway
            for (int b = -1; b <= 1; b += 2)
            {
                var boot = GameObject.CreatePrimitive(PrimitiveType.Cube);
                boot.name = $"Keeper Muddy Boot {b}";
                boot.transform.SetParent(entranceGroup, false);
                boot.transform.position = new Vector3(-6.80f + b * 0.14f, 0.15f, -5.22f);
                boot.transform.localScale = new Vector3(0.12f, 0.28f, 0.24f);
                boot.GetComponent<Renderer>().sharedMaterial = matIron;
                Object.DestroyImmediate(boot.GetComponent<Collider>());
            }

            // B. Straight Wide Flight of Stone Steps Descending into Earth (Panel 1)
            // Descends Northward from z = -5.00m, y = 0.05m down to z = -0.50m, y = -3.20m at x = -8.00m
            var stairGroup = new GameObject("Descending Cellar Stone Stairs").transform;
            stairGroup.SetParent(cellarRoot, false);

            const int cellarSteps = 18;
            float stepRise = 3.25f / cellarSteps;   // ~0.18m
            float stepRun = 4.50f / cellarSteps;    // ~0.25m along Z
            float stepWidth = 1.30f; // Width along X

            for (int s = 0; s < cellarSteps; s++)
            {
                float t = s / (float)cellarSteps;
                float sz = Mathf.Lerp(-5.00f + stepRun * 0.5f, -0.50f - stepRun * 0.5f, t);
                float sy = Mathf.Lerp(0.05f - stepRise * 0.5f, -3.20f + stepRise * 0.5f, t);

                var step = GameObject.CreatePrimitive(PrimitiveType.Cube);
                step.name = $"Cellar Stone Step {s:00}";
                step.transform.SetParent(stairGroup, false);
                step.transform.position = new Vector3(-8.00f, sy, sz);
                step.transform.localScale = new Vector3(stepWidth, stepRise, stepRun * 1.15f);
                step.GetComponent<Renderer>().sharedMaterial = matStone;
                Object.DestroyImmediate(step.GetComponent<Collider>()); // Smooth ramp collider handles navigation

                // Wet footprint decals descending steps (Panel 1)
                if (s % 2 == 0)
                {
                    var fp = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    fp.name = $"Step Footprint {s}";
                    fp.transform.SetParent(step.transform, false);
                    fp.transform.localPosition = new Vector3(0.12f * (s % 4 == 0 ? 1 : -1), 0.52f, 0);
                    fp.transform.rotation = Quaternion.Euler(90f, 0, 0);
                    fp.transform.localScale = new Vector3(0.32f, 0.65f, 1f);
                    fp.GetComponent<Renderer>().sharedMaterial = matWetFootprint;
                    Object.DestroyImmediate(fp.GetComponent<Collider>());
                }
            }

            // Smooth Ramp MeshCollider for effortless navigation without step snagging
            var rampGo = new GameObject("Cellar Stair Ramp Collider");
            rampGo.transform.SetParent(stairGroup, false);
            var rampMesh = new Mesh();
            float halfW = stepWidth * 0.55f;
            Vector3 r0 = new Vector3(-8.00f + halfW, 0.08f, -5.00f);
            Vector3 r1 = new Vector3(-8.00f - halfW, 0.08f, -5.00f);
            Vector3 r2 = new Vector3(-8.00f + halfW, -3.18f, -0.45f);
            Vector3 r3 = new Vector3(-8.00f - halfW, -3.18f, -0.45f);
            rampMesh.vertices = new[] { r0, r1, r2, r3 };
            rampMesh.triangles = new[] { 0, 1, 2, 2, 1, 3, 2, 1, 0, 3, 1, 2 };
            rampMesh.RecalculateNormals();
            var rampCollider = rampGo.AddComponent<MeshCollider>();
            rampCollider.sharedMesh = rampMesh;

            // Wall-mounted Iron Pipe Handrail along Right-Hand Wall (East wall: x = -8.00f + halfW) (Panel 1)
            var handrailGroup = new GameObject("Iron Wall Handrail").transform;
            handrailGroup.SetParent(stairGroup, false);

            Vector3 railStart = new Vector3(-8.00f + halfW - 0.06f, 1.05f, -5.00f);
            Vector3 railEnd = new Vector3(-8.00f + halfW - 0.06f, -2.20f, -0.50f);
            Vector3 railDiff = railEnd - railStart;
            float railLen = railDiff.magnitude;

            var railBar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            railBar.name = "Handrail Pipe Bar";
            railBar.transform.SetParent(handrailGroup, false);
            railBar.transform.position = (railStart + railEnd) * 0.5f;
            railBar.transform.localScale = new Vector3(0.055f, railLen * 0.5f, 0.055f);
            railBar.transform.rotation = Quaternion.LookRotation(railDiff) * Quaternion.Euler(90f, 0, 0);
            railBar.GetComponent<Renderer>().sharedMaterial = matIron;
            Object.DestroyImmediate(railBar.GetComponent<Collider>());

            // Wall mounting brackets for iron pipe
            for (int b = 0; b < 4; b++)
            {
                float bt = b / 3f;
                Vector3 bp = Vector3.Lerp(railStart, railEnd, bt);
                var bracket = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bracket.name = $"Handrail Bracket {b}";
                bracket.transform.SetParent(handrailGroup, false);
                bracket.transform.position = bp + new Vector3(0.06f, 0, 0);
                bracket.transform.localScale = new Vector3(0.12f, 0.04f, 0.04f);
                bracket.GetComponent<Renderer>().sharedMaterial = matIron;
                Object.DestroyImmediate(bracket.GetComponent<Collider>());
            }

            // Enclosing stone walls flanking the stair flight
            var stairWallW = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stairWallW.name = "Stair Tunnel Wall West";
            stairWallW.transform.SetParent(stairGroup, false);
            stairWallW.transform.position = new Vector3(-8.00f - halfW - 0.12f, -1.25f, -2.75f);
            stairWallW.transform.localScale = new Vector3(0.25f, 3.80f, 4.80f);
            stairWallW.GetComponent<Renderer>().sharedMaterial = matStone;

            var stairWallE = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stairWallE.name = "Stair Tunnel Wall East";
            stairWallE.transform.SetParent(stairGroup, false);
            stairWallE.transform.position = new Vector3(-8.00f + halfW + 0.12f, -1.25f, -2.75f);
            stairWallE.transform.localScale = new Vector3(0.25f, 3.80f, 4.80f);
            stairWallE.GetComponent<Renderer>().sharedMaterial = matStone;

            // Sloping stone tunnel ceiling over stairs
            var stairCeil = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stairCeil.name = "Stair Tunnel Sloping Ceiling";
            stairCeil.transform.SetParent(stairGroup, false);
            stairCeil.transform.position = new Vector3(-8.00f, 1.20f, -2.75f);
            stairCeil.transform.localScale = new Vector3(stepWidth + 0.35f, 0.25f, 5.10f);
            stairCeil.transform.rotation = Quaternion.Euler(-35.8f, 0, 0);
            stairCeil.GetComponent<Renderer>().sharedMaterial = matStone;
            Object.DestroyImmediate(stairCeil.GetComponent<Collider>());

            // Bare Hanging Incandescent Bulb in Stairwell
            var bulbGroup = new GameObject("Dangling Stairwell Bulb").transform;
            bulbGroup.SetParent(stairGroup, false);
            bulbGroup.position = new Vector3(-8.00f, -0.80f, -2.75f);

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

            // C. Low Subterranean Corridor (Romanesque Stone Arches, Enclosed Walls, Puddles - Panel 2)
            var corridorGroup = new GameObject("Low Subterranean Corridor").transform;
            corridorGroup.SetParent(cellarRoot, false);

            Vector3 corridorCenter = new Vector3(-8.00f, -3.20f, 2.00f);

            // Flagstone floor
            var cellarFloor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cellarFloor.name = "Cellar Flagstone Floor";
            cellarFloor.transform.SetParent(corridorGroup, false);
            cellarFloor.transform.position = corridorCenter;
            cellarFloor.transform.localScale = new Vector3(3.60f, 0.15f, 5.60f);
            cellarFloor.GetComponent<Renderer>().sharedMaterial = matStone;

            // Arched stone ceiling vault
            var ceilingVault = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ceilingVault.name = "Cellar Stone Vault Ceiling";
            ceilingVault.transform.SetParent(corridorGroup, false);
            ceilingVault.transform.position = corridorCenter + new Vector3(0, 2.6f, 0);
            ceilingVault.transform.localScale = new Vector3(3.60f, 0.20f, 5.00f);
            ceilingVault.GetComponent<Renderer>().sharedMaterial = matStone;

            // Enclosing corridor side walls
            var wallW = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallW.name = "Corridor Wall West";
            wallW.transform.SetParent(corridorGroup, false);
            wallW.transform.position = corridorCenter + new Vector3(-1.80f, 1.3f, 0);
            wallW.transform.localScale = new Vector3(0.25f, 2.6f, 5.00f);
            wallW.GetComponent<Renderer>().sharedMaterial = matStone;

            var wallE = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallE.name = "Corridor Wall East";
            wallE.transform.SetParent(corridorGroup, false);
            wallE.transform.position = corridorCenter + new Vector3(1.80f, 1.3f, 0);
            wallE.transform.localScale = new Vector3(0.25f, 2.6f, 5.00f);
            wallE.GetComponent<Renderer>().sharedMaterial = matStone;

            // Romanesque Arched Vaults along corridor (Panel 2)
            for (int a = 0; a < 3; a++)
            {
                float az = 0.50f + a * 1.50f;
                var arch = new GameObject($"Romanesque Arch {a}").transform;
                arch.SetParent(corridorGroup, false);
                arch.position = new Vector3(-8.00f, -3.2f, az);

                // Left pillar (West)
                var pilL = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pilL.transform.SetParent(arch, false);
                pilL.transform.localPosition = new Vector3(-1.65f, 1.25f, 0);
                pilL.transform.localScale = new Vector3(0.35f, 2.5f, 0.35f);
                pilL.GetComponent<Renderer>().sharedMaterial = matStone;

                // Right pillar (East)
                var pilR = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pilR.transform.SetParent(arch, false);
                pilR.transform.localPosition = new Vector3(1.65f, 1.25f, 0);
                pilR.transform.localScale = new Vector3(0.35f, 2.5f, 0.35f);
                pilR.GetComponent<Renderer>().sharedMaterial = matStone;

                // Transverse Arch Rib
                var archRib = GameObject.CreatePrimitive(PrimitiveType.Cube);
                archRib.transform.SetParent(arch, false);
                archRib.transform.localPosition = new Vector3(0, 2.45f, 0);
                archRib.transform.localScale = new Vector3(3.40f, 0.32f, 0.38f);
                archRib.GetComponent<Renderer>().sharedMaterial = matStone;
            }

            // Stacked wooden nautical storage crates & barrels along walls (Panel 2)
            var crate1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            crate1.name = "Cellar Storage Crate 1";
            crate1.transform.SetParent(corridorGroup, false);
            crate1.transform.position = new Vector3(-9.20f, -2.75f, 1.20f);
            crate1.transform.localScale = new Vector3(0.85f, 0.75f, 0.85f);
            crate1.GetComponent<Renderer>().sharedMaterial = matWood;

            var crate2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            crate2.name = "Cellar Storage Crate 2";
            crate2.transform.SetParent(corridorGroup, false);
            crate2.transform.position = new Vector3(-9.20f, -2.10f, 1.20f);
            crate2.transform.localScale = new Vector3(0.70f, 0.60f, 0.70f);
            crate2.transform.rotation = Quaternion.Euler(0, 14f, 0);
            crate2.GetComponent<Renderer>().sharedMaterial = matWood;

            var barrel1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            barrel1.name = "Cellar Storage Barrel 1";
            barrel1.transform.SetParent(corridorGroup, false);
            barrel1.transform.position = new Vector3(-6.65f, -2.70f, 2.80f);
            barrel1.transform.localScale = new Vector3(0.65f, 0.50f, 0.65f);
            barrel1.GetComponent<Renderer>().sharedMaterial = matWood;

            // Iron water pipes and electrical conduits along West wall (Panel 2)
            var pipe = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pipe.name = "Cellar Iron Conduit Pipe";
            pipe.transform.SetParent(corridorGroup, false);
            pipe.transform.position = corridorCenter + new Vector3(-1.70f, 1.6f, 0);
            pipe.transform.localScale = new Vector3(0.06f, 2.50f, 0.06f);
            pipe.transform.rotation = Quaternion.identity;
            pipe.GetComponent<Renderer>().sharedMaterial = matIron;
            Object.DestroyImmediate(pipe.GetComponent<Collider>());

            // Puddle reflections on corridor floor (Panel 2)
            for (int p = 0; p < 3; p++)
            {
                var puddle = GameObject.CreatePrimitive(PrimitiveType.Quad);
                puddle.name = $"Corridor Wet Puddle {p}";
                puddle.transform.SetParent(corridorGroup, false);
                puddle.transform.position = new Vector3(-8.00f + (p % 2 == 0 ? 0.25f : -0.25f), -3.12f, 0.80f + p * 1.20f);
                puddle.transform.rotation = Quaternion.Euler(90f, 0, 0);
                puddle.transform.localScale = new Vector3(0.75f, 1.10f, 1f);
                puddle.GetComponent<Renderer>().sharedMaterial = matPuddle;
                Object.DestroyImmediate(puddle.GetComponent<Collider>());
            }

            // Wet footprints leading along corridor toward secret room doorway (Panel 2)
            for (int fpIdx = 0; fpIdx < 5; fpIdx++)
            {
                var fp = GameObject.CreatePrimitive(PrimitiveType.Quad);
                fp.name = $"Corridor Footprint {fpIdx}";
                fp.transform.SetParent(corridorGroup, false);
                fp.transform.position = new Vector3(-8.00f + (fpIdx % 2 == 0 ? 0.15f : -0.15f), -3.11f, 0.20f + fpIdx * 0.90f);
                fp.transform.rotation = Quaternion.Euler(90f, 0, 0);
                fp.transform.localScale = new Vector3(0.30f, 0.65f, 1f);
                fp.GetComponent<Renderer>().sharedMaterial = matWetFootprint;
                Object.DestroyImmediate(fp.GetComponent<Collider>());
            }

            // D. Hidden Cellar Entryway & Recorded Room (Reel-to-Reel, Ledger, Maps, Radio)
            // Layout reference: Image 11 Panel 4 & Image 12 Panels 2-3
            Vector3 secretRoomPos = new Vector3(-8.00f, -3.20f, 7.00f);
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

            // East Wall (Right flank)
            var srWallE = GameObject.CreatePrimitive(PrimitiveType.Cube);
            srWallE.name = "Secret Room Wall East";
            srWallE.transform.SetParent(secretRoomGroup, false);
            srWallE.transform.position = secretRoomPos + new Vector3(2.35f, 1.3f, 0);
            srWallE.transform.localScale = new Vector3(0.25f, 2.6f, 4.8f);
            srWallE.GetComponent<Renderer>().sharedMaterial = matStone;

            // South Wall with Arched Secret Entryway connecting to corridor (Panel 3)
            var srWallSJambL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            srWallSJambL.name = "Secret Room Doorway Jamb L";
            srWallSJambL.transform.SetParent(secretRoomGroup, false);
            srWallSJambL.transform.position = secretRoomPos + new Vector3(-1.65f, 1.3f, -2.35f);
            srWallSJambL.transform.localScale = new Vector3(1.6f, 2.6f, 0.25f);
            srWallSJambL.GetComponent<Renderer>().sharedMaterial = matStone;

            var srWallSJambR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            srWallSJambR.name = "Secret Room Doorway Jamb R";
            srWallSJambR.transform.SetParent(secretRoomGroup, false);
            srWallSJambR.transform.position = secretRoomPos + new Vector3(1.65f, 1.3f, -2.35f);
            srWallSJambR.transform.localScale = new Vector3(1.6f, 2.6f, 0.25f);
            srWallSJambR.GetComponent<Renderer>().sharedMaterial = matStone;

            var srWallSArch = GameObject.CreatePrimitive(PrimitiveType.Cube);
            srWallSArch.name = "Secret Room Doorway Arched Header";
            srWallSArch.transform.SetParent(secretRoomGroup, false);
            srWallSArch.transform.position = secretRoomPos + new Vector3(0, 2.35f, -2.35f);
            srWallSArch.transform.localScale = new Vector3(1.80f, 0.50f, 0.28f);
            srWallSArch.GetComponent<Renderer>().sharedMaterial = matStone;

            // Heavy Timber Door Slightly Ajar (Panel 3)
            var srDoor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            srDoor.name = "Secret Room Timber Door (Ajar)";
            srDoor.transform.SetParent(secretRoomGroup, false);
            srDoor.transform.position = secretRoomPos + new Vector3(0.40f, 1.15f, -2.30f);
            srDoor.transform.localScale = new Vector3(1.10f, 2.15f, 0.08f);
            srDoor.transform.rotation = Quaternion.Euler(0, 35f, 0);
            srDoor.GetComponent<Renderer>().sharedMaterial = matWood;
            Object.DestroyImmediate(srDoor.GetComponent<Collider>());

            // Subterranean NavMesh Links guaranteeing continuous navigation across stairs and portal
            var stairLinkGo = new GameObject("Cellar Stair NavMesh Link");
            stairLinkGo.transform.SetParent(cellarRoot, false);
            var stairLink = stairLinkGo.AddComponent<Unity.AI.Navigation.NavMeshLink>();
            stairLink.startPoint = new Vector3(-8.15f, 0.10f, -4.65f);
            stairLink.endPoint = new Vector3(-8.00f, -2.90f, -0.50f);
            stairLink.width = 1.30f;
            stairLink.bidirectional = true;

            var doorwayLinkGo = new GameObject("Secret Room Doorway NavMesh Link");
            doorwayLinkGo.transform.SetParent(cellarRoot, false);
            var doorwayLink = doorwayLinkGo.AddComponent<Unity.AI.Navigation.NavMeshLink>();
            doorwayLink.startPoint = new Vector3(-8.00f, -3.07f, 3.92f);
            doorwayLink.endPoint = new Vector3(-8.00f, -3.07f, 5.80f);
            doorwayLink.width = 1.40f;
            doorwayLink.bidirectional = true;

            // Heavy Timber Workbench Desk (against North wall)
            Vector3 deskPos = secretRoomPos + new Vector3(0, 0, 1.70f);
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

            var rtrRec = rtrObj.GetComponent<ReelToReelRecording>();
            if (!rtrRec) rtrRec = rtrObj.AddComponent<ReelToReelRecording>();
            var rtrAudio = rtrObj.GetComponent<AudioSource>();
            if (!rtrAudio) rtrAudio = rtrObj.AddComponent<AudioSource>();
            rtrAudio.spatialBlend = 1.0f;
            rtrAudio.minDistance = 1.5f;
            rtrAudio.maxDistance = 16.0f;
            rtrAudio.rolloffMode = AudioRolloffMode.Logarithmic;
            rtrAudio.playOnAwake = false;

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

            // 3. The Harbor Board Ledger Book (Center-Left on Desk, directly under Lamp - Panel 4)
            // Open Navigation Chart of Harrow Rock spread under ledger and photo (Panel 4)
            var chartSpread = GameObject.CreatePrimitive(PrimitiveType.Quad);
            chartSpread.name = "Workbench Nautical Chart Spread";
            chartSpread.transform.SetParent(secretRoomGroup, false);
            chartSpread.transform.position = deskPos + new Vector3(0.05f, 0.846f, -0.10f);
            chartSpread.transform.localScale = new Vector3(1.10f, 0.75f, 1f);
            chartSpread.transform.rotation = Quaternion.Euler(90f, 0, 0);
            chartSpread.GetComponent<Renderer>().sharedMaterial = matNauticalChart;
            Object.DestroyImmediate(chartSpread.GetComponent<Collider>());

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

            // Brass drafting calipers / dividers resting on chart (Panel 4)
            Vector3 calPos = deskPos + new Vector3(0.38f, 0.852f, -0.18f);
            var calHinge = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            calHinge.name = "Drafting Caliper Hinge";
            calHinge.transform.SetParent(secretRoomGroup, false);
            calHinge.transform.position = calPos;
            calHinge.transform.localScale = new Vector3(0.035f, 0.006f, 0.035f);
            calHinge.GetComponent<Renderer>().sharedMaterial = matBrass;
            Object.DestroyImmediate(calHinge.GetComponent<Collider>());

            var calLegL = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            calLegL.name = "Drafting Caliper Leg L";
            calLegL.transform.SetParent(secretRoomGroup, false);
            calLegL.transform.position = calPos + new Vector3(-0.035f, 0.003f, 0.07f);
            calLegL.transform.localScale = new Vector3(0.008f, 0.08f, 0.008f);
            calLegL.transform.rotation = Quaternion.Euler(0, -18f, 90f);
            calLegL.GetComponent<Renderer>().sharedMaterial = matBrass;
            Object.DestroyImmediate(calLegL.GetComponent<Collider>());

            var calLegR = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            calLegR.name = "Drafting Caliper Leg R";
            calLegR.transform.SetParent(secretRoomGroup, false);
            calLegR.transform.position = calPos + new Vector3(0.035f, 0.003f, 0.07f);
            calLegR.transform.localScale = new Vector3(0.008f, 0.08f, 0.008f);
            calLegR.transform.rotation = Quaternion.Euler(0, 180f + 18f, 90f);
            calLegR.GetComponent<Renderer>().sharedMaterial = matBrass;
            Object.DestroyImmediate(calLegR.GetComponent<Collider>());

            // Heavy-Duty Vintage Brass Flashlight on desk edge (Panel 4)
            Vector3 torchPos = deskPos + new Vector3(-0.25f, 0.865f, -0.38f);
            var torchBody = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            torchBody.name = "Vintage Brass Flashlight Body";
            torchBody.transform.SetParent(secretRoomGroup, false);
            torchBody.transform.position = torchPos;
            torchBody.transform.localScale = new Vector3(0.052f, 0.22f, 0.052f);
            torchBody.transform.rotation = Quaternion.Euler(0, 75f, 90f);
            torchBody.GetComponent<Renderer>().sharedMaterial = matBrass;
            Object.DestroyImmediate(torchBody.GetComponent<Collider>());

            var torchBezel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            torchBezel.name = "Flashlight Head Bezel";
            torchBezel.transform.SetParent(secretRoomGroup, false);
            torchBezel.transform.position = torchPos + new Vector3(0.12f, 0, 0.032f);
            torchBezel.transform.localScale = new Vector3(0.075f, 0.04f, 0.075f);
            torchBezel.transform.rotation = Quaternion.Euler(0, 75f, 90f);
            torchBezel.GetComponent<Renderer>().sharedMaterial = matBrass;
            Object.DestroyImmediate(torchBezel.GetComponent<Collider>());

            var torchLens = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            torchLens.name = "Flashlight Glass Lens";
            torchLens.transform.SetParent(secretRoomGroup, false);
            torchLens.transform.position = torchPos + new Vector3(0.14f, 0, 0.037f);
            torchLens.transform.localScale = new Vector3(0.070f, 0.008f, 0.070f);
            torchLens.transform.rotation = Quaternion.Euler(0, 75f, 90f);
            torchLens.GetComponent<Renderer>().sharedMaterial = matSpecimenJar;
            Object.DestroyImmediate(torchLens.GetComponent<Collider>());

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
            chartBacking.transform.position = deskPos + new Vector3(0, 1.55f, 0.50f);
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

        private static void SetupPlayerFlashlightAndAudio(Transform parent)
        {
            // 1. Setup Voiceover & Spatial Audio Director
            var voGo = GameObject.Find("Voiceover Director");
            if (!voGo)
            {
                voGo = new GameObject("Voiceover Director");
            }
            if (!voGo.GetComponent<VoiceoverDirector>())
            {
                voGo.AddComponent<VoiceoverDirector>();
            }

            // 2. Setup Player Flashlight Rig
            var player = Object.FindAnyObjectByType<FirstPersonController>();
            if (player)
            {
                var cam = player.GetComponentInChildren<Camera>()?.transform;
                if (cam)
                {
                    // Ensure AudioListener
                    if (!cam.GetComponent<AudioListener>())
                    {
                        cam.gameObject.AddComponent<AudioListener>();
                    }

                    var flTransform = cam.Find("Player_Flashlight");
                    GameObject flGo = flTransform ? flTransform.gameObject : new GameObject("Player_Flashlight");
                    flGo.transform.SetParent(cam, false);
                    flGo.transform.localPosition = new Vector3(0.22f, -0.18f, 0.45f);
                    flGo.transform.localRotation = Quaternion.Euler(1.5f, -1.8f, 0f);

                    var flCtrl = flGo.GetComponent<FlashlightController>();
                    if (!flCtrl) flCtrl = flGo.AddComponent<FlashlightController>();

                    var oldModel = flGo.transform.Find("Flashlight_1940s_Ribbed_Mesh");
                    if (oldModel) Object.DestroyImmediate(oldModel.gameObject);

                    var model = FlashlightModelBuilder.BuildFlashlightModel(flGo.transform, out var bulbRend, out var bulbMat);
                    flCtrl.SetupLightsAndAudio();
                    flCtrl.AssignModelReferences(model.transform, bulbRend, bulbMat);
                }
            }

            // 3. Attach ReelToReelRecording in Cellar Hidden Room
            var rtr = GameObject.Find("Marta reel-to-reel recorder");
            if (!rtr)
            {
                var env = GameObject.Find("Harrow Rock Master Environment");
                if (env)
                {
                    var transforms = env.GetComponentsInChildren<Transform>(true);
                    foreach (var t in transforms)
                    {
                        if (t.name == "Marta reel-to-reel recorder")
                        {
                            rtr = t.gameObject;
                            break;
                        }
                    }
                }
            }

            if (rtr)
            {
                var rtrRec = rtr.GetComponent<ReelToReelRecording>();
                if (!rtrRec) rtrRec = rtr.AddComponent<ReelToReelRecording>();
                var rtrAudio = rtr.GetComponent<AudioSource>();
                if (!rtrAudio) rtrAudio = rtr.AddComponent<AudioSource>();
                rtrAudio.spatialBlend = 1.0f;
                rtrAudio.minDistance = 1.5f;
                rtrAudio.maxDistance = 16.0f;
                rtrAudio.rolloffMode = AudioRolloffMode.Logarithmic;
                rtrAudio.playOnAwake = false;
                EditorUtility.SetDirty(rtrAudio);
                EditorUtility.SetDirty(rtrRec);
            }

            // Boatman 3D Audio
            var boatman = Object.FindAnyObjectByType<Boatman>();
            if (boatman)
            {
                var bmAudio = boatman.GetComponent<AudioSource>();
                if (!bmAudio) bmAudio = boatman.gameObject.AddComponent<AudioSource>();
                bmAudio.spatialBlend = 1.0f;
                bmAudio.minDistance = 2.0f;
                bmAudio.maxDistance = 15.0f;
                bmAudio.rolloffMode = AudioRolloffMode.Logarithmic;
                bmAudio.playOnAwake = false;
                EditorUtility.SetDirty(bmAudio);
                EditorUtility.SetDirty(boatman);
            }

            // 4. Ensure Radio Voiceover
            var radio = Object.FindAnyObjectByType<Radio>();
            if (radio)
            {
                var radioAudio = radio.GetComponent<AudioSource>();
                if (!radioAudio) radioAudio = radio.gameObject.AddComponent<AudioSource>();
                radioAudio.spatialBlend = 1.0f;
                radioAudio.minDistance = 1.5f;
                radioAudio.maxDistance = 14.0f;
                EditorUtility.SetDirty(radioAudio);
                EditorUtility.SetDirty(radio);
            }

            // 5. Ensure CellarHum Voiceover
            var hum = Object.FindAnyObjectByType<CellarHum>();
            if (hum)
            {
                var humAudio = hum.GetComponent<AudioSource>();
                if (!humAudio) humAudio = hum.gameObject.AddComponent<AudioSource>();
                humAudio.spatialBlend = 1.0f;
                humAudio.minDistance = 3.0f;
                humAudio.maxDistance = 24.0f;
                EditorUtility.SetDirty(humAudio);
                EditorUtility.SetDirty(hum);
            }
            {
                var humAudio = hum.GetComponent<AudioSource>();
                if (!humAudio) humAudio = hum.gameObject.AddComponent<AudioSource>();
                humAudio.spatialBlend = 1.0f;
                humAudio.minDistance = 3.0f;
                humAudio.maxDistance = 24.0f;
            }
        }
    }
}
