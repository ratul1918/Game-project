using UnityEngine;

namespace TheLightHouse
{
    public sealed class ReadingGrace : MonoBehaviour
    {
        [SerializeField] ActFourSettings settings;
        [SerializeField] LampController lamp;
        readonly System.Collections.Generic.HashSet<string> rewarded=new();
        void OnEnable()=>GameEvents.PageRead+=PageRead;
        void OnDisable()=>GameEvents.PageRead-=PageRead;
        void PageRead(PageReadEvent e){if(rewarded.Add(e.Id)&&lamp&&lamp.ChargeMode&&settings)lamp.AddCharge(settings.pageChargeTopUp);}
    }
}
