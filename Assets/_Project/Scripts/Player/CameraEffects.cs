using UnityEngine;

namespace TheLightHouse
{
    public sealed class CameraEffects : MonoBehaviour
    {
        float dip;
        float shake;
        public void Land(float speed, PlayerMovementSettings settings)
        {
            dip = settings.landingDip * Mathf.Clamp01(speed / settings.hardLandingSpeed);
            if (speed >= settings.hardLandingSpeed) shake = settings.hardLandingShake;
        }
        public void Tick(Transform view, float pitch, float capsuleHeight, float bob, bool sprinting,
            float stamina, float lean, PlayerMovementSettings settings, float deltaTime)
        {
            float t = Mathf.InverseLerp(settings.crouchingHeight, settings.standingHeight, capsuleHeight);
            float height = Mathf.Lerp(settings.crouchingCameraHeight, settings.standingCameraHeight, t);
            float breath = stamina < settings.lowStaminaThreshold && !settings.disableBobAndFov
                ? Mathf.Sin(Time.time * settings.breathingFrequency) * settings.breathingAmplitude : 0f;
            Vector3 jitter = settings.disableBobAndFov ? Vector3.zero : Random.insideUnitSphere * shake;
            Vector3 target = new Vector3(lean * settings.leanDistance, height + bob + breath - (settings.disableBobAndFov ? 0f : dip), 0f) + jitter;
            float blend = 1f - Mathf.Exp(-settings.cameraSmoothing * deltaTime);
            view.localPosition = Vector3.Lerp(view.localPosition, target, blend);
            view.localRotation = Quaternion.Euler(pitch, 0f, -lean * settings.leanAngle);
            var camera = view.GetComponent<Camera>();
            if (camera) camera.fieldOfView = Mathf.Lerp(camera.fieldOfView,
                settings.baseFov + (sprinting && !settings.disableBobAndFov ? settings.sprintFovIncrease : 0f), blend);
            dip = Mathf.Lerp(dip, 0f, blend);
            shake = Mathf.Lerp(shake, 0f, blend);
        }
    }
}
