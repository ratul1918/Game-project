using UnityEngine;

namespace TheLightHouse
{
    /// <summary>
    /// Procedurally constructs Daniel Hale's rigged first-person arms with heavy mariner
    /// wool sweater, weathered oilskin jacket cuff, and weathered sailor hand with skeletal joints.
    /// Sockets the 1940s vintage ribbed flashlight to Hand_R_Socket and links the lens light rig.
    /// </summary>
    public static class FPVArmsRigBuilder
    {
        private const string MarinerWoolMatPath = "Assets/_Project/Materials/Reference/FPV_MarinerWool.mat";
        private const string OilskinCuffMatPath = "Assets/_Project/Materials/Reference/FPV_OilskinCuff.mat";
        private const string SailorHandMatPath = "Assets/_Project/Materials/Reference/FPV_SailorHand.mat";

        public static GameObject BuildFPVArmsRig(Transform cameraTransform, out FPVFlashlightHandsController handsController)
        {
            var existing = cameraTransform.Find("FPV_Flashlight_Hands");
            if (existing)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            var rootGo = new GameObject("FPV_Flashlight_Hands");
            rootGo.transform.SetParent(cameraTransform, false);
            rootGo.transform.localPosition = Vector3.zero;
            rootGo.transform.localRotation = Quaternion.identity;

            // Load Materials
            var matWool = LoadMaterial(MarinerWoolMatPath, new Color(0.18f, 0.22f, 0.28f));
            var matOilskin = LoadMaterial(OilskinCuffMatPath, new Color(0.28f, 0.30f, 0.22f));
            var matSkin = LoadMaterial(SailorHandMatPath, new Color(0.72f, 0.56f, 0.48f));

            // Arm_R_Root (Animated by Animator and offset by procedural camera lag/sway)
            var armRoot = new GameObject("Arm_R_Root");
            armRoot.transform.SetParent(rootGo.transform, false);
            armRoot.transform.localPosition = new Vector3(0.20f, -0.20f, 0.44f);
            armRoot.transform.localRotation = Quaternion.Euler(2.0f, -2.0f, 0.0f);

            // Shoulder / Sleeve Base
            var shoulder = new GameObject("Arm_R_Sleeve");
            shoulder.transform.SetParent(armRoot.transform, false);
            shoulder.transform.localPosition = new Vector3(0.04f, 0.04f, -0.10f);
            shoulder.transform.localRotation = Quaternion.Euler(10.0f, -8.0f, 0.0f);

            // Forearm (Heavy Mariner Wool Sweater)
            var forearm = new GameObject("Arm_R_Forearm");
            forearm.transform.SetParent(shoulder.transform, false);
            forearm.transform.localPosition = new Vector3(-0.02f, 0.02f, 0.16f);
            forearm.transform.localRotation = Quaternion.Euler(14.0f, 4.0f, -4.0f);

            // Mariner Wool Sleeve Cylinder (Extending back towards shoulder offscreen)
            var sleeveMesh = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            sleeveMesh.name = "Heavy_Mariner_Wool_Sleeve";
            sleeveMesh.transform.SetParent(forearm.transform, false);
            sleeveMesh.transform.localPosition = new Vector3(0f, 0f, -0.06f);
            sleeveMesh.transform.localRotation = Quaternion.Euler(80f, 0f, 0f);
            sleeveMesh.transform.localScale = new Vector3(0.082f, 0.26f, 0.078f);
            sleeveMesh.GetComponent<Renderer>().sharedMaterial = matWool;
            Object.DestroyImmediate(sleeveMesh.GetComponent<Collider>());

            // Oilskin Jacket Cuff Band
            var cuffMesh = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cuffMesh.name = "Oilskin_Jacket_Cuff";
            cuffMesh.transform.SetParent(forearm.transform, false);
            cuffMesh.transform.localPosition = new Vector3(0f, 0.012f, 0.11f);
            cuffMesh.transform.localRotation = Quaternion.Euler(80f, 0f, 0f);
            cuffMesh.transform.localScale = new Vector3(0.088f, 0.045f, 0.084f);
            cuffMesh.GetComponent<Renderer>().sharedMaterial = matOilskin;
            Object.DestroyImmediate(cuffMesh.GetComponent<Collider>());

            // Wrist Bone
            var wrist = new GameObject("Arm_R_Wrist");
            wrist.transform.SetParent(forearm.transform, false);
            wrist.transform.localPosition = new Vector3(0f, 0.02f, 0.18f);
            wrist.transform.localRotation = Quaternion.Euler(-10f, 8f, 4f);

            // Hand Palm Bone (Scale 1,1,1 so children don't inherit non-uniform squash)
            var palm = new GameObject("Hand_R_Palm");
            palm.transform.SetParent(wrist.transform, false);
            palm.transform.localPosition = new Vector3(0f, 0.025f, 0.045f);
            palm.transform.localRotation = Quaternion.Euler(-12f, 0f, 0f);

            // Palm Visual Mesh
            var palmMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            palmMesh.name = "Hand_R_Palm_Mesh";
            palmMesh.transform.SetParent(palm.transform, false);
            palmMesh.transform.localPosition = Vector3.zero;
            palmMesh.transform.localRotation = Quaternion.identity;
            palmMesh.transform.localScale = new Vector3(0.072f, 0.038f, 0.085f);
            palmMesh.GetComponent<Renderer>().sharedMaterial = matSkin;
            Object.DestroyImmediate(palmMesh.GetComponent<Collider>());

            // Thumb Joints
            var thumbRoot = new GameObject("Thumb_Root");
            thumbRoot.transform.SetParent(palm.transform, false);
            thumbRoot.transform.localPosition = new Vector3(-0.028f, 0.022f, -0.015f);
            thumbRoot.transform.localRotation = Quaternion.Euler(15f, 25f, -15f);

            var thumbProx = GameObject.CreatePrimitive(PrimitiveType.Cube);
            thumbProx.name = "Thumb_Proximal_Mesh";
            thumbProx.transform.SetParent(thumbRoot.transform, false);
            thumbProx.transform.localPosition = new Vector3(0f, 0.010f, 0.016f);
            thumbProx.transform.localScale = new Vector3(0.020f, 0.018f, 0.032f);
            thumbProx.GetComponent<Renderer>().sharedMaterial = matSkin;
            Object.DestroyImmediate(thumbProx.GetComponent<Collider>());

            var thumbDist = new GameObject("Thumb_Distal");
            thumbDist.transform.SetParent(thumbRoot.transform, false);
            thumbDist.transform.localPosition = new Vector3(0f, 0.018f, 0.030f);
            thumbDist.transform.localRotation = Quaternion.Euler(12f, 0f, 0f);

            var thumbTip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            thumbTip.name = "Thumb_Distal_Mesh";
            thumbTip.transform.SetParent(thumbDist.transform, false);
            thumbTip.transform.localPosition = new Vector3(0f, 0.008f, 0.014f);
            thumbTip.transform.localScale = new Vector3(0.018f, 0.016f, 0.026f);
            thumbTip.GetComponent<Renderer>().sharedMaterial = matSkin;
            Object.DestroyImmediate(thumbTip.GetComponent<Collider>());

            // Curled Grip Fingers (Wrapping around Flashlight Barrel)
            string[] fingerNames = { "Index", "Middle", "Ring", "Pinky" };
            float[] fingerOffsets = { 0.032f, 0.011f, -0.010f, -0.030f };
            float[] fingerLengths = { 0.028f, 0.030f, 0.027f, 0.022f };

            for (int i = 0; i < 4; i++)
            {
                var fingerBase = new GameObject($"Finger_{fingerNames[i]}");
                fingerBase.transform.SetParent(palm.transform, false);
                fingerBase.transform.localPosition = new Vector3(0.035f, -0.010f, fingerOffsets[i]);
                fingerBase.transform.localRotation = Quaternion.Euler(35f, 0f, -95f);

                var fMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
                fMesh.name = $"{fingerNames[i]}_Segment";
                fMesh.transform.SetParent(fingerBase.transform, false);
                fMesh.transform.localPosition = new Vector3(0f, 0f, fingerLengths[i] * 0.5f);
                fMesh.transform.localScale = new Vector3(0.018f, 0.016f, fingerLengths[i]);
                fMesh.GetComponent<Renderer>().sharedMaterial = matSkin;
                Object.DestroyImmediate(fMesh.GetComponent<Collider>());
            }

            // Right Hand Grip Socket Bone
            var socketGo = new GameObject("Hand_R_Socket");
            socketGo.transform.SetParent(palm.transform, false);
            socketGo.transform.localPosition = new Vector3(-0.005f, 0.010f, 0.025f);
            socketGo.transform.localRotation = Quaternion.Euler(-2f, -4f, 0f); // Aligns barrel forward with crosshair

            // Construct 1940s Vintage Flashlight Model under Hand_R_Socket
            var flashlightGo = new GameObject("Player_Flashlight");
            flashlightGo.transform.SetParent(socketGo.transform, false);
            flashlightGo.transform.localPosition = Vector3.zero;
            flashlightGo.transform.localRotation = Quaternion.identity;

            var model = FlashlightModelBuilder.BuildFlashlightModel(flashlightGo.transform, out var bulbRend, out var bulbMat);

            // Front Lens Socket for Light Components
            var lensSocket = new GameObject("Flashlight_Lens_Socket");
            lensSocket.transform.SetParent(flashlightGo.transform, false);
            lensSocket.transform.localPosition = new Vector3(0f, 0f, 0.075f);
            lensSocket.transform.localRotation = Quaternion.identity;

            int vmLayer = LayerMask.NameToLayer("Viewmodel");
            if (vmLayer < 0) vmLayer = 2;

            // Spot Light
            var spotGo = new GameObject("Flashlight_Primary_Spot");
            spotGo.transform.SetParent(lensSocket.transform, false);
            spotGo.transform.localPosition = Vector3.zero;
            spotGo.transform.localRotation = Quaternion.identity;
            var spot = spotGo.AddComponent<Light>();
            spot.type = LightType.Spot;
            spot.range = 22f;
            spot.innerSpotAngle = 18f;
            spot.spotAngle = 38f;
            spot.color = new Color(1.0f, 0.957f, 0.839f);
            spot.intensity = 24f;
            spot.shadows = LightShadows.Soft;
            spot.shadowNearPlane = 0.05f;
            spot.cullingMask &= ~(1 << vmLayer);
            spot.renderingLayerMask = 1;

#if UNITY_EDITOR
            var cookie = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Project/Art/Textures/Lighting/flashlight_cookie_grungy.png");
            if (cookie) spot.cookie = cookie;
#endif

            // Ambient Fill Point Light
            var fillGo = new GameObject("Flashlight_Ambient_Fill");
            fillGo.transform.SetParent(lensSocket.transform, false);
            fillGo.transform.localPosition = Vector3.zero;
            fillGo.transform.localRotation = Quaternion.identity;
            var fill = fillGo.AddComponent<Light>();
            fill.type = LightType.Point;
            fill.range = 1.25f;
            fill.color = new Color(1.0f, 0.957f, 0.839f);
            fill.intensity = 0f;
            fill.enabled = false;
            fill.shadows = LightShadows.None;
            fill.cullingMask &= ~(1 << vmLayer);
            fill.renderingLayerMask = 1;

            // Audio Sources (1 for switch click & rustle, 1 for continuous filament hum loop)
            var sfxAudio = rootGo.AddComponent<AudioSource>();
            sfxAudio.spatialBlend = 0f;
            sfxAudio.playOnAwake = false;
            sfxAudio.priority = 48;

            var humAudio = flashlightGo.AddComponent<AudioSource>();
            humAudio.spatialBlend = 0f;
            humAudio.loop = true;
            humAudio.playOnAwake = false;
            humAudio.volume = 0.08f;
            humAudio.priority = 64;

            // Animator
            var animator = rootGo.AddComponent<Animator>();
#if UNITY_EDITOR
            var controller = UnityEditor.AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/_Project/Animations/FPV/FPV_Flashlight_Hands.controller");
            if (controller) animator.runtimeAnimatorController = controller;
#endif

            // Attach FPV Controller
            handsController = rootGo.AddComponent<FPVFlashlightHandsController>();
            handsController.Initialize(
                animator: animator,
                sfxAudio: sfxAudio,
                humAudio: humAudio,
                spot: spot,
                fill: fill,
                bulbRenderer: bulbRend,
                bulbMat: bulbMat,
                armRootTransform: armRoot.transform,
                flashlightRoot: flashlightGo.transform);

            // Also keep FlashlightController attached for legacy interfaces and inspector queries
            var legacyController = flashlightGo.AddComponent<FlashlightController>();
            legacyController.SetupLightsAndAudio();
            legacyController.AssignModelReferences(model.transform, bulbRend, bulbMat);

            return rootGo;
        }

        private static Material LoadMaterial(string path, Color fallbackColor)
        {
#if UNITY_EDITOR
            var mat = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat) return mat;
#endif
            var fallback = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            fallback.color = fallbackColor;
            return fallback;
        }
    }
}
