using UnityEngine;
using UnityEngine.InputSystem;
namespace TheLightHouse
{
    public sealed class HidingSpot : MonoBehaviour,IInteractable
    {
        [SerializeField] string spotId="wardrobe";
        [SerializeField] Transform hidingPose;
        [SerializeField] string prompt="Hide";
        [SerializeField] InputActionAsset inputActions;
        [SerializeField] Transform stalker;
        [SerializeField] PlayerHealthState player;
        [SerializeField] float peekYaw=18;
        [SerializeField] float peekPitch=12;
        [SerializeField] float dangerRadius=6;
        [SerializeField] float calmBreathLoudness=1;
        [SerializeField] float scaredBreathLoudness=5;
        [SerializeField] float breathInterval=2;
        [SerializeField] float breathCapacity=5;
        [SerializeField] float breathDrainRate=1;
        [SerializeField] float breathRegen=1;
        [SerializeField] float heldBreathLoudness=.1f;
        [SerializeField] float breathRecoveryDelay=2;
        float breath;float nextNoise;bool occupied;bool released;
        float breathReadyAt;
        bool rearmed=true;
        InputAction interact,hold;
        public Transform Pose=>hidingPose?hidingPose:transform;
        public float PeekYaw=>peekYaw;
        public float PeekPitch=>peekPitch;
        public string Id=>spotId;
        public string Prompt=>prompt;
        public bool Occupied=>occupied;
        void Awake(){breath=breathCapacity;if(inputActions){interact=inputActions.FindAction("Player/Interact");hold=inputActions.FindAction("Player/HoldBreath");}}
        void OnEnable(){GameEvents.HidingExited+=Exited;interact?.Enable();hold?.Enable();}
        void OnDisable(){GameEvents.HidingExited-=Exited;if(occupied)GameEvents.RaiseHidingExited();}
        void Exited(){if(occupied)rearmed=false;occupied=false;released=false;}
        public void Interact(){if(occupied||!rearmed)return;occupied=true;released=false;nextNoise=Time.time+breathInterval;GameEvents.RaiseHidingEntered(this);interact?.Enable();}
        public void StopInteract(){}
        void Update()
        {
            if(!occupied){if(interact!=null&&!interact.IsPressed())rearmed=true;return;}
            if(!player || player.CurrentSpot!=this){occupied=false;return;}
            if(interact!=null&&!interact.IsPressed())released=true;
            if(released && interact!=null && interact.WasPressedThisFrame()){GameEvents.RaiseHidingExited();return;}
            bool holding=hold!=null&&hold.IsPressed()&&breath>0&&Time.time>=breathReadyAt;
            breath=Mathf.Clamp(breath+(holding?-breathDrainRate:breathRegen)*Time.deltaTime,0,breathCapacity);
            if(holding&&breath<=0)breathReadyAt=Time.time+breathRecoveryDelay;
            GameEvents.RaiseBreathChanged(breath/breathCapacity);
            if(Time.time<nextNoise)return;nextNoise=Time.time+breathInterval;
            float loudness=stalker&&stalker.gameObject.activeInHierarchy&&Vector3.Distance(stalker.position,Pose.position)<dangerRadius?scaredBreathLoudness:calmBreathLoudness;
            player.GetComponent<NoiseEmitter>().Emit(holding?heldBreathLoudness:loudness);
            GameEvents.RaiseStorySignal("hide-success");
        }
    }
}
