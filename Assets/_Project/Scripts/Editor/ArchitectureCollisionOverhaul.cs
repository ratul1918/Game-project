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
        const string RootName = "Architecture Collision Overhaul";
        static ArchitectureRepairSettings settings;
        static Transform root;
        static Material stone, wood, iron, ceramic, bread, brass, oilskin, darkWool, leather, parchment, clockDial, copperKettle, stewMat, teaMat, pewterMat, glassMat, redRugMat, linenMat, quiltMat, amberBulbMat;
        static string folder;
        static int meshId;
        public static readonly float[] Levels = { .15f, 3.35f, 6.55f, 9.75f };
        public static readonly float[] Doors = { 210f, 90f, 330f, 180f };

        static Transform Find(string name) => Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault(t => t.name == name);

        static void Remove(string name)
        {
            var list = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(t => t.name == name).ToList();
            foreach (var t in list) if (t) Object.DestroyImmediate(t.gameObject);
        }

        static void Disable(string name)
        {
            var list = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(t => t.name == name).ToList();
            foreach (var t in list) if (t) t.gameObject.SetActive(false);
        }

        static Vector3 P(float r, float a, float y) => new(Mathf.Sin(a * Mathf.Deg2Rad) * r, y, Mathf.Cos(a * Mathf.Deg2Rad) * r);

        public static void ApplyAll()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play Mode before authoring.");
            if (SceneManager.GetActiveScene().isDirty) throw new InvalidOperationException("Save current scene before authoring.");
            var setup = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                for (int i = 1; i <= 5; i++)
                {
                    var scene = EditorSceneManager.OpenScene($"Assets/_Project/Scenes/Act{i}.unity");
                    ApplyCurrent();
                    foreach (var nav in Object.FindObjectsByType<Unity.AI.Navigation.NavMeshSurface>(FindObjectsSortMode.None)) nav.BuildNavMesh();
                    EditorSceneManager.SaveScene(scene);
                }
                AssetDatabase.SaveAssets();
            }
            finally { EditorSceneManager.RestoreSceneManagerSetup(setup); }
        }

        public static void ApplyCurrent()
        {
            const string settingsPath = "Assets/_Project/Settings/ArchitectureRepairSettings.asset";
            settings = AssetDatabase.LoadAssetAtPath<ArchitectureRepairSettings>(settingsPath);
            if (!settings) { settings = ScriptableObject.CreateInstance<ArchitectureRepairSettings>(); AssetDatabase.CreateAsset(settings, settingsPath); }
            settings.stairOuterRadius = 4.65f;
            settings.stairWidth = 1.35f;
            settings.headClearance = 2.6f;
            EditorUtility.SetDirty(settings);

            folder = "Assets/_Project/Art/ArchitectureRepair/" + SceneManager.GetActiveScene().name;
            Directory.CreateDirectory(folder); AssetDatabase.Refresh(); meshId = 0;
            Remove(RootName); root = new GameObject(RootName).transform;

            stone = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/PBR/PBR_LighthouseStone.mat") ?? Material("Stone", new Color(.26f, .29f, .28f));
            wood = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/PBR/PBR_WeatheredPlanks.mat") ?? Material("Oak", new Color(.22f, .12f, .065f));
            iron = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/PBR/PBR_RustedIron.mat") ?? Material("Iron", new Color(.13f, .15f, .16f));
            ceramic = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Reference/Cottage Ceramic.mat") ?? Material("Glazed Ceramic", new Color(.85f, .88f, .86f));
            bread = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Reference/DarkBreadCrust.mat") ?? Material("Dark Bread", new Color(.42f, .22f, .10f));
            brass = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Reference/Maritime Brass.mat") ?? Material("Brass", new Color(.85f, .68f, .28f));
            oilskin = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Reference/Yellow Oilskin.mat") ?? Material("Yellow Oilskin", new Color(.92f, .72f, .08f));
            darkWool = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Reference/Dark Wool Navy.mat") ?? Material("Dark Wool", new Color(.08f, .11f, .16f));
            leather = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Reference/Weathered Leather.mat") ?? Material("Leather", new Color(.38f, .22f, .12f));
            parchment = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Reference/Aged Parchment.mat") ?? Material("Parchment", new Color(.91f, .85f, .72f));
            clockDial = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Reference/Station Clock Dial.mat") ?? Material("Clock Dial", new Color(.94f, .92f, .86f));
            copperKettle = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Reference/TarnishedCopperKettle.mat") ?? Material("Tarnished Copper", new Color(0.72f, .44f, .28f));
            stewMat = Material("Savory Stew Lit", new Color(.20f, .07f, .02f)); stewMat.SetFloat("_Smoothness", .85f); stewMat.SetFloat("_Metallic", .05f);
            teaMat = Material("Amber Tea Lit", new Color(.28f, .10f, .02f)); teaMat.SetFloat("_Smoothness", .90f); teaMat.SetFloat("_Metallic", .05f);
            pewterMat = Material("Pewter Spoon Mat", new Color(.70f, .72f, .73f)); pewterMat.SetFloat("_Metallic", .75f); pewterMat.SetFloat("_Smoothness", .55f);

            glassMat = Material("Cold Glass Transparent", new Color(.70f, .85f, .95f, .22f));
            glassMat.SetFloat("_Surface", 1.0f);
            glassMat.SetFloat("_Blend", 0.0f);
            glassMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            glassMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            glassMat.SetInt("_ZWrite", 0);
            glassMat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            glassMat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            glassMat.SetFloat("_Smoothness", .95f);
            glassMat.SetFloat("_Metallic", .05f);
            EditorUtility.SetDirty(glassMat);

            redRugMat = Material("Persian Red Rug Mat", new Color(.48f, .10f, .08f));
            linenMat = Material("White Linen Mat", new Color(.88f, .87f, .83f));
            quiltMat = Material("Patchwork Quilt Mat", new Color(.28f, .24f, .20f));
            amberBulbMat = Material("Amber Filament Glow 2600K", new Color(1.0f, .77f, .44f));
            amberBulbMat.EnableKeyword("_EMISSION");
            amberBulbMat.SetColor("_EmissionColor", new Color(1.0f, .77f, .44f) * 6.0f);

            var activeScene = SceneManager.GetActiveScene();
            foreach (var n in new[] { "Tower Interior Modular", "Keeper House Modular", "Cellar Complex Modular" })
            {
                foreach (var r in activeScene.GetRootGameObjects())
                    if (r && r.name == n) Object.DestroyImmediate(r);
                Remove(n);
            }
            EnsureGameplayObjects();
            var td = Find("Tower Door"); if (td) td.position = new Vector3(-0.60f, 0.15f, -6.50f);
            var kh = Find("Keeper House Front Door"); if (kh) kh.position = new Vector3(-9.08f, 0.15f, -9.05f);
            var fridge = Find("Vintage Enameled Refrigerator"); if (fridge) fridge.position = new Vector3(-11.20f, 0.95f, -4.80f);
            var lampObj = Find("Lamp"); if (lampObj) { var lmr = lampObj.GetComponent<MeshRenderer>(); if (lmr) lmr.enabled = false; }
            var beam = Find("Lamp Beam Visual"); if (beam) beam.position = new Vector3(0f, 11.00f, 0f);
            var lc = Find("lantern-cabinet"); if (lc) lc.position = new Vector3(1.80f, 9.75f, -1.80f);
            var cb = Find("crane-bed"); if (cb) cb.position = new Vector3(-1.80f, 6.55f, 0.20f);
            var ul = Find("upper-locker"); if (ul) ul.position = new Vector3(-0.20f, 6.55f, -2.60f);
            var cpw = Find("Cottage Partition West");
            if (cpw)
            {
                cpw.position = new Vector3(-11.35f, 1.50f, -5.00f);
                cpw.localScale = new Vector3(2.30f, 3.00f, 0.20f);
            }

            RepairTower();
            LighthouseInteriorJointRepair.ApplyCurrent();
            RepairCellar();
            RepairMeal();
            RepairTorch();
            EnsureNavMeshLinks();

            Remove("Keeper House Front Door Frame");
            Disable("Cottage Door Jamb North"); Disable("Cottage Door Jamb South");
            Disable("Tower L0 wall 14"); Disable("Tower L0 wall 15"); Disable("Tower L0 wall 16");
            Disable("Tower Cornice 0 wall 14"); Disable("Tower Cornice 0 wall 15"); Disable("Tower Cornice 0 wall 16");
            Disable("GroundFloor"); Disable("LanternFloor"); Disable("Dining Table Lantern");

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

        static Material Material(string name, Color color)
        {
            string path = "Assets/_Project/Art/ArchitectureRepair/" + name + ".mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!m) { m = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = name }; AssetDatabase.CreateAsset(m, path); }
            m.SetColor("_BaseColor", color); m.SetFloat("_Smoothness", .3f); EditorUtility.SetDirty(m); return m;
        }

        static GameObject Box(string name, Vector3 position, Vector3 size, Quaternion rotation, Material material, bool collision = true, Transform parent = null)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube); go.name = name; go.transform.SetParent(parent ? parent : root, false);
            go.transform.position = position; go.transform.rotation = rotation; go.transform.localScale = size; go.GetComponent<Renderer>().sharedMaterial = material;
            if (!collision) Object.DestroyImmediate(go.GetComponent<Collider>()); return go;
        }

        static GameObject Cylinder(string name, Vector3 position, Vector3 size, Quaternion rotation, Material material, bool collision = true, Transform parent = null)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder); go.name = name; go.transform.SetParent(parent ? parent : root, false);
            go.transform.position = position; go.transform.rotation = rotation; go.transform.localScale = size; go.GetComponent<Renderer>().sharedMaterial = material;
            if (!collision) Object.DestroyImmediate(go.GetComponent<Collider>()); return go;
        }

        static GameObject MeshObject(string name, Mesh mesh, Material material, bool collision = true)
        {
            mesh = ArchitectureMeshBuilder.Persist(mesh, $"{folder}/{meshId++:000}.asset");
            var go = new GameObject(name); go.transform.SetParent(root, false);
            if (material) { go.AddComponent<MeshFilter>().sharedMesh = mesh; go.AddComponent<MeshRenderer>().sharedMaterial = material; }
            if (collision) go.AddComponent<MeshCollider>().sharedMesh = mesh;
            return go;
        }

        static void Sector(string name, float inner, float outer, float a, float b, float y0, float y1, float thickness, Material mat, bool collision = true)
        { MeshObject(name, ArchitectureMeshBuilder.Strip(name, inner, outer, a, b, y0, y1, thickness, Mathf.Max(1, Mathf.CeilToInt(Mathf.Abs(b - a) / 3f))), mat, collision); }

        static void RepairTower()
        {
            // 1. Disable all old circular room slabs, stairs, and partition walls
            Disable("Circular Room Floor Slabs");
            Disable("Continuous Spiral Stair Ramp MeshCollider");
            for (int i = 1; i <= 3; i++)
            {
                Disable($"Outer Spiral Stair Flight {i}");
                Disable($"Floor {i} Stair Landing Platform");
            }
            Disable("Lantern Central Beacon Deck"); Disable("Lantern Solid Walkway"); Disable("Lantern Stair Landing");
            Disable("Beacon Iron Pedestal"); Disable("Fresnel Brass Housing"); Disable("Beacon Concentric Glass Lens");
            foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
            {
                if (t.name.StartsWith("Lantern Outer Walkway Segment") || t.name.StartsWith("Wellhole ")
                   || t.name == "Lantern Solid Walkway" || t.name == "Lantern Stair Landing")
                    t.gameObject.SetActive(false);
                if (t.name.StartsWith("Lantern Mullion") || t.name.StartsWith("Lantern Glass Pane")
                    || t.name.StartsWith("Balcony Rail Post") || t.name == "Balcony Rail Top Ring")
                    t.gameObject.SetActive(false);
                if (t.name.StartsWith("Room 1 Wall Arc") || t.name.StartsWith("Room 2 Wall Arc") || t.name.StartsWith("Room 3 Wall Arc")
                    || t.name.StartsWith("Floor 1 Door Jamb") || t.name.StartsWith("Floor 2 Door Jamb") || t.name.StartsWith("Floor 3 Door Jamb")
                    || t.name == "Floor 1 Door Lintel" || t.name == "Floor 2 Door Lintel" || t.name == "Floor 3 Door Lintel"
                    || t.name == "Floor 1 Door Threshold" || t.name == "Floor 2 Door Threshold" || t.name == "Floor 3 Door Threshold"
                    || t.name.StartsWith("Room 1 Masonry") || t.name.StartsWith("Room 2 Masonry") || t.name.StartsWith("Room 3 Masonry")
                    || t.name.StartsWith("Inner Stair Guard"))
                    t.gameObject.SetActive(false);
            }

            // Disable Floor 3 clutter to make it an open Keeper's Bedroom
            Disable("Battery Rack Frame");
            for (int r = 0; r < 2; r++)
                for (int c = 0; c < 4; c++)
                {
                    Disable($"Battery Jar {r}_{c}");
                    Disable($"Lead Terminal {r}_{c}");
                }
            Disable("Fuel Staging Plinth");
            for (int f = 1; f <= 3; f++) Disable($"Reserve Fuel Can {f}");
            Disable("Watchroom Workbench Top"); Disable("Workbench Leg L"); Disable("Workbench Leg R");
            Disable("Maritime Wall Barometer"); Disable("Telegraph Baseplate"); Disable("Telegraph Sounder Coil"); Disable("Telegraph Key Lever");
            Disable("Watchroom Indicator Green"); Disable("Watchroom Indicator Red");

            var bss = Find("Boot Stool Seat"); if (bss) foreach (var c in bss.GetComponents<Collider>()) c.enabled = false;
            var scb = Find("Sea Chest Body"); if (scb) foreach (var c in scb.GetComponents<Collider>()) c.enabled = false;
            var lw = Find("library-wardrobe"); if (lw) lw.position = new Vector3(-0.20f, 6.55f, -2.60f);
            var gl = Find("generator-locker"); if (gl) gl.position = new Vector3(0.50f, 1.00f, -1.50f);
            var ub1 = Find("Utility Barrel 1"); if (ub1) ub1.position = new Vector3(0.20f, 0.55f, -1.85f);
            var tes = Find("Tower East Step"); if (tes) tes.position = new Vector3(5.95f, 0.05f, 0.00f);

            var tm = Find("Tapered Masonry");
            if (tm && tm.TryGetComponent<MeshCollider>(out var tmc))
            {
                tmc.enabled = false;
            }

            // Geometry boundaries
            // Traversal test is at r = 3.925m, capsule r = 0.28m (spans 3.645m to 4.205m)
            // Stairs span r = 3.25m to 4.65m.
            // Central living room floor radius is 3.15m (clearance > 0.49m from stair test).
            float R_wall = 4.65f;
            float R_stair_in = 3.25f;
            float R_room_center = 3.15f;

            // -------------------------------------------------------------
            // FLOOR 1: GROUND ENTRY VESTIBULE & COTTAGE FOUNDATION (y = 0.15m)
            // -------------------------------------------------------------
            Sector("Repaired Floor 1", 0.001f, R_wall, 0, 360, Levels[0], Levels[0], .14f, stone, true);

            // Solid continuous ground floor foundation slabs covering cottage and tower base (Size: 12m x 12m foundation coverage)
            // Main cottage floor ends at Z = -5.0f so it does not obstruct the descending cellar stairwell at (-8, 0.15f, -4.8f)
            var cMain = Box("Cottage Ground Foundation Main", new Vector3(-7.0f, Levels[0] - 0.25f, -8.0f), new Vector3(12.0f, 0.50f, 6.0f), Quaternion.identity, stone, true);
            cMain.layer = LayerMask.NameToLayer("Default");
            var cWest = Box("Cottage Ground Foundation West", new Vector3(-11.1f, Levels[0] - 0.25f, -2.5f), new Vector3(3.8f, 0.50f, 5.0f), Quaternion.identity, stone, true);
            cWest.layer = LayerMask.NameToLayer("Default");
            var cEast = Box("Cottage Ground Foundation East", new Vector3(-3.9f, Levels[0] - 0.25f, -2.5f), new Vector3(5.8f, 0.50f, 5.0f), Quaternion.identity, stone, true);
            cEast.layer = LayerMask.NameToLayer("Default");
            var tEntry = Box("Tower Entryway Foundation Slab", new Vector3(-1.0f, Levels[0] - 0.25f, -5.5f), new Vector3(4.5f, 0.50f, 3.5f), Quaternion.identity, stone, true);
            tEntry.layer = LayerMask.NameToLayer("Default");

            // Seamless Doorway Connector Colliders across thresholds to prevent falling through mesh seams
            var towerThreshold = Box("Doorway Seam Connector Tower", new Vector3(-0.60f, Levels[0] - 0.10f, -6.50f), new Vector3(2.5f, 0.30f, 1.8f), Quaternion.identity, stone, true);
            towerThreshold.layer = LayerMask.NameToLayer("Default");
            var cottageThreshold = Box("Doorway Seam Connector Cottage", new Vector3(-9.08f, Levels[0] - 0.10f, -9.05f), new Vector3(2.5f, 0.30f, 1.8f), Quaternion.identity, stone, true);
            cottageThreshold.layer = LayerMask.NameToLayer("Default");
            var courtyardThreshold = Box("Doorway Seam Connector Courtyard", new Vector3(0.0f, Levels[0] - 0.10f, -8.0f), new Vector3(4.5f, 0.30f, 2.5f), Quaternion.identity, stone, true);
            courtyardThreshold.layer = LayerMask.NameToLayer("Default");

            // Fallback Safety Plane: Invisible trigger box below the island (Y = -2.0m)
            var safetyPlaneObj = GameObject.Find("Fallback Safety Plane");
            if (safetyPlaneObj == null)
            {
                safetyPlaneObj = new GameObject("Fallback Safety Plane");
            }
            safetyPlaneObj.transform.SetParent(root, false);
            safetyPlaneObj.transform.position = new Vector3(0f, -2.0f, -6f);
            var boxCol = safetyPlaneObj.GetComponent<BoxCollider>();
            if (!boxCol) boxCol = safetyPlaneObj.AddComponent<BoxCollider>();
            boxCol.isTrigger = true;
            boxCol.size = new Vector3(140f, 1.0f, 140f);
            if (!safetyPlaneObj.GetComponent<FallbackSafetyPlane>())
            {
                safetyPlaneObj.AddComponent<FallbackSafetyPlane>();
            }

            RepairVestibuleProps();

            // -------------------------------------------------------------
            // FLOOR 2: STUDY / MAP ROOM (y = 3.35m) - OPEN-PLAN WITH CRESCENT WELLHOLE
            // -------------------------------------------------------------
            // Flight 1 ascends from 225° to 435° (75°). Outer stairwell (3.15m to 4.65m) is OPEN from 220° to 75°
            Sector("Repaired Floor 2 Central", 0.001f, R_room_center, 0, 360, Levels[1], Levels[1], .14f, wood, true);
            Sector("Repaired Floor 2 Outer", R_room_center, R_wall, 70, 220, Levels[1], Levels[1], .14f, wood, true);
            BuildWellholeBalustrade("Floor 2 Wellhole Guard", R_room_center, 220, 70 + 360, Levels[1]);
            RepairStudyFloor2();

            // -------------------------------------------------------------
            // FLOOR 3: KEEPER'S BEDROOM / QUARTERS (y = 6.55m) - OPEN-PLAN WITH CRESCENT WELLHOLE
            // -------------------------------------------------------------
            // Flight 2 ascends from 105° to 315°. Outer stairwell (3.15m to 4.65m) is OPEN from 100° to 315°
            Sector("Repaired Floor 3 Central", 0.001f, R_room_center, 0, 360, Levels[2], Levels[2], .14f, wood, true);
            Sector("Repaired Floor 3 Outer", R_room_center, R_wall, 310, 460, Levels[2], Levels[2], .14f, wood, true);
            BuildWellholeBalustrade("Floor 3 Wellhole Guard", R_room_center, 100, 310, Levels[2]);
            RepairBedroomFloor3();

            // -------------------------------------------------------------
            // FLOOR 4: LANTERN ROOM & 360-DEGREE BALCONY CATWALK (y = 9.75m)
            // -------------------------------------------------------------
            // Flight 3 ascends from 345° to 525° (165°). Outer stairwell (3.15m to 4.85m) is OPEN from 340° to 165°
            Sector("Repaired Floor 4 Central", 0.001f, R_room_center, 0, 360, Levels[3], Levels[3], .16f, iron, true);
            Sector("Repaired Landing 4", R_room_center, 4.85f, 166f, 200f, Levels[3], Levels[3], .16f, iron, true);
            BuildWellholeBalustrade("Lantern Deck Wellhole Guard", R_room_center, 340, 520, Levels[3]);
            RepairLanternAndBalcony();

            // -------------------------------------------------------------
            // SPIRAL STAIR FLIGHTS WITH SMOOTH WEDGE RAMPS & 2.6M CLEARANCE
            // -------------------------------------------------------------
            var starts = new[] { 225f, 105f, 345f }; var ends = new[] { 435f, 315f, 525f };
            for (int f = 0; f < 3; f++)
            {
                // Smooth invisible wedge ramp collider (Slope: ~32°, width 1.40m)
                Sector($"Smooth Stair Ramp {f + 1}", R_stair_in, R_wall, starts[f], ends[f], Levels[f], Levels[f + 1], .08f, null, true);

                int count = 24;
                for (int s = 0; s < count; s++)
                {
                    float a = Mathf.Lerp(starts[f], ends[f], (float)s / count);
                    float b = Mathf.Lerp(starts[f], ends[f], (float)(s + 1) / count);
                    float y = Mathf.Lerp(Levels[f], Levels[f + 1], (float)(s + 1) / count);
                    Sector($"Visual Tread {f + 1}-{s}", R_stair_in, R_wall, a, b, y, y, .05f, iron, false);
                }

                // Inner Victorian balustrade & handrail along open inner edge of stair flight (at r = 3.20m)
                int railSteps = 20;
                for (int r = 0; r <= railSteps; r++)
                {
                    float t = (float)r / railSteps;
                    float ang = Mathf.Lerp(starts[f], ends[f], t);
                    float y = Mathf.Lerp(Levels[f], Levels[f + 1], t);
                    Vector3 bp = P(R_stair_in - 0.05f, ang, y);
                    Cylinder($"Stair Baluster {f + 1}_{r}", bp + Vector3.up * 0.525f, new Vector3(0.03f, 0.525f, 0.03f), Quaternion.identity, iron, false);
                }
            }

            // Door frames and approaches for narrative and validation
            RepairDoorsAndTransitions();
        }

        static void BuildWellholeBalustrade(string name, float radius, float startDeg, float endDeg, float deckY)
        {
            var grp = new GameObject(name); grp.transform.SetParent(root, false);
            float span = endDeg - startDeg;
            int posts = Mathf.Max(4, Mathf.CeilToInt(Mathf.Abs(span) / 15f));
            for (int i = 0; i <= posts; i++)
            {
                float t = (float)i / posts;
                float ang = startDeg + t * span;
                Vector3 bp = P(radius, ang, deckY);
                var post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                post.name = $"{name} Post {i}"; post.transform.SetParent(grp.transform, false);
                post.transform.position = bp + Vector3.up * 0.525f;
                post.transform.localScale = new Vector3(0.035f, 0.525f, 0.035f);
                post.GetComponent<Renderer>().sharedMaterial = iron;
                Object.DestroyImmediate(post.GetComponent<Collider>());
            }
        }

        static void RepairDoorsAndTransitions()
        {
            float w = settings.roomDoorWidth, h = settings.roomDoorHeight;

            // Build clear doorways on landing approaches
            for (int k = 0; k < 3; k++)
            {
                float a = Doors[k], y = Levels[k];
                var rot = Quaternion.Euler(0, a, 0); var right = rot * Vector3.right; var p = P(2.95f, a, y);
                var door = Find($"Floor {k + 1} Door");
                if (door)
                {
                    door.position = p - right * w * .5f; door.rotation = rot; door.localScale = Vector3.one;
                    var leaf = door.GetComponentInChildren<MeshRenderer>();
                    if (leaf)
                    {
                        leaf.transform.localPosition = new(w * .5f, h * .5f, 0);
                        leaf.transform.localScale = new(w - .02f, h - .02f, .055f);
                    }
                }
            }

            // Floor 1 Door (arched door between vestibule and cottage)
            var f1d = Find("Floor 1 Door");
            if (f1d)
            {
                var rot = Quaternion.Euler(0, Doors[0], 0); var right = rot * Vector3.right; var p = P(2.95f, Doors[0], Levels[0]);
                f1d.position = p - right * w * .5f; f1d.rotation = rot;
            }

            // Act 4: Crane's Room Door / LockedRoomDoor
            var lrd = Find("Crane's Room Door");
            if (lrd)
            {
                var rot = Quaternion.Euler(0, Doors[2], 0); var right = rot * Vector3.right; var p = P(2.95f, Doors[2], Levels[2]);
                lrd.position = p - right * w * .5f; lrd.rotation = rot;
            }
        }

        static void RepairVestibuleProps()
        {
            GameObject VProp(string name, PrimitiveType type, Vector3 p, Vector3 scale, Quaternion rot, Material mat)
            {
                var g = GameObject.CreatePrimitive(type); g.name = name; g.transform.SetParent(root, false);
                g.transform.position = p; g.transform.rotation = rot; g.transform.localScale = scale;
                g.GetComponent<Renderer>().sharedMaterial = mat; Object.DestroyImmediate(g.GetComponent<Collider>()); return g;
            }
            VProp("Vestibule Coat Rack", PrimitiveType.Cube, new(-3.75f, 1.85f, -1.80f), new(0.08f, 0.14f, 1.80f), Quaternion.identity, wood);
            for (int i = 0; i < 4; i++)
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
        }

        static void RepairStudyFloor2()
        {
            float f2Y = Levels[1]; // 3.35m
            var studyGroup = new GameObject("Study Open Chamber Furnishings").transform;
            studyGroup.SetParent(root, false);

            // Red Persian Study Rug centered in the chamber
            Box("Persian Study Rug", new Vector3(0f, f2Y + 0.015f, 0.20f), new Vector3(3.2f, 0.012f, 2.6f), Quaternion.identity, redRugMat, false, studyGroup);

            // Heavy Mahogany Writing Desk centered in chamber
            Vector3 deskPos = new Vector3(0f, f2Y, 0.35f);
            Box("Mahogany Desk Top", deskPos + new Vector3(0f, 0.75f, 0f), new Vector3(1.8f, 0.08f, 0.95f), Quaternion.identity, wood, true, studyGroup);
            Box("Desk Pedestal Left", deskPos + new Vector3(-0.65f, 0.36f, 0f), new Vector3(0.44f, 0.70f, 0.85f), Quaternion.identity, wood, true, studyGroup);
            Box("Desk Pedestal Right", deskPos + new Vector3(0.65f, 0.36f, 0f), new Vector3(0.44f, 0.70f, 0.85f), Quaternion.identity, wood, true, studyGroup);

            // Terrestrial Globe on Brass Stand
            Cylinder("Globe Base", deskPos + new Vector3(-0.55f, 0.81f, -0.20f), new Vector3(0.18f, 0.04f, 0.18f), Quaternion.identity, brass, false, studyGroup);
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = "Globe Sphere"; sphere.transform.SetParent(studyGroup, false);
            sphere.transform.position = deskPos + new Vector3(-0.55f, 1.04f, -0.20f);
            sphere.transform.localScale = new Vector3(0.24f, 0.24f, 0.24f);
            sphere.transform.rotation = Quaternion.Euler(23.5f, 35f, 0f);
            sphere.GetComponent<Renderer>().sharedMaterial = parchment;
            Object.DestroyImmediate(sphere.GetComponent<Collider>());

            // Open Handwritten Maritime Logbook
            Vector3 logP = deskPos + new Vector3(-0.10f, 0.801f, -0.06f);
            Box("Logbook Cover", logP, new Vector3(0.50f, 0.018f, 0.35f), Quaternion.Euler(0, 10f, 0), leather, false, studyGroup);
            Box("Logbook Pages Left", logP + new Vector3(-0.12f, 0.012f, 0f), new Vector3(0.22f, 0.012f, 0.32f), Quaternion.Euler(0, 10f, 0), parchment, false, studyGroup);
            Box("Logbook Pages Right", logP + new Vector3(0.12f, 0.012f, 0f), new Vector3(0.22f, 0.012f, 0.32f), Quaternion.Euler(0, 10f, 0), parchment, false, studyGroup);

            // Vintage Gooseneck Oil Lamp
            Vector3 lampP = deskPos + new Vector3(0.55f, 0.795f, 0.18f);
            Cylinder("Study Desk Lamp Base", lampP + Vector3.up * 0.04f, new Vector3(0.15f, 0.08f, 0.15f), Quaternion.identity, brass, false, studyGroup);
            Cylinder("Study Desk Lamp Chimney", lampP + Vector3.up * 0.25f, new Vector3(0.08f, 0.20f, 0.08f), Quaternion.identity, glassMat, false, studyGroup);

            // Carved Armchair behind desk
            Box("Armchair Seat", deskPos + new Vector3(0.10f, 0.44f, -0.60f), new Vector3(0.55f, 0.08f, 0.50f), Quaternion.Euler(0, 15f, 0), wood, true, studyGroup);
            Box("Armchair Back", deskPos + new Vector3(0.10f, 0.85f, -0.85f), new Vector3(0.50f, 0.75f, 0.06f), Quaternion.Euler(0, 15f, 0), wood, false, studyGroup);

            // Tall Curved Wooden Bookcases along East stone wall (away from stairs)
            Box("Tall Bookcase Frame", new Vector3(2.50f, f2Y + 1.40f, -1.80f), new Vector3(1.60f, 2.60f, 0.40f), Quaternion.Euler(0, -45f, 0), wood, true, studyGroup);
            // Firewood logs stack beside bookcase
            for (int log = 0; log < 4; log++)
            {
                float ly = f2Y + 0.10f + (log / 2) * 0.16f;
                float lz = -1.20f + (log % 2) * 0.16f;
                Cylinder($"Firewood Log {log}", new Vector3(2.80f, ly, lz), new Vector3(0.12f, 0.35f, 0.12f), Quaternion.Euler(0, 0, 90f), wood, false, studyGroup);
            }

            // Warm Low-Hanging Amber Pendant Lamp
            var pendant = new GameObject("Study Ambient Pendant Light");
            pendant.transform.SetParent(studyGroup, false);
            pendant.transform.position = new Vector3(0f, f2Y + 2.40f, 0.35f);
            Cylinder("Pendant Shade", pendant.transform.position + Vector3.up * 0.08f, new Vector3(0.45f, 0.10f, 0.45f), Quaternion.identity, iron, false, studyGroup);
            var pLight = pendant.AddComponent<Light>();
            pLight.type = LightType.Point;
            pLight.color = new Color(1.0f, 0.74f, 0.40f);
            pLight.intensity = 3.5f;
            pLight.range = 7.5f;
            pLight.shadows = LightShadows.Soft;
        }

        static void RepairBedroomFloor3()
        {
            float f3Y = Levels[2]; // 6.55m
            var bedGroup = new GameObject("Keeper Bedroom Quarters Furnishings").transform;
            bedGroup.SetParent(root, false);

            // 1. Victorian Black Cast-Iron Frame Bed with Quilt & Pillows (East/NE sector, safe from stairs)
            Vector3 bedPos = new Vector3(1.60f, f3Y, 0.80f);
            Quaternion bedRot = Quaternion.Euler(0, -60f, 0);

            // Bed Mattress & Quilt
            Box("Bed Mattress", bedPos + new Vector3(0f, 0.38f, 0f), new Vector3(1.30f, 0.30f, 1.90f), bedRot, linenMat, true, bedGroup);
            Box("Patchwork Quilt", bedPos + new Vector3(0f, 0.46f, -0.15f), new Vector3(1.26f, 0.16f, 1.50f), bedRot, quiltMat, false, bedGroup);
            Box("Bed Pillow", bedPos + new Vector3(0f, 0.56f, 0.75f), new Vector3(0.85f, 0.14f, 0.30f), bedRot, linenMat, false, bedGroup);

            // Cast-iron headboard and footboard
            Box("Bed Headboard", bedPos + bedRot * new Vector3(0f, 0.65f, 0.95f), new Vector3(1.35f, 0.80f, 0.06f), bedRot, iron, true, bedGroup);
            Box("Bed Footboard", bedPos + bedRot * new Vector3(0f, 0.45f, -0.95f), new Vector3(1.35f, 0.50f, 0.06f), bedRot, iron, true, bedGroup);

            // 2. Bedside Wooden Nightstand & Family Photo (Crane holding Daniel)
            Vector3 standPos = new Vector3(0.65f, f3Y, 1.80f);
            Box("Nightstand Body", standPos + Vector3.up * 0.35f, new Vector3(0.50f, 0.70f, 0.50f), Quaternion.identity, wood, true, bedGroup);

            // Framed Childhood Family Photo (A4/A5 Narrative Key Anchor)
            var photoObj = Find("Family Photo");
            if (photoObj == null)
            {
                photoObj = Box("Family Photo", standPos + new Vector3(0f, 0.85f, 0f), new Vector3(0.22f, 0.26f, 0.04f), Quaternion.Euler(-10f, 20f, 0f), wood, true, bedGroup).transform;
                photoObj.gameObject.AddComponent<FamilyPhoto>();
            }
            else
            {
                photoObj.position = standPos + new Vector3(0f, 0.85f, 0f);
                photoObj.rotation = Quaternion.Euler(-10f, 20f, 0f);
            }
            Box("Family Photo Print", photoObj.position + photoObj.forward * 0.02f, new Vector3(0.18f, 0.22f, 0.01f), photoObj.rotation, parchment, false, bedGroup);

            // Bedside Warm Brass Lamp
            Cylinder("Bedside Lamp Base", standPos + new Vector3(0.12f, 0.74f, 0.12f), new Vector3(0.10f, 0.06f, 0.10f), Quaternion.identity, brass, false, bedGroup);
            Cylinder("Bedside Lamp Shade", standPos + new Vector3(0.12f, 0.88f, 0.12f), new Vector3(0.14f, 0.14f, 0.14f), Quaternion.identity, glassMat, false, bedGroup);
            var bLight = new GameObject("Bedside Ambient Light").AddComponent<Light>();
            bLight.transform.SetParent(bedGroup, false);
            bLight.transform.position = standPos + new Vector3(0.12f, 0.90f, 0.12f);
            bLight.type = LightType.Point;
            bLight.color = new Color(1.0f, 0.72f, 0.36f);
            bLight.intensity = 1.8f;
            bLight.range = 5.0f;

            // 3. Heavy Leather/Brass Sea Chest at foot of bed
            Box("Sea Chest", bedPos + bedRot * new Vector3(0f, 0.22f, -1.45f), new Vector3(1.00f, 0.44f, 0.48f), bedRot, leather, true, bedGroup);

            // 4. Solid Wood Wardrobe / Armoire against South wall
            Box("Keeper Wardrobe", new Vector3(-0.20f, f3Y + 1.20f, -2.40f), new Vector3(1.10f, 2.30f, 0.55f), Quaternion.Euler(0, 10f, 0), wood, true, bedGroup);

            // 5. Cast-Iron Potbelly Heating Stove with Glowing Embers & Flue Pipe
            Vector3 stovePos = new Vector3(-1.80f, f3Y, -0.60f);
            Cylinder("Stove Hearth Base", stovePos + Vector3.up * 0.04f, new Vector3(0.75f, 0.08f, 0.75f), Quaternion.identity, iron, true, bedGroup);
            Cylinder("Potbelly Stove Belly", stovePos + Vector3.up * 0.42f, new Vector3(0.55f, 0.55f, 0.55f), Quaternion.identity, iron, true, bedGroup);
            Cylinder("Potbelly Stove Top Cap", stovePos + Vector3.up * 0.75f, new Vector3(0.40f, 0.10f, 0.40f), Quaternion.identity, iron, false, bedGroup);
            Cylinder("Stove Stovepipe Flue", stovePos + Vector3.up * 2.0f, new Vector3(0.14f, 2.40f, 0.14f), Quaternion.identity, iron, false, bedGroup);

            // Warm firebox glow light from the stove
            var stoveLight = new GameObject("Stove Hearth Fire Light").AddComponent<Light>();
            stoveLight.transform.SetParent(bedGroup, false);
            stoveLight.transform.position = stovePos + Vector3.up * 0.42f;
            stoveLight.type = LightType.Point;
            stoveLight.color = new Color(1.0f, 0.45f, 0.15f);
            stoveLight.intensity = 2.2f;
            stoveLight.range = 5.5f;

            // 6. Round Station Wall Clock mounted on stone wall
            Cylinder("Station Wall Clock", new Vector3(0.00f, f3Y + 2.20f, 4.60f), new Vector3(0.45f, 0.04f, 0.45f), Quaternion.Euler(90, 0, 0), clockDial, false, bedGroup);
        }

        static void RepairLanternAndBalcony()
        {
            float deckY = Levels[3]; // 9.75m
            var lanternRoot = new GameObject("Lantern Deck & Catwalk Master").transform;
            lanternRoot.SetParent(root, false);

            float glassR = 3.35f;
            float catwalkR = 4.85f; // Exactly 1.50m width (3.35m to 4.85m)

            // Catwalk open sector: Flight 3 ascends from 345° to 165°.
            // Catwalk platform covers 166° through 180°, 270°, to 340°!
            Sector("Lantern Balcony Catwalk", glassR, catwalkR, 166f, 340f, deckY, deckY, .16f, iron, true);

            // 12 Cantilevered Corbel Brackets supporting catwalk overhang
            for (int b = 0; b < 12; b++)
            {
                float ang = 166f + b * (174f / 11f);
                float rad = ang * Mathf.Deg2Rad;
                Vector3 bp = new Vector3(Mathf.Sin(rad) * ((glassR + catwalkR) * 0.5f), deckY - 0.20f, Mathf.Cos(rad) * ((glassR + catwalkR) * 0.5f));
                Box($"Catwalk Corbel Bracket {b}", bp, new Vector3(0.12f, 0.30f, 1.10f), Quaternion.Euler(0, ang, 0), stone, false, lanternRoot);
            }

            // Outer Victorian Cast-Iron Guard Rail (1.10m high at R = 4.85m)
            int railPosts = 16;
            for (int r = 0; r <= railPosts; r++)
            {
                float ang = 166f + r * (174f / railPosts);
                float rad = ang * Mathf.Deg2Rad;
                Vector3 rp = new Vector3(Mathf.Sin(rad) * (catwalkR - 0.05f), deckY, Mathf.Cos(rad) * (catwalkR - 0.05f));
                Cylinder($"Catwalk Rail Post {r}", rp + Vector3.up * 0.55f, new Vector3(0.04f, 0.55f, 0.04f), Quaternion.identity, iron, false, lanternRoot);
            }
            Sector("Catwalk Outer Safety Collider", catwalkR - 0.05f, catwalkR, 166f, 340f, deckY + 1.10f, deckY + 1.10f, 1.10f, null, true);

            // Glazed Glass Dome & Bronze Mullions with Doorway Opening at 180°
            int panes = 16;
            float doorAng = 180f;
            for (int p = 0; p < panes; p++)
            {
                float ang = p * (360f / panes);
                if (Mathf.Abs(Mathf.DeltaAngle(ang, doorAng)) < 12.0f) continue;

                float rad = ang * Mathf.Deg2Rad;
                Vector3 gp = new Vector3(Mathf.Sin(rad) * glassR, deckY + 1.35f, Mathf.Cos(rad) * glassR);
                Quaternion gr = Quaternion.Euler(0, ang, 0);

                Box($"Lantern Mullion {p}", gp, new Vector3(0.08f, 2.70f, 0.10f), gr, brass, false, lanternRoot);
                float tangW = 2f * Mathf.PI * glassR / panes;
                Box($"Lantern Glass {p}", gp, new Vector3(tangW, 2.65f, 0.02f), gr, glassMat, false, lanternRoot);
            }

            // Heavy Weather-Sealed Metal Door leading out onto Catwalk (0.90m x 2.00m)
            Vector3 doorCenter = P(glassR, doorAng, deckY);
            Quaternion doorRot = Quaternion.Euler(0, doorAng, 0);
            Vector3 rightDir = doorRot * Vector3.right;

            Box("Balcony Door Jamb L", doorCenter + Vector3.up * 1.05f - rightDir * 0.48f, new Vector3(0.10f, 2.10f, 0.18f), doorRot, iron, true, lanternRoot);
            Box("Balcony Door Jamb R", doorCenter + Vector3.up * 1.05f + rightDir * 0.48f, new Vector3(0.10f, 2.10f, 0.18f), doorRot, iron, true, lanternRoot);
            Box("Balcony Door Lintel", doorCenter + Vector3.up * 2.15f, new Vector3(1.06f, 0.16f, 0.18f), doorRot, iron, true, lanternRoot);

            var balconyDoor = Find("Lantern Balcony Door");
            if (balconyDoor == null)
            {
                var bdGo = new GameObject("Lantern Balcony Door");
                bdGo.transform.SetParent(lanternRoot, false);
                bdGo.transform.position = doorCenter - rightDir * 0.44f;
                bdGo.transform.rotation = doorRot;
                var doorComp = bdGo.AddComponent<Door>();
                var so = new SerializedObject(doorComp);
                so.FindProperty("doorId").stringValue = "lantern-balcony-door";
                so.FindProperty("openAngle").floatValue = 100f;
                so.FindProperty("turnSpeed").floatValue = 120f;
                so.ApplyModifiedPropertiesWithoutUndo();

                var leaf = Box("Balcony Door Leaf", bdGo.transform.position + rightDir * 0.44f + Vector3.up * 1.00f, new Vector3(0.88f, 1.98f, 0.055f), doorRot, iron, true, bdGo.transform);
                Box("Balcony Door Handle", leaf.transform.position + rightDir * 0.32f, new Vector3(0.04f, 0.10f, 0.10f), doorRot, brass, false, leaf.transform);
            }
            else
            {
                balconyDoor.position = doorCenter - rightDir * 0.44f;
                balconyDoor.rotation = doorRot;
            }

            RepairKnifeSwitchAndCrank(lanternRoot, deckY);
        }

        static void RepairKnifeSwitchAndCrank(Transform parent, float deckY)
        {
            Vector3 switchPos = new Vector3(1.05f, deckY + 1.10f, 0.50f);
            var switchGo = Find("Beacon Wall Knife Switch");
            if (switchGo == null)
            {
                switchGo = new GameObject("Beacon Wall Knife Switch").transform;
                switchGo.SetParent(parent, false);
                switchGo.position = switchPos;
                switchGo.rotation = Quaternion.Euler(0, -90f, 0);

                Cylinder("Switch Conduit Pipe", new Vector3(1.05f, deckY + 0.55f, 0.50f), new Vector3(0.05f, 1.10f, 0.05f), Quaternion.identity, iron, false, switchGo);
                Box("Switch Slate Base", switchPos, new Vector3(0.18f, 0.30f, 0.04f), switchGo.rotation, stone, true, switchGo);
                Box("Switch Jaw Top", switchPos + Vector3.up * 0.09f + switchGo.forward * 0.03f, new Vector3(0.04f, 0.05f, 0.04f), switchGo.rotation, brass, false, switchGo);
                Box("Switch Jaw Bottom", switchPos - Vector3.up * 0.09f + switchGo.forward * 0.03f, new Vector3(0.04f, 0.05f, 0.04f), switchGo.rotation, brass, false, switchGo);

                var leverRoot = new GameObject("Knife Switch Lever");
                leverRoot.transform.SetParent(switchGo, false);
                leverRoot.transform.position = switchPos - Vector3.up * 0.09f + switchGo.forward * 0.03f;
                Box("Copper Blade", leverRoot.transform.position + Vector3.up * 0.09f, new Vector3(0.015f, 0.18f, 0.03f), switchGo.rotation, copperKettle, false, leverRoot.transform);
                Cylinder("Ebonite Handle", leverRoot.transform.position + Vector3.up * 0.20f, new Vector3(0.03f, 0.08f, 0.03f), switchGo.rotation, darkWool, false, leverRoot.transform);

                var sparkGo = new GameObject("Contact Spark FX");
                sparkGo.transform.SetParent(switchGo, false);
                sparkGo.transform.position = switchPos + Vector3.up * 0.09f + switchGo.forward * 0.04f;
                var ps = sparkGo.AddComponent<ParticleSystem>();
                var psr = sparkGo.GetComponent<ParticleSystemRenderer>();
                var particleMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Level/Particle.mat");
                if (psr && particleMat) psr.sharedMaterial = particleMat;
                var main = ps.main;
                main.startLifetime = 0.15f;
                main.startSpeed = 3.5f;
                main.startSize = 0.06f;
                main.startColor = new Color(0.8f, 0.9f, 1.0f);
                main.playOnAwake = false;
                main.loop = false;
                var emit = ps.emission; emit.rateOverTime = 0;
                var burst = new ParticleSystem.Burst(0f, 15);
                emit.SetBursts(new[] { burst });
                var sparkLight = sparkGo.AddComponent<Light>();
                sparkLight.type = LightType.Point;
                sparkLight.color = new Color(0.7f, 0.85f, 1.0f);
                sparkLight.range = 3.5f;
                sparkLight.intensity = 5.0f;
                sparkLight.enabled = false;

                var ksComp = switchGo.gameObject.AddComponent<BeaconKnifeSwitch>();
                var so = new SerializedObject(ksComp);
                so.FindProperty("switchLever").objectReferenceValue = leverRoot.transform;
                so.FindProperty("sparkParticles").objectReferenceValue = ps;
                so.FindProperty("sparkFlashLight").objectReferenceValue = sparkLight;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            var crankGo = Find("Crank Station");
            if (crankGo != null)
            {
                crankGo.position = new Vector3(-0.65f, deckY + 0.65f, -0.35f);
                crankGo.rotation = Quaternion.Euler(0, 45f, 0);

                Box("Crank Gearbox Housing", crankGo.position, new Vector3(0.35f, 0.35f, 0.35f), crankGo.rotation, iron, true, crankGo);
                Cylinder("Fresnel Spur Gear", crankGo.position + Vector3.up * 0.12f, new Vector3(0.28f, 0.04f, 0.28f), crankGo.rotation, brass, false, crankGo);

                var handleArm = crankGo.Find("Mechanical Crank Arm");
                if (handleArm == null)
                {
                    handleArm = new GameObject("Mechanical Crank Arm").transform;
                    handleArm.SetParent(crankGo, false);
                    handleArm.localPosition = new Vector3(0f, 0f, 0.20f);

                    Box("Iron Crank Crankshaft", handleArm.position + handleArm.up * 0.12f, new Vector3(0.04f, 0.25f, 0.025f), handleArm.rotation, iron, false, handleArm);
                    Cylinder("Turned Wooden Crank Knob", handleArm.position + handleArm.up * 0.24f + handleArm.forward * 0.08f, new Vector3(0.06f, 0.12f, 0.06f), handleArm.rotation * Quaternion.Euler(90f, 0, 0), wood, false, handleArm);
                }

                var cs = crankGo.GetComponent<CrankStation>();
                if (cs != null)
                {
                    var so = new SerializedObject(cs);
                    so.FindProperty("prompt").stringValue = "Press [E] to Crank Fresnel Beacon (hold)";
                    so.FindProperty("handle").objectReferenceValue = handleArm;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            Vector3 beaconCenter = new Vector3(0f, deckY, 0f);
            Cylinder("Beacon Cast-Iron Pedestal", beaconCenter + Vector3.up * 0.28f, new Vector3(1.20f, 0.28f, 1.20f), Quaternion.identity, iron, true, parent);
            Cylinder("Fresnel Concentric Brass Collar", beaconCenter + Vector3.up * 0.70f, new Vector3(0.95f, 0.35f, 0.95f), Quaternion.identity, brass, true, parent);
            Cylinder("Fresnel Prism Ring Assembly", beaconCenter + Vector3.up * 1.25f, new Vector3(0.88f, 0.75f, 0.88f), Quaternion.identity, glassMat, false, parent);

            var bulb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bulb.name = "Fresnel Filament Bulb 2600K"; bulb.transform.SetParent(parent, false);
            bulb.transform.position = beaconCenter + Vector3.up * 1.25f;
            bulb.transform.localScale = new Vector3(0.24f, 0.32f, 0.24f);
            bulb.GetComponent<Renderer>().sharedMaterial = amberBulbMat;
            Object.DestroyImmediate(bulb.GetComponent<Collider>());

            var beaconGlow = new GameObject("Beacon Chamber 2600K Glow").AddComponent<Light>();
            beaconGlow.transform.SetParent(parent, false);
            beaconGlow.transform.position = beaconCenter + Vector3.up * 1.25f;
            beaconGlow.type = LightType.Point;
            beaconGlow.color = new Color(1.0f, 0.77f, 0.44f);
            beaconGlow.intensity = 8.0f;
            beaconGlow.range = 14.0f;
            beaconGlow.shadows = LightShadows.Soft;

            var beamObj = Find("Lamp Beam Visual");
            if (beamObj != null)
            {
                beamObj.position = beaconCenter + Vector3.up * 1.25f;
                var spot = beamObj.GetComponentInChildren<Light>();
                if (spot != null)
                {
                    spot.type = LightType.Spot;
                    spot.color = new Color(1.0f, 0.80f, 0.52f);
                    spot.spotAngle = 28f;
                    spot.innerSpotAngle = 14f;
                    spot.range = 180f;
                    spot.intensity = 28f;
                    spot.shadows = LightShadows.Soft;
                }
            }
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
            Disable("Flooded Corridor");
            Disable("Flooded Corridor Trigger");

            // Water Depth Mask / Exclusion Volume covering subterranean staircase and radio room
            var maskObj = GameObject.Find("Cellar Water Depth Mask");
            if (maskObj == null)
            {
                maskObj = new GameObject("Cellar Water Depth Mask");
            }
            maskObj.transform.SetParent(root, false);
            maskObj.transform.position = new Vector3(-8.0f, -2.5f, 3.5f);
            if (!maskObj.GetComponent<WaterDepthMask>())
            {
                maskObj.AddComponent<WaterDepthMask>();
            }
            if (!Find("Low Subterranean Corridor"))
            {
                var p = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Art/ArchitectureRepair/SubterraneanCorridor.prefab");
                if (p) { var inst = Object.Instantiate(p); inst.name = "Low Subterranean Corridor"; }
            }
            if (!Find("Secret Recorded Radio Room"))
            {
                var p = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Art/ArchitectureRepair/SecretRadioRoom.prefab");
                if (p) { var inst = Object.Instantiate(p); inst.name = "Secret Recorded Radio Room"; }
            }
            float top = .15f, bottom = top - settings.cellarDescent;
            foreach (string n in new[] { "Low Subterranean Corridor", "Secret Recorded Radio Room" })
            {
                var g = Find(n); if (g) g.localPosition = new Vector3(0, bottom - (-3.125f), 2f);
            }
            Disable("Cellar Stone Vault Ceiling");
            Disable("Secret Room Timber Door (Ajar)");
            var door = Find("Locked Cellar Door");
            if (door)
            {
                foreach (var r in door.GetComponentsInChildren<Renderer>()) r.enabled = false;
                foreach (var c in door.GetComponentsInChildren<Collider>()) c.enabled = false;
                door.localScale = Vector3.one; door.position = new(-8.6f, top, -5f); door.rotation = Quaternion.identity;
                Box("Cellar Hinged Door", new(-8, top + 1.15f, -5), new(1.18f, 2.28f, .06f), Quaternion.identity, wood, true, door);
            }
            float half = settings.cellarWidth * .5f;
            Box("Cellar Masonry Jamb L", new(-8 - half - .12f, top + 1.2f, -5), new(.24f, 2.4f, .35f), Quaternion.identity, stone);
            Box("Cellar Masonry Jamb R", new(-8 + half + .12f, top + 1.2f, -5), new(.24f, 2.4f, .35f), Quaternion.identity, stone);
            for (int i = 0; i < 18; i++)
            {
                float a = (i + .5f) * 180f / 18f * Mathf.Deg2Rad;
                Box("Cellar Arch Voussoir", new(-8 + Mathf.Cos(a) * (half + .12f), top + 2.4f + Mathf.Sin(a) * (half + .12f), -5), new(.25f, .25f, .35f), Quaternion.Euler(0, 0, a * Mathf.Rad2Deg - 90), stone);
            }
            var v = new[] { new Vector3(-8 - half, top, -5), new Vector3(-8 + half, top, -5), new Vector3(-8 - half, bottom, 1.5f), new Vector3(-8 + half, bottom, 1.5f) };
            var mesh = new Mesh { vertices = v, triangles = new[] { 0, 2, 1, 1, 2, 3 }, uv = new[] { Vector2.zero, Vector2.right, Vector2.up * 6.5f, new Vector2(1, 6.5f) } };
            mesh.RecalculateNormals(); MeshObject("Repaired Cellar Ramp", mesh, null);
            for (int i = 0; i < 30; i++)
            {
                float t = (i + .5f) / 30f;
                Box("Cellar Stone Tread", new(-8, Mathf.Lerp(top, bottom, (i + 1f) / 30) - .06f, Mathf.Lerp(-5, 1.5f, t)), new(settings.cellarWidth, .12f, 6.5f / 30), Quaternion.identity, stone, false);
            }
            for (int side = -1; side <= 1; side += 2)
                Box("Cellar Stair Side Wall", new(-8 + side * (half + .13f), (top + bottom) * .5f + 1.3f, -1.75f), new(.25f, 7.5f, 6.5f), Quaternion.identity, stone);
            for (int i = 0; i < 24; i++)
            {
                float a = (i + .5f) * 180f / 24 * Mathf.Deg2Rad;
                Box("Corridor Barrel Vault", new(-8 + Mathf.Cos(a) * 1.8f, bottom + 2.6f + Mathf.Sin(a) * 1.8f, 4), new(.25f, .25f, 5.6f), Quaternion.Euler(0, 0, a * Mathf.Rad2Deg - 90), stone);
            }
            var g1 = Find("hidden shelf"); if (g1) g1.position = new Vector3(-8.00f, bottom + 1.30f, 4.50f);
            var g2 = Find("Harbor board ledger"); if (g2) g2.position = new Vector3(-7.60f, bottom + 0.82f, 10.60f);
            var g3 = Find("Marta reel-to-reel recorder"); if (g3) g3.position = new Vector3(-8.60f, bottom + 0.90f, 10.70f);
            var csd = Find("Courtyard slam door"); if (csd) csd.position = new Vector3(-6.00f, 0.00f, -10.50f);

            var nw = Find("North Wall");
            if (nw)
            {
                nw.gameObject.SetActive(false);
                Box("North Wall Left", new(-10.65f, 1.5f, -1.0f), new(3.7f, 3.0f, 0.3f), Quaternion.identity, stone);
                Box("North Wall Right", new(-6.20f, 1.5f, -1.0f), new(2.0f, 3.0f, 0.3f), Quaternion.identity, stone);
                Box("North Wall Lintel", new(-8.00f, 1.85f, -1.0f), new(1.6f, 2.3f, 0.3f), Quaternion.identity, stone);
            }
            var hdr = Find("Secret Room Doorway Arched Header");
            if (hdr) hdr.localPosition += Vector3.up * 0.15f;

            var island = Find("Harrow Rock Island");
            if (island && island.TryGetComponent<MeshCollider>(out var islandCollider))
            {
                var fallbackMesh = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/_Project/Art/ArchitectureRepair/Act1/IslandOpening.asset");
                if (islandCollider.sharedMesh == null || islandCollider.sharedMesh.vertexCount < 100)
                {
                    if (fallbackMesh)
                    {
                        islandCollider.sharedMesh = fallbackMesh;
                        if (island.TryGetComponent<MeshFilter>(out var filter)) filter.sharedMesh = fallbackMesh;
                    }
                }
                else if (islandCollider.sharedMesh.name != "Island with cellar opening")
                {
                    var cut = ArchitectureTerrainCut.Cut(islandCollider.sharedMesh, island, new Rect(-8 - half - .08f, -5.02f, 2 * half + .16f, 6.6f));
                    if (cut != null && cut.vertexCount > 100)
                    {
                        cut = ArchitectureMeshBuilder.Persist(cut, folder + "/IslandOpening.asset");
                        islandCollider.sharedMesh = cut;
                        if (island.TryGetComponent<MeshFilter>(out var filter)) filter.sharedMesh = cut;
                    }
                    else if (fallbackMesh)
                    {
                        islandCollider.sharedMesh = fallbackMesh;
                        if (island.TryGetComponent<MeshFilter>(out var filter)) filter.sharedMesh = fallbackMesh;
                    }
                }
            }
            if (Find("Grand Entrance Threshold Sill") == null)
            {
                Box("Grand Entrance Threshold Sill", new Vector3(-0.60f, 0.05f, -6.25f), new Vector3(1.4f, 0.2f, 0.8f), Quaternion.identity, stone);
            }
        }

        static void RepairMeal()
        {
            foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
                if (t.name.StartsWith("Meal ") || t.name.StartsWith("Sunflower ") || t.name.StartsWith("Table Lantern ") || t.name == "Dining Table Runner" || t.name == "Dining Tabletop" || t.name == "Dining Table Base" || t.name == "Dining Table Pedestal" || new[] { "Steaming Stew Bowl", "Chipped Ceramic Mug", "Loaf of Dark Bread", "Steam placeholder", "Pewter Spoon Handle", "Pewter Spoon Head", "Ceramic Stew Bowl", "Stew Surface Lit", "Meal Steam", "Ceramic Cup", "Cup Tea", "Bread Loaf", "Breadboard" }.Contains(t.name)) t.gameObject.SetActive(false);
            var da = Find("DiningArea");
            if (da)
            {
                foreach (var r in da.GetComponentsInChildren<Renderer>(true))
                    if (new[] { "Aged Parchment", "Maritime Brass", "Warm Cottage Glass", "Cottage Ceramic", "Coastal Scrub", "Dark Wool Navy", "Sunflower Petal", "Lamp Enamel" }.Contains(r.gameObject.name))
                        r.gameObject.SetActive(false);
            }
            Disable("Dining Overhead Pendant Light");
            Disable("Dining Table Ambient Fill");
            Disable("Dining Table Lantern");
            var bulb = Find("Kitchen Lamp Bulb");
            if (bulb)
            {
                var l = bulb.GetComponent<Light>();
                if (!l) l = bulb.gameObject.AddComponent<Light>();
                l.type = LightType.Point; l.color = new Color(1f, .82f, .62f); l.intensity = 2.4f; l.range = 5f; l.enabled = true;
            }
            var tbl = Find("Table"); if (tbl) tbl.position = new Vector3(-9.50f, 0.42f, -6.80f);
            var hm = Find("Hot Meal");
            if (hm)
            {
                hm.position = new Vector3(-9.50f, 0.88f, -6.85f);
                var mr = hm.GetComponent<MeshRenderer>();
                if (mr) mr.enabled = false;
            }
            var spoon = Find("Wooden Spoon");
            if (spoon)
            {
                var mr = spoon.GetComponent<MeshRenderer>();
                if (mr) mr.enabled = false;
            }

            GameObject Prop(string name, PrimitiveType type, Vector3 p, Vector3 scale, Material mat)
            {
                var g = GameObject.CreatePrimitive(type); g.name = name; g.transform.SetParent(root, false);
                g.transform.position = p; g.transform.localScale = scale; g.GetComponent<Renderer>().sharedMaterial = mat;
                Object.DestroyImmediate(g.GetComponent<Collider>()); return g;
            }

            Prop("Breadboard", PrimitiveType.Cube, new(-9.20f, 0.872f, -6.85f), new(0.32f, 0.024f, 0.44f), wood);
            Prop("Bread Loaf", PrimitiveType.Sphere, new(-9.20f, 0.934f, -6.85f), new(0.18f, 0.10f, 0.28f), bread);

            Prop("Ceramic Stew Bowl", PrimitiveType.Cylinder, new(-9.50f, 0.910f, -6.85f), new(0.30f, 0.050f, 0.30f), ceramic);
            Prop("Stew Surface Lit", PrimitiveType.Cylinder, new(-9.50f, 0.961f, -6.85f), new(0.26f, 0.002f, 0.26f), stewMat);
            var steam = new GameObject("Meal Steam"); steam.transform.SetParent(root, false); steam.transform.position = new(-9.50f, 0.965f, -6.85f); steam.AddComponent<SteamEmitter>();

            var sh = Prop("Pewter Spoon Handle", PrimitiveType.Cube, new(-9.42f, 0.865f, -7.08f), new(0.022f, 0.010f, 0.16f), pewterMat);
            sh.transform.rotation = Quaternion.Euler(0, 35, 0);
            var sp = Prop("Pewter Spoon Head", PrimitiveType.Sphere, new(-9.36f, 0.866f, -7.00f), new(0.048f, 0.012f, 0.075f), pewterMat);
            sp.transform.rotation = Quaternion.Euler(0, 35, 0);

            Prop("Ceramic Cup", PrimitiveType.Cylinder, new(-9.80f, 0.920f, -6.85f), new(0.11f, 0.060f, 0.11f), ceramic);
            Prop("Cup Tea", PrimitiveType.Cylinder, new(-9.80f, 0.9805f, -6.85f), new(0.095f, 0.002f, 0.095f), teaMat);

            var kettle = Find("Tarnished Copper Kettle");
            if (kettle)
            {
                kettle.position = new Vector3(-9.62f, 0.860f, -6.48f);
                var kb = kettle.Find("Kettle Body");
                if (kb)
                {
                    kb.localPosition = new Vector3(0, 0.11f, 0);
                    kb.localScale = new Vector3(0.26f, 0.11f, 0.26f);
                    kb.GetComponent<Renderer>().sharedMaterial = copperKettle;
                }
            }
            else
            {
                var kRoot = new GameObject("Tarnished Copper Kettle");
                kRoot.transform.SetParent(root, false);
                kRoot.transform.position = new Vector3(-9.62f, 0.860f, -6.48f);
                var kb = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                kb.name = "Kettle Body"; kb.transform.SetParent(kRoot.transform, false);
                kb.transform.localPosition = new Vector3(0, 0.11f, 0);
                kb.transform.localScale = new Vector3(0.26f, 0.11f, 0.26f);
                kb.GetComponent<Renderer>().sharedMaterial = copperKettle;
                Object.DestroyImmediate(kb.GetComponent<Collider>());
            }
            kettle = Find("Tarnished Copper Kettle");
            if (kettle)
            {
                var knob = kettle.Find("Kettle Lid Knob")?.gameObject;
                if (!knob) { knob = GameObject.CreatePrimitive(PrimitiveType.Sphere); knob.name = "Kettle Lid Knob"; knob.transform.SetParent(kettle, false); Object.DestroyImmediate(knob.GetComponent<Collider>()); }
                knob.transform.localPosition = new Vector3(0, 0.23f, 0); knob.transform.localScale = new Vector3(0.06f, 0.05f, 0.06f); knob.GetComponent<Renderer>().sharedMaterial = brass;

                var handle = kettle.Find("Kettle Handle")?.gameObject;
                if (!handle) { handle = GameObject.CreatePrimitive(PrimitiveType.Cube); handle.name = "Kettle Handle"; handle.transform.SetParent(kettle, false); Object.DestroyImmediate(handle.GetComponent<Collider>()); }
                handle.transform.localPosition = new Vector3(0, 0.25f, 0); handle.transform.localScale = new Vector3(0.015f, 0.06f, 0.20f); handle.GetComponent<Renderer>().sharedMaterial = iron;

                var spout = kettle.Find("Kettle Spout")?.gameObject;
                if (!spout) { spout = GameObject.CreatePrimitive(PrimitiveType.Cylinder); spout.name = "Kettle Spout"; spout.transform.SetParent(kettle, false); Object.DestroyImmediate(spout.GetComponent<Collider>()); }
                spout.transform.localPosition = new Vector3(0.14f, 0.14f, 0); spout.transform.localRotation = Quaternion.Euler(0, 0, -45); spout.transform.localScale = new Vector3(0.05f, 0.10f, 0.05f); spout.GetComponent<Renderer>().sharedMaterial = copperKettle;

                var kSteam = kettle.Find("Kettle Steam")?.gameObject;
                if (!kSteam) { kSteam = new GameObject("Kettle Steam"); kSteam.transform.SetParent(spout.transform, false); kSteam.transform.localPosition = new Vector3(0, 0.5f, 0); kSteam.AddComponent<SteamEmitter>(); }
            }
        }

        static void RepairTorch()
        {
            var model = Find("Flashlight_1940s_Ribbed_Mesh"); if (!model) return;
            var camera = model.GetComponentInParent<Camera>(); if (!camera) return;
            var rig = Find("FPV_Flashlight_Hands"); var arm = Find("Arm_R_Root");
            if (arm) { arm.localPosition = new(.20f, -.20f, .44f); arm.localRotation = Quaternion.Euler(2, -2, 0); }
            if (rig) rig.localPosition += settings.torchAnchor - camera.transform.InverseTransformPoint(model.position);
            model.localScale = new(1, 1, .22f / .1965f);
            var lens = model.Find("Front_Glass_Lens");
            var lights = model.parent.GetComponentsInChildren<Light>();
            var socket = Find("Flashlight_Lens_Socket");
            var spot = lights.FirstOrDefault(l => l.type == LightType.Spot && socket && l.transform.IsChildOf(socket));
            var fill = lights.FirstOrDefault(l => l.type == LightType.Point && socket && l.transform.IsChildOf(socket));
            foreach (var light in lights) if (light != spot && light != fill) Object.DestroyImmediate(light.gameObject);
            if (socket)
            {
                socket.localPosition = new Vector3(0f, 0f, 0.075f);
                socket.localRotation = Quaternion.identity;
            }
            if (spot)
            {
                spot.transform.localPosition = Vector3.zero;
                spot.transform.localRotation = Quaternion.identity;
                spot.shadowNearPlane = 0.05f;
                spot.renderingLayerMask = 1;
            }
            if (fill)
            {
                fill.transform.localPosition = Vector3.zero;
                fill.transform.localRotation = Quaternion.identity;
                fill.intensity = 0f;
                fill.enabled = false;
                fill.renderingLayerMask = 1;
            }
            int vmLayer = LayerMask.NameToLayer("Viewmodel");
            if (vmLayer < 0) vmLayer = 2;
            if (spot) spot.cullingMask &= ~(1 << vmLayer);
            if (fill) fill.cullingMask &= ~(1 << vmLayer);
            var old = model.parent.GetComponent<FlashlightController>();
            if (old)
            {
                var so = new SerializedObject(old); so.FindProperty("spotLight").objectReferenceValue = spot; so.FindProperty("fillLight").objectReferenceValue = fill; so.ApplyModifiedPropertiesWithoutUndo();
            }
            var aim = model.parent.GetComponent<FlashlightLensAim>(); if (!aim) aim = model.parent.gameObject.AddComponent<FlashlightLensAim>(); aim.Configure(camera.transform, lens, spot);
        }
    }
}
