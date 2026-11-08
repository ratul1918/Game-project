using UnityEngine;
using UnityEngine.Audio;

namespace TheLightHouse.Voice
{
    [CreateAssetMenu(fileName = "VoiceSpeaker", menuName = "TheLightHouse/Voice/Voice Speaker")]
    public sealed class VoiceSpeaker : ScriptableObject
    {
        [SerializeField] private SpeakerId speakerId;
        [SerializeField] private string displayName;
        [SerializeField] private AudioMixerGroup outputGroup;
        [Range(-6f, 3f)]
        [SerializeField] private float volumeTrimDb;
        [SerializeField] private Vector2 pitchRandomRange = new(0.98f, 1.02f);
        [Range(10f, 22000f)]
        [SerializeField] private float lowPassCutoff = 22000f;
        [Range(10f, 22000f)]
        [SerializeField] private float highPassCutoff = 10f;

        public SpeakerId SpeakerId { get => speakerId; set => speakerId = value; }
        public string DisplayName { get => displayName; set => displayName = value; }
        public AudioMixerGroup OutputGroup { get => outputGroup; set => outputGroup = value; }
        public float VolumeTrimDb { get => volumeTrimDb; set => volumeTrimDb = value; }
        public Vector2 PitchRandomRange { get => pitchRandomRange; set => pitchRandomRange = value; }
        public float LowPassCutoff { get => lowPassCutoff; set => lowPassCutoff = value; }
        public float HighPassCutoff { get => highPassCutoff; set => highPassCutoff = value; }

        public float GetRandomPitch()
        {
            if (pitchRandomRange.x >= pitchRandomRange.y) return pitchRandomRange.x;
            return Random.Range(pitchRandomRange.x, pitchRandomRange.y);
        }

        public float GetLinearVolumeMultiplier()
        {
            return Mathf.Pow(10f, volumeTrimDb / 20f);
        }
    }
}
