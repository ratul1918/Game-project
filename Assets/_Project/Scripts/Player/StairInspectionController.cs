using UnityEngine;
using UnityEngine.InputSystem;

namespace TheLightHouse
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class StairInspectionController : MonoBehaviour
    {
        [SerializeField] private PlayerMovementSettings settings;
        [SerializeField] private float orbitDistance = 2.4f;
        [SerializeField] private float orbitHeight = 1.5f;
        [SerializeField] private float cameraProbeRadius = .12f;
        private CharacterController capsule;
        private Camera view;
        private float yaw, pitch = 15f, vertical;
        public bool Automated { get; set; }
        public bool Grounded { get; private set; }
        public CharacterController Capsule => capsule;

        private void Awake()
        {
            settings = Resources.Load<PlayerMovementSettings>("PlayerMovementSettings");
            capsule = GetComponent<CharacterController>();
            capsule.height = settings.standingHeight;
            capsule.radius = settings.capsuleRadius;
            capsule.center = Vector3.up * capsule.height * .5f;
            capsule.skinWidth = settings.skinWidth;
            capsule.slopeLimit = settings.slopeLimit;
            capsule.stepOffset = settings.stepOffset;
            capsule.minMoveDistance = 0f;
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Third Person Inspection Body";
            body.transform.SetParent(transform, false);
            body.transform.localPosition = capsule.center;
            body.transform.localScale = new Vector3(capsule.radius * 2f, capsule.height * .5f, capsule.radius * 2f);
            Destroy(body.GetComponent<Collider>());
            view = new GameObject("Third Person Inspection Camera").AddComponent<Camera>();
            view.nearClipPlane = .05f;
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update()
        {
            if (Automated) return;
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            if (mouse != null)
            {
                var delta = mouse.delta.ReadValue() * settings.lookSensitivity;
                yaw += delta.x;
                pitch = Mathf.Clamp(pitch - delta.y, -25f, 65f);
            }
            Vector3 input = keyboard == null ? Vector3.zero : new Vector3(
                (keyboard.dKey.isPressed ? 1 : 0) - (keyboard.aKey.isPressed ? 1 : 0), 0,
                (keyboard.wKey.isPressed ? 1 : 0) - (keyboard.sKey.isPressed ? 1 : 0));
            Simulate(Quaternion.Euler(0, yaw, 0) * Vector3.ClampMagnitude(input, 1) * settings.walkSpeed, Time.deltaTime);
        }

        public void Simulate(Vector3 velocity, float dt)
        {
            bool before = capsule.isGrounded || ControllerGrounding.Probe(capsule, settings, out _);
            if (before) vertical = settings.groundedVelocity;
            vertical = Mathf.Max(vertical + Physics.gravity.y * settings.gravityMultiplier * dt, -settings.terminalVelocity);
            capsule.stepOffset = before ? settings.stepOffset : 0f;
            var flags = capsule.Move((velocity + Vector3.up * vertical) * dt);
            if (before && (flags & CollisionFlags.Below) == 0) flags |= ControllerGrounding.Snap(capsule, settings);
            Grounded = (flags & CollisionFlags.Below) != 0 || ControllerGrounding.Probe(capsule, settings, out _);
            if (velocity.sqrMagnitude > .01f) transform.rotation = Quaternion.LookRotation(velocity);
        }

        private void LateUpdate()
        {
            if (!view) return;
            if (Automated) yaw = transform.eulerAngles.y;
            Vector3 pivot = transform.position + Vector3.up * orbitHeight;
            Vector3 offset = Quaternion.Euler(pitch, yaw, 0) * Vector3.back * orbitDistance;
            float distance = orbitDistance;
            foreach (var hit in Physics.SphereCastAll(pivot, cameraProbeRadius, offset.normalized, orbitDistance,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                if (hit.collider != capsule && !hit.transform.IsChildOf(transform)) distance = Mathf.Min(distance, hit.distance);
            view.transform.position = pivot + offset.normalized * Mathf.Max(cameraProbeRadius, distance);
            view.transform.LookAt(pivot);
        }

        private void OnDestroy() { if (view) Destroy(view.gameObject); }
    }
}
