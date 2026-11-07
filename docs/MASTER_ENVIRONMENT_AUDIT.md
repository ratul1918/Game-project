# MASTER ENVIRONMENT AUDIT & TECHNICAL PRESERVATION REPORT
**Project:** The LightHouse  
**Engine:** Unity 6000.6.6f1 (Universal Render Pipeline - URP)  
**Target Hardware:** MacBook Air M3 (8-core CPU / 10-core GPU, 8 GB Unified Memory)  
**Date:** November 2026  
**Auditor:** Senior AAA Environment & Technical Art Team  

---

## 1. Project Context & Executive Overview

*The LightHouse* is a first-person maritime psychological horror game set on Harrow Rock, an isolated island in the North Atlantic. The player controls Daniel Hale, a guilt-ridden sailor tasked with maintaining a remote lighthouse following the mysterious disappearance of keeper Thomas Crane.

### Core Environmental Elements
1. **Lighthouse Tower:** A cylindrical stone tower with 8 distinct elevation levels:
   - Level 0 (Ground): Entrance vestibule, coat hooks, storage alcove, starter curb.
   - Level 1 ($y = 3.20\text{m}$): Archive and investigation workshop (bookcase, bureau desk, repair bench).
   - Level 2 ($y = 6.40\text{m}$): Keeper's living quarters (bedstead, nightstand, wardrobe, drafting table, clock).
   - Level 3 ($y = 9.60\text{m}$): Crane's private room (iron bed, sea chest with child photo).
   - Level 4 ($y = 12.80\text{m}$): Storeroom & oil locker (paraffin barrels, supply crates).
   - Level 5 ($y = 16.00\text{m}$): Service landing (heavy workbench, tool vise).
   - Level 6 ($y = 19.20\text{m}$): Rotation clockwork & winch room (drive base, cable drum, governor).
   - Level 7 ($y = 22.40\text{m}$): Upper lantern service platform (central support collar, wall ladder).
   - Level 8 ($y = 25.60\text{m}$): Top lantern room, Fresnel lens beacon, rotating mechanism, Hand-Crank Station.
2. **Attached Cottage:** Separate domestic structure with kitchen cabinetry, cast-iron stove with glowing embers, dining table with hot meal, and radio office.
3. **Island & Exterior Environment:** Harrow Rock island terrain, rocky bluffs, coastal mud roads, weathered fences, winding paths, wooden pier/dock, coastal pine trees, ocean waters, and storm weather.

---

## 2. Technical Stack & Pipeline Audit

| Subsystem | Specification / Existing State | Audit Finding | Upgrade Action |
|---|---|---|---|
| **Engine Version** | Unity 6000.6.6f1 | Stable, fully compatible with Render Graph & URP | Preserve existing Unity 6 engine |
| **Render Pipeline** | Universal Render Pipeline (`URP_Mac` active asset) | High dynamic range (`supportsHDR=true`), Depth Texture (`true`), Opaque Texture (`true`) | Retain URP; do NOT migrate to HDRP (M3 8GB memory budget requires URP efficiency) |
| **Post-Processing** | 5 Global Volumes in scene (`Dusk`, `Night`, `Storm`, `Finale`, `Dawn`) | **Empty profiles (0 components)**; no tonemapping, bloom, vignette, or film grain | Populate all 5 volume profiles with ACES tonemapping, bloom, color grading, vignette, and grain |
| **Ocean / Water** | Custom HLSL shader (`TheLightHouse/Harrow Sea`) on mesh disc | Basic vertex displacement and specular reflection; lacks depth foam, shore contact, and normal ripple detail | Upgrade `HarrowSea.shader` with depth-based shore foam, normal ripple blending, and dark oceanic absorption |
| **Terrain & Island** | Custom mesh `Island Arrival` (`Act1-Island.asset`) | Procedural wet shale and mud road textures; rocks lack irregular micro-detail | Integrate multi-scale PBR coastal rock formations, wet shoreline gradient, and vegetation variation |
| **Pier & Dock** | Procedural timber boardwalk with posts and railings | Good proportions, but wood material is flat without individual plank grain or nail detail | Upgrade with high-resolution weathered timber PBR (albedo, normal, roughness, rusted fittings) |
| **Lighthouse Architecture** | Repaired modular geometry (`ReferenceVisualPass.cs`) | All 8 staircase flights connected and 100% playable in CharacterController test | Upgrade exterior masonry stone texture, mortar depth, salt efflorescence, and weathered plaster |
| **Lighthouse Beam** | `LampBeamURP.shader` and rotating light | Basic cone mesh; lacks realistic volumetric scattering and lens synchronization | Enhance beam cone with smooth edge falloff, atmospheric scattering, and rotating Fresnel synchronization |
| **Weather & Storm** | `ReferenceWeatherAuthoring.cs` particle rain and lightning | Good foundation, but rain streaks lack ground splash impacts and wind variation | Add ground/roof splash particles, wet surface specular boost, and atmospheric lightning flashes |
| **Player Controller** | `CharacterController` ($r=0.35\text{m}, h=1.8\text{m}, stepOffset=0.3\text{m}$) | Validated 100% traversal from ground to lantern room and back | Preserve player controller and physics configuration |
| **Story & Gameplay** | Acts 1 to 5 scenes, StoryBeats, Inventory, Audio | Fully operational | Strictly preserve all story anchors and scripts |

