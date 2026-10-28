using UnityEngine;
namespace TheLightHouse {
public sealed class StoryDirector : MonoBehaviour {
[SerializeField] StoryBeat[] beats;
int next;
void OnEnable(){GameEvents.StorySignal+=Signal;GameEvents.DoorStateChanged+=Door;GameEvents.LampStateChanged+=Lamp;}
void OnDisable(){GameEvents.StorySignal-=Signal;GameEvents.DoorStateChanged-=Door;GameEvents.LampStateChanged-=Lamp;}
void Signal(string id)=>TryTrigger(StoryTrigger.Signal,id);
void Door(DoorStateChangedEvent e){if(e.Current==DoorState.Open)TryTrigger(StoryTrigger.DoorOpened,e.DoorId);}
void Lamp(LampStateChangedEvent e){if(e.Current==LampState.FuelLit)TryTrigger(StoryTrigger.LampLit,"");}
void TryTrigger(StoryTrigger type,string id){
if(beats==null || next>=beats.Length)return;
var beat=beats[next]; if(beat.trigger!=type || beat.triggerId!=id)return;
next++; Debug.Log("Story beat: "+beat.id,this);
GameEvents.RaiseStoryBeatTriggered(new(beat.id));
if(!string.IsNullOrEmpty(beat.objective))GameEvents.RaiseObjectiveChanged(beat.objective);
if(beat.lines!=null)foreach(var line in beat.lines)GameEvents.RequestDialogue(line);
beat.optionalAction?.Invoke();
}
}}
