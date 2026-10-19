using System;
using Newtonsoft.Json.Linq;
using NativeWebSocket;
using UnityEngine;

namespace EmberBlade.Network
{
    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager Instance { get; private set; }

        [SerializeField] private string websocketUrl = "ws://localhost:8080/ws";

        private WebSocket socket;
        private WebSocketOpenEventHandler openHandler;
        private WebSocketErrorEventHandler errorHandler;
        private WebSocketCloseEventHandler closeHandler;
        private WebSocketMessageEventHandler messageHandler;

        public event Action<WarStateDelta> WarStateDeltaReceived;
        public event Action<RaidAlert> RaidAlertReceived;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public async void Connect(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                Debug.LogError("Cannot connect to Ember Blade server without a token.");
                return;
            }

            if (socket != null)
            {
                DetachSocketHandlers();
                await socket.Close();
            }

            socket = new WebSocket($"{websocketUrl}?token={Uri.EscapeDataString(token)}");
            openHandler = () => Debug.Log("Connected to Ember Blade server.");
            errorHandler = error => Debug.LogWarning($"Ember Blade WebSocket error: {error}");
            closeHandler = code => Debug.Log($"Ember Blade WebSocket disconnected: {code}");
            messageHandler = HandleMessage;
            socket.OnOpen += openHandler;
            socket.OnError += errorHandler;
            socket.OnClose += closeHandler;
            socket.OnMessage += messageHandler;

            try
            {
                await socket.Connect();
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Could not connect to Ember Blade server: {exception.Message}");
            }
        }

        private void Update()
        {
            socket?.DispatchMessageQueue();
        }

        private void HandleMessage(byte[] bytes)
        {
            string json = System.Text.Encoding.UTF8.GetString(bytes);
            try
            {
                JObject envelope = JObject.Parse(json);
                string type = envelope.Value<string>("type");
                JObject payload = envelope["payload"] as JObject;

                switch (type)
                {
                    case "WAR_STATE_DELTA":
                        WarStateDeltaReceived?.Invoke(payload.ToObject<WarStateDelta>());
                        break;
                    case "ALERT_RAID":
                        RaidAlertReceived?.Invoke(payload.ToObject<RaidAlert>());
                        break;
                    default:
                        Debug.LogWarning($"Unknown Ember Blade message type: {type}");
                        break;
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Could not parse Ember Blade message: {exception.Message}");
            }
        }

        private async void OnDestroy()
        {
            if (socket != null)
            {
                DetachSocketHandlers();
                await socket.Close();
            }
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void DetachSocketHandlers()
        {
            if (socket == null)
            {
                return;
            }

            socket.OnOpen -= openHandler;
            socket.OnError -= errorHandler;
            socket.OnClose -= closeHandler;
            socket.OnMessage -= messageHandler;
        }

        [Serializable]
        public class WarStateDelta
        {
            public string front;
            public int value;
        }

        [Serializable]
        public class RaidAlert
        {
            public string front;
            public string message;
        }
    }
}