---

## 3. Hardware Target Constraints (MacBook Air M3, 8 GB Shared RAM)

The target hardware is an Apple Silicon M3 with 8 GB shared unified memory:
- **Maximum System RAM Budget:** 8,192 MB (shared between macOS, CPU, and Metal GPU).
- **Safe Game Memory Budget:** $\le 2,500\text{ MB}$ total allocated memory (leaving ample room for OS and editor/background tasks).
- **Target Resolution:** $1600 \times 900$ to $1920 \times 1080$ at consistent $60\text{ FPS}$.
- **Texture Budget:** 1K–2K textures with ASTC / BC7 compression; avoid uncompressed 4K textures that exhaust VRAM.
- **Draw Call & Batching Budget:** $\le 450$ draw calls per frame; maintain SRP batching and GPU instancing on all materials.
- **Shadow Budget:** Controlled shadow distance ($24\text{m} - 35\text{m}$) with 1 cascade or soft shadows on directional light; point lights non-shadow casting except critical hero lights.

---

## 4. Preservation Matrix

The following systems are protected and will NOT be overwritten or broken:
1. `Assets/_Project/Scripts/Player/FirstPersonController.cs` (Movement & camera input)
2. `Assets/_Project/Scripts/Interaction/` (Door opening, crank interaction, item pickup)
3. `Assets/_Project/Scripts/Lamp/` (Lamp fuel, hand-cranking mechanics, lighthouse beam rotation)
4. `Assets/_Project/Scripts/Story/` (Act progression, Marta recordings, radio messages)
5. `Assets/_Project/Scripts/Drowned/` (Stalker AI, sight detection, swimming logic)
6. `Assets/_Project/Scripts/Audio/` (Sound propagation, shanty humming, storm audio)
7. Gameplay Anchors: `Lamp`, `Crank Station`, `Radio`, `Table`, `Hot Meal`, `Locked Cellar Door`, `Wet footprints`, `Cellar Hum`.

---

## 5. Master Implementation Sequence

```
1. Master Environment Audit & Preservation (Completed)
   ↓
2. Architecture & Staircase Foundation (Completed in previous pass, preserved)
   ↓
3. Photorealistic PBR Material Library Upgrade (Stone, Wood, Metal, Glass, Cloth)
   ↓
4. Realistic Ocean, Shoreline & Coastal Rocks Enhancement
   ↓
5. Pier, Boardwalk & Wooden Structures Upgrade
   ↓
6. Coastal Vegetation & Wind-Swept Maritime Trees
   ↓
7. Interior Photorealism, Props & Atmospheric Micro-Details
   ↓
8. Cinematic Lighting, Volumetric Lighthouse Beam & Storm Weather
   ↓
9. URP Volume Post-Processing Tuning (5 GameStates: Dusk -> Night -> Storm -> Finale -> Dawn)
   ↓
10. Performance Optimization, Multi-Preset Quality & M3 8GB Benchmarking
   ↓
11. Full Play Mode QA, 16 Validation Screenshots & Final Documentation
```
