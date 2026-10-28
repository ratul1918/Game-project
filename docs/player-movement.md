# Player movement

Tune the shared asset at `Assets/_Project/Resources/PlayerMovementSettings.asset`.
Existing scenes load this asset automatically when their controller's settings reference is empty.
Scene builders preserve the existing input/camera/NoiseEmitter wiring. Helpers are added to the player
in Awake so existing Act 1 scenes also gain stamina, surface footsteps, camera effects and safety.

Controls: WASD movement, Shift sprint, hold C crouch, Space jump, Z/X lean, F3 debug panel.
E remains interaction. Jump, sprint and crouch reuse existing actions; LeanLeft, LeanRight and
MovementDebug were added to the same InputActions asset.

`MovementEnabled` remains compatible with Boatman. `SetMovementLocked(bool)` changes the same property;
`SetLookLocked(bool)` independently locks looking. PlayerHidden events prevent movement, jumping and lean.
Locking movement immediately clears horizontal velocity and buffered jumps. Gravity still applies.

HeadBob owns phase advancement; FootstepSystem emits only on half-cycle crossings while actually moving
on walkable ground. Camera accessibility disables visual bob, sway, landing effects and sprint FOV,
while the phase and noise model continue unchanged. Lean uses sphere checks and emits no noise.

The ground probe and CharacterController collision flags determine grounded state; step offset is disabled
while airborne. Jump height, coyote time, buffer, acceleration, air control and terminal velocity are tunable.
Standing clearance checks exclude the player's own capsule. The camera and capsule lower together.

There is no fall damage. A landing emits scaled surface noise and a dip; hard impacts add brief shake.
Below killPlaneY, the controller respawns at its latest stable grounded position after safeGroundSeconds.

## Tuning

- Floaty: raise gravityMultiplier; lower jumpHeight or airControl.
- Slow: raise walkSpeed/sprintSpeed; raise acceleration for quicker starts.
- Heavy: lower gravityMultiplier or increase jumpHeight; shorten crouch transitions by raising crouchTransitionSpeed.
- Sliding after release: raise deceleration.
- Stairs: compare riser height with stepOffset; tune skinWidth and groundProbeDistance carefully.
- Motion comfort: enable disableBobAndFov or reduce bobAmplitude, landingDip, hardLandingShake and sprintFovIncrease.

## Editor acceptance

Run TheLightHouse/Build Movement Test Scene, then Play. No NavMesh is required.
Test each 0.4/0.8/1.1 m ledge from the floor, stairs, the ramp, the 1.25 m tunnel,
surface patches, gaps and the high drop platform accessed by the tall staircase.
Release C under the tunnel: the capsule must stay crouched. Exhaust stamina with Shift,
then confirm sprint recovers after lockout and regeneration. Test jumping just after a ledge edge
and just before landing. F3 shows actual speed, grounded, stamina, last noise radius and surface.
Walk off a gap island to verify recovery below the kill plane.
Replay Act 1: look during Boatman, movement/jump blocked until he finishes, then verify E interactions/subtitles.
Audio arrays remain empty; gameplay noise appears in diagnostics and NoiseManager gizmos.

## Verification recorded

Unity's strict live recompilation completed with zero errors and zero warnings.
The builder generated MovementTest through Unity's scene APIs.
Live Play Mode checks with injected Input System keyboard input confirmed:
silent standing; walking noise at 6 m; sprint speed approximately 7 m/s and stamina drain;
zero displacement while movement-locked; jump and grounded landing;
capsule height remaining 1.1 m after C is released in the tunnel;
stairs climbed; all three ledges reached with a running approach;
hidden-state movement blocked; hard landing at 30 m base loudness;
stamina reaching zero, sprint lockout and regeneration; and recovery above the kill plane.
No manual mouse/camera comfort evaluation or complete Act 1 dialogue/interaction replay was performed.
