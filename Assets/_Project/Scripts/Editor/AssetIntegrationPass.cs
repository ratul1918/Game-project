using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheLightHouse.Editor
{
    public static class AssetIntegrationPass
    {
        private const string PbrTexRoot = "Assets/_Project/Art/Textures/PBR";
        private const string RefMatRoot = "Assets/_Project/Materials/Reference";
        private const string ModelRoot = "Assets/_Project/Art/Models";
        private const string AudioRoot = "Assets/_Project/Audio";

        public static void ApplyToCurrentScene()
        {
            UpgradeReferenceMaterialsWithPbr();
            var scene = SceneManager.GetActiveScene();
            IntegrateSceneAssets(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[AssetIntegrationPass] Applied asset integration to active scene: {scene.name}");
        }

        public static void ApplyToAllScenes()
        {
            UpgradeReferenceMaterialsWithPbr();

            string[] scenePaths = new[]
            {
                "Assets/_Project/Scenes/MainMenu.unity",
                "Assets/_Project/Scenes/Act1.unity",
                "Assets/_Project/Scenes/Act2.unity",
                "Assets/_Project/Scenes/Act3.unity",
                "Assets/_Project/Scenes/Act4.unity",
                "Assets/_Project/Scenes/Act5.unity"
            };

            foreach (var path in scenePaths)
            {
                if (!File.Exists(path)) continue;
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                IntegrateSceneAssets(scene);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log($"[AssetIntegrationPass] Integrated assets into {scene.name}");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[AssetIntegrationPass] Completed asset integration for all scenes.");
        }

        public static void UpgradeReferenceMaterialsWithPbr()
        {
            AssignPbrMaps("Salt Plaster", "LighthouseStone", 3f, 3f, 0.05f, 0.45f);
            AssignPbrMaps("Old Timber", "WeatheredPlanks", 2.5f, 2.5f, 0.0f, 0.40f);
            AssignPbrMaps("Oxidised Iron", "RustedIron", 2f, 2f, 0.65f, 0.35f);
            AssignPbrMaps("Wet Shale", "CoastalRock", 5f, 5f, 0.1f, 0.35f);
            AssetDatabase.SaveAssets();
        }

        private static void AssignPbrMaps(string matName, string pbrFolder, float tileX, float tileY, float metallic, float smoothness)
        {
            string matPath = $"{RefMatRoot}/{matName}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (!mat) return;

            string diffPath = $"{PbrTexRoot}/{pbrFolder}/{pbrFolder}_diff.jpg";
            string norPath = $"{PbrTexRoot}/{pbrFolder}/{pbrFolder}_nor.jpg";
            string aoPath = $"{PbrTexRoot}/{pbrFolder}/{pbrFolder}_ao.jpg";

            // Ensure normal map importer is set
            if (File.Exists(norPath))
            {
                var importer = AssetImporter.GetAtPath(norPath) as TextureImporter;
                if (importer != null && importer.textureType != TextureImporterType.NormalMap)
                {
                    importer.textureType = TextureImporterType.NormalMap;
                    importer.SaveAndReimport();
                }
            }

            var diff = AssetDatabase.LoadAssetAtPath<Texture2D>(diffPath);
            var nor = AssetDatabase.LoadAssetAtPath<Texture2D>(norPath);
            var ao = AssetDatabase.LoadAssetAtPath<Texture2D>(aoPath);

            if (diff)
            {
                mat.SetTexture("_BaseMap", diff);
                mat.SetTextureScale("_BaseMap", new Vector2(tileX, tileY));
            }

            if (nor)
            {
                mat.SetTexture("_BumpMap", nor);
                mat.SetTextureScale("_BumpMap", new Vector2(tileX, tileY));
                mat.EnableKeyword("_NORMALMAP");
            }

            if (ao)
            {
                mat.SetTexture("_OcclusionMap", ao);
                mat.SetTextureScale("_OcclusionMap", new Vector2(tileX, tileY));
                mat.SetFloat("_OcclusionStrength", 1.0f);
            }

            mat.SetFloat("_Metallic", metallic);
            mat.SetFloat("_Smoothness", smoothness);
            EditorUtility.SetDirty(mat);
        }

        private static void IntegrateSceneAssets(Scene scene)
        {
            IntegrateAmbientAudio(scene);
            IntegrateRealProps(scene);
        }

        private static void IntegrateAmbientAudio(Scene scene)
        {
            var existing = GameObject.Find("Ambient Audio");
            AmbientAudioDirector director;
            if (existing)
            {
                director = existing.GetComponent<AmbientAudioDirector>() ?? existing.AddComponent<AmbientAudioDirector>();
            }
            else
            {
                var go = new GameObject("Ambient Audio");
                director = go.AddComponent<AmbientAudioDirector>();
            }

            var ocean = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioRoot}/Ambience_Ocean_Waves.ogg");
            var rain = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioRoot}/Ambience_Rain_Storm.ogg");
            var foghorn = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioRoot}/Sfx_Lighthouse_Foghorn.ogg");
            var radio = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioRoot}/Sfx_Radio_Static.ogg");

            director.ConfigureClips(ocean, rain, foghorn, radio);
            EditorUtility.SetDirty(director);
        }

        private static void IntegrateRealProps(Scene scene)
        {
            var barrelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{ModelRoot}/MaritimeBarrel/MaritimeBarrel.fbx");
            var rockPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{ModelRoot}/CoastalRock_01/CoastalRock_01.fbx");
            var lampPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{ModelRoot}/IndustrialHangingLamp/IndustrialHangingLamp.fbx");

            var propsRoot = GameObject.Find("High Quality Props");
            if (!propsRoot)
            {
                propsRoot = new GameObject("High Quality Props");
            }

            // 1. Maritime Barrels near dock and under stairs
            if (barrelPrefab && propsRoot.transform.Find("Dock Maritime Barrel 1") == null)
            {
                var b1 = Object.Instantiate(barrelPrefab, propsRoot.transform);
                b1.name = "Dock Maritime Barrel 1";
                b1.transform.position = new Vector3(-3.8f, 0.25f, -31.5f);
                b1.transform.localScale = Vector3.one * 1.25f;
                b1.transform.rotation = Quaternion.Euler(0, 42f, 0);

                var b2 = Object.Instantiate(barrelPrefab, propsRoot.transform);
                b2.name = "Dock Maritime Barrel 2";
                b2.transform.position = new Vector3(-3.2f, 0.25f, -31.8f);
                b2.transform.localScale = Vector3.one * 1.25f;
                b2.transform.rotation = Quaternion.Euler(0, 115f, 0);

                var b3 = Object.Instantiate(barrelPrefab, propsRoot.transform);
                b3.name = "Tower Vestibule Barrel";
                b3.transform.position = new Vector3(3.2f, 0.25f, 1.2f);
                b3.transform.localScale = Vector3.one * 1.25f;
                b3.transform.rotation = Quaternion.Euler(0, -65f, 0);
            }

            // 2. Coastal Photogrammetry Rocks at shoreline (placed well away from boat berth at -5.8, -29.8)
            var rockMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Models/CoastalRock_01/M_CoastalRock_01.mat");
            if (rockPrefab)
            {
                // Remove any high-poly mud blobs/rocks blocking the dock arrival on the left (Ref: Screenshots 15, 16)
                var existingR1 = propsRoot.transform.Find("Shoreline Coastal Rock 1");
                if (existingR1) Object.DestroyImmediate(existingR1.gameObject);

                var existingR2 = propsRoot.transform.Find("Shoreline Coastal Rock 2");
                if (existingR2) Object.DestroyImmediate(existingR2.gameObject);
            }

            // 3. Vintage Industrial Hanging Lamps
            if (lampPrefab && propsRoot.transform.Find("Vestibule Vintage Lamp") == null)
            {
                var l1 = Object.Instantiate(lampPrefab, propsRoot.transform);
                l1.name = "Vestibule Vintage Lamp";
                l1.transform.position = new Vector3(0f, 3.1f, 0f);
                l1.transform.localScale = Vector3.one * 0.75f;

                var l2 = Object.Instantiate(lampPrefab, propsRoot.transform);
                l2.name = "Kitchen Vintage Lamp";
                l2.transform.position = new Vector3(-4.5f, 2.7f, -4.5f);
                l2.transform.localScale = Vector3.one * 0.75f;
            }
        }
    }
}
