using UnityEngine;
using UnityEngine.Rendering;

namespace TheLightHouse
{
    /// <summary>
    /// Procedurally constructs and materials a detailed 1940s-style vintage ribbed metal flashlight
    /// with knurled barrel rings, flared brass bezel, reflector cone, emissive bulb filament,
    /// and scratched translucent front glass lens.
    /// </summary>
    public static class FlashlightModelBuilder
    {
        private const string MatRoot = "Assets/_Project/Materials/Reference";

        public static GameObject BuildFlashlightModel(Transform parent, out Renderer bulbRenderer, out Material bulbMaterial)
        {
            var modelRoot = new GameObject("Flashlight_1940s_Ribbed_Mesh");
            modelRoot.transform.SetParent(parent, false);
            modelRoot.transform.localPosition = Vector3.zero;
            modelRoot.transform.localRotation = Quaternion.identity;

            // Materials
            var matMetal = EnsureMaterial("FlashlightGunmetal", "Universal Render Pipeline/Lit");
            matMetal.SetColor("_BaseColor", new Color(0.18f, 0.20f, 0.22f));
            matMetal.SetFloat("_Metallic", 0.88f);
            matMetal.SetFloat("_Smoothness", 0.48f);

            var matBrass = EnsureMaterial("FlashlightBrassTrim", "Universal Render Pipeline/Lit");
            matBrass.SetColor("_BaseColor", new Color(0.78f, 0.62f, 0.32f));
            matBrass.SetFloat("_Metallic", 0.92f);
            matBrass.SetFloat("_Smoothness", 0.65f);

            var matReflector = EnsureMaterial("FlashlightChromeReflector", "Universal Render Pipeline/Lit");
            matReflector.SetColor("_BaseColor", new Color(0.92f, 0.92f, 0.95f));
            matReflector.SetFloat("_Metallic", 0.98f);
            matReflector.SetFloat("_Smoothness", 0.96f);

            var matGlass = EnsureMaterial("FlashlightLensGlass", "Universal Render Pipeline/Lit");
            matGlass.SetColor("_BaseColor", new Color(0.85f, 0.90f, 0.95f, 0.35f));
            matGlass.SetFloat("_Surface", 1); // Transparent
            matGlass.SetFloat("_Blend", 0);
            matGlass.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            matGlass.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            matGlass.SetFloat("_Smoothness", 0.95f);
            matGlass.SetFloat("_Metallic", 0.15f);

            bulbMaterial = EnsureMaterial("FlashlightBulbFilament", "Universal Render Pipeline/Lit");
            bulbMaterial.SetColor("_BaseColor", new Color(1.0f, 0.95f, 0.80f));
            bulbMaterial.EnableKeyword("_EMISSION");
            bulbMaterial.SetColor("_EmissionColor", new Color(1.0f, 0.92f, 0.75f) * 2.5f);

            // 1. Main Cylindrical Barrel (Length 0.14m, Radius 0.022m)
            var barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            barrel.name = "Flashlight_Barrel";
            barrel.transform.SetParent(modelRoot.transform, false);
            barrel.transform.localPosition = new Vector3(0, 0, -0.04f);
            barrel.transform.localRotation = Quaternion.Euler(90f, 0, 0);
            barrel.transform.localScale = new Vector3(0.044f, 0.070f, 0.044f);
            barrel.GetComponent<Renderer>().sharedMaterial = matMetal;
            Object.DestroyImmediate(barrel.GetComponent<Collider>());

            // 2. Ribbed Knurling Rings along Barrel (vintage grip ribs)
            for (int r = 0; r < 5; r++)
            {
                float rz = -0.085f + r * 0.022f;
                var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                ring.name = $"Grip_Rib_{r + 1}";
                ring.transform.SetParent(modelRoot.transform, false);
                ring.transform.localPosition = new Vector3(0, 0, rz);
                ring.transform.localRotation = Quaternion.Euler(90f, 0, 0);
                ring.transform.localScale = new Vector3(0.047f, 0.003f, 0.047f);
                ring.GetComponent<Renderer>().sharedMaterial = matBrass;
                Object.DestroyImmediate(ring.GetComponent<Collider>());
            }

            // 3. Flared Bezel / Bulb Collar (forward head)
            var collar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            collar.name = "Flashlight_Bezel_Collar";
            collar.transform.SetParent(modelRoot.transform, false);
            collar.transform.localPosition = new Vector3(0, 0, 0.055f);
            collar.transform.localRotation = Quaternion.Euler(90f, 0, 0);
            collar.transform.localScale = new Vector3(0.068f, 0.028f, 0.068f);
            collar.GetComponent<Renderer>().sharedMaterial = matBrass;
            Object.DestroyImmediate(collar.GetComponent<Collider>());

            // 4. Parabolic Reflector Cone inside Bezel
            var reflector = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            reflector.name = "Reflector_Cone";
            reflector.transform.SetParent(modelRoot.transform, false);
            reflector.transform.localPosition = new Vector3(0, 0, 0.062f);
            reflector.transform.localRotation = Quaternion.Euler(90f, 0, 0);
            reflector.transform.localScale = new Vector3(0.062f, 0.016f, 0.062f);
            reflector.GetComponent<Renderer>().sharedMaterial = matReflector;
            Object.DestroyImmediate(reflector.GetComponent<Collider>());

            // 5. Incandescent Filament Bulb (Center of reflector)
            var bulb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bulb.name = "Bulb_Filament";
            bulb.transform.SetParent(modelRoot.transform, false);
            bulb.transform.localPosition = new Vector3(0, 0, 0.068f);
            bulb.transform.localScale = Vector3.one * 0.016f;
            bulbRenderer = bulb.GetComponent<Renderer>();
            bulbRenderer.sharedMaterial = bulbMaterial;
            Object.DestroyImmediate(bulb.GetComponent<Collider>());

            // 6. Front Glass Lens (Translucent, scratched protective glass disc)
            var lens = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            lens.name = "Front_Glass_Lens";
            lens.transform.SetParent(modelRoot.transform, false);
            lens.transform.localPosition = new Vector3(0, 0, 0.082f);
            lens.transform.localRotation = Quaternion.Euler(90f, 0, 0);
            lens.transform.localScale = new Vector3(0.065f, 0.0015f, 0.065f);
            lens.GetComponent<Renderer>().sharedMaterial = matGlass;
            Object.DestroyImmediate(lens.GetComponent<Collider>());

            // 7. Mechanical Slide Switch on Barrel Top
            var swBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
            swBase.name = "Slide_Switch_Base";
            swBase.transform.SetParent(modelRoot.transform, false);
            swBase.transform.localPosition = new Vector3(0, 0.023f, -0.015f);
            swBase.transform.localScale = new Vector3(0.012f, 0.006f, 0.034f);
            swBase.GetComponent<Renderer>().sharedMaterial = matBrass;
            Object.DestroyImmediate(swBase.GetComponent<Collider>());

            var swKnob = GameObject.CreatePrimitive(PrimitiveType.Cube);
            swKnob.name = "Slide_Switch_Knob";
            swKnob.transform.SetParent(swBase.transform, false);
            swKnob.transform.localPosition = new Vector3(0, 0.75f, 0.20f);
            swKnob.transform.localScale = new Vector3(1.15f, 0.85f, 0.40f);
            swKnob.GetComponent<Renderer>().sharedMaterial = matMetal;
            Object.DestroyImmediate(swKnob.GetComponent<Collider>());

            return modelRoot;
        }

        private static Material EnsureMaterial(string name, string shaderName)
        {
#if UNITY_EDITOR
            string path = $"{MatRoot}/{name}.mat";
            var mat = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!mat)
            {
                var shader = Shader.Find(shaderName) ?? Shader.Find("Universal Render Pipeline/Lit");
                mat = new Material(shader) { name = name };
                UnityEditor.AssetDatabase.CreateAsset(mat, path);
            }
            return mat;
#else
            var s = Shader.Find(shaderName) ?? Shader.Find("Universal Render Pipeline/Lit");
            return new Material(s) { name = name };
#endif
        }
    }
}
