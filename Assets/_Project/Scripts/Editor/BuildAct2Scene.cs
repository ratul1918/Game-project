using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;

namespace TheLightHouse.Editor
{
    public static class BuildAct2Scene
    {
        const string ScenePath="Assets/_Project/Scenes/Act2.unity";
        const string BeatFolder="Assets/_Project/StoryBeats";
        const int FigureCount=4;
        const int FigurePathPoints=5;
        const float StartingFuel=65f;
        const float DrainRate=.5f;

        [MenuItem("TheLightHouse/Build Act 2 Scene")]
        public static void Build()
        {
            BuildTo(ScenePath);
        }
        public static void BuildTo(string scenePath)
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)return;
            BuildAct1Scene.BuildTo(scenePath);
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if(scene.path!=scenePath)return;
            Object.DestroyImmediate(GameObject.Find("Boatman"));
            var player=GameObject.Find("Player");
            player.transform.position=new(0,3.1f,5);
            var health=player.AddComponent<PlayerHealthState>();
            Set(Object.FindAnyObjectByType<GameStateMachine>(),"startAtNight",true);
            var lamp=Object.FindAnyObjectByType<LampController>();
            Set(lamp,"startWarmButUnlit",false);Set(lamp,"enableNightDrain",true);
            Set(lamp,"startingFuel",StartingFuel);Set(lamp,"nightDrainRate",DrainRate);
            Set(lamp.GetComponent<LanternInteract>(),"fuelObjective","Find more fuel in the storeroom");
            var stalker=GameObject.Find("Stalker");
            if(!stalker)foreach(var root in scene.GetRootGameObjects())if(root.name=="Stalker"){stalker=root;break;}
            var ai=stalker.GetComponent<StalkerAI>();
            Set(ai,"actTwo",true);Set(ai,"player",health);
            stalker.AddComponent<CraneHerding>();
            Set(stalker.GetComponent<CraneHerding>(),"boatShed",GameObject.Find("Boat Shed Floor").transform);
            var spawn=new GameObject("Hidden Crane Spawn").transform;spawn.position=new(-7,0,7);
            stalker.transform.position=spawn.position;
            var spawner=new GameObject("Stalker Spawner").AddComponent<StalkerSpawner>();
            Set(spawner,"stalker",stalker);Set(spawner,"spawn",spawn);
            var route=new Transform[5];
            Vector3[] points={new(-6,0,6),new(-4,0,-3),new(4,0,-3),new(7,0,-4),new(7,3,2.7f)};
            for(int i=0;i<route.Length;i++){route[i]=new GameObject("Patrol "+i).transform;route[i].position=points[i];}
            SetArray(ai,"waypoints",route);
            var actions=AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions");
            var spots=new HidingSpot[3];
            Vector3[] spotPositions={new(-6,0,-5),new(-4,-1,-4),new(4,0,6)};
            string[] names={"Wardrobe","Under table","Crate"};
            for(int i=0;i<spots.Length;i++)
            {
                var box=Cube(names[i]+" hiding spot",new Vector3(spotPositions[i].x,.5f,spotPositions[i].z),new(1.4f,1,1.2f));
                spots[i]=box.AddComponent<HidingSpot>();
                Object.DestroyImmediate(box.GetComponent<MeshRenderer>());
                Object.DestroyImmediate(box.GetComponent<BoxCollider>());
                if(i==1)
                {
                    var table=GameObject.Find("Table");table.transform.position=new(-4,.8f,-4);table.transform.localScale=new(2,.1f,1);
                    table.transform.SetParent(box.transform,true);
                    Part("Table leg",new(-4.8f,.4f,-4.4f),new(.12f,.8f,.12f),box.transform);
                    Part("Table leg",new(-3.2f,.4f,-4.4f),new(.12f,.8f,.12f),box.transform);
                    Part("Table leg",new(-4.8f,.4f,-3.6f),new(.12f,.8f,.12f),box.transform);
                    Part("Table leg",new(-3.2f,.4f,-3.6f),new(.12f,.8f,.12f),box.transform);
                }
                else
                {
                    Vector3 center=new(spotPositions[i].x,1.1f,spotPositions[i].z);
                    Part("Back",center+new Vector3(-.65f,0,0),new(.15f,2.2f,1.4f),box.transform);
                    Part("Side",center+new Vector3(0,0,-.65f),new(1.4f,2.2f,.15f),box.transform);
                    Part("Side",center+new Vector3(0,0,.65f),new(1.4f,2.2f,.15f),box.transform);
                    Part("Front left",center+new Vector3(.65f,0,-.4f),new(.15f,2.2f,.5f),box.transform);
                    Part("Front right",center+new Vector3(.65f,0,.4f),new(.15f,2.2f,.5f),box.transform);
                    Part("Roof",center+Vector3.up*1.1f,new(1.4f,.15f,1.4f),box.transform);
                }
                var pose=new GameObject(names[i]+" pose").transform;pose.position=spotPositions[i];pose.rotation=Quaternion.Euler(0,90,0);
                Set(spots[i],"spotId",names[i]);Set(spots[i],"hidingPose",pose);Set(spots[i],"inputActions",actions);
                Set(spots[i],"player",health);Set(spots[i],"stalker",stalker.transform);
            }
            SetArray(ai,"hidingSpots",spots);
            var catchSequence=new GameObject("Catch Sequence").AddComponent<CatchSequence>();
            Set(catchSequence,"player",health);Set(catchSequence,"stalker",stalker.transform);
            GameObject.Find("Debug Canvas").AddComponent<HideBreathBar>();
            var hint=GameObject.Find("Debug Canvas").AddComponent<StalkerAwarenessHint>();
            Set(hint,"player",player.transform);Set(hint,"stalker",stalker.transform);

