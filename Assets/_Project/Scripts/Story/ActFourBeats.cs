using UnityEngine;

namespace TheLightHouse
{
    public sealed class ActFourBeats : MonoBehaviour
    {
        bool memoryShown;
        void OnEnable(){GameEvents.PageRead+=Page;GameEvents.StoryBeatTriggered+=Beat;}
        void OnDisable(){GameEvents.PageRead-=Page;GameEvents.StoryBeatTriggered-=Beat;}
        void Page(PageReadEvent e){if(!string.IsNullOrEmpty(e.StoryFlag))GameEvents.RaiseStorySignal(e.StoryFlag);}
        void Beat(StoryBeatTriggeredEvent e)
        {
            if(e.Id=="A4-B05"&&!memoryShown)
            {
                memoryShown=true;
                GameEvents.RequestDialogue(new("Daniel","He kept turning me toward the boat shed—away from the water."));
                GameEvents.RequestDialogue(new("","(nearby, Crane hums a lullaby)"));
            }
        }
    }
}
