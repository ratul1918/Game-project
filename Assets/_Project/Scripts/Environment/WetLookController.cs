using UnityEngine;

namespace TheLightHouse
{
    public sealed class WetLookController : MonoBehaviour
    {
        [SerializeField] StormController stormController;
        [SerializeField,Range(0f,1f)] float maximumWetness=.85f;
        [SerializeField,Range(0f,1f)] float maximumSmoothnessBoost=.35f;
        [SerializeField,Min(.01f)] float responseSpeed=.2f;
        [SerializeField] string wetnessProperty="_GlobalWetness";
        [SerializeField] string smoothnessProperty="_GlobalWetSmoothnessBoost";
        float wetness;
        void Update()
        {
            float target=stormController?stormController.Intensity*maximumWetness:0f;
            wetness=Mathf.MoveTowards(wetness,target,responseSpeed*Time.deltaTime);
            Shader.SetGlobalFloat(wetnessProperty,wetness);
            Shader.SetGlobalFloat(smoothnessProperty,wetness*maximumSmoothnessBoost);
        }
        void OnDisable(){Shader.SetGlobalFloat(wetnessProperty,0f);Shader.SetGlobalFloat(smoothnessProperty,0f);}
    }
}
