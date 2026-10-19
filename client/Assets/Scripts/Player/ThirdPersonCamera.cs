using UnityEngine;
using UnityEngine.InputSystem;
using EmberBlade.Config;

namespace EmberBlade.Player
{
    public class ThirdPersonCamera : MonoBehaviour
    {
        [SerializeField] private GameConfig config;
        [SerializeField] private Transform target;

        private float yaw;
        private float pitch = 18f;

        public Vector3 PlanarForward
        {
            get
            {
                Vector3 forward = transform.forward;
                forward.y = 0f;
                return forward.sqrMagnitude > 0.001f ? forward.normalized : Vector3.forward;
            }
        }

        public Vector3 PlanarRight => Vector3.Cross(Vector3.up, PlanarForward);

        public void Initialize(GameConfig gameConfig, Transform cameraTarget)
        {
            config = gameConfig;
            target = cameraTarget;
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            {
                Vector2 delta = Mouse.current.delta.ReadValue();
                float sensitivity = config != null ? config.cameraLookSensitivity : 0.12f;
                yaw += delta.x * sensitivity;
                pitch = Mathf.Clamp(pitch - delta.y * sensitivity, -10f, 65f);
            }

            float distance = config != null ? config.cameraDistance : 6f;
            float height = config != null ? config.cameraHeight : 2.5f;
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 focus = target.position + Vector3.up * height;
            transform.position = focus - rotation * Vector3.forward * distance;
            transform.rotation = rotation;
        }
    }
}
