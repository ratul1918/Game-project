using UnityEngine;
namespace TheLightHouse {
public sealed class WetFootprints : MonoBehaviour {
[SerializeField] int count=8;
[SerializeField] float spacing=.45f;
[SerializeField] float width=.12f;
[SerializeField] float length=.28f;
[SerializeField] float alternatingOffset=.12f;
public void Reveal(){if(transform.childCount>0)return;for(int i=0;i<count;i++){var foot=GameObject.CreatePrimitive(PrimitiveType.Quad);Destroy(foot.GetComponent<Collider>());foot.transform.SetParent(transform,false);foot.transform.localPosition=new(i%2==0?alternatingOffset:-alternatingOffset,.02f,i*spacing);foot.transform.localRotation=Quaternion.Euler(90,0,0);foot.transform.localScale=new(width,length,1);}}
}}
