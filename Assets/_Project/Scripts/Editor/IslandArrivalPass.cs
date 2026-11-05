using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

namespace TheLightHouse.Editor
{
    public static class IslandArrivalPass
    {
        const string RootName="Island Arrival";
        const string Folder="Assets/_Project/Art/Island";
        static Transform root;
        static Material wood,iron,stone,water,coat,skin;
        static IslandArrivalSettings settings;
        static string act;

        [MenuItem("TheLightHouse/Apply Island Arrival and Doors")]
        public static void ApplyAll()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Exit Play Mode first.");
            if(Enumerable.Range(0,SceneManager.sceneCount).Any(i=>SceneManager.GetSceneAt(i).isDirty))throw new InvalidOperationException("Save scene changes first.");
            var setup=EditorSceneManager.GetSceneManagerSetup();
            Prepare();
            try
            {
                for(int i=1;i<=5;i++)
                {
                    var scene=EditorSceneManager.OpenScene($"Assets/_Project/Scenes/Act{i}.unity");
                    ApplyToOpenScene();EditorSceneManager.SaveScene(scene);
                }
                AssetDatabase.SaveAssets();
            }
            finally{if(setup.Any(s=>s.isLoaded)&&setup.Any(s=>s.isActive))EditorSceneManager.RestoreSceneManagerSetup(setup);}
        }
        static void Prepare()
        {
            Directory.CreateDirectory(Folder);AssetDatabase.Refresh();
            const string path="Assets/_Project/Settings/IslandArrivalSettings.asset";
            settings=AssetDatabase.LoadAssetAtPath<IslandArrivalSettings>(path);
            if(!settings){settings=ScriptableObject.CreateInstance<IslandArrivalSettings>();AssetDatabase.CreateAsset(settings,path);}
            wood=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Reference/Old Timber.mat");
            iron=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Reference/Oxidised Iron.mat");
            stone=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Reference/Wet Shale.mat");
            coat=ReferenceSurfaceLibrary.Solid("Boatman Oilskin",new(.065f,.08f,.07f));
            skin=ReferenceSurfaceLibrary.Solid("Boatman Skin",new(.3f,.27f,.22f));
            const string seaPath="Assets/_Project/Materials/Reference/Harrow Sea.mat";
            water=AssetDatabase.LoadAssetAtPath<Material>(seaPath);if(!water){water=new Material(Shader.Find("TheLightHouse/Harrow Sea"));AssetDatabase.CreateAsset(water,seaPath);}
            water.SetFloat("_WaveHeight",settings.waveHeight);EditorUtility.SetDirty(water);
        }
        public static void ApplyToOpenScene()
        {
            if(!settings)Prepare();
            act=SceneManager.GetActiveScene().name;
            var old=GameObject.Find(RootName);if(old)Object.DestroyImmediate(old);
            root=new GameObject(RootName).transform;
            foreach(var fog in Object.FindObjectsByType<FogCards>(FindObjectsSortMode.None))fog.gameObject.SetActive(false);
            foreach(string name in new[]{"Beach","Sea Plane","Cliff West","Cliff East","Dock","Cliff Path","Courtyard Window L0","Shallows Window L0","Salt Grass"})
            {
                var go=GameObject.Find(name);if(!go)continue;
                foreach(var r in go.GetComponents<Renderer>())r.enabled=false;
                foreach(var c in go.GetComponents<Collider>())c.enabled=false;
            }
            var coast=GameObject.Find("Reference Visuals/Coast Dressing");if(coast)coast.SetActive(false);
            foreach(var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
                if(r.name.StartsWith("Rock South ")||r.name.StartsWith("Rock North "))
                {r.enabled=false;foreach(var c in r.GetComponents<Collider>())c.enabled=false;}
            Land();Scrub();Sea();Dock();Boat();Doors();
            if(act=="Act1")
            {
                var player=GameObject.Find("Player");player.transform.position=settings.playerStart;
                Vector3 direction=settings.boatPosition+new Vector3(0,settings.boatmanHeight,0)-(settings.playerStart+Vector3.up*1.65f);
                player.transform.rotation=Quaternion.Euler(0,Mathf.Atan2(direction.x,direction.z)*Mathf.Rad2Deg,0);
                var camera=player.GetComponentInChildren<Camera>();if(camera)camera.transform.localRotation=Quaternion.Euler(-Mathf.Atan2(direction.y,new Vector2(direction.x,direction.z).magnitude)*Mathf.Rad2Deg,0,0);
            }
            Debug.Log($"{act}: island, sea, boat, arrival view and three operable doorways authored.");
        }
        static GameObject Box(string name,Transform parent,Vector3 position,Vector3 size,Material material,bool collision=false)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=position;go.transform.localScale=size;go.GetComponent<Renderer>().sharedMaterial=material;
            if(!collision)Object.DestroyImmediate(go.GetComponent<Collider>());else go.tag=material==wood?"Wood":"Stone";
            return go;
        }
        static void Rod(string name,Transform parent,Vector3 a,Vector3 b,float width,Material material)
        {
            var go=Box(name,parent,(a+b)*.5f,new(width,Vector3.Distance(a,b),width),material);go.transform.localRotation=Quaternion.FromToRotation(Vector3.up,b-a);
        }
        static Mesh SaveMesh(string name,List<Vector3> vertices,List<int> triangles,List<Vector2> uv=null)
        {
            if(vertices.Any(p=>float.IsNaN(p.x)||float.IsNaN(p.y)||float.IsNaN(p.z)||float.IsInfinity(p.x)||float.IsInfinity(p.y)||float.IsInfinity(p.z)))throw new InvalidOperationException($"Invalid vertices in {name}");
            string path=$"{Folder}/{act}-{name}.asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            bool exists=mesh;if(!exists)mesh=new Mesh{name=name};else mesh.Clear();
            mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.SetUVs(0,uv??vertices.Select(p=>new Vector2(p.x,p.z)*.3f).ToList());mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();
            if(!exists)AssetDatabase.CreateAsset(mesh,path);else EditorUtility.SetDirty(mesh);return mesh;
        }
        static GameObject MeshObject(string name,Mesh mesh,Material material,Transform parent,bool collision=false)
        {
            var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(parent,false);go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<Renderer>().sharedMaterial=material;
            if(collision){go.AddComponent<MeshCollider>().sharedMesh=mesh;go.tag="Stone";}return go;
        }
        static void Land()
        {
            var points=new List<Vector3>{new(0,settings.landHeight,-6)};var triangles=new List<int>();
            int count=settings.shoreSegments;
            for(int ring=0;ring<2;ring++)for(int i=0;i<count;i++)
            {
                float angle=i*Mathf.PI*2/count;float variation=1+.035f*Mathf.Sin(angle*7)+.025f*Mathf.Cos(angle*11);float radius=ring==0?.92f:1;
                points.Add(new(Mathf.Sin(angle)*settings.islandRadius.x*radius*variation,ring==0?settings.landHeight:settings.seaLevel-1.2f,-6+Mathf.Cos(angle)*settings.islandRadius.y*radius*variation));
            }
            for(int i=0;i<count;i++)
            {
                int j=(i+1)%count;triangles.AddRange(new[]{0,i+1,j+1});int a=i+1,b=j+1,c=count+i+1,d=count+j+1;triangles.AddRange(new[]{a,c,b,b,c,d});
            }
            MeshObject("Harrow Rock Island",SaveMesh("Island",points,triangles),stone,root,true);
            var random=new System.Random(84);
            for(int i=0;i<settings.shoreRockCount;i++)
            {
                float a=i*Mathf.PI*2/settings.shoreRockCount;if(Mathf.Abs(Mathf.DeltaAngle(a*Mathf.Rad2Deg,180))<13)continue;
                var rock=GameObject.CreatePrimitive(PrimitiveType.Sphere);rock.name="Shore Boulder";rock.transform.SetParent(root);rock.transform.position=new(Mathf.Sin(a)*settings.islandRadius.x*.94f,settings.seaLevel-.05f,-6+Mathf.Cos(a)*settings.islandRadius.y*.94f);rock.transform.localScale=new(1.6f+(float)random.NextDouble()*2,1.1f,1.7f);rock.GetComponent<Renderer>().sharedMaterial=stone;Object.DestroyImmediate(rock.GetComponent<Collider>());
            }
        }
        static void Scrub()
        {
            var oldGrass=GameObject.Find("Salt Grass");
            if(oldGrass)Object.DestroyImmediate(oldGrass);
        }
        static void Sea()
        {
            var points=new List<Vector3>();var triangles=new List<int>();int n=settings.seaSegments;
            for(int z=0;z<=n;z++)for(int x=0;x<=n;x++)points.Add(new((x/(float)n-.5f)*settings.seaSize,settings.seaLevel,(z/(float)n-.5f)*settings.seaSize));
            for(int z=0;z<n;z++)for(int x=0;x<n;x++){int k=z*(n+1)+x;triangles.AddRange(new[]{k,k+n+1,k+1,k+1,k+n+1,k+n+2});}
            MeshObject("Open Sea",SaveMesh("Sea",points,triangles),water,root);
        }
        static void Dock()
        {
            Box("Island Path",root,new(0,settings.landHeight+.025f,-19),new(3.6f,.1f,12),stone,true);
            for(int i=0;i<28;i++)Box("Dock Plank",root,new(0,settings.landHeight+.02f,-25-i*.3f),new(3.2f,.18f,.28f),wood,true);
            for(int side=-1;side<=1;side+=2)for(int i=0;i<5;i++)
            {
                float z=-25-i*2;Box("Dock Pile",root,new(side*1.35f,-.2f,z),new(.18f,1.8f,.18f),wood);
                if(i<3)Rod("Dock Rope",root,new(side*1.4f,.7f,z),new(side*1.4f,.7f,z-2),.035f,iron);
            }
            for(int side=-1;side<=1;side+=2)Box("Dock Bearer",root,new(side*1.1f,settings.landHeight-.18f,-29),new(.2f,.25f,8.8f),wood);
        }
        static void Boat()
        {
            var boat=GameObject.Find("Boat");if(!boat)return;
            foreach(var r in boat.GetComponents<Renderer>())r.enabled=false;foreach(var c in boat.GetComponents<Collider>())c.enabled=false;
            var previous=boat.transform.Find("Boat Visuals");if(previous)Object.DestroyImmediate(previous.gameObject);
            boat.transform.localScale=Vector3.one;boat.transform.position=settings.boatPosition;boat.transform.rotation=Quaternion.identity;
            var visuals=new GameObject("Boat Visuals").transform;visuals.SetParent(boat.transform,false);
            var points=new List<Vector3>();var triangles=new List<int>();const int sections=12;
            for(int i=0;i<=sections;i++)
            {
                float t=i/(float)sections,z=Mathf.Lerp(-3,3,t),width=.12f+1.05f*Mathf.Pow(Mathf.Max(0,Mathf.Sin(t*Mathf.PI)),.6f);
                points.AddRange(new[]{new Vector3(-width,.42f,z),new Vector3(-width*.65f,-.18f,z),new Vector3(0,-.48f,z),new Vector3(width*.65f,-.18f,z),new Vector3(width,.42f,z)});
            }
            for(int i=0;i<sections;i++)for(int j=0;j<4;j++){int k=i*5+j;triangles.AddRange(new[]{k,k+5,k+1,k+1,k+5,k+6});}
            // Separate back-face vertices preserve outward lighting on the open hull.
            int vertexCount=points.Count;var inside=triangles.ToArray();points.AddRange(points.ToArray());
            for(int i=0;i<inside.Length;i+=3)triangles.AddRange(new[]{inside[i+2]+vertexCount,inside[i+1]+vertexCount,inside[i]+vertexCount});
            MeshObject("Clinker Hull",SaveMesh("Boat Hull",points,triangles),wood,visuals);
            Box("Boat Floor",visuals,new(0,-.02f,0),new(1.5f,.1f,3.9f),wood);
            for(int i=-1;i<=1;i++)Box("Thwart",visuals,new(0,.38f,i*1.4f),new(1.85f,.1f,.34f),wood);
            for(int side=-1;side<=1;side+=2)for(int i=0;i<sections;i++)
            {
                Vector3 Edge(int index){float t=index/(float)sections;return new(side*(.12f+1.05f*Mathf.Pow(Mathf.Max(0,Mathf.Sin(t*Mathf.PI)),.6f)),.46f,Mathf.Lerp(-3,3,t));}
                Rod("Gunwale",visuals,Edge(i),Edge(i+1),.09f,iron);
            }
            Rod("Oar",visuals,new(-1.7f,.55f,-2),new(.6f,.58f,1.5f),.055f,wood);
            Box("Oar Blade",visuals,new(-1.78f,.55f,-2.12f),new(.22f,.06f,.65f),wood).transform.localRotation=Quaternion.Euler(0,33,0);
            var person=new GameObject("Boatman Figure").transform;person.SetParent(visuals,false);person.localPosition=new(0,.08f,-.6f);person.localRotation=Quaternion.Euler(0,35,0);
            FigurePart("Oilskin Coat",person,PrimitiveType.Capsule,new(0,.86f,0),new(.55f,.62f,.38f),coat);
            FigurePart("Head",person,PrimitiveType.Sphere,new(0,1.58f,0),new(.25f,.31f,.25f),skin);
            FigurePart("Cap",person,PrimitiveType.Cylinder,new(0,1.74f,0),new(.34f,.045f,.34f),coat);
            for(int side=-1;side<=1;side+=2){FigurePart("Trouser Leg",person,PrimitiveType.Capsule,new(side*.13f,.35f,0),new(.17f,.32f,.18f),coat);Rod("Sleeve",person,new(side*.24f,1.2f,0),new(side*.31f,.73f,.15f),.16f,coat);FigurePart("Hand",person,PrimitiveType.Sphere,new(side*.31f,.72f,.15f),new(.12f,.15f,.1f),skin);}
            var glow=ReferenceSurfaceLibrary.Solid("Boat Lantern Glass",new(.8f,.46f,.18f),3f);
            Box("Boat Lantern",visuals,new(.5f,.65f,.4f),new(.16f,.25f,.16f),glow);
            var lightObject=new GameObject("Boat Lantern Light",typeof(Light));lightObject.transform.SetParent(visuals,false);lightObject.transform.localPosition=new(.5f,.9f,.4f);
            var light=lightObject.GetComponent<Light>();light.type=LightType.Point;light.color=new(1,.67f,.35f);light.intensity=1.8f;light.range=4;light.shadows=LightShadows.None;
            var controller=Object.FindAnyObjectByType<Boatman>();if(controller){var so=new SerializedObject(controller);so.FindProperty("departureOffset").vector3Value=settings.departureOffset;so.ApplyModifiedPropertiesWithoutUndo();}
        }
        static void FigurePart(string name,Transform parent,PrimitiveType type,Vector3 p,Vector3 scale,Material material)
        {
            var go=GameObject.CreatePrimitive(type);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=p;go.transform.localScale=scale;go.GetComponent<Renderer>().sharedMaterial=material;Object.DestroyImmediate(go.GetComponent<Collider>());
        }
        static void Doors()
        {
            // Exterior approach and entrance steps for Keeper House Front Door
            Box("House Plinth West",root,new(-9f,.08f,-9.05f),new(6.4f,.18f,.28f),stone,true);
            Box("House Plinth East",root,new(-.8f,.08f,-9.05f),new(6.0f,.18f,.28f),stone,true);
            Box("Front Door Threshold Sill",root,new(-5f,.12f,-9.15f),new(2.3f,.18f,.45f),stone,true);
            Box("Front Door Intermediate Step",root,new(-5f,.06f,-9.6f),new(2.6f,.14f,.55f),stone,true);
            Box("Front Door Approach Step",root,new(-5f,.01f,-10.2f),new(2.9f,.12f,.7f),stone,true);
            for(int i=0;i<3;i++)Box("Front Walkway Stone",root,new(-5f+i*0.1f,-.01f,-11f-i*1.1f),new(1.8f,.1f,1.0f),stone,true);
            Box("Front Weather Hood",root,new(-5f,2.62f,-9.24f),new(2.35f,.12f,.38f),stone,true);

            DoorLeaf("Keeper House Front Door",new(-5.9f,.15f,-9.05f),0,1.8f,2.4f,"Keeper House Front Door");
            DoorLeaf("Tower Door",new(-.7f,.15f,-4.03f),0,1.4f,2.2f,"tower-front-door");
            DoorLeaf("Lighthouse Sea Door",new(4.02f,.15f,.8f),90,1.6f,2.2f,"tower-seaward-door");
            Box("Tower East Step",root,new(4.35f,.12f,0),new(1.3f,.24f,2),stone,true);
            for(int i=0;i<3;i++)Box("East Approach Stone",root,new(4.6f,.12f,-1.7f-i*1.5f),new(1.5f,.22f,1.5f),stone,true);
        }
        static void DoorLeaf(string name,Vector3 hinge,float yaw,float width,float height,string id)
        {
            var go=GameObject.Find(name);if(!go){go=new GameObject(name);go.AddComponent<NoiseEmitter>();go.AddComponent<Door>();}
            var old=go.transform.Find("Operable Leaf");if(old)Object.DestroyImmediate(old.gameObject);
            foreach(var renderer in go.GetComponentsInChildren<Renderer>(true))renderer.enabled=false;
            foreach(var collider in go.GetComponentsInChildren<Collider>(true))collider.enabled=false;
            go.transform.position=hinge;go.transform.rotation=Quaternion.Euler(0,yaw,0);go.transform.localScale=Vector3.one;
            var leaf=Box("Operable Leaf",go.transform,new(width*.5f,height*.5f,0),new(width,height,.14f),wood,true);
            Box("Iron Door Handle",leaf.transform,new(.32f,0,-.62f),new(.045f,.09f,.12f),iron);
            Box("Iron Door Handle",leaf.transform,new(.32f,0,.62f),new(.045f,.09f,.12f),iron);
            // Decorative iron strap hinges visually mounting the door to the jamb
            Box("Hinge Strap Top",leaf.transform,new(-width*.22f,height*.32f,0),new(width*.52f,.065f,.16f),iron);
            Box("Hinge Strap Bottom",leaf.transform,new(-width*.22f,-height*.32f,0),new(width*.52f,.065f,.16f),iron);
            var door=go.GetComponent<Door>();var so=new SerializedObject(door);so.FindProperty("doorId").stringValue=id;so.FindProperty("locked").boolValue=false;so.FindProperty("openAngle").floatValue=-105;
            if(name=="Tower Door")so.FindProperty("blockDuringActTwo").boolValue=false;
            so.ApplyModifiedPropertiesWithoutUndo();
            Quaternion rotation=Quaternion.Euler(0,yaw,0);
            for(int i=0;i<2;i++)Box("Masonry Door Reveal",root,hinge+rotation*new Vector3(i==0?-.4f:width+.4f,height*.5f,.15f),new(.64f,height+.12f,.6f),stone,true).transform.rotation=rotation;
            for(int i=0;i<2;i++)Box("Fixed Door Jamb",root,hinge+rotation*new Vector3(i==0?-.09f:width+.09f,height*.5f,0),new(.16f,height+.12f,.25f),wood,true).transform.rotation=rotation;
            Box("Fixed Door Header",root,hinge+rotation*new Vector3(width*.5f,height+.08f,0),new(width+.34f,.16f,.25f),wood,true).transform.rotation=rotation;
            // Continuous threshold sill plate bridging the floor directly under the door sweep
            Box("Door Threshold",root,hinge+rotation*new Vector3(width*.5f,-.03f,0),new(width+.18f,.07f,.34f),stone,true).transform.rotation=rotation;
        }
    }
}
