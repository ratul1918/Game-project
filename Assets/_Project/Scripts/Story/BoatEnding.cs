using UnityEngine;

namespace TheLightHouse
{
    public sealed class BoatEnding : MonoBehaviour,IInteractable
    {
        [SerializeField] LogbookReaderUI reader;
        [SerializeField] Boatman boatman;
        [SerializeField] string nextKeeperName="Elias Ward";
        [SerializeField] string ledgerTitle="Harbor Board Keeper Ledger";
        bool available,held,finished;
        public string Prompt=>available?"Board the morning boat":"The boat has not arrived";
        void OnEnable()=>GameEvents.StoryBeatTriggered+=Beat;
        void OnDisable()=>GameEvents.StoryBeatTriggered-=Beat;
        void Beat(StoryBeatTriggeredEvent e){if(e.Id=="A5-B05")available=true;}
        public void Interact()
        {
            if(held||!available||finished)return;held=true;finished=true;
            GameEvents.RequestDialogue(new("Boatman","Light stayed on. That's enough for me."));
            reader?.Show(ledgerTitle,$"~~ Daniel Hale ~~\n\nMarta's hand: Next keeper — {nextKeeperName}");
            boatman?.PlayFinale(new("Boatman","Sit down. We leave Harrow Rock behind."));
            GameEvents.RaiseLedgerEndingRead();GameEvents.RaiseStorySignal("next-keeper");
        }
        public void StopInteract()=>held=false;
    }
}
