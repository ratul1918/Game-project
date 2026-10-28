using UnityEngine;
namespace TheLightHouse
{
    public sealed class ReserveFuel : MonoBehaviour
    {
        [SerializeField] GameObject fuelCan;
        [SerializeField] string beatId="A2-B01";
        void OnEnable()=>GameEvents.StoryBeatTriggered+=Beat;
        void OnDisable()=>GameEvents.StoryBeatTriggered-=Beat;
        void Start()=>fuelCan.SetActive(false);
        void Beat(StoryBeatTriggeredEvent e){if(e.Id==beatId)fuelCan.SetActive(true);}
    }
}
