using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace TheLightHouse.Editor
{
 public static class BuildAct3Scene
 {
  const string ScenePath="Assets/_Project/Scenes/Act3.unity";
  public const string SettingsPath="Assets/_Project/Settings/StormSettings.asset";
  [MenuItem("TheLightHouse/Build Act 3 Scene")]
  public static void Build()
  {
   if(EditorApplication.isPlayingOrWillChangePlaymode)return;
   var settings=AssetDatabase.LoadAssetAtPath<StormSettings>(SettingsPath);
   if(!settings){Directory.CreateDirectory("Assets/_Project/Settings");AssetDatabase.Refresh();settings=ScriptableObject.CreateInstance<StormSettings>();AssetDatabase.CreateAsset(settings,SettingsPath);}
   BuildAct2Scene.BuildTo(ScenePath);
   var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();if(scene.path!=ScenePath)return;
   // NewScene can unload assets referenced only by the calling stack; reload before wiring.
   settings=AssetDatabase.LoadAssetAtPath<StormSettings>(SettingsPath);
   var state=Object.FindAnyObjectByType<GameStateMachine>();Set(state,"startAtNight",false);Set(state,"startAtStorm",true);
   var player=GameObject.Find("Player");var stamina=player.GetComponent<PlayerStamina>()??player.AddComponent<PlayerStamina>();
   var spawner=Object.FindAnyObjectByType<StalkerSpawner>();if(spawner)Object.DestroyImmediate(spawner);
   GameObject stalker=null;foreach(var root in scene.GetRootGameObjects())if(root.name=="Stalker")stalker=root;
   stalker.SetActive(true);var ai=stalker.GetComponent<StalkerAI>();Set(ai,"stormSettings",settings);var stairAudio=stalker.AddComponent<AudioSource>();stairAudio.spatialBlend=1;stairAudio.playOnAwake=false;
   var actorModifier=stalker.AddComponent<Unity.AI.Navigation.NavMeshModifier>();actorModifier.ignoreFromBuild=true;
   var lamp=Object.FindAnyObjectByType<LampController>();Set(lamp,"stormSettings",settings);
   var beam=Cube("Rotating lamp beam placeholder",new(0,5.2f,6),new(.12f,.12f,2));Object.DestroyImmediate(beam.GetComponent<Collider>());Set(lamp,"beam",beam.transform);
   var crank=Object.FindAnyObjectByType<CrankStation>();Set(crank,"stormSettings",settings);Set(crank,"stamina",stamina);
   foreach(var obstacle in new[]{lamp.gameObject,crank.gameObject})
   {
    var modifier=obstacle.AddComponent<Unity.AI.Navigation.NavMeshModifier>();
    modifier.overrideArea=true;modifier.area=UnityEngine.AI.NavMesh.GetAreaFromName("Not Walkable");
   }
   var handle=Cube("Crank Handle",crank.transform.position+Vector3.up*.85f,new(.65f,.12f,.12f));handle.transform.SetParent(crank.transform,true);Object.DestroyImmediate(handle.GetComponent<Collider>());Set(crank,"handle",handle.transform);
   var gauge=Cube("Lamp charge gauge",new(1,4.5f,6),new(.15f,1,.15f));Object.DestroyImmediate(gauge.GetComponent<Collider>());
   var ui=lamp.gameObject.AddComponent<LampChargeUI>();Set(ui,"gauge",gauge.transform);
   var canvas=GameObject.Find("Debug Canvas");var options=canvas.AddComponent<AccessibilityOptions>();Set(options,"inputActions",AssetDatabase.LoadAssetAtPath<UnityEngine.InputSystem.InputActionAsset>("Assets/InputSystem_Actions.inputactions"));
   var hud=canvas.GetComponent<DebugHud>();if(hud)hud.enabled=false;
   GameObject.Find("Fuel Text").SetActive(false);GameObject.Find("Stalker State Text").SetActive(false);
   var rainObject=new GameObject("Storm rain placeholder");rainObject.transform.SetPositionAndRotation(new(0,9,0),Quaternion.Euler(90,0,0));
   var rain=rainObject.AddComponent<ParticleSystem>();rain.Stop();var main=rain.main;main.startSpeed=15;main.startLifetime=1;main.startSize=.025f;main.maxParticles=2000;var shape=rain.shape;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=new(35,30,1);var emission=rain.emission;emission.rateOverTime=0;
   var wind=new GameObject("Storm Wind").AddComponent<WindZone>();wind.windMain=0;
   var storm=new GameObject("Storm Controller").AddComponent<StormController>();Set(storm,"settings",settings);Set(storm,"rain",rain);Set(storm,"wind",wind);Set(storm,"skyLight",GameObject.Find("Dusk Sun").GetComponent<Light>());Set(storm,"accessibility",options);
   var mask=GameObject.Find("Noise Manager").AddComponent<NoiseMasking>();Set(mask,"settings",settings);Set(Object.FindAnyObjectByType<NoiseManager>(),"masking",mask);
   Cube("Generator room partition",new(2,1.5f,-7),new(.2f,3,4));
   var generatorObject=Cube("Flooded Generator",new(5,.75f,-7),new(2,1.5f,1));
   generatorObject.AddComponent<NoiseEmitter>();var generator=generatorObject.AddComponent<Generator>();Set(generator,"settings",settings);
   var bulb=new GameObject("Generator electric light").AddComponent<Light>();bulb.transform.position=new(5,2.5f,-7);bulb.type=LightType.Point;bulb.range=7;
   Array(generator,"electricLights",new Object[]{bulb});
   var bridge=new GameObject("Storm Beats").AddComponent<StormBeats>();Set(bridge,"player",player.transform);
   var ending=new GameObject("Storm End Condition").AddComponent<StormEndCondition>();Set(ending,"settings",settings);

   var template=GameObject.Find("Wardrobe hiding spot");
   var extraSpots=new HidingSpot[3];Vector3[] poses={new(-3,3,5),new(1,3,7.8f),new(4.5f,3,3.5f)};
   string[] names={"Lantern Cabinet","Lamp Base Screen","Stair Landing Cabinet"};
   for(int i=0;i<3;i++){var hide=Object.Instantiate(template);hide.name=names[i];hide.transform.position=new(poses[i].x,poses[i].y+.5f,poses[i].z);extraSpots[i]=hide.GetComponent<HidingSpot>();var pose=new GameObject(names[i]+" pose").transform;pose.position=poses[i];pose.rotation=Quaternion.Euler(0,90,0);Set(extraSpots[i],"hidingPose",pose);Set(extraSpots[i],"spotId",names[i]);}
   Array(ai,"hidingSpots",Object.FindObjectsByType<HidingSpot>().Cast<Object>().ToArray());
   var manager=Object.FindAnyObjectByType<DrownedManager>();Set(manager,"stormSettings",settings);
   var figures=Object.FindObjectsByType<DrownedFigure>().ToList();
   for(int i=0;i<4;i++)
   {
    var root=Object.Instantiate(figures[0].gameObject);root.name="Courtyard Drowned "+i;var figure=root.GetComponent<DrownedFigure>();figures.Add(figure);Set(figure,"figureId","courtyard-"+i);
    Vector3 initial=new(-6+i*4,0,-17);root.transform.position=initial;
    var serialized=new SerializedObject(figure);var path=serialized.FindProperty("path");path.arraySize=5;
    for(int j=0;j<5;j++)path.GetArrayElementAtIndex(j).vector3Value=Vector3.Lerp(initial,new Vector3(-1.5f+i,0,-11),j/4f);
    serialized.ApplyModifiedPropertiesWithoutUndo();
   }
   Array(manager,"figures",figures.Cast<Object>().ToArray());
   Object.DestroyImmediate(GameObject.Find("East window lintel"));
   Cube("Upper window floor band",new(10,2.825f,0),new(.5f,.45f,20));
   Cube("Upper window roof lintel",new(10,5.7f,0),new(.5f,.6f,20));
   Cube("Upper window south pier",new(10,4.2f,-8.5f),new(.5f,2.4f,3));
   Cube("Upper window middle pier",new(10,4.2f,-1),new(.5f,2.4f,4));
   Cube("Upper window north pier",new(10,4.2f,7.5f),new(.5f,2.4f,5));

   var director=Object.FindAnyObjectByType<StoryDirector>();
   Array(director,"beats",new Object[]{
    Beat("A3-B01","storm-start","The storm is coming"),
    Beat("A3-B02","generator-dies","Keep the lamp alive with the crank"),
    Beat("A3-B03","first-crank","Crank, hide, and watch the windows"),
    Beat("A3-B04","storm-darkness","Restore the light")});
   Array(director,"optionalBeats",new Object[]{
    Beat("A3-B05","generator-restart-fails","","A3-B02"),
    Beat("A3-B06","stalker-stairs","","A3-B03"),
    Beat("A3-B07","lamp-pulse","","A3-B03"),
    Beat("A3-B08","courtyard-filling","","A3-B04"),
    Beat("A3-B09","storm-end","Act 3 complete — Crane's logbook is available (Act 4 pending)","A3-B04")});
   EditorSceneManager.SaveScene(scene,ScenePath);AssetDatabase.SaveAssets();
   Debug.Log("Act 3 built. Bake Navigation/NavMeshSurface, save, then Play. Reduced storm visuals are enabled by default.");
  }
  static StoryBeat Beat(string id,string signal,string objective,string prerequisite="")
  {
   string path="Assets/_Project/StoryBeats/"+id+".asset";var beat=AssetDatabase.LoadAssetAtPath<StoryBeat>(path);if(beat)return beat;
   beat=ScriptableObject.CreateInstance<StoryBeat>();beat.id=id;beat.trigger=StoryTrigger.Signal;beat.triggerId=signal;beat.objective=objective;beat.prerequisiteId=prerequisite;beat.lines=System.Array.Empty<DialogueLine>();AssetDatabase.CreateAsset(beat,path);return beat;
  }
  static GameObject Cube(string name,Vector3 position,Vector3 scale){var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.position=position;go.transform.localScale=scale;return go;}
  static void Set(Object target,string name,Object value){var so=new SerializedObject(target);so.FindProperty(name).objectReferenceValue=value;so.ApplyModifiedPropertiesWithoutUndo();}
  static void Set(Object target,string name,bool value){var so=new SerializedObject(target);so.FindProperty(name).boolValue=value;so.ApplyModifiedPropertiesWithoutUndo();}
  static void Set(Object target,string name,string value){var so=new SerializedObject(target);so.FindProperty(name).stringValue=value;so.ApplyModifiedPropertiesWithoutUndo();}
  static void Array(Object target,string name,Object[] values){var so=new SerializedObject(target);var array=so.FindProperty(name);array.arraySize=values.Length;for(int i=0;i<values.Length;i++)array.GetArrayElementAtIndex(i).objectReferenceValue=values[i];so.ApplyModifiedPropertiesWithoutUndo();}
 }
}
