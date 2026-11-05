using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheLightHouse.Editor
{
    public static class UiPreviewExporter
    {
        const string Output="docs/ui-previews";
        public static void Export()
        {
            Directory.CreateDirectory(Output);
            Scene act=EditorSceneManager.OpenScene("Assets/_Project/Scenes/Act1.unity",OpenSceneMode.Single);
            Camera camera=Object.FindAnyObjectByType<Camera>();
            if(camera)
            {
                ConfigureCanvases(camera);
                SetActive("Pause Menu",false);
                SetActive("Journal",false);
                SetActive("Options",false);
                SetActive("Subtitle Background",false);
                Capture(camera,"hud-start");
                Capture(camera,"hud-keeper-room");
                SetActive("Subtitle Background",true);
                SetFirstText("Subtitle","The sea keeps its promises.");
                Capture(camera,"hud-mid-dialogue");
                SetActive("Pause Menu",true);Capture(camera,"pause-menu");
                SetActive("Journal",true);Capture(camera,"journal");
                SetActive("Journal",false);
                SetActive("Options",true);Capture(camera,"options-menu");
                SetActive("Pause Menu",false);
            }
            EditorSceneManager.CloseScene(act,true);
            Scene menu=EditorSceneManager.OpenScene("Assets/_Project/Scenes/MainMenu.unity",OpenSceneMode.Single);
            Camera menuCamera=Object.FindAnyObjectByType<Camera>();if(menuCamera){ConfigureCanvases(menuCamera);Capture(menuCamera,"main-menu");}
            EditorSceneManager.CloseScene(menu,true);
        }
        static void SetActive(string name,bool value){GameObject go=GameObject.Find(name);if(go)go.SetActive(value);}
        static void SetFirstText(string parentName,string value)
        {
            GameObject parent=GameObject.Find(parentName);
            if(!parent)return;
            TMPro.TMP_Text text=parent.GetComponentInChildren<TMPro.TMP_Text>(true);
            if(text)text.text=value;
        }
        static void ConfigureCanvases(Camera camera)
        {
            foreach(Canvas canvas in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            {
                canvas.renderMode=RenderMode.ScreenSpaceCamera;
                canvas.worldCamera=camera;
                canvas.planeDistance=1f;
            }
        }
        static void Capture(Camera camera,string name)
        {
            RenderTexture texture=new(1280,720,24);Texture2D image=new(1280,720,TextureFormat.RGB24,false);RenderTexture previous=RenderTexture.active;camera.targetTexture=texture;camera.Render();RenderTexture.active=texture;image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();File.WriteAllBytes($"{Output}/{name}.png",image.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=previous;Object.DestroyImmediate(image);Object.DestroyImmediate(texture);
        }
    }
}
