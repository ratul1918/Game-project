using System.Collections;
using UnityEngine;

namespace TheLightHouse
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class Boatman : MonoBehaviour
    {
        [SerializeField] private FirstPersonController player;
        [SerializeField] private SubtitleUI subtitles;
        [SerializeField] private Transform boat;
        [SerializeField] private Vector3 departureOffset = new(-5, 0, -35);
        [SerializeField] private float departureSeconds = 12f;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip line1Clip;
        [SerializeField] private AudioClip line2Clip;

        [SerializeField] private DialogueLine[] warnings =
        {
            new("Boatman", "Crane's missed three weeks of calls. Could be the set. Could be worse."),
            new("Boatman", "Light the lamp before dark. And if you hear singing—don't answer it."),
            new("Daniel", "When do you come back?"),
            new("Boatman", "Morning. If the light's on.")
        };

        [SerializeField] private bool finaleMode;
        private bool departed;

        private void Awake()
        {
            if (Time.timeScale <= 0f) Time.timeScale = 1.0f;
            if (!audioSource) audioSource = GetComponent<AudioSource>();
            if (!audioSource) audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1.0f; // 3D spatial audio at the dock
            audioSource.minDistance = 2.0f;
            audioSource.maxDistance = 18.0f;
            audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
            audioSource.playOnAwake = false;

            if (!player) player = FindAnyObjectByType<FirstPersonController>();
            if (!subtitles) subtitles = FindAnyObjectByType<SubtitleUI>() ?? SubtitleUI.Instance;
            if (!boat)
            {
                var boatGo = GameObject.Find("Boat");
                if (boatGo) boat = boatGo.transform;
            }

            if (!line1Clip) line1Clip = LoadAudio("Voice/Boatman/A1_BOAT_001.wav") ?? LoadAudio("Voice_Boatman_Line1.wav");
            if (!line2Clip) line2Clip = LoadAudio("Voice/Boatman/A1_BOAT_002.wav") ?? LoadAudio("Voice_Boatman_Line2.wav");

            if (warnings != null && warnings.Length >= 4)
            {
                if (line1Clip) warnings[0].clip = line1Clip;
                if (line2Clip) warnings[1].clip = line2Clip;
                var danielClip = LoadAudio("Voice/DanielHale/A1_DAN_BOAT_001.wav");
                if (danielClip) warnings[2].clip = danielClip;
                var boatmanLine3 = LoadAudio("Voice/Boatman/A1_BOAT_003.wav");
                if (boatmanLine3) warnings[3].clip = boatmanLine3;

#if UNITY_EDITOR
                var db = UnityEditor.AssetDatabase.LoadAssetAtPath<TheLightHouse.Voice.VoiceDatabase>("Assets/_Project/Audio/Voice/VoiceDatabase.asset");
                if (db)
                {
                    if (db.TryGetLine("A1_BOAT_001", out var b1) && b1.Clip) warnings[0].clip = b1.Clip;
                    if (db.TryGetLine("A1_BOAT_002", out var b2) && b2.Clip) warnings[1].clip = b2.Clip;
                    if (db.TryGetLine("A1_DAN_BOAT_001", out var d1) && d1.Clip) warnings[2].clip = d1.Clip;
                    if (db.TryGetLine("A1_BOAT_003", out var b3) && b3.Clip) warnings[3].clip = b3.Clip;
                }
#endif
            }
        }

        private void OnDisable()
        {
            // Unconditional recovery if component or object disabled mid-sequence
            ReleasePlayerControls();
        }

        private void OnDestroy()
        {
            ReleasePlayerControls();
        }

        private IEnumerator Start()
        {
            if (finaleMode) yield break;

            if (player)
            {
                player.MovementEnabled = false;
                player.SetMovementLocked(true);
            }
            GameEvents.RaiseStorySignal("arrival");

            // Dialogue runner with auto-advance timeout
            for (int i = 0; i < warnings.Length; i++)
            {
                yield return StartCoroutine(PlayDialogueLine(warnings[i]));
            }

            // Sequence Completion Guarantee:
            // Re-enable player movement and camera controls unconditionally
            ReleasePlayerControls();

            if (subtitles) subtitles.Hide();
            else if (SubtitleUI.Instance) SubtitleUI.Instance.Hide();

            GameEvents.RaiseStorySignal("dock");
            Depart();
        }

        public IEnumerator PlayDialogueLine(DialogueLine line)
        {
            if (line == null) yield break;

            if (subtitles != null)
            {
                subtitles.Show(line.speaker, line.text);
            }
            else if (SubtitleUI.Instance != null)
            {
                SubtitleUI.Instance.Show(line.speaker, line.text);
            }

            GameEvents.RequestDialogue(line);

            if (line.clip != null && audioSource != null)
            {
                audioSource.PlayOneShot(line.clip);
            }

            // Unscaled time advancement with player skip option (Space, E, Enter, LMB)
            float duration = line.clip != null ? line.clip.length + 0.5f : Mathf.Max(2.5f, line.text.Length * 0.06f);
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                if (UnityEngine.InputSystem.Keyboard.current != null &&
                    (UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame ||
                     UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame ||
                     UnityEngine.InputSystem.Keyboard.current.enterKey.wasPressedThisFrame))
                {
                    break;
                }
                if (UnityEngine.InputSystem.Mouse.current != null &&
                    UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame)
                {
                    break;
                }
                yield return null;
            }
        }

        private void ReleasePlayerControls()
        {
            if (player != null)
            {
                player.enabled = true;
                player.MovementEnabled = true;
                player.SetMovementLocked(false);
                player.SetLookLocked(false);
            }

            var characterController = player != null ? player.GetComponent<CharacterController>() : null;
            if (characterController != null)
            {
                characterController.enabled = true;
            }
        }

        public void Depart()
        {
            if (departed) return;
            departed = true;
            StopCoroutine(nameof(MoveBoat));
            StartCoroutine(MoveBoat());
        }

        public void PlayFinale(DialogueLine line)
        {
            if (!finaleMode) return;
            GameEvents.RequestDialogue(line);
            if (line.clip != null && audioSource)
            {
                audioSource.PlayOneShot(line.clip);
            }
            Depart();
        }

        private IEnumerator MoveBoat()
        {
            if (!boat) yield break;
            Vector3 start = boat.position;
            float elapsed = 0;
            while (elapsed < departureSeconds)
            {
                elapsed += Time.deltaTime;
                boat.position = Vector3.Lerp(start, start + departureOffset, elapsed / departureSeconds);
                yield return null;
            }
        }

        private static AudioClip LoadAudio(string relativePath)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>($"Assets/_Project/Audio/{relativePath}");
#else
            return null;
#endif
        }
    }
}
