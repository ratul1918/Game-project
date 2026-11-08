using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace TheLightHouse.Editor
{
    public static class ArchitectureMeshBuilder
    {
        public static Mesh Strip(string name, float inner, float outer, float start, float end, float y0, float y1, float thickness, int count)
        {
            var v = new List<Vector3>(); var t = new List<int>(); var uv = new List<Vector2>();
            void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
            {
                int k=v.Count; v.AddRange(new[]{a,b,c,d});
                uv.AddRange(new[]{Vector2.zero,new Vector2(Vector3.Distance(a,b),0),new Vector2(Vector3.Distance(a,b),Vector3.Distance(b,c)),new Vector2(0,Vector3.Distance(a,d))});
                t.AddRange(new[]{k,k+2,k+1,k,k+3,k+2});
            }
            Vector3 P(float r,float deg,float y) => new(Mathf.Sin(deg*Mathf.Deg2Rad)*r,y,Mathf.Cos(deg*Mathf.Deg2Rad)*r);
            for(int i=0;i<count;i++)
            {
                float a=Mathf.Lerp(start,end,(float)i/count), b=Mathf.Lerp(start,end,(float)(i+1)/count);
                float ya=Mathf.Lerp(y0,y1,(float)i/count), yb=Mathf.Lerp(y0,y1,(float)(i+1)/count);
                var ia=P(inner,a,ya); var oa=P(outer,a,ya); var ib=P(inner,b,yb); var ob=P(outer,b,yb);
                var down=Vector3.down*thickness;
                Quad(ia,ib,ob,oa); Quad(ia+down,oa+down,ob+down,ib+down);
                Quad(ia,ia+down,ib+down,ib); Quad(oa,ob,ob+down,oa+down);
                if(i==0)Quad(ia,oa,oa+down,ia+down);
                if(i==count-1)Quad(ib,ib+down,ob+down,ob);
            }
            var mesh=new Mesh{name=name};mesh.SetVertices(v);mesh.SetTriangles(t,0);mesh.SetUVs(0,uv);mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
        }
        public static Mesh Persist(Mesh mesh,string path)
        {
            var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(old){EditorUtility.CopySerialized(mesh,old);Object.DestroyImmediate(mesh);EditorUtility.SetDirty(old);return old;}
            AssetDatabase.CreateAsset(mesh,path);return mesh;
        }
    }
}
