using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace TheLightHouse.Editor
{
    public static class LighthouseSolidEnvelopeRepair
    {
        public static void ApplyCurrent()
        {
            var root = GameObject.Find("Architecture Collision Overhaul").transform;
            var old = root.Find("Solid Stair Envelope");
            if (old) Object.DestroyImmediate(old.gameObject);
            var group = new GameObject("Solid Stair Envelope").transform;
            group.SetParent(root, false);
            var settings = AssetDatabase.LoadAssetAtPath<ArchitectureRepairSettings>("Assets/_Project/Settings/ArchitectureRepairSettings.asset");
            string folder = "Assets/_Project/Art/ArchitectureRepair/" + SceneManager.GetActiveScene().name + "/SolidEnvelope";
            Directory.CreateDirectory(folder);
            AssetDatabase.Refresh();
            var arcs = Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(r => r.name.StartsWith("Outer Tower Stone Arc ")).ToArray();
            var stone = arcs.FirstOrDefault()?.sharedMaterial ?? AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/PBR/PBR_LighthouseStone.mat");
            var wood = root.GetComponentsInChildren<MeshRenderer>().First(r => r.name == "Repaired Floor 2 Outer").sharedMaterial;
            var iron = root.GetComponentsInChildren<MeshRenderer>().First(r => r.name == "Visual Tread 1-0").sharedMaterial;
            GameObject Strip(string name, float inner, float outer, float start, float end, float lower, float upper, float depth, Material material)
            {
                var mesh = ArchitectureMeshBuilder.Strip(name, inner, outer, start, end, lower, upper, depth,
                    Mathf.Max(1, Mathf.CeilToInt((end - start) / settings.stairMeshAngle)));
                mesh = ArchitectureMeshBuilder.Persist(mesh, folder + "/" + name + ".asset");
                var go = new GameObject(name);
                go.transform.SetParent(group, false);
                go.AddComponent<MeshFilter>().sharedMesh = mesh;
                go.AddComponent<MeshRenderer>().sharedMaterial = material;
                var collider = go.AddComponent<MeshCollider>();
                collider.sharedMesh = mesh;
                collider.convex = false;
                collider.isTrigger = false;
                go.isStatic = true;
                return go;
            }
            // Replace only the inner masonry lining, retaining the exterior tapered finish.
            foreach (var arc in arcs) arc.gameObject.SetActive(false);
            for (int i = 0; i < 32; i++)
            {
                bool doorway = i >= 7 && i <= 9 || i >= 15 && i <= 17 || i >= 20 && i <= 22;
                float bottom = doorway ? settings.architecturalDoorTop : 0f;
                Strip("Closed Masonry Sector " + i, settings.wallInnerRadius,
                    settings.wallInnerRadius + settings.solidWallThickness,
                    (i - .5f) * 360f / 32, (i + .5f) * 360f / 32,
                    settings.masonryTop, settings.masonryTop, settings.masonryTop - bottom, stone);
            }
            float[] starts = {225f, 105f, 345f};
            float[] ends = {435f, 315f, 525f};
            for (int flight = 0; flight < 3; flight++)
            {
                float lower = ArchitectureCollisionOverhaul.Levels[flight];
                float upper = ArchitectureCollisionOverhaul.Levels[flight + 1];
                float openingStart = starts[flight] + (upper - lower - settings.headClearance - .18f)
                    / (upper - lower) * (ends[flight] - starts[flight]) - settings.stairOpeningMargin;
                if (flight < 2)
                    Strip("Floor " + (flight + 1) + " Perimeter Support", 4.65f, settings.wallInnerRadius,
                        openingStart, ends[flight], upper, upper, .14f, wood);
                Strip("Stair " + flight + " Outer Guard", 4.65f, 4.65f + settings.handrailThickness,
                    starts[flight], ends[flight], lower + settings.stairGuardHeight, upper + settings.stairGuardHeight,
                    settings.stairGuardHeight, iron);
            }
            foreach (var c in root.GetComponentsInChildren<Collider>())
            {
                if (c.GetComponent<FallbackSafetyPlane>()) continue;
                if (c.name.StartsWith("Visual Tread")) { c.enabled = false; continue; }
                c.isTrigger = false;
                if (c is MeshCollider mc) mc.convex = false;
            }
            var movement = Resources.Load<PlayerMovementSettings>("PlayerMovementSettings");
            foreach (var player in Object.FindObjectsByType<FirstPersonController>(FindObjectsSortMode.None))
            {
                var capsule = player.GetComponent<CharacterController>();
                capsule.height = movement.standingHeight;
                capsule.radius = movement.capsuleRadius;
                capsule.center = Vector3.up * capsule.height * .5f;
                capsule.stepOffset = movement.stepOffset;
                capsule.slopeLimit = movement.slopeLimit;
                capsule.skinWidth = movement.skinWidth;
                capsule.minMoveDistance = 0f;
                EditorUtility.SetDirty(capsule);
            }
        }
    }
}
