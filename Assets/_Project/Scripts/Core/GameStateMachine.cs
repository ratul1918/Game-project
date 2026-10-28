using UnityEngine;
namespace TheLightHouse {
public enum GameState { Intro, Dusk, Night, Storm, Finale, Dawn, Ending }
public sealed class GameStateMachine : MonoBehaviour {
[SerializeField] bool startAtNight;
[SerializeField] bool startAtStorm;
public GameState Current { get; private set; } = GameState.Intro;
void Start(){if(startAtStorm)Change(GameState.Storm);else if(startAtNight){Change(GameState.Night);GameEvents.RaiseObjectiveChanged("Keep the lamp fed");}}
void OnEnable()=>GameEvents.StoryBeatTriggered+=Beat;
void OnDisable()=>GameEvents.StoryBeatTriggered-=Beat;
void Beat(StoryBeatTriggeredEvent e) { if(e.Id=="A1-B01") Change(GameState.Dusk); if(e.Id=="A1-B06") Change(GameState.Night); if(e.Id=="A3-B01"&&Current!=GameState.Storm)Change(GameState.Storm); }
void Change(GameState next) { var previous=Current; Current=next; GameEvents.RaiseGameStateChanged(new(previous,next)); }
}}
