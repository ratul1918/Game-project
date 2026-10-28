# Act 3 storm playtest

Build with TheLightHouse/Build Act 3 Scene, bake Navigation/NavMeshSurface, save, then Play.
The scene begins in Storm with eight figures, six hiding spots, and a charged lamp.
It reuses Act 2 authoring without overwriting Act1.unity or Act2.unity.
AI Navigation 2.0.16 and Unity 6000.6.6f1 are the tested assumptions.

## Contracts and sequence

Primary beats: A3-B01 storm → A3-B02 generator failure → A3-B03 effective crank → A3-B04 darkness causes an advance.
Conditional beats can occur in player-dependent order:
A3-B05 restart fails (after B02), B06 stair warning (after B03), B07 first pulse (after B03),
B08 courtyard gathering (after B04), B09 storm ending (after B04).

After the initial generator delay and flicker, inspect its flooded restart in the downstairs generator room.
It emits 10 m restart noise and a failure subtitle; it cannot restore power.
Return upstairs and hold E at the crank. Speed ramps, the handle turns, and charge raises lamp brightness
and the placeholder beam's rotation speed. Releasing E coasts the handle/noise but stops energy gain.
Charge continues to drain while hiding. The lamp-mounted gauge is the default charge display; its screen bar is off.

Thunder opens a three-second mask. A 20 m crank pulse is routed through NoiseManager as 5 m NoiseHeard;
raw NoiseEmitted and gizmos retain the original value. No second copy is heard.
Crane hears this routed signal in older scenes too, forwarded unchanged when no masking component exists.

Hide in the lantern cabinet, behind the lamp-base screen, or at the stair-landing cabinet.
Use E to enter/exit after releasing the key; R holds breath, Z/X lean outside hiding.
Listen via the stair-footstep subtitle; cranking on Crane's arrival uses the existing catch/respawn.
The Storm checkpoint is captured at state entry.

Charge above the pulse threshold for its hold duration produces LampPulse.
Only unobserved figures move back: the docs' watched-figure freeze rule still applies.
Healthy freezes all figures; Failing and Extinguished increase unobserved advance rates.
The requested movement while watched/ignoring sight was not implemented because it conflicts with that contract.

## Ending

The chosen condition is 90 accumulated seconds of healthy charge after generator failure,
with the first darkness advance (B04) also complete. B09 stops rain, pursuit and figure pressure,
stabilizes the lamp, and gives a logbook/Act 4 placeholder subtitle and objective.
The state remains Storm: architecture permits Storm → Finale only after A4-B06.
There is no automatic Act 4 scene load or early Finale transition.

## Tuning

The shared asset is Assets/_Project/Settings/StormSettings.asset.
Adjust chargeDecay, chargeGain, crankAcceleration/deceleration and crankNoiseCurve for crank difficulty.
Adjust staminaDrain, exhaustionRecovery and exhaustedSpeedCap for exertion.
Adjust maskDuration/maskReduction, lightningInterval and thunderDelay for thunder timing.
Adjust stairPause, lanternSearchTime and lanternHidingCheckChance for upstairs danger;
StalkerAI hearingRange/hearingCurve and NavMeshAgent speed remain on Crane.
Adjust failingAdvanceInterval/extinguishedAdvanceInterval, pulseThreshold/pulseHold/pulseCooldown for figures.
stormHealthyTime controls completion duration. All AudioClip slots remain empty.

Reduced rain/lightning is on by default. F8 toggles it through the existing InputActions asset;
the on-screen toggle is also available when the pointer is unlocked.
No voice, thunder or footstep recordings play until clips are assigned; subtitles are the placeholders.

Manual visual comfort, upstairs hiding sightlines, hearing difficulty, and an unassisted full playthrough remain necessary.
The game-event hub resets subscriptions at session initialization to support disabled domain reload.

## Verification

Unity 6000.6.6f1 strict recompilation returned zero errors and warnings.
Unity generated and baked the scene; a sampled floor-to-crank route returned PathComplete.
Live checks confirmed eight figures/six hiding spots, reduced visuals enabled,
generator/first-crank/pulse/stair-warning/darkness/end beat publication, and the Storm ending state.
Focused masking checks returned crank 20→5 m, player 12→3 m, generator 24→24 m and crank 20 m after expiry.
While hidden, charge fell from 0.6 to approximately 0.578 over the check interval.
The long combined harness exceeded its request timeout but continued to the ending;
the focused checks and subsequent state queries completed successfully. End-time counters were accelerated
in memory, so this does not certify a full 90-second player-controlled run.
Act 1 regression: Dusk with intro movement locked and fuel mode intact.
Act 2 regression: Night with fuel mode intact and an unmasked 6 m noise routed as 6 m.
