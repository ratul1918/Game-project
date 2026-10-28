using UnityEngine;

namespace TheLightHouse
{
    public sealed class Lean : MonoBehaviour
    {
        public float Amount { get; private set; }
        public void Tick(float input, Transform view, Vector3 neutralPosition, PlayerMovementSettings settings, float deltaTime)
        {
            Vector3 direction = view.right * Mathf.Sign(input);
            float distance = settings.leanDistance * Mathf.Abs(input);
            if (distance > 0f && Physics.SphereCast(neutralPosition, settings.leanProbeRadius, direction,
                out RaycastHit hit, distance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                distance = Mathf.Max(0f, hit.distance - settings.ceilingClearance);
            float target = settings.leanDistance > 0f ? Mathf.Sign(input) * distance / settings.leanDistance : 0f;
            Amount = Mathf.Lerp(Amount, target, 1f - Mathf.Exp(-settings.cameraSmoothing * deltaTime));
            if (Mathf.Sign(Amount) == Mathf.Sign(target) && Mathf.Abs(Amount) > Mathf.Abs(target))
                Amount = target;
        }
    }
}
