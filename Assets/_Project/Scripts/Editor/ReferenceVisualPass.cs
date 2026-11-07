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
        static Material plaster, wood, iron, roof, dark, glass, warm, leaf, ground, grate, towerStone;
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
            towerStone = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/PBR/PBR_LighthouseStone.mat");
            plaster = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/PBR/PBR_WeatheredPlaster.mat");
            if (!plaster) plaster = ReferenceSurfaceLibrary.Create("Salt Plaster", look.plaster, 0, look.surfaceWear);
            wood = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/PBR/PBR_WeatheredPlanks.mat");
            if (!wood) wood = ReferenceSurfaceLibrary.Create("Old Timber", look.wood, 1, look.surfaceWear);
            iron = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/PBR/PBR_RustedIron.mat");
            if (!iron) iron = ReferenceSurfaceLibrary.Create("Oxidised Iron", look.iron, 2, look.surfaceWear, .45f, .25f);
            ground = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/PBR/PBR_CoastalRock.mat");
            if (!ground) ground = ReferenceSurfaceLibrary.Create("Wet Shale",new(.065f,.08f,.075f),0,.9f,0,.3f);
            grate = ReferenceSurfaceLibrary.Grating(iron);
            roof = ReferenceSurfaceLibrary.Create("Slate", new(.12f,.15f,.16f), 1, .7f, .1f, .32f);
            dark = ReferenceSurfaceLibrary.Solid("Recess", new(.018f,.027f,.026f));
            glass = ReferenceSurfaceLibrary.Solid("Cold Window", new(.42f,.58f,.58f), 1.3f);
            warm = ReferenceSurfaceLibrary.Solid("Lamp Enamel", new(.66f,.47f,.25f), .15f);
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
                if(n=="Beach"||n=="Sea Plane"){renderer.enabled=false;foreach(var c in renderer.GetComponents<Collider>())c.enabled=false;continue;}
                if(n=="Courtyard"){renderer.transform.position=new(0,.05f,-12.5f);renderer.transform.localScale=new(24,.2f,12);}
                if(n=="Courtyard"||n=="Cliff Path")renderer.sharedMaterial=ground;
                if(n.StartsWith("Boat Shed"))renderer.sharedMaterial=n.Contains("Roof")?roof:wood;
                if (n.StartsWith("House ") || n.StartsWith("Lantern Ring") || n == "Lantern Cap" || n == "Gallery Floor" || n.StartsWith("Spiral L") || n == "Tower Central Support")
                { renderer.enabled = false; foreach (var c in renderer.GetComponents<Collider>()) c.enabled = false; continue; }
                if (n == "Courtyard Wall West") { renderer.transform.position = new(-13.8f,1.1f,-12.5f); renderer.transform.localScale = new(.45f,2.2f,12); }
                if (n == "Courtyard Wall East") { renderer.transform.position = new(10,1.1f,-12.5f); renderer.transform.localScale = new(.45f,2.2f,12); }
                if (n.StartsWith("Tower L") && n.Contains(" wall "))
                {
                    renderer.enabled = false;
                    foreach (var c in renderer.GetComponents<Collider>()) c.enabled = false;
                    continue;
                }
                if (n.StartsWith("Tower Floor")) { renderer.enabled = false; foreach (var c in renderer.GetComponents<Collider>()) c.enabled = false; continue; }
                if (n.StartsWith("Tower Cornice")) { renderer.enabled = false; foreach (var c in renderer.GetComponents<Collider>()) c.enabled = false; continue; }
                if (n.StartsWith("Gallery Rail")) { renderer.enabled = false; foreach (var c in renderer.GetComponents<Collider>()) c.enabled = false; continue; }
                if (n.Contains("Window")) { renderer.enabled = false; continue; }
                string materialName = renderer.sharedMaterial ? renderer.sharedMaterial.name : "";
                if (materialName == "Stone" || materialName == "Salt Plaster") renderer.sharedMaterial = plaster;
                if (materialName == "Wood" || materialName == "Old Timber") renderer.sharedMaterial = wood;
                if (materialName == "Metal" || materialName == "Oxidised Iron") renderer.sharedMaterial = iron;
                if (renderer.sharedMaterial == plaster || renderer.sharedMaterial == wood || renderer.sharedMaterial == iron || renderer.sharedMaterial == ground) MapUV(renderer.GetComponent<MeshFilter>());
            }

            // 1. Exterior
            area = Group("Lighthouse/Exterior"); TowerShell(); CombineArea();

            // 2. Attached Cottage
            area = Group("Lighthouse/AttachedCottage/Architecture"); House(); CombineArea();
            area = Group("Lighthouse/AttachedCottage/RadioOffice"); Office(); CombineArea();
            area = Group("Lighthouse/AttachedCottage/Kitchen"); KitchenCountersAndStove(); CombineArea();
            area = Group("Lighthouse/AttachedCottage/DiningArea"); DiningRoom(); CombineArea();

            // 3. Ground Floor
            area = Group("Lighthouse/GroundFloor/Architecture"); VestibuleArchitecture(); CombineArea();
            area = Group("Lighthouse/GroundFloor/EntranceRoom"); VestibuleEntrance(); CombineArea();
            area = Group("Lighthouse/GroundFloor/Storage"); VestibuleStorage(); CombineArea();
            area = Group("Lighthouse/GroundFloor/Staircase_A"); StaircaseA(); CombineArea();

            // 4. First Floor
            area = Group("Lighthouse/FirstFloor/Architecture"); LibraryArchitecture(); CombineArea();
            area = Group("Lighthouse/FirstFloor/ArchiveRoom"); LibraryArchiveAndWorkshop(); CombineArea();
            area = Group("Lighthouse/FirstFloor/Staircase_B"); StaircaseB(); CombineArea();

            // 5. Second Floor
            area = Group("Lighthouse/SecondFloor/Architecture"); WatchRoomArchitecture(); CombineArea();
            area = Group("Lighthouse/SecondFloor/LivingRoom"); WatchRoomLiving(); CombineArea();
            area = Group("Lighthouse/SecondFloor/Staircase_C"); StaircaseC(); CombineArea();

            // 6. Upper Tower
            area = Group("Lighthouse/UpperTower/CraneQuarters_Level3"); Landing3(); CombineArea();
            area = Group("Lighthouse/UpperTower/IntermediateFloors"); TowerIntermediateFloors(); CombineArea();
            area = Group("Lighthouse/UpperTower/Stairs_Flights3_7"); UpperTowerStairs(); CombineArea();

            // 7. Lantern Floor
            area = Group("Lighthouse/LanternFloor/Architecture"); LanternArchitecture(); CombineArea();
            area = Group("Lighthouse/LanternFloor/LanternRoom"); LanternRoom(); CombineArea();
            area = Group("Lighthouse/LanternFloor/LampMechanism"); LampMechanism(); CombineArea();

            // 8. Shared & Exterior Dressing
            area = Group("Lighthouse/Shared/CoastDressing"); Coast(); CombineArea();
            var aliasCoast = Group("Coast Dressing");
            aliasCoast.gameObject.SetActive(true);

            var radioAnchor=GameObject.Find("Radio"); if(radioAnchor && radioAnchor.GetComponent<Renderer>())radioAnchor.GetComponent<Renderer>().enabled=false;
            var wardrobe=GameObject.Find("keeper-wardrobe"); if(wardrobe)wardrobe.transform.position=new(-11.2f,1,-3.2f);
            var cab=GameObject.Find("lantern-cabinet"); 
            if(cab)
            {
                cab.transform.position = new Vector3(0.0f, 26.50f, 2.10f);
                var bc = cab.GetComponent<BoxCollider>();
                if (bc)
                {
                    bc.center = Vector3.zero;
                    bc.size = new Vector3(0.85f, 1.80f, 0.55f);
                }
            }

            // Relocate Hiding Spots and Gameplay Anchors into designated protected zones
            var crank = GameObject.Find("Crank Station");
            if (crank)
            {
                crank.transform.position = new Vector3(-1.8f, 25.60f, 1.2f);
                var bc = crank.GetComponent<BoxCollider>();
                if (bc)
                {
                    bc.size = new Vector3(0.8f, 1.1f, 0.8f);
                    bc.center = new Vector3(0, 0.55f, 0);
                }
            }
            var lamp = GameObject.Find("Lamp");
            if (lamp) lamp.transform.position = new Vector3(0, 26.70f, 0);

            var l2Desk = GameObject.Find("level2-desk");
            if (l2Desk) l2Desk.transform.position = new Vector3(0.0f, 3.2f, 1.05f);

            var upLocker = GameObject.Find("upper-locker");
            if (upLocker) upLocker.transform.position = new Vector3(0.0f, 9.6f, 1.2f);

            var craneBed = GameObject.Find("crane-bed");
            if (craneBed) craneBed.transform.position = new Vector3(-0.2f, 9.6f, 0.6f);

            foreach(string name in new[]{"Table","Hot Meal","Steam placeholder"})
            {
                var obj=GameObject.Find(name);
                if(obj)
                {
                    var p=new Vector3(-9.5f, name=="Table"?0.42f:name=="Hot Meal"?0.88f:1.05f, -7.32f);
                    obj.transform.position=p;
                    var mr=obj.GetComponent<MeshRenderer>();
                    if(mr)mr.enabled=false;
                }
            }
            Lighting();
            ReferenceWeatherAuthoring.ApplyToOpenScene();
            Debug.Log($"Reference visuals applied to {act}; gameplay anchors retained.");
        }

        static Transform Group(string path)
        {
            string[] parts = path.Split('/');
            Transform current = root;
            foreach (var part in parts)
            {
                Transform child = current.Find(part);
                if (child == null)
                {
                    var go = new GameObject(part);
                    go.transform.SetParent(current, false);
                    child = go.transform;
                }
                current = child;
            }
            return current;
        }
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
                // South Grand Main Entrance (angle 180°)
                if(level==0 && row<2 && Mathf.Abs(Mathf.DeltaAngle(angle,180))<11)continue;
                // West Cottage Connecting Portal (angle -120° / 240°)
                if(level==0 && row<2 && Mathf.Abs(Mathf.DeltaAngle(angle,-120))<11)continue;
                // Top Lantern arrival portal at 50°
                if(level==7 && row<2 && Mathf.Abs(Mathf.DeltaAngle(angle,50))<20)continue;
                // Upper tower window embrasures
                if(level>=1 && level<7 && row==1 && (Mathf.Abs(Mathf.DeltaAngle(angle,180))<6 || Mathf.Abs(Mathf.DeltaAngle(angle,0))<6))continue;
                Vector3 P(float theta,float y){
                    float outerR=Mathf.Lerp(6.50f,4.80f,y/25.6f);
                    float wallThick=Mathf.Lerp(0.85f,0.55f,y/25.6f);
                    float r=outerR-(side==1?wallThick:0);
                    return new(Mathf.Sin(theta)*r,y,Mathf.Cos(theta)*r);
                }
                int k=vertices.Count;vertices.AddRange(new[]{P(a,y0),P(a,y1),P(b,y1),P(b,y0)});
                Vector3 N(float theta)=>new Vector3(Mathf.Sin(theta),.0586f,Mathf.Cos(theta)).normalized*(side==0?1:-1);
                normals.AddRange(new[]{N(a),N(a),N(b),N(b)});uv.AddRange(new[]{new Vector2(a*2,y0*.5f),new Vector2(a*2,y1*.5f),new Vector2(b*2,y1*.5f),new Vector2(b*2,y0*.5f)});
                triangles.AddRange(side==0?new[]{k+2,k+1,k,k+3,k+2,k}:new[]{k,k+1,k+2,k,k+2,k+3});
            }
            var mesh=new Mesh{vertices=vertices.ToArray(),normals=normals.ToArray(),uv=uv.ToArray(),triangles=triangles.ToArray()};
            var go=new GameObject("Tapered Masonry",typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(area);go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<Renderer>().sharedMaterial=towerStone??plaster;go.AddComponent<MeshCollider>().sharedMesh=mesh;
        }

        static void House()
        {
            // Sited strictly on the West flank: x in [-12.5, -5.2], z in [-9.0, -1.0]
            // Leaving x in [-5.2, +7.0], z in [-12.0, -6.5] completely open for the Grand Tower Entrance and courtyard!
            // South living/dining room floor: z in [-9.0, -5.0]
            Box("Timber Floor South",new(-8.85f,.06f,-7.0f),new(7.3f,.18f,4.0f),wood,true);
            // North kitchen floor flanking cellar stairwell (stair wellhole x in [-8.8, -7.2], z in [-5.0, -1.0])
            Box("Timber Floor North West",new(-10.65f,.06f,-3.0f),new(3.7f,.18f,4.0f),wood,true);
            Box("Timber Floor North East",new(-6.20f,.06f,-3.0f),new(2.0f,.18f,4.0f),wood,true);
            Box("Timber Ceiling",new(-8.85f,2.95f,-5.0f),new(7.3f,.18f,8.0f),wood,true);
            Box("West Wall",new(-12.5f,1.5f,-5.0f),new(.3f,3f,8.0f),towerStone??plaster,true);
            Box("East Wall",new(-5.2f,1.5f,-6.5f),new(.3f,3f,5.0f),towerStone??plaster,true);
            Box("North Wall",new(-8.85f,1.5f,-1.0f),new(7.3f,3f,.3f),towerStone??plaster,true);

            // Front stone cottage facade with standardized human doorway opening (width = 1.20m, height = 2.30m, Ref: Screenshot 18)
            Box("Front West",new(-10.80f,1.5f,-9.0f),new(3.40f,3.0f,.30f),towerStone??plaster,true);
            Box("Front East",new(-6.55f,1.5f,-9.0f),new(2.70f,3.0f,.30f),towerStone??plaster,true);
            Box("Front Lintel",new(-8.50f,2.65f,-9.0f),new(1.20f,.70f,.30f),towerStone??plaster,true);

            // Finished connecting hallway linking rear of cottage to tower base west portal (Ref: Task 4)
            Box("Corridor Floor",new(-5.50f,.06f,-3.25f),new(1.0f,.18f,1.80f),wood,true);
            Box("Corridor Ceiling",new(-5.50f,2.95f,-3.25f),new(1.0f,.18f,1.80f),wood,true);
            Box("Corridor Wall North",new(-5.50f,1.5f,-2.35f),new(1.0f,3.0f,.25f),towerStone??plaster,true);
            Box("Corridor Wall South",new(-5.50f,1.5f,-4.15f),new(1.0f,3.0f,.25f),towerStone??plaster,true);

            // Cottage internal room partition dividing North Kitchen (z in [-5, -1]) and South Dining/Office (z in [-9, -5])
            Box("Cottage Partition West",new(-11.15f,1.5f,-5.0f),new(2.7f,3f,.2f),plaster,true);
            Box("Cottage Partition East",new(-6.0f,1.5f,-5.0f),new(2.4f,3f,.2f),plaster,true);
            Box("Cottage Partition Lintel",new(-8.5f,2.75f,-5.0f),new(2.8f,.5f,.2f),wood);
            Box("Cottage Door Jamb North",new(-7.5f,1.4f,-5.0f),new(.16f,2.6f,.24f),wood,true);
            Box("Cottage Door Jamb South",new(-9.5f,1.4f,-5.0f),new(.16f,2.6f,.24f),wood,true);

            // Ceiling exposed timber beams
            for(int i=0;i<4;i++){float x=-11.3f+i*2.2f;Box("Ceiling Beam",new(x,2.77f,-5.0f),new(.2f,.28f,7.8f),wood);}

            // Gabled slate roof over keeper cottage
            float pitch=27;
            Box("Slate Roof South",new(-8.85f,4.05f,-7.0f),new(7.6f,.2f,4.8f),roof,true).transform.rotation=Quaternion.Euler(-pitch,0,0);
            Box("Slate Roof North",new(-8.85f,4.05f,-3.0f),new(7.6f,.2f,4.8f),roof,true).transform.rotation=Quaternion.Euler(pitch,0,0);
            Box("Roof Ridge",new(-8.85f,5.16f,-5.0f),new(7.6f,.18f,.22f),iron);

            Triangle("West Gable",new(-12.5f,3f,-9f),new(-12.5f,5.2f,-5f),new(-12.5f,3f,-1f),towerStone??plaster);
            Triangle("East Gable",new(-5.2f,3f,-9f),new(-5.2f,5.2f,-5f),new(-5.2f,3f,-1f),towerStone??plaster);
            Rod("West Gable Trim L",new(-12.5f,3f,-9.2f),new(-12.5f,5.24f,-5f),.16f,wood);
            Rod("West Gable Trim R",new(-12.5f,5.24f,-5f),new(-12.5f,3f,-.8f),.16f,wood);

            // Round Attic Porthole Window on West Gable
            Cylinder("Attic Porthole Frame", new(-12.52f, 4.15f, -5.0f), .44f, .10f, iron, new(0, 0, 90));
            Cylinder("Attic Porthole Glass", new(-12.53f, 4.15f, -5.0f), .38f, .04f, warmWindow, new(0, 0, 90));
            Rod("Attic Porthole Mullion V", new(-12.54f, 3.75f, -5.0f), new(-12.54f, 4.55f, -5.0f), .04f, iron);
            Rod("Attic Porthole Mullion H", new(-12.54f, 4.15f, -5.40f), new(-12.54f, 4.15f, -4.60f), .04f, iron);

            // Brick kitchen chimney
            Box("Chimney",new(-10.5f,4.6f,-3.0f),new(.65f,2.2f,.7f),plaster);
            Box("Chimney Crown",new(-10.5f,5.7f,-3.0f),new(.85f,.16f,.9f),iron);

            // Cottage windows
            Window(new(-10.5f,1.65f,-9.18f),1.15f,1.45f,false,true);
            Window(new(-6.5f,1.65f,-9.18f),1.15f,1.45f,false,true);
            Window(new(-8.8f,1.8f,-1.18f),1.9f,1.7f,true,false);

            // Cottage porch carriage lantern
            Box("Porch Lantern Bracket", new(-8.5f, 2.78f, -9.4f), new(.06f, .16f, .22f), iron);
            Box("Porch Lantern Hood", new(-8.5f, 2.82f, -9.6f), new(.24f, .06f, .24f), iron);
            Box("Porch Lantern Chimney", new(-8.5f, 2.68f, -9.6f), new(.18f, .24f, .18f), warmWindow);
            Box("Porch Lantern Base", new(-8.5f, 2.54f, -9.6f), new(.20f, .05f, .20f), iron);

            Rod("Rain Gutter S",new(-12.7f,3f,-9.25f),new(-5.0f,3f,-9.25f),.13f,iron);
            Rod("Rain Gutter N",new(-12.7f,3f,-0.75f),new(-5.0f,3f,-0.75f),.13f,iron);
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
            Vector3 desk = new(-6.8f, 0, -7.0f);
            Box("Office Floor Cloth", desk + new Vector3(.2f, .072f, -.6f), new(2.5f, .008f, 2.2f), dark);
            Table(desk, 2.2f, 1.05f);
            var radio = GameObject.Find("Radio"); if (radio) radio.transform.position = desk + new Vector3(.4f, 1.13f, 0);
            Box("Analogue Radio Case", desk + new Vector3(.42f, 1.14f, 0), new(.8f, .36f, .38f), iron);
            Box("Radio Face", desk + new Vector3(.42f, 1.14f, -.20f), new(.75f, .29f, .025f), dark);
            Box("Tuning Scale", desk + new Vector3(.32f, 1.2f, -.22f), new(.42f, .075f, .016f), warm);
            for (int i = 0; i < 9; i++) Box("Radio Dial Tick", desk + new Vector3(.14f + i * .045f, 1.2f, -.231f), new(.009f, .05f, .01f), glass);
            for (int i = 0; i < 3; i++) Cylinder("Radio Knob", desk + new Vector3(.18f + i * .22f, 1.06f, -.23f), .04f, .04f, iron, new(90, 0, 0));
            Box("Recorder", desk + new Vector3(-.45f, 1.04f, .05f), new(.65f, .16f, .32f), dark);
            for (int i = 0; i < 2; i++) Cylinder("Tape Reel", desk + new Vector3(-.62f + i * .34f, 1.14f, .05f), .13f, .025f, iron, Vector3.zero);
            Cylinder("Lamp Base", desk + new Vector3(-.9f, 1.02f, .12f), .16f, .05f, iron, Vector3.zero);
            Rod("Lamp Arm 1", desk + new Vector3(-.9f, 1.04f, .12f), desk + new Vector3(-.92f, 1.62f, .18f), .035f, iron);
            Rod("Lamp Arm 2", desk + new Vector3(-.92f, 1.62f, .18f), desk + new Vector3(-.60f, 1.83f, .1f), .04f, iron);
            Cylinder("Task Shade", desk + new Vector3(-.58f, 1.76f, .09f), .2f, .16f, warm, new(0, 0, -20));
            // Vintage typewriter beside radio
            Box("Typewriter Base", desk + new Vector3(-.02f, 1.01f, -.25f), new(.42f, .09f, .38f), dark);
            Box("Typewriter Carriage", desk + new Vector3(-.02f, 1.07f, -.36f), new(.48f, .08f, .14f), iron);
            Box("Typewriter Paper Sheet", desk + new Vector3(-.02f, 1.14f, -.34f), new(.32f, .18f, .01f), parchment);
            Box("Logbook", desk + new Vector3(-.3f, .96f, -.3f), new(.4f, .035f, .26f), warm).transform.rotation = Quaternion.Euler(0, 12, 0);
            Vector3 chair = desk + new Vector3(.3f, 0, -1.0f);
            Box("Chair Seat", chair + Vector3.up * .54f, new(.52f, .09f, .52f), wood);
            for (int x = -1; x <= 1; x += 2) for (int z = -1; z <= 1; z += 2) Box("Chair Leg", chair + new Vector3(x * .21f, .27f, z * .21f), new(.06f, .54f, .06f), wood);
            for (int x = -1; x <= 1; x += 2) Box("Chair Back Upright", chair + new Vector3(x * .21f, .94f, -.22f), new(.06f, .9f, .06f), wood);
            Box("Chair Back", chair + new Vector3(0, 1.22f, -.22f), new(.48f, .2f, .06f), wood);
            Box("Framed Chart", new(-5.35f, 1.95f, -6.8f), new(.06f, .7f, .5f), iron);
            Box("Chart Paper", new(-5.38f, 1.95f, -6.8f), new(.015f, .62f, .43f), warm);
            Rod("Chart Lighthouse", new(-5.39f, 1.7f, -6.8f), new(-5.39f, 2.18f, -6.8f), .04f, iron);
            // Framed photo on wall
            Box("Keeper Lighthouse Photo Frame", new(-6.8f, 2.1f, -8.85f), new(.48f, .62f, .05f), dark);
            Box("Keeper Lighthouse Photo Canvas", new(-6.8f, 2.1f, -8.83f), new(.40f, .54f, .015f), parchment);
        }

        static void KitchenCountersAndStove()
        {
            // Base Kitchen Cabinetry & Countertop along West wall (x = -12.1m, depth 0.70m, height 0.90m)
            Vector3 counterPos = new(-12.05f, .45f, -3.5f);
            Box("Kitchen Base Cabinets", counterPos, new(.70f, .86f, 2.4f), wood, true);
            Box("Kitchen Countertop", counterPos + Vector3.up * .45f, new(.76f, .06f, 2.5f), dark, true);

            // Cabinet doors, drawers and brass pulls across 3 bays
            for (int b = 0; b < 3; b++)
            {
                float bz = -4.2f + b * .70f;
                Box($"Base Door Panel {b}", new(-11.68f, .38f, bz), new(.02f, .58f, .64f), wood);
                Box($"Base Door Recess {b}", new(-11.675f, .38f, bz), new(.01f, .46f, .52f), dark);
                Cylinder($"Base Door Pull {b}", new(-11.66f, .55f, bz + .20f), .025f, .03f, brass, new(0, 0, 90));
                Box($"Base Drawer Panel {b}", new(-11.68f, .76f, bz), new(.02f, .15f, .64f), wood);
                Box($"Base Drawer Pull {b}", new(-11.66f, .76f, bz), new(.02f, .03f, .10f), brass);
            }

            // Kitchen Wash Sink & Brass Gooseneck Faucet
            Box("Wash Sink Basin", new(-12.05f, .88f, -3.5f), new(.48f, .02f, .60f), dark);
            Box("Wash Sink Rim N", new(-12.05f, .91f, -3.18f), new(.50f, .05f, .04f), iron);
            Box("Wash Sink Rim S", new(-12.05f, .91f, -3.82f), new(.50f, .05f, .04f), iron);
            Box("Wash Sink Rim Back", new(-12.31f, .91f, -3.5f), new(.04f, .05f, .64f), iron);
            Box("Wash Sink Rim Front", new(-11.79f, .91f, -3.5f), new(.04f, .05f, .64f), iron);

            // Brass curved gooseneck faucet
            Rod("Faucet Spout Vertical", new(-12.28f, .93f, -3.5f), new(-12.28f, 1.18f, -3.5f), .03f, brass);
            Rod("Faucet Spout Arch", new(-12.28f, 1.18f, -3.5f), new(-12.16f, 1.22f, -3.5f), .028f, brass);
            Rod("Faucet Spout Drop", new(-12.16f, 1.22f, -3.5f), new(-12.13f, 1.15f, -3.5f), .025f, brass);
            Cylinder("Hot Water Tap", new(-12.28f, .95f, -3.62f), .03f, .04f, brass, Vector3.zero);
            Cylinder("Cold Water Tap", new(-12.28f, .95f, -3.38f), .03f, .04f, brass, Vector3.zero);

            // Countertop prep accessories
            Box("Oak Chopping Board", new(-12.02f, .94f, -4.2f), new(.34f, .035f, .40f), wood);
            Cylinder("Ceramic Prep Bowl", new(-12.02f, .99f, -2.7f), .13f, .12f, ceramic, Vector3.zero);
            Cylinder("Ceramic Pitcher", new(-11.92f, 1.05f, -4.4f), .10f, .24f, ceramic, Vector3.zero);

            // Upper Hanging Kitchen Cabinets
            Vector3 upperPos = new(-12.22f, 2.22f, -3.5f);
            Box("Kitchen Upper Cabinets", upperPos, new(.36f, .84f, 2.4f), wood, true);
            Box("Upper Crown Molding", upperPos + Vector3.up * .44f, new(.40f, .06f, 2.5f), wood);
            for (int b = 0; b < 3; b++)
            {
                float bz = -4.2f + b * .70f;
                Box($"Upper Door Panel {b}", new(-12.02f, 2.22f, bz), new(.02f, .76f, .64f), wood);
                Box($"Upper Door Recess {b}", new(-12.015f, 2.22f, bz), new(.01f, .64f, .52f), dark);
                Cylinder($"Upper Door Knob {b}", new(-12.00f, 1.95f, bz + .22f), .02f, .025f, brass, new(0, 0, 90));
            }

            // Top display shelf with ceramic dinner plates and spice jars
            for (int p = 0; p < 3; p++)
            {
                float pz = -4.1f + p * .65f;
                Cylinder($"Display Plate {p}", new(-12.20f, 2.82f, pz), .13f, .015f, ceramic, new(0, 0, 78));
            }
            for (int j = 0; j < 3; j++)
            {
                float jz = -2.7f + j * .30f;
                Cylinder($"Spice Jar {j}", new(-12.20f, 2.76f, jz), .055f, .12f, ceramic, Vector3.zero);
                Cylinder($"Spice Lid {j}", new(-12.20f, 2.83f, jz), .06f, .025f, wood, Vector3.zero);
            }

            // Under-cabinet warm task lighting strip
            Box("Under Cabinet Light Strip", new(-12.22f, 1.78f, -3.5f), new(.28f, .02f, 2.2f), warmWindow);

            // Traditional Cast-Iron Cooking Range / Oven Stove
            Vector3 stovePos = new(-12.05f, .46f, -1.80f);
            Box("Stove Cast Body", stovePos, new(.76f, .88f, .90f), iron, true);
            Box("Stove Cooktop", stovePos + Vector3.up * .45f, new(.80f, .04f, .94f), iron, true);
            for (int sx = -1; sx <= 1; sx += 2) for (int sz = -1; sz <= 1; sz += 2)
                Cylinder("Stove Burner Ring", stovePos + new Vector3(sx * .18f, .48f, sz * .22f), .11f, .015f, iron, Vector3.zero);

            // Oven door with glowing warm fire/embers
            Box("Oven Door Cast Frame", stovePos + new Vector3(.39f, -.02f, 0), new(.04f, .56f, .76f), iron);
            Box("Oven Glowing Fire", stovePos + new Vector3(.40f, -.02f, 0), new(.02f, .34f, .50f), stoveFire);
            Box("Oven Door Handle", stovePos + new Vector3(.42f, .20f, 0), new(.03f, .03f, .32f), brass);

            // Simmering Cast-Iron Stew Pot on front burner
            Vector3 potPos = stovePos + new Vector3(.18f, .59f, -.22f);
            Cylinder("Stovetop Stew Pot", potPos, .20f, .22f, iron, Vector3.zero);
            Cylinder("Stew Pot Lid", potPos + Vector3.up * .12f, .21f, .03f, brass, Vector3.zero);
            Cylinder("Stew Pot Handle L", potPos + new Vector3(0, .04f, -.22f), .03f, .04f, brass, new(90, 0, 0));
            Cylinder("Stew Pot Handle R", potPos + new Vector3(0, .04f, .22f), .03f, .04f, brass, new(90, 0, 0));

            // Range Hood and Flue
            Box("Range Hood Base Rim", stovePos + new Vector3(0, 1.42f, 0), new(.82f, .06f, 1.0f), iron);
            Box("Range Hood Taper 1", stovePos + new Vector3(0, 1.55f, 0), new(.68f, .20f, .82f), iron);
            Box("Range Hood Taper 2", stovePos + new Vector3(0, 1.75f, 0), new(.48f, .20f, .58f), iron);
            Box("Range Hood Taper 3", stovePos + new Vector3(0, 1.95f, 0), new(.32f, .20f, .36f), iron);
            Cylinder("Stove Chimney Flue", stovePos + new Vector3(0, 2.45f, 0), .14f, .95f, iron, Vector3.zero);
        }

        static void DiningRoom()
        {
            // Circular Wooden Dining Table (matching Image 7)
            Vector3 tablePos = new(-9.5f, 0, -7.0f);
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

            // Ceramic Flower Vase with Sunflowers
            Vector3 vasePos = tablePos + new Vector3(.18f, .86f, -.16f);
            Cylinder("Sunflower Vase Base", vasePos + Vector3.up * .02f, .09f, .04f, ceramic, Vector3.zero);
            Cylinder("Sunflower Vase Belly", vasePos + Vector3.up * .12f, .11f, .16f, ceramic, Vector3.zero);
            Cylinder("Sunflower Vase Neck", vasePos + Vector3.up * .23f, .065f, .08f, ceramic, Vector3.zero);

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

            // Chairs
            Vector3 chair1 = tablePos + new Vector3(0, 0, .85f);
            Box("Chair 1 Seat", chair1 + Vector3.up * .46f, new(.48f, .05f, .46f), wood, true);
            for (int x = -1; x <= 1; x += 2) for (int z = -1; z <= 1; z += 2)
                Box("Chair 1 Leg", chair1 + new Vector3(x * .18f, .23f, z * .18f), new(.05f, .46f, .05f), wood);
            Rod("Chair 1 Back Post L", chair1 + new Vector3(-.19f, .48f, .19f), chair1 + new Vector3(-.19f, 1.15f, .19f), .045f, wood);
            Rod("Chair 1 Back Post R", chair1 + new Vector3(.19f, .48f, .19f), chair1 + new Vector3(.19f, 1.15f, .19f), .045f, wood);
            Box("Chair 1 Crest Rail", chair1 + new Vector3(0, 1.12f, .19f), new(.45f, .08f, .04f), wood);
            for (int s = -1; s <= 1; s++)
                Rod($"Chair 1 Slat {s}", chair1 + new Vector3(s * .10f, .48f, .19f), chair1 + new Vector3(s * .10f, 1.08f, .19f), .025f, wood);

            Vector3 chair2 = tablePos + new Vector3(.85f, 0, 0);
            Box("Chair 2 Seat", chair2 + Vector3.up * .46f, new(.46f, .05f, .48f), wood, true);
            for (int x = -1; x <= 1; x += 2) for (int z = -1; z <= 1; z += 2)
                Box("Chair 2 Leg", chair2 + new Vector3(x * .18f, .23f, z * .18f), new(.05f, .46f, .05f), wood);
            Rod("Chair 2 Back Post L", chair2 + new Vector3(.19f, .48f, -.19f), chair2 + new Vector3(.19f, 1.15f, -.19f), .045f, wood);
            Rod("Chair 2 Back Post R", chair2 + new Vector3(.19f, .48f, .19f), chair2 + new Vector3(.19f, 1.15f, .19f), .045f, wood);
            Box("Chair 2 Crest Rail", chair2 + new Vector3(.19f, 1.12f, 0), new(.04f, .08f, .45f), wood);
            for (int s = -1; s <= 1; s++)
                Rod($"Chair 2 Slat {s}", chair2 + new Vector3(.19f, .48f, s * .10f), chair2 + new Vector3(.19f, 1.08f, s * .10f), .025f, wood);

            // Rustic Wooden Wagon Wheel
            Vector3 wheelPos = new(-12.2f, .46f, -6.0f);
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

            // Upright Oak Barrel in corner
            Barrel("Kitchen Corner Barrel", new(-12.0f, .45f, -8.4f), Vector3.zero, .34f, .88f);

            // Striped Rug under dining table
            Box("Kitchen Striped Rug", tablePos + new Vector3(0, .072f, 0), new(2.2f, .01f, 2.2f), stripedRug);

            // Overhead Drop Pendant Lamp
            Vector3 lampDrop = new(-9.5f, 2.9f, -7.0f);
            Rod("Kitchen Lamp Cord", lampDrop, lampDrop + Vector3.down * .65f, .02f, iron);
            Cylinder("Kitchen Lamp Shade", lampDrop + Vector3.down * .70f, .30f, .14f, iron, Vector3.zero);
            Cylinder("Kitchen Lamp Bulb", lampDrop + Vector3.down * .76f, .09f, .07f, warm, Vector3.zero);
        }

        static void Kitchen()
        {
            KitchenCountersAndStove();
            DiningRoom();
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
        static readonly (float startDeg, float endDeg, float yStart, float yEnd)[] flights = new[]
        {
            (100f, -110f, 0.15f, 3.20f),     // Flight 0: Vestibule (y=0.15) to Library (y=3.20), North arc
            (-110f, -310f, 3.20f, 6.40f),    // Flight 1: Library (y=3.20) to Watch Room (y=6.40), South arc
            (50f, -110f, 6.40f, 9.60f),      // Flight 2: Watch Room (y=6.40) to Crane's Quarters (y=9.60), North arc
            (-110f, -310f, 9.60f, 12.80f),   // Flight 3: Crane's Quarters (y=9.60) to Storeroom (y=12.80), South arc
            (50f, -110f, 12.80f, 16.00f),    // Flight 4: Storeroom (y=12.80) to Service Landing (y=16.00), North arc
            (-110f, -310f, 16.00f, 19.20f),  // Flight 5: Service Landing (y=16.00) to Machine Room (y=19.20), South arc
            (50f, -110f, 19.20f, 22.40f),    // Flight 6: Machine Room (y=19.20) to Lantern Service (y=22.40), North arc
            (-110f, -310f, 22.40f, 25.60f)   // Flight 7: Lantern Service (y=22.40) to Lantern Gallery Deck (y=25.60), South arc
        };

        static void BuildSingleFlight(int f, float stairWidth = 1.25f, int stepsPerFlight = 20)
        {
            var fl = flights[f];
            BuildFlightRampCollider(f, fl.startDeg, fl.endDeg, fl.yStart, fl.yEnd, stairWidth, stepsPerFlight);

            for (int s = 0; s < stepsPerFlight; s++)
            {
                float t0 = s / (float)stepsPerFlight;
                float t1 = (s + 1) / (float)stepsPerFlight;

                float degA = Mathf.Lerp(fl.startDeg, fl.endDeg, t0);
                float degB = Mathf.Lerp(fl.startDeg, fl.endDeg, t1);
                float a = degA * Mathf.Deg2Rad;
                float b = degB * Mathf.Deg2Rad;

                float y0 = Mathf.Lerp(fl.yStart, fl.yEnd, t0);
                float y1 = Mathf.Lerp(fl.yStart, fl.yEnd, t1);

                float wallR = Mathf.Lerp(5.65f, 4.25f, y1 / 25.6f);
                float outer = wallR - 0.02f;
                float inner = outer - stairWidth;

                // Solid cast iron horizontal tread (visual only, collision handled smoothly by ramp collider)
                Wedge("Iron Tread", inner, outer, a, b, y1, .055f, iron, false);

                // Solid vertical riser closing the gap between steps
                WedgeRiser("Stair Riser", inner, outer, a, y0, y1, .035f, iron);

                // Solid nosing lip along front edge
                Vector3 nosingInner = new(Mathf.Sin(a) * inner, y1 + .01f, Mathf.Cos(a) * inner);
                Vector3 nosingOuter = new(Mathf.Sin(a) * outer, y1 + .01f, Mathf.Cos(a) * outer);
                Rod("Tread Nosing", nosingInner, nosingOuter, .032f, iron);

                // Inner balustrade & sweeping handrail
                Vector3 railA = new(Mathf.Sin(a) * inner, y1 + 1.05f, Mathf.Cos(a) * inner);
                Vector3 railB = new(Mathf.Sin(b) * inner, y1 + 1.05f + (y1 - y0), Mathf.Cos(b) * inner);
                Rod("Baluster", new(Mathf.Sin(a) * inner, y1, Mathf.Cos(a) * inner), railA, .038f, iron);
                Rod("Following Handrail", railA, railB, .055f, iron);

                // Wall cantilever bracket anchoring into masonry wall
                if (s % 2 == 0)
                {
                    Vector3 bInner = new(Mathf.Sin(b) * (inner + 0.15f), y1 - .04f, Mathf.Cos(b) * (inner + 0.15f));
                    Vector3 bOuter = new(Mathf.Sin(b) * (outer + .25f), y1 - .12f, Mathf.Cos(b) * (outer + .25f));
                    Rod("Cantilever Wall Bracket", bInner, bOuter, .048f, iron);
                }
            }
        }

        static void StaircaseA()
        {
            float stairWidth = 1.25f;
            float baseAngle = 100f * Mathf.Deg2Rad;
            float outerLevel0 = 5.63f;
            float innerLevel0 = outerLevel0 - stairWidth;

            // Solid stone starter curb and iron newel post anchoring foot of spiral stairs on East wall
            Vector3 curbPos = new(Mathf.Sin(baseAngle)*(outerLevel0 - 0.55f), .08f, Mathf.Cos(baseAngle)*(outerLevel0 - 0.55f));
            Box("Spiral Stair Starter Curb", curbPos, new(stairWidth + .12f, .16f, .44f), ground, true);
            Vector3 newelPos = new(Mathf.Sin(baseAngle)*innerLevel0, .14f, Mathf.Cos(baseAngle)*innerLevel0);
            Rod("Stair Starter Newel", newelPos, newelPos + Vector3.up * 1.11f, .075f, iron);
            Cylinder("Stair Starter Newel Base", newelPos + Vector3.up * .01f, .12f, .03f, iron, Vector3.zero);

            BuildSingleFlight(0, stairWidth);
        }

        static void StaircaseB()
        {
            BuildSingleFlight(1);
        }

        static void StaircaseC()
        {
            BuildSingleFlight(2);
        }

        static void UpperTowerStairs()
        {
            for (int f = 3; f <= 7; f++)
            {
                BuildSingleFlight(f);
            }
        }

        static void Stairs()
        {
            StaircaseA();
            StaircaseB();
            StaircaseC();
            UpperTowerStairs();
        }

        static void BuildFlightRampCollider(int f, float startDeg, float endDeg, float yStart, float yEnd, float stairWidth, int steps)
        {
            var rampGo = new GameObject($"Stair Ramp Collider Flight {f}");
            rampGo.transform.SetParent(area);
            rampGo.tag = "Metal";

            var points = new List<Vector3>();
            var tris = new List<int>();

            Vector3[] prevPts = null;
            for (int s = 0; s <= steps; s++)
            {
                float t = s / (float)steps;
                float deg = Mathf.Lerp(startDeg, endDeg, t);
                float rad = deg * Mathf.Deg2Rad;
                float y = Mathf.Lerp(yStart, yEnd, t);
                float wallR = Mathf.Lerp(5.65f, 4.25f, y / 25.6f);
                float outer = wallR - 0.02f;
                float inner = outer - stairWidth;

                Vector3 pIn = new Vector3(Mathf.Sin(rad) * inner, y, Mathf.Cos(rad) * inner);
                Vector3 pOut = new Vector3(Mathf.Sin(rad) * outer, y, Mathf.Cos(rad) * outer);
                Vector3[] currPts = new[] { pIn, pOut };

                if (prevPts != null)
                {
                    // Tread walking ramp surface: double-sided quad
                    int k = points.Count;
                    points.AddRange(new[] { prevPts[0], prevPts[1], currPts[1], currPts[0] });
                    tris.AddRange(new[] { k, k + 1, k + 2, k, k + 2, k + 3 });
                    int k2 = points.Count;
                    points.AddRange(new[] { currPts[0], currPts[1], prevPts[1], prevPts[0] });
                    tris.AddRange(new[] { k2, k2 + 1, k2 + 2, k2, k2 + 2, k2 + 3 });

                    // Inner guard rail (double-sided, 1.25m height along inner edge to prevent falling)
                    // Stops 3 steps before landing (s < steps - 2) so the exit gate onto the room floor is completely open!
                    if (s < steps - 2)
                    {
                        Vector3 inG0 = prevPts[0] + Vector3.up * 1.25f;
                        Vector3 inG1 = currPts[0] + Vector3.up * 1.25f;
                        int kg = points.Count;
                        points.AddRange(new[] { prevPts[0], inG0, inG1, currPts[0] });
                        tris.AddRange(new[] { kg, kg + 1, kg + 2, kg, kg + 2, kg + 3 });
                        int kg2 = points.Count;
                        points.AddRange(new[] { currPts[0], inG1, inG0, prevPts[0] });
                        tris.AddRange(new[] { kg2, kg2 + 1, kg2 + 2, kg2, kg2 + 2, kg2 + 3 });
                    }
                }
                prevPts = currPts;
            }

            // Smooth landing apron: extends 1.5 steps onto the floor landing at flat y = yEnd
            // Guarantees zero step-lip snag as player transitions from ramp onto floor surface
            float stepDegDelta = (endDeg - startDeg) / steps;
            float apronDeg = endDeg + stepDegDelta * 1.5f;
            float apronRad = apronDeg * Mathf.Deg2Rad;
            float wallRApron = Mathf.Lerp(5.65f, 4.25f, yEnd / 25.6f);
            float outerApron = wallRApron - 0.02f;
            float innerApron = outerApron - stairWidth;
            Vector3 apronIn = new Vector3(Mathf.Sin(apronRad) * innerApron, yEnd + 0.005f, Mathf.Cos(apronRad) * innerApron);
            Vector3 apronOut = new Vector3(Mathf.Sin(apronRad) * outerApron, yEnd + 0.005f, Mathf.Cos(apronRad) * outerApron);
            Vector3[] apronPts = new[] { apronIn, apronOut };

            int ka = points.Count;
            points.AddRange(new[] { prevPts[0], prevPts[1], apronPts[1], apronPts[0] });
            tris.AddRange(new[] { ka, ka + 1, ka + 2, ka, ka + 2, ka + 3 });
            int ka2 = points.Count;
            points.AddRange(new[] { apronPts[0], apronPts[1], prevPts[1], prevPts[0] });
            tris.AddRange(new[] { ka2, ka2 + 1, ka2 + 2, ka2, ka2 + 2, ka2 + 3 });

            var mesh = new Mesh { vertices = points.ToArray(), triangles = tris.ToArray() };
            mesh.RecalculateNormals();
            var mc = rampGo.AddComponent<MeshCollider>();
            mc.sharedMesh = Persist(mesh);
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
        static void WedgeRiser(string name, float inner, float outer, float angle, float yBottom, float yTop, float thick, Material material)
        {
            Vector3 pInnerBot = new Vector3(Mathf.Sin(angle) * inner, yBottom, Mathf.Cos(angle) * inner);
            Vector3 pOuterBot = new Vector3(Mathf.Sin(angle) * outer, yBottom, Mathf.Cos(angle) * outer);
            Vector3 pOuterTop = new Vector3(Mathf.Sin(angle) * outer, yTop, Mathf.Cos(angle) * outer);
            Vector3 pInnerTop = new Vector3(Mathf.Sin(angle) * inner, yTop, Mathf.Cos(angle) * inner);

            Vector3 normal = new Vector3(Mathf.Cos(angle), 0, -Mathf.Sin(angle));
            Vector3 offset = normal * (thick * 0.5f);

            var points = new List<Vector3>();
            var tris = new List<int>();

            void Quad(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
            {
                int k = points.Count;
                points.AddRange(new[] { p0, p1, p2, p3 });
                tris.AddRange(new[] { k, k + 1, k + 2, k, k + 2, k + 3 });
            }

            Quad(pInnerBot + offset, pOuterBot + offset, pOuterTop + offset, pInnerTop + offset);
            Quad(pInnerTop - offset, pOuterTop - offset, pOuterBot - offset, pInnerBot - offset);
            Quad(pInnerBot - offset, pInnerBot + offset, pInnerTop + offset, pInnerTop - offset);
            Quad(pOuterBot + offset, pOuterBot - offset, pOuterTop - offset, pOuterTop + offset);

            var mesh = new Mesh { vertices = points.ToArray(), triangles = tris.ToArray() };
            mesh.RecalculateNormals();
            mesh.uv = points.Select(p => new Vector2(p.x, p.y)).ToArray();
            var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(area);
            go.GetComponent<MeshFilter>().sharedMesh = Persist(mesh);
            go.GetComponent<Renderer>().sharedMaterial = material;
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
        static void VestibuleArchitecture()
        {
            // Continuous circular stone foundation plinth and flagstone floor (expanded 11.3m usable interior)
            Cylinder("Tower Stone Foundation", new(0, .07f, 0), 5.65f, .15f, ground, Vector3.zero);
            Cylinder("Tower Flagstone Floor", new(0, .145f, 0), 5.55f, .06f, ground, Vector3.zero);
            Cylinder("Entry Inlay Outer Ring", new(0, .150f, 0), 2.50f, .005f, brass, Vector3.zero);
            Cylinder("Entry Inlay Compass Center", new(0, .152f, 0), .65f, .006f, dark, Vector3.zero);

            // 8-Point Compass Rose Star Inlay in polished brass and dark slate
            for (int p = 0; p < 8; p++)
            {
                float ang = p * 45f * Mathf.Deg2Rad;
                float rLen = (p % 2 == 0) ? 2.40f : 1.60f;
                Vector3 tip = new(Mathf.Sin(ang) * rLen, .151f, Mathf.Cos(ang) * rLen);
                Vector3 baseL = new(Mathf.Sin(ang - 0.18f) * .35f, .151f, Mathf.Cos(ang - 0.18f) * .35f);
                Vector3 baseR = new(Mathf.Sin(ang + 0.18f) * .35f, .151f, Mathf.Cos(ang + 0.18f) * .35f);
                Triangle($"Compass Point L {p}", new(0, .151f, 0), tip, baseL, brass);
                Triangle($"Compass Point R {p}", new(0, .151f, 0), baseR, tip, dark);
            }

            // GRAND SOUTH MAIN ENTRANCE PORTAL at z = -6.50m (facing courtyard approach)
            Vector3 southPortal = new(0, 1.4f, -6.50f);
            Box("Grand Portal Left Jamb", southPortal + new Vector3(-1.15f, 0, 0), new(.45f, 2.7f, .85f), plaster, true);
            Box("Grand Portal Right Jamb", southPortal + new Vector3(1.15f, 0, 0), new(.45f, 2.7f, .85f), plaster, true);
            Box("Grand Portal Arched Header", southPortal + new Vector3(0, 1.35f, 0), new(2.75f, .45f, .90f), plaster, true);
            Box("Grand Entrance Threshold Sill", new(0, .14f, -6.50f), new(2.20f, .06f, .85f), wood, true);

            // Exterior Carriage Lanterns flanking Grand Main Entrance on courtyard facade
            for (int side = -1; side <= 1; side += 2)
            {
                Vector3 lPos = new(side * 1.55f, 2.35f, -6.55f);
                Box($"Entrance Sconce Bracket {side}", lPos, new(.08f, .22f, .18f), iron);
                Box($"Entrance Lantern Hood {side}", lPos + Vector3.back * .14f + Vector3.up * .12f, new(.24f, .06f, .24f), iron);
                Box($"Entrance Lantern Glass {side}", lPos + Vector3.back * .14f, new(.18f, .26f, .18f), warmWindow);
                Box($"Entrance Lantern Base {side}", lPos + Vector3.back * .14f + Vector3.down * .14f, new(.20f, .05f, .20f), iron);
            }

            // Architectural Stone Portal connecting Tower to Keeper's Cottage at West (x = -5.2m, z = -3.0m)
            Vector3 cottagePortal = new(-5.20f, 1.4f, -3.0f);
            Box("Cottage Link Portal Jamb L", cottagePortal + new Vector3(0, 0, -1.05f), new(.85f, 2.7f, .35f), plaster, true);
            Box("Cottage Link Portal Jamb R", cottagePortal + new Vector3(0, 0, 1.05f), new(.85f, 2.7f, .35f), plaster, true);
            Box("Cottage Link Arched Header", cottagePortal + new Vector3(0, 1.35f, 0), new(.90f, .45f, 2.45f), plaster, true);
            Box("Cottage Link Threshold", new(-5.20f, .14f, -3.0f), new(.85f, .06f, 1.95f), wood, true);
        }

        static void VestibuleEntrance()
        {
            // Coat peg rack along Southwest entrance wall (x in [-4.2, -2.4], z in [-4.2, -2.4])
            Vector3 rackPos = new(-3.6f, 1.85f, -3.4f);
            Box("Coat Rack Rail", rackPos, new(.08f, .14f, 2.4f), wood);
            for (int i = 0; i < 5; i++)
            {
                float z = -4.2f + i * .40f;
                Box($"Coat Hook {i}", new(-3.55f, 1.85f, z), new(.06f, .08f, .03f), brass);
            }

            // Iconic Yellow Oilskin Raincoat / Slicker on Hook 3 (z = -3.4)
            Box("Yellow Slicker Torso", new(-3.52f, 1.35f, -3.4f), new(.12f, .85f, .46f), yellowSlicker);
            Box("Yellow Slicker Sleeve L", new(-3.50f, 1.32f, -3.66f), new(.11f, .72f, .12f), yellowSlicker);
            Box("Yellow Slicker Sleeve R", new(-3.50f, 1.32f, -3.14f), new(.11f, .72f, .12f), yellowSlicker);
            Box("Yellow Slicker Hood", new(-3.50f, 1.86f, -3.4f), new(.14f, .22f, .24f), yellowSlicker);
            for (int b = 0; b < 4; b++) Box($"Slicker Button {b}", new(-3.45f, 1.15f + b * .16f, -3.4f), new(.02f, .02f, .02f), dark);

            // Dark Navy Wool Keeper's Greatcoat on Hook 2 (z = -3.8)
            Box("Navy Coat Torso", new(-3.52f, 1.30f, -3.85f), new(.14f, .95f, .50f), darkCoat);
            Box("Navy Coat Sleeve L", new(-3.50f, 1.28f, -4.14f), new(.12f, .80f, .13f), darkCoat);
            Box("Navy Coat Sleeve R", new(-3.50f, 1.28f, -3.56f), new(.12f, .80f, .13f), darkCoat);
            for (int b = 0; b < 4; b++)
            {
                Box($"Navy Button L {b}", new(-3.45f, 1.15f + b * .16f, -3.89f), new(.02f, .025f, .025f), brass);
                Box($"Navy Button R {b}", new(-3.45f, 1.15f + b * .16f, -3.81f), new(.02f, .025f, .025f), brass);
            }

            // Keeper Cap / Hat on Hook 4 (z = -3.0)
            Cylinder("Keeper Cap Crown", new(-3.50f, 1.95f, -3.0f), .14f, .08f, darkCoat, new(0, 0, 15));
            Cylinder("Keeper Cap Brim", new(-3.44f, 1.90f, -3.0f), .16f, .015f, darkCoat, new(0, 0, 35));

            // Overhead luggage shelf mounted above coat rack at y = 2.45
            Box("Vestibule Luggage Shelf", new(-3.60f, 2.45f, -3.4f), new(.38f, .05f, 2.4f), wood);
            Rod("Shelf Bracket 1", new(-3.70f, 2.25f, -4.3f), new(-3.45f, 2.45f, -4.3f), .035f, iron);
            Rod("Shelf Bracket 2", new(-3.70f, 2.25f, -3.4f), new(-3.45f, 2.45f, -3.4f), .035f, iron);
            Rod("Shelf Bracket 3", new(-3.70f, 2.25f, -2.5f), new(-3.45f, 2.45f, -2.5f), .035f, iron);
            Box("Vintage Suitcase 1", new(-3.55f, 2.62f, -3.8f), new(.32f, .30f, .55f), leatherLuggage);
            Cylinder("Vintage Hat Box", new(-3.55f, 2.60f, -3.0f), .18f, .24f, wood, Vector3.zero);

            // Wall gallery of vintage framed photographs along Southwest wall
            Box("Photo Frame 1", new(-3.60f, 1.85f, -2.0f), new(.04f, .52f, .42f), wood);
            Box("Photo Canvas 1", new(-3.57f, 1.85f, -2.0f), new(.015f, .44f, .34f), parchment);
            Box("Photo Frame 2", new(-3.60f, 1.25f, -2.0f), new(.04f, .34f, .48f), iron);
            Box("Photo Canvas 2", new(-3.57f, 1.25f, -2.0f), new(.015f, .28f, .42f), parchment);

            // Floor props: Weathered sea chest & boot stool
            Box("Sea Chest Body", new(-2.65f, .38f, -3.4f), new(.58f, .46f, .95f), wood, true);
            Box("Sea Chest Iron Straps", new(-2.65f, .38f, -3.4f), new(.34f, .48f, .97f), iron);
            Box("Sea Chest Brass Lock", new(-2.36f, .46f, -3.4f), new(.03f, .09f, .09f), brass);
            Box("Boot Stool Seat", new(-2.65f, .38f, -2.5f), new(.32f, .05f, .48f), wood, true);
            for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)
                Box("Boot Stool Leg", new(-2.65f+x*.10f, .20f, -2.5f+z*.15f), new(.05f, .35f, .05f), wood);
            Box("Keeper Boot L", new(-2.35f, .32f, -2.58f), new(.26f, .32f, .12f), dark);
            Box("Keeper Boot R", new(-2.35f, .32f, -2.42f), new(.26f, .32f, .12f), dark);

            // Overhead drop pendant lamp and wall sconce
            Rod("Vestibule Lamp Cord", new(0, 3.1f, 0), new(0, 2.45f, 0), .02f, iron);
            Cylinder("Vestibule Lamp Shade", new(0, 2.40f, 0), .35f, .14f, iron, Vector3.zero);
            Cylinder("Vestibule Lamp Bulb", new(0, 2.34f, 0), .10f, .07f, warm, Vector3.zero);
            Box("Wall Sconce Base", new(-3.10f, 2.0f, -4.1f), new(.04f, .22f, .10f), brass);
            Rod("Wall Sconce Arm", new(-3.10f, 1.95f, -4.1f), new(-2.95f, 2.05f, -4.1f), .03f, brass);
            Cylinder("Wall Sconce Lantern", new(-2.95f, 2.10f, -4.1f), .09f, .22f, glass, Vector3.zero);
            Cylinder("Wall Sconce Cap", new(-2.95f, 2.22f, -4.1f), .11f, .04f, brass, Vector3.zero);
        }

        static void VestibuleStorage()
        {
            // Dedicated Storage Alcove along Northwest curved wall (x in [-4.6, -2.2], z in [1.5, 4.2])
            Vector3 benchPos = new(-3.4f, 0, 2.6f);
            Box("Storage Workbench Top", benchPos + Vector3.up * .78f, new(1.4f, .07f, .75f), wood, true);
            Box("Storage Workbench Shelf", benchPos + Vector3.up * .22f, new(1.25f, .04f, .65f), wood);
            for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)
                Box("Storage Bench Leg", benchPos + new Vector3(x * .60f, .39f, z * .30f), new(.08f, .78f, .08f), wood);
            Box("Storage Bench Draped Cloth", benchPos + new Vector3(.1f, .55f, -.33f), new(.55f, .48f, .04f), darkCoat);
            Cylinder("Work Oil Can", benchPos + new Vector3(-.35f, .88f, .05f), .09f, .18f, iron, Vector3.zero);
            Rod("Oil Can Spout", benchPos + new Vector3(-.35f, .97f, .05f), benchPos + new Vector3(-.44f, 1.12f, .02f), .02f, brass);
            Box("Work Lantern", benchPos + new Vector3(.32f, .95f, .08f), new(.16f, .28f, .16f), brass);
            Box("Work Lantern Glass", benchPos + new Vector3(.32f, .95f, .08f), new(.12f, .20f, .12f), warm);

            // Vintage travel trunks neatly stowed under the workbench
            Box("Travel Trunk 1", benchPos + new Vector3(0, .14f, 0), new(.85f, .26f, .60f), leatherLuggage, true);
            Box("Trunk 1 Brass Latch", benchPos + new Vector3(0, .14f, -.31f), new(.09f, .06f, .03f), brass);

            // Barrels along the Northwest curved wall
            Barrel("Storage Paraffin Barrel 1", new(-4.2f, .38f, 1.8f), new(0, 0, 90), .36f, .88f);
            Barrel("Storage Paraffin Barrel 2", new(-4.2f, .38f, 3.2f), new(0, 0, 90), .36f, .88f);
            Barrel("Storage Barrel Stack Top", new(-4.2f, .82f, 2.5f), new(0, 0, 90), .35f, .85f);
            Barrel("Storage Upright Freshwater Cask", new(-3.2f, .45f, 3.8f), Vector3.zero, .36f, .92f);

            // Northeast carpenter bench and heavy toolchest (x in [2.5, 4.5], z in [2.2, 4.0])
            Vector3 carpPos = new(3.4f, 0, 3.0f);
            Box("Carpenter Bench Top", carpPos + Vector3.up * .80f, new(1.5f, .08f, .70f), wood, true);
            for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)
                Box("Carpenter Leg", carpPos + new Vector3(x * .65f, .40f, z * .28f), new(.08f, .80f, .08f), wood);
            Box("Iron Bench Vise", carpPos + new Vector3(-.55f, .92f, -.28f), new(.18f, .16f, .15f), iron);
            Box("Heavy Tool Chest", carpPos + new Vector3(.35f, .92f, 0), new(.55f, .22f, .35f), iron);

            // Wet floor puddle in storage alcove
            var puddle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            puddle.name = "Storage Wet Puddle";
            puddle.transform.SetParent(area, false);
            puddle.transform.position = new(-3.6f, .155f, 2.8f);
            puddle.transform.localScale = new(1.8f, .002f, 1.8f);
            puddle.GetComponent<Renderer>().sharedMaterial = dark;
            Object.DestroyImmediate(puddle.GetComponent<Collider>());
        }

        static void Vestibule()
        {
            VestibuleArchitecture();
            VestibuleEntrance();
            VestibuleStorage();
        }

        static void RoomFloor(int level, float y, float floorThick = 0.14f)
        {
            float wallR = Mathf.Lerp(5.65f, 4.25f, y / 25.6f);
            float stairWidth = 1.25f;
            float outerStairR = wallR - 0.02f;
            float innerStairR = outerStairR - stairWidth;

            bool isOddLevel = (level % 2 == 1);

            // Center Floor Cap covering wedge convergence point at (0, y, 0)
            Box("Room Center Floor Cap", new Vector3(0, y - floorThick * 0.5f, 0), new Vector3(0.25f, floorThick, 0.25f), wood, true);

            const int segs = 32;
            for (int i = 0; i < segs; i++)
            {
                float degMid = (i + 0.5f) * 360f / segs;
                if (degMid > 180f) degMid -= 360f; // [-180, 180]

                // Odd levels: flight arriving from below ascends along North arc (from +50° down to -110°)
                // Headroom slot needed over arriving flight: from +55° down to -115°
                // Even levels: flight arriving from below ascends along South arc (from -110° down to -310° / +50°)
                // Headroom slot needed over arriving flight: from -105° through +180° to +50°
                bool isStairWell = isOddLevel
                    ? (degMid >= -115f && degMid <= 55f)
                    : (degMid <= -105f || degMid >= 50f);

                float degA = i * 360f / segs;
                float degB = (i + 1) * 360f / segs;
                float a = degA * Mathf.Deg2Rad;
                float b = degB * Mathf.Deg2Rad;

                if (isStairWell)
                {
                    // IN THE STAIRWELL OPENING:
                    // 1. Leave outer ring (innerStairR - 0.10m to outerStairR) 100% EMPTY OPEN AIR for vertical stair clearance!
                    // 2. Build floor on inner room disc (r = 0.05m to innerStairR - 0.10m) - leaving center room >8.5m across!
                    Wedge("Room Floor Inner", 0.05f, innerStairR - 0.10f, a, b, y, floorThick, wood, true);
                }
                else
                {
                    // OUTSIDE STAIRWELL (Living / Room side):
                    // Build solid floor all the way from center to outer masonry wall!
                    Wedge("Room Floor Solid", 0.05f, outerStairR, a, b, y, floorThick, wood, true);
                }
            }

            // 3. Solid Arrival Landing Platform connecting stair top directly into room living floor
            // Odd: Flight arrives at -110° -> landing platform extends forward from -110° to -145°
            // Even: Flight arrives at +50° -> landing platform extends forward from +50° to +15°
            float landingDeg = isOddLevel ? -127.5f : 32.5f;
            float landingRad = landingDeg * Mathf.Deg2Rad;
            float landHalfSpan = 17.5f * Mathf.Deg2Rad;
            Wedge("Stair Landing Platform", innerStairR - 0.15f, outerStairR, landingRad - landHalfSpan, landingRad + landHalfSpan, y, floorThick, wood, true);

            // 4. Protective wrought-iron balustrade guarding stair opening with generous open landing gate
            float railH = 1.05f;
            int balSteps = 16;

            Vector3 prevTop = Vector3.zero, prevBot = Vector3.zero;
            for (int i = 0; i <= balSteps; i++)
            {
                float t = i / (float)balSteps;
                float deg;
                if (isOddLevel)
                {
                    // Span from +50° clockwise to -85° (leaving -85° to -145° landing exit 100% open gate)
                    deg = Mathf.Lerp(50f, -85f, t);
                }
                else
                {
                    // Span from -105° clockwise to +45° (total span 210° guarding stair well)
                    float totalSpan = 210f;
                    float cur = -105f - t * totalSpan;
                    if (cur < -180f) cur += 360f;
                    deg = cur;
                }
                float rad = deg * Mathf.Deg2Rad;
                float r = innerStairR - 0.10f;

                Vector3 bot = new Vector3(Mathf.Sin(rad) * r, y + 0.02f, Mathf.Cos(rad) * r);
                Vector3 top = bot + Vector3.up * railH;
                Rod("Balustrade Baluster", bot, top, 0.035f, iron);
                if (i > 0)
                {
                    Rod("Balustrade Top Rail", prevTop, top, 0.05f, iron);
                    Rod("Balustrade Mid Rail", (prevBot + prevTop) * 0.5f, (bot + top) * 0.5f, 0.035f, iron);
                    Rod("Balustrade Base Rail", prevBot, bot, 0.04f, iron);
                }
                prevTop = top;
                prevBot = bot;
            }

            // Prominent Brass Newel Post at the Landing Gate Entrance
            float newelDeg = isOddLevel ? -140f : 15f;
            float newelRad = newelDeg * Mathf.Deg2Rad;
            Vector3 newelPos = new Vector3(Mathf.Sin(newelRad) * (innerStairR - 0.10f), y, Mathf.Cos(newelRad) * (innerStairR - 0.10f));
            Rod("Balustrade Landing Newel", newelPos, newelPos + Vector3.up * 1.15f, 0.075f, brass);
            Cylinder("Balustrade Newel Finial", newelPos + Vector3.up * 1.18f, 0.06f, 0.08f, brass, Vector3.zero);
        }

        static void LibraryArchitecture()
        {
            RoomFloor(1, 3.2f, .14f);
        }

        static void LibraryArchiveAndWorkshop()
        {
            float wallR = Mathf.Lerp(5.65f, 4.25f, 3.2f / 25.6f);

            // Large Area Rug on Archive Floor (centered in spacious North room zone)
            Box("Library Area Rug", new(0f, 3.21f, 1.60f), new(3.8f, .012f, 2.8f), rugFabric);

            // Grand Floor-to-Ceiling Library Bookcase (against NORTH wall at z = 4.40m, safely away from stairs!)
            float bz = 4.40f;
            Box("Bookcase Left Upright", new(-1.60f, 4.5f, bz), new(.10f, 2.6f, .45f), wood, true);
            Box("Bookcase Right Upright", new(1.60f, 4.5f, bz), new(.10f, 2.6f, .45f), wood, true);
            Box("Bookcase Center Divider 1", new(-.55f, 4.5f, bz), new(.08f, 2.6f, .42f), wood);
            Box("Bookcase Center Divider 2", new(.55f, 4.5f, bz), new(.08f, 2.6f, .42f), wood);
            Box("Bookcase Top Crown", new(0f, 5.82f, bz), new(3.40f, .12f, .50f), wood);
            Box("Bookcase Shelf 1", new(0f, 3.32f, bz), new(3.20f, .06f, .42f), wood);
            Box("Bookcase Shelf 2", new(0f, 3.95f, bz), new(3.20f, .06f, .42f), wood);
            Box("Bookcase Shelf 3", new(0f, 4.60f, bz), new(3.20f, .06f, .42f), wood);
            Box("Bookcase Shelf 4", new(0f, 5.25f, bz), new(3.20f, .06f, .42f), wood);

            // Dense leatherbound books on Shelf 2
            for (int b = 0; b < 28; b++)
            {
                float bx = -1.45f + b * .105f;
                float bh = .24f + (b % 4) * .03f;
                Material bm = (b % 4 == 0) ? bookRed : (b % 4 == 1) ? bookGreen : (b % 4 == 2) ? bookBlue : bookGold;
                Box($"Book S2_{b}", new(bx, 3.98f + bh * .5f, bz - .02f), new(.07f, bh, .24f), bm);
                Box($"Book Spine S2_{b}", new(bx, 3.98f + bh * .5f, bz - .14f), new(.065f, bh * .9f, .02f), bookGold);
            }
            // Books on Shelf 3
            for (int b = 0; b < 26; b++)
            {
                float bx = -1.35f + b * .11f;
                float bh = .22f + ((b * 3) % 5) * .025f;
                Material bm = (b % 3 == 0) ? bookBlue : (b % 3 == 1) ? bookRed : bookGreen;
                Box($"Book S3_{b}", new(bx, 4.63f + bh * .5f, bz - .02f), new(.075f, bh, .22f), bm);
            }

            // Shelf 1: Heavy log ledgers and chart tubes
            Box("Log Ledger Stack", new(-.95f, 3.48f, bz - .02f), new(.65f, .28f, .30f), parchment);
            Box("Harbor Board Folios", new(.65f, 3.52f, bz - .02f), new(.75f, .32f, .28f), darkCoat);
            Cylinder("Chart Tube 1", new(-1.35f, 3.42f, bz - .02f), .055f, .75f, iron, new(0, 0, 75));
            Cylinder("Chart Tube 2", new(1.25f, 3.54f, bz - .02f), .045f, .80f, brass, new(0, 0, -80));

            // Planter pot with trailing ivy on Shelf 4 crown
            Cylinder("Library Planter Pot", new(1.15f, 5.38f, bz - .02f), .13f, .18f, ceramic, Vector3.zero);
            Cylinder("Planter Soil", new(1.15f, 5.48f, bz - .02f), .12f, .02f, dark, Vector3.zero);
            for (int v = 0; v < 8; v++)
            {
                float vy = 5.40f - v * .08f;
                Triangle($"Trailing Ivy {v}", new(1.15f, vy, bz - .14f), new(1.22f, vy - .07f, bz - .16f), new(1.10f, vy - .05f, bz - .14f), leaf);
            }

            // Investigation Desk facing North at z = 1.60m
            float dz = 1.60f;
            Box("Desk Top", new(0f, 3.96f, dz), new(1.90f, .08f, .95f), wood, true);
            Box("Desk Pedestal L", new(-.70f, 3.58f, dz), new(.45f, .68f, .82f), wood, true);
            Box("Desk Drawer Pull L1", new(-.70f, 3.82f, dz - .42f), new(.12f, .03f, .02f), brass);
            Box("Desk Drawer Pull L2", new(-.70f, 3.60f, dz - .42f), new(.12f, .03f, .02f), brass);
            Box("Desk Drawer Pull L3", new(-.70f, 3.38f, dz - .42f), new(.12f, .03f, .02f), brass);
            Box("Desk Pedestal R", new(.70f, 3.58f, dz), new(.45f, .68f, .82f), wood, true);
            Box("Desk Drawer Pull R1", new(.70f, 3.82f, dz - .42f), new(.12f, .03f, .02f), brass);
            Box("Desk Drawer Pull R2", new(.70f, 3.60f, dz - .42f), new(.12f, .03f, .02f), brass);
            Box("Desk Drawer Pull R3", new(.70f, 3.38f, dz - .42f), new(.12f, .03f, .02f), brass);
            Box("Desk Kneehole Panel", new(0f, 3.62f, dz + .38f), new(.85f, .58f, .04f), wood);

            // Brass Desktop Terrestrial Globe
            Cylinder("Globe Base", new(-.55f, 4.02f, dz + .10f), .11f, .03f, wood, Vector3.zero);
            Rod("Globe Spindle", new(-.55f, 4.03f, dz + .10f), new(-.55f, 4.16f, dz + .10f), .025f, brass);
            var globe = GameObject.CreatePrimitive(PrimitiveType.Sphere); globe.name = "Globe Sphere"; globe.transform.SetParent(area);
            globe.transform.position = new(-.55f, 4.28f, dz + .10f); globe.transform.localScale = Vector3.one * .24f;
            globe.transform.eulerAngles = new(23.5f, 30f, 0); globe.GetComponent<Renderer>().sharedMaterial = parchment; Object.DestroyImmediate(globe.GetComponent<Collider>());
            Cylinder("Globe Meridian Ring", new(-.55f, 4.28f, dz + .10f), .14f, .015f, brass, new(0, 0, 90));

            // Brass Navigation Astrolabe / Armillary Sphere
            Cylinder("Astrolabe Pedestal", new(.55f, 4.02f, dz + .10f), .08f, .03f, brass, Vector3.zero);
            Rod("Astrolabe Stand", new(.55f, 4.03f, dz + .10f), new(.55f, 4.14f, dz + .10f), .025f, brass);
            Cylinder("Astrolabe Outer Ring", new(.55f, 4.22f, dz + .10f), .12f, .015f, brass, Vector3.zero);
            Cylinder("Astrolabe Meridian Ring", new(.55f, 4.22f, dz + .10f), .11f, .015f, brass, new(90, 0, 0));
            Cylinder("Astrolabe Equator Ring", new(.55f, 4.22f, dz + .10f), .10f, .015f, brass, new(0, 0, 90));

            // Brass Reading Desk Banker Lamp
            Cylinder("Banker Lamp Base", new(-.15f, 4.02f, dz + .15f), .09f, .025f, brass, Vector3.zero);
            Rod("Banker Lamp Neck", new(-.15f, 4.03f, dz + .15f), new(-.15f, 4.30f, dz + .10f), .025f, brass);
            Box("Banker Lamp Shade", new(-.15f, 4.28f, dz + .10f), new(.24f, .09f, .14f), warm);

            // Open Keeper Logbook, Inkwell & Quill
            Box("Logbook Page L", new(.08f, 4.02f, dz - .10f), new(.18f, .015f, .26f), parchment);
            Box("Logbook Page R", new(.27f, 4.02f, dz - .10f), new(.18f, .015f, .26f), parchment);
            Box("Logbook Cover", new(.175f, 4.005f, dz - .10f), new(.39f, .015f, .28f), wood);
            Cylinder("Inkwell", new(-.28f, 4.02f, dz - .12f), .04f, .05f, dark, Vector3.zero);
            Rod("Quill", new(-.28f, 4.03f, dz - .12f), new(-.32f, 4.22f, dz - .08f), .015f, parchment);

            // Carved Library Armchair (facing North toward desk)
            float cz = dz - .65f;
            Box("Chair Seat", new(0f, 3.65f, cz), new(.58f, .08f, .54f), wood, true);
            Box("Chair Leg FL", new(-.24f, 3.42f, cz + .22f), new(.06f, .44f, .06f), wood);
            Box("Chair Leg FR", new(.24f, 3.42f, cz + .22f), new(.06f, .44f, .06f), wood);
            Box("Chair Leg BL", new(-.24f, 3.42f, cz - .22f), new(.06f, .44f, .06f), wood);
            Box("Chair Leg BR", new(.24f, 3.42f, cz - .22f), new(.06f, .44f, .06f), wood);
            Rod("Chair Back Post L", new(-.24f, 3.65f, cz - .22f), new(-.24f, 4.45f, cz - .22f), .06f, wood);
            Rod("Chair Back Post R", new(.24f, 3.65f, cz - .22f), new(.24f, 4.45f, cz - .22f), .06f, wood);
            Box("Chair Top Rail", new(0f, 4.42f, cz - .22f), new(.54f, .12f, .06f), wood);
            Box("Chair Back Slat 1", new(-.12f, 4.05f, cz - .22f), new(.04f, .65f, .03f), wood);
            Box("Chair Back Slat 2", new(0f, 4.05f, cz - .22f), new(.04f, .65f, .03f), wood);
            Box("Chair Back Slat 3", new(.12f, 4.05f, cz - .22f), new(.04f, .65f, .03f), wood);
            Rod("Chair Arm L", new(-.24f, 3.95f, cz + .10f), new(-.24f, 4.05f, cz - .22f), .05f, wood);
            Rod("Chair Arm R", new(.24f, 3.95f, cz + .10f), new(.24f, 4.05f, cz - .22f), .05f, wood);

            // Workshop Repair Bench along Northwest curved wall (x = -3.4m, z = 2.2m)
            Vector3 wBenchPos = new(-3.40f, 3.2f, 2.20f);
            Box("Workshop Bench Top", wBenchPos + Vector3.up * .78f, new(1.8f, .07f, .75f), wood, true);
            Box("Workshop Bench Shelf", wBenchPos + Vector3.up * .22f, new(1.6f, .04f, .65f), wood);
            for (int x = -1; x <= 1; x += 2) for (int z = -1; z <= 1; z += 2)
                Box("Workshop Bench Leg", wBenchPos + new Vector3(x * .80f, .39f, z * .30f), new(.08f, .78f, .08f), wood);
            Box("Clockwork Repair Vise", wBenchPos + new Vector3(.55f, .90f, -.28f), new(.18f, .16f, .15f), iron);
            Cylinder("Brass Gear Spare 1", wBenchPos + new Vector3(-.25f, .82f, -.15f), .09f, .018f, brass, Vector3.zero);
            Cylinder("Brass Gear Spare 2", wBenchPos + new Vector3(-.42f, .82f, .10f), .06f, .018f, brass, Vector3.zero);
            Box("Instrument Test Box", wBenchPos + new Vector3(-.05f, .88f, .15f), new(.28f, .15f, .22f), wood);

            // South wall observation window overlooking harbor approach
            Window(new(0, 4.8f, -wallR - .07f), 1.1f, 1.5f, false);

            // Overhead Drop Pendant Lamp
            Rod("Library Lamp Cord", new(0, 6.3f, 1.60f), new(0, 5.05f, 1.60f), .02f, iron);
            Cylinder("Library Lamp Shade", new(0, 5.0f, 1.60f), .35f, .14f, iron, Vector3.zero);
            Cylinder("Library Lamp Bulb", new(0, 4.94f, 1.60f), .10f, .07f, warm, Vector3.zero);
        }

        static void Library()
        {
            LibraryArchitecture();
            LibraryArchiveAndWorkshop();
        }

        static void WatchRoomArchitecture()
        {
            RoomFloor(2, 6.4f, .14f);

            // Observation Window at South
            float wallR = Mathf.Lerp(5.65f, 4.25f, 6.4f / 25.6f);
            Window(new(0, 7.8f, -wallR - .07f), 1.1f, 1.5f, false);
        }

        static void WatchRoomLiving()
        {
            float wallR = Mathf.Lerp(5.65f, 4.25f, 6.4f / 25.6f);

            // Living Quarters Braided Rug centered on South living floor
            Box("Watch Room Braided Rug", new(0f, 6.41f, -1.80f), new(3.2f, .012f, 3.2f), braidedRug);

            // Large Prominent Nautical Station Wall Clock mounted on South curved wall
            Cylinder("Clock Iron Rim", new(0, 8.2f, -wallR + .05f), .48f, .05f, iron, new(90, 0, 0));
            Cylinder("Clock Dial Face", new(0, 8.2f, -wallR + .08f), .44f, .03f, clockDial, new(90, 0, 0));
            for (int h = 0; h < 12; h++)
            {
                float alpha = h * 30f * Mathf.Deg2Rad;
                Box($"Clock Tick {h}", new(Mathf.Sin(alpha) * .38f, 8.2f + Mathf.Cos(alpha) * .38f, -wallR + .10f), new(.022f, .055f, .012f), dark);
            }
            Rod("Clock Hour Hand", new(0, 8.2f, -wallR + .11f), new(-.14f, 8.35f, -wallR + .11f), .025f, dark);
            Rod("Clock Minute Hand", new(0, 8.2f, -wallR + .11f), new(.20f, 8.40f, -wallR + .11f), .018f, dark);
            Cylinder("Clock Hub Cap", new(0, 8.2f, -wallR + .12f), .045f, .02f, brass, new(90, 0, 0));

            // Framed Sea Navigation Charts Flanking Clock on South Wall
            Box("Depth Chart Frame", new(-1.45f, 8.2f, -wallR + .15f), new(.75f, .58f, .04f), wood);
            Box("Depth Chart Paper", new(-1.45f, 8.2f, -wallR + .18f), new(.68f, .50f, .015f), parchment);
            Box("Shipping Chart Frame", new(1.45f, 8.2f, -wallR + .15f), new(.75f, .58f, .04f), wood);
            Box("Shipping Chart Paper", new(1.45f, 8.2f, -wallR + .18f), new(.68f, .50f, .015f), parchment);

            // Keeper's Sturdy Timber Bedstead positioned in Northeast living quarters
            Vector3 bedPos = new(2.40f, 6.4f, 2.00f);
            Box("Keeper Bed Frame L", bedPos + new Vector3(-.65f, .25f, 0), new(.08f, .50f, 2.15f), wood, true);
            Box("Keeper Bed Frame R", bedPos + new Vector3(.65f, .25f, 0), new(.08f, .50f, 2.15f), wood, true);
            Box("Keeper Bed Headboard", bedPos + new Vector3(0, .65f, 1.05f), new(1.40f, .95f, .08f), wood, true);
            Box("Keeper Bed Footboard", bedPos + new Vector3(0, .42f, -1.05f), new(1.40f, .62f, .08f), wood, true);
            Box("Keeper Bed Mattress", bedPos + new Vector3(0, .32f, 0), new(1.25f, .28f, 2.05f), warm);
            Box("Keeper Bed Wool Blanket", bedPos + new Vector3(0, .36f, -.25f), new(1.28f, .22f, 1.45f), darkCoat);
            Box("Keeper Bed Pillow", bedPos + new Vector3(0, .48f, .75f), new(.85f, .14f, .45f), parchment);

            // Bedside Nightstand & Oil Lamp (x = 3.5m, z = 2.0m)
            Vector3 nightPos = new(3.50f, 6.4f, 2.00f);
            Box("Bedside Nightstand Top", nightPos + Vector3.up * .62f, new(.55f, .05f, .55f), wood, true);
            Box("Nightstand Body", nightPos + Vector3.up * .30f, new(.48f, .58f, .48f), wood);
            Cylinder("Nightstand Lamp Base", nightPos + Vector3.up * .67f, .10f, .04f, brass, Vector3.zero);
            Cylinder("Nightstand Lamp Glass", nightPos + Vector3.up * .80f, .08f, .22f, warmWindow, Vector3.zero);

            // Weathered Sea Trunk at foot of bed with framed photo of Crane holding young Daniel
            Vector3 chestPos = new(2.40f, 6.4f, 0.60f);
            Box("Crane Sea Chest Base", chestPos + Vector3.up * .26f, new(1.05f, .52f, .60f), wood, true);
            Box("Crane Sea Chest Lid", chestPos + Vector3.up * .55f, new(1.08f, .08f, .62f), wood);
            Box("Chest Iron Strap 1", chestPos + new Vector3(-.35f, .26f, 0), new(.06f, .54f, .63f), iron);
            Box("Chest Iron Strap 2", chestPos + new Vector3(.35f, .26f, 0), new(.06f, .54f, .63f), iron);
            Box("Family Photo Frame", chestPos + new Vector3(0, .68f, -.10f), new(.26f, .22f, .025f), brass);
            Box("Family Photo Print", chestPos + new Vector3(0, .68f, -.12f), new(.22f, .18f, .01f), parchment);

            // Keeper's Wardrobe Locker along West wall (x = -3.8m, z = 0.2m)
            Vector3 wardPos = new(-3.80f, 6.4f, 0.20f);
            Box("Keeper Wardrobe Body", wardPos + Vector3.up * 1.30f, new(.75f, 2.4f, 1.40f), wood, true);
            Box("Wardrobe Crown Cornice", wardPos + Vector3.up * 2.54f, new(.82f, .10f, 1.48f), wood);
            Box("Wardrobe Door L", wardPos + new Vector3(.38f, 1.25f, -.33f), new(.02f, 2.15f, .64f), wood);
            Box("Wardrobe Door R", wardPos + new Vector3(.38f, 1.25f, .33f), new(.02f, 2.15f, .64f), wood);
            Cylinder("Wardrobe Handle L", wardPos + new Vector3(.40f, 1.22f, -.05f), .02f, .03f, brass, new(0, 0, 90));
            Cylinder("Wardrobe Handle R", wardPos + new Vector3(.40f, 1.22f, .05f), .02f, .03f, brass, new(0, 0, 90));

            // Cast-Iron Heating Stove & Rocking Chair at Southwest (x = -2.8m, z = -2.8m)
            Vector3 stovePos = new(-2.80f, 6.4f, -2.80f);
            Box("Living Stove Body", stovePos + Vector3.up * .45f, new(.68f, .86f, .68f), iron, true);
            Box("Living Stove Door", stovePos + new Vector3(.35f, .45f, 0), new(.04f, .50f, .50f), iron);
            Box("Living Stove Embers", stovePos + new Vector3(.36f, .45f, 0), new(.02f, .32f, .32f), stoveFire);
            Cylinder("Living Stove Flue", stovePos + Vector3.up * 1.95f, .10f, 2.2f, iron, Vector3.zero);
            // Rocking Chair beside stove
            Vector3 rockChair = new(-1.85f, 6.4f, -2.40f);
            Box("Rocking Chair Seat", rockChair + Vector3.up * .46f, new(.54f, .06f, .54f), wood, true);
            for (int x = -1; x <= 1; x += 2) for (int z = -1; z <= 1; z += 2)
                Box("Rocking Leg", rockChair + new Vector3(x * .20f, .24f, z * .20f), new(.05f, .44f, .05f), wood);
            Box("Rocker Runner L", rockChair + new Vector3(-.20f, .04f, 0), new(.04f, .08f, .85f), wood);
            Box("Rocker Runner R", rockChair + new Vector3(.20f, .04f, 0), new(.04f, .08f, .85f), wood);
            Box("Rocker Back", rockChair + new Vector3(0, 1.05f, .24f), new(.50f, .68f, .05f), wood);

            // Secretary Writing Desk & Chair at Southeast (x = 1.8m, z = -3.2m)
            Vector3 secPos = new(1.80f, 6.4f, -3.20f);
            Box("Secretary Desk Top", secPos + Vector3.up * .82f, new(1.30f, .06f, .75f), wood, true);
            Box("Secretary Hutch", secPos + new Vector3(0, 1.35f, -.25f), new(1.25f, 1.0f, .28f), wood);
            for (int x = -1; x <= 1; x += 2) for (int z = -1; z <= 1; z += 2)
                Box("Secretary Leg", secPos + new Vector3(x * .55f, .41f, z * .30f), new(.06f, .82f, .06f), wood);
            Box("Secretary Chair Seat", secPos + new Vector3(0, .46f, .50f), new(.46f, .05f, .46f), wood, true);

            // Overhead Drop Pendant Lamp
            Rod("Watch Lamp Cord", new(0, 9.5f, -1.8f), new(0, 8.15f, -1.8f), .02f, iron);
            Cylinder("Watch Lamp Shade", new(0, 8.10f, -1.8f), .35f, .14f, iron, Vector3.zero);
            Cylinder("Watch Lamp Bulb", new(0, 8.04f, -1.8f), .10f, .07f, warm, Vector3.zero);
        }

        static void WatchRoom()
        {
            WatchRoomArchitecture();
            WatchRoomLiving();
        }

        static void Landing3()
        {
            // Level 3 (y = 9.6m): Crane's Room / Storeroom (Solid floor on North side)
            RoomFloor(3, 9.6f, .14f);

            // Crane's Sturdy Iron Bedstead positioned in room core
            Box("Crane Bed Mattress", new(-0.2f, 9.95f, 1.6f), new(1.2f, .35f, 2.0f), warm);
            Box("Crane Bed Frame L", new(-0.8f, 9.85f, 1.6f), new(.06f, .45f, 2.05f), iron, true);
            Box("Crane Bed Frame R", new(0.4f, 9.85f, 1.6f), new(.06f, .45f, 2.05f), iron, true);
            Box("Crane Bed Headboard", new(-0.2f, 10.25f, 2.6f), new(1.25f, .95f, .06f), iron, true);
            Box("Crane Bed Pillow", new(-0.2f, 10.20f, 2.3f), new(.75f, .14f, .45f), parchment);

            // Bedside Maritime Table & Lantern
            Box("Crane Nightstand", new(0.8f, 9.95f, 2.0f), new(.55f, .68f, .55f), wood, true);
            Cylinder("Nightstand Lantern Base", new(0.8f, 10.32f, 2.0f), .09f, .04f, brass, Vector3.zero);
            Cylinder("Nightstand Lantern Glass", new(0.8f, 10.45f, 2.0f), .07f, .20f, warmWindow, Vector3.zero);

            // Heavy Sea Trunk with Framed Photograph of Crane holding Daniel as a child
            Box("Crane Sea Chest Base", new(-0.2f, 9.95f, 0.4f), new(.95f, .60f, .60f), wood, true);
            Box("Crane Sea Chest Lid", new(-0.2f, 10.28f, 0.4f), new(.98f, .08f, .62f), wood);
            Box("Chest Iron Strap 1", new(-0.45f, 9.95f, 0.4f), new(.06f, .62f, .63f), iron);
            Box("Chest Iron Strap 2", new(0.05f, 9.95f, 0.4f), new(.06f, .62f, .63f), iron);

            // Framed Family Photograph: Crane holding young Daniel
            Box("Family Photo Frame", new(-0.2f, 10.42f, 0.35f), new(.24f, .20f, .025f), brass);
            Box("Family Photo Print", new(-0.2f, 10.42f, 0.33f), new(.20f, .16f, .01f), parchment);

            // Hanging Keeper Coat on Wall Peg
            Cylinder("Coat Peg", new(-1.8f, 10.8f, 1.6f), .02f, .15f, wood, new(90, 0, 0));
            Box("Crane Keeper Coat", new(-1.8f, 10.2f, 1.55f), new(.45f, 1.1f, .20f), darkCoat);

            // Overhead drop lamp
            Rod("Crane Room Lamp Cord", new(0, 12.7f, 1.5f), new(0, 11.35f, 1.5f), .02f, iron);
            Cylinder("Crane Room Lamp Hood", new(0, 11.30f, 1.5f), .28f, .12f, iron, Vector3.zero);
            Cylinder("Crane Room Lamp Bulb", new(0, 11.25f, 1.5f), .09f, .06f, warm, Vector3.zero);
        }

        static void TowerStoreroom()
        {
            // Level 4 (y = 12.8m): Storeroom & Oil Locker (Solid floor on South side)
            RoomFloor(4, 12.8f, .14f);

            // Heavy maritime storage barrels along South wall
            Barrel("Oil Barrel 1", new Vector3(-1.4f, 12.8f, -2.8f), Vector3.zero, .36f, .85f);
            Barrel("Oil Barrel 2", new Vector3(-0.6f, 12.8f, -3.0f), Vector3.zero, .34f, .80f);
            Barrel("Oil Barrel 3", new Vector3(0.4f, 12.8f, -2.8f), Vector3.zero, .35f, .82f);

            // Storage crate stack against South inner wall
            Box("Supply Crate Large", new Vector3(-1.0f, 13.25f, -1.2f), new Vector3(.90f, .75f, .70f), wood, true);
            Box("Supply Crate Medium", new Vector3(-1.0f, 13.88f, -1.2f), new Vector3(.70f, .50f, .60f), wood, true);

            // Wall storage shelving with spare lantern parts and oil canisters
            Box("Storage Shelf Upright L", new Vector3(1.4f, 14.1f, -2.2f), new Vector3(.08f, 2.2f, .35f), wood);
            Box("Storage Shelf Upright R", new Vector3(2.6f, 14.1f, -2.2f), new Vector3(.08f, 2.2f, .35f), wood);
            Box("Storage Shelf Tier 1", new Vector3(2.0f, 13.5f, -2.2f), new Vector3(1.2f, .04f, .32f), wood);
            Box("Storage Shelf Tier 2", new Vector3(2.0f, 14.1f, -2.2f), new Vector3(1.2f, .04f, .32f), wood);
            Box("Storage Shelf Tier 3", new Vector3(2.0f, 14.7f, -2.2f), new Vector3(1.2f, .04f, .32f), wood);

            // Oil canister and toolboxes on shelves
            Box("Brass Oil Can", new Vector3(1.8f, 13.7f, -2.18f), new Vector3(.22f, .35f, .18f), brass);
            Box("Iron Tool Chest", new Vector3(2.3f, 13.65f, -2.20f), new Vector3(.40f, .24f, .22f), iron);

            // Atmospheric ceiling drop lantern
            Rod("Storeroom Lamp Cord", new Vector3(0, 15.9f, -1.4f), new Vector3(0, 14.6f, -1.4f), .02f, iron);
            Cylinder("Storeroom Lamp Hood", new Vector3(0, 14.55f, -1.4f), .28f, .12f, iron, Vector3.zero);
            Cylinder("Storeroom Lamp Bulb", new Vector3(0, 14.50f, -1.4f), .09f, .06f, warm, Vector3.zero);
        }

        static void TowerServiceLanding()
        {
            // Level 5 (y = 16.0m): Maintenance & Service Landing (Solid floor on North side)
            RoomFloor(5, 16.0f, .14f);

            // Sturdy maintenance workbench against North wall
            Box("Service Workbench Top", new Vector3(0, 16.85f, 2.6f), new Vector3(2.2f, .08f, .85f), wood, true);
            Box("Service Leg L", new Vector3(-1.0f, 16.42f, 2.6f), new Vector3(.08f, .78f, .75f), wood);
            Box("Service Leg R", new Vector3(1.0f, 16.42f, 2.6f), new Vector3(.08f, .78f, .75f), wood);
            Box("Bench Tool Vise", new Vector3(-0.85f, 16.98f, 2.35f), new Vector3(.18f, .16f, .15f), iron);
            Box("Wrench Tray", new Vector3(0.2f, 16.92f, 2.55f), new Vector3(.42f, .04f, .28f), iron);

            // Overhead service lamp
            Rod("Service Lamp Cord", new Vector3(0, 19.1f, 1.8f), new Vector3(0, 17.8f, 1.8f), .02f, iron);
            Cylinder("Service Lamp Hood", new Vector3(0, 17.75f, 1.8f), .28f, .12f, iron, Vector3.zero);
            Cylinder("Service Lamp Bulb", new Vector3(0, 17.70f, 1.8f), .09f, .06f, warm, Vector3.zero);
        }

        static void TowerMachineRoom()
        {
            // Level 6 (y = 19.2m): Rotation Clockwork & Winch Room (Solid floor on South side)
            RoomFloor(6, 19.2f, .14f);

            // Heavy cast-iron rotation drive housing and cable drum (kept within inner core r <= 1.2m)
            Box("Rotation Drive Base", new Vector3(0, 19.6f, -1.0f), new Vector3(1.4f, .65f, 1.0f), iron, true);
            Cylinder("Clockwork Winch Drum", new Vector3(0, 19.95f, -1.0f), .35f, .85f, iron, new Vector3(0, 0, 90));
            Rod("Steel Drive Cable", new Vector3(0, 19.95f, -1.0f), new Vector3(0, 1.0f, -1.0f), .025f, iron);
            Box("Governor Flyball Stand", new Vector3(0.95f, 20.0f, -0.9f), new Vector3(.32f, .85f, .32f), brass);
            Cylinder("Governor Spindle", new Vector3(0.95f, 20.55f, -0.9f), .04f, .35f, brass, Vector3.zero);

            // Overhead drop lamp
            Rod("Machine Lamp Cord", new Vector3(0, 22.3f, -1.2f), new Vector3(0, 21.0f, -1.2f), .02f, iron);
            Cylinder("Machine Lamp Hood", new Vector3(0, 20.95f, -1.2f), .28f, .12f, iron, Vector3.zero);
            Cylinder("Machine Lamp Bulb", new Vector3(0, 20.90f, -1.2f), .09f, .06f, warm, Vector3.zero);
        }

        static void TowerLanternService()
        {
            // Level 7 (y = 22.4m): Upper Service Platform below Lantern (Solid floor on North side)
            RoomFloor(7, 22.4f, .14f);

            // Compact central collar support (kept within r <= 0.85m to never encroach on stair envelope)
            Box("Lantern Support Base Ring", new Vector3(0, 22.75f, 0), new Vector3(1.6f, .55f, 1.6f), iron, true);
            Cylinder("Lantern Turning Collar", new Vector3(0, 23.1f, 0), .65f, .22f, brass, Vector3.zero);

            // Wall ladder to lantern floor along North wall
            for (int r = 0; r < 7; r++)
            {
                float ry = 22.6f + r * .42f;
                Rod($"Upper Service Ladder Rung {r}", new Vector3(-0.25f, ry, 2.2f), new Vector3(0.25f, ry, 2.2f), .03f, iron);
            }
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
        static void TowerIntermediateFloors()
        {
            TowerStoreroom();
            TowerServiceLanding();
            TowerMachineRoom();
            TowerLanternService();
        }

        static void LanternArchitecture()
        {
            var settings = AssetDatabase.LoadAssetAtPath<LighthouseLayoutSettings>("Assets/_Project/Settings/LighthouseLayoutSettings.asset");
            float y = settings.floorHeight * settings.towerLevelCount; // 25.6m

            const int segs = 32;
            float topTowerOuterR = 4.80f;
            float topTowerInnerR = 4.25f;
            float galleryOuterR = 5.65f;

            // 1. SOLID FLARED STONE CORNICE UNDER GALLERY DECK (Resolves Screenshot 2 gap!)
            // Smoothly flares the masonry tower head from 4.80m out to 5.65m (y from 25.10m to 25.55m)
            for (int i = 0; i < segs; i++)
            {
                float a = i * Mathf.PI * 2 / segs;
                float b = (i + 1) * Mathf.PI * 2 / segs;
                Vector3 cBotA = new(Mathf.Sin(a) * topTowerOuterR, y - 0.50f, Mathf.Cos(a) * topTowerOuterR);
                Vector3 cBotB = new(Mathf.Sin(b) * topTowerOuterR, y - 0.50f, Mathf.Cos(b) * topTowerOuterR);
                Vector3 cTopA = new(Mathf.Sin(a) * galleryOuterR, y - 0.05f, Mathf.Cos(a) * galleryOuterR);
                Vector3 cTopB = new(Mathf.Sin(b) * galleryOuterR, y - 0.05f, Mathf.Cos(b) * galleryOuterR);
                Quad("Cornice Flare", cBotA, cTopA, cTopB, cBotB, plaster);
            }

            // 2. SOLID CAST-IRON SOFFIT UNDERBELLY PLATE under entire gallery overhang (full 360° solid plate)
            for (int i = 0; i < segs; i++)
            {
                float a = i * Mathf.PI * 2 / segs;
                float b = (i + 1) * Mathf.PI * 2 / segs;
                Wedge("Gallery Soffit Plate", topTowerInnerR, galleryOuterR, a, b, y - 0.05f, 0.08f, iron, true);
            }

            // 3. 16 CAST-IRON RADIAL CORBEL SUPPORT BRACKETS around full 360° circumference
            for (int i = 0; i < segs; i += 2)
            {
                float a = i * Mathf.PI * 2 / segs;
                Vector3 bracketOuter = new Vector3(Mathf.Sin(a) * (galleryOuterR - 0.10f), y - 0.06f, Mathf.Cos(a) * (galleryOuterR - 0.10f));
                Vector3 bracketWall = new Vector3(Mathf.Sin(a) * topTowerOuterR, y - 0.85f, Mathf.Cos(a) * topTowerOuterR);
                Rod("Gallery Corbel Bracket", bracketOuter, bracketWall, .09f, iron);
                // Gusset web between bracket and soffit
                Vector3 bracketMid = (bracketOuter + bracketWall) * 0.5f;
                Box("Corbel Web", bracketMid, new(.05f, .22f, .45f), iron);
            }

            // 4. FULL 360° SOLID EXTERIOR GALLERY WALKWAY DECK (r = 4.25m to 5.65m at y = 25.60m)
            // Complete solid iron diamond-plate walkway for ALL 32 segments - NO SKIPPING!
            for (int i = 0; i < segs; i++)
            {
                float a = i * Mathf.PI * 2 / segs;
                float b = (i + 1) * Mathf.PI * 2 / segs;

                Wedge("Gallery Exterior Walkway", topTowerInnerR, galleryOuterR, a, b, y, 0.14f, iron, true);

                // Heavy cast-iron safety balustrade at outer gallery perimeter (r = 5.65m)
                Vector3 p = new(Mathf.Sin(a) * galleryOuterR, y, Mathf.Cos(a) * galleryOuterR);
                Vector3 q = new(Mathf.Sin(b) * galleryOuterR, y, Mathf.Cos(b) * galleryOuterR);
                Rod("Gallery Baluster", p, p + Vector3.up * 1.15f, .055f, iron);
                Rod("Gallery Top Rail", p + Vector3.up * 1.15f, q + Vector3.up * 1.15f, .065f, iron);
                Rod("Gallery Mid Rail", p + Vector3.up * .55f, q + Vector3.up * .55f, .04f, iron);
                Rod("Gallery Toe Kick", p + Vector3.up * .08f, q + Vector3.up * .08f, .045f, iron);
            }

            // 5. SOLID CENTRAL DECK INSIDE (r = 0.05m to 2.60m) under Fresnel Lens
            Box("Lantern Central Floor Plate", new(0, y - 0.07f, 0), new(3.20f, 0.14f, 3.20f), iron, true);
            for (int i = 0; i < segs; i++)
            {
                float a = i * Mathf.PI * 2 / segs;
                float b = (i + 1) * Mathf.PI * 2 / segs;
                Wedge("Lantern Central Deck", 0.05f, 2.60f, a, b, y, 0.14f, iron, true);
            }

            // 6. INTERIOR WALKWAY & CONTROLLED STAIR HATCH (r = 2.60m to 4.25m)
            // Flight 7 ascends along South arc from -110° to -310° (+50°)
            // The stairwell opening is needed ONLY along Flight 7: angles from -120° to +40°
            for (int i = 0; i < segs; i++)
            {
                float a = i * Mathf.PI * 2 / segs;
                float b = (i + 1) * Mathf.PI * 2 / segs;
                float degMid = (i + 0.5f) * 360f / segs;
                if (degMid > 180f) degMid -= 360f; // [-180, 180]

                bool isFlight7StairHatch = (degMid <= -120f || degMid >= 40f);

                if (isFlight7StairHatch)
                {
                    // Leave outer stair slot (r = 3.08m to 4.23m) open for stair arrival
                    // Keep inner walkway (r = 2.60m to 3.08m) solid!
                    Wedge("Lantern Inner Walkway", 2.60f, 3.08f, a, b, y, 0.14f, iron, true);
                }
                else
                {
                    // Outside stair hatch: full solid walkway from lens to outer wall!
                    Wedge("Lantern Solid Walkway", 2.60f, topTowerInnerR, a, b, y, 0.14f, iron, true);
                }
            }

            // 7. SOLID ARRIVAL LANDING PLATFORM AT FLIGHT 7 TOP (+15° to +65°)
            float landRad = 40.0f * Mathf.Deg2Rad;
            float landHalf = 25.0f * Mathf.Deg2Rad;
            Wedge("Lantern Stair Landing", 3.00f, topTowerInnerR, landRad - landHalf, landRad + landHalf, y, 0.14f, iron, true);

            // 8. SAFETY GUARD RAILING ALONG INNER STAIRWELL DROP: from -120° clockwise to +35° at r = 3.08m
            int gSteps = 16;
            Vector3 prevG = Vector3.zero;
            for (int gi = 0; gi <= gSteps; gi++)
            {
                float gt = gi / (float)gSteps;
                float cur = -120f - gt * 205f;
                if (cur < -180f) cur += 360f;
                float grad = cur * Mathf.Deg2Rad;
                Vector3 gPt = new(Mathf.Sin(grad) * 3.08f, y, Mathf.Cos(grad) * 3.08f);
                Rod("Stair Guard Post", gPt, gPt + Vector3.up * 1.10f, .045f, iron);
                if (gi > 0)
                {
                    Rod("Stair Guard Top Rail", prevG + Vector3.up * 1.10f, gPt + Vector3.up * 1.10f, .05f, iron);
                    Rod("Stair Guard Mid Rail", prevG + Vector3.up * 0.55f, gPt + Vector3.up * 0.55f, .035f, iron);
                }
                prevG = gPt;
            }
            Vector3 newelTop = new(Mathf.Sin(35f * Mathf.Deg2Rad) * 3.08f, y, Mathf.Cos(35f * Mathf.Deg2Rad) * 3.08f);
            Rod("Lantern Guard Newel", newelTop, newelTop + Vector3.up * 1.15f, .075f, brass);
            Cylinder("Lantern Newel Ball", newelTop + Vector3.up * 1.18f, .06f, .08f, brass, Vector3.zero);

            for (int level = 2; level < 8; level += 2)
            {
                float radius = Mathf.Lerp(5.65f, 4.25f, level / 8f);
                Window(new(0, level * 3.2f + 1.6f, -radius - .07f), .85f, 1.2f, false);
            }
        }

        static void LanternRoom()
        {
            var settings = AssetDatabase.LoadAssetAtPath<LighthouseLayoutSettings>("Assets/_Project/Settings/LighthouseLayoutSettings.asset");
            float y = settings.floorHeight * settings.towerLevelCount; // 25.6m

            // GRAND ARCHED ENTRYWAY PORTAL INTO LANTERN ROOM AT +50°
            float gateAng = 50f * Mathf.Deg2Rad;
            Vector3 jambInner = new(Mathf.Sin(gateAng) * 3.00f, y, Mathf.Cos(gateAng) * 3.00f);
            Vector3 jambOuter = new(Mathf.Sin(gateAng) * 4.25f, y, Mathf.Cos(gateAng) * 4.25f);
            Rod("Lantern Portal Inner Jamb", jambInner, jambInner + Vector3.up * 2.50f, 0.10f, iron);
            Rod("Lantern Portal Outer Jamb", jambOuter, jambOuter + Vector3.up * 2.50f, 0.10f, iron);
            Rod("Lantern Portal Arched Header", jambInner + Vector3.up * 2.50f, jambOuter + Vector3.up * 2.50f, 0.12f, iron);
            Cylinder("Portal Lantern Inner Glow", jambInner + Vector3.up * 2.0f, 0.06f, 0.12f, warm, Vector3.zero);
            Cylinder("Portal Lantern Outer Glow", jambOuter + Vector3.up * 2.0f, 0.06f, 0.12f, warm, Vector3.zero);

            // 16-PANEL FACETED BRONZE & BEACON GLASS ENCLOSURE (r = 4.20m, height 3.6m)
            const int panels = 16;
            float rGlass = 4.20f;
            for (int i = 0; i < panels; i++)
            {
                float a = i * Mathf.PI * 2 / panels;
                float b = (i + 1) * Mathf.PI * 2 / panels;
                float degMid = (i + 0.5f) * 360f / panels;

                Vector3 p = new(Mathf.Sin(a) * rGlass, y, Mathf.Cos(a) * rGlass);
                Vector3 q = new(Mathf.Sin(b) * rGlass, y, Mathf.Cos(b) * rGlass);

                // Vertical Bronze Astragal Mullions
                Rod("Lantern Bronze Astragal", p, p + Vector3.up * 3.60f, .085f, brass);
                // Top Bronze Lintel Ring
                Rod("Lantern Top Lintel", p + Vector3.up * 3.60f, q + Vector3.up * 3.60f, .10f, brass);

                // Lower Cast-Iron Murette Wall Base (height 0.90m)
                // Leave threshold clear at portal entrance (approx 45° to 67.5°)
                bool isPortalBay = Mathf.Abs(Mathf.DeltaAngle(degMid, 50f)) < 18f;
                if (!isPortalBay)
                {
                    Quad("Lantern Murette Base", p, p + Vector3.up * .90f, q + Vector3.up * .90f, q, iron);
                    // Clear Beacon Glass Panel from 0.90m to 3.60m
                    Quad("Lantern Glass Pane", p + Vector3.up * .90f, p + Vector3.up * 3.60f, q + Vector3.up * 3.60f, q + Vector3.up * .90f, glass);
                    // Horizontal bronze saddle bar reinforcing glass
                    Rod("Glass Saddle Bar", p + Vector3.up * 2.25f, q + Vector3.up * 2.25f, .04f, brass);
                }

                // 16-FACET CONICAL COPPER ROOF CAP
                Vector3 apex = new(0, y + 5.20f, 0);
                Triangle("Lantern Copper Roof Cap", p + Vector3.up * 3.60f, apex, q + Vector3.up * 3.60f, roof);
            }

            // Apex Ventilation Ball and Lightning Rod
            Vector3 roofApex = new(0, y + 5.20f, 0);
            Cylinder("Lantern Roof Vent Ball", roofApex + Vector3.up * .25f, .35f, .50f, brass, Vector3.zero);
            Rod("Lantern Lightning Rod", roofApex + Vector3.up * .50f, roofApex + Vector3.up * 2.10f, .04f, brass);

            // Maintenance spares cabinet against North wall
            Box("Lantern Tool Cabinet", new Vector3(0.0f, y + 0.9f, 2.1f), new Vector3(.85f, 1.8f, .55f), iron, true);
            // Visual Hand-Crank Stand matching Crank Station
            Box("Lamp Hand-Crank Stand", new Vector3(-1.8f, y + 0.5f, 1.2f), new Vector3(.45f, 1.0f, .45f), iron, true);
            Cylinder("Crank Wheel", new Vector3(-1.8f, y + 1.05f, 1.2f), .25f, .05f, brass, new Vector3(0, 0, 90));
        }

        static void Quad(string name, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Material m)
        {
            var mesh = new Mesh
            {
                vertices = new[] { p0, p1, p2, p3, p0, p3, p2, p1 },
                triangles = new[] { 0, 1, 2, 0, 2, 3, 4, 5, 6, 4, 6, 7 },
                uv = new[] { Vector2.zero, Vector2.up, Vector2.one, Vector2.right, Vector2.zero, Vector2.right, Vector2.one, Vector2.up }
            };
            mesh.RecalculateNormals();
            var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(area, false);
            go.GetComponent<MeshFilter>().sharedMesh = Persist(mesh);
            go.GetComponent<MeshRenderer>().sharedMaterial = m;
        }

        static void LampMechanism()
        {
            var settings = AssetDatabase.LoadAssetAtPath<LighthouseLayoutSettings>("Assets/_Project/Settings/LighthouseLayoutSettings.asset");
            float y = settings.floorHeight * settings.towerLevelCount;

            var lens = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            lens.name = "Beacon Core";
            lens.transform.SetParent(area);
            lens.transform.position = new(0, y + 2.4f, 0);
            lens.transform.localScale = new(1.4f, 1.9f, 1.4f);
            Object.DestroyImmediate(lens.GetComponent<Collider>());
            lens.GetComponent<Renderer>().sharedMaterial = ReferenceSurfaceLibrary.Solid("Beacon Glass", new(.7f, .86f, 1f), 5);
            Cylinder("Lamp Pedestal", new(0, y + .5f, 0), .55f, 1, iron, Vector3.zero);
            for (int i = 0; i < 8; i++)
                Cylinder("Fresnel Lens Ring", new(0, y + 1.7f + i * .15f, 0), .58f, .065f, glass, Vector3.zero);
        }

        static void Lantern()
        {
            LanternArchitecture();
            LanternRoom();
            LampMechanism();
        }
        static void Coast()
        {
            var random=new System.Random(173);
            for(int i=0;i<look.scrubCount;i++)
            {
                float x=-16+(float)random.NextDouble()*32,z=-10-(float)random.NextDouble()*13;
                if(Mathf.Abs(x)<3.5f || (x>6 && z<-11)) continue;
                // Exclude entire boat cove and dock approach from coastal grass
                if ((x > -14f && x < 4f && z < -20f) || Vector2.Distance(new Vector2(x, z), new Vector2(-5.8f, -29.8f)) < 8.0f) continue;
                float h=.25f+(float)random.NextDouble()*.6f;
                for(int j=0;j<4;j++)
                {
                    float a=j*Mathf.PI*.5f;Vector3 p=new(x,.2f,z);
                    Triangle("Coastal Grass",p+new Vector3(-.2f,0,0),p+new Vector3(Mathf.Sin(a)*.35f,h,Mathf.Cos(a)*.35f),p+new Vector3(.2f,0,.15f),leaf);
                }
            }
        }
        static void Lighting()
        {
            const string skyPath="Assets/_Project/Materials/Reference/Harrow Sky.mat";
            var sky=AssetDatabase.LoadAssetAtPath<Material>(skyPath);if(!sky){sky=new Material(Shader.Find("TheLightHouse/Harrow Storm Sky"));AssetDatabase.CreateAsset(sky,skyPath);}RenderSettings.skybox=sky;RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(0.08f, 0.11f, 0.15f);
            RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogColor=new Color(0.05f, 0.08f, 0.10f);RenderSettings.fogDensity=0.012f;
            var lights=Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            var practicals=lights.Where(l=>l.name=="Warm Lantern").ToArray();
            for(int i=0;i<practicals.Length;i++)
            {
                var light=practicals[i];light.transform.position=i==0?new(-8.65f,1.65f,-2.4f):new(-1.4f,2.1f,1.2f);light.color=i==0?new Color(1f, 0.82f, 0.55f):look.moon;light.intensity=i==0?2.6f:1.4f;light.range=i==0?5.5f:6.5f;light.shadows=LightShadows.Soft;
            }
            foreach(var light in lights.Where(l=>l.type==LightType.Directional)){light.intensity=0.45f;light.color=new Color(0.75f, 0.82f, 0.90f);light.transform.rotation=Quaternion.Euler(38,-38,0);light.shadows=LightShadows.Soft;light.shadowStrength=.85f;}
            var beamOrigin=GameObject.Find("Lamp Beam Visual");if(beamOrigin)beamOrigin.transform.position=new(0,28,0);
            var beam=GameObject.Find("Soft Beam Cone");
            if(beam) { beam.transform.localScale=Vector3.one*42; beam.GetComponent<Renderer>().sharedMaterial.SetColor("_BeamColor",new Color(.85f,.95f,1f,.45f)); }
            var pivot=GameObject.Find("Beam Pivot"); if(pivot) pivot.transform.localRotation=Quaternion.Euler(0,55,0);
            var spot=lights.FirstOrDefault(l=>l.name=="Beam Spot"); if(spot){spot.color=new(.75f,.88f,1);spot.intensity=20;spot.range=50;spot.spotAngle=38;}
            var glow=GameObject.Find("Lamp Glow"); if(glow)glow.GetComponent<SpriteRenderer>().color=new(.85f,.95f,1,1);
            foreach(var camera in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None)){
                var ucd=camera.GetUniversalAdditionalCameraData();
                ucd.renderPostProcessing=true;
                ucd.antialiasing=AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                ucd.antialiasingQuality=AntialiasingQuality.High;
                camera.nearClipPlane=0.15f;
                camera.clearFlags=CameraClearFlags.Skybox;
                camera.backgroundColor=new(.025f,.037f,.045f);
            }
            foreach(string guid in AssetDatabase.FindAssets("t:VolumeProfile",new[]{"Assets/_Project/Settings"}))
            {
                var p=AssetDatabase.LoadAssetAtPath<VolumeProfile>(AssetDatabase.GUIDToAssetPath(guid));
                if(p.TryGet<ColorAdjustments>(out var c)){c.saturation.Override(-15);c.contrast.Override(12);c.postExposure.Override(0f);}
                if(p.TryGet<Vignette>(out var v))v.intensity.Override(0.24f);
                if(p.TryGet<FilmGrain>(out var g))g.intensity.Override(0.12f);
                EditorUtility.SetDirty(p);
            }
            foreach(string guid in AssetDatabase.FindAssets("t:AtmosphereSettings"))
            {
                var s=AssetDatabase.LoadAssetAtPath<AtmosphereSettings>(AssetDatabase.GUIDToAssetPath(guid));
                s.night=NightPreset(s.night);s.storm=NightPreset(s.storm);s.finale=NightPreset(s.finale);
                s.dusk.ambientColor=new(.10f,.13f,.17f);s.dusk.fogColor=new(.06f,.09f,.12f);s.dusk.skyboxExposure=.1f;
                EditorUtility.SetDirty(s);
            }
            EnsureTowerLight("Vestibule Light", new(0, 2.25f, 0), new Color(1f, .82f, .55f), 4.5f, 7f, true);
            EnsureTowerLight("Vestibule Wall Sconce", new(1.2f, 2.0f, 1.9f), new Color(1f, .75f, .45f), 2.0f, 4.5f, false);
            EnsureTowerLight("Storage Alcove Light", new(-2.0f, 2.2f, 1.0f), new Color(1f, .80f, .50f), 3.0f, 5f, false);
            EnsureTowerLight("Library Light", new(0, 4.9f, .3f), new Color(1f, .82f, .55f), 5.0f, 7.5f, false);
            EnsureTowerLight("Library Desk Light", new(-.1f, 4.25f, .62f), new Color(1f, .85f, .5f), 2.5f, 3f, false);
            EnsureTowerLight("Library Stair Light", new(1.5f, 4.3f, 1.5f), new Color(1f, .75f, .45f), 2.0f, 4.5f, false);
            EnsureTowerLight("Archive Workshop Light", new(1.85f, 4.3f, 0.85f), new Color(1f, .82f, .50f), 2.5f, 4f, false);
            EnsureTowerLight("Watch Room Light", new(-.25f, 8.0f, -1.4f), new Color(1f, .82f, .55f), 5.0f, 7.5f, false);
            EnsureTowerLight("Living Room Bedside Light", new(-0.75f, 7.2f, -2.6f), new Color(1f, .82f, .48f), 2.5f, 4f, false);
            EnsureTowerLight("Landing 3 Light", new(0, 11.2f, 0), new Color(1f, .8f, .5f), 3.5f, 6f, false);
            EnsureTowerLight("House Porch Lantern", new(-5f, 2.65f, -9.6f), new Color(1f, .78f, .42f), 4.8f, 9f, true);
            EnsureTowerLight("House Interior Glow", new(-5f, 1.8f, -7.0f), new Color(1f, .78f, .38f), 3.5f, 6.5f, false);
            EnsureTowerLight("Keeper Desk Lamp Light", new(-8.6f, 1.8f, -1.9f), new Color(1f, .82f, .48f), 3.8f, 3f, false);
            EnsureTowerLight("House Kitchen Light", new(-5.85f, 2.3f, -3.5f), new Color(1f, .80f, .45f), 4.5f, 7.5f, false);
            EnsureTowerLight("Stove Hearth Glow", new(-5.45f, .45f, -1.75f), new Color(1f, .50f, .15f), 2.5f, 3.5f, false);
            EnsureTowerLight("Dining Table Lantern", new(-3.04f, 1.05f, -5.64f), new Color(1f, .82f, .45f), 2.8f, 4.0f, false);
        }
        static void EnsureTowerLight(string name, Vector3 pos, Color color, float intensity, float range, bool castShadows = false)
        {
            var go = GameObject.Find(name);
            if (!go)
            {
                go = new GameObject(name, typeof(Light));
                go.transform.SetParent(Group("Lighthouse/Shared/Lighting"), false);
            }
            go.transform.position = pos;
            var l = go.GetComponent<Light>();
            l.type = LightType.Point;
            l.color = color;
            l.intensity = intensity;
            l.range = range;
            l.shadows = castShadows ? LightShadows.Soft : LightShadows.None;
        }
        static AtmospherePreset NightPreset(AtmospherePreset p)
        {
            p.ambientColor=new Color(0.04f, 0.06f, 0.08f);
            p.directionalLightColor=new Color(0.75f, 0.82f, 0.90f);
            p.directionalLightIntensity=0.45f;
            p.fogColor=new Color(0.04f, 0.06f, 0.08f);
            p.fogDensity=0.012f;
            p.skyboxExposure=.05f;
            return p;
        }
    }
}
