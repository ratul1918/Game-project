# PHOTOREALISM & GRAPHICS AUDIT
**Project:** The LightHouse  
**Pipeline:** Unity 6 Universal Render Pipeline (URP)  
**Hardware Profile:** Apple Silicon M3 (Unified Memory Architecture)  
**Date:** November 2026  
**Auditor:** Technical Art & Lighting / Materials Lead  

---

## 1. Visual Quality Assessment by Asset Category

### 1.1 Lighthouse Exterior Stone & Masonry
- **Current State:** The exterior cylindrical tower uses a procedural `Salt Plaster` material with noise-based diffuse tint and generated normal map.
- **Defects / Limitations:**
  - Lacks tactile stone relief; mortar joints between granite blocks are shallow.
  - Tiling pattern can appear uniform over the 26m vertical tower elevation.
  - Absence of weathering streaks, salt spray crusting near the base, and rain runoff stains.
- **Target Photorealistic Upgrade:**
  - High-definition PBR granite masonry with deep recessed mortar lines and chiseled block relief.
  - Height-modulated roughness to represent damp, water-saturated stone vs dry wind-exposed ridges.
  - Macro-variation gradient: dark damp algae wash along the lower 4m, wind-weathered salt efflorescence on the windward side.

### 1.2 Pier & Wooden Boardwalk
- **Current State:** Procedural `Old Timber` / `Weathered Fence Timber` with basic plank seams.
- **Defects / Limitations:**
  - Boards look flat; missing distinct grain, split ends, and edge chamfers.
  - Lacks oxidized iron nail heads and heavy structural bolts.
  - Specular response is uniform across planks rather than showing damp puddles in depressions.
- **Target Photorealistic Upgrade:**
  - Weathered maritime timber planks with high-frequency splinter detail, cracks, and uneven edges.
  - PBR roughness map reflecting wet surface gloss in puddles and weathered matte finish on raised ridges.
  - Detailed rusted iron bolt studs pinning the boardwalk timbers to the piling posts.

### 1.3 Ocean & Shoreline
- **Current State:** `HarrowSea.shader` evaluates two sine waves in vertex displacement and a simple Fresnel curve in the fragment shader.
- **Defects / Limitations:**
  - No shore foam or depth intersection where water meets rocks and the beach.
  - Wave shape is purely sinusoidal; lacks realistic Gerstner wave cresting and chop.
  - Normal map ripples are generated from a mathematical formula rather than layered wave normals.
- **Target Photorealistic Upgrade:**
  - Gerstner wave calculation for sharp, authentic ocean wave peaks and broad troughs.
  - Depth-based shoreline foam using `_CameraDepthTexture` for dynamic surf washing against the island shale and rocks.
  - Layered scrolling wave normal maps giving micro-ripples and wind chop.
  - Cold, deep North Atlantic color absorption: `#0A1822` deep sea, `#1A3842` shallows, `#C4D8E0` sea spray foam.

### 1.4 Coastal Rocks & Terrain
- **Current State:** Terrain mesh with `Wet Shale` and `Wet Mud Road` textures. Several spherical placeholder rock formations around the coast.
- **Defects / Limitations:**
  - Spherical rock meshes look artificial and low-poly.
  - Mud road has procedural Perlin ruts but lacks stone aggregate and standing puddle reflections.
- **Target Photorealistic Upgrade:**
  - Integration of multi-scale `CoastalRock_01` PBR rock meshes with irregular jagged silhouettes, tidal watermarks, and fissure crevices.
  - Terrain material enhancement with blended gravel, mud ruts, and specular puddle planes.

### 1.5 Vegetation & Trees
- **Current State:** Primitive pine tree trunk and conical needle layers, plus procedural coastal scrub cubes.
- **Defects / Limitations:**
  - Conical needle geometry looks like programmer art.
  - Scrub consists of small colored boxes.
- **Target Photorealistic Upgrade:**
  - Wind-swept maritime pine trees with twisted timber trunks, layered needle fronds, and natural silhouettes.
  - Coastal scrub bushes with gorse branch detail, leaf cards, and wind oscillation.

### 1.6 Interior Materials & Micro-Details
- **Current State:** Repaired room layouts feature high-quality procedural furniture (Archive bookcase, desk, globe, keeper bed, dining table, stove).
- **Defects / Limitations:**
  - Furniture materials could benefit from micro-scratch normal detail and aged varnish roughness.
  - Dust motes and atmospheric ceiling drop shadows can be enhanced.
- **Target Photorealistic Upgrade:**
  - PBR wood with aged varnish and brass trim.
  - Worn parchment and leather textures for logbooks, chart folios, and suitcases.
  - Warm interior point lights with realistic physical falloff.

### 1.7 Lighting, Beam & Storm Atmosphere
- **Current State:** Basic directional light and coastal rain particle emitter.
- **Defects / Limitations:**
  - Global Volume profiles for post-processing are completely empty.
  - Lighthouse beam is a simple cone without atmospheric scattering.
  - Rain particles fall straight down without ground splashes.
- **Target Photorealistic Upgrade:**
  - Full URP Volume post-processing stack: ACES Tonemapping, Bloom, Color Adjustments, Vignette, Film Grain.
  - Atmospheric rotating lighthouse beam with soft edge falloff and volumetric appearance.
  - Multi-tier storm system: angled wind-driven rain, ground splash impacts, and lightning flash illumination.

---

## 2. Priority Action Matrix

```
[CRITICAL] URP Volume Post-Processing (Populate all 5 GameState Profiles)
[HIGH]     Ocean Shader Upgrade (Gerstner waves, depth foam, shore surf)
[HIGH]     PBR Materials Enhancement (Stone, Timber, Iron, Shale)
[HIGH]     Volumetric Lighthouse Rotating Beam
[MEDIUM]   Storm Weather Multi-Tier System (Rain, Splashes, Lightning)
[MEDIUM]   Coastal Rocks & Vegetation Realism
[MEDIUM]   Interior Lighting Falloff & Environmental Polish
```
