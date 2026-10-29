using UnityEngine;

namespace TheLightHouse
{
    public sealed class FinaleDirector : MonoBehaviour
    {
        [SerializeField] FinaleSettings settings;
        [SerializeField] LampController lamp;
        [SerializeField] DrownedManager drowned;
        [SerializeField] PlayerStamina stamina;
        [SerializeField] PlayerHealthState player;
        FinalePhase phase;float phaseTime,extinguishedTime;bool active,requested;
        void OnEnable(){GameEvents.GameStateChanged+=State;GameEvents.StoryBeatTriggered+=Beat;GameEvents.LampStateChanged+=OnLampState;GameEvents.LampPulse+=Pulse;GameEvents.PlayerRespawned+=Respawn;}
        void OnDisable(){GameEvents.GameStateChanged-=State;GameEvents.StoryBeatTriggered-=Beat;GameEvents.LampStateChanged-=OnLampState;GameEvents.LampPulse-=Pulse;GameEvents.PlayerRespawned-=Respawn;}
        void State(GameStateChangedEvent e){if(e.Current==GameState.Finale)Begin();}
        void Begin(){active=true;requested=false;phase=FinalePhase.Flood;phaseTime=0;GameEvents.RaiseFinalePhaseChanged(new(phase,0));GameEvents.RaiseObjectiveChanged("Reach the courtyard and hold the light");}
        void Beat(StoryBeatTriggeredEvent e)
        {
            if(e.Id=="A5-B01")SetPhase(FinalePhase.Hold);
            if(e.Id=="A5-B02")SetPhase(FinalePhase.CraneIntervenes);
            if(e.Id=="A5-B03")SetPhase(FinalePhase.DawnCountdown);
            if(e.Id=="A5-B04")SetPhase(FinalePhase.Escape);
        }
        void SetPhase(FinalePhase next){phase=next;phaseTime=0;requested=false;GameEvents.RaiseFinalePhaseChanged(new(phase,0));}
        void OnLampState(LampStateChangedEvent e){if(e.Current!=TheLightHouse.LampState.Extinguished)extinguishedTime=0;}
        void Pulse(){if(active&&stamina&&settings)stamina.Spend(settings.pulseStaminaCost,settings.extinguishedGrace);}
        void Respawn(){if(!active)return;lamp?.SetCharge(settings.initialCharge);SetPhase(FinalePhase.Hold);}
        void Update()
        {
            if(!active||!settings)return;phaseTime+=Time.deltaTime;
            if(lamp&&lamp.CurrentState==TheLightHouse.LampState.Extinguished&&drowned&&drowned.FiguresAtDoor>=settings.figuresAtDoorForFailure)
            {
                extinguishedTime+=Time.deltaTime;if(extinguishedTime>=settings.extinguishedGrace){extinguishedTime=0;if(player)GameEvents.RaisePlayerSpotted(new("drowned-at-door",player.transform.position));GameEvents.RaiseCatchRequested();}
            }
            if(phase==FinalePhase.Flood&&!requested&&phaseTime>=settings.floodDelay){requested=true;GameEvents.RaiseStorySignal("courtyard-floods");}
            if(phase==FinalePhase.Hold)
            {
                float progress=Mathf.Clamp01(phaseTime/settings.holdDuration);GameEvents.RaiseFinalePhaseChanged(new(phase,progress));
                if(!requested&&progress>=1){requested=true;GameEvents.RaiseStorySignal("hold-light");}
            }
            if(phase==FinalePhase.CraneIntervenes&&!requested&&phaseTime>=settings.interventionDelay){requested=true;GameEvents.RaiseStorySignal("crane-remembers");}
        }
    }
}
