using UnityEngine;

namespace TheLightHouse
{
    public sealed class HudSettingsPanel : MonoBehaviour
    {
        [SerializeField] HudSettings settings;
        [SerializeField] HudRoot hud;
        [SerializeField] SubtitleUI subtitles;
        void Apply(){hud?.ApplySettings();subtitles?.ApplySettings(settings);}
        public void SetNoise(bool value){settings?.SetElement(HudSettings.Element.Noise,value);Apply();}
        public void SetStamina(bool value){settings?.SetElement(HudSettings.Element.Stamina,value);Apply();}
        public void SetLamp(bool value){settings?.SetElement(HudSettings.Element.Lamp,value);Apply();}
        public void SetObjective(bool value){settings?.SetElement(HudSettings.Element.Objective,value);Apply();}
        public void SetInventory(bool value){settings?.SetElement(HudSettings.Element.Inventory,value);Apply();}
        public void SetInteraction(bool value){settings?.SetElement(HudSettings.Element.Interaction,value);Apply();}
        public void SetToasts(bool value){settings?.SetElement(HudSettings.Element.Toasts,value);Apply();}
        public void SetRoomNames(bool value){settings?.SetElement(HudSettings.Element.RoomNames,value);Apply();}
        public void SetComposure(bool value){settings?.SetElement(HudSettings.Element.Composure,value);Apply();}
        public void SetMinimal(bool value){settings?.ApplyMinimalPreset(value);Apply();}
        public void SetHudScale(float value){settings?.SetScale(value);Apply();}
        public void SetSubtitleScale(float value){settings?.SetSubtitleScale(value);Apply();}
        public void SetSubtitleBackground(float value){settings?.SetSubtitleBackground(value);Apply();}
    }
}
