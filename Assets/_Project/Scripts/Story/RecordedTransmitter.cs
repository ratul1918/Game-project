using UnityEngine;

namespace TheLightHouse
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class RecordedTransmitter : MonoBehaviour, IInteractable
    {
        [SerializeField] private TheLightHouse.Voice.VoiceLine martaVoiceLine;
        [SerializeField] private AudioClip loopingGreeting;
        [SerializeField] private float repeatInterval = 6f;
        [SerializeField] private float radioNoise = 7f;
        private bool held, revealed;
        private float nextRepeat;
        private AudioSource source;

        public TheLightHouse.Voice.VoiceLine MartaVoiceLine => martaVoiceLine;
        public string Prompt => revealed ? "The recorder is stopped" : "Stop the recorder";
        public bool RequiresHold => false;

        private void Awake()
        {
            source = GetComponent<AudioSource>();
            if (!martaVoiceLine)
            {
#if UNITY_EDITOR
                martaVoiceLine = UnityEditor.AssetDatabase.LoadAssetAtPath<TheLightHouse.Voice.VoiceLine>("Assets/_Project/Audio/Voice/Lines/A1_MARTA_001.asset");
#endif
            }

            if (martaVoiceLine != null && martaVoiceLine.Clip != null)
            {
                loopingGreeting = martaVoiceLine.Clip;
                repeatInterval = Mathf.Max(3f, martaVoiceLine.GetEffectiveDuration() + 0.5f);
            }

            source.loop = true;
            source.clip = loopingGreeting;
            source.spatialBlend = 1f;
        }
        void Start(){if(loopingGreeting)source.Play();nextRepeat=Time.time;}
        void Update(){if(revealed||Time.time<nextRepeat)return;nextRepeat=Time.time+repeatInterval;GameEvents.RequestDialogue(new("Marta (recording)","Daniel Hale. You made good time."));GameEvents.RaiseNoiseEmitted(new(transform.position,radioNoise,gameObject));}
        public void Interact()
        {
            if(held||revealed)return;held=true;revealed=true;source.Stop();
            GameEvents.RequestDialogue(new("Marta (recording)","Daniel Hale. You made good time."));
            GameEvents.RequestDialogue(new("Daniel","The exact same line. It was recorded."));
            GameEvents.RaiseStorySignal("marta-recording");GameEvents.RaiseObjectiveChanged("Read the harbor ledger");
        }
        public void StopInteract()=>held=false;
    }
}
