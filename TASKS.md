# The LightHouse Task Board

The cards below mirror `docs/roadmap.md`. A card is complete only when its acceptance checkboxes are complete and the build remains playable.

## Card 1 — Step 3: Thin Slice

- [ ] Press Play into a greybox lighthouse tower.
- [x] Walk and look using the new Input System.
- [x] Interact with and crank the lamp.
- [x] Emit a crank noise through the event contract.
- [x] Make a placeholder cube stalker investigate that noise.
- [ ] Produce a playable build of the slice.

## Card 2 — Act 1: Arrival

- [x] Implement the approved Act 1 beats and interactions.
- [x] Add ferry arrival, boatman warning, tower entry, meal, radio greeting, and first lamp objective.
- [ ] Validate all Act 1 unlocks against `docs/story-beats.md`.
- [ ] Produce a playable Act 1 build.

## Card 3 — Act 2: Cellar and Stalker

- [x] Implement fuel pressure, cellar shanty, impossible footprints, and the sound-hunting stalker.
- [x] Introduce Drowned figures advancing when unobserved.
- [x] Add hiding/hold-breath, checkpoint catch sequence, knocking and singing-lure hooks.
- [x] Add a reusable Act 2 scene builder starting directly at Night.
- [ ] Validate all Act 2 unlocks against `docs/story-beats.md`.
- [ ] Produce a playable Acts 1–2 build.

## Card 4 — Act 3: Storm and Crank

- [x] Implement the storm and generator failure.
- [x] Make hand-cranking restore lamp power while emitting dangerous noise.
- [x] Make darkness advance the Drowned threat.
- [x] Add thunder masking, charge gauge/pulse, stamina pressure, stair warning, lantern hiding and an Act 4 placeholder.
- [ ] Validate all Act 3 unlocks against `docs/story-beats.md`.
- [ ] Produce a playable Acts 1–3 build.

## Card 5 — Act 4: Truth

- [x] Implement the *Marguerite* and lighthouse-leash revelations.
- [x] Reveal the hidden room, Marta recording, ledger, family photograph, and Crane's final entry.
- [x] Reframe the stalker's behavior as Crane protecting Daniel.
- [x] Validate all Act 4 unlocks against `docs/story-beats.md`.
- [ ] Produce a playable Acts 1–4 build.

## Card 6 — Act 5: Dawn

- [x] Implement the flooded courtyard and failing-lamp finale.
- [x] Implement Crane's protective route and noise-covering actions.
- [x] Resolve the dawn escape, lullaby, Crane's rest, and morning-boat epilogue.
- [x] Show Daniel crossed out and the next keeper entered in the ledger.
- [ ] Produce a complete playable build.

## Card 7 — Polish

- [x] Implement movement upgrade: jump, stamina, crouch clearance, phase-driven surface noise, camera accessibility, lean, safety and F3 diagnostics.
- [x] Add the reusable MovementTest scene builder and shared PlayerMovementSettings asset.
- [ ] Playtest the movement course and Act 1 intro/interaction regression in the Editor.

- [x] Add Graphics 1 atmosphere presets, URP post-processing, lamp beam, fog cards, wetness and lightweight lighting helpers.
- [x] Add Mac and Low URP quality tiers plus the reusable MoodTest and Graphics 1 applicator.
- [ ] Bake lighting and profile Acts 1–5 on the target MacBook Air M3.
- [x] Define and enforce the 8 GB asset/runtime budget with an asset checker, F4 HUD, act unloading, quality limits and Drowned LOD cap.
- [ ] Record a sustained target-hardware memory and frame-time profile for each act.

- [ ] Replace only proven placeholders with final art and audio.
- [ ] Tune stealth readability, noise values, lamp pressure, pacing, and accessibility.
- [ ] Fix defects and performance regressions without expanding scope.
- [ ] Complete a final end-to-end playtest and release build.
