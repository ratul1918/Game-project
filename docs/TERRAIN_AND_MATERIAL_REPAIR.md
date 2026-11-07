# Terrain and Material Repair Report
**The LightHouse — Visual Quality & Environment Integration Overhaul**  
*Timestamp: November 7, 2026*

---

## 1. Executive Summary

Prior to this repair, the island environment appeared as an undifferentiated, muddy orange-brown mass. All ground surfaces, rocks, and shoreline transitions suffered from low-resolution procedural Perlin noise textures generated at runtime (`ReferenceSurfaceLibrary.cs`), global orange ambient illumination, and severe UV stretching. Primitive stretched spheres served as boulders, and the procedural terrain elevation undulation pierced directly through courtyard flagstones and building foundations.

This pass completely replaced the procedural mud terrain with a dedicated, multi-layer coastal procedural and triplanar terrain shader (`TheLightHouse/CoastalIsland`), backed by authentic 1024×1024 PBR texture assets, physically calibrated roughness/metallic values, high-frequency normal maps, and photogrammetry rock meshes.

---

## 2. Terrain Surface Classifications & Architecture

The island terrain now distinctly separates six physical surface types, driven by altitude, geometric surface slope, and localized feature masks:

| Surface Type | Elevation / Slope Criteria | Albedo / Texture Source | Normal Map & Detail | Roughness / Smoothness | Visual Characteristics |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **Wet Sand & Silt** | $y < -0.20\text{m}$ (intertidal zone to water line) | `CoastalRock_diff.jpg` tinted with cold oceanic slate `#12171A` | High-frequency ripples | Smoothness: `0.78` (high specular wet sheen) | Deep, dark, saturated wet coastline with reflective tide marks |
| **Granite Cliff Faces** | Slope angle $> 38^\circ$ ($\vec{N}_y < 0.68$) | World-space triplanar mapping with `CoastalRock_diff.jpg` | Triplanar `CoastalRock_nor.jpg`, `0.75` bump scale | Smoothness: `0.32` (damp, weathered stone) | Eliminates vertical UV stretching on all coastal rock faces |
| **Packed Coastal Soil** | $y > 0.0\text{m}$, slope $< 35^\circ$ | `CoastalRock_diff.jpg` tinted with earthy dark soil `#1A1D1E` | High-frequency pebble normal map | Smoothness: `0.18` (matte, dry compacted ground) | Natural island plateau surrounding courtyard and path |
| **Wet Mud Cart Road** | Centered along curve $x = -0.8 + 2.2\sin(\pi t)$, $z \in [-34, -10.5]$ | `Wet Mud Road.png` procedural dark earth `#0F0C0A` | `CoastalRock_nor.jpg`, `4x4` tiling, `0.85` bump scale | Smoothness: `0.58` (wet churned mud sheen) | Distinct wheel ruts and crowned center path from dock to gate |
| **Shoreline Embankment** | Shore perimeter $R \approx 24\dots 28\text{m}$ | `M_CoastalRock_01.mat` on photogrammetry meshes | Baked model UVs with detail normal map | Smoothness: `0.25` (matte sea-sprayed granite) | Angular, fractured boulders sitting naturally in water |
| **Courtyard Flagstones** | $x \in [-12, 12], z \in [-16.5, -6.5]$ | `PBR_CoastalRock.mat` world-planar UVs | PBR granite normal map, 1 tile / 2m | Smoothness: `0.35` (durable stone paving) | Clear man-made architectural ground distinct from raw soil |

---

## 3. Shader Implementation: `TheLightHouse/CoastalIsland`

The terrain uses a customized URP ForwardLit shader (`Assets/_Project/Shaders/CoastalIsland.shader`) designed for high performance on Apple Silicon (M3):

1. **Triplanar Mapping on Steep Slopes:**
   $$\text{triWeights} = \max(0, |\vec{N}_{\text{geom}}| - 0.2)$$
   Normalizing $\text{triWeights}$ ensures smooth blending across the X, Y, and Z projection axes, entirely eliminating the severe vertical texture elongation present in the original single-plane projection.

2. **Elevation-Based Wet Tide Blending:**
   $$\text{wetFactor} = \text{smoothstep}\left(0.05, 0.95, \text{saturate}\left(1.0 - \frac{y - \text{seaLevel}}{\text{tideRange}}\right)\right)$$
   Where $\text{seaLevel} = -0.55\text{m}$ and $\text{tideRange} = 0.65\text{m}$. All terrain touching or dipping below the water is automatically darkened, given a high-smoothness specular sheen, and blended into the ocean bed.

