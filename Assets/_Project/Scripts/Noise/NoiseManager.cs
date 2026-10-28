using System.Collections.Generic;
using UnityEngine;

namespace TheLightHouse
{
    public sealed class NoiseManager : MonoBehaviour
    {
        private readonly struct RecentNoise
        {
            public readonly Vector3 Position;
            public readonly float Loudness;
            public readonly float ExpiresAt;

            public RecentNoise(Vector3 position, float loudness, float expiresAt)
            {
                Position = position;
                Loudness = loudness;
                ExpiresAt = expiresAt;
            }
        }

        [SerializeField, Min(0.1f)] private float gizmoDuration = 2f;
        [SerializeField] private Color gizmoColor = new(1f, 0.55f, 0.1f, 0.8f);

        private readonly List<RecentNoise> recentNoises = new();

        private void OnEnable()
        {
            GameEvents.NoiseEmitted += HandleNoiseEmitted;
        }

        private void OnDisable()
        {
            GameEvents.NoiseEmitted -= HandleNoiseEmitted;
            recentNoises.Clear();
        }

        private void Update()
        {
            float now = Time.time;
            recentNoises.RemoveAll(noise => noise.ExpiresAt <= now);
        }

        private void HandleNoiseEmitted(NoiseEmittedEvent eventData)
        {
            recentNoises.Add(
                new RecentNoise(eventData.Position, eventData.Loudness, Time.time + gizmoDuration));
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            Gizmos.color = gizmoColor;
            float now = Time.time;
            foreach (RecentNoise noise in recentNoises)
            {
                if (noise.ExpiresAt > now)
                {
                    Gizmos.DrawWireSphere(noise.Position, noise.Loudness);
                }
            }
        }
    }
}
