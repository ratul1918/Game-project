using UnityEngine;
namespace TheLightHouse
{
    [DefaultExecutionOrder(200)]
    public sealed class FlashlightLensAim : MonoBehaviour
    {
        [SerializeField] Transform view;
        [SerializeField] Transform lens;
        [SerializeField] Light beam;
        [SerializeField, Min(1f)] float aimDistance = 30f;
        public void Configure(Transform cameraTransform, Transform glass, Light light)
        { view = cameraTransform; lens = glass; beam = light; }
        void LateUpdate()
        {
            if (!view || !lens || !beam) return;
            var target = view.position + view.forward * aimDistance;
            foreach (var hit in Physics.RaycastAll(view.position, view.forward, aimDistance, ~0, QueryTriggerInteraction.Ignore))
            {
                if (hit.transform.IsChildOf(view.root)) continue;
                if ((hit.point - view.position).sqrMagnitude < (target - view.position).sqrMagnitude)
                    target = hit.point;
            }
            beam.transform.position = lens.position + lens.up * .002f;
            beam.transform.rotation = Quaternion.LookRotation(target - beam.transform.position, view.up);
        }
    }
}
