using UnityEngine;

namespace TheLightHouse
{
    public sealed class DawnController : MonoBehaviour
    {
        [SerializeField] FinaleSettings settings;
        [SerializeField] Light skyLight;
        [SerializeField] ParticleSystem rain;
        [SerializeField] WindZone wind;
        [SerializeField] Transform player;
        float elapsed,startLight,startFog,startRain,startWind;bool active,done;
        void OnEnable()=>GameEvents.StoryBeatTriggered+=Beat;
        void OnDisable()=>GameEvents.StoryBeatTriggered-=Beat;
        void Beat(StoryBeatTriggeredEvent e){if(e.Id=="A5-B03")Begin();}
        void Begin(){active=true;elapsed=0;startLight=skyLight?skyLight.intensity:0;startFog=RenderSettings.fogDensity;startRain=rain?rain.emission.rateOverTime.constant:0;startWind=wind?wind.windMain:0;GameEvents.RaiseCheckpointReached(new("DawnCountdown",player?player.position:transform.position));}
        void Update()
        {
            if(!active||done||!settings)return;elapsed+=Time.deltaTime;float p=Mathf.Clamp01(elapsed/settings.dawnDuration);
            if(skyLight)skyLight.intensity=Mathf.Lerp(startLight,settings.dawnLightIntensity,p);
            RenderSettings.fogDensity=Mathf.Lerp(startFog,settings.finalFogDensity,p);
            if(rain){var emission=rain.emission;emission.rateOverTime=Mathf.Lerp(startRain,0,p);}
            if(wind)wind.windMain=Mathf.Lerp(startWind,0,p);
            if(p>=1){done=true;GameEvents.RaiseDawnThresholdReached(settings.dawnDuration);}
        }
    }
}
