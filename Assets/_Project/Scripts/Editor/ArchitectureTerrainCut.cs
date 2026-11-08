using System.Collections.Generic;
using UnityEngine;
namespace TheLightHouse.Editor
{
    public static class ArchitectureTerrainCut
    {
        struct Vertex { public Vector3 p; public Vector2 uv; public Vertex(Vector3 p,Vector2 uv){this.p=p;this.uv=uv;} }
        public static Mesh Cut(Mesh source,Transform owner,Rect opening)
        {
            var verts=new List<Vector3>();var uv=new List<Vector2>();var tris=new List<int>();
            var points=source.vertices;var tex=source.uv;var indices=source.triangles;
            List<Vertex> Clip(List<Vertex> input,int axis,float edge,bool greater)
            {
                var output=new List<Vertex>();
                if(input.Count==0)return output;
                float D(Vertex p)=>(axis==0?p.p.x:p.p.z)-edge;
                for(int i=0;i<input.Count;i++)
                {
                    var a=input[i];var b=input[(i+1)%input.Count];float da=D(a),db=D(b);
                    bool ina=greater?da>=0:da<=0,inb=greater?db>=0:db<=0;
                    if(ina)output.Add(a);
                    if(ina!=inb){float t=da/(da-db);output.Add(new Vertex(Vector3.Lerp(a.p,b.p,t),Vector2.Lerp(a.uv,b.uv,t)));}
                }
                return output;
            }
            void Emit(List<Vertex> poly)
            {
                for(int i=1;i<poly.Count-1;i++)foreach(var v in new[]{poly[0],poly[i],poly[i+1]})
                {tris.Add(verts.Count);verts.Add(owner.InverseTransformPoint(v.p));uv.Add(v.uv);}
            }
            for(int i=0;i<indices.Length;i+=3)
            {
                var polygon=new List<Vertex>();for(int j=0;j<3;j++){int k=indices[i+j];polygon.Add(new Vertex(owner.TransformPoint(points[k]),tex.Length==points.Length?tex[k]:Vector2.zero));}
                foreach(var edge in new[]{(0,opening.xMin,true),(0,opening.xMax,false),(2,opening.yMin,true),(2,opening.yMax,false)})
                {Emit(Clip(polygon,edge.Item1,edge.Item2,!edge.Item3));polygon=Clip(polygon,edge.Item1,edge.Item2,edge.Item3);}
            }
            var mesh=new Mesh{name="Island with cellar opening",indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};mesh.SetVertices(verts);mesh.SetUVs(0,uv);mesh.SetTriangles(tris,0);mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
        }
    }
}
