using UnityEngine;
namespace TheLightHouse {
public sealed class LanternInteract : MonoBehaviour,IInteractable {
[SerializeField] LampController lamp;
[SerializeField] Inventory inventory;
[SerializeField] string fuelItem="FuelCan";
[SerializeField] float fuelAmount=60;
[SerializeField] string fuelObjective="Find fuel in the boat shed";
bool held;bool inspected;
public string Prompt=>"Inspect / fuel / light lamp";
public void Interact(){if(held)return;held=true;if(!inspected){inspected=true;GameEvents.RequestDialogue(new("Daniel","Warm."));}if(inventory.Consume(fuelItem))lamp.AddFuel(fuelAmount);
if(!lamp.TryIgnite()){GameEvents.RequestDialogue(new("Daniel","Not enough fuel."));GameEvents.RaiseObjectiveChanged(fuelObjective);GameEvents.RaiseStorySignal("find-fuel");}}
public void StopInteract()=>held=false;
}}
