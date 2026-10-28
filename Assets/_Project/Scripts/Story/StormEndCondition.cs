using UnityEngine;
namespace TheLightHouse
{
 public sealed class StormEndCondition:MonoBehaviour
 {
  [SerializeField] StormSettings settings;
  float total,charge;bool active,ready,finished;
  void OnEnable(){GameEvents.StoryBeatTriggered+=Beat;GameEvents.LampChargeChanged+=Charge;}
  void OnDisable(){GameEvents.StoryBeatTriggered-=Beat;GameEvents.LampChargeChanged-=Charge;}
  void Charge(float value)=>charge=value;
  void Beat(StoryBeatTriggeredEvent e){if(e.Id=="A3-B02")active=true;if(e.Id=="A3-B04")ready=true;}
  void Update(){if(!active||finished)return;if(charge>=settings.healthyThreshold)total+=Time.deltaTime;if(ready&&total>=settings.stormHealthyTime){finished=true;GameEvents.RaiseStorySignal("storm-end");GameEvents.RequestDialogue(new("Daniel","The storm is easing. Crane's logbook might have answers."));}}
 }
}
