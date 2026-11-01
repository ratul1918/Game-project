using UnityEngine;

namespace TheLightHouse
{
    [CreateAssetMenu(menuName="The LightHouse/HUD Settings",fileName="HudSettings")]
    public sealed class HudSettings : ScriptableObject
    {
        public enum Element { Noise,Stamina,Lamp,Objective,Inventory,Interaction,Toasts,RoomNames,Composure }
        [SerializeField] bool showNoise=true;
        [SerializeField] bool showStamina=true;
        [SerializeField] bool showLamp=true;
        [SerializeField] bool showObjective=true;
        [SerializeField] bool showInventory=true;
        [SerializeField] bool showInteraction=true;
        [SerializeField] bool showToasts=true;
        [SerializeField] bool showRoomNames=true;
        [SerializeField] bool showComposure;
        [SerializeField,Range(.7f,1.5f)] float hudScale=1f;
        [SerializeField,Range(.75f,2f)] float subtitleScale=1f;
        [SerializeField,Range(0f,1f)] float subtitleBackgroundOpacity=.65f;
        [SerializeField] bool minimalPreset;

        public bool ShowNoise=>showNoise&&!minimalPreset;
        public bool ShowStamina=>showStamina&&!minimalPreset;
        public bool ShowLamp=>showLamp;
        public bool ShowObjective=>showObjective;
        public bool ShowInventory=>showInventory&&!minimalPreset;
        public bool ShowInteraction=>showInteraction;
        public bool ShowToasts=>showToasts&&!minimalPreset;
        public bool ShowRoomNames=>showRoomNames;
        public bool ShowComposure=>showComposure&&!minimalPreset;
        public float HudScale=>hudScale;
        public float SubtitleScale=>subtitleScale;
        public float SubtitleBackgroundOpacity=>subtitleBackgroundOpacity;

        public void ApplyMinimalPreset(bool value)=>minimalPreset=value;
        public void SetScale(float value)=>hudScale=Mathf.Clamp(value,.7f,1.5f);
        public void SetRoomNames(bool value)=>showRoomNames=value;
        public void SetSubtitleScale(float value)=>subtitleScale=Mathf.Clamp(value,.75f,2f);
        public void SetSubtitleBackground(float value)=>subtitleBackgroundOpacity=Mathf.Clamp01(value);
        public void SetElement(Element element,bool value)
        {
            switch(element){case Element.Noise:showNoise=value;break;case Element.Stamina:showStamina=value;break;case Element.Lamp:showLamp=value;break;case Element.Objective:showObjective=value;break;case Element.Inventory:showInventory=value;break;case Element.Interaction:showInteraction=value;break;case Element.Toasts:showToasts=value;break;case Element.RoomNames:showRoomNames=value;break;case Element.Composure:showComposure=value;break;}
        }
    }
}
