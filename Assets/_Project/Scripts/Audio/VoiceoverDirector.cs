using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace TheLightHouse
{
    /// <summary>
    /// Manages narrative voiceover spatial audio playback and dynamic ducking.
    /// When voiceover plays, ambient soundscapes and SFX are smoothly ducked by -4dB,
    /// and restored seamlessly when the speech concludes.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class VoiceoverDirector : MonoBehaviour
    {
        public static VoiceoverDirector Instance { get; private set; }

        [Header("Mixer Ducking Configuration")]
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private string ambienceVolumeParam = "AmbienceVolume";
        [SerializeField] private string sfxVolumeParam = "SfxVolume";
        [SerializeField] private float duckingDb = -4.0f;
        [SerializeField] private float duckAttackSeconds = 0.15f;
        [SerializeField] private float duckReleaseSeconds = 0.55f;

        [Header("Audio Sources")]
        [SerializeField] private AudioSource voiceoverSource2D;
        [SerializeField] private AmbientAudioDirector ambientDirector;

        private int _activeVoiceCount;
        private Coroutine _duckRoutine;
        private float _currentDuckMultiplier = 1.0f;
        private float _targetDuckMultiplier = 1.0f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;

            EnsureVoiceSource();
            ambientDirector ??= FindAnyObjectByType<AmbientAudioDirector>();
        }

        private void OnEnable()
        {
            GameEvents.DialogueRequested += OnDialogueRequested;
        }

        private void OnDisable()
        {
            GameEvents.DialogueRequested -= OnDialogueRequested;

            if (Instance == this) Instance = null;
            if (_duckRoutine != null)
            {
                StopCoroutine(_duckRoutine);
                _duckRoutine = null;
            }
        }

        private void EnsureVoiceSource()
        {
            if (!voiceoverSource2D)
            {
                voiceoverSource2D = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
                voiceoverSource2D.spatialBlend = 0f; // 2D voiceover
                voiceoverSource2D.playOnAwake = false;
                voiceoverSource2D.priority = 16; // Higher priority than SFX
            }
        }

        private void OnDialogueRequested(DialogueLine line)
        {
            if (line == null) return;

            // If line has a clip and no custom spatial emitter handled it, play via 2D voiceover source
            if (line.clip != null)
            {
                StartCoroutine(PlayVoiceRoutine(voiceoverSource2D, line.clip, line.duration));
            }
            else if (line.duration > 0.1f)
            {
                // Duck during text-only dialogue subtitle duration
                StartCoroutine(TrackVoiceDuration(line.duration));
            }
        }

        public void PlaySpatialVoice(AudioSource spatialSource, DialogueLine line)
        {
            if (spatialSource == null || line == null) return;

            GameEvents.RequestDialogue(line);
            if (line.clip != null)
            {
                StartCoroutine(PlayVoiceRoutine(spatialSource, line.clip, line.duration));
            }
        }

        private IEnumerator PlayVoiceRoutine(AudioSource source, AudioClip clip, float fallbackDuration)
        {
            _activeVoiceCount++;
            ApplyDucking(true);

            if (source)
            {
                source.PlayOneShot(clip);
            }

            float duration = clip != null ? clip.length : fallbackDuration;
            yield return new WaitForSeconds(duration);

            _activeVoiceCount = Mathf.Max(0, _activeVoiceCount - 1);
            if (_activeVoiceCount == 0)
            {
                ApplyDucking(false);
            }
        }

        private IEnumerator TrackVoiceDuration(float duration)
        {
            _activeVoiceCount++;
            ApplyDucking(true);

            yield return new WaitForSeconds(duration);

            _activeVoiceCount = Mathf.Max(0, _activeVoiceCount - 1);
            if (_activeVoiceCount == 0)
            {
                ApplyDucking(false);
            }
        }

        private void ApplyDucking(bool active)
        {
            // -4dB equals amplitude multiplier of 10^(-4/20) ~ 0.6309
            _targetDuckMultiplier = active ? 0.631f : 1.0f;

            if (_duckRoutine != null) StopCoroutine(_duckRoutine);
            _duckRoutine = StartCoroutine(DuckTransitionRoutine(active ? duckAttackSeconds : duckReleaseSeconds));
        }

        private IEnumerator DuckTransitionRoutine(float transitionDuration)
        {
            float startMult = _currentDuckMultiplier;
            float elapsed = 0f;

            while (elapsed < transitionDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / transitionDuration);
                _currentDuckMultiplier = Mathf.Lerp(startMult, _targetDuckMultiplier, t);

                // Update AudioMixer parameters if present
                if (audioMixer)
                {
                    float db = Mathf.Lerp(0f, duckingDb, 1f - (_currentDuckMultiplier - 0.631f) / 0.369f);
                    audioMixer.SetFloat(ambienceVolumeParam, db);
                    audioMixer.SetFloat(sfxVolumeParam, db);
                }

                // Also directly scale AmbientAudioDirector volume
                if (ambientDirector)
                {
                    AudioListener.volume = _targetDuckMultiplier < 0.9f ? 0.88f : 1.0f;
                }

                yield return null;
            }

            _currentDuckMultiplier = _targetDuckMultiplier;
            _duckRoutine = null;
        }
    }
}
