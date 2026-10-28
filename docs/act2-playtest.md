# Act 2 greybox playtest

Run `TheLightHouse/Build Act 2 Scene`, select Navigation, and Bake its NavMeshSurface.
AI Navigation is pinned to 2.0.16: 2.0.9 failed on Unity 6000.6.6f1 because it still called GetInstanceID.
Rebuilding the scene replaces its scene contents; existing StoryBeat assets are reused.
The current test scene starts directly at Night in the lantern room, with 65 fuel units.
Crane stays disabled until the footprints beat. Act 1 still has the dusk/Boatman opening.

## Expected sequence

1. Walk down the staircase without jumping. The open upper-floor stairwell and controller grounding
   must allow both ascent and descent. The boat-shed path stays walkable.
2. Wait until fuel reaches 30 units (about 70 seconds at 0.5 units/second).
   A2-B01 sets **Find more fuel in the storeroom** and reveals the reserve can.
3. Approach the storeroom beside the locked cellar. A2-B02 displays **(distant humming)** and reveals
   the footprints pointing toward the cellar. Inspect the final trail with E to fire A2-B03.
4. Crane appears behind the storeroom, away from the locked door. Make a noise to fire A2-B04.
   He reacts to sound, investigates its last position, searches, and may check a nearby hiding spot.
5. Hide in the wardrobe, under the table or in the crate with E. Use the visible parts of the prop
   as interaction targets. Look is limited to a gap; press E again after releasing it to leave.
   Hold R to suppress breath noise for a finite reserve. Remaining hidden through a breathing interval
   records hide success; StoryDirector waits for earlier beats before publishing A2-B07.
6. Observe a figure through either east window. While the lamp is healthy it must remain still.
   Allow fuel to drop into Failing (20 units), then look away and back: a previously observed figure
   advancing completes A2-B05. Watching it freezes it. Extinguished lamps increase the advance rate.
7. When a figure reaches the front door, A2-B06 starts escalating knocking. The subtitle is
   **(knocking)**; each knock is a noise event. E at the front door warns **Don't open it**,
   keeps it closed and sends that figure one pace back. Healthy light or timeout stops knocking.
8. With the earlier beats complete, successful hiding publishes A2-B07.
9. After A2-B04, singing can periodically display **(singing from the shore)**.
   Moving toward a window/front door during the response window produces an alert noise and requests
   an extra figure advance. Healthy-lamp and visibility protection still take precedence.
10. Let Crane catch you outside hiding. PlayerSpotted precedes the brief look/fade/subtitle sequence.
    You respawn at the checkpoint and Crane returns to Patrol. There is no combat or gore.

All AudioClip slots are empty. Hear no voice, shanty, knock or singing recording until clips are assigned;
subtitles and gameplay noise events provide the placeholders.

## Tuning

- StalkerAI: hearingRange and hearingCurve, closePresenceRadius, catchDistance, hearingCooldown,
  searchDuration, searchRadius, searchPointCount and hidingCheckChance.
- NavMeshAgent: speed, acceleration and stopping distance.
- HidingSpot: dangerRadius, calm/scared/heldBreathLoudness, breathCapacity, breathDrainRate,
  breathRegen, breathRecoveryDelay, breathInterval and peek limits.
- LampController: nightDrainRate, reserveFuelThreshold and failingThresholdNormalized.
- DrownedManager: checkRate, advanceInterval, failing/extinguished/missedDusk multipliers.
- DrownedFigure: visibilityAngle and path positions.
- DoorKnocker: intervals, duration and loudness.
- SingingLure: interval, responseWindow, approachDistance and alertLoudness.
- StalkerAwarenessHint: showHint is off by default. Enabling it shows only an approximate sound direction.

## Validation boundaries

The generated NavMesh includes a complete path from ground floor to the lantern room.
Live Play Mode checks exercised the seven Act 2 beat IDs, NavMesh spawn, sound reaction, hiding entry/exit,
window visibility, figure movement, blocked front door and catch-respawn reset. Tests accelerated fuel
and advance rates in memory and invoked interactions/signals directly; this is not a complete unassisted playthrough.
Manual camera comfort, hiding-gap sightlines, subtitle pacing, hearing difficulty and final audio are still unverified.

The repaired staircase was traversed in Play Mode to (7, 3.04, 5.99) without jumping.
A dedicated visibility check confirmed a figure visible through a window refused to advance,
stayed still with healthy fuel after the player looked away, and advanced unobserved after fuel became low.
