using UnityEngine;

namespace TheLightHouse
{
    public sealed class FogCards : MonoBehaviour
    {
        [SerializeField] Transform[] anchors;
        [SerializeField] Material fogMaterial;
        [SerializeField,Range(1,24)] int particlesPerLayer=8;
        [SerializeField,Min(1f)] float layerWidth=18f;
        [SerializeField,Min(.1f)] float layerHeight=3f;
        [SerializeField] Vector2 particleSizeRange=new(3f,7f);
        [SerializeField,Min(.1f)] float lifetime=18f;
        [SerializeField,Min(0f)] float scrollSpeed=.12f;
        [SerializeField,Range(0f,1f)] float opacity=.14f;

        void Start()
        {
            if(anchors==null)return;
            foreach(Transform anchor in anchors)if(anchor)CreateLayer(anchor);
        }

        void CreateLayer(Transform anchor)
        {
            var layer=new GameObject("Fog Layer",typeof(ParticleSystem),typeof(ParticleSystemRenderer));layer.transform.SetParent(anchor,false);
            var particles=layer.GetComponent<ParticleSystem>();var main=particles.main;main.loop=true;main.startLifetime=lifetime;main.startSpeed=scrollSpeed;main.startSize3D=true;main.startSizeX=new ParticleSystem.MinMaxCurve(particleSizeRange.x,particleSizeRange.y);main.startSizeY=new ParticleSystem.MinMaxCurve(particleSizeRange.x*.35f,particleSizeRange.y*.35f);main.startColor=new Color(1f,1f,1f,opacity);main.maxParticles=particlesPerLayer;
            var emission=particles.emission;emission.rateOverTime=particlesPerLayer/lifetime;
            var shape=particles.shape;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=new Vector3(layerWidth,layerHeight,.5f);
            var velocity=particles.velocityOverLifetime;velocity.enabled=true;velocity.x=scrollSpeed;
            var renderer=layer.GetComponent<ParticleSystemRenderer>();renderer.renderMode=ParticleSystemRenderMode.Billboard;renderer.material=fogMaterial;renderer.sortingFudge=-1f;
            particles.Play();
        }
    }
}
