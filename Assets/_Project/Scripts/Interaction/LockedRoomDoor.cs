using UnityEngine;

namespace TheLightHouse
{
    [RequireComponent(typeof(NoiseEmitter))]
    public sealed class LockedRoomDoor : MonoBehaviour, IInteractable
    {
        [SerializeField] Inventory inventory;
        [SerializeField] string keyId="CraneRoomKey";
        [SerializeField] string doorId="crane-room-door";
        [SerializeField] float openAngle=100f;
        [SerializeField] float turnSpeed=120f;
        [SerializeField] float unlockNoise=5f;
        Quaternion closed;
        bool held,locked=true,open;
        public string Prompt=>locked?"Unlock Crane's room":open?"Close Crane's room":"Open Crane's room";
        void Awake()=>closed=transform.localRotation;
        void Update()=>transform.localRotation=Quaternion.RotateTowards(transform.localRotation,closed*Quaternion.Euler(0,open?openAngle:0,0),turnSpeed*Time.deltaTime);
        public void Interact()
        {
            if(held)return;held=true;
            if(locked)
            {
                if(!inventory||!inventory.Contains(keyId)){GameEvents.RequestDialogue(new("","(the lock holds)"));return;}
                locked=false;GetComponent<NoiseEmitter>().Emit(unlockNoise);GameEvents.RaiseStorySignal("crane-room-unlocked");GameEvents.RaiseDoorStateChanged(new(doorId,DoorState.Locked,DoorState.Closed));
            }
            var previous=open?DoorState.Open:DoorState.Closed;open=!open;GameEvents.RaiseDoorStateChanged(new(doorId,previous,open?DoorState.Open:DoorState.Closed));
        }
        public void StopInteract()=>held=false;
    }
}
