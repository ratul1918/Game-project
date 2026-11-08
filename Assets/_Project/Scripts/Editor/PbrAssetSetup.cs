using System.IO;
using UnityEditor;
using UnityEngine;

namespace TheLightHouse.Editor
{
    public static class PbrAssetSetup
    {
        private const string PbrTexRoot = "Assets/_Project/Art/Textures/PBR";
        private const string PbrMatRoot = "Assets/_Project/Materials/PBR";

        public static void SetupAll()
        {
            if (!Directory.Exists(PbrMatRoot))
            {
                Directory.CreateDirectory(PbrMatRoot);
                AssetDatabase.Refresh();
            }

            SetupPbrMaterial("LighthouseStone", "PBR_LighthouseStone", 4f, 4f, 0.05f, 0.45f);
            SetupPbrMaterial("CoastalRock", "PBR_CoastalRock", 6f, 6f, 0.1f, 0.35f);
            SetupPbrMaterial("WeatheredPlanks", "PBR_WeatheredPlanks", 3f, 3f, 0.0f, 0.4f);
            SetupPbrMaterial("RustedIron", "PBR_RustedIron", 2f, 2f, 0.65f, 0.35f);
            SetupPbrMaterial("WeatheredPlaster", "PBR_WeatheredPlaster", 3f, 3f, 0.0f, 0.25f);

            SetupModelMaterials();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[PbrAssetSetup] All PBR materials, textures, and models successfully configured.");
        }

        private static void SetupPbrMaterial(string folderName, string matName, float tileX, float tileY, float metallic, float smoothness)
        {
            string texDir = $"{PbrTexRoot}/{folderName}";
            string diffPath = $"{texDir}/{folderName}_diff.jpg";
            string norPath = $"{texDir}/{folderName}_nor.jpg";
            string aoPath = $"{texDir}/{folderName}_ao.jpg";

            // Configure Normal Map texture importer
            if (File.Exists(norPath))
            {
                var importer = AssetImporter.GetAtPath(norPath) as TextureImporter;
                if (importer != null && importer.textureType != TextureImporterType.NormalMap)
                {
                    importer.textureType = TextureImporterType.NormalMap;
                    importer.SaveAndReimport();
                }
            }

            var diffTex = AssetDatabase.LoadAssetAtPath<Texture2D>(diffPath);
            var norTex = AssetDatabase.LoadAssetAtPath<Texture2D>(norPath);
            var aoTex = AssetDatabase.LoadAssetAtPath<Texture2D>(aoPath);

            Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (!litShader) litShader = Shader.Find("Standard");

            string matPath = $"{PbrMatRoot}/{matName}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (!mat)
            {
                mat = new Material(litShader);
                AssetDatabase.CreateAsset(mat, matPath);
            }
            else
            {
                mat.shader = litShader;
            }

            if (diffTex)
            {
                mat.SetTexture("_BaseMap", diffTex);
                mat.SetTextureScale("_BaseMap", new Vector2(tileX, tileY));
            }

            if (norTex)
            {
                mat.SetTexture("_BumpMap", norTex);
                mat.SetTextureScale("_BumpMap", new Vector2(tileX, tileY));
                mat.EnableKeyword("_NORMALMAP");
            }

            if (aoTex)
            {
                mat.SetTexture("_OcclusionMap", aoTex);
                mat.SetTextureScale("_OcclusionMap", new Vector2(tileX, tileY));
                mat.SetFloat("_OcclusionStrength", 1.0f);
            }

            mat.SetFloat("_Metallic", metallic);
            mat.SetFloat("_Smoothness", smoothness);

            EditorUtility.SetDirty(mat);
        }

        private static void SetupModelMaterials()
        {
            // Configure MaritimeBarrel material
            string barrelFbx = "Assets/_Project/Art/Models/MaritimeBarrel/MaritimeBarrel.fbx";
            string barrelDiff = "Assets/_Project/Art/Models/MaritimeBarrel/barrel_03_diff_1k.jpg";
            if (File.Exists(barrelDiff))
            {
                string barrelMatPath = "Assets/_Project/Art/Models/MaritimeBarrel/M_MaritimeBarrel.mat";
                var mat = AssetDatabase.LoadAssetAtPath<Material>(barrelMatPath);
                Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
                if (!mat)
                {
                    mat = new Material(litShader);
                    AssetDatabase.CreateAsset(mat, barrelMatPath);
                }
                var diff = AssetDatabase.LoadAssetAtPath<Texture2D>(barrelDiff);
                if (diff) mat.SetTexture("_BaseMap", diff);
                mat.SetFloat("_Metallic", 0.3f);
                mat.SetFloat("_Smoothness", 0.35f);
                EditorUtility.SetDirty(mat);
            }

            // Configure CoastalRock_01 material
            string rockDiff = "Assets/_Project/Art/Models/CoastalRock_01/coast_rocks_01_diff_1k.jpg";
            if (File.Exists(rockDiff))
            {
                string rockMatPath = "Assets/_Project/Art/Models/CoastalRock_01/M_CoastalRock_01.mat";
                var mat = AssetDatabase.LoadAssetAtPath<Material>(rockMatPath);
                Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
                if (!mat)
                {
                    mat = new Material(litShader);
                    AssetDatabase.CreateAsset(mat, rockMatPath);
                }
                var diff = AssetDatabase.LoadAssetAtPath<Texture2D>(rockDiff);
                if (diff) mat.SetTexture("_BaseMap", diff);
                mat.SetFloat("_Metallic", 0.05f);
                mat.SetFloat("_Smoothness", 0.25f);
                EditorUtility.SetDirty(mat);
            }

            // Configure IndustrialHangingLamp material
            string lampDiff = "Assets/_Project/Art/Models/IndustrialHangingLamp/hanging_industrial_lamp_diff_1k.jpg";
            if (File.Exists(lampDiff))
            {
                string lampMatPath = "Assets/_Project/Art/Models/IndustrialHangingLamp/M_IndustrialLamp.mat";
                var mat = AssetDatabase.LoadAssetAtPath<Material>(lampMatPath);
                Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
                if (!mat)
                {
                    mat = new Material(litShader);
                    AssetDatabase.CreateAsset(mat, lampMatPath);
                }
                var diff = AssetDatabase.LoadAssetAtPath<Texture2D>(lampDiff);
                if (diff) mat.SetTexture("_BaseMap", diff);
                mat.SetFloat("_Metallic", 0.8f);
                mat.SetFloat("_Smoothness", 0.45f);
                EditorUtility.SetDirty(mat);
            }
        }
    }
}
