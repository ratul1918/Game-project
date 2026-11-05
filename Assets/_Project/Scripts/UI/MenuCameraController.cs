using UnityEngine;

namespace TheLightHouse
{
    public sealed class MenuCameraController : MonoBehaviour
    {
        [Header("Cinematic Sway")]
        [SerializeField] private float swayAmount = 0.35f;
        [SerializeField] private float swaySpeed = 0.45f;
        [SerializeField] private float pitchSway = 0.8f;
        [SerializeField] private float yawSway = 1.2f;

        [Header("Rotating Lighthouse Beacon")]
        [SerializeField] private Transform lighthouseBeam;
        [SerializeField] private float beamRotationSpeed = 22f;

        [Header("Distant Lightning")]
        [SerializeField] private Light lightningLight;
        [SerializeField] private float minLightningInterval = 7f;
        [SerializeField] private float maxLightningInterval = 18f;

        private Vector3 initialPosition;
        private Quaternion initialRotation;
        private float nextLightningTime;
        private float lightningFlashUntil;

        private void Start()
        {
            initialPosition = transform.position;
            initialRotation = transform.rotation;
            nextLightningTime = Time.time + Random.Range(minLightningInterval, maxLightningInterval);
            if (lightningLight) lightningLight.intensity = 0f;
        }

        private void Update()
        {
            // Subtle cinematic drift
            float t = Time.time * swaySpeed;
            float offsetX = Mathf.Sin(t) * swayAmount;
            float offsetY = Mathf.Cos(t * 0.7f) * (swayAmount * 0.6f);
            transform.position = initialPosition + new Vector3(offsetX, offsetY, 0f);

            float pitch = Mathf.Sin(t * 0.8f) * pitchSway;
            float yaw = Mathf.Cos(t * 0.5f) * yawSway;
            transform.rotation = initialRotation * Quaternion.Euler(pitch, yaw, 0f);

            // Rotate lighthouse beam
            if (lighthouseBeam)
            {
                lighthouseBeam.Rotate(Vector3.up, beamRotationSpeed * Time.deltaTime, Space.World);
            }

            // Distant lightning
            if (Time.time >= nextLightningTime)
            {
                lightningFlashUntil = Time.time + Random.Range(0.08f, 0.22f);
                nextLightningTime = Time.time + Random.Range(minLightningInterval, maxLightningInterval);
            }

            if (lightningLight)
            {
                if (Time.time < lightningFlashUntil)
                {
                    lightningLight.intensity = Random.Range(1.8f, 3.5f);
                }
                else
                {
                    lightningLight.intensity = Mathf.MoveTowards(lightningLight.intensity, 0f, Time.deltaTime * 6f);
                }
            }
        }
    }
}
