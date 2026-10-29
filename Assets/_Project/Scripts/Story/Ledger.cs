using UnityEngine;

namespace TheLightHouse
{
    public sealed class Ledger : MonoBehaviour, IInteractable
    {
        [SerializeField] LogbookReaderUI reader;
        [SerializeField] string entryId="HarborBoardLedger";
        [SerializeField] string title="Harbor Board Keeper Ledger";
        [SerializeField,TextArea(8,16)] string entries="03 October 1911 — Daniel Hale — selected weeks before arrival. Carries sufficient guilt: abandoned six sailors in the storm.";
        bool held,read;
        public string Prompt=>"Read the harbor ledger";
        public void Interact(){if(held)return;held=true;reader?.Show(title,entries);if(read)return;read=true;GameEvents.RaiseLedgerRead(new(entryId));GameEvents.RaiseItemCollected(new(entryId));GameEvents.RaiseStorySignal("daniel-chosen");GameEvents.RaiseObjectiveChanged("Find the key to Crane's room");}
        public void StopInteract()=>held=false;
    }
}
