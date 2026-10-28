using UnityEngine;

namespace TheLightHouse
{
    public sealed class HeadBob : MonoBehaviour
    {
        float phase;
        public float Offset { get; private set; }
        public bool Tick(bool moving, float speed, PlayerMovementSettings settings, float deltaTime)
        {
            if (!moving) { Offset = 0f; phase = 0f; return false; }
            float previous = phase;
            phase += deltaTime * settings.bobFrequency * speed / Mathf.Max(settings.walkSpeed, settings.movementThreshold);
            // Half-cycle crossings drive footsteps even when visual bob is disabled.
            bool step = Mathf.FloorToInt(phase / Mathf.PI) > Mathf.FloorToInt(previous / Mathf.PI);
            Offset = settings.disableBobAndFov ? 0f : Mathf.Sin(phase) * settings.bobAmplitude;
            return step;
        }
        public void ResetPhase() { phase = 0f; Offset = 0f; }
    }
}
