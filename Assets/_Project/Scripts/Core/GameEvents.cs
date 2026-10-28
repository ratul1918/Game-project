using System;
using UnityEngine;

namespace TheLightHouse
{
    public enum LampState
    {
        Unlit,
        FuelLit,
        CrankPowered,
        Failing,
        Extinguished
    }

    public enum DoorState
    {
        Closed,
        Open,
        Locked,
        Blocked
    }

    public readonly struct NoiseEmittedEvent
    {
        public readonly Vector3 Position;
        public readonly float Loudness;
        public readonly GameObject Source;

        public NoiseEmittedEvent(Vector3 position, float loudness, GameObject source)
        {
            Position = position;
            Loudness = loudness;
            Source = source;
        }
    }

    public readonly struct LampStateChangedEvent
    {
        public readonly LampState Previous;
        public readonly LampState Current;
        public readonly float IntensityNormalized;
        public readonly bool IsLit;

        public LampStateChangedEvent(
            LampState previous,
            LampState current,
            float intensityNormalized,
            bool isLit)
        {
            Previous = previous;
            Current = current;
            IntensityNormalized = intensityNormalized;
            IsLit = isLit;
        }
    }

    public readonly struct FuelChangedEvent
    {
        public readonly float Previous;
        public readonly float Current;
        public readonly float Capacity;
        public readonly float Delta;

        public FuelChangedEvent(float previous, float current, float capacity, float delta)
        {
            Previous = previous;
            Current = current;
            Capacity = capacity;
            Delta = delta;
        }
    }

    public readonly struct FigureAdvancedEvent
    {
        public readonly string FigureId;
        public readonly Vector3 From;
        public readonly Vector3 To;
        public readonly int AdvanceIndex;

        public FigureAdvancedEvent(string figureId, Vector3 from, Vector3 to, int advanceIndex)
        {
            FigureId = figureId;
            From = from;
            To = to;
            AdvanceIndex = advanceIndex;
        }
    }

    public readonly struct PlayerHiddenEvent
    {
        public readonly bool IsHidden;
        public readonly string HidingSpotId;

        public PlayerHiddenEvent(bool isHidden, string hidingSpotId)
        {
            IsHidden = isHidden;
            HidingSpotId = hidingSpotId;
        }
    }

    public readonly struct PlayerSpottedEvent
    {
        public readonly string ObserverId;
        public readonly Vector3 PlayerPosition;

        public PlayerSpottedEvent(string observerId, Vector3 playerPosition)
        {
            ObserverId = observerId;
            PlayerPosition = playerPosition;
        }
    }

    public readonly struct StoryBeatTriggeredEvent
    {
        public readonly string Id;

        public StoryBeatTriggeredEvent(string id)
        {
            Id = id;
        }
    }

    public readonly struct ItemCollectedEvent
    {
        public readonly string Id;

        public ItemCollectedEvent(string id)
        {
            Id = id;
        }
    }

    public readonly struct DoorStateChangedEvent
    {
        public readonly string DoorId;
        public readonly DoorState Previous;
        public readonly DoorState Current;

        public DoorStateChangedEvent(string doorId, DoorState previous, DoorState current)
        {
            DoorId = doorId;
            Previous = previous;
            Current = current;
        }
    }

    public readonly struct DawnReachedEvent
    {
        public readonly float NightElapsedSeconds;

        public DawnReachedEvent(float nightElapsedSeconds)
        {
            NightElapsedSeconds = nightElapsedSeconds;
        }
    }

    public static class GameEvents
    {
        public static event Action<NoiseEmittedEvent> NoiseEmitted;
        public static event Action<LampStateChangedEvent> LampStateChanged;
        public static event Action<FuelChangedEvent> FuelChanged;
        public static event Action<FigureAdvancedEvent> FigureAdvanced;
        public static event Action<PlayerHiddenEvent> PlayerHidden;
        public static event Action<PlayerSpottedEvent> PlayerSpotted;
        public static event Action<StoryBeatTriggeredEvent> StoryBeatTriggered;
        public static event Action<ItemCollectedEvent> ItemCollected;
        public static event Action<DoorStateChangedEvent> DoorStateChanged;
        public static event Action<DawnReachedEvent> DawnReached;

        public static void RaiseNoiseEmitted(NoiseEmittedEvent eventData)
        {
            NoiseEmitted?.Invoke(eventData);
        }

        public static void RaiseLampStateChanged(LampStateChangedEvent eventData)
        {
            LampStateChanged?.Invoke(eventData);
        }

        public static void RaiseFuelChanged(FuelChangedEvent eventData)
        {
            FuelChanged?.Invoke(eventData);
        }
    }
}
