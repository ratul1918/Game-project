using UnityEngine;

namespace TheLightHouse
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class RoomTrigger : MonoBehaviour
    {
        [SerializeField] string roomId;
        [SerializeField] string displayName;
        [SerializeField] int floorLevel;

        void Reset()
        {
            GetComponent<BoxCollider>().isTrigger=true;
        }

        void OnTriggerEnter(Collider other)
        {
            if(!other.CompareTag("Player")&&!other.GetComponentInParent<FirstPersonController>())return;
            GameEvents.RaiseRoomEntered(new(roomId,displayName,floorLevel));
        }
    }
}
