using UnityEngine;
namespace TheLightHouse
{
    public sealed class DrownedManager : MonoBehaviour
    {
        [SerializeField] DrownedFigure[] figures;
        [SerializeField] Camera view;
        [SerializeField] float checkRate=.15f;
        [SerializeField] float advanceInterval=6;
        [SerializeField] float failingMultiplier=1.5f;
        [SerializeField] float extinguishedMultiplier=2;
        [SerializeField] float missedDuskMultiplier=1.5f;
        float nextCheck,nextAdvance;bool threat;bool missedDusk;bool night;
        LampState lamp=LampState.FuelLit;
        [SerializeField] StormSettings stormSettings;
        bool storm;int courtyard;
        void OnEnable(){GameEvents.LampStateChanged+=Lamp;GameEvents.LampNotLitInTime+=Missed;GameEvents.StoryBeatTriggered+=Beat;GameEvents.GameStateChanged+=State;GameEvents.LampPulse+=Pulse;}
        void OnDisable(){GameEvents.LampStateChanged-=Lamp;GameEvents.LampNotLitInTime-=Missed;GameEvents.StoryBeatTriggered-=Beat;GameEvents.GameStateChanged-=State;GameEvents.LampPulse-=Pulse;}
        void State(GameStateChangedEvent e){night=e.Current==GameState.Night||e.Current==GameState.Storm;storm=e.Current==GameState.Storm;if(storm)threat=true;}
        void Pulse(){foreach(var figure in figures){figure.CheckVisibility(view);if(!figure.Seen)figure.StepBack();}}
        void Lamp(LampStateChangedEvent e)=>lamp=e.Current;
        void Missed()=>missedDusk=true;
        void Beat(StoryBeatTriggeredEvent e){if(e.Id=="A2-B04")threat=true;if(e.Id=="A3-B09")threat=false;}
        bool CanAdvance=>night&&threat&&(lamp==LampState.Failing||lamp==LampState.Extinguished||lamp==LampState.Unlit);
        void LateUpdate()
        {
            if(Time.time<nextCheck)return;nextCheck=Time.time+checkRate;
            foreach(var figure in figures)figure.CheckVisibility(view);
            if(!CanAdvance||Time.time<nextAdvance)return;
            float rate=lamp==LampState.Failing?failingMultiplier:extinguishedMultiplier;
            if(missedDusk)rate*=missedDuskMultiplier;
            nextAdvance=Time.time+(storm&&stormSettings?(lamp==LampState.Failing?stormSettings.failingAdvanceInterval:stormSettings.extinguishedAdvanceInterval):advanceInterval/Mathf.Max(.01f,rate));
            foreach(var figure in figures)if(figure.Advance()&&storm){GameEvents.RaiseStorySignal("storm-darkness");if(figure.AtDoor)courtyard++;if(courtyard>=stormSettings.courtyardCount)GameEvents.RaiseStorySignal("courtyard-filling");}
        }
        public void LureNearest(Vector3 position)
        {
            if(!CanAdvance)return;DrownedFigure nearest=null;float distance=float.MaxValue;
            foreach(var figure in figures){figure.CheckVisibility(view);float value=(figure.transform.position-position).sqrMagnitude;if(!figure.Seen&&value<distance){nearest=figure;distance=value;}}
            if(nearest)nearest.Advance();
        }
    }
}
