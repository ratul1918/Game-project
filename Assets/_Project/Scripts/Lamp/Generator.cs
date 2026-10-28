using System.Collections;
using UnityEngine;
namespace TheLightHouse
{
 [RequireComponent(typeof(NoiseEmitter),typeof(AudioSource))]
 public sealed class Generator:MonoBehaviour,IInteractable
 {
  [SerializeField] StormSettings settings;
  [SerializeField] Light[] electricLights;
  [SerializeField] AudioClip sputter;
  bool dead,held;
  void Awake(){GetComponent<AudioSource>().spatialBlend=1;GetComponent<AudioSource>().playOnAwake=false;}
  public string Prompt=>dead?"Try generator restart":"Inspect generator";
  void OnEnable()=>GameEvents.StoryBeatTriggered+=Beat;
  void OnDisable()=>GameEvents.StoryBeatTriggered-=Beat;
  void Beat(StoryBeatTriggeredEvent e){if(e.Id=="A3-B01")StartCoroutine(Fail());}
  IEnumerator Fail(){yield return new WaitForSeconds(settings.generatorDelay);if(sputter)GetComponent<AudioSource>().PlayOneShot(sputter);for(int i=0;i<settings.generatorFlickers;i++){foreach(var bulb in electricLights)if(bulb)bulb.enabled=false;yield return new WaitForSeconds(settings.flickerInterval);foreach(var bulb in electricLights)if(bulb)bulb.enabled=true;yield return new WaitForSeconds(settings.flickerInterval);}foreach(var light in electricLights)if(light)light.enabled=false;dead=true;GetComponent<NoiseEmitter>().Emit(settings.generatorNoise);GameEvents.RequestDialogue(new("","(generator sputters and dies)"));GameEvents.RaiseStorySignal("generator-dies");}
  public void Interact(){if(held)return;held=true;if(!dead){GameEvents.RequestDialogue(new("Daniel","The generator is struggling."));return;}GameEvents.RequestDialogue(new("Daniel","Flooded. It won't start."));GetComponent<NoiseEmitter>().Emit(settings.restartNoise);GameEvents.RaiseStorySignal("generator-restart-fails");}
  public void StopInteract()=>held=false;
 }
}
