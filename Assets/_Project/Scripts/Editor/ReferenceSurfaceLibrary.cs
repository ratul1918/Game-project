using System.IO;
using UnityEditor;
using UnityEngine;

namespace TheLightHouse.Editor
{
    public static class ReferenceSurfaceLibrary
    {
        public const string Folder = "Assets/_Project/Materials/Reference";
        public static Material Create(string name, Color tint, int pattern, float wear, float metallic = 0, float smoothness = .24f)
        {
            Directory.CreateDirectory(Folder);
            string texturePath = $"{Folder}/{name}.png";
            const int size = 512;
            var texture = new Texture2D(size, size, TextureFormat.RGB24, true);
            var colors = new Color[size * size];
            for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
            {
                float u = x / (float)size, v = y / (float)size;
                float broad = Mathf.PerlinNoise(u * 7 + 13, v * 7 + 7);
                float fine = Mathf.PerlinNoise(u * 140 + 6, v * 140 + 12);
                float streak = Mathf.PerlinNoise(u * 44 + 4, v * 3 + 8);
                float value = .75f + broad * .18f + (fine - .5f) * .4f * wear;
                Color c = tint;
                if (pattern == 1)
                {
                    float seam = Mathf.Repeat(u * 8, 1);
                    float grain = Mathf.PerlinNoise(u * 160, v * 8);
                    value = .45f + grain * .6f + broad * .24f;
                    if (seam < .024f || Mathf.Repeat(v * 3 + Mathf.Floor(u * 8) * .37f, 1) < .015f) value *= .25f;
                }
                else if (pattern == 2)
                {
                    c = Color.Lerp(tint, new Color(.24f, .13f, .07f), Mathf.SmoothStep(.52f, .8f, broad) * wear);
                    value = .6f + fine * .5f;
                }
                else
                {
                    float damp = Mathf.Clamp01((.50f - broad) * 2 + (.42f - streak) * .5f) * wear;
                    c = Color.Lerp(tint, new Color(.065f, .11f, .09f), damp);
                    value += (fine - .5f) * .6f;
                    c *= 1f - Mathf.Clamp01((.32f - broad) * 3f) * .18f;
                }
                colors[y * size + x] = c * value;
            }
            texture.SetPixels(colors); texture.Apply(); File.WriteAllBytes(texturePath, texture.EncodeToPNG()); Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(texturePath);
            var importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);
            importer.maxTextureSize = 512; importer.mipmapEnabled = true; importer.wrapMode = TextureWrapMode.Repeat; importer.textureCompression = TextureImporterCompression.Compressed; importer.SaveAndReimport();
            string normalPath = $"{Folder}/{name} Normal.png";
            File.Copy(texturePath, normalPath, true); AssetDatabase.ImportAsset(normalPath);
            var normalImporter = (TextureImporter)AssetImporter.GetAtPath(normalPath);
            normalImporter.textureType = TextureImporterType.NormalMap; normalImporter.convertToNormalmap = true; normalImporter.heightmapScale = pattern == 1 ? .025f : .035f;
            normalImporter.maxTextureSize=512;normalImporter.mipmapEnabled=true;normalImporter.wrapMode=TextureWrapMode.Repeat;normalImporter.SaveAndReimport();
            var material = AssetDatabase.LoadAssetAtPath<Material>($"{Folder}/{name}.mat");
            if (!material) { material = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(material, $"{Folder}/{name}.mat"); }
            material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath));
            material.SetTexture("_BumpMap", AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath)); material.SetFloat("_BumpScale", .55f); material.EnableKeyword("_NORMALMAP");
            material.SetColor("_BaseColor", Color.white); material.SetFloat("_Metallic", metallic); material.SetFloat("_Smoothness", smoothness); material.enableInstancing = true;
            EditorUtility.SetDirty(material); return material;
        }
        public static Material Grating(Material iron)
        {
            string path=$"{Folder}/Iron Grating.mat",texturePath=$"{Folder}/Grating.png";
            var texture=new Texture2D(128,128,TextureFormat.RGBA32,true);
            for(int y=0;y<128;y++)for(int x=0;x<128;x++){bool bar=(x+y)%16<3 || (x-y+128)%16<3;texture.SetPixel(x,y,new Color(.2f,.27f,.28f,bar?1:0));}
            texture.Apply();File.WriteAllBytes(texturePath,texture.EncodeToPNG());Object.DestroyImmediate(texture);AssetDatabase.ImportAsset(texturePath);
            var importer=(TextureImporter)AssetImporter.GetAtPath(texturePath);importer.alphaSource=TextureImporterAlphaSource.FromInput;importer.maxTextureSize=128;importer.mipmapEnabled=true;importer.alphaIsTransparency=true;importer.SaveAndReimport();
            var material=AssetDatabase.LoadAssetAtPath<Material>(path);if(!material){material=new Material(iron);AssetDatabase.CreateAsset(material,path);}
            material.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath));material.SetFloat("_AlphaClip",1);material.SetFloat("_Cutoff",.5f);material.SetFloat("_Cull",0);material.EnableKeyword("_ALPHATEST_ON");material.renderQueue=2450;EditorUtility.SetDirty(material);return material;
        }
        public static Material Solid(string name, Color color, float emission = 0)
        {
            var m = AssetDatabase.LoadAssetAtPath<Material>($"{Folder}/{name}.mat");
            if (!m) { m = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(m, $"{Folder}/{name}.mat"); }
            m.color = color;
            if (emission > 0) { m.EnableKeyword("_EMISSION"); m.SetColor("_EmissionColor", color * emission); }
            m.enableInstancing = true; EditorUtility.SetDirty(m); return m;
        }
    }
}
