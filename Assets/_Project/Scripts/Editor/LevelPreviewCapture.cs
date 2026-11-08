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
            return new[]
            {
                new PreviewShot("dock-approach", new Vector3(0f, 1.4f, -27f), new Vector3(0f, 2.0f, -12f), 65f),
                new PreviewShot("courtyard", new Vector3(-6.0f, 1.6f, -14f), new Vector3(-4.0f, 3.5f, -5f), 65f),
                new PreviewShot("tower-exterior", new Vector3(8.0f, 2.5f, -28.0f), new Vector3(0.0f, 7.5f, -2.0f), 65f),
                new PreviewShot("entry-vestibule", new Vector3(-1.20f, 1.60f, -4.80f), new Vector3(-3.50f, 1.50f, -1.80f), 70f),
                new PreviewShot("tower-stair-spiral", new Vector3(-2.4f, 0.85f, -2.8f), new Vector3(-3.8f, 1.5f, 0.2f), 70f),
                new PreviewShot("floor-02-study-landing", new Vector3(2.5f, 4.5f, 0.0f), new Vector3(0.0f, 4.2f, 0.5f), 65f),
                new PreviewShot("floor-03-watchroom", new Vector3(-1.48f, 7.7f, 2.55f), new Vector3(0.0f, 7.4f, -1.0f), 70f),
                new PreviewShot("lantern-room", new Vector3(-2.6f, 11.35f, -1.8f), new Vector3(0.0f, 11.1f, 0.0f), 70f),
                new PreviewShot("dining-table-meal", new Vector3(-8.80f, 1.30f, -7.30f), new Vector3(-9.50f, 0.90f, -6.85f), 55f),
                new PreviewShot("kitchen-hallway-doorway", new Vector3(-9.0f, 1.5f, -8.2f), new Vector3(-11.0f, 1.4f, -4.0f), 70f),
                new PreviewShot("cellar-arched-portal", new Vector3(-8.0f, 1.4f, -7.0f), new Vector3(-8.0f, 1.9f, -5.0f), 70f),
                new PreviewShot("cellar-stair-descent", new Vector3(-8.0f, 0.8f, -4.5f), new Vector3(-8.0f, -3.8f, 1.5f), 65f),
                new PreviewShot("cellar-vault-corridor", new Vector3(-8.0f, -3.2f, 2.0f), new Vector3(-8.0f, -3.2f, 9.0f), 70f),
                new PreviewShot("hidden-radio-room", new Vector3(-8.0f, -3.2f, 8.5f), new Vector3(-8.0f, -3.5f, 10.7f), 70f),
                new PreviewShot("shoreline", new Vector3(-12.0f, 1.6f, -22.0f), new Vector3(0f, 0.8f, -30f), 65f)
            };
        }

        public static void CaptureAll()
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Act1")
            {
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/_Project/Scenes/Act1.unity");
            }
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
                Object.DestroyImmediate(rt);
                Object.DestroyImmediate(tex);
                Object.DestroyImmediate(camGo);
            }
        }
    }
}
