using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace TheLightHouse
{
    public static class TorchGripMesh
    {
        public static void Build(Transform parent, Material skin, Material wool, Material cuff)
        {
            // Coordinates are in the barrel frame: +Z points through the lens, +X is the back of the right hand.
            Ellipsoid(parent, "Right palm", new Vector3(.032f, .002f, -.052f), new Vector3(.035f, .058f, .087f), skin);
            Tube(parent, "Wrist", new[] { new Vector3(.039f, -.008f, -.083f), new Vector3(.049f, -.021f, -.118f), new Vector3(.068f, -.044f, -.155f) }, new[] { .026f, .025f, .031f }, skin);
            Tube(parent, "Wool sleeve", new[] { new Vector3(.061f, -.037f, -.145f), new Vector3(.105f, -.105f, -.19f), new Vector3(.16f, -.24f, -.23f) }, new[] { .038f, .048f, .058f }, wool);
            Tube(parent, "Ribbed cuff", new[] { new Vector3(.052f, -.027f, -.124f), new Vector3(.069f, -.046f, -.16f) }, new[] { .034f, .038f }, cuff);
            for (int finger = 0; finger < 4; finger++)
            {
                float z = -.018f - finger * .021f;
                float radius = finger == 3 ? .0075f : .009f;
                var points = new List<Vector3>();
                var radii = new List<float>();
                for (int i = 0; i <= 18; i++)
                {
                    float angle = Mathf.Lerp(25f, -205f, i / 18f) * Mathf.Deg2Rad;
                    points.Add(new Vector3(Mathf.Cos(angle) * .029f, Mathf.Sin(angle) * .029f, z));
                    radii.Add(radius * Mathf.Lerp(1f, .78f, i / 18f));
                }
                Tube(parent, "Curled finger " + finger, points.ToArray(), radii.ToArray(), skin);
            }
            Tube(parent, "Opposed thumb", new[] { new Vector3(.038f, .015f, -.071f), new Vector3(.031f, .029f, -.049f), new Vector3(.015f, .034f, -.026f), new Vector3(.001f, .031f, -.012f) }, new[] { .013f, .012f, .011f, .009f }, skin);
        }

        private static void Ellipsoid(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            var collider = go.GetComponent<Collider>();
            collider.enabled = false;
            Object.Destroy(collider);
            var renderer = go.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
        }

        private static void Tube(Transform parent, string name, Vector3[] points, float[] radii, Material material)
        {
            const int sides = 16;
            var vertices = new List<Vector3>();
            var uv = new List<Vector2>();
            var triangles = new List<int>();
            for (int i = 0; i < points.Length; i++)
            {
                var tangent = (points[Mathf.Min(i + 1, points.Length - 1)] - points[Mathf.Max(0, i - 1)]).normalized;
                var normal = Vector3.Cross(tangent, Mathf.Abs(tangent.z) > .9f ? Vector3.up : Vector3.forward).normalized;
                var binormal = Vector3.Cross(tangent, normal);
                for (int j = 0; j <= sides; j++)
                {
                    float angle = j * Mathf.PI * 2f / sides;
                    vertices.Add(points[i] + (normal * Mathf.Cos(angle) + binormal * Mathf.Sin(angle)) * radii[i]);
                    uv.Add(new Vector2(j / (float)sides, i / (float)(points.Length - 1)));
                    if (i == 0 || j == sides) continue;
                    int a = (i - 1) * (sides + 1) + j, b = i * (sides + 1) + j;
                    triangles.Add(a); triangles.Add(a + 1); triangles.Add(b);
                    triangles.Add(a + 1); triangles.Add(b + 1); triangles.Add(b);
                }
            }
            for (int end = 0; end < 2; end++)
            {
                int center = vertices.Count;
                vertices.Add(points[end == 0 ? 0 : points.Length - 1]); uv.Add(new Vector2(.5f, .5f));
                int offset = end == 0 ? 0 : (points.Length - 1) * (sides + 1);
                for (int j = 0; j < sides; j++)
                {
                    triangles.Add(center); triangles.Add(offset + (end == 0 ? j + 1 : j)); triangles.Add(offset + (end == 0 ? j : j + 1));
                }
            }
            var mesh = new Mesh { name = name };
            mesh.SetVertices(vertices); mesh.SetUVs(0, uv); mesh.SetTriangles(triangles, 0); mesh.RecalculateNormals(); mesh.RecalculateBounds();
            var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(parent, false);
            go.GetComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.GetComponent<MeshRenderer>(); renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
        }
    }
}
