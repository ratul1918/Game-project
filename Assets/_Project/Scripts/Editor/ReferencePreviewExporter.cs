using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace TheLightHouse.Editor
{
    public static class ReferencePreviewExporter
    {
        public static void Export()
        {
            Directory.CreateDirectory("docs/reference-previews");
            var setup=EditorSceneManager.GetSceneManagerSetup();
            try
            {
                EditorSceneManager.OpenScene("Assets/_Project/Scenes/Act1.unity");
                foreach(var ps in Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None)) if(ps.name=="Coastal Rain") ps.Simulate(2,true,true);
                var go=new GameObject("Reference Capture",typeof(Camera));var camera=go.GetComponent<Camera>();
                camera.GetUniversalAdditionalCameraData().renderPostProcessing=true;camera.GetUniversalAdditionalCameraData().antialiasing=AntialiasingMode.FastApproximateAntialiasing;
                camera.clearFlags=CameraClearFlags.Skybox;camera.backgroundColor=new(.025f,.037f,.045f);camera.nearClipPlane=.05f;camera.farClipPlane=300;
                var start=GameObject.Find("Player").GetComponentInChildren<Camera>();
                Capture(camera,"arrival",start.transform.position,start.transform.position+start.transform.forward*10,start.fieldOfView);
                Capture(camera,"sea-door",new(10,1.8f,-5),new(3.9f,1.3f,0),65);
                Capture(camera,"exterior",new(-16,1.8f,-34),new(-4,13,-3),48);
                Capture(camera,"office",new(-9.7f,1.65f,-6.8f),new(-7.8f,1.4f,-1.6f),68);
                Capture(camera,"stairwell",new(-1.1f,1.6f,-1.85f),new(.3f,1.8f,1),78);
            }
            finally { if(setup.Any(s=>s.isLoaded) && setup.Any(s=>s.isActive)) EditorSceneManager.RestoreSceneManagerSetup(setup); }
        }
        static void Capture(Camera camera,string name,Vector3 position,Vector3 target,float fov)
        {
            camera.transform.position=position;camera.transform.LookAt(target);camera.fieldOfView=fov;
            var rt=new RenderTexture(1280,720,24);var image=new Texture2D(1280,720,TextureFormat.RGB24,false);
            var previous=RenderTexture.active;camera.targetTexture=rt;
            try {camera.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();File.WriteAllBytes($"docs/reference-previews/{name}.png",image.EncodeToPNG());}
            finally{camera.targetTexture=null;RenderTexture.active=previous;Object.DestroyImmediate(image);Object.DestroyImmediate(rt);}
        }
    }
}
