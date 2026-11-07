using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace TheLightHouse.Editor
{
    public static class LevelPreviewCapture
    {
        private const string PreviewDir = "docs/level-previews";

        public struct PreviewShot
        {
            public string Name;
            public Vector3 Position;
            public Vector3 Target;
            public float Fov;

            public PreviewShot(string name, Vector3 pos, Vector3 target, float fov = 65f)
            {
                Name = name;
                Position = pos;
                Target = target;
                Fov = fov;
            }
        }

        public static PreviewShot[] GetCurrentShots()
        {
            var settings = InteriorKitBuilder.EnsureSettings();
            int totalLevels = 3 + settings.stairOnlyLevels; // e.g. 6 levels = 19.2m
            float totalH = totalLevels * settings.floorHeight;

            return new[]
            {
                new PreviewShot("dock-approach", new Vector3(0f, 1.4f, -27f), new Vector3(0f, 2.0f, -12f), 65f),
                new PreviewShot("courtyard", new Vector3(0f, 1.6f, -13f), new Vector3(0f, 2.5f, -4f), 65f),
                new PreviewShot("tower-exterior", new Vector3(-11f, 2.0f, -20f), new Vector3(0f, totalH * 0.6f, 0f), 62f),
                new PreviewShot("entry-vestibule", new Vector3(0f, 1.6f, -3.8f), new Vector3(-1.0f, 1.6f, -0.5f), 70f),
                new PreviewShot("floor-02-corridor", new Vector3(0f, 4.8f, -2.8f), new Vector3(-1.5f, 4.8f, 0.5f), 70f),
                new PreviewShot("floor-03-corridor", new Vector3(0f, 8.0f, -2.8f), new Vector3(1.5f, 8.0f, 0.5f), 70f),
                new PreviewShot("cranes-room", new Vector3(-2.0f, 4.8f, -0.6f), new Vector3(-3.3f, 4.2f, 0.4f), 70f),
                new PreviewShot("lantern-room", new Vector3(-1.8f, totalH + 1.6f, -1.8f), new Vector3(0f, totalH + 1.4f, 0f), 70f),
                new PreviewShot("cellar-stair", new Vector3(-2.2f, 0.8f, -1.2f), new Vector3(-2.2f, -3.5f, 5.0f), 65f),
                new PreviewShot("cellar-corridor", new Vector3(-2.2f, -3.6f, 6.5f), new Vector3(-2.2f, -3.6f, 20.0f), 70f),
                new PreviewShot("hidden-room", new Vector3(-2.2f, -3.6f, 29.8f), new Vector3(-2.6f, -4.0f, 27.2f), 70f),
                new PreviewShot("shoreline", new Vector3(-16f, 1.4f, -18f), new Vector3(0f, 0.5f, -32f), 65f)
            };
        }

        [MenuItem("TheLightHouse/Capture All Previews")]
        public static void CaptureAll()
        {
            Directory.CreateDirectory(PreviewDir);
            var shots = GetCurrentShots();
            foreach (var shot in shots)
            {
                CaptureShot(shot);
            }
            AssetDatabase.Refresh();
            Debug.Log($"[LevelPreviewCapture] Captured all {shots.Length} preview shots to {PreviewDir}/");
        }

        public static string CaptureShot(PreviewShot shot, int width = 1280, int height = 720)
        {
            Directory.CreateDirectory(PreviewDir);
            var camGo = new GameObject("PreviewCameraTemp");
            var cam = camGo.AddComponent<Camera>();
            var camData = camGo.AddComponent<UniversalAdditionalCameraData>();
            camData.renderPostProcessing = true;
            cam.fieldOfView = shot.Fov;
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 250f;
            cam.clearFlags = CameraClearFlags.Skybox;
            camGo.transform.position = shot.Position;
            camGo.transform.LookAt(shot.Target);

            var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32)
            {
                antiAliasing = 1
            };
            var prevActive = RenderTexture.active;
            var tex = new Texture2D(width, height, TextureFormat.RGB24, false);

            try
            {
                cam.targetTexture = rt;
                cam.Render();
                RenderTexture.active = rt;
                tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                tex.Apply();

                byte[] bytes = tex.EncodeToPNG();
                string path = Path.Combine(PreviewDir, $"{shot.Name}.png");
                File.WriteAllBytes(path, bytes);
                Debug.Log($"[LevelPreviewCapture] Saved preview: {path}");
                return path;
            }
            finally
            {
                cam.targetTexture = null;
                RenderTexture.active = prevActive;
                Object.DestroyImmediate(tex);
                Object.DestroyImmediate(rt);
                Object.DestroyImmediate(camGo);
            }
        }
    }
}
