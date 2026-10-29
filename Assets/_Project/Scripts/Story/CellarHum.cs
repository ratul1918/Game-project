using UnityEngine;
namespace TheLightHouse {
[RequireComponent(typeof(AudioSource))]
public sealed class CellarHum : MonoBehaviour {
[SerializeField] AudioClip shanty;
[SerializeField] WetFootprints footprints;
[SerializeField] bool enabledForActTwo;
bool searching;bool played;
void OnEnable(){GameEvents.StorySignal+=Signal;GameEvents.StoryBeatTriggered+=Beat;}
void OnDisable(){GameEvents.StorySignal-=Signal;GameEvents.StoryBeatTriggered-=Beat;}
void Beat(StoryBeatTriggeredEvent e){if(e.Id=="A2-B01"||e.Id=="A4-B02")searching=true;}
void Signal(string id){if(id=="find-fuel")searching=true;}
void OnTriggerStay(Collider other){if(!enabledForActTwo||!searching||played||!other.GetComponent<FirstPersonController>())return;played=true;var audioSource=GetComponent<AudioSource>();audioSource.spatialBlend=1;if(shanty)audioSource.PlayOneShot(shanty);GameEvents.RequestDialogue(new("","(distant humming)"));GameEvents.RaiseStorySignal("cellar-hum");footprints.Reveal();}
}}
