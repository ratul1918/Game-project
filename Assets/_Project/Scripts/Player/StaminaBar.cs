using UnityEngine;

namespace TheLightHouse
{
    public sealed class StaminaBar : MonoBehaviour
    {
        float normalized = 1f;
        void OnEnable() => GameEvents.StaminaChanged += Changed;
        void OnDisable() => GameEvents.StaminaChanged -= Changed;
        void Changed(StaminaChangedEvent e) => normalized = e.Capacity > 0f ? e.Current / e.Capacity : 1f;
        void OnGUI()
        {
            if (normalized >= 1f) return;
            var settings = GetComponent<FirstPersonController>().Settings;
            float width = Screen.width * settings.staminaBarWidth;
            var rect = new Rect((Screen.width-width)*.5f, Screen.height*settings.staminaBarY, width, settings.staminaBarHeight);
            GUI.Box(rect, GUIContent.none);
            rect.width *= normalized;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
        }
    }
}
