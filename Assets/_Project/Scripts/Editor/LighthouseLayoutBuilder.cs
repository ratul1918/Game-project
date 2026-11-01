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
        const float FloorHeight=3.2f;
        const float TowerRadius=4.5f;
        static readonly Dictionary<string,Material> Materials=new();

        public static void BuildSharedLayout(string actName)
        {
            EnsureAssets();
            RemoveLegacyGeometry();
            GameObject old=GameObject.Find(RootName);if(old)Object.DestroyImmediate(old);
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
            BuildHud();
            FixParticleMaterials();
            ReportMaterialProblems();
            EditorUtility.SetDirty(root.gameObject);
        }

        static void EnsureAssets()
        {
            Directory.CreateDirectory(PrefabFolder);Directory.CreateDirectory(MaterialFolder);Directory.CreateDirectory(SettingsFolder);AssetDatabase.Refresh();
            EnsureTag("Window");
            Material("Stone",new(.19f,.21f,.22f),false);Material("Wood",new(.24f,.16f,.1f),false);Material("Metal",new(.22f,.25f,.27f),false);Material("Sand",new(.32f,.29f,.22f),false);Material("Water",new(.06f,.16f,.2f,.72f),true);Material("Particle",new(.62f,.68f,.7f,.35f),true,true);
            Prefab("Floor",new(1,.25f,1),"Stone");Prefab("Wall",new(1,1,0.22f),"Stone");Prefab("Door Frame",new(1,1,.25f),"Wood");Prefab("Window",new(1,1,.12f),"Metal");Prefab("Stair",new(1,.2f,.3f),"Wood");Prefab("Curved Wall",new(1.45f,1,.22f),"Stone");
            UITheme theme=AssetDatabase.LoadAssetAtPath<UITheme>($"{SettingsFolder}/UITheme.asset");if(!theme){theme=ScriptableObject.CreateInstance<UITheme>();AssetDatabase.CreateAsset(theme,$"{SettingsFolder}/UITheme.asset");}
            HudSettings hud=AssetDatabase.LoadAssetAtPath<HudSettings>($"{SettingsFolder}/HudSettings.asset");if(!hud){hud=ScriptableObject.CreateInstance<HudSettings>();AssetDatabase.CreateAsset(hud,$"{SettingsFolder}/HudSettings.asset");}
            AssignThemeSprites(theme);EditorUtility.SetDirty(theme);AssetDatabase.SaveAssets();
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

        static void BuildExterior(Transform root)
        {
            Transform exterior=Group("Exterior",root);
            Place("Beach",new(0,-.35f,-14),new(42,.35f,38),"Floor","Sand",exterior);
            Place("Sea Plane",new(0,-.55f,-28),new(90,.12f,70),"Floor","Water",exterior);
            Place("Dock",new(0,-.05f,-25),new(6,.35f,12),"Floor","Wood",exterior);
            Place("Cliff Path",new(0,.05f,-15),new(3,.35f,14),"Floor","Stone",exterior);
            Place("Courtyard",new(0,.05f,-8),new(18,.35f,12),"Floor","Stone",exterior);
            Place("Courtyard Wall West",new(-9,1.25f,-8),new(.45f,2.5f,12),"Wall","Stone",exterior);
            Place("Courtyard Wall East",new(9,1.25f,-8),new(.45f,2.5f,12),"Wall","Stone",exterior);
            Place("Boat Shed Floor",new(10,.1f,-14),new(7,.3f,6),"Floor","Wood",exterior);
            Place("Boat Shed Back",new(13.5f,1.5f,-14),new(.35f,3,6),"Wall","Wood",exterior);
            Place("Boat Shed Roof",new(10,3,-14),new(7,.3f,6),"Floor","Wood",exterior);
            for(int i=0;i<9;i++){float x=-20+i*5f;Place($"Rock South {i}",new(x,.4f,-35+Mathf.Abs(x)*.08f),new(5,1.6f,4),"Floor","Stone",exterior);}
            Place("Cliff West",new(-23,2,0),new(5,5,48),"Wall","Stone",exterior);Place("Cliff East",new(23,2,0),new(5,5,48),"Wall","Stone",exterior);
            Bounds(exterior,new(0,3,9),new(50,8,1));Bounds(exterior,new(-25,3,-12),new(1,8,52));Bounds(exterior,new(25,3,-12),new(1,8,52));Bounds(exterior,new(0,3,-39),new(50,8,1));
        }

        static void BuildTower(Transform root)
        {
            Transform tower=Group("Tower",root);
            for(int level=0;level<4;level++)
            {
                float y=level*FloorHeight;
                Place($"Tower Floor {level}",new(0,y-.12f,0),new(8.6f,.24f,8.6f),"Floor",level==3?"Metal":"Wood",tower);
                for(int segment=0;segment<20;segment++)
                {
                    float angle=segment*18f;if(level==0&&(segment==9||segment==10))continue;
                    float radians=angle*Mathf.Deg2Rad;Vector3 position=new(Mathf.Sin(radians)*TowerRadius,y+1.55f,Mathf.Cos(radians)*TowerRadius);
                    GameObject wall=Place($"Tower L{level} wall {segment:00}",position,new(1.45f,3.1f,.28f),"Curved Wall","Stone",tower);wall.transform.rotation=Quaternion.Euler(0,angle,0);
                }
            }
            for(int segment=0;segment<28;segment++){float angle=segment*(360f/28f);float radians=angle*Mathf.Deg2Rad;Place($"Gallery {segment:00}",new(Mathf.Sin(radians)*5.25f,9.45f,Mathf.Cos(radians)*5.25f),new(1.3f,.22f,1.1f),"Floor","Metal",tower).transform.rotation=Quaternion.Euler(0,angle,0);}
        }

        static void BuildRooms(Transform root)
        {
            Transform rooms=Group("Room Dividers",root);
            Wall("Ground Keeper divider",new(-1.5f,1.5f,-.5f),new(.22f,3,7),rooms);Wall("Ground Generator divider",new(2.2f,1.5f,1.8f),new(4.2f,3,.22f),rooms);Wall("Pantry divider",new(-2.8f,1.5f,2.4f),new(3,.3f,3),rooms);
            Wall("Level 2 library divider",new(0,4.75f,.8f),new(7,.22f,3),rooms);Wall("Level 3 Crane divider",new(-.8f,7.95f,0),new(.22f,3,7),rooms);
            Place("Generator Plinth",new(2.8f,.35f,2.8f),new(2,.7f,1.5f),"Floor","Metal",rooms);
            Place("Cellar Floor",new(-2,-3.3f,2),new(8,.3f,7),"Floor","Stone",rooms);Place("Hidden Room Floor",new(-7,-3.3f,2),new(4,.3f,5),"Floor","Stone",rooms);Place("Flooded Corridor",new(2.5f,-3.45f,2),new(3,.18f,8),"Floor","Water",rooms);
            for(int i=0;i<3;i++){Place($"Cellar Arch Left {i}",new(-4.3f,-1.8f,-.2f+i*2.2f),new(.35f,2.8f,.35f),"Wall","Stone",rooms);Place($"Cellar Arch Top {i}",new(-3.5f,-.55f,-.2f+i*2.2f),new(1.6f,.3f,.35f),"Wall","Stone",rooms);}
            Place("Drain",new(2.5f,-3.25f,4.8f),new(1.2f,.08f,1.2f),"Floor","Metal",rooms);
        }

        static void BuildStairs(Transform root)
        {
            Transform stairs=Group("Stairs",root);
            for(int level=0;level<3;level++){Flight(stairs,$"Main L{level}",new(2.7f,level*FloorHeight,-2.4f),1);Flight(stairs,$"Service L{level}",new(-2.7f,level*FloorHeight,2.4f),-1);Link(stairs,$"Main Link {level}",new(2.7f,level*FloorHeight,-2.4f),new(2.7f,(level+1)*FloorHeight,2.4f));Link(stairs,$"Service Link {level}",new(-2.7f,level*FloorHeight,2.4f),new(-2.7f,(level+1)*FloorHeight,-2.4f));}
            Flight(stairs,"Cellar Stair",new(-2.8f,-3.2f,-1.8f),1);Flight(stairs,"Generator Service Stair",new(2.8f,-3.2f,3.2f),-1);
            Link(stairs,"Cellar Link",new(-2.8f,-3.2f,-1.8f),new(-2.8f,0,3));Link(stairs,"Generator Basement Link",new(2.8f,-3.2f,3.2f),new(2.8f,0,-1.6f));
        }

        static void Flight(Transform parent,string name,Vector3 start,float direction)
        {
            Transform flight=Group(name,parent);for(int i=0;i<16;i++)Place($"{name} {i:00}",start+new Vector3(0,(i+.5f)*.2f,direction*i*.3f),new(1.5f,.2f,.34f),"Stair","Wood",flight);
        }
        static void Link(Transform parent,string name,Vector3 start,Vector3 end){var link=new GameObject(name).AddComponent<NavMeshLink>();link.transform.SetParent(parent);link.startPoint=start;link.endPoint=end;link.width=1.4f;link.bidirectional=true;}

        static void BuildWindows(Transform root)
        {
            Transform windows=Group("Windows",root);for(int level=0;level<4;level++){float y=level*FloorHeight+1.65f;Window(windows,$"Courtyard Window L{level}",new(0,y,-4.52f),0);Window(windows,$"Shallows Window L{level}",new(4.52f,y,0),90);}
        }
        static void Window(Transform parent,string name,Vector3 position,float angle){GameObject frame=Place(name,position,new(1.6f,1.4f,.12f),"Window","Metal",parent);frame.transform.rotation=Quaternion.Euler(0,angle,0);frame.tag="Window";}

        static void BuildRoomVolumes(Transform root)
        {
            Transform rooms=Group("Room Triggers",root);
            Room(rooms,"entry-hall","Entry Hall",0,new(1,1.4f,-2.1f),new(5,2.8f,3.5f));Room(rooms,"keeper-room","Keeper's Room",0,new(-2.3f,1.4f,0),new(3.8f,2.8f,5));Room(rooms,"kitchen-pantry","Kitchen & Pantry",0,new(-2.3f,1.4f,2.9f),new(3.5f,2.8f,2.2f));Room(rooms,"generator-room","Generator Room",0,new(2.7f,1.4f,2.8f),new(2.8f,2.8f,2.5f));
            Room(rooms,"cellar","Cellar",-1,new(-2,-1.8f,2),new(8,2.7f,7));Room(rooms,"hidden-room","Hidden Room",-1,new(-7,-1.8f,2),new(4,2.7f,5));Room(rooms,"flooded-corridor","Flooded Corridor",-1,new(2.5f,-1.8f,2),new(3,2.7f,8));
            Room(rooms,"bedroom-library","Bedroom & Library",2,new(0,4.7f,0),new(8,2.8f,8));Room(rooms,"crane-room","Crane's Room",3,new(-2,7.9f,0),new(4,2.8f,7));Room(rooms,"upper-landing","Upper Landing",3,new(2.2f,7.9f,0),new(3.5f,2.8f,7));Room(rooms,"lantern-room","Lantern Room",4,new(0,11.05f,0),new(8,2.8f,8));
        }
        static void Room(Transform parent,string id,string display,int floor,Vector3 position,Vector3 size){var go=new GameObject(display+" Trigger");go.transform.SetParent(parent);go.transform.position=position;var box=go.AddComponent<BoxCollider>();box.isTrigger=true;box.size=size;var room=go.AddComponent<RoomTrigger>();Set(room,"roomId",id);Set(room,"displayName",display);Set(room,"floorLevel",floor);}

        static void BuildHidingSpots(Transform root)
        {
            Transform group=Group("Hiding Spots",root);Vector3[] positions={new(-3,0,-.5f),new(-3,0,3),new(3,0,3),new(-3,-3.2f,1),new(2.6f,-3.2f,1),new(-2.8f,3.2f,1),new(2.8f,3.2f,-1),new(2.6f,6.4f,1),new(-2.7f,6.4f,-1),new(2.8f,9.6f,1),new(10,0,-14)};string[] ids={"keeper-wardrobe","pantry-cupboard","generator-locker","cellar-coal-bin","drain-alcove","library-wardrobe","level2-desk","upper-locker","crane-bed","lantern-cabinet","boat-shed-crate"};
            var actions=AssetDatabase.LoadAssetAtPath<UnityEngine.InputSystem.InputActionAsset>("Assets/InputSystem_Actions.inputactions");var health=Object.FindAnyObjectByType<PlayerHealthState>();var stalker=GameObject.Find("Stalker");List<HidingSpot> spots=new();
            for(int i=0;i<positions.Length;i++){GameObject prop=Place(ids[i],positions[i]+Vector3.up,new(1.1f,2,1.1f),"Wall","Wood",group);var collider=prop.GetComponent<BoxCollider>();collider.size=new(.95f,.9f,.95f);collider.center=new(0,-.45f,0);var spot=prop.AddComponent<HidingSpot>();Transform pose=new GameObject(ids[i]+" pose").transform;pose.SetParent(prop.transform);pose.position=positions[i]+Vector3.up*.2f;Set(spot,"spotId",ids[i]);Set(spot,"hidingPose",pose);Set(spot,"inputActions",actions);Set(spot,"player",health);Set(spot,"stalker",stalker?stalker.transform:null);spots.Add(spot);}
            StalkerAI ai=stalker?stalker.GetComponent<StalkerAI>():null;if(ai){spots.AddRange(Object.FindObjectsByType<HidingSpot>(FindObjectsSortMode.None).Where(value=>!spots.Contains(value)));SetArray(ai,"hidingSpots",spots.ToArray());}
        }

        static void BuildWaypoints(Transform root)
        {
            Transform group=Group("Waypoints",root);List<Transform> points=new();for(int floor=-1;floor<=3;floor++){float y=floor<0?-3.15f:floor*FloorHeight;Vector3[] ring={new(-2.8f,y,-2),new(2.8f,y,-2),new(2.8f,y,2),new(-2.8f,y,2)};for(int i=0;i<ring.Length;i++){Transform point=new GameObject($"Floor {floor} Waypoint {i}").transform;point.SetParent(group);point.position=ring[i];points.Add(point);}}
            StalkerAI ai=Object.FindAnyObjectByType<StalkerAI>();if(ai)SetArray(ai,"waypoints",points.ToArray());
        }

        static void BuildOcclusion(Transform root)
        {
            Transform group=Group("Occlusion",root);for(int floor=-1;floor<=3;floor++){GameObject area=new($"Floor {floor} Occlusion Area");area.transform.SetParent(group);area.transform.position=new(0,floor<0?-1.8f:floor*FloorHeight+1.5f,0);var component=area.AddComponent<OcclusionArea>();component.size=new(10,3,10);}
            for(int i=0;i<4;i++){GameObject portal=new($"Floor {i} Stair Portal");portal.transform.SetParent(group);portal.transform.position=new(0,i*FloorHeight+.1f,0);portal.AddComponent<OcclusionPortal>().open=true;}
        }

        static void RelocateGameplayAnchors(string actName)
        {
            Move("Tower Door",new(0,0,-4.65f));MoveChild("Tower Door","Door panel",new(0,1.5f,0));
            Move("Radio",new(-2.6f,1,-.2f));Move("Hot Meal",new(-2.5f,.9f,-1.8f));Move("Table",new(-2.5f,.45f,-1.8f));Move("Steam placeholder",new(-2.5f,1.1f,-1.8f));
            Move("Lamp",new(0,11.1f,0));Move("Crank Station",new(2.2f,10.35f,0));
            Move("Locked Cellar Door",new(-4.2f,-1.7f,2));Move("Wet footprints (Act 2)",new(-1.5f,-3.15f,1));Move("Cellar Hum",new(-1.5f,-1.8f,1));Move("Reserve Fuel Can",new(-3,-2.75f,3));
            Move("hidden shelf",new(-5,-2.2f,2));Move("Marta reel-to-reel recorder",new(-7,-2.65f,2.5f));Move("Harbor board ledger",new(-7,-2.65f,.8f));
            Move("Crane's Room Door",new(-.8f,6.4f,-1.2f));Move("Family Photo",new(-4.15f,7.8f,1));Move("Crane's Desk",new(-2.6f,6.75f,1.8f));Move("Crane Room Key",new(10,.4f,-14));
            Move("Boat",new(-3.5f,.15f,-27));Move("Shoreline ending trigger",new(0,1,-25));
            GameObject player=GameObject.Find("Player");if(player)player.transform.position=actName=="Act1"?new(0,.15f,-27):new(0,9.8f,-1.5f);
        }

        static void BuildHud()
        {
            GameObject canvas=GameObject.Find("Debug Canvas");if(!canvas)return;CanvasScaler scaler=canvas.GetComponent<CanvasScaler>();if(scaler){scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new(1920,1080);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;scaler.matchWidthOrHeight=.5f;}
            Transform previous=canvas.transform.Find("HUD Root");if(previous)Object.DestroyImmediate(previous.gameObject);RectTransform root=Rect("HUD Root",canvas.transform,Vector2.zero,Vector2.one);UITheme theme=AssetDatabase.LoadAssetAtPath<UITheme>($"{SettingsFolder}/UITheme.asset");HudSettings settings=AssetDatabase.LoadAssetAtPath<HudSettings>($"{SettingsFolder}/HudSettings.asset");HudRoot hud=root.gameObject.AddComponent<HudRoot>();Set(hud,"theme",theme);Set(hud,"settings",settings);Set(hud,"scalableRoot",root);
            GameObject noise=Panel("NoiseMeter",root,new(.03f,.06f),new(.17f,.11f),theme.Panel);Image[] noiseSegments=new Image[3];for(int i=0;i<3;i++)noiseSegments[i]=Panel($"Noise {i+1}",noise.transform,new(.08f+i*.31f,.22f),new(.32f+i*.31f,.78f),theme.Panel).GetComponent<Image>();SetArray(hud,"noiseSegments",noiseSegments);Set(hud,"noiseRoot",noise);
            GameObject stamina=Panel("StaminaBar",root,new(.38f,.035f),new(.62f,.06f),theme.Panel);Image staminaFill=Panel("Fill",stamina.transform,Vector2.zero,Vector2.one,theme.Safe).GetComponent<Image>();staminaFill.type=Image.Type.Filled;staminaFill.fillMethod=Image.FillMethod.Horizontal;Set(hud,"staminaFill",staminaFill);Set(hud,"staminaRoot",stamina);
            GameObject lamp=Panel("LampGauge",root,new(.82f,.045f),new(.96f,.11f),theme.Panel);Image lampFill=Panel("Fill",lamp.transform,new(.05f,.12f),new(.95f,.5f),theme.Accent).GetComponent<Image>();lampFill.type=Image.Type.Filled;lampFill.fillMethod=Image.FillMethod.Horizontal;TMP_Text lampLabel=Text("State",lamp.transform,new(.05f,.52f),new(.95f,.96f),"LAMP",22,theme);Set(hud,"lampFill",lampFill);Set(hud,"lampState",lampLabel);Set(hud,"lampRoot",lamp);
            GameObject objective=Panel("ObjectiveText",root,new(.28f,.9f),new(.72f,.97f),theme.Panel);TMP_Text objectiveLabel=Text("Label",objective.transform,new(.03f,.05f),new(.97f,.95f),"",28,theme);Set(hud,"objectiveText",objectiveLabel);Set(hud,"objectiveRoot",objective);
            GameObject inventory=Panel("InventoryStrip",root,new(.31f,.075f),new(.69f,.12f),theme.Panel);TMP_Text inventoryLabel=Text("Items",inventory.transform,new(.03f,.05f),new(.97f,.95f),"",20,theme);Set(hud,"inventoryText",inventoryLabel);Set(hud,"inventoryRoot",inventory);
            InteractionPrompt prompt=Object.FindAnyObjectByType<InteractionPrompt>();GameObject promptRoot=prompt?prompt.gameObject:Panel("InteractionPrompt",root,new(.36f,.16f),new(.64f,.22f),theme.Panel);TMP_Text promptText=Text("Styled Prompt",promptRoot.transform,Vector2.zero,Vector2.one,"",24,theme);if(prompt){Set(prompt,"styledLabel",promptText);Set(prompt,"keyIcon",(Object)null);}Set(hud,"interactionRoot",promptRoot);
            Toast(hud,root,"CheckpointToast","checkpointToast","checkpointText",new(.37f,.72f),new(.63f,.78f),theme);Toast(hud,root,"PageAddedToast","pageToast","pageText",new(.37f,.78f),new(.63f,.84f),theme);Toast(hud,root,"RoomNameFlash","roomToast","roomText",new(.35f,.62f),new(.65f,.7f),theme);
            GameObject composure=Panel("ComposureMeter",root,new(.44f,.01f),new(.56f,.025f),theme.Panel);Image composureFill=Panel("Fill",composure.transform,Vector2.zero,Vector2.one,theme.Safe).GetComponent<Image>();composureFill.type=Image.Type.Filled;composureFill.fillMethod=Image.FillMethod.Horizontal;Set(hud,"composureRoot",composure);Set(hud,"composureFill",composureFill);
            GameObject pulse=Panel("Composure Edge Pulse",root,Vector2.zero,Vector2.one,new Color(theme.Danger.r,theme.Danger.g,theme.Danger.b,0));pulse.GetComponent<Image>().raycastTarget=false;Set(hud,"screenEdgePulse",pulse.GetComponent<Image>());
            SubtitleUI subtitles=Object.FindAnyObjectByType<SubtitleUI>();if(subtitles){Transform oldBackground=canvas.transform.Find("Subtitle Background");if(oldBackground)Object.DestroyImmediate(oldBackground.gameObject);GameObject background=Panel("Subtitle Background",canvas.transform,new(.16f,.025f),new(.84f,.115f),new Color(theme.Panel.r,theme.Panel.g,theme.Panel.b,settings.SubtitleBackgroundOpacity));background.GetComponent<Image>().raycastTarget=false;background.transform.SetSiblingIndex(Mathf.Max(0,subtitles.transform.GetSiblingIndex()-1));Set(subtitles,"background",background.GetComponent<Image>());subtitles.ApplySettings(settings);}
            BuildPauseMenu(canvas.transform,theme);
        }

        static void Toast(HudRoot hud,Transform root,string name,string groupField,string textField,Vector2 min,Vector2 max,UITheme theme){GameObject go=Panel(name,root,min,max,theme.Panel);CanvasGroup group=go.AddComponent<CanvasGroup>();TMP_Text text=Text("Label",go.transform,new(.03f,.05f),new(.97f,.95f),"",22,theme);Set(hud,groupField,group);Set(hud,textField,text);}
        static void BuildPauseMenu(Transform canvas,UITheme theme)
        {
            Transform old=canvas.Find("Pause Menu");if(old)Object.DestroyImmediate(old.gameObject);PauseMenu existing=canvas.GetComponent<PauseMenu>();if(existing)Object.DestroyImmediate(existing);GameObject panel=Panel("Pause Menu",canvas,new(.32f,.18f),new(.68f,.82f),new Color(theme.Panel.r,theme.Panel.g,theme.Panel.b,.97f));PauseMenu pause=canvas.gameObject.AddComponent<PauseMenu>();Set(pause,"panel",panel);Set(pause,"player",Object.FindAnyObjectByType<FirstPersonController>());Set(pause,"interactor",Object.FindAnyObjectByType<Interactor>());Set(pause,"journal",Object.FindAnyObjectByType<JournalUI>());
            Text("Title",panel.transform,new(.08f,.84f),new(.92f,.96f),"PAUSED",38,theme);string[] names={"Resume","Journal","Options","Save","Quit to MainMenu"};for(int i=0;i<names.Length;i++){Button button=Button(names[i],panel.transform,new(.15f,.68f-i*.12f),new(.85f,.77f-i*.12f),theme);switch(i){case 0:UnityEventTools.AddPersistentListener(button.onClick,pause.Resume);break;case 1:UnityEventTools.AddPersistentListener(button.onClick,pause.OpenJournal);break;case 2:UnityEventTools.AddPersistentListener(button.onClick,pause.OpenOptions);break;case 3:UnityEventTools.AddPersistentListener(button.onClick,pause.Save);break;case 4:UnityEventTools.AddPersistentListener(button.onClick,pause.QuitToMainMenu);break;}}
            GameObject options=Panel("Options",panel.transform,new(.04f,.03f),new(.96f,.83f),theme.Panel);HudSettings hudSettings=AssetDatabase.LoadAssetAtPath<HudSettings>($"{SettingsFolder}/HudSettings.asset");HudSettingsPanel controller=options.AddComponent<HudSettingsPanel>();Set(controller,"settings",hudSettings);Set(controller,"hud",Object.FindAnyObjectByType<HudRoot>());Set(controller,"subtitles",Object.FindAnyObjectByType<SubtitleUI>());Text("Options Label",options.transform,new(.05f,.88f),new(.95f,.98f),"HUD OPTIONS",25,theme);
            string[] labels={"Noise","Stamina","Lamp","Objective","Inventory","Interaction","Toasts","Room names","Composure","Minimal HUD"};bool[] values={true,true,true,true,true,true,true,true,false,false};for(int i=0;i<labels.Length;i++){int column=i/5;int row=i%5;Toggle toggle=OptionToggle(labels[i],options.transform,new(.05f+column*.48f,.74f-row*.13f),new(.47f+column*.48f,.84f-row*.13f),theme,values[i]);switch(i){case 0:UnityEventTools.AddPersistentListener(toggle.onValueChanged,controller.SetNoise);break;case 1:UnityEventTools.AddPersistentListener(toggle.onValueChanged,controller.SetStamina);break;case 2:UnityEventTools.AddPersistentListener(toggle.onValueChanged,controller.SetLamp);break;case 3:UnityEventTools.AddPersistentListener(toggle.onValueChanged,controller.SetObjective);break;case 4:UnityEventTools.AddPersistentListener(toggle.onValueChanged,controller.SetInventory);break;case 5:UnityEventTools.AddPersistentListener(toggle.onValueChanged,controller.SetInteraction);break;case 6:UnityEventTools.AddPersistentListener(toggle.onValueChanged,controller.SetToasts);break;case 7:UnityEventTools.AddPersistentListener(toggle.onValueChanged,controller.SetRoomNames);break;case 8:UnityEventTools.AddPersistentListener(toggle.onValueChanged,controller.SetComposure);break;case 9:UnityEventTools.AddPersistentListener(toggle.onValueChanged,controller.SetMinimal);break;}}
            Slider scale=OptionSlider("HUD Scale",options.transform,new(.08f,.04f),new(.92f,.12f),theme,.7f,1.5f,1f);UnityEventTools.AddPersistentListener(scale.onValueChanged,controller.SetHudScale);options.SetActive(false);Set(pause,"optionsPanel",options);
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
            string[] exact={"Greybox Tower","South wall left","South wall right","Door lintel","Dock","Path","Boat Shed Floor","Boat Shed Back","Boat Shed Roof","Lantern Room Floor","Lantern Room Floor Back","Stair Top Landing","Storeroom rear wall","Storeroom divider","East window sill","East window lintel","Window pier south","Window pier middle","Window pier north","Shallows","Cellar floor","Cellar north wall","Cellar south wall","Cellar west wall","Hidden room floor","Hidden room west wall","Hidden room north wall","Hidden room south wall","Crane room partition","Flooded courtyard","Lower window pressure ledge"};foreach(string name in exact){GameObject go=GameObject.Find(name);if(go)Object.DestroyImmediate(go);}foreach(GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))if(go.name.StartsWith("Stair ",StringComparison.Ordinal)&&int.TryParse(go.name.Substring(6),out _))Object.DestroyImmediate(go);
        }

        static GameObject Place(string name,Vector3 position,Vector3 scale,string prefab,string surface,Transform parent){GameObject source=AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabFolder}/{prefab}.prefab");GameObject go=(GameObject)PrefabUtility.InstantiatePrefab(source);go.name=name;go.transform.SetParent(parent);go.transform.position=position;go.transform.localScale=scale;go.tag=surface;go.GetComponent<Renderer>().sharedMaterial=Materials[surface];GameObjectUtility.SetStaticEditorFlags(go,StaticEditorFlags.BatchingStatic|StaticEditorFlags.OccludeeStatic|StaticEditorFlags.OccluderStatic|StaticEditorFlags.NavigationStatic);return go;}
        static void Wall(string name,Vector3 position,Vector3 scale,Transform parent)=>Place(name,position,scale,"Wall","Stone",parent);
        static Transform Group(string name,Transform parent){Transform group=new GameObject(name).transform;group.SetParent(parent);return group;}
        static void Bounds(Transform parent,Vector3 position,Vector3 size){GameObject go=new("Invisible Bounds");go.transform.SetParent(parent);go.transform.position=position;BoxCollider box=go.AddComponent<BoxCollider>();box.size=size;}
        static void Move(string name,Vector3 position){GameObject go=GameObject.Find(name);if(go)go.transform.position=position;}
        static void MoveChild(string rootName,string childName,Vector3 localPosition){GameObject root=GameObject.Find(rootName);if(!root)return;Transform child=root.transform.Find(childName);if(child)child.localPosition=localPosition;}
        static void Set(Object target,string name,Object value){if(!target)return;SerializedObject serialized=new(target);SerializedProperty property=serialized.FindProperty(name);if(property!=null){property.objectReferenceValue=value;serialized.ApplyModifiedPropertiesWithoutUndo();}}
        static void Set(Object target,string name,string value){if(!target)return;SerializedObject serialized=new(target);SerializedProperty property=serialized.FindProperty(name);if(property!=null){property.stringValue=value;serialized.ApplyModifiedPropertiesWithoutUndo();}}
        static void Set(Object target,string name,int value){if(!target)return;SerializedObject serialized=new(target);SerializedProperty property=serialized.FindProperty(name);if(property!=null){property.intValue=value;serialized.ApplyModifiedPropertiesWithoutUndo();}}
        static void SetArray<T>(Object target,string name,T[] values) where T:Object{if(!target)return;SerializedObject serialized=new(target);SerializedProperty property=serialized.FindProperty(name);if(property==null)return;property.arraySize=values.Length;for(int i=0;i<values.Length;i++)property.GetArrayElementAtIndex(i).objectReferenceValue=values[i];serialized.ApplyModifiedPropertiesWithoutUndo();}
    }
}
