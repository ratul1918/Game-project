using UnityEngine;
using UnityEngine.Events;
namespace TheLightHouse {
public enum StoryTrigger { Signal, DoorOpened, LampLit }
[CreateAssetMenu(menuName="TheLightHouse/Story Beat")]
public sealed class StoryBeat : ScriptableObject {
public string id;
public StoryTrigger trigger;
public string triggerId;
public string objective;
public DialogueLine[] lines;
public UnityEvent optionalAction=new();
}}
