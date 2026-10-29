using UnityEngine;
using UnityEngine.AI;

namespace TheLightHouse
{
    [RequireComponent(typeof(NavMeshAgent),typeof(AudioSource))]
    public sealed class CraneFinale : MonoBehaviour
    {
        [SerializeField] FinaleSettings settings;
        [SerializeField] Door[] slamDoors;
        [SerializeField] Transform[] lurePoints;
        [SerializeField] Transform player;
        [SerializeField] AudioClip lullabyHum;
        NavMeshAgent agent;AudioSource source;bool active;float nextAction,stutterUntil;int point,door;
        void Awake(){agent=GetComponent<NavMeshAgent>();source=GetComponent<AudioSource>();}
        void OnEnable(){GameEvents.StoryBeatTriggered+=Beat;GameEvents.LampStateChanged+=Lamp;}
        void OnDisable(){GameEvents.StoryBeatTriggered-=Beat;GameEvents.LampStateChanged-=Lamp;}
        void Beat(StoryBeatTriggeredEvent e){if(e.Id=="A5-B02")Activate();if(e.Id=="A5-B04")active=false;}
        void Activate()
        {
            // Finale payoff for CraneHerding and PostRevealMode: Crane now uses that protective intent openly.
            active=true;agent.speed=settings?settings.craneSpeed:4f;nextAction=Time.time;
            if(lullabyHum){source.clip=lullabyHum;source.loop=true;source.spatialBlend=1;source.Play();}
        }
        void Lamp(LampStateChangedEvent e){if(!active||!source)return;source.pitch=e.Current==LampState.Extinguished?.65f:1f;}
        void Update()
        {
            if(!active||!settings)return;
            if(Time.time<stutterUntil){agent.isStopped=true;return;}agent.isStopped=false;
            if(player&&source){float distance=Vector3.Distance(transform.position,player.position);source.volume=distance<=settings.nearHumRange?.65f:distance>=settings.farHumRange?.25f:.45f;}
            if(Time.time<nextAction)return;nextAction=Time.time+settings.slamInterval;
            stutterUntil=Time.time+settings.controlStutter;GameEvents.RequestDialogue(new("","(humming, wavering)"));
            if(slamDoors!=null&&slamDoors.Length>0){var target=slamDoors[door++%slamDoors.Length];target?.SlamForce();if(target)GameEvents.RaiseDrownedLureRequested(new(target.transform.position,settings.lureRadius));}
            if(lurePoints!=null&&lurePoints.Length>0){var target=lurePoints[point++%lurePoints.Length];if(target&&agent.isOnNavMesh){agent.SetDestination(target.position);GameEvents.RaiseDrownedLureRequested(new(target.position,settings.lureRadius));}}
        }
    }
}
