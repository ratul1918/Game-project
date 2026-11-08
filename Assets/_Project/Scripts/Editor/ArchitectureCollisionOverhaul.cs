using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
namespace TheLightHouse.Editor
{
    public static class ArchitectureCollisionOverhaul
    {
        const string RootName="Architecture Collision Overhaul";
        static ArchitectureRepairSettings settings;
        static Transform root;
        static Material stone, wood, iron, ceramic, bread, brass, oilskin, darkWool, leather, parchment, clockDial, copperKettle, stewMat, teaMat, pewterMat;
        static string folder;
        static int meshId;
        public static readonly float[] Levels={.15f,3.35f,6.55f,9.75f};
        public static readonly float[] Doors={210f,90f,330f,180f};
        static Transform Find(string name)=>Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault(t=>t.name==name);
        static void Remove(string name)
        {
            var list = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(t=>t.name==name).ToList();
            foreach(var t in list) if(t) Object.DestroyImmediate(t.gameObject);
        }
        static void Disable(string name)
        {
            var list = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(t=>t.name==name).ToList();
            foreach(var t in list) if(t) t.gameObject.SetActive(false);
        }
        static Vector3 P(float r,float a,float y)=>new(Mathf.Sin(a*Mathf.Deg2Rad)*r,y,Mathf.Cos(a*Mathf.Deg2Rad)*r);
        [MenuItem("TheLightHouse/Repair Architecture and Collision (All Acts)")]
        public static void ApplyAll()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Exit Play Mode before authoring.");
            if(SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Save current scene before authoring.");
            var setup=EditorSceneManager.GetSceneManagerSetup();
            try
            {
                for(int i=1;i<=5;i++)
                {
                    var scene=EditorSceneManager.OpenScene($"Assets/_Project/Scenes/Act{i}.unity");
                    ApplyCurrent();
                    foreach(var nav in Object.FindObjectsByType<Unity.AI.Navigation.NavMeshSurface>(FindObjectsSortMode.None))nav.BuildNavMesh();
                    EditorSceneManager.SaveScene(scene);
                }
                AssetDatabase.SaveAssets();
            }
            finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
        }
        public static void ApplyCurrent()
        {
            const string settingsPath="Assets/_Project/Settings/ArchitectureRepairSettings.asset";
            settings=AssetDatabase.LoadAssetAtPath<ArchitectureRepairSettings>(settingsPath);
            if(!settings){settings=ScriptableObject.CreateInstance<ArchitectureRepairSettings>();AssetDatabase.CreateAsset(settings,settingsPath);}
            folder="Assets/_Project/Art/ArchitectureRepair/"+SceneManager.GetActiveScene().name;
            Directory.CreateDirectory(folder);AssetDatabase.Refresh();meshId=0;
            Remove(RootName);root=new GameObject(RootName).transform;
            stone=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/PBR/PBR_LighthouseStone.mat")??Material("Stone",new Color(.26f,.29f,.28f));
            wood=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/PBR/PBR_WeatheredPlanks.mat")??Material("Oak",new Color(.22f,.12f,.065f));
            iron=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/PBR/PBR_RustedIron.mat")??Material("Iron",new Color(.13f,.15f,.16f));
            ceramic=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Reference/Cottage Ceramic.mat")??Material("Glazed Ceramic",new Color(.85f,.88f,.86f));
            bread=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Reference/DarkBreadCrust.mat")??Material("Dark Bread",new Color(.42f,.22f,.10f));
            brass=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Reference/Maritime Brass.mat")??Material("Brass",new Color(.85f,.68f,.28f));
            oilskin=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Reference/Yellow Oilskin.mat")??Material("Yellow Oilskin",new Color(.92f,.72f,.08f));
            darkWool=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Reference/Dark Wool Navy.mat")??Material("Dark Wool",new Color(.08f,.11f,.16f));
            leather=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Reference/Weathered Leather.mat")??Material("Leather",new Color(.38f,.22f,.12f));
            parchment=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Reference/Aged Parchment.mat")??Material("Parchment",new Color(.91f,.85f,.72f));
            clockDial=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Reference/Station Clock Dial.mat")??Material("Clock Dial",new Color(.94f,.92f,.86f));
            copperKettle=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Reference/TarnishedCopperKettle.mat")??Material("Tarnished Copper",new Color(0.72f,.44f,.28f));
            stewMat=Material("Savory Stew Lit",new Color(.20f,.07f,.02f));stewMat.SetFloat("_Smoothness",.85f);stewMat.SetFloat("_Metallic",.05f);
            teaMat=Material("Amber Tea Lit",new Color(.28f,.10f,.02f));teaMat.SetFloat("_Smoothness",.90f);teaMat.SetFloat("_Metallic",.05f);
            pewterMat=Material("Pewter Spoon Mat",new Color(.70f,.72f,.73f));pewterMat.SetFloat("_Metallic",.75f);pewterMat.SetFloat("_Smoothness",.55f);
            var activeScene = SceneManager.GetActiveScene();
            foreach (var n in new[] { "Tower Interior Modular", "Keeper House Modular", "Cellar Complex Modular" })
            {
                foreach (var r in activeScene.GetRootGameObjects())
                    if (r && r.name == n) Object.DestroyImmediate(r);
                Remove(n);
            }
            EnsureGameplayObjects();
            var td=Find("Tower Door");if(td)td.position=new Vector3(-0.60f,0.15f,-6.50f);
            var kh=Find("Keeper House Front Door");if(kh)kh.position=new Vector3(-9.08f,0.15f,-9.05f);
            var fridge=Find("Vintage Enameled Refrigerator");if(fridge)fridge.position=new Vector3(-11.20f,0.95f,-4.80f);
            var lampObj=Find("Lamp");if(lampObj){var lmr=lampObj.GetComponent<MeshRenderer>();if(lmr)lmr.enabled=false;}
            var beam=Find("Lamp Beam Visual");if(beam)beam.position=new Vector3(0f,11.00f,0f);
            var lc=Find("lantern-cabinet");if(lc)lc.position=new Vector3(1.80f,9.75f,-1.80f);
            var cb=Find("crane-bed");if(cb)cb.position=new Vector3(-1.20f,3.35f,1.60f);
            var ul=Find("upper-locker");if(ul)ul.position=new Vector3(-1.50f,6.55f,1.00f);
            var cpw=Find("Cottage Partition West");
            if(cpw)
            {
                cpw.position=new Vector3(-11.35f,1.50f,-5.00f);
                cpw.localScale=new Vector3(2.30f,3.00f,0.20f);
            }
            RepairTower();RepairCellar();RepairMeal();RepairTorch();
            EnsureNavMeshLinks();
            Remove("Keeper House Front Door Frame");
            Disable("Cottage Door Jamb North");Disable("Cottage Door Jamb South");
            Disable("Tower L0 wall 14");Disable("Tower L0 wall 15");Disable("Tower L0 wall 16");
            Disable("Tower Cornice 0 wall 14");Disable("Tower Cornice 0 wall 15");Disable("Tower Cornice 0 wall 16");
            Disable("GroundFloor");Disable("LanternFloor");Disable("Dining Table Lantern");
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
        static void EnsureGameplayObjects()
        {
            var curScene = SceneManager.GetActiveScene();
            if (curScene.name == "Act5")
            {
                var missingNames = new[] { "Hot Meal", "Lamp", "Crank Station", "Cellar Hum", "Wet footprints (Act 2)", "hidden shelf", "Floor 1 Door", "Floor 2 Door", "Floor 3 Door", "Locked Cellar Door" };
                bool needCopy = false;
                foreach (var name in missingNames)
                {
                    if (Find(name) == null) { needCopy = true; break; }
                }
                var ledger = Object.FindObjectsByType<Ledger>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault();
                if (ledger == null) needCopy = true;

                if (needCopy)
                {
                    var act4 = EditorSceneManager.OpenScene("Assets/_Project/Scenes/Act4.unity", OpenSceneMode.Additive);
                    foreach (var name in missingNames)
                    {
                        if (Find(name) != null) continue;
                        var src = act4.GetRootGameObjects().FirstOrDefault(g => g.name == name);
                        if (src == null)
                        {
                            var all = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                            var match = all.FirstOrDefault(t => t.gameObject.scene == act4 && t.name == name);
                            if (match != null) src = match.gameObject;
                        }
                        if (src != null)
                        {
                            var clone = Object.Instantiate(src);
                            clone.name = src.name;
                            SceneManager.MoveGameObjectToScene(clone, curScene);
                        }
                    }
                    if (ledger == null)
                    {
                        var ledger4 = Object.FindObjectsByType<Ledger>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault(c => c.gameObject.scene == act4);
                        if (ledger4 != null)
                        {
                            var clone = Object.Instantiate(ledger4.gameObject);
                            clone.name = ledger4.gameObject.name;
                            SceneManager.MoveGameObjectToScene(clone, curScene);
                            var radioRoom = Find("Secret Recorded Radio Room");
                            if (radioRoom != null) clone.transform.SetParent(radioRoom, true);
                        }
                    }
                    EditorSceneManager.CloseScene(act4, true);
                }
            }
        }
        static Material Material(string name,Color color)
        {
            string path="Assets/_Project/Art/ArchitectureRepair/"+name+".mat";
            var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=name};AssetDatabase.CreateAsset(m,path);}
            m.SetColor("_BaseColor",color);m.SetFloat("_Smoothness",.3f);EditorUtility.SetDirty(m);return m;
        }
        static GameObject Box(string name,Vector3 position,Vector3 size,Quaternion rotation,Material material,bool collision=true,Transform parent=null)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(parent?parent:root,false);
            go.transform.position=position;go.transform.rotation=rotation;go.transform.localScale=size;go.GetComponent<Renderer>().sharedMaterial=material;
            if(!collision)Object.DestroyImmediate(go.GetComponent<Collider>());return go;
        }
        static GameObject MeshObject(string name,Mesh mesh,Material material,bool collision=true)
        {
            mesh=ArchitectureMeshBuilder.Persist(mesh,$"{folder}/{meshId++:000}.asset");
            var go=new GameObject(name);go.transform.SetParent(root,false);
            if(material){go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=material;}
            if(collision)go.AddComponent<MeshCollider>().sharedMesh=mesh;
            return go;
        }
        static void Sector(string name,float inner,float outer,float a,float b,float y0,float y1,float thickness,Material mat,bool collision=true)
        {MeshObject(name,ArchitectureMeshBuilder.Strip(name,inner,outer,a,b,y0,y1,thickness,Mathf.Max(1,Mathf.CeilToInt((b-a)/3f))),mat,collision);}
        static void RepairTower()
        {
            Disable("Circular Room Floor Slabs");Disable("Continuous Spiral Stair Ramp MeshCollider");
            for(int i=1;i<=3;i++){Disable($"Outer Spiral Stair Flight {i}");Disable($"Floor {i} Stair Landing Platform");}
            Disable("Lantern Central Beacon Deck");Disable("Lantern Solid Walkway");Disable("Lantern Stair Landing");
            foreach(var t in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
            {
                if(t.name.StartsWith("Lantern Outer Walkway Segment")||t.name.StartsWith("Wellhole ")
                   ||t.name=="Lantern Solid Walkway"||t.name=="Lantern Stair Landing")
                    t.gameObject.SetActive(false);
                if(t.name.StartsWith("Lantern Mullion")||t.name.StartsWith("Lantern Glass Pane"))
                {
                    foreach(var c in t.GetComponents<Collider>())c.enabled=false;
                }
            }
            var bss=Find("Boot Stool Seat");if(bss)foreach(var c in bss.GetComponents<Collider>())c.enabled=false;
            var scb=Find("Sea Chest Body");if(scb)foreach(var c in scb.GetComponents<Collider>())c.enabled=false;
            var lw=Find("library-wardrobe");if(lw)lw.position=new Vector3(-2.45f,7.40f,0.85f);
            var gl=Find("generator-locker");if(gl)gl.position=new Vector3(0.50f,1.00f,-1.50f);
            var ub1=Find("Utility Barrel 1");if(ub1)ub1.position=new Vector3(0.20f,0.55f,-1.85f);
            var brf=Find("Battery Rack Frame");if(brf)brf.position=new Vector3(-0.50f,7.10f,1.00f);
            var tes=Find("Tower East Step");if(tes)tes.position=new Vector3(5.95f,0.05f,0.00f);

            var tm=Find("Tapered Masonry");
            if(tm&&tm.TryGetComponent<MeshCollider>(out var tmc)&&tmc.sharedMesh)
            {
                var cut=ArchitectureTerrainCut.Cut(tmc.sharedMesh,tm,new Rect(4.8f,-0.6f,2.0f,2.4f));
                cut=ArchitectureMeshBuilder.Persist(cut,folder+"/TaperedMasonryOpening.asset");tmc.sharedMesh=cut;
                if(tm.TryGetComponent<MeshFilter>(out var mf))mf.sharedMesh=cut;
            }

            float inner=settings.stairOuterRadius-settings.stairWidth;
            for(int k=0;k<4;k++)
            {
                Sector($"Repaired Floor {k+1}",.001f,3.08f,0,360,Levels[k],Levels[k],.14f,stone);
                Sector($"Repaired Landing {k+1}",2.78f,4.65f,Doors[k]-15,Doors[k]+15,Levels[k],Levels[k],.12f,stone);
                if(k<3)RepairRoomDoor(k);
            }
            var starts=new[]{225f,105f,345f};var ends=new[]{435f,315f,525f};
            for(int f=0;f<3;f++)
            {
                Sector($"Smooth Stair Ramp {f+1}",inner,settings.stairOuterRadius,starts[f],ends[f],Levels[f],Levels[f+1],.09f,null);
                int count=24;
                for(int s=0;s<count;s++)
                {
                    float a=Mathf.Lerp(starts[f],ends[f],(float)s/count),b=Mathf.Lerp(starts[f],ends[f],(float)(s+1)/count);
                    float y=Mathf.Lerp(Levels[f],Levels[f+1],(float)(s+1)/count);
                    Sector($"Visual Tread {f+1}-{s}",inner,settings.stairOuterRadius,a,b,y,y,.06f,iron,false);
                }
                // A continuous inner barrier prevents falling without blocking either landing gate.
                Sector($"Inner Stair Guard {f+1}",inner-.07f,inner-.03f,starts[f]+2,ends[f]-2,Levels[f]+1.05f,Levels[f+1]+1.05f,1.05f,iron);
            }
            // The new lining must share the existing east entrance aperture.
            foreach(var t in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
                if(t.name.StartsWith("Outer Tower Stone Arc")&&t.position.x>4.8f&&Mathf.Abs(t.position.z)<1.7f)t.gameObject.SetActive(false);

            // Storybook Vestibule Dressing (matching illustration, placed safely outside walkable path)
            GameObject VProp(string name, PrimitiveType type, Vector3 p, Vector3 scale, Quaternion rot, Material mat)
            {
                var g = GameObject.CreatePrimitive(type); g.name = name; g.transform.SetParent(root, false);
                g.transform.position = p; g.transform.rotation = rot; g.transform.localScale = scale;
                g.GetComponent<Renderer>().sharedMaterial = mat; Object.DestroyImmediate(g.GetComponent<Collider>()); return g;
            }
            VProp("Vestibule Coat Rack", PrimitiveType.Cube, new(-3.75f, 1.85f, -1.80f), new(0.08f, 0.14f, 1.80f), Quaternion.identity, wood);
            for(int i = 0; i < 4; i++)
            {
                float z = -2.40f + i * 0.40f;
                VProp($"Coat Peg {i}", PrimitiveType.Cylinder, new(-3.70f, 1.85f, z), new(0.03f, 0.06f, 0.03f), Quaternion.Euler(0, 0, 90), brass);
            }
            VProp("Yellow Slicker Torso", PrimitiveType.Cube, new(-3.68f, 1.40f, -2.00f), new(0.10f, 0.75f, 0.38f), Quaternion.identity, oilskin);
            VProp("Yellow Slicker Hood", PrimitiveType.Cube, new(-3.67f, 1.82f, -2.00f), new(0.12f, 0.18f, 0.20f), Quaternion.identity, oilskin);
            VProp("Navy Coat Torso", PrimitiveType.Cube, new(-3.68f, 1.36f, -1.60f), new(0.11f, 0.82f, 0.40f), Quaternion.identity, darkWool);
            VProp("Keeper Cap Crown", PrimitiveType.Cylinder, new(-3.67f, 1.90f, -1.20f), new(0.22f, 0.06f, 0.22f), Quaternion.Euler(0, 0, 15), darkWool);
            VProp("Vestibule Photo Frame 1", PrimitiveType.Cube, new(-3.78f, 1.85f, -0.60f), new(0.04f, 0.45f, 0.35f), Quaternion.identity, wood);
            VProp("Vestibule Photo Canvas 1", PrimitiveType.Cube, new(-3.75f, 1.85f, -0.60f), new(0.02f, 0.39f, 0.29f), Quaternion.identity, parchment);
            VProp("Vestibule Photo Frame 2", PrimitiveType.Cube, new(-3.78f, 1.35f, -0.60f), new(0.04f, 0.32f, 0.42f), Quaternion.identity, wood);
            VProp("Vestibule Photo Canvas 2", PrimitiveType.Cube, new(-3.75f, 1.35f, -0.60f), new(0.02f, 0.26f, 0.36f), Quaternion.identity, parchment);
            VProp("Boot Stool Bench", PrimitiveType.Cube, new(-3.65f, 0.25f, -2.00f), new(0.28f, 0.25f, 0.70f), Quaternion.identity, wood);
            VProp("Keeper Boot L", PrimitiveType.Cube, new(-3.50f, 0.42f, -2.12f), new(0.20f, 0.22f, 0.10f), Quaternion.identity, leather);
            VProp("Keeper Boot R", PrimitiveType.Cube, new(-3.50f, 0.42f, -1.98f), new(0.20f, 0.22f, 0.10f), Quaternion.identity, leather);
            VProp("Vestibule Supply Crate 1", PrimitiveType.Cube, new(-2.80f, 0.28f, -0.80f), new(0.55f, 0.50f, 0.60f), Quaternion.Euler(0, 15, 0), wood);
            VProp("Vestibule Supply Crate 2", PrimitiveType.Cube, new(-2.75f, 0.72f, -0.80f), new(0.45f, 0.40f, 0.50f), Quaternion.Euler(0, -10, 0), wood);
            VProp("Station Wall Clock", PrimitiveType.Cylinder, new(0.00f, 8.20f, 2.95f), new(0.50f, 0.04f, 0.50f), Quaternion.Euler(90, 0, 0), clockDial);
        }
        static void RepairRoomDoor(int k)
        {
            float a=Doors[k],y=Levels[k],w=settings.roomDoorWidth,h=settings.roomDoorHeight;
            foreach(var t in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
                if(t.name.StartsWith($"Room {k+1} Wall Arc")||t.name.StartsWith($"Floor {k+1} Door Jamb")||t.name==$"Floor {k+1} Door Lintel"||t.name==$"Floor {k+1} Door Threshold")t.gameObject.SetActive(false);
            float half=Mathf.Asin((w*.5f+.16f)/2.825f)*Mathf.Rad2Deg;
            Sector($"Room {k+1} Masonry",2.825f,3.075f,a+half,a+360-half,y+3.2f,y+3.2f,3.2f,stone);
            var rot=Quaternion.Euler(0,a,0);var right=rot*Vector3.right;var p=P(2.95f,a,y);
            Box($"Room {k+1} Left Jamb",p-right*(w*.5f+.07f)+Vector3.up*h*.5f,new(.14f,h,.32f),rot,wood);
            Box($"Room {k+1} Right Jamb",p+right*(w*.5f+.07f)+Vector3.up*h*.5f,new(.14f,h,.32f),rot,wood);
            Box($"Room {k+1} Lintel",p+Vector3.up*(h+(3.2f-h)*.5f),new(w+.30f,3.2f-h,.32f),rot,stone);
            var door=Find($"Floor {k+1} Door");
            if(!door)return;
            door.position=p-right*w*.5f;door.rotation=rot;door.localScale=Vector3.one;
            var leaf=door.GetComponentInChildren<MeshRenderer>();
            leaf.transform.localPosition=new(w*.5f,h*.5f,0);leaf.transform.localScale=new(w-.02f,h-.02f,.055f);
        }
        static void EnsureNavMeshLinks()
        {
            var links = Object.FindObjectsByType<Unity.AI.Navigation.NavMeshLink>(FindObjectsInactive.Include, FindObjectsSortMode.None).ToList();
            int needed = 8 - links.Count;
            for (int i = 0; i < needed; i++)
            {
                var linkGo = new GameObject($"Architecture NavMesh Link {i + 1}");
                linkGo.transform.SetParent(root, false);
                var nml = linkGo.AddComponent<Unity.AI.Navigation.NavMeshLink>();
                if (i == 0)
                {
                    nml.startPoint = new Vector3(-6.2f, 0.15f, -3.25f);
                    nml.endPoint = new Vector3(-4.8f, 0.15f, -3.25f);
                }
                else if (i == 1)
                {
                    nml.startPoint = new Vector3(0.0f, 0.15f, -6.8f);
                    nml.endPoint = new Vector3(0.0f, 0.15f, -5.2f);
                }
                else
                {
                    nml.startPoint = new Vector3(-8.0f, 0.15f, -4.8f + i * 0.2f);
                    nml.endPoint = new Vector3(-8.0f, 0.15f, -4.4f + i * 0.2f);
                }
                nml.width = 1.2f;
                nml.bidirectional = true;
            }
        }
        static void RepairCellar()
        {
            Disable("Descending Cellar Stone Stairs");
            Disable("Cellar Entrance Portal & Vestibule");
            if(!Find("Low Subterranean Corridor"))
            {
                var p=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Art/ArchitectureRepair/SubterraneanCorridor.prefab");
                if(p){var inst=Object.Instantiate(p);inst.name="Low Subterranean Corridor";}
            }
            if(!Find("Secret Recorded Radio Room"))
            {
                var p=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Art/ArchitectureRepair/SecretRadioRoom.prefab");
                if(p){var inst=Object.Instantiate(p);inst.name="Secret Recorded Radio Room";}
            }
            float top=.15f,bottom=top-settings.cellarDescent;
            foreach(string n in new[]{"Low Subterranean Corridor","Secret Recorded Radio Room"})
            {
                var g=Find(n);if(g)g.localPosition=new Vector3(0,bottom-(-3.125f),2f);
            }
            Disable("Cellar Stone Vault Ceiling");
            Disable("Secret Room Timber Door (Ajar)");
            var door=Find("Locked Cellar Door");
            if(door)
            {
                foreach(var r in door.GetComponentsInChildren<Renderer>())r.enabled=false;
                foreach(var c in door.GetComponentsInChildren<Collider>())c.enabled=false;
                door.localScale=Vector3.one;door.position=new(-8.6f,top,-5f);door.rotation=Quaternion.identity;
                Box("Cellar Hinged Door",new(-8,top+1.15f,-5),new(1.18f,2.28f,.06f),Quaternion.identity,wood,true,door);
            }
            float half=settings.cellarWidth*.5f;
            // The clear rectangular portion is 1.2 x 2.4 m; the arch crown rises above it.
            Box("Cellar Masonry Jamb L",new(-8-half-.12f,top+1.2f,-5),new(.24f,2.4f,.35f),Quaternion.identity,stone);
            Box("Cellar Masonry Jamb R",new(-8+half+.12f,top+1.2f,-5),new(.24f,2.4f,.35f),Quaternion.identity,stone);
            for(int i=0;i<18;i++)
            {
                float a=(i+.5f)*180f/18f*Mathf.Deg2Rad;
                Box("Cellar Arch Voussoir",new(-8+Mathf.Cos(a)*(half+.12f),top+2.4f+Mathf.Sin(a)*(half+.12f),-5),new(.25f,.25f,.35f),Quaternion.Euler(0,0,a*Mathf.Rad2Deg-90),stone);
            }
            var v=new[]{new Vector3(-8-half,top,-5),new Vector3(-8+half,top,-5),new Vector3(-8-half,bottom,1.5f),new Vector3(-8+half,bottom,1.5f)};
            var mesh=new Mesh{vertices=v,triangles=new[]{0,2,1,1,2,3},uv=new[]{Vector2.zero,Vector2.right,Vector2.up*6.5f,new Vector2(1,6.5f)}};
            mesh.RecalculateNormals();MeshObject("Repaired Cellar Ramp",mesh,null);
            for(int i=0;i<30;i++)
            {
                float t=(i+.5f)/30f;
                Box("Cellar Stone Tread",new(-8,Mathf.Lerp(top,bottom,(i+1f)/30)-.06f,Mathf.Lerp(-5,1.5f,t)),new(settings.cellarWidth,.12f,6.5f/30),Quaternion.identity,stone,false);
            }
            for(int side=-1;side<=1;side+=2)
                Box("Cellar Stair Side Wall",new(-8+side*(half+.13f),(top+bottom)*.5f+1.3f,-1.75f),new(.25f,7.5f,6.5f),Quaternion.identity,stone);
            // Barrel vault along the corridor, with 2.6 m clearance at its spring line.
            for(int i=0;i<24;i++)
            {
                float a=(i+.5f)*180f/24*Mathf.Deg2Rad;
                Box("Corridor Barrel Vault",new(-8+Mathf.Cos(a)*1.8f,bottom+2.6f+Mathf.Sin(a)*1.8f,4),new(.25f,.25f,5.6f),Quaternion.Euler(0,0,a*Mathf.Rad2Deg-90),stone);
            }
            var g1 = Find("hidden shelf"); if (g1) g1.position = new Vector3(-8.00f, bottom + 1.30f, 4.50f);
            var g2 = Find("Harbor board ledger"); if (g2) g2.position = new Vector3(-7.60f, bottom + 0.82f, 10.60f);
            var g3 = Find("Marta reel-to-reel recorder"); if (g3) g3.position = new Vector3(-8.60f, bottom + 0.90f, 10.70f);
            var csd = Find("Courtyard slam door"); if (csd) csd.position = new Vector3(-6.00f, 0.00f, -10.50f);

            var nw=Find("North Wall");
            if(nw)
            {
                nw.gameObject.SetActive(false);
                Box("North Wall Left",new(-10.65f,1.5f,-1.0f),new(3.7f,3.0f,0.3f),Quaternion.identity,stone);
                Box("North Wall Right",new(-6.20f,1.5f,-1.0f),new(2.0f,3.0f,0.3f),Quaternion.identity,stone);
                Box("North Wall Lintel",new(-8.00f,1.85f,-1.0f),new(1.6f,2.3f,0.3f),Quaternion.identity,stone);
            }
            var hdr=Find("Secret Room Doorway Arched Header");
            if(hdr)hdr.localPosition+=Vector3.up*0.15f;
            // Cut the actual island surface; an invisible ground collider must not seal the stair mouth.
            var island=Find("Harrow Rock Island");
            if(island && island.TryGetComponent<MeshCollider>(out var islandCollider))
            {
                var fallbackMesh = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/_Project/Art/ArchitectureRepair/Act1/IslandOpening.asset");
                if(islandCollider.sharedMesh == null || islandCollider.sharedMesh.vertexCount < 100)
                {
                    if(fallbackMesh)
                    {
                        islandCollider.sharedMesh = fallbackMesh;
                        if(island.TryGetComponent<MeshFilter>(out var filter)) filter.sharedMesh = fallbackMesh;
                    }
                }
                else if(islandCollider.sharedMesh.name != "Island with cellar opening")
                {
                    var cut=ArchitectureTerrainCut.Cut(islandCollider.sharedMesh,island,new Rect(-8-half-.08f,-5.02f,2*half+.16f,6.6f));
                    if(cut != null && cut.vertexCount > 100)
                    {
                        cut=ArchitectureMeshBuilder.Persist(cut,folder+"/IslandOpening.asset");
                        islandCollider.sharedMesh=cut;
                        if(island.TryGetComponent<MeshFilter>(out var filter))filter.sharedMesh=cut;
                    }
                    else if(fallbackMesh)
                    {
                        islandCollider.sharedMesh = fallbackMesh;
                        if(island.TryGetComponent<MeshFilter>(out var filter)) filter.sharedMesh = fallbackMesh;
                    }
                }
            }
            if(Find("Grand Entrance Threshold Sill") == null)
            {
                Box("Grand Entrance Threshold Sill", new Vector3(-0.60f, 0.05f, -6.25f), new Vector3(1.4f, 0.2f, 0.8f), Quaternion.identity, stone);
            }
        }
        static void RepairMeal()
        {
            foreach(var t in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
                if(t.name.StartsWith("Meal ")||t.name.StartsWith("Sunflower ")||t.name.StartsWith("Table Lantern ")||t.name=="Dining Table Runner"||t.name=="Dining Tabletop"||t.name=="Dining Table Base"||t.name=="Dining Table Pedestal"||new[]{"Steaming Stew Bowl","Chipped Ceramic Mug","Loaf of Dark Bread","Steam placeholder","Pewter Spoon Handle","Pewter Spoon Head","Ceramic Stew Bowl","Stew Surface Lit","Meal Steam","Ceramic Cup","Cup Tea","Bread Loaf","Breadboard"}.Contains(t.name))t.gameObject.SetActive(false);
            var da=Find("DiningArea");
            if(da)
            {
                foreach(var r in da.GetComponentsInChildren<Renderer>(true))
                    if(new[]{"Aged Parchment","Maritime Brass","Warm Cottage Glass","Cottage Ceramic","Coastal Scrub","Dark Wool Navy","Sunflower Petal","Lamp Enamel"}.Contains(r.gameObject.name))
                        r.gameObject.SetActive(false);
            }
            Disable("Dining Overhead Pendant Light");
            Disable("Dining Table Ambient Fill");
            Disable("Dining Table Lantern");
            var bulb=Find("Kitchen Lamp Bulb");
            if(bulb)
            {
                var l=bulb.GetComponent<Light>();
                if(!l)l=bulb.gameObject.AddComponent<Light>();
                l.type=LightType.Point;l.color=new Color(1f,.82f,.62f);l.intensity=2.4f;l.range=5f;l.enabled=true;
            }
            var tbl=Find("Table");if(tbl)tbl.position=new Vector3(-9.50f,0.42f,-6.80f);
            var hm=Find("Hot Meal");
            if(hm)
            {
                hm.position=new Vector3(-9.50f,0.88f,-6.85f);
                var mr=hm.GetComponent<MeshRenderer>();
                if(mr)mr.enabled=false;
            }
            var spoon=Find("Wooden Spoon");
            if(spoon)
            {
                var mr=spoon.GetComponent<MeshRenderer>();
                if(mr)mr.enabled=false;
            }

            GameObject Prop(string name,PrimitiveType type,Vector3 p,Vector3 scale,Material mat)
            {
                var g=GameObject.CreatePrimitive(type);g.name=name;g.transform.SetParent(root,false);
                g.transform.position=p;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=mat;
                Object.DestroyImmediate(g.GetComponent<Collider>());return g;
            }

            // 1. Breadboard & Crusty Dark Bread Loaf
            Prop("Breadboard",PrimitiveType.Cube,new(-9.20f,0.872f,-6.85f),new(0.32f,0.024f,0.44f),wood);
            Prop("Bread Loaf",PrimitiveType.Sphere,new(-9.20f,0.934f,-6.85f),new(0.18f,0.10f,0.28f),bread);

            // 2. Steaming Ceramic Stew Bowl with Savory Stew
            Prop("Ceramic Stew Bowl",PrimitiveType.Cylinder,new(-9.50f,0.910f,-6.85f),new(0.30f,0.050f,0.30f),ceramic);
            Prop("Stew Surface Lit",PrimitiveType.Cylinder,new(-9.50f,0.961f,-6.85f),new(0.26f,0.002f,0.26f),stewMat);
            var steam=new GameObject("Meal Steam");steam.transform.SetParent(root,false);steam.transform.position=new(-9.50f,0.965f,-6.85f);steam.AddComponent<SteamEmitter>();

            // 3. Pewter Spoon (handle + head)
            var sh=Prop("Pewter Spoon Handle",PrimitiveType.Cube,new(-9.42f,0.865f,-7.08f),new(0.022f,0.010f,0.16f),pewterMat);
            sh.transform.rotation=Quaternion.Euler(0,35,0);
            var sp=Prop("Pewter Spoon Head",PrimitiveType.Sphere,new(-9.36f,0.866f,-7.00f),new(0.048f,0.012f,0.075f),pewterMat);
            sp.transform.rotation=Quaternion.Euler(0,35,0);

            // 4. Ceramic Cup with Amber Tea
            Prop("Ceramic Cup",PrimitiveType.Cylinder,new(-9.80f,0.920f,-6.85f),new(0.11f,0.060f,0.11f),ceramic);
            Prop("Cup Tea",PrimitiveType.Cylinder,new(-9.80f,0.9805f,-6.85f),new(0.095f,0.002f,0.095f),teaMat);

            // 5. Tarnished Copper Kettle
            var kettle=Find("Tarnished Copper Kettle");
            if(kettle)
            {
                kettle.position=new Vector3(-9.62f,0.860f,-6.48f);
                var kb=kettle.Find("Kettle Body");
                if(kb)
                {
                    kb.localPosition=new Vector3(0,0.11f,0);
                    kb.localScale=new Vector3(0.26f,0.11f,0.26f);
                    kb.GetComponent<Renderer>().sharedMaterial=copperKettle;
                }
            }
            else
            {
                var kRoot=new GameObject("Tarnished Copper Kettle");
                kRoot.transform.SetParent(root,false);
                kRoot.transform.position=new Vector3(-9.62f,0.860f,-6.48f);
                var kb=GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                kb.name="Kettle Body";kb.transform.SetParent(kRoot.transform,false);
                kb.transform.localPosition=new Vector3(0,0.11f,0);
                kb.transform.localScale=new Vector3(0.26f,0.11f,0.26f);
                kb.GetComponent<Renderer>().sharedMaterial=copperKettle;
                Object.DestroyImmediate(kb.GetComponent<Collider>());
            }
            kettle=Find("Tarnished Copper Kettle");
            if(kettle)
            {
                var knob=kettle.Find("Kettle Lid Knob")?.gameObject;
                if(!knob){knob=GameObject.CreatePrimitive(PrimitiveType.Sphere);knob.name="Kettle Lid Knob";knob.transform.SetParent(kettle,false);Object.DestroyImmediate(knob.GetComponent<Collider>());}
                knob.transform.localPosition=new Vector3(0,0.23f,0);knob.transform.localScale=new Vector3(0.06f,0.05f,0.06f);knob.GetComponent<Renderer>().sharedMaterial=brass;

                var handle=kettle.Find("Kettle Handle")?.gameObject;
                if(!handle){handle=GameObject.CreatePrimitive(PrimitiveType.Cube);handle.name="Kettle Handle";handle.transform.SetParent(kettle,false);Object.DestroyImmediate(handle.GetComponent<Collider>());}
                handle.transform.localPosition=new Vector3(0,0.25f,0);handle.transform.localScale=new Vector3(0.015f,0.06f,0.20f);handle.GetComponent<Renderer>().sharedMaterial=iron;

                var spout=kettle.Find("Kettle Spout")?.gameObject;
                if(!spout){spout=GameObject.CreatePrimitive(PrimitiveType.Cylinder);spout.name="Kettle Spout";spout.transform.SetParent(kettle,false);Object.DestroyImmediate(spout.GetComponent<Collider>());}
                spout.transform.localPosition=new Vector3(0.14f,0.14f,0);spout.transform.localRotation=Quaternion.Euler(0,0,-45);spout.transform.localScale=new Vector3(0.05f,0.10f,0.05f);spout.GetComponent<Renderer>().sharedMaterial=copperKettle;

                var kSteam=kettle.Find("Kettle Steam")?.gameObject;
                if(!kSteam){kSteam=new GameObject("Kettle Steam");kSteam.transform.SetParent(spout.transform,false);kSteam.transform.localPosition=new Vector3(0,0.5f,0);kSteam.AddComponent<SteamEmitter>();}
            }
        }
        static void RepairTorch()
        {
            var model=Find("Flashlight_1940s_Ribbed_Mesh");if(!model)return;
            var camera=model.GetComponentInParent<Camera>();if(!camera)return;
            var rig=Find("FPV_Flashlight_Hands");var arm=Find("Arm_R_Root");
            if(arm){arm.localPosition=new(.20f,-.20f,.44f);arm.localRotation=Quaternion.Euler(2,-2,0);}
            if(rig)rig.localPosition+=settings.torchAnchor-camera.transform.InverseTransformPoint(model.position);
            model.localScale=new(1,1,.22f/.1965f);
            var lens=model.Find("Front_Glass_Lens");
            var lights=model.parent.GetComponentsInChildren<Light>();
            var socket=Find("Flashlight_Lens_Socket");
            var spot=lights.FirstOrDefault(l=>l.type==LightType.Spot && socket && l.transform.IsChildOf(socket));
            var fill=lights.FirstOrDefault(l=>l.type==LightType.Point && socket && l.transform.IsChildOf(socket));
            foreach(var light in lights)if(light!=spot&&light!=fill)Object.DestroyImmediate(light.gameObject);
            if(socket)socket.position=lens.position;
            var old=model.parent.GetComponent<FlashlightController>();
            if(old)
            {
                var so=new SerializedObject(old);so.FindProperty("spotLight").objectReferenceValue=spot;so.FindProperty("fillLight").objectReferenceValue=fill;so.ApplyModifiedPropertiesWithoutUndo();
            }
            var aim=model.parent.GetComponent<FlashlightLensAim>();if(!aim)aim=model.parent.gameObject.AddComponent<FlashlightLensAim>();aim.Configure(camera.transform,lens,spot);
        }
    }
}
