using UnityEngine;

namespace TheLightHouse
{
    public sealed class FallDamageSafety : MonoBehaviour
    {
        Vector3 lastSafe;
        float groundedTime;
        public void Initialize() => lastSafe = transform.position;
        public bool Tick(CharacterController controller, bool grounded, PlayerMovementSettings settings, float deltaTime)
        {
            groundedTime = grounded ? groundedTime + deltaTime : 0f;
            if (groundedTime >= settings.safeGroundSeconds) lastSafe = transform.position;
            if (transform.position.y >= settings.killPlaneY) return false;
            controller.enabled = false;
            transform.position = lastSafe;
            controller.enabled = true;
            groundedTime = 0f;
            return true;
        }
    }
}
