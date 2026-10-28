using UnityEngine;
namespace TheLightHouse
{
 public sealed class StormBeats:MonoBehaviour
 {
  [SerializeField] Transform player;
  [SerializeField] float lanternHeight=2.7f;
  bool eligible,requested;
  void OnEnable(){GameEvents.GameStateChanged+=State;GameEvents.StoryBeatTriggered+=Beat;}
  void OnDisable(){GameEvents.GameStateChanged-=State;GameEvents.StoryBeatTriggered-=Beat;}
  void Beat(StoryBeatTriggeredEvent e){if(e.Id=="A2-B05")eligible=true;}
  void State(GameStateChangedEvent e){if(e.Current==GameState.Storm&&!requested){requested=true;GameEvents.RaiseStorySignal("storm-start");}}
  void Update(){if(eligible&&!requested&&player.position.y>=lanternHeight){requested=true;GameEvents.RaiseStorySignal("storm-start");}}
 }
}
