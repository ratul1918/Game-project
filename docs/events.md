# Game Event Contracts

## Act 3 additions

- `NoiseHeard: Action<NoiseEmittedEvent>`: only NoiseManager publishes this routed, immutable payload.
  NoiseEmitted remains the raw producer event and its fields/table are unchanged. StalkerAI now consumes NoiseHeard
  in all slices. NoiseManager forwards unchanged when no NoiseMasking is installed.
- `Thunder: Action`: StormController announces thunder after a lightning delay.
  NoiseMasking opens a 3-second window, applying a 0.25 multiplier before NoiseManager forwards player and
  player-operated crank sources to NoiseHeard. Generator, knocking and Crane's own sounds are not masked.
  Example: a 20 m crank pulse becomes a 5 m hearing radius before Crane's hearing curve. Raw debug gizmos remain 20 m.
- `LampChargeChanged: Action<float>`: LampController publishes normalized 0–1 charge for the gauge/end condition.
- `LampPulse: Action`: LampController publishes after charge exceeds 0.85 for 3 seconds, with a 15-second cooldown.
  DrownedManager pushes back unobserved figures by one path point.
- `CrankActiveChanged: Action<bool>`: CrankStation publishes held/released state so arrival can use the existing catch flow.

Storm crank noise = 20 m × serialized crankNoiseCurve(speed), emitted once per second while speed is positive.
Default curve ranges from 0.1 at rest to 1 at maximum speed; zero speed emits nothing.
Charge gain defaults to 0.16 × speed per second, and charge decays by 0.025 per second independently.
Healthy charge ≥ 0.35 maps to CrankPowered; 0.02 < charge < 0.35 maps to Failing; charge ≤ 0.02 maps to Extinguished.
LampStateChanged fields and existing states are preserved. Fuel pickups cannot power the lamp in charge mode.
Generator failure stays at 24 m; restart attempts use 10 m; stair warning footsteps use 6 m.

Storm signals: storm-start, generator-dies, first-crank, storm-darkness, generator-restart-fails,
stalker-stairs, lamp-pulse, courtyard-filling, storm-end. StoryDirector alone publishes their beat IDs.
The contract's watched-figure freeze rule takes precedence over the request to ignore sight when extinguished.

## Act 2 additions

- `CatchRequested: Action`: StalkerAI requests a non-gory catch only after PlayerSpotted.
- `PlayerRespawned: Action`: PlayerHealthState announces checkpoint recovery; StalkerAI returns to Patrol.
- `HidingEntered: Action<HidingEnteredEvent>`: immutable payload with readonly `HidingSpot Spot`. PlayerHealthState owns the hidden state and publishes the existing PlayerHidden payload.
- `HidingExited: Action`: request to leave hiding, from the player or Crane's hiding-spot check.
- `BreathChanged: Action<float>`: normalized hold-breath reserve for HideBreathBar.
- `FigureAtDoor: Action<FigureAtDoorEvent>`: immutable payload with readonly `DrownedFigure Figure` identifying the figure at its last path point.
- `FrontDoorTried: Action`: blocked front-door interaction during Act 2; requests a warning and one figure step back.
- `StalkerStateChanged: Action<StalkerAI.StalkerState>`: state transition notification; logs contain no herding bias information.

New StorySignal IDs: `fuel-low`, `cellar-hum`, `footprints`, `stalker-noise`, `figure-advanced`, `knocking`, `hide-success`.
StoryDirector remains the only publisher of StoryBeatTriggered. Existing payloads are unchanged.
FigureAtDoor and HidingEntered carry identity references for a local interaction handoff; no receivers discover scene objects.

Act 2 noise defaults (radius in metres): calm hidden breathing 1; nearby-Crane breathing 5; held breath 0.1;
front-door knock 10; singing-lure alert 18; locked handle/rattle 5. These use the existing NoiseEmitted payload.
Hold R to reduce breathing for a finite reserve, then recover after a lockout. Hidden footsteps/jumps remain silent.
Crank pulses stay at the existing 20 m; hard landings may be louder. Sound travels by radius and hearing curve,
without wall attenuation in this greybox slice. Crane has no sight and pursues a last-heard position, not an unseen moving player.
Close-presence sensing and confirmed hiding-spot checks can still expose a quiet player.
An extra lure advance obeys observation and healthy-lamp freezing; the lure alert noise can still fire when movement is blocked.

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
| Crouch step | 2 m | Before surface multiplier. |
| Walk step | 6 m | Normal indoor traversal. |
| Sprint step | 12 m | Repeated impulse per footfall. |
| Door opened carefully | 5 m | Standard interaction. |
| Door opened quickly | 10 m | Includes latch and impact. |
| Door slammed | 18 m | Deliberate distraction or AI action. |
| Lamp hand-crank pulse | 20 m | Earlier acts use a fixed pulse; Storm scales this maximum by the speed curve before masking. |
| Dropped small item | 8 m | Metal/stone default. |
| Dropped large item | 14 m | Fuel can or similar prop. |
| Radio | 7 m | Active voice at its world position. |
| Generator failure | 24 m | One-shot story noise. |
| Player landing | 10 m | Default fall/step-down landing. |
| Jump takeoff | 3 m | One impulse when a jump actually succeeds; before surface multiplier. |
| Scaled landing | 10 × impact speed / 6 m | Only at impact speeds of at least 3 m/s; cap base radius at 30 m, then apply surface multiplier. No fall damage. |
| Calm hidden breathing | 1 m | Repeated only while hidden. |
| Scared hidden breathing | 5 m | Crane is within the configured danger radius. |
| Held breath | 0.1 m | While R is held and breath reserve remains. |
| Front door knock | 10 m | Each knock is a separate event. |
| Singing lure alert | 18 m | Only when the player approaches after singing. |

### Surface multipliers

Wood/default: 1.0; Stone (wet stone baseline): 1.2; Metal: 1.5; Water: 1.3; Sand: 0.6.
These multiply step, takeoff and landing radii. Unknown tags use 1.0. AudioClip arrays remain empty.
Examples: sprint on Metal is 18 m; crouch on Sand is 1.2 m; landing at 6 m/s on Wood is 10 m.
Standing still, leaning, intro-locked movement and jumping while hidden emit no movement noise.
Footsteps follow half-cycle crossings of the head-bob phase only while actually moving on the ground.
Disabling visual bob does not change audible gameplay events.

### StaminaChanged

PlayerStamina publishes `Action<StaminaChangedEvent>` via `GameEvents.StaminaChanged`.
The immutable payload contains `float Current`, `float Capacity`, and `bool Exhausted`.
`Exhausted` describes the temporary sprint/jump lockout after reaching zero.
Example subscription: `GameEvents.StaminaChanged += HandleStamina;`; unsubscribe on disable.
No changes were made to the NoiseEmitted payload or existing 2/6/12 m movement bases.
