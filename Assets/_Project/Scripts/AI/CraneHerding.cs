using UnityEngine;
namespace TheLightHouse
{
    public sealed class CraneHerding : MonoBehaviour
    {
        [SerializeField] Transform boatShed;
        [SerializeField,Range(0,1)] float biasStrength=.08f;
        [SerializeField] ActFourSettings actFourSettings;
        bool postReveal;
        void OnEnable()=>GameEvents.StoryBeatTriggered+=Beat;
        void OnDisable()=>GameEvents.StoryBeatTriggered-=Beat;
        void Beat(StoryBeatTriggeredEvent e){if(e.Id=="A4-B05")postReveal=true;}
        public Vector3 Bias(Vector3 destination)
        {
            // Intentional foreshadowing: Crane subtly favours the boat shed, protecting his son (Act 4).
            float strength=postReveal&&actFourSettings?actFourSettings.postRevealHerdingBias:biasStrength;
            return boatShed?Vector3.Lerp(destination,boatShed.position,strength):destination;
        }
    }
}
