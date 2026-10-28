using UnityEngine;

namespace TheLightHouse
{
    [RequireComponent(typeof(NoiseEmitter))]
    public sealed class CrankStation : MonoBehaviour, IInteractable
    {
        [SerializeField] private string prompt = "Crank lamp (hold)";
        [SerializeField] private LampController lamp;
        [SerializeField] private NoiseEmitter noiseEmitter;
        [SerializeField, Min(0f)] private float fuelPerSecond = 8f;
        [SerializeField, Min(0f)] private float noiseLoudness = 20f;
        [SerializeField, Min(0.1f)] private float noiseInterval = 1f;

        private float nextNoiseTime;
        private bool isCranking;

        public string Prompt => prompt;

        private void Awake()
        {
            noiseEmitter ??= GetComponent<NoiseEmitter>();
        }

        private void OnDisable()
        {
            StopInteract();
        }

        public void Interact()
        {
            if (lamp == null)
            {
                return;
            }

            if (!isCranking)
            {
                isCranking = true;
                nextNoiseTime = Time.time;
                lamp.SetCranking(true);
            }

            lamp.AddFuel(fuelPerSecond * Time.deltaTime);

            if (Time.time >= nextNoiseTime)
            {
                noiseEmitter.Emit(noiseLoudness);
                nextNoiseTime = Time.time + noiseInterval;
            }
        }

        public void StopInteract()
        {
            if (!isCranking)
            {
                return;
            }

            isCranking = false;
            lamp?.SetCranking(false);
        }
    }
}
