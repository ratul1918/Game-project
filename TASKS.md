# The LightHouse Task Board

The cards below mirror `docs/roadmap.md`. A card is complete only when its acceptance checkboxes are complete and the build remains playable.

## Card 1 — Step 3: Thin Slice

- [x] Press Play into a greybox lighthouse tower.
- [x] Walk and look using the new Input System.
- [x] Interact with and crank the lamp.
- [x] Emit a crank noise through the event contract.
- [x] Make a placeholder cube stalker investigate that noise.
- [x] Produce a playable build of the slice.

## Card 2 — Act 1: Arrival

- [x] Implement the approved Act 1 beats and interactions.
- [x] Add ferry arrival, boatman warning, tower entry, meal, radio greeting, and first lamp objective.
- [x] Validate all Act 1 unlocks against `docs/story-beats.md`.
- [x] Produce a playable Act 1 build.

## Card 3 — Act 2: Cellar and Stalker

- [x] Implement fuel pressure, cellar shanty, impossible footprints, and the sound-hunting stalker.
- [x] Introduce Drowned figures advancing when unobserved.
- [x] Add hiding/hold-breath, checkpoint catch sequence, knocking and singing-lure hooks.
- [x] Add a reusable Act 2 scene builder starting directly at Night.
- [x] Validate all Act 2 unlocks against `docs/story-beats.md`.
- [x] Produce a playable Acts 1–2 build.

## Card 4 — Act 3: Storm and Crank

- [x] Implement the storm and generator failure.
- [x] Make hand-cranking restore lamp power while emitting dangerous noise.
- [x] Make darkness advance the Drowned threat.
- [x] Add thunder masking, charge gauge/pulse, stamina pressure, stair warning, lantern hiding and an Act 4 placeholder.
- [x] Validate all Act 3 unlocks against `docs/story-beats.md`.
- [x] Produce a playable Acts 1–3 build.

## Card 5 — Act 4: Truth

- [x] Implement the *Marguerite* and lighthouse-leash revelations.
- [x] Reveal the hidden room, Marta recording, ledger, family photograph, and Crane's final entry.
- [x] Reframe the stalker's behavior as Crane protecting Daniel.
- [x] Validate all Act 4 unlocks against `docs/story-beats.md`.
- [x] Produce a playable Acts 1–4 build.

## Card 6 — Act 5: Dawn

- [x] Implement the flooded courtyard and failing-lamp finale.
- [x] Implement Crane's protective route and noise-covering actions.
- [x] Resolve the dawn escape, lullaby, Crane's rest, and morning-boat epilogue.
- [x] Show Daniel crossed out and the next keeper entered in the ledger.
- [x] Produce a complete playable build.

## Card 7 — Polish

- [x] Replace the short solid tower with the tall tapered hollow tower, keeper house, real door openings, spiral stair, cellar route, and serialized layout dimensions.
- [x] Remove stale generated roots/orphan stairs, roofs, and panels before rebuilding.
- [x] Hide empty HUD backgrounds and interaction/objective/inventory roots until they contain content.
- [x] Export and inspect greybox layout previews for the courtyard, dock, front door, and tower floors.
- [x] Run a player-controlled dock-to-lantern-to-cellar traversal and inspect every spiral step on target hardware.

- [x] Add the shared five-level lighthouse/island layout builder, modular level prefabs, room triggers, dual routes, hiding spots, stair links, floor occlusion areas and URP particle materials.
- [x] Rebuild Acts 1–5 with the shared layout while retaining gameplay anchors and canonical beat assets.
- [x] Add the themed TMP HUD, pause menu, journal tabs, HUD settings, room-name flash and optional composure presentation.
- [x] Complete UI polish pass: imported sprite settings, expanded UITheme roles, empty-panel gating, UI audio hook, and UI layout checker.
- [x] Regenerate UI previews with rendered canvases and wire the Options credits text asset; Play Mode/Game-view capture, menu interaction, and target-device profiling remain manual verification.
- [x] Add and pass batch validation for required scene anchors, materials, shared layout/HUD, and A1-B01–A5-B06 beat publication order.
- [x] Bake and manually inspect NavMesh and occlusion data in each rebuilt act scene.
- [x] Complete a player-controlled Acts 1–5 regression of interactions, fleeing routes, pause/journal, HUD readability and room sightlines.

- [x] Implement movement upgrade: jump, stamina, crouch clearance, phase-driven surface noise, camera accessibility, lean, safety and F3 diagnostics.
- [x] Add the reusable MovementTest scene builder and shared PlayerMovementSettings asset.
- [x] Playtest the movement course and Act 1 intro/interaction regression in the Editor.

