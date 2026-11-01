using UnityEngine;
using UnityEngine.UI;

namespace TheLightHouse
{
    public sealed class DebugHud : MonoBehaviour
    {
        [SerializeField] private LampController lamp;
        [SerializeField] private StalkerAI stalker;
        [SerializeField] private Text fuelText;
        [SerializeField] private Text stalkerStateText;
        [SerializeField] private string fuelFormat = "Fuel: {0:0.0} / {1:0.0} ({2})";
        [SerializeField] private string stalkerFormat = "Stalker: {0}";
        [SerializeField] private string serverFormat = "Server: {0}  Storm: {1:0.000}";
        [SerializeField] private bool showServerInfluence = true;
        [SerializeField] private string influenceFormat = "Influence: {0}  Seq: {1}  Pressure: {2:0.00}  Latency: {3:0}ms";
        [SerializeField] private string telemetryFormat = "Telemetry: sent {0}  queued {1}  dropped {2}";
        string serverStatus="Offline";float serverStorm;
        TelemetryReporter telemetry;
        PlayerDebugPanel movementDebug;
        PerformanceHud performanceHud;

        void OnEnable(){telemetry=FindAnyObjectByType<TelemetryReporter>();movementDebug=FindAnyObjectByType<PlayerDebugPanel>();performanceHud=FindAnyObjectByType<PerformanceHud>();GameEvents.ServerConnectionChanged+=ConnectionChanged;GameEvents.ServerStormState+=StormChanged;serverStatus=NetworkRuntimeState.Status;serverStorm=NetworkRuntimeState.StormIntensity;}
        void OnDisable(){GameEvents.ServerConnectionChanged-=ConnectionChanged;GameEvents.ServerStormState-=StormChanged;}
        void ConnectionChanged(ServerConnectionChangedEvent e)=>serverStatus=e.Status;
        void StormChanged(ServerStormStateEvent e)=>serverStorm=e.Intensity;

        private void Update()
        {
            movementDebug??=FindAnyObjectByType<PlayerDebugPanel>();
            performanceHud??=FindAnyObjectByType<PerformanceHud>();
            bool debugVisible=(movementDebug&&movementDebug.IsVisible)||(performanceHud&&performanceHud.IsVisible);
            if(fuelText)fuelText.enabled=debugVisible;
            if(stalkerStateText)stalkerStateText.enabled=debugVisible;
            if(!debugVisible)return;
            if (fuelText != null && lamp != null)
            {
                fuelText.text = string.Format(
                    fuelFormat,
                    lamp.CurrentFuel,
                    lamp.FuelCapacity,
                    lamp.CurrentState);
            }

            if (stalkerStateText != null && stalker != null)
            {
                stalkerStateText.text = string.Format(stalkerFormat, stalker.StateName)+"\n"+string.Format(serverFormat,serverStatus,serverStorm)+(showServerInfluence?"\n"+string.Format(influenceFormat,NetworkRuntimeState.ServerInfluenceEnabled?"On":"Fallback",NetworkRuntimeState.LastSequence,NetworkRuntimeState.ServerPressureMultiplier,NetworkRuntimeState.LatencyMilliseconds):"")+(telemetry?"\n"+string.Format(telemetryFormat,telemetry.EventsSent,telemetry.QueuedEvents,telemetry.DroppedEvents):"");
            }
            else if(stalkerStateText != null)
            {
                stalkerStateText.text=string.Format(serverFormat,serverStatus,serverStorm)+(showServerInfluence?"\n"+string.Format(influenceFormat,NetworkRuntimeState.ServerInfluenceEnabled?"On":"Fallback",NetworkRuntimeState.LastSequence,NetworkRuntimeState.ServerPressureMultiplier,NetworkRuntimeState.LatencyMilliseconds):"")+(telemetry?"\n"+string.Format(telemetryFormat,telemetry.EventsSent,telemetry.QueuedEvents,telemetry.DroppedEvents):"");
            }
        }
    }
}
