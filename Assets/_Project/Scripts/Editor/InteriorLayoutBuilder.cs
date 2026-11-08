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
        public const string KeeperHouseRootName = "Keeper House Modular";
        public const string CellarRootName = "Cellar Complex Modular";

        public static T GetOrAddComponent<T>(GameObject go) where T : Component
        {
            var comp = go.GetComponent<T>();
            return comp != null ? comp : go.AddComponent<T>();
        }

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

        public static void TeleportToCellar()
        {
            var cellarDoor = GameObject.Find("Locked Cellar Door")?.GetComponent<Door>();
            if (cellarDoor) cellarDoor.SetLocked(false);
            var gate = Object.FindAnyObjectByType<ActFourGate>();
            if (gate)
            {
                GameEvents.RaiseStoryBeatTriggered(new StoryBeatTriggeredEvent("A4-B02"));
            }
            GameEvents.RaiseStorySignal("cellar-unlocked");

            var player = GameObject.FindWithTag("Player") ?? GameObject.Find("Player");
            if (player)
            {
                // Move player to top of cellar stair in keeper's house storeroom
                var cc = player.GetComponent<CharacterController>();
                if (cc) cc.enabled = false;
                player.transform.position = new Vector3(-9.5f, 0.25f, -2.5f);
                if (cc) cc.enabled = true;
                Physics.SyncTransforms();
                Debug.Log("[Teleport] Cellar door unlocked and player moved to top of cellar stair.");
            }
        }

        public static void RebuildScene(Scene scene, string actName)
        {
            var settings = InteriorKitBuilder.EnsureSettings();
            InteriorKitBuilder.BuildMaterials(settings);
            InteriorKitBuilder.BuildPrefabs(settings);

            // A0: Delete legacy generated tower interior, stairs, mezzanines, cellar and hidden room
            CleanLegacyInteriorAndCellar();

            // STEP 1: Build Tower Interior (5.0 x 5.0m square shaft inside round shell)
            var towerRoot = BuildTowerInterior(settings, actName);

            // STEP 2: Build Keeper's House (14.0 x 8.0m rectangular two storeys with pitched roof)
            var houseRoot = BuildKeeperHouse(settings, actName);

            // STEP 3: Build Underground Entry & Cellar (descends 5m behind cellar door in house)
            var cellarRoot = BuildCellarAndHiddenRoom(settings, actName);

            // Hiding spots, Logbook pages, CharacterController & NavMesh links
            SetupHidingSpots(towerRoot, houseRoot, cellarRoot);
            SetupLogbookPages(actName);
            SetupNavMeshLinks(towerRoot, houseRoot, cellarRoot);

            // Set static flags on all interior geometry BEFORE baking NavMesh
            SetStaticGeometryFlags(towerRoot);
            SetStaticGeometryFlags(houseRoot);
            SetStaticGeometryFlags(cellarRoot);

            // STEP 4: Configure Player CharacterController and Bake NavMesh
            ConfigurePlayerAndNavMesh();

            Physics.SyncTransforms();

            EditorUtility.SetDirty(towerRoot);
            EditorUtility.SetDirty(houseRoot);
            EditorUtility.SetDirty(cellarRoot);
        }

        public static void CleanLegacyInteriorAndCellar()
        {
            // 1. Delete previous modular roots if present
            var prevTower = GameObject.Find(TowerInteriorRootName);
            if (prevTower) Object.DestroyImmediate(prevTower);

            var prevHouse = GameObject.Find(KeeperHouseRootName);
            if (prevHouse) Object.DestroyImmediate(prevHouse);

            var prevCellar = GameObject.Find(CellarRootName);
            if (prevCellar) Object.DestroyImmediate(prevCellar);

            // 2. Remove legacy generated interior hierarchies
            string[] legacyGroupPaths = new[]
            {
                "Reference Visuals/Lighthouse/GroundFloor",
                "Reference Visuals/Lighthouse/FirstFloor",
                "Reference Visuals/Lighthouse/SecondFloor",
                "Reference Visuals/Lighthouse/UpperTower",
                "Reference Visuals/Lighthouse/LanternFloor",
                "Reference Visuals/Lighthouse/AttachedCottage",
                "Reference Visuals/Lighthouse/Shared/Lighting",
                "Reference Visuals/Lighthouse/Shared/CoastDressing",
                "Harrow Rock Master Environment/Keeper House Living & Kitchen Master",
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

            // Clean duplicate front door if both exist
            var dupFrontDoor = GameObject.Find("Keeper_House_Front_Door");
            if (dupFrontDoor && GameObject.Find("Keeper House Front Door") != null)
            {
                Object.DestroyImmediate(dupFrontDoor);
            }

            // Clean any legacy CellarDoorArch or Vestibule artifacts on Locked Cellar Door
            var existingCellarDoor = GameObject.Find("Locked Cellar Door");
            if (existingCellarDoor)
            {
                var oldArch = existingCellarDoor.transform.Find("CellarDoorArch");
                if (oldArch) Object.DestroyImmediate(oldArch.gameObject);
            }

            // Remove legacy cellar arch walls in Room Dividers
            var dividers = GameObject.Find("Lighthouse Layout/Room Dividers");
            if (dividers)
            {
                var toDelete = new List<GameObject>();
                foreach (Transform child in dividers.transform)
                {
                    if (child.name.StartsWith("Cellar Arch") || child.name.StartsWith("Cellar floor") || child.name.StartsWith("Hidden room") || child.name.StartsWith("House"))
                    {
                        toDelete.Add(child.gameObject);
                    }
                }
                foreach (var go in toDelete) Object.DestroyImmediate(go);
            }

            // Clean internal tower slabs
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
        }

        // =========================================================================
        // STEP 1: TOWER INTERIOR (Square plan 5.0 x 5.0m inside round shell)
        // =========================================================================
        private static GameObject BuildTowerInterior(InteriorSettings settings, string actName)
        {
            var root = new GameObject(TowerInteriorRootName);
            float h = settings.floorHeight; // 3.2m
            float shaftSize = settings.towerShaftSize; // 5.0m
            float halfShaft = shaftSize * 0.5f; // 2.5m
            int totalLevels = 3 + settings.stairOnlyLevels; // 6 levels up to deck at 19.2m
            float totalH = totalLevels * h; // 19.2m

            // Shaft Enclosing Flat Walls (thick masonry infill between 5x5m square and outer shell)
            var shaftGroup = new GameObject("Shaft_Enclosure_Walls").transform;
            shaftGroup.SetParent(root.transform, false);

            float wallThick = 0.40f;
            // North Flat Wall (z = +2.5m)
            var northWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            northWall.name = "Shaft_Wall_North";
            northWall.transform.SetParent(shaftGroup, false);
            northWall.transform.localPosition = new Vector3(0f, totalH * 0.5f, halfShaft + wallThick * 0.5f);
            northWall.transform.localScale = new Vector3(shaftSize + wallThick * 2f, totalH, wallThick);
            northWall.GetComponent<Renderer>().sharedMaterial = settings.stoneWallMaterial;

            // East Flat Wall (x = +2.5m)
            var eastWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            eastWall.name = "Shaft_Wall_East";
            eastWall.transform.SetParent(shaftGroup, false);
            eastWall.transform.localPosition = new Vector3(halfShaft + wallThick * 0.5f, totalH * 0.5f, 0f);
            eastWall.transform.localScale = new Vector3(wallThick, totalH, shaftSize);
            eastWall.GetComponent<Renderer>().sharedMaterial = settings.stoneWallMaterial;

            // South Flat Wall (z = -2.5m) with ground floor doorway opening
            // Lower south wall has doorway x in [-0.8, 0.8]
            var southWallLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
            southWallLeft.name = "Shaft_Wall_South_Left";
            southWallLeft.transform.SetParent(shaftGroup, false);
            southWallLeft.transform.localPosition = new Vector3(-1.65f, totalH * 0.5f, -halfShaft - wallThick * 0.5f);
            southWallLeft.transform.localScale = new Vector3(1.7f, totalH, wallThick);
            southWallLeft.GetComponent<Renderer>().sharedMaterial = settings.stoneWallMaterial;

            var southWallRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
            southWallRight.name = "Shaft_Wall_South_Right";
            southWallRight.transform.SetParent(shaftGroup, false);
            southWallRight.transform.localPosition = new Vector3(1.65f, totalH * 0.5f, -halfShaft - wallThick * 0.5f);
            southWallRight.transform.localScale = new Vector3(1.7f, totalH, wallThick);
            southWallRight.GetComponent<Renderer>().sharedMaterial = settings.stoneWallMaterial;

            var southWallUpper = GameObject.CreatePrimitive(PrimitiveType.Cube);
            southWallUpper.name = "Shaft_Wall_South_Upper";
            southWallUpper.transform.SetParent(shaftGroup, false);
            southWallUpper.transform.localPosition = new Vector3(0f, (totalH + 2.5f) * 0.5f, -halfShaft - wallThick * 0.5f);
            southWallUpper.transform.localScale = new Vector3(1.6f, totalH - 2.5f, wallThick);
            southWallUpper.GetComponent<Renderer>().sharedMaterial = settings.stoneWallMaterial;

            // West Flat Wall (x = -2.5m) with openings at Floor 1 (to Keeper's House) and Floor 2 (upper bridge)
            var westWallNorth = GameObject.CreatePrimitive(PrimitiveType.Cube);
            westWallNorth.name = "Shaft_Wall_West_North";
            westWallNorth.transform.SetParent(shaftGroup, false);
            westWallNorth.transform.localPosition = new Vector3(-halfShaft - wallThick * 0.5f, totalH * 0.5f, 1.4f);
            westWallNorth.transform.localScale = new Vector3(wallThick, totalH, 2.2f);
            westWallNorth.GetComponent<Renderer>().sharedMaterial = settings.stoneWallMaterial;

            var westWallUpper = GameObject.CreatePrimitive(PrimitiveType.Cube);
            westWallUpper.name = "Shaft_Wall_West_Upper";
            westWallUpper.transform.SetParent(shaftGroup, false);
            westWallUpper.transform.localPosition = new Vector3(-halfShaft - wallThick * 0.5f, (totalH + 6.4f) * 0.5f, -1.35f);
            westWallUpper.transform.localScale = new Vector3(wallThick, totalH - 6.4f, 2.3f);
            westWallUpper.GetComponent<Renderer>().sharedMaterial = settings.stoneWallMaterial;

            // Build Levels 1 to 6
            for (int lvl = 0; lvl < totalLevels; lvl++)
            {
                float floorY = lvl * h;
                var levelGroup = new GameObject($"Tower_Floor_{lvl + 1}").transform;
                levelGroup.SetParent(root.transform, false);
                levelGroup.localPosition = new Vector3(0f, floorY, 0f);

                // Build Floor Slabs
                if (lvl == 0)
                {
                    // Floor 1 Vestibule Flagstone Floor
                    var vestFloor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    vestFloor.name = "Vestibule_Floor";
                    vestFloor.transform.SetParent(levelGroup, false);
                    vestFloor.transform.localPosition = new Vector3(0f, -0.1f, 0f);
                    vestFloor.transform.localScale = new Vector3(shaftSize, 0.2f, shaftSize);
                    vestFloor.GetComponent<Renderer>().sharedMaterial = settings.flagstoneMaterial;

                    // Vestibule Dressing: Front door from courtyard, door to keeper's house, oil drums and tools
                    BuildFloor1Vestibule(levelGroup, settings);
                }
                else
                {
                    // Floor 2 to 6: Slabs with switchback openings
                    var slab = (GameObject)PrefabUtility.InstantiatePrefab(settings.floorSlabPrefab, levelGroup);
                    slab.name = $"FloorSlab_L{lvl + 1}";
                    slab.transform.localPosition = Vector3.zero;
                    slab.transform.localRotation = Quaternion.identity;

                    if (lvl == 1)
                    {
                        // Floor 2 landing with door to keeper's house upper floor
                        BuildFloor2Landing(levelGroup, settings);
                    }
                    else if (lvl == 2)
                    {
                        // Floor 3 rectangular watchroom (3 x 4 m, flat walls)
                        BuildFloor3Watchroom(levelGroup, settings);
                    }
                    else
                    {
                        // Floors 4, 5, 6: Stair-only levels with slit windows
                        BuildStairOnlyLevel(levelGroup, settings, lvl);
                    }
                }

                // Straight Switchback Stairs (Two flights per floor, width 1.2m, 18 steps total, rise 0.178, run 0.28)
                // Flight A (East Side, climbing South to North): x in [1.3, 2.5]
                var flightA = (GameObject)PrefabUtility.InstantiatePrefab(settings.stairFlightPrefab, levelGroup);
                flightA.name = $"Flight_L{lvl + 1}_East_Up";
                flightA.transform.localPosition = new Vector3(1.90f, 0f, -1.26f);
                flightA.transform.localRotation = Quaternion.identity;

                // North Turn Landing (top at y = 1.602m, z in [1.26, 2.50]):
                var landingNorth = (GameObject)PrefabUtility.InstantiatePrefab(settings.landingPrefab, levelGroup);
                landingNorth.name = $"TurnLanding_L{lvl + 1}_North";
                landingNorth.transform.localPosition = new Vector3(0f, 1.602f - 0.125f, 1.88f);
                landingNorth.transform.localScale = new Vector3(shaftSize, 0.25f, 1.24f);

                // Flight B (West Side, climbing North to South): x in [-2.5, -1.3]
                var flightB = (GameObject)PrefabUtility.InstantiatePrefab(settings.stairFlightPrefab, levelGroup);
                flightB.name = $"Flight_L{lvl + 1}_West_Up";
                flightB.transform.localPosition = new Vector3(-1.90f, 1.602f, 1.26f);
                flightB.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

                // Handrails along inner stairwell spine (x = 1.30 and x = -1.30)
                BuildInnerHandrails(levelGroup, settings, flightA.transform, flightB.transform);
            }

            // Top Deck (y = 19.2m): Flat-sided octagonal lantern room
            BuildLanternRoom(root.transform, settings, totalH, actName);

            return root;
        }

        private static void BuildInnerHandrails(Transform parent, InteriorSettings settings, Transform flightA, Transform flightB)
        {
            var railGroup = new GameObject("Stair_Safety_Railings").transform;
            railGroup.SetParent(parent, false);

            // Handrail along Flight A inner side (x = 1.28)
            var railA = (GameObject)PrefabUtility.InstantiatePrefab(settings.handrailPrefab, railGroup);
            railA.name = "Handrail_Inner_East";
            railA.transform.localPosition = new Vector3(1.28f, 0f, -1.26f);
            railA.transform.localRotation = Quaternion.identity;

            // Handrail along Flight B inner side (x = -1.28)
            var railB = (GameObject)PrefabUtility.InstantiatePrefab(settings.handrailPrefab, railGroup);
            railB.name = "Handrail_Inner_West";
            railB.transform.localPosition = new Vector3(-1.28f, 1.602f, 1.26f);
            railB.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

            // Turn landing barrier at North well opening
            var barrier = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            barrier.name = "Turn_Landing_Barrier";
            barrier.transform.SetParent(railGroup, false);
            barrier.transform.localPosition = new Vector3(0f, 2.55f, 1.28f);
            barrier.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            barrier.transform.localScale = new Vector3(0.05f, 1.25f, 0.05f);
            barrier.GetComponent<Renderer>().sharedMaterial = settings.rustMetalMaterial;
            var c = barrier.GetComponent<Collider>();
            if (c) Object.DestroyImmediate(c);
        }

        private static void BuildFloor1Vestibule(Transform parent, InteriorSettings settings)
        {
            var vestGroup = new GameObject("Vestibule_Dressing").transform;
            vestGroup.SetParent(parent, false);

            // 1. Existing Front Door from Courtyard (Tower Door)
            var frontDoor = GameObject.Find("Tower Door");
            if (frontDoor)
            {
                frontDoor.transform.position = new Vector3(0f, 0.15f, -2.50f);
                frontDoor.transform.rotation = Quaternion.identity;
            }

            // Covered entrance passage bridging outer shell door to square shaft south wall
            var passage = new GameObject("Courtyard_Entrance_Passage").transform;
            passage.SetParent(vestGroup, false);
            var passageFloor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            passageFloor.name = "PassageFloor";
            passageFloor.transform.SetParent(passage, false);
            passageFloor.transform.localPosition = new Vector3(0f, -0.1f, -4.5f);
            passageFloor.transform.localScale = new Vector3(2.4f, 0.2f, 4.0f);
            passageFloor.GetComponent<Renderer>().sharedMaterial = settings.flagstoneMaterial;

            // 2. Door into Keeper's House Generator Room on West Wall (x = -2.5, z = -1.8)
            var houseDoor = (GameObject)PrefabUtility.InstantiatePrefab(settings.doorFramePrefab, vestGroup);
            houseDoor.name = "Door_To_Keepers_House";
            houseDoor.transform.localPosition = new Vector3(-2.50f, 0f, -1.80f);
            houseDoor.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);

            // 3. Oil drums, toolboxes, lantern
            for (int i = 0; i < 3; i++)
            {
                var drum = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                drum.name = $"Vestibule_OilDrum_{i + 1}";
                drum.transform.SetParent(vestGroup, false);
                drum.transform.localPosition = new Vector3(-0.8f + i * 0.45f, 0.45f, 0.8f);
                drum.transform.localScale = new Vector3(0.42f, 0.45f, 0.42f);
                drum.GetComponent<Renderer>().sharedMaterial = settings.rustMetalMaterial;
            }

            var toolbox = GameObject.CreatePrimitive(PrimitiveType.Cube);
            toolbox.name = "Vestibule_Toolbox";
            toolbox.transform.SetParent(vestGroup, false);
            toolbox.transform.localPosition = new Vector3(0.5f, 0.25f, 0.8f);
            toolbox.transform.localScale = new Vector3(0.55f, 0.35f, 0.35f);
            toolbox.GetComponent<Renderer>().sharedMaterial = settings.rustMetalMaterial;

            CreateCagedLantern("Vestibule_Lantern", vestGroup, new Vector3(0f, 2.2f, -1.2f));
        }

        private static void BuildFloor2Landing(Transform parent, InteriorSettings settings)
        {
            var landingGroup = new GameObject("Floor2_Landing_Dressing").transform;
            landingGroup.SetParent(parent, false);

            // Door on West Wall (x = -2.5, z = -1.8) connecting to Keeper's House Upper Floor
            var bridgeDoor = (GameObject)PrefabUtility.InstantiatePrefab(settings.doorFramePrefab, landingGroup);
            bridgeDoor.name = "Door_To_House_Upper_Floor";
            bridgeDoor.transform.localPosition = new Vector3(-2.50f, 0f, -1.80f);
            bridgeDoor.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);

            CreateCagedLantern("Floor2_Lantern", landingGroup, new Vector3(0f, 2.2f, -1.8f));
        }

        private static void BuildFloor3Watchroom(Transform parent, InteriorSettings settings)
        {
            var watchGroup = new GameObject("Service_Watchroom").transform;
            watchGroup.SetParent(parent, false);

            // Rectangular watchroom (3 x 4 m, flat walls): extends West from shaft at x in [-5.5, -2.5], z in [-2.0, 2.0]
            float w = 3.0f;
            float d = 4.0f;
            float h = 3.2f;
            float thick = 0.25f;

            var roomFloor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roomFloor.name = "Watchroom_Floor";
            roomFloor.transform.SetParent(watchGroup, false);
            roomFloor.transform.localPosition = new Vector3(-4.0f, -0.125f, 0f);
            roomFloor.transform.localScale = new Vector3(w, thick, d);
            roomFloor.GetComponent<Renderer>().sharedMaterial = settings.woodMaterial;

            var roomCeil = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roomCeil.name = "Watchroom_Ceiling";
            roomCeil.transform.SetParent(watchGroup, false);
            roomCeil.transform.localPosition = new Vector3(-4.0f, h + thick * 0.5f, 0f);
            roomCeil.transform.localScale = new Vector3(w, thick, d);
            roomCeil.GetComponent<Renderer>().sharedMaterial = settings.plasterMaterial;

            // Outer West wall (x = -5.5)
            var westWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            westWall.name = "Watchroom_Wall_West";
            westWall.transform.SetParent(watchGroup, false);
            westWall.transform.localPosition = new Vector3(-5.5f - thick * 0.5f, h * 0.5f, 0f);
            westWall.transform.localScale = new Vector3(thick, h, d);
            westWall.GetComponent<Renderer>().sharedMaterial = settings.stoneWallMaterial;

            // North wall (z = +2.0)
            var northWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            northWall.name = "Watchroom_Wall_North";
            northWall.transform.SetParent(watchGroup, false);
            northWall.transform.localPosition = new Vector3(-4.0f, h * 0.5f, 2.0f + thick * 0.5f);
            northWall.transform.localScale = new Vector3(w + thick * 2f, h, thick);
            northWall.GetComponent<Renderer>().sharedMaterial = settings.stoneWallMaterial;

            // South wall (z = -2.0)
            var southWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            southWall.name = "Watchroom_Wall_South";
            southWall.transform.SetParent(watchGroup, false);
            southWall.transform.localPosition = new Vector3(-4.0f, h * 0.5f, -2.0f - thick * 0.5f);
            southWall.transform.localScale = new Vector3(w + thick * 2f, h, thick);
            southWall.GetComponent<Renderer>().sharedMaterial = settings.stoneWallMaterial;

            // Doorway connecting from tower shaft landing into watchroom
            var watchDoor = (GameObject)PrefabUtility.InstantiatePrefab(settings.doorFramePrefab, watchGroup);
            watchDoor.name = "Door_To_Watchroom";
            watchDoor.transform.localPosition = new Vector3(-2.50f, 0f, 0f);
            watchDoor.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);

            // Non-interactive dead generator prop (large cast iron housing, flywheel, rusted fuel lines)
            var genHousing = GameObject.CreatePrimitive(PrimitiveType.Cube);
            genHousing.name = "Dead_Generator_Housing_Prop";
            genHousing.transform.SetParent(watchGroup, false);
            genHousing.transform.localPosition = new Vector3(-4.5f, 0.65f, 0.8f);
            genHousing.transform.localScale = new Vector3(1.1f, 1.3f, 1.5f);
            genHousing.GetComponent<Renderer>().sharedMaterial = settings.rustMetalMaterial;

            var flywheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            flywheel.name = "Generator_Flywheel";
            flywheel.transform.SetParent(genHousing.transform, false);
            flywheel.transform.localPosition = new Vector3(0f, 0f, 0.8f);
            flywheel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            flywheel.transform.localScale = new Vector3(0.95f, 0.12f, 0.95f);
            flywheel.GetComponent<Renderer>().sharedMaterial = settings.rustMetalMaterial;
            var fc = flywheel.GetComponent<Collider>();
            if (fc) Object.DestroyImmediate(fc);

            // Breakers and fuel lines along wall
            var breaker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            breaker.name = "Watchroom_Breakers";
            breaker.transform.SetParent(watchGroup, false);
            breaker.transform.localPosition = new Vector3(-5.35f, 1.6f, -0.6f);
            breaker.transform.localScale = new Vector3(0.18f, 0.85f, 0.65f);
            breaker.GetComponent<Renderer>().sharedMaterial = settings.rustMetalMaterial;

            var worktable = GameObject.CreatePrimitive(PrimitiveType.Cube);
            worktable.name = "Watchroom_Worktable";
            worktable.transform.SetParent(watchGroup, false);
            worktable.transform.localPosition = new Vector3(-4.2f, 0.45f, -1.2f);
            worktable.transform.localScale = new Vector3(1.4f, 0.85f, 0.7f);
            worktable.GetComponent<Renderer>().sharedMaterial = settings.woodMaterial;

            CreateCagedLantern("Watchroom_Lantern", watchGroup, new Vector3(-4.0f, 2.3f, 0f));
        }

        private static void BuildStairOnlyLevel(Transform parent, InteriorSettings settings, int lvl)
        {
            var levelGroup = new GameObject($"StairLevel_{lvl + 1}_Dressing").transform;
            levelGroup.SetParent(parent, false);

            // Flat slit window on North or South flat wall
            bool isNorth = (lvl % 2 == 0);
            Vector3 winPos = isNorth ? new Vector3(0f, 1.5f, 2.45f) : new Vector3(0f, 1.5f, -2.45f);
            float winAngle = isNorth ? 180f : 0f;

            var win = (GameObject)PrefabUtility.InstantiatePrefab(settings.windowFramePrefab, levelGroup);
            win.name = $"SlitWindow_L{lvl + 1}";
            win.transform.localPosition = winPos;
            win.transform.localRotation = Quaternion.Euler(0f, winAngle, 0f);

            CreateCagedLantern($"Stair_Lantern_L{lvl + 1}", levelGroup, new Vector3(0f, 2.3f, isNorth ? 1.88f : -1.88f));
        }

        private static void BuildLanternRoom(Transform parent, InteriorSettings settings, float deckY, string actName)
        {
            var lanternGroup = new GameObject("Lantern_Room_Chamber").transform;
            lanternGroup.SetParent(parent, false);
            lanternGroup.localPosition = new Vector3(0f, deckY, 0f);

            // 1. Lantern Room Deck Floor (flat octagonal deck, radius 3.6m with open stairwell)
            var deckGroup = new GameObject("Lantern_Deck_Floor").transform;
            deckGroup.SetParent(lanternGroup, false);

            var deckS = GameObject.CreatePrimitive(PrimitiveType.Cube);
            deckS.name = "Deck_South";
            deckS.transform.SetParent(deckGroup, false);
            deckS.transform.localPosition = new Vector3(0f, -0.125f, -2.43f);
            deckS.transform.localScale = new Vector3(7.2f, 0.25f, 2.34f);
            deckS.GetComponent<Renderer>().sharedMaterial = settings.flagstoneMaterial;

            // North Deck: starts north of the 5.0m shaft (z >= 2.50) to leave the Level 6 North Turn Landing open!
            var deckN = GameObject.CreatePrimitive(PrimitiveType.Cube);
            deckN.name = "Deck_North";
            deckN.transform.SetParent(deckGroup, false);
            deckN.transform.localPosition = new Vector3(0f, -0.125f, 3.05f);
            deckN.transform.localScale = new Vector3(7.2f, 0.25f, 1.10f);
            deckN.GetComponent<Renderer>().sharedMaterial = settings.flagstoneMaterial;

            // Center Deck: inside shaft center between flights (x in [-1.25, 1.25], z in [-1.26, 1.26])
            var deckC = GameObject.CreatePrimitive(PrimitiveType.Cube);
            deckC.name = "Deck_Center";
            deckC.transform.SetParent(deckGroup, false);
            deckC.transform.localPosition = new Vector3(0f, -0.125f, 0f);
            deckC.transform.localScale = new Vector3(2.5f, 0.25f, 2.52f);
            deckC.GetComponent<Renderer>().sharedMaterial = settings.flagstoneMaterial;

            // East walkway: east of shaft (x in [2.50, 3.60], z in [-1.26, 2.50]) connecting South Deck to North Deck
            var deckE = GameObject.CreatePrimitive(PrimitiveType.Cube);
            deckE.name = "Deck_East";
            deckE.transform.SetParent(deckGroup, false);
            deckE.transform.localPosition = new Vector3(3.05f, -0.125f, 0.62f);
            deckE.transform.localScale = new Vector3(1.1f, 0.25f, 3.76f);
            deckE.GetComponent<Renderer>().sharedMaterial = settings.flagstoneMaterial;

            // West walkway: west of shaft (x in [-3.60, -2.50], z in [-1.26, 2.50]) connecting South Deck to North Deck
            var deckW = GameObject.CreatePrimitive(PrimitiveType.Cube);
            deckW.name = "Deck_West";
            deckW.transform.SetParent(deckGroup, false);
            deckW.transform.localPosition = new Vector3(-3.05f, -0.125f, 0.62f);
            deckW.transform.localScale = new Vector3(1.1f, 0.25f, 3.76f);
            deckW.GetComponent<Renderer>().sharedMaterial = settings.flagstoneMaterial;

            // 2. Flat-Sided Octagonal Glazed Chamber (8 straight glass panes, no smooth cylinder!)
            int facets = 8;
            float r = 3.6f;
            float glassH = 2.8f;
            for (int i = 0; i < facets; i++)
            {
                float ang = (i / (float)facets) * 360f;
                float rad = ang * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(Mathf.Sin(rad) * r, glassH * 0.5f, Mathf.Cos(rad) * r);

                var pane = (GameObject)PrefabUtility.InstantiatePrefab(settings.windowFramePrefab, lanternGroup);
                pane.name = $"Octagonal_Glass_Pane_{i:00}";
                pane.transform.localPosition = pos;
                pane.transform.localRotation = Quaternion.Euler(0f, ang + 90f, 0f);

                // Strip sills/reveals to keep thin brass/iron astragals
                foreach (var col in pane.GetComponentsInChildren<Collider>())
                {
                    if (col.name.Contains("Sill") || col.name.Contains("Reveal")) Object.DestroyImmediate(col);
                }
            }

            // Outside door to gallery catwalk on South-West facet
            var outsideDoor = (GameObject)PrefabUtility.InstantiatePrefab(settings.doorFramePrefab, lanternGroup);
            outsideDoor.name = "Catwalk_Outside_Door";
            outsideDoor.transform.localPosition = new Vector3(-2.55f, 0f, -2.55f);
            outsideDoor.transform.localRotation = Quaternion.Euler(0f, 45f, 0f);

            // 3. Catwalk & Metal Railing (outside gallery catwalk around perimeter, r in [3.6, 4.5])
            var catwalkRoot = new GameObject("Catwalk_Deck").transform;
            catwalkRoot.SetParent(lanternGroup, false);

            var catN = GameObject.CreatePrimitive(PrimitiveType.Cube);
            catN.name = "Catwalk_North";
            catN.transform.SetParent(catwalkRoot, false);
            catN.transform.localPosition = new Vector3(0f, -0.15f, 4.05f);
            catN.transform.localScale = new Vector3(9.0f, 0.2f, 0.9f);
            catN.GetComponent<Renderer>().sharedMaterial = settings.rustMetalMaterial;

            var catS = GameObject.CreatePrimitive(PrimitiveType.Cube);
            catS.name = "Catwalk_South";
            catS.transform.SetParent(catwalkRoot, false);
            catS.transform.localPosition = new Vector3(0f, -0.15f, -4.05f);
            catS.transform.localScale = new Vector3(9.0f, 0.2f, 0.9f);
            catS.GetComponent<Renderer>().sharedMaterial = settings.rustMetalMaterial;

            var catE = GameObject.CreatePrimitive(PrimitiveType.Cube);
            catE.name = "Catwalk_East";
            catE.transform.SetParent(catwalkRoot, false);
            catE.transform.localPosition = new Vector3(4.05f, -0.15f, 0f);
            catE.transform.localScale = new Vector3(0.9f, 0.2f, 7.2f);
            catE.GetComponent<Renderer>().sharedMaterial = settings.rustMetalMaterial;

            var catW = GameObject.CreatePrimitive(PrimitiveType.Cube);
            catW.name = "Catwalk_West";
            catW.transform.SetParent(catwalkRoot, false);
            catW.transform.localPosition = new Vector3(-4.05f, -0.15f, 0f);
            catW.transform.localScale = new Vector3(0.9f, 0.2f, 7.2f);
            catW.GetComponent<Renderer>().sharedMaterial = settings.rustMetalMaterial;

            // 4. Central Fresnel Lens Pedestal & Placeholder
            var pedestal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pedestal.name = "Fresnel_Lens_Pedestal";
            pedestal.transform.SetParent(lanternGroup, false);
            pedestal.transform.localPosition = new Vector3(0f, 0.45f, 0f);
            pedestal.transform.localScale = new Vector3(1.2f, 0.9f, 1.2f);
            pedestal.GetComponent<Renderer>().sharedMaterial = settings.rustMetalMaterial;
            var pedCol = pedestal.GetComponent<BoxCollider>();
            if (pedCol) pedCol.size = new Vector3(0.7f, 1.0f, 0.7f);

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

            var lamp = GetOrAddComponent<LampController>(lampGo);
            var lantern = GetOrAddComponent<LanternInteract>(lampGo);
            var inventory = Object.FindAnyObjectByType<Inventory>();

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
            crankGo.transform.localPosition = new Vector3(1.8f, 0.45f, -2.0f);
            crankGo.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
            crankGo.transform.localScale = new Vector3(0.8f, 1.0f, 0.8f);
            crankGo.GetComponent<Renderer>().sharedMaterial = settings.rustMetalMaterial;

            var noise = GetOrAddComponent<NoiseEmitter>(crankGo);
            var crank = GetOrAddComponent<CrankStation>(crankGo);
            var crankSo = new SerializedObject(crank);
            crankSo.FindProperty("lamp").objectReferenceValue = lamp;
            crankSo.FindProperty("noiseEmitter").objectReferenceValue = noise;
            crankSo.FindProperty("stormSettings").objectReferenceValue = stormSettings;
            crankSo.ApplyModifiedPropertiesWithoutUndo();

            // 7. Roof Cap over Lantern
            var roofCap = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roofCap.name = "Lantern_Roof_Cap";
            roofCap.transform.SetParent(lanternGroup, false);
            roofCap.transform.localPosition = new Vector3(0f, glassH + 0.15f, 0f);
            roofCap.transform.localScale = new Vector3(7.4f, 0.25f, 7.4f);
            roofCap.GetComponent<Renderer>().sharedMaterial = settings.rustMetalMaterial;
        }

        // =========================================================================
        // STEP 2: KEEPER'S HOUSE (Rectangular, 14 x 8m, 2 storeys, pitched roof)
        // =========================================================================
        private static GameObject BuildKeeperHouse(InteriorSettings settings, string actName)
        {
            var root = new GameObject(KeeperHouseRootName);
            float w = settings.keeperHouseDimensions.x; // 14.0m: x in [-16.5, -2.5]
            float d = settings.keeperHouseDimensions.z; // 8.0m: z in [-9.0, -1.0]
            float h = settings.floorHeight; // 3.2m per floor
            float thick = 0.35f; // thick masonry outer walls
            float partThick = 0.25f; // partition walls

            Vector3 center = new Vector3(-9.5f, 0f, -5.0f);

            // Ground Floor Slab (y = 0)
            var gFloor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gFloor.name = "House_GroundFloor_Slab";
            gFloor.transform.SetParent(root.transform, false);
            gFloor.transform.position = new Vector3(center.x, -0.125f, center.z);
            gFloor.transform.localScale = new Vector3(w, 0.25f, d);
            gFloor.GetComponent<Renderer>().sharedMaterial = settings.flagstoneMaterial;

            // Second Floor Slab (y = 3.2m) with opening for house straight staircase
            var f2FloorGroup = new GameObject("House_UpperFloor_Slab").transform;
            f2FloorGroup.SetParent(root.transform, false);

            // West section (Crane's room & Library): x in [-16.5, -10.0]
            var f2West = GameObject.CreatePrimitive(PrimitiveType.Cube);
            f2West.name = "Floor2_Slab_West";
            f2West.transform.SetParent(f2FloorGroup, false);
            f2West.transform.position = new Vector3(-13.25f, h - 0.125f, -5.0f);
            f2West.transform.localScale = new Vector3(6.5f, 0.25f, d);
            f2West.GetComponent<Renderer>().sharedMaterial = settings.woodMaterial;

            // Corridor section outside Crane's room & Library: x in [-10.0, -8.4] (1.6m wide corridor)
            var f2Corridor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            f2Corridor.name = "Floor2_Slab_Corridor";
            f2Corridor.transform.SetParent(f2FloorGroup, false);
            f2Corridor.transform.position = new Vector3(-9.20f, h - 0.125f, -5.0f);
            f2Corridor.transform.localScale = new Vector3(1.6f, 0.25f, d);
            f2Corridor.GetComponent<Renderer>().sharedMaterial = settings.woodMaterial;

            // Stair North Landing (arrival at top of stairs): spans across x in [-8.6, -6.5], z in [-2.46, -1.0]
            var f2StairTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            f2StairTop.name = "Floor2_Slab_StairTop";
            f2StairTop.transform.SetParent(f2FloorGroup, false);
            f2StairTop.transform.position = new Vector3(-7.55f, h - 0.125f, -1.73f);
            f2StairTop.transform.localScale = new Vector3(2.1f, 0.25f, 1.46f);
            f2StairTop.GetComponent<Renderer>().sharedMaterial = settings.woodMaterial;

            // South Landing above entry: x in [-8.6, -6.5], z in [-9.0, -7.5]
            var f2StairSouth = GameObject.CreatePrimitive(PrimitiveType.Cube);
            f2StairSouth.name = "Floor2_Slab_StairSouth";
            f2StairSouth.transform.SetParent(f2FloorGroup, false);
            f2StairSouth.transform.position = new Vector3(-7.55f, h - 0.125f, -8.25f);
            f2StairSouth.transform.localScale = new Vector3(2.1f, 0.25f, 1.5f);
            f2StairSouth.GetComponent<Renderer>().sharedMaterial = settings.woodMaterial;

            // East section: x in [-6.5, -2.5]
            var f2East = GameObject.CreatePrimitive(PrimitiveType.Cube);
            f2East.name = "Floor2_Slab_East";
            f2East.transform.SetParent(f2FloorGroup, false);
            f2East.transform.position = new Vector3(-4.5f, h - 0.125f, -5.0f);
            f2East.transform.localScale = new Vector3(4.0f, 0.25f, d);
            f2East.GetComponent<Renderer>().sharedMaterial = settings.woodMaterial;

            // Exterior Thick Walls (Ground + Upper = 6.4m total height)
            float totalWallH = h * 2f; // 6.4m
            var extWallsGroup = new GameObject("House_Exterior_Walls").transform;
            extWallsGroup.SetParent(root.transform, false);

            // West Wall (x = -16.5)
            var wallW = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallW.name = "House_Wall_West";
            wallW.transform.SetParent(extWallsGroup, false);
            wallW.transform.position = new Vector3(-16.5f - thick * 0.5f, totalWallH * 0.5f, -5.0f);
            wallW.transform.localScale = new Vector3(thick, totalWallH, d);
            wallW.GetComponent<Renderer>().sharedMaterial = settings.stoneWallMaterial;

            // North Wall (z = -1.0) with cellar door opening at x in [-10.25, -8.75]
            var wallNWest = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallNWest.name = "House_Wall_North_West";
            wallNWest.transform.SetParent(extWallsGroup, false);
            wallNWest.transform.position = new Vector3(-13.51f, totalWallH * 0.5f, -1.0f + thick * 0.5f);
            wallNWest.transform.localScale = new Vector3(6.25f + thick, totalWallH, thick);
            wallNWest.GetComponent<Renderer>().sharedMaterial = settings.stoneWallMaterial;

            var wallNEast = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallNEast.name = "House_Wall_North_East";
            wallNEast.transform.SetParent(extWallsGroup, false);
            wallNEast.transform.position = new Vector3(-5.49f, totalWallH * 0.5f, -1.0f + thick * 0.5f);
            wallNEast.transform.localScale = new Vector3(6.25f + thick, totalWallH, thick);
            wallNEast.GetComponent<Renderer>().sharedMaterial = settings.stoneWallMaterial;

            var wallNUpper = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallNUpper.name = "House_Wall_North_Upper";
            wallNUpper.transform.SetParent(extWallsGroup, false);
            wallNUpper.transform.position = new Vector3(-9.5f, (totalWallH + 2.65f) * 0.5f, -1.0f + thick * 0.5f);
            wallNUpper.transform.localScale = new Vector3(1.5f, totalWallH - 2.65f, thick);
            wallNUpper.GetComponent<Renderer>().sharedMaterial = settings.stoneWallMaterial;

            // South Wall (z = -9.0) with double front door opening at x in [-9.25, -7.75]
            var wallSLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallSLeft.name = "House_Wall_South_West";
            wallSLeft.transform.SetParent(extWallsGroup, false);
            wallSLeft.transform.position = new Vector3(-13.01f, totalWallH * 0.5f, -9.0f - thick * 0.5f);
            wallSLeft.transform.localScale = new Vector3(7.25f + thick, totalWallH, thick);
            wallSLeft.GetComponent<Renderer>().sharedMaterial = settings.stoneWallMaterial;

            var wallSRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallSRight.name = "House_Wall_South_East";
            wallSRight.transform.SetParent(extWallsGroup, false);
            wallSRight.transform.position = new Vector3(-4.99f, totalWallH * 0.5f, -9.0f - thick * 0.5f);
            wallSRight.transform.localScale = new Vector3(5.25f + thick, totalWallH, thick);
            wallSRight.GetComponent<Renderer>().sharedMaterial = settings.stoneWallMaterial;

            var wallSUpper = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallSUpper.name = "House_Wall_South_Upper";
            wallSUpper.transform.SetParent(extWallsGroup, false);
            wallSUpper.transform.position = new Vector3(-8.5f, (totalWallH + 2.5f) * 0.5f, -9.0f - thick * 0.5f);
            wallSUpper.transform.localScale = new Vector3(1.5f, totalWallH - 2.5f, thick);
            wallSUpper.GetComponent<Renderer>().sharedMaterial = settings.stoneWallMaterial;

            // Pitched Roof (Pitched gable roof, ridge at z = -5.0, y = 8.5m, stops at tower wall x = -2.5)
            var roofGroup = new GameObject("House_Pitched_Roof").transform;
            roofGroup.SetParent(root.transform, false);

            float roofWidth = 14.6f;
            float roofCenterX = -9.8f;

            var roofSouth = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roofSouth.name = "Roof_South_Slope";
            roofSouth.transform.SetParent(roofGroup, false);
            roofSouth.transform.position = new Vector3(roofCenterX, 7.3f, -7.0f);
            roofSouth.transform.rotation = Quaternion.Euler(26f, 0f, 0f);
            roofSouth.transform.localScale = new Vector3(roofWidth, 0.25f, 4.6f);
            roofSouth.GetComponent<Renderer>().sharedMaterial = settings.woodMaterial;

            var roofNorth = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roofNorth.name = "Roof_North_Slope";
            roofNorth.transform.SetParent(roofGroup, false);
            roofNorth.transform.position = new Vector3(roofCenterX, 7.3f, -3.0f);
            roofNorth.transform.rotation = Quaternion.Euler(-26f, 0f, 0f);
            roofNorth.transform.localScale = new Vector3(roofWidth, 0.25f, 4.6f);
            roofNorth.GetComponent<Renderer>().sharedMaterial = settings.woodMaterial;

            // Windows tagged "Window"
            BuildHouseWindows(root.transform, settings);

            // Double Front Door at Courtyard (x = -8.5, z = -9.0)
            var frontDoor = GameObject.Find("Keeper House Front Door");
            if (!frontDoor) frontDoor = GameObject.Find("Keeper_House_Front_Door");
            if (!frontDoor)
            {
                frontDoor = (GameObject)PrefabUtility.InstantiatePrefab(settings.doorFramePrefab, root.transform);
                frontDoor.name = "Keeper House Front Door";
            }
            frontDoor.transform.position = new Vector3(-8.5f, 0f, -9.0f);
            frontDoor.transform.rotation = Quaternion.identity;

            // Straight Staircase Up (in entry hall along x = -7.5, climbing South to North from z = -7.5 to z = -2.46)
            BuildHouseStairs(root.transform, settings);

            // Ground Floor Rooms & Dressing
            BuildGroundFloorRooms(root.transform, settings);

            // Upper Floor Rooms & Dressing (Crane's room, bedroom/library, door to tower)
            BuildUpperFloorRooms(root.transform, settings);

            return root;
        }

        private static void BuildHouseWindows(Transform parent, InteriorSettings settings)
        {
            var winGroup = new GameObject("House_Windows").transform;
            winGroup.SetParent(parent, false);

            // Ground Floor Windows
            Vector3[] groundWinPositions = new[]
            {
                new Vector3(-13.5f, 1.4f, -9.0f), // Living room south
                new Vector3(-16.5f, 1.4f, -6.5f), // Living room west
                new Vector3(-16.5f, 1.4f, -3.0f), // Kitchen west
                new Vector3(-13.5f, 1.4f, -1.0f), // Kitchen north
                new Vector3(-5.5f, 1.4f, -9.0f)   // Generator room south
            };

            foreach (var pos in groundWinPositions)
            {
                var win = (GameObject)PrefabUtility.InstantiatePrefab(settings.windowFramePrefab, winGroup);
                win.transform.position = pos;
                win.transform.rotation = pos.z == -9.0f ? Quaternion.identity : (pos.z == -1.0f ? Quaternion.Euler(0, 180, 0) : Quaternion.Euler(0, -90, 0));
            }

            // Upper Floor Windows
            Vector3[] upperWinPositions = new[]
            {
                new Vector3(-13.5f, 4.6f, -9.0f), // Crane's room south
                new Vector3(-16.5f, 4.6f, -5.0f), // Crane's room west
                new Vector3(-13.5f, 4.6f, -1.0f)  // Library north
            };

            foreach (var pos in upperWinPositions)
            {
                var win = (GameObject)PrefabUtility.InstantiatePrefab(settings.windowFramePrefab, winGroup);
                win.transform.position = pos;
                win.transform.rotation = pos.z == -9.0f ? Quaternion.identity : (pos.z == -1.0f ? Quaternion.Euler(0, 180, 0) : Quaternion.Euler(0, -90, 0));
            }
        }

        private static void BuildHouseStairs(Transform parent, InteriorSettings settings)
        {
            var stairGroup = new GameObject("House_Straight_Stairs").transform;
            stairGroup.SetParent(parent, false);

            // Straight flight: 18 steps, rise 0.178, run 0.28 (run = 5.04m, rise = 3.204m)
            // Starts at x = -8.0, z = -7.5, y = 0, climbs South to North to z = -2.46, y = 3.2m
            int count = 18;
            float rise = 0.178f;
            float run = 0.28f;
            float width = 1.2f;
            float totalRise = count * rise;
            float totalRun = count * run;

            Vector3 stairBase = new Vector3(-8.0f, 0f, -7.5f);

            for (int i = 0; i < count; i++)
            {
                float y = (i + 0.5f) * rise;
                float z = (i + 0.5f) * run;

                var step = GameObject.CreatePrimitive(PrimitiveType.Cube);
                step.name = $"HouseStep_{i:00}";
                step.transform.SetParent(stairGroup, false);
                step.transform.position = stairBase + new Vector3(0f, y, z);
                step.transform.localScale = new Vector3(width, rise, run);
                step.GetComponent<Renderer>().sharedMaterial = settings.woodMaterial;
                var sc = step.GetComponent<Collider>();
                if (sc) Object.DestroyImmediate(sc);
            }

            // Continuous ramp collider
            float diagLen = Mathf.Sqrt(totalRise * totalRise + totalRun * totalRun);
            float angleDeg = Mathf.Atan2(totalRise, totalRun) * Mathf.Rad2Deg; // 32.44 degrees
            var rampRot = Quaternion.Euler(-angleDeg, 0f, 0f);
            var rampNormal = rampRot * Vector3.up;

            float rampThick = 0.18f;
            Vector3 rampOffset = -rampNormal * (rampThick * 0.5f);
            var rampObj = new GameObject("HouseStairRampCollider");
            rampObj.transform.SetParent(stairGroup, false);
            rampObj.transform.position = stairBase + new Vector3(0f, totalRise * 0.5f, totalRun * 0.5f) + rampOffset;
            rampObj.transform.localRotation = rampRot;
            var box = rampObj.AddComponent<BoxCollider>();
            box.size = new Vector3(width, rampThick, diagLen);
        }

        private static void BuildGroundFloorRooms(Transform parent, InteriorSettings settings)
        {
            var gGroup = new GameObject("GroundFloor_Rooms").transform;
            gGroup.SetParent(parent, false);

            // Partition Walls with door openings:
            // 1. Dividing Living/Kitchen (West) from Entry/Store (x = -11.5), Door at z = -6.5 (width 1.2m)
            var part1South = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part1South.name = "Part_Living_Entry_South";
            part1South.transform.SetParent(gGroup, false);
            part1South.transform.position = new Vector3(-11.5f, 1.6f, -7.95f);
            part1South.transform.localScale = new Vector3(0.25f, 3.2f, 2.1f);
            part1South.GetComponent<Renderer>().sharedMaterial = settings.plasterMaterial;

            var part1North = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part1North.name = "Part_Living_Entry_North";
            part1North.transform.SetParent(gGroup, false);
            part1North.transform.position = new Vector3(-11.5f, 1.6f, -3.35f);
            part1North.transform.localScale = new Vector3(0.25f, 3.2f, 4.7f);
            part1North.GetComponent<Renderer>().sharedMaterial = settings.plasterMaterial;

            var part1Upper = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part1Upper.name = "Part_Living_Entry_Upper";
            part1Upper.transform.SetParent(gGroup, false);
            part1Upper.transform.position = new Vector3(-11.5f, 2.8f, -6.5f);
            part1Upper.transform.localScale = new Vector3(0.25f, 0.8f, 1.2f);
            part1Upper.GetComponent<Renderer>().sharedMaterial = settings.plasterMaterial;

            // 2. Dividing Living Room (South) from Kitchen (North) at z = -4.5, Door at x = -14.0 (width 1.2m)
            var part2West = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part2West.name = "Part_Living_Kitchen_West";
            part2West.transform.SetParent(gGroup, false);
            part2West.transform.position = new Vector3(-15.55f, 1.6f, -4.5f);
            part2West.transform.localScale = new Vector3(1.9f, 3.2f, 0.25f);
            part2West.GetComponent<Renderer>().sharedMaterial = settings.plasterMaterial;

            var part2East = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part2East.name = "Part_Living_Kitchen_East";
            part2East.transform.SetParent(gGroup, false);
            part2East.transform.position = new Vector3(-12.45f, 1.6f, -4.5f);
            part2East.transform.localScale = new Vector3(1.9f, 3.2f, 0.25f);
            part2East.GetComponent<Renderer>().sharedMaterial = settings.plasterMaterial;

            var part2Upper = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part2Upper.name = "Part_Living_Kitchen_Upper";
            part2Upper.transform.SetParent(gGroup, false);
            part2Upper.transform.position = new Vector3(-14.0f, 2.8f, -4.5f);
            part2Upper.transform.localScale = new Vector3(1.2f, 0.8f, 0.25f);
            part2Upper.GetComponent<Renderer>().sharedMaterial = settings.plasterMaterial;

            // 3. Dividing Entry/Stair (West) from Generator Room (East) at x = -6.5, Door at z = -6.5 (width 1.2m)
            var part3South = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part3South.name = "Part_Entry_Generator_South";
            part3South.transform.SetParent(gGroup, false);
            part3South.transform.position = new Vector3(-6.5f, 1.6f, -7.95f);
            part3South.transform.localScale = new Vector3(0.25f, 3.2f, 2.1f);
            part3South.GetComponent<Renderer>().sharedMaterial = settings.plasterMaterial;

            var part3North = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part3North.name = "Part_Entry_Generator_North";
            part3North.transform.SetParent(gGroup, false);
            part3North.transform.position = new Vector3(-6.5f, 1.6f, -3.35f);
            part3North.transform.localScale = new Vector3(0.25f, 3.2f, 4.7f);
            part3North.GetComponent<Renderer>().sharedMaterial = settings.plasterMaterial;

            var part3Upper = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part3Upper.name = "Part_Entry_Generator_Upper";
            part3Upper.transform.SetParent(gGroup, false);
            part3Upper.transform.position = new Vector3(-6.5f, 2.8f, -6.5f);
            part3Upper.transform.localScale = new Vector3(0.25f, 0.8f, 1.2f);
            part3Upper.GetComponent<Renderer>().sharedMaterial = settings.plasterMaterial;

            // Door openings between rooms
            var d1 = (GameObject)PrefabUtility.InstantiatePrefab(settings.doorFramePrefab, gGroup);
            d1.name = "Door_Entry_To_Living";
            d1.transform.position = new Vector3(-11.5f, 0f, -6.5f);
            d1.transform.rotation = Quaternion.Euler(0, 90, 0);

            var d2 = (GameObject)PrefabUtility.InstantiatePrefab(settings.doorFramePrefab, gGroup);
            d2.name = "Door_Living_To_Kitchen";
            d2.transform.position = new Vector3(-14.0f, 0f, -4.5f);
            d2.transform.rotation = Quaternion.identity;

            var d3 = (GameObject)PrefabUtility.InstantiatePrefab(settings.doorFramePrefab, gGroup);
            d3.name = "Door_Entry_To_Generator";
            d3.transform.position = new Vector3(-6.5f, 0f, -6.5f);
            d3.transform.rotation = Quaternion.Euler(0, 90, 0);

            // A. KEEPER'S LIVING ROOM:
            // Dining Table, Hot Meal with detailed props, Radio, Stove, Chairs
            BuildDetailedMealTable(gGroup, new Vector3(-14.0f, 0f, -6.8f), settings);

            // Radio on sideboard
            var radio = GameObject.Find("Radio");
            if (!radio)
            {
                radio = GameObject.CreatePrimitive(PrimitiveType.Cube);
                radio.name = "Radio";
                radio.AddComponent<Radio>();
            }
            radio.transform.position = new Vector3(-15.8f, 0.85f, -6.8f);
            radio.transform.localScale = new Vector3(0.6f, 0.45f, 0.35f);
            radio.GetComponent<Renderer>().sharedMaterial = settings.rustMetalMaterial;
            if (!radio.GetComponent<Collider>()) radio.AddComponent<BoxCollider>();

            // Cast iron wood stove
            var stove = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stove.name = "Cast_Iron_Stove";
            stove.transform.SetParent(gGroup, false);
            stove.transform.position = new Vector3(-15.8f, 0.6f, -8.2f);
            stove.transform.localScale = new Vector3(0.85f, 1.2f, 0.85f);
            stove.GetComponent<Renderer>().sharedMaterial = settings.rustMetalMaterial;

            // B. KITCHEN / PANTRY:
            // Shelves, kettle
            var kettle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            kettle.name = "Copper_Kettle";
            kettle.transform.SetParent(gGroup, false);
            kettle.transform.position = new Vector3(-15.6f, 0.85f, -2.5f);
            kettle.transform.localScale = new Vector3(0.35f, 0.25f, 0.35f);
            kettle.GetComponent<Renderer>().sharedMaterial = settings.rustMetalMaterial;

            // C. STOREROOM:
            // Reserve Fuel Can, Logbook page, supply crates
            var fuelCan = GameObject.Find("Fuel Can");
            if (fuelCan)
            {
                fuelCan.transform.position = new Vector3(-9.5f, 0.35f, -3.8f);
            }

            // Heavy Arched Cellar Door on North Wall (z = -1.0, x = -9.5)
            BuildHeavyCellarDoor(gGroup, new Vector3(-9.5f, 0f, -1.0f), settings);

            // D. GENERATOR ROOM:
            // Interactable Generator
            var generatorGo = GameObject.Find("Generator");
            if (!generatorGo)
            {
                generatorGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                generatorGo.name = "Generator";
                generatorGo.AddComponent<Generator>();
            }
            generatorGo.transform.position = new Vector3(-4.5f, 0.75f, -4.5f);
            generatorGo.transform.localScale = new Vector3(1.4f, 1.5f, 1.8f);
            generatorGo.GetComponent<Renderer>().sharedMaterial = settings.rustMetalMaterial;
            if (!generatorGo.GetComponent<Collider>()) generatorGo.AddComponent<BoxCollider>();
        }

        private static void BuildDetailedMealTable(Transform parent, Vector3 pos, InteriorSettings settings)
        {
            var mealGroup = new GameObject("Meal_Table_Set").transform;
            mealGroup.SetParent(parent, false);
            mealGroup.position = pos;

            // Wooden dining table
            var table = GameObject.Find("Table");
            if (!table)
            {
                table = GameObject.CreatePrimitive(PrimitiveType.Cube);
                table.name = "Table";
            }
            table.transform.SetParent(mealGroup, false);
            table.transform.localPosition = new Vector3(0f, 0.38f, 0f);
            table.transform.localScale = new Vector3(1.8f, 0.76f, 1.1f);
            table.GetComponent<Renderer>().sharedMaterial = settings.woodMaterial;

            // Hot Meal Root (contains MealTable script & BoxCollider)
            var meal = GameObject.Find("Hot Meal");
            if (!meal)
            {
                meal = new GameObject("Hot Meal");
                meal.AddComponent<MealTable>();
            }
            meal.transform.SetParent(mealGroup, false);
            meal.transform.localPosition = new Vector3(0f, 0.78f, 0f);
            var mealCol = GetOrAddComponent<BoxCollider>(meal);
            mealCol.size = new Vector3(1.2f, 0.45f, 0.8f);
            mealCol.center = new Vector3(0f, 0.15f, 0f);

            // Ceramic dinner plate
            var plate = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            plate.name = "Ceramic_Plate";
            plate.transform.SetParent(meal.transform, false);
            plate.transform.localPosition = new Vector3(-0.25f, 0.02f, 0f);
            plate.transform.localScale = new Vector3(0.35f, 0.025f, 0.35f);
            plate.GetComponent<Renderer>().sharedMaterial = settings.flagstoneMaterial;
            Object.DestroyImmediate(plate.GetComponent<Collider>());

            // Stew bowl
            var bowl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            bowl.name = "Stew_Bowl";
            bowl.transform.SetParent(meal.transform, false);
            bowl.transform.localPosition = new Vector3(-0.25f, 0.06f, 0f);
            bowl.transform.localScale = new Vector3(0.24f, 0.06f, 0.24f);
            bowl.GetComponent<Renderer>().sharedMaterial = settings.flagstoneMaterial;
            Object.DestroyImmediate(bowl.GetComponent<Collider>());

            // Soft steam particles rising from the bowl
            var steamGo = GameObject.Find("Steam placeholder");
            if (!steamGo)
            {
                steamGo = new GameObject("Steam placeholder");
            }
            steamGo.transform.SetParent(bowl.transform, false);
            steamGo.transform.localPosition = new Vector3(0f, 0.08f, 0f);

            var ps = GetOrAddComponent<ParticleSystem>(steamGo);
            var psr = GetOrAddComponent<ParticleSystemRenderer>(steamGo);
            var main = ps.main;
            main.startLifetime = 1.8f;
            main.startSpeed = 0.25f;
            main.startSize = 0.18f;
            main.startColor = new Color(0.85f, 0.88f, 0.90f, 0.12f);
            main.maxParticles = 24;

            var emission = ps.emission;
            emission.rateOverTime = 8f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.06f;

            var pMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Level/Particle.mat");
            if (pMat) psr.sharedMaterial = pMat;

            // Bread loaf
            var loaf = GameObject.CreatePrimitive(PrimitiveType.Cube);
            loaf.name = "Crusty_Bread_Loaf";
            loaf.transform.SetParent(meal.transform, false);
            loaf.transform.localPosition = new Vector3(0.25f, 0.05f, -0.15f);
            loaf.transform.localScale = new Vector3(0.28f, 0.09f, 0.16f);
            loaf.GetComponent<Renderer>().sharedMaterial = settings.woodMaterial;
            Object.DestroyImmediate(loaf.GetComponent<Collider>());

            // Wine / Ale bottle
            var bottle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            bottle.name = "Ale_Bottle";
            bottle.transform.SetParent(meal.transform, false);
            bottle.transform.localPosition = new Vector3(0.35f, 0.14f, 0.20f);
            bottle.transform.localScale = new Vector3(0.09f, 0.14f, 0.09f);
            bottle.GetComponent<Renderer>().sharedMaterial = settings.rustMetalMaterial;
            Object.DestroyImmediate(bottle.GetComponent<Collider>());

            // Brass candlestick with warm light
            var candle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            candle.name = "Candlestick";
            candle.transform.SetParent(meal.transform, false);
            candle.transform.localPosition = new Vector3(0f, 0.10f, 0.22f);
            candle.transform.localScale = new Vector3(0.06f, 0.10f, 0.06f);
            candle.GetComponent<Renderer>().sharedMaterial = settings.rustMetalMaterial;
            Object.DestroyImmediate(candle.GetComponent<Collider>());

            var candleLight = candle.AddComponent<Light>();
            candleLight.type = LightType.Point;
            candleLight.range = 3.5f;
            candleLight.intensity = 1.2f;
            candleLight.color = new Color(1.0f, 0.75f, 0.45f);
        }

        private static void BuildHeavyCellarDoor(Transform parent, Vector3 pos, InteriorSettings settings)
        {
            var doorRoot = new GameObject("Cellar_Doorway_Arched").transform;
            doorRoot.SetParent(parent, false);
            doorRoot.position = pos;

            // Heavy arched timber lintel & jambs
            float w = 1.3f;
            float h = 2.65f;
            float d = 0.45f;

            // Lintel
            var lintel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lintel.name = "Timber_Lintel";
            lintel.transform.SetParent(doorRoot, false);
            lintel.transform.localPosition = new Vector3(0f, h - 0.1f, 0f);
            lintel.transform.localScale = new Vector3(w + 0.3f, 0.22f, d + 0.1f);
            lintel.GetComponent<Renderer>().sharedMaterial = settings.woodMaterial;

            // Jambs
            for (int s = -1; s <= 1; s += 2)
            {
                var jamb = GameObject.CreatePrimitive(PrimitiveType.Cube);
                jamb.name = s < 0 ? "Stone_Jamb_Left" : "Stone_Jamb_Right";
                jamb.transform.SetParent(doorRoot, false);
                jamb.transform.localPosition = new Vector3(s * (w * 0.5f + 0.08f), h * 0.5f, 0f);
                jamb.transform.localScale = new Vector3(0.18f, h, d);
                jamb.GetComponent<Renderer>().sharedMaterial = settings.stoneWallMaterial;
            }

            // Locked Cellar Door Leaf (existing Door component + ActFourGate)
            var cellarDoorGo = GameObject.Find("Locked Cellar Door");
            if (!cellarDoorGo)
            {
                cellarDoorGo = (GameObject)PrefabUtility.InstantiatePrefab(settings.doorFramePrefab, doorRoot);
                cellarDoorGo.name = "Locked Cellar Door";
            }
            cellarDoorGo.transform.SetParent(doorRoot, false);
            cellarDoorGo.transform.localPosition = Vector3.zero;
            cellarDoorGo.transform.localRotation = Quaternion.Euler(0f, 180f, 0f); // Facing into the storeroom

            // Remove threshold board and standard lintel so descending stair treads are completely open under heavy arched lintel
            var threshold = cellarDoorGo.transform.Find("Threshold");
            if (threshold) Object.DestroyImmediate(threshold.gameObject);
            var standardLintel = cellarDoorGo.transform.Find("Lintel");
            if (standardLintel) Object.DestroyImmediate(standardLintel.gameObject);

            var doorComp = GetOrAddComponent<Door>(cellarDoorGo);
            var dSo = new SerializedObject(doorComp);
            dSo.FindProperty("doorId").stringValue = "cellar-door";
            dSo.FindProperty("locked").boolValue = true;
            dSo.FindProperty("turnSpeed").floatValue = 110f;
            dSo.ApplyModifiedPropertiesWithoutUndo();

            // ActFourGate setup
            var gate = Object.FindAnyObjectByType<ActFourGate>();
            if (gate)
            {
                var gSo = new SerializedObject(gate);
                gSo.FindProperty("cellarDoor").objectReferenceValue = doorComp;
                gSo.ApplyModifiedPropertiesWithoutUndo();
            }

            CreateCagedLantern("Cellar_Door_Lantern", doorRoot, new Vector3(0.85f, 2.1f, 0.35f));
        }

        private static void BuildUpperFloorRooms(Transform parent, InteriorSettings settings)
        {
            var uGroup = new GameObject("UpperFloor_Rooms").transform;
            uGroup.SetParent(parent, false);
            uGroup.position = new Vector3(0f, settings.floorHeight, 0f);

            // Partition Wall dividing Crane's Room (South) from Upper Library/Bedroom (North) at z = -4.5
            var part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.name = "Part_CraneRoom_Library";
            part.transform.SetParent(uGroup, false);
            part.transform.position = new Vector3(-13.25f, settings.floorHeight + 1.6f, -4.5f);
            part.transform.localScale = new Vector3(6.5f, 3.2f, 0.25f);
            part.GetComponent<Renderer>().sharedMaterial = settings.plasterMaterial;

            // Partition Wall dividing corridor from Crane's Room at x = -10.0 with door opening at z = -6.5 (width 1.2m)
            var partCorridorSouth = GameObject.CreatePrimitive(PrimitiveType.Cube);
            partCorridorSouth.name = "Part_CraneRoom_Corridor_South";
            partCorridorSouth.transform.SetParent(uGroup, false);
            partCorridorSouth.transform.position = new Vector3(-10.0f, settings.floorHeight + 1.6f, -8.05f);
            partCorridorSouth.transform.localScale = new Vector3(0.25f, 3.2f, 1.9f);
            partCorridorSouth.GetComponent<Renderer>().sharedMaterial = settings.plasterMaterial;

            var partCorridorNorth = GameObject.CreatePrimitive(PrimitiveType.Cube);
            partCorridorNorth.name = "Part_CraneRoom_Corridor_North";
            partCorridorNorth.transform.SetParent(uGroup, false);
            partCorridorNorth.transform.position = new Vector3(-10.0f, settings.floorHeight + 1.6f, -5.20f);
            partCorridorNorth.transform.localScale = new Vector3(0.25f, 3.2f, 1.4f);
            partCorridorNorth.GetComponent<Renderer>().sharedMaterial = settings.plasterMaterial;

            var partCorridorUpper = GameObject.CreatePrimitive(PrimitiveType.Cube);
            partCorridorUpper.name = "Part_CraneRoom_Corridor_Upper";
            partCorridorUpper.transform.SetParent(uGroup, false);
            partCorridorUpper.transform.position = new Vector3(-10.0f, settings.floorHeight + 2.8f, -6.5f);
            partCorridorUpper.transform.localScale = new Vector3(0.25f, 0.8f, 1.2f);
            partCorridorUpper.GetComponent<Renderer>().sharedMaterial = settings.plasterMaterial;

            // Decorative stairwell safety rail along corridor edge
            var houseUpperRail = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            houseUpperRail.name = "House_Upper_Stair_Rail";
            houseUpperRail.transform.SetParent(uGroup, false);
            houseUpperRail.transform.position = new Vector3(-8.45f, settings.floorHeight + 0.9f, -4.98f);
            houseUpperRail.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            houseUpperRail.transform.localScale = new Vector3(0.06f, 2.52f, 0.06f);
            houseUpperRail.GetComponent<Renderer>().sharedMaterial = settings.rustMetalMaterial;
            var rc = houseUpperRail.GetComponent<Collider>();
            if (rc) Object.DestroyImmediate(rc);

            // A. CRANE'S ROOM (x in [-16.5, -10.0], z in [-9.0, -4.5]):
            // Door into Crane's room (LockedRoomDoor, CraneRoomKey)
            var craneDoor = GameObject.Find("Crane's Room Door");
            if (!craneDoor)
            {
                craneDoor = (GameObject)PrefabUtility.InstantiatePrefab(settings.doorFramePrefab, uGroup);
                craneDoor.name = "Crane's Room Door";
            }
            craneDoor.transform.position = new Vector3(-10.0f, settings.floorHeight, -6.5f);
            craneDoor.transform.rotation = Quaternion.Euler(0, -90, 0);

            var lockedDoor = GetOrAddComponent<LockedRoomDoor>(craneDoor);
            var player = Object.FindAnyObjectByType<FirstPersonController>();
            if (player)
            {
                var inv = player.GetComponent<Inventory>();
                var so = new SerializedObject(lockedDoor);
                so.FindProperty("inventory").objectReferenceValue = inv;
                so.FindProperty("keyId").stringValue = "CraneRoomKey";
                so.FindProperty("doorId").stringValue = "crane-room-door";
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            // Crane's Desk with last logbook entry
            var desk = GameObject.Find("Crane's Desk");
            if (!desk) desk = GameObject.CreatePrimitive(PrimitiveType.Cube);
            desk.name = "Crane's Desk";
            desk.transform.SetParent(uGroup, false);
            desk.transform.position = new Vector3(-14.5f, settings.floorHeight + 0.38f, -7.5f);
            desk.transform.localScale = new Vector3(1.6f, 0.76f, 0.9f);
            desk.GetComponent<Renderer>().sharedMaterial = settings.woodMaterial;

            // Family Photograph with inspection pose (stays in Crane's room!)
            var photoGo = GameObject.Find("Family Photo");
            if (!photoGo) photoGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            photoGo.name = "Family Photo";
            photoGo.transform.SetParent(uGroup, false);
            photoGo.transform.position = new Vector3(-14.5f, settings.floorHeight + 1.25f, -7.1f);
            photoGo.transform.localScale = new Vector3(0.35f, 0.45f, 0.04f);
            photoGo.GetComponent<Renderer>().sharedMaterial = settings.paperMaterial;

            var photoScript = GetOrAddComponent<FamilyPhoto>(photoGo);
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

            // Bed, Bookshelves, Non-interactive Radio Prop
            var bed = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bed.name = "Crane_Bed";
            bed.transform.SetParent(uGroup, false);
            bed.transform.position = new Vector3(-15.5f, settings.floorHeight + 0.35f, -5.5f);
            bed.transform.localScale = new Vector3(1.1f, 0.7f, 2.0f);
            bed.GetComponent<Renderer>().sharedMaterial = settings.woodMaterial;

            var craneRadio = GameObject.CreatePrimitive(PrimitiveType.Cube);
            craneRadio.name = "Crane_Radio_Prop";
            craneRadio.transform.SetParent(uGroup, false);
            craneRadio.transform.position = new Vector3(-13.4f, settings.floorHeight + 0.85f, -7.5f);
            craneRadio.transform.localScale = new Vector3(0.55f, 0.42f, 0.35f);
            craneRadio.GetComponent<Renderer>().sharedMaterial = settings.rustMetalMaterial;

            CreateCagedLantern("Crane_Room_Lantern", uGroup, new Vector3(-14.5f, 2.3f, -7.0f));

            // B. UPPER BEDROOM / LIBRARY (x in [-16.5, -9.5], z in [-4.5, -1.0])
            var libShelf = GameObject.CreatePrimitive(PrimitiveType.Cube);
            libShelf.name = "Library_Bookshelf";
            libShelf.transform.SetParent(uGroup, false);
            libShelf.transform.position = new Vector3(-13.5f, settings.floorHeight + 1.2f, -1.3f);
            libShelf.transform.localScale = new Vector3(3.2f, 2.2f, 0.45f);
            libShelf.GetComponent<Renderer>().sharedMaterial = settings.woodMaterial;

            CreateCagedLantern("Library_Lantern", uGroup, new Vector3(-13.5f, 2.3f, -2.8f));

            // C. CONNECTING PASSAGE / DOOR TO TOWER FLOOR 2 (at x = -2.5, z = -1.8)
            // Bridge covered corridor bridging house upper floor (x = -6.5 to -2.5)
            var bridge = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bridge.name = "Bridge_To_Tower_Floor2";
            bridge.transform.SetParent(uGroup, false);
            bridge.transform.position = new Vector3(-4.5f, settings.floorHeight - 0.125f, -1.8f);
            bridge.transform.localScale = new Vector3(4.0f, 0.25f, 1.6f);
            bridge.GetComponent<Renderer>().sharedMaterial = settings.woodMaterial;
        }

        // =========================================================================
        // STEP 3: UNDERGROUND ENTRY & CELLAR (Behind cellar door, descends 5m)
        // =========================================================================
        private static GameObject BuildCellarAndHiddenRoom(InteriorSettings settings, string actName)
        {
            var root = new GameObject(CellarRootName);

            // Locked Cellar Door is located in Storeroom on North Wall at (-9.5, 0, -1.0)
            Vector3 stairTop = new Vector3(-9.5f, 0f, -1.0f);

            // Straight stair descending 5m: starts at stairTop and descends to y = -5.0m
            // Rise = 5.0m, Run = 7.5m, Width = 1.2m
            float descentH = settings.cellarDepth; // 5.0m
            float stairRun = 7.5f;
            int stepCount = 28;
            float stepRise = descentH / stepCount;
            float stepRun = stairRun / stepCount;

            var stairGroup = new GameObject("Cellar_Descent_Stair").transform;
            stairGroup.SetParent(root.transform, false);
            stairGroup.position = stairTop;

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

            // Continuous ramp collider descending at ~33.69 degrees
            float diagLen = Mathf.Sqrt(descentH * descentH + stairRun * stairRun);
            float angleDeg = Mathf.Atan2(descentH, stairRun) * Mathf.Rad2Deg;
            var rampRot = Quaternion.Euler(angleDeg, 0f, 0f);
            var rampNormal = rampRot * Vector3.up;

            float boxThick = 0.20f;
            Vector3 rampOffset = -rampNormal * (boxThick * 0.5f);
            var rampObj = new GameObject("CellarRampCollider");
            rampObj.transform.SetParent(stairGroup, false);
            rampObj.transform.localPosition = new Vector3(0f, -descentH * 0.5f, stairRun * 0.5f) + rampOffset;
            rampObj.transform.localRotation = rampRot;
            var rampCol = rampObj.AddComponent<BoxCollider>();
            rampCol.size = new Vector3(settings.stairWidth, boxThick, diagLen);

            // Corridor starts at base of stair: (-9.5, -5.0, 6.5)
            Vector3 corridorStart = stairTop + new Vector3(0f, -descentH, stairRun);

            // Vaulted corridor: 2.8m wide, 2.6m high, ~20m long
            int segmentCount = 10;
            float segLen = 2.0f;
            var corridorGroup = new GameObject("Vaulted_Corridor_20m").transform;
            corridorGroup.SetParent(root.transform, false);
            corridorGroup.position = corridorStart;

            for (int i = 0; i < segmentCount; i++)
            {
                var seg = (GameObject)PrefabUtility.InstantiatePrefab(settings.vaultSegmentPrefab, corridorGroup);
                seg.name = $"VaultSegment_{i:00}";
                seg.transform.localPosition = new Vector3(0f, 0f, (i + 0.5f) * segLen);
                seg.transform.localRotation = Quaternion.identity;
            }

            // Props along corridor: conduit & cables, barrels, drums, crates, warm caged lantern
            BuildCorridorProps(corridorGroup, segmentCount, segLen, settings);

            // WetFootprints leading INTO the cellar, none leading out
            SetupWetFootprints(corridorGroup, segmentCount * segLen);

            // CellarHum audio zone
            SetupCellarHum(corridorGroup);

            // Hidden room at end of corridor (behind sliding barrel stack)
            float roomHalfDepth = settings.hiddenRoomDimensions.y * 0.5f;
            Vector3 hiddenRoomCenter = corridorStart + new Vector3(0f, 0f, segmentCount * segLen + roomHalfDepth + 0.125f);
            BuildHiddenCellarRoom(root.transform, hiddenRoomCenter, settings, actName);

            return root;
        }

        private static void BuildCorridorProps(Transform parent, int count, float segLen, InteriorSettings settings)
        {
            var propsGroup = new GameObject("Corridor_Props").transform;
            propsGroup.SetParent(parent, false);

            var conduit = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            conduit.name = "WallConduit";
            conduit.transform.SetParent(propsGroup, false);
            conduit.transform.localPosition = new Vector3(-1.32f, 1.85f, (count * segLen) * 0.5f);
            conduit.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            conduit.transform.localScale = new Vector3(0.05f, (count * segLen) * 0.5f, 0.05f);
            conduit.GetComponent<Renderer>().sharedMaterial = settings.rustMetalMaterial;
            var cc = conduit.GetComponent<Collider>();
            if (cc) Object.DestroyImmediate(cc);

            for (int i = 1; i < count - 1; i++)
            {
                float z = i * segLen;
                float side = (i % 2 == 0) ? 1.05f : -1.05f;

                if (i % 3 == 0)
                {
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
                    var drum = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    drum.name = $"Drum_{i}";
                    drum.transform.SetParent(propsGroup, false);
                    drum.transform.localPosition = new Vector3(side, 0.45f, z);
                    drum.transform.localScale = new Vector3(0.55f, 0.45f, 0.55f);
                    drum.GetComponent<Renderer>().sharedMaterial = (i % 2 == 0) ? settings.rustMetalMaterial : settings.woodMaterial;
                }

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
            printsGo.transform.localRotation = Quaternion.identity;
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
            var box = GetOrAddComponent<BoxCollider>(humGo);
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
            var northWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            northWall.name = "HiddenRoom_WallNorth";
            northWall.transform.SetParent(hiddenRoot, false);
            northWall.transform.localPosition = new Vector3(0f, h * 0.5f, d * 0.5f + thick * 0.5f);
            northWall.transform.localScale = new Vector3(w + thick * 2f, h, thick);
            northWall.GetComponent<Renderer>().sharedMaterial = settings.stoneWallMaterial;

            var eastWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            eastWall.name = "HiddenRoom_WallEast";
            eastWall.transform.SetParent(hiddenRoot, false);
            eastWall.transform.localPosition = new Vector3(w * 0.5f + thick * 0.5f, h * 0.5f, 0f);
            eastWall.transform.localScale = new Vector3(thick, h, d);
            eastWall.GetComponent<Renderer>().sharedMaterial = settings.stoneWallMaterial;

            var westWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            westWall.name = "HiddenRoom_WallWest";
            westWall.transform.SetParent(hiddenRoot, false);
            westWall.transform.localPosition = new Vector3(-w * 0.5f - thick * 0.5f, h * 0.5f, 0f);
            westWall.transform.localScale = new Vector3(thick, h, d);
            westWall.GetComponent<Renderer>().sharedMaterial = settings.stoneWallMaterial;

            // South Partition Wall with opening for sliding barrel stack
            var southWallLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
            southWallLeft.name = "HiddenRoom_WallSouth_Left";
            southWallLeft.transform.SetParent(hiddenRoot, false);
            southWallLeft.transform.localPosition = new Vector3(-w * 0.5f + 0.6f, h * 0.5f, -d * 0.5f - thick * 0.5f);
            southWallLeft.transform.localScale = new Vector3(1.2f, h, thick);
            southWallLeft.GetComponent<Renderer>().sharedMaterial = settings.stoneWallMaterial;

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
            var shelfMod = GetOrAddComponent<Unity.AI.Navigation.NavMeshModifier>(shelf);
            shelfMod.ignoreFromBuild = true;

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

            var hiddenComp = GetOrAddComponent<HiddenCellarRoom>(shelf);
            var noiseEmitter = GetOrAddComponent<NoiseEmitter>(shelf);
            var act4Settings = AssetDatabase.LoadAssetAtPath<ActFourSettings>("Assets/_Project/Settings/ActFourSettings.asset");
            var hSo = new SerializedObject(hiddenComp);
            hSo.FindProperty("shelf").objectReferenceValue = shelf.transform;
            hSo.FindProperty("settings").objectReferenceValue = act4Settings;
            hSo.ApplyModifiedPropertiesWithoutUndo();

            // Inside Hidden Room:
            // 1. Recorded Transmitter (Marta reel-to-reel recorder with ReelRotator)
            var recGo = GameObject.Find("Recorded Transmitter");
            if (!recGo) recGo = GameObject.Find("Marta reel-to-reel recorder");
            if (!recGo)
            {
                recGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                recGo.name = "Recorded Transmitter";
                recGo.AddComponent<RecordedTransmitter>();
            }
            recGo.transform.SetParent(hiddenRoot, false);
            recGo.transform.localPosition = new Vector3(-1.1f, 0.85f, 1.0f);
            recGo.transform.localScale = new Vector3(0.65f, 0.45f, 0.45f);
            recGo.GetComponent<Renderer>().sharedMaterial = settings.rustMetalMaterial;

            // Add ReelRotator script to simulate turning tape reels
            var leftReel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            leftReel.name = "Left_Tape_Reel";
            leftReel.transform.SetParent(recGo.transform, false);
            leftReel.transform.localPosition = new Vector3(-0.2f, 0.15f, 0.52f);
            leftReel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            leftReel.transform.localScale = new Vector3(0.25f, 0.02f, 0.25f);
            leftReel.GetComponent<Renderer>().sharedMaterial = settings.rustMetalMaterial;
            Object.DestroyImmediate(leftReel.GetComponent<Collider>());

            var rightReel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rightReel.name = "Right_Tape_Reel";
            rightReel.transform.SetParent(recGo.transform, false);
            rightReel.transform.localPosition = new Vector3(0.2f, 0.15f, 0.52f);
            rightReel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            rightReel.transform.localScale = new Vector3(0.25f, 0.02f, 0.25f);
            rightReel.GetComponent<Renderer>().sharedMaterial = settings.rustMetalMaterial;
            Object.DestroyImmediate(rightReel.GetComponent<Collider>());

            var reelRot = GetOrAddComponent<ReelRotator>(recGo);
            var rSo = new SerializedObject(reelRot);
            rSo.FindProperty("leftReel").objectReferenceValue = leftReel.transform;
            rSo.FindProperty("rightReel").objectReferenceValue = rightReel.transform;
            rSo.ApplyModifiedPropertiesWithoutUndo();

            // 2. Glowing tube radios with faint emissive glow
            var radioRack = GameObject.CreatePrimitive(PrimitiveType.Cube);
            radioRack.name = "Tube_Radios_Rack";
            radioRack.transform.SetParent(hiddenRoot, false);
            radioRack.transform.localPosition = new Vector3(-1.4f, 1.1f, 0f);
            radioRack.transform.localScale = new Vector3(0.45f, 1.4f, 1.6f);
            radioRack.GetComponent<Renderer>().sharedMaterial = settings.rustMetalMaterial;

            // 3. Workbench
            var bench = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bench.name = "Hidden_Workbench";
            bench.transform.SetParent(hiddenRoot, false);
            bench.transform.localPosition = new Vector3(0f, 0.42f, 1.2f);
            bench.transform.localScale = new Vector3(2.2f, 0.84f, 0.9f);
            bench.GetComponent<Renderer>().sharedMaterial = settings.woodMaterial;

            // 4. Maritime Charts with red Marguerite wreck lines
            var chart = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chart.name = "Marguerite_Maritime_Charts";
            chart.transform.SetParent(hiddenRoot, false);
            chart.transform.localPosition = new Vector3(0.4f, 0.86f, 1.2f);
            chart.transform.localScale = new Vector3(0.85f, 0.02f, 0.65f);
            chart.GetComponent<Renderer>().sharedMaterial = settings.paperMaterial;

            // 5. Existing Ledger under a brass desk lamp
            var ledgerGo = GameObject.Find("Ledger") ?? GameObject.Find("Harbor board ledger");
            if (!ledgerGo)
            {
                ledgerGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                ledgerGo.name = "Ledger";
                ledgerGo.AddComponent<Ledger>();
            }
            ledgerGo.transform.SetParent(hiddenRoot, false);
            ledgerGo.transform.localPosition = new Vector3(-0.4f, 0.86f, 1.2f);
            ledgerGo.transform.localScale = new Vector3(0.45f, 0.05f, 0.35f);
            ledgerGo.GetComponent<Renderer>().sharedMaterial = settings.paperMaterial;

            var lampGo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            lampGo.name = "Desk_Lamp";
            lampGo.transform.SetParent(hiddenRoot, false);
            lampGo.transform.localPosition = new Vector3(-0.8f, 1.05f, 1.2f);
            lampGo.transform.localScale = new Vector3(0.18f, 0.25f, 0.18f);
            lampGo.GetComponent<Renderer>().sharedMaterial = settings.rustMetalMaterial;
            var dLight = lampGo.AddComponent<Light>();
            dLight.type = LightType.Point;
            dLight.range = 3.5f;
            dLight.intensity = 1.4f;
            dLight.color = new Color(1.0f, 0.85f, 0.6f);

            // 6. Photograph of the SHIP Marguerite (Ship photo stays in hidden room!)
            var shipPhoto = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shipPhoto.name = "Marguerite_Ship_Photo";
            shipPhoto.transform.SetParent(hiddenRoot, false);
            shipPhoto.transform.localPosition = new Vector3(0f, 1.5f, 1.95f);
            shipPhoto.transform.localScale = new Vector3(0.55f, 0.40f, 0.03f);
            shipPhoto.GetComponent<Renderer>().sharedMaterial = settings.paperMaterial;
        }

        private static void CreateCagedLantern(string name, Transform parent, Vector3 localPos)
        {
            var lantern = new GameObject(name);
            lantern.transform.SetParent(parent, false);
            lantern.transform.localPosition = localPos;

            var bracket = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bracket.name = "Bracket";
            bracket.transform.SetParent(lantern.transform, false);
            bracket.transform.localScale = new Vector3(0.08f, 0.25f, 0.15f);
            var bMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Interior/RustMetal.mat");
            if (bMat) bracket.GetComponent<Renderer>().sharedMaterial = bMat;
            var bc = bracket.GetComponent<Collider>();
            if (bc) Object.DestroyImmediate(bc);

            var cage = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cage.name = "Cage";
            cage.transform.SetParent(lantern.transform, false);
            cage.transform.localPosition = new Vector3(0f, -0.15f, 0.12f);
            cage.transform.localScale = new Vector3(0.16f, 0.18f, 0.16f);
            if (bMat) cage.GetComponent<Renderer>().sharedMaterial = bMat;
            var cc = cage.GetComponent<Collider>();
            if (cc) Object.DestroyImmediate(cc);

            var light = lantern.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 7.0f;
            light.intensity = 1.5f;
            light.color = new Color(1.0f, 0.65f, 0.30f); // Warm caged amber glow
            light.shadows = LightShadows.None;
            light.lightmapBakeType = LightmapBakeType.Baked;
        }

        // =========================================================================
        // HIDING SPOTS (At least 8 across complex)
        // =========================================================================
        private static void SetupHidingSpots(GameObject towerRoot, GameObject houseRoot, GameObject cellarRoot)
        {
            var hidingDefs = new (string id, Vector3 pos, Transform parent)[]
            {
                ("keeper-wardrobe", new Vector3(-13.0f, 0f, -8.5f), houseRoot.transform),
                ("pantry-cupboard", new Vector3(-15.8f, 0f, -3.5f), houseRoot.transform),
                ("generator-locker", new Vector3(-3.2f, 0f, -3.5f), houseRoot.transform),
                ("boat-shed-crate", new Vector3(6.5f, 0f, -15.5f), null),
                ("vestibule-nook", new Vector3(1.5f, 0f, 0.8f), towerRoot.transform),
                ("crane-wardrobe", new Vector3(-15.8f, 3.2f, -8.2f), houseRoot.transform),
                ("watchroom-alcove", new Vector3(-4.8f, 6.4f, -1.2f), towerRoot.transform),
                ("lantern-cabinet", new Vector3(1.8f, 19.2f, -1.8f), towerRoot.transform),
                ("cellar-coal-bin", new Vector3(-8.3f, -5.0f, 15.0f), cellarRoot.transform),
                ("hidden-room-crate", new Vector3(-10.8f, -5.0f, 29.5f), cellarRoot.transform)
            };

            foreach (var def in hidingDefs)
            {
                var spotGo = GameObject.Find(def.id);
                if (!spotGo)
                {
                    spotGo = new GameObject(def.id);
                    spotGo.AddComponent<HidingSpot>();
                }

                if (def.parent != null) spotGo.transform.SetParent(def.parent, true);
                spotGo.transform.position = def.pos;

                var box = GetOrAddComponent<BoxCollider>(spotGo);
                box.isTrigger = true;
                box.size = new Vector3(1.2f, 1.8f, 1.2f);
                box.center = new Vector3(0f, 0.9f, 0f);

                var mod = GetOrAddComponent<Unity.AI.Navigation.NavMeshModifier>(spotGo);
                mod.ignoreFromBuild = true;

                var spot = GetOrAddComponent<HidingSpot>(spotGo);
                var so = new SerializedObject(spot);
                var spotIdProp = so.FindProperty("spotId");
                if (spotIdProp != null) spotIdProp.stringValue = def.id;
                var promptProp = so.FindProperty("prompt");
                if (promptProp != null) promptProp.stringValue = $"Hide in {def.id.Replace('-', ' ')}";
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        // =========================================================================
        // LOGBOOK PICKUPS (6 pages by ID)
        // =========================================================================
        private static void SetupLogbookPages(string actName)
        {
            var pageDefs = new (string id, Vector3 pos)[]
            {
                ("marguerite-wreck", new Vector3(-9.5f, 0.85f, -3.8f)), // Storeroom shelf
                ("wrecker-tally", new Vector3(8.0f, 0.85f, -15.5f)),   // Boat shed
                ("lighthouse-leash", new Vector3(0.5f, 0.65f, 0.8f)),   // Vestibule tool bench
                ("keeper-selection", new Vector3(-9.5f, -4.1f, 29.5f)), // Hidden room workbench
                ("crane-observation", new Vector3(-13.5f, 4.0f, -2.5f)),// Upper library desk
                ("crane-final-entry", new Vector3(-14.5f, 4.0f, -7.5f)) // Crane's desk upstairs
            };

            var journal = UnityEngine.Object.FindAnyObjectByType<LogbookJournal>();

            foreach (var def in pageDefs)
            {
                string pageAssetPath = $"Assets/_Project/LogbookPages/{def.id}.asset";
                var pageAsset = AssetDatabase.LoadAssetAtPath<LogbookPage>(pageAssetPath);
                if (!pageAsset)
                {
                    pageAsset = AssetDatabase.LoadAssetAtPath<LogbookPage>($"Assets/_Project/Settings/Logbook/{def.id}.asset");
                }

                string pickupName = $"Logbook_Pickup_{def.id}";
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

                var mod = GetOrAddComponent<Unity.AI.Navigation.NavMeshModifier>(pickupGo);
                mod.ignoreFromBuild = true;

                var pickup = GetOrAddComponent<LogbookPickup>(pickupGo);
                var so = new SerializedObject(pickup);
                so.FindProperty("page").objectReferenceValue = pageAsset;
                so.FindProperty("journal").objectReferenceValue = journal;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        // =========================================================================
        // NAVMESH LINKS
        // =========================================================================
        private static void SetupNavMeshLinks(GameObject towerRoot, GameObject houseRoot, GameObject cellarRoot)
        {
            var linksGroup = new GameObject("NavMesh_Links").transform;
            linksGroup.SetParent(towerRoot.transform, false);

            var settings = InteriorKitBuilder.EnsureSettings();
            int totalLevels = 3 + settings.stairOnlyLevels;

            // Tower switchback stair links
            for (int lvl = 0; lvl < totalLevels; lvl++)
            {
                float floorY = lvl * settings.floorHeight;

                // Flight A link (East up)
                var linkAGo = new GameObject($"Tower_FlightA_L{lvl + 1}_NavMeshLink");
                linkAGo.transform.SetParent(linksGroup, false);
                var linkA = linkAGo.AddComponent<Unity.AI.Navigation.NavMeshLink>();
                linkA.startPoint = new Vector3(1.90f, floorY + 0.1f, -1.40f);
                linkA.endPoint = new Vector3(1.90f, floorY + 1.602f + 0.1f, 1.40f);
                linkA.width = settings.stairWidth;
                linkA.bidirectional = true;

                // Flight B link (West up)
                var linkBGo = new GameObject($"Tower_FlightB_L{lvl + 1}_NavMeshLink");
                linkBGo.transform.SetParent(linksGroup, false);
                var linkB = linkBGo.AddComponent<Unity.AI.Navigation.NavMeshLink>();
                linkB.startPoint = new Vector3(-1.90f, floorY + 1.602f + 0.1f, 1.40f);
                linkB.endPoint = new Vector3(-1.90f, floorY + settings.floorHeight + 0.1f, -1.40f);
                linkB.width = settings.stairWidth;
                linkB.bidirectional = true;
            }

            // House straight stairs link
            var houseStairLinkGo = new GameObject("House_Stairs_NavMeshLink");
            houseStairLinkGo.transform.SetParent(houseRoot.transform, false);
            var hLink = houseStairLinkGo.AddComponent<Unity.AI.Navigation.NavMeshLink>();
            hLink.startPoint = new Vector3(-8.0f, 0.1f, -7.5f);
            hLink.endPoint = new Vector3(-8.0f, 3.3f, -2.46f);
            hLink.width = 1.2f;
            hLink.bidirectional = true;

            // Crane's Room Doorway link (Corridor to Crane's Room)
            var craneDoorLinkGo = new GameObject("Crane_Door_NavMeshLink");
            craneDoorLinkGo.transform.SetParent(houseRoot.transform, false);
            var cLink = craneDoorLinkGo.AddComponent<Unity.AI.Navigation.NavMeshLink>();
            cLink.startPoint = new Vector3(-9.20f, settings.floorHeight + 0.1f, -6.5f);
            cLink.endPoint = new Vector3(-10.8f, settings.floorHeight + 0.1f, -6.5f);
            cLink.width = 1.2f;
            cLink.bidirectional = true;

            // Cellar descent stair link
            var cellarLinkGo = new GameObject("Cellar_Stair_NavMeshLink");
            cellarLinkGo.transform.SetParent(cellarRoot.transform, false);
            var cellarLink = cellarLinkGo.AddComponent<Unity.AI.Navigation.NavMeshLink>();
            cellarLink.startPoint = new Vector3(-9.5f, 0.1f, -1.0f);
            cellarLink.endPoint = new Vector3(-9.5f, -4.9f, 6.5f);
            cellarLink.width = settings.stairWidth;
            cellarLink.bidirectional = true;

            // Hidden room doorway link
            var secretLinkGo = new GameObject("Secret_Room_NavMeshLink");
            secretLinkGo.transform.SetParent(cellarRoot.transform, false);
            var secretLink = secretLinkGo.AddComponent<Unity.AI.Navigation.NavMeshLink>();
            secretLink.startPoint = new Vector3(-9.5f, -4.9f, 25.5f);
            secretLink.endPoint = new Vector3(-9.5f, -4.9f, 27.5f);
            secretLink.width = 1.6f;
            secretLink.bidirectional = true;

            // House to Tower Ground Floor link (Generator room to Vestibule)
            var houseTowerGroundLinkGo = new GameObject("House_Tower_Ground_NavMeshLink");
            houseTowerGroundLinkGo.transform.SetParent(houseRoot.transform, false);
            var gLink = houseTowerGroundLinkGo.AddComponent<Unity.AI.Navigation.NavMeshLink>();
            gLink.startPoint = new Vector3(-2.8f, 0.1f, -1.8f);
            gLink.endPoint = new Vector3(-2.2f, 0.1f, -1.8f);
            gLink.width = 1.2f;
            gLink.bidirectional = true;

            // House to Tower Upper Floor bridge link (House upper floor to Tower Floor 2)
            var houseTowerUpperLinkGo = new GameObject("House_Tower_Upper_NavMeshLink");
            houseTowerUpperLinkGo.transform.SetParent(houseRoot.transform, false);
            var uLink = houseTowerUpperLinkGo.AddComponent<Unity.AI.Navigation.NavMeshLink>();
            uLink.startPoint = new Vector3(-2.8f, settings.floorHeight + 0.1f, -1.8f);
            uLink.endPoint = new Vector3(-2.2f, settings.floorHeight + 0.1f, -1.8f);
            uLink.width = 1.2f;
            uLink.bidirectional = true;
        }

        private static void SetStaticGeometryFlags(GameObject root)
        {
            if (!root) return;
            foreach (var r in root.GetComponentsInChildren<MeshRenderer>(true))
            {
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

        private static void ConfigurePlayerAndNavMesh()
        {
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

            var navSurface = Object.FindAnyObjectByType<NavMeshSurface>();
            if (navSurface)
            {
                navSurface.agentTypeID = 0;
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
    }
}
