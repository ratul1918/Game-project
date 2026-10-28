using UnityEngine;
using UnityEngine.AI;
namespace TheLightHouse
{
    public sealed class StalkerSpawner : MonoBehaviour
    {
        [SerializeField] GameObject stalker;
        [SerializeField] Transform spawn;
        [SerializeField] string beatId="A2-B03";
        void OnEnable()=>GameEvents.StoryBeatTriggered+=Beat;
        void OnDisable()=>GameEvents.StoryBeatTriggered-=Beat;
        void Start(){stalker.SetActive(false);}
        void Beat(StoryBeatTriggeredEvent e)
        {
            if(e.Id!=beatId)return;
            stalker.transform.SetPositionAndRotation(spawn.position,spawn.rotation);stalker.SetActive(true);
            var agent=stalker.GetComponent<NavMeshAgent>();if(agent.isOnNavMesh)agent.Warp(spawn.position);
        }
    }
}
