using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace TheLightHouse
{
    public sealed class TelemetryReporter : MonoBehaviour
    {
        [Serializable] sealed class PendingEvent
        {
            public string type;
            public string payload;
            public int bytes;
        }

        [Serializable] sealed class HiddenPayload
        {
            public bool hiding;
            public float durationSeconds;
        }

        [Serializable] sealed class NoisePayload
        {
            public float loudness;
            public float durationSeconds;
        }

        [Serializable] sealed class BeatPayload { public string id; }
        [Serializable] sealed class CheckpointPayload { public string act; public string checkpoint; }

        [SerializeField, Min(1024)] int maxQueueSerializedBytes = 262144;
        [SerializeField] bool enabledByDefault = true;
        readonly Queue<PendingEvent> pending = new();
        NetworkClient client;
        ServerEventBridge eventBridge;
        SaveSystem saveSystem;
        int pendingBytes;
        float hiddenStartedAt = -1f;
        string act = "Act1";

        public int EventsSent { get; private set; }
        public int QueuedEvents => pending.Count;
        public int DroppedEvents { get; private set; }
        public bool Enabled => saveSystem?.Data?.options?.telemetryEnabled ?? enabledByDefault;

        void Awake()
        {
            DontDestroyOnLoad(gameObject);
            client = FindAnyObjectByType<NetworkClient>();
            saveSystem = FindAnyObjectByType<SaveSystem>();
            AttachNoiseBridge();
        }

        void OnEnable()
        {
            GameEvents.PlayerDeath += Death;
            GameEvents.PlayerHidden += Hidden;
            GameEvents.StoryBeatTriggered += Beat;
            GameEvents.CheckpointReached += Checkpoint;
            GameEvents.ServerConnectionChanged += ConnectionChanged;
            AttachNoiseBridge();
        }

        void OnDisable()
        {
            GameEvents.PlayerDeath -= Death;
            GameEvents.PlayerHidden -= Hidden;
            GameEvents.StoryBeatTriggered -= Beat;
            GameEvents.CheckpointReached -= Checkpoint;
            GameEvents.ServerConnectionChanged -= ConnectionChanged;
            if (eventBridge) eventBridge.NoiseBatchReady -= NoiseBatch;
        }

        void AttachNoiseBridge()
        {
            eventBridge ??= FindAnyObjectByType<ServerEventBridge>();
            if (eventBridge) eventBridge.NoiseBatchReady -= NoiseBatch;
            if (eventBridge) eventBridge.NoiseBatchReady += NoiseBatch;
        }

        void Death() => Report("DEATH", "{}");

        void Hidden(PlayerHiddenEvent value)
        {
            if (value.IsHidden)
            {
                hiddenStartedAt = Time.unscaledTime;
                Report("PLAYER_HID", JsonUtility.ToJson(new HiddenPayload { hiding = true, durationSeconds = 0f }));
                return;
            }

            float duration = hiddenStartedAt < 0f ? 0f : Mathf.Max(0f, Time.unscaledTime - hiddenStartedAt);
            hiddenStartedAt = -1f;
            Report("PLAYER_HID", JsonUtility.ToJson(new HiddenPayload { hiding = false, durationSeconds = duration }));
        }

        void NoiseBatch(float loudness, float durationSeconds) =>
            Report("NOISE_BATCH", JsonUtility.ToJson(new NoisePayload { loudness = loudness, durationSeconds = durationSeconds }));

        void Beat(StoryBeatTriggeredEvent value)
        {
            if (value.Id.Length > 1 && value.Id[0] == 'A') act = "Act" + value.Id[1];
            Report("BEAT_REACHED", JsonUtility.ToJson(new BeatPayload { id = value.Id }));
        }

        void Checkpoint(CheckpointReachedEvent value)
        {
            Report("CHECKPOINT", JsonUtility.ToJson(new CheckpointPayload { act = act, checkpoint = value.Id }));
        }

        void ConnectionChanged(ServerConnectionChangedEvent value)
        {
            if (value.Connected) Flush();
        }

        public void SendTestTelemetry()
        {
            Report("DEATH", "{}", true);
            Report("PLAYER_HID", JsonUtility.ToJson(new HiddenPayload { hiding = false, durationSeconds = 1f }), true);
            Report("NOISE_BATCH", JsonUtility.ToJson(new NoisePayload { loudness = 4.5f, durationSeconds = .5f }), true);
            Report("BEAT_REACHED", JsonUtility.ToJson(new BeatPayload { id = "A1-TEST" }), true);
            Report("CHECKPOINT", JsonUtility.ToJson(new CheckpointPayload { act = "Act1", checkpoint = "TelemetryTest" }), true);
        }

        void Report(string type, string payload, bool force = false)
        {
            if (!force && !Enabled) return;
            client ??= FindAnyObjectByType<NetworkClient>();
            if (client && client.Send(type, payload))
            {
                EventsSent++;
                return;
            }

            Enqueue(type, payload);
        }

        void Enqueue(string type, string payload)
        {
            int bytes = Encoding.UTF8.GetByteCount(type) + Encoding.UTF8.GetByteCount(payload);
            if (bytes > maxQueueSerializedBytes)
            {
                DroppedEvents++;
                return;
            }

            while (pendingBytes + bytes > maxQueueSerializedBytes && pending.Count > 0)
            {
                pendingBytes -= pending.Dequeue().bytes;
                DroppedEvents++;
            }

            pending.Enqueue(new PendingEvent { type = type, payload = payload, bytes = bytes });
            pendingBytes += bytes;
        }

        void Flush()
        {
            client ??= FindAnyObjectByType<NetworkClient>();
            while (pending.Count > 0 && client && client.Send(pending.Peek().type, pending.Peek().payload))
            {
                pendingBytes -= pending.Dequeue().bytes;
                EventsSent++;
            }
        }
    }
}
