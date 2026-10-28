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
                stalkerStateText.text = string.Format(stalkerFormat, stalker.StateName);
            }
        }
    }
}
