using System.Collections;
using UnityEngine;

namespace TheLightHouse
{
    public sealed class DoorwayBlock : MonoBehaviour
    {
        [SerializeField] FinaleSettings settings;
        [SerializeField] Transform crane;
        [SerializeField] Transform blockPose;
        [SerializeField] Transform stepAsidePose;
        [SerializeField] CatchSequence catchSequence;
        void OnEnable()=>GameEvents.StoryBeatTriggered+=Beat;
        void OnDisable()=>GameEvents.StoryBeatTriggered-=Beat;
        void Beat(StoryBeatTriggeredEvent e){if(e.Id=="A5-B04")StartCoroutine(Block());}
        IEnumerator Block()
        {
            if(catchSequence)catchSequence.enabled=false;GameEvents.RaiseObjectiveChanged("Get to the dock");
            if(crane&&blockPose)crane.SetPositionAndRotation(blockPose.position,blockPose.rotation);
            GameEvents.RequestDialogue(new("","Crane braces himself in the doorway."));
            yield return new WaitForSeconds(settings?settings.doorwayBlockDuration:4f);
            if(crane&&stepAsidePose)crane.SetPositionAndRotation(stepAsidePose.position,stepAsidePose.rotation);
        }
    }
}
