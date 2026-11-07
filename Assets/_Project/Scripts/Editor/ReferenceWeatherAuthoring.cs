using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace TheLightHouse.Editor
{
    public static class ReferenceWeatherAuthoring
    {
        public static void ApplyToOpenScene()
        {
            var root=GameObject.Find("Reference Visuals");
            var go=new GameObject("Coastal Rain");go.transform.SetParent(root.transform);go.transform.position=new(0,18,-18);
            var ps=go.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=ps.main;main.startSpeed=0;main.startLifetime=1.15f;main.startSize=.018f;main.startColor=new Color(.58f,.72f,.77f,.36f);main.maxParticles=800;main.simulationSpace=ParticleSystemSimulationSpace.World;main.prewarm=true;
            var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=new(48,1f,36);
            var velocity=ps.velocityOverLifetime;velocity.enabled=true;velocity.space=ParticleSystemSimulationSpace.World;velocity.x=-2.2f;velocity.y=-22;velocity.z=1.2f;
            var emission=ps.emission;emission.rateOverTime=90;
            const string path="Assets/_Project/Materials/Reference/Rain.mat";
            var material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!material){material=new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));AssetDatabase.CreateAsset(material,path);}
            material.SetFloat("_Surface",1);material.SetFloat("_Blend",0);material.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);material.SetFloat("_DstBlend",(float)BlendMode.OneMinusSrcAlpha);material.SetFloat("_ZWrite",0);material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");material.renderQueue=3000;material.SetColor("_BaseColor",new Color(.65f,.8f,.85f,.35f));
            var renderer=ps.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=material;renderer.renderMode=ParticleSystemRenderMode.Stretch;renderer.velocityScale=.02f;renderer.lengthScale=1;renderer.cameraVelocityScale=0;
            go.AddComponent<ReferenceRain>();ps.Play();EditorUtility.SetDirty(material);
        }
    }
}
