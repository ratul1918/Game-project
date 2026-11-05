using UnityEngine;

namespace TheLightHouse
{
    [RequireComponent(typeof(ParticleSystem))]
    public sealed class ReferenceRain : MonoBehaviour
    {
        [SerializeField, Min(0)] float drizzleRate = 90;
        [SerializeField, Min(0)] float stormRate = 220;
        [SerializeField, Min(.1f)] float fadeSeconds = 5;
        ParticleSystem particles;
        float target;
        void Awake() { particles = GetComponent<ParticleSystem>(); target = drizzleRate; }
        void OnEnable() => GameEvents.GameStateChanged += StateChanged;
        void OnDisable() => GameEvents.GameStateChanged -= StateChanged;
        void StateChanged(GameStateChangedEvent change)
        {
            target = change.Current == GameState.Dawn || change.Current == GameState.Ending ? 0 :
                change.Current == GameState.Storm || change.Current == GameState.Finale ? stormRate : drizzleRate;
        }
        void Update()
        {
            var emission = particles.emission;
            emission.rateOverTime = Mathf.MoveTowards(emission.rateOverTime.constant, target, stormRate * Time.deltaTime / fadeSeconds);
        }
    }
}
