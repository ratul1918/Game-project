using UnityEngine;

namespace TheLightHouse
{
    [RequireComponent(typeof(NoiseEmitter))]
    public sealed class HiddenCellarRoom : MonoBehaviour, IInteractable
    {
        [SerializeField] Transform shelf;
        [SerializeField] ActFourSettings settings;
        [SerializeField] string prompt="Slide the false shelf";
        Vector3 closed,open;
        bool held,revealed;
        public string Prompt=>revealed?"Hidden room revealed":prompt;
        void Awake(){shelf??=transform;closed=shelf.localPosition;open=closed+Vector3.right*(settings?settings.shelfTravel:1.8f);}
        void Update(){if(revealed)shelf.localPosition=Vector3.MoveTowards(shelf.localPosition,open,(settings?settings.shelfSpeed:1.2f)*Time.deltaTime);}
        public void Interact(){if(held||revealed)return;held=true;revealed=true;GetComponent<NoiseEmitter>().Emit(settings?settings.shelfNoise:10f);GameEvents.RaiseStorySignal("hidden-room-found");GameEvents.RaiseObjectiveChanged("Find the source of Marta's voice");}
        public void StopInteract()=>held=false;
    }
}
