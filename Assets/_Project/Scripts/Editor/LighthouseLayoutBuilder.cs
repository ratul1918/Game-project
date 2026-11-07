using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Object=UnityEngine.Object;

namespace TheLightHouse.Editor
{
    public static class LighthouseLayoutBuilder
    {
        const string RootName="Lighthouse Layout";
        const string PrefabFolder="Assets/_Project/Prefabs/Level";
        const string MaterialFolder="Assets/_Project/Materials/Level";
        const string SettingsFolder="Assets/_Project/Settings/UI";
        static readonly Dictionary<string,Material> Materials=new();
        static LighthouseLayoutSettings dimensions;

        public static void BuildSharedLayout(string actName)
        {
            EnsureAssets();
            dimensions=EnsureLayoutSettings();
            RemoveLegacyGeometry();
            Transform root=new GameObject(RootName).transform;
            BuildExterior(root);
            BuildTower(root);
            BuildRooms(root);
            BuildStairs(root);
            BuildWindows(root);
            BuildRoomVolumes(root);
            BuildHidingSpots(root);
            BuildWaypoints(root);
            BuildOcclusion(root);
            RelocateGameplayAnchors(actName);
            TunePlayerMovement();
            BuildHud();
            FixParticleMaterials();
            ReportMaterialProblems();
            EditorUtility.SetDirty(root.gameObject);
        }

        static void EnsureAssets()
        {
            Directory.CreateDirectory(PrefabFolder);Directory.CreateDirectory(MaterialFolder);Directory.CreateDirectory(SettingsFolder);AssetDatabase.Refresh();
            ConfigureUiAssets();
            EnsureTag("Window");
            Material("Stone",new(.19f,.21f,.22f),false);Material("Wood",new(.24f,.16f,.1f),false);Material("Metal",new(.22f,.25f,.27f),false);Material("Sand",new(.32f,.29f,.22f),false);Material("Water",new(.06f,.16f,.2f,.72f),true);Material("Particle",new(.62f,.68f,.7f,.35f),true,true);
            Prefab("Floor",new(1,.25f,1),"Stone");Prefab("Wall",new(1,1,0.22f),"Stone");Prefab("Door Frame",new(1,1,.25f),"Wood");Prefab("Window",new(1,1,.12f),"Metal");Prefab("Stair",new(1,.2f,.3f),"Wood");Prefab("Curved Wall",new(1.45f,1,.22f),"Stone");
            UITheme theme=AssetDatabase.LoadAssetAtPath<UITheme>($"{SettingsFolder}/UITheme.asset");if(!theme){theme=ScriptableObject.CreateInstance<UITheme>();AssetDatabase.CreateAsset(theme,$"{SettingsFolder}/UITheme.asset");}
            HudSettings hud=AssetDatabase.LoadAssetAtPath<HudSettings>($"{SettingsFolder}/HudSettings.asset");if(!hud){hud=ScriptableObject.CreateInstance<HudSettings>();AssetDatabase.CreateAsset(hud,$"{SettingsFolder}/HudSettings.asset");}
            AssignThemeSprites(theme);EditorUtility.SetDirty(theme);AssetDatabase.SaveAssets();
        }

        static void ConfigureUiAssets()
        {
            foreach(string guid in AssetDatabase.FindAssets("t:Texture2D",new[]{"Assets/_Project/Art/UI"}))
            {
                string path=AssetDatabase.GUIDToAssetPath(guid);
                TextureImporter importer=AssetImporter.GetAtPath(path) as TextureImporter;
                if(!importer)continue;
                TextureImporterCompression compression=TextureImporterCompression.Compressed;
                int maxSize=path.Contains("/Icons/")||path.Contains("/icons/")?128:1024;
                bool changed=importer.textureType!=TextureImporterType.Sprite||importer.spriteImportMode!=SpriteImportMode.Single||importer.mipmapEnabled||importer.maxTextureSize!=maxSize||importer.textureCompression!=compression;
                if(!changed)continue;
                importer.textureType=TextureImporterType.Sprite;
                importer.spriteImportMode=SpriteImportMode.Single;
                importer.mipmapEnabled=false;
                importer.maxTextureSize=maxSize;
                importer.textureCompression=compression;
                importer.SaveAndReimport();
            }
        }

        static void TunePlayerMovement()
        {
            const string path="Assets/_Project/Resources/PlayerMovementSettings.asset";
            PlayerMovementSettings settings=AssetDatabase.LoadAssetAtPath<PlayerMovementSettings>(path);
            if(!settings)return;
            settings.stepOffset=Mathf.Min(settings.stepOffset,.2f);
            settings.slopeLimit=Mathf.Min(settings.slopeLimit,38f);
            EditorUtility.SetDirty(settings);
        }

        static LighthouseLayoutSettings EnsureLayoutSettings()
        {
            const string path="Assets/_Project/Settings/LighthouseLayoutSettings.asset";
            LighthouseLayoutSettings value=AssetDatabase.LoadAssetAtPath<LighthouseLayoutSettings>(path);
            if(!value){value=ScriptableObject.CreateInstance<LighthouseLayoutSettings>();AssetDatabase.CreateAsset(value,path);}
            EditorUtility.SetDirty(value);AssetDatabase.SaveAssets();return value;
        }

        static void EnsureTag(string tag)
        {
            Object[] assets=AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");if(assets.Length==0)return;SerializedObject manager=new(assets[0]);SerializedProperty tags=manager.FindProperty("tags");
            for(int i=0;i<tags.arraySize;i++)if(tags.GetArrayElementAtIndex(i).stringValue==tag)return;
            tags.InsertArrayElementAtIndex(tags.arraySize);tags.GetArrayElementAtIndex(tags.arraySize-1).stringValue=tag;manager.ApplyModifiedPropertiesWithoutUndo();
        }

        static void AssignThemeSprites(UITheme theme)
        {
            Set(theme,"keySprite",FindSprite("button1"));
            Set(theme,"itemSprite",FindSprite("open"));
            Set(theme,"checkpointSprite",FindSprite("save"));
            Set(theme,"pageSprite",FindSprite("menuList"));
            Set(theme,"lampSprite",FindSprite("power"));
            Set(theme,"bodyFont",FindFont("kenney_input_generic"));
            Set(theme,"displayFont",FindFont("Kenney Future"));
            Set(theme,"journalFont",FindFont("Kenney Future"));
            Set(theme,"notesFont",FindFont("kenney_input_generic"));
            Set(theme,"buttonSprite",FindSprite("button"));
            Set(theme,"barFrameSprite",FindSprite("bar"));
            Set(theme,"barFillSprite",FindSprite("fill"));
            Set(theme,"toastSprite",FindSprite("panel"));
            Set(theme,"tabSprite",FindSprite("button"));
            Set(theme,"fuelSprite",FindSprite("fuel"));
            Set(theme,"ledgerSprite",FindSprite("book"));
            Set(theme,"lanternSprite",FindSprite("power"));
            Set(theme,"hideSprite",FindSprite("hide"));
            Set(theme,"crankSprite",FindSprite("power"));
            Set(theme,"interactSprite",FindSprite("button1"));
        }

