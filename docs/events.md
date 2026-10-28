# Game Event Contracts

## Act 1 additions

All existing payloads remain unchanged. Fuel is measured in authored units (capacity defaults to 100).

- `GameStateChanged: Action<GameStateChangedEvent>`: GameStateMachine publishes immutable `Previous` and `Current` GameState fields after transition.
- `LampNotLitInTime: Action`: DuskTimer publishes once when dusk expires without a lit lamp. For Act 1 it logs and requests a warning subtitle.
- `StorySignal: Action<string>`: interaction/sequence components request evaluation using stable signal IDs (`arrival`, `dock`, `meal`, `radio`, `find-fuel`). Only StoryDirector commits beat IDs.
- `DialogueRequested: Action<DialogueLine>`: queues speaker, text, optional AudioClip and fallback duration in SubtitleUI.
- `ObjectiveChanged: Action<string>`: StoryDirector or an unsuccessful lamp interaction requests objective text.

Example: `GameEvents.RaiseStorySignal("meal");` requests evaluation; it does not directly complete a story beat.

This document is the source of truth for cross-system communication. Event payloads are immutable value types. Publishers raise events through a single event hub; subscribers subscribe and unsubscribe in matching Unity lifecycle methods. Scene objects do not discover one another to bypass these contracts.

## Shared types

```csharp
namespace TheLightHouse
{
    public enum LampState { Unlit, FuelLit, CrankPowered, Failing, Extinguished }
    public enum DoorState { Closed, Open, Locked, Blocked }
}
```

Identifiers are stable serialized strings, not display text. Positions are world-space `Vector3` values. Loudness is a non-negative gameplay radius in meters, not decibels.

## Event hub signature

```csharp
using System;

namespace TheLightHouse
{
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
    }
}
```

The implementation may expose narrowly named `Raise...` methods, but only the owning system publishes each event. Public code never invokes C# events directly.

## Payload specifications

### `NoiseEmitted`

Published by Player, Interaction, Lamp, or physics-backed item systems. Consumed primarily by `NoiseManager`, which decides which listeners can hear it.

```csharp
public readonly struct NoiseEmittedEvent
{
    public readonly Vector3 Position;
    public readonly float Loudness;
    public readonly GameObject Source;
}
```

Example: `GameEvents.RaiseNoiseEmitted(new(position, 18f, gameObject));`

### `LampStateChanged`

Published by Lamp when its discrete operating state changes.

```csharp
public readonly struct LampStateChangedEvent
{
    public readonly LampState Previous;
    public readonly LampState Current;
    public readonly float IntensityNormalized;
    public readonly bool IsLit;
}
```

Example: `GameEvents.RaiseLampStateChanged(new(previous, current, intensity, isLit));`

### `FuelChanged`

Published by Lamp after fuel is consumed, added, or set. Values use one consistent project unit selected during Step 3.

```csharp
public readonly struct FuelChangedEvent
{
    public readonly float Previous;
    public readonly float Current;
    public readonly float Capacity;
    public readonly float Delta;
}
```

Example: `GameEvents.RaiseFuelChanged(new(previous, current, capacity, current - previous));`

### `FigureAdvanced`

Published by `DrownedManager` after one pale figure completes an advance.

```csharp
public readonly struct FigureAdvancedEvent
{
    public readonly string FigureId;
    public readonly Vector3 From;
    public readonly Vector3 To;
    public readonly int AdvanceIndex;
}
```

Example: `GameEvents.RaiseFigureAdvanced(new("shallows-west-01", from, to, 2));`

### `PlayerHidden`

Published by Player when the authoritative hidden state changes.

```csharp
public readonly struct PlayerHiddenEvent
{
    public readonly bool IsHidden;
    public readonly string HidingSpotId;
}
```

Example: `GameEvents.RaisePlayerHidden(new(true, "cellar-coal-bin"));`

### `PlayerSpotted`

Published by an observing AI when it confirms the player is visible.

```csharp
public readonly struct PlayerSpottedEvent
{
    public readonly string ObserverId;
    public readonly Vector3 PlayerPosition;
}
```

Example: `GameEvents.RaisePlayerSpotted(new("crane-stalker", playerPosition));`

### `StoryBeatTriggered`

Published only by `StoryDirector` after it validates a beat's prerequisites. Each ID must exist in `docs/story-beats.md`.

```csharp
public readonly struct StoryBeatTriggeredEvent
{
    public readonly string Id;
}
```

Example: `GameEvents.RaiseStoryBeatTriggered(new("A1-B04"));`

### `ItemCollected`

Published by Interaction after a collectible enters player ownership or is consumed as a story pickup.

```csharp
public readonly struct ItemCollectedEvent
{
    public readonly string Id;
}
```

Example: `GameEvents.RaiseItemCollected(new("cellar-fuel-can-01"));`

### `DoorStateChanged`

Published by the door interaction when its authoritative state changes.

```csharp
public readonly struct DoorStateChangedEvent
{
    public readonly string DoorId;
    public readonly DoorState Previous;
    public readonly DoorState Current;
}
```

Example: `GameEvents.RaiseDoorStateChanged(new("tower-front-door", previous, current));`

### `DawnReached`

Published once by `StoryDirector` when the Finale-to-Dawn transition is committed.

```csharp
public readonly struct DawnReachedEvent
{
    public readonly float NightElapsedSeconds;
}
```

Example: `GameEvents.RaiseDawnReached(new(nightElapsedSeconds));`

## Subscription example

```csharp
private void OnEnable() => GameEvents.NoiseEmitted += HandleNoise;
private void OnDisable() => GameEvents.NoiseEmitted -= HandleNoise;
```

## Noise loudness table

Initial greybox tuning values; change this table before changing implementation defaults.

| Source | Loudness radius | Notes |
|---|---:|---|
| Crouch step | 2 m | Quiet baseline; surface modifiers may apply later. |
| Walk step | 6 m | Normal indoor traversal. |
| Sprint step | 12 m | Repeated impulse per footfall. |
| Door opened carefully | 5 m | Standard interaction. |
| Door opened quickly | 10 m | Includes latch and impact. |
| Door slammed | 18 m | Deliberate distraction or AI action. |
| Lamp hand-crank pulse | 20 m | Emitted on each meaningful crank stroke. |
| Dropped small item | 8 m | Metal/stone default. |
| Dropped large item | 14 m | Fuel can or similar prop. |
| Radio | 7 m | Active voice at its world position. |
| Generator failure | 24 m | One-shot story noise. |
| Player landing | 10 m | Default fall/step-down landing. |
