using UnityEngine;

namespace TheLightHouse
{
    public enum ArmState
    {
        Empty,
        HoldingAxe,
        HoldingLantern,
        SwingingLight,
        SwingingHeavy,
        Blocking,
        Shoving,
        Interacting
    }

    /// <summary>
    /// Renders and animates Daniel Hale's first-person arms, sleeves, and weapon rig.
    /// Provides procedural look sway, movement bob, and combat swing animations.
    /// </summary>
    public sealed class FirstPersonArms : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private Material coatSleeveMaterial;
        [SerializeField] private Material skinMaterial;

        [Header("Procedural Sway & Lag")]
        [SerializeField] private float swayAmount = 1.8f;
        [SerializeField] private float maxSway = 4.5f;
        [SerializeField] private float swaySmooth = 7f;

        [Header("Bobbing")]
        [SerializeField] private float bobFrequency = 7f;
        [SerializeField] private float bobAmount = 0.025f;

        [Header("Sockets")]
        [SerializeField] private Transform rightHandSocket;
        [SerializeField] private Transform leftHandSocket;

        private GameObject armsRoot;
        private Transform rightArmTransform;
        private Transform leftArmTransform;
        private ArmState currentState = ArmState.HoldingAxe;
        private Vector3 defaultArmPosition;
        private Quaternion defaultArmRotation;
        private Vector3 targetLocalPosition;
        private Quaternion targetLocalRotation;
        private float bobTimer;

        public Transform RightHandSocket => rightHandSocket;
        public Transform LeftHandSocket => leftHandSocket;
        public ArmState CurrentState => currentState;

        private void Awake()
        {
            if (!cameraTransform && Camera.main)
                cameraTransform = Camera.main.transform;

            ConstructArmsRig();
        }

        private void ConstructArmsRig()
        {
            if (armsRoot) return;

            armsRoot = new GameObject("FirstPerson_ArmsRig");
            armsRoot.transform.SetParent(cameraTransform ? cameraTransform : transform, false);
            armsRoot.transform.localPosition = new Vector3(0f, -0.38f, 0.45f);
            armsRoot.transform.localRotation = Quaternion.identity;

            defaultArmPosition = armsRoot.transform.localPosition;
            defaultArmRotation = armsRoot.transform.localRotation;
            targetLocalPosition = defaultArmPosition;
            targetLocalRotation = defaultArmRotation;

            // Load or create dark wool coat material
            if (!coatSleeveMaterial)
            {
                var refMat = Resources.Load<Material>("Materials/Reference/Wet Shale");
                coatSleeveMaterial = refMat ? new Material(refMat) : new Material(Shader.Find("Universal Render Pipeline/Lit"));
                coatSleeveMaterial.color = new Color(0.12f, 0.16f, 0.22f); // Dark naval wool coat
            }

            if (!skinMaterial)
            {
                skinMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                skinMaterial.color = new Color(0.68f, 0.52f, 0.45f); // Weathered sailor skin
            }

            // Right Arm (Forearm & Coat Sleeve)
            var rightArmGo = new GameObject("Right_Forearm");
            rightArmGo.transform.SetParent(armsRoot.transform, false);
            rightArmGo.transform.localPosition = new Vector3(0.24f, -0.08f, 0.05f);
            rightArmGo.transform.localRotation = Quaternion.Euler(68f, -22f, 15f);
            rightArmTransform = rightArmGo.transform;

            var rSleeve = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rSleeve.name = "Right_CoatSleeve";
            rSleeve.transform.SetParent(rightArmTransform, false);
            rSleeve.transform.localScale = new Vector3(0.085f, 0.22f, 0.085f);
            DestroyImmediate(rSleeve.GetComponent<Collider>());
            rSleeve.GetComponent<Renderer>().sharedMaterial = coatSleeveMaterial;

            // Right Hand
            var rHand = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rHand.name = "Right_Hand";
            rHand.transform.SetParent(rightArmTransform, false);
            rHand.transform.localPosition = new Vector3(0f, 0.24f, 0.01f);
            rHand.transform.localScale = new Vector3(0.075f, 0.09f, 0.055f);
            DestroyImmediate(rHand.GetComponent<Collider>());
            rHand.GetComponent<Renderer>().sharedMaterial = skinMaterial;

            var rSocketGo = new GameObject("RightHand_Socket");
            rSocketGo.transform.SetParent(rHand.transform, false);
            rSocketGo.transform.localPosition = new Vector3(0f, 0.02f, 0.05f);
            rSocketGo.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            rightHandSocket = rSocketGo.transform;

            // Left Arm (Forearm & Coat Sleeve)
            var leftArmGo = new GameObject("Left_Forearm");
            leftArmGo.transform.SetParent(armsRoot.transform, false);
            leftArmGo.transform.localPosition = new Vector3(-0.24f, -0.12f, 0.02f);
            leftArmGo.transform.localRotation = Quaternion.Euler(62f, 25f, -12f);
            leftArmTransform = leftArmGo.transform;

            var lSleeve = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            lSleeve.name = "Left_CoatSleeve";
            lSleeve.transform.SetParent(leftArmTransform, false);
            lSleeve.transform.localScale = new Vector3(0.085f, 0.22f, 0.085f);
            DestroyImmediate(lSleeve.GetComponent<Collider>());
            lSleeve.GetComponent<Renderer>().sharedMaterial = coatSleeveMaterial;

            // Left Hand
            var lHand = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lHand.name = "Left_Hand";
            lHand.transform.SetParent(leftArmTransform, false);
            lHand.transform.localPosition = new Vector3(0f, 0.24f, 0.01f);
            lHand.transform.localScale = new Vector3(0.075f, 0.09f, 0.055f);
            DestroyImmediate(lHand.GetComponent<Collider>());
            lHand.GetComponent<Renderer>().sharedMaterial = skinMaterial;

            var lSocketGo = new GameObject("LeftHand_Socket");
            lSocketGo.transform.SetParent(lHand.transform, false);
            lSocketGo.transform.localPosition = new Vector3(0f, 0.02f, 0.05f);
            leftHandSocket = lSocketGo.transform;
        }

