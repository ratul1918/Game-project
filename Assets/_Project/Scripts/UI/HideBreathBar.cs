using UnityEngine;
namespace TheLightHouse
{
    public sealed class HideBreathBar : MonoBehaviour
    {
        [SerializeField] Rect area=new(20,110,260,26);
        bool hidden;float breath=1;
        void OnEnable(){GameEvents.PlayerHidden+=Hidden;GameEvents.BreathChanged+=Changed;}
        void OnDisable(){GameEvents.PlayerHidden-=Hidden;GameEvents.BreathChanged-=Changed;}
        void Hidden(PlayerHiddenEvent e)=>hidden=e.IsHidden;
        void Changed(float value)=>breath=value;
        void OnGUI(){if(hidden)GUI.Box(area,$"Hold R: breath {breath:P0} — E: exit");}
    }
}
