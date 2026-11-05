using System;
using System.Collections;
using UnityEngine;

namespace TheLightHouse
{
    public sealed class ServerEventBridge : MonoBehaviour
    {
        [SerializeField, Min(.1f)] float noiseBatchInterval = .5f;
        NetworkClient client;
        float noiseTotal;
        bool hidden;
        LampState lampState;
        float lampCharge;
        string act = "Act1";
        string checkpoint = "start";
        public event Action<float,float> NoiseBatchReady;
        void Awake(){DontDestroyOnLoad(gameObject);client=FindAnyObjectByType<NetworkClient>();}
        void OnEnable(){GameEvents.NoiseEmitted+=Noise;GameEvents.PlayerHidden+=Hidden;GameEvents.LampStateChanged+=Lamp;GameEvents.StoryBeatTriggered+=Beat;GameEvents.CheckpointReached+=Checkpoint;StartCoroutine(FlushNoise());}
        void OnDisable(){GameEvents.NoiseEmitted-=Noise;GameEvents.PlayerHidden-=Hidden;GameEvents.LampStateChanged-=Lamp;GameEvents.StoryBeatTriggered-=Beat;GameEvents.CheckpointReached-=Checkpoint;StopAllCoroutines();}
        void Noise(NoiseEmittedEvent e)=>noiseTotal+=Mathf.Max(0,e.Loudness);
        void Hidden(PlayerHiddenEvent e){hidden=e.IsHidden;}
        void Lamp(LampStateChangedEvent e){lampState=e.Current;lampCharge=e.IntensityNormalized;Send("LAMP_STATE",JsonUtility.ToJson(new LampPayload{state=lampState.ToString(),charge=lampCharge}));}
        void Beat(StoryBeatTriggeredEvent e){if(e.Id.Length>1&&e.Id[0]=='A')act="Act"+e.Id[1];}
        void Checkpoint(CheckpointReachedEvent e){checkpoint=e.Id;}
        IEnumerator FlushNoise(){while(true){yield return new WaitForSecondsRealtime(noiseBatchInterval);if(noiseTotal>0){float loudness=noiseTotal;NoiseBatchReady?.Invoke(loudness,noiseBatchInterval);Send("NOISE_EMITTED",JsonUtility.ToJson(new NoisePayload{loudness=loudness}));noiseTotal=0;}}}
        void Send(string type,string payload){client??=FindAnyObjectByType<NetworkClient>();client?.Send(type,payload);}
        [Serializable] sealed class NoisePayload{public float loudness;}
        [Serializable] sealed class LampPayload{public string state;public float charge;}
        [Serializable] sealed class BeatPayload{public string id;}
        [Serializable] sealed class CheckpointPayload{public string act;public string checkpoint;}
    }
}
