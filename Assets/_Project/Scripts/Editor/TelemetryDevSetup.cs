using UnityEditor;
using UnityEngine;

namespace TheLightHouse.Editor
{
    public static class TelemetryDevSetup
    {
        [MenuItem("TheLightHouse/Send Test Telemetry")]
        static void SendTestTelemetry()
        {
            if (!EditorApplication.isPlaying)
            {
                Debug.LogWarning("Enter Play Mode before sending test telemetry.");
                return;
            }

            var reporter = Object.FindAnyObjectByType<TelemetryReporter>();
            if (reporter == null)
            {
                Debug.LogError("TelemetryReporter is not present in the running scene.");
                return;
            }

            reporter.SendTestTelemetry();
            Debug.Log("Sent test telemetry events.");
        }
    }
}
