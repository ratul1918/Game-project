using UnityEngine;
namespace TheLightHouse {
public sealed class MealTable : MonoBehaviour,IInteractable {
[SerializeField] string prompt="Inspect warm meal";
public string Prompt=>prompt;
public void Interact()=>GameEvents.RaiseStorySignal("meal");
public void StopInteract(){}
}}
