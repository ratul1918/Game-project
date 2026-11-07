using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace TheLightHouse.Editor
{
    public static class CinematicVolumeSetup
    {
        private const string ProfileRoot = "Assets/_Project/Settings";

        [MenuItem("TheLightHouse/Setup Cinematic Post-Processing Volumes")]
        public static void SetupAllVolumes()
        {
            Directory.CreateDirectory(ProfileRoot);

            SetupProfile("Volume_Dusk", 0.10f, 10f, -10f, new Color(0.92f, 0.95f, 0.98f), -5f, -2f, 0.90f, 0.25f, 0.22f, 0.08f);
            SetupProfile("Volume_Night", -0.15f, 12f, -14f, new Color(0.88f, 0.93f, 0.98f), -10f, -3f, 0.88f, 0.28f, 0.26f, 0.09f);
            SetupProfile("Volume_Storm", -0.25f, 14f, -16f, new Color(0.86f, 0.92f, 0.96f), -12f, -4f, 0.88f, 0.30f, 0.28f, 0.10f);
            SetupProfile("Volume_Finale", -0.30f, 15f, -18f, new Color(0.84f, 0.90f, 0.96f), -14f, -4f, 0.85f, 0.32f, 0.30f, 0.10f);
            SetupProfile("Volume_Dawn", 0.15f, 10f, -8f, new Color(0.96f, 0.96f, 0.98f), -3f, -1f, 0.90f, 0.22f, 0.18f, 0.07f);

            AssignProfilesToOpenScene();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[CinematicVolumeSetup] All 5 atmospheric post-processing volume profiles configured and serialized successfully.");
        }

        public static void AssignProfilesToOpenScene()
        {
            AssignProfileToGameObject("Dusk Volume", "Volume_Dusk");
            AssignProfileToGameObject("Night Volume", "Volume_Night");
            AssignProfileToGameObject("Storm Volume", "Volume_Storm");
            AssignProfileToGameObject("Finale Volume", "Volume_Finale");
            AssignProfileToGameObject("Dawn Volume", "Volume_Dawn");
        }

        private static void AssignProfileToGameObject(string gameObjectName, string profileAssetName)
        {
            var go = GameObject.Find(gameObjectName);
            if (!go) return;
            var vol = go.GetComponent<Volume>();
            if (!vol) vol = go.AddComponent<Volume>();

            string path = $"{ProfileRoot}/{profileAssetName}.asset";
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
            if (profile)
            {
                vol.sharedProfile = profile;
                EditorUtility.SetDirty(vol);
            }
        }

        private static void SetupProfile(
            string profileName,
            float exposure,
            float contrast,
            float saturation,
            Color filter,
            float whiteBalanceTemp,
            float whiteBalanceTint,
            float bloomThreshold,
            float bloomIntensity,
            float vignetteIntensity,
            float grainIntensity)
        {
            string path = $"{ProfileRoot}/{profileName}.asset";
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
            if (!profile)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, path);
            }

            var subAssets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var sub in subAssets)
            {
                if (sub != profile && sub is VolumeComponent vc)
                {
                    Object.DestroyImmediate(vc, true);
                }
            }
            profile.components.Clear();

            var tone = profile.Add<Tonemapping>(true);
            tone.mode.Override(TonemappingMode.ACES);
            tone.name = "Tonemapping";
            AssetDatabase.AddObjectToAsset(tone, profile);

            var ca = profile.Add<ColorAdjustments>(true);
            ca.postExposure.Override(exposure);
            ca.contrast.Override(contrast);
            ca.saturation.Override(saturation);
            ca.colorFilter.Override(filter);
            ca.name = "ColorAdjustments";
            AssetDatabase.AddObjectToAsset(ca, profile);

            var wb = profile.Add<WhiteBalance>(true);
            wb.temperature.Override(whiteBalanceTemp);
            wb.tint.Override(whiteBalanceTint);
            wb.name = "WhiteBalance";
            AssetDatabase.AddObjectToAsset(wb, profile);

            var bloom = profile.Add<Bloom>(true);
            bloom.threshold.Override(bloomThreshold);
            bloom.intensity.Override(bloomIntensity);
            bloom.scatter.Override(0.55f);
            bloom.tint.Override(new Color(1.0f, 0.90f, 0.78f));
            bloom.name = "Bloom";
            AssetDatabase.AddObjectToAsset(bloom, profile);

            var vig = profile.Add<Vignette>(true);
            vig.intensity.Override(vignetteIntensity);
            vig.smoothness.Override(0.35f);
            vig.color.Override(new Color(0.01f, 0.02f, 0.03f));
            vig.name = "Vignette";
            AssetDatabase.AddObjectToAsset(vig, profile);

            var grain = profile.Add<FilmGrain>(true);
            grain.type.Override(FilmGrainLookup.Thin1);
            grain.intensity.Override(grainIntensity);
            grain.response.Override(0.70f);
            grain.name = "FilmGrain";
            AssetDatabase.AddObjectToAsset(grain, profile);

            EditorUtility.SetDirty(profile);
        }
    }
}
