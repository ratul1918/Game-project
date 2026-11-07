using System.Collections;
using UnityEngine;

namespace TheLightHouse
{
    public sealed class ShorelineEnding : MonoBehaviour
    {
        [SerializeField] private FinaleSettings settings;
        [SerializeField] private LullabyAsset lullaby;
        [SerializeField] private FirstPersonController player;
        [SerializeField] private Transform crane;
        [SerializeField] private Transform waterPose;
        [SerializeField] private Transform beam;
        [SerializeField] private AudioSource craneAudioSource;
        [SerializeField] private AudioClip farewellClip;

        private bool available;
        private bool played;

        private void Awake()
        {
            if (crane && !craneAudioSource)
            {
                craneAudioSource = crane.GetComponent<AudioSource>() ?? crane.gameObject.AddComponent<AudioSource>();
                craneAudioSource.spatialBlend = 1.0f; // 3D spatial on Crane
                craneAudioSource.minDistance = 2.0f;
                craneAudioSource.maxDistance = 25.0f;
                craneAudioSource.rolloffMode = AudioRolloffMode.Logarithmic;
                craneAudioSource.playOnAwake = false;
            }

            if (!farewellClip) farewellClip = LoadAudio("Voice_Crane_ShorelineFarewell.wav");

            if (lullaby)
            {
                lullaby.finalLine = "Time for us to leave her...";
                if (farewellClip) lullaby.finalLineClip = farewellClip;
            }
        }

        private void OnEnable() => GameEvents.StoryBeatTriggered += Beat;
        private void OnDisable() => GameEvents.StoryBeatTriggered -= Beat;

        private void Beat(StoryBeatTriggeredEvent e)
        {
            if (e.Id == "A5-B04") available = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (available && !played && other.GetComponent<FirstPersonController>())
            {
                StartCoroutine(Sequence());
            }
        }

        private IEnumerator Sequence()
        {
            played = true;
            if (player)
            {
                player.SetMovementLocked(true);
                player.SetPeekLimits(settings ? settings.shorelineLookYaw : 30f, settings ? settings.shorelineLookPitch : 15f);
            }

            GameEvents.RequestDialogue(new("Daniel", "You kept me from the water."));
            GameEvents.RequestDialogue(new("", "(Crane hums, steady now)"));

            AudioClip clipToPlay = (lullaby && lullaby.finalLineClip) ? lullaby.finalLineClip : farewellClip;
            string speechText = (lullaby && !string.IsNullOrEmpty(lullaby.finalLine)) ? lullaby.finalLine : "Time for us to leave her...";

            var farewellLine = new DialogueLine("Thomas Crane", speechText)
            {
                clip = clipToPlay,
                duration = clipToPlay != null ? clipToPlay.length : 5.0f
            };
            GameEvents.RequestDialogue(farewellLine);

            if (craneAudioSource && clipToPlay)
            {
                craneAudioSource.PlayOneShot(clipToPlay);
            }

            Vector3 start = crane ? crane.position : Vector3.zero;
            float duration = settings ? settings.shorelineSequenceDuration : 9f;
            float t = 0;

            while (t < duration)
            {
                t += Time.deltaTime;
                float p = t / duration;
                if (waterPose && crane) crane.position = Vector3.Lerp(start, waterPose.position, p);
                if (beam) beam.Rotate(Vector3.up, (settings ? settings.finalBeamSweepDegrees : 360f) * Time.deltaTime / duration);
                yield return null;
            }

            if (crane) crane.gameObject.SetActive(false);
            GameEvents.RaiseStorySignal("last-line");
            if (player) player.SetPeekLimits(0, 0);
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
