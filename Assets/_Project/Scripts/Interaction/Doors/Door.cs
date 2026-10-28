using UnityEngine;
namespace TheLightHouse {
[RequireComponent(typeof(NoiseEmitter))]
public sealed class Door : MonoBehaviour,IInteractable {
[SerializeField] string doorId="tower-front-door";
[SerializeField] bool locked;
[SerializeField] float openAngle=100;
[SerializeField] float turnSpeed=120;
[SerializeField] float loudness=5;
bool held; bool open;
Quaternion closedRotation;
public string Prompt=>locked?"Locked — try handle":open?"Close door":"Open door";
void Awake()=>closedRotation=transform.localRotation;
void Update()=>transform.localRotation=Quaternion.RotateTowards(transform.localRotation,closedRotation*Quaternion.Euler(0,open?openAngle:0,0),turnSpeed*Time.deltaTime);
public void Interact(){if(held)return;held=true;GetComponent<NoiseEmitter>().Emit(loudness);if(locked){GameEvents.RequestDialogue(new("Daniel","Locked."));return;}var previous=open?DoorState.Open:DoorState.Closed;open=!open;GameEvents.RaiseDoorStateChanged(new(doorId,previous,open?DoorState.Open:DoorState.Closed));}
public void StopInteract()=>held=false;
}}
