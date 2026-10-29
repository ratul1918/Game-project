using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TheLightHouse.Editor
{
    public static class BuildAct4Scene
    {
        const string ScenePath="Assets/_Project/Scenes/Act4.unity";
        const string SettingsPath="Assets/_Project/Settings/ActFourSettings.asset";
        const string PageFolder="Assets/_Project/LogbookPages";

        [MenuItem("TheLightHouse/Build Act 4 Scene")]
        public static void Build()
        {
            BuildTo(ScenePath);
        }
        public static void BuildTo(string scenePath)
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)return;
            Directory.CreateDirectory("Assets/_Project/Settings");Directory.CreateDirectory(PageFolder);AssetDatabase.Refresh();
            var settings=AssetDatabase.LoadAssetAtPath<ActFourSettings>(SettingsPath);
            if(!settings){settings=ScriptableObject.CreateInstance<ActFourSettings>();AssetDatabase.CreateAsset(settings,SettingsPath);}
            BuildAct3Scene.BuildTo(scenePath);
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();if(scene.path!=scenePath)return;
            settings=AssetDatabase.LoadAssetAtPath<ActFourSettings>(SettingsPath);
            var actions=AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions");
            var player=GameObject.Find("Player");var controller=player.GetComponent<FirstPersonController>();var health=player.GetComponent<PlayerHealthState>();var interactor=player.GetComponent<Interactor>();var inventory=player.GetComponent<Inventory>();var view=player.GetComponentInChildren<Camera>();
            player.transform.position=new Vector3(0,3.1f,5);

            foreach(var component in Object.FindObjectsByType<StormBeats>())component.enabled=false;
            foreach(var component in Object.FindObjectsByType<StormEndCondition>())component.enabled=false;
            foreach(var component in Object.FindObjectsByType<StormController>())component.enabled=false;
            foreach(var component in Object.FindObjectsByType<Generator>())component.enabled=false;

            var lamp=Object.FindAnyObjectByType<LampController>();Set(lamp,"actFourSettings",settings);Set(lamp,"startInChargeMode",true);
            var crane=GameObject.Find("Stalker");crane.SetActive(true);var ai=crane.GetComponent<StalkerAI>();Set(ai,"postRevealMode",false);Set(ai,"actFourSettings",settings);
            var herding=crane.GetComponent<CraneHerding>();Set(herding,"actFourSettings",settings);

            var canvas=GameObject.Find("Debug Canvas").transform;
            var readerPanel=Panel("Logbook Reader",canvas,new Vector2(.15f,.12f),new Vector2(.85f,.88f));
            var readerTitle=Text("Reader Title",readerPanel.transform,new Vector2(.08f,.82f),new Vector2(.92f,.96f),32,TextAnchor.MiddleCenter);
            var readerBody=Text("Reader Body",readerPanel.transform,new Vector2(.08f,.1f),new Vector2(.92f,.82f),24,TextAnchor.UpperLeft);
            var reader=canvas.gameObject.AddComponent<LogbookReaderUI>();Set(reader,"panel",readerPanel);Set(reader,"titleLabel",readerTitle);Set(reader,"bodyLabel",readerBody);Set(reader,"inputActions",actions);Set(reader,"player",controller);Set(reader,"health",health);Set(reader,"interactor",interactor);Set(reader,"settings",settings);readerPanel.SetActive(false);
            var journal=player.AddComponent<LogbookJournal>();Set(journal,"reader",reader);
            var journalPanel=Panel("Journal",canvas,new Vector2(.05f,.15f),new Vector2(.38f,.85f));
            var journalText=Text("Journal List",journalPanel.transform,new Vector2(.08f,.06f),new Vector2(.92f,.94f),22,TextAnchor.UpperLeft);
            var journalUi=canvas.gameObject.AddComponent<JournalUI>();Set(journalUi,"journal",journal);Set(journalUi,"panel",journalPanel);Set(journalUi,"listLabel",journalText);Set(journalUi,"inputActions",actions);Set(journalUi,"player",controller);Set(journalUi,"health",health);Set(journalUi,"interactor",interactor);Set(journalUi,"settings",settings);journalPanel.SetActive(false);
            var grace=new GameObject("Reading Grace").AddComponent<ReadingGrace>();Set(grace,"settings",settings);Set(grace,"lamp",lamp);
            var vignette=Panel("Flashback Vignette",canvas,Vector2.zero,Vector2.one).GetComponent<Image>();vignette.color=new Color(.08f,0,0,0);vignette.raycastTarget=false;
            var flashback=vignette.gameObject.AddComponent<GuiltFlashback>();Set(flashback,"vignette",vignette);

            var pages=new[]{
                Page("marguerite-wreck","The Marguerite","The ship Marguerite did not strike the reef by accident. Harrow Rock's wreckers moved the warning lantern and deliberately drew her onto stone. Her passengers drowned within sight of shore.","marguerite",false),
                Page("wrecker-tally","Wreckers' Tally","A tally lists false lights, salvaged cargo, and payments shared among island families after the Marguerite was wrecked.","",false),
                Page("lighthouse-leash","The Light Is a Leash","The lighthouse is not a warning for ships. Its beam is a leash: while it burns, the Drowned are held in the water.","lighthouse-leash",false),
                Page("keeper-selection","Keeper Selection","The harbor board does not choose keepers for skill alone. It sends people burdened by guilt—people the island can call and hold.","",false),
                Page("crane-observation","Crane's Observation","The thing wearing the keeper's coat blocks the water path and leaves the way toward the boat shed open. It is herding, not hunting alone.","",false),
                Page("crane-final-entry","Crane's Last Entry","I went into the sea to offer myself in place of my son. The sea took me, but it would not let me rest.","crane-bargain",true)};
            SetArray(journal,"allPages",pages);
            PlacePage("Marguerite page — keeper's room",new(-5,3.35f,6.8f),pages[0],journal);
            PlacePage("Wrecker tally — stair landing",new(6.2f,3.25f,3.3f),pages[1],journal);
            PlacePage("Leash page — storeroom",new(-7,.45f,5.5f),pages[2],journal);
            PlacePage("Keeper selection — boat shed",new(8,.35f,-16),pages[3],journal);
            PlacePage("Crane observation — cellar",new(-12,.35f,1.2f),pages[4],journal);

            var cellarDoor=GameObject.Find("Locked Cellar Door").GetComponent<Door>();Set(cellarDoor,"locked",true);
            var gate=new GameObject("Act Four Cellar Gate").AddComponent<ActFourGate>();Set(gate,"cellarDoor",cellarDoor);Set(gate,"settings",settings);
            Cube("Cellar floor",new(-13,-.25f,3),new(7,.5f,8));Cube("Cellar north wall",new(-13,1.5f,7),new(7,3,.3f));Cube("Cellar south wall",new(-13,1.5f,-1),new(7,3,.3f));Cube("Cellar west wall",new(-16.5f,1.5f,3),new(.3f,3,8));
            var shelf=Cube("False cellar shelf",new(-14,1,3),new(.4f,2,3));shelf.AddComponent<NoiseEmitter>();var hidden=shelf.AddComponent<HiddenCellarRoom>();Set(hidden,"shelf",shelf.transform);Set(hidden,"settings",settings);
            Cube("Hidden room floor",new(-19,-.25f,3),new(5,.5f,6));Cube("Hidden room west wall",new(-21.5f,1.5f,3),new(.3f,3,6));Cube("Hidden room north wall",new(-19,1.5f,6),new(5,3,.3f));Cube("Hidden room south wall",new(-19,1.5f,0),new(5,3,.3f));
            var transmitter=Cube("Hidden signal transmitter",new(-20,.7f,4.5f),new(.8f,1.4f,.8f)).transform;
            var recorder=Cube("Marta reel-to-reel recorder",new(-19,.7f,3),new(1.2f,1,.8f));recorder.AddComponent<AudioSource>();recorder.AddComponent<RecordedTransmitter>();
            var ledgerObject=Cube("Harbor board ledger",new(-18.5f,.9f,1.5f),new(1.4f,.12f,1));var ledger=ledgerObject.AddComponent<Ledger>();Set(ledger,"reader",reader);Set(ledger,"entries","03 October 1911 — three weeks before Daniel's 24 October 1911 arrival — Daniel Hale. Selected because his guilt over abandoning six sailors makes him suitable.");
            var oldRadio=GameObject.Find("Radio");var old=oldRadio.GetComponent<Radio>();if(old)old.enabled=false;oldRadio.AddComponent<AudioSource>();var tracker=oldRadio.AddComponent<RadioSignalTracker>();Set(tracker,"player",player.transform);Set(tracker,"transmitter",transmitter);Set(tracker,"inputActions",actions);Set(tracker,"settings",settings);
            var needle=Slider("Signal Needle",canvas,new Vector2(.42f,.88f),new Vector2(.58f,.92f));Set(tracker,"needle",needle);needle.gameObject.SetActive(false);

            Cube("Crane room partition",new(-4,4.25f,5.5f),new(.25f,2.8f,7));
            var roomDoor=new GameObject("Crane's Room Door");roomDoor.transform.position=new(-4,2.95f,4);var roomPanel=Cube("Crane door panel",new(-4,4.1f,4),new(.25f,2.3f,2));roomPanel.transform.SetParent(roomDoor.transform,true);roomDoor.AddComponent<NoiseEmitter>();var locked=roomDoor.AddComponent<LockedRoomDoor>();Set(locked,"inventory",inventory);
            var key=Cube("Crane Room Key",new(8,.35f,-15.5f),new(.15f,.05f,.35f));key.AddComponent<KeyItem>();
            var photo=Cube("Family Photo",new(-7.7f,4.2f,6),new(.08f,1.1f,1.4f));var photoScript=photo.AddComponent<FamilyPhoto>();var pose=new GameObject("Photo inspection pose").transform;pose.position=new(-6.4f,4.2f,6);pose.LookAt(photo.transform);Set(photoScript,"view",view);Set(photoScript,"player",controller);Set(photoScript,"inspectionPose",pose);
            var desk=Cube("Crane's Desk",new(-6,3.35f,4.8f),new(2,.35f,1.2f));PlacePage("Crane's final entry",desk.transform.position+Vector3.up*.25f,pages[5],journal);

            new GameObject("Act Four Beats").AddComponent<ActFourBeats>();new GameObject("Act Four End Condition").AddComponent<ActFourEndCondition>();
            var director=Object.FindAnyObjectByType<StoryDirector>();
            SetArray(director,"beats",new[]{
                Beat("A4-B01","marguerite","Find out what happened"),
                Beat("A4-B02","lighthouse-leash","Follow the signal"),
                Beat("A4-B03","marta-recording","Read the harbor ledger"),
                Beat("A4-B04","daniel-chosen","Find the key to Crane's room"),
                Beat("A4-B05","father-son","Read Crane's final entry"),
                Beat("A4-B06","crane-bargain","Hold the light until dawn")});
            SetArray(director,"optionalBeats",System.Array.Empty<StoryBeat>());Set(director,"allowPendingActTwoSignals",true);
            GameObject.Find("Front Door Knocking")?.SetActive(false);GameObject.Find("Singing From Shore")?.SetActive(false);
            EditorSceneManager.SaveScene(scene,scenePath);AssetDatabase.SaveAssets();
            Debug.Log("Act 4 built after the storm. Bake Navigation/NavMeshSurface, save, then Play. Tune with Q/T or the mouse wheel; J opens the journal.");
        }

        static LogbookPage Page(string id,string title,string body,string flag,bool locked)
        {
            string path=$"{PageFolder}/{id}.asset";var page=AssetDatabase.LoadAssetAtPath<LogbookPage>(path);if(!page){page=ScriptableObject.CreateInstance<LogbookPage>();AssetDatabase.CreateAsset(page,path);}
            page.id=id;page.title=title;page.body=body;page.storyFlag=flag;page.lockedUntilPhoto=locked;EditorUtility.SetDirty(page);return page;
        }
        static StoryBeat Beat(string id,string signal,string objective)
        {
            string path=$"Assets/_Project/StoryBeats/{id}.asset";var beat=AssetDatabase.LoadAssetAtPath<StoryBeat>(path);if(!beat){beat=ScriptableObject.CreateInstance<StoryBeat>();AssetDatabase.CreateAsset(beat,path);}
            beat.id=id;beat.trigger=StoryTrigger.Signal;beat.triggerId=signal;beat.objective=objective;beat.prerequisiteId="";beat.lines=System.Array.Empty<DialogueLine>();EditorUtility.SetDirty(beat);return beat;
        }
        static void PlacePage(string name,Vector3 position,LogbookPage page,LogbookJournal journal){var go=Cube(name,position,new(.45f,.08f,.65f));var pickup=go.AddComponent<LogbookPickup>();Set(pickup,"page",page);Set(pickup,"journal",journal);}
        static GameObject Cube(string name,Vector3 position,Vector3 scale){var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.position=position;go.transform.localScale=scale;return go;}
        static GameObject Panel(string name,Transform parent,Vector2 min,Vector2 max){var go=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));go.transform.SetParent(parent,false);var rect=go.GetComponent<RectTransform>();rect.anchorMin=min;rect.anchorMax=max;rect.offsetMin=rect.offsetMax=Vector2.zero;go.GetComponent<Image>().color=new Color(.03f,.04f,.045f,.94f);return go;}
        static Text Text(string name,Transform parent,Vector2 min,Vector2 max,int size,TextAnchor anchor){var go=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(Text));go.transform.SetParent(parent,false);var rect=go.GetComponent<RectTransform>();rect.anchorMin=min;rect.anchorMax=max;rect.offsetMin=rect.offsetMax=Vector2.zero;var text=go.GetComponent<Text>();text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");text.fontSize=size;text.alignment=anchor;text.color=Color.white;text.raycastTarget=false;return text;}
        static Slider Slider(string name,Transform parent,Vector2 min,Vector2 max){var root=Panel(name,parent,min,max);var slider=root.AddComponent<Slider>();slider.minValue=0;slider.maxValue=1;var fill=Panel("Fill",root.transform,Vector2.zero,Vector2.one).GetComponent<RectTransform>();fill.GetComponent<Image>().color=new Color(.8f,.75f,.35f,1);slider.fillRect=fill;return slider;}
        static void Set(Object target,string name,Object value){var so=new SerializedObject(target);var property=so.FindProperty(name);if(property!=null){property.objectReferenceValue=value;so.ApplyModifiedPropertiesWithoutUndo();}}
        static void Set(Object target,string name,bool value){var so=new SerializedObject(target);var property=so.FindProperty(name);if(property!=null){property.boolValue=value;so.ApplyModifiedPropertiesWithoutUndo();}}
        static void Set(Object target,string name,string value){var so=new SerializedObject(target);var property=so.FindProperty(name);if(property!=null){property.stringValue=value;so.ApplyModifiedPropertiesWithoutUndo();}}
        static void SetArray<T>(Object target,string name,T[] values) where T:Object{var so=new SerializedObject(target);var array=so.FindProperty(name);array.arraySize=values.Length;for(int i=0;i<values.Length;i++)array.GetArrayElementAtIndex(i).objectReferenceValue=values[i];so.ApplyModifiedPropertiesWithoutUndo();}
    }
}
