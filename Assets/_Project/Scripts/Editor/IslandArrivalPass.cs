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
        static Material mudRoad,puddle,fenceWood,lanternGlow,pineBark,pineLeaf,seaSpray;
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

            mudRoad=ReferenceSurfaceLibrary.Create("Wet Mud Road",new Color(.07f,.052f,.042f),3,.85f,0,.42f);
            puddle=ReferenceSurfaceLibrary.Solid("Mud Puddle",new Color(.025f,.028f,.032f));
            puddle.SetFloat("_Smoothness",.94f);puddle.SetFloat("_Metallic",0f);

            fenceWood=ReferenceSurfaceLibrary.Create("Weathered Fence Timber",new Color(.24f,.22f,.20f),1,.8f,0,.15f);
            lanternGlow=ReferenceSurfaceLibrary.Solid("Dock Lantern Glow",new Color(1f,.78f,.38f),6.0f);
            pineBark=ReferenceSurfaceLibrary.Create("Pine Bark",new Color(.18f,.13f,.10f),1,.85f,0,.12f);
            pineLeaf=ReferenceSurfaceLibrary.Solid("Pine Needles",new Color(.045f,.095f,.055f));
            seaSpray=ReferenceSurfaceLibrary.Solid("Sea Spray Foam",new Color(.92f,.96f,1f,.85f),1.2f);

            const string seaPath="Assets/_Project/Materials/Reference/Harrow Sea.mat";
            water=AssetDatabase.LoadAssetAtPath<Material>(seaPath);if(!water){water=new Material(Shader.Find("TheLightHouse/Harrow Sea"));AssetDatabase.CreateAsset(water,seaPath);}
            water.SetFloat("_WaveHeight",settings.waveHeight);EditorUtility.SetDirty(water);
        }

        public static void ApplyToOpenScene()
        {
            Prepare();
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

            Land();Scrub();Sea();Dock();WindingRoadAndFences();PineTree();Boat();Doors();

            if(act=="Act1")
            {
                var player=GameObject.Find("Player");
                if(player)
                {
                    player.transform.position=new Vector3(-1.1f,.22f,-33.5f);
                    player.transform.rotation=Quaternion.Euler(0,6f,0);
                    var camera=player.GetComponentInChildren<Camera>();
                    if(camera)
                    {
                        camera.fieldOfView=68f;
                        camera.transform.localRotation=Quaternion.Euler(-2.5f,0,0);
                    }
                }
            }
            Debug.Log($"{act}: island, sea, dock lantern, winding cart road, rustic fences, pine tree, boat and doorways authored.");
        }

        static GameObject Box(string name,Transform parent,Vector3 position,Vector3 size,Material material,bool collision=false)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=position;go.transform.localScale=size;go.GetComponent<Renderer>().sharedMaterial=material;
            if(!collision)Object.DestroyImmediate(go.GetComponent<Collider>());else go.tag=material==wood||material==fenceWood?"Wood":"Stone";
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
            float radiusX=26f,radiusZ=32f;
            int count=settings.shoreSegments;
            const int rings=6;
            var points=new List<Vector3>();
            var triangles=new List<int>();

            // Center vertex
            points.Add(new Vector3(0, settings.landHeight + 0.12f, -6f));

            // Concentric terrain rings with harmonic waves and carved dock bay cove
            for (int r = 1; r <= rings; r++)
            {
                float t = r / (float)rings;
                float ringFrac = r == 1 ? 0.20f : r == 2 ? 0.40f : r == 3 ? 0.65f : r == 4 ? 0.88f : r == 5 ? 0.98f : 1.06f;

                for (int i = 0; i < count; i++)
                {
                    float angle = i * Mathf.PI * 2f / count;
                    float variation = 1f + 0.045f * Mathf.Sin(angle * 5f) + 0.030f * Mathf.Cos(angle * 7f) + 0.018f * Mathf.Sin(angle * 11f);
                    float px = Mathf.Sin(angle) * radiusX * ringFrac * variation;
                    float pz = -6f + Mathf.Cos(angle) * radiusZ * ringFrac * variation;

                    float py;
                    // Dock slipway bay cove: around x = -3.2f, z < -23.0f
                    if (px < 0.8f && pz < -23.0f)
                    {
                        float coveT = Mathf.Clamp01((-23.0f - pz) / 3.5f);
                        float coveW = Mathf.Clamp01(1f - Mathf.Abs(px - (-3.2f)) / 5.2f);
                        py = Mathf.Lerp(settings.landHeight, settings.seaLevel - 1.8f, coveT * coveW);
                    }
                    else if (r == rings)
                    {
                        // Underwater perimeter skirt
                        py = settings.seaLevel - 1.4f;
                    }
                    else
                    {
                        // Organic natural wavy undulating ground
                        float wave = 0.14f * Mathf.Sin(px * 0.28f) * Mathf.Cos((pz + 6f) * 0.22f) + 0.08f * Mathf.Sin((px + pz) * 0.45f);
                        float slope = r >= 4 ? Mathf.Lerp(0f, -0.6f, (r - 4) / 2f) : 0f;
                        py = settings.landHeight + wave * (1f - t * 0.2f) + slope;
                    }

                    points.Add(new Vector3(px, py, pz));
                }
            }

            // Triangles from center to ring 1
            for (int i = 0; i < count; i++)
            {
                int next = (i + 1) % count;
                triangles.AddRange(new[] { 0, i + 1, next + 1 });
            }

            // Triangles between concentric rings
            for (int r = 0; r < rings - 1; r++)
            {
                int rowA = 1 + r * count;
                int rowB = 1 + (r + 1) * count;
                for (int i = 0; i < count; i++)
                {
                    int next = (i + 1) % count;
                    int a = rowA + i;
                    int b = rowA + next;
                    int c = rowB + i;
                    int d = rowB + next;
                    triangles.AddRange(new[] { a, c, b, b, c, d });
                }
            }

            MeshObject("Harrow Rock Island", SaveMesh("Island", points, triangles), stone, root, true);

            // Rocky shore boulders along water perimeter
            var random=new System.Random(84);
            for(int i=0;i<settings.shoreRockCount;i++)
            {
                float a=i*Mathf.PI*2/settings.shoreRockCount;if(Mathf.Abs(Mathf.DeltaAngle(a*Mathf.Rad2Deg,180))<13)continue;
                var rock=GameObject.CreatePrimitive(PrimitiveType.Sphere);rock.name="Shore Boulder";rock.transform.SetParent(root);rock.transform.position=new(Mathf.Sin(a)*radiusX*.94f,settings.seaLevel-.05f,-6+Mathf.Cos(a)*radiusZ*.94f);rock.transform.localScale=new(1.6f+(float)random.NextDouble()*2,1.1f,1.7f);rock.GetComponent<Renderer>().sharedMaterial=stone;Object.DestroyImmediate(rock.GetComponent<Collider>());
            }
            // Coastal hillside boulders specifically positioned away from the cart track and dock
            for(int i=0;i<10;i++)
            {
                float side=i%2==0?-1f:1f;
                float bx=side*(3.4f+(float)random.NextDouble()*5.0f);
                float bz=-13f-(float)random.NextDouble()*14f;
                if(bx<0 && bz<-22f)continue; // Keep dock, boat and shore approach clear
                var rock=GameObject.CreatePrimitive(PrimitiveType.Sphere);rock.name="Hillside Boulder";rock.transform.SetParent(root,false);
                rock.transform.position=new Vector3(bx,settings.landHeight+.15f,bz);
                rock.transform.localScale=new Vector3(1.6f+(float)random.NextDouble()*1.2f,1.0f+(float)random.NextDouble()*.6f,1.6f);
                rock.GetComponent<Renderer>().sharedMaterial=stone;Object.DestroyImmediate(rock.GetComponent<Collider>());
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
            var dockGroup=new GameObject("Dock Jetty").transform;dockGroup.SetParent(root,false);
            float dockX=-3.2f;
            // Longitudinal heavy timber bearers under dock planks
            for(int b=-1;b<=1;b++)Box("Dock Bearer",dockGroup,new(dockX+b*1.1f,settings.landHeight-.12f,-31.0f),new(.22f,.26f,10.5f),wood);

            // Weathered wooden dock planks
            const int plankCount=36;
            for(int i=0;i<plankCount;i++)
            {
                float z=-26.0f-i*.28f;
                Box("Dock Plank",dockGroup,new(dockX,settings.landHeight+.03f,z),new(2.8f,.12f,.25f),wood,true);
            }

            // Mooring piles and rope loops along dock perimeter
            float[] pileZ=new[]{-26.0f,-28.5f,-31.0f,-33.5f,-35.8f};
            foreach(float pz in pileZ)
            {
                for(int side=-1;side<=1;side+=2)
                {
                    float px=dockX+side*1.35f;
                    Box("Dock Mooring Pile",dockGroup,new(px,.45f,pz),new(.20f,1.75f,.20f),wood,true);
                    Box("Mooring Cap",dockGroup,new(px,1.35f,pz),new(.22f,.06f,.22f),iron);
                }
            }
            for(int i=0;i<pileZ.Length-1;i++)
            {
                for(int side=-1;side<=1;side+=2)
                {
                    float px=dockX+side*1.35f;
                    Rod("Mooring Line",dockGroup,new(px,.95f,pileZ[i]),new(px,.95f,pileZ[i+1]),.04f,iron);
                }
            }

            // Gallows Lamp Post positioned prominently at the dock head (x = -2.6, z = -28.5)
            var gallows=new GameObject("Gallows Lamp Post").transform;gallows.SetParent(dockGroup,false);
            Vector3 postBase=new(-2.6f,settings.landHeight,-28.5f);
            Box("Post Footing",gallows,postBase+new Vector3(0,.15f,0),new(.34f,.30f,.34f),stone,true);
            Box("Vertical Post",gallows,postBase+new Vector3(0,1.75f,0),new(.20f,3.2f,.20f),wood,true);
            Box("Horizontal Crossarm",gallows,postBase+new Vector3(.60f,3.30f,0),new(1.25f,.16f,.16f),wood);
            Rod("Knee Brace",gallows,postBase+new Vector3(0,2.5f,0),postBase+new Vector3(.60f,3.25f,0),.11f,wood);

            // Hanging iron chain and detailed hurricane storm lantern
            Vector3 lanternDrop=postBase+new Vector3(1.10f,3.30f,0);
            Rod("Lantern Chain",gallows,lanternDrop,lanternDrop+Vector3.down*.45f,.03f,iron);
            Vector3 lanternCenter=lanternDrop+Vector3.down*.72f;
            Box("Lantern Vented Hood",gallows,lanternCenter+Vector3.up*.20f,new(.26f,.07f,.26f),iron);
            Box("Lantern Glass Chimney",gallows,lanternCenter,new(.18f,.32f,.18f),lanternGlow);
            Box("Lantern Oil Reservoir",gallows,lanternCenter+Vector3.down*.18f,new(.22f,.07f,.22f),iron);
            for(int i=-1;i<=1;i+=2)
            {
                Rod("Cage Rib X",gallows,lanternCenter+new Vector3(i*.10f,-.16f,-.10f),lanternCenter+new Vector3(i*.10f,.18f,-.10f),.02f,iron);
                Rod("Cage Rib Z",gallows,lanternCenter+new Vector3(-.10f,-.16f,i*.10f),lanternCenter+new Vector3(-.10f,.18f,i*.10f),.02f,iron);
            }

            // Rich warm golden light pool illuminating dock, boat, and road entrance
            var lightGo=new GameObject("Dock Lantern Light",typeof(Light));lightGo.transform.SetParent(gallows,false);
            lightGo.transform.position=lanternCenter;
            var l=lightGo.GetComponent<Light>();
            l.type=LightType.Point;l.color=new Color(1f,.78f,.40f);l.intensity=15.0f;l.range=20f;l.shadows=LightShadows.None;

            // Shoreline boulders along the water line away from the boat berth
            var random=new System.Random(42);
            for(int i=0;i<6;i++)
            {
                float rx = i < 3 ? (-7.8f - (float)random.NextDouble() * 2.2f) : (0.5f + (float)random.NextDouble() * 2.5f);
                float rz = -25f - (float)random.NextDouble() * 7f;
                var rock=GameObject.CreatePrimitive(PrimitiveType.Sphere);rock.name="Dock Shore Rock";rock.transform.SetParent(dockGroup,false);
                rock.transform.position=new Vector3(rx,settings.seaLevel+.15f,rz);
                rock.transform.localScale=new Vector3(1.4f+(float)random.NextDouble()*1.2f,.8f+(float)random.NextDouble()*.6f,1.5f);
                rock.GetComponent<Renderer>().sharedMaterial=stone;Object.DestroyImmediate(rock.GetComponent<Collider>());
            }
        }

        static void WindingRoadAndFences()
        {
            var roadGroup=new GameObject("Winding Coastal Road").transform;roadGroup.SetParent(root,false);
            const int n=28;
            Vector3[] center=new Vector3[n+1];
            Vector3[] tangent=new Vector3[n+1];
            Vector3[] normal=new Vector3[n+1];

            for(int i=0;i<=n;i++)
            {
                float t=i/(float)n;
                float z=Mathf.Lerp(-34.0f,-10.5f,t);
                float x=-.8f+2.2f*Mathf.Sin(t*Mathf.PI)-.4f*t;
                float y=Mathf.Lerp(settings.landHeight+.03f,settings.landHeight+.06f,t);
                center[i]=new Vector3(x,y,z);
            }
            for(int i=0;i<=n;i++)
            {
                Vector3 fwd;
                if(i<n)fwd=(center[i+1]-center[i]).normalized;
                else fwd=(center[n]-center[n-1]).normalized;
                tangent[i]=fwd;
                normal[i]=Vector3.Cross(Vector3.up,fwd).normalized;
            }

            // Road surface mesh: dark wet churned mud track
            var roadVerts=new List<Vector3>();var roadTris=new List<int>();var roadUVs=new List<Vector2>();
            float halfWidth=1.5f;
            for(int i=0;i<=n;i++)
            {
                float t=i/(float)n;
                Vector3 c=center[i],norm=normal[i];
                Vector3 p0=c-norm*halfWidth;
                Vector3 p1=c-norm*.68f+Vector3.down*.02f; // left wheel rut
                Vector3 p2=c+Vector3.up*.03f;             // crowned center
                Vector3 p3=c+norm*.68f+Vector3.down*.02f;  // right wheel rut
                Vector3 p4=c+norm*halfWidth;
                roadVerts.AddRange(new[]{p0,p1,p2,p3,p4});
                roadUVs.AddRange(new[]{new Vector2(0,t*6),new Vector2(.25f,t*6),new Vector2(.5f,t*6),new Vector2(.75f,t*6),new Vector2(1,t*6)});
            }
            for(int i=0;i<n;i++)
            {
                int rowA=i*5,rowB=(i+1)*5;
                for(int s=0;s<4;s++)
                {
                    int a=rowA+s,b=rowA+s+1,c=rowB+s,d=rowB+s+1;
                    roadTris.AddRange(new[]{a,c,b,b,c,d});
                }
            }
            MeshObject("Mud Cart Road",SaveMesh("CartRoad",roadVerts,roadTris,roadUVs),mudRoad,roadGroup,true);

            // Rustic wooden post-and-rail fences on both sides of the cart road
            var fenceGroup=new GameObject("Rustic Fences").transform;fenceGroup.SetParent(root,false);
            int postStep=3;
            List<Vector3> leftPosts=new();List<Vector3> rightPosts=new();

            for(int i=0;i<=n;i+=postStep)
            {
                Vector3 c=center[i],norm=normal[i];
                float postLean=(i%2==0?1:-1)*2.2f;
                // Left fence post
                Vector3 lPos=c-norm*(halfWidth+.16f);leftPosts.Add(lPos);
                var lp=Box("Fence Post Left",fenceGroup,lPos+Vector3.up*.55f,new(.14f,1.15f,.14f),fenceWood,true);
                lp.transform.localRotation=Quaternion.Euler(postLean,0,postLean*.5f);

                // Right fence post
                Vector3 rPos=c+norm*(halfWidth+.16f);rightPosts.Add(rPos);
                var rp=Box("Fence Post Right",fenceGroup,rPos+Vector3.up*.55f,new(.14f,1.15f,.14f),fenceWood,true);
                rp.transform.localRotation=Quaternion.Euler(-postLean,0,-postLean*.5f);
            }

            // Horizontal twin split rails connecting posts
            for(int i=0;i<leftPosts.Count-1;i++)
            {
                Vector3 la=leftPosts[i],lb=leftPosts[i+1];
                Rod("Left Rail Top",fenceGroup,la+Vector3.up*.92f,lb+Vector3.up*.92f,.09f,fenceWood);
                Rod("Left Rail Bottom",fenceGroup,la+Vector3.up*.46f,lb+Vector3.up*.46f,.09f,fenceWood);

                Vector3 ra=rightPosts[i],rb=rightPosts[i+1];
                Rod("Right Rail Top",fenceGroup,ra+Vector3.up*.92f,rb+Vector3.up*.92f,.09f,fenceWood);
                Rod("Right Rail Bottom",fenceGroup,ra+Vector3.up*.46f,rb+Vector3.up*.46f,.09f,fenceWood);

                if(i%3==1)
                {
                    Rod("Left Support Stake",fenceGroup,la+Vector3.up*.85f,la+normal[i*postStep]*(-.60f),.08f,fenceWood);
                    Rod("Right Support Stake",fenceGroup,ra+Vector3.up*.85f,ra+normal[i*postStep]*(.60f),.08f,fenceWood);
                }
            }
        }

        static void PineTree()
        {
            var tree=new GameObject("Coastal Gnarled Pine").transform;tree.SetParent(root,false);
            Vector3 basePos=new(5.2f,settings.landHeight+.05f,-27.5f);

            // Gnarled leaning trunk segments curving across the right side of the frame
            Vector3 t0=basePos;
            Vector3 t1=basePos+new Vector3(-.5f,2.0f,.2f);
            Vector3 t2=basePos+new Vector3(-.9f,3.8f,.4f);
            Vector3 t3=basePos+new Vector3(-1.3f,5.4f,.6f);
            Vector3 t4=basePos+new Vector3(-1.7f,6.8f,.8f);

            Rod("Pine Trunk 1",tree,t0,t1,.40f,pineBark);
            Rod("Pine Trunk 2",tree,t1,t2,.32f,pineBark);
            Rod("Pine Trunk 3",tree,t2,t3,.24f,pineBark);
            Rod("Pine Trunk 4",tree,t3,t4,.16f,pineBark);

            // Buttress roots
            Rod("Pine Root 1",tree,t0+new Vector3(.1f,.2f,0),t0+new Vector3(.8f,-.15f,.35f),.20f,pineBark);
            Rod("Pine Root 2",tree,t0+new Vector3(-.1f,.2f,0),t0+new Vector3(-.7f,-.15f,-.4f),.18f,pineBark);

            // Twisted branches reaching out into the upper right camera frame
            Vector3 b1_end=t2+new Vector3(-1.2f,.9f,-.6f);
            Rod("Pine Bough 1",tree,t2,b1_end,.16f,pineBark);
            Vector3 b1_sub1=b1_end+new Vector3(-.7f,.5f,-.4f);
            Vector3 b1_sub2=b1_end+new Vector3(-.3f,.8f,.3f);
            Rod("Pine Branch 1A",tree,b1_end,b1_sub1,.10f,pineBark);
            Rod("Pine Branch 1B",tree,b1_end,b1_sub2,.09f,pineBark);

            Vector3 b2_end=t3+new Vector3(-1.0f,.8f,.2f);
            Rod("Pine Bough 2",tree,t3,b2_end,.14f,pineBark);
            Vector3 b2_sub=b2_end+new Vector3(-.5f,.5f,-.3f);
            Rod("Pine Branch 2A",tree,b2_end,b2_sub,.08f,pineBark);

            Vector3 b3_end=t1+new Vector3(-.8f,.6f,-.6f);
            Rod("Pine Bough 3",tree,t1,b3_end,.14f,pineBark);

            // Organic layered needle clumps scattered along branches framing the upper right
            void Needle(Vector3 p,Vector3 s)
            {
                var f=GameObject.CreatePrimitive(PrimitiveType.Sphere);f.name="Pine Needle Cluster";f.transform.SetParent(tree,false);
                f.transform.position=p;f.transform.localScale=s;f.GetComponent<Renderer>().sharedMaterial=pineLeaf;Object.DestroyImmediate(f.GetComponent<Collider>());
            }

            Needle(b1_sub1,new(1.1f,.45f,.9f));
            Needle(b1_sub2,new(1.0f,.40f,.85f));
            Needle(b1_end+Vector3.up*.25f,new(1.2f,.50f,1.0f));
            Needle(b2_sub,new(1.3f,.45f,1.1f));
            Needle(b2_end+Vector3.up*.3f,new(1.4f,.50f,1.2f));
            Needle(t4+new Vector3(-.2f,.25f,0),new(1.5f,.55f,1.3f));
            Needle(t4+new Vector3(.3f,.3f,.2f),new(1.2f,.45f,1.0f));
            Needle(b3_end,new(.9f,.35f,.8f));
        }

        static void Boat()
        {
            var boat=GameObject.Find("Boat");if(!boat)return;
            foreach(var r in boat.GetComponents<Renderer>())r.enabled=false;foreach(var c in boat.GetComponents<Collider>())c.enabled=false;
            var previous=boat.transform.Find("Boat Visuals");if(previous)Object.DestroyImmediate(previous.gameObject);
            Vector3 boatPos=new(-5.8f,settings.seaLevel+.12f,-29.8f);
            boat.transform.localScale=Vector3.one;boat.transform.position=boatPos;boat.transform.rotation=Quaternion.Euler(1.2f,18f,-2.2f);
            var visuals=new GameObject("Boat Visuals").transform;visuals.SetParent(boat.transform,false);
            var points=new List<Vector3>();var triangles=new List<int>();const int sections=12;
            for(int i=0;i<=sections;i++)
            {
                float t=i/(float)sections,z=Mathf.Lerp(-3,3,t),width=.12f+1.05f*Mathf.Pow(Mathf.Max(0,Mathf.Sin(t*Mathf.PI)),.6f);
                points.AddRange(new[]{new Vector3(-width,.42f,z),new Vector3(-width*.65f,-.18f,z),new Vector3(0,-.48f,z),new Vector3(width*.65f,-.18f,z),new Vector3(width,.42f,z)});
            }
            for(int i=0;i<sections;i++)for(int j=0;j<4;j++){int k=i*5+j;triangles.AddRange(new[]{k,k+5,k+1,k+1,k+5,k+6});}
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

            // Sea foam crests around the clinker hull floating in ocean water
            for(int f=0;f<4;f++)
            {
                float fz=-1.8f+f*1.2f;
                var foamL=GameObject.CreatePrimitive(PrimitiveType.Cylinder);foamL.name=$"Boat Foam L {f}";foamL.transform.SetParent(visuals,false);
                foamL.transform.localPosition=new(-1.25f,-.15f,fz);foamL.transform.localScale=new(.8f,.04f,1.1f);
                foamL.GetComponent<Renderer>().sharedMaterial=seaSpray;Object.DestroyImmediate(foamL.GetComponent<Collider>());
                var foamR=GameObject.CreatePrimitive(PrimitiveType.Cylinder);foamR.name=$"Boat Foam R {f}";foamR.transform.SetParent(visuals,false);
                foamR.transform.localPosition=new(1.25f,-.15f,fz);foamR.transform.localScale=new(.8f,.04f,1.1f);
                foamR.GetComponent<Renderer>().sharedMaterial=seaSpray;Object.DestroyImmediate(foamR.GetComponent<Collider>());
            }

            // Mooring line to dock pile
            Rod("Dock Mooring Line",root,boatPos+new Vector3(.85f,.45f,1.2f),new Vector3(-4.55f,settings.landHeight+.7f,-31.0f),.035f,iron);

            var controller=Object.FindAnyObjectByType<Boatman>();if(controller){var so=new SerializedObject(controller);so.FindProperty("departureOffset").vector3Value=settings.departureOffset;so.ApplyModifiedPropertiesWithoutUndo();}
        }

        static void FigurePart(string name,Transform parent,PrimitiveType type,Vector3 p,Vector3 scale,Material material)
        {
            var go=GameObject.CreatePrimitive(type);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=p;go.transform.localScale=scale;go.GetComponent<Renderer>().sharedMaterial=material;Object.DestroyImmediate(go.GetComponent<Collider>());
        }

        static void Doors()
        {
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
            Box("Hinge Strap Top",leaf.transform,new(-width*.22f,height*.32f,0),new(width*.52f,.065f,.16f),iron);
            Box("Hinge Strap Bottom",leaf.transform,new(-width*.22f,-height*.32f,0),new(width*.52f,.065f,.16f),iron);
            var door=go.GetComponent<Door>();var so=new SerializedObject(door);so.FindProperty("doorId").stringValue=id;so.FindProperty("locked").boolValue=false;so.FindProperty("openAngle").floatValue=-105;
            if(name=="Tower Door")so.FindProperty("blockDuringActTwo").boolValue=false;
            so.ApplyModifiedPropertiesWithoutUndo();
            Quaternion rotation=Quaternion.Euler(0,yaw,0);
            for(int i=0;i<2;i++)Box("Masonry Door Reveal",root,hinge+rotation*new Vector3(i==0?-.4f:width+.4f,height*.5f,.15f),new(.64f,height+.12f,.6f),stone,true).transform.rotation=rotation;
            for(int i=0;i<2;i++)Box("Fixed Door Jamb",root,hinge+rotation*new Vector3(i==0?-.09f:width+.09f,height*.5f,0),new(.16f,height+.12f,.25f),wood,true).transform.rotation=rotation;
            Box("Fixed Door Header",root,hinge+rotation*new Vector3(width*.5f,height+.08f,0),new(width+.34f,.16f,.25f),wood,true).transform.rotation=rotation;
            Box("Door Threshold",root,hinge+rotation*new Vector3(width*.5f,-.03f,0),new(width+.18f,.07f,.34f),stone,true).transform.rotation=rotation;
        }
    }
}