# Lighting and Post-Processing Repair Report
**The LightHouse — Atmospheric & Visual Calibration**  
*Timestamp: November 7, 2026*

---

## 1. Executive Summary

The original scene suffered from an aggressive global orange-brown color cast, washed-out contrast, missing shadows on localized lights, and blurry anti-aliasing. A global ambient color of `RGB(0.5, 0.4, 0.35)` was being re-injected every frame by `DuskTimer.cs`, directional light was set to harsh orange, and additional light shadows were disabled in the URP pipeline asset. As a result, point lights inside the cottage and tower penetrated freely through solid masonry walls and floors into the night exterior, while whole-screen Fast Approximate Anti-Aliasing (FXAA) destroyed high-frequency texture detail.

This pass eliminated the global orange wash, restored true cold maritime twilight and oceanic darkness, activated soft shadows across all additional light sources, and tuned a restrained, cinematic post-processing stack with Subpixel Morphological Anti-Aliasing (SMAA High).

---

## 2. Maritime Horror Color Palette

### Exterior Palette
- **Ocean & Horizon:** Deep oceanic indigo-slate `#03080C` to `#0B1319`.
- **Sky & Mist:** Cold silver-slate fog `#0D141A` with a density of `0.012`, maintaining lighthouse visibility at distance.
- **Moonlight / Twilight:** Cold silver-blue illumination `#C0D0E0` at an intensity of `0.45` and pitch of $38^\circ$.
- **Weathered Granite Masonry:** Neutral, desaturated dark granite `#2B2D30` with damp mortar recesses.
- **Localized Lantern Warmth:** Warm amber glow `#FFE0B0` (color temperature $\sim 2800\text{K}$) strictly confined to lanterns, windows, and doorways.

### Interior Palette
- **Living Quarters Plaster:** Muted sea-salt plaster `#C8C4BC` with age staining.
- **Timber Furniture & Beams:** Aged North Atlantic pine `#3A342C`.
- **Cast Iron & Stoves:** Matte oxidized charcoal iron `#1E2022`.
- **Focal Warmth:** Controlled tungsten/oil illumination (`2.0\dots 2.8` intensity) casting realistic soft shadows on walls and floors.

---

## 3. Light Source Calibration

| Light Source | Type | Previous Settings | Corrected Settings | Technical Rationale |
| :--- | :--- | :--- | :--- | :--- |
| **Main Directional Light** | Directional | Color: `{1.0, 0.61, 0.38}`, Intensity: `1.0`, Shadows: Hard | Color: `{0.75, 0.82, 0.90}`, Intensity: `0.45`, Shadows: `Soft`, 4 Cascades | Replaces harsh orange sunset with cold, eerie moonlight; cascades at `{0.08, 0.20, 0.50}` prevent shadow stepping. |
| **Dock Lantern Light** | Point | Intensity: `15.0`, Range: `20.0m`, Shadows: `None` | Intensity: `2.4`, Range: `8.5m`, Shadows: `Soft` | Was bathing the entire bay, dock, and shoreline in blinding orange. Now a focused, atmospheric pool of light. |
| **House Porch Lantern** | Point | Intensity: `4.8`, Range: `9.0m`, Shadows: `None` | Intensity: `2.8`, Range: `6.5m`, Shadows: `Soft` | Illuminates the courtyard threshold; soft shadows prevent light leaking through the cottage siding. |
| **House Interior Glow** | Point | Intensity: `3.5`, Range: `6.5m`, Shadows: `None` | Intensity: `2.2`, Range: `5.5m`, Shadows: `Soft` | Creates warm window spill without penetrating into adjacent rooms. |
| **Kitchen / Dining Lamps** | Point | Intensity: `4.5`, Range: `7.5m`, Shadows: `None` | Intensity: `2.5`, Range: `5.0m`, Shadows: `Soft` | Contains illumination to room boundaries; casts soft shadows behind cupboards and tables. |
| **Tower Vestibule Light** | Point | Intensity: `4.5`, Range: `7.0m`, Shadows: `None` | Intensity: `2.6`, Range: `6.0m`, Shadows: `Soft` | Eliminates vertical light leakage up through the spiral stairwell ceiling. |
| **Watch Room / Quarters** | Point | Intensity: `5.0`, Range: `7.5m`, Shadows: `None` | Intensity: `2.5`, Range: `5.5m`, Shadows: `Soft` | Maintains dramatic shadows under the bunk and desk. |
| **Ambient Lighting** | Flat | `RGB(0.50, 0.40, 0.35)` | `RGB(0.08, 0.11, 0.15)` | Eliminates flat orange ambient wash; shadows now have cold oceanic depth. |

