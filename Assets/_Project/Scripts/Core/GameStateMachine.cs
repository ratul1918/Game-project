using UnityEngine;
namespace TheLightHouse {
public enum GameState { Intro, Dusk, Night, Storm, Finale, Dawn, Ending }
public sealed class GameStateMachine : MonoBehaviour {
public GameState Current { get; private set; } = GameState.Intro;
void OnEnable()=>GameEvents.StoryBeatTriggered+=Beat;
void OnDisable()=>GameEvents.StoryBeatTriggered-=Beat;
void Beat(StoryBeatTriggeredEvent e) { if(e.Id=="A1-B01") Change(GameState.Dusk); if(e.Id=="A1-B06") Change(GameState.Night); }
void Change(GameState next) { var previous=Current; Current=next; GameEvents.RaiseGameStateChanged(new(previous,next)); }
}}
