using UnityEngine;

namespace TheLightHouse
{
    public sealed class LampBeamVisual : MonoBehaviour
    {
        [SerializeField] Transform beamPivot;
        [SerializeField] Light spotLight;
        [SerializeField] Renderer coneRenderer;
        [SerializeField] SpriteRenderer glowSprite;
        [SerializeField,Min(0f)] float rotationDegreesPerSecond=24f;
        [SerializeField,Min(0f)] float minimumIntensity=.2f;
        [SerializeField,Min(0f)] float maximumIntensity=12f;
        [SerializeField,Min(.1f)] float minimumLength=8f;
        [SerializeField,Min(.1f)] float maximumLength=30f;
        [SerializeField,Range(0f,1f)] float unlitGlow=.05f;
        [SerializeField,Min(.01f)] float responseSpeed=4f;
        [SerializeField] string shaderIntensityProperty="_BeamIntensity";
        float targetCharge=1f,displayedCharge=1f;
        MaterialPropertyBlock properties;

        void OnEnable(){GameEvents.LampChargeChanged+=ChargeChanged;GameEvents.LampStateChanged+=StateChanged;}
        void OnDisable(){GameEvents.LampChargeChanged-=ChargeChanged;GameEvents.LampStateChanged-=StateChanged;}
        void Awake()=>properties=new MaterialPropertyBlock();
        void ChargeChanged(float value)=>targetCharge=Mathf.Clamp01(value);
        void StateChanged(LampStateChangedEvent change)=>targetCharge=change.IsLit?Mathf.Clamp01(change.IntensityNormalized):0f;
        void Update()
        {
            displayedCharge=Mathf.MoveTowards(displayedCharge,targetCharge,responseSpeed*Time.deltaTime);
            if(beamPivot)beamPivot.Rotate(Vector3.up,rotationDegreesPerSecond*Mathf.Lerp(.2f,1f,displayedCharge)*Time.deltaTime,Space.Self);
            float length=Mathf.Lerp(minimumLength,maximumLength,displayedCharge);
            if(spotLight){spotLight.enabled=displayedCharge>0f;spotLight.intensity=Mathf.Lerp(minimumIntensity,maximumIntensity,displayedCharge);spotLight.range=length;}
            if(coneRenderer)
            {
                coneRenderer.transform.localScale=new Vector3(length,length,length);
                coneRenderer.GetPropertyBlock(properties);properties.SetFloat(shaderIntensityProperty,displayedCharge);coneRenderer.SetPropertyBlock(properties);
            }
            if(glowSprite){Color color=glowSprite.color;color.a=Mathf.Lerp(unlitGlow,1f,displayedCharge);glowSprite.color=color;}
        }
    }
}