        static Sprite FindSprite(string name)
        {
            string[] guids=AssetDatabase.FindAssets($"{name} t:Texture2D",new[]{"Assets/_Project/Art/UI"});
            foreach(string guid in guids)
            {
                string path=AssetDatabase.GUIDToAssetPath(guid);
                TextureImporter importer=AssetImporter.GetAtPath(path) as TextureImporter;
                if(importer&&importer.textureType!=TextureImporterType.Sprite)
                {
                    importer.textureType=TextureImporterType.Sprite;
                    importer.SaveAndReimport();
                }
                Sprite sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if(sprite)return sprite;
            }
            return null;
        }

        static TMP_FontAsset FindFont(string name)
        {
            string[] guids=AssetDatabase.FindAssets($"{name} t:TMP_FontAsset",new[]{"Assets/_Project/Art"});
            foreach(string guid in guids)
            {
                TMP_FontAsset font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guid));
                if(font)return font;
            }
            return null;
        }

        static Material Material(string name,Color color,bool transparent,bool particle=false)
        {
            string path=$"{MaterialFolder}/{name}.mat";Material material=AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader shader=Shader.Find(particle?"Universal Render Pipeline/Particles/Unlit":"Universal Render Pipeline/Lit");shader??=Shader.Find("Sprites/Default");
            if(!material){material=new Material(shader){name=name};AssetDatabase.CreateAsset(material,path);}else if(material.shader!=shader)material.shader=shader;
            material.color=color;if(transparent){material.SetFloat("_Surface",1f);material.renderQueue=(int)RenderQueue.Transparent;}EditorUtility.SetDirty(material);Materials[name]=material;return material;
        }

        static void Prefab(string name,Vector3 scale,string surface)
        {
            string path=$"{PrefabFolder}/{name}.prefab";if(AssetDatabase.LoadAssetAtPath<GameObject>(path))return;
            GameObject go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.localScale=scale;go.tag=surface=="Water"?"Water":surface;go.GetComponent<Renderer>().sharedMaterial=Materials[surface];
            PrefabUtility.SaveAsPrefabAsset(go,path);Object.DestroyImmediate(go);
        }

        static void Ring(Transform parent,string name,float y,float diameter,float thickness,float height,int segments,bool doorway)
        {
            float radius=diameter*.5f-thickness*.5f;
            for(int i=0;i<segments;i++)
            {
                float angle=i*360f/segments;
                if(doorway&&Mathf.Abs(Mathf.DeltaAngle(angle,180f))<18f)continue;
                float radians=angle*Mathf.Deg2Rad;
                float tangent=2f*Mathf.PI*radius/segments*dimensions.wallSegmentCoverage;
                GameObject segment=Place($"{name} wall {i:00}",new(Mathf.Sin(radians)*radius,y+height*.5f,Mathf.Cos(radians)*radius),new(tangent,height,thickness/.22f),"Wall","Stone",parent);
                segment.transform.rotation=Quaternion.Euler(0,angle,0);
            }
        }

        static void Band(Transform parent,string name,float y,float diameter,float height,float depth,int segments)
        {
            Ring(parent,name,y,diameter,depth,height,segments,false);
        }

        static void Gallery(Transform parent,float y)
        {
            float radius=dimensions.towerGalleryDiameter*.5f+dimensions.galleryWidth*.5f;
            Place("Gallery Floor",new(0,y-.12f,0),new(dimensions.towerGalleryDiameter+dimensions.galleryWidth*2f,.24f,dimensions.towerGalleryDiameter+dimensions.galleryWidth*2f),"Floor","Metal",parent);
            for(int i=0;i<dimensions.towerWallSegments;i++)
            {
                float angle=i*360f/dimensions.towerWallSegments;float radians=angle*Mathf.Deg2Rad;
                Place($"Gallery Railing {i:00}",new(Mathf.Sin(radians)*radius,y+dimensions.railingHeight*.5f,Mathf.Cos(radians)*radius),new(.12f,dimensions.railingHeight,.12f),"Wall","Metal",parent);
            }
            Ring(parent,"Gallery Rail",y+dimensions.railingHeight,dimensions.towerGalleryDiameter+dimensions.galleryWidth*2f,.12f,.12f,dimensions.towerWallSegments,true);
        }

        static void HouseWall(Transform parent,string name,Vector3 center,Vector2 size,bool horizontal)
        {
            Vector3 scale=horizontal?new(size.x,dimensions.houseSize.y,dimensions.houseWallThickness):new(dimensions.houseWallThickness,dimensions.houseSize.y,size.y);
            Wall(name,new(center.x,dimensions.houseSize.y*.5f,center.z),scale,parent);
        }

        static void BuildExterior(Transform root)
        {
            Transform exterior=Group("Exterior",root);
            Place("Beach",new(0,-.35f,-16),new(46,.35f,42),"Floor","Sand",exterior);
            Place("Sea Plane",new(0,-.55f,-31),new(96,.12f,76),"Floor","Water",exterior);
            Place("Dock",new(0,-.05f,-27),new(6,.35f,12),"Floor","Wood",exterior);
            Place("Cliff Path",new(0,.05f,-18),new(3.5f,.35f,16),"Floor","Stone",exterior);
            Place("Courtyard",new(0,.05f,-10),new(20,.35f,14),"Floor","Stone",exterior);
            Place("Courtyard Wall West",new(-10,1.25f,-10),new(.45f,2.5f,14),"Wall","Stone",exterior);
            Place("Courtyard Wall East",new(10,1.25f,-10),new(.45f,2.5f,14),"Wall","Stone",exterior);
            Place("Boat Shed Floor",new(10,.1f,-14),new(7,.3f,6),"Floor","Wood",exterior);
            Place("Boat Shed Back",new(13.5f,1.5f,-14),new(.35f,3,6),"Wall","Wood",exterior);
            Place("Boat Shed Roof",new(10,3,-14),new(7,.3f,6),"Floor","Wood",exterior);
            for(int i=0;i<dimensions.cliffRockCount;i++){float x=-22+i*5.5f;Place($"Rock South {i}",new(x,.4f,-39+Mathf.Abs(x)*.08f),new(5,1.6f,4),"Floor","Stone",exterior);}
            Place("Cliff West",new(-25,2,0),new(5,5,52),"Wall","Stone",exterior);Place("Cliff East",new(25,2,0),new(5,5,52),"Wall","Stone",exterior);
            Bounds(exterior,new(0,3,11),new(54,8,1));Bounds(exterior,new(-27,3,-14),new(1,8,56));Bounds(exterior,new(27,3,-14),new(1,8,56));Bounds(exterior,new(0,3,-42),new(54,8,1));
        }

        static void BuildTower(Transform root)
        {
            Transform tower=Group("Tower",root);
            for(int level=0;level<=dimensions.towerLevelCount;level++)
            {
                float y=level*dimensions.floorHeight;
                float t=level/(float)Mathf.Max(1,dimensions.towerLevelCount);
                float diameter=Mathf.Lerp(dimensions.towerBaseDiameter,dimensions.towerGalleryDiameter,t);
                FloorRing(tower,$"Tower Floor {level}",y,diameter,level==dimensions.towerLevelCount?"Metal":"Stone");
                if(level==dimensions.towerLevelCount)continue;
                Ring(tower,$"Tower L{level}",y,diameter,Mathf.Lerp(dimensions.towerBaseWallThickness,dimensions.towerGalleryWallThickness,t),dimensions.floorHeight,dimensions.towerWallSegments,level==0);
                Band(tower,$"Tower Cornice {level}",y+dimensions.floorHeight-.16f,diameter+.25f,dimensions.ringCorniceHeight,dimensions.ringCorniceDepth,dimensions.towerWallSegments);
            }
            float galleryY=dimensions.towerLevelCount*dimensions.floorHeight;
            Ring(tower,"Lantern Ring",galleryY,dimensions.towerGalleryDiameter,dimensions.towerGalleryWallThickness,dimensions.floorHeight,dimensions.lanternPaneCount,false);
            Gallery(tower,galleryY);
            Place("Lantern Cap",new(0,galleryY+dimensions.floorHeight+.7f,0),new(dimensions.towerGalleryDiameter*.55f,1.4f,dimensions.towerGalleryDiameter*.55f),"Curved Wall","Metal",tower);
            Place("Tower Door Lintel",new(0,dimensions.towerDoorHeight,-dimensions.towerBaseDiameter*.5f-.08f),new(dimensions.towerDoorWidth+.36f,.18f,.28f),"Door Frame","Wood",tower);
            Place("Tower Door Left Jamb",new(-(dimensions.towerDoorWidth*.5f+.09f),dimensions.towerDoorHeight*.5f,-dimensions.towerBaseDiameter*.5f-.08f),new(.18f,dimensions.towerDoorHeight,.28f),"Door Frame","Wood",tower);
            Place("Tower Door Right Jamb",new(dimensions.towerDoorWidth*.5f+.09f,dimensions.towerDoorHeight*.5f,-dimensions.towerBaseDiameter*.5f-.08f),new(.18f,dimensions.towerDoorHeight,.28f),"Door Frame","Wood",tower);
            Place("Tower Door Step",new(0,.08f,-dimensions.towerBaseDiameter*.5f-.2f),new(dimensions.towerDoorWidth+.5f,.16f,.45f),"Floor","Stone",tower);
        }

        static void FloorRing(Transform parent,string name,float y,float diameter,string surface)
        {
            float outerRadius=diameter*.5f;
            float innerRadius=dimensions.stairwellRadius;
            float radialWidth=Mathf.Max(.2f,outerRadius-innerRadius);
            float radius=innerRadius+radialWidth*.5f;
            for(int i=0;i<dimensions.towerWallSegments;i++)
            {
                float angle=i*360f/dimensions.towerWallSegments;
                float radians=angle*Mathf.Deg2Rad;
                float tangent=2f*Mathf.PI*radius/dimensions.towerWallSegments*dimensions.wallSegmentCoverage;
                GameObject segment=Place($"{name} segment {i:00}",new(Mathf.Sin(radians)*radius,y-.12f,Mathf.Cos(radians)*radius),new(tangent,.24f,radialWidth),"Floor",surface,parent);
                segment.transform.rotation=Quaternion.Euler(0,angle,0);
            }
        }

        static void BuildRooms(Transform root)
        {
            Transform rooms=Group("Room Dividers",root);
            float hw=dimensions.houseSize.x*.5f,hd=dimensions.houseSize.z*.5f,hh=dimensions.houseSize.y;
            HouseWall(rooms,"House Front Left",new(-5-hw*.5f,-5-hd),new(hw*.5f-dimensions.houseDoorWidth*.5f,hh),true);
            HouseWall(rooms,"House Front Right",new(-5+hw*.5f*.5f,-5-hd),new(hw*.5f-dimensions.houseDoorWidth*.5f,hh),true);
            Wall("House Back",new(-5,-5+hd,hh*.5f),new(dimensions.houseSize.x,hh,dimensions.houseWallThickness),rooms);
            HouseWall(rooms,"House West",new(-5-hw,-5),new(dimensions.houseWallThickness,hh),false);
            HouseWall(rooms,"House East",new(-5+hw,-5),new(dimensions.houseWallThickness,hh),false);
            Place("House Roof South",new(-5,-5,hh+dimensions.roofPitch*.5f),new(dimensions.houseSize.x+.5f,dimensions.roofPitch,dimensions.houseSize.z*.55f),"Wall","Stone",rooms).transform.rotation=Quaternion.Euler(0,0,18);
            Place("House Roof North",new(-5,-5,hh+dimensions.roofPitch*.5f),new(dimensions.houseSize.x+.5f,dimensions.roofPitch,dimensions.houseSize.z*.55f),"Wall","Stone",rooms).transform.rotation=Quaternion.Euler(0,0,-18);
            Place("House Chimney",new(-8,-3.5f,hh+1.2f),new(.8f,2.4f,.8f),"Wall","Stone",rooms);
            Wall("House Keeper Divider",new(-5.5f,-5,1.4f),new(.3f,2.8f,7.4f),rooms);
            Wall("House Kitchen Divider",new(-2,-5,1.4f),new(.3f,2.8f,7.4f),rooms);
            Wall("House Pantry Divider",new(-8,-3.2f,1.4f),new(5,.3f,2.8f),rooms);
            CreateDoor("Keeper House Front Door",new(-5,0,-9),dimensions.houseDoorWidth,dimensions.houseDoorHeight,rooms,false);
            Place("Generator Plinth",new(-1,-3, .35f),new(1.8f,.7f,1.4f),"Floor","Metal",rooms);
            Place("Cellar Floor",new(-5,-5,-3.3f),new(12,.3f,7),"Floor","Stone",rooms);
            Place("Hidden Room Floor",new(-12,-5,-3.3f),new(4,.3f,5),"Floor","Stone",rooms);
            Place("Flooded Corridor",new(1,-5,-3.45f),new(3,.18f,8),"Floor","Water",rooms);
            for(int i=0;i<3;i++){Place($"Cellar Arch Left {i}",new(-9,-1.8f,-7+i*2.2f),new(.35f,2.8f,.35f),"Wall","Stone",rooms);Place($"Cellar Arch Top {i}",new(-8.2f,-.55f,-7+i*2.2f),new(1.6f,.3f,.35f),"Wall","Stone",rooms);}
            Place("Drain",new(1,-3.25f,-1.2f),new(1.2f,.08f,1.2f),"Floor","Metal",rooms);
        }

        static void BuildStairs(Transform root)
        {
            Transform stairs=Group("Stairs",root);
            GameObject support=GameObject.CreatePrimitive(PrimitiveType.Cylinder);support.name="Tower Central Support";support.transform.SetParent(stairs);support.transform.position=new(0,dimensions.towerLevelCount*dimensions.floorHeight*.5f,0);support.transform.localScale=new(.35f,dimensions.towerLevelCount*dimensions.floorHeight*.5f,.35f);support.GetComponent<Renderer>().sharedMaterial=Materials["Metal"];GameObjectUtility.SetStaticEditorFlags(support,StaticEditorFlags.BatchingStatic|StaticEditorFlags.OccludeeStatic|StaticEditorFlags.NavigationStatic);
            for(int level=0;level<dimensions.towerLevelCount;level++)SpiralFlight(stairs,$"Spiral L{level}",level*dimensions.floorHeight);
            for(int level=0;level<dimensions.towerLevelCount;level++)Link(stairs,$"Spiral Link {level}",new(0,level*dimensions.floorHeight+dimensions.floorHeight-.2f,0),new(0,(level+1)*dimensions.floorHeight+.1f,0));
            StraightFlight(stairs,"Cellar Stair",new(-8f,0.05f,-5f),new(-8f,-3.2f,-0.5f),18);
            Link(stairs,"Cellar Link",new(-8f,0.05f,-5f),new(-8f,-3.2f,-0.5f));
        }

        static void SpiralFlight(Transform parent,string name,float baseY)
        {
            Transform flight=Group(name,parent);int steps=Mathf.CeilToInt(dimensions.floorHeight/dimensions.stairStepHeight);float progress=baseY/(dimensions.floorHeight*Mathf.Max(1,dimensions.towerLevelCount));float diameter=Mathf.Lerp(dimensions.towerBaseDiameter,dimensions.towerGalleryDiameter,progress);float radius=diameter*.5f-Mathf.Lerp(dimensions.towerBaseWallThickness,dimensions.towerGalleryWallThickness,progress)-dimensions.stairWidth*.5f;
            for(int i=0;i<steps;i++){float angle=(100f-i*360f/steps);float radians=angle*Mathf.Deg2Rad;GameObject step=Place($"{name} {i:00}",new(Mathf.Sin(radians)*radius,baseY+(i+.5f)*dimensions.stairStepHeight,Mathf.Cos(radians)*radius),new(dimensions.stairWidth,dimensions.stairStepHeight,dimensions.stairStepDepth),"Stair","Wood",flight);step.transform.rotation=Quaternion.Euler(0,-angle,0);var sc=step.GetComponent<Collider>();if(sc)Object.DestroyImmediate(sc);}
            Place($"{name} Landing",new(0,baseY+dimensions.floorHeight-.1f,0),new(2.6f,.2f,2.6f),"Floor","Wood",flight);
            for(int i=0;i<6;i++){float a=100f-i*60f;float r=a*Mathf.Deg2Rad;GameObject hr=Place($"{name} Handrail {i}",new(Mathf.Sin(r)*3.1f,baseY+1.1f,Mathf.Cos(r)*3.1f),new(.08f,1.1f,.08f),"Wall","Metal",flight);var hc=hr.GetComponent<Collider>();if(hc)Object.DestroyImmediate(hc);}
        }
        static void StraightFlight(Transform parent,string name,Vector3 start,Vector3 end,int count){for(int i=0;i<count;i++){float t=(i+.5f)/count;GameObject step=Place($"{name} {i:00}",Vector3.Lerp(start,end,t),new(dimensions.stairWidth,dimensions.stairStepHeight,.5f),"Stair","Wood",parent);var c=step.GetComponent<Collider>();if(c)Object.DestroyImmediate(c);}}
        static void Link(Transform parent,string name,Vector3 start,Vector3 end){var link=new GameObject(name).AddComponent<NavMeshLink>();link.transform.SetParent(parent);link.startPoint=start;link.endPoint=end;link.width=dimensions.stairWidth;link.bidirectional=true;}

        static void BuildWindows(Transform root)
        {
            Transform windows=Group("Windows",root);for(int level=0;level<=dimensions.towerLevelCount;level++){float t=level/(float)Mathf.Max(1,dimensions.towerLevelCount);float radius=Mathf.Lerp(dimensions.towerBaseDiameter,dimensions.towerGalleryDiameter,t)*.5f;float y=level*dimensions.floorHeight+1.5f;if(level>0){Window(windows,$"Courtyard Window L{level}",new(0,y,-radius-.03f),0);Window(windows,$"Shallows Window L{level}",new(radius+.03f,y,0),90);}}
            Window(windows,"House Courtyard Window",new(-5,1.5f,-1.05f),0);Window(windows,"House Shallows Window",new(1.95f,1.5f,-5),90);
        }
        static void Window(Transform parent,string name,Vector3 position,float angle){GameObject frame=Place(name,position,new(1.6f,1.4f,.12f),"Window","Metal",parent);frame.transform.rotation=Quaternion.Euler(0,angle,0);frame.tag="Window";}

        static void BuildRoomVolumes(Transform root)
        {
            Transform rooms=Group("Room Triggers",root);
            Room(rooms,"entry-hall","Entry Hall",0,new(-5,1.4f,-5),new(3,2.8f,3));Room(rooms,"keeper-room","Keeper's Room",0,new(-8,1.4f,-5),new(3,2.8f,3));Room(rooms,"kitchen-pantry","Kitchen & Pantry",0,new(-3,1.4f,-5),new(2.5f,2.8f,3));Room(rooms,"generator-room","Generator Room",0,new(-1,1.4f,-3),new(2,2.8f,2));
            Room(rooms,"cellar","Cellar",-1,new(-8,-1.8f,2),new(4,2.7f,6));Room(rooms,"hidden-room","Hidden Room",-1,new(-8,-1.8f,7),new(5,2.7f,5));Room(rooms,"flooded-corridor","Flooded Corridor",-1,new(1,-1.8f,-5),new(3,2.7f,8));
            Room(rooms,"bedroom-library","Bedroom & Library",2,new(0,dimensions.floorHeight*2+1.4f,0),new(5,2.8f,5));Room(rooms,"crane-room","Crane's Room",3,new(-1.5f,dimensions.floorHeight*3+1.4f,0),new(3,2.8f,5));Room(rooms,"upper-landing","Upper Landing",3,new(1.5f,dimensions.floorHeight*3+1.4f,0),new(2,2.8f,5));Room(rooms,"lantern-room","Lantern Room",8,new(0,dimensions.towerLevelCount*dimensions.floorHeight+1.4f,0),new(4.5f,2.8f,4.5f));
        }
        static void Room(Transform parent,string id,string display,int floor,Vector3 position,Vector3 size){var go=new GameObject(display+" Trigger");go.transform.SetParent(parent);go.transform.position=position;var box=go.AddComponent<BoxCollider>();box.isTrigger=true;box.size=size;var room=go.AddComponent<RoomTrigger>();Set(room,"roomId",id);Set(room,"displayName",display);Set(room,"floorLevel",floor);}

        static void BuildHidingSpots(Transform root)
        {
            Transform group=Group("Hiding Spots",root);Vector3[] positions={new(-8,0,-5),new(-3,0,-5),new(-1,0,-3),new(-7,-3.2f,-5),new(1,-3.2f,-5),new(-2.8f,dimensions.floorHeight*2,1),new(2.2f,dimensions.floorHeight*2,-1),new(2.2f,dimensions.floorHeight*3,1),new(-2.2f,dimensions.floorHeight*3,-1),new(2.2f,dimensions.towerLevelCount*dimensions.floorHeight,1),new(10,0,-14)};string[] ids={"keeper-wardrobe","pantry-cupboard","generator-locker","cellar-coal-bin","drain-alcove","library-wardrobe","level2-desk","upper-locker","crane-bed","lantern-cabinet","boat-shed-crate"};
            var actions=AssetDatabase.LoadAssetAtPath<UnityEngine.InputSystem.InputActionAsset>("Assets/InputSystem_Actions.inputactions");var health=Object.FindAnyObjectByType<PlayerHealthState>();var stalker=GameObject.Find("Stalker");List<HidingSpot> spots=new();
            for(int i=0;i<positions.Length;i++){GameObject prop=Place(ids[i],positions[i]+Vector3.up,new(1.1f,2,1.1f),"Wall","Wood",group);var collider=prop.GetComponent<BoxCollider>();collider.size=new(.95f,.9f,.95f);collider.center=new(0,-.45f,0);var spot=prop.AddComponent<HidingSpot>();Transform pose=new GameObject(ids[i]+" pose").transform;pose.SetParent(prop.transform);pose.position=positions[i]+Vector3.up*.2f;Set(spot,"spotId",ids[i]);Set(spot,"hidingPose",pose);Set(spot,"inputActions",actions);Set(spot,"player",health);Set(spot,"stalker",stalker?stalker.transform:null);spots.Add(spot);}
            StalkerAI ai=stalker?stalker.GetComponent<StalkerAI>():null;if(ai){spots.AddRange(Object.FindObjectsByType<HidingSpot>(FindObjectsSortMode.None).Where(value=>!spots.Contains(value)));SetArray(ai,"hidingSpots",spots.ToArray());}
        }

        static void BuildWaypoints(Transform root)
        {
            Transform group=Group("Waypoints",root);List<Transform> points=new();for(int floor=-1;floor<=dimensions.towerLevelCount;floor++){float y=floor<0?-3.15f:floor*dimensions.floorHeight;Vector3[] ring={new(-2.2f,y,-2),new(2.2f,y,-2),new(2.2f,y,2),new(-2.2f,y,2)};for(int i=0;i<ring.Length;i++){Transform point=new GameObject($"Floor {floor} Waypoint {i}").transform;point.SetParent(group);point.position=ring[i];points.Add(point);}}
            StalkerAI ai=Object.FindAnyObjectByType<StalkerAI>();if(ai)SetArray(ai,"waypoints",points.ToArray());
        }

        static void BuildOcclusion(Transform root)
        {
            Transform group=Group("Occlusion",root);for(int floor=-1;floor<=dimensions.towerLevelCount;floor++){GameObject area=new($"Floor {floor} Occlusion Area");area.transform.SetParent(group);area.transform.position=new(0,floor<0?-1.8f:floor*dimensions.floorHeight+1.5f,0);var component=area.AddComponent<OcclusionArea>();component.size=new(10,3,10);}
            for(int i=0;i<dimensions.towerLevelCount;i++){GameObject portal=new($"Floor {i} Stair Portal");portal.transform.SetParent(group);portal.transform.position=new(0,i*dimensions.floorHeight+.1f,0);portal.AddComponent<OcclusionPortal>().open=true;}
        }

        static void RelocateGameplayAnchors(string actName)
        {
            Move("Tower Door",new(0,0,-4.15f));MoveChild("Tower Door","Door panel",new(0,1.1f,0));EnsureDoorCollider("Tower Door",dimensions.towerDoorWidth,dimensions.towerDoorHeight);
            var oldPanel=GameObject.Find("Tower Door")?.transform.Find("Door panel");if(oldPanel)Object.DestroyImmediate(oldPanel.gameObject);
            MakeNonBlocking("Door panel");
            Move("Radio",new(-8,1,-5));Move("Hot Meal",new(-8,.9f,-6));Move("Table",new(-8,.45f,-6));Move("Steam placeholder",new(-8,1.1f,-6));
            Move("Lamp",new(0,dimensions.towerLevelCount*dimensions.floorHeight+1.1f,0));Move("Crank Station",new(2.2f,dimensions.towerLevelCount*dimensions.floorHeight+.35f,0));
            Move("Locked Cellar Door",new(-8f,1.1f,-5f));Move("Wet footprints (Act 2)",new(-8f,0.05f,-6f));Move("Cellar Hum",new(-8f,-1.8f,2f));Move("Reserve Fuel Can",new(-9f,-3.2f,2f));
            Move("hidden shelf",new(-8f,-2.2f,4.5f));Move("Marta reel-to-reel recorder",new(-8.75f,-2.25f,8.86f));Move("Harbor board ledger",new(-8.15f,-2.32f,8.58f));
            Move("Crane's Room Door",new(-1.5f,dimensions.floorHeight*3,-2.4f));Move("Family Photo",new(-2.5f,dimensions.floorHeight*3+1.4f,1));Move("Crane's Desk",new(-1.5f,dimensions.floorHeight*3+.35f,1.2f));Move("Crane Room Key",new(10,.4f,-14));
            Move("Boat",new(-3.5f,.15f,-29));Move("Shoreline ending trigger",new(0,1,-27));
            GameObject player=GameObject.Find("Player");if(player)player.transform.position=actName=="Act1"?new(0,.15f,-29):new(0,dimensions.towerLevelCount*dimensions.floorHeight-.8f,-1.5f);
        }

        static void BuildHud()
        {
            GameObject canvas=GameObject.Find("Debug Canvas");if(!canvas)return;if(!canvas.GetComponent<UiAudio>())canvas.AddComponent<UiAudio>();CanvasScaler scaler=canvas.GetComponent<CanvasScaler>();if(scaler){scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new(1920,1080);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;scaler.matchWidthOrHeight=.5f;}
            Transform legacyObjective=canvas.transform.Find("Objective");if(legacyObjective)Object.DestroyImmediate(legacyObjective.gameObject);
            Transform previous=canvas.transform.Find("HUD Root");if(previous)Object.DestroyImmediate(previous.gameObject);RectTransform root=Rect("HUD Root",canvas.transform,Vector2.zero,Vector2.one);UITheme theme=AssetDatabase.LoadAssetAtPath<UITheme>($"{SettingsFolder}/UITheme.asset");HudSettings settings=AssetDatabase.LoadAssetAtPath<HudSettings>($"{SettingsFolder}/HudSettings.asset");HudRoot hud=root.gameObject.AddComponent<HudRoot>();Set(hud,"theme",theme);Set(hud,"settings",settings);Set(hud,"scalableRoot",root);
            GameObject noise=Panel("NoiseMeter",root,new(.03f,.06f),new(.17f,.11f),new Color(theme.Panel.r,theme.Panel.g,theme.Panel.b,.72f));CanvasGroup noiseGroup=noise.AddComponent<CanvasGroup>();noiseGroup.alpha=0;noiseGroup.blocksRaycasts=false;Image[] noiseSegments=new Image[3];for(int i=0;i<3;i++)noiseSegments[i]=Panel($"Noise {i+1}",noise.transform,new(.08f+i*.31f,.22f),new(.32f+i*.31f,.78f),theme.ColdBlueGrey).GetComponent<Image>();SetArray(hud,"noiseSegments",noiseSegments);Set(hud,"noiseRoot",noise);Set(hud,"noiseGroup",noiseGroup);
            GameObject stamina=Panel("StaminaBar",root,new(.38f,.035f),new(.62f,.06f),new Color(theme.Panel.r,theme.Panel.g,theme.Panel.b,.65f));CanvasGroup staminaGroup=stamina.AddComponent<CanvasGroup>();staminaGroup.alpha=0;staminaGroup.blocksRaycasts=false;Image staminaFill=Panel("Fill",stamina.transform,Vector2.zero,Vector2.one,theme.ColdBlueGrey).GetComponent<Image>();staminaFill.type=Image.Type.Filled;staminaFill.fillMethod=Image.FillMethod.Horizontal;Set(hud,"staminaFill",staminaFill);Set(hud,"staminaRoot",stamina);Set(hud,"staminaGroup",staminaGroup);
            GameObject lamp=Panel("LampGauge",root,new(.82f,.045f),new(.96f,.11f),new Color(theme.Panel.r,theme.Panel.g,theme.Panel.b,.65f));CanvasGroup lampGroup=lamp.AddComponent<CanvasGroup>();lampGroup.alpha=0;lampGroup.blocksRaycasts=false;Image lampFill=Panel("Fill",lamp.transform,new(.05f,.12f),new(.95f,.5f),theme.Amber).GetComponent<Image>();lampFill.type=Image.Type.Filled;lampFill.fillMethod=Image.FillMethod.Horizontal;TMP_Text lampLabel=Text("State",lamp.transform,new(.05f,.52f),new(.95f,.96f),"LAMP",18,theme);Set(hud,"lampFill",lampFill);Set(hud,"lampState",lampLabel);Set(hud,"lampRoot",lamp);Set(hud,"lampGroup",lampGroup);
            GameObject objective=Panel("ObjectiveText",root,new(.32f,.91f),new(.68f,.96f),new Color(theme.Panel.r,theme.Panel.g,theme.Panel.b,.58f));CanvasGroup objectiveGroup=objective.AddComponent<CanvasGroup>();objectiveGroup.alpha=0;objectiveGroup.blocksRaycasts=false;TMP_Text objectiveLabel=Text("Label",objective.transform,new(.04f,.05f),new(.96f,.95f),"",20,theme);Set(hud,"objectiveText",objectiveLabel);Set(hud,"objectiveRoot",objective);Set(hud,"objectiveGroup",objectiveGroup);
            GameObject inventory=Panel("InventoryStrip",root,new(.31f,.075f),new(.69f,.12f),theme.Panel);CanvasGroup inventoryGroup=inventory.AddComponent<CanvasGroup>();inventoryGroup.alpha=0;inventoryGroup.blocksRaycasts=false;TMP_Text inventoryLabel=Text("Items",inventory.transform,new(.03f,.05f),new(.97f,.95f),"",20,theme);Set(hud,"inventoryText",inventoryLabel);Set(hud,"inventoryRoot",inventory);Set(hud,"inventoryGroup",inventoryGroup);
            InteractionPrompt prompt=Object.FindAnyObjectByType<InteractionPrompt>();GameObject promptRoot=prompt?prompt.gameObject:Panel("InteractionPrompt",root,new(.36f,.16f),new(.64f,.22f),theme.Panel);CanvasGroup promptGroup=promptRoot.GetComponent<CanvasGroup>();if(!promptGroup)promptGroup=promptRoot.AddComponent<CanvasGroup>();promptGroup.alpha=0;promptGroup.blocksRaycasts=false;TMP_Text promptText=Text("Styled Prompt",promptRoot.transform,Vector2.zero,Vector2.one,"",24,theme);if(prompt){Set(prompt,"styledLabel",promptText);Set(prompt,"keyIcon",theme.KeySprite);}Set(hud,"interactionRoot",promptRoot);
            Toast(hud,root,"CheckpointToast","checkpointToast","checkpointText",new(.37f,.72f),new(.63f,.78f),theme);Toast(hud,root,"PageAddedToast","pageToast","pageText",new(.37f,.78f),new(.63f,.84f),theme);Toast(hud,root,"RoomNameFlash","roomToast","roomText",new(.35f,.62f),new(.65f,.7f),theme);
            GameObject composure=Panel("ComposureMeter",root,new(.44f,.01f),new(.56f,.025f),theme.Panel);Image composureFill=Panel("Fill",composure.transform,Vector2.zero,Vector2.one,theme.ColdBlueGrey).GetComponent<Image>();composureFill.type=Image.Type.Filled;composureFill.fillMethod=Image.FillMethod.Horizontal;Set(hud,"composureRoot",composure);Set(hud,"composureFill",composureFill);
            GameObject pulse=Panel("Composure Edge Pulse",root,Vector2.zero,Vector2.one,new Color(theme.Danger.r,theme.Danger.g,theme.Danger.b,0));pulse.GetComponent<Image>().raycastTarget=false;Set(hud,"screenEdgePulse",pulse.GetComponent<Image>());
            SubtitleUI subtitles=Object.FindAnyObjectByType<SubtitleUI>();if(subtitles){Transform oldBackground=canvas.transform.Find("Subtitle Background");if(oldBackground)Object.DestroyImmediate(oldBackground.gameObject);GameObject background=Panel("Subtitle Background",canvas.transform,new(.16f,.025f),new(.84f,.115f),new Color(theme.Panel.r,theme.Panel.g,theme.Panel.b,settings.SubtitleBackgroundOpacity));background.GetComponent<Image>().raycastTarget=false;background.transform.SetSiblingIndex(Mathf.Max(0,subtitles.transform.GetSiblingIndex()-1));Set(subtitles,"background",background.GetComponent<Image>());subtitles.ApplySettings(settings);}
            BuildPauseMenu(canvas.transform,theme);
            EnsureCreditsAsset();
        }

        static void EnsureCreditsAsset()
        {
            const string path="Assets/_Project/Settings/UI/Credits.txt";
            if(!AssetDatabase.LoadAssetAtPath<TextAsset>(path))AssetDatabase.CreateAsset(new TextAsset("UI assets, input prompts, interface sounds, and Kenney Future fonts by Kenney Vleugels, released under CC0 1.0.\nhttps://kenney.nl/assets"),path);
        }

        static void Toast(HudRoot hud,Transform root,string name,string groupField,string textField,Vector2 min,Vector2 max,UITheme theme){GameObject go=Panel(name,root,min,max,theme.Panel);CanvasGroup group=go.AddComponent<CanvasGroup>();group.alpha=0;group.blocksRaycasts=false;TMP_Text text=Text("Label",go.transform,new(.03f,.05f),new(.97f,.95f),"",22,theme);Set(hud,groupField,group);Set(hud,textField,text);}
        static void BuildPauseMenu(Transform canvas,UITheme theme)
        {
            Transform old=canvas.Find("Pause Menu");if(old)Object.DestroyImmediate(old.gameObject);PauseMenu existing=canvas.GetComponent<PauseMenu>();if(existing)Object.DestroyImmediate(existing);GameObject panel=Panel("Pause Menu",canvas,new(.32f,.18f),new(.68f,.82f),new Color(theme.Panel.r,theme.Panel.g,theme.Panel.b,.97f));PauseMenu pause=canvas.gameObject.AddComponent<PauseMenu>();Set(pause,"panel",panel);Set(pause,"player",Object.FindAnyObjectByType<FirstPersonController>());Set(pause,"interactor",Object.FindAnyObjectByType<Interactor>());Set(pause,"journal",Object.FindAnyObjectByType<JournalUI>());
            Text("Title",panel.transform,new(.08f,.84f),new(.92f,.96f),"PAUSED",38,theme);string[] names={"Resume","Journal","Options","Save","Quit to MainMenu"};for(int i=0;i<names.Length;i++){Button button=Button(names[i],panel.transform,new(.15f,.68f-i*.12f),new(.85f,.77f-i*.12f),theme);switch(i){case 0:UnityEventTools.AddPersistentListener(button.onClick,pause.Resume);break;case 1:UnityEventTools.AddPersistentListener(button.onClick,pause.OpenJournal);break;case 2:UnityEventTools.AddPersistentListener(button.onClick,pause.OpenOptions);break;case 3:UnityEventTools.AddPersistentListener(button.onClick,pause.Save);break;case 4:UnityEventTools.AddPersistentListener(button.onClick,pause.QuitToMainMenu);break;}}
            GameObject options=Panel("Options",panel.transform,new(.04f,.03f),new(.96f,.83f),theme.Panel);HudSettings hudSettings=AssetDatabase.LoadAssetAtPath<HudSettings>($"{SettingsFolder}/HudSettings.asset");HudSettingsPanel controller=options.AddComponent<HudSettingsPanel>();Set(controller,"settings",hudSettings);Set(controller,"hud",Object.FindAnyObjectByType<HudRoot>());Set(controller,"subtitles",Object.FindAnyObjectByType<SubtitleUI>());Text("Options Label",options.transform,new(.05f,.88f),new(.95f,.98f),"HUD OPTIONS",25,theme);
            string[] labels={"Noise","Stamina","Lamp","Objective","Inventory","Interaction","Toasts","Room names","Composure","Minimal HUD"};bool[] values={true,true,true,true,true,true,true,true,false,false};for(int i=0;i<labels.Length;i++){int column=i/5;int row=i%5;Toggle toggle=OptionToggle(labels[i],options.transform,new(.05f+column*.48f,.74f-row*.13f),new(.47f+column*.48f,.84f-row*.13f),theme,values[i]);switch(i){case 0:UnityEventTools.AddPersistentListener(toggle.onValueChanged,controller.SetNoise);break;case 1:UnityEventTools.AddPersistentListener(toggle.onValueChanged,controller.SetStamina);break;case 2:UnityEventTools.AddPersistentListener(toggle.onValueChanged,controller.SetLamp);break;case 3:UnityEventTools.AddPersistentListener(toggle.onValueChanged,controller.SetObjective);break;case 4:UnityEventTools.AddPersistentListener(toggle.onValueChanged,controller.SetInventory);break;case 5:UnityEventTools.AddPersistentListener(toggle.onValueChanged,controller.SetInteraction);break;case 6:UnityEventTools.AddPersistentListener(toggle.onValueChanged,controller.SetToasts);break;case 7:UnityEventTools.AddPersistentListener(toggle.onValueChanged,controller.SetRoomNames);break;case 8:UnityEventTools.AddPersistentListener(toggle.onValueChanged,controller.SetComposure);break;case 9:UnityEventTools.AddPersistentListener(toggle.onValueChanged,controller.SetMinimal);break;}}
            Slider scale=OptionSlider("HUD Scale",options.transform,new(.08f,.04f),new(.92f,.12f),theme,.7f,1.5f,1f);UnityEventTools.AddPersistentListener(scale.onValueChanged,controller.SetHudScale);options.SetActive(false);Set(pause,"optionsPanel",options);
            TextAsset credits=AssetDatabase.LoadAssetAtPath<TextAsset>($"{SettingsFolder}/Credits.txt");if(credits)Text("Credits",options.transform,new(.05f,.13f),new(.95f,.2f),credits.text,12,theme).alignment=TextAlignmentOptions.Left;
        }

        static Button Button(string label,Transform parent,Vector2 min,Vector2 max,UITheme theme){GameObject go=Panel(label,parent,min,max,theme.Accent);Button button=go.AddComponent<Button>();button.targetGraphic=go.GetComponent<Image>();Text("Label",go.transform,Vector2.zero,Vector2.one,label,22,theme);return button;}
        static Toggle OptionToggle(string label,Transform parent,Vector2 min,Vector2 max,UITheme theme,bool value){GameObject go=Panel(label,parent,min,max,theme.Panel);Toggle toggle=go.AddComponent<Toggle>();Image mark=Panel("Mark",go.transform,new(.02f,.2f),new(.14f,.8f),theme.Accent).GetComponent<Image>();toggle.graphic=mark;toggle.targetGraphic=go.GetComponent<Image>();toggle.isOn=value;Text("Label",go.transform,new(.18f,0),new(1,1),label,18,theme).alignment=TextAlignmentOptions.MidlineLeft;return toggle;}
        static Slider OptionSlider(string label,Transform parent,Vector2 min,Vector2 max,UITheme theme,float low,float high,float value){GameObject go=Panel(label,parent,min,max,theme.Panel);Slider slider=go.AddComponent<Slider>();slider.minValue=low;slider.maxValue=high;slider.value=value;Image fill=Panel("Fill",go.transform,new(.28f,.25f),new(.95f,.75f),theme.Accent).GetComponent<Image>();slider.fillRect=fill.rectTransform;Text("Label",go.transform,new(.02f,0),new(.26f,1),label,17,theme);return slider;}
        static TMP_Text Text(string name,Transform parent,Vector2 min,Vector2 max,string value,float size,UITheme theme){GameObject go=new(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(TextMeshProUGUI));go.transform.SetParent(parent,false);RectTransform rect=go.GetComponent<RectTransform>();rect.anchorMin=min;rect.anchorMax=max;rect.offsetMin=rect.offsetMax=Vector2.zero;var text=go.GetComponent<TextMeshProUGUI>();text.text=value;text.fontSize=size;text.color=theme.Text;text.alignment=TextAlignmentOptions.Center;text.raycastTarget=false;if(theme.BodyFont)text.font=theme.BodyFont;return text;}
        static GameObject Panel(string name,Transform parent,Vector2 min,Vector2 max,Color color){GameObject go=new(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));go.transform.SetParent(parent,false);RectTransform rect=go.GetComponent<RectTransform>();rect.anchorMin=min;rect.anchorMax=max;rect.offsetMin=rect.offsetMax=Vector2.zero;go.GetComponent<Image>().color=color;return go;}
        static RectTransform Rect(string name,Transform parent,Vector2 min,Vector2 max){GameObject go=new(name,typeof(RectTransform));go.transform.SetParent(parent,false);RectTransform rect=go.GetComponent<RectTransform>();rect.anchorMin=min;rect.anchorMax=max;rect.offsetMin=rect.offsetMax=Vector2.zero;return rect;}

        static void FixParticleMaterials(){Material particle=Materials["Particle"];foreach(ParticleSystemRenderer renderer in Object.FindObjectsByType<ParticleSystemRenderer>(FindObjectsInactive.Include,FindObjectsSortMode.None))renderer.sharedMaterial=particle;}
        static void ReportMaterialProblems(){foreach(Renderer renderer in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(renderer.sharedMaterial==null||renderer.sharedMaterial.shader==null||!renderer.sharedMaterial.shader.isSupported)Debug.LogWarning($"Missing or unsupported material/shader: {renderer.name}",renderer);}

        static void RemoveLegacyGeometry()
        {
            foreach(string rootName in new[]{"Lighthouse Layout","Tower","Stairs","Room Dividers","Exterior","Windows","Room Triggers","Hiding Spots","Waypoints","Occlusion"})
            {
                foreach(GameObject root in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None).Where(value=>value.transform.parent==null&&value.name==rootName).ToArray())Object.DestroyImmediate(root);
            }
            string[] exact={"Greybox Tower","South wall left","South wall right","Door lintel","Dock","Path","Boat Shed Floor","Boat Shed Back","Boat Shed Roof","Lantern Room Floor","Lantern Room Floor Back","Stair Top Landing","Storeroom rear wall","Storeroom divider","East window sill","East window lintel","Window pier south","Window pier middle","Window pier north","Shallows","Cellar floor","Cellar north wall","Cellar south wall","Cellar west wall","Hidden room floor","Hidden room west wall","Hidden room north wall","Hidden room south wall","Crane room partition","Flooded courtyard","Lower window pressure ledge"};foreach(string name in exact){GameObject go=GameObject.Find(name);if(go)Object.DestroyImmediate(go);}foreach(GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))if(go.name.StartsWith("Stair ",StringComparison.Ordinal)&&int.TryParse(go.name.Substring(6),out _))Object.DestroyImmediate(go);
        }

        static GameObject Place(string name,Vector3 position,Vector3 scale,string prefab,string surface,Transform parent){GameObject source=AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabFolder}/{prefab}.prefab");GameObject go=(GameObject)PrefabUtility.InstantiatePrefab(source);go.name=name;go.transform.SetParent(parent);go.transform.position=position;go.transform.localScale=scale;go.tag=surface;go.GetComponent<Renderer>().sharedMaterial=Materials[surface];GameObjectUtility.SetStaticEditorFlags(go,StaticEditorFlags.BatchingStatic|StaticEditorFlags.OccludeeStatic|StaticEditorFlags.OccluderStatic|StaticEditorFlags.NavigationStatic);return go;}
        static void Wall(string name,Vector3 position,Vector3 scale,Transform parent)=>Place(name,position,scale,"Wall","Stone",parent);
        static Transform Group(string name,Transform parent){Transform group=new GameObject(name).transform;group.SetParent(parent);return group;}
        static void Bounds(Transform parent,Vector3 position,Vector3 size){GameObject go=new("Invisible Bounds");go.transform.SetParent(parent);go.transform.position=position;BoxCollider box=go.AddComponent<BoxCollider>();box.size=size;}
        static void Move(string name,Vector3 position){GameObject go=GameObject.Find(name);if(go)go.transform.position=position;}
        static void MoveChild(string rootName,string childName,Vector3 localPosition){GameObject root=GameObject.Find(rootName);if(!root)return;Transform child=root.transform.Find(childName);if(child)child.localPosition=localPosition;}
        static void MakeNonBlocking(string name)
        {
            GameObject go=GameObject.Find(name);
            if(!go)return;
            foreach(Collider collider in go.GetComponentsInChildren<Collider>())collider.enabled=false;
            foreach(Renderer renderer in go.GetComponentsInChildren<Renderer>())renderer.enabled=false;
        }
        static void CreateDoor(string name,Vector3 position,float width,float height,Transform parent,bool locked)
        {
            if(GameObject.Find(name))return;
            GameObject root=new(name);root.transform.SetParent(parent);root.transform.position=position;
            GameObject frame=Place($"{name} Frame",position+Vector3.up*height*.5f,new(.18f,height,.2f),"Door Frame","Wood",parent);
            frame.transform.SetParent(root.transform,true);
            NoiseEmitter noise=root.AddComponent<NoiseEmitter>();Door door=root.AddComponent<Door>();
            EnsureDoorCollider(root,width,height);
            Set(door,"locked",locked);Set(door,"doorId",name);
        }
        static void EnsureDoorCollider(string name,float width,float height){GameObject go=GameObject.Find(name);if(go)EnsureDoorCollider(go,width,height);}
        static void EnsureDoorCollider(GameObject go,float width,float height)
        {
            BoxCollider collider=go.GetComponent<BoxCollider>();
            if(!collider)collider=go.AddComponent<BoxCollider>();
            collider.center=new(0,height*.5f,0);collider.size=new(width,.12f,.16f);
        }
        static void Set(Object target,string name,Object value){if(!target)return;SerializedObject serialized=new(target);SerializedProperty property=serialized.FindProperty(name);if(property!=null){property.objectReferenceValue=value;serialized.ApplyModifiedPropertiesWithoutUndo();}}
        static void Set(Object target,string name,string value){if(!target)return;SerializedObject serialized=new(target);SerializedProperty property=serialized.FindProperty(name);if(property!=null){property.stringValue=value;serialized.ApplyModifiedPropertiesWithoutUndo();}}
        static void Set(Object target,string name,bool value){if(!target)return;SerializedObject serialized=new(target);SerializedProperty property=serialized.FindProperty(name);if(property!=null){property.boolValue=value;serialized.ApplyModifiedPropertiesWithoutUndo();}}
        static void Set(Object target,string name,int value){if(!target)return;SerializedObject serialized=new(target);SerializedProperty property=serialized.FindProperty(name);if(property!=null){property.intValue=value;serialized.ApplyModifiedPropertiesWithoutUndo();}}
        static void SetArray<T>(Object target,string name,T[] values) where T:Object{if(!target)return;SerializedObject serialized=new(target);SerializedProperty property=serialized.FindProperty(name);if(property==null)return;property.arraySize=values.Length;for(int i=0;i<values.Length;i++)property.GetArrayElementAtIndex(i).objectReferenceValue=values[i];serialized.ApplyModifiedPropertiesWithoutUndo();}
    }
}
