using UnityEngine;

namespace TheLightHouse
{
    public sealed class LampController : MonoBehaviour
    {
        [Header("Fuel")]
        [SerializeField, Min(0.01f)] private float fuelCapacity = 100f;
        [SerializeField, Min(0f)] private float startingFuel = 25f;
        [SerializeField, Min(0f)] private float drainRatePerSecond = 1f;
        [SerializeField, Range(0f, 1f)] private float failingThresholdNormalized = 0.2f;

        [Header("Light")]
        [SerializeField] private Light lampLight;
        [SerializeField, Min(0f)] private float maximumLightIntensity = 8f;

        private float currentFuel;
        private LampState currentState = LampState.Unlit;
        private bool isCranking;
        [SerializeField] private bool startWarmButUnlit;
        [SerializeField] private bool requireFuelCan;
        [SerializeField] private float minimumIgnitionFuel = 30f;
        private bool ignited;
        public bool TryIgnite()
        {
            if (requireFuelCan && currentFuel < minimumIgnitionFuel) return false;
            ignited=true;
            RefreshState(false);
            return true;
        }

        public float CurrentFuel => currentFuel;
        public float FuelCapacity => fuelCapacity;
        public LampState CurrentState => currentState;

        private void Start()
        {
            ignited = !startWarmButUnlit;
            float previousFuel = currentFuel;
            currentFuel = Mathf.Clamp(startingFuel, 0f, fuelCapacity);
            PublishFuelChanged(previousFuel);
            RefreshState(true);
        }

        private void Update()
        {
            if (!ignited || currentFuel <= 0f || drainRatePerSecond <= 0f)
            {
                RefreshState(false);
                return;
            }

            float previousFuel = currentFuel;
            currentFuel = Mathf.Max(0f, currentFuel - drainRatePerSecond * Time.deltaTime);
            PublishFuelChanged(previousFuel);
            RefreshState(false);
        }

        public void AddFuel(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            float previousFuel = currentFuel;
            currentFuel = Mathf.Clamp(currentFuel + amount, 0f, fuelCapacity);
            PublishFuelChanged(previousFuel);
            RefreshState(false);
        }

        public void SetCranking(bool cranking)
        {
            if (requireFuelCan && !ignited) return;
            if (isCranking == cranking)
            {
                return;
            }

            isCranking = cranking;
            RefreshState(false);
        }

        private void PublishFuelChanged(float previousFuel)
        {
            if (Mathf.Approximately(previousFuel, currentFuel))
            {
                return;
            }

            GameEvents.RaiseFuelChanged(
                new FuelChangedEvent(
                    previousFuel,
                    currentFuel,
                    fuelCapacity,
                    currentFuel - previousFuel));
        }

        private void RefreshState(bool forceEvent)
        {
            LampState nextState = DetermineState();
            LampState previousState = currentState;
            currentState = nextState;

            float intensityNormalized = fuelCapacity > 0f ? currentFuel / fuelCapacity : 0f;
            bool isLit = currentState != LampState.Unlit && currentState != LampState.Extinguished;

            if (lampLight != null)
            {
                lampLight.enabled = isLit;
                lampLight.intensity = maximumLightIntensity * intensityNormalized;
            }

            if (forceEvent || previousState != currentState)
            {
                GameEvents.RaiseLampStateChanged(
                    new LampStateChangedEvent(
                        previousState,
                        currentState,
                        intensityNormalized,
                        isLit));
            }
        }

        private LampState DetermineState()
        {
            if (!ignited) return LampState.Unlit;
            if (currentFuel <= 0f)
            {
                return LampState.Extinguished;
            }

            if (isCranking)
            {
                return LampState.CrankPowered;
            }

            float normalizedFuel = currentFuel / fuelCapacity;
            return normalizedFuel <= failingThresholdNormalized
                ? LampState.Failing
                : LampState.FuelLit;
        }
    }
}
