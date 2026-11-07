using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace TheLightHouse.Editor
{
    public static class InteriorLayoutBuilder
    {
        public const string TowerInteriorRootName = "Tower Interior Modular";
        public const string CellarRootName = "Cellar Complex Modular";

        [MenuItem("TheLightHouse/Rebuild Complete Interior & Cellar (All Acts)")]
        public static void RebuildAllActs()
        {
            var activeSetup = EditorSceneManager.GetSceneManagerSetup();
            AssetDatabase.StartAssetEditing();
            try
            {
                for (int act = 1; act <= 5; act++)
                {
                    string scenePath = $"Assets/_Project/Scenes/Act{act}.unity";
                    var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                    RebuildScene(scene, $"Act{act}");
                    EditorSceneManager.SaveScene(scene);
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                if (activeSetup.Length > 0)
                {
                    EditorSceneManager.RestoreSceneManagerSetup(activeSetup);
                }
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[InteriorLayoutBuilder] Complete Interior & Cellar rebuilt across all Act scenes!");
        }

        public static void RebuildCurrentScene()
        {
            var scene = EditorSceneManager.GetActiveScene();
            RebuildScene(scene, scene.name);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log($"[InteriorLayoutBuilder] Rebuilt complete interior & cellar in {scene.name}.");
        }

        public static void RebuildScene(Scene scene, string actName)
        {
            var settings = InteriorKitBuilder.EnsureSettings();
            InteriorKitBuilder.BuildMaterials(settings);
            InteriorKitBuilder.BuildPrefabs(settings);

            // A0: Delete generated tower interior, stairs, mezzanines, cellar and hidden room
            CleanLegacyInteriorAndCellar();

            // Part A2: Build Tower Interior inside tapered shell
            var towerRoot = BuildTowerInterior(settings, actName);

            // Part A4: Build Cellar and Hidden Room
            var cellarRoot = BuildCellarAndHiddenRoom(settings, actName);

            // Part A3: Hiding spots, Logbook pages, CharacterController & NavMesh bake
            SetupHidingSpots(towerRoot, cellarRoot);
            SetupLogbookPages(actName);
            SetupNavMeshLinks(towerRoot, cellarRoot);

            // Set static flags on all interior geometry BEFORE baking NavMesh
            SetStaticGeometryFlags(towerRoot);
            SetStaticGeometryFlags(cellarRoot);

            ConfigurePlayerAndNavMesh();

            EditorUtility.SetDirty(towerRoot);
            EditorUtility.SetDirty(cellarRoot);
        }

        public static void CleanLegacyInteriorAndCellar()
        {
            // 1. Delete previous modular roots if present
            var prevTower = GameObject.Find(TowerInteriorRootName);
            if (prevTower) Object.DestroyImmediate(prevTower);

            var prevCellar = GameObject.Find(CellarRootName);
            if (prevCellar) Object.DestroyImmediate(prevCellar);

            // 2. Remove legacy generated stairs, intermediate rooms, and study dressing from other passes
            string[] legacyGroupPaths = new[]
            {
                "Reference Visuals/Lighthouse/GroundFloor",
                "Reference Visuals/Lighthouse/FirstFloor",
                "Reference Visuals/Lighthouse/SecondFloor",
                "Reference Visuals/Lighthouse/UpperTower",
                "Reference Visuals/Lighthouse/LanternFloor",
                "Harrow Rock Master Environment/Tower Interior & Circular Rooms Master",
                "Harrow Rock Master Environment/Underground Cellar Vault Complex",
                "Harrow Rock Master Environment/Underground Cellar & Secret Room Master",
                "Lighthouse Layout/Stairs",
                "Lighthouse Layout/Room Dividers/Cellar Floor",
                "Lighthouse Layout/Room Dividers/Hidden Room Floor",
                "Lighthouse Layout/Room Dividers/Flooded Corridor",
                "Lighthouse Layout/Room Dividers/Drain"
            };

            foreach (var path in legacyGroupPaths)
            {
                var go = GameObject.Find(path);
                if (go) Object.DestroyImmediate(go);
            }

            // Remove obsolete duplicate hiding spots that intersect tower stairs or cellar
            string[] obsoleteHidingSpots = new[] { "library-wardrobe", "level2-desk", "upper-locker", "crane-bed", "drain-alcove" };
            foreach (var name in obsoleteHidingSpots)
            {
                var spot = GameObject.Find(name);
                if (spot != null) Object.DestroyImmediate(spot);
            }

            // Also remove legacy floor slabs and internal stairs directly in Lighthouse Layout/Tower
            var towerLayout = GameObject.Find("Lighthouse Layout/Tower");
            if (towerLayout)
            {
                var toDelete = new List<GameObject>();
                foreach (Transform child in towerLayout.transform)
                {
                    string n = child.name;
                    if (n.StartsWith("Tower Floor") || n.StartsWith("Tower Cornice") || n.StartsWith("Spiral") || n.StartsWith("Lantern Ring") || n.StartsWith("Gallery Floor") || n == "Lantern Cap")
                    {
                        toDelete.Add(child.gameObject);
                    }
                }
                foreach (var go in toDelete) Object.DestroyImmediate(go);
            }

            // Remove legacy cellar arch walls
            var dividers = GameObject.Find("Lighthouse Layout/Room Dividers");
            if (dividers)
            {
                var toDelete = new List<GameObject>();
                foreach (Transform child in dividers.transform)
                {
                    if (child.name.StartsWith("Cellar Arch") || child.name.StartsWith("Cellar floor") || child.name.StartsWith("Hidden room"))
                    {
                        toDelete.Add(child.gameObject);
                    }
                }
                foreach (var go in toDelete) Object.DestroyImmediate(go);
            }
        }

        private static GameObject BuildTowerInterior(InteriorSettings settings, string actName)
        {
            var root = new GameObject(TowerInteriorRootName);
            float h = settings.floorHeight; // 3.2m
            float coreD = settings.centralCoreDiameter; // 3.0m
            float coreR = coreD * 0.5f; // 1.5m

            // 1. Central Round Masonry Core (diameter 3.0m) from y = 0 to y = 19.2m
            float totalH = (settings.stairOnlyLevels + 3) * h; // 6 levels * 3.2m = 19.2m
            var core = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            core.name = "Central_Core_3m";
            core.transform.SetParent(root.transform, false);
            core.transform.localPosition = new Vector3(0f, totalH * 0.5f, 0f);
            core.transform.localScale = new Vector3(coreD, totalH * 0.5f, coreD);
            core.GetComponent<Renderer>().sharedMaterial = settings.stoneWallMaterial;

            // 2. Build Levels: Floor 1 (y=0), Floor 2 (y=3.2), Floor 3 (y=6.4), plus stair-only levels up to lantern room
            // Total levels = 1 (Vestibule) + 1 (Crane's Room) + 1 (Watchroom) + settings.stairOnlyLevels (3) = 6 levels
            int totalLevels = 3 + settings.stairOnlyLevels; // 6 levels, top deck at y = 19.2m

            for (int lvl = 0; lvl < totalLevels; lvl++)
            {
                float floorY = lvl * h;
                var levelGroup = new GameObject($"Tower_Floor_{lvl + 1}").transform;
                levelGroup.SetParent(root.transform, false);
                levelGroup.localPosition = new Vector3(0f, floorY, 0f);

                // Build Floor Slab with Stairwell Opening (including opening for cellar descent stair on Ground Floor)
                if (lvl == 0)
                {
                    var floorGroup = new GameObject("Vestibule_Floor").transform;
                    floorGroup.SetParent(levelGroup, false);

                    // 1. East half (covers entire eastern semicircle where front door & flight 1 base are)
                    var east = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    east.name = "Floor_EastHalf";
                    east.transform.SetParent(floorGroup, false);
                    east.transform.localPosition = new Vector3(2.25f, -0.1f, 0f);
                    east.transform.localScale = new Vector3(4.5f, 0.2f, 9.0f);
                    east.GetComponent<Renderer>().sharedMaterial = settings.flagstoneMaterial;

                    // 2. South-West quarter (covers area south of cellar door, z in [-4.5, -1.8])
                    var sw = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    sw.name = "Floor_SouthWest";
                    sw.transform.SetParent(floorGroup, false);
                    sw.transform.localPosition = new Vector3(-2.25f, -0.1f, -3.15f);
                    sw.transform.localScale = new Vector3(4.5f, 0.2f, 2.7f);
                    sw.GetComponent<Renderer>().sharedMaterial = settings.flagstoneMaterial;

                    // 3. North-West quarter (covers area north of cellar stair well, z in [2.5, 4.5] where clearance > 2.8m)
                    var nw = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    nw.name = "Floor_NorthWest";
                    nw.transform.SetParent(floorGroup, false);
                    nw.transform.localPosition = new Vector3(-2.25f, -0.1f, 3.5f);
                    nw.transform.localScale = new Vector3(4.5f, 0.2f, 2.0f);
                    nw.GetComponent<Renderer>().sharedMaterial = settings.flagstoneMaterial;

                    // 4. West outer rim (covers outer edge x in [-4.5, -2.9], z in [-1.8, 2.5])
                    var wOuter = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    wOuter.name = "Floor_WestOuter";
                    wOuter.transform.SetParent(floorGroup, false);
                    wOuter.transform.localPosition = new Vector3(-3.7f, -0.1f, 0.35f);
                    wOuter.transform.localScale = new Vector3(1.6f, 0.2f, 4.3f);
                    wOuter.GetComponent<Renderer>().sharedMaterial = settings.flagstoneMaterial;
                }
                else
                {
                    // Modular Floor Slab with open stairwell slot
                    BuildFloorRingWithStairSlot(levelGroup, floorY, lvl, settings);
                }

                // Straight switchback flights wrapping around the core:
                // Alternating: Even floors (0, 2, 4...) run East flight (climbing South to North)
                // Odd floors (1, 3, 5...) run West flight (climbing North to South)
                bool isEastFlight = (lvl % 2 == 0);
                float stairX = isEastFlight ? 2.15f : -2.15f;
                float flightAngle = isEastFlight ? 0f : 180f; // 0 deg = South to North; 180 deg = North to South

                var flight = (GameObject)PrefabUtility.InstantiatePrefab(settings.stairFlightPrefab, levelGroup);
                flight.name = $"Flight_L{lvl + 1}_to_L{lvl + 2}";
                // Position start of flight
                if (isEastFlight)
                {
                    flight.transform.localPosition = new Vector3(stairX, 0f, -2.52f);
                    flight.transform.localRotation = Quaternion.identity;
                }
                else
                {
                    flight.transform.localPosition = new Vector3(stairX, 0f, 2.52f);
                    flight.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                }

                // Landing at top of this flight (1.6m x 1.6m at elevation y = floorY + h)
                var landing = (GameObject)PrefabUtility.InstantiatePrefab(settings.landingPrefab, levelGroup);
                landing.name = $"Landing_L{lvl + 1}_Top";
                landing.transform.localPosition = isEastFlight ? new Vector3(stairX, h, 3.32f) : new Vector3(stairX, h, -3.32f);

                // Base landing for next flight if another flight follows
                if (lvl + 1 < totalLevels)
                {
                    var baseLanding = (GameObject)PrefabUtility.InstantiatePrefab(settings.landingPrefab, levelGroup);
                    baseLanding.name = $"Landing_L{lvl + 2}_Base";
                    baseLanding.transform.localPosition = isEastFlight ? new Vector3(-stairX, h, 3.32f) : new Vector3(-stairX, h, -3.32f);
                }

                // Handrails along flight
                var handrail = (GameObject)PrefabUtility.InstantiatePrefab(settings.handrailPrefab, levelGroup);
                handrail.name = $"Handrail_L{lvl + 1}";
                float railOffset = isEastFlight ? -0.6f : 0.6f; // Inner edge towards core
                handrail.transform.localPosition = flight.transform.localPosition + new Vector3(railOffset, 0f, 0f);
                handrail.transform.localRotation = flight.transform.localRotation;

                // Specific room contents per floor
                if (lvl == 0)
                {
                    // Floor 1: Vestibule
                    BuildFloor1Vestibule(levelGroup, settings);
                }
                else if (lvl == 1)
                {
                    // Floor 2: Crane's Room
                    BuildFloor2CraneRoom(levelGroup, settings, floorY);
                }
                else if (lvl == 2)
                {
                    // Floor 3: Service Watchroom with dead generator prop
                    BuildFloor3Watchroom(levelGroup, settings, floorY);
                }
                else
                {
                    // Stair-only levels: Slit windows looking out
                    BuildStairOnlyLevel(levelGroup, settings, lvl, floorY);
                }
            }

            // Top: Lantern Room at y = totalLevels * h (19.2m)
            BuildLanternRoom(root.transform, settings, totalLevels * h, actName);

            return root;
        }

        private static void BuildFloorRingWithStairSlot(Transform parent, float floorY, int lvl, InteriorSettings settings)
        {
            float thick = 0.25f;

            // North cross corridor connecting East and West landings at z = 3.32m
            var north = GameObject.CreatePrimitive(PrimitiveType.Cube);
            north.name = "FloorSlab_North";
            north.transform.SetParent(parent, false);
            north.transform.localPosition = new Vector3(0f, -thick * 0.5f, 3.32f);
            north.transform.localScale = new Vector3(3.0f, thick, 1.6f);
            north.GetComponent<Renderer>().sharedMaterial = settings.woodMaterial;

            // South cross corridor connecting East and West landings at z = -3.32m
            var south = GameObject.CreatePrimitive(PrimitiveType.Cube);
            south.name = "FloorSlab_South";
            south.transform.SetParent(parent, false);
            south.transform.localPosition = new Vector3(0f, -thick * 0.5f, -3.32f);
            south.transform.localScale = new Vector3(3.0f, thick, 1.6f);
            south.GetComponent<Renderer>().sharedMaterial = settings.woodMaterial;
        }

        private static void BuildFloor1Vestibule(Transform parent, InteriorSettings settings)
        {
            var vestGroup = new GameObject("Vestibule_Dressing").transform;
            vestGroup.SetParent(parent, false);

            // Front door frame and door leading from courtyard / keeper's house
            var frontDoor = GameObject.Find("Tower Door");
            if (!frontDoor)
            {
                frontDoor = (GameObject)PrefabUtility.InstantiatePrefab(settings.doorFramePrefab, vestGroup);
                frontDoor.name = "Tower Door";
            }
            frontDoor.transform.SetParent(vestGroup, false);
            frontDoor.transform.localPosition = new Vector3(0f, 0f, -4.15f);
            frontDoor.transform.localRotation = Quaternion.identity;

            // Ensure Door component exists directly on Tower Door for external test queries
            var frontDoorDirect = frontDoor.GetComponent<Door>() ?? frontDoor.AddComponent<Door>();
            var directSo = new SerializedObject(frontDoorDirect);
            directSo.FindProperty("doorId").stringValue = "tower-front-door";
            directSo.FindProperty("locked").boolValue = false;
            directSo.FindProperty("openAngle").floatValue = 90f;
            directSo.ApplyModifiedPropertiesWithoutUndo();

            var frontDoorComp = frontDoor.GetComponentInChildren<Door>();
            if (frontDoorComp && frontDoorComp != frontDoorDirect)
            {
                var so = new SerializedObject(frontDoorComp);
                so.FindProperty("doorId").stringValue = "tower-front-door";
                so.FindProperty("locked").boolValue = false;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            // Locked Arched Cellar Door in Vestibule
            var cellarDoorGo = GameObject.Find("Locked Cellar Door");
            if (!cellarDoorGo)
            {
                cellarDoorGo = (GameObject)PrefabUtility.InstantiatePrefab(settings.doorFramePrefab, vestGroup);
                cellarDoorGo.name = "Locked Cellar Door";
            }
            cellarDoorGo.transform.SetParent(vestGroup, false);
            cellarDoorGo.transform.localPosition = new Vector3(-2.2f, 0f, -1.8f);
            cellarDoorGo.transform.localRotation = Quaternion.identity;

            var cellarDirect = cellarDoorGo.GetComponent<Door>() ?? cellarDoorGo.AddComponent<Door>();
            var cDirectSo = new SerializedObject(cellarDirect);
            cDirectSo.FindProperty("doorId").stringValue = "cellar-door";
            cDirectSo.FindProperty("locked").boolValue = true;
            cDirectSo.ApplyModifiedPropertiesWithoutUndo();

            var cellarDoor = cellarDoorGo.GetComponentInChildren<Door>();
            if (cellarDoor && cellarDoor != cellarDirect)
            {
                var so = new SerializedObject(cellarDoor);
                so.FindProperty("doorId").stringValue = "cellar-door";
                so.FindProperty("locked").boolValue = true;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            // Arched stone reveal above cellar door
            var cellarArch = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cellarArch.name = "CellarDoorArch";
            cellarArch.transform.SetParent(cellarDoorGo.transform, false);
            cellarArch.transform.localPosition = new Vector3(0f, 2.45f, 0f);
            cellarArch.transform.localScale = new Vector3(1.35f, 0.28f, 0.42f);
            cellarArch.GetComponent<Renderer>().sharedMaterial = settings.stoneWallMaterial;

            // Warm caged lantern on wall (#FFA64D)
            CreateCagedLantern("Vestibule_Lantern", vestGroup, new Vector3(0.8f, 2.2f, -3.9f));

            // Boot bench and coat rack
            var bench = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bench.name = "Vestibule_Bench";
            bench.transform.SetParent(vestGroup, false);
            bench.transform.localPosition = new Vector3(-1.8f, 0.22f, -3.5f);
            bench.transform.localScale = new Vector3(1.4f, 0.44f, 0.45f);
            bench.GetComponent<Renderer>().sharedMaterial = settings.woodMaterial;
        }

        private static void BuildFloor2CraneRoom(Transform parent, InteriorSettings settings, float floorY)
        {
            var roomGroup = new GameObject("Crane_Quarters").transform;
            roomGroup.SetParent(parent, false);

            // Room Floor Slab (West quadrant, outside the stair)
            var roomFloor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roomFloor.name = "CraneRoom_Floor";
            roomFloor.transform.SetParent(roomGroup, false);
            roomFloor.transform.localPosition = new Vector3(-3.5f, -0.125f, 2.0f);
            roomFloor.transform.localScale = new Vector3(1.8f, 0.25f, 3.2f);
            roomFloor.GetComponent<Renderer>().sharedMaterial = settings.woodMaterial;

            // Partition wall separating corridor from Crane's room
            var wall = (GameObject)PrefabUtility.InstantiatePrefab(settings.wallPrefab, roomGroup);
            wall.name = "CraneRoom_Wall";
            wall.transform.localPosition = new Vector3(-2.85f, 0f, 1.8f);
            wall.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);

            // LockedRoomDoor into Crane's Room (at North landing: x = -2.85, z = 3.32)
            var craneDoorGo = GameObject.Find("Crane's Room Door");
            if (!craneDoorGo)
            {
                craneDoorGo = (GameObject)PrefabUtility.InstantiatePrefab(settings.doorFramePrefab, roomGroup);
                craneDoorGo.name = "Crane's Room Door";
            }
            craneDoorGo.transform.SetParent(roomGroup, false);
            craneDoorGo.transform.localPosition = new Vector3(-2.85f, 0f, 3.32f);
            craneDoorGo.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);

            // Ensure LockedRoomDoor component
            var lockedComp = craneDoorGo.GetComponent<LockedRoomDoor>() ?? craneDoorGo.AddComponent<LockedRoomDoor>();
            var player = Object.FindAnyObjectByType<FirstPersonController>();
            if (player)
            {
                var inv = player.GetComponent<Inventory>();
                var so = new SerializedObject(lockedComp);
                so.FindProperty("inventory").objectReferenceValue = inv;
                so.FindProperty("keyId").stringValue = "CraneRoomKey";
                so.FindProperty("doorId").stringValue = "crane-room-door";
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            // Crane's Desk
            var desk = GameObject.Find("Crane's Desk");
            if (!desk)
            {
                desk = GameObject.CreatePrimitive(PrimitiveType.Cube);
                desk.name = "Crane's Desk";
            }
            desk.transform.SetParent(roomGroup, false);
            desk.transform.localPosition = new Vector3(-3.6f, 0.4f, 2.0f);
            desk.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
            desk.transform.localScale = new Vector3(1.6f, 0.78f, 0.85f);
            desk.GetComponent<Renderer>().sharedMaterial = settings.woodMaterial;

            // Desk Chair
            var chair = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chair.name = "DeskChair";
            chair.transform.SetParent(roomGroup, false);
            chair.transform.localPosition = new Vector3(-3.0f, 0.45f, 2.0f);
            chair.transform.localScale = new Vector3(0.55f, 0.9f, 0.55f);
            chair.GetComponent<Renderer>().sharedMaterial = settings.woodMaterial;

            // Bookshelves with books along back wall
            var shelf = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shelf.name = "Bookshelf";
            shelf.transform.SetParent(roomGroup, false);
            shelf.transform.localPosition = new Vector3(-3.8f, 1.2f, 1.0f);
            shelf.transform.localScale = new Vector3(0.45f, 2.2f, 1.6f);
            shelf.GetComponent<Renderer>().sharedMaterial = settings.woodMaterial;

            // Non-interactive radio prop on side table
            var radioProp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            radioProp.name = "Crane_Radio_Prop";
            radioProp.transform.SetParent(roomGroup, false);
            radioProp.transform.localPosition = new Vector3(-3.5f, 0.95f, 3.0f);
            radioProp.transform.localScale = new Vector3(0.6f, 0.45f, 0.35f);
            radioProp.GetComponent<Renderer>().sharedMaterial = settings.rustMetalMaterial;

            // Family Photo with inspection pose
            var photoGo = GameObject.Find("Family Photo");
            if (!photoGo)
            {
                photoGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                photoGo.name = "Family Photo";
            }
            photoGo.transform.SetParent(roomGroup, false);
            photoGo.transform.localPosition = new Vector3(-3.6f, 1.25f, 2.35f);
            photoGo.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
            photoGo.transform.localScale = new Vector3(0.35f, 0.45f, 0.04f);
            photoGo.GetComponent<Renderer>().sharedMaterial = settings.paperMaterial;

            var photoScript = photoGo.GetComponent<FamilyPhoto>() ?? photoGo.AddComponent<FamilyPhoto>();
            var cam = player ? player.GetComponentInChildren<Camera>() : Camera.main;
            var inspectPose = photoGo.transform.Find("InspectionPose");
            if (!inspectPose)
            {
                var p = new GameObject("InspectionPose");
                p.transform.SetParent(photoGo.transform, false);
                p.transform.localPosition = new Vector3(0f, 0f, -0.65f);
                inspectPose = p.transform;
            }
            var pSo = new SerializedObject(photoScript);
            pSo.FindProperty("view").objectReferenceValue = cam;
            pSo.FindProperty("player").objectReferenceValue = player;
            pSo.FindProperty("inspectionPose").objectReferenceValue = inspectPose;
            pSo.ApplyModifiedPropertiesWithoutUndo();

            // Caged warm lamp over desk
            CreateCagedLantern("Crane_Desk_Lamp", roomGroup, new Vector3(-3.6f, 1.6f, 2.0f));

            // Slit window in Crane's room
            var win = (GameObject)PrefabUtility.InstantiatePrefab(settings.windowFramePrefab, roomGroup);
            win.name = "CraneRoom_Window";
            win.transform.localPosition = new Vector3(-4.2f, 1.4f, 2.0f);
            win.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
        }

        private static void BuildFloor3Watchroom(Transform parent, InteriorSettings settings, float floorY)
        {
            var watchGroup = new GameObject("Service_Watchroom").transform;
            watchGroup.SetParent(parent, false);

            // Floor slab inside watchroom (West quadrant, outside stair flight x < -2.9m)
            var watchFloor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            watchFloor.name = "Watchroom_Floor";
            watchFloor.transform.SetParent(watchGroup, false);
            watchFloor.transform.localPosition = new Vector3(-3.65f, -0.125f, 0f);
            watchFloor.transform.localScale = new Vector3(1.3f, 0.25f, 3.6f);
            watchFloor.GetComponent<Renderer>().sharedMaterial = settings.woodMaterial;

            // Non-interactive dead generator prop (large cast iron housing, flywheel, rusted exhaust pipe)
            var genHousing = GameObject.CreatePrimitive(PrimitiveType.Cube);
            genHousing.name = "Dead_Generator_Housing_Prop";
            genHousing.transform.SetParent(watchGroup, false);
            genHousing.transform.localPosition = new Vector3(-3.65f, 0.65f, 0f);
            genHousing.transform.localScale = new Vector3(1.0f, 1.3f, 1.4f);
            genHousing.GetComponent<Renderer>().sharedMaterial = settings.rustMetalMaterial;

            // Flywheel facing along Z on front of generator
            var flywheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            flywheel.name = "Generator_Flywheel";
            flywheel.transform.SetParent(genHousing.transform, false);
            flywheel.transform.localPosition = new Vector3(0f, 0f, 0.75f);
            flywheel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            flywheel.transform.localScale = new Vector3(0.95f, 0.12f, 0.95f);
            flywheel.GetComponent<Renderer>().sharedMaterial = settings.rustMetalMaterial;
            var fc = flywheel.GetComponent<Collider>();
            if (fc) Object.DestroyImmediate(fc);

            // Exhaust pipe heading upward into core
            var pipe = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pipe.name = "ExhaustPipe";
            pipe.transform.SetParent(genHousing.transform, false);
            pipe.transform.localPosition = new Vector3(0f, 0.85f, -0.3f);
            pipe.transform.localScale = new Vector3(0.18f, 0.85f, 0.18f);
            pipe.GetComponent<Renderer>().sharedMaterial = settings.rustMetalMaterial;
            var pipCol = pipe.GetComponent<Collider>();
            if (pipCol) Object.DestroyImmediate(pipCol);

            // Workbench with oil drums
            var bench = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bench.name = "Watchroom_Workbench";
            bench.transform.SetParent(watchGroup, false);
            bench.transform.localPosition = new Vector3(-3.65f, 0.45f, 1.8f);
            bench.transform.localScale = new Vector3(1.0f, 0.85f, 1.4f);
            bench.GetComponent<Renderer>().sharedMaterial = settings.woodMaterial;

            // Oil drum
            var drum = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            drum.name = "OilDrum";
            drum.transform.SetParent(watchGroup, false);
            drum.transform.localPosition = new Vector3(-3.4f, 0.5f, -1.5f);
            drum.transform.localScale = new Vector3(0.55f, 0.5f, 0.55f);
            drum.GetComponent<Renderer>().sharedMaterial = settings.rustMetalMaterial;

            // Caged warm lantern
            CreateCagedLantern("Watchroom_Lantern", watchGroup, new Vector3(-3.4f, 2.2f, 0f));

            // Slit window
            var win = (GameObject)PrefabUtility.InstantiatePrefab(settings.windowFramePrefab, watchGroup);
            win.name = "Watchroom_Window";
            win.transform.localPosition = new Vector3(-4.2f, 1.4f, 0f);
            win.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
        }

        private static void BuildStairOnlyLevel(Transform parent, InteriorSettings settings, int lvl, float floorY)
        {
            var levelGroup = new GameObject($"StairLevel_{lvl + 1}_Dressing").transform;
            levelGroup.SetParent(parent, false);

            // Slit window facing seaward / courtyard
            float winAngle = (lvl % 2 == 0) ? 90f : -90f;
            Vector3 winPos = (lvl % 2 == 0) ? new Vector3(3.8f, 1.5f, 0f) : new Vector3(-3.8f, 1.5f, 0f);

            var win = (GameObject)PrefabUtility.InstantiatePrefab(settings.windowFramePrefab, levelGroup);
            win.name = $"SlitWindow_L{lvl + 1}";
            win.transform.localPosition = winPos;
            win.transform.localRotation = Quaternion.Euler(0f, winAngle, 0f);

            // Wall lantern on landing
            CreateCagedLantern($"Stair_Lantern_L{lvl + 1}", levelGroup, new Vector3(0f, 2.2f, (lvl % 2 == 0 ? 2.5f : -2.5f)));
        }

        private static void BuildLanternRoom(Transform parent, InteriorSettings settings, float deckY, string actName)
        {
            var lanternGroup = new GameObject("Lantern_Room_Chamber").transform;
            lanternGroup.SetParent(parent, false);
            lanternGroup.localPosition = new Vector3(0f, deckY, 0f);

            // 1. Lantern Room Floor Deck with open stairwell slot for top flight arrival (A1/A2 requirement)
            var deckGroup = new GameObject("Lantern_Deck_Floor").transform;
            deckGroup.SetParent(lanternGroup, false);

            // East half (covers eastern semicircular deck)
            var eastDeck = GameObject.CreatePrimitive(PrimitiveType.Cube);
            eastDeck.name = "Deck_East";
            eastDeck.transform.SetParent(deckGroup, false);
            eastDeck.transform.localPosition = new Vector3(1.55f, -0.125f, 0f);
            eastDeck.transform.localScale = new Vector3(3.1f, 0.25f, 6.2f);
            eastDeck.GetComponent<Renderer>().sharedMaterial = settings.flagstoneMaterial;

            // Inner rim (between central pedestal and stairwell opening)
            var innerRim = GameObject.CreatePrimitive(PrimitiveType.Cube);
            innerRim.name = "Deck_InnerRim";
            innerRim.transform.SetParent(deckGroup, false);
            innerRim.transform.localPosition = new Vector3(-0.75f, -0.125f, 0f);
            innerRim.transform.localScale = new Vector3(1.5f, 0.25f, 5.1f);
            innerRim.GetComponent<Renderer>().sharedMaterial = settings.flagstoneMaterial;

            // West outer rim (outside the stairwell opening, leaving x in [-3.0, -1.5], z in [-2.6, 2.6] completely open!)
            var westOuter = GameObject.CreatePrimitive(PrimitiveType.Cube);
            westOuter.name = "Deck_WestOuterRim";
            westOuter.transform.SetParent(deckGroup, false);
            westOuter.transform.localPosition = new Vector3(-3.25f, -0.125f, 0f);
            westOuter.transform.localScale = new Vector3(0.5f, 0.25f, 6.2f);
            westOuter.GetComponent<Renderer>().sharedMaterial = settings.flagstoneMaterial;

            // North cross strip (north of stair flight treads)
            var northConnector = GameObject.CreatePrimitive(PrimitiveType.Cube);
            northConnector.name = "Deck_NorthConnector";
            northConnector.transform.SetParent(deckGroup, false);
            northConnector.transform.localPosition = new Vector3(-1.55f, -0.125f, 2.82f);
            northConnector.transform.localScale = new Vector3(3.1f, 0.25f, 0.56f);
            northConnector.GetComponent<Renderer>().sharedMaterial = settings.flagstoneMaterial;

            // South connector slab bridging Landing_L6_Top at z = -3.32 to the Lantern Deck
            var southConnector = GameObject.CreatePrimitive(PrimitiveType.Cube);
            southConnector.name = "Deck_SouthConnector";
            southConnector.transform.SetParent(deckGroup, false);
            southConnector.transform.localPosition = new Vector3(-1.0f, -0.125f, -3.32f);
            southConnector.transform.localScale = new Vector3(3.5f, 0.25f, 1.6f);
            southConnector.GetComponent<Renderer>().sharedMaterial = settings.flagstoneMaterial;

            // 2. Exterior Catwalk & Metal Railing (diameter 8.2m)
            var catwalk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            catwalk.name = "Exterior_Catwalk_Gallery";
            catwalk.transform.SetParent(lanternGroup, false);
            catwalk.transform.localPosition = new Vector3(0f, -0.15f, 0f);
            catwalk.transform.localScale = new Vector3(8.4f, 0.2f, 8.4f);
            catwalk.GetComponent<Renderer>().sharedMaterial = settings.rustMetalMaterial;
            var catCol = catwalk.GetComponent<Collider>();
            if (catCol) Object.DestroyImmediate(catCol);

            // Add 4 perimeter box colliders (strictly outside glass radius 2.85m, never intersecting stairwell)
            var catN = catwalk.AddComponent<BoxCollider>();
            catN.center = new Vector3(0f, 0f, 0.42f);
            catN.size = new Vector3(0.6f, 1.0f, 0.16f);
            var catS = catwalk.AddComponent<BoxCollider>();
            catS.center = new Vector3(0f, 0f, -0.42f);
            catS.size = new Vector3(0.6f, 1.0f, 0.16f);
            var catE = catwalk.AddComponent<BoxCollider>();
            catE.center = new Vector3(0.42f, 0f, 0f);
            catE.size = new Vector3(0.16f, 1.0f, 0.6f);
            var catW = catwalk.AddComponent<BoxCollider>();
            catW.center = new Vector3(-0.42f, 0f, 0f);
            catW.size = new Vector3(0.16f, 1.0f, 0.6f);

            // Railing ring around catwalk
            int railPosts = 16;
            float railRadius = 4.1f;
            for (int i = 0; i < railPosts; i++)
            {
                float ang = (i / (float)railPosts) * 360f;
                float rad = ang * Mathf.Deg2Rad;
                var post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                post.name = $"Catwalk_Post_{i:00}";
                post.transform.SetParent(catwalk.transform, false);
                post.transform.localPosition = new Vector3(Mathf.Sin(rad) * railRadius, 0.55f, Mathf.Cos(rad) * railRadius);
                post.transform.localScale = new Vector3(0.06f, 0.55f, 0.06f);
                post.GetComponent<Renderer>().sharedMaterial = settings.rustMetalMaterial;
                var pc = post.GetComponent<Collider>();
                if (pc) Object.DestroyImmediate(pc);
            }

            // 3. Glazed Chamber Enclosure (12 polygonal glass facets with iron astragals)
            int panes = 12;
            float glassR = 3.6f;
            float glassH = 2.8f;
            for (int i = 0; i < panes; i++)
            {
                float ang = (i / (float)panes) * 360f;
                float rad = ang * Mathf.Deg2Rad;
                var frame = (GameObject)PrefabUtility.InstantiatePrefab(settings.windowFramePrefab, lanternGroup);
                frame.name = $"Lantern_Glass_Pane_{i:00}";
                frame.transform.localPosition = new Vector3(Mathf.Sin(rad) * glassR, glassH * 0.5f, Mathf.Cos(rad) * glassR);
                frame.transform.localRotation = Quaternion.Euler(0f, ang + 90f, 0f);
                foreach (var c in frame.GetComponentsInChildren<Collider>())
                {
                    if (c.name.Contains("Sill") || c.name.Contains("Reveal")) Object.DestroyImmediate(c);
                }
            }

            // 4. Central Fresnel Lens Placeholder (Brass & tiered prismatic glass rings)
            var lensBase = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            lensBase.name = "Fresnel_Lens_Pedestal";
            lensBase.transform.SetParent(lanternGroup, false);
            lensBase.transform.localPosition = new Vector3(0f, 0.45f, 0f);
            lensBase.transform.localScale = new Vector3(1.2f, 0.45f, 1.2f);
            lensBase.GetComponent<Renderer>().sharedMaterial = settings.rustMetalMaterial;

            var lensGlass = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            lensGlass.name = "Fresnel_Lens_Prism_Core";
            lensGlass.transform.SetParent(lensBase.transform, false);
            lensGlass.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            lensGlass.transform.localScale = new Vector3(0.95f, 0.75f, 0.95f);
            lensGlass.GetComponent<Renderer>().sharedMaterial = settings.flagstoneMaterial;

            // 5. Existing LampController and LanternInteract anchors
            var lampGo = GameObject.Find("Lamp");
            if (!lampGo)
            {
                var lampComp = Object.FindAnyObjectByType<LampController>();
                if (lampComp) lampGo = lampComp.gameObject;
                else
                {
                    lampGo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    lampGo.name = "Lamp";
                }
            }
            lampGo.transform.SetParent(lanternGroup, false);
            lampGo.transform.localPosition = new Vector3(0f, 1.25f, 0f);
            lampGo.transform.localScale = new Vector3(1.0f, 1.2f, 1.0f);
            lampGo.GetComponent<Renderer>().sharedMaterial = settings.rustMetalMaterial;

            var lamp = lampGo.GetComponent<LampController>() ?? lampGo.AddComponent<LampController>();
            var lantern = lampGo.GetComponent<LanternInteract>() ?? lampGo.AddComponent<LanternInteract>();
            var inventory = Object.FindAnyObjectByType<Inventory>();

            // Setup Lamp light if missing
            var lightTrans = lampGo.transform.Find("Lamp Light");
            Light lampLight = null;
            if (lightTrans) lampLight = lightTrans.GetComponent<Light>();
            else
            {
                var lGo = new GameObject("Lamp Light");
                lGo.transform.SetParent(lampGo.transform, false);
                lGo.transform.localPosition = Vector3.up * 1.5f;
                lampLight = lGo.AddComponent<Light>();
                lampLight.type = LightType.Point;
                lampLight.range = 25f;
                lampLight.intensity = 8f;
                lampLight.color = new Color(1.0f, 0.9f, 0.7f);
            }

            var stormSettings = AssetDatabase.LoadAssetAtPath<StormSettings>("Assets/_Project/Settings/StormSettings.asset");
            var actFourSettings = AssetDatabase.LoadAssetAtPath<ActFourSettings>("Assets/_Project/Settings/ActFourSettings.asset");
            var finaleSettings = AssetDatabase.LoadAssetAtPath<FinaleSettings>("Assets/_Project/Settings/FinaleSettings.asset");

            var lampSo = new SerializedObject(lamp);
            lampSo.FindProperty("lampLight").objectReferenceValue = lampLight;
            lampSo.FindProperty("stormSettings").objectReferenceValue = stormSettings;
            lampSo.FindProperty("actFourSettings").objectReferenceValue = actFourSettings;
            lampSo.FindProperty("finaleSettings").objectReferenceValue = finaleSettings;
            lampSo.ApplyModifiedPropertiesWithoutUndo();

            var lanternSo = new SerializedObject(lantern);
            lanternSo.FindProperty("lamp").objectReferenceValue = lamp;
            lanternSo.FindProperty("inventory").objectReferenceValue = inventory;
            lanternSo.ApplyModifiedPropertiesWithoutUndo();

            // 6. Existing Crank Station anchor
            var crankGo = GameObject.Find("Crank Station");
            if (!crankGo)
            {
                var crankComp = Object.FindAnyObjectByType<CrankStation>();
                if (crankComp) crankGo = crankComp.gameObject;
                else
                {
                    crankGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    crankGo.name = "Crank Station";
                }
            }
            crankGo.transform.SetParent(lanternGroup, false);
            crankGo.transform.localPosition = new Vector3(1.6f, 0.45f, 0f);
            crankGo.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
            crankGo.transform.localScale = new Vector3(0.8f, 1.0f, 0.8f);
            crankGo.GetComponent<Renderer>().sharedMaterial = settings.rustMetalMaterial;

            var noise = crankGo.GetComponent<NoiseEmitter>() ?? crankGo.AddComponent<NoiseEmitter>();
            var crank = crankGo.GetComponent<CrankStation>() ?? crankGo.AddComponent<CrankStation>();
            var crankSo = new SerializedObject(crank);
            crankSo.FindProperty("lamp").objectReferenceValue = lamp;
            crankSo.FindProperty("noiseEmitter").objectReferenceValue = noise;
            crankSo.FindProperty("stormSettings").objectReferenceValue = stormSettings;
            crankSo.ApplyModifiedPropertiesWithoutUndo();

            // 7. Roof Cap over Lantern
            var roofCap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            roofCap.name = "Lantern_Roof_Cap";
            roofCap.transform.SetParent(lanternGroup, false);
            roofCap.transform.localPosition = new Vector3(0f, glassH + 0.15f, 0f);
            roofCap.transform.localScale = new Vector3(6.2f, 0.15f, 6.2f);
            roofCap.GetComponent<Renderer>().sharedMaterial = settings.rustMetalMaterial;
            var capCol = roofCap.GetComponent<Collider>();
            if (capCol) Object.DestroyImmediate(capCol);
            roofCap.AddComponent<MeshCollider>();
        }

        private static GameObject BuildCellarAndHiddenRoom(InteriorSettings settings, string actName)
        {
            var root = new GameObject(CellarRootName);

            // Locked Cellar Door in Vestibule is at (-2.2, 0, -1.8)
            Vector3 stairTop = new Vector3(-2.2f, 0f, -1.6f);

            // Straight stair descending 5m: starts at stairTop and descends to y = -5.0m
            // Rise = 5.0m, Run = 7.5m, Width = 1.2m
            float descentH = settings.cellarDepth; // 5.0m
            float stairRun = 7.5f;
            int stepCount = 28;
            float stepRise = descentH / stepCount;
            float stepRun = stairRun / stepCount;

            var stairGroup = new GameObject("Cellar_Descent_Stair").transform;
            stairGroup.SetParent(root.transform, false);
            stairGroup.localPosition = stairTop;

            // Visual treads
            for (int i = 0; i < stepCount; i++)
            {
                float y = -(i + 0.5f) * stepRise;
                float z = (i + 0.5f) * stepRun;
                var step = GameObject.CreatePrimitive(PrimitiveType.Cube);
                step.name = $"CellarStep_{i:00}";
                step.transform.SetParent(stairGroup, false);
                step.transform.localPosition = new Vector3(0f, y, z);
                step.transform.localScale = new Vector3(settings.stairWidth, stepRise, stepRun);
                step.GetComponent<Renderer>().sharedMaterial = settings.stoneWallMaterial;
                var sc = step.GetComponent<Collider>();
                if (sc) Object.DestroyImmediate(sc);
            }

            // Continuous ramp collider descending at ~33 degrees
            float diagLen = Mathf.Sqrt(descentH * descentH + stairRun * stairRun);
            float angleDeg = Mathf.Atan2(descentH, stairRun) * Mathf.Rad2Deg; // 33.69 degrees (pitch down)
            var rampRot = Quaternion.Euler(angleDeg, 0f, 0f);
            var rampNormal = rampRot * Vector3.up;

            float boxThick = 0.20f;
            Vector3 rampOffset = -rampNormal * (boxThick * 0.5f);
            var rampObj = new GameObject("CellarRampCollider");
            rampObj.transform.SetParent(stairGroup, false);
            // Mathematically flush with ground floor at (0, 0, 0) and corridor floor at (0, -descentH, stairRun)
            rampObj.transform.localPosition = new Vector3(0f, -descentH * 0.5f, stairRun * 0.5f) + rampOffset;
            rampObj.transform.localRotation = rampRot;
            var rampCol = rampObj.AddComponent<BoxCollider>();
            rampCol.size = new Vector3(settings.stairWidth, boxThick, diagLen);

            // Base of stair at y = -5.0m, z = -1.8 + 7.5 = 5.7m
            Vector3 corridorStart = stairTop + new Vector3(0f, -descentH, stairRun);

            // Vaulted corridor: 2.8m wide, 2.6m high, ~20m long
            // 10 segments of 2.0m each
            int segmentCount = 10;
            float segLen = 2.0f;
            var corridorGroup = new GameObject("Vaulted_Corridor_20m").transform;
            corridorGroup.SetParent(root.transform, false);
            corridorGroup.localPosition = corridorStart;

            for (int i = 0; i < segmentCount; i++)
            {
                var seg = (GameObject)PrefabUtility.InstantiatePrefab(settings.vaultSegmentPrefab, corridorGroup);
                seg.name = $"VaultSegment_{i:00}";
                seg.transform.localPosition = new Vector3(0f, 0f, (i + 0.5f) * segLen);
                seg.transform.localRotation = Quaternion.identity;
            }

            // Props along corridor: conduit & cables, barrels, drums, crates
            BuildCorridorProps(corridorGroup, segmentCount, segLen, settings);

            // WetFootprints leading INTO the cellar, none leading out
            SetupWetFootprints(corridorGroup, segmentCount * segLen);

            // CellarHum audio zone
            SetupCellarHum(corridorGroup);

            // Hidden room at end of corridor (behind sliding barrel stack)
            // Corridor ends at z = segmentCount * segLen = 20.0m
            float roomHalfDepth = settings.hiddenRoomDimensions.y * 0.5f;
            Vector3 hiddenRoomCenter = corridorStart + new Vector3(0f, 0f, segmentCount * segLen + roomHalfDepth + 0.125f);
            BuildHiddenCellarRoom(root.transform, hiddenRoomCenter, settings, actName);

            return root;
        }

        private static void BuildCorridorProps(Transform parent, int count, float segLen, InteriorSettings settings)
        {
            var propsGroup = new GameObject("Corridor_Props").transform;
            propsGroup.SetParent(parent, false);

            // Conduit and electrical cable running high along left wall
            var conduit = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            conduit.name = "WallConduit";
            conduit.transform.SetParent(propsGroup, false);
            conduit.transform.localPosition = new Vector3(-1.32f, 1.85f, (count * segLen) * 0.5f);
            conduit.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            conduit.transform.localScale = new Vector3(0.05f, (count * segLen) * 0.5f, 0.05f);
            conduit.GetComponent<Renderer>().sharedMaterial = settings.rustMetalMaterial;
            var cc = conduit.GetComponent<Collider>();
            if (cc) Object.DestroyImmediate(cc);

            // Barrels, crates and drums scattered in corridor niches
            for (int i = 1; i < count - 1; i++)
            {
                float z = i * segLen;
                float side = (i % 2 == 0) ? 1.05f : -1.05f;

                if (i % 3 == 0)
                {
                    // Wooden crate
                    var crate = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    crate.name = $"Crate_{i}";
                    crate.transform.SetParent(propsGroup, false);
                    crate.transform.localPosition = new Vector3(side, 0.35f, z);
                    crate.transform.localRotation = Quaternion.Euler(0f, 15f * i, 0f);
                    crate.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
                    crate.GetComponent<Renderer>().sharedMaterial = settings.woodMaterial;
                }
                else
                {
                    // Metal oil drum / barrel
                    var drum = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    drum.name = $"Drum_{i}";
                    drum.transform.SetParent(propsGroup, false);
                    drum.transform.localPosition = new Vector3(side, 0.45f, z);
                    drum.transform.localScale = new Vector3(0.55f, 0.45f, 0.55f);
                    drum.GetComponent<Renderer>().sharedMaterial = (i % 2 == 0) ? settings.rustMetalMaterial : settings.woodMaterial;
                }

                // Caged lantern every 6m
                if (i % 3 == 1)
                {
                    CreateCagedLantern($"Corridor_Lantern_{i}", propsGroup, new Vector3(-side * 0.8f, 1.9f, z));
                }
            }
        }

        private static void SetupWetFootprints(Transform corridorGroup, float length)
        {
            var printsGo = GameObject.Find("Wet footprints (Act 2)");
            if (!printsGo)
            {
                printsGo = new GameObject("Wet footprints (Act 2)");
                printsGo.AddComponent<WetFootprints>();
            }
            printsGo.transform.SetParent(corridorGroup, false);
            printsGo.transform.localPosition = new Vector3(0.15f, 0.02f, 2.0f);
            printsGo.transform.localRotation = Quaternion.identity; // Facing down the corridor
            var wetComp = printsGo.GetComponent<WetFootprints>();
            if (wetComp)
            {
                var so = new SerializedObject(wetComp);
                so.FindProperty("count").intValue = 12;
                so.FindProperty("spacing").floatValue = 0.55f;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void SetupCellarHum(Transform corridorGroup)
        {
            var humGo = GameObject.Find("Cellar Hum");
            if (!humGo)
            {
                humGo = new GameObject("Cellar Hum");
                humGo.AddComponent<AudioSource>();
                humGo.AddComponent<CellarHum>();
            }
            humGo.transform.SetParent(corridorGroup, false);
            humGo.transform.localPosition = new Vector3(0f, 1.2f, 6.0f);
            var box = humGo.GetComponent<BoxCollider>();
            if (!box) box = humGo.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(3.2f, 2.8f, 12f);
        }

        private static void BuildHiddenCellarRoom(Transform parent, Vector3 center, InteriorSettings settings, string actName)
        {
            var hiddenRoot = new GameObject("Hidden_Room_Complex").transform;
            hiddenRoot.SetParent(parent, false);
            hiddenRoot.position = center;

            float w = settings.hiddenRoomDimensions.x; // 4.0m
            float d = settings.hiddenRoomDimensions.y; // 4.0m
            float h = 2.8f;
            float thick = 0.25f;

            // Floor & Ceiling
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "HiddenRoom_Floor";
            floor.transform.SetParent(hiddenRoot, false);
            floor.transform.localPosition = new Vector3(0f, -thick * 0.5f, 0f);
            floor.transform.localScale = new Vector3(w + thick * 2f, thick, d + thick * 2f);
            floor.GetComponent<Renderer>().sharedMaterial = settings.flagstoneMaterial;

            var ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ceiling.name = "HiddenRoom_Ceiling";
            ceiling.transform.SetParent(hiddenRoot, false);
            ceiling.transform.localPosition = new Vector3(0f, h + thick * 0.5f, 0f);
            ceiling.transform.localScale = new Vector3(w + thick * 2f, thick, d + thick * 2f);
            ceiling.GetComponent<Renderer>().sharedMaterial = settings.stoneWallMaterial;

            // Walls (North, East, West)
            // North Wall
            var northWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            northWall.name = "HiddenRoom_WallNorth";
            northWall.transform.SetParent(hiddenRoot, false);
            northWall.transform.localPosition = new Vector3(0f, h * 0.5f, d * 0.5f + thick * 0.5f);
            northWall.transform.localScale = new Vector3(w + thick * 2f, h, thick);
            northWall.GetComponent<Renderer>().sharedMaterial = settings.stoneWallMaterial;

            // East Wall
            var eastWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            eastWall.name = "HiddenRoom_WallEast";
            eastWall.transform.SetParent(hiddenRoot, false);
            eastWall.transform.localPosition = new Vector3(w * 0.5f + thick * 0.5f, h * 0.5f, 0f);
            eastWall.transform.localScale = new Vector3(thick, h, d);
            eastWall.GetComponent<Renderer>().sharedMaterial = settings.stoneWallMaterial;

            // West Wall
            var westWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            westWall.name = "HiddenRoom_WallWest";
            westWall.transform.SetParent(hiddenRoot, false);
            westWall.transform.localPosition = new Vector3(-w * 0.5f - thick * 0.5f, h * 0.5f, 0f);
            westWall.transform.localScale = new Vector3(thick, h, d);
            westWall.GetComponent<Renderer>().sharedMaterial = settings.stoneWallMaterial;

            // South Partition Wall with opening for the secret entrance
            // Left jamb
            var southWallLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
            southWallLeft.name = "HiddenRoom_WallSouth_Left";
            southWallLeft.transform.SetParent(hiddenRoot, false);
            southWallLeft.transform.localPosition = new Vector3(-w * 0.5f + 0.6f, h * 0.5f, -d * 0.5f - thick * 0.5f);
            southWallLeft.transform.localScale = new Vector3(1.2f, h, thick);
            southWallLeft.GetComponent<Renderer>().sharedMaterial = settings.stoneWallMaterial;

            // Right jamb
            var southWallRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
            southWallRight.name = "HiddenRoom_WallSouth_Right";
            southWallRight.transform.SetParent(hiddenRoot, false);
            southWallRight.transform.localPosition = new Vector3(w * 0.5f - 0.6f, h * 0.5f, -d * 0.5f - thick * 0.5f);
            southWallRight.transform.localScale = new Vector3(1.2f, h, thick);
            southWallRight.GetComponent<Renderer>().sharedMaterial = settings.stoneWallMaterial;

            // Sliding Barrel Stack (Secret door)
            var shelf = GameObject.Find("hidden shelf");
            if (!shelf)
            {
                shelf = GameObject.CreatePrimitive(PrimitiveType.Cube);
                shelf.name = "hidden shelf";
            }
            shelf.transform.SetParent(hiddenRoot, false);
            shelf.transform.localPosition = new Vector3(0f, 1.1f, -d * 0.5f - thick * 0.5f);
            shelf.transform.localScale = new Vector3(1.6f, 2.2f, 0.45f);
            shelf.GetComponent<Renderer>().sharedMaterial = settings.woodMaterial;
            var shelfMod = shelf.GetComponent<Unity.AI.Navigation.NavMeshModifier>();
            if (!shelfMod) shelfMod = shelf.AddComponent<Unity.AI.Navigation.NavMeshModifier>();
            shelfMod.ignoreFromBuild = true;

            // Add modeled stacked barrels to the sliding shelf face (B3)
            for (int b = 0; b < 3; b++)
            {
                var barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                barrel.name = $"FalseBarrel_{b}";
                barrel.transform.SetParent(shelf.transform, false);
                barrel.transform.localPosition = new Vector3(-0.35f + b * 0.35f, -0.2f + (b % 2) * 0.35f, -0.45f);
                barrel.transform.localScale = new Vector3(0.28f, 0.35f, 0.28f);
                barrel.GetComponent<Renderer>().sharedMaterial = settings.woodMaterial;
                var bc = barrel.GetComponent<Collider>();
                if (bc) Object.DestroyImmediate(bc);
            }

            var hiddenComp = shelf.GetComponent<HiddenCellarRoom>() ?? shelf.AddComponent<HiddenCellarRoom>();
            var noiseEmitter = shelf.GetComponent<NoiseEmitter>() ?? shelf.AddComponent<NoiseEmitter>();
            var act4Settings = AssetDatabase.LoadAssetAtPath<ActFourSettings>("Assets/_Project/Settings/ActFourSettings.asset");
            var hSo = new SerializedObject(hiddenComp);
            hSo.FindProperty("shelf").objectReferenceValue = shelf.transform;
            hSo.FindProperty("settings").objectReferenceValue = act4Settings;
            hSo.ApplyModifiedPropertiesWithoutUndo();

            // Inside Hidden Room:
            // 1. RadioSignalTracker transmitter position: "Hidden signal transmitter"
            var transmitter = GameObject.Find("Hidden signal transmitter");
            if (!transmitter)
            {
                transmitter = new GameObject("Hidden signal transmitter");
            }
            transmitter.transform.SetParent(hiddenRoot, false);
            transmitter.transform.localPosition = new Vector3(-0.8f, 0.95f, 0.5f);

            // 2. Marta reel-to-reel recorder on desk
            var recorder = GameObject.Find("Marta reel-to-reel recorder");
            if (!recorder)
            {
                recorder = GameObject.CreatePrimitive(PrimitiveType.Cube);
                recorder.name = "Marta reel-to-reel recorder";
            }
            recorder.transform.SetParent(hiddenRoot, false);
            recorder.transform.localPosition = new Vector3(-0.8f, 0.95f, 0.5f);
            recorder.transform.localScale = new Vector3(0.65f, 0.45f, 0.42f);
            recorder.GetComponent<Renderer>().sharedMaterial = settings.rustMetalMaterial;

            // Tape Reels & ReelRotator
            var leftReel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            leftReel.name = "LeftReel";
            leftReel.transform.SetParent(recorder.transform, false);
            leftReel.transform.localPosition = new Vector3(-0.25f, 0.15f, 0.52f);
            leftReel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            leftReel.transform.localScale = new Vector3(0.35f, 0.04f, 0.35f);
            leftReel.GetComponent<Renderer>().sharedMaterial = settings.rustMetalMaterial;
            var lrc = leftReel.GetComponent<Collider>(); if (lrc) Object.DestroyImmediate(lrc);

            var rightReel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rightReel.name = "RightReel";
            rightReel.transform.SetParent(recorder.transform, false);
            rightReel.transform.localPosition = new Vector3(0.25f, 0.15f, 0.52f);
            rightReel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            rightReel.transform.localScale = new Vector3(0.35f, 0.04f, 0.35f);
            rightReel.GetComponent<Renderer>().sharedMaterial = settings.rustMetalMaterial;
            var rrc = rightReel.GetComponent<Collider>(); if (rrc) Object.DestroyImmediate(rrc);

            var recComp = recorder.GetComponent<RecordedTransmitter>() ?? recorder.AddComponent<RecordedTransmitter>();
            var rotator = recorder.GetComponent<ReelRotator>() ?? recorder.AddComponent<ReelRotator>();
            var rotSo = new SerializedObject(rotator);
            rotSo.FindProperty("leftReel").objectReferenceValue = leftReel.transform;
            rotSo.FindProperty("rightReel").objectReferenceValue = rightReel.transform;
            rotSo.FindProperty("transmitter").objectReferenceValue = recComp;
            rotSo.ApplyModifiedPropertiesWithoutUndo();

            // 3. Tube Radios with faint emissive glow
            var tubeRadio = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tubeRadio.name = "Vacuum_Tube_Radio_Chassis";
            tubeRadio.transform.SetParent(hiddenRoot, false);
            tubeRadio.transform.localPosition = new Vector3(-1.4f, 0.95f, 0.5f);
            tubeRadio.transform.localScale = new Vector3(0.55f, 0.45f, 0.4f);
            tubeRadio.GetComponent<Renderer>().sharedMaterial = settings.rustMetalMaterial;

            // Emissive glowing vacuum tubes
            for (int t = 0; t < 3; t++)
            {
                var tube = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                tube.name = $"VacuumTube_{t}";
                tube.transform.SetParent(tubeRadio.transform, false);
                tube.transform.localPosition = new Vector3(-0.35f + t * 0.35f, 0.55f, 0f);
                tube.transform.localScale = new Vector3(0.18f, 0.25f, 0.18f);
                tube.GetComponent<Renderer>().sharedMaterial = settings.paperMaterial; // Or glowing glass
                var tc = tube.GetComponent<Collider>(); if (tc) Object.DestroyImmediate(tc);
            }

            // 4. Sturdy Workbench
            var workbench = GameObject.CreatePrimitive(PrimitiveType.Cube);
            workbench.name = "Secret_Workbench";
            workbench.transform.SetParent(hiddenRoot, false);
            workbench.transform.localPosition = new Vector3(-0.6f, 0.38f, 0.5f);
            workbench.transform.localScale = new Vector3(2.4f, 0.76f, 0.95f);
            workbench.GetComponent<Renderer>().sharedMaterial = settings.woodMaterial;

            // 5. Maritime charts with red Marguerite wreck lines
            var charts = GameObject.CreatePrimitive(PrimitiveType.Cube);
            charts.name = "Marguerite_Nautical_Chart";
            charts.transform.SetParent(hiddenRoot, false);
            charts.transform.localPosition = new Vector3(0.4f, 0.77f, 0.5f);
            charts.transform.localScale = new Vector3(0.9f, 0.02f, 0.65f);
            charts.GetComponent<Renderer>().sharedMaterial = settings.paperMaterial;

            // 6. Harbor board ledger under a warm shaded task lamp
            var ledgerGo = GameObject.Find("Harbor board ledger");
            if (!ledgerGo)
            {
                ledgerGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                ledgerGo.name = "Harbor board ledger";
            }
            ledgerGo.transform.SetParent(hiddenRoot, false);
            ledgerGo.transform.localPosition = new Vector3(0.35f, 0.8f, 0.5f);
            ledgerGo.transform.localScale = new Vector3(0.45f, 0.06f, 0.35f);
            ledgerGo.GetComponent<Renderer>().sharedMaterial = settings.paperMaterial;
            var ledgerComp = ledgerGo.GetComponent<Ledger>() ?? ledgerGo.AddComponent<Ledger>();

            // Warm shaded task lamp focused on the ledger
            var lampGo = new GameObject("Task_Lamp_Ledger");
            lampGo.transform.SetParent(hiddenRoot, false);
            lampGo.transform.localPosition = new Vector3(0.35f, 1.4f, 0.5f);
            var lampLight = lampGo.AddComponent<Light>();
            lampLight.type = LightType.Spot;
            lampLight.spotAngle = 65f;
            lampLight.innerSpotAngle = 35f;
            lampLight.range = 3.5f;
            lampLight.intensity = 3.2f;
            lampLight.color = new Color(1.0f, 0.65f, 0.30f); // Warm #FFA64D
            lampLight.shadows = LightShadows.Soft;

            // 7. Framed photo of the SHIP (The Marguerite steamship, NOT the family photo!)
            var shipPhoto = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shipPhoto.name = "Photo_Of_Ship_Marguerite";
            shipPhoto.transform.SetParent(hiddenRoot, false);
            shipPhoto.transform.localPosition = new Vector3(0f, 1.8f, d * 0.5f);
            shipPhoto.transform.localScale = new Vector3(0.9f, 0.6f, 0.04f);
            shipPhoto.GetComponent<Renderer>().sharedMaterial = settings.paperMaterial;
        }

        private static void CreateCagedLantern(string name, Transform parent, Vector3 localPos)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.localPosition = localPos;

            // Caged iron bracket & hood
            var bracket = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bracket.name = "Bracket";
            bracket.transform.SetParent(root.transform, false);
            bracket.transform.localScale = new Vector3(0.18f, 0.32f, 0.18f);
            var bCol = bracket.GetComponent<Collider>();
            if (bCol) Object.DestroyImmediate(bCol);

            // Light source: warm amber #FFA64D
            var lightGo = new GameObject("Lantern_Light");
            lightGo.transform.SetParent(root.transform, false);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1.0f, 0.65f, 0.30f); // #FFA64D
            light.intensity = 1.6f;
            light.range = 5.5f;
            light.shadows = LightShadows.None; // Low real-time shadow cost
        }

        private static void SetupHidingSpots(GameObject towerRoot, GameObject cellarRoot)
        {
            var stalker = GameObject.Find("Stalker");
            var player = Object.FindAnyObjectByType<PlayerHealthState>();
            var actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions");

            // 10 Hiding spots across rooms (minimum 8 required by A3)
            var spotDefs = new (string id, Vector3 pos, string room)[]
            {
                ("keeper-wardrobe", new Vector3(-8f, 0f, -5f), "Keeper House Bedroom"),
                ("pantry-cupboard", new Vector3(-3f, 0f, -5f), "Keeper House Pantry"),
                ("generator-locker", new Vector3(-1f, 0f, -3f), "Keeper House Generator"),
                ("boat-shed-crate", new Vector3(10f, 0f, -14f), "Boat Shed"),
                ("vestibule-nook", new Vector3(1.2f, 0f, -4.0f), "Tower Vestibule"),
                ("crane-wardrobe", new Vector3(-3.2f, 3.2f, 1.8f), "Floor 2 Crane's Room"),
                ("watchroom-alcove", new Vector3(1.6f, 6.4f, 2.8f), "Floor 3 Watchroom"),
                ("lantern-cabinet", new Vector3(1.8f, 19.2f, 1.2f), "Lantern Room"),
                ("cellar-coal-bin", new Vector3(-1.1f, -5.0f, 12.0f), "Cellar Corridor"),
                ("hidden-room-crate", new Vector3(-0.8f, -5.0f, 28.0f), "Hidden Room")
            };

            var spotList = new List<HidingSpot>();

            foreach (var def in spotDefs)
            {
                var spotGo = GameObject.Find(def.id);
                if (!spotGo)
                {
                    spotGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    spotGo.name = def.id;
                }
                spotGo.transform.position = def.pos + Vector3.up * 1.0f;
                spotGo.transform.localScale = new Vector3(1.1f, 2.0f, 1.1f);
                var col = spotGo.GetComponent<BoxCollider>();
                if (col)
                {
                    col.size = new Vector3(0.95f, 0.9f, 0.95f);
                    col.center = new Vector3(0f, -0.45f, 0f);
                }

                var pose = spotGo.transform.Find("Pose");
                if (!pose)
                {
                    var p = new GameObject("Pose");
                    p.transform.SetParent(spotGo.transform, false);
                    p.transform.localPosition = new Vector3(0f, -0.8f, 0f);
                    pose = p.transform;
                }

                var spot = spotGo.GetComponent<HidingSpot>() ?? spotGo.AddComponent<HidingSpot>();
                var so = new SerializedObject(spot);
                so.FindProperty("spotId").stringValue = def.id;
                so.FindProperty("hidingPose").objectReferenceValue = pose;
                so.FindProperty("inputActions").objectReferenceValue = actions;
                so.FindProperty("player").objectReferenceValue = player;
                so.FindProperty("stalker").objectReferenceValue = stalker ? stalker.transform : null;
                so.ApplyModifiedPropertiesWithoutUndo();

                spotList.Add(spot);
            }

            if (stalker && stalker.TryGetComponent<StalkerAI>(out var ai))
            {
                var aiSo = new SerializedObject(ai);
                var prop = aiSo.FindProperty("hidingSpots");
                if (prop != null)
                {
                    prop.arraySize = spotList.Count;
                    for (int i = 0; i < spotList.Count; i++)
                    {
                        prop.GetArrayElementAtIndex(i).objectReferenceValue = spotList[i];
                    }
                    aiSo.ApplyModifiedPropertiesWithoutUndo();
                }
            }
        }

        private static void SetupLogbookPages(string actName)
        {
            var journal = Object.FindAnyObjectByType<LogbookJournal>();
            if (!journal) return;

            // 6 logbook pages back in place by ID
            string pageFolder = "Assets/_Project/LogbookPages";
            var pageDefs = new (string id, string title, string flag, Vector3 pos)[]
            {
                ("marguerite-wreck", "The Marguerite", "marguerite", new Vector3(-8f, 0.9f, -6f)),
                ("wrecker-tally", "Wreckers' Tally", "", new Vector3(1.0f, 3.35f, 3.32f)),
                ("lighthouse-leash", "The Light Is a Leash", "lighthouse-leash", new Vector3(-3f, 0.9f, -5f)),
                ("keeper-selection", "Keeper Selection", "", new Vector3(10f, 0.5f, -14f)),
                ("crane-observation", "Crane's Observation", "", new Vector3(-1.1f, -4.6f, 15f)),
                ("crane-final-entry", "Crane's Last Entry", "crane-bargain", new Vector3(-3.2f, 4.05f, 0.2f))
            };

            foreach (var def in pageDefs)
            {
                string assetPath = $"{pageFolder}/{def.id}.asset";
                var pageAsset = AssetDatabase.LoadAssetAtPath<LogbookPage>(assetPath);
                if (!pageAsset) continue;

                string pickupName = $"LogbookPagePickup_{def.id}";
                var pickupGo = GameObject.Find(pickupName);
                if (!pickupGo)
                {
                    pickupGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    pickupGo.name = pickupName;
                }
                pickupGo.transform.position = def.pos;
                pickupGo.transform.localScale = new Vector3(0.45f, 0.08f, 0.65f);
                var col = pickupGo.GetComponent<Collider>();
                if (col) col.isTrigger = true;

                var mod = pickupGo.GetComponent<Unity.AI.Navigation.NavMeshModifier>();
                if (!mod) mod = pickupGo.AddComponent<Unity.AI.Navigation.NavMeshModifier>();
                mod.ignoreFromBuild = true;

                var pickup = pickupGo.GetComponent<LogbookPickup>() ?? pickupGo.AddComponent<LogbookPickup>();
                var so = new SerializedObject(pickup);
                so.FindProperty("page").objectReferenceValue = pageAsset;
                so.FindProperty("journal").objectReferenceValue = journal;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static void ConfigurePlayerAndNavMesh()
        {
            // Configure CharacterController settings
            var player = GameObject.Find("Player");
            if (player && player.TryGetComponent<CharacterController>(out var cc))
            {
                cc.height = 1.8f;
                cc.radius = 0.4f;
                cc.slopeLimit = 45f;
                cc.stepOffset = 0.3f;
                EditorUtility.SetDirty(cc);
            }

            var moveSettings = Resources.Load<PlayerMovementSettings>("PlayerMovementSettings");
            if (moveSettings)
            {
                moveSettings.standingHeight = 1.8f;
                moveSettings.slopeLimit = 45f;
                moveSettings.stepOffset = 0.3f;
                EditorUtility.SetDirty(moveSettings);
            }

            // Bake NavMesh: agent radius 0.4, height 1.8, step 0.3, max slope 45
            var navSurface = Object.FindAnyObjectByType<NavMeshSurface>();
            if (navSurface)
            {
                navSurface.agentTypeID = 0; // Humanoid
                navSurface.collectObjects = CollectObjects.All;
                navSurface.useGeometry = UnityEngine.AI.NavMeshCollectGeometry.PhysicsColliders;
                navSurface.overrideVoxelSize = true;
                navSurface.voxelSize = 0.08f;
                navSurface.overrideTileSize = true;
                navSurface.tileSize = 64;
                navSurface.minRegionArea = 0.1f;
                navSurface.BuildNavMesh();
                EditorUtility.SetDirty(navSurface);
                Debug.Log("[InteriorLayoutBuilder] NavMesh successfully baked from physics colliders!");
            }
        }

        private static void SetupNavMeshLinks(GameObject towerRoot, GameObject cellarRoot)
        {
            var linksGroup = new GameObject("NavMesh_Links").transform;
            linksGroup.SetParent(towerRoot.transform, false);

            var settings = InteriorKitBuilder.EnsureSettings();
            int totalLevels = 3 + settings.stairOnlyLevels;

            for (int lvl = 0; lvl < totalLevels; lvl++)
            {
                float floorY = lvl * settings.floorHeight;
                bool isEast = (lvl % 2 == 0);
                float stairX = isEast ? 2.15f : -2.15f;

                var linkGo = new GameObject($"Stair_Flight_{lvl + 1}_NavMeshLink");
                linkGo.transform.SetParent(linksGroup, false);
                var link = linkGo.AddComponent<Unity.AI.Navigation.NavMeshLink>();
                link.startPoint = isEast ? new Vector3(stairX, floorY + 0.1f, -2.52f) : new Vector3(stairX, floorY + 0.1f, 2.52f);
                link.endPoint = isEast ? new Vector3(stairX, floorY + settings.floorHeight + 0.1f, 2.52f) : new Vector3(stairX, floorY + settings.floorHeight + 0.1f, -2.52f);
                link.width = settings.stairWidth;
                link.bidirectional = true;
            }

            // Cellar descent link
            var cellarLinkGo = new GameObject("Cellar_Stair_NavMeshLink");
            cellarLinkGo.transform.SetParent(cellarRoot.transform, false);
            var cellarLink = cellarLinkGo.AddComponent<Unity.AI.Navigation.NavMeshLink>();
            cellarLink.startPoint = new Vector3(-2.2f, 0.1f, -1.6f);
            cellarLink.endPoint = new Vector3(-2.2f, -4.9f, 5.9f);
            cellarLink.width = settings.stairWidth;
            cellarLink.bidirectional = true;

            // Secret room doorway link
            var secretLinkGo = new GameObject("Secret_Room_NavMeshLink");
            secretLinkGo.transform.SetParent(cellarRoot.transform, false);
            var secretLink = secretLinkGo.AddComponent<Unity.AI.Navigation.NavMeshLink>();
            secretLink.startPoint = new Vector3(-2.2f, -4.9f, 25.0f);
            secretLink.endPoint = new Vector3(-2.2f, -4.9f, 27.0f);
            secretLink.width = 1.6f;
            secretLink.bidirectional = true;
        }

        private static void SetStaticGeometryFlags(GameObject root)
        {
            if (!root) return;
            foreach (var r in root.GetComponentsInChildren<MeshRenderer>(true))
            {
                // Skip moving door panels or interactive props
                if (r.GetComponentInParent<Door>() != null && r.transform.name.Contains("Panel")) continue;
                if (r.name.Contains("DoorLeaf") || r.name.Contains("DoorPivot") || r.name.Contains("hidden shelf")) continue;

                GameObjectUtility.SetStaticEditorFlags(r.gameObject,
                    StaticEditorFlags.BatchingStatic |
                    StaticEditorFlags.OccludeeStatic |
                    StaticEditorFlags.OccluderStatic |
                    StaticEditorFlags.NavigationStatic |
                    StaticEditorFlags.ContributeGI);
            }
        }
    }
}
