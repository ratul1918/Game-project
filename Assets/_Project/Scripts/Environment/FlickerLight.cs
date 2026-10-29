using UnityEngine;

namespace TheLightHouse
{
    public sealed class FlickerLight : MonoBehaviour
    {
        [SerializeField] Light target;
        [SerializeField,Min(0f)] float baseIntensity=2f;
        [SerializeField,Min(0f)] float amplitude=.25f;
        [SerializeField,Min(.01f)] float frequency=7f;
        [SerializeField] float noiseOffset;
        void Reset()=>target=GetComponent<Light>();
        void Update(){if(target)target.intensity=baseIntensity+(Mathf.PerlinNoise(noiseOffset,Time.time*frequency)-.5f)*2f*amplitude;}
    }
}