        public void SetState(ArmState state)
        {
            currentState = state;
        }

        public void AnimateLightSwing(float progress)
        {
            // progress 0 to 1
            float curve = Mathf.Sin(progress * Mathf.PI);
            targetLocalPosition = defaultArmPosition + new Vector3(-0.15f * curve, -0.08f * curve, 0.12f * curve);
            targetLocalRotation = defaultArmRotation * Quaternion.Euler(25f * curve, -35f * curve, -20f * curve);
        }

        public void AnimateHeavySwing(float progress)
        {
            float curve = Mathf.Sin(progress * Mathf.PI);
            targetLocalPosition = defaultArmPosition + new Vector3(0.05f * curve, 0.18f * curve, 0.15f * curve);
            targetLocalRotation = defaultArmRotation * Quaternion.Euler(-45f * curve, 10f * curve, 15f * curve);
        }

        public void AnimateBlock(bool active)
        {
            if (active)
            {
                targetLocalPosition = defaultArmPosition + new Vector3(-0.06f, 0.12f, -0.05f);
                targetLocalRotation = defaultArmRotation * Quaternion.Euler(15f, 25f, -30f);
            }
            else
            {
                targetLocalPosition = defaultArmPosition;
                targetLocalRotation = defaultArmRotation;
            }
        }

        public void AnimateShove(float progress)
        {
            float curve = Mathf.Sin(progress * Mathf.PI);
            targetLocalPosition = defaultArmPosition + new Vector3(0f, 0.05f * curve, 0.25f * curve);
            targetLocalRotation = defaultArmRotation * Quaternion.Euler(10f * curve, 0f, 0f);
        }

        public void UpdateMotion(Vector2 lookDelta, float moveSpeed, float dt)
        {
            if (!armsRoot) return;

            // 1. Procedural Look Sway
            float swayX = Mathf.Clamp(-lookDelta.x * swayAmount * 0.05f, -maxSway, maxSway);
            float swayY = Mathf.Clamp(-lookDelta.y * swayAmount * 0.05f, -maxSway, maxSway);
            Quaternion swayRotation = Quaternion.Euler(swayY, swayX, -swayX * 0.5f);

            // 2. Procedural Walking/Running Bob
            Vector3 bobOffset = Vector3.zero;
            if (moveSpeed > 0.1f)
            {
                bobTimer += dt * bobFrequency * (moveSpeed > 4.5f ? 1.4f : 1f);
                bobOffset.x = Mathf.Cos(bobTimer * 0.5f) * bobAmount * 0.5f;
                bobOffset.y = Mathf.Sin(bobTimer) * bobAmount;
            }
            else
            {
                // Subtle breathing idle
                bobTimer += dt * 1.5f;
                bobOffset.y = Mathf.Sin(bobTimer) * 0.006f;
            }

            Vector3 finalPos = targetLocalPosition + bobOffset;
            Quaternion finalRot = targetLocalRotation * swayRotation;

            armsRoot.transform.localPosition = Vector3.Lerp(armsRoot.transform.localPosition, finalPos, dt * swaySmooth);
            armsRoot.transform.localRotation = Quaternion.Slerp(armsRoot.transform.localRotation, finalRot, dt * swaySmooth);
        }
    }
}
