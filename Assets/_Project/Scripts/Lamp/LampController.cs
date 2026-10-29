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
        [SerializeField] private bool enableNightDrain;
        [SerializeField] private float nightDrainRate = .7f;
        [SerializeField] private float reserveFuelThreshold = 30f;
        private bool nightActive;
        private bool requestedReserve;
        [SerializeField] private StormSettings stormSettings;
        [SerializeField] private bool startInChargeMode;
        [SerializeField] private Transform beam;
        private bool storm;
        private bool stormComplete;
        private float charge,pulseTime,nextPulse;
        [SerializeField] private ActFourSettings actFourSettings;
        private float readingDecayFactor = 1f;
        public bool ChargeMode=>storm;
        public float Charge=>charge;
        private void OnEnable(){GameEvents.GameStateChanged+=HandleState;GameEvents.StoryBeatTriggered+=StormBeat;GameEvents.ReadingStateChanged+=Reading;}
        private void OnDisable(){GameEvents.GameStateChanged-=HandleState;GameEvents.StoryBeatTriggered-=StormBeat;GameEvents.ReadingStateChanged-=Reading;}
        private void Reading(ReadingStateChangedEvent e)=>readingDecayFactor=e.IsReading?e.LampDecayFactor:1f;
        private void HandleState(GameStateChangedEvent e){HandleGameState(e);if(e.Current==GameState.Storm&&stormSettings){charge=stormSettings.initialCharge;GameEvents.RaiseLampChargeChanged(charge);}}
        private void StormBeat(StoryBeatTriggeredEvent e)
        {
            if(e.Id=="A3-B09"&&storm){stormComplete=true;charge=Mathf.Max(charge,stormSettings.healthyThreshold);RefreshState(true);}
            if(e.Id=="A3-B02"&&stormSettings){storm=true;charge=stormSettings.initialCharge;RefreshState(true);}
        }
        public void AddCharge(float amount){if(!storm)return;charge=Mathf.Clamp01(charge+amount);}
        private void HandleGameState(GameStateChangedEvent e) => nightActive = e.Current == GameState.Night;
        public bool TryIgnite()
        {
            if(storm)return false;
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
            if(startInChargeMode&&stormSettings){storm=true;charge=stormSettings.initialCharge;GameEvents.RaiseLampChargeChanged(charge);}
            ignited = !startWarmButUnlit;
            float previousFuel = currentFuel;
            currentFuel = Mathf.Clamp(startingFuel, 0f, fuelCapacity);
            PublishFuelChanged(previousFuel);
            RefreshState(true);
        }

        private void Update()
        {
            if(storm)
            {
                if(!stormComplete)charge=Mathf.Max(0,charge-stormSettings.chargeDecay*readingDecayFactor*Time.deltaTime);
                RefreshState(false);GameEvents.RaiseLampChargeChanged(charge);
                if(beam)beam.Rotate(Vector3.up,stormSettings.beamDegreesPerSecond*charge*Time.deltaTime);
                pulseTime=charge>=stormSettings.pulseThreshold?pulseTime+Time.deltaTime:0f;
                if(pulseTime>=stormSettings.pulseHold&&Time.time>=nextPulse){pulseTime=0;nextPulse=Time.time+stormSettings.pulseCooldown;GameEvents.RaiseLampPulse();GameEvents.RaiseStorySignal("lamp-pulse");}
                return;
            }
            if (!ignited || currentFuel <= 0f || drainRatePerSecond <= 0f)
            {
                RefreshState(false);
                return;
            }

            float previousFuel = currentFuel;
            float drain = enableNightDrain && nightActive ? nightDrainRate : drainRatePerSecond;
            currentFuel = Mathf.Max(0f, currentFuel - drain * Time.deltaTime);
            if (enableNightDrain && nightActive && !requestedReserve && currentFuel <= reserveFuelThreshold)
            {
                requestedReserve = true;
                GameEvents.RaiseStorySignal("fuel-low");
            }
            PublishFuelChanged(previousFuel);
            RefreshState(false);
        }

        public void AddFuel(float amount)
        {
            if(storm)return;
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

            float intensityNormalized = storm ? charge : fuelCapacity > 0f ? currentFuel / fuelCapacity : 0f;
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
            if(storm)return charge<=stormSettings.extinguishedThreshold?LampState.Extinguished:
                charge<stormSettings.healthyThreshold?LampState.Failing:LampState.CrankPowered;
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
