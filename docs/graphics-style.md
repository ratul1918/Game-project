# Graphics Style & Visual Architecture Guide

*The LightHouse* targets desaturated, atmospheric realism on the Apple Silicon M3 MacBook Air (8 GB unified memory), achieving a solid 30–60 FPS. The visual core balances warm amber practical lights (#FFA64D) against cold blue moonlight, coastal storm haze, and damp subterranean shadow.

## Material & Texture Architecture (1K Budget)

All texture folders in `Assets/_Project/Art/Textures` are built into URP Lit materials with packed Metallic-Smoothness maps (Red = Metallic, Alpha = Smoothness, inverted from Roughness if necessary), 1K resolution, correct sRGB flags, and BC7/ASTC compression:
- **StoneWall**: Tower exterior masonry, central core, interior stone reveals, cellar corridor walls and vaulting.
- **Flagstone**: Courtyard pavers, tower vestibule floor, lantern room deck.
- **Plaster**: Interior living quarters (Crane's room, service watchroom).
- **Wood**: Floorboards, timber ceilings, doors, stair treads, furniture, dock planks.
- **RustMetal**: Window astragals, railings, handrails, generator, conduits, lanterns, hardware.
- **WetRock & Sand**: Shoreline, coastal cliffs, dock approach.
- **Paper**: Nautical charts, harbor board ledger, logbook pages, photographs.

Total runtime texture memory remains strictly budgeted under 105 MB.

## Surface Detailing & Triplanar Shading

- **World-Space Triplanar Shading**: Applied to cliffs, shoreline boulders, and coastal rock faces to prevent texture stretching on sheer vertical drops.
- **Dynamic Wetness Input**: Modulated by `WetLookController` to darken albedo and elevate smoothness near the sea line and throughout the subterranean cellar.
- **Macro Breakup & Damp Gradients**: Dual-layer normal tiling and vertex-color dampness masks ground the tower base and cellar walls.

## Architectural Modeling & Prop Standards

- **Edge Quality & Bevels**: Interior kit prefabs feature modeled cornices, plinths, stone reveals, and door hardware (strap hinges, latches, iron bands).
- **Subterranean Vaulting**: 2.8 m wide by 2.6 m high vaulted corridor reinforced with modeled timber rib beams.
- **Prop Fidelity**: Greybox placeholders replaced with shaped multi-primitive props (recorder with rotating tape reels, vacuum tube radios with emissive filaments, secret workbench, drafting desk, wardrobe) kept under 2,000 triangles per asset.

## Lighting Architecture & Budget

- **Realtime Lights Budget**: Strictly at most 4 realtime lights active in any view.
- **Warm Practical Lanterns**: Caged wall lanterns emitting warm amber (#FFA64D) with small radii (4.5–6.0 m) marked baked or mixed.
- **Moonlight & Window Shafts**: Cold blue directional moonlight filtering through deep stone window reveals, complemented by cone mesh light-shaft cards with dust motes.
- **Reflection Probes**: Low-resolution, box-projected reflection probes in every room, the cellar vault, and the lantern room.
- **Lightmap Baking**: Progressive GPU/CPU baking with 1024 atlas limits, ContributeGI static flags, and light probe coverage.

## Post-Processing & Atmosphere

- **URP Volume Profiles**: Filmic ACES tonemapping, restrained bloom on the lighthouse beacon and vacuum tubes, subtle vignette, and fine-grain film grain.
- **Atmospheric Depth**: Dust motes in window beams, drifting fog cards along the shoreline and cellar stair, and subtle dripping water particle emitters underground.
- **Water & Shoreline**: Soft depth-faded foam along water edges, elevated threshold heights above wave crests, and wet sand specular highlights.

## Audio Environment

- **Reverb Zones**: 3D AudioReverbZones with stone presets in the lighthouse tower and subterranean cellar vault.
- **Bandpass Filtering**: Low-pass and high-pass filtering applied to the reel-to-reel transmitter voice.

## Asset Budget Conformance

The automated `AssetBudgetChecker` (`TheLightHouse/Check Asset Budget`) scans all models, textures, audio clips, and scenes against the M3 8 GB budget. Result: **PASS — no imported assets exceed the configured budget.**
