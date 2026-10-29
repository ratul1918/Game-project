using UnityEngine;

namespace TheLightHouse
{
    public sealed class ActFourEndCondition : MonoBehaviour
    {
        bool finished;
        void OnEnable()=>GameEvents.StoryBeatTriggered+=Beat;
        void OnDisable()=>GameEvents.StoryBeatTriggered-=Beat;
        void Beat(StoryBeatTriggeredEvent e)
        {
            if(finished||e.Id!="A4-B06")return;finished=true;
            GameEvents.RaiseObjectiveChanged("Hold the light until dawn");
            GameEvents.RequestDialogue(new("Daniel","He gave himself to the sea for me—and it would not let him rest."));
            GameEvents.RequestDialogue(new("","Act 5 is not built yet. The light must be held until dawn."));
        }
    }
}
