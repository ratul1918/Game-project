using UnityEngine;

namespace TheLightHouse
{
    [DefaultExecutionOrder(150)]
    public sealed class TorchViewmodel : MonoBehaviour
    {
        [SerializeField] private Vector3 gripPosition = new Vector3(.22f, -.19f, .43f);
        [SerializeField] private Vector3 gripAngles = new Vector3(-8f, -12f, -12f);
        [SerializeField] private Vector3 pocketOffset = new Vector3(.08f, -.38f, -.12f);
        [SerializeField] private Vector3 pocketAngles = new Vector3(45f, 10f, -25f);
        [SerializeField] private float followSpeed = 14f;
        [SerializeField] private float swayScale = .025f;
        [SerializeField] private float swayLimit = 3f;
        [SerializeField] private float bobAmount = .0025f;
        [SerializeField] private float bobSpeed = 2f;
        [SerializeField] private float clearance = .65f;
        [SerializeField] private float probeRadius = .16f;
        [SerializeField, Range(0f, 1f)] private float concealAtObstruction = .6f;
        [SerializeField] private LayerMask obstructionMask = ~0;
        private Transform _view;
        private Transform _grip;
        private Quaternion _lastRotation;
        private Vector2 _sway;
        private float _obstruction;
        private float _phase;
        public float DrawAmount { get; set; } = 1f;

        public void Initialize(Transform oldArm)
        {
            _view = GetComponentInParent<Camera>().transform;
            _lastRotation = _view.rotation;
            var model = GetComponentInChildren<FlashlightController>(true);
            if (!model || !oldArm) return;
            var torch = model.transform;
            var skin = FindMaterial(oldArm, "Hand_R_Palm_Mesh");
            var wool = FindMaterial(oldArm, "Heavy_Mariner_Wool_Sleeve");
            var cuff = FindMaterial(oldArm, "Oilskin_Jacket_Cuff");
            _grip = new GameObject("RightHand_Torch_Grip").transform;
            _grip.SetParent(_view, false);
            torch.SetParent(_grip, false);
            torch.localPosition = Vector3.zero;
            torch.localRotation = Quaternion.identity;
            torch.localScale = Vector3.one;
            oldArm.gameObject.SetActive(false);
            TorchGripMesh.Build(_grip, skin, wool, cuff);
            var mesh = torch.Find("Flashlight_1940s_Ribbed_Mesh");
            if (mesh)
            {
                mesh.localPosition = Vector3.zero;
                mesh.localRotation = Quaternion.identity;
                mesh.localScale = Vector3.one;
            }
            var socket = torch.Find("Flashlight_Lens_Socket");
            if (!socket)
            {
                var socketGo = new GameObject("Flashlight_Lens_Socket");
                socketGo.transform.SetParent(torch, false);
                socket = socketGo.transform;
            }
            socket.SetParent(torch, false);
            socket.localPosition = new Vector3(0f, 0f, 0.075f);
            socket.localRotation = Quaternion.identity;

            var lights = torch.GetComponentsInChildren<Light>(true);
            var spot = System.Array.Find(lights, l => l.type == LightType.Spot);
            var fill = System.Array.Find(lights, l => l.type == LightType.Point);
            if (spot)
            {
                spot.transform.SetParent(socket, false);
                spot.transform.localPosition = Vector3.zero;
                spot.transform.localRotation = Quaternion.identity;
                spot.shadowNearPlane = 0.05f;
                spot.renderingLayerMask = 1;
            }
            if (fill)
            {
                fill.transform.SetParent(socket, false);
                fill.transform.localPosition = Vector3.zero;
                fill.transform.localRotation = Quaternion.identity;
                fill.intensity = 0f;
                fill.enabled = false;
            }

            int vmLayer = LayerMask.NameToLayer("Viewmodel");
            if (vmLayer < 0) vmLayer = 2;
            ConfigureViewmodelHierarchy(_grip, vmLayer, 2u);
            if (spot) spot.cullingMask &= ~(1 << vmLayer);
            if (fill) fill.cullingMask &= ~(1 << vmLayer);

            foreach (var aim in GetComponentsInChildren<FlashlightLensAim>(true))
                aim.enabled = false;
            foreach (var aim in torch.GetComponentsInParent<FlashlightLensAim>(true))
                aim.enabled = false;
            // Keep the compatibility controller under its owning state component.
            _grip.SetParent(transform, true);
        }

        private static void ConfigureViewmodelHierarchy(Transform root, int layer, uint renderingLayerMask)
        {
            if (!root) return;
            root.gameObject.layer = layer;
            var rend = root.GetComponent<Renderer>();
            if (rend)
            {
                rend.renderingLayerMask = renderingLayerMask;
            }
            for (int i = 0; i < root.childCount; i++)
            {
                ConfigureViewmodelHierarchy(root.GetChild(i), layer, renderingLayerMask);
            }
        }

        private static Material FindMaterial(Transform root, string name)
        {
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
                if (renderer.name == name) return renderer.sharedMaterial;
            return null;
        }

        private void LateUpdate()
        {
            if (!_grip || !_view) return;
            float dt = Time.deltaTime;
            var delta = Quaternion.Inverse(_lastRotation) * _view.rotation;
            _lastRotation = _view.rotation;
            var target = new Vector2(Mathf.DeltaAngle(0f, delta.eulerAngles.x),
                -Mathf.DeltaAngle(0f, delta.eulerAngles.y)) * (swayScale / Mathf.Max(dt, .001f));
            target = Vector2.ClampMagnitude(target, swayLimit);
            _sway = Vector2.Lerp(_sway, target, 1f - Mathf.Exp(-followSpeed * dt));
            float nearest = clearance;
            foreach (var hit in Physics.SphereCastAll(_view.position, probeRadius, _view.forward,
                         clearance, obstructionMask, QueryTriggerInteraction.Ignore))
                if (!hit.transform.IsChildOf(_view.root)) nearest = Mathf.Min(nearest, hit.distance);
            float blocked = 1f - Mathf.Clamp01(nearest / clearance);
            // Retract immediately on contact; recover gently when stepping away.
            _obstruction = blocked > _obstruction ? blocked : Mathf.Lerp(_obstruction, blocked, 1f - Mathf.Exp(-followSpeed * dt));
            float lowered = Mathf.Max(1f - DrawAmount, _obstruction);
            _phase += dt * bobSpeed;
            var position = gripPosition + pocketOffset * lowered;
            position.y += Mathf.Sin(_phase) * bobAmount * (1f - lowered);
            _grip.position = _view.TransformPoint(position);
            _grip.rotation = _view.rotation * Quaternion.Euler(gripAngles + pocketAngles * lowered
                + new Vector3(_sway.x, _sway.y, 0f));
            _grip.gameObject.SetActive(DrawAmount > .001f && _obstruction < concealAtObstruction && isActiveAndEnabled);
        }

        private void OnDisable() { if (_grip) _grip.gameObject.SetActive(false); }
    }
}
