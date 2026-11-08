using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace TheLightHouse.Editor
{
    public static class FPVAssetSetup
    {
        private const string TexRoot = "Assets/_Project/Art/Textures/FPV";
        private const string MatRoot = "Assets/_Project/Materials/Reference";
        private const string AnimRoot = "Assets/_Project/Animations/FPV";

        public static void SetupAll()
        {
            SetupTextureImportSettings();
            CreateMaterials();
            CreateAnimationClipsAndController();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("<color=#44FF88>[FPV Setup Complete]</color> Textures, Materials, Animation Clips, and Controller successfully configured.");
        }

        public static void SetupTextureImportSettings()
        {
            string[] normalMaps = {
                $"{TexRoot}/MarinerWool_nor.png",
                $"{TexRoot}/OilskinCuff_nor.png",
                $"{TexRoot}/SailorHand_nor.png"
            };

            foreach (var path in normalMaps)
            {
                if (File.Exists(path))
                {
                    var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (importer && importer.textureType != TextureImporterType.NormalMap)
                    {
                        importer.textureType = TextureImporterType.NormalMap;
                        importer.SaveAndReimport();
                    }
                }
            }
        }

        public static void CreateMaterials()
        {
            if (!Directory.Exists(MatRoot)) Directory.CreateDirectory(MatRoot);

            Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (!litShader) litShader = Shader.Find("Standard");

            // 1. Mariner Wool Material
            CreateOrUpdateMaterial("FPV_MarinerWool.mat", litShader,
                diffusePath: $"{TexRoot}/MarinerWool_diff.png",
                normalPath: $"{TexRoot}/MarinerWool_nor.png",
                baseColor: new Color(0.18f, 0.22f, 0.28f), // Deep navy charcoal
                smoothness: 0.35f, // Wet ocean spray sheen
                metallic: 0.0f);

            // 2. Oilskin Jacket Cuff Material
            CreateOrUpdateMaterial("FPV_OilskinCuff.mat", litShader,
                diffusePath: $"{TexRoot}/OilskinCuff_diff.png",
                normalPath: $"{TexRoot}/OilskinCuff_nor.png",
                baseColor: new Color(0.28f, 0.30f, 0.22f), // Weathered dark olive wax
                smoothness: 0.58f, // Water-resistant wax sheen
                metallic: 0.05f);

            // 3. Sailor Hand Material
            CreateOrUpdateMaterial("FPV_SailorHand.mat", litShader,
                diffusePath: $"{TexRoot}/SailorHand_diff.png",
                normalPath: $"{TexRoot}/SailorHand_nor.png",
                baseColor: new Color(0.72f, 0.56f, 0.48f), // Weathered sailor flesh
                smoothness: 0.42f, // Wet cold dampness
                metallic: 0.0f);
        }

        private static Material CreateOrUpdateMaterial(string fileName, Shader shader, string diffusePath, string normalPath, Color baseColor, float smoothness, float metallic)
        {
            string fullPath = $"{MatRoot}/{fileName}";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(fullPath);
            if (!mat)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, fullPath);
            }

            mat.color = baseColor;
            mat.SetFloat("_Smoothness", smoothness);
            mat.SetFloat("_Metallic", metallic);

            if (File.Exists(diffusePath))
            {
                var diffTex = AssetDatabase.LoadAssetAtPath<Texture2D>(diffusePath);
                if (diffTex) mat.mainTexture = diffTex;
            }

            if (File.Exists(normalPath))
            {
                var norTex = AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);
                if (norTex)
                {
                    mat.SetTexture("_BumpMap", norTex);
                    mat.EnableKeyword("_NORMALMAP");
                }
            }

            EditorUtility.SetDirty(mat);
            return mat;
        }

        public static void CreateAnimationClipsAndController()
        {
            if (!Directory.Exists(AnimRoot)) Directory.CreateDirectory(AnimRoot);

            // Create Animation Clips
            var clipHidden = CreatePocketHiddenClip();
            var clipDraw = CreateDrawFromPocketClip();
            var clipIdle = CreateTorchIdleClip();
            var clipPut = CreatePutIntoPocketClip();

            // Create Animator Controller
            string controllerPath = $"{AnimRoot}/FPV_Flashlight_Hands.controller";
            var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

            // Add Parameter
            controller.AddParameter("isEquipped", AnimatorControllerParameterType.Bool);

            var rootStateMachine = controller.layers[0].stateMachine;

            // Add States
            var stateHidden = rootStateMachine.AddState("Pocket_Hidden");
            stateHidden.motion = clipHidden;

            var stateDraw = rootStateMachine.AddState("Draw_From_Pocket");
            stateDraw.motion = clipDraw;

            var stateIdle = rootStateMachine.AddState("Torch_Idle");
            stateIdle.motion = clipIdle;

            var statePut = rootStateMachine.AddState("Put_Into_Pocket");
            statePut.motion = clipPut;

            // Default State
            rootStateMachine.defaultState = stateHidden;

            // Transition 1: Pocket_Hidden -> Draw_From_Pocket (isEquipped == true)
            var transDraw = stateHidden.AddTransition(stateDraw);
            transDraw.AddCondition(AnimatorConditionMode.If, 0, "isEquipped");
            transDraw.hasExitTime = false;
            transDraw.duration = 0.10f;

            // Transition 2: Draw_From_Pocket -> Torch_Idle (Exit Time at end of draw)
            var transIdle = stateDraw.AddTransition(stateIdle);
            transIdle.hasExitTime = true;
            transIdle.exitTime = 1.0f;
            transIdle.duration = 0.12f;

            // Transition 3: Torch_Idle -> Put_Into_Pocket (isEquipped == false)
            var transPut = stateIdle.AddTransition(statePut);
            transPut.AddCondition(AnimatorConditionMode.IfNot, 0, "isEquipped");
            transPut.hasExitTime = false;
            transPut.duration = 0.08f;

            // Transition 4: Put_Into_Pocket -> Pocket_Hidden (Exit Time at end of holster)
            var transHidden = statePut.AddTransition(stateHidden);
            transHidden.hasExitTime = true;
            transHidden.exitTime = 1.0f;
            transHidden.duration = 0.10f;

            EditorUtility.SetDirty(controller);
        }

        private static AnimationClip CreatePocketHiddenClip()
        {
            string path = $"{AnimRoot}/Pocket_Hidden.anim";
            var clip = new AnimationClip { name = "Pocket_Hidden" };

            // Lowered down and back out of camera view
            var curveX = AnimationCurve.Constant(0f, 1f, 0.28f);
            var curveY = AnimationCurve.Constant(0f, 1f, -0.72f);
            var curveZ = AnimationCurve.Constant(0f, 1f, 0.22f);

            var rotX = AnimationCurve.Constant(0f, 1f, 52f);
            var rotY = AnimationCurve.Constant(0f, 1f, -22f);
            var rotZ = AnimationCurve.Constant(0f, 1f, -28f);

            SetPositionCurves(clip, "Arm_R_Root", curveX, curveY, curveZ);
            SetEulerRotationCurves(clip, "Arm_R_Root", rotX, rotY, rotZ);

            AssetDatabase.CreateAsset(clip, path);
            return clip;
        }

        private static AnimationClip CreateDrawFromPocketClip()
        {
            string path = $"{AnimRoot}/Draw_From_Pocket.anim";
            var clip = new AnimationClip { name = "Draw_From_Pocket" };

            float duration = 0.60f;

            // Position: from pocket (0.28, -0.72, 0.22) rising up into frame (0.20, -0.20, 0.44)
            var posX = new AnimationCurve(
                new Keyframe(0.00f, 0.28f, 0f, -0.2f),
                new Keyframe(0.35f, 0.22f, -0.1f, -0.05f),
                new Keyframe(duration, 0.20f, 0f, 0f));

            var posY = new AnimationCurve(
                new Keyframe(0.00f, -0.72f, 0f, 1.8f),
                new Keyframe(0.35f, -0.26f, 0.8f, 0.3f),
                new Keyframe(0.48f, -0.19f, 0.2f, -0.05f), // Slight overshoot for organic weight
                new Keyframe(duration, -0.20f, 0f, 0f));

            var posZ = new AnimationCurve(
                new Keyframe(0.00f, 0.22f, 0f, 0.6f),
                new Keyframe(0.35f, 0.38f, 0.3f, 0.15f),
                new Keyframe(duration, 0.44f, 0f, 0f));

            // Rotation: tilts up from pocket angle (52, -22, -28) to idle level (2, -2, 0)
            var rotX = new AnimationCurve(
                new Keyframe(0.00f, 52f, 0f, -140f),
                new Keyframe(0.35f, 12f, -60f, -20f),
                new Keyframe(duration, 2f, 0f, 0f));

            var rotY = new AnimationCurve(
                new Keyframe(0.00f, -22f, 0f, 45f),
                new Keyframe(0.35f, -6f, 25f, 10f),
                new Keyframe(duration, -2f, 0f, 0f));

            var rotZ = new AnimationCurve(
                new Keyframe(0.00f, -28f, 0f, 65f),
                new Keyframe(0.35f, -6f, 30f, 12f),
                new Keyframe(duration, 0f, 0f, 0f));

            // Thumb switch click action: thumb raises slightly at 0.35s and clicks down on switch at 0.45s
            var thumbX = new AnimationCurve(
                new Keyframe(0.00f, 0f),
                new Keyframe(0.32f, -12f),
                new Keyframe(0.45f, 16f), // Depress switch
                new Keyframe(duration, 5f));

            SetPositionCurves(clip, "Arm_R_Root", posX, posY, posZ);
            SetEulerRotationCurves(clip, "Arm_R_Root", rotX, rotY, rotZ);
            SetEulerRotationCurves(clip, "Arm_R_Root/Arm_R_Sleeve/Arm_R_Forearm/Arm_R_Wrist/Hand_R_Palm/Thumb_Root/Thumb_Distal",
                thumbX, AnimationCurve.Constant(0f, duration, 0f), AnimationCurve.Constant(0f, duration, 0f));

            // Animation Events:
            // 1. Cloth rustle right as arm leaves pocket (t = 0.05s)
            var rustleEv = new AnimationEvent { time = 0.05f, functionName = "PlayClothRustle" };
            // 2. Fire TurnOnLight at t = 0.45s (Thumb depresses switch, click sound, spotlight on)
            var turnOnEv = new AnimationEvent { time = 0.45f, functionName = "TurnOnLight" };

            AnimationUtility.SetAnimationEvents(clip, new[] { rustleEv, turnOnEv });

            AssetDatabase.CreateAsset(clip, path);
            return clip;
        }

        private static AnimationClip CreateTorchIdleClip()
        {
            string path = $"{AnimRoot}/Torch_Idle.anim";
            var clip = new AnimationClip { name = "Torch_Idle" };

            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            float duration = 2.4f;

            // Subtle organic breathing motion
            var posX = new AnimationCurve(
                new Keyframe(0.0f, 0.200f),
                new Keyframe(1.2f, 0.203f),
                new Keyframe(2.4f, 0.200f));

            var posY = new AnimationCurve(
                new Keyframe(0.0f, -0.200f),
                new Keyframe(1.2f, -0.194f),
                new Keyframe(2.4f, -0.200f));

            var posZ = new AnimationCurve(
                new Keyframe(0.0f, 0.440f),
                new Keyframe(1.2f, 0.442f),
                new Keyframe(2.4f, 0.440f));

            var rotX = new AnimationCurve(
                new Keyframe(0.0f, 2.0f),
                new Keyframe(1.2f, 2.8f),
                new Keyframe(2.4f, 2.0f));

            var rotY = new AnimationCurve(
                new Keyframe(0.0f, -2.0f),
                new Keyframe(1.2f, -1.6f),
                new Keyframe(2.4f, -2.0f));

            var rotZ = new AnimationCurve(
                new Keyframe(0.0f, 0.0f),
                new Keyframe(1.2f, 0.4f),
                new Keyframe(2.4f, 0.0f));

            SetPositionCurves(clip, "Arm_R_Root", posX, posY, posZ);
            SetEulerRotationCurves(clip, "Arm_R_Root", rotX, rotY, rotZ);

            AssetDatabase.CreateAsset(clip, path);
            return clip;
        }

        private static AnimationClip CreatePutIntoPocketClip()
        {
            string path = $"{AnimRoot}/Put_Into_Pocket.anim";
            var clip = new AnimationClip { name = "Put_Into_Pocket" };

            float duration = 0.50f;

            // Position: descends down and into pocket (0.20, -0.20, 0.44) -> (0.28, -0.72, 0.22)
            var posX = new AnimationCurve(
                new Keyframe(0.00f, 0.20f, 0f, 0.1f),
                new Keyframe(0.25f, 0.24f, 0.15f, 0.2f),
                new Keyframe(duration, 0.28f, 0f, 0f));

            var posY = new AnimationCurve(
                new Keyframe(0.00f, -0.20f, 0f, -0.4f),
                new Keyframe(0.25f, -0.42f, -1.2f, -1.6f),
                new Keyframe(duration, -0.72f, 0f, 0f));

            var posZ = new AnimationCurve(
                new Keyframe(0.00f, 0.44f, 0f, -0.2f),
                new Keyframe(0.25f, 0.32f, -0.4f, -0.5f),
                new Keyframe(duration, 0.22f, 0f, 0f));

            // Rotation: tilts downward into coat pocket
            var rotX = new AnimationCurve(
                new Keyframe(0.00f, 2f, 0f, 40f),
                new Keyframe(0.25f, 26f, 80f, 110f),
                new Keyframe(duration, 52f, 0f, 0f));

            var rotY = new AnimationCurve(
                new Keyframe(0.00f, -2f, 0f, -20f),
                new Keyframe(0.25f, -14f, -35f, -40f),
                new Keyframe(duration, -22f, 0f, 0f));

            var rotZ = new AnimationCurve(
                new Keyframe(0.00f, 0f, 0f, -30f),
                new Keyframe(0.25f, -15f, -50f, -60f),
                new Keyframe(duration, -28f, 0f, 0f));

            // Thumb flick back switch off at start
            var thumbX = new AnimationCurve(
                new Keyframe(0.00f, 5f),
                new Keyframe(0.06f, -14f),
                new Keyframe(duration, 0f));

            SetPositionCurves(clip, "Arm_R_Root", posX, posY, posZ);
            SetEulerRotationCurves(clip, "Arm_R_Root", rotX, rotY, rotZ);
            SetEulerRotationCurves(clip, "Arm_R_Root/Arm_R_Sleeve/Arm_R_Forearm/Arm_R_Wrist/Hand_R_Palm/Thumb_Root/Thumb_Distal",
                thumbX, AnimationCurve.Constant(0f, duration, 0f), AnimationCurve.Constant(0f, duration, 0f));

            // Animation Events:
            // 1. Cloth rustle right as arm descends into pocket (t = 0.05s)
            var rustleEv = new AnimationEvent { time = 0.05f, functionName = "PlayClothRustle" };
            // 2. Disable torch mesh / complete unequip at t = 0.48s
            var disableEv = new AnimationEvent { time = 0.48f, functionName = "DisableTorchMesh" };

            AnimationUtility.SetAnimationEvents(clip, new[] { rustleEv, disableEv });

            AssetDatabase.CreateAsset(clip, path);
            return clip;
        }

        private static void SetPositionCurves(AnimationClip clip, string relativePath, AnimationCurve x, AnimationCurve y, AnimationCurve z)
        {
            clip.SetCurve(relativePath, typeof(Transform), "m_LocalPosition.x", x);
            clip.SetCurve(relativePath, typeof(Transform), "m_LocalPosition.y", y);
            clip.SetCurve(relativePath, typeof(Transform), "m_LocalPosition.z", z);
        }

        private static void SetEulerRotationCurves(AnimationClip clip, string relativePath, AnimationCurve x, AnimationCurve y, AnimationCurve z)
        {
            clip.SetCurve(relativePath, typeof(Transform), "localEulerAnglesRaw.x", x);
            clip.SetCurve(relativePath, typeof(Transform), "localEulerAnglesRaw.y", y);
            clip.SetCurve(relativePath, typeof(Transform), "localEulerAnglesRaw.z", z);
        }
    }
}
