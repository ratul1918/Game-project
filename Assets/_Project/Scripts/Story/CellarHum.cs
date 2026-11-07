using System.Collections;
using UnityEngine;

namespace TheLightHouse
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class CellarHum : MonoBehaviour
    {
        [SerializeField] private AudioClip shanty;
        [SerializeField] private WetFootprints footprints;
        [SerializeField] private bool enabledForActTwo;
        [SerializeField] private Transform hummingEmitterPoint;

        private AudioSource _audioSource;
        private bool _searching;
        private bool _played;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _audioSource.spatialBlend = 1.0f; // 3D spatial sound deep in the subterranean corridor
            _audioSource.minDistance = 3.0f;
            _audioSource.maxDistance = 24.0f;
            _audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
            _audioSource.playOnAwake = false;

            if (!shanty) shanty = LoadAudio("Voice_Crane_CellarShanty.wav");
        }

        private void OnEnable()
        {
            GameEvents.StorySignal += Signal;
            GameEvents.StoryBeatTriggered += Beat;
        }

        private void OnDisable()
        {
            GameEvents.StorySignal -= Signal;
            GameEvents.StoryBeatTriggered -= Beat;
        }

        private void Beat(StoryBeatTriggeredEvent e)
        {
            if (e.Id == "A2-B01" || e.Id == "A4-B02") _searching = true;
        }

        private void Signal(string id)
        {
            if (id == "find-fuel") _searching = true;
        }

        private void OnTriggerStay(Collider other)
        {
            if (!enabledForActTwo || !_searching || _played) return;
            var player = other.GetComponent<FirstPersonController>();
            if (!player) return;

            _played = true;

            // Trigger flashlight flicker when stepping into deep cellar corridor
            var flashlight = player.GetComponentInChildren<FlashlightController>();
            if (flashlight)
            {
                flashlight.TriggerFlicker(1.6f, 6);
            }

            if (shanty && _audioSource)
            {
                _audioSource.PlayOneShot(shanty, 0.95f);
            }

            var line = new DialogueLine("Thomas Crane", "(low, wet baritone humming 'Leave Her, Johnny')")
            {
                clip = shanty,
                duration = shanty != null ? shanty.length : 8f
            };
            GameEvents.RequestDialogue(line);

            GameEvents.RaiseStorySignal("cellar-hum");
            if (footprints) footprints.Reveal();
        }

        private static AudioClip LoadAudio(string filename)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>($"Assets/_Project/Audio/{filename}");
#else
            return null;
#endif
        }
    }
}
