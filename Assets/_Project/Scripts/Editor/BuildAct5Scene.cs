using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TheLightHouse.Editor
{
    public static class BuildAct5Scene
    {
        const string ScenePath="Assets/_Project/Scenes/Act5.unity";
        const string SettingsPath="Assets/_Project/Settings/FinaleSettings.asset";
        const string LullabyPath="Assets/_Project/Story/Lullaby.asset";
        [MenuItem("TheLightHouse/Build Act 5 Scene")]
        public static void Build()=>BuildTo(ScenePath);
        public static void BuildTo(string scenePath)
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)return;
            Directory.CreateDirectory("Assets/_Project/Settings");Directory.CreateDirectory("Assets/_Project/Story");AssetDatabase.Refresh();
            var settings=AssetDatabase.LoadAssetAtPath<FinaleSettings>(SettingsPath);if(!settings){settings=ScriptableObject.CreateInstance<FinaleSettings>();AssetDatabase.CreateAsset(settings,SettingsPath);}
            var lullaby=AssetDatabase.LoadAssetAtPath<LullabyAsset>(LullabyPath);if(!lullaby){lullaby=ScriptableObject.CreateInstance<LullabyAsset>();lullaby.placeholderLines=new[]{"Hush now, the storm is passing.","Follow the lamp, not the tide."};AssetDatabase.CreateAsset(lullaby,LullabyPath);}
            BuildAct4Scene.BuildTo(scenePath);var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();if(scene.path!=scenePath)return;
            settings=AssetDatabase.LoadAssetAtPath<FinaleSettings>(SettingsPath);lullaby=AssetDatabase.LoadAssetAtPath<LullabyAsset>(LullabyPath);
            var actions=AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions");
            var state=Object.FindAnyObjectByType<GameStateMachine>();Set(state,"startAtNight",false);Set(state,"startAtStorm",false);Set(state,"startAtFinale",true);
            var player=GameObject.Find("Player");player.transform.position=new(0,3.1f,5);var health=player.GetComponent<PlayerHealthState>();var stamina=player.GetComponent<PlayerStamina>();
            var lamp=Object.FindAnyObjectByType<LampController>();Set(lamp,"finaleSettings",settings);Set(lamp,"startInChargeMode",true);
            var drowned=Object.FindAnyObjectByType<DrownedManager>();Set(drowned,"finaleSettings",settings);

            var figures=Object.FindObjectsByType<DrownedFigure>().ToList();var template=figures[0];
            for(int i=figures.Count;i<settings.figureCount;i++)
            {
                var root=Object.Instantiate(template.gameObject);root.name="Finale Drowned "+i;var figure=root.GetComponent<DrownedFigure>();figures.Add(figure);
                Vector3 initial=new(-14+(i%7)*4,0,-22-(i/7)*3);root.transform.position=initial;
                var so=new SerializedObject(figure);so.FindProperty("figureId").stringValue="finale-"+i;var path=so.FindProperty("path");path.arraySize=6;
                for(int p=0;p<6;p++)path.GetArrayElementAtIndex(p).vector3Value=Vector3.Lerp(initial,new Vector3(-5+(i%6)*2,0,-10),p/5f);so.ApplyModifiedPropertiesWithoutUndo();
            }
            SetArray(drowned,"figures",figures.ToArray());
            Cube("Flooded courtyard",new(0,-.35f,-14),new(24,.3f,12));
            Cube("Lower window pressure ledge",new(9,.2f,-3),new(3,.4f,8));
            var courtyardLink=new GameObject("Tower to courtyard NavMesh link").AddComponent<Unity.AI.Navigation.NavMeshLink>();courtyardLink.startPoint=new(0,0,-8.5f);courtyardLink.endPoint=new(0,0,-11.5f);courtyardLink.width=2f;courtyardLink.bidirectional=true;

            var frontDoor=GameObject.Find("Tower Door").GetComponent<Door>();Set(frontDoor,"blockDuringActTwo",false);Set(frontDoor,"slamLoudness",settings.slamLoudness);
            var courtyardDoor=DoorObject("Courtyard slam door",new(-6,0,-10),settings.slamLoudness);
            var shutter=DoorObject("Lower window shutter",new(9,0,-3),settings.slamLoudness);

            var crane=GameObject.Find("Stalker");crane.SetActive(true);var oldAi=crane.GetComponent<StalkerAI>();oldAi.enabled=false;
            var finaleAi=crane.AddComponent<CraneFinale>();Set(finaleAi,"settings",settings);Set(finaleAi,"player",player.transform);SetArray(finaleAi,"slamDoors",new[]{frontDoor,courtyardDoor,shutter});
            var lurePoints=new[]{Point("Crane lure west",new Vector3(-7,0,-13)),Point("Crane lure east",new Vector3(7,0,-13)),Point("Crane lure shed",new Vector3(8,0,-16))};SetArray(finaleAi,"lurePoints",lurePoints);

            var finale=new GameObject("Finale Director").AddComponent<FinaleDirector>();Set(finale,"settings",settings);Set(finale,"lamp",lamp);Set(finale,"drowned",drowned);Set(finale,"stamina",stamina);Set(finale,"player",health);
            var dawn=new GameObject("Dawn Controller").AddComponent<DawnController>();Set(dawn,"settings",settings);Set(dawn,"skyLight",GameObject.Find("Dusk Sun")?.GetComponent<Light>());Set(dawn,"rain",GameObject.Find("Storm rain placeholder")?.GetComponent<ParticleSystem>());Set(dawn,"wind",GameObject.Find("Storm Wind")?.GetComponent<WindZone>());Set(dawn,"player",player.transform);

            var catchSequence=Object.FindAnyObjectByType<CatchSequence>();var blockPose=Point("Crane doorway block",new(0,0,-9.4f));var aside=Point("Crane steps aside",new(-2.2f,0,-9));
            var doorway=new GameObject("Doorway Block").AddComponent<DoorwayBlock>();Set(doorway,"settings",settings);Set(doorway,"crane",crane.transform);Set(doorway,"blockPose",blockPose);Set(doorway,"stepAsidePose",aside);Set(doorway,"catchSequence",catchSequence);

            var shore=Cube("Shoreline ending trigger",new(0,1,-23),new(8,2,3));Object.DestroyImmediate(shore.GetComponent<MeshRenderer>());shore.GetComponent<BoxCollider>().isTrigger=true;
            var shoreline=shore.AddComponent<ShorelineEnding>();Set(shoreline,"settings",settings);Set(shoreline,"lullaby",lullaby);Set(shoreline,"player",player.GetComponent<FirstPersonController>());Set(shoreline,"crane",crane.transform);Set(shoreline,"waterPose",Point("Crane water rest",new(0,-2,-31)));Set(shoreline,"beam",GameObject.Find("Rotating lamp beam placeholder")?.transform);
            var boat=GameObject.Find("Boat");boat.transform.position=new(-5,0,-24);var boatmanObject=Cube("Morning Boatman",boat.transform.position+new Vector3(0,1,0),new(.6f,1.8f,.6f));var boatman=boatmanObject.AddComponent<Boatman>();Set(boatman,"player",player.GetComponent<FirstPersonController>());Set(boatman,"subtitles",Object.FindAnyObjectByType<SubtitleUI>());Set(boatman,"boat",boat.transform);Set(boatman,"finaleMode",true);Set(boatman,"departureSeconds",settings.boatTravelDuration);
            var boatEnding=boat.AddComponent<BoatEnding>();Set(boatEnding,"reader",Object.FindAnyObjectByType<LogbookReaderUI>());Set(boatEnding,"boatman",boatman);

            var canvas=GameObject.Find("Debug Canvas").transform;var endingRoot=Panel("Ending Fade",canvas,Vector2.zero,Vector2.one);var group=endingRoot.AddComponent<CanvasGroup>();group.alpha=0;var title=Text("Ending Title",endingRoot.transform,new(.1f,.55f),new(.9f,.75f),52);var credits=Text("Ending Credits",endingRoot.transform,new(.1f,.25f),new(.9f,.5f),26);var ending=endingRoot.AddComponent<EndingUI>();Set(ending,"group",group);Set(ending,"title",title);Set(ending,"credits",credits);

            var flow=new GameObject("Act Flow (direct test)").AddComponent<ActFlowController>();var debug=new GameObject("Game Debug Menu").AddComponent<GameDebugMenu>();Set(debug,"inputActions",actions);Set(debug,"flow",flow);Set(debug,"lamp",lamp);Set(debug,"drowned",drowned);Set(debug,"stalker",oldAi);Set(debug,"journal",Object.FindAnyObjectByType<LogbookJournal>());Set(debug,"player",player.transform);SetArray(debug,"teleportPoints",new[]{Point("Debug lantern",new(0,3.1f,5)),Point("Debug courtyard",new(0,.1f,-13)),Point("Debug dock",new(0,.1f,-23))});

            var director=Object.FindAnyObjectByType<StoryDirector>();SetArray(director,"beats",new[]{Beat("A5-B01","courtyard-floods","Hold the light"),Beat("A5-B02","hold-light","Crane is drawing them away"),Beat("A5-B03","crane-remembers","Hold until first light"),Beat("A5-B04","first-light","Get to the dock"),Beat("A5-B05","last-line","Board the morning boat"),Beat("A5-B06","next-keeper","The LightHouse")});SetArray(director,"optionalBeats",System.Array.Empty<StoryBeat>());Set(director,"allowPendingActTwoSignals",true);
            EditorSceneManager.SaveScene(scene,scenePath);AssetDatabase.SaveAssets();Debug.Log("Act 5 built in Finale with 14 Drowned. Bake NavMeshSurface, save, then Play.");
        }
        static StoryBeat Beat(string id,string signal,string objective){string path=$"Assets/_Project/StoryBeats/{id}.asset";var beat=AssetDatabase.LoadAssetAtPath<StoryBeat>(path);if(!beat){beat=ScriptableObject.CreateInstance<StoryBeat>();AssetDatabase.CreateAsset(beat,path);}beat.id=id;beat.trigger=StoryTrigger.Signal;beat.triggerId=signal;beat.objective=objective;beat.prerequisiteId="";beat.lines=System.Array.Empty<DialogueLine>();EditorUtility.SetDirty(beat);return beat;}
        static Door DoorObject(string name,Vector3 position,float loudness){var root=new GameObject(name);root.transform.position=position;var panel=Cube(name+" panel",position+new Vector3(1,1.5f,0),new(2,3,.25f));panel.transform.SetParent(root.transform,true);root.AddComponent<NoiseEmitter>();var door=root.AddComponent<Door>();Set(door,"doorId",name);Set(door,"slamLoudness",loudness);return door;}
        static Transform Point(string name,Vector3 position){var point=new GameObject(name).transform;point.position=position;return point;}
        static GameObject Cube(string name,Vector3 position,Vector3 scale){var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.position=position;go.transform.localScale=scale;return go;}
        static GameObject Panel(string name,Transform parent,Vector2 min,Vector2 max){var go=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));go.transform.SetParent(parent,false);var rect=go.GetComponent<RectTransform>();rect.anchorMin=min;rect.anchorMax=max;rect.offsetMin=rect.offsetMax=Vector2.zero;go.GetComponent<Image>().color=Color.black;return go;}
        static Text Text(string name,Transform parent,Vector2 min,Vector2 max,int size){var go=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(Text));go.transform.SetParent(parent,false);var rect=go.GetComponent<RectTransform>();rect.anchorMin=min;rect.anchorMax=max;rect.offsetMin=rect.offsetMax=Vector2.zero;var text=go.GetComponent<Text>();text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");text.fontSize=size;text.alignment=TextAnchor.MiddleCenter;text.color=Color.white;return text;}
        static void Set(Object target,string name,Object value){if(!target)return;var so=new SerializedObject(target);var p=so.FindProperty(name);if(p!=null){p.objectReferenceValue=value;so.ApplyModifiedPropertiesWithoutUndo();}}
        static void Set(Object target,string name,bool value){if(!target)return;var so=new SerializedObject(target);var p=so.FindProperty(name);if(p!=null){p.boolValue=value;so.ApplyModifiedPropertiesWithoutUndo();}}
        static void Set(Object target,string name,float value){if(!target)return;var so=new SerializedObject(target);var p=so.FindProperty(name);if(p!=null){p.floatValue=value;so.ApplyModifiedPropertiesWithoutUndo();}}
        static void Set(Object target,string name,string value){if(!target)return;var so=new SerializedObject(target);var p=so.FindProperty(name);if(p!=null){p.stringValue=value;so.ApplyModifiedPropertiesWithoutUndo();}}
        static void SetArray<T>(Object target,string name,T[] values) where T:Object{var so=new SerializedObject(target);var p=so.FindProperty(name);p.arraySize=values.Length;for(int i=0;i<values.Length;i++)p.GetArrayElementAtIndex(i).objectReferenceValue=values[i];so.ApplyModifiedPropertiesWithoutUndo();}
    }
}
