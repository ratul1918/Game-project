using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TheLightHouse.Editor
{
    public static class BuildFullGameScenes
    {
        const string MenuPath="Assets/_Project/Scenes/MainMenu.unity";
        static readonly Color BackgroundColor=new(.012f,.018f,.025f,1f);
        static readonly Color PanelColor=new(.025f,.035f,.045f,.96f);
        static readonly Color ControlColor=new(.09f,.12f,.14f,1f);
        static readonly Color HoverColor=new(.15f,.20f,.22f,1f);
        static readonly Color AccentColor=new(.82f,.58f,.28f,1f);
        static readonly Color TextColor=new(.9f,.92f,.91f,1f);
        static readonly Vector2 ReferenceResolution=new(1920f,1080f);

        [MenuItem("TheLightHouse/Build Full Game Scenes")]
        public static void Build()
        {
            BuildAct1Scene.BuildTo("Assets/_Project/Scenes/Act1.unity");BuildAct2Scene.BuildTo("Assets/_Project/Scenes/Act2.unity");BuildAct3Scene.BuildTo("Assets/_Project/Scenes/Act3.unity");BuildAct4Scene.BuildTo("Assets/_Project/Scenes/Act4.unity");BuildAct5Scene.BuildTo("Assets/_Project/Scenes/Act5.unity");BuildMenu();
            string[] paths={MenuPath,"Assets/_Project/Scenes/Act1.unity","Assets/_Project/Scenes/Act2.unity","Assets/_Project/Scenes/Act3.unity","Assets/_Project/Scenes/Act4.unity","Assets/_Project/Scenes/Act5.unity"};var scenes=new EditorBuildSettingsScene[paths.Length];for(int i=0;i<paths.Length;i++)scenes[i]=new EditorBuildSettingsScene(paths[i],true);EditorBuildSettings.scenes=scenes;AssetDatabase.SaveAssets();Debug.Log("Full game scenes built and added to Build Settings. MainMenu is scene zero.");
        }

        [MenuItem("TheLightHouse/Build Main Menu")]
        public static void BuildMenu()
        {
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var cameraObject=new GameObject("Menu Camera",typeof(Camera),typeof(AudioListener));var camera=cameraObject.GetComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=BackgroundColor;camera.cullingMask=0;camera.depth=-10f;cameraObject.tag="MainCamera";
            var flowObject=new GameObject("Act Flow");var flow=flowObject.AddComponent<ActFlowController>();Set(flow,"enableSceneChaining",true);flowObject.AddComponent<SaveSystem>();var debug=flowObject.AddComponent<GameDebugMenu>();Set(debug,"flow",flow);Set(debug,"inputActions",AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions"));
            var canvasObject=new GameObject("Main Menu Canvas",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));var canvas=canvasObject.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=10;var scaler=canvasObject.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=ReferenceResolution;scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;scaler.matchWidthOrHeight=.5f;
            var menu=canvasObject.AddComponent<MainMenu>();Set(menu,"flow",flow);Set(menu,"root",canvasObject);
            Image background=Image("Background",canvasObject.transform,Vector2.zero,ReferenceResolution,BackgroundColor);Stretch(background.rectTransform);
            Image panel=Image("Menu Panel",background.transform,Vector2.zero,new Vector2(620f,960f),PanelColor);
            Text title=Label("The LightHouse",panel.transform,new Vector2(0f,365f),new Vector2(540f,90f),54,TextAnchor.MiddleCenter);title.fontStyle=FontStyle.Bold;title.color=TextColor;title.gameObject.AddComponent<Shadow>().effectColor=new Color(0f,0f,0f,.8f);
            Text subtitle=Label("HARROW ROCK",panel.transform,new Vector2(0f,315f),new Vector2(520f,38f),17,TextAnchor.MiddleCenter);subtitle.color=new Color(AccentColor.r,AccentColor.g,AccentColor.b,.9f);
            Text warning=Label("Keep the light. Never answer the singing.",panel.transform,new Vector2(0f,275f),new Vector2(520f,34f),16,TextAnchor.MiddleCenter);warning.color=new Color(TextColor.r,TextColor.g,TextColor.b,.62f);
            Button("START",panel.transform,new Vector2(0f,195f),menu.StartGame);
            Button("CONTINUE",panel.transform,new Vector2(0f,115f),menu.ContinueGame);
            Toggle("Reduced storm visuals",panel.transform,new Vector2(0f,35f),menu.SetReducedStorm,true);
            Toggle("Disable camera bob and FOV shift",panel.transform,new Vector2(0f,-20f),menu.SetDisableBob,false);
            Slider("LOOK SENSITIVITY",panel.transform,new Vector2(0f,-110f),menu.SetSensitivity,.1f,2f,1f);
            Slider("RENDER SCALE",panel.transform,new Vector2(0f,-205f),menu.SetRenderScale,.7f,1f,1f);
            Slider("MASTER VOLUME",panel.transform,new Vector2(0f,-300f),menu.SetVolume,0f,1f,1f);
            Button("QUIT",panel.transform,new Vector2(0f,-395f),menu.Quit,new Vector2(300f,54f));
            Label("F1 debug tools • F4 performance",panel.transform,new Vector2(0f,-448f),new Vector2(520f,24f),13,TextAnchor.MiddleCenter).color=new Color(TextColor.r,TextColor.g,TextColor.b,.4f);
            new GameObject("EventSystem",typeof(UnityEngine.EventSystems.EventSystem),typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));EditorSceneManager.SaveScene(scene,MenuPath);AssetDatabase.SaveAssets();Debug.Log("Main menu rebuilt with camera and responsive controls.");
        }

        static Button Button(string label,Transform parent,Vector2 position,UnityEngine.Events.UnityAction action,Vector2? size=null)
        {
            Vector2 dimensions=size??new Vector2(460f,62f);Image image=Image(label,parent,position,dimensions,ControlColor);var button=image.gameObject.AddComponent<Button>();button.targetGraphic=image;button.transition=Selectable.Transition.ColorTint;button.colors=new ColorBlock{normalColor=ControlColor,highlightedColor=HoverColor,pressedColor=AccentColor,selectedColor=HoverColor,disabledColor=new Color(.05f,.06f,.07f,.55f),colorMultiplier=1f,fadeDuration=.12f};UnityEventTools.AddPersistentListener(button.onClick,action);Text text=Label(label,image.transform,Vector2.zero,dimensions,20,TextAnchor.MiddleCenter);text.fontStyle=FontStyle.Bold;text.raycastTarget=false;return button;
        }

        static Toggle Toggle(string label,Transform parent,Vector2 position,UnityEngine.Events.UnityAction<bool> action,bool initial)
        {
            var root=RectObject(label,parent,position,new Vector2(460f,44f));Image box=Image("Box",root.transform,new Vector2(-205f,0f),new Vector2(30f,30f),ControlColor);Image check=Image("Checkmark",box.transform,Vector2.zero,new Vector2(18f,18f),AccentColor);Text text=Label(label,root.transform,new Vector2(18f,0f),new Vector2(400f,40f),17,TextAnchor.MiddleLeft);text.raycastTarget=false;var toggle=root.AddComponent<Toggle>();toggle.targetGraphic=box;toggle.graphic=check;toggle.isOn=initial;toggle.colors=new ColorBlock{normalColor=Color.white,highlightedColor=Color.white,pressedColor=AccentColor,selectedColor=Color.white,disabledColor=Color.gray,colorMultiplier=1f,fadeDuration=.1f};UnityEventTools.AddPersistentListener(toggle.onValueChanged,action);return toggle;
        }

        static Slider Slider(string label,Transform parent,Vector2 position,UnityEngine.Events.UnityAction<float> action,float low,float high,float initial)
        {
            var root=RectObject(label,parent,position,new Vector2(460f,72f));Text text=Label(label,root.transform,new Vector2(0f,22f),new Vector2(460f,24f),14,TextAnchor.MiddleLeft);text.color=new Color(TextColor.r,TextColor.g,TextColor.b,.72f);text.raycastTarget=false;
            Image track=Image("Track",root.transform,new Vector2(0f,-12f),new Vector2(460f,12f),ControlColor);var fillArea=RectObject("Fill Area",track.transform,Vector2.zero,Vector2.zero);Stretch(fillArea.GetComponent<RectTransform>());fillArea.GetComponent<RectTransform>().offsetMin=new Vector2(6f,2f);fillArea.GetComponent<RectTransform>().offsetMax=new Vector2(-6f,-2f);Image fill=Image("Fill",fillArea.transform,Vector2.zero,Vector2.zero,AccentColor);Stretch(fill.rectTransform);var handleArea=RectObject("Handle Slide Area",track.transform,Vector2.zero,Vector2.zero);Stretch(handleArea.GetComponent<RectTransform>());handleArea.GetComponent<RectTransform>().offsetMin=new Vector2(10f,-8f);handleArea.GetComponent<RectTransform>().offsetMax=new Vector2(-10f,8f);Image handle=Image("Handle",handleArea.transform,Vector2.zero,new Vector2(20f,28f),TextColor);
            var slider=root.AddComponent<Slider>();slider.minValue=low;slider.maxValue=high;slider.value=initial;slider.fillRect=fill.rectTransform;slider.handleRect=handle.rectTransform;slider.targetGraphic=handle;slider.direction=UnityEngine.UI.Slider.Direction.LeftToRight;UnityEventTools.AddPersistentListener(slider.onValueChanged,action);return slider;
        }

        static GameObject RectObject(string name,Transform parent,Vector2 position,Vector2 size)
        {
            var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);var rect=go.GetComponent<RectTransform>();rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);rect.pivot=new Vector2(.5f,.5f);rect.anchoredPosition=position;rect.sizeDelta=size;return go;
        }

        static Image Image(string name,Transform parent,Vector2 position,Vector2 size,Color color)
        {
            var go=RectObject(name,parent,position,size);go.AddComponent<CanvasRenderer>();var image=go.AddComponent<Image>();image.color=color;return image;
        }

        static Text Label(string value,Transform parent,Vector2 position,Vector2 size,int fontSize,TextAnchor alignment)
        {
            var go=RectObject(value+" Text",parent,position,size);go.AddComponent<CanvasRenderer>();var text=go.AddComponent<Text>();text.text=value;text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");text.fontSize=fontSize;text.alignment=alignment;text.color=TextColor;return text;
        }

        static void Stretch(RectTransform rect){rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;}
        static void Set(Object target,string name,Object value){var so=new SerializedObject(target);so.FindProperty(name).objectReferenceValue=value;so.ApplyModifiedPropertiesWithoutUndo();}
        static void Set(Object target,string name,bool value){var so=new SerializedObject(target);so.FindProperty(name).boolValue=value;so.ApplyModifiedPropertiesWithoutUndo();}
    }
}
