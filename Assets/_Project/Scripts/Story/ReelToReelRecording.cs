using System.Collections;
using UnityEngine;

namespace TheLightHouse
{
    /// <summary>
    /// Operates the reel-to-reel magnetic tape recorder in Thomas Crane's hidden bunker room.
    /// Spins the tape spools, plays the Harbor Board recruitment recording revealing Daniel's selection,
    /// triggers horror flashlight flickering, and raises the 'marta-recording' story signal.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public sealed class ReelToReelRecording : MonoBehaviour, IInteractable
    {
        [SerializeField] private AudioClip recordingClip;
        [SerializeField] private Transform reelLeft;
        [SerializeField] private Transform reelRight;
        [SerializeField] private float reelSpinSpeed = 160f;
        [SerializeField] private float triggerDistance = 3.2f;

        private AudioSource _audioSource;
        private bool _isPlaying;
        private bool _hasPlayed;
        private bool _isSpinning;

        public string Prompt => _hasPlayed ? "Replay recording" : "Play tape reel";
        public bool RequiresHold => false;

        private void Awake()
        {
            if (!_audioSource) _audioSource = GetComponent<AudioSource>();
            if (!_audioSource) _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.spatialBlend = 1.0f; // 3D spatial on tape deck
            _audioSource.minDistance = 1.5f;
            _audioSource.maxDistance = 16.0f;
            _audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
            _audioSource.playOnAwake = false;

            if (!recordingClip) recordingClip = LoadAudio("Voice_Marta_HarborBoardLedger.wav");

            FindReelsIfMissing();
        }

        private void OnEnable()
        {
            GameEvents.StorySignal += OnStorySignal;
        }

        private void OnDisable()
        {
            GameEvents.StorySignal -= OnStorySignal;
        }

        private void Update()
        {
            if (_isSpinning)
            {
                float delta = reelSpinSpeed * Time.deltaTime;
                if (reelLeft) reelLeft.Rotate(Vector3.up, delta, Space.Self);
                if (reelRight) reelRight.Rotate(Vector3.up, delta, Space.Self);
            }
        }

        private void OnStorySignal(string id)
        {
            // Auto-trigger when hidden room is revealed
            if (id == "hidden-room-found" && !_hasPlayed)
            {
                StartCoroutine(AutoTriggerWhenNear());
            }
        }

        private IEnumerator AutoTriggerWhenNear()
        {
            var player = FindAnyObjectByType<FirstPersonController>();
            if (!player) yield break;

            while (!_hasPlayed)
            {
                float dist = Vector3.Distance(transform.position, player.transform.position);
                if (dist <= triggerDistance)
                {
                    PlayRecording();
                    yield break;
                }
                yield return new WaitForSeconds(0.35f);
            }
        }

        public void Interact()
        {
            PlayRecording();
        }

        public void StopInteract() { }

        public void PlayRecording()
        {
            if (_isPlaying) return;

            StartCoroutine(PlaybackSequence());
        }

        private IEnumerator PlaybackSequence()
        {
            _isPlaying = true;
            _hasPlayed = true;
            _isSpinning = true;

            // Trigger flashlight flicker when approaching the tape deck
            var player = FindAnyObjectByType<FirstPersonController>();
            if (player)
            {
                var fl = player.GetComponentInChildren<FlashlightController>();
                if (fl) fl.TriggerFlicker(1.5f, 6);
            }

            if (_audioSource && recordingClip)
            {
                _audioSource.PlayOneShot(recordingClip, 1.0f);
            }

            var line = new DialogueLine("Harbor Board Recording", "Candidate Daniel Hale. Six souls lost on the Marguerite. Guilt confirmed. Assign to Harrow Rock. He will keep the leash.")
            {
                clip = recordingClip,
                duration = recordingClip != null ? recordingClip.length : 9.5f
            };
            GameEvents.RequestDialogue(line);
            GameEvents.RaiseStorySignal("marta-recording");

            float duration = recordingClip != null ? recordingClip.length : 9.5f;
            yield return new WaitForSeconds(duration);

            _isSpinning = false;
            _isPlaying = false;
        }

        private void FindReelsIfMissing()
        {
            if (!reelLeft)
            {
                var t = transform.Find("Magnetic Tape Reel Left");
                if (t) reelLeft = t;
            }
            if (!reelRight)
            {
                var t = transform.Find("Magnetic Tape Reel Right");
                if (t) reelRight = t;
            }
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
