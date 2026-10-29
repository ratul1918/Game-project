using System;
using UnityEngine;

namespace TheLightHouse
{
    [Serializable]
    public struct AtmospherePreset
    {
        public Color fogColor;
        [Min(0f)] public float fogDensity;
        public Color ambientColor;
        public Color directionalLightColor;
        [Min(0f)] public float directionalLightIntensity;
        [Min(0f)] public float skyboxExposure;
        [Range(0f,1f)] public float postProcessWeight;
    }

    [CreateAssetMenu(menuName="The LightHouse/Atmosphere Settings",fileName="AtmosphereSettings")]
    public sealed class AtmosphereSettings : ScriptableObject
    {
        [Min(.01f)] public float blendSeconds=3f;
        public AtmospherePreset dusk;
        public AtmospherePreset night;
        public AtmospherePreset storm;
        public AtmospherePreset finale;
        public AtmospherePreset dawn;

        public AtmospherePreset For(GameState state)
        {
            return state switch
            {
                GameState.Night=>night,
                GameState.Storm=>storm,
                GameState.Finale=>finale,
                GameState.Dawn=>dawn,
                GameState.Ending=>dawn,
                _=>dusk
            };
        }
    }
}
