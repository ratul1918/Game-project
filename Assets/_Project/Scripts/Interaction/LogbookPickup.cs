using UnityEngine;

namespace TheLightHouse
{
    public sealed class LogbookPickup : MonoBehaviour, IInteractable
    {
        [SerializeField] LogbookPage page;
        [SerializeField] LogbookJournal journal;
        bool held;
        bool unlocked;
        public string Prompt => page && page.lockedUntilPhoto && !unlocked ? "The entry is sealed" : "Read logbook page";
        void OnEnable() => GameEvents.StoryBeatTriggered += Beat;
        void OnDisable() => GameEvents.StoryBeatTriggered -= Beat;
        void Beat(StoryBeatTriggeredEvent e) { if (e.Id == "A4-B05") { unlocked = true; journal?.UnlockFinalEntry(); } }
        public void Interact()
        {
            if (held || !page || !journal || (page.lockedUntilPhoto && !unlocked)) return;
            held = true; journal.CollectAndRead(page);
        }
        public void StopInteract() => held = false;
    }
}