---

## 4. Render Pipeline & Shadow Infrastructure

The Universal Render Pipeline configuration asset (`Assets/_Project/Settings/URP_Mac.asset`) and Quality Settings tier were updated:

1. **Additional Light Shadows:**
   - `supportsAdditionalLightShadows = true`
   - `additionalLightsShadowmapResolution = 1024`
   - `additionalLightsPerObjectLimit = 8`
   Enables real-time shadow casting for all room practicals and exterior lanterns, eliminating all indoor-to-outdoor light bleed.

2. **Main Light Shadows:**
   - `shadowDistance = 64.0m` (scaled for the island perimeter)
   - `shadowCascadeCount = 4` with splits at `8%` ($5.1\text{m}$), `20%` ($12.8\text{m}$), and `50%` ($32\text{m}$)
   - `mainLightShadowmapResolution = 2048`
   - `supportsSoftShadows = true` with PCF filtering

3. **Anti-Aliasing Upgrade:**
   - Replaced FXAA on `Player Camera` with `SubpixelMorphologicalAntiAliasing` (SMAA High).
   - Set 4x Multi-Sample Anti-Aliasing (`msaaSampleCount = 4`) in URP asset.
   - Enforced 4x Anisotropic Filtering across all PBR textures, preserving surface crispness at grazing angles.

---

## 5. Post-Processing Stack Calibration

Volume profiles across all five game states (`Volume_Dusk.asset`, `Volume_Night.asset`, `Volume_Storm.asset`, `Volume_Finale.asset`, `Volume_Dawn.asset`) were serialized with sub-assets:

| Effect | Parameter | Previous Value | Calibrated Value | Visual Result |
| :--- | :--- | :--- | :--- | :--- |
| **Tonemapping** | Mode | None / Neutral | **ACES** | Cinematic film S-curve response; eliminates blown-out highlights on lantern glass and preserves deep shadows. |
| **Color Adjustments** | Saturation | `-28.0` (crushed dull) | **`-15.0`** | Retains authentic subtle coloration of weathered wood, brass, and maritime fabrics. |
| **Color Adjustments** | Contrast | `+12.0` | **`+12.0`** | Clean separation between dark oceanic shadows and localized light pools. |
| **Color Adjustments** | Post Exposure | `+0.15` (milky blacks) | **`0.00`** | Deep, rich black floor without shadow crushing. |
| **White Balance** | Temperature | `0.0` | **`-4.0`** | Subtle cold oceanic tint across exterior twilight. |
| **Bloom** | Intensity / Threshold | `1.5` / `0.9` | **`0.35` / `1.15`** | Removes blinding haze around lanterns; creates a soft, natural glow only on hot incandescent filaments and lamp flame. |
| **Vignette** | Intensity / Smoothness | `0.45` / `0.35` | **`0.24` / `0.40`** | Subtle perimeter frame darkening without artificial circular tunnel vision. |
| **Film Grain** | Type / Intensity | Large / `0.35` | **Thin / `0.12`** | Subtle cinematic 35mm film texture; eliminates heavy digital noise artifacts. |

---

## 6. Volumetric Fog & Beam Updates

### Volumetric Lighthouse Beam (`LampBeamURP.shader`)
- **Problem:** The original cone shader used a grazing rim glow (`pow(1.0 - NdotV, edgeSoftness)`), producing an artificial, hollow glowing shell with a hard outline against the night sky.
- **Correction:** Replaced rim glow with an angle-of-incidence core density calculation:
  $$\text{coreGlow} = (N \cdot V)^{1 / \text{edgeSoftness}}$$
  The light density is now concentrated in the thickest central axis of the cone and smoothly feathers to zero at the outer silhouette, creating a soft atmospheric beam cutting through oceanic mist.

### Fog Cards & Distance Fog
- `RenderSettings.fogMode = FogMode.ExponentialSquared`
- Fog color: `#0D141A` (deep slate-charcoal)
- Density: `0.012`
- Distance fog obscures the open ocean horizon naturally while keeping the full height of the lighthouse tower readable.
