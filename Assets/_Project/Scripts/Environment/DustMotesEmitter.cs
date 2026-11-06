using UnityEngine;

namespace TheLightHouse
{
    [RequireComponent(typeof(ParticleSystem))]
    public class DustMotesEmitter : MonoBehaviour
    {
        [SerializeField] private float boxWidth = 3.5f;
        [SerializeField] private float boxHeight = 2.8f;
        [SerializeField] private int maxParticles = 65;
        [SerializeField] private Color dustTint = new Color(1.0f, 0.88f, 0.65f, 0.45f);

        private void Reset()
        {
            SetupParticles();
        }

        private void Awake()
        {
            SetupParticles();
        }

        public void SetupParticles()
        {
            var ps = GetComponent<ParticleSystem>();
            if (!ps) return;

            var main = ps.main;
            main.loop = true;
            main.playOnAwake = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(4f, 8f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.015f, 0.045f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.015f, 0.035f);
            main.startColor = dustTint;
            main.maxParticles = maxParticles;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = 8f;

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(boxWidth, boxHeight, boxWidth);

            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.x = new ParticleSystem.MinMaxCurve(-0.02f, 0.02f);
            vel.y = new ParticleSystem.MinMaxCurve(-0.01f, 0.02f);
            vel.z = new ParticleSystem.MinMaxCurve(-0.02f, 0.02f);

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(dustTint, 0f), new GradientColorKey(dustTint, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(dustTint.a, 0.35f), new GradientAlphaKey(dustTint.a, 0.65f), new GradientAlphaKey(0f, 1f) }
            );
            colorOverLifetime.color = grad;

            var psr = GetComponent<ParticleSystemRenderer>();
            if (psr && !psr.sharedMaterial)
            {
                var mat = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
                mat.SetColor("_BaseColor", dustTint);
                psr.sharedMaterial = mat;
            }
        }
    }
}