            Set(Object.FindAnyObjectByType<CellarHum>(),"enabledForActTwo",true);
            var footprints=Object.FindAnyObjectByType<WetFootprints>();
            footprints.transform.SetPositionAndRotation(new(-5.5f,0,3),Quaternion.Euler(0,-90,0));
            var reserve=Cube("Reserve Fuel Can",new(-7,.5f,2),Vector3.one);
            reserve.AddComponent<FuelCan>();
            var reserveController=new GameObject("Reserve Fuel Activator").AddComponent<ReserveFuel>();
            Set(reserveController,"fuelCan",reserve);
            Cube("Storeroom rear wall",new(-6.5f,1.5f,5),new(3,3,.2f));
            Cube("Storeroom divider",new(-5,1.5f,3),new(.2f,3,4));

            Object.DestroyImmediate(GameObject.Find("East Wall"));
            Cube("East window sill",new(10,.5f,0),new(.5f,1,20));
            Cube("East window lintel",new(10,4.3f,0),new(.5f,3.4f,20));
            Cube("Window pier south",new(10,1.8f,-8.5f),new(.5f,1.6f,3));
            Cube("Window pier middle",new(10,1.8f,-1),new(.5f,1.6f,4));
            Cube("Window pier north",new(10,1.8f,7.5f),new(.5f,1.6f,5));
            Cube("Shallows",new(21,-.3f,0),new(22,.2f,26));
            var figures=new DrownedFigure[FigureCount];
            for(int i=0;i<FigureCount;i++)
            {
                var root=new GameObject("Drowned "+i);
                var position=new Vector3(17+i*3,0,-5+i*3);
                root.transform.position=position;
                var body=GameObject.CreatePrimitive(PrimitiveType.Capsule);body.name="Pale placeholder";body.transform.SetParent(root.transform,false);body.transform.localPosition=Vector3.up;
                figures[i]=root.AddComponent<DrownedFigure>();Set(figures[i],"figureId","shallows-"+i);
                var serialized=new SerializedObject(figures[i]);var path=serialized.FindProperty("path");path.arraySize=FigurePathPoints;
                Vector3[] figurePath={position,new(14+i,0,-7+i*.6f),new(11+i*.5f,0,-11-i*.5f),new(6+i*.7f,0,-11-i*.5f),new(i*.5f,0,-11)};
                for(int j=0;j<figurePath.Length;j++)path.GetArrayElementAtIndex(j).vector3Value=figurePath[j];
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            var drowned=new GameObject("Drowned Manager").AddComponent<DrownedManager>();SetArray(drowned,"figures",figures);Set(drowned,"view",player.GetComponentInChildren<Camera>());
            var frontDoor=GameObject.Find("Tower Door").GetComponent<Door>();Set(frontDoor,"blockDuringActTwo",true);
            var knock=new GameObject("Front Door Knocking");knock.transform.position=new(0,1,-10);knock.AddComponent<NoiseEmitter>();knock.AddComponent<DoorKnocker>();
            var lure=new GameObject("Singing From Shore");lure.transform.position=new(15,1,-2);lure.AddComponent<NoiseEmitter>();var singing=lure.AddComponent<SingingLure>();
            Set(singing,"player",player.transform);Set(singing,"figures",drowned);
            var lureTargets=new Transform[3];
            Vector3[] targetPoints={new(8,1,-5),new(8,1,3),new(0,1,-9)};
            for(int i=0;i<3;i++){lureTargets[i]=new GameObject("Lure approach "+i).transform;lureTargets[i].position=targetPoints[i];}
            SetArray(singing,"lureTargets",lureTargets);

            Directory.CreateDirectory(BeatFolder);AssetDatabase.Refresh();
            var beats=new StoryBeat[7];
            beats[0]=Beat("A2-B01","fuel-low","Find more fuel in the storeroom");
            beats[1]=Beat("A2-B02","cellar-hum","Inspect the wet footprints");
            beats[2]=Beat("A2-B03","footprints","Keep quiet. Find somewhere to hide.");
            beats[3]=Beat("A2-B04","stalker-noise","Stay quiet; watch the shallows through the windows");
            beats[4]=Beat("A2-B05","figure-advanced","Keep the light alive");
            beats[5]=Beat("A2-B06","knocking","Do not open the front door");
            beats[6]=Beat("A2-B07","hide-success","You escaped notice. Keep the lamp fed.");
            var director=Object.FindAnyObjectByType<StoryDirector>();SetArray(director,"beats",beats);Set(director,"allowPendingActTwoSignals",true);
            stalker.SetActive(false);
            EditorSceneManager.SaveScene(scene,scenePath);AssetDatabase.SaveAssets();
            Debug.Log("Act 2 saved at Night. Bake Navigation/NavMeshSurface, then Play. Crane spawns after A2-B03.");
        }
        static StoryBeat Beat(string id,string signal,string objective)
        {
            string path=BeatFolder+"/"+id+".asset";var asset=AssetDatabase.LoadAssetAtPath<StoryBeat>(path);
            if(asset)return asset;
            asset=ScriptableObject.CreateInstance<StoryBeat>();asset.id=id;asset.trigger=StoryTrigger.Signal;asset.triggerId=signal;asset.objective=objective;asset.lines=System.Array.Empty<DialogueLine>();
            AssetDatabase.CreateAsset(asset,path);return asset;
        }
        static GameObject Cube(string name,Vector3 position,Vector3 scale){var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.position=position;go.transform.localScale=scale;return go;}
        static void Part(string name,Vector3 position,Vector3 scale,Transform parent){Cube(name,position,scale).transform.SetParent(parent,true);}
        static void Set(Object target,string name,Object value){var so=new SerializedObject(target);so.FindProperty(name).objectReferenceValue=value;so.ApplyModifiedPropertiesWithoutUndo();}
        static void Set(Object target,string name,bool value){var so=new SerializedObject(target);so.FindProperty(name).boolValue=value;so.ApplyModifiedPropertiesWithoutUndo();}
        static void Set(Object target,string name,float value){var so=new SerializedObject(target);so.FindProperty(name).floatValue=value;so.ApplyModifiedPropertiesWithoutUndo();}
        static void Set(Object target,string name,string value){var so=new SerializedObject(target);so.FindProperty(name).stringValue=value;so.ApplyModifiedPropertiesWithoutUndo();}
        static void SetArray<T>(Object target,string name,T[] values) where T:Object
        {
            var so=new SerializedObject(target);var array=so.FindProperty(name);array.arraySize=values.Length;
            for(int i=0;i<values.Length;i++)array.GetArrayElementAtIndex(i).objectReferenceValue=values[i];so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
