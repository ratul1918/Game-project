using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace TheLightHouse.Voice
{
    /// <summary>
    /// Central manager for voice playback, audio source pooling, priority ducking,
    /// and subtitle synchronization across 2D and 3D spatial emitters.
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

        [Header("Queue & Timing")]
        [SerializeField] private float cadenceGap = 0.25f;
        [SerializeField] private float subtitleTail = 0.4f;

        [Header("Ducking Configuration")]
        [SerializeField] private string ambienceVolumeParam = "AmbienceVolume";
        [SerializeField] private string musicVolumeParam = "MusicVolume";
        [SerializeField] private float duckingDb = -4.0f;
        [SerializeField] private float duckAttackSeconds = 0.045f;
        [SerializeField] private float duckReleaseSeconds = 0.350f;

        private const int PoolSize = 6;
        private readonly List<AudioSource> _sourcePool = new();
        private readonly Queue<QueuedVoiceRequest> _playbackQueue = new();

        private QueuedVoiceRequest _currentActive;
        private Coroutine _queueWorkerRoutine;
        private Coroutine _duckRoutine;
        private int _duckingRefCount;

        private struct QueuedVoiceRequest
        {
            public VoiceLine Line;
            public Transform SourceTransform;
            public Action OnComplete;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializePool();
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
            _playbackQueue.Clear();
            _currentActive = default;
        }

        private float _lastStaminaReactionTime;
        private int _staminaReactionIndex;
        private float _lastCrankReactionTime;

        private void OnCrankActiveChanged(bool active)
        {
            if (active && Time.time - _lastCrankReactionTime > 3.5f)
            {
                _lastCrankReactionTime = Time.time;
                if (voiceDatabase != null && voiceDatabase.TryGetLine("REACT_DAN_CRANK_EFFORT", out var line))
                {
                    PlayVoice(line);
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
                    PlayVoice(line);
                }
            }
        }

        private void OnPlayerHidden(PlayerHiddenEvent e)
        {
            if (e.IsHidden && voiceDatabase != null && voiceDatabase.TryGetLine("REACT_DAN_HOLD_BREATH", out var line))
            {
                PlayVoice(line);
            }
        }

        private void InitializePool()
        {
            if (_sourcePool.Count > 0) return;

            for (int i = 0; i < PoolSize; i++)
            {
                var sourceGo = new GameObject($"VoiceSource_Pool_{i:D2}");
                sourceGo.transform.SetParent(transform, false);
                var src = sourceGo.AddComponent<AudioSource>();
                src.playOnAwake = false;
                src.spatialBlend = 0f;
                src.rolloffMode = AudioRolloffMode.Logarithmic;
                src.minDistance = 1.5f;
                src.maxDistance = 18f;
                src.outputAudioMixerGroup = voiceGroup;
                _sourcePool.Add(src);
            }
        }

        private AudioSource GetAvailableSource()
        {
            foreach (var src in _sourcePool)
            {
                if (src && !src.isPlaying) return src;
            }
            // If all busy, pick the oldest or return first
            return _sourcePool.Count > 0 ? _sourcePool[0] : null;
        }

        private void OnDialogueRequested(DialogueLine line)
        {
            if (line == null) return;

            // Try to find matching VoiceLine in database by text or speaker
            if (voiceDatabase != null)
            {
                foreach (var vLine in voiceDatabase.AllLines)
                {
                    if (vLine != null && !string.IsNullOrEmpty(vLine.Text) &&
                        string.Equals(vLine.Text.Trim(), line.text.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        PlayVoice(vLine);
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

            PlayVoice(temp);
        }

        public void PlayVoice(VoiceLine line, Transform sourceTransform = null, Action onComplete = null)
        {
            if (line == null) return;

            var request = new QueuedVoiceRequest
            {
                Line = line,
                SourceTransform = sourceTransform,
                OnComplete = onComplete
            };

            // Priority Check: Story lines (Priority >= 50) immediately interrupt reactions (Priority 0)
            if (_currentActive.Line != null)
            {
                if (_currentActive.Line.Priority == 0 && line.Priority >= 50)
                {
                    // Interrupt lower priority reaction
                    InterruptCurrent();
                    _playbackQueue.Enqueue(request);
                    if (_queueWorkerRoutine == null) _queueWorkerRoutine = StartCoroutine(QueueWorker());
                    return;
                }

                if (line.Priority < 50 && _currentActive.Line.Priority >= 50)
                {
                    // Reactions never interrupt story dialogue, drop or silently discard stamina gasps during speech
                    if (line.AudioType == VoiceAudioType.Reaction) return;
                }
            }

            _playbackQueue.Enqueue(request);
            if (_queueWorkerRoutine == null)
            {
                _queueWorkerRoutine = StartCoroutine(QueueWorker());
            }
        }

        private void InterruptCurrent()
        {
            foreach (var src in _sourcePool)
            {
                if (src && src.isPlaying) src.Stop();
            }

            if (_currentActive.Line != null)
            {
                VoiceLineEnded?.Invoke(_currentActive.Line);
                _currentActive.OnComplete?.Invoke();
                _currentActive = default;
            }

            if (_queueWorkerRoutine != null)
            {
                StopCoroutine(_queueWorkerRoutine);
                _queueWorkerRoutine = null;
            }
        }

        private IEnumerator QueueWorker()
        {
            while (_playbackQueue.Count > 0)
            {
                var req = _playbackQueue.Dequeue();
                _currentActive = req;
                var line = req.Line;

                // Fire event
                VoiceLineStarted?.Invoke(line);
                GameEvents.RaiseVoiceLineStarted(line);

                // Acquire audio source
                var src = GetAvailableSource();
                VoiceSpeaker speakerCfg = GetSpeakerConfig(line.Speaker);

                if (src)
                {
                    ConfigureSource(src, line, req.SourceTransform, speakerCfg);
                }

                // Show subtitles
                float effectiveDuration = line.GetEffectiveDuration();
                var subtitleUi = FindAnyObjectByType<SubtitleUI>();
                if (subtitleUi)
                {
                    subtitleUi.enabled = true;
                    // Subtitle is handled synchronously with line.duration + tail
                }

                // Begin Ducking
                StartDucking();

                // Play Audio
                if (src && line.Clip != null)
                {
                    src.clip = line.Clip;
                    src.Play();
                }

                float waitTime = effectiveDuration + subtitleTail;
                yield return new WaitForSeconds(waitTime);

                // Stop Audio & Ducking
                if (src && src.isPlaying)
                {
                    src.Stop();
                }

                StopDucking();

                VoiceLineEnded?.Invoke(line);
                GameEvents.RaiseVoiceLineEnded(line);
                req.OnComplete?.Invoke();
                _currentActive = default;

                // Back-to-back cadence gap
                if (_playbackQueue.Count > 0)
                {
                    yield return new WaitForSeconds(cadenceGap);
                }
            }

            _queueWorkerRoutine = null;
        }

        private void ConfigureSource(AudioSource src, VoiceLine line, Transform sourceTransform, VoiceSpeaker speakerCfg)
        {
            if (line.SpatialMode == SpatialMode.ThreeDimensional && sourceTransform != null)
            {
                src.transform.position = sourceTransform.position;
                src.spatialBlend = 1.0f;
                src.minDistance = 1.5f;
                src.maxDistance = 18.0f;
                src.rolloffMode = AudioRolloffMode.Logarithmic;
            }
            else
            {
                src.transform.position = transform.position;
                src.spatialBlend = 0f;
            }

            // Assign mixer group
            if (line.AudioType == VoiceAudioType.Radio || line.AudioType == VoiceAudioType.Tape)
            {
                src.outputAudioMixerGroup = radioGroup ? radioGroup : voiceGroup;
            }
            else if (speakerCfg && speakerCfg.OutputGroup)
            {
                src.outputAudioMixerGroup = speakerCfg.OutputGroup;
            }
            else
            {
                src.outputAudioMixerGroup = voiceGroup;
            }

            // Pitch & Volume trim
            float pitch = speakerCfg ? speakerCfg.GetRandomPitch() : 1.0f;
            float volumeMul = speakerCfg ? speakerCfg.GetLinearVolumeMultiplier() : 1.0f;
            src.pitch = pitch;
            src.volume = volumeMul;
        }

        private VoiceSpeaker GetSpeakerConfig(SpeakerId speakerId)
        {
            foreach (var cfg in speakerConfigs)
            {
                if (cfg != null && cfg.SpeakerId == speakerId) return cfg;
            }
            return null;
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
                audioMixer.SetFloat(musicVolumeParam, curDb);
                yield return null;
            }

            audioMixer.SetFloat(ambienceVolumeParam, targetDb);
            audioMixer.SetFloat(musicVolumeParam, targetDb);
            _duckRoutine = null;
        }
    }
}
