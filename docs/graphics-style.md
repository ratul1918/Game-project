# Graphics Style Guide

*The LightHouse* uses desaturated realism with readable silhouettes and restrained effects. The lighthouse lamp is the dominant local light source: warm amber against cold moonlight, rain and blue-grey sea fog. Warm highlights should feel scarce and safe; shadows remain cold without crushing interaction targets into pure black.

Keep the Drowned half-hidden by fog, framing and darkness. Their shapes should read before their surface detail does. The player should rarely see a figure under flat, even light.

## Lighting and atmosphere

- Prefer one dominant motivated light per composition.
- Dusk is muted violet-grey with the last warm horizon light.
- Night is cold, desaturated and denser near water.
- Storm is darker and higher contrast, with brief lightning supplied by the existing storm system.
- Finale is the densest, most oppressive state; the lamp remains the visual anchor.
- Dawn lifts fog and contrast gradually into a pale warm-grey morning.
- Bloom belongs primarily to the lamp and bright lightning. Film grain and vignette stay subtle.
- Do not use motion blur or depth of field in gameplay.

## Performance rules

Bake static lighting, reflection probes and light probes where practical. Keep additional real-time lights few, non-shadowed unless essential, and short-ranged. Use low-count fog particles, shared materials, LODs and occlusion-friendly greybox layouts. The visual target is a stable 30–60 FPS on an M3 MacBook Air under sustained passive cooling.
