using UnityEngine;

namespace TheLightHouse
{
    [RequireComponent(typeof(NoiseEmitter))]
    [RequireComponent(typeof(AudioSource))]
    public sealed class Radio : MonoBehaviour, IInteractable
    {
        [SerializeField] private DialogueLine greeting = new("Marta", "Daniel... Daniel Hale. You made it before dark. Thomas left the lamp cold. You have to climb the tower and light it now. The fuel is in the cellar, Daniel.");
        [SerializeField] private DialogueLine[] reassurance = { new("Marta", "Questions after the lamp, Daniel. You have minutes.") };
        [SerializeField] private TheLightHouse.Voice.VoiceLine martaVoiceLine;
        [SerializeField] private float loudness = 7f;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip radioTransmissionClip;

        public TheLightHouse.Voice.VoiceLine MartaVoiceLine => martaVoiceLine;

        private bool available;
        private bool held;
        private int index;

        public string Prompt => "Operate radio";
        public bool RequiresHold => false;

        private void Awake()
        {
            if (!audioSource) audioSource = GetComponent<AudioSource>();
            if (!audioSource) audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1.0f; // 3D spatial sound placed on the radio cabinet speaker
            audioSource.minDistance = 1.5f;
            audioSource.maxDistance = 14.0f;
            audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
            audioSource.playOnAwake = false;

            if (!martaVoiceLine)
            {
#if UNITY_EDITOR
                martaVoiceLine = UnityEditor.AssetDatabase.LoadAssetAtPath<TheLightHouse.Voice.VoiceLine>("Assets/_Project/Audio/Voice/Lines/A1_MARTA_001.asset");
#endif
            }

            if (!radioTransmissionClip)
            {
                if (martaVoiceLine != null && martaVoiceLine.Clip != null) radioTransmissionClip = martaVoiceLine.Clip;
                else radioTransmissionClip = LoadAudio("Voice_Marta_RadioGreeting.wav");
            }
            if (greeting != null && radioTransmissionClip != null)
            {
                greeting.clip = radioTransmissionClip;
            }
        }

        private void OnEnable() => GameEvents.StoryBeatTriggered += Beat;
        private void OnDisable() => GameEvents.StoryBeatTriggered -= Beat;

        private void Beat(StoryBeatTriggeredEvent e)
        {
            if (e.Id != "A1-B04") return;
            available = true;
            PlayTransmission();
            GetComponent<NoiseEmitter>().Emit(loudness);
        }

        public void Interact()
        {
            if (held || !available) return;
            held = true;
            GameEvents.RaiseStorySignal("radio");

            if (index == 0)
            {
                PlayTransmission();
            }
            else if (reassurance.Length > 0)
            {
                GameEvents.RequestDialogue(reassurance[(index - 1) % reassurance.Length]);
            }
            index++;
        }

        public void StopInteract() => held = false;

        private void PlayTransmission()
        {
            GameEvents.RequestDialogue(greeting);
            if (audioSource && greeting.clip)
            {
                audioSource.PlayOneShot(greeting.clip);
            }
        }

        public void ReceiveServerMessage(string id, string text)
        {
            if (!string.IsNullOrEmpty(text)) GameEvents.RequestDialogue(new("Marta", text));
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
