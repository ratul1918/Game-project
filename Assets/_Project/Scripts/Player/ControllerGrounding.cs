using UnityEngine;

namespace TheLightHouse
{
    public static class ControllerGrounding
    {
        public static bool Probe(CharacterController controller, PlayerMovementSettings settings, out RaycastHit hit)
        {
            float radius = Mathf.Max(settings.groundProbeInset, controller.radius - settings.groundProbeInset);
            Vector3 feet = controller.transform.TransformPoint(controller.center) - Vector3.up * controller.height * .5f;
            Vector3 origin = feet + Vector3.up * (controller.radius + settings.groundProbeInset);
            hit = default;
            float nearest = float.PositiveInfinity;
            foreach (var candidate in Physics.SphereCastAll(origin, radius, Vector3.down,
                settings.groundProbeDistance + settings.groundProbeInset * 2f,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                if (candidate.collider == controller || candidate.transform.IsChildOf(controller.transform)
                    || Vector3.Angle(candidate.normal, Vector3.up) > settings.slopeLimit || candidate.distance >= nearest) continue;
                nearest = candidate.distance;
                hit = candidate;
            }
            return hit.collider;
        }

        public static CollisionFlags Snap(CharacterController controller, PlayerMovementSettings settings)
        {
            if (!Probe(controller, settings, out var hit)) return CollisionFlags.None;
            float feetY = controller.transform.TransformPoint(controller.center).y - controller.height * .5f;
            float drop = feetY - hit.point.y;
            return drop > 0f && drop <= settings.groundProbeDistance
                ? controller.Move(Vector3.down * (drop + settings.skinWidth)) : CollisionFlags.None;
        }
    }
}
