using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace TheLightHouse.Editor
{
    public static class ApplyGraphics1
    {
        const string RootName="[Graphics 1]";
        const string SettingsFolder="Assets/_Project/Settings";
        const string MaterialFolder="Assets/_Project/Materials/Graphics1";
        const string MoodScene="Assets/_Project/Scenes/MoodTest.unity";
        static readonly string[] ActScenes={"Assets/_Project/Scenes/Act1.unity","Assets/_Project/Scenes/Act2.unity","Assets/_Project/Scenes/Act3.unity","Assets/_Project/Scenes/Act4.unity","Assets/_Project/Scenes/Act5.unity"};

        [MenuItem("TheLightHouse/Apply Graphics 1 (Atmosphere)")]
        public static void Apply()
        {
            EnsureFolders();
            ConfigureMetalAndQuality();
            AtmosphereSettings settings=CreateAtmosphereSettings();
            VolumeProfile[] profiles=CreateProfiles();
            Material fogMaterial=CreateMaterial(MaterialFolder+"/Fog.mat","Universal Render Pipeline/Particles/Unlit",new Color(.5f,.57f,.63f,.13f));
            Material beamMaterial=CreateMaterial(MaterialFolder+"/LampBeam.mat","TheLightHouse/Lamp Beam URP",new Color(1f,.68f,.25f,.13f));
            Mesh cone=CreateConeMesh();
            Sprite glow=CreateGlowSprite();
            foreach(string path in ActScenes)if(File.Exists(path))ApplyToScene(path,settings,profiles,fogMaterial,beamMaterial,cone,glow);
            BuildMoodTest(settings,profiles,fogMaterial,beamMaterial,cone,glow);
            ConfigureLightmapping();
            AssetDatabase.SaveAssets();AssetDatabase.Refresh();
            Debug.Log("Graphics 1 applied to Acts 1-5 and MoodTest. Lighting is configured but intentionally not baked in this command.");
        }

        static void EnsureFolders()
        {
            Directory.CreateDirectory(SettingsFolder);Directory.CreateDirectory(MaterialFolder);Directory.CreateDirectory("Assets/_Project/Scenes");AssetDatabase.Refresh();
        }

        static void ConfigureMetalAndQuality()
        {
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneOSX,false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneOSX,new[]{GraphicsDeviceType.Metal});
            var source=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Settings/PC_RPAsset.asset");
            var mac=ClonePipeline(source,SettingsFolder+"/URP_Mac.asset",1f,28f,2);
            var low=ClonePipeline(source,SettingsFolder+"/URP_Low.asset",.75f,20f,1);
            Object qualityAsset=AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/QualitySettings.asset")[0];
            var quality=new SerializedObject(qualityAsset);SerializedProperty levels=quality.FindProperty("m_QualitySettings");
            int macIndex=EnsureQuality(levels,"Mac",mac,2,28f);EnsureQuality(levels,"Low",low,1,20f);
            quality.FindProperty("m_CurrentQuality").intValue=macIndex;quality.ApplyModifiedPropertiesWithoutUndo();
        }

        static UniversalRenderPipelineAsset ClonePipeline(UniversalRenderPipelineAsset source,string path,float renderScale,float shadowDistance,int lightLimit)
        {
            var asset=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(path);
            if(!asset){asset=Object.Instantiate(source);asset.name=Path.GetFileNameWithoutExtension(path);AssetDatabase.CreateAsset(asset,path);}
            var serialized=new SerializedObject(asset);
            Set(serialized,"m_RenderScale",renderScale);Set(serialized,"m_SupportsHDR",true);Set(serialized,"m_ShadowDistance",shadowDistance);Set(serialized,"m_ShadowCascadeCount",1);Set(serialized,"m_SoftShadowsSupported",false);Set(serialized,"m_AdditionalLightsPerObjectLimit",lightLimit);Set(serialized,"m_AdditionalLightShadowsSupported",false);Set(serialized,"m_MainLightShadowmapResolution",1024);Set(serialized,"m_AdditionalLightsShadowmapResolution",512);Set(serialized,"m_MSAA",1);serialized.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(asset);return asset;
        }

        static int EnsureQuality(SerializedProperty levels,string name,RenderPipelineAsset pipeline,int pixelLights,float shadowDistance)
        {
            int index=-1;for(int i=0;i<levels.arraySize;i++)if(levels.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue==name){index=i;break;}
            if(index<0){levels.InsertArrayElementAtIndex(levels.arraySize);index=levels.arraySize-1;}
            SerializedProperty level=levels.GetArrayElementAtIndex(index);level.FindPropertyRelative("name").stringValue=name;level.FindPropertyRelative("pixelLightCount").intValue=pixelLights;level.FindPropertyRelative("shadows").intValue=1;level.FindPropertyRelative("shadowResolution").intValue=1;level.FindPropertyRelative("shadowCascades").intValue=0;level.FindPropertyRelative("shadowDistance").floatValue=shadowDistance;level.FindPropertyRelative("antiAliasing").intValue=0;level.FindPropertyRelative("realtimeReflectionProbes").boolValue=false;level.FindPropertyRelative("customRenderPipeline").objectReferenceValue=pipeline;return index;
        }

        static AtmosphereSettings CreateAtmosphereSettings()
        {
            string path=SettingsFolder+"/AtmosphereSettings.asset";var settings=AssetDatabase.LoadAssetAtPath<AtmosphereSettings>(path);if(!settings){settings=ScriptableObject.CreateInstance<AtmosphereSettings>();AssetDatabase.CreateAsset(settings,path);}
            settings.blendSeconds=3f;
            settings.dusk=Preset(new Color(.18f,.16f,.18f),.012f,new Color(.20f,.16f,.14f),new Color(1f,.61f,.38f),.75f,.8f,.9f);
            settings.night=Preset(new Color(.045f,.065f,.10f),.021f,new Color(.035f,.045f,.075f),new Color(.35f,.45f,.70f),.18f,.35f,1f);
            settings.storm=Preset(new Color(.04f,.055f,.07f),.028f,new Color(.025f,.035f,.055f),new Color(.26f,.34f,.50f),.12f,.25f,1f);
            settings.finale=Preset(new Color(.055f,.06f,.075f),.033f,new Color(.035f,.035f,.05f),new Color(.34f,.38f,.52f),.10f,.22f,1f);
            settings.dawn=Preset(new Color(.32f,.37f,.42f),.009f,new Color(.23f,.22f,.22f),new Color(1f,.72f,.54f),.65f,.85f,.85f);
            EditorUtility.SetDirty(settings);return settings;
        }

        static AtmospherePreset Preset(Color fog,float density,Color ambient,Color sun,float intensity,float exposure,float weight)=>new(){fogColor=fog,fogDensity=density,ambientColor=ambient,directionalLightColor=sun,directionalLightIntensity=intensity,skyboxExposure=exposure,postProcessWeight=weight};

        static VolumeProfile[] CreateProfiles()
        {
            string[] names={"Dusk","Night","Storm","Finale","Dawn"};float[] exposure={-.15f,-.55f,-.7f,-.8f,.05f};float[] bloom={.35f,.45f,.42f,.5f,.3f};float[] vignette={.2f,.28f,.31f,.34f,.16f};
            var profiles=new VolumeProfile[names.Length];for(int i=0;i<names.Length;i++){string path=SettingsFolder+"/Volume_"+names[i]+".asset";var profile=AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);if(!profile){profile=ScriptableObject.CreateInstance<VolumeProfile>();AssetDatabase.CreateAsset(profile,path);}ConfigureProfile(profile,exposure[i],bloom[i],vignette[i]);profiles[i]=profile;}return profiles;
        }

        static void ConfigureProfile(VolumeProfile profile,float exposure,float bloom,float vignette)
        {
            foreach(var component in profile.components.ToArray())Object.DestroyImmediate(component,true);
            var tone=profile.Add<Tonemapping>(true);tone.mode.Override(TonemappingMode.ACES);
            var color=profile.Add<ColorAdjustments>(true);color.postExposure.Override(exposure);color.contrast.Override(8f);color.saturation.Override(-18f);color.colorFilter.Override(new Color(.92f,.96f,1f));
            var split=profile.Add<SplitToning>(true);split.shadows.Override(new Color(.30f,.43f,.62f));split.highlights.Override(new Color(1f,.74f,.53f));split.balance.Override(-18f);
            var glow=profile.Add<Bloom>(true);glow.intensity.Override(bloom);glow.threshold.Override(.9f);glow.scatter.Override(.55f);
            var edge=profile.Add<Vignette>(true);edge.intensity.Override(vignette);edge.smoothness.Override(.45f);
            var grain=profile.Add<FilmGrain>(true);grain.type.Override(FilmGrainLookup.Thin1);grain.intensity.Override(.12f);grain.response.Override(.75f);EditorUtility.SetDirty(profile);
        }

        static void ApplyToScene(string path,AtmosphereSettings settings,VolumeProfile[] profiles,Material fogMaterial,Material beamMaterial,Mesh cone,Sprite glow)
        {
            Scene scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);ApplyToOpenScene(settings,profiles,fogMaterial,beamMaterial,cone,glow,false);ConfigureLightmapping();EditorSceneManager.SaveScene(scene,path);
        }

        static AtmosphereController ApplyToOpenScene(AtmosphereSettings settings,VolumeProfile[] profiles,Material fogMaterial,Material beamMaterial,Mesh cone,Sprite glow,bool mood)
        {
            var old=GameObject.Find(RootName);if(old)Object.DestroyImmediate(old);
            var root=new GameObject(RootName);Light sun=Object.FindAnyObjectByType<DuskTimer>()?.GetComponentInChildren<Light>();if(!sun)sun=Object.FindAnyObjectByType<StormController>()?.GetComponentInChildren<Light>();if(!sun)sun=Object.FindObjectsByType<Light>().FirstOrDefault(l=>l.type==LightType.Directional);
            var controller=root.AddComponent<AtmosphereController>();Set(controller,"settings",settings);Set(controller,"directionalLight",sun);Set(controller,"duskTimer",Object.FindAnyObjectByType<DuskTimer>());Set(controller,"stormController",Object.FindAnyObjectByType<StormController>());Set(controller,"dawnController",Object.FindAnyObjectByType<DawnController>());
            string[] names={"Dusk","Night","Storm","Finale","Dawn"};string[] fields={"duskVolume","nightVolume","stormVolume","finaleVolume","dawnVolume"};for(int i=0;i<profiles.Length;i++){var volumeObject=new GameObject(names[i]+" Volume");volumeObject.transform.SetParent(root.transform);var volume=volumeObject.AddComponent<Volume>();volume.isGlobal=true;volume.priority=i;volume.profile=profiles[i];volume.weight=i==0?1f:0f;Set(controller,fields[i],volume);}
            AddFog(root.transform,fogMaterial);
            AddProbes(root.transform);
            AddWarmLights(root.transform);
            AddLampBeam(root.transform,beamMaterial,cone,glow);
            var wet=root.AddComponent<WetLookController>();Set(wet,"stormController",Object.FindAnyObjectByType<StormController>());
            foreach(Camera camera in Object.FindObjectsByType<Camera>()){var data=camera.GetUniversalAdditionalCameraData();data.renderPostProcessing=true;}
            MarkStaticGeometry();
            if(mood){var switcher=root.AddComponent<MoodStateSwitcher>();Set(switcher,"atmosphere",controller);}
            return controller;
        }

        static void AddFog(Transform root,Material material)
        {
            var fogObject=new GameObject("Fog Cards");fogObject.transform.SetParent(root);var fog=fogObject.AddComponent<FogCards>();Set(fog,"fogMaterial",material);Vector3[] positions={new(0,1,-23),new(0,1,-12),new(-8,.7f,3)};var anchors=new Transform[positions.Length];for(int i=0;i<positions.Length;i++){var anchor=new GameObject("Fog Anchor "+(i+1));anchor.transform.SetParent(fogObject.transform);anchor.transform.position=positions[i];anchors[i]=anchor.transform;}SetArray(fog,"anchors",anchors);
        }

        static void AddProbes(Transform root)
        {
            Vector3[] centers={new(0,2,0),new(0,5,6),new(0,2,-12)};string[] names={"Interior","Lantern Room","Courtyard"};for(int i=0;i<centers.Length;i++){var probes=new GameObject(names[i]+" Light Probes");probes.transform.SetParent(root);probes.transform.position=centers[i];var group=probes.AddComponent<LightProbeGroup>();group.probePositions=ProbeCube(new Vector3(5f,2f,5f));var reflection=probes.AddComponent<ReflectionProbe>();reflection.mode=ReflectionProbeMode.Baked;reflection.resolution=64;reflection.size=new Vector3(10f,5f,10f);reflection.intensity=.65f;}
        }

        static Vector3[] ProbeCube(Vector3 extents)=>new[]{new Vector3(-extents.x,-extents.y,-extents.z),new Vector3(extents.x,-extents.y,-extents.z),new Vector3(-extents.x,extents.y,-extents.z),new Vector3(extents.x,extents.y,-extents.z),new Vector3(-extents.x,-extents.y,extents.z),new Vector3(extents.x,-extents.y,extents.z),new Vector3(-extents.x,extents.y,extents.z),new Vector3(extents.x,extents.y,extents.z)};

        static void AddWarmLights(Transform root)
        {
            Vector3[] positions={new(-4f,2f,-3f),new(0f,5.4f,6f)};foreach(Vector3 position in positions){var lightObject=new GameObject("Warm Lantern");lightObject.transform.SetParent(root);lightObject.transform.position=position;var light=lightObject.AddComponent<Light>();light.type=LightType.Point;light.color=new Color(1f,.55f,.25f);light.range=7f;light.intensity=2f;light.shadows=LightShadows.None;var flicker=lightObject.AddComponent<FlickerLight>();Set(flicker,"target",light);}
        }

        static void AddLampBeam(Transform root,Material material,Mesh cone,Sprite glow)
        {
            LampController lamp=Object.FindAnyObjectByType<LampController>();if(!lamp)return;var visualObject=new GameObject("Lamp Beam Visual");visualObject.transform.SetParent(root);visualObject.transform.position=lamp.transform.position;var pivot=new GameObject("Beam Pivot");pivot.transform.SetParent(visualObject.transform,false);var spotObject=new GameObject("Beam Spot");spotObject.transform.SetParent(pivot.transform,false);spotObject.transform.localRotation=Quaternion.Euler(0f,0f,0f);var spot=spotObject.AddComponent<Light>();spot.type=LightType.Spot;spot.spotAngle=32f;spot.innerSpotAngle=20f;spot.color=new Color(1f,.72f,.38f);spot.shadows=LightShadows.Hard;spot.cookie=AssetDatabase.LoadAssetAtPath<Texture2D>(MaterialFolder+"/LampGlow.asset");
            var coneObject=new GameObject("Soft Beam Cone",typeof(MeshFilter),typeof(MeshRenderer));coneObject.transform.SetParent(pivot.transform,false);coneObject.GetComponent<MeshFilter>().sharedMesh=cone;coneObject.GetComponent<MeshRenderer>().sharedMaterial=material;
            var glowObject=new GameObject("Lamp Glow",typeof(SpriteRenderer));glowObject.transform.SetParent(visualObject.transform,false);var sprite=glowObject.GetComponent<SpriteRenderer>();sprite.sprite=glow;sprite.color=new Color(1f,.65f,.25f,.8f);glowObject.transform.localScale=Vector3.one*1.3f;
            var visual=visualObject.AddComponent<LampBeamVisual>();Set(visual,"beamPivot",pivot.transform);Set(visual,"spotLight",spot);Set(visual,"coneRenderer",coneObject.GetComponent<Renderer>());Set(visual,"glowSprite",sprite);
        }

        static void MarkStaticGeometry()
        {
            foreach(MeshRenderer renderer in Object.FindObjectsByType<MeshRenderer>()){string name=renderer.gameObject.name.ToLowerInvariant();if(name.Contains("player")||name.Contains("door")||name.Contains("stalker")||name.Contains("drowned")||name.Contains("figure")||name.Contains("crane")||name.Contains("boat")||name.Contains("lamp"))continue;if(renderer.GetComponentInParent<Rigidbody>()||renderer.GetComponentInParent<CharacterController>())continue;GameObjectUtility.SetStaticEditorFlags(renderer.gameObject,StaticEditorFlags.ContributeGI|StaticEditorFlags.BatchingStatic|StaticEditorFlags.ReflectionProbeStatic);}
        }

        static void ConfigureLightmapping()
        {
#pragma warning disable 618
            Lightmapping.giWorkflowMode=Lightmapping.GIWorkflowMode.OnDemand;LightmapEditorSettings.lightmapper=SystemInfo.supportsComputeShaders?LightmapEditorSettings.Lightmapper.ProgressiveGPU:LightmapEditorSettings.Lightmapper.ProgressiveCPU;LightmapEditorSettings.bakeResolution=18f;LightmapEditorSettings.maxAtlasSize=1024;LightmapEditorSettings.padding=2;
#pragma warning restore 618
        }

        static void BuildMoodTest(AtmosphereSettings settings,VolumeProfile[] profiles,Material fogMaterial,Material beamMaterial,Mesh cone,Sprite glow)
        {
            Scene scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);Cube("Ground",new(0,-.25f,0),new(35,.5f,35));Cube("Tower",new(0,4,4),new(10,8,10));Cube("Dock",new(0,.1f,-13),new(5,.3f,14));var figure=Cube("Figure Silhouette",new(5,1,-11),new(.7f,2.2f,.5f));var renderer=figure.GetComponent<Renderer>();var silhouette=CreateMaterial(MaterialFolder+"/Silhouette.mat","Universal Render Pipeline/Lit",new Color(.015f,.02f,.025f));renderer.sharedMaterial=silhouette;
            var cameraObject=new GameObject("Mood Camera",typeof(Camera));cameraObject.tag="MainCamera";cameraObject.transform.position=new(13,7,-20);cameraObject.transform.LookAt(new Vector3(0,3,0));
            var sunObject=new GameObject("Dusk Sun",typeof(Light));var sun=sunObject.GetComponent<Light>();sun.type=LightType.Directional;sun.transform.rotation=Quaternion.Euler(28,-35,0);
            var lampObject=new GameObject("Lamp",typeof(Light),typeof(LampController));lampObject.transform.position=new(0,7,4);ApplyToOpenScene(settings,profiles,fogMaterial,beamMaterial,cone,glow,true);ConfigureLightmapping();EditorSceneManager.SaveScene(scene,MoodScene);
        }

        static GameObject Cube(string name,Vector3 position,Vector3 scale){var cube=GameObject.CreatePrimitive(PrimitiveType.Cube);cube.name=name;cube.transform.position=position;cube.transform.localScale=scale;return cube;}

        static Material CreateMaterial(string path,string shaderName,Color color){var material=AssetDatabase.LoadAssetAtPath<Material>(path);if(!material){Shader shader=Shader.Find(shaderName);material=new Material(shader){name=Path.GetFileNameWithoutExtension(path)};AssetDatabase.CreateAsset(material,path);}if(material.HasProperty("_BaseColor"))material.SetColor("_BaseColor",color);if(material.HasProperty("_BeamColor"))material.SetColor("_BeamColor",color);if(shaderName.Contains("Particles")){material.SetFloat("_Surface",1f);material.SetFloat("_ZWrite",0f);material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");material.renderQueue=(int)RenderQueue.Transparent;}EditorUtility.SetDirty(material);return material;}

        static Mesh CreateConeMesh()
        {
            string path=MaterialFolder+"/LampCone.asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(mesh)return mesh;const int segments=24;var vertices=new Vector3[segments+1];var uv=new Vector2[segments+1];var triangles=new int[segments*3];vertices[0]=Vector3.zero;uv[0]=new Vector2(0f,0f);for(int i=0;i<segments;i++){float angle=Mathf.PI*2f*i/segments;vertices[i+1]=new Vector3(Mathf.Cos(angle)*.18f,Mathf.Sin(angle)*.18f,1f);uv[i+1]=new Vector2(1f,1f);int next=(i+1)%segments;triangles[i*3]=0;triangles[i*3+1]=i+1;triangles[i*3+2]=next+1;}mesh=new Mesh{name="Lamp Cone"};mesh.vertices=vertices;mesh.uv=uv;mesh.triangles=triangles;mesh.RecalculateNormals();mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,path);return mesh;
        }

        static Sprite CreateGlowSprite()
        {
            string path=MaterialFolder+"/LampGlow.asset";var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);if(sprite)return sprite;const int size=64;var texture=new Texture2D(size,size,TextureFormat.RGBA32,false){name="Lamp Glow Texture",filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};for(int y=0;y<size;y++)for(int x=0;x<size;x++){float dx=(x+.5f)/size*2f-1f,dy=(y+.5f)/size*2f-1f;float alpha=Mathf.Pow(Mathf.Clamp01(1f-Mathf.Sqrt(dx*dx+dy*dy)),2f);texture.SetPixel(x,y,new Color(1f,1f,1f,alpha));}texture.Apply();AssetDatabase.CreateAsset(texture,path);sprite=Sprite.Create(texture,new Rect(0,0,size,size),new Vector2(.5f,.5f),size);sprite.name="Lamp Glow";AssetDatabase.AddObjectToAsset(sprite,texture);AssetDatabase.ImportAsset(path);return sprite;
        }

        static void Set(SerializedObject serialized,string name,float value){var property=serialized.FindProperty(name);if(property!=null)property.floatValue=value;}
        static void Set(SerializedObject serialized,string name,int value){var property=serialized.FindProperty(name);if(property!=null)property.intValue=value;}
        static void Set(SerializedObject serialized,string name,bool value){var property=serialized.FindProperty(name);if(property!=null)property.boolValue=value;}
        static void Set(Object target,string name,Object value){var serialized=new SerializedObject(target);var property=serialized.FindProperty(name);if(property!=null){property.objectReferenceValue=value;serialized.ApplyModifiedPropertiesWithoutUndo();}}
        static void SetArray(Object target,string name,Transform[] values){var serialized=new SerializedObject(target);var property=serialized.FindProperty(name);property.arraySize=values.Length;for(int i=0;i<values.Length;i++)property.GetArrayElementAtIndex(i).objectReferenceValue=values[i];serialized.ApplyModifiedPropertiesWithoutUndo();}
    }
}
