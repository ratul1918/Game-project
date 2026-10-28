using UnityEngine;
namespace TheLightHouse
{
    public sealed class PlayerHealthState : MonoBehaviour
    {
        [SerializeField] Transform checkpoint;
        FirstPersonController controller;
        CharacterController capsule;
        Interactor interactor;
        Vector3 exitPosition;
        Quaternion exitRotation;
        public bool IsHidden { get; private set; }
        public bool IsCaught { get; set; }
        public Vector3 CheckpointPosition { get; private set; }
        public HidingSpot CurrentSpot { get; private set; }
        void Awake()
        {
            controller=GetComponent<FirstPersonController>(); capsule=GetComponent<CharacterController>();
            interactor=GetComponent<Interactor>(); CheckpointPosition=checkpoint?checkpoint.position:transform.position;
        }
        void OnEnable(){GameEvents.HidingEntered+=Enter;GameEvents.HidingExited+=Exit;GameEvents.GameStateChanged+=State;}
        void OnDisable(){GameEvents.HidingEntered-=Enter;GameEvents.HidingExited-=Exit;GameEvents.GameStateChanged-=State;}
        void State(GameStateChangedEvent e){if(e.Current==GameState.Storm)CheckpointPosition=transform.position;}
        void Enter(HidingEnteredEvent request)
        {
            var spot=request.Spot;
            if(IsHidden||IsCaught)return;
            exitPosition=transform.position;exitRotation=transform.rotation;CurrentSpot=spot;IsHidden=true;
            capsule.enabled=false;transform.SetPositionAndRotation(spot.Pose.position,spot.Pose.rotation);
            if(interactor)interactor.enabled=false;
            controller.SetMovementLocked(true);controller.SetPeekLimits(spot.PeekYaw,spot.PeekPitch);
            GameEvents.RaisePlayerHidden(new(true,spot.Id));
        }
        void Exit()
        {
            if(!IsHidden)return;
            transform.SetPositionAndRotation(exitPosition,exitRotation);capsule.enabled=true;
            controller.SetPeekLimits(0,0);controller.SetMovementLocked(false);
            if(interactor)interactor.enabled=true;
            IsHidden=false;CurrentSpot=null;GameEvents.RaisePlayerHidden(new(false,""));
        }
        public void Respawn()
        {
            Exit();capsule.enabled=false;transform.position=CheckpointPosition;capsule.enabled=true;
            IsCaught=false;controller.SetMovementLocked(false);controller.SetLookLocked(false);
            GameEvents.RaisePlayerRespawned();
        }
    }
}
