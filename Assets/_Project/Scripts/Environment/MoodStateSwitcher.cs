using UnityEngine;
using UnityEngine.InputSystem;

namespace TheLightHouse
{
    public sealed class MoodStateSwitcher : MonoBehaviour
    {
        [SerializeField] AtmosphereController atmosphere;
        void Update()
        {
            Keyboard keyboard=Keyboard.current;if(keyboard==null||!atmosphere)return;
            if(keyboard.digit1Key.wasPressedThisFrame)atmosphere.PreviewState(GameState.Dusk);
            else if(keyboard.digit2Key.wasPressedThisFrame)atmosphere.PreviewState(GameState.Night);
            else if(keyboard.digit3Key.wasPressedThisFrame)atmosphere.PreviewState(GameState.Storm);
            else if(keyboard.digit4Key.wasPressedThisFrame)atmosphere.PreviewState(GameState.Finale);
            else if(keyboard.digit5Key.wasPressedThisFrame)atmosphere.PreviewState(GameState.Dawn);
        }
    }
}
