using UnityEngine;

namespace TheLightHouse
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class RecordedTransmitter : MonoBehaviour, IInteractable
    {
        [SerializeField] AudioClip loopingGreeting;
        [SerializeField] float repeatInterval=6f;
        [SerializeField] float radioNoise=7f;
        bool held,revealed;
        float nextRepeat;
        AudioSource source;
        public string Prompt=>revealed?"The recorder is stopped":"Stop the recorder";
        void Awake(){source=GetComponent<AudioSource>();source.loop=true;source.clip=loopingGreeting;source.spatialBlend=1f;}
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
