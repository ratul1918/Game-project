using System.Collections;
using UnityEngine;
namespace TheLightHouse
{
    public sealed class DrownedFigure : MonoBehaviour
    {
        [SerializeField] string figureId;
        [SerializeField] Vector3[] path;
        [SerializeField] Vector3 visibilityOffset=new(0,1,0);
        [SerializeField] float visibilityAngle=100;
        [SerializeField] LODGroup lodGroup;
        [SerializeField] Renderer fullDetailRenderer;
        [SerializeField] Renderer silhouetteRenderer;
        int step;
        bool everSeen;
        public bool Seen { get; private set; }
        public bool HasBeenSeen=>everSeen;
        public bool AtDoor=>path!=null&&step>=path.Length-1;
        void Awake(){if(!lodGroup)lodGroup=GetComponent<LODGroup>();if(!fullDetailRenderer)fullDetailRenderer=GetComponentInChildren<Renderer>();}
        public void SetFullDetail(bool fullDetail)
        {
            if(lodGroup){lodGroup.ForceLOD(fullDetail?0:1);return;}
            if(silhouetteRenderer){if(fullDetailRenderer)fullDetailRenderer.enabled=fullDetail;silhouetteRenderer.enabled=!fullDetail;}
        }
        public void CheckVisibility(Camera view)
        {
            Vector3 point=transform.position+visibilityOffset;Vector3 delta=point-view.transform.position;
            Seen=Vector3.Angle(view.transform.forward,delta)<=visibilityAngle*.5f &&
                GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(view),new Bounds(point,Vector3.one)) &&
                (!Physics.Raycast(view.transform.position,delta.normalized,out RaycastHit hit,delta.magnitude,
                    Physics.DefaultRaycastLayers,QueryTriggerInteraction.Ignore)||hit.transform.IsChildOf(transform));
            everSeen|=Seen;
        }
        public bool Advance()
        {
            if(Seen||path==null||path.Length==0||AtDoor)return false;
            Vector3 from=transform.position;step++;transform.position=path[step];
            GameEvents.RaiseFigureAdvanced(new(figureId,from,transform.position,step));
            if(everSeen)GameEvents.RaiseStorySignal("figure-advanced");
            if(AtDoor)GameEvents.RaiseFigureAtDoor(this);
            return true;
        }
        public void StepBack(){if(path==null||step<=0)return;step--;transform.position=path[step];}
        public void Divert(Vector3 target,float distance){if(Seen)return;Vector3 flat=target-transform.position;flat.y=0;if(flat.sqrMagnitude>.01f)transform.position+=flat.normalized*Mathf.Min(distance,flat.magnitude);}
        public void ReturnToWater(float duration,float sinkDepth)
        {
            StopAllCoroutines();StartCoroutine(ReturnRoutine(duration,sinkDepth));
        }
        IEnumerator ReturnRoutine(float duration,float sinkDepth)
        {
            Vector3 from=transform.position;Vector3 shore=path!=null&&path.Length>0?path[0]:from;float t=0;
            while(t<duration){t+=Time.deltaTime;transform.position=Vector3.Lerp(from,shore,t/duration);yield return null;}
            from=transform.position;t=0;
            while(t<duration*.5f){t+=Time.deltaTime;transform.position=Vector3.Lerp(from,from+Vector3.down*sinkDepth,t/(duration*.5f));yield return null;}
            gameObject.SetActive(false);
        }
    }
}
