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
        [SerializeField] private float departureSeconds = 12;
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

        private void Awake()
        {
            if (!audioSource) audioSource = GetComponent<AudioSource>();
            if (!audioSource) audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1.0f; // 3D spatial audio at the dock
            audioSource.minDistance = 2.0f;
            audioSource.maxDistance = 15.0f;
            audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
            audioSource.playOnAwake = false;

            if (!line1Clip) line1Clip = LoadAudio("Voice_Boatman_Line1.wav");
            if (!line2Clip) line2Clip = LoadAudio("Voice_Boatman_Line2.wav");

            if (warnings != null && warnings.Length >= 4)
            {
                if (line1Clip) warnings[1].clip = line1Clip;
                if (line2Clip) warnings[3].clip = line2Clip;

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

        private IEnumerator Start()
        {
            if (finaleMode) yield break;

            if (player) player.MovementEnabled = false;
            GameEvents.RaiseStorySignal("arrival");

            foreach (var line in warnings)
            {
                GameEvents.RequestDialogue(line);
                if (line.clip != null && audioSource)
                {
                    audioSource.PlayOneShot(line.clip);
                }

                float waitTime = line.clip != null ? line.clip.length : line.duration;
                yield return new WaitForSeconds(waitTime + 0.3f);
            }

            yield return null;
            if (subtitles)
            {
                while (subtitles.Busy) yield return null;
            }

            GameEvents.RaiseStorySignal("dock");
            if (player) player.MovementEnabled = true;
            yield return MoveBoat();
        }

        public void PlayFinale(DialogueLine line)
        {
            if (!finaleMode) return;
            GameEvents.RequestDialogue(line);
            if (line.clip != null && audioSource)
            {
                audioSource.PlayOneShot(line.clip);
            }
            StartCoroutine(MoveBoat());
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
