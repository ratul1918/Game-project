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

    public readonly struct PageReadEvent
    {
        public readonly string Id;
        public readonly string StoryFlag;
        public PageReadEvent(string id, string storyFlag) { Id = id; StoryFlag = storyFlag; }
    }

    public readonly struct LedgerReadEvent
    {
        public readonly string EntryId;
        public LedgerReadEvent(string entryId) { EntryId = entryId; }
    }

    public readonly struct ReadingStateChangedEvent
    {
        public readonly bool IsReading;
        public readonly float ThreatSpeed;
        public readonly float LampDecayFactor;
        public ReadingStateChangedEvent(bool isReading, float threatSpeed, float lampDecayFactor)
        { IsReading = isReading; ThreatSpeed = threatSpeed; LampDecayFactor = lampDecayFactor; }
    }

    public static class GameEvents
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSessionEvents()
        {
            NoiseHeard = null;
            Thunder = null;
            LampChargeChanged = null;
            LampPulse = null;
            CrankActiveChanged = null;
            StaminaChanged = null;
            CatchRequested = null;
            BreathChanged = null;
            PlayerRespawned = null;
            HidingEntered = null;
            HidingExited = null;
            FigureAtDoor = null;
            FrontDoorTried = null;
            StalkerStateChanged = null;
            GameStateChanged = null;
            LampNotLitInTime = null;
            StorySignal = null;
            ObjectiveChanged = null;
            DialogueRequested = null;
            NoiseEmitted = null;
            LampStateChanged = null;
            FuelChanged = null;
            FigureAdvanced = null;
            PlayerHidden = null;
            PlayerSpotted = null;
            StoryBeatTriggered = null;
            ItemCollected = null;
            DoorStateChanged = null;
            DawnReached = null;
            PageRead = null;
            LedgerRead = null;
            ReadingStateChanged = null;
        }
        public static event Action<NoiseEmittedEvent> NoiseHeard;
        public static event Action Thunder;
        public static event Action<float> LampChargeChanged;
        public static event Action LampPulse;
        public static event Action<bool> CrankActiveChanged;
        public static void RaiseCrankActiveChanged(bool active) => CrankActiveChanged?.Invoke(active);
        public static void RaiseNoiseHeard(NoiseEmittedEvent e) => NoiseHeard?.Invoke(e);
        public static void RaiseThunder() => Thunder?.Invoke();
        public static void RaiseLampChargeChanged(float value) => LampChargeChanged?.Invoke(value);
        public static void RaiseLampPulse() => LampPulse?.Invoke();
        public static event Action<StaminaChangedEvent> StaminaChanged;
        public static event Action CatchRequested;
        public static event Action<float> BreathChanged;
        public static void RaiseBreathChanged(float normalized) => BreathChanged?.Invoke(normalized);
        public static event Action PlayerRespawned;
        public static event Action<HidingEnteredEvent> HidingEntered;
        public static event Action HidingExited;
        public static event Action<FigureAtDoorEvent> FigureAtDoor;
        public static event Action FrontDoorTried;
        public static event Action<StalkerAI.StalkerState> StalkerStateChanged;
        public static void RaiseCatchRequested() => CatchRequested?.Invoke();
        public static void RaisePlayerRespawned() => PlayerRespawned?.Invoke();
        public static void RaiseHidingEntered(HidingSpot spot) => HidingEntered?.Invoke(new(spot));
        public static void RaiseHidingExited() => HidingExited?.Invoke();
        public static void RaiseFigureAtDoor(DrownedFigure figure) => FigureAtDoor?.Invoke(new(figure));
        public static void RaiseFrontDoorTried() => FrontDoorTried?.Invoke();
        public static void RaiseStalkerStateChanged(StalkerAI.StalkerState state) => StalkerStateChanged?.Invoke(state);
        public static void RaisePlayerSpotted(PlayerSpottedEvent e) => PlayerSpotted?.Invoke(e);
        public static void RaiseFigureAdvanced(FigureAdvancedEvent e) => FigureAdvanced?.Invoke(e);
        public static void RaiseStaminaChanged(StaminaChangedEvent e) => StaminaChanged?.Invoke(e);
        public static void RaisePlayerHidden(PlayerHiddenEvent e) => PlayerHidden?.Invoke(e);
        public static event Action<GameStateChangedEvent> GameStateChanged;
        public static event Action LampNotLitInTime;
        public static event Action<string> StorySignal;
        public static event Action<string> ObjectiveChanged;
        public static event Action<DialogueLine> DialogueRequested;
        public static void RaiseGameStateChanged(GameStateChangedEvent e) => GameStateChanged?.Invoke(e);
        public static void RaiseLampNotLitInTime() => LampNotLitInTime?.Invoke();
        public static void RaiseStorySignal(string id) => StorySignal?.Invoke(id);
        public static void RaiseObjectiveChanged(string text) => ObjectiveChanged?.Invoke(text);
        public static void RequestDialogue(DialogueLine line) => DialogueRequested?.Invoke(line);
        public static void RaiseStoryBeatTriggered(StoryBeatTriggeredEvent e) => StoryBeatTriggered?.Invoke(e);
        public static void RaiseItemCollected(ItemCollectedEvent e) => ItemCollected?.Invoke(e);
        public static void RaiseDoorStateChanged(DoorStateChangedEvent e) => DoorStateChanged?.Invoke(e);
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
        public static event Action<PageReadEvent> PageRead;
        public static event Action<LedgerReadEvent> LedgerRead;
        public static event Action<ReadingStateChangedEvent> ReadingStateChanged;
        public static void RaisePageRead(PageReadEvent e) => PageRead?.Invoke(e);
        public static void RaiseLedgerRead(LedgerReadEvent e) => LedgerRead?.Invoke(e);
        public static void RaiseReadingStateChanged(ReadingStateChangedEvent e) => ReadingStateChanged?.Invoke(e);

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
    public readonly struct GameStateChangedEvent
    {
        public readonly GameState Previous;
        public readonly GameState Current;
        public GameStateChangedEvent(GameState previous, GameState current) { Previous=previous; Current=current; }
    }
    public readonly struct StaminaChangedEvent
    {
        public readonly float Current;
        public readonly float Capacity;
        public readonly bool Exhausted;
        public StaminaChangedEvent(float current, float capacity, bool exhausted)
        {
            Current = current; Capacity = capacity; Exhausted = exhausted;
        }
    }
    public readonly struct HidingEnteredEvent
    {
        public readonly HidingSpot Spot;
        public HidingEnteredEvent(HidingSpot spot) { Spot=spot; }
    }
    public readonly struct FigureAtDoorEvent
    {
        public readonly DrownedFigure Figure;
        public FigureAtDoorEvent(DrownedFigure figure) { Figure=figure; }
    }
}
