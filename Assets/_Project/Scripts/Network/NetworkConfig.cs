using UnityEngine;

namespace TheLightHouse
{
    [CreateAssetMenu(menuName="The LightHouse/Network Config",fileName="NetworkConfig")]
    public sealed class NetworkConfig : ScriptableObject
    {
        [SerializeField] bool enabled=true;
        [SerializeField] bool offlineFallback=true;
        [SerializeField] string baseUrl="http://localhost:8080";
        [SerializeField] string webSocketUrl="ws://localhost:8080/ws";
        [SerializeField] string devUsername="daniel";
        [SerializeField] string devPassword="lighthouse-dev";
        [SerializeField] bool autoRegisterDevUser=true;
        [SerializeField,Min(.1f)] float reconnectInitialSeconds=1f;
        [SerializeField,Min(.1f)] float reconnectMaximumSeconds=15f;
        [SerializeField,Min(1f)] float pingIntervalSeconds=10f;
        public bool Enabled=>enabled;public bool OfflineFallback=>offlineFallback;public string BaseUrl=>baseUrl;public string WebSocketUrl=>webSocketUrl;public string DevUsername=>devUsername;public string DevPassword=>devPassword;public bool AutoRegisterDevUser=>autoRegisterDevUser;public float ReconnectInitialSeconds=>reconnectInitialSeconds;public float ReconnectMaximumSeconds=>reconnectMaximumSeconds;public float PingIntervalSeconds=>pingIntervalSeconds;
    }
}
