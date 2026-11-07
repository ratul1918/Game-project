# THE LIGHTHOUSE — AAA GRAPHICS IMPLEMENTATION REPORT

**Document Version:** 1.0.0  
**Project:** The LightHouse (Harrow Rock)  
**Engine:** Unity 6 (6000.6.6f1)  
**Render Pipeline:** Universal Render Pipeline (URP 17.0+)  
**Target Hardware:** Apple MacBook Air M3 (8 GB Unified Memory, 60 FPS target)  
**Related Audits:**  
- [MASTER_ENVIRONMENT_AUDIT.md](file:///Users/rafiurrahman/The%20LightHouse/docs/MASTER_ENVIRONMENT_AUDIT.md)  
- [LIGHTHOUSE_ARCHITECTURE_FIX_REPORT.md](file:///Users/rafiurrahman/The%20LightHouse/docs/LIGHTHOUSE_ARCHITECTURE_FIX_REPORT.md)  
- [PHOTOREALISM_GRAPHICS_AUDIT.md](file:///Users/rafiurrahman/The%20LightHouse/docs/PHOTOREALISM_GRAPHICS_AUDIT.md)  
- [ASSET_SOURCES_AND_INTEGRATION.md](file:///Users/rafiurrahman/The%20LightHouse/docs/ASSET_SOURCES_AND_INTEGRATION.md)  

---

## 1. Executive Summary

This report provides the complete technical and artistic documentation of the AAA graphics and environmental photorealism overhaul executed for *The LightHouse*. 

Following the successful architectural repair and staircase reconstruction documented in `docs/LIGHTHOUSE_ARCHITECTURE_FIX_REPORT.md`, the environment art, shaders, materials, atmospheric lighting, and post-processing volumes were upgraded to achieve a cinematic, highly detailed maritime psychological horror aesthetic (*Still Wakes the Deep*, *The Lighthouse*, *Amnesia: The Bunker*).

All gameplay mechanics, character controller physics, interactive anchors, story progression triggers, sound propagation, UI interfaces, and SQLite backend integrations were 100% preserved.

---

## 2. Exterior Architecture & Masonry

### 2.1 Conical Lighthouse Tower Masonry
- **Geometry:** 8-stage conical truncated tower shaft ($y = 0.00\text{m} \to y = 25.60\text{m}$) with a continuous outward taper from base ($r = 4.0\text{m}$) to lantern gallery ($r = 2.4\text{m}$).
- **Material Binding:** `PBR_LighthouseStone.mat` and `Salt Plaster.mat` upgraded to URP Lit PBR.
- **PBR Maps:**
  - **Albedo / BaseMap:** High-resolution 1K diffuse map (`castle_wall_slates`) capturing weathered coastal stone, lichen stains, and salt efflorescence.
  - **Normal Map (`_BumpMap`):** Full tangent-space normal map with depth-carved mortar recessed seams, surface chisel marks, and rough rock facets.
  - **Ambient Occlusion (`_OcclusionMap`):** Baked cavity shadow map with strength $1.0$, creating pronounced deep shadowing inside mortar joints and stone recesses.
  - **PBR Parameters:** `_Metallic = 0.05`, `_Smoothness = 0.45` (subtle moisture sheen across stone faces).
  - **UV Tiling:** Tiled $4.0 \times 4.0$ continuously around the circumference to eliminate visible seam repetition or texture stretching.

### 2.2 Attached Keeper's Cottage
- **Geometry:** Double-gable stone masonry cottage ($12.0\text{m} \times 7.5\text{m}$) containing the kitchen, hearth, and dining room, physically abutting the southwest tower flank with an authentic stepped flashing stone plinth.
- **Roofing:** Slanted slate shake shingles using `Wet Shale.mat` with normal map displacement, wetness roughness variation, and overhang drip eaves.
- **Windows & Doors:** Weathered timber frames with bevelled glazing beads, brass hinges, and warm interior glowing glass.

---

## 3. Dynamic North Atlantic Ocean & Shoreline Shader

The legacy flat water plane was completely replaced with a custom HLSL shader: `Assets/_Project/Shaders/HarrowSea.shader`.

### 3.1 Gerstner Wave Displacement
- **Algorithm:** Multi-directional Gerstner wave calculations running directly in the vertex shader:
  $$\vec{P}_{displaced} = \vec{P} + \sum_{i=1}^{3} \begin{pmatrix} Q_i A_i D_{i,x} \cos(\omega_i \vec{D}_i \cdot \vec{P} + \phi_i t) \\ A_i \sin(\omega_i \vec{D}_i \cdot \vec{P} + \phi_i t) \\ Q_i A_i D_{i,z} \cos(\omega_i \vec{D}_i \cdot \vec{P} + \phi_i t) \end{pmatrix}$$
- **Wave Parameters:**
  - Direction 1: Vector $(0.8, 0.6)$ (primary North Atlantic ocean swell)
  - Direction 2: Vector $(-0.5, 0.8)$ (cross-chop wave interference)
  - Direction 3: Vector $(0.3, -0.9)$ (high-frequency coastal rebound)
  - Steepness ($Q$): $0.55$, creating natural peaked wave crests without self-intersection.

### 3.2 Depth-Based Shoreline Surf Foam
- **Implementation:** Leverages URP's `_CameraDepthTexture` via `DeclareDepthTexture.hlsl`:
  $$\text{waterDepth} = \text{LinearEyeDepth}(\text{rawDepth}) - \text{screenPosition.w}$$
- **Shore Foam Calculation:**
  $$\text{foamIntensity} = \text{saturate}(1.0 - \frac{\text{waterDepth}}{\text{_ShoreFoamDistance}})$$
- Produces organic dynamic white surf foam hugging Harrow Rock's shoreline, the pier piles, and the boat hull without requiring hand-painted textures or hard polygon edges.

### 3.3 Water Color & Optical Transmission
- **Deep Water Color:** Cold midnight navy `#0A1822` ($R=0.04, G=0.09, B=0.13$).
- **Shallow Water Color:** Frigid Atlantic teal `#1A3B44` ($R=0.10, G=0.23, B=0.27$).
- **Normal Maps:** Dual-scrolling tangent normal ripple textures scrolling at opposing angles to simulate wind-blown surface tension.
- **Smoothness:** $0.94$, reflecting moonlight, dock lanterns, and the sweeping lighthouse beam across wave crests.

---

## 4. Dock, Boardwalk & Coastal Environment

### 4.1 Heavy Timber Pier & Boardwalk
- **Walkway:** 36 individually modelled timber planks ($2.8\text{m} \times 0.25\text{m} \times 0.12\text{m}$) spaced with $3\text{cm}$ realistic drainage gaps, resting on 3 heavy longitudinal wooden bearers ($0.22\text{m} \times 0.26\text{m} \times 10.5\text{m}$).
- **Material:** `PBR_WeatheredPlanks.mat` (`wood_planks_grey`) with 1K diffuse, normal map for deep wood grain and splinter crevices, and wetness sheen.
- **Mooring Hardware:** 10 vertical round timber mooring piles ($d = 0.20\text{m}$, $h = 1.75\text{m}$) capped with oxidized iron banding and weathered rope loops.
- **Gallows Lantern Post:** Prominently positioned at the dock head with heavy knee bracing, iron hanging chain, vented hood, and a 15-lux point light casting long timber shadows down the pier.

### 4.2 Coastal Shoreline & Vegetation
- **Shoreline Promontory:** Organic concentric island mesh (`Harrow Rock Island`) with slipway cove dipping to $y = -2.35\text{m}$ to ensure clean floating clearance for the boat.
- **Photogrammetry Rocks:** Integrated CC0 scanned `CoastalRock_01` assets along the waterline and approaches, replacing primitive greybox shapes.
- **Winding Cart Track:** Weathered gravel and packed mud cart track bordered by rustic two-rail wooden fences leading from the jetty to the keeper's cottage threshold.

---

## 5. Interior PBR Materials & Furnishing

Each floor of the lighthouse tower and cottage was textured and furnished according to the master cutaway architectural reference:

| Floor | Architectural Function | Primary Floor Material | Wall Material | Key Furnishings & PBR Details |
|---|---|---|---|---|
| **Cottage Kitchen** | Cooking & Preparation | Weathered Timber Planks | Rough Plaster / Stone | Cast-iron potbelly stove with emissive firebox, copper kettles, wash sink with brass faucets, pine wall cabinetry. |
| **Cottage Dining** | Living & Radio Office | Dark Oak Floorboards | Fieldstone Masonry | Heavy round dining table, steaming stew bowl, ceramic vase with sunflowers, Marta's radio console. |
| **Ground Vestibule** | Entry & Maritime Storage | Earthen Stone Flagstones | Conical Stone Masonry | Coat peg rack with Daniel's yellow slicker & Crane's wool greatcoat, leather luggage, oak paraffin barrels, boot bench. |
| **First Floor** | Archive & Workshop | Tongue-and-Groove Oak | Dressed Masonry | Grand floor-to-ceiling bookcase, investigation desk, brass armillary sphere, terrestrial globe, logbook, clockwork vise bench. |
| **Second Floor** | Keeper's Living Quarters | Dark Stained Planks | Insulated Plaster | Robust keeper's timber bedstead, wool blanket, wardrobe locker, nautical station clock, chart drafting table. |
| **Level 3** | Thomas Crane's Quarters | Weathered Timber | Chiseled Stone | Iron bedstead, brass oil lamp, personal sea chest containing the childhood photo of Crane and Daniel. |
| **Level 4** | Storeroom & Oil Locker | Heavy Industrial Decking | Exposed Shaft Stone | Fuel drums, spare lantern mantle crates, rope coils, glass chimney storage. |
| **Level 5** | Service & Repair Landing | Heavy Timber Decking | Exposed Shaft Stone | Maintenance workbench, machinist vise, replacement bevel gears, lubricant cans. |
| **Level 6** | Rotation Clockwork Room | Cast Iron Floor Plating | Shaft Stone | Heavy clockwork rotation drive, cable drum, descending weight channel, centrifugal flyball governor. |
| **Level 7** | Lantern Service Platform | Checkerplate Steel | Upper Shaft Collar | Central support collar, access ladder, cleaning supply cabinet. |
| **Level 8** | Lantern Room & Beacon | Concentric Steel Walkway | Curved Iron & Glazing | 16-panel faceted glass enclosure, brass Fresnel lens housing, rotating mechanism, Hand-Crank Station. |

---

## 6. Volumetric Lighthouse Beam & Fresnel Beacon

- **Beacon Assembly:** Positioned at elevation $y = 26.80\text{m}$ on a massive central cast-iron pedestal.
- **Optics & Housing:** Brass Fresnel tiered lens structure with inner arc-lamp illuminant.
- **Volumetric Beam:** 
  - Cone mesh projecting $45\text{m}$ out through the lantern glazing into the coastal atmosphere.
  - Multiplied by an additive falloff shader with soft edge feathering.
  - Directional spot component ($20\text{ lux}$, $30^\circ$ outer angle, cool blue-white `#BFDEFF`) synchronized with mechanical gear rotation ($6^\circ/\text{second}$).

---

## 7. Cinematic URP Post-Processing Volumes

Five custom global Volume Profiles were authored and assigned to the game's atmospheric states via `Assets/_Project/Scripts/Editor/CinematicVolumeSetup.cs`:

### 7.1 Volume_Dusk (Act 1: Arrival & Exploration)
- **Tonemapping:** ACES cinematic curve.
- **Color Adjustments:** Post-Exposure $+0.40\,\text{EV}$, Contrast $+14$, Saturation $-10$, warm dusk filter `#F5F0EB`.
- **White Balance:** Temperature $-6\,\text{K}$, Tint $-2$.
- **Bloom:** Intensity $0.55$, Threshold $0.85$, Scatter $0.65$, warm golden tint (`#FFE6BF`).
- **Vignette:** Intensity $0.32$, Smoothness $0.40$, rounded.
- **Film Grain:** `FilmGrainLookup.Thin1`, Intensity $0.18$, Response $0.80$.
- **Shadows/Midtones/Highlights:** Calibrated linear ranges: Shadows `(0.95, 0.98, 1.05)`, Midtones `(1.0, 1.0, 1.0)`, Highlights `(1.04, 1.01, 0.97)`.

### 7.2 Volume_Night (Act 2: The Stalker)
- **Post-Exposure:** $+0.20\,\text{EV}$, Contrast $+20$, Saturation $-20$, cold oceanic filter `#D9EBF0`.
- **White Balance:** Temperature $-14\,\text{K}$, Tint $-4$.
- **Bloom:** Intensity $0.65$, Threshold $0.85$.
- **Vignette:** Intensity $0.38$.
- **Film Grain:** `FilmGrainLookup.Thin1`, Intensity $0.24$.

### 7.3 Volume_Storm (Act 3: Hand-Cranking & Generator Failure)
- **Post-Exposure:** $+0.15\,\text{EV}$, Contrast $+24$, Saturation $-28$, stormy steel-blue filter `#CCDFF0`.
- **White Balance:** Temperature $-18\,\text{K}$, Tint $-6$.
- **Bloom:** Intensity $0.75$, Threshold $0.88$.
- **Vignette:** Intensity $0.42$.
- **Film Grain:** `FilmGrainLookup.Medium1`, Intensity $0.30$.

### 7.4 Volume_Finale (Act 4: The Courtyard Flooding)
- **Post-Exposure:** $+0.18\,\text{EV}$, Contrast $+28$, Saturation $-32$, cold desperate filter `#C7E0EB`.
- **White Balance:** Temperature $-16\,\text{K}$, Tint $-5$.
- **Bloom:** Intensity $0.85$, Threshold $0.85$.
- **Vignette:** Intensity $0.44$.
- **Film Grain:** `FilmGrainLookup.Medium1`, Intensity $0.32$.

### 7.5 Volume_Dawn (Act 5: Morning Escape & Departure)
- **Post-Exposure:** $+0.45\,\text{EV}$, Contrast $+14$, Saturation $-5$, crisp morning light `#FFF7F0`.
- **White Balance:** Temperature $+4\,\text{K}$, Tint $0$.
- **Bloom:** Intensity $0.45$, Threshold $0.88$.
- **Vignette:** Intensity $0.28$.
- **Film Grain:** `FilmGrainLookup.Thin1`, Intensity $0.16$.

---

## 8. Preservation of Working Systems & Architecture

Throughout all graphics and material passes, zero regressions were introduced:
1. **Character Controller:** Fully tested with automated traversal scripts in Play Mode across all 8 flights ($y = 0.15\text{m} \to 25.60\text{m}$).
2. **Gameplay Anchors:** `Lamp`, `Crank Station`, `Radio`, `Table`, `Hot Meal`, `Locked Cellar Door`, `Wet footprints`, and `Cellar Hum` remain in their exact game-event positions with all colliders intact.
3. **Sound Propagation:** `NoiseEmitter` and `SoundPropagation` contracts remain attached to all interactive doors, crank stations, and footsteps.
4. **Scene Consistency:** Assets, volume profiles, and layout passes applied and verified identically across `MainMenu.unity`, `Act1.unity`, `Act2.unity`, `Act3.unity`, `Act4.unity`, and `Act5.unity`.
