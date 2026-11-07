using UnityEngine;

namespace TheLightHouse
{
    public sealed class ReelRotator : MonoBehaviour
    {
        [SerializeField] private Transform leftReel;
        [SerializeField] private Transform rightReel;
        [SerializeField] private float rotationSpeed = 180f;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private RecordedTransmitter transmitter;

        private void Awake()
        {
            if (!audioSource) audioSource = GetComponent<AudioSource>();
            if (!transmitter) transmitter = GetComponent<RecordedTransmitter>();
        }

        private void Update()
        {
            bool isPlaying = (audioSource && audioSource.isPlaying) || (transmitter && transmitter.Prompt != "The recorder is stopped");
            if (!isPlaying) return;

            float delta = rotationSpeed * Time.deltaTime;
            if (leftReel) leftReel.Rotate(Vector3.forward, delta, Space.Self);
            if (rightReel) rightReel.Rotate(Vector3.forward, delta * 1.05f, Space.Self);
        }
    }
}
