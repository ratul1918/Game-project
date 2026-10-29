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
        [SerializeField,Min(1)] int maxFullDetailFigures=4;
        [SerializeField,Min(.05f)] float detailRefreshInterval=.5f;
        [SerializeField] Transform detailTarget;
        float nextCheck,nextAdvance,nextDetailRefresh;bool threat;bool missedDusk;bool night;float readingSpeed=1f;
        LampState lamp=LampState.FuelLit;
        [SerializeField] StormSettings stormSettings;
        bool storm;int courtyard;
        [SerializeField] FinaleSettings finaleSettings;
        bool finale,frozen,dawnActive;float pressure;
        void OnEnable(){GameEvents.LampStateChanged+=Lamp;GameEvents.LampNotLitInTime+=Missed;GameEvents.StoryBeatTriggered+=Beat;GameEvents.GameStateChanged+=State;GameEvents.LampPulse+=Pulse;GameEvents.ReadingStateChanged+=Reading;GameEvents.FinalePhaseChanged+=Phase;GameEvents.DrownedLureRequested+=Lure;GameEvents.DawnReached+=Dawn;}
        void OnDisable(){GameEvents.LampStateChanged-=Lamp;GameEvents.LampNotLitInTime-=Missed;GameEvents.StoryBeatTriggered-=Beat;GameEvents.GameStateChanged-=State;GameEvents.LampPulse-=Pulse;GameEvents.ReadingStateChanged-=Reading;GameEvents.FinalePhaseChanged-=Phase;GameEvents.DrownedLureRequested-=Lure;GameEvents.DawnReached-=Dawn;}
        void Reading(ReadingStateChangedEvent e)
        {
            float previous=Mathf.Max(.05f,readingSpeed);float next=e.IsReading?e.ThreatSpeed:1f;
            if(nextAdvance>Time.time)nextAdvance=Time.time+(nextAdvance-Time.time)*previous/Mathf.Max(.05f,next);
            readingSpeed=next;
        }
        void State(GameStateChangedEvent e){night=e.Current==GameState.Night||e.Current==GameState.Storm||e.Current==GameState.Finale;storm=e.Current==GameState.Storm||e.Current==GameState.Finale;finale=e.Current==GameState.Finale;if(storm)threat=true;}
        void Phase(FinalePhaseChangedEvent e){if(e.Phase==FinalePhase.Hold)pressure=e.Progress;}
        void Lure(DrownedLureEvent e){if(!finale||frozen)return;foreach(var figure in figures)if(figure&&Vector3.Distance(figure.transform.position,e.Position)<=e.Radius)figure.Divert(e.Position,finaleSettings?finaleSettings.lureStepDistance:2f);}
        void Dawn(DawnReachedEvent e){dawnActive=true;frozen=true;foreach(var figure in figures)if(figure&&figure.gameObject.activeSelf)figure.ReturnToWater(finaleSettings?finaleSettings.returnDuration:6f,finaleSettings?finaleSettings.sinkDepth:2.5f);}
        public int FiguresAtDoor{get{int count=0;foreach(var figure in figures)if(figure&&figure.AtDoor)count++;return count;}}
        public void SetFrozen(bool value)=>frozen=value;
        void Pulse(){foreach(var figure in figures){figure.CheckVisibility(view);if(!figure.Seen)figure.StepBack();}}
        void Lamp(LampStateChangedEvent e)=>lamp=e.Current;
        void Missed()=>missedDusk=true;
        void Beat(StoryBeatTriggeredEvent e){if(e.Id=="A2-B04")threat=true;if(e.Id=="A3-B09")threat=false;}
        bool CanAdvance=>night&&threat&&(lamp==LampState.Failing||lamp==LampState.Extinguished||lamp==LampState.Unlit);
        void LateUpdate()
        {
            RefreshDetailLevels();
            if(frozen||dawnActive||Time.time<nextCheck)return;nextCheck=Time.time+checkRate;
            foreach(var figure in figures)figure.CheckVisibility(view);
            if(!CanAdvance||Time.time<nextAdvance)return;
            float rate=lamp==LampState.Failing?failingMultiplier:extinguishedMultiplier;
            if(missedDusk)rate*=missedDuskMultiplier;
            float finalePressure=finale&&finaleSettings?Mathf.Lerp(1f,finaleSettings.maximumPressureMultiplier,pressure):1f;
            nextAdvance=Time.time+((storm&&stormSettings?(lamp==LampState.Failing?stormSettings.failingAdvanceInterval:stormSettings.extinguishedAdvanceInterval):advanceInterval/Mathf.Max(.01f,rate))/Mathf.Max(.05f,readingSpeed))/finalePressure;
            foreach(var figure in figures)if(figure.Advance()&&storm){GameEvents.RaiseStorySignal("storm-darkness");if(figure.AtDoor)courtyard++;if(courtyard>=stormSettings.courtyardCount)GameEvents.RaiseStorySignal("courtyard-filling");}
        }
        void RefreshDetailLevels()
        {
            if(Time.time<nextDetailRefresh||figures==null)return;nextDetailRefresh=Time.time+detailRefreshInterval;Transform target=detailTarget?detailTarget:(view?view.transform:null);if(!target)return;
            System.Array.Sort(figures,(a,b)=>Distance(a,target).CompareTo(Distance(b,target)));
            int full=0;foreach(var figure in figures){if(!figure||!figure.gameObject.activeSelf)continue;figure.SetFullDetail(full<maxFullDetailFigures);full++;}
        }
        static float Distance(DrownedFigure figure,Transform target)=>figure?(figure.transform.position-target.position).sqrMagnitude:float.MaxValue;
        public void LureNearest(Vector3 position)
        {
            if(!CanAdvance)return;DrownedFigure nearest=null;float distance=float.MaxValue;
            foreach(var figure in figures){figure.CheckVisibility(view);float value=(figure.transform.position-position).sqrMagnitude;if(!figure.Seen&&value<distance){nearest=figure;distance=value;}}
            if(nearest)nearest.Advance();
        }
    }
}
