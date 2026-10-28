using UnityEngine;

namespace TheLightHouse
{
    [RequireComponent(typeof(NoiseEmitter), typeof(FootstepSurface))]
    public sealed class FootstepSystem : MonoBehaviour
    {
        NoiseEmitter emitter;
        FootstepSurface surface;
        public void Initialize() { emitter = GetComponent<NoiseEmitter>(); surface = GetComponent<FootstepSurface>(); }
        public void Step(bool crouching, bool sprinting, PlayerMovementSettings settings)
            => Emit(crouching ? settings.crouchLoudness : sprinting ? settings.sprintLoudness : settings.walkLoudness);
        public void Takeoff(PlayerMovementSettings settings) => Emit(settings.jumpLoudness);
        public void Land(float fallSpeed, PlayerMovementSettings settings)
        {
            if (fallSpeed < settings.minimumLandingSpeed) return;
            Emit(Mathf.Min(settings.maximumLandingLoudness,
                settings.landingLoudness * fallSpeed / settings.referenceLandingSpeed));
        }
        void Emit(float baseLoudness) => emitter.Emit(baseLoudness * surface.Multiplier);
    }
}
