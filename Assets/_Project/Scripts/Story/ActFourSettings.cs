using UnityEngine;

namespace TheLightHouse
{
    [CreateAssetMenu(menuName = "The LightHouse/Act Four Settings")]
    public sealed class ActFourSettings : ScriptableObject
    {
        [Header("Reading grace")]
        [Range(0.05f, 1f)] public float readingThreatSpeed = 0.35f;
        [Range(0.05f, 1f)] public float readingLampDecayFactor = 0.3f;
        [Range(0f, 1f)] public float pageChargeTopUp = 0.08f;

        [Header("Discovery")]
        [Min(1)] public int pagesForCellar = 2;
        [Min(0f)] public float shelfNoise = 10f;
        [Min(0.1f)] public float shelfTravel = 1.8f;
        [Min(0.1f)] public float shelfSpeed = 1.2f;
        [Min(0.1f)] public float signalRange = 45f;
        [Range(0f, 1f)] public float tuningTolerance = 0.12f;
        [Min(0.01f)] public float tuningSpeed = 0.35f;
        [Min(0.01f)] public float tuningWheelStep = 0.06f;

        [Header("Crane after the reveal")]
        [Range(0f, 1f)] public float postRevealHerdingBias = 0.35f;
        [Min(0.5f)] public float postRevealStopDistance = 2.2f;
        [Min(0.1f)] public float postRevealPause = 2.5f;
        [Min(0.1f)] public float lullabyRange = 9f;
        [Range(0f, 1f)] public float lullabyVolume = 0.45f;
    }
}
