using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace TheLightHouse.Editor
{
    public static class ReferenceVisualPass
    {
        const string RootName = "Reference Visuals";
        const string MeshFolder = "Assets/_Project/Art/ReferenceMeshes";
        static ReferenceLookSettings look;
        static Material plaster, wood, iron, roof, dark, glass, warm, leaf, ground, grate;
        static Transform root, area;
        static int meshIndex;
        static string act;
        static string meshRunFolder;

        [MenuItem("TheLightHouse/Apply Reference Visuals")]
        public static void ApplyAll()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play Mode before applying visuals.");
            if (Enumerable.Range(0, SceneManager.sceneCount).Any(i => SceneManager.GetSceneAt(i).isDirty))
                throw new InvalidOperationException("Save open scene changes before applying visuals.");
            var setup = EditorSceneManager.GetSceneManagerSetup();
            Prepare();
            AssetDatabase.StartAssetEditing();
            try
            {
                for (int i = 1; i <= 5; i++)
                {
                    var scene = EditorSceneManager.OpenScene($"Assets/_Project/Scenes/Act{i}.unity", OpenSceneMode.Single);
                    ApplyScene(); EditorSceneManager.SaveScene(scene);
                }
                AssetDatabase.SaveAssets();
            }
            finally { AssetDatabase.StopAssetEditing(); if (setup.Any(s => s.isLoaded) && setup.Any(s => s.isActive)) EditorSceneManager.RestoreSceneManagerSetup(setup); }
            RemoveUnusedGeneratedMeshes();
        }

        public static int RemoveUnusedGeneratedMeshes()
        {
            var scenePaths=AssetDatabase.FindAssets("t:Scene",new[]{"Assets/_Project/Scenes"}).Select(AssetDatabase.GUIDToAssetPath).ToArray();
            var used=new HashSet<string>(AssetDatabase.GetDependencies(scenePaths,true));
            var unused=AssetDatabase.FindAssets("t:Mesh",new[]{MeshFolder}).Select(AssetDatabase.GUIDToAssetPath).Where(p=>!used.Contains(p)).ToArray();
            AssetDatabase.StartAssetEditing();
            try { foreach(string path in unused)AssetDatabase.DeleteAsset(path); }
            finally { AssetDatabase.StopAssetEditing(); }
            return unused.Length;
        }

        static void Prepare()
        {
            meshRunFolder = MeshFolder + "/Run-" + DateTime.UtcNow.Ticks;
            Directory.CreateDirectory(meshRunFolder); Directory.CreateDirectory("Assets/_Project/Settings"); AssetDatabase.Refresh();
            const string path = "Assets/_Project/Settings/ReferenceLookSettings.asset";
            look = AssetDatabase.LoadAssetAtPath<ReferenceLookSettings>(path);
            if (!look) { look = ScriptableObject.CreateInstance<ReferenceLookSettings>(); AssetDatabase.CreateAsset(look, path); }
            plaster = ReferenceSurfaceLibrary.Create("Salt Plaster", look.plaster, 0, look.surfaceWear);
            wood = ReferenceSurfaceLibrary.Create("Old Timber", look.wood, 1, look.surfaceWear);
            iron = ReferenceSurfaceLibrary.Create("Oxidised Iron", look.iron, 2, look.surfaceWear, .45f, .25f);
            grate = ReferenceSurfaceLibrary.Grating(iron);
            roof = ReferenceSurfaceLibrary.Create("Slate", new(.12f,.15f,.16f), 1, .7f, .1f, .32f);
            dark = ReferenceSurfaceLibrary.Solid("Recess", new(.018f,.027f,.026f));
            glass = ReferenceSurfaceLibrary.Solid("Cold Window", new(.42f,.58f,.58f), 1.3f);
            warm = ReferenceSurfaceLibrary.Solid("Lamp Enamel", new(.66f,.47f,.25f), .15f);
            ground = ReferenceSurfaceLibrary.Create("Wet Shale",new(.065f,.08f,.075f),0,.9f,0,.3f);
            leaf = ReferenceSurfaceLibrary.Create("Coastal Scrub", new(.16f,.21f,.16f), 0, .7f);
        }

        static void ApplyScene()
        {
            act = SceneManager.GetActiveScene().name; meshIndex = 0;
            var old = GameObject.Find(RootName); if (old) Object.DestroyImmediate(old);
            root = new GameObject(RootName).transform;
            var layout = GameObject.Find("Lighthouse Layout");
            if (!layout) throw new InvalidOperationException($"{act} has no shared layout.");
            foreach (var renderer in layout.GetComponentsInChildren<MeshRenderer>(true))
            {
                string n = renderer.name;
                if(n=="Courtyard"){renderer.transform.position=new(0,.05f,-13.2f);renderer.transform.localScale=new(20,.2f,8);}
                if(n=="Courtyard"||n=="Beach"||n=="Cliff Path")renderer.sharedMaterial=ground;
                if (n.StartsWith("House ") || n.StartsWith("Lantern Ring") || n == "Lantern Cap" || n == "Gallery Floor" || n.StartsWith("Spiral L") || n == "Tower Central Support")
                { renderer.enabled = false; foreach (var c in renderer.GetComponents<Collider>()) c.enabled = false; continue; }
                if (n == "Courtyard Wall West") { renderer.transform.position = new(-13.8f,1.1f,-13); renderer.transform.localScale = new(.45f,2.2f,8); }
                if (n == "Courtyard Wall East") { renderer.transform.position = new(10,1.1f,-13); renderer.transform.localScale = new(.45f,2.2f,8); }
                if (n.StartsWith("Tower L") && n.Contains(" wall "))
                {
                    renderer.enabled=false;
                    int level = int.Parse(n.Substring(7, n.IndexOf(" wall ", StringComparison.Ordinal)-7));
                    var scale = renderer.transform.localScale; scale.z = Mathf.Lerp(.8f,.45f,level/8f); renderer.transform.localScale = scale;
                    if(level==0){int segment=int.Parse(n.Substring(n.LastIndexOf(' ')+1));if(Mathf.Abs(Mathf.DeltaAngle(segment*15,90))<20)foreach(var c in renderer.GetComponents<Collider>())c.enabled=false;}
                }
                if (n.StartsWith("Tower Floor 1 segment")) {renderer.enabled=false;foreach(var c in renderer.GetComponents<Collider>())c.enabled=false;continue;}
                if(n.StartsWith("Tower Floor") || n.StartsWith("Tower Cornice")) renderer.enabled=false;
                if (n.StartsWith("Tower Cornice") || n.StartsWith("Gallery Rail"))
                { var scale=renderer.transform.localScale; scale.z=.15f; renderer.transform.localScale=scale; }
                if (n.Contains("Window")) { renderer.enabled = false; continue; }
                string materialName = renderer.sharedMaterial ? renderer.sharedMaterial.name : "";
                if (materialName == "Stone" || materialName == "Salt Plaster") renderer.sharedMaterial = plaster;
                if (materialName == "Wood" || materialName == "Old Timber") renderer.sharedMaterial = wood;
                if (materialName == "Metal" || materialName == "Oxidised Iron") renderer.sharedMaterial = iron;
                if (renderer.sharedMaterial == plaster || renderer.sharedMaterial == wood || renderer.sharedMaterial == iron || renderer.sharedMaterial == ground) MapUV(renderer.GetComponent<MeshFilter>());
            }
            area = Group("Tower Shell"); TowerShell(); CombineArea();
            area = Group("Keeper House"); House(); CombineArea();
            area = Group("Radio Office"); Office(); CombineArea();
            area = Group("Iron Spiral"); Stairs(); CombineArea();
            area = Group("Tower Store"); Store(); CombineArea();
            area = Group("Lantern Gallery"); Lantern(); CombineArea();
            area = Group("Coast Dressing"); Coast(); CombineArea();
            var radioAnchor=GameObject.Find("Radio"); if(radioAnchor && radioAnchor.GetComponent<Renderer>())radioAnchor.GetComponent<Renderer>().enabled=false;
            var wardrobe=GameObject.Find("keeper-wardrobe"); if(wardrobe)wardrobe.transform.position=new(-11.2f,1,-3.2f);
            foreach(string name in new[]{"Table","Hot Meal","Steam placeholder"}){var obj=GameObject.Find(name);if(obj){var p=obj.transform.position;p.x=-3;obj.transform.position=p;}}
            Lighting();
            ReferenceWeatherAuthoring.ApplyToOpenScene();
            Debug.Log($"Reference visuals applied to {act}; gameplay anchors retained.");
        }

        static Transform Group(string name) { var go = new GameObject(name); go.transform.SetParent(root); return go.transform; }
        static GameObject Box(string name, Vector3 p, Vector3 scale, Material m, bool collision = false)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube); go.name = name; go.transform.SetParent(area); go.transform.position = p; go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = m;
            if (!collision) Object.DestroyImmediate(go.GetComponent<Collider>());
            else go.tag = m == wood ? "Wood" : m == iron ? "Metal" : "Stone";
            MapUV(go.GetComponent<MeshFilter>(), false); return go;
        }
        static void Rod(string name, Vector3 a, Vector3 b, float width, Material m)
        {
            var go = Box(name, (a+b)*.5f, new(width, Vector3.Distance(a,b), width), m);
            go.transform.rotation = Quaternion.FromToRotation(Vector3.up, b-a);
        }
        static void MapUV(MeshFilter filter, bool persist = true)
        {
            if (!filter || !filter.sharedMesh) return;
            var mesh = Object.Instantiate(filter.sharedMesh); var vertices = mesh.vertices; var normals = mesh.normals; var uv = new Vector2[vertices.Length];
            for (int i=0;i<vertices.Length;i++)
            {
                Vector3 p = filter.transform.TransformPoint(vertices[i]); Vector3 n = filter.transform.TransformDirection(normals[i]);
                uv[i] = Mathf.Abs(n.y) > .5f ? new Vector2(p.x,p.z)*.5f : Mathf.Abs(n.x) > .5f ? new Vector2(p.z,p.y)*.5f : new Vector2(p.x,p.y)*.5f;
            }
            mesh.uv = uv; filter.sharedMesh = persist ? Persist(mesh) : mesh;
        }
        static Mesh Persist(Mesh mesh)
        {
            if(mesh.uv.Length==mesh.vertexCount && mesh.normals.Length==mesh.vertexCount)mesh.RecalculateTangents();
            string path = $"{meshRunFolder}/{act}-{meshIndex++:0000}.asset";
            var previous = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (previous) { EditorUtility.CopySerialized(mesh, previous); Object.DestroyImmediate(mesh); return previous; }
            AssetDatabase.CreateAsset(mesh, path); return mesh;
        }
        static void CombineArea()
        {
            var filters = area.GetComponentsInChildren<MeshFilter>().Where(f=>f.GetComponent<MeshRenderer>()).ToArray();
            foreach (var grouping in filters.GroupBy(f=>f.GetComponent<MeshRenderer>().sharedMaterial))
            {
                var mesh = new Mesh { indexFormat = IndexFormat.UInt32 };
                mesh.CombineMeshes(grouping.Select(f=>new CombineInstance {mesh=f.sharedMesh,transform=f.transform.localToWorldMatrix}).ToArray());
                var go = new GameObject(grouping.Key.name, typeof(MeshFilter), typeof(MeshRenderer)); go.transform.SetParent(area);
                go.GetComponent<MeshFilter>().sharedMesh = Persist(mesh); go.GetComponent<MeshRenderer>().sharedMaterial = grouping.Key;
                GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccludeeStatic);
                foreach(var f in grouping) { Object.DestroyImmediate(f.GetComponent<MeshRenderer>()); Object.DestroyImmediate(f); }
            }
        }

        static void TowerShell()
        {
            var vertices=new List<Vector3>();var normals=new List<Vector3>();var uv=new List<Vector2>();var triangles=new List<int>();
            const int segments=64;
            for(int level=0;level<8;level++)for(int row=0;row<3;row++)for(int side=0;side<2;side++)for(int i=0;i<segments;i++)
            {
                float[] heights={0,1,level==0?2.4f:2.2f,3.2f};float y0=level*3.2f+heights[row],y1=level*3.2f+heights[row+1];
                float a=i*Mathf.PI*2/segments,b=(i+1)*Mathf.PI*2/segments;
                float angle=(i+.5f)*360/segments;
                if(level==0 && row<2 && (Mathf.Abs(Mathf.DeltaAngle(angle,180))<16 || Mathf.Abs(Mathf.DeltaAngle(angle,90))<16))continue;
                if(level%2==0 && row==1 && Mathf.Abs(Mathf.DeltaAngle(angle,180))<6)continue;
                Vector3 P(float theta,float y){float r=Mathf.Lerp(4,2.5f,y/25.6f)-(side==1?.6f:0);return new(Mathf.Sin(theta)*r,y,Mathf.Cos(theta)*r);}
                int k=vertices.Count;vertices.AddRange(new[]{P(a,y0),P(a,y1),P(b,y1),P(b,y0)});
                Vector3 N(float theta)=>new Vector3(Mathf.Sin(theta),.0586f,Mathf.Cos(theta)).normalized*(side==0?1:-1);
                normals.AddRange(new[]{N(a),N(a),N(b),N(b)});uv.AddRange(new[]{new Vector2(a*2,y0*.5f),new Vector2(a*2,y1*.5f),new Vector2(b*2,y1*.5f),new Vector2(b*2,y0*.5f)});
                triangles.AddRange(side==0?new[]{k+2,k+1,k,k+3,k+2,k}:new[]{k,k+1,k+2,k,k+2,k+3});
            }
            var mesh=new Mesh{vertices=vertices.ToArray(),normals=normals.ToArray(),uv=uv.ToArray(),triangles=triangles.ToArray()};
            var go=new GameObject("Tapered Masonry",typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(area);go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<Renderer>().sharedMaterial=plaster;
        }

        static void House()
        {
            Box("Timber Floor",new(-5,.06f,-5.4f),new(14,.18f,7.2f),wood,true);
            Box("Timber Ceiling West",new(-8.15f,2.95f,-5),new(7.7f,.18f,8),wood,true);
            Box("Timber Ceiling South",new(-1.15f,2.95f,-6.65f),new(6.3f,.18f,4.7f),wood,true);
            Box("Front West",new(-9,1.5f,-9),new(6,3,.3f),plaster,true);
            Box("Front East",new(-.8f,1.5f,-9),new(5.6f,3,.3f),plaster,true);
            Box("Front Lintel",new(-5,2.75f,-9),new(2, .5f,.3f),plaster,true);
            Box("West Wall",new(-12,1.5f,-5),new(.3f,3,8),plaster,true);
            Box("East Wall",new(2,1.5f,-6.4f),new(.3f,3,5.2f),plaster,true);
            Box("Rear West",new(-7.7f,1.5f,-1),new(8.6f,3,.3f),plaster,true);
            Box("Rear Tower Connection",new(-2.7f,2.7f,-1),new(1.5f,.6f,.3f),plaster,true);
            // The tower door at z=-4 remains the route from the east side of the house.
            // Office divider at x=-6.3 aligns with the west door reveal, keeping the front entrance fully clear.
            Box("Office Divider",new(-6.3f,1.5f,-3.1f),new(.18f,3,4.2f),plaster,true);
            Box("Office Divider South",new(-6.3f,1.5f,-8.1f),new(.18f,3,1.8f),plaster,true);
            Box("Office Door Header",new(-6.3f,2.7f,-6.2f),new(.22f,.6f,2.0f),wood);
            Box("Office Door Threshold",new(-6.3f,.16f,-6.2f),new(.28f,.045f,1.85f),wood,true);
            Box("Office Door Jamb North",new(-6.3f,1.4f,-5.2f),new(.22f,2.6f,.14f),wood,true);
            Box("Office Door Jamb South",new(-6.3f,1.4f,-7.2f),new(.22f,2.6f,.14f),wood,true);
            for(int i=0;i<5;i++){float x=-11+i*2.6f;Box("Ceiling Beam",new(x,2.77f,x>-4.3f?-6.65f:-5),new(.2f,.28f,x>-4.3f?4.7f:8),wood);}
            for(int i=0;i<3;i++) Box("Wall Baseboard",new(-9, .34f+i*.19f,-8.8f),new(5.7f,.17f,.1f),wood);
            Box("Office Baseboard West",new(-8.75f,.25f,-1.15f),new(6.4f,.16f,.08f),wood);
            Box("Office Baseboard South",new(-11.85f,.25f,-5f),new(.08f,.16f,7.6f),wood);
            float pitch=27;
            Box("Slate Roof South",new(-5,4.05f,-7.1f),new(14.7f,.2f,4.9f),roof,true).transform.rotation=Quaternion.Euler(-pitch,0,0);
            Box("Slate Roof North",new(-8.15f,4.05f,-2.9f),new(8.4f,.2f,4.9f),roof,true).transform.rotation=Quaternion.Euler(pitch,0,0);
            Box("Roof Ridge",new(-5,5.16f,-5),new(14.8f,.18f,.22f),iron);
            for(int side=0;side<2;side++)
            {
                float x=side==0?-12:2;
                Triangle("Gable",new(x,3,-9),new(x,5.2f,-5),new(x,3,-1),plaster);
                Rod("Gable Trim",new(x,3,-9.4f),new(x,5.24f,-5),.16f,wood);
                Rod("Gable Trim",new(x,5.24f,-5),new(x,3,-.6f),.16f,wood);
            }
            Box("Chimney",new(-9,4.6f,-4),new(.65f,2.2f,.7f),plaster);
            Box("Chimney Crown",new(-9,5.7f,-4),new(.85f,.16f,.9f),iron);
            for(int i=0;i<3;i++) Window(new(-10+i*3.6f,1.65f,-9.18f),1.05f,1.45f,false);
            Window(new(-8,1.8f,-1.18f),1.9f,1.7f,true);
            for(int i=0;i<5;i++)
            {
                float x=-11.9f+i*3.4f;
                if(x>-6f && x<-4f) continue;
                Box("Facade Timber",new(x,1.5f,-9.19f),new(.12f,3,.12f),wood);
            }
            for(int i=0;i<2;i++) Rod("Rain Gutter",new(-12.3f,3,-9.35f+i*8.7f),new(2.3f,3,-9.35f+i*8.7f),.13f,iron);
        }
        static void Window(Vector3 p,float w,float h,bool blinds)
        {
            Box("Window Recess",p,new(w+.18f,h+.18f,.10f),dark);
            Box("Window Glass",p+Vector3.back*.06f,new(w,h,.04f),glass);
            for(int i=-1;i<=1;i+=2){Box("Window Jamb",p+new Vector3(i*w*.5f,0,-.1f),new(.09f,h+.2f,.13f),wood);Box("Window Sill",p+new Vector3(0,i*h*.5f,-.1f),new(w+.2f,.1f,.18f),wood);}
            Box("Window Mullion",p+Vector3.back*.1f,new(.045f,h,.08f),iron);
            Box("Window Crossbar",p+Vector3.back*.1f,new(w,.045f,.08f),iron);
            if(blinds)for(int i=0;i<12;i++)Box("Venetian Slat",p+new Vector3(0,-h*.5f+.08f+i*h/12,-.18f),new(w,.075f,.09f),wood).transform.rotation=Quaternion.Euler(-15,0,0);
        }
        static void Triangle(string name,Vector3 a,Vector3 b,Vector3 c,Material m)
        {
            var mesh=new Mesh{vertices=new[]{a,b,c,a,b,c},triangles=new[]{0,1,2,5,4,3},uv=new[]{Vector2.zero,Vector2.up,Vector2.one,Vector2.zero,Vector2.up,Vector2.one}};mesh.RecalculateNormals();
            var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(area);go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<MeshRenderer>().sharedMaterial=m;
        }

        static void Office()
        {
            Vector3 desk=new(-8,0,-2.0f);
            Box("Office Floor Cloth",desk+new Vector3(.2f,.152f,-.6f),new(3.0f,.008f,2.4f),dark);
            Table(desk,2.7f,1.05f);
            var radio=GameObject.Find("Radio"); if(radio) radio.transform.position=desk+new Vector3(.5f,1.13f,0);
            Box("Analogue Radio Case",desk+new Vector3(.52f,1.14f,0),new(.9f,.36f,.4f),iron);
            Box("Radio Face",desk+new Vector3(.52f,1.14f,-.22f),new(.83f,.29f,.025f),dark);
            Box("Tuning Scale",desk+new Vector3(.42f,1.2f,-.24f),new(.46f,.075f,.016f),warm);
            for(int i=0;i<9;i++)Box("Radio Dial Tick",desk+new Vector3(.22f+i*.048f,1.2f,-.251f),new(.009f,.05f,.01f),glass);
            for(int i=0;i<3;i++)Cylinder("Radio Knob",desk+new Vector3(.22f+i*.25f,1.06f,-.25f),.045f,.04f,iron,new(90,0,0));
            Box("Recorder",desk+new Vector3(-.48f,1.04f,.05f),new(.75f,.16f,.34f),dark);
            for(int i=0;i<2;i++)Cylinder("Tape Reel",desk+new Vector3(-.68f+i*.36f,1.14f,.05f),.14f,.025f,iron,Vector3.zero);
            Cylinder("Lamp Base",desk+new Vector3(-1,1.02f,.12f),.18f,.05f,iron,Vector3.zero);
            Rod("Lamp Arm",desk+new Vector3(-1,1.04f,.12f),desk+new Vector3(-1.02f,1.62f,.18f),.035f,iron);
            Rod("Lamp Arm",desk+new Vector3(-1.02f,1.62f,.18f),desk+new Vector3(-.65f,1.83f,.1f),.04f,iron);
            Cylinder("Task Shade",desk+new Vector3(-.62f,1.76f,.09f),.2f,.16f,warm,new(0,0,-20));
            Box("Logbook",desk+new Vector3(-.3f,.96f,-.3f),new(.4f,.035f,.26f),warm).transform.rotation=Quaternion.Euler(0,12,0);
            Vector3 chair=desk+new Vector3(.4f,0,-1.2f);
            Box("Chair Seat",chair+Vector3.up*.54f,new(.58f,.09f,.58f),wood);
            for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)Box("Chair Leg",chair+new Vector3(x*.23f,.27f,z*.23f),new(.07f,.54f,.07f),wood);
            for(int x=-1;x<=1;x+=2)Box("Chair Back Upright",chair+new Vector3(x*.23f,.94f,-.24f),new(.07f,.9f,.07f),wood);
            Box("Chair Back",chair+new Vector3(0,1.22f,-.24f),new(.53f,.2f,.07f),wood);
            Box("Framed Chart",new(-6.35f,1.95f,-1.22f),new(.5f,.7f,.08f),iron);
            Box("Chart Paper",new(-6.35f,1.95f,-1.27f),new(.43f,.62f,.015f),warm);
            Rod("Chart Lighthouse",new(-6.35f,1.7f,-1.29f),new(-6.35f,2.18f,-1.29f),.04f,iron);
        }
        static void Table(Vector3 p,float width,float depth)
        {
            Box("Table Top",p+Vector3.up*.9f,new(width,.1f,depth),wood);
            for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)Box("Table Leg",p+new Vector3(x*(width*.5f-.1f),.45f,z*(depth*.5f-.1f)),new(.1f,.9f,.1f),wood);
            Box("Table Apron",p+new Vector3(0,.76f,-depth*.5f+.05f),new(width,.19f,.08f),wood);
        }
        static void Cylinder(string name,Vector3 p,float radius,float height,Material m,Vector3 rotation)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cylinder);go.name=name;go.transform.SetParent(area);go.transform.position=p;go.transform.localScale=new(radius*2,height*.5f,radius*2);go.transform.eulerAngles=rotation;go.GetComponent<Renderer>().sharedMaterial=m;Object.DestroyImmediate(go.GetComponent<Collider>());
        }
        static void Stairs()
        {
            var settings=AssetDatabase.LoadAssetAtPath<LighthouseLayoutSettings>("Assets/_Project/Settings/LighthouseLayoutSettings.asset");
            int count=Mathf.CeilToInt(settings.floorHeight/settings.stairStepHeight);
            float outerLevel0=settings.towerBaseDiameter*.5f-settings.towerBaseWallThickness-.03f;
            float innerLevel0=outerLevel0-settings.stairWidth;
            // Solid stone starter curb and iron newel post anchoring foot of spiral stairs
            Box("Spiral Stair Starter Curb",new(0,.08f,outerLevel0-.55f),new(settings.stairWidth+.12f,.16f,.44f),ground,true);
            Rod("Stair Starter Newel",new(0,.14f,innerLevel0),new(0,1.25f,innerLevel0),.075f,iron);
            Cylinder("Stair Starter Newel Base",new(0,.15f,innerLevel0),.12f,.03f,iron,Vector3.zero);
            for(int s=0;s<4;s++)
            {
                float sa=s*Mathf.PI*2/count;
                float sy=(s+1)*settings.floorHeight/count;
                Rod("Stair Wall Cantilever Bracket",
                    new(Mathf.Sin(sa)*innerLevel0,sy,Mathf.Cos(sa)*innerLevel0),
                    new(Mathf.Sin(sa)*(outerLevel0+.18f),sy-.06f,Mathf.Cos(sa)*(outerLevel0+.18f)),
                    .045f,iron);
            }
            for(int level=0;level<settings.towerLevelCount;level++)
            {
                float t=level/(float)settings.towerLevelCount;
                float outer=Mathf.Lerp(settings.towerBaseDiameter,settings.towerGalleryDiameter,t)*.5f-Mathf.Lerp(settings.towerBaseWallThickness,settings.towerGalleryWallThickness,t)-.03f;
                float inner=outer-settings.stairWidth;
                for(int step=0;step<count;step++)
                {
                    float a=step*Mathf.PI*2/count, b=(step+1)*Mathf.PI*2/count;
                    float y=level*settings.floorHeight+(step+1)*settings.floorHeight/count;
                    Wedge("Iron Tread",inner,outer,a,b,y,.075f,grate,true);
                    Vector3 railA=new(Mathf.Sin(a)*inner,y+1.03f,Mathf.Cos(a)*inner);
                    Vector3 railB=new(Mathf.Sin(b)*inner,y+1.03f+settings.floorHeight/count,Mathf.Cos(b)*inner);
                    Rod("Baluster",railA-Vector3.up*1.03f,railA,.035f,iron);Rod("Following Handrail",railA,railB,.048f,iron);
                    // Narrow transverse ridges catch the cold rim light on each tread.
                    Rod("Tread Lip",new(Mathf.Sin(a)*inner,y+.015f,Mathf.Cos(a)*inner),new(Mathf.Sin(a)*outer,y+.015f,Mathf.Cos(a)*outer),.025f,iron);
                }
            }
        }
        static void Wedge(string name,float inner,float outer,float a,float b,float y,float thick,Material material,bool collision)
        {
            var points=new List<Vector3>();var tris=new List<int>();
            Vector3 P(float r,float angle,float h)=>new(Mathf.Sin(angle)*r,h,Mathf.Cos(angle)*r);
            Vector3[] top={P(inner,a,y),P(outer,a,y),P(outer,b,y),P(inner,b,y)};
            void Face(Vector3 p0,Vector3 p1,Vector3 p2,Vector3 p3){int k=points.Count;points.AddRange(new[]{p0,p1,p2,p3});tris.AddRange(new[]{k,k+1,k+2,k,k+2,k+3});}
            Face(top[0],top[1],top[2],top[3]);Face(top[3]-Vector3.up*thick,top[2]-Vector3.up*thick,top[1]-Vector3.up*thick,top[0]-Vector3.up*thick);
            for(int i=0;i<4;i++){int j=(i+1)%4;Face(top[j],top[i],top[i]-Vector3.up*thick,top[j]-Vector3.up*thick);}
            var mesh=new Mesh{vertices=points.ToArray(),triangles=tris.ToArray()};mesh.RecalculateNormals();mesh.uv=points.Select(p=>new Vector2(p.x,p.z)).ToArray();
            var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(area);go.GetComponent<MeshFilter>().sharedMesh=Persist(mesh);go.GetComponent<Renderer>().sharedMaterial=material;
            if(collision){go.AddComponent<MeshCollider>().sharedMesh=go.GetComponent<MeshFilter>().sharedMesh;go.tag="Metal";}
        }
        static void Store()
        {
            // Continuous circular stone foundation plinth extending to the masonry wall
            Cylinder("Tower Stone Foundation",new(0,.07f,0),3.52f,.15f,ground,Vector3.zero);
            Cylinder("Tower Flagstone Floor",new(0,.145f,0),3.38f,.06f,ground,Vector3.zero);
            // Raised timber duckboard platform in the central storage bay for provisions
            Box("Storage Duckboard Platform",new(-.3f,.175f,.6f),new(2.7f,.04f,2.3f),wood,true);
            Barrel(new(-.8f,.2f,1.1f),.48f,1.05f,true);Barrel(new(.25f,.2f,1.2f),.46f,1.05f,true);Barrel(new(-.3f,1.2f,1.15f),.42f,.9f,true);
            Barrel(new(1.15f,.1f,.45f),.44f,1.15f);
            Table(new(-.9f,.08f,-.1f),1.1f,.65f);
            Box("Folded Keeper Cloth",new(-.9f,1.04f,-.16f),new(.6f,.04f,.5f),dark);
            Box("Hanging Keeper Cloth",new(-.9f,.76f,-.43f),new(.6f,.55f,.035f),dark);
            Box("Supply Crate",new(.3f,.4f,.1f),new(.64f,.65f,.65f),wood);
            for(int i=0;i<3;i++)Box("Crate Batten",new(.3f,.16f+i*.25f,-.24f),new(.7f,.06f,.05f),iron);
        }
        static void Barrel(Vector3 origin,float radius,float height,bool horizontal=false)
        {
            int first=area.childCount;
            const int segments=20;
            for(int i=0;i<segments;i++)
            {
                float angle=i*Mathf.PI*2/segments;
                for(int j=0;j<4;j++)
                {
                    float t=(j+.5f)/4;float r=radius*(.85f+.15f*Mathf.Sin(t*Mathf.PI));
                    var stave=Box("Barrel Stave",origin+new Vector3(Mathf.Sin(angle)*r,height*t,Mathf.Cos(angle)*r),new(radius*.31f,height*.25f-.008f,.055f),wood);
                    stave.transform.rotation=Quaternion.Euler(0,angle*Mathf.Rad2Deg,0);
                }
                for(int j=0;j<4;j++)
                {
                    float t=.1f+j*.26f;float r=radius*(.86f+.15f*Mathf.Sin(t*Mathf.PI))+.035f;
                    var hoop=Box("Riveted Hoop",origin+new Vector3(Mathf.Sin(angle)*r,height*t,Mathf.Cos(angle)*r),new(radius*.32f,.055f,.025f),iron);hoop.transform.rotation=Quaternion.Euler(0,angle*Mathf.Rad2Deg,0);
                }
            }
            Cylinder("Barrel Lid",origin+Vector3.up*height,radius*.85f,.035f,wood,Vector3.zero);
            Cylinder("Barrel Front",origin+Vector3.up*.02f,radius*.85f,.035f,wood,Vector3.zero);
            if(horizontal){var rotation=Quaternion.Euler(90,0,0);for(int i=first;i<area.childCount;i++){var t=area.GetChild(i);t.position=origin+rotation*(t.position-origin)+new Vector3(0,radius,-height*.5f);t.rotation=rotation*t.rotation;}}
        }
        static void Lantern()
        {
            var settings=AssetDatabase.LoadAssetAtPath<LighthouseLayoutSettings>("Assets/_Project/Settings/LighthouseLayoutSettings.asset");
            float y=settings.floorHeight*settings.towerLevelCount;
            for(int i=0;i<32;i++)
            {
                float a=i*Mathf.PI*2/32,b=(i+1)*Mathf.PI*2/32;
                Wedge("Circular Gallery",1.05f,3.9f,a,b,y,.18f,iron,true);
                Vector3 p=new(Mathf.Sin(a)*3.82f,y,Mathf.Cos(a)*3.82f),q=new(Mathf.Sin(b)*3.82f,y,Mathf.Cos(b)*3.82f);
                Rod("Gallery Baluster",p,p+Vector3.up*1.1f,.05f,iron);Rod("Gallery Top Rail",p+Vector3.up*1.1f,q+Vector3.up*1.1f,.06f,iron);Rod("Gallery Lower Rail",p+Vector3.up*.45f,q+Vector3.up*.45f,.035f,iron);
                if(i%4==0)Rod("Gallery Bracket",p-Vector3.up*.1f,p*.58f+Vector3.up*(y*.42f-1.5f),.12f,iron);
            }
            for(int i=0;i<8;i++)
            {
                float a=i*Mathf.PI/4,b=(i+1)*Mathf.PI/4;
                Vector3 p=new(Mathf.Sin(a)*2.2f,y,Mathf.Cos(a)*2.2f),q=new(Mathf.Sin(b)*2.2f,y,Mathf.Cos(b)*2.2f);
                Rod("Lantern Iron Post",p,p+Vector3.up*3.6f,.09f,iron);
                Rod("Lantern Lintel",p+Vector3.up*3.6f,q+Vector3.up*3.6f,.12f,iron);
                Rod("Lantern Sill",p+Vector3.up*.2f,q+Vector3.up*.2f,.12f,iron);
                Triangle("Lantern Pitched Cap",p+Vector3.up*3.6f,new(0,y+4.8f,0),q+Vector3.up*3.6f,roof);
            }
            var lens=GameObject.CreatePrimitive(PrimitiveType.Sphere);lens.name="Beacon Core";lens.transform.SetParent(area);lens.transform.position=new(0,y+2.4f,0);lens.transform.localScale=new(1.2f,1.7f,1.2f);Object.DestroyImmediate(lens.GetComponent<Collider>());lens.GetComponent<Renderer>().sharedMaterial=ReferenceSurfaceLibrary.Solid("Beacon Glass",new(.7f,.86f,1f),5);
            Cylinder("Lamp Pedestal",new(0,y+.5f,0),.4f,1,iron,Vector3.zero);
            for(int i=0;i<8;i++)Cylinder("Fresnel Lens Ring",new(0,y+1.7f+i*.15f,0),.48f,.065f,glass,Vector3.zero);
            for(int level=0;level<8;level+=2)
            {
                float radius=Mathf.Lerp(4,2.5f,level/8f);
                Window(new(0,level*3.2f+1.6f,-radius-.07f),.75f,1.1f,false);
            }
        }
        static void Coast()
        {
            var random=new System.Random(173);
            for(int i=0;i<look.scrubCount;i++)
            {
                float x=-16+(float)random.NextDouble()*32,z=-10-(float)random.NextDouble()*13;
                if(Mathf.Abs(x)<3.5f || (x>6 && z<-11)) continue;
                float h=.25f+(float)random.NextDouble()*.6f;
                for(int j=0;j<4;j++)
                {
                    float a=j*Mathf.PI*.5f;Vector3 p=new(x,.2f,z);
                    Triangle("Coastal Grass",p+new Vector3(-.2f,0,0),p+new Vector3(Mathf.Sin(a)*.35f,h,Mathf.Cos(a)*.35f),p+new Vector3(.2f,0,.15f),leaf);
                }
            }
            for(int i=0;i<30;i++)
            {
                float x=-18+(float)random.NextDouble()*36,z=-12-(float)random.NextDouble()*21;if(Mathf.Abs(x)<4)continue;
                var rock=GameObject.CreatePrimitive(PrimitiveType.Sphere);rock.name="Shore Rock";rock.transform.SetParent(area);rock.transform.position=new(x,-.1f,z);rock.transform.localScale=new(1+(float)random.NextDouble()*2,.5f+(float)random.NextDouble(),1.5f);rock.GetComponent<Renderer>().sharedMaterial=plaster;Object.DestroyImmediate(rock.GetComponent<Collider>());
            }
        }
        static void Lighting()
        {
            const string skyPath="Assets/_Project/Materials/Reference/Harrow Sky.mat";
            var sky=AssetDatabase.LoadAssetAtPath<Material>(skyPath);if(!sky){sky=new Material(Shader.Find("TheLightHouse/Harrow Storm Sky"));AssetDatabase.CreateAsset(sky,skyPath);}RenderSettings.skybox=sky;RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new(.16f,.21f,.23f);
            RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogColor=new(.055f,.085f,.095f);RenderSettings.fogDensity=look.fogDensity;
            var lights=Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            var practicals=lights.Where(l=>l.name=="Warm Lantern").ToArray();
            for(int i=0;i<practicals.Length;i++)
            {
                var light=practicals[i];light.transform.position=i==0?new(-8.65f,1.65f,-2.4f):new(-1.4f,2.1f,1.2f);light.color=i==0?look.practical:look.moon;light.intensity=look.practicalIntensity;light.range=i==0?5:6;light.shadows=LightShadows.None;
            }
            foreach(var light in lights.Where(l=>l.type==LightType.Directional)){light.intensity=look.moonIntensity;light.color=look.moon;light.transform.rotation=Quaternion.Euler(38,-38,0);light.shadows=LightShadows.Soft;light.shadowStrength=.85f;}
            var beamOrigin=GameObject.Find("Lamp Beam Visual");if(beamOrigin)beamOrigin.transform.position=new(0,28,0);
            var beam=GameObject.Find("Soft Beam Cone");
            if(beam) { beam.transform.localScale=Vector3.one*22; beam.GetComponent<Renderer>().sharedMaterial.SetColor("_BeamColor",new Color(.63f,.79f,.86f,.2f)); }
            var pivot=GameObject.Find("Beam Pivot"); if(pivot) pivot.transform.localRotation=Quaternion.Euler(0,65,0);
            var spot=lights.FirstOrDefault(l=>l.name=="Beam Spot"); if(spot){spot.color=new(.7f,.85f,1);spot.intensity=12;spot.range=22;}
            var glow=GameObject.Find("Lamp Glow"); if(glow)glow.GetComponent<SpriteRenderer>().color=new(.8f,.91f,1,1);
            foreach(var camera in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None)){camera.GetUniversalAdditionalCameraData().renderPostProcessing=true;camera.GetUniversalAdditionalCameraData().antialiasing=AntialiasingMode.FastApproximateAntialiasing;camera.clearFlags=CameraClearFlags.Skybox;camera.backgroundColor=new(.025f,.037f,.045f);}
            foreach(string guid in AssetDatabase.FindAssets("t:VolumeProfile",new[]{"Assets/_Project/Settings"}))
            {
                var p=AssetDatabase.LoadAssetAtPath<VolumeProfile>(AssetDatabase.GUIDToAssetPath(guid));
                if(p.TryGet<ColorAdjustments>(out var c)){c.saturation.Override(-28);c.contrast.Override(12);c.postExposure.Override(.15f);}
                if(p.TryGet<Vignette>(out var v))v.intensity.Override(look.vignette);
                if(p.TryGet<FilmGrain>(out var g))g.intensity.Override(look.grain);
                EditorUtility.SetDirty(p);
            }
            foreach(string guid in AssetDatabase.FindAssets("t:AtmosphereSettings"))
            {
                var s=AssetDatabase.LoadAssetAtPath<AtmosphereSettings>(AssetDatabase.GUIDToAssetPath(guid));
                s.night=NightPreset(s.night);s.storm=NightPreset(s.storm);s.finale=NightPreset(s.finale);
                s.dusk.ambientColor=new(.14f,.19f,.20f);s.dusk.fogColor=new(.09f,.13f,.15f);s.dusk.skyboxExposure=.1f;
                EditorUtility.SetDirty(s);
            }
        }
        static AtmospherePreset NightPreset(AtmospherePreset p)
        {
            p.ambientColor=new(.09f,.14f,.16f);p.directionalLightColor=look.moon;p.directionalLightIntensity=look.moonIntensity;p.fogColor=new(.055f,.085f,.095f);p.fogDensity=look.fogDensity;p.skyboxExposure=.05f;return p;
        }
    }
}