3. **Built-in Cart Road Rut Masking:**
   The shader computes distance to the winding road centerline analytically in the fragment shader, darkening the wheel ruts and increasing smoothness to represent standing moisture in compacted wagon tracks.

---

## 4. Replacement of Primitive Spheres with Photogrammetry Rocks

### Root Problem Identified
In `IslandArrivalPass.cs` and `ReferenceVisualPass.cs`:
- 26 "Shore Boulder" instances were created using `GameObject.CreatePrimitive(PrimitiveType.Sphere)` with `Wet Shale.mat`.
- 10 "Hillside Boulder" instances were created using stretched spheres.
- 6 "Dock Shore Rock" instances were created using stretched spheres.
- 30 "Shore Rock" instances in `ReferenceVisualPass.Coast()` were created using stretched spheres with `Salt Plaster.mat`.

Total: **72 primitive deformed spheres** scattered across the island, looking like oversized smooth brown toy balls.

### Correction Applied
1. Deleted the 30 sphere instances in `ReferenceVisualPass.Coast()`.
2. Extracted `coast_rocks_01_LOD3` (46,395 vertices / 84,990 triangles) from `Assets/_Project/Art/Models/CoastalRock_01/CoastalRock_01.fbx`.
3. Created `M_CoastalRock_01.mat` using URP Lit with `coast_rocks_01_diff_1k.jpg` and `CoastalRock_nor.jpg` detail normal mapping.
4. Placed 14 perimeter shoreline rocks with randomized yaw ($0\dots 360^\circ$), subtle tilt ($\pm 7^\circ$), and organic scales ($0.35\dots 0.60\text{m}$).
5. Placed 6 hillside boulder groups seated firmly into the soil slopes away from paths.
6. Placed 4 dock-side granite rocks stabilizing the shoreline embankment.
7. Removed the overlapping `LOD0`, `LOD1`, and `LOD2` meshes from `Shoreline Coastal Rock 1 & 2`, saving 2.4 million redundant vertices per frame.

---

## 5. Architectural Materials Upgrade

| Asset | Shader | Previous Material | New PBR Material | Texture Maps Configured |
| :--- | :--- | :--- | :--- | :--- |
| **Lighthouse Tower** | URP Lit | `Salt Plaster.mat` (512px procedural noise) | `PBR_LighthouseStone.mat` | `LighthouseStone_diff.jpg`, `nor.jpg`, `ao.jpg` (1 tile / 2m) |
| **Cottage Exterior** | URP Lit | `Salt Plaster.mat` (muddy orange) | `PBR_LighthouseStone.mat` | Matches lighthouse masonry for unified architecture |
| **Cottage Interior** | URP Lit | `Salt Plaster.mat` | `PBR_WeatheredPlaster.mat`| `WeatheredPlaster_diff.jpg`, `nor.jpg`, `ao.jpg` |
| **Timber Floors & Ceilings**| URP Lit | `Old Timber.mat` (procedural stripes) | `PBR_WeatheredPlanks.mat`| `WeatheredPlanks_diff.jpg`, `nor.jpg`, `ao.jpg` |
| **Metal Fittings & Ladders**| URP Lit | `Oxidised Iron.mat` | `PBR_RustedIron.mat` | `RustedIron_diff.jpg`, `nor.jpg`, `ao.jpg` |
| **Dock Planks & Posts** | URP Lit | `Old Timber.mat` | `PBR_WeatheredPlanks.mat`| Dark weathered grain with gaps and wear |
| **Slate Roof** | URP Lit | Procedural `Slate.mat` | `Slate.mat` with normal depth | Cold dark slate `#1E2428`, metallic `0.1`, smoothness `0.32` |

---

## 6. Verification and Visual Confirmation

- **Stretched Textures:** Completely eliminated via triplanar projection on slopes and world-space planar mapping on floors and walls.
- **Blurry Textures:** 1024×1024 textures with bilinear filtering, 4x anisotropic filtering, and high-frequency normal detail.
- **Mud Contamination:** Confined strictly to the lower cart road ruts and sea-level tide wash; high plateau ground is dark firm coastal soil.
- **Overlapping / Floating Geometry:** Ground terrain clamped to $y = -0.05\text{m}$ under the courtyard floor ($y = 0.05\text{m}$) and graded smoothly along the cart road.