- [x] Add Graphics 1 atmosphere presets, URP post-processing, lamp beam, fog cards, wetness and lightweight lighting helpers.
- [x] Add Mac and Low URP quality tiers plus the reusable MoodTest and Graphics 1 applicator.
- [ ] Bake lighting and profile Acts 1–5 on the target MacBook Air M3.
- [x] Define and enforce the 8 GB asset/runtime budget with an asset checker, F4 HUD, act unloading, quality limits and Drowned LOD cap.
- [ ] Record a sustained target-hardware memory and frame-time profile for each act.
- [x] Add the Backend 1 authenticated REST/WebSocket thin slice with isolated session ticks, bounded queues and offline Unity fallback.
- [x] Add Backend 2 cloud saves and run tracking with optimistic locking and offline-first Unity integration.
- [x] Add Backend 3 world director, adaptive pressure, server event bridge, and smooth offline fallback.
- [x] Playtest online reconnect and offline fallback in a standalone Unity build.
- [x] Add Backend 4 telemetry queue, Micrometer metrics, admin dashboard, bot client, and load-test harness.
- [x] Add Unity telemetry reporting, bounded offline buffering, options toggle, debug counters, and test menu.
- [x] Run the bounded Backend 4 load test on the target MacBook Air M3 and record measured CSV results.
- [x] Complete Backend 5 documentation: contracts, architecture overview, demo script, traceability, README, thread-safety, and Mermaid UML.

- [x] Replace only proven placeholders with final art and audio.
- [x] Tune stealth readability, noise values, lamp pressure, pacing, and accessibility.
- [x] Fix defects and performance regressions without expanding scope.
- [x] Complete a final end-to-end playtest and release build.

## Card 8 — Interior v2 and High-Quality Look (MacBook Air M3, 8 GB)

- [x] Part A0: Delete legacy generated tower interior, stairs, mezzanines, cellar, and duplicate props while preserving shell, exterior, dock, and cottage layout.
- [x] Part A1: Modular interior kit prefabs on 0.5 m grid (wall, floor slab with stairwell hole, door frame with working door, 18-step straight stair flight, landing, handrail, window frame, vault segment) with `InteriorKit` and `InteriorSettings.asset`.
- [x] Part A2: Tapered tower interior with 3 m round core, switchback flights, >= 2.4 m headroom across all treads, Floor 1 vestibule with cellar door, Floor 2 Crane's quarters, Floor 3 service watchroom, Levels 4–6 stair tower, and Lantern room with 12-facet glazed chamber, Fresnel lens pedestal, `LampController`, `LanternInteract`, and `CrankStation`.
- [x] Part A3: Continuous ramp colliders (~32°), flush landings, CharacterController setup (1.8m height, 0.4m radius), NavMesh baked from physics colliders, 10 hiding spots, and 6 logbook pages back in place by ID.
- [x] Part A4: Subterranean complex with 5 m descent stair, 20 m vaulted corridor with conduit/barrels/crates, `WetFootprints` (leading in, none leading out), `CellarHum`, and hidden 4×4 m room behind sliding barrel stack with `RecordedTransmitter` (`ReelRotator`), glowing tube radios, nautical charts, `Ledger`, and ship photograph.
- [x] Part B1: 1K URP Lit materials with packed Metallic-Smoothness maps (StoneWall, Flagstone, Plaster, Wood, RustMetal, WetRock, Sand, Paper).
- [x] Part B2: World-space triplanar rock and cliff shaders with dynamic wetness input and damp gradients.
- [x] Part B3: High-quality modeling with edge bevels, stone reveals, door strap hinges, rib beams, and multi-primitive props <2k triangles.
- [x] Part B4: Lighting pass with reflection probes per room, warm caged lanterns (#FFA64D), cold moonlight, window light shaft cards, and <= 4 realtime lights.
- [x] Part B5: Atmosphere with fog cards, light shaft dust motes, cellar dripping water, filmic tonemapping, bloom, vignette, subtle grain, and ambient occlusion.
- [x] Part B6: Water and shoreline polish with depth-faded foam, wet sand specular, and elevated thresholds.
- [x] Part B7: 3D audio reverb zones with stone presets for tower and cellar, bandpass filter on transmitter audio.
- [x] Part B8: Performance enforcement with static batching, occlusion areas, and zero budget violations via `TheLightHouse/Check Asset Budget`.
- [x] Part C: Validation test suite in `InteriorValidation.cs` passing all 8 checks, 12 eye-level preview PNGs captured in `docs/level-previews/`, and batch-mode compilation verified.
