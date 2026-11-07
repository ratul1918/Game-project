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
        static Mesh rockMesh;
        static Material rockMat;
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
            wood=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/PBR/PBR_WeatheredPlanks.mat");
            if(!wood)wood=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Reference/Old Timber.mat");
            iron=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/PBR/PBR_RustedIron.mat");
            if(!iron)iron=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Reference/Oxidised Iron.mat");
            stone=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Level/CoastalIsland.mat");
            if(!stone)stone=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/PBR/PBR_CoastalRock.mat");
            coat=ReferenceSurfaceLibrary.Solid("Boatman Oilskin",new(.065f,.08f,.07f));
            skin=ReferenceSurfaceLibrary.Solid("Boatman Skin",new(.3f,.27f,.22f));

            mudRoad=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Reference/Wet Mud Road.mat");
            if(!mudRoad)
            {
                mudRoad=new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(mudRoad,"Assets/_Project/Materials/Reference/Wet Mud Road.mat");
            }
            var mudDiff=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Project/Art/Textures/Environment/CoastalMud_diff.png");
            var mudNor=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Project/Art/Textures/Environment/CoastalMud_nor.png");
            var mudMask=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Project/Art/Textures/Environment/CoastalMud_mask.png");
            if(mudDiff)mudRoad.SetTexture("_BaseMap",mudDiff);
            if(mudNor){mudRoad.SetTexture("_BumpMap",mudNor);mudRoad.EnableKeyword("_NORMALMAP");}
            if(mudMask){mudRoad.SetTexture("_MetallicGlossMap",mudMask);mudRoad.EnableKeyword("_METALLICSPECGLOSSMAP");}
            mudRoad.SetColor("_BaseColor",new Color(.20f,.18f,.16f));
            mudRoad.SetFloat("_Smoothness",.88f);

            puddle=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Reference/FloorWaterPuddle.mat");
            if(!puddle)puddle=ReferenceSurfaceLibrary.Solid("Mud Puddle",new Color(.025f,.028f,.032f));
            puddle.SetFloat("_Smoothness",.96f);puddle.SetFloat("_Metallic",0f);

            fenceWood=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/PBR/PBR_WeatheredPlanks.mat");
            if(!fenceWood)fenceWood=ReferenceSurfaceLibrary.Create("Weathered Fence Timber",new Color(.24f,.22f,.20f),1,.8f,0,.15f);
            lanternGlow=ReferenceSurfaceLibrary.Solid("Dock Lantern Glow",new Color(1f,.82f,.55f),3.5f);
            pineBark=ReferenceSurfaceLibrary.Create("Pine Bark",new Color(.18f,.13f,.10f),1,.85f,0,.12f);
            pineLeaf=ReferenceSurfaceLibrary.Solid("Pine Needles",new Color(.045f,.095f,.055f));
            seaSpray=ReferenceSurfaceLibrary.Solid("Sea Spray Foam",new Color(.92f,.96f,1f,.85f),1.2f);

            var allRockAssets=AssetDatabase.LoadAllAssetsAtPath("Assets/_Project/Art/Models/CoastalRock_01/CoastalRock_01.fbx");
            foreach(var a in allRockAssets){if(a.name=="coast_rocks_01_LOD2"||a.name=="coast_rocks_01_LOD3"){rockMesh=a as Mesh;if(a.name=="coast_rocks_01_LOD2")break;}}
            rockMat=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Models/CoastalRock_01/M_CoastalRock_01.mat");
            if(!rockMat)rockMat=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/PBR/PBR_CoastalRock.mat");

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
            foreach(var c in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if(c && (c.name=="Coast Dressing" || c.name=="CoastDressing")) c.gameObject.SetActive(false);
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
            float radiusX = 26f, radiusZ = 32f;
            int count = 64; // High vertex density (was 32)
            const int rings = 16; // High radial resolution (was 6)
            var points = new List<Vector3>();
            var triangles = new List<int>();

            // Center vertex
            points.Add(new Vector3(0, settings.landHeight + 0.05f, -6f));

            // Concentric terrain rings with terraced coastal inclines
            for (int r = 1; r <= rings; r++)
            {
                float t = r / (float)rings;
                float ringFrac = Mathf.Lerp(0.08f, 1.05f, Mathf.Pow(t, 0.88f));

                for (int i = 0; i < count; i++)
                {
                    float angle = i * Mathf.PI * 2f / count;
                    float variation = 1f + 0.030f * Mathf.Sin(angle * 5f) + 0.020f * Mathf.Cos(angle * 7f) + 0.012f * Mathf.Sin(angle * 11f);
                    float px = Mathf.Sin(angle) * radiusX * ringFrac * variation;
                    float pz = -6f + Mathf.Cos(angle) * radiusZ * ringFrac * variation;

                    float py;
                    if (r == rings)
                    {
                        // Underwater perimeter skirt
                        py = settings.seaLevel - 1.4f;
                    }
                    else
                    {
                        // Base elevation with gentle natural variation (NO large sinusoidal bulging)
                        float baseUndulation = 0.025f * Mathf.Sin(px * 0.35f) * Mathf.Cos((pz + 6f) * 0.35f);
                        float perimeterSlope = r >= 11 ? Mathf.Lerp(0f, -0.65f, (r - 11) / 5f) : 0f;
                        py = settings.landHeight + baseUndulation + perimeterSlope;

                        // Cart road approach tracking (z = -35.0 to -10.0)
                        float roadT = Mathf.Clamp01((-34.0f - pz) / -23.5f);
                        float pathCenter = -0.8f + 2.2f * Mathf.Sin(roadT * Mathf.PI) - 0.4f * roadT;
                        float distToPath = Mathf.Abs(px - pathCenter);
                        float roadY = Mathf.Lerp(settings.landHeight + 0.02f, settings.landHeight + 0.05f, 1f - roadT);

                        if (pz >= -35.0f && pz <= -10.0f)
                        {
                            if (distToPath < 2.0f)
                            {
                                // Flat gradual incline along the road surface
                                float roadCamber = 0.015f * (1f - Mathf.Pow(distToPath / 2.0f, 2f));
                                py = roadY + roadCamber;
                            }
                            else if (px < pathCenter - 2.0f)
                            {
                                // --- GENTLE ROCKY TIDAL SHELF ---
                                // Replaces any earthen blobs/dunes with a low rocky shelf sloping naturally into the sea (Ref: Screenshots 15, 16)
                                float embankDist = (pathCenter - 2.0f) - px;
                                float maxSpan = 14.0f;
                                float embankFrac = Mathf.Clamp01(embankDist / maxSpan);

                                float targetShoreY = settings.seaLevel + 0.05f;
                                py = Mathf.Lerp(roadY - 0.04f, targetShoreY, Mathf.Pow(embankFrac, 0.75f));
                                py = Mathf.Min(py, roadY - 0.03f);
                            }
                            else if (px > pathCenter + 2.0f)
                            {
                                // Right gentle slope
                                float rightDist = px - (pathCenter + 2.0f);
                                float rightFrac = Mathf.Clamp01(rightDist / 12.0f);
                                py = Mathf.Lerp(roadY, settings.landHeight + 0.08f, rightFrac);
                            }
                        }

                        // Dock slipway cove & boat berth (pz < -24.0f, px in [-20.0, 1.0])
                        if (px < 1.0f && px > -22.0f && pz < -24.0f)
                        {
                            float coveT = Mathf.Clamp01((-24.0f - pz) / 6.0f);
                            float coveW = px >= -3.2f
                                ? Mathf.Clamp01(1f - (px - (-3.2f)) / 3.8f)
                                : Mathf.Clamp01((px - (-22.0f)) / 6.0f);
                            float coveDepth = settings.seaLevel - 0.45f;
                            py = Mathf.Lerp(py, coveDepth, coveT * coveW);
                        }

                        // Courtyard elevation protection
                        if (px > -14.5f && px < 11.5f && pz > -16.5f && pz < -6.5f)
                        {
                            py = Mathf.Min(py, -0.05f);
                        }
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

            // Realistic coastal photogrammetry boulders around water perimeter
            var random = new System.Random(84);
            int rockCount = 14;
            for (int i = 0; i < rockCount; i++)
            {
                float a = i * Mathf.PI * 2 / rockCount;
                float rx = Mathf.Sin(a) * radiusX * .94f;
                float rz = -6 + Mathf.Cos(a) * radiusZ * .94f;
                if (rx > -14.0f && rx < 2.5f && rz < -20.0f) continue;
                if (Vector2.Distance(new Vector2(rx, rz), new Vector2(-5.8f, -29.8f)) < 8.0f) continue;

                var rock = new GameObject("Shore Boulder", typeof(MeshFilter), typeof(MeshRenderer));
                rock.transform.SetParent(root, false);
                rock.transform.position = new Vector3(rx, settings.seaLevel - 0.12f, rz);
                float yaw = (float)random.NextDouble() * 360f;
                float tilt = (float)(random.NextDouble() * 14.0 - 7.0);
                rock.transform.rotation = Quaternion.Euler(tilt, yaw, tilt * 0.5f);
                float s = 0.35f + (float)random.NextDouble() * 0.25f;
                rock.transform.localScale = new Vector3(s, s * 0.85f, s);
                rock.GetComponent<MeshFilter>().sharedMesh = rockMesh;
                rock.GetComponent<MeshRenderer>().sharedMaterial = rockMat;
            }

            // Scatter detailed embankment rocks, driftwood, puddles, decals, and grass tufts
            ScatterShorelineAndEmbankmentDetails();
        }

        static void ScatterShorelineAndEmbankmentDetails()
        {
            var detailGroup = new GameObject("Shoreline & Embankment Dressing").transform;
            detailGroup.SetParent(root, false);

            var random = new System.Random(117);

            // 1. Photogrammetry jagged cliff rocks along the left embankment
            for (int i = 0; i < 22; i++)
            {
                float t = i / 21f;
                float pz = Mathf.Lerp(-33.0f, -11.0f, t) + (float)(random.NextDouble() * 1.5 - 0.75);
                float roadT = Mathf.Clamp01((-34.0f - pz) / -23.5f);
                float pathCenter = -0.8f + 2.2f * Mathf.Sin(roadT * Mathf.PI) - 0.4f * roadT;

                float offset = 2.4f + (float)random.NextDouble() * 5.5f;
                float px = pathCenter - offset;

                float roadY = Mathf.Lerp(settings.landHeight + 0.02f, settings.landHeight + 0.05f, 1f - roadT);
                float embankFrac = Mathf.Clamp01(offset / 14f);
                float linearSlope = Mathf.Lerp(roadY, settings.seaLevel + 0.05f, embankFrac);
                float py = linearSlope - 0.06f; // Partially sunken

                var rock = new GameObject($"Embankment Rock {i}", typeof(MeshFilter), typeof(MeshRenderer));
                rock.transform.SetParent(detailGroup, false);
                rock.transform.position = new Vector3(px, py, pz);
                float yaw = (float)random.NextDouble() * 360f;
                float tilt = (float)(random.NextDouble() * 24.0 - 12.0);
                rock.transform.rotation = Quaternion.Euler(tilt, yaw, tilt * 0.7f);
                float s = 0.16f + (float)random.NextDouble() * 0.28f;
                rock.transform.localScale = new Vector3(s * 1.15f, s * 0.75f, s * 0.95f);
                rock.GetComponent<MeshFilter>().sharedMesh = rockMesh;
                rock.GetComponent<MeshRenderer>().sharedMaterial = rockMat;
            }

            // 2. Weathered Decaying Driftwood Planks & Timber logs along the shoreline & embankment
            var woodMat = wood;
            for (int i = 0; i < 10; i++)
            {
                float t = i / 9f;
                float pz = Mathf.Lerp(-33.5f, -14.0f, t) + (float)(random.NextDouble() * 2.0 - 1.0);
                float roadT = Mathf.Clamp01((-34.0f - pz) / -23.5f);
                float pathCenter = -0.8f + 2.2f * Mathf.Sin(roadT * Mathf.PI) - 0.4f * roadT;
                float px = pathCenter - (2.6f + (float)random.NextDouble() * 4.5f);
                float roadY = Mathf.Lerp(settings.landHeight + 0.02f, settings.landHeight + 0.05f, 1f - roadT);
                float py = Mathf.Lerp(roadY - 0.02f, settings.seaLevel + 0.08f, Mathf.Clamp01((pathCenter - 2f - px) / 12f));

                float logLen = 1.4f + (float)random.NextDouble() * 1.8f;
                float logWidth = 0.16f + (float)random.NextDouble() * 0.12f;
                var plank = Box($"Driftwood Plank {i}", detailGroup, new Vector3(px, py + 0.03f, pz), new Vector3(logWidth, 0.08f, logLen), woodMat, true);
                float yaw = (float)random.NextDouble() * 160f - 80f;
                float roll = (float)(random.NextDouble() * 10f - 5f);
                plank.transform.localRotation = Quaternion.Euler(roll, yaw, 0f);
            }

            // 3. Shoreline & Embankment Dressing (no mud required at spawn)
            // Water Puddle Pools and Wet Mud Decals removed per spawn requirements.

            // 5. Tufts of wet dead coastal marram grass along fence posts, stone walls, and rock bases
            var grassMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Reference/CoastalMarramGrass.mat");
            if (grassMat)
            {
                for (int i = 0; i < 28; i++)
                {
                    float t = i / 27f;
                    float pz = Mathf.Lerp(-33.0f, -10.0f, t) + (float)(random.NextDouble() * 1.0 - 0.5);
                    float roadT = Mathf.Clamp01((-34.0f - pz) / -23.5f);
                    float pathCenter = -0.8f + 2.2f * Mathf.Sin(roadT * Mathf.PI) - 0.4f * roadT;

                    float side = (i % 2 == 0) ? -1f : 1f;
                    float px = pathCenter + side * (1.75f + (float)random.NextDouble() * 0.65f);
                    float roadY = Mathf.Lerp(settings.landHeight + 0.02f, settings.landHeight + 0.05f, 1f - roadT);

                    var grassGroup = new GameObject($"Marram Grass Clump {i}").transform;
                    grassGroup.SetParent(detailGroup, false);
                    grassGroup.position = new Vector3(px, roadY + 0.01f, pz);

                    for (int q = 0; q < 2; q++)
                    {
                        var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                        quad.name = $"Blade Quad {q}";
                        quad.transform.SetParent(grassGroup, false);
                        quad.transform.localPosition = new Vector3(0, 0.28f, 0);
                        quad.transform.localRotation = Quaternion.Euler(0, q * 90f + (float)random.NextDouble() * 30f, 0);
                        quad.transform.localScale = new Vector3(0.55f, 0.58f, 1f);
                        quad.GetComponent<Renderer>().sharedMaterial = grassMat;
                        Object.DestroyImmediate(quad.GetComponent<Collider>());
                    }
                }
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

            // Gallows Lamp Post positioned prominently at the dock head (x = -2.85, z = -28.5)
            // Scaled to standard human proportion: ~2.9m total height (Ref: Task requirement 2.8m - 3.2m)
            var gallows=new GameObject("Gallows Lamp Post").transform;gallows.SetParent(dockGroup,false);
            Vector3 postBase=new(-2.85f,settings.landHeight,-28.5f);
            Box("Post Footing",gallows,postBase+new Vector3(0,.08f,0),new(.24f,.16f,.24f),stone,true);
            Box("Vertical Post",gallows,postBase+new Vector3(0,1.43f,0),new(.14f,2.70f,.14f),wood,true);
            Box("Horizontal Crossarm",gallows,postBase+new Vector3(.45f,2.72f,0),new(.90f,.12f,.12f),wood);
            Rod("Knee Brace",gallows,postBase+new Vector3(0,2.15f,0),postBase+new Vector3(.45f,2.68f,0),.08f,wood);

            // Hanging iron chain and hurricane storm lantern (eye-level glow ~2.25m)
            Vector3 lanternDrop=postBase+new Vector3(.75f,2.68f,0);
            Rod("Lantern Chain",gallows,lanternDrop,lanternDrop+Vector3.down*.25f,.025f,iron);
            Vector3 lanternCenter=lanternDrop+Vector3.down*.42f;
            Box("Lantern Vented Hood",gallows,lanternCenter+Vector3.up*.15f,new(.20f,.05f,.20f),iron);
            Box("Lantern Glass Chimney",gallows,lanternCenter,new(.14f,.24f,.14f),lanternGlow);
            Box("Lantern Oil Reservoir",gallows,lanternCenter+Vector3.down*.14f,new(.16f,.05f,.16f),iron);
            Rod("Cage Rib 1",gallows,lanternCenter+new Vector3(-.07f,-.12f,-.07f),lanternCenter+new Vector3(-.07f,.14f,-.07f),.016f,iron);
            Rod("Cage Rib 2",gallows,lanternCenter+new Vector3( .07f,-.12f,-.07f),lanternCenter+new Vector3( .07f,.14f,-.07f),.016f,iron);
            Rod("Cage Rib 3",gallows,lanternCenter+new Vector3(-.07f,-.12f, .07f),lanternCenter+new Vector3(-.07f,.14f, .07f),.016f,iron);
            Rod("Cage Rib 4",gallows,lanternCenter+new Vector3( .07f,-.12f, .07f),lanternCenter+new Vector3( .07f,.14f, .07f),.016f,iron);

            // Rich warm golden light pool illuminating dock, boat, and road entrance
            var lightGo=new GameObject("Dock Lantern Light",typeof(Light));lightGo.transform.SetParent(gallows,false);
            lightGo.transform.position=lanternCenter;
            var l=lightGo.GetComponent<Light>();
            l.type=LightType.Point;l.color=new Color(1f,.82f,.55f);l.intensity=2.4f;l.range=8.5f;l.shadows=LightShadows.Soft;

            // Shoreline boulders along the water line away from the boat berth
            var random=new System.Random(42);
            for(int i=0;i<4;i++)
            {
                float rx = i < 2 ? (-15.5f - (float)random.NextDouble() * 3.5f) : (1.5f + (float)random.NextDouble() * 2.5f);
                float rz = -25f - (float)random.NextDouble() * 7f;
                var rock=new GameObject("Dock Shore Rock",typeof(MeshFilter),typeof(MeshRenderer));rock.transform.SetParent(dockGroup,false);
                rock.transform.position=new Vector3(rx,settings.seaLevel+0.05f,rz);
                float yaw=(float)random.NextDouble()*360f;
                rock.transform.rotation=Quaternion.Euler(0,yaw,0);
                float s=0.32f+(float)random.NextDouble()*0.18f;
                rock.transform.localScale=Vector3.one*s;
                rock.GetComponent<MeshFilter>().sharedMesh=rockMesh;
                rock.GetComponent<MeshRenderer>().sharedMaterial=rockMat;
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
            MeshObject("Coastal Road Track",SaveMesh("CartRoad",roadVerts,roadTris,roadUVs),stone,roadGroup,true);

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
            var oldTree=GameObject.Find("Coastal Gnarled Pine");
            if(oldTree)Object.DestroyImmediate(oldTree);
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
            var person=new GameObject("Boatman Figure").transform;person.SetParent(visuals,false);person.localPosition=new(0,.40f,-.4f);person.localRotation=Quaternion.Euler(0,35,0);
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
            // Cottage West Front Plinth & Steps at x = -8.5, z = -9.05
            Box("House Plinth West",root,new(-8.85f,.08f,-9.05f),new(7.3f,.18f,.28f),stone,true);
            Box("Cottage Door Threshold Sill",root,new(-8.5f,.12f,-9.15f),new(1.8f,.18f,.45f),stone,true);
            Box("Cottage Door Intermediate Step",root,new(-8.5f,.06f,-9.6f),new(2.1f,.14f,.55f),stone,true);
            Box("Cottage Door Approach Step",root,new(-8.5f,.01f,-10.1f),new(2.4f,.12f,.65f),stone,true);
            // Standard human doorway: width = 1.16m, height = 2.25m, clearance = 1.20m (Ref: Screenshot 18)
            DoorLeaf("Keeper House Front Door",new(-9.08f,.15f,-9.05f),0,1.16f,2.25f,"Keeper House Front Door");

            // GRAND TOWER MAIN ENTRANCE (South wall, facing courtyard at x = 0, z = -6.50m)
            Box("Grand Tower Door Threshold Sill",root,new(0f,.12f,-6.65f),new(2.2f,.18f,.55f),stone,true);
            Box("Grand Tower Intermediate Step",root,new(0f,.06f,-7.15f),new(2.5f,.14f,.55f),stone,true);
            Box("Grand Tower Approach Step",root,new(0f,.01f,-7.70f),new(2.8f,.12f,.65f),stone,true);
            for(int i=0;i<4;i++)Box("Courtyard Approach Flagstone",root,new(0f,.01f,-8.4f-i*1.0f),new(2.0f,.08f,.90f),stone,true);
            Box("Grand Entrance Weather Hood",root,new(0f,2.62f,-6.70f),new(2.4f,.14f,.45f),stone,true);

            // Operable Grand Main Entrance Door (triggers Story Beat A1-B03 on open!)
            DoorLeaf("Tower Door",new(-.60f,.15f,-6.50f),0,1.20f,2.25f,"tower-front-door");

            DoorLeaf("Lighthouse Sea Door",new(5.62f,.15f,.8f),90,1.4f,2.2f,"tower-seaward-door");
            Box("Tower East Step",root,new(5.95f,.12f,0),new(1.3f,.24f,2),stone,true);
            for(int i=0;i<3;i++)Box("East Approach Stone",root,new(6.2f,.12f,-1.7f-i*1.5f),new(1.5f,.22f,1.5f),stone,true);
        }

        static void DoorLeaf(string name,Vector3 hinge,float yaw,float width,float height,string id)
        {
            var go=GameObject.Find(name);if(!go){go=new GameObject(name);go.AddComponent<NoiseEmitter>();go.AddComponent<Door>();}
            var old=go.transform.Find("Operable Leaf");if(old)Object.DestroyImmediate(old.gameObject);
            foreach(var renderer in go.GetComponentsInChildren<Renderer>(true))renderer.enabled=false;
            foreach(var collider in go.GetComponentsInChildren<Collider>(true))collider.enabled=false;
            go.transform.position=hinge;go.transform.rotation=Quaternion.Euler(0,yaw,0);go.transform.localScale=Vector3.one;
            var leaf=Box("Operable Leaf",go.transform,new(width*.5f,height*.5f,0),new(width,height,.08f),wood,true);
            Box("Iron Door Handle",leaf.transform,new(width*.5f-.08f,0,-.06f),new(.04f,.10f,.03f),iron);
            Box("Iron Door Handle",leaf.transform,new(width*.5f-.08f,0,.06f),new(.04f,.10f,.03f),iron);
            Box("Hinge Strap Top",leaf.transform,new(-width*.25f,height*.32f,0),new(width*.45f,.055f,.10f),iron);
            Box("Hinge Strap Bottom",leaf.transform,new(-width*.25f,-height*.32f,0),new(width*.45f,.055f,.10f),iron);
            var door=go.GetComponent<Door>();var so=new SerializedObject(door);so.FindProperty("doorId").stringValue=id;so.FindProperty("locked").boolValue=false;so.FindProperty("openAngle").floatValue=-105;
            if(name=="Tower Door")so.FindProperty("blockDuringActTwo").boolValue=false;
            so.ApplyModifiedPropertiesWithoutUndo();
            Quaternion rotation=Quaternion.Euler(0,yaw,0);
            for(int i=0;i<2;i++)Box("Masonry Door Reveal",root,hinge+rotation*new Vector3(i==0?-.35f:width+.35f,height*.5f,.15f),new(.55f,height+.12f,.6f),stone,true).transform.rotation=rotation;
            for(int i=0;i<2;i++)Box("Fixed Door Jamb",root,hinge+rotation*new Vector3(i==0?-.06f:width+.06f,height*.5f,0),new(.12f,height+.12f,.22f),wood,true).transform.rotation=rotation;
            Box("Fixed Door Header",root,hinge+rotation*new Vector3(width*.5f,height+.06f,0),new(width+.24f,.12f,.22f),wood,true).transform.rotation=rotation;
            Box("Door Threshold",root,hinge+rotation*new Vector3(width*.5f,-.03f,0),new(width+.16f,.07f,.30f),stone,true).transform.rotation=rotation;
        }
    }
}
