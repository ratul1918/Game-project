using UnityEngine;
namespace TheLightHouse
{
    public sealed class StalkerAwarenessHint : MonoBehaviour
    {
        [SerializeField] bool showHint;
        [SerializeField] Transform player;
        [SerializeField] Transform stalker;
        [SerializeField] float range=8;
        [SerializeField] Rect area=new(20,75,260,25);
        void OnGUI(){if(!showHint||!stalker||!stalker.gameObject.activeInHierarchy)return;
            var delta=stalker.position-player.position;if(delta.magnitude>range)return;
            GUI.Label(area,Vector3.Dot(player.right,delta)>0?"Sound to the right":"Sound to the left");}
    }
}
