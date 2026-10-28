using System;
using UnityEngine;

namespace TheLightHouse
{
    [CreateAssetMenu(menuName = "TheLightHouse/Player Movement Settings")]
    public sealed class PlayerMovementSettings : ScriptableObject
    {
        [Header("Locomotion")]
        [Min(0)] public float walkSpeed = 4f;
        [Min(0)] public float sprintSpeed = 7f;
        [Min(0)] public float crouchSpeed = 2f;
        [Min(0)] public float acceleration = 22f;
        [Min(0)] public float deceleration = 28f;
        [Range(0, 1)] public float airControl = .15f;
        [Min(.01f)] public float movementThreshold = .05f;
        [Header("Jump and grounding")]
        [Min(.01f)] public float jumpHeight = 1.2f;
        [Min(.01f)] public float gravityMultiplier = 2.5f;
        [Min(.01f)] public float terminalVelocity = 40f;
        [Min(0)] public float coyoteTime = .12f;
        [Min(0)] public float jumpBuffer = .15f;
        public float groundedVelocity = -2f;
        [Range(0, 89)] public float slopeLimit = 45f;
        [Min(0)] public float stepOffset = .3f;
        [Min(.001f)] public float skinWidth = .04f;
        [Min(.001f)] public float groundProbeDistance = .12f;
        [Min(.001f)] public float groundProbeInset = .04f;
        [Header("Crouch")]
        [Min(.1f)] public float standingHeight = 1.8f;
        [Min(.1f)] public float crouchingHeight = 1.1f;
        [Min(.01f)] public float crouchTransitionSpeed = 4f;
        [Min(0)] public float standingCameraHeight = 1.65f;
        [Min(0)] public float crouchingCameraHeight = .95f;
        [Min(.001f)] public float ceilingClearance = .03f;
        [Header("Look")]
        public float lookSensitivity = .08f;
        public float minimumPitch = -85f;
        public float maximumPitch = 85f;
        [Min(.01f)] public float cameraNearClip = .05f;
        [Header("Stamina")]
        [Min(.01f)] public float staminaCapacity = 100f;
        [Min(0)] public float sprintDrain = 22f;
        [Min(0)] public float jumpCost = 18f;
        [Min(0)] public float staminaRegeneration = 16f;
        [Min(0)] public float exhaustionLockout = 1.5f;
        [Range(0, 1)] public float lowStaminaThreshold = .25f;
        [Header("Noise")]
        [Min(0)] public float crouchLoudness = 2f;
        [Min(0)] public float walkLoudness = 6f;
        [Min(0)] public float sprintLoudness = 12f;
        [Min(0)] public float jumpLoudness = 3f;
        [Min(0)] public float landingLoudness = 10f;
        [Min(0)] public float minimumLandingSpeed = 3f;
        [Min(.01f)] public float referenceLandingSpeed = 6f;
        [Min(0)] public float maximumLandingLoudness = 30f;
        [Min(0)] public float hardLandingSpeed = 10f;
        [Header("Camera accessibility")]
        public bool disableBobAndFov;
        [Min(0)] public float bobAmplitude = .035f;
        [Min(.01f)] public float bobFrequency = 10f;
        [Min(.01f)] public float cameraSmoothing = 12f;
        public float baseFov = 65f;
        public float sprintFovIncrease = 4f;
        [Min(0)] public float landingDip = .08f;
        [Min(0)] public float hardLandingShake = .025f;
        [Min(0)] public float breathingAmplitude = .008f;
        [Min(0)] public float breathingFrequency = 1.8f;
        [Header("Lean")]
        [Min(0)] public float leanDistance = .25f;
        [Min(0)] public float leanAngle = 10f;
        [Min(.01f)] public float leanProbeRadius = .12f;
        [Header("Safety")]
        public float killPlaneY = -20f;
        [Min(.01f)] public float safeGroundSeconds = .5f;
        [Header("Debug and stamina UI")]
        public Rect debugPanelRect = new(20f, 150f, 340f, 180f);
        [Range(.01f, 1f)] public float staminaBarWidth = .15f;
        [Range(0f, 1f)] public float staminaBarY = .92f;
        [Min(1f)] public float staminaBarHeight = 8f;
        public SurfaceProfile[] surfaces =
        {
            new("Wood", 1f), new("Stone", 1.2f), new("Metal", 1.5f),
            new("Water", 1.3f), new("Sand", .6f)
        };
    }

    [Serializable]
    public sealed class SurfaceProfile
    {
        public string tag;
        [Min(0)] public float multiplier;
        public AudioClip[] clips = Array.Empty<AudioClip>();
        public SurfaceProfile(string tag, float multiplier) { this.tag = tag; this.multiplier = multiplier; }
    }
}
