using System.Collections;
using UnityEngine;

namespace TheLightHouse
{
    public sealed class ShorelineEnding : MonoBehaviour
    {
        [SerializeField] FinaleSettings settings;
        [SerializeField] LullabyAsset lullaby;
        [SerializeField] FirstPersonController player;
        [SerializeField] Transform crane;
        [SerializeField] Transform waterPose;
        [SerializeField] Transform beam;
        bool available,played;
        void OnEnable()=>GameEvents.StoryBeatTriggered+=Beat;
        void OnDisable()=>GameEvents.StoryBeatTriggered-=Beat;
        void Beat(StoryBeatTriggeredEvent e){if(e.Id=="A5-B04")available=true;}
        void OnTriggerEnter(Collider other){if(available&&!played&&other.GetComponent<FirstPersonController>())StartCoroutine(Sequence());}
        IEnumerator Sequence()
        {
            played=true;player.SetMovementLocked(true);player.SetPeekLimits(settings?settings.shorelineLookYaw:30f,settings?settings.shorelineLookPitch:15f);
            GameEvents.RequestDialogue(new("Daniel","You kept me from the water."));
            GameEvents.RequestDialogue(new("","(Crane hums, steady now)"));
            if(lullaby)GameEvents.RequestDialogue(new DialogueLine("Crane",lullaby.finalLine){clip=lullaby.finalLineClip});
            Vector3 start=crane.position;float duration=settings?settings.shorelineSequenceDuration:9f;float t=0;
            while(t<duration){t+=Time.deltaTime;float p=t/duration;if(waterPose)crane.position=Vector3.Lerp(start,waterPose.position,p);if(beam)beam.Rotate(Vector3.up,(settings?settings.finalBeamSweepDegrees:360f)*Time.deltaTime/duration);yield return null;}
            if(crane)crane.gameObject.SetActive(false);GameEvents.RaiseStorySignal("last-line");player.SetPeekLimits(0,0);
        }
    }
}
