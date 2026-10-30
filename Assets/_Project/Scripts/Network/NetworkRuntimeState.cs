namespace TheLightHouse
{
    public static class NetworkRuntimeState
    {
        public static string Status{get;private set;}="Offline";
        public static float StormIntensity{get;private set;}
        public static void SetStatus(string value,bool connected){Status=value;GameEvents.RaiseServerConnectionChanged(new(value,connected));}
        public static void SetStorm(float value,long sequence){StormIntensity=value;GameEvents.RaiseServerStormState(new(value,sequence));}
    }
}
