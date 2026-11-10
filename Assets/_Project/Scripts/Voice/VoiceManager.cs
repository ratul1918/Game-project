using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace TheLightHouse.Voice
{
    /// <summary>
    /// Central manager for voice playback, dedicated voice channel, priority management,
    /// dynamic audio mixer ducking, and subtitle synchronization.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class VoiceManager : MonoBehaviour
    {
        public static VoiceManager Instance { get; private set; }

        public static event Action<VoiceLine> VoiceLineStarted;
        public static event Action<VoiceLine> VoiceLineEnded;

        [Header("Database & Routing")]
        [SerializeField] private VoiceDatabase voiceDatabase;
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private AudioMixerGroup voiceGroup;
        [SerializeField] private AudioMixerGroup radioGroup;
        [SerializeField] private AudioMixerGroup sfxGroup;
        [SerializeField] private List<VoiceSpeaker> speakerConfigs = new();

        [Header("Ducking Configuration")]
        [SerializeField] private string ambienceVolumeParam = "AmbienceVolume";
        [SerializeField] private string sfxVolumeParam = "SfxVolume";
        [SerializeField] private string musicVolumeParam = "MusicVolume";
        [SerializeField] private float duckingDb = -4.0f;
        [SerializeField] private float duckAttackSeconds = 0.045f;
        [SerializeField] private float duckReleaseSeconds = 0.350f;

        [Header("Primary Voice Audio Channel")]
        [SerializeField] private AudioSource currentVoiceSource;
        [SerializeField] private int currentPriority = 0;

        private VoiceLine _currentActiveLine;
        private Coroutine _speechTrackRoutine;
        private Coroutine _duckRoutine;
        private int _duckingRefCount;

        private float _lastStaminaReactionTime;
        private int _staminaReactionIndex;
        private float _lastCrankReactionTime;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            EnsureVoiceSource();

            if (voiceDatabase) voiceDatabase.InitializeLookup();
        }

        private void OnEnable()
        {
            GameEvents.DialogueRequested += OnDialogueRequested;
            GameEvents.StaminaChanged += OnStaminaChanged;
            GameEvents.PlayerHidden += OnPlayerHidden;
            GameEvents.CrankActiveChanged += OnCrankActiveChanged;
        }

        private void OnDisable()
        {
            GameEvents.DialogueRequested -= OnDialogueRequested;
            GameEvents.StaminaChanged -= OnStaminaChanged;
            GameEvents.PlayerHidden -= OnPlayerHidden;
            GameEvents.CrankActiveChanged -= OnCrankActiveChanged;

            if (Instance == this) Instance = null;
            StopAllCoroutines();
            if (currentVoiceSource && currentVoiceSource.isPlaying) currentVoiceSource.Stop();
            currentPriority = 0;
            _currentActiveLine = null;
        }

        private void EnsureVoiceSource()
        {
            if (!currentVoiceSource)
            {
                currentVoiceSource = GetComponent<AudioSource>();
                if (!currentVoiceSource)
                {
                    currentVoiceSource = gameObject.AddComponent<AudioSource>();
                }
            }

            currentVoiceSource.playOnAwake = false;
            currentVoiceSource.spatialBlend = 0.0f;
            currentVoiceSource.rolloffMode = AudioRolloffMode.Logarithmic;
            currentVoiceSource.minDistance = 1.5f;
            currentVoiceSource.maxDistance = 18.0f;
            currentVoiceSource.priority = 10; // High audio priority
            if (voiceGroup) currentVoiceSource.outputAudioMixerGroup = voiceGroup;
        }

        private void OnDialogueRequested(DialogueLine line)
        {
            if (line == null) return;

            // Try to find matching VoiceLine in database by text
            if (voiceDatabase != null)
            {
                foreach (var vLine in voiceDatabase.AllLines)
                {
                    if (vLine != null && !string.IsNullOrEmpty(vLine.Text) &&
                        string.Equals(vLine.Text.Trim(), line.text.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        PlayVoiceLine(vLine, null);
                        return;
                    }
                }
            }

            // Fallback: create temporary runtime VoiceLine
            var temp = ScriptableObject.CreateInstance<VoiceLine>();
            temp.LineId = "RUNTIME_" + Guid.NewGuid().ToString("N")[..8];
            temp.Speaker = ParseSpeaker(line.speaker);
            temp.Text = line.text;
            temp.Clip = line.clip;
            temp.Priority = 50;
            temp.SubtitleOverrideDuration = line.duration;
            temp.SpatialMode = SpatialMode.TwoDimensional;
            temp.AudioType = VoiceAudioType.Spoken;

            PlayVoiceLine(temp, null);
        }

        /// <summary>
        /// Dedicated Audio Channel playback method to prevent voice stacking and overlapping.
        /// </summary>
        public void PlayVoiceLine(VoiceLine line, Transform emitter = null, Action onComplete = null)
        {
            if (line == null) return;
            EnsureVoiceSource();

            // 1. If a lower or equal priority line is playing, stop it immediately.
            // If the playing line has higher priority, ignore incoming lower priority request.
            if (currentVoiceSource.isPlaying)
            {
                if (line.Priority < currentPriority)
                {
                    return; // Ignore lower priority voice request
                }
                currentVoiceSource.Stop();
                if (_speechTrackRoutine != null)
                {
                    StopCoroutine(_speechTrackRoutine);
                    _speechTrackRoutine = null;
                }
                if (_currentActiveLine != null)
                {
                    VoiceLineEnded?.Invoke(_currentActiveLine);
                    GameEvents.RaiseVoiceLineEnded(_currentActiveLine);
                }
            }

            // 2. Set channel & priority
            currentPriority = line.Priority;
            _currentActiveLine = line;
            currentVoiceSource.clip = line.Clip;

            // 3. Configure 2D vs 3D Spatial Blend
            if (line.SpatialMode == SpatialMode.ThreeDimensional && emitter != null)
            {
                currentVoiceSource.transform.position = emitter.position;
                currentVoiceSource.spatialBlend = 1.0f;
                currentVoiceSource.minDistance = 1.5f;
                currentVoiceSource.maxDistance = 18.0f;
                currentVoiceSource.rolloffMode = AudioRolloffMode.Logarithmic;
            }
            else
            {
                currentVoiceSource.transform.position = transform.position;
                currentVoiceSource.spatialBlend = 0.0f;
            }

            // Route audio through AudioMixer/Voice (or Radio group)
            VoiceSpeaker speakerCfg = GetSpeakerConfig(line.Speaker);
            if (line.AudioType == VoiceAudioType.Radio || line.AudioType == VoiceAudioType.Tape)
            {
                currentVoiceSource.outputAudioMixerGroup = radioGroup ? radioGroup : voiceGroup;
            }
            else if (speakerCfg && speakerCfg.OutputGroup)
            {
                currentVoiceSource.outputAudioMixerGroup = speakerCfg.OutputGroup;
            }
            else
            {
                currentVoiceSource.outputAudioMixerGroup = voiceGroup;
            }

            currentVoiceSource.pitch = speakerCfg ? speakerCfg.GetRandomPitch() : 1.0f;
            currentVoiceSource.volume = speakerCfg ? speakerCfg.GetLinearVolumeMultiplier() : 1.0f;

            if (line.Clip != null)
            {
                currentVoiceSource.Play();
            }

            // 4. Fire Subtitles with exact matching duration
            float displayTime = (line.Clip != null) ? line.Clip.length + 0.4f : line.SubtitleOverrideDuration;
            if (displayTime <= 0.05f) displayTime = line.GetEffectiveDuration();

            string speakerName = GetSpeakerDisplayName(line.Speaker);
            if (SubtitleUI.Instance != null)
            {
                SubtitleUI.Instance.Show(speakerName, line.Text, displayTime);
            }
            else
            {
                var sub = FindAnyObjectByType<SubtitleUI>();
                if (sub) sub.Show(speakerName, line.Text, displayTime);
            }

            // Fire events
            VoiceLineStarted?.Invoke(line);
            GameEvents.RaiseVoiceLineStarted(line);

            // Audio Mixer Ducking & duration tracking
            if (_speechTrackRoutine != null) StopCoroutine(_speechTrackRoutine);
            _speechTrackRoutine = StartCoroutine(DuckAndTrackSpeechRoutine(displayTime, line, onComplete));
        }

        /// <summary>
        /// Backwards compatible signature for PlayVoice callers.
        /// </summary>
        public void PlayVoice(VoiceLine line, Transform sourceTransform = null, Action onComplete = null)
        {
            PlayVoiceLine(line, sourceTransform, onComplete);
        }

        private IEnumerator DuckAndTrackSpeechRoutine(float duration, VoiceLine line, Action onComplete)
        {
            StartDucking();

            yield return new WaitForSeconds(duration);

            if (currentVoiceSource != null && currentVoiceSource.isPlaying)
            {
                currentVoiceSource.Stop();
            }

            StopDucking();

            currentPriority = 0;
            _currentActiveLine = null;
            _speechTrackRoutine = null;

            VoiceLineEnded?.Invoke(line);
            GameEvents.RaiseVoiceLineEnded(line);
            onComplete?.Invoke();
        }

        private void StartDucking()
        {
            _duckingRefCount++;
            if (_duckRoutine != null) StopCoroutine(_duckRoutine);
            _duckRoutine = StartCoroutine(AnimateMixerDucking(duckingDb, duckAttackSeconds));
        }

        private void StopDucking()
        {
            _duckingRefCount = Mathf.Max(0, _duckingRefCount - 1);
            if (_duckingRefCount == 0)
            {
                if (_duckRoutine != null) StopCoroutine(_duckRoutine);
                _duckRoutine = StartCoroutine(AnimateMixerDucking(0f, duckReleaseSeconds));
            }
        }

        private IEnumerator AnimateMixerDucking(float targetDb, float duration)
        {
            if (!audioMixer) yield break;

            audioMixer.GetFloat(ambienceVolumeParam, out float startAmbience);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float curDb = Mathf.Lerp(startAmbience, targetDb, t);

                audioMixer.SetFloat(ambienceVolumeParam, curDb);
                audioMixer.SetFloat(sfxVolumeParam, curDb);
                audioMixer.SetFloat(musicVolumeParam, curDb);
                yield return null;
            }

            audioMixer.SetFloat(ambienceVolumeParam, targetDb);
            audioMixer.SetFloat(sfxVolumeParam, targetDb);
            audioMixer.SetFloat(musicVolumeParam, targetDb);
            _duckRoutine = null;
        }

        private void OnCrankActiveChanged(bool active)
        {
            if (active && Time.time - _lastCrankReactionTime > 3.5f)
            {
                _lastCrankReactionTime = Time.time;
                if (voiceDatabase != null && voiceDatabase.TryGetLine("REACT_DAN_CRANK_EFFORT", out var line))
                {
                    PlayVoiceLine(line);
                }
            }
        }

        private void OnStaminaChanged(StaminaChangedEvent e)
        {
            if (e.Capacity <= 0) return;
            float norm = e.Current / e.Capacity;
            if (norm < 0.20f && Time.time - _lastStaminaReactionTime > 4.5f)
            {
                _lastStaminaReactionTime = Time.time;
                string[] exhaustIds = { "REACT_DAN_EXHAUST_01", "REACT_DAN_EXHAUST_02", "REACT_DAN_EXHAUST_03" };
                string id = exhaustIds[_staminaReactionIndex % exhaustIds.Length];
                _staminaReactionIndex++;
                if (voiceDatabase != null && voiceDatabase.TryGetLine(id, out var line))
                {
                    PlayVoiceLine(line);
                }
            }
        }

        private void OnPlayerHidden(PlayerHiddenEvent e)
        {
            if (e.IsHidden && voiceDatabase != null && voiceDatabase.TryGetLine("REACT_DAN_HOLD_BREATH", out var line))
            {
                PlayVoiceLine(line);
            }
        }

        private VoiceSpeaker GetSpeakerConfig(SpeakerId speakerId)
        {
            foreach (var cfg in speakerConfigs)
            {
                if (cfg != null && cfg.SpeakerId == speakerId) return cfg;
            }
            return null;
        }

        private string GetSpeakerDisplayName(SpeakerId id)
        {
            var cfg = GetSpeakerConfig(id);
            if (cfg != null && !string.IsNullOrEmpty(cfg.DisplayName)) return cfg.DisplayName;
            return id switch
            {
                SpeakerId.DanielHale => "Daniel",
                SpeakerId.Boatman => "Boatman",
                SpeakerId.Marta => "Marta",
                SpeakerId.ThomasCrane => "Thomas Crane",
                SpeakerId.TheDrowned => "The Drowned",
                _ => id.ToString()
            };
        }

        private static SpeakerId ParseSpeaker(string speakerName)
        {
            if (string.IsNullOrEmpty(speakerName)) return SpeakerId.DanielHale;
            var lower = speakerName.ToLowerInvariant();
            if (lower.Contains("boat")) return SpeakerId.Boatman;
            if (lower.Contains("marta")) return SpeakerId.Marta;
            if (lower.Contains("crane")) return SpeakerId.ThomasCrane;
            if (lower.Contains("drown")) return SpeakerId.TheDrowned;
            return SpeakerId.DanielHale;
        }
    }
}
