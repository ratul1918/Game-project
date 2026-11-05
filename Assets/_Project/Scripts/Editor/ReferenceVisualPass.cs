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
        static Material brass, parchment, yellowSlicker, darkCoat, rugFabric, braidedRug, bookRed, bookGreen, bookBlue, bookGold, clockDial, leatherLuggage;
        static Material warmWindow, barrelWood, barrelIron, stoveFire, ceramic, sunflowerYellow, stripedRug, curtainFabric;
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
            brass = ReferenceSurfaceLibrary.Create("Maritime Brass", new(.84f,.68f,.28f), 0, .2f, .82f, .65f);
            parchment = ReferenceSurfaceLibrary.Create("Aged Parchment", new(.91f,.85f,.72f), 0, .35f, 0, .15f);
            yellowSlicker = ReferenceSurfaceLibrary.Create("Yellow Oilskin", new(.92f,.72f,.08f), 0, .15f, .05f, .65f);
            darkCoat = ReferenceSurfaceLibrary.Solid("Dark Wool Navy", new(.08f,.11f,.16f));
            rugFabric = ReferenceSurfaceLibrary.Create("Persian Carpet", new(.52f,.12f,.14f), 1, .4f, 0, .12f);
            braidedRug = ReferenceSurfaceLibrary.Create("Braided Nautical Rug", new(.32f,.28f,.22f), 1, .35f, 0, .15f);
            bookRed = ReferenceSurfaceLibrary.Solid("Book Crimson", new(.58f,.12f,.14f));
            bookGreen = ReferenceSurfaceLibrary.Solid("Book Emerald", new(.12f,.36f,.20f));
            bookBlue = ReferenceSurfaceLibrary.Solid("Book Navy", new(.12f,.20f,.45f));
            bookGold = ReferenceSurfaceLibrary.Solid("Book Gold Spine", new(.85f,.72f,.28f));
            clockDial = ReferenceSurfaceLibrary.Create("Station Clock Dial", new(.94f,.92f,.86f), 0, .15f, 0, .2f);
            leatherLuggage = ReferenceSurfaceLibrary.Create("Weathered Leather", new(.38f,.22f,.12f), 2, .5f, .1f, .3f);
            warmWindow = ReferenceSurfaceLibrary.Solid("Warm Cottage Glass", new Color(1f, .78f, .38f), 3.5f);
            barrelWood = ReferenceSurfaceLibrary.Create("Oak Barrel", new Color(.30f, .22f, .16f), 1, .6f, 0, .22f);
            barrelIron = ReferenceSurfaceLibrary.Solid("Barrel Iron Hoop", new Color(.15f, .16f, .17f), 0);
            stoveFire = ReferenceSurfaceLibrary.Solid("Stove Embers", new Color(1f, .45f, .10f), 4.5f);
            ceramic = ReferenceSurfaceLibrary.Solid("Cottage Ceramic", new Color(.92f, .90f, .86f));
            sunflowerYellow = ReferenceSurfaceLibrary.Solid("Sunflower Petal", new Color(1f, .82f, .05f));
            stripedRug = ReferenceSurfaceLibrary.Create("Cottage Kitchen Rug", new Color(.48f, .22f, .15f), 1, .4f, 0, .15f);
            curtainFabric = ReferenceSurfaceLibrary.Solid("Cottage Curtains", new Color(.85f, .82f, .78f));
        }

        public static void ApplyOpenScene()
        {
            Prepare();
            AssetDatabase.StartAssetEditing();
            try { ApplyScene(); }
            finally { AssetDatabase.StopAssetEditing(); }
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
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
                if(n.StartsWith("Boat Shed"))renderer.sharedMaterial=n.Contains("Roof")?roof:wood;
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
                if (n.StartsWith("Tower Floor")) { renderer.enabled = false; foreach (var c in renderer.GetComponents<Collider>()) c.enabled = false; continue; }
                if (n.StartsWith("Tower Cornice")) renderer.enabled = false;
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
            area = Group("Keeper House Kitchen"); Kitchen(); CombineArea();
            area = Group("Iron Spiral"); Stairs(); CombineArea();
            area = Group("Tower Vestibule"); Vestibule(); CombineArea();
            area = Group("Tower Library"); Library(); CombineArea();
            area = Group("Tower Watch Room"); WatchRoom(); CombineArea();
            area = Group("Tower Landing 3"); Landing3(); CombineArea();
            area = Group("Lantern Gallery"); Lantern(); CombineArea();
            area = Group("Coast Dressing"); Coast(); CombineArea();
            var radioAnchor=GameObject.Find("Radio"); if(radioAnchor && radioAnchor.GetComponent<Renderer>())radioAnchor.GetComponent<Renderer>().enabled=false;
            var wardrobe=GameObject.Find("keeper-wardrobe"); if(wardrobe)wardrobe.transform.position=new(-11.2f,1,-3.2f);
            foreach(string name in new[]{"Table","Hot Meal","Steam placeholder"})
            {
                var obj=GameObject.Find(name);
                if(obj)
                {
                    var p=new Vector3(-2.8f, name=="Table"?0.42f:name=="Hot Meal"?0.88f:1.05f, -5.8f);
                    obj.transform.position=p;
                    if(name=="Table"){var mr=obj.GetComponent<MeshRenderer>();if(mr)mr.enabled=false;}
                }
            }
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
            // Round Attic Porthole Window on East Gable (matching Image 7)
            Cylinder("Attic Porthole Frame", new(2.02f, 4.15f, -5.0f), .44f, .10f, iron, new(0, 0, 90));
            Cylinder("Attic Porthole Glass", new(2.03f, 4.15f, -5.0f), .38f, .04f, warmWindow, new(0, 0, 90));
            Rod("Attic Porthole Mullion V", new(2.04f, 3.75f, -5.0f), new(2.04f, 4.55f, -5.0f), .04f, iron);
            Rod("Attic Porthole Mullion H", new(2.04f, 4.15f, -5.40f), new(2.04f, 4.15f, -4.60f), .04f, iron);

            Box("Chimney",new(-9,4.6f,-4),new(.65f,2.2f,.7f),plaster);
            Box("Chimney Crown",new(-9,5.7f,-4),new(.85f,.16f,.9f),iron);
            // Front windows: Office South and Kitchen South, strictly leaving the door opening [-6.0, -4.0] clear
            Window(new(-9.5f,1.65f,-9.18f),1.15f,1.45f,false,true);
            Window(new(-1.8f,1.65f,-9.18f),1.15f,1.45f,false,true);
            Window(new(-8,1.8f,-1.18f),1.9f,1.7f,true,false);
            // Front porch carriage lantern mounted above front steps
            Box("Porch Lantern Bracket", new(-5f, 2.78f, -9.4f), new(.06f, .16f, .22f), iron);
            Box("Porch Lantern Hood", new(-5f, 2.82f, -9.6f), new(.24f, .06f, .24f), iron);
            Box("Porch Lantern Chimney", new(-5f, 2.68f, -9.6f), new(.18f, .24f, .18f), warmWindow);
            Box("Porch Lantern Base", new(-5f, 2.54f, -9.6f), new(.20f, .05f, .20f), iron);
            for(int i=0;i<5;i++)
            {
                float x=-11.9f+i*3.4f;
                if(x>-6f && x<-4f) continue;
                Box("Facade Timber",new(x,1.5f,-9.19f),new(.12f,3,.12f),wood);
            }
            for(int i=0;i<2;i++) Rod("Rain Gutter",new(-12.3f,3,-9.35f+i*8.7f),new(2.3f,3,-9.35f+i*8.7f),.13f,iron);
        }
        static void Window(Vector3 p,float w,float h,bool blinds,bool warmGlow=false)
        {
            Box("Window Recess",p,new(w+.18f,h+.18f,.10f),dark);
            Box("Window Glass",p+Vector3.back*.06f,new(w,h,.04f),warmGlow?warmWindow:glass);
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
            // Vintage typewriter beside radio
            Box("Typewriter Base",desk+new Vector3(-.02f,1.01f,-.25f),new(.46f,.09f,.42f),dark);
            Box("Typewriter Carriage",desk+new Vector3(-.02f,1.07f,-.38f),new(.52f,.08f,.14f),iron);
            Box("Typewriter Paper Sheet",desk+new Vector3(-.02f,1.14f,-.36f),new(.34f,.18f,.01f),parchment);
            Box("Logbook",desk+new Vector3(-.3f,.96f,-.3f),new(.4f,.035f,.26f),warm).transform.rotation=Quaternion.Euler(0,12,0);
            Vector3 chair=desk+new Vector3(.4f,0,-1.2f);
            Box("Chair Seat",chair+Vector3.up*.54f,new(.58f,.09f,.58f),wood);
            for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)Box("Chair Leg",chair+new Vector3(x*.23f,.27f,z*.23f),new(.07f,.54f,.07f),wood);
            for(int x=-1;x<=1;x+=2)Box("Chair Back Upright",chair+new Vector3(x*.23f,.94f,-.24f),new(.07f,.9f,.07f),wood);
            Box("Chair Back",chair+new Vector3(0,1.22f,-.24f),new(.53f,.2f,.07f),wood);
            Box("Framed Chart",new(-6.35f,1.95f,-1.22f),new(.5f,.7f,.08f),iron);
            Box("Chart Paper",new(-6.35f,1.95f,-1.27f),new(.43f,.62f,.015f),warm);
            Rod("Chart Lighthouse",new(-6.35f,1.7f,-1.29f),new(-6.35f,2.18f,-1.29f),.04f,iron);
            // Framed black-and-white lighthouse picture on wall above desk
            Box("Keeper Lighthouse Photo Frame",new(-7.2f,2.1f,-1.22f),new(.48f,.62f,.05f),dark);
            Box("Keeper Lighthouse Photo Canvas",new(-7.2f,2.1f,-1.24f),new(.40f,.54f,.015f),parchment);
            Rod("Photo Tower Silhouette",new(-7.2f,1.9f,-1.25f),new(-7.2f,2.3f,-1.25f),.045f,darkCoat);
        }

        static void Kitchen()
        {
            // Stone Backsplash along North wall behind kitchen counter
            Box("Kitchen Backsplash", new(-3.9f, 1.5f, -1.16f), new(4.4f, 2.7f, .04f), ground);

            // Base Kitchen Cabinetry & Countertop (depth 0.70m, height 0.90m)
            Vector3 counterPos = new(-4.2f, .45f, -1.50f);
            Box("Kitchen Base Cabinets", counterPos, new(3.2f, .86f, .70f), wood, true);
            Box("Kitchen Countertop", counterPos + Vector3.up * .45f, new(3.3f, .06f, .76f), dark, true);

            // Cabinet doors, drawers and brass pulls across 4 bays
            for (int b = 0; b < 4; b++)
            {
                float bx = -5.35f + b * .77f;
                Box($"Base Door Panel {b}", new(bx, .38f, -1.14f), new(.68f, .58f, .02f), wood);
                Box($"Base Door Recess {b}", new(bx, .38f, -1.135f), new(.56f, .46f, .01f), dark);
                Cylinder($"Base Door Pull {b}", new(bx + .24f, .55f, -1.12f), .025f, .03f, brass, new(90, 0, 0));
                Box($"Base Drawer Panel {b}", new(bx, .76f, -1.14f), new(.68f, .15f, .02f), wood);
                Box($"Base Drawer Pull {b}", new(bx, .76f, -1.12f), new(.10f, .03f, .02f), brass);
            }

            // Kitchen Wash Sink & Brass Gooseneck Faucet (matching Image 7)
            Box("Wash Sink Basin", new(-4.2f, .88f, -1.48f), new(.65f, .02f, .48f), dark);
            Box("Wash Sink Rim L", new(-4.53f, .91f, -1.48f), new(.04f, .05f, .50f), iron);
            Box("Wash Sink Rim R", new(-3.87f, .91f, -1.48f), new(.04f, .05f, .50f), iron);
            Box("Wash Sink Rim Back", new(-4.2f, .91f, -1.72f), new(.68f, .05f, .04f), iron);
            Box("Wash Sink Rim Front", new(-4.2f, .91f, -1.24f), new(.68f, .05f, .04f), iron);

            // Brass curved gooseneck faucet
            Rod("Faucet Spout Vertical", new(-4.2f, .93f, -1.70f), new(-4.2f, 1.18f, -1.70f), .03f, brass);
            Rod("Faucet Spout Arch", new(-4.2f, 1.18f, -1.70f), new(-4.2f, 1.22f, -1.58f), .028f, brass);
            Rod("Faucet Spout Drop", new(-4.2f, 1.22f, -1.58f), new(-4.2f, 1.15f, -1.55f), .025f, brass);
            Cylinder("Hot Water Tap", new(-4.32f, .95f, -1.70f), .03f, .04f, brass, Vector3.zero);
            Cylinder("Cold Water Tap", new(-4.08f, .95f, -1.70f), .03f, .04f, brass, Vector3.zero);

            // Countertop prep accessories
            Box("Oak Chopping Board", new(-5.2f, .94f, -1.45f), new(.42f, .035f, .34f), wood);
            Cylinder("Ceramic Prep Bowl", new(-3.3f, .99f, -1.45f), .14f, .12f, ceramic, Vector3.zero);
            Cylinder("Ceramic Pitcher", new(-5.5f, 1.05f, -1.35f), .11f, .24f, ceramic, Vector3.zero);
            Rod("Pitcher Handle", new(-5.5f, 1.14f, -1.22f), new(-5.5f, .96f, -1.22f), .025f, ceramic);

            // Upper Hanging Kitchen Cabinets (depth 0.38m, height 0.84m)
            Vector3 upperPos = new(-4.2f, 2.22f, -1.32f);
            Box("Kitchen Upper Cabinets", upperPos, new(3.2f, .84f, .38f), wood, true);
            Box("Upper Crown Molding", upperPos + Vector3.up * .44f, new(3.3f, .06f, .42f), wood);
            for (int b = 0; b < 4; b++)
            {
                float bx = -5.35f + b * .77f;
                Box($"Upper Door Panel {b}", new(bx, 2.22f, -1.12f), new(.68f, .76f, .02f), wood);
                Box($"Upper Door Recess {b}", new(bx, 2.22f, -1.115f), new(.56f, .64f, .01f), dark);
                Cylinder($"Upper Door Knob {b}", new(bx + .26f, 1.95f, -1.10f), .02f, .025f, brass, new(90, 0, 0));
            }

            // Top display shelf with ceramic dinner plates and spice jars
            for (int p = 0; p < 4; p++)
            {
                float px = -5.2f + p * .65f;
                Cylinder($"Display Plate {p}", new(px, 2.82f, -1.30f), .14f, .015f, ceramic, new(78, 0, 0));
            }
            for (int j = 0; j < 3; j++)
            {
                float jx = -3.2f + j * .32f;
                Cylinder($"Spice Jar {j}", new(jx, 2.76f, -1.30f), .06f, .12f, ceramic, Vector3.zero);
                Cylinder($"Spice Lid {j}", new(jx, 2.83f, -1.30f), .065f, .025f, wood, Vector3.zero);
            }

            // Under-cabinet warm task lighting strip
            Box("Under Cabinet Light Strip", new(-4.2f, 1.78f, -1.32f), new(3.0f, .02f, .30f), warmWindow);

            // Traditional Cast-Iron Cooking Range / Oven Stove (matching Image 7)
            Vector3 stovePos = new(-2.0f, .46f, -1.55f);
            Box("Stove Cast Body", stovePos, new(.92f, .88f, .76f), iron, true);
            Box("Stove Cooktop", stovePos + Vector3.up * .45f, new(.96f, .04f, .80f), iron, true);
            for (int sx = -1; sx <= 1; sx += 2) for (int sz = -1; sz <= 1; sz += 2)
                Cylinder("Stove Burner Ring", stovePos + new Vector3(sx * .24f, .48f, sz * .18f), .11f, .015f, iron, Vector3.zero);

            // Cast-iron oven door with glowing warm fire/embers viewing window
            Box("Oven Door Cast Frame", stovePos + new Vector3(0, -.02f, .39f), new(.78f, .56f, .04f), iron);
            Box("Oven Glowing Fire", stovePos + new Vector3(0, -.02f, .40f), new(.52f, .34f, .02f), stoveFire);
            Box("Oven Door Handle", stovePos + new Vector3(0, .20f, .42f), new(.34f, .03f, .03f), brass);

            // Simmering Cast-Iron Stew Pot on front burner
            Vector3 potPos = stovePos + new Vector3(-.24f, .59f, .18f);
            Cylinder("Stovetop Stew Pot", potPos, .20f, .22f, iron, Vector3.zero);
            Cylinder("Stew Pot Lid", potPos + Vector3.up * .12f, .21f, .03f, brass, Vector3.zero);
            Cylinder("Stew Pot Handle L", potPos + new Vector3(-.22f, .04f, 0), .03f, .04f, brass, new(0, 0, 90));
            Cylinder("Stew Pot Handle R", potPos + new Vector3(.22f, .04f, 0), .03f, .04f, brass, new(0, 0, 90));

            // Pyramidal Copper/Iron Chimney Range Hood
            Box("Range Hood Base Rim", stovePos + new Vector3(0, 1.42f, 0), new(1.05f, .06f, .82f), iron);
            Box("Range Hood Taper 1", stovePos + new Vector3(0, 1.55f, 0), new(.85f, .20f, .68f), iron);
            Box("Range Hood Taper 2", stovePos + new Vector3(0, 1.75f, 0), new(.60f, .20f, .48f), iron);
            Box("Range Hood Taper 3", stovePos + new Vector3(0, 1.95f, 0), new(.38f, .20f, .32f), iron);
            Cylinder("Stove Chimney Flue", stovePos + new Vector3(0, 2.25f, 0), .14f, .55f, iron, Vector3.zero);

            // Circular Wooden Dining Table (matching Image 7)
            Vector3 tablePos = new(-2.8f, 0, -5.8f);
            Cylinder("Dining Table Base", tablePos + Vector3.up * .05f, .40f, .05f, wood, Vector3.zero);
            Cylinder("Dining Table Pedestal", tablePos + Vector3.up * .42f, .12f, .74f, wood, Vector3.zero);
            Cylinder("Dining Tabletop", tablePos + Vector3.up * .82f, .68f, .06f, wood, Vector3.zero);

            // Decorative woven table runner
            Box("Dining Table Runner", tablePos + Vector3.up * .855f, new(.34f, .01f, 1.24f), parchment);

            // Glass Hurricane Storm Lantern on Dining Table
            Vector3 tableLamp = tablePos + new Vector3(-.24f, .86f, .16f);
            Cylinder("Table Lantern Base", tableLamp + Vector3.up * .03f, .10f, .04f, brass, Vector3.zero);
            Cylinder("Table Lantern Glass", tableLamp + Vector3.up * .15f, .085f, .20f, warmWindow, Vector3.zero);
            Cylinder("Table Lantern Hood", tableLamp + Vector3.up * .26f, .10f, .035f, brass, Vector3.zero);
            Rod("Table Lantern Bail", tableLamp + new Vector3(-.09f, .15f, 0), tableLamp + new Vector3(.09f, .15f, 0), .015f, brass);

            // Ceramic Flower Vase with Sunflowers (matching Image 7)
            Vector3 vasePos = tablePos + new Vector3(.18f, .86f, -.16f);
            Cylinder("Sunflower Vase Base", vasePos + Vector3.up * .02f, .09f, .04f, ceramic, Vector3.zero);
            Cylinder("Sunflower Vase Belly", vasePos + Vector3.up * .12f, .11f, .16f, ceramic, Vector3.zero);
            Cylinder("Sunflower Vase Neck", vasePos + Vector3.up * .23f, .065f, .08f, ceramic, Vector3.zero);

            // 3 Cheerful Sunflowers
            Vector3[] flowerCenters = new[] {
                vasePos + new Vector3(0, .36f, 0),
                vasePos + new Vector3(-.10f, .32f, .06f),
                vasePos + new Vector3(.10f, .33f, -.04f)
            };
            foreach (var fc in flowerCenters)
            {
                Rod("Sunflower Stem", vasePos + Vector3.up * .25f, fc, .018f, leaf);
                Cylinder("Sunflower Disk", fc, .048f, .015f, darkCoat, new(35, 15, 0));
                for (int p = 0; p < 8; p++)
                {
                    float pa = p * 45f * Mathf.Deg2Rad;
                    Vector3 petalDir = new Vector3(Mathf.Sin(pa), Mathf.Cos(pa), 0) * .075f;
                    Box("Sunflower Petal", fc + petalDir, new(.035f, .06f, .015f), sunflowerYellow);
                }
            }

            // Ceramic Soup Bowl & Spoon (The Hot Meal!)
            Vector3 mealPos = tablePos + new Vector3(0, .86f, -.32f);
            Cylinder("Meal Stew Bowl", mealPos + Vector3.up * .04f, .14f, .07f, ceramic, Vector3.zero);
            Cylinder("Meal Stew Surface", mealPos + Vector3.up * .065f, .125f, .015f, warm, Vector3.zero);
            Rod("Meal Soup Spoon", mealPos + new Vector3(.18f, .05f, 0), mealPos + new Vector3(.24f, .05f, -.18f), .018f, brass);

            // Turned Wooden Dining Chairs
            // Chair 1 (North of table facing South)
            Vector3 chair1 = tablePos + new Vector3(0, 0, .85f);
            Box("Chair 1 Seat", chair1 + Vector3.up * .46f, new(.48f, .05f, .46f), wood, true);
            for (int x = -1; x <= 1; x += 2) for (int z = -1; z <= 1; z += 2)
                Box("Chair 1 Leg", chair1 + new Vector3(x * .18f, .23f, z * .18f), new(.05f, .46f, .05f), wood);
            Rod("Chair 1 Back Post L", chair1 + new Vector3(-.19f, .48f, .19f), chair1 + new Vector3(-.19f, 1.15f, .19f), .045f, wood);
            Rod("Chair 1 Back Post R", chair1 + new Vector3(.19f, .48f, .19f), chair1 + new Vector3(.19f, 1.15f, .19f), .045f, wood);
            Box("Chair 1 Crest Rail", chair1 + new Vector3(0, 1.12f, .19f), new(.45f, .08f, .04f), wood);
            for (int s = -1; s <= 1; s++)
                Rod($"Chair 1 Slat {s}", chair1 + new Vector3(s * .10f, .48f, .19f), chair1 + new Vector3(s * .10f, 1.08f, .19f), .025f, wood);

            // Chair 2 (East of table facing West)
            Vector3 chair2 = tablePos + new Vector3(.85f, 0, 0);
            Box("Chair 2 Seat", chair2 + Vector3.up * .46f, new(.46f, .05f, .48f), wood, true);
            for (int x = -1; x <= 1; x += 2) for (int z = -1; z <= 1; z += 2)
                Box("Chair 2 Leg", chair2 + new Vector3(x * .18f, .23f, z * .18f), new(.05f, .46f, .05f), wood);
            Rod("Chair 2 Back Post L", chair2 + new Vector3(.19f, .48f, -.19f), chair2 + new Vector3(.19f, 1.15f, -.19f), .045f, wood);
            Rod("Chair 2 Back Post R", chair2 + new Vector3(.19f, .48f, .19f), chair2 + new Vector3(.19f, 1.15f, .19f), .045f, wood);
            Box("Chair 2 Crest Rail", chair2 + new Vector3(.19f, 1.12f, 0), new(.04f, .08f, .45f), wood);
            for (int s = -1; s <= 1; s++)
                Rod($"Chair 2 Slat {s}", chair2 + new Vector3(.19f, .48f, s * .10f), chair2 + new Vector3(.19f, 1.08f, s * .10f), .025f, wood);

            // Rustic Wooden Wagon Wheel leaning against East Wall (matching Image 7)
            Vector3 wheelPos = new(1.75f, .46f, -7.2f);
            Cylinder("Wagon Wheel Hub", wheelPos, .10f, .14f, wood, new(0, 0, 90));
            Cylinder("Wagon Wheel Hub Band", wheelPos, .105f, .03f, iron, new(0, 0, 90));
            Cylinder("Wagon Wheel Rim", wheelPos, .46f, .06f, wood, new(0, 0, 90));
            Cylinder("Wagon Wheel Iron Tire", wheelPos, .475f, .035f, iron, new(0, 0, 90));
            for (int sp = 0; sp < 10; sp++)
            {
                float sa = sp * 36f * Mathf.Deg2Rad;
                Vector3 spokeEnd = wheelPos + new Vector3(0, Mathf.Sin(sa) * .43f, Mathf.Cos(sa) * .43f);
                Rod($"Wheel Spoke {sp}", wheelPos, spokeEnd, .035f, wood);
            }

            // Upright Oak Barrel in corner (matching Image 7)
            Barrel("Kitchen Corner Barrel", new(-5.9f, .45f, -3.6f), Vector3.zero, .34f, .88f);

            // Striped Woven Kitchen Runner Rug (matching Image 7)
            Box("Kitchen Striped Rug", new(-3.9f, .072f, -2.4f), new(2.8f, .01f, .95f), stripedRug);

            // East Kitchen Window with rustic checkered curtains (matching Image 7)
            Window(new(1.95f, 1.70f, -5.0f), 1.10f, 1.35f, false, false);
            Rod("Curtain Rod", new(1.85f, 2.45f, -5.75f), new(1.85f, 2.45f, -4.25f), .035f, wood);
            Cylinder("Curtain Finial 1", new(1.85f, 2.45f, -5.77f), .04f, .04f, brass, new(90, 0, 0));
            Cylinder("Curtain Finial 2", new(1.85f, 2.45f, -4.23f), .04f, .04f, brass, new(90, 0, 0));
            Box("Curtain Left", new(1.86f, 1.72f, -5.60f), new(.06f, 1.40f, .28f), curtainFabric);
            Box("Curtain Right", new(1.86f, 1.72f, -4.40f), new(.06f, 1.40f, .28f), curtainFabric);
            Box("Curtain Tie Left", new(1.84f, 1.60f, -5.60f), new(.08f, .06f, .29f), darkCoat);
            Box("Curtain Tie Right", new(1.84f, 1.60f, -4.40f), new(.08f, .06f, .29f), darkCoat);

            // Overhead Drop Industrial Pendant Lamp (matching Image 7)
            Vector3 lampDrop = new(-3.0f, 2.9f, -4.5f);
            Rod("Kitchen Lamp Cord", lampDrop, lampDrop + Vector3.down * .65f, .02f, iron);
            Cylinder("Kitchen Lamp Shade", lampDrop + Vector3.down * .70f, .30f, .14f, iron, Vector3.zero);
            Cylinder("Kitchen Lamp Bulb", lampDrop + Vector3.down * .76f, .09f, .07f, warm, Vector3.zero);
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
            float baseAngle = 100f * Mathf.Deg2Rad;
            // Solid stone starter curb and iron newel post anchoring foot of spiral stairs on East wall
            Vector3 curbPos = new(Mathf.Sin(baseAngle)*(outerLevel0-.55f),.08f,Mathf.Cos(baseAngle)*(outerLevel0-.55f));
            Box("Spiral Stair Starter Curb",curbPos,new(settings.stairWidth+.12f,.16f,.44f),ground,true);
            Vector3 newelPos = new(Mathf.Sin(baseAngle)*innerLevel0,.14f,Mathf.Cos(baseAngle)*innerLevel0);
            Rod("Stair Starter Newel",newelPos,newelPos+Vector3.up*1.11f,.075f,iron);
            Cylinder("Stair Starter Newel Base",newelPos+Vector3.up*.01f,.12f,.03f,iron,Vector3.zero);
            for(int s=0;s<4;s++)
            {
                float sa=baseAngle - s*Mathf.PI*2/count;
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
                    float a=baseAngle - step*Mathf.PI*2/count, b=baseAngle - (step+1)*Mathf.PI*2/count;
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
            Vector3[] top = b > a 
                ? new[]{P(inner,a,y),P(outer,a,y),P(outer,b,y),P(inner,b,y)}
                : new[]{P(inner,b,y),P(outer,b,y),P(outer,a,y),P(inner,a,y)};
            void Face(Vector3 p0,Vector3 p1,Vector3 p2,Vector3 p3){int k=points.Count;points.AddRange(new[]{p0,p1,p2,p3});tris.AddRange(new[]{k,k+1,k+2,k,k+2,k+3});}
            Face(top[0],top[1],top[2],top[3]);Face(top[3]-Vector3.up*thick,top[2]-Vector3.up*thick,top[1]-Vector3.up*thick,top[0]-Vector3.up*thick);
            for(int i=0;i<4;i++){int j=(i+1)%4;Face(top[j],top[i],top[i]-Vector3.up*thick,top[j]-Vector3.up*thick);}
            var mesh=new Mesh{vertices=points.ToArray(),triangles=tris.ToArray()};mesh.RecalculateNormals();mesh.uv=points.Select(p=>new Vector2(p.x,p.z)).ToArray();
            var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(area);go.GetComponent<MeshFilter>().sharedMesh=Persist(mesh);go.GetComponent<Renderer>().sharedMaterial=material;
            if(collision){go.AddComponent<MeshCollider>().sharedMesh=go.GetComponent<MeshFilter>().sharedMesh;go.tag="Metal";}
        }
        static void Barrel(string name, Vector3 pos, Vector3 rot, float r, float h)
        {
            Cylinder(name + " Staves", pos, r, h, barrelWood, rot);
            Quaternion q = Quaternion.Euler(rot);
            Vector3 up = q * Vector3.up;
            Cylinder(name + " Hoop Top", pos + up * (h * .38f), r * 1.02f, .035f, barrelIron, rot);
            Cylinder(name + " Hoop Bottom", pos - up * (h * .38f), r * 1.02f, .035f, barrelIron, rot);
            Cylinder(name + " Hoop Mid Top", pos + up * (h * .18f), r * 1.04f, .03f, barrelIron, rot);
            Cylinder(name + " Hoop Mid Bottom", pos - up * (h * .18f), r * 1.04f, .03f, barrelIron, rot);
        }
        static void Vestibule()
        {
            // Continuous circular stone foundation plinth and flagstone floor
            Cylinder("Tower Stone Foundation", new(0, .07f, 0), 3.52f, .15f, ground, Vector3.zero);
            Cylinder("Tower Flagstone Floor", new(0, .145f, 0), 3.38f, .06f, ground, Vector3.zero);
            Cylinder("Entry Inlay Outer Ring", new(0, .150f, 0), 1.25f, .005f, brass, Vector3.zero);
            Cylinder("Entry Inlay Compass Center", new(0, .152f, 0), .40f, .006f, dark, Vector3.zero);

            // Under-stairs barrels, rustic workbench, and cloth (matching Image 1 under spiral stairs at East wall)
            Barrel("UnderStairs Barrel 1", new(2.05f, .38f, .75f), new(0, 85, 90), .34f, .85f);
            Barrel("UnderStairs Barrel 2", new(1.95f, .38f, 1.45f), new(0, 95, 90), .34f, .85f);
            Barrel("UnderStairs Barrel Top", new(2.0f, .82f, 1.10f), new(0, 90, 90), .33f, .82f);
            Barrel("UnderStairs Upright Barrel", new(1.35f, .45f, 1.55f), Vector3.zero, .34f, .88f);

            // Rustic wooden workbench beside barrel stack with draped cloth
            Vector3 benchPos = new(1.48f, 0, .75f);
            Box("Stair Workbench Top", benchPos + Vector3.up * .78f, new(.65f, .06f, 1.1f), wood, true);
            Box("Stair Workbench Shelf", benchPos + Vector3.up * .22f, new(.55f, .04f, .95f), wood);
            for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)
                Box("Bench Leg", benchPos + new Vector3(x * .25f, .39f, z * .45f), new(.07f, .78f, .07f), wood);
            Box("Bench Draped Cloth", benchPos + new Vector3(-.28f, .55f, .1f), new(.04f, .48f, .42f), darkCoat);
            Cylinder("Work Oil Can", benchPos + new Vector3(.05f, .88f, -.25f), .08f, .16f, iron, Vector3.zero);
            Rod("Oil Can Spout", benchPos + new Vector3(.05f, .96f, -.25f), benchPos + new Vector3(.02f, 1.10f, -.32f), .02f, brass);
            Box("Work Lantern", benchPos + new Vector3(.08f, .95f, .22f), new(.14f, .26f, .14f), brass);
            Box("Work Lantern Glass", benchPos + new Vector3(.08f, .95f, .22f), new(.10f, .18f, .10f), warm);

            // Vintage travel trunks neatly stowed under the workbench
            Box("Travel Trunk 1", benchPos + new Vector3(0, .14f, 0), new(.5f, .24f, .65f), leatherLuggage, true);
            Box("Trunk 1 Brass Latch", benchPos + new Vector3(-.26f, .14f, 0), new(.03f, .06f, .08f), brass);

            // Wet puddle under the stairs on stone floor
            var puddleUnder = GameObject.CreatePrimitive(PrimitiveType.Cylinder); puddleUnder.name = "Stair Wet Puddle";
            puddleUnder.transform.SetParent(area, false); puddleUnder.transform.position = new(1.7f, .155f, 1.1f);
            puddleUnder.transform.localScale = new(1.2f, .002f, 1.6f);
            puddleUnder.GetComponent<Renderer>().sharedMaterial = dark; Object.DestroyImmediate(puddleUnder.GetComponent<Collider>());

            // Coat peg rack along North curved wall
            Box("Coat Rack Rail", new(-.8f, 1.85f, 2.3f), new(2.2f, .14f, .06f), wood);
            for (int i = 0; i < 5; i++)
            {
                float x = -1.6f + i * .4f;
                Box($"Coat Hook {i}", new(x, 1.85f, 2.25f), new(.03f, .08f, .06f), brass);
            }

            // Iconic Yellow Oilskin Raincoat / Slicker on Hook 3 (x = -0.4)
            Box("Yellow Slicker Torso", new(-.4f, 1.35f, 2.22f), new(.46f, .85f, .12f), yellowSlicker);
            Box("Yellow Slicker Sleeve L", new(-.66f, 1.32f, 2.20f), new(.12f, .72f, .11f), yellowSlicker);
            Box("Yellow Slicker Sleeve R", new(-.14f, 1.32f, 2.20f), new(.12f, .72f, .11f), yellowSlicker);
            Box("Yellow Slicker Hood", new(-.4f, 1.86f, 2.20f), new(.24f, .22f, .14f), yellowSlicker);
            for (int b = 0; b < 4; b++) Box($"Slicker Button {b}", new(-.4f, 1.15f + b * .16f, 2.15f), new(.02f, .02f, .02f), dark);

            // Dark Navy Wool Keeper's Greatcoat on Hook 2 (x = -0.8)
            Box("Navy Coat Torso", new(-.85f, 1.30f, 2.23f), new(.50f, .95f, .14f), darkCoat);
            Box("Navy Coat Sleeve L", new(-1.14f, 1.28f, 2.21f), new(.13f, .80f, .12f), darkCoat);
            Box("Navy Coat Sleeve R", new(-.56f, 1.28f, 2.21f), new(.13f, .80f, .12f), darkCoat);
            for (int b = 0; b < 4; b++)
            {
                Box($"Navy Button L {b}", new(-.89f, 1.15f + b * .16f, 2.15f), new(.025f, .025f, .02f), brass);
                Box($"Navy Button R {b}", new(-.81f, 1.15f + b * .16f, 2.15f), new(.025f, .025f, .02f), brass);
            }

            // Keeper Cap / Hat on Hook 4 (x = 0.0)
            Cylinder("Keeper Cap Crown", new(0f, 1.95f, 2.20f), .14f, .08f, darkCoat, new(15, 0, 0));
            Cylinder("Keeper Cap Brim", new(0f, 1.90f, 2.14f), .16f, .015f, darkCoat, new(35, 0, 0));

            // Overhead luggage shelf mounted above coat rack at y = 2.45
            Box("Vestibule Luggage Shelf", new(-.8f, 2.45f, 2.28f), new(2.2f, .05f, .38f), wood);
            Rod("Shelf Bracket 1", new(-1.7f, 2.25f, 2.38f), new(-1.7f, 2.45f, 2.15f), .035f, iron);
            Rod("Shelf Bracket 2", new(-.8f, 2.25f, 2.38f), new(-.8f, 2.45f, 2.15f), .035f, iron);
            Rod("Shelf Bracket 3", new(.1f, 2.25f, 2.38f), new(.1f, 2.45f, 2.15f), .035f, iron);
            Box("Vintage Suitcase 1", new(-1.3f, 2.62f, 2.26f), new(.55f, .30f, .32f), leatherLuggage);
            Cylinder("Vintage Hat Box", new(-.5f, 2.60f, 2.26f), .18f, .24f, wood, Vector3.zero);

            // Wall gallery of 6 vintage framed black-and-white photographs / portraits
            Box("Photo Frame 1", new(-2.0f, 1.85f, 1.8f), new(.42f, .52f, .04f), wood);
            Box("Photo Canvas 1", new(-2.0f, 1.85f, 1.77f), new(.34f, .44f, .015f), parchment);
            Box("Photo Frame 2", new(-2.0f, 1.25f, 1.8f), new(.48f, .34f, .04f), iron);
            Box("Photo Canvas 2", new(-2.0f, 1.25f, 1.77f), new(.42f, .28f, .015f), parchment);
            Box("Photo Frame 3", new(-1.5f, 1.20f, 2.15f), new(.32f, .38f, .04f), wood);
            Box("Photo Canvas 3", new(-1.5f, 1.20f, 2.12f), new(.24f, .30f, .015f), parchment);
            Box("Photo Frame 4", new(-2.0f, 2.40f, 1.75f), new(.36f, .28f, .04f), wood);
            Box("Photo Canvas 4", new(-2.0f, 2.40f, 1.72f), new(.30f, .22f, .015f), parchment);
            Box("Photo Frame 5", new(.45f, 1.65f, 2.35f), new(.38f, .46f, .04f), wood);
            Box("Photo Canvas 5", new(.45f, 1.65f, 2.32f), new(.30f, .38f, .015f), parchment);
            Box("Photo Frame 6", new(.45f, 2.20f, 2.35f), new(.34f, .26f, .04f), iron);
            Box("Photo Canvas 6", new(.45f, 2.20f, 2.32f), new(.28f, .20f, .015f), parchment);

            // Floor props: Weathered sea chest & boot stool
            Box("Sea Chest Body", new(-.6f, .38f, 1.75f), new(.95f, .46f, .52f), wood, true);
            Box("Sea Chest Iron Straps", new(-.6f, .38f, 1.75f), new(.97f, .48f, .30f), iron);
            Box("Sea Chest Brass Lock", new(-.6f, .46f, 1.48f), new(.09f, .09f, .03f), brass);
            Box("Boot Stool Seat", new(.25f, .38f, 1.75f), new(.48f, .05f, .32f), wood, true);
            Box("Boot Stool Leg 1", new(.1f, .20f, 1.65f), new(.05f, .35f, .05f), wood);
            Box("Boot Stool Leg 2", new(.4f, .20f, 1.65f), new(.05f, .35f, .05f), wood);
            Box("Boot Stool Leg 3", new(.1f, .20f, 1.85f), new(.05f, .35f, .05f), wood);
            Box("Boot Stool Leg 4", new(.4f, .20f, 1.85f), new(.05f, .35f, .05f), wood);
            Box("Keeper Boot L", new(.2f, .32f, 1.45f), new(.12f, .32f, .26f), dark);
            Box("Keeper Boot R", new(.35f, .32f, 1.45f), new(.12f, .32f, .26f), dark);

            // Overhead drop pendant lamp and wall carriage sconce
            Rod("Vestibule Lamp Cord", new(0, 3.1f, 0), new(0, 2.45f, 0), .02f, iron);
            Cylinder("Vestibule Lamp Shade", new(0, 2.40f, 0), .28f, .12f, iron, Vector3.zero);
            Cylinder("Vestibule Lamp Bulb", new(0, 2.34f, 0), .08f, .06f, warm, Vector3.zero);
            Box("Wall Sconce Base", new(1.2f, 2.0f, 1.95f), new(.10f, .22f, .04f), brass);
            Rod("Wall Sconce Arm", new(1.2f, 1.95f, 1.95f), new(1.2f, 2.05f, 1.80f), .03f, brass);
            Cylinder("Wall Sconce Lantern", new(1.2f, 2.10f, 1.80f), .09f, .22f, glass, Vector3.zero);
            Cylinder("Wall Sconce Cap", new(1.2f, 2.22f, 1.80f), .11f, .04f, brass, Vector3.zero);
        }

        static void RoomFloor(float y, float innerStairR, float wallR, float floorThick, float openCenterDeg, float openHalfSpan)
        {
            const int segments = 24;
            for (int i = 0; i < segments; i++)
            {
                float a = i * Mathf.PI * 2 / segments, b = (i + 1) * Mathf.PI * 2 / segments;
                Wedge("Room Floor Center", .05f, innerStairR, a, b, y, floorThick, wood, true);
            }
            const int outerSegments = 36;
            for (int i = 0; i < outerSegments; i++)
            {
                float deg = i * 360f / outerSegments;
                if (Mathf.Abs(Mathf.DeltaAngle(deg, openCenterDeg)) < openHalfSpan) continue;
                float a = deg * Mathf.Deg2Rad, b = (deg + 360f / outerSegments) * Mathf.Deg2Rad;
                Wedge("Room Floor Outer", innerStairR, wallR, a, b, y, floorThick, wood, true);
            }
            // Curved protective wrought-iron balustrade along stairwell cutout
            float railH = 1.05f;
            int railSteps = Mathf.CeilToInt(openHalfSpan * 2f / 10f);
            Vector3 prevTop = Vector3.zero, prevBot = Vector3.zero;
            for (int i = 0; i <= railSteps; i++)
            {
                float t = i / (float)railSteps;
                float deg = (openCenterDeg - openHalfSpan + 6f) + t * (openHalfSpan * 2f - 12f);
                float rad = deg * Mathf.Deg2Rad;
                Vector3 bot = new(Mathf.Sin(rad) * innerStairR, y + .04f, Mathf.Cos(rad) * innerStairR);
                Vector3 top = bot + Vector3.up * railH;
                Rod("Balustrade Baluster", bot, top, .035f, iron);
                if (i > 0)
                {
                    Rod("Balustrade Top Rail", prevTop, top, .05f, iron);
                    Rod("Balustrade Mid Rail", (prevBot + prevTop) * .5f, (bot + top) * .5f, .035f, iron);
                    Rod("Balustrade Base Rail", prevBot, bot, .04f, iron);
                }
                prevTop = top; prevBot = bot;
            }
            float landingDeg = 85f;
            float landingRad = landingDeg * Mathf.Deg2Rad;
            Vector3 newelP = new(Mathf.Sin(landingRad) * innerStairR, y, Mathf.Cos(landingRad) * innerStairR);
            Rod("Balustrade Newel", newelP, newelP + Vector3.up * 1.15f, .08f, iron);
            Cylinder("Balustrade Newel Finial", newelP + Vector3.up * 1.18f, .06f, .08f, brass, Vector3.zero);
        }

        static void Library()
        {
            RoomFloor(3.2f, 1.90f, 3.03f, .12f, -180f, 95f);
            Box("Library Area Rug", new(.1f, 3.21f, .2f), new(2.4f, .012f, 1.6f), rugFabric);

            // Grand Curved Floor-to-Ceiling Library Bookcase
            Box("Bookcase Left Upright", new(-1.3f, 4.5f, 2.35f), new(.08f, 2.6f, .40f), wood, true);
            Box("Bookcase Right Upright", new(1.3f, 4.5f, 2.35f), new(.08f, 2.6f, .40f), wood, true);
            Box("Bookcase Center Divider", new(0f, 4.5f, 2.35f), new(.06f, 2.6f, .38f), wood);
            Box("Bookcase Top Crown", new(0f, 5.82f, 2.35f), new(2.72f, .10f, .44f), wood);
            Box("Bookcase Shelf 1", new(0f, 3.32f, 2.35f), new(2.6f, .05f, .38f), wood);
            Box("Bookcase Shelf 2", new(0f, 3.95f, 2.35f), new(2.6f, .05f, .38f), wood);
            Box("Bookcase Shelf 3", new(0f, 4.60f, 2.35f), new(2.6f, .05f, .38f), wood);
            Box("Bookcase Shelf 4", new(0f, 5.25f, 2.35f), new(2.6f, .05f, .38f), wood);

            // Dense leatherbound books on Shelf 2
            for (int b = 0; b < 22; b++)
            {
                float bx = -1.15f + b * .105f;
                float bh = .24f + (b % 4) * .03f;
                Material bm = (b % 4 == 0) ? bookRed : (b % 4 == 1) ? bookGreen : (b % 4 == 2) ? bookBlue : bookGold;
                Box($"Book S2_{b}", new(bx, 3.98f + bh * .5f, 2.32f), new(.07f, bh, .24f), bm);
                Box($"Book Spine S2_{b}", new(bx, 3.98f + bh * .5f, 2.19f), new(.065f, bh * .9f, .02f), bookGold);
            }
            // Books on Shelf 3
            for (int b = 0; b < 20; b++)
            {
                float bx = -1.05f + b * .11f;
                float bh = .22f + ((b * 3) % 5) * .025f;
                Material bm = (b % 3 == 0) ? bookBlue : (b % 3 == 1) ? bookRed : bookGreen;
                Box($"Book S3_{b}", new(bx, 4.63f + bh * .5f, 2.32f), new(.075f, bh, .22f), bm);
            }

            // Shelf 1: Heavy log ledgers and chart tubes
            Box("Log Ledger Stack", new(-.7f, 3.48f, 2.32f), new(.48f, .25f, .30f), darkCoat);
            Box("Harbor Board Folios", new(.6f, 3.52f, 2.32f), new(.55f, .32f, .28f), parchment);
            Cylinder("Chart Tube 1", new(-.2f, 3.42f, 2.32f), .06f, .42f, wood, new(0, 0, 90));
            Cylinder("Chart Tube 2", new(-.2f, 3.54f, 2.32f), .05f, .40f, parchment, new(0, 0, 90));

            // Shelf 4: Potted plant with trailing green ivy
            Cylinder("Library Planter Pot", new(.7f, 5.38f, 2.30f), .16f, .20f, warm, Vector3.zero);
            Box("Planter Soil", new(.7f, 5.47f, 2.30f), new(.28f, .02f, .28f), ground);
            for (int v = 0; v < 8; v++)
            {
                float vy = 5.40f - v * .08f;
                float vx = .7f + Mathf.Sin(v * .8f) * .12f;
                Box($"Trailing Ivy {v}", new(vx, vy, 2.12f), new(.18f, .07f, .04f), leaf);
            }

            // Study Writing Bureau / Desk
            Box("Desk Top", new(.1f, 3.96f, .5f), new(1.7f, .08f, .85f), wood, true);
            Box("Desk Pedestal L", new(-.55f, 3.58f, .5f), new(.42f, .68f, .78f), wood, true);
            Box("Desk Drawer Pull L1", new(-.55f, 3.82f, .10f), new(.12f, .03f, .02f), brass);
            Box("Desk Drawer Pull L2", new(-.55f, 3.60f, .10f), new(.12f, .03f, .02f), brass);
            Box("Desk Drawer Pull L3", new(-.55f, 3.38f, .10f), new(.12f, .03f, .02f), brass);
            Box("Desk Pedestal R", new(.75f, 3.58f, .5f), new(.42f, .68f, .78f), wood, true);
            Box("Desk Drawer Pull R1", new(.75f, 3.82f, .10f), new(.12f, .03f, .02f), brass);
            Box("Desk Drawer Pull R2", new(.75f, 3.60f, .10f), new(.12f, .03f, .02f), brass);
            Box("Desk Drawer Pull R3", new(.75f, 3.38f, .10f), new(.12f, .03f, .02f), brass);
            Box("Desk Kneehole Panel", new(.1f, 3.62f, .84f), new(.85f, .58f, .04f), wood);

            // Brass Desktop Terrestrial Globe
            Cylinder("Globe Base", new(-.45f, 4.02f, .65f), .11f, .03f, wood, Vector3.zero);
            Rod("Globe Spindle", new(-.45f, 4.03f, .65f), new(-.45f, 4.16f, .65f), .025f, brass);
            var globe = GameObject.CreatePrimitive(PrimitiveType.Sphere); globe.name = "Globe Sphere"; globe.transform.SetParent(area);
            globe.transform.position = new(-.45f, 4.28f, .65f); globe.transform.localScale = Vector3.one * .24f;
            globe.transform.eulerAngles = new(23.5f, 30f, 0); globe.GetComponent<Renderer>().sharedMaterial = parchment; Object.DestroyImmediate(globe.GetComponent<Collider>());
            Cylinder("Globe Meridian Ring", new(-.45f, 4.28f, .65f), .14f, .015f, brass, new(0, 0, 90));

            // Brass Navigation Astrolabe / Armillary Sphere
            Cylinder("Astrolabe Pedestal", new(.55f, 4.02f, .60f), .08f, .03f, brass, Vector3.zero);
            Rod("Astrolabe Stand", new(.55f, 4.03f, .60f), new(.55f, 4.14f, .60f), .025f, brass);
            Cylinder("Astrolabe Outer Ring", new(.55f, 4.22f, .60f), .12f, .015f, brass, Vector3.zero);
            Cylinder("Astrolabe Meridian Ring", new(.55f, 4.22f, .60f), .11f, .015f, brass, new(90, 0, 0));
            Cylinder("Astrolabe Equator Ring", new(.55f, 4.22f, .60f), .10f, .015f, brass, new(0, 0, 90));

            // Brass Reading Desk Banker Lamp
            Cylinder("Banker Lamp Base", new(-.1f, 4.02f, .72f), .09f, .025f, brass, Vector3.zero);
            Rod("Banker Lamp Neck", new(-.1f, 4.03f, .72f), new(-.1f, 4.30f, .62f), .025f, brass);
            Box("Banker Lamp Shade", new(-.1f, 4.28f, .58f), new(.24f, .09f, .14f), warm);

            // Open Keeper Logbook, Inkwell & Quill
            Box("Logbook Page L", new(.10f, 4.02f, .45f), new(.18f, .015f, .26f), parchment);
            Box("Logbook Page R", new(.29f, 4.02f, .45f), new(.18f, .015f, .26f), parchment);
            Box("Logbook Cover", new(.195f, 4.005f, .45f), new(.39f, .015f, .28f), wood);
            Cylinder("Inkwell", new(-.25f, 4.02f, .42f), .04f, .05f, dark, Vector3.zero);
            Rod("Quill", new(-.25f, 4.03f, .42f), new(-.28f, 4.22f, .38f), .015f, parchment);

            // Carved Library Armchair
            Box("Chair Seat", new(.1f, 3.65f, 1.25f), new(.58f, .08f, .55f), wood, true);
            Box("Chair Leg FL", new(-.15f, 3.42f, 1.02f), new(.06f, .44f, .06f), wood);
            Box("Chair Leg FR", new(.35f, 3.42f, 1.02f), new(.06f, .44f, .06f), wood);
            Box("Chair Leg BL", new(-.15f, 3.42f, 1.48f), new(.06f, .44f, .06f), wood);
            Box("Chair Leg BR", new(.35f, 3.42f, 1.48f), new(.06f, .44f, .06f), wood);
            Rod("Chair Back Post L", new(-.15f, 3.65f, 1.48f), new(-.15f, 4.45f, 1.48f), .06f, wood);
            Rod("Chair Back Post R", new(.35f, 3.65f, 1.48f), new(.35f, 4.45f, 1.48f), .06f, wood);
            Box("Chair Top Rail", new(.10f, 4.42f, 1.48f), new(.56f, .12f, .06f), wood);
            Box("Chair Back Slat 1", new(-.02f, 4.05f, 1.48f), new(.04f, .65f, .03f), wood);
            Box("Chair Back Slat 2", new(.10f, 4.05f, 1.48f), new(.04f, .65f, .03f), wood);
            Box("Chair Back Slat 3", new(.22f, 4.05f, 1.48f), new(.04f, .65f, .03f), wood);
            Rod("Chair Arm L", new(-.15f, 3.95f, 1.02f), new(-.15f, 4.05f, 1.48f), .05f, wood);
            Rod("Chair Arm R", new(.35f, 3.95f, 1.02f), new(.35f, 4.05f, 1.48f), .05f, wood);

            // Overhead Drop Pendant Lamp
            Rod("Library Lamp Cord", new(0, 6.3f, .3f), new(0, 5.05f, .3f), .02f, iron);
            Cylinder("Library Lamp Shade", new(0, 5.0f, .3f), .28f, .12f, iron, Vector3.zero);
            Cylinder("Library Lamp Bulb", new(0, 4.94f, .3f), .08f, .06f, warm, Vector3.zero);
        }

        static void WatchRoom()
        {
            RoomFloor(6.4f, 1.76f, 2.88f, .12f, -180f, 95f);
            Box("Watch Room Braided Rug", new(-.2f, 6.41f, .3f), new(1.8f, .012f, 1.8f), braidedRug);

            // Large Prominent Nautical Station Wall Clock
            Cylinder("Clock Iron Rim", new(0, 8.1f, 2.65f), .44f, .05f, iron, new(90, 0, 0));
            Cylinder("Clock Dial Face", new(0, 8.1f, 2.62f), .40f, .03f, clockDial, new(90, 0, 0));
            for (int h = 0; h < 12; h++)
            {
                float alpha = h * 30f * Mathf.Deg2Rad;
                Box($"Clock Tick {h}", new(Mathf.Sin(alpha) * .34f, 8.1f + Mathf.Cos(alpha) * .34f, 2.60f), new(.022f, .055f, .012f), dark);
            }
            Rod("Clock Hour Hand", new(0, 8.1f, 2.59f), new(-.13f, 8.24f, 2.59f), .025f, dark);
            Rod("Clock Minute Hand", new(0, 8.1f, 2.59f), new(.18f, 8.28f, 2.59f), .018f, dark);
            Cylinder("Clock Hub Cap", new(0, 8.1f, 2.58f), .045f, .02f, brass, new(90, 0, 0));

            // Framed Sea Navigation Charts Flanking Clock
            Box("Depth Chart Frame", new(-.95f, 8.1f, 2.45f), new(.68f, .52f, .04f), wood);
            Box("Depth Chart Paper", new(-.95f, 8.1f, 2.42f), new(.60f, .44f, .015f), parchment);
            Box("Shipping Chart Frame", new(.95f, 8.1f, 2.45f), new(.68f, .52f, .04f), wood);
            Box("Shipping Chart Paper", new(.95f, 8.1f, 2.42f), new(.60f, .44f, .015f), parchment);

            // Observation Window at South
            Window(new(0, 7.8f, -2.75f), 1.0f, 1.4f, false);

            // Chart Drafting Table & Maritime Instruments
            Box("Drafting Table Top", new(-.25f, 7.28f, .5f), new(1.8f, .07f, .95f), wood, true);
            Box("Drafting Leg L", new(-1.0f, 6.82f, .5f), new(.08f, .84f, .85f), wood);
            Box("Drafting Leg R", new(.5f, 6.82f, .5f), new(.08f, .84f, .85f), wood);
            Box("Drafting Stretcher", new(-.25f, 6.62f, .5f), new(1.4f, .06f, .12f), wood);

            // Tabletop navigation charts and instruments
            Box("Unrolled Sea Chart", new(-.35f, 7.33f, .5f), new(.95f, .012f, .65f), parchment);
            Cylinder("Chart Weight 1", new(-.78f, 7.35f, .22f), .035f, .03f, brass, Vector3.zero);
            Cylinder("Chart Weight 2", new(.08f, 7.35f, .22f), .035f, .03f, brass, Vector3.zero);
            Cylinder("Chart Weight 3", new(-.78f, 7.35f, .78f), .035f, .03f, brass, Vector3.zero);
            Cylinder("Chart Weight 4", new(.08f, 7.35f, .78f), .035f, .03f, brass, Vector3.zero);
            Cylinder("Drafting Scroll 1", new(-.3f, 7.36f, .85f), .045f, .60f, parchment, new(0, 0, 90));
            Cylinder("Drafting Scroll 2", new(-.2f, 7.42f, .85f), .040f, .55f, parchment, new(0, 0, 90));
            Rod("Divider Leg 1", new(-.1f, 7.35f, .45f), new(-.18f, 7.35f, .32f), .015f, brass);
            Rod("Divider Leg 2", new(-.1f, 7.35f, .45f), new(-.02f, 7.35f, .32f), .015f, brass);
            Box("Compass Wooden Box", new(-.65f, 7.35f, .48f), new(.16f, .08f, .16f), wood);
            Cylinder("Compass Brass Dial", new(-.65f, 7.39f, .48f), .06f, .015f, brass, Vector3.zero);

            // Maritime Spyglass / Telescope on Desktop Tripod
            Cylinder("Spyglass Main Tube", new(.35f, 7.45f, .45f), .035f, .35f, brass, new(-12, 25, 0));
            Cylinder("Spyglass Lens Cell", new(.28f, 7.48f, .30f), .045f, .08f, brass, new(-12, 25, 0));
            Rod("Spyglass Tripod Leg 1", new(.38f, 7.42f, .48f), new(.32f, 7.32f, .40f), .018f, wood);
            Rod("Spyglass Tripod Leg 2", new(.38f, 7.42f, .48f), new(.44f, 7.32f, .40f), .018f, wood);
            Rod("Spyglass Tripod Leg 3", new(.38f, 7.42f, .48f), new(.38f, 7.32f, .56f), .018f, wood);

            // Watchkeeper Equipment Shelf
            Box("Equipment Wall Shelf", new(-1.3f, 7.7f, 1.8f), new(.95f, .04f, .28f), wood);
            Cylinder("Aneroid Barometer", new(-1.5f, 7.82f, 1.8f), .10f, .04f, brass, new(90, 0, 0));
            Cylinder("Specimen Bottle 1", new(-1.25f, 7.80f, 1.8f), .05f, .16f, glass, Vector3.zero);
            Cylinder("Specimen Bottle 2", new(-1.10f, 7.82f, 1.8f), .06f, .20f, glass, Vector3.zero);
            Cylinder("Spare Hurricane Lamp", new(-.92f, 7.85f, 1.8f), .08f, .24f, brass, Vector3.zero);

            // Watchkeeper Stool
            Cylinder("Watch Stool Seat", new(.55f, 7.05f, 1.0f), .20f, .05f, wood, Vector3.zero);
            for (int l = 0; l < 4; l++)
            {
                float la = l * 90f * Mathf.Deg2Rad;
                Rod($"Watch Stool Leg {l}", new(.55f + Mathf.Sin(la) * .14f, 7.05f, 1.0f + Mathf.Cos(la) * .14f), new(.55f + Mathf.Sin(la) * .22f, 6.4f, 1.0f + Mathf.Cos(la) * .22f), .04f, wood);
            }
            Cylinder("Watch Stool Footring", new(.55f, 6.62f, 1.0f), .21f, .02f, iron, Vector3.zero);

            // Overhead Drop Pendant Lamp
            Rod("Watch Lamp Cord", new(-.25f, 9.5f, .5f), new(-.25f, 8.15f, .5f), .02f, iron);
            Cylinder("Watch Lamp Shade", new(-.25f, 8.10f, .5f), .28f, .12f, iron, Vector3.zero);
            Cylinder("Watch Lamp Bulb", new(-.25f, 8.04f, .5f), .08f, .06f, warm, Vector3.zero);
        }

        static void Landing3()
        {
            RoomFloor(9.6f, 1.62f, 2.74f, .12f, -180f, 95f);
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
            for(int level=2;level<8;level+=2)
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
            if(beam) { beam.transform.localScale=Vector3.one*42; beam.GetComponent<Renderer>().sharedMaterial.SetColor("_BeamColor",new Color(.85f,.95f,1f,.45f)); }
            var pivot=GameObject.Find("Beam Pivot"); if(pivot) pivot.transform.localRotation=Quaternion.Euler(0,55,0);
            var spot=lights.FirstOrDefault(l=>l.name=="Beam Spot"); if(spot){spot.color=new(.75f,.88f,1);spot.intensity=20;spot.range=50;spot.spotAngle=38;}
            var glow=GameObject.Find("Lamp Glow"); if(glow)glow.GetComponent<SpriteRenderer>().color=new(.85f,.95f,1,1);
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
            EnsureTowerLight("Vestibule Light", new(0, 2.25f, 0), new Color(1f, .82f, .55f), 4.5f, 7f);
            EnsureTowerLight("Vestibule Wall Sconce", new(1.2f, 2.0f, 1.9f), new Color(1f, .75f, .45f), 2.0f, 4.5f);
            EnsureTowerLight("Library Light", new(0, 4.9f, .3f), new Color(1f, .82f, .55f), 5.0f, 7.5f);
            EnsureTowerLight("Library Desk Light", new(-.1f, 4.25f, .62f), new Color(1f, .85f, .5f), 2.5f, 3f);
            EnsureTowerLight("Library Stair Light", new(1.5f, 4.3f, 1.5f), new Color(1f, .75f, .45f), 2.0f, 4.5f);
            EnsureTowerLight("Watch Room Light", new(-.25f, 8.0f, .5f), new Color(1f, .82f, .55f), 5.0f, 7.5f);
            EnsureTowerLight("Landing 3 Light", new(0, 11.2f, 0), new Color(1f, .8f, .5f), 3.5f, 6f);
            EnsureTowerLight("House Porch Lantern", new(-5f, 2.65f, -9.6f), new Color(1f, .78f, .42f), 4.8f, 9f);
            EnsureTowerLight("House Interior Glow", new(-5f, 1.8f, -7.0f), new Color(1f, .78f, .38f), 3.5f, 6.5f);
            EnsureTowerLight("Keeper Desk Lamp Light", new(-8.6f, 1.8f, -1.9f), new Color(1f, .82f, .48f), 3.8f, 3f);
            EnsureTowerLight("House Kitchen Light", new(-3.0f, 2.3f, -4.5f), new Color(1f, .80f, .45f), 4.5f, 7.5f);
            EnsureTowerLight("Stove Hearth Glow", new(-2.0f, .45f, -1.1f), new Color(1f, .50f, .15f), 2.5f, 3.5f);
            EnsureTowerLight("Dining Table Lantern", new(-3.04f, 1.05f, -5.64f), new Color(1f, .82f, .45f), 2.8f, 4.0f);
        }
        static void EnsureTowerLight(string name, Vector3 pos, Color color, float intensity, float range)
        {
            var go = GameObject.Find(name);
            if (!go)
            {
                go = new GameObject(name, typeof(Light));
                go.transform.SetParent(root);
            }
            go.transform.position = pos;
            var l = go.GetComponent<Light>();
            l.type = LightType.Point;
            l.color = color;
            l.intensity = intensity;
            l.range = range;
            l.shadows = LightShadows.None;
        }
        static AtmospherePreset NightPreset(AtmospherePreset p)
        {
            p.ambientColor=new(.09f,.14f,.16f);p.directionalLightColor=look.moon;p.directionalLightIntensity=look.moonIntensity;p.fogColor=new(.055f,.085f,.095f);p.fogDensity=look.fogDensity;p.skyboxExposure=.05f;return p;
        }
    }
}
