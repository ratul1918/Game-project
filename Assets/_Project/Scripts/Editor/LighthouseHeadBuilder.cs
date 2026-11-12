using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace TheLightHouse.Editor
{
    public static class LighthouseHeadBuilder
    {
        private const string MeshFolder = "Assets/_Project/Art/LighthouseHead/Meshes";
        private const string RootName = "Lighthouse Tower Head";

        private const float TowerShaftTopY = 25.60f;
        private const float TowerShaftOuterR = 4.80f;
        private const float GalleryDeckRadius = 5.85f;
        private const float RailingRadius = 5.70f;
        private const float LanternGlassRadius = 4.25f;

        private static Material matStone;
        private static Material matIron;
        private static Material matCopper;
        private static Material matBrass;
        private static Material matGlass;
        private static Material matBeam;

        [MenuItem("TheLightHouse/Environment/Build Lighthouse Head (Active Scene)")]
        public static void BuildHeadForActiveScene()
        {
            EnsureMaterials();
            EnsureMeshFolder();

            var activeScene = SceneManager.GetActiveScene();
            BuildHeadInScene(activeScene);
            EditorSceneManager.MarkSceneDirty(activeScene);
            Debug.Log($"[LighthouseHeadBuilder] Successfully constructed complete lighthouse tower head in scene: {activeScene.name}");
        }

        [MenuItem("TheLightHouse/Environment/Build Lighthouse Head (All Acts)")]
        public static void ApplyToAllActs()
        {
            EnsureMaterials();
            EnsureMeshFolder();

            string currentPath = SceneManager.GetActiveScene().path;
            string[] actPaths = new[]
            {
                "Assets/_Project/Scenes/Act1.unity",
                "Assets/_Project/Scenes/Act2.unity",
                "Assets/_Project/Scenes/Act3.unity",
                "Assets/_Project/Scenes/Act4.unity",
                "Assets/_Project/Scenes/Act5.unity"
            };

            foreach (var path in actPaths)
            {
                if (!File.Exists(path))
                {
                    Debug.LogWarning($"[LighthouseHeadBuilder] Scene not found: {path}");
                    continue;
                }

                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                BuildHeadInScene(scene);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log($"[LighthouseHeadBuilder] Processed and saved: {path}");
            }

            if (!string.IsNullOrEmpty(currentPath) && File.Exists(currentPath))
            {
                EditorSceneManager.OpenScene(currentPath, OpenSceneMode.Single);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[LighthouseHeadBuilder] Successfully applied lighthouse head across all canonical acts!");
        }

        public static void BuildHeadInScene(Scene scene)
        {
            EnsureMaterials();
            EnsureMeshFolder();

            // 1. Clean up any obsolete head or trapped spotlight in the active scene
            var existingHead = GameObject.Find(RootName);
            if (existingHead != null)
            {
                Object.DestroyImmediate(existingHead);
            }

            var oldTrappedSpot = GameObject.Find("Top Lantern Sweeping Spotlight");
            if (oldTrappedSpot != null)
            {
                oldTrappedSpot.SetActive(false);
            }

            // 2. Resolve parent container: prefer Reference Visuals/Lighthouse if present
            Transform parent = null;
            var refLighthouse = GameObject.Find("/Reference Visuals/Lighthouse");
            if (refLighthouse != null)
            {
                parent = refLighthouse.transform;
            }

            var headRoot = new GameObject(RootName);
            if (parent != null)
            {
                headRoot.transform.SetParent(parent, false);
            }
            headRoot.transform.position = Vector3.zero;
            headRoot.transform.rotation = Quaternion.identity;
            headRoot.transform.localScale = Vector3.one;

            // 3. Flared Masonry Capital Corbel (smoothly flares shaft from r=4.80m out to r=5.85m)
            float corbelHeight = 1.00f;
            float corbelBaseY = TowerShaftTopY - corbelHeight; // 24.60m
            var corbelMesh = GetOrPersistMesh("Lighthouse_CorbelCapital", () =>
                CreateTaperedCylinderMesh(TowerShaftOuterR, GalleryDeckRadius, corbelHeight, 32, 2.5f));

            var corbelGo = new GameObject("Gallery Masonry Corbel Capital");
            corbelGo.transform.SetParent(headRoot.transform, false);
            corbelGo.transform.position = new Vector3(0f, corbelBaseY, 0f);
            corbelGo.AddComponent<MeshFilter>().sharedMesh = corbelMesh;
            corbelGo.AddComponent<MeshRenderer>().sharedMaterial = matStone;

            // 4. 16 Cast-Iron Radial Corbel Support Brackets under overhang
            var corbelBracketsGroup = new GameObject("Gallery Cast-Iron Brackets").transform;
            corbelBracketsGroup.SetParent(headRoot.transform, false);
            corbelBracketsGroup.position = Vector3.zero;

            const int numBrackets = 16;
            for (int i = 0; i < numBrackets; i++)
            {
                float angle = i * Mathf.PI * 2f / numBrackets;
                float sin = Mathf.Sin(angle);
                float cos = Mathf.Cos(angle);

                Vector3 pWall = new Vector3(sin * TowerShaftOuterR, TowerShaftTopY - 0.90f, cos * TowerShaftOuterR);
                Vector3 pOuter = new Vector3(sin * (GalleryDeckRadius - 0.12f), TowerShaftTopY - 0.06f, cos * (GalleryDeckRadius - 0.12f));

                CreateStrut("Corbel Strut " + i, pWall, pOuter, 0.085f, matIron, corbelBracketsGroup);

                // Gusset web between strut and soffit
                Vector3 mid = (pWall + pOuter) * 0.5f;
                var web = GameObject.CreatePrimitive(PrimitiveType.Cube);
                web.name = "Corbel Web " + i;
                web.transform.SetParent(corbelBracketsGroup, false);
                web.transform.position = mid + Vector3.up * 0.10f;
                web.transform.rotation = Quaternion.Euler(0f, -angle * Mathf.Rad2Deg + 90f, 0f);
                web.transform.localScale = new Vector3(0.05f, 0.24f, 0.60f);
                if (matIron) web.GetComponent<Renderer>().sharedMaterial = matIron;
                Object.DestroyImmediate(web.GetComponent<Collider>());
            }

            // 5. Watch Gallery Platform Floor Slab (masonry underside & structure)
            float floorThickness = 0.35f;
            var floorMesh = GetOrPersistMesh("Lighthouse_GalleryFloorPlate", () =>
                CreateCylinderPlateMesh(GalleryDeckRadius, floorThickness, 32));

            var floorGo = new GameObject("Watch Gallery Platform Floor");
            floorGo.transform.SetParent(headRoot.transform, false);
            floorGo.transform.position = new Vector3(0f, TowerShaftTopY + floorThickness * 0.5f, 0f); // 25.775m
            floorGo.AddComponent<MeshFilter>().sharedMesh = floorMesh;
            floorGo.AddComponent<MeshRenderer>().sharedMaterial = matStone;

            // 6. Diamond-Plate Iron Catwalk Deck Plate
            float deckY = TowerShaftTopY + floorThickness; // 25.95m
            var deckMesh = GetOrPersistMesh("Lighthouse_GalleryDiamondPlateDeck", () =>
                CreateCylinderPlateMesh(GalleryDeckRadius, 0.06f, 32));

            var deckGo = new GameObject("Watch Gallery Diamond-Plate Deck");
            deckGo.transform.SetParent(headRoot.transform, false);
            deckGo.transform.position = new Vector3(0f, deckY + 0.03f, 0f);
            deckGo.AddComponent<MeshFilter>().sharedMesh = deckMesh;
            deckGo.AddComponent<MeshRenderer>().sharedMaterial = matIron;

            // 7. Outer Structural Iron Curb Band
            var curbMesh = GetOrPersistMesh("Lighthouse_GalleryOuterCurb", () =>
                CreateTubularBandMesh(GalleryDeckRadius + 0.02f, 0.42f, 32));

            var curbGo = new GameObject("Gallery Outer Iron Curb");
            curbGo.transform.SetParent(headRoot.transform, false);
            curbGo.transform.position = new Vector3(0f, TowerShaftTopY + floorThickness * 0.5f, 0f);
            curbGo.AddComponent<MeshFilter>().sharedMesh = curbMesh;
            curbGo.AddComponent<MeshRenderer>().sharedMaterial = matIron;

            // 8. Metal Balcony Railings (24 cast-iron balusters + top handrail ring + mid rail + toe kick)
            var railingsGroup = new GameObject("Metal Balcony Railings").transform;
            railingsGroup.SetParent(headRoot.transform, false);
            railingsGroup.position = new Vector3(0f, deckY, 0f);

            const int numBalusters = 24;
            float railingHeight = 1.15f;
            for (int i = 0; i < numBalusters; i++)
            {
                float angle = i * Mathf.PI * 2f / numBalusters;
                float bx = Mathf.Sin(angle) * RailingRadius;
                float bz = Mathf.Cos(angle) * RailingRadius;

                var post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                post.name = $"Balcony Stanchion {i}";
                post.transform.SetParent(railingsGroup, false);
                post.transform.localPosition = new Vector3(bx, railingHeight * 0.5f, bz);
                post.transform.localScale = new Vector3(0.09f, railingHeight * 0.5f, 0.09f);
                if (matIron) post.GetComponent<Renderer>().sharedMaterial = matIron;
                Object.DestroyImmediate(post.GetComponent<Collider>());
            }

            // Top Handrail Ring (tubular iron)
            var handrailMesh = GetOrPersistMesh("Lighthouse_TopHandrailRing", () =>
                CreateTubularBandMesh(RailingRadius, 0.09f, 32));
            var handrailGo = new GameObject("Balcony Top Handrail Ring");
            handrailGo.transform.SetParent(railingsGroup, false);
            handrailGo.transform.localPosition = new Vector3(0f, railingHeight, 0f);
            handrailGo.AddComponent<MeshFilter>().sharedMesh = handrailMesh;
            handrailGo.AddComponent<MeshRenderer>().sharedMaterial = matIron;

            // Mid Rail Ring
            var midRailMesh = GetOrPersistMesh("Lighthouse_MidRailRing", () =>
                CreateTubularBandMesh(RailingRadius, 0.05f, 32));
            var midRailGo = new GameObject("Balcony Mid Rail Ring");
            midRailGo.transform.SetParent(railingsGroup, false);
            midRailGo.transform.localPosition = new Vector3(0f, railingHeight * 0.52f, 0f);
            midRailGo.AddComponent<MeshFilter>().sharedMesh = midRailMesh;
            midRailGo.AddComponent<MeshRenderer>().sharedMaterial = matIron;

            // Toe Kick Ring
            var toeKickMesh = GetOrPersistMesh("Lighthouse_ToeKickRing", () =>
                CreateTubularBandMesh(RailingRadius, 0.12f, 32));
            var toeKickGo = new GameObject("Balcony Toe Kick Ring");
            toeKickGo.transform.SetParent(railingsGroup, false);
            toeKickGo.transform.localPosition = new Vector3(0f, 0.08f, 0f);
            toeKickGo.AddComponent<MeshFilter>().sharedMesh = toeKickMesh;
            toeKickGo.AddComponent<MeshRenderer>().sharedMaterial = matIron;

            // 9. Lantern Iron Murette Wall Base (height 0.90m)
            float muretteHeight = 0.90f;
            float muretteBaseY = deckY;
            var muretteMesh = GetOrPersistMesh("Lighthouse_LanternMuretteBase", () =>
                CreateTubularBandMesh(LanternGlassRadius, muretteHeight, 32));

            var muretteGo = new GameObject("Lantern Iron Murette Base");
            muretteGo.transform.SetParent(headRoot.transform, false);
            muretteGo.transform.position = new Vector3(0f, muretteBaseY + muretteHeight * 0.5f, 0f);
            muretteGo.AddComponent<MeshFilter>().sharedMesh = muretteMesh;
            muretteGo.AddComponent<MeshRenderer>().sharedMaterial = matIron;

            // 10. 16-Pane Beacon Glass Enclosure (height 3.20m)
            float glassBaseY = muretteBaseY + muretteHeight; // 26.85m
            float glassHeight = 3.20f;
            var glassMesh = GetOrPersistMesh("Lighthouse_LanternGlassChamber", () =>
                CreateTubularBandMesh(LanternGlassRadius, glassHeight, 32, 2f));

            var glassGo = new GameObject("Lantern Glass Chamber");
            glassGo.transform.SetParent(headRoot.transform, false);
            glassGo.transform.position = new Vector3(0f, glassBaseY + glassHeight * 0.5f, 0f);
            glassGo.AddComponent<MeshFilter>().sharedMesh = glassMesh;
            glassGo.AddComponent<MeshRenderer>().sharedMaterial = matGlass;

            // 11. 16 Vertical Bronze Astragals and Trim Rings
            var astragalsGroup = new GameObject("Lantern Astragals and Framework").transform;
            astragalsGroup.SetParent(headRoot.transform, false);
            astragalsGroup.position = new Vector3(0f, glassBaseY, 0f);

            const int numAstragals = 16;
            for (int i = 0; i < numAstragals; i++)
            {
                float angle = i * Mathf.PI * 2f / numAstragals;
                float ax = Mathf.Sin(angle) * (LanternGlassRadius + 0.02f);
                float az = Mathf.Cos(angle) * (LanternGlassRadius + 0.02f);

                var strut = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                strut.name = $"Astragal Mullion {i}";
                strut.transform.SetParent(astragalsGroup, false);
                strut.transform.localPosition = new Vector3(ax, glassHeight * 0.5f, az);
                strut.transform.localScale = new Vector3(0.09f, glassHeight * 0.5f, 0.09f);
                if (matBrass) strut.GetComponent<Renderer>().sharedMaterial = matBrass;
                Object.DestroyImmediate(strut.GetComponent<Collider>());
            }

            // Top Bronze Lintel Ring (above glass, below copper dome)
            var lintelMesh = GetOrPersistMesh("Lighthouse_TopLintelRing", () =>
                CreateTubularBandMesh(LanternGlassRadius + 0.03f, 0.16f, 32));
            var lintelGo = new GameObject("Lantern Top Bronze Lintel");
            lintelGo.transform.SetParent(astragalsGroup, false);
            lintelGo.transform.localPosition = new Vector3(0f, glassHeight, 0f);
            lintelGo.AddComponent<MeshFilter>().sharedMesh = lintelMesh;
            lintelGo.AddComponent<MeshRenderer>().sharedMaterial = matBrass;

            // Mid Bronze Saddle Reinforcing Ring
            var saddleMesh = GetOrPersistMesh("Lighthouse_MidSaddleRing", () =>
                CreateTubularBandMesh(LanternGlassRadius + 0.03f, 0.06f, 32));
            var saddleGo = new GameObject("Lantern Mid Saddle Ring");
            saddleGo.transform.SetParent(astragalsGroup, false);
            saddleGo.transform.localPosition = new Vector3(0f, glassHeight * 0.5f, 0f);
            saddleGo.AddComponent<MeshFilter>().sharedMesh = saddleMesh;
            saddleGo.AddComponent<MeshRenderer>().sharedMaterial = matBrass;

            // 12. Central Fresnel Pedestal & Mechanism Base
            var pedestalGroup = new GameObject("Fresnel Pedestal Mechanism").transform;
            pedestalGroup.SetParent(headRoot.transform, false);
            pedestalGroup.position = new Vector3(0f, deckY, 0f);

            // Lower Plinth Base
            var plinth = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            plinth.name = "Fresnel Lower Plinth";
            plinth.transform.SetParent(pedestalGroup, false);
            plinth.transform.localPosition = new Vector3(0f, 0.25f, 0f);
            plinth.transform.localScale = new Vector3(2.40f, 0.25f, 2.40f);
            if (matIron) plinth.GetComponent<Renderer>().sharedMaterial = matIron;
            Object.DestroyImmediate(plinth.GetComponent<Collider>());

            // Upper Iron Column
            var column = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            column.name = "Fresnel Pedestal Column";
            column.transform.SetParent(pedestalGroup, false);
            column.transform.localPosition = new Vector3(0f, 0.65f, 0f);
            column.transform.localScale = new Vector3(1.70f, 0.25f, 1.70f);
            if (matIron) column.GetComponent<Renderer>().sharedMaterial = matIron;
            Object.DestroyImmediate(column.GetComponent<Collider>());

            // Polished Brass Clockwork Collar
            var collar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            collar.name = "Fresnel Clockwork Collar";
            collar.transform.SetParent(pedestalGroup, false);
            collar.transform.localPosition = new Vector3(0f, 1.05f, 0f);
            collar.transform.localScale = new Vector3(1.50f, 0.18f, 1.50f);
            if (matBrass) collar.GetComponent<Renderer>().sharedMaterial = matBrass;
            Object.DestroyImmediate(collar.GetComponent<Collider>());

            // Concentric Fresnel Glass Prism Drum
            var prismDrum = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            prismDrum.name = "Fresnel Glass Prism Lens";
            prismDrum.transform.SetParent(pedestalGroup, false);
            prismDrum.transform.localPosition = new Vector3(0f, 2.05f, 0f);
            prismDrum.transform.localScale = new Vector3(1.70f, 0.75f, 1.70f);
            if (matGlass) prismDrum.GetComponent<Renderer>().sharedMaterial = matGlass;
            Object.DestroyImmediate(prismDrum.GetComponent<Collider>());

            // 13. Classical Curved Weathered Copper Lantern Dome Roof
            float domeBaseY = glassBaseY + glassHeight; // 30.05m
            float domeHeight = 3.20f;
            var domeMesh = GetOrPersistMesh("Lighthouse_CurvedCopperDome", () =>
                CreateCurvedDomeMesh(LanternGlassRadius + 0.10f, domeHeight, 32, 14));

            var domeGo = new GameObject("Copper Lantern Dome");
            domeGo.transform.SetParent(headRoot.transform, false);
            domeGo.transform.position = new Vector3(0f, domeBaseY, 0f);
            domeGo.AddComponent<MeshFilter>().sharedMesh = domeMesh;
            domeGo.AddComponent<MeshRenderer>().sharedMaterial = matCopper;

            // 14. Copper Ball Finial & Lightning Rod Spire
            float apexY = domeBaseY + domeHeight; // 33.25m
            var finialSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            finialSphere.name = "Lantern Copper Vent Ball";
            finialSphere.transform.SetParent(headRoot.transform, false);
            finialSphere.transform.position = new Vector3(0f, apexY + 0.35f, 0f);
            finialSphere.transform.localScale = new Vector3(0.70f, 0.70f, 0.70f);
            if (matCopper) finialSphere.GetComponent<Renderer>().sharedMaterial = matCopper;
            Object.DestroyImmediate(finialSphere.GetComponent<Collider>());

            var spireCollar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            spireCollar.name = "Lightning Rod Spire Collar";
            spireCollar.transform.SetParent(headRoot.transform, false);
            spireCollar.transform.position = new Vector3(0f, apexY + 0.75f, 0f);
            spireCollar.transform.localScale = new Vector3(0.24f, 0.15f, 0.24f);
            if (matBrass) spireCollar.GetComponent<Renderer>().sharedMaterial = matBrass;
            Object.DestroyImmediate(spireCollar.GetComponent<Collider>());

            var spireRod = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            spireRod.name = "Lightning Rod Spire Needle";
            spireRod.transform.SetParent(headRoot.transform, false);
            spireRod.transform.position = new Vector3(0f, apexY + 2.15f, 0f);
            spireRod.transform.localScale = new Vector3(0.06f, 1.35f, 0.06f);
            if (matBrass) spireRod.GetComponent<Renderer>().sharedMaterial = matBrass;
            Object.DestroyImmediate(spireRod.GetComponent<Collider>());

            // 15. Sweeping Volumetric Beacon Spotlight & Radiant Core Glow
            float beaconY = deckY + 2.05f; // 28.00m
            var radiantBulbGo = new GameObject("Beacon Core Radiant Light");
            radiantBulbGo.transform.SetParent(headRoot.transform, false);
            radiantBulbGo.transform.position = new Vector3(0f, beaconY, 0f);

            var radiantLight = radiantBulbGo.AddComponent<Light>();
            radiantLight.type = LightType.Point;
            radiantLight.color = new Color(1.0f, 0.86f, 0.60f); // Warm incandescent kerosene mantle
            radiantLight.intensity = 22.0f;
            radiantLight.range = 50.0f;
            radiantLight.shadows = LightShadows.Soft;

            // Rotating Sweeping Beacon Pivot
            var beaconPivot = new GameObject("Lighthouse Beacon Pivot").transform;
            beaconPivot.SetParent(headRoot.transform, false);
            beaconPivot.position = new Vector3(0f, beaconY, 0f);
            beaconPivot.gameObject.AddComponent<VolumetricSweepLight>();

            var spotObj = new GameObject("Beacon Sweeping Spotlight");
            spotObj.transform.SetParent(beaconPivot, false);
            spotObj.transform.localPosition = Vector3.zero;
            spotObj.transform.localRotation = Quaternion.Euler(11f, 0f, 0f); // Slightly tilted downward across island and sea

            var spotLight = spotObj.AddComponent<Light>();
            spotLight.type = LightType.Spot;
            spotLight.color = new Color(1.0f, 0.94f, 0.82f);
            spotLight.intensity = 28.0f;
            spotLight.range = 220.0f;
            spotLight.spotAngle = 34.0f;
            spotLight.innerSpotAngle = 16.0f;
            spotLight.shadows = LightShadows.Soft;

            // Volumetric Cone Mesh
            var coneMesh = GetOrPersistMesh("Lighthouse_VolumetricLightCone", () =>
                CreateVolumetricConeMesh(18.0f, 180.0f, 32));

            var coneObj = new GameObject("Beacon Volumetric Light Cone");
            coneObj.transform.SetParent(spotObj.transform, false);
            coneObj.transform.localPosition = Vector3.zero;
            coneObj.transform.localRotation = Quaternion.identity;
            coneObj.AddComponent<MeshFilter>().sharedMesh = coneMesh;
            coneObj.AddComponent<MeshRenderer>().sharedMaterial = matBeam;
        }

        private static void CreateStrut(string name, Vector3 start, Vector3 end, float diameter, Material mat, Transform parent)
        {
            var rod = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rod.name = name;
            rod.transform.SetParent(parent, false);

            Vector3 delta = end - start;
            float length = delta.magnitude;
            rod.transform.position = (start + end) * 0.5f;
            if (length > 0.001f)
            {
                rod.transform.rotation = Quaternion.FromToRotation(Vector3.up, delta.normalized);
            }
            rod.transform.localScale = new Vector3(diameter, length * 0.5f, diameter);
            if (mat != null)
            {
                rod.GetComponent<Renderer>().sharedMaterial = mat;
            }
            Object.DestroyImmediate(rod.GetComponent<Collider>());
        }

        private static Mesh CreateTaperedCylinderMesh(float bottomR, float topR, float height, int segments, float uvVScale)
        {
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();

            float slope = (bottomR - topR) / height;

            for (int ring = 0; ring <= 1; ring++)
            {
                float y = ring * height;
                float r = Mathf.Lerp(bottomR, topR, ring);
                for (int i = 0; i <= segments; i++)
                {
                    float angle = i * Mathf.PI * 2f / segments;
                    float sin = Mathf.Sin(angle);
                    float cos = Mathf.Cos(angle);

                    vertices.Add(new Vector3(sin * r, y, cos * r));
                    normals.Add(new Vector3(sin, slope, cos).normalized);
                    uvs.Add(new Vector2((float)i / segments * 4f, ring * uvVScale));
                }
            }

            int ringStride = segments + 1;
            for (int i = 0; i < segments; i++)
            {
                int b0 = i;
                int b1 = i + 1;
                int t0 = ringStride + i;
                int t1 = ringStride + i + 1;

                triangles.Add(b0);
                triangles.Add(t0);
                triangles.Add(b1);

                triangles.Add(b1);
                triangles.Add(t0);
                triangles.Add(t1);
            }

            var mesh = new Mesh { name = "TaperedCylinderMesh" };
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh CreateTubularBandMesh(float radius, float height, int segments, float uvHMult = 4f)
        {
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();

            float halfH = height * 0.5f;
            for (int ring = 0; ring <= 1; ring++)
            {
                float y = (ring == 0) ? -halfH : halfH;
                for (int i = 0; i <= segments; i++)
                {
                    float angle = i * Mathf.PI * 2f / segments;
                    float sin = Mathf.Sin(angle);
                    float cos = Mathf.Cos(angle);

                    vertices.Add(new Vector3(sin * radius, y, cos * radius));
                    normals.Add(new Vector3(sin, 0f, cos));
                    uvs.Add(new Vector2((float)i / segments * uvHMult, ring));
                }
            }

            int ringStride = segments + 1;
            for (int i = 0; i < segments; i++)
            {
                int b0 = i;
                int b1 = i + 1;
                int t0 = ringStride + i;
                int t1 = ringStride + i + 1;

                triangles.Add(b0);
                triangles.Add(t0);
                triangles.Add(b1);

                triangles.Add(b1);
                triangles.Add(t0);
                triangles.Add(t1);
            }

            var mesh = new Mesh { name = "TubularBandMesh" };
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh CreateCylinderPlateMesh(float radius, float height, int segments)
        {
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();

            float halfH = height * 0.5f;

            // 1. Bottom Cap (Facing Down)
            int bottomCenterIndex = vertices.Count;
            vertices.Add(new Vector3(0, -halfH, 0));
            normals.Add(Vector3.down);
            uvs.Add(new Vector2(0.5f, 0.5f));

            int bottomRimStart = vertices.Count;
            for (int i = 0; i < segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                float x = Mathf.Sin(angle) * radius;
                float z = Mathf.Cos(angle) * radius;
                vertices.Add(new Vector3(x, -halfH, z));
                normals.Add(Vector3.down);
                uvs.Add(new Vector2(x / (radius * 2f) + 0.5f, z / (radius * 2f) + 0.5f));
            }
            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                triangles.Add(bottomCenterIndex);
                triangles.Add(bottomRimStart + next);
                triangles.Add(bottomRimStart + i);
            }

            // 2. Top Cap (Facing Up)
            int topCenterIndex = vertices.Count;
            vertices.Add(new Vector3(0, halfH, 0));
            normals.Add(Vector3.up);
            uvs.Add(new Vector2(0.5f, 0.5f));

            int topRimStart = vertices.Count;
            for (int i = 0; i < segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                float x = Mathf.Sin(angle) * radius;
                float z = Mathf.Cos(angle) * radius;
                vertices.Add(new Vector3(x, halfH, z));
                normals.Add(Vector3.up);
                uvs.Add(new Vector2(x / (radius * 2f) + 0.5f, z / (radius * 2f) + 0.5f));
            }
            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                triangles.Add(topCenterIndex);
                triangles.Add(topRimStart + i);
                triangles.Add(topRimStart + next);
            }

            // 3. Cylinder Side Walls
            int sideStart = vertices.Count;
            for (int i = 0; i <= segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                float sin = Mathf.Sin(angle);
                float cos = Mathf.Cos(angle);
                Vector3 n = new Vector3(sin, 0, cos);

                // Bottom vertex
                vertices.Add(new Vector3(sin * radius, -halfH, cos * radius));
                normals.Add(n);
                uvs.Add(new Vector2((float)i / segments * 4f, 0f));

                // Top vertex
                vertices.Add(new Vector3(sin * radius, halfH, cos * radius));
                normals.Add(n);
                uvs.Add(new Vector2((float)i / segments * 4f, 1f));
            }

            for (int i = 0; i < segments; i++)
            {
                int b0 = sideStart + i * 2;
                int t0 = sideStart + i * 2 + 1;
                int b1 = sideStart + (i + 1) * 2;
                int t1 = sideStart + (i + 1) * 2 + 1;

                triangles.Add(b0);
                triangles.Add(t0);
                triangles.Add(b1);

                triangles.Add(b1);
                triangles.Add(t0);
                triangles.Add(t1);
            }

            var mesh = new Mesh { name = "CylinderPlateMesh" };
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh CreateCurvedDomeMesh(float baseRadius, float height, int segments, int rings)
        {
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();

            int ringStride = segments + 1;

            for (int j = 0; j <= rings; j++)
            {
                float t = (float)j / rings; // 0 at base, 1 at apex
                float theta = t * Mathf.PI * 0.5f;
                float r = baseRadius * Mathf.Cos(theta);
                float y = height * Mathf.Sin(theta);

                for (int i = 0; i <= segments; i++)
                {
                    float angle = i * Mathf.PI * 2f / segments;
                    float sin = Mathf.Sin(angle);
                    float cos = Mathf.Cos(angle);

                    vertices.Add(new Vector3(sin * r, y, cos * r));

                    Vector3 n = new Vector3(sin * Mathf.Cos(theta), Mathf.Sin(theta), cos * Mathf.Cos(theta)).normalized;
                    normals.Add(n);
                    uvs.Add(new Vector2((float)i / segments * 3f, t * 2f));
                }
            }

            for (int j = 0; j < rings; j++)
            {
                int row0 = j * ringStride;
                int row1 = (j + 1) * ringStride;

                for (int i = 0; i < segments; i++)
                {
                    int b0 = row0 + i;
                    int b1 = row0 + i + 1;
                    int t0 = row1 + i;
                    int t1 = row1 + i + 1;

                    triangles.Add(b0);
                    triangles.Add(t0);
                    triangles.Add(b1);

                    triangles.Add(b1);
                    triangles.Add(t0);
                    triangles.Add(t1);
                }
            }

            var mesh = new Mesh { name = "CurvedDomeMesh" };
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh CreateVolumetricConeMesh(float baseRadius, float length, int segments)
        {
            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var normals = new List<Vector3>();
            var triangles = new List<int>();

            // Apex at (0, 0, 0)
            vertices.Add(Vector3.zero);
            uvs.Add(new Vector2(0.5f, 0f));
            normals.Add(-Vector3.forward);

            for (int i = 0; i <= segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                float x = Mathf.Sin(angle) * baseRadius;
                float y = Mathf.Cos(angle) * baseRadius;
                Vector3 basePt = new Vector3(x, y, length);

                vertices.Add(basePt);
                uvs.Add(new Vector2((float)i / segments, 1.0f));
                normals.Add(new Vector3(Mathf.Sin(angle), Mathf.Cos(angle), 0f));
            }

            for (int i = 1; i <= segments; i++)
            {
                triangles.Add(0);
                triangles.Add(i);
                triangles.Add(i + 1);
            }

            var mesh = new Mesh { name = "VolumetricLightConeMesh" };
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh GetOrPersistMesh(string filename, Func<Mesh> generator)
        {
            EnsureMeshFolder();
            string path = $"{MeshFolder}/{filename}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing != null)
            {
                return existing;
            }

            var created = generator();
            created.name = filename;
            AssetDatabase.CreateAsset(created, path);
            return created;
        }

        private static void EnsureMeshFolder()
        {
            if (!Directory.Exists(MeshFolder))
            {
                Directory.CreateDirectory(MeshFolder);
                AssetDatabase.Refresh();
            }
        }

        private static void EnsureMaterials()
        {
            matStone = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/PBR/PBR_LighthouseStone.mat");
            matIron = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/PBR/PBR_RustedIron.mat");
            matCopper = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Reference/TarnishedCopperKettle.mat");
            matBrass = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Reference/PolishedMaritimeBrass.mat");
            matGlass = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Reference/Beacon Glass.mat");
            matBeam = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Graphics1/LampBeam.mat");
        }
    }
}
