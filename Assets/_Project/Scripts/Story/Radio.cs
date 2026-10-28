using UnityEngine;
namespace TheLightHouse {
[RequireComponent(typeof(NoiseEmitter))]
public sealed class Radio : MonoBehaviour,IInteractable {
[SerializeField] DialogueLine greeting=new("Marta","Daniel Hale. You made good time.");
[SerializeField] DialogueLine[] reassurance={new("Marta","Questions after the lamp, Daniel. You have minutes.")};
[SerializeField] float loudness=7;
bool available;bool held;int index;
public string Prompt=>"Operate radio";
void OnEnable()=>GameEvents.StoryBeatTriggered+=Beat;
void OnDisable()=>GameEvents.StoryBeatTriggered-=Beat;
void Beat(StoryBeatTriggeredEvent e){if(e.Id!="A1-B04")return;available=true;GameEvents.RequestDialogue(greeting);GetComponent<NoiseEmitter>().Emit(loudness);}
public void Interact(){if(held||!available)return;held=true;GameEvents.RaiseStorySignal("radio");if(index>0 && reassurance.Length>0)GameEvents.RequestDialogue(reassurance[(index-1)%reassurance.Length]);index++;}
public void StopInteract()=>held=false;
}}
