namespace TheLightHouse
{
    public static class NetworkRuntimeState
    {
        public static string Status{get;private set;}="Offline";
        public static float StormIntensity{get;private set;}
        public static float ServerPressureMultiplier{get;private set;}=1f;
        public static long LastSequence{get;private set;}
        public static float LatencyMilliseconds{get;private set;}
        public static bool ServerInfluenceEnabled{get;private set;}
        public static event System.Action<string,string,long> DirectorMessage;
        public static void SetStatus(string value,bool connected){Status=value;GameEvents.RaiseServerConnectionChanged(new(value,connected));}
        public static void SetStorm(float value,long sequence){StormIntensity=value;GameEvents.RaiseServerStormState(new(value,sequence));}
        public static void RaiseDirectorMessage(string type,string payload,long sequence){LastSequence=sequence;DirectorMessage?.Invoke(type,payload,sequence);}
        public static void SetDirectorState(float pressure,float latency,bool active){ServerPressureMultiplier=pressure;LatencyMilliseconds=latency;ServerInfluenceEnabled=active;}
    }
}
