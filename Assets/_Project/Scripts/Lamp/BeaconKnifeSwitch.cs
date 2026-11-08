using UnityEngine;

namespace TheLightHouse
{
    public sealed class BeaconKnifeSwitch : MonoBehaviour, IInteractable
    {
        [Header("Interaction")]
        [SerializeField] private string prompt = "Press [E] to Throw Power Switch";
        [SerializeField] private bool requiresHold = false;

        [Header("Switch Geometry")]
        [SerializeField] private Transform switchLever;
        [SerializeField] private float openAngle = 45f;
        [SerializeField] private float closedAngle = 0f;
        [SerializeField] private float throwSpeed = 360f;

        [Header("Effects & Audio")]
        [SerializeField] private ParticleSystem sparkParticles;
        [SerializeField] private Light sparkFlashLight;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip throwSound;

        [Header("Lamp Connection")]
        [SerializeField] private LampController lampController;

        private bool isClosed = true;
        private float currentAngle;
        private float sparkTimer;

        public string Prompt => prompt;
        public bool RequiresHold => requiresHold;
        public bool IsClosed => isClosed;

        private void Awake()
        {
            if (audioSource == null) audioSource = GetComponent<AudioSource>();
            if (lampController == null) lampController = Object.FindAnyObjectByType<LampController>();
            currentAngle = isClosed ? closedAngle : openAngle;
            if (switchLever != null)
            {
                switchLever.localRotation = Quaternion.Euler(currentAngle, 0f, 0f);
            }
        }

        private void Update()
        {
            float targetAngle = isClosed ? closedAngle : openAngle;
            if (!Mathf.Approximately(currentAngle, targetAngle))
            {
                currentAngle = Mathf.MoveTowards(currentAngle, targetAngle, throwSpeed * Time.deltaTime);
                if (switchLever != null)
                {
                    switchLever.localRotation = Quaternion.Euler(currentAngle, 0f, 0f);
                }
            }

            if (sparkTimer > 0f)
            {
                sparkTimer -= Time.deltaTime;
                if (sparkTimer <= 0f && sparkFlashLight != null)
                {
                    sparkFlashLight.enabled = false;
                }
            }
        }

        public void Interact()
        {
            isClosed = !isClosed;

            // Trigger mechanical clunk audio
            if (audioSource != null)
            {
                if (throwSound != null)
                {
                    audioSource.PlayOneShot(throwSound);
                }
                else
                {
                    // Synthesize metallic clunk pitch
                    audioSource.pitch = Random.Range(0.85f, 1.15f);
                    audioSource.Play();
                }
            }

            // Contact spark FX
            if (sparkParticles != null)
            {
                sparkParticles.Play();
            }
            if (sparkFlashLight != null)
            {
                sparkFlashLight.enabled = true;
                sparkTimer = 0.08f;
            }

            // Signal game events and lamp
            GameEvents.RaiseStorySignal(isClosed ? "beacon-switch-on" : "beacon-switch-off");

            if (lampController != null)
            {
                if (isClosed)
                {
                    lampController.TryIgnite();
                }
            }
        }

        public void StopInteract() { }
    }
}
