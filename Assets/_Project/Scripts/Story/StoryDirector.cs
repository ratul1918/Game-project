using UnityEngine;
namespace TheLightHouse {
public sealed class StoryDirector : MonoBehaviour {
[SerializeField] StoryBeat[] beats;
[SerializeField] bool allowPendingActTwoSignals;
[SerializeField] StoryBeat[] optionalBeats;
readonly System.Collections.Generic.HashSet<string> completed=new();
readonly System.Collections.Generic.HashSet<string> pending = new();
int next;
void OnEnable(){GameEvents.StorySignal+=Signal;GameEvents.DoorStateChanged+=Door;GameEvents.LampStateChanged+=Lamp;}
void OnDisable(){GameEvents.StorySignal-=Signal;GameEvents.DoorStateChanged-=Door;GameEvents.LampStateChanged-=Lamp;}
void Signal(string id){if(allowPendingActTwoSignals)pending.Add(id);TryTrigger(StoryTrigger.Signal,id);CheckOptional();}
void CheckOptional(){if(optionalBeats==null)return;foreach(var beat in optionalBeats){if(!beat||completed.Contains(beat.id)||!pending.Contains(beat.triggerId)||(!string.IsNullOrEmpty(beat.prerequisiteId)&&!completed.Contains(beat.prerequisiteId)))continue;Publish(beat);}}
void Publish(StoryBeat beat){completed.Add(beat.id);Debug.Log("Story beat: "+beat.id,this);GameEvents.RaiseStoryBeatTriggered(new(beat.id));if(!string.IsNullOrEmpty(beat.objective))GameEvents.RaiseObjectiveChanged(beat.objective);if(beat.lines!=null)foreach(var line in beat.lines)GameEvents.RequestDialogue(line);beat.optionalAction?.Invoke();}
void Door(DoorStateChangedEvent e){if(e.Current==DoorState.Open)TryTrigger(StoryTrigger.DoorOpened,e.DoorId);}
void Lamp(LampStateChangedEvent e){if(e.Current==LampState.FuelLit)TryTrigger(StoryTrigger.LampLit,"");}
void TryTrigger(StoryTrigger type,string id){
if(beats==null || next>=beats.Length)return;
var beat=beats[next]; if(beat.trigger!=type || beat.triggerId!=id)return;
next++; Publish(beat);
if(allowPendingActTwoSignals && next<beats.Length && beats[next].trigger==StoryTrigger.Signal && pending.Remove(beats[next].triggerId))
TryTrigger(StoryTrigger.Signal,beats[next].triggerId);
}
}}
