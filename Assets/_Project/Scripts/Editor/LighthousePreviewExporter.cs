using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheLightHouse.Editor
{
    public static class LighthousePreviewExporter
    {
        const string OutputFolder="docs/level-previews";
        const string ScenePath="Assets/_Project/Scenes/Act1.unity";

        public static void Export()
        {
            Directory.CreateDirectory(OutputFolder);
            Scene scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
            Camera camera=Object.FindAnyObjectByType<Camera>();
            if(!camera)throw new IOException("Act1 has no camera for lighthouse previews.");
            ExportView(camera,"exterior-courtyard",new(16,12,-24),new(0,8,-2));
            ExportView(camera,"exterior-dock",new(-16,9,-33),new(0,5,-12));
            ExportView(camera,"front-door-interior",new(-5,1.7f,-10.5f),new(-5,1.7f,-5));
            for(int floor=0;floor<=8;floor++)
            {
                float y=floor<0?-2.5f:floor*3.2f+5f;
                ExportView(camera,$"floor-{floor:00}-topdown",new(0,y,0),new(0,y-10f,0));
            }
            EditorSceneManager.CloseScene(scene,true);
        }

        static void ExportView(Camera camera,string name,Vector3 position,Vector3 target)
        {
            camera.transform.position=position;
            camera.transform.LookAt(target);
            RenderTexture texture=new(960,540,24);
            Texture2D image=new(960,540,TextureFormat.RGB24,false);
            RenderTexture previous=RenderTexture.active;
            camera.targetTexture=texture;
            camera.Render();
            RenderTexture.active=texture;
            image.ReadPixels(new Rect(0,0,texture.width,texture.height),0,0);
            image.Apply();
            File.WriteAllBytes($"{OutputFolder}/{name}.png",image.EncodeToPNG());
            camera.targetTexture=null;
            RenderTexture.active=previous;
            Object.DestroyImmediate(image);
            Object.DestroyImmediate(texture);
        }
    }
}
