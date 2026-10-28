using UnityEngine;

namespace TheLightHouse
{
    public sealed class FootstepSurface : MonoBehaviour
    {
        public string SurfaceName { get; private set; } = "Default";
        public float Multiplier { get; private set; } = 1f;
        public AudioClip[] Clips { get; private set; } = System.Array.Empty<AudioClip>();
        public void Detect(RaycastHit hit, PlayerMovementSettings settings)
        {
            SurfaceName = "Default";
            Multiplier = 1f;
            Clips = System.Array.Empty<AudioClip>();
            if (!hit.collider) return;
            foreach (var surface in settings.surfaces)
            {
                if (surface == null || hit.collider.tag != surface.tag) continue;
                SurfaceName = surface.tag;
                Multiplier = surface.multiplier;
                Clips = surface.clips;
                return;
            }
        }
    }
}
