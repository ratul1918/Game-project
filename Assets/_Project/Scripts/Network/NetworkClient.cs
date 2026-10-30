using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace TheLightHouse
{
    public sealed class NetworkClient : MonoBehaviour
    {
        [Serializable] sealed class StormPayload{public float intensity;public string serverTime;}
        [Serializable] sealed class SocketEnvelope{public string type;public long seq;public StormPayload payload;}
        MainThreadDispatcher dispatcher;CancellationTokenSource lifetime;ClientWebSocket socket;bool loggedOffline,offlineFallback;string webSocketUrl;float reconnectInitialSeconds,reconnectMaximumSeconds,pingIntervalSeconds;
        public void Configure(NetworkConfig value,MainThreadDispatcher mainThread){dispatcher=mainThread;offlineFallback=value.OfflineFallback;webSocketUrl=value.WebSocketUrl;reconnectInitialSeconds=value.ReconnectInitialSeconds;reconnectMaximumSeconds=value.ReconnectMaximumSeconds;pingIntervalSeconds=value.PingIntervalSeconds;lifetime=new CancellationTokenSource();}
        public void Connect(string token)=>_ = ConnectLoop(token,lifetime.Token);
        public void UseOfflineFallback(string reason)
        {
            dispatcher.Enqueue(()=>{NetworkRuntimeState.SetStatus("Offline",false);if(offlineFallback&&!loggedOffline){loggedOffline=true;Debug.LogWarning("Network unavailable ("+reason+"). Continuing with offline gameplay.");}});
        }
        async Task ConnectLoop(string token,CancellationToken cancellation)
        {
            float delay=reconnectInitialSeconds;
            while(!cancellation.IsCancellationRequested)
            {
                try
                {
                    dispatcher.Enqueue(()=>NetworkRuntimeState.SetStatus("Connecting",false));socket=new ClientWebSocket();var uri=new Uri(webSocketUrl+"?token="+Uri.EscapeDataString(token));await socket.ConnectAsync(uri,cancellation);loggedOffline=false;dispatcher.Enqueue(()=>NetworkRuntimeState.SetStatus("Online",true));delay=reconnectInitialSeconds;
                    Task receive=ReceiveLoop(socket,cancellation);Task ping=PingLoop(socket,cancellation);await receive;await ping;
                }
                catch(OperationCanceledException) when(cancellation.IsCancellationRequested){break;}
                catch(Exception error){UseOfflineFallback(error.GetType().Name);}
                finally{socket?.Dispose();socket=null;}
                try{await Task.Delay(TimeSpan.FromSeconds(delay),cancellation);}catch(OperationCanceledException){break;}delay=Math.Min(delay*2f,reconnectMaximumSeconds);
            }
        }
        async Task ReceiveLoop(ClientWebSocket active,CancellationToken cancellation)
        {
            byte[] buffer=new byte[4096];using var stream=new MemoryStream();while(active.State==WebSocketState.Open&&!cancellation.IsCancellationRequested){WebSocketReceiveResult result=await active.ReceiveAsync(new ArraySegment<byte>(buffer),cancellation);if(result.MessageType==WebSocketMessageType.Close)break;stream.Write(buffer,0,result.Count);if(!result.EndOfMessage)continue;string message=Encoding.UTF8.GetString(stream.ToArray());stream.SetLength(0);dispatcher.Enqueue(()=>HandleMessage(message));}
        }
        async Task PingLoop(ClientWebSocket active,CancellationToken cancellation)
        {
            byte[] ping=Encoding.UTF8.GetBytes("{\"type\":\"PING\",\"seq\":0,\"payload\":{}}");while(active.State==WebSocketState.Open&&!cancellation.IsCancellationRequested){await Task.Delay(TimeSpan.FromSeconds(pingIntervalSeconds),cancellation);if(active.State==WebSocketState.Open)await active.SendAsync(new ArraySegment<byte>(ping),WebSocketMessageType.Text,true,cancellation);}
        }
        void HandleMessage(string json)
        {
            SocketEnvelope envelope=JsonUtility.FromJson<SocketEnvelope>(json);if(envelope==null)return;if(envelope.type=="STORM_STATE"&&envelope.payload!=null)NetworkRuntimeState.SetStorm(Mathf.Clamp01(envelope.payload.intensity),envelope.seq);
        }
        async void OnDestroy(){if(lifetime==null)return;lifetime.Cancel();if(socket!=null&&socket.State==WebSocketState.Open)try{await socket.CloseAsync(WebSocketCloseStatus.NormalClosure,"shutdown",CancellationToken.None);}catch(Exception){}lifetime.Dispose();}
    }
}
