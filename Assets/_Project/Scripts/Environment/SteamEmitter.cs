using UnityEngine;

namespace TheLightHouse
{
    public sealed class SteamEmitter : MonoBehaviour
    {
        [SerializeField, Min(0.01f)] private float startLifetime = 1.6f;
        [SerializeField, Min(0.01f)] private float startSpeed = 0.35f;
        [SerializeField, Min(0.01f)] private float startSize = 0.12f;
        [SerializeField] private Color steamColor = new Color(0.85f, 0.88f, 0.92f, 0.35f);

        private ParticleSystem ps;

        private void Awake()
        {
            SetupParticles();
        }

        private void SetupParticles()
        {
            ps = GetComponent<ParticleSystem>();
            if (!ps) ps = gameObject.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.startLifetime = startLifetime;
            main.startSpeed = startSpeed;
            main.startSize = startSize;
            main.startColor = steamColor;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 60;
            main.loop = true;

            var emission = ps.emission;
            emission.rateOverTime = 12f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 14f;
            shape.radius = 0.04f;

            var colOverLife = ps.colorOverLifetime;
            colOverLife.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(steamColor, 0f), new GradientColorKey(Color.white, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(steamColor.a, 0.2f), new GradientAlphaKey(0f, 1f) }
            );
            colOverLife.color = grad;

            var sizeOverLife = ps.sizeOverLifetime;
            sizeOverLife.enabled = true;
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0f, 0.5f);
            curve.AddKey(1f, 2.5f);
            sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f, curve);

            var renderer = GetComponent<ParticleSystemRenderer>();
            if (renderer)
            {
                renderer.renderMode = ParticleSystemRenderMode.Billboard;
                var mat = Resources.Load<Material>("SteamMaterial");
                if (!mat)
                {
                    var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
                    if (shader) mat = new Material(shader);
                }
                if (mat) renderer.sharedMaterial = mat;
            }
        }
    }
}
