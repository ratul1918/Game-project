using UnityEngine;
using UnityEngine.Rendering;

namespace TheLightHouse
{
    public sealed class AtmosphereController : MonoBehaviour
    {
        [SerializeField] AtmosphereSettings settings;
        [SerializeField] Light directionalLight;
        [SerializeField] Material skyboxMaterial;
        [SerializeField] Volume duskVolume;
        [SerializeField] Volume nightVolume;
        [SerializeField] Volume stormVolume;
        [SerializeField] Volume finaleVolume;
        [SerializeField] Volume dawnVolume;
        [SerializeField] DuskTimer duskTimer;
        [SerializeField] StormController stormController;
        [SerializeField] DawnController dawnController;
        [SerializeField] bool enableFog=true;

        GameState state=GameState.Dusk;
        AtmospherePreset displayed;
        AtmospherePreset from;
        AtmospherePreset target;
        float blendElapsed;

        void Awake()
        {
            if(!settings)return;
            displayed=from=target=settings.For(state);
            Apply(displayed);
            ApplyVolumeWeights(1f);
        }

        void OnEnable()=>GameEvents.GameStateChanged+=OnGameStateChanged;
        void OnDisable()=>GameEvents.GameStateChanged-=OnGameStateChanged;

        void OnGameStateChanged(GameStateChangedEvent change)=>SetState(change.Current);

        public void PreviewState(GameState previewState)=>SetState(previewState);

        void SetState(GameState next)
        {
            if(!settings)return;
            state=next;
            from=displayed;
            target=settings.For(next);
            blendElapsed=0f;
        }

        void Update()
        {
            if(!settings)return;
            blendElapsed+=Time.deltaTime;
            float t=Mathf.SmoothStep(0f,1f,Mathf.Clamp01(blendElapsed/Mathf.Max(.01f,settings.blendSeconds)));
            displayed=Lerp(from,target,t);
            Apply(displayed);
            ApplyVolumeWeights(t);
        }

        void Apply(AtmospherePreset preset)
        {
            RenderSettings.fog=enableFog;
            RenderSettings.fogMode=FogMode.ExponentialSquared;
            RenderSettings.fogColor=preset.fogColor;
            if(!dawnController||!dawnController.IsActive)RenderSettings.fogDensity=preset.fogDensity;
            bool duskOwnsLight=duskTimer&&duskTimer.IsActive;
            bool dawnOwnsLight=dawnController&&dawnController.IsActive;
            bool lightningOwnsLight=stormController&&stormController.IsFlashing;
            if(!duskOwnsLight){RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=preset.ambientColor;}
            if(directionalLight&&!duskOwnsLight&&!dawnOwnsLight&&!lightningOwnsLight)
            {
                directionalLight.color=preset.directionalLightColor;
                directionalLight.intensity=preset.directionalLightIntensity;
            }
            Material sky=skyboxMaterial?skyboxMaterial:RenderSettings.skybox;
            if(sky&&sky.HasProperty("_Exposure"))sky.SetFloat("_Exposure",preset.skyboxExposure);
        }

        void ApplyVolumeWeights(float blend)
        {
            SetWeight(duskVolume,GameState.Dusk,blend);
            SetWeight(nightVolume,GameState.Night,blend);
            SetWeight(stormVolume,GameState.Storm,blend);
            SetWeight(finaleVolume,GameState.Finale,blend);
            SetWeight(dawnVolume,GameState.Dawn,blend);
        }

        void SetWeight(Volume volume,GameState volumeState,float blend)
        {
            if(!volume)return;
            bool selected=state==volumeState||(volumeState==GameState.Dusk&&state==GameState.Intro)||(volumeState==GameState.Dawn&&state==GameState.Ending);
            volume.weight=Mathf.MoveTowards(volume.weight,selected?settings.For(state).postProcessWeight:0f,Time.deltaTime/Mathf.Max(.01f,settings.blendSeconds));
        }

        static AtmospherePreset Lerp(AtmospherePreset a,AtmospherePreset b,float t)
        {
            return new AtmospherePreset
            {
                fogColor=Color.Lerp(a.fogColor,b.fogColor,t),fogDensity=Mathf.Lerp(a.fogDensity,b.fogDensity,t),
                ambientColor=Color.Lerp(a.ambientColor,b.ambientColor,t),directionalLightColor=Color.Lerp(a.directionalLightColor,b.directionalLightColor,t),
                directionalLightIntensity=Mathf.Lerp(a.directionalLightIntensity,b.directionalLightIntensity,t),skyboxExposure=Mathf.Lerp(a.skyboxExposure,b.skyboxExposure,t),
                postProcessWeight=Mathf.Lerp(a.postProcessWeight,b.postProcessWeight,t)
            };
        }
    }
}
