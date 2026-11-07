using UnityEngine;

namespace TheLightHouse
{
    public sealed class MenuCameraController : MonoBehaviour
    {
        [Header("Cinematic Drift & Handheld Sway")]
        [SerializeField] private float swayAmount = 0.28f;
        [SerializeField] private float swaySpeed = 0.32f;
        [SerializeField] private float pitchSway = 0.65f;
        [SerializeField] private float yawSway = 0.95f;
        [SerializeField] private float dollySpeed = 0.025f;
        [SerializeField] private float dollyMaxDistance = 1.4f;

        [Header("Rotating Lighthouse Beacon")]
        [SerializeField] private Transform lighthouseBeam;
        [SerializeField] private float beamRotationSpeed = 16f;

        [Header("Distant Lightning")]
        [SerializeField] private Light lightningLight;
        [SerializeField] private Material skyboxMaterial;
        [SerializeField] private float minLightningInterval = 9f;
        [SerializeField] private float maxLightningInterval = 20f;

        [Header("Audio Feedback")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip hoverSound;
        [SerializeField] private AudioClip clickSound;

        private Vector3 initialPosition;
        private Quaternion initialRotation;
        private float nextLightningTime;
        private float lightningFlashUntil;
        private float currentLightningIntensity;
        private float dollyProgress;

        private void Start()
        {
            initialPosition = transform.position;
            initialRotation = transform.rotation;
            nextLightningTime = Time.time + Random.Range(minLightningInterval, maxLightningInterval);
            if (lightningLight) lightningLight.intensity = 0f;
            if (!skyboxMaterial && RenderSettings.skybox) skyboxMaterial = RenderSettings.skybox;
            if (skyboxMaterial && skyboxMaterial.HasProperty("_LightningIntensity"))
            {
                skyboxMaterial.SetFloat("_LightningIntensity", 0f);
            }

            if (!audioSource)
            {
                audioSource = GetComponent<AudioSource>();
                if (!audioSource) audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0f;
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            // Slow cinematic dolly push-in
            dollyProgress = Mathf.Repeat(dollyProgress + dollySpeed * dt, dollyMaxDistance * 2f);
            float forwardPush = Mathf.PingPong(dollyProgress, dollyMaxDistance);

            // Subtle handheld breathing sway
            float t = Time.time * swaySpeed;
            float offsetX = Mathf.Sin(t) * swayAmount;
            float offsetY = Mathf.Cos(t * 0.72f) * (swayAmount * 0.65f);
            Vector3 swayOffset = transform.right * offsetX + transform.up * offsetY + transform.forward * forwardPush;
            transform.position = initialPosition + swayOffset;

            float pitch = Mathf.Sin(t * 0.78f) * pitchSway;
            float yaw = Mathf.Cos(t * 0.54f) * yawSway;
            transform.rotation = initialRotation * Quaternion.Euler(pitch, yaw, 0f);

            // Rotate sweeping lighthouse beacon beam
            if (lighthouseBeam)
            {
                lighthouseBeam.Rotate(Vector3.up, beamRotationSpeed * dt, Space.World);
            }

            // Distant lightning logic
            if (Time.time >= nextLightningTime)
            {
                lightningFlashUntil = Time.time + Random.Range(0.09f, 0.24f);
                nextLightningTime = Time.time + Random.Range(minLightningInterval, maxLightningInterval);
            }

            if (Time.time < lightningFlashUntil)
            {
                currentLightningIntensity = Random.Range(1.8f, 3.6f);
            }
            else
            {
                currentLightningIntensity = Mathf.MoveTowards(currentLightningIntensity, 0f, dt * 7.5f);
            }

            if (lightningLight)
            {
                lightningLight.intensity = currentLightningIntensity;
            }

            if (skyboxMaterial && skyboxMaterial.HasProperty("_LightningIntensity"))
            {
                skyboxMaterial.SetFloat("_LightningIntensity", currentLightningIntensity * 0.85f);
            }
        }

        public void PlayHoverSound()
        {
            if (audioSource && hoverSound)
            {
                audioSource.pitch = Random.Range(0.96f, 1.04f);
                audioSource.PlayOneShot(hoverSound, 0.55f);
            }
        }

        public void PlayClickSound()
        {
            if (audioSource && clickSound)
            {
                audioSource.pitch = 1.0f;
                audioSource.PlayOneShot(clickSound, 0.75f);
            }
        }
    }
}
