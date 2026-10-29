using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TheLightHouse
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class RadioSignalTracker : MonoBehaviour, IInteractable
    {
        [SerializeField] Transform player;
        [SerializeField] Transform transmitter;
        [SerializeField] InputActionAsset inputActions;
        [SerializeField] ActFourSettings settings;
        [SerializeField] Slider needle;
        [SerializeField] AudioClip staticLoop;
        [SerializeField, Range(0f,1f)] float targetFrequency=.68f;
        [SerializeField, Range(0f,1f)] float startingFrequency=.2f;
        [SerializeField] float minimumPitch=.75f;
        [SerializeField] float maximumPitch=1.35f;
        [SerializeField, Range(0f,1f)] float maximumVolume=.55f;
        InputAction tuneLeft,tuneRight,scroll;
        AudioSource source;
        float frequency;
        bool active,held,reported;
        public string Prompt=>active?"Stop tuning":"Tune Marta's radio";

        void Awake()
        {
            source=GetComponent<AudioSource>();frequency=startingFrequency;
            var playerMap=inputActions?.FindActionMap("Player");tuneLeft=playerMap?.FindAction("TuneLeft");tuneRight=playerMap?.FindAction("TuneRight");
            scroll=inputActions?.FindActionMap("UI")?.FindAction("ScrollWheel");
            source.loop=true;source.clip=staticLoop;source.spatialBlend=0f;
            if(needle)needle.gameObject.SetActive(false);
        }
        void OnEnable(){tuneLeft?.Enable();tuneRight?.Enable();scroll?.Enable();}
        void OnDisable(){tuneLeft?.Disable();tuneRight?.Disable();scroll?.Disable();source?.Stop();}
        void Update()
        {
            if(!active||!player||!transmitter||!settings)return;
            float axis=(tuneRight?.ReadValue<float>()??0f)-(tuneLeft?.ReadValue<float>()??0f);
            float wheel=scroll?.ReadValue<Vector2>().y??0f;
            frequency=Mathf.Clamp01(frequency+axis*settings.tuningSpeed*Time.deltaTime+Mathf.Sign(wheel)*settings.tuningWheelStep);
            float tuning=Mathf.Clamp01(1f-Mathf.Abs(frequency-targetFrequency)/Mathf.Max(.001f,settings.tuningTolerance));
            float proximity=1f-Mathf.Clamp01(Vector3.Distance(player.position,transmitter.position)/settings.signalRange);
            float strength=tuning*proximity;
            if(needle)needle.value=strength;
            source.volume=maximumVolume*strength;source.pitch=Mathf.Lerp(minimumPitch,maximumPitch,strength);
            if(!reported&&tuning>.9f){reported=true;GameEvents.RaiseStorySignal("signal-tuned");GameEvents.RaiseObjectiveChanged("Follow the signal into the cellar");}
        }
        public void Interact(){if(held)return;held=true;active=!active;if(needle)needle.gameObject.SetActive(active);if(active&&staticLoop)source.Play();else source.Stop();}
        public void StopInteract()=>held=false;
    }
}
