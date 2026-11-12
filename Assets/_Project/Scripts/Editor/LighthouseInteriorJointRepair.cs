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
    public static class LighthouseInteriorJointRepair
    {
        const string GeneratedName = "Interior Ceilings and Handrails";
        static readonly float[] Starts = { 225f, 105f, 345f };
        static readonly float[] Ends = { 435f, 315f, 525f };

        [MenuItem("TheLightHouse/Level/Repair Interior Joints")]
        public static void ApplyAll()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play Mode before repairing geometry.");
            if (SceneManager.GetActiveScene().isDirty) throw new InvalidOperationException("Save the current scene first.");
            var setup = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                for (int act = 1; act <= 5; act++)
                {
                    var scene = EditorSceneManager.OpenScene($"Assets/_Project/Scenes/Act{act}.unity");
                    ApplyCurrent();
                    foreach (var nav in Object.FindObjectsByType<Unity.AI.Navigation.NavMeshSurface>(FindObjectsSortMode.None))
                        nav.BuildNavMesh();
                    EditorSceneManager.SaveScene(scene);
                }
                AssetDatabase.SaveAssets();
            }
            finally { EditorSceneManager.RestoreSceneManagerSetup(setup); }
        }

        public static void ApplyCurrent()
        {
            var root = GameObject.Find("Architecture Collision Overhaul");
            if (!root) throw new InvalidOperationException("The repaired tower is missing.");
            var settings = AssetDatabase.LoadAssetAtPath<ArchitectureRepairSettings>("Assets/_Project/Settings/ArchitectureRepairSettings.asset");
            if (!settings) throw new InvalidOperationException("Architecture repair settings are missing.");
            var old = root.transform.Find(GeneratedName);
            if (old) Object.DestroyImmediate(old.gameObject);
            var generated = new GameObject(GeneratedName).transform;
            generated.SetParent(root.transform, false);
            string folder = "Assets/_Project/Art/ArchitectureRepair/" + SceneManager.GetActiveScene().name + "/InteriorJoints";
            Directory.CreateDirectory(folder);
            AssetDatabase.Refresh();
            var filters = root.GetComponentsInChildren<MeshFilter>();
            MeshFilter Find(string name) => filters.First(f => f.name == name);
            void Replace(string name, float inner, float outer, float start, float end, float y, float depth)
            {
                var filter = Find(name);
                var mesh = ArchitectureMeshBuilder.Strip(name, inner, outer, start, end, y, y, depth,
                    Mathf.Max(2, Mathf.CeilToInt(Mathf.Abs(end - start) / 3f)));
                var uv = mesh.uv;
                var vertices = mesh.vertices;
                var normals = mesh.normals;
                for (int i = 0; i < uv.Length; i++)
                    if (Mathf.Abs(normals[i].y) > .9f) uv[i] = new Vector2(vertices[i].x, vertices[i].z);
                mesh.uv = uv;
                mesh = ArchitectureMeshBuilder.Persist(mesh, folder + "/" + name + ".asset");
                filter.sharedMesh = mesh;
                if (filter.TryGetComponent<MeshCollider>(out var collider)) collider.sharedMesh = mesh;
            }

            Replace("Repaired Floor 1", .001f, settings.wallInnerRadius, 0, 360, .15f, .14f);
            for (int level = 1; level < 4; level++)
                Replace($"Repaired Floor {level + 1} Central", .001f, 3.15f, 0, 360,
                    ArchitectureCollisionOverhaul.Levels[level], level == 3 ? .16f : .14f);
            float gapAngle = 0f;
            float openingMargin = settings.stairOpeningMargin;
            float openingStart = Starts[0] + (3.2f - settings.headClearance - .18f) / 3.2f * 210f - openingMargin;
            Replace("Repaired Floor 2 Outer", 3.15f, settings.wallInnerRadius, 75f + gapAngle, openingStart, 3.35f, .14f);
            Replace("Repaired Floor 3 Outer", 3.15f, settings.wallInnerRadius, 315f + gapAngle, openingStart + 240f, 6.55f, .14f);
            Replace("Repaired Landing 4", 3.15f, 4.85f, 165f + gapAngle, 200f, 9.75f, .16f);
            // The landing already fills this part of the catwalk; adjoining edges replace coplanar slabs.
            Replace("Lantern Balcony Catwalk", 3.35f, 4.85f, 200f, 340f, 9.75f, .16f);

            for (int flight = 0; flight < 3; flight++)
            {
                float lower = ArchitectureCollisionOverhaul.Levels[flight];
                float upper = ArchitectureCollisionOverhaul.Levels[flight + 1];
                var treads = filters.Where(f => f.name.StartsWith($"Visual Tread {flight + 1}-")).ToArray();
                for (int step = 0; step < treads.Length; step++)
                {
                    float a = Mathf.Lerp(Starts[flight], Ends[flight], (float)step / treads.Length);
                    float b = Mathf.Lerp(Starts[flight], Ends[flight], (float)(step + 1) / treads.Length);
                    float y = Mathf.Lerp(lower, upper, (float)step / (treads.Length - 1));
                    Replace($"Visual Tread {flight + 1}-{step}", 3.25f, 4.65f, a, b, y,
                        (upper - lower) / treads.Length + settings.stairSoffitDepth);
                }
            }

            foreach (var floor in filters.Where(f => f.name.StartsWith("Repaired Floor") || f.name == "Repaired Landing 4" || f.name == "Lantern Balcony Catwalk"))
            {
                if (floor.sharedMesh.bounds.max.y < 1f) continue;
                var source = floor.sharedMesh;
                var mesh = Object.Instantiate(source);
                mesh.name = floor.name + " Ceiling";
                var vertices = mesh.vertices;
                float top = source.bounds.min.y;
                for (int i = 0; i < vertices.Length; i++)
                    vertices[i].y = Mathf.Lerp(top - settings.ceilingThickness, top,
                        Mathf.InverseLerp(source.bounds.min.y, source.bounds.max.y, vertices[i].y));
                mesh.vertices = vertices;
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
                mesh = ArchitectureMeshBuilder.Persist(mesh, folder + "/" + mesh.name + ".asset");
                var ceiling = new GameObject(mesh.name);
                ceiling.transform.SetParent(generated, false);
                ceiling.AddComponent<MeshFilter>().sharedMesh = mesh;
                ceiling.AddComponent<MeshRenderer>().sharedMaterial = floor.GetComponent<MeshRenderer>().sharedMaterial;
                ceiling.AddComponent<MeshCollider>().sharedMesh = mesh;
            }

            void Rail(Transform a, Transform b, string name)
            {
                Vector3 start = a.position + Vector3.up * .525f;
                Vector3 end = b.position + Vector3.up * .525f;
                var rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rail.name = name;
                rail.transform.SetParent(generated, false);
                rail.transform.position = (start + end) * .5f;
                rail.transform.rotation = Quaternion.LookRotation(end - start);
                rail.transform.localScale = new Vector3(settings.handrailThickness, settings.handrailThickness, Vector3.Distance(start, end));
                rail.GetComponent<Renderer>().sharedMaterial = a.GetComponent<Renderer>().sharedMaterial;
            }
            for (int flight = 1; flight <= 3; flight++)
            {
                var posts = root.GetComponentsInChildren<Transform>().Where(t => t.name.StartsWith($"Stair Baluster {flight}_"))
                    .OrderBy(t => int.Parse(t.name.Split('_').Last())).ToArray();
                for (int i = 1; i < posts.Length; i++) Rail(posts[i - 1], posts[i], $"Stair {flight} Handrail {i}");
            }
            foreach (var group in root.GetComponentsInChildren<Transform>().Where(t => t.name.EndsWith("Wellhole Guard")))
            {
                var posts = group.Cast<Transform>().Where(t => t.name.Contains(" Post "))
                    .OrderBy(t => int.Parse(t.name.Split(' ').Last())).ToArray();
                for (int i = 1; i < posts.Length; i++) Rail(posts[i - 1], posts[i], group.name + " Handrail " + i);
            }
            LighthouseSolidEnvelopeRepair.ApplyCurrent();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
    }
}
