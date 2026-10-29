using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace TheLightHouse.Editor
{
    public static class BuildFullGameScenes
    {
        const string MenuPath="Assets/_Project/Scenes/MainMenu.unity";
        [MenuItem("TheLightHouse/Build Full Game Scenes")]
        public static void Build()
        {
            BuildAct1Scene.BuildTo("Assets/_Project/Scenes/Act1.unity");BuildAct2Scene.BuildTo("Assets/_Project/Scenes/Act2.unity");BuildAct3Scene.BuildTo("Assets/_Project/Scenes/Act3.unity");BuildAct4Scene.BuildTo("Assets/_Project/Scenes/Act4.unity");BuildAct5Scene.BuildTo("Assets/_Project/Scenes/Act5.unity");BuildMenu();
            string[] paths={MenuPath,"Assets/_Project/Scenes/Act1.unity","Assets/_Project/Scenes/Act2.unity","Assets/_Project/Scenes/Act3.unity","Assets/_Project/Scenes/Act4.unity","Assets/_Project/Scenes/Act5.unity"};var scenes=new EditorBuildSettingsScene[paths.Length];for(int i=0;i<paths.Length;i++)scenes[i]=new EditorBuildSettingsScene(paths[i],true);EditorBuildSettings.scenes=scenes;AssetDatabase.SaveAssets();Debug.Log("Full game scenes built and added to Build Settings. MainMenu is scene zero.");
        }
        static void BuildMenu()
        {
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);var flowObject=new GameObject("Act Flow");var flow=flowObject.AddComponent<ActFlowController>();Set(flow,"enableSceneChaining",true);flowObject.AddComponent<SaveSystem>();var debug=flowObject.AddComponent<GameDebugMenu>();Set(debug,"flow",flow);Set(debug,"inputActions",AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions"));
            var canvasObject=new GameObject("Main Menu Canvas",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));var canvas=canvasObject.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;var menu=canvasObject.AddComponent<MainMenu>();Set(menu,"flow",flow);Set(menu,"root",canvasObject);
            Label("The LightHouse",canvasObject.transform,new(.2f,.72f),new(.8f,.9f),56);
            Button("Start",canvasObject.transform,new(.35f,.58f),new(.65f,.66f),menu.StartGame);
            Button("Continue",canvasObject.transform,new(.35f,.48f),new(.65f,.56f),menu.ContinueGame);
            Toggle("Reduced storm visuals",canvasObject.transform,new(.3f,.38f),new(.7f,.44f),menu.SetReducedStorm,true);
            Toggle("Disable bob and FOV",canvasObject.transform,new(.3f,.31f),new(.7f,.37f),menu.SetDisableBob,false);
            Slider("Sensitivity",canvasObject.transform,new(.3f,.23f),new(.7f,.28f),menu.SetSensitivity,.1f,2f,1f);
            Slider("Volume placeholder",canvasObject.transform,new(.3f,.15f),new(.7f,.2f),menu.SetVolume,0f,1f,1f);
            Button("Quit",canvasObject.transform,new(.35f,.05f),new(.65f,.12f),menu.Quit);
            new GameObject("EventSystem",typeof(UnityEngine.EventSystems.EventSystem),typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));EditorSceneManager.SaveScene(scene,MenuPath);
        }
        static void Button(string label,Transform parent,Vector2 min,Vector2 max,UnityEngine.Events.UnityAction action){var go=new GameObject(label,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image),typeof(Button));go.transform.SetParent(parent,false);var rect=go.GetComponent<RectTransform>();rect.anchorMin=min;rect.anchorMax=max;rect.offsetMin=rect.offsetMax=Vector2.zero;var button=go.GetComponent<Button>();UnityEventTools.AddPersistentListener(button.onClick,action);Label(label,go.transform,Vector2.zero,Vector2.one,28);}
        static void Toggle(string label,Transform parent,Vector2 min,Vector2 max,UnityEngine.Events.UnityAction<bool> action,bool initial){var go=new GameObject(label,typeof(RectTransform),typeof(CanvasRenderer),typeof(Toggle));go.transform.SetParent(parent,false);var rect=go.GetComponent<RectTransform>();rect.anchorMin=min;rect.anchorMax=max;rect.offsetMin=rect.offsetMax=Vector2.zero;var toggle=go.GetComponent<Toggle>();toggle.isOn=initial;UnityEventTools.AddPersistentListener(toggle.onValueChanged,action);Label(label,go.transform,Vector2.zero,Vector2.one,22);}
        static void Slider(string label,Transform parent,Vector2 min,Vector2 max,UnityEngine.Events.UnityAction<float> action,float low,float high,float initial){var go=new GameObject(label,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image),typeof(Slider));go.transform.SetParent(parent,false);var rect=go.GetComponent<RectTransform>();rect.anchorMin=min;rect.anchorMax=max;rect.offsetMin=rect.offsetMax=Vector2.zero;var slider=go.GetComponent<Slider>();slider.minValue=low;slider.maxValue=high;slider.value=initial;UnityEventTools.AddPersistentListener(slider.onValueChanged,action);Label(label,go.transform,Vector2.zero,Vector2.one,20);}
        static void Label(string value,Transform parent,Vector2 min,Vector2 max,int size){var go=new GameObject(value+" Text",typeof(RectTransform),typeof(CanvasRenderer),typeof(Text));go.transform.SetParent(parent,false);var rect=go.GetComponent<RectTransform>();rect.anchorMin=min;rect.anchorMax=max;rect.offsetMin=rect.offsetMax=Vector2.zero;var text=go.GetComponent<Text>();text.text=value;text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");text.fontSize=size;text.alignment=TextAnchor.MiddleCenter;text.color=Color.white;}
        static void Set(Object target,string name,Object value){var so=new SerializedObject(target);so.FindProperty(name).objectReferenceValue=value;so.ApplyModifiedPropertiesWithoutUndo();}
        static void Set(Object target,string name,bool value){var so=new SerializedObject(target);so.FindProperty(name).boolValue=value;so.ApplyModifiedPropertiesWithoutUndo();}
    }
}
