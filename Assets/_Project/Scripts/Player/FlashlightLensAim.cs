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
            if (!lens || !beam) return;
            beam.transform.position = lens.position + lens.up * .003f;
            beam.transform.rotation = lens.parent ? lens.parent.rotation : transform.rotation;
        }
    }
}
