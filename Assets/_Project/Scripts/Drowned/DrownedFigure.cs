using UnityEngine;
namespace TheLightHouse
{
    public sealed class DrownedFigure : MonoBehaviour
    {
        [SerializeField] string figureId;
        [SerializeField] Vector3[] path;
        [SerializeField] Vector3 visibilityOffset=new(0,1,0);
        [SerializeField] float visibilityAngle=100;
        int step;
        bool everSeen;
        public bool Seen { get; private set; }
        public bool HasBeenSeen=>everSeen;
        public bool AtDoor=>path!=null&&step>=path.Length-1;
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
    }
}
