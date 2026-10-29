using UnityEngine;
namespace TheLightHouse {
[RequireComponent(typeof(NoiseEmitter))]
public sealed class Door : MonoBehaviour,IInteractable {
[SerializeField] string doorId="tower-front-door";
[SerializeField] bool locked;
[SerializeField] float openAngle=100;
[SerializeField] float turnSpeed=120;
[SerializeField] float loudness=5;
[SerializeField] bool blockDuringActTwo;
[SerializeField] AudioClip rattle;
bool held; bool open;
Quaternion closedRotation;
public string Prompt=>locked?"Locked — try handle":open?"Close door":"Open door";
public bool IsLocked=>locked;
public void SetLocked(bool value)
{
 if(locked==value)return;
 var previous=locked?DoorState.Locked:(open?DoorState.Open:DoorState.Closed);
 locked=value;
 GameEvents.RaiseDoorStateChanged(new(doorId,previous,locked?DoorState.Locked:(open?DoorState.Open:DoorState.Closed)));
}
void Awake()=>closedRotation=transform.localRotation;
void Update()=>transform.localRotation=Quaternion.RotateTowards(transform.localRotation,closedRotation*Quaternion.Euler(0,open?openAngle:0,0),turnSpeed*Time.deltaTime);
public void Interact(){if(held)return;held=true;if(blockDuringActTwo){GameEvents.RaiseFrontDoorTried();return;}GetComponent<NoiseEmitter>().Emit(loudness);if(locked){if(rattle&&TryGetComponent<AudioSource>(out var source))source.PlayOneShot(rattle);GameEvents.RequestDialogue(new("","(door rattles softly)"));return;}var previous=open?DoorState.Open:DoorState.Closed;open=!open;GameEvents.RaiseDoorStateChanged(new(doorId,previous,open?DoorState.Open:DoorState.Closed));}
public void StopInteract()=>held=false;
}}
