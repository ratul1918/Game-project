using UnityEngine;

namespace TheLightHouse
{
    public sealed class NoiseEmitter : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float defaultLoudness = 6f;

        public void Emit()
        {
            Emit(defaultLoudness);
        }

        public void Emit(float loudness)
        {
            EmitAt(transform.position, loudness);
        }

        public void EmitAt(Vector3 position, float loudness)
        {
            float clampedLoudness = Mathf.Max(0f, loudness);
            GameEvents.RaiseNoiseEmitted(
                new NoiseEmittedEvent(position, clampedLoudness, gameObject));
        }
    }
}
