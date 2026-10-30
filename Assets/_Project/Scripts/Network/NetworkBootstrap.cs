using UnityEngine;

namespace TheLightHouse
{
    public static class NetworkBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Start()
        {
            NetworkConfig config=Resources.Load<NetworkConfig>("NetworkConfig");if(!config||!config.Enabled){NetworkRuntimeState.SetStatus("Offline",false);return;}
            var root=new GameObject("Network Runtime");Object.DontDestroyOnLoad(root);var dispatcher=root.AddComponent<MainThreadDispatcher>();var auth=root.AddComponent<AuthClient>();var client=root.AddComponent<NetworkClient>();client.Configure(config,dispatcher);NetworkRuntimeState.SetStatus("Logging in",false);auth.Authenticate(config,token=>{if(string.IsNullOrEmpty(token))client.UseOfflineFallback("Login failed");else client.Connect(token);});
        }
    }
}
