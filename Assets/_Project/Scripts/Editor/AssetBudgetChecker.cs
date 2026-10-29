using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheLightHouse.Editor
{
    public sealed class AssetBudgetChecker : EditorWindow
    {
        [SerializeField,Min(128)] int defaultTextureLimit=1024;
        [SerializeField,Min(128)] int smallPropTextureLimit=512;
        [SerializeField,Min(256)] int skyTextureLimit=2048;
        [SerializeField,Min(100)] int modelTriangleLimit=5000;
        [SerializeField,Min(1)] int realtimeLightLimit=4;
        [SerializeField,Min(1f)] float longAudioSeconds=30f;
        Vector2 scroll;readonly List<string> findings=new();

        [MenuItem("TheLightHouse/Check Asset Budget")]
        static void Open()=>GetWindow<AssetBudgetChecker>("Asset Budget");

        void OnGUI()
        {
            EditorGUILayout.LabelField("MacBook Air M3 / 8 GB",EditorStyles.boldLabel);
            defaultTextureLimit=EditorGUILayout.IntField("Texture max",defaultTextureLimit);smallPropTextureLimit=EditorGUILayout.IntField("Small prop max",smallPropTextureLimit);skyTextureLimit=EditorGUILayout.IntField("Sky/HDRI max",skyTextureLimit);modelTriangleLimit=EditorGUILayout.IntField("Figure/model triangles",modelTriangleLimit);realtimeLightLimit=EditorGUILayout.IntField("Realtime lights / scene",realtimeLightLimit);longAudioSeconds=EditorGUILayout.FloatField("Streaming threshold",longAudioSeconds);
            using(new EditorGUILayout.HorizontalScope()){if(GUILayout.Button("Scan Project"))Scan();if(GUILayout.Button("Auto-fix import settings"))AutoFixImports();}
            EditorGUILayout.HelpBox("Auto-fix changes texture and audio import settings only. It does not alter models, scenes, materials or gameplay.",MessageType.Info);
            scroll=EditorGUILayout.BeginScrollView(scroll);if(findings.Count==0)EditorGUILayout.LabelField("No scan results yet.");foreach(string finding in findings)EditorGUILayout.LabelField(finding,EditorStyles.wordWrappedLabel);EditorGUILayout.EndScrollView();
        }

        void Scan()
        {
            findings.Clear();ScanTextures();ScanModels();ScanAudio();ScanScenes();if(findings.Count==0)findings.Add("PASS — no imported assets exceed the configured budget.");foreach(string finding in findings)Debug.Log(finding);Repaint();
        }

        void ScanTextures()
        {
            foreach(string guid in AssetDatabase.FindAssets("t:Texture")){string path=AssetDatabase.GUIDToAssetPath(guid);if(path.StartsWith("Packages/"))continue;var texture=AssetDatabase.LoadAssetAtPath<Texture>(path);var importer=AssetImporter.GetAtPath(path) as TextureImporter;if(!texture||importer==null)continue;int limit=TextureLimit(path);if(texture.width>limit||texture.height>limit||importer.maxTextureSize>limit)findings.Add($"TEXTURE {path}: {texture.width}x{texture.height}, importer max {importer.maxTextureSize}, budget {limit}");}
        }

        void ScanModels()
        {
            foreach(string guid in AssetDatabase.FindAssets("t:Model")){string path=AssetDatabase.GUIDToAssetPath(guid);if(path.StartsWith("Packages/"))continue;GameObject root=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(!root)continue;var meshes=new HashSet<Mesh>();foreach(var filter in root.GetComponentsInChildren<MeshFilter>(true))if(filter.sharedMesh)meshes.Add(filter.sharedMesh);foreach(var skin in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))if(skin.sharedMesh)meshes.Add(skin.sharedMesh);long triangles=meshes.Sum(mesh=>mesh.triangles.LongLength/3);if(triangles>modelTriangleLimit)findings.Add($"MODEL {path}: {triangles} triangles, budget {modelTriangleLimit}; author two LODs for figures.");}
        }

        void ScanAudio()
        {
            foreach(string guid in AssetDatabase.FindAssets("t:AudioClip")){string path=AssetDatabase.GUIDToAssetPath(guid);if(path.StartsWith("Packages/"))continue;var clip=AssetDatabase.LoadAssetAtPath<AudioClip>(path);var importer=AssetImporter.GetAtPath(path) as AudioImporter;if(!clip||importer==null)continue;AudioImporterSampleSettings sample=importer.defaultSampleSettings;bool longFile=clip.length>=longAudioSeconds;if(sample.compressionFormat==AudioCompressionFormat.PCM||sample.loadType==AudioClipLoadType.DecompressOnLoad||(longFile&&sample.loadType!=AudioClipLoadType.Streaming))findings.Add($"AUDIO {path}: {clip.length:0.0}s, {sample.compressionFormat}/{sample.loadType}; use compressed {(longFile?"streaming":"memory")}.");if(IsAmbience(path)&&!importer.forceToMono)findings.Add($"AUDIO {path}: ambience should be mono.");}
        }

        void ScanScenes()
        {
            SceneSetup[] setup=EditorSceneManager.GetSceneManagerSetup();try{foreach(string guid in AssetDatabase.FindAssets("t:Scene",new[]{"Assets/_Project/Scenes"})){string path=AssetDatabase.GUIDToAssetPath(guid);Scene scene=SceneManager.GetSceneByPath(path);bool opened=scene.IsValid()&&scene.isLoaded;if(!opened)scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);int count=scene.GetRootGameObjects().SelectMany(root=>root.GetComponentsInChildren<Light>(true)).Count(light=>light.enabled&&light.lightmapBakeType!=LightmapBakeType.Baked);if(count>realtimeLightLimit)findings.Add($"SCENE {path}: {count} realtime/mixed lights, budget {realtimeLightLimit}.");if(!opened)EditorSceneManager.CloseScene(scene,true);}}finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
        }

        void AutoFixImports()
        {
            int changed=0;foreach(string guid in AssetDatabase.FindAssets("t:Texture")){string path=AssetDatabase.GUIDToAssetPath(guid);if(path.StartsWith("Packages/"))continue;if(AssetImporter.GetAtPath(path) is not TextureImporter importer)continue;int limit=TextureLimit(path);bool dirty=importer.maxTextureSize!=limit||importer.textureCompression!=TextureImporterCompression.Compressed;if(!dirty)continue;importer.maxTextureSize=limit;importer.textureCompression=TextureImporterCompression.Compressed;importer.SaveAndReimport();changed++;}foreach(string guid in AssetDatabase.FindAssets("t:AudioClip")){string path=AssetDatabase.GUIDToAssetPath(guid);if(path.StartsWith("Packages/"))continue;var importer=AssetImporter.GetAtPath(path) as AudioImporter;var clip=AssetDatabase.LoadAssetAtPath<AudioClip>(path);if(importer==null||!clip)continue;AudioImporterSampleSettings sample=importer.defaultSampleSettings;sample.compressionFormat=AudioCompressionFormat.Vorbis;sample.loadType=clip.length>=longAudioSeconds?AudioClipLoadType.Streaming:AudioClipLoadType.CompressedInMemory;sample.quality=.65f;importer.defaultSampleSettings=sample;if(IsAmbience(path))importer.forceToMono=true;importer.SaveAndReimport();changed++;}AssetDatabase.Refresh();Debug.Log($"Asset budget auto-fix updated {changed} texture/audio importers.");Scan();
        }

        int TextureLimit(string path){string lower=path.ToLowerInvariant();if(lower.Contains("sky")||lower.Contains("hdri")||lower.Contains("cubemap"))return skyTextureLimit;if(lower.Contains("/props/")||lower.Contains("smallprop")||lower.Contains("small_prop"))return smallPropTextureLimit;return defaultTextureLimit;}
        static bool IsAmbience(string path){string lower=path.ToLowerInvariant();return lower.Contains("ambience")||lower.Contains("ambient")||lower.Contains("atmosphere");}
    }
}
