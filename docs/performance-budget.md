# Performance and Memory Budget — MacBook Air M3 / 8 GB

The target has 8 GB of shared CPU/GPU memory and passive cooling. Budgets are hard limits for imported and runtime content, not goals to exceed during polish.

## Assets

- Textures: maximum 1K. Small props use 512 or lower. Never import or ship 4K textures.
- Skyboxes and HDRIs: maximum 2K.
- Lightmaps: approximately one 1K atlas per act scene; add another only after target-hardware profiling proves it is necessary.
- Drowned figures: under 5,000 triangles at LOD0, two LODs, one shared GPU-instanced material, and only about four figures at full detail near the player. Remaining figures use LOD1 or silhouettes.
- Audio: compressed. Long dialogue, music and ambience stream from disk; short effects use compressed-in-memory. Ambience is mono unless spatial review proves stereo is essential.

## Runtime

- Load one act scene at a time. Act transitions load the next scene additively, activate it, unload the previous act, then call `Resources.UnloadUnusedAssets`.
- Real-time lights: directional moon/sun, lamp spot, and one or two lanterns only.
- Real-time shadows: directional light and lamp spot only, at short shadow distance with one cascade.
- MSAA is off. Use the 0.7–1.0 render-scale option before adding expensive effects.
- Target 30–60 FPS during a sustained play session, not only a cold editor run.

Run **TheLightHouse → Check Asset Budget** before importing an art/audio batch and before every release candidate. Auto-fix is restricted to safe texture and audio importer changes; model reduction and scene-light decisions remain manual.
