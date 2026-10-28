using System.Collections;
using UnityEngine;
namespace TheLightHouse
{
 [RequireComponent(typeof(AudioSource))]
 public sealed class StormController:MonoBehaviour
 {
  [SerializeField] StormSettings settings;
  [SerializeField] ParticleSystem rain;
  [SerializeField] WindZone wind;
  [SerializeField] Light skyLight;
  [SerializeField] AccessibilityOptions accessibility;
  [SerializeField] AudioClip thunder;
  bool active;float started,baseIntensity;
  void OnEnable(){GameEvents.GameStateChanged+=State;GameEvents.StoryBeatTriggered+=Beat;}
  void OnDisable(){GameEvents.GameStateChanged-=State;GameEvents.StoryBeatTriggered-=Beat;StopAllCoroutines();if(skyLight)skyLight.intensity=baseIntensity;}
  void Awake(){baseIntensity=skyLight.intensity;GetComponent<AudioSource>().playOnAwake=false;}
  void State(GameStateChangedEvent e){if(e.Current!=GameState.Storm||active)return;active=true;started=Time.time;rain.Play();StartCoroutine(Flashes());}
  void Beat(StoryBeatTriggeredEvent e){if(e.Id=="A3-B09"){active=false;rain.Stop();if(wind)wind.windMain=0;}}
  void Update(){if(!active)return;float t=Mathf.Clamp01((Time.time-started)/Mathf.Max(.01f,settings.rampDuration));var emission=rain.emission;emission.rateOverTime=settings.rainRate*t*(accessibility.Reduced?settings.rainReduction:1);wind.windMain=settings.windStrength*t;}
  IEnumerator Flashes(){while(active){yield return new WaitForSeconds(Random.Range(settings.lightningInterval.x,settings.lightningInterval.y));if(!active)yield break;skyLight.intensity=baseIntensity+(accessibility.Reduced?settings.safeFlashIntensity:settings.flashIntensity);yield return new WaitForSeconds(settings.flashDuration);skyLight.intensity=baseIntensity;yield return new WaitForSeconds(settings.thunderDelay);if(!active)yield break;if(thunder)GetComponent<AudioSource>().PlayOneShot(thunder);GameEvents.RequestDialogue(new("","(thunder)"){duration=1});GameEvents.RaiseThunder();}}
 }
}
