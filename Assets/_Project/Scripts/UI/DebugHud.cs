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
        string serverStatus="Offline";float serverStorm;

        void OnEnable(){GameEvents.ServerConnectionChanged+=ConnectionChanged;GameEvents.ServerStormState+=StormChanged;serverStatus=NetworkRuntimeState.Status;serverStorm=NetworkRuntimeState.StormIntensity;}
        void OnDisable(){GameEvents.ServerConnectionChanged-=ConnectionChanged;GameEvents.ServerStormState-=StormChanged;}
        void ConnectionChanged(ServerConnectionChangedEvent e)=>serverStatus=e.Status;
        void StormChanged(ServerStormStateEvent e)=>serverStorm=e.Intensity;

        private void Update()
        {
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
                stalkerStateText.text = string.Format(stalkerFormat, stalker.StateName)+"\n"+string.Format(serverFormat,serverStatus,serverStorm);
            }
            else if(stalkerStateText != null)
            {
                stalkerStateText.text=string.Format(serverFormat,serverStatus,serverStorm);
            }
        }
    }
}
