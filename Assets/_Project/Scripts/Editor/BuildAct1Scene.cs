using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
namespace TheLightHouse.Editor {
public static class BuildAct1Scene {
const string AssetFolder="Assets/_Project/StoryBeats";
const string ScenePath="Assets/_Project/Scenes/Act1.unity";
const float StairRise=.2f;
const float StairRun=.5f;
const int StairCount=15;
[MenuItem("TheLightHouse/Build Act 1 Scene")]
public static void Build(){
BuildTo(ScenePath);
}
public static void BuildTo(string scenePath){
BuildThinSliceScene.BuildTo(scenePath);
if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().path!=scenePath)return;
var player=GameObject.Find("Player");player.transform.position=new(0,.1f,-23);
var inventory=player.AddComponent<Inventory>();
GameObject.Find("Stalker").SetActive(false);
Object.DestroyImmediate(GameObject.Find("South Wall"));
Cube("South wall left",new(-6,3,-10),new(8,6,.5f));
Cube("South wall right",new(6,3,-10),new(8,6,.5f));
Cube("Door lintel",new(0,4.5f,-10),new(4,3,.5f));
var doorRoot=new GameObject("Tower Door");doorRoot.transform.position=new(-2,0,-10);
var panel=Cube("Door panel",new(0,1.5f,-10),new(4,3,.25f));panel.transform.SetParent(doorRoot.transform,true);
doorRoot.AddComponent<NoiseEmitter>();doorRoot.AddComponent<Door>();
Cube("Dock",new(0,-.25f,-23),new(8,.5f,8));
Cube("Path",new(0,-.25f,-15),new(4,.5f,10));
var boat=Cube("Boat",new(-5,0,-23),new(3,.8f,6));
Cube("Boat Shed Floor",new(8,-.25f,-16),new(5,.5f,5));
Cube("Boat Shed Back",new(10.5f,1.5f,-16),new(.3f,3,5));
Cube("Boat Shed Roof",new(8,3,-16),new(5,.3f,5));
var can=Cube("Fuel Can",new(8,.5f,-16),Vector3.one);can.AddComponent<FuelCan>();
for(int i=0;i<StairCount;i++)Cube("Stair "+i,new(7,(i+1)*StairRise/2,-5+i*StairRun),new(2,(i+1)*StairRise,StairRun));
// Keep the stairwell open until the final riser; a solid floor lip here blocks the capsule's head.
Cube("Lantern Room Floor",new(-1.75f,2.85f,5.5f),new(14.5f,.3f,7));
Cube("Lantern Room Floor Back",new(7.25f,2.85f,6),new(3.5f,.3f,6));
Cube("Stair Top Landing",new(7,2.85f,2.625f),new(2,.3f,.75f));
var lamp=Object.FindAnyObjectByType<LampController>();
lamp.transform.position=new(0,4.5f,6);
Set(lamp,"startWarmButUnlit",true);Set(lamp,"requireFuelCan",true);Set(lamp,"startingFuel",5f);
var lantern=lamp.gameObject.AddComponent<LanternInteract>();Set(lantern,"lamp",lamp);Set(lantern,"inventory",inventory);
GameObject.Find("Crank Station").transform.position=new(3,3.75f,6);
var meal=Cube("Hot Meal",new(-4,.9f,-4),new(2,.2f,1));meal.AddComponent<MealTable>();
Cube("Table",new(-4,.4f,-4),new(2,.8f,1));
var steam=new GameObject("Steam placeholder").AddComponent<ParticleSystem>();steam.transform.position=new(-4,1,-4);var main=steam.main;main.startSpeed=.2f;main.startLifetime=1;main.startSize=.1f;main.startColor=new Color(1,1,1,.3f);
var radio=Cube("Radio",new(-4,1,-1),Vector3.one);radio.AddComponent<NoiseEmitter>();radio.AddComponent<Radio>();
var cellar=Cube("Locked Cellar Door",new(-9,1.5f,3),new(.3f,3,2));cellar.AddComponent<NoiseEmitter>();var cellarDoor=cellar.AddComponent<Door>();Set(cellarDoor,"locked",true);Set(cellarDoor,"doorId","cellar-door");
var trail=new GameObject("Wet footprints (Act 2)").AddComponent<WetFootprints>();trail.transform.position=new(-9,0,0);
var hum=new GameObject("Cellar Hum");hum.transform.position=cellar.transform.position;var zone=hum.AddComponent<BoxCollider>();zone.isTrigger=true;zone.size=new(4,3,4);var humComponent=hum.AddComponent<CellarHum>();Set(humComponent,"footprints",trail);
var canvas=GameObject.Find("Debug Canvas").transform;
var subtitleText=Label("Subtitles",canvas,new(.5f,.08f));
var subtitleUI=subtitleText.gameObject.AddComponent<SubtitleUI>();Set(subtitleUI,"label",subtitleText);
var objective=Label("Objective",canvas,new(.5f,.94f));Set(objective.gameObject.AddComponent<ObjectiveUI>(),"label",objective);
new GameObject("Game State").AddComponent<GameStateMachine>();
var sun=new GameObject("Dusk Sun").AddComponent<Light>();sun.type=LightType.Directional;sun.transform.rotation=Quaternion.Euler(20,-30,0);
var dusk=new GameObject("Dusk Timer").AddComponent<DuskTimer>();Set(dusk,"sun",sun);
Directory.CreateDirectory(AssetFolder);AssetDatabase.Refresh();
var beats=new StoryBeat[6];
beats[0]=Beat("A1-B01",StoryTrigger.Signal,"arrival","",new DialogueLine[0]);
beats[1]=Beat("A1-B02",StoryTrigger.Signal,"dock","Reach the tower",new DialogueLine[0]);
beats[2]=Beat("A1-B03",StoryTrigger.DoorOpened,"tower-front-door","Inspect the meal",new[]{new DialogueLine("Daniel","Crane?")});
beats[3]=Beat("A1-B04",StoryTrigger.Signal,"meal","Operate the radio",new[]{new DialogueLine("Daniel","This was just made.")});
beats[4]=Beat("A1-B05",StoryTrigger.Signal,"radio","Light the lamp",new[]{new DialogueLine("Daniel","Who is this? Where's Thomas Crane?"),new DialogueLine("Marta","Marta. Harbor relay. Questions after the lamp, Daniel. You have minutes."),new DialogueLine("Daniel","I never gave you my name.")});
beats[5]=Beat("A1-B06",StoryTrigger.LampLit,"","Act 1 complete — keep the lamp fed",new[]{new DialogueLine("Marta","There. Now keep it fed.")});
var director=new GameObject("Story Director").AddComponent<StoryDirector>();var so=new SerializedObject(director);var array=so.FindProperty("beats");array.arraySize=beats.Length;for(int i=0;i<beats.Length;i++)array.GetArrayElementAtIndex(i).objectReferenceValue=beats[i];so.ApplyModifiedPropertiesWithoutUndo();
var boatman=new GameObject("Boatman").AddComponent<Boatman>();Set(boatman,"player",player.GetComponent<FirstPersonController>());Set(boatman,"subtitles",subtitleUI);Set(boatman,"boat",boat.transform);
EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),scenePath);AssetDatabase.SaveAssets();
Debug.Log("Act 1 built. Bake Navigation/NavMeshSurface, then Play.");
}
static StoryBeat Beat(string id,StoryTrigger trigger,string signal,string objective,DialogueLine[] lines){string path=AssetFolder+"/"+id+".asset";var beat=AssetDatabase.LoadAssetAtPath<StoryBeat>(path);if(beat)return beat;if(!beat){beat=ScriptableObject.CreateInstance<StoryBeat>();AssetDatabase.CreateAsset(beat,path);}beat.id=id;beat.trigger=trigger;beat.triggerId=signal;beat.objective=objective;beat.lines=lines;EditorUtility.SetDirty(beat);return beat;}
static GameObject Cube(string name,Vector3 position,Vector3 scale){var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.position=position;go.transform.localScale=scale;return go;}
static Text Label(string name,Transform parent,Vector2 anchor){var go=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(Text));go.transform.SetParent(parent,false);var rect=go.GetComponent<RectTransform>();rect.anchorMin=rect.anchorMax=anchor;rect.sizeDelta=new(1400,100);var text=go.GetComponent<Text>();text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");text.fontSize=26;text.alignment=TextAnchor.MiddleCenter;text.raycastTarget=false;return text;}
static void Set(Object target,string name,Object value){var so=new SerializedObject(target);so.FindProperty(name).objectReferenceValue=value;so.ApplyModifiedPropertiesWithoutUndo();}
static void Set(Object target,string name,bool value){var so=new SerializedObject(target);so.FindProperty(name).boolValue=value;so.ApplyModifiedPropertiesWithoutUndo();}
static void Set(Object target,string name,float value){var so=new SerializedObject(target);so.FindProperty(name).floatValue=value;so.ApplyModifiedPropertiesWithoutUndo();}
static void Set(Object target,string name,string value){var so=new SerializedObject(target);so.FindProperty(name).stringValue=value;so.ApplyModifiedPropertiesWithoutUndo();}
}}
