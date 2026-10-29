using System.Collections;
using UnityEngine;
namespace TheLightHouse {
public sealed class Boatman : MonoBehaviour {
[SerializeField] FirstPersonController player;
[SerializeField] SubtitleUI subtitles;
[SerializeField] Transform boat;
[SerializeField] Vector3 departureOffset=new(-30,0,0);
[SerializeField] float departureSeconds=12;
[SerializeField] DialogueLine[] warnings={new("Boatman","Crane's missed three weeks of calls. Could be the set. Could be worse."),new("Boatman","Light the lamp before dark. And if you hear singing—don't answer it."),new("Daniel","When do you come back?"),new("Boatman","Morning. If the light's on.")};
[SerializeField] bool finaleMode;
IEnumerator Start(){if(finaleMode)yield break;player.MovementEnabled=false;GameEvents.RaiseStorySignal("arrival");foreach(var line in warnings)GameEvents.RequestDialogue(line);yield return null;while(subtitles.Busy)yield return null;GameEvents.RaiseStorySignal("dock");player.MovementEnabled=true;yield return MoveBoat();}
public void PlayFinale(DialogueLine line){if(!finaleMode)return;GameEvents.RequestDialogue(line);StartCoroutine(MoveBoat());}
IEnumerator MoveBoat(){Vector3 start=boat.position;float elapsed=0;while(elapsed<departureSeconds){elapsed+=Time.deltaTime;boat.position=Vector3.Lerp(start,start+departureOffset,elapsed/departureSeconds);yield return null;}}
}}
