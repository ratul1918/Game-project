using System;
using UnityEngine;

namespace TheLightHouse
{
    public sealed class ServerDirectorApplier : MonoBehaviour
    {
        [SerializeField, Min(.1f)] float timeoutSeconds = 5f;
        [SerializeField, Min(.5f)] float pressureMin = .75f;
        [SerializeField, Min(.5f)] float pressureMax = 1.5f;
        [SerializeField] StormController storm;
        [SerializeField] DrownedManager drowned;
        [SerializeField] StalkerAI stalker;
        [SerializeField] SubtitleUI subtitles;
        long lastSequence;
        float lastMessageTime;
        bool influenced;
        public bool ServerInfluenced=>influenced;
        public float PressureMultiplier{get;private set;}=1f;
        void Awake(){DontDestroyOnLoad(gameObject);}
        void OnEnable(){NetworkRuntimeState.DirectorMessage+=Message;GameEvents.ServerConnectionChanged+=Connection;}
        void OnDisable(){NetworkRuntimeState.DirectorMessage-=Message;GameEvents.ServerConnectionChanged-=Connection;}
        void Update()
        {
            if(influenced&&Time.unscaledTime-lastMessageTime>timeoutSeconds)Fallback("server silent");
        }
        void Connection(ServerConnectionChangedEvent e){if(!e.Connected)Fallback("connection dropped");}
        void Message(string type,string payload,long sequence)
        {
            if(sequence<=lastSequence)return;lastSequence=sequence;lastMessageTime=Time.unscaledTime;
            storm??=FindAnyObjectByType<StormController>();drowned??=FindAnyObjectByType<DrownedManager>();stalker??=FindAnyObjectByType<StalkerAI>();
            switch(type)
            {
                case "STORM_STATE":{var p=JsonUtility.FromJson<StormPayload>(payload);storm?.ApplyServerIntensity(Mathf.Clamp01(p.intensity));influenced=true;break;}
                case "PRESSURE_DELTA":{var p=JsonUtility.FromJson<PressurePayload>(payload);PressureMultiplier=Mathf.Clamp(p.multiplier,pressureMin,pressureMax);drowned?.SetServerPressure(PressureMultiplier);stalker?.SetServerHearingMultiplier(PressureMultiplier);NetworkRuntimeState.SetDirectorState(PressureMultiplier,Latency(payload),true);influenced=true;break;}
                case "RADIO_MESSAGE":{var p=JsonUtility.FromJson<TextPayload>(payload);FindAnyObjectByType<Radio>()?.ReceiveServerMessage(p.id,p.text);break;}
                case "ALERT":{var p=JsonUtility.FromJson<TextPayload>(payload);if(!string.IsNullOrEmpty(p.text))GameEvents.RequestDialogue(new("Alert",p.text));break;}
                case "STORY_BEAT":{var p=JsonUtility.FromJson<IdPayload>(payload);if(!string.IsNullOrEmpty(p.id))Debug.Log("Server story hint: "+p.id);break;}
            }
        }
        void Fallback(string reason)
        {
            if(!influenced)return;influenced=false;storm?.ClearServerIntensity();drowned?.ClearServerPressure();stalker?.ClearServerHearingMultiplier();PressureMultiplier=1f;NetworkRuntimeState.SetDirectorState(1,0,false);Debug.LogWarning("Server director fallback: "+reason);
        }
        static float Latency(string payload){return 0;}
        [Serializable] sealed class StormPayload{public float intensity;public float thunderIn;}
        [Serializable] sealed class PressurePayload{public float multiplier;}
        [Serializable] sealed class TextPayload{public string id;public string text;}
        [Serializable] sealed class IdPayload{public string id;}
    }
}
