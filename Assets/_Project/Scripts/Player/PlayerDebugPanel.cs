using UnityEngine;

namespace TheLightHouse
{
    public sealed class PlayerDebugPanel : MonoBehaviour
    {
        FirstPersonController player;
        float loudness;
        bool visible;
        public void Initialize(FirstPersonController value) => player = value;
        public void Toggle() => visible = !visible;
        void OnEnable() => GameEvents.NoiseEmitted += Noise;
        void OnDisable() => GameEvents.NoiseEmitted -= Noise;
        void Noise(NoiseEmittedEvent e) { if (e.Source == gameObject) loudness = e.Loudness; }
        void OnGUI()
        {
            if (!visible || !player) return;
            GUILayout.BeginArea(player.Settings.debugPanelRect, GUI.skin.box);
            GUILayout.Label($"Speed: {player.Speed:0.00} m/s\nGrounded: {player.Grounded}\nStamina: {player.Stamina.Normalized:P0}\nLast noise: {loudness:0.0} m\nSurface: {player.Surface.SurfaceName}");
            GUILayout.EndArea();
        }
    }
}
