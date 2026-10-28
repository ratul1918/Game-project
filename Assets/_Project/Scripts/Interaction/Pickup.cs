using UnityEngine;
namespace TheLightHouse {
public class Pickup : MonoBehaviour,IInteractable {
[SerializeField] string itemId="FuelCan";
[SerializeField] string prompt="Take fuel can";
bool taken;
public string Prompt=>prompt;
public void Interact(){if(taken)return;taken=true;GameEvents.RaiseItemCollected(new(itemId));gameObject.SetActive(false);}
public void StopInteract(){}
}}
