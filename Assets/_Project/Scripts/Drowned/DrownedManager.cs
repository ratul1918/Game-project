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
        void OnEnable(){GameEvents.LampStateChanged+=Lamp;GameEvents.LampNotLitInTime+=Missed;GameEvents.StoryBeatTriggered+=Beat;GameEvents.GameStateChanged+=State;}
        void OnDisable(){GameEvents.LampStateChanged-=Lamp;GameEvents.LampNotLitInTime-=Missed;GameEvents.StoryBeatTriggered-=Beat;GameEvents.GameStateChanged-=State;}
        void State(GameStateChangedEvent e)=>night=e.Current==GameState.Night;
        void Lamp(LampStateChangedEvent e)=>lamp=e.Current;
        void Missed()=>missedDusk=true;
        void Beat(StoryBeatTriggeredEvent e){if(e.Id=="A2-B04")threat=true;}
        bool CanAdvance=>night&&threat&&(lamp==LampState.Failing||lamp==LampState.Extinguished||lamp==LampState.Unlit);
        void LateUpdate()
        {
            if(Time.time<nextCheck)return;nextCheck=Time.time+checkRate;
            foreach(var figure in figures)figure.CheckVisibility(view);
            if(!CanAdvance||Time.time<nextAdvance)return;
            float rate=lamp==LampState.Failing?failingMultiplier:extinguishedMultiplier;
            if(missedDusk)rate*=missedDuskMultiplier;
            nextAdvance=Time.time+advanceInterval/Mathf.Max(.01f,rate);
            foreach(var figure in figures)figure.Advance();
        }
        public void LureNearest(Vector3 position)
        {
            if(!CanAdvance)return;DrownedFigure nearest=null;float distance=float.MaxValue;
            foreach(var figure in figures){figure.CheckVisibility(view);float value=(figure.transform.position-position).sqrMagnitude;if(!figure.Seen&&value<distance){nearest=figure;distance=value;}}
            if(nearest)nearest.Advance();
        }
    }
}
