using UnityEngine;

namespace TheLightHouse
{
    public sealed class KeyItem : MonoBehaviour, IInteractable
    {
        [SerializeField] string itemId="CraneRoomKey";
        [SerializeField] string prompt="Take Crane's room key";
        bool available,held,taken;
        public string Prompt=>available?prompt:"Nothing useful here yet";
        void OnEnable()=>GameEvents.StoryBeatTriggered+=Beat;
        void OnDisable()=>GameEvents.StoryBeatTriggered-=Beat;
        void Beat(StoryBeatTriggeredEvent e){if(e.Id=="A4-B04")available=true;}
        public void Interact(){if(held||taken||!available)return;held=true;taken=true;GameEvents.RaiseItemCollected(new(itemId));GameEvents.RaiseStorySignal("crane-key-found");GameEvents.RaiseObjectiveChanged("Open Crane's room");gameObject.SetActive(false);}
        public void StopInteract()=>held=false;
    }
}
