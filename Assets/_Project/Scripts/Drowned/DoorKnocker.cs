using System.Collections;
using UnityEngine;
namespace TheLightHouse
{
    [RequireComponent(typeof(NoiseEmitter),typeof(AudioSource))]
    public sealed class DoorKnocker : MonoBehaviour
    {
        [SerializeField] float[] intervals={3,2,1,1,.5f};
        [SerializeField] float duration=25;
        [SerializeField] float loudness=10;
        [SerializeField] AudioClip knock;
        [SerializeField] float subtitleDuration=.8f;
        DrownedFigure figure;bool running;float finishAt;
        void Awake(){var source=GetComponent<AudioSource>();source.spatialBlend=1;source.playOnAwake=false;}
        void OnEnable(){GameEvents.FigureAtDoor+=Begin;GameEvents.FrontDoorTried+=Tried;GameEvents.LampStateChanged+=Lamp;}
        void OnDisable(){GameEvents.FigureAtDoor-=Begin;GameEvents.FrontDoorTried-=Tried;GameEvents.LampStateChanged-=Lamp;StopAllCoroutines();}
        void Begin(FigureAtDoorEvent value){if(running)return;figure=value.Figure;running=true;finishAt=Time.time+duration;GameEvents.RaiseStorySignal("knocking");StartCoroutine(Knock());}
        void Lamp(LampStateChangedEvent e){if(e.Current==LampState.FuelLit||e.Current==LampState.CrankPowered)running=false;}
        void Tried(){GameEvents.RequestDialogue(new("Daniel","Don't open it."));if(figure)figure.StepBack();}
        IEnumerator Knock(){int index=0;while(running&&Time.time<finishAt){GetComponent<NoiseEmitter>().Emit(loudness);if(knock)GetComponent<AudioSource>().PlayOneShot(knock);if(index==0)GameEvents.RequestDialogue(new("","(knocking)"){duration=subtitleDuration});yield return new WaitForSeconds(intervals[Mathf.Min(index++,intervals.Length-1)]);}running=false;}
        public bool IsKnocking=>running;
    }
}
