using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Object=UnityEngine.Object;
namespace TheLightHouse.Editor
{
    public static class ArchitectureTraversalValidation
    {
        public static string Inspect()
        {
            Physics.SyncTransforms();var results=new HashSet<string>();int samples=0;
            void Check(Vector3 feet,float height,string label)
            {
                samples++;
                foreach(var c in Physics.OverlapCapsule(feet+Vector3.up*.36f,feet+Vector3.up*(height-.3f),.28f,~0,QueryTriggerInteraction.Ignore))
                {
                    if(c.GetComponentInParent<FirstPersonController>()||c.GetComponentInParent<Door>()||c.GetComponentInParent<HiddenCellarRoom>())continue;
                    results.Add(label+": "+c.name);
                }
                if(!Physics.Raycast(feet+Vector3.up*.12f,Vector3.down,.35f,~0,QueryTriggerInteraction.Ignore))results.Add(label+": missing floor");
            }
            float[] start={225,105,345},end={435,315,525};
            for(int f=0;f<3;f++)for(int i=0;i<=80;i++)
            {
                float t=i/80f,a=Mathf.Lerp(start[f],end[f],t)*Mathf.Deg2Rad,y=Mathf.Lerp(ArchitectureCollisionOverhaul.Levels[f],ArchitectureCollisionOverhaul.Levels[f+1],t);
                Check(new Vector3(Mathf.Sin(a)*3.925f,y,Mathf.Cos(a)*3.925f),2.6f,$"Flight {f+1} sample {i}");
            }
            for(int k=0;k<4;k++)for(int i=0;i<=20;i++)
            {
                float a=ArchitectureCollisionOverhaul.Doors[k]*Mathf.Deg2Rad,r=Mathf.Lerp(2.35f,4.1f,i/20f);
                Check(new Vector3(Mathf.Sin(a)*r,ArchitectureCollisionOverhaul.Levels[k],Mathf.Cos(a)*r),2.05f,$"Room {k+1} approach {i}");
            }
            for(int i=0;i<=50;i++)Check(Vector3.Lerp(new(-8,.15f,-4.8f),new(-8,-4.35f,1.5f),i/50f),2.6f,$"Cellar stair {i}");
            for(int i=0;i<=60;i++)Check(new(-8,-4.35f,Mathf.Lerp(1.5f,9.5f,i/60f)),2.05f,$"Bunker route {i}");
            foreach(var door in Object.FindObjectsByType<Door>(FindObjectsSortMode.None))
            {
                var leaf=door.GetComponentsInChildren<Collider>().FirstOrDefault(c=>c.enabled&&!c.isTrigger);
                if(!leaf || door.name.ToLower().Contains("window") || door.name.ToLower().Contains("shutter"))continue;
                var center=leaf.bounds.center;float floor=leaf.bounds.min.y;
                for(int i=0;i<9;i++)
                {
                    var p=center+door.transform.forward*Mathf.Lerp(-1f,1f,i/8f);
                    if(Physics.Raycast(new Vector3(p.x,floor+0.35f,p.z),Vector3.down,out var hit,2.0f,~0,QueryTriggerInteraction.Ignore))
                        p.y=hit.point.y;
                    else
                        p.y=floor;
                    Check(p,1.9f,$"Door {door.name} approach {i}");
                }
            }
            return $"{samples} clearance/floor samples; {results.Count} issues\n"+string.Join("\n",results);
        }
    }
}
