using System.Collections;
using UnityEngine;

namespace TheLightHouse
{
    public sealed class FamilyPhoto : MonoBehaviour, IInteractable
    {
        [SerializeField] Camera view;
        [SerializeField] FirstPersonController player;
        [SerializeField] Transform inspectionPose;
        [SerializeField] float moveDuration=1.2f;
        [SerializeField] float inspectDuration=4f;
        bool held,revealed;
        public string Prompt=>"Inspect the family photograph";
        public void Interact(){if(held||revealed||!view||!inspectionPose)return;held=true;revealed=true;StartCoroutine(Reveal());}
        public void StopInteract()=>held=false;
        IEnumerator Reveal()
        {
            player?.SetMovementLocked(true);player?.SetLookLocked(true);
            Vector3 start=view.transform.position;Quaternion rotation=view.transform.rotation;float t=0;
            while(t<moveDuration){t+=Time.deltaTime;float p=Mathf.SmoothStep(0,1,t/moveDuration);view.transform.SetPositionAndRotation(Vector3.Lerp(start,inspectionPose.position,p),Quaternion.Slerp(rotation,inspectionPose.rotation,p));yield return null;}
            GameEvents.RequestDialogue(new("Daniel","That child is me."));
            GameEvents.RequestDialogue(new("Daniel","Crane... the thing in the keeper's coat... is my father."));
            yield return new WaitForSeconds(inspectDuration);
            view.transform.SetPositionAndRotation(start,rotation);player?.SetLookLocked(false);player?.SetMovementLocked(false);
            GameEvents.RaiseStorySignal("father-son");GameEvents.RaiseObjectiveChanged("Read Crane's final entry");
        }
    }
}
