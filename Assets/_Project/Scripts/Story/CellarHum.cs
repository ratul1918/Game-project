using UnityEngine;
namespace TheLightHouse {
[RequireComponent(typeof(AudioSource))]
public sealed class CellarHum : MonoBehaviour {
[SerializeField] AudioClip shanty;
[SerializeField] WetFootprints footprints;
[SerializeField] bool enabledForActTwo;
bool searching;bool played;
void OnEnable()=>GameEvents.StorySignal+=Signal;
void OnDisable()=>GameEvents.StorySignal-=Signal;
void Signal(string id){if(id=="find-fuel")searching=true;}
void OnTriggerEnter(Collider other){if(!enabledForActTwo||!searching||played||!other.GetComponent<FirstPersonController>())return;played=true;var audioSource=GetComponent<AudioSource>();audioSource.spatialBlend=1;if(shanty)audioSource.PlayOneShot(shanty);GameEvents.RequestDialogue(new("","(distant humming)"));footprints.Reveal();}
}}
