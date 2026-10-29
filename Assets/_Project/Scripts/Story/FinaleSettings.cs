using UnityEngine;

namespace TheLightHouse
{
    [CreateAssetMenu(menuName="The LightHouse/Finale Settings")]
    public sealed class FinaleSettings : ScriptableObject
    {
        [Header("Pacing")]
        [Min(.1f)] public float floodDelay=2f;
        [Min(1f)] public float holdDuration=75f;
        [Min(.1f)] public float interventionDelay=4f;
        [Min(1f)] public float dawnDuration=45f;
        [Header("Lamp")]
        [Range(0f,1f)] public float initialCharge=.18f;
        [Min(1f)] public float chargeDecayFactor=1.8f;
        [Min(.1f)] public float extinguishedGrace=5f;
        [Min(0f)] public float pulseStaminaCost=18f;
        [Min(1)] public int figuresAtDoorForFailure=2;
        [Header("Drowned")]
        [Min(1)] public int figureCount=14;
        [Min(1f)] public float maximumPressureMultiplier=3f;
        [Min(.1f)] public float lureStepDistance=2f;
        [Min(.1f)] public float returnDuration=6f;
        [Min(.1f)] public float sinkDepth=2.5f;
        [Header("Crane")]
        [Min(.1f)] public float slamInterval=8f;
        [Min(0f)] public float slamLoudness=28f;
        [Min(.1f)] public float lureRadius=12f;
        [Min(.1f)] public float craneSpeed=4f;
        [Min(.1f)] public float controlStutter=1.2f;
        [Min(.1f)] public float nearHumRange=6f;
        [Min(.1f)] public float farHumRange=18f;
        [Header("Dawn visuals")]
        [Min(0f)] public float dawnLightIntensity=1.25f;
        [Range(0f,1f)] public float finalFogDensity=.005f;
        [Min(.1f)] public float doorwayBlockDuration=4f;
        [Min(.1f)] public float shorelineSequenceDuration=9f;
        [Min(.1f)] public float boatTravelDuration=8f;
        [Range(1f,90f)] public float shorelineLookYaw=30f;
        [Range(1f,60f)] public float shorelineLookPitch=15f;
        [Min(0f)] public float finalBeamSweepDegrees=360f;
    }
}
