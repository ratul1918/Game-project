using System.Collections.Generic;
using UnityEngine;

namespace TheLightHouse
{
    public sealed class ActFourGate : MonoBehaviour
    {
        [SerializeField] Door cellarDoor;
        [SerializeField] ActFourSettings settings;
        readonly HashSet<string> read=new();
        bool opened;
        void OnEnable(){GameEvents.PageRead+=Page;GameEvents.StoryBeatTriggered+=Beat;}
        void OnDisable(){GameEvents.PageRead-=Page;GameEvents.StoryBeatTriggered-=Beat;}
        void Page(PageReadEvent e){read.Add(e.Id);TryOpen();}
        void Beat(StoryBeatTriggeredEvent e){if(e.Id=="A4-B02")Open();}
        void TryOpen(){if(settings&&read.Count>=settings.pagesForCellar)Open();}
        void Open(){if(opened)return;opened=true;cellarDoor?.SetLocked(false);GameEvents.RaiseStorySignal("cellar-unlocked");GameEvents.RaiseObjectiveChanged("Follow the signal");GameEvents.RequestDialogue(new("Daniel","The cellar lock released."));}
    }
}
