# THE LIGHTHOUSE — COMPLETE GRAPHICS PROBLEM AUDIT

**Audit Date:** November 2026  
**Project:** The LightHouse (Harrow Rock)  
**Engine:** Unity 6 (6000.6.6f1)  
**Render Pipeline:** Universal Render Pipeline (URP 17.0+ / URP_Mac)  
**Target Profile:** Apple MacBook Air M3 (8 GB Unified Memory, 60 FPS target)  
**Visual Target:** Realistic, cinematic maritime psychological horror (*The Lighthouse*, *Still Wakes the Deep*, *Amnesia: The Bunker*)  

---

## 1. Executive Summary & Diagnostic Findings

A comprehensive diagnostic audit of the entire rendering setup, scene geometry, shaders, lighting configuration, post-processing stacks, and material assignments was conducted on the actual Unity project. 

The investigation confirmed that the visual defects described by the user—"muddy, blurry, overly brown/orange, visually overlapped, and poorly integrated"—are not subjective or solely texture-based. They are caused by **specific, identifiable technical flaws across five major subsystems**:

1. **Procedural Perlin Noise Asset Overrides:** High-quality PBR textures existed in `Assets/_Project/Art/Textures/PBR/` (`LighthouseStone`, `WeatheredPlanks`, `CoastalRock`, `WeatheredPlaster`, `RustedIron`), but `ReferenceVisualPass.cs` and `IslandArrivalPass.cs` actively ignored them. Instead, `ReferenceSurfaceLibrary.cs` procedurally generated low-resolution (512x512) Perlin noise bitmaps (`Salt Plaster.png`, `Wet Shale.png`, `Wet Mud Road.png`), ran software normal-map conversion on diffuse noise, and stamped them across the entire lighthouse tower, cottage walls, terrain, rocks, and roads.
2. **Double Orange Lighting Contamination:**
   - `DuskTimer.cs` hardcoded `duskAmbient = new Color(0.5f, 0.4f, 0.35f)` and forced it into `RenderSettings.ambientLight` every frame in `Update()`, bathing every polygon on Harrow Rock in flat, unshaded mud-orange illumination.
   - `AtmosphereSettings.asset` set `dusk.directionalLightColor` to saturated orange `{1.0, 0.61, 0.38}`, intensity `0.75`.
   - `Gallows Lamp Post / Dock Lantern Light` had `intensity = 15.0f`, `range = 20.0m`, and `shadows = None`, bleaching the entire dock and shoreline.
3. **URP Asset Shadow Suppression:** In `Assets/_Project/Settings/URP_Mac.asset`, `m_AdditionalLightShadowsSupported` was set to `0` (False) and `m_SoftShadowsSupported` to `0`. Consequently, every point and spot light in the lighthouse, cottage, and dock shone straight through solid masonry walls, timber floors, and ceilings without casting shadows, destroying all depth, local contrast, and interior ambient occlusion.
4. **Empty Post-Processing Volumes:** All five state volume profile assets (`Volume_Dusk.asset`, `Volume_Night.asset`, `Volume_Storm.asset`, `Volume_Finale.asset`, `Volume_Dawn.asset`) had `components: []` because editor setup scripts failed to call `AssetDatabase.AddObjectToAsset` when creating volume components. Thus, ACES tonemapping, vignette, color correction, and bloom were completely absent, while the camera fell back to FXAA which blurred fine geometry.
5. **Geometry Intersections & Z-Fighting:**
   - Concentric harmonic waves in `Harrow Rock Island` ($\pm 0.22\text{m}$) caused the terrain mesh to penetrate and poke through the flat `Courtyard` floor ($y = 0.05\text{m}$) and the `Mud Cart Road` ($y = 0.13\text{m}$).
   - 35+ `Shore Boulder` and `Hillside Boulder` objects were Unity primitive spheres squashed flat with dark noise textures.
   - `Shoreline Coastal Rock 1` and `2` had all 4 LOD levels (`LOD0`, `LOD1`, `LOD2`, `LOD3`) active simultaneously, causing severe geometric tearing and Z-fighting above the shoreline.
   - `HarrowSea.shader` suffered from quadrilateral stepping foam artifacts caused by low-tessellation vertex crest foam, and depth Z-fighting caused by `ZWrite On` on a transparent queue pass.

---

## 2. Granular Graphics Problem Audit Matrix

| # | What Looks Wrong | Technical Root Cause | Exact Unity Object / Asset / Setting | Proposed Correction | Test Methodology |
|---|---|---|---|---|---|
| **1** | **Entire island terrain looks like a massive muddy brown/grey blob with no material separation.** | `Harrow Rock Island` mesh is textured with a single material (`Wet Shale.mat`) using a 512x512 low-res procedural noise texture tiled linearly across 100 meters. No distinction between wet shoreline, dry sand, rocky slope, gravel, and cart road. | GameObject: `Island Arrival / Harrow Rock Island`<br>Mesh: `Island.asset`<br>Material: `Wet Shale.mat`<br>Code: `IslandArrivalPass.cs:Land()` | Author `TheLightHouse/CoastalIsland` multi-texture terrain shader blending shoreline wet sand, rocky cliff slope, gravelly soil, and path via world-normal slope and height modulation. Feed authentic PBR textures (`CoastalRock`, `WeatheredPlanks`, dark wet silt). | Close-up inspection at player start, dock transition, and courtyard approach. Verify distinct material boundaries and roughness. |
| **2** | **Ground terrain pokes up through the courtyard floor and cart road.** | `Courtyard` is positioned at $y = 0.05\text{m}$. `Harrow Rock Island` harmonic waves reach $y = 0.10\text{m} \to 0.22\text{m}$ in the courtyard area ($x \in [-14, 11], z \in [-16, -7]$), intersecting and piercing the stone flagstones. `Mud Cart Road` is also at $y = 0.13\text{m}$ and is intersected by terrain waves. | GameObjects: `Courtyard`, `Mud Cart Road`, `Harrow Rock Island`<br>Code: `IslandArrivalPass.cs:Land()`, `BuildFullGameScenes.cs` | In `IslandArrivalPass.cs`, explicitly flatten/depress terrain under the courtyard ($y = -0.05\text{m}$) and conform terrain directly along the cart road curve ($z \in [-34, -10.5]$). | Walk around Courtyard and along Cart Road in Play Mode. Verify zero terrain protrusion or z-fighting. |
| **3** | **Coastal boulders look like smooth melted plastic spheres.** | 35+ boulders along the shoreline and hillside were generated using `GameObject.CreatePrimitive(PrimitiveType.Sphere)` with `scale = (2.5, 1.1, 1.7)` and assigned `Wet Shale.mat` with uniform specular. | GameObjects: `Shore Boulder (1..26)`, `Hillside Boulder (1..10)`, `Dock Shore Rock (1..6)`<br>Code: `IslandArrivalPass.cs:223, 232, 321` | Replace sphere primitives with authentic 3D photogrammetry scanned rock assets (`CoastalRock_01.fbx`) with varied rotations, scales, natural shoreline embedding, and `PBR_CoastalRock.mat`. | Inspect shoreline from dock and beach. Verify sharp, natural rock facets and proper PBR response. |
| **4** | **Torn, jagged polygon mess floating over the dock and water.** | In `High Quality Props / Shoreline Coastal Rock 1 & 2`, all four child LOD levels (`coast_rocks_01_LOD0`, `LOD1`, `LOD2`, `LOD3`) are active simultaneously in hierarchy, Z-fighting violently against one another. | GameObjects: `High Quality Props/Shoreline Coastal Rock 1 & 2`<br>Components: `MeshRenderer` on LOD0, LOD1, LOD2, LOD3 | Configure the `LODGroup` properly and ensure only the active LOD level is rendered. Deactivate redundant overlapping renderers. | Camera view looking down the dock from player spawn. Verify rock has clean, solid geometry with no flickering. |
| **5** | **Absurd primitive "pine tree" made of cubes and spheres next to the dock.** | `Coastal Gnarled Pine` was assembled from 12 stretched cubes for trunk/boughs and 8 primitive spheres with green noise for needle clusters. | GameObject: `Island Arrival / Coastal Gnarled Pine`<br>Code: `IslandArrivalPass.cs:PineTree()` | Remove primitive programmer-art pine tree. Replace with weathered coastal drift timbers or natural low-lying coastal gorse shrubs. | Inspect right flank of the dock boardwalk. Verify natural silhouette and absence of geometric primitives. |
| **6** | **Ocean displays massive white quadrilateral stepping blocks across waves.** | In `HarrowSea.shader`, vertex shader calculates `waveOffset.y` across large quads and fragment shader applies `smoothstep(0.15, 0.75, crestFoam)`. Linear vertex interpolation across low-resolution quads produces diamond/quad stepping bands. | Shader: `TheLightHouse/Harrow Sea`<br>Material: `Assets/_Project/Materials/Reference/Harrow Sea.mat`<br>Lines: 101, 146-150 | Replace vertex-interpolated crest foam with high-frequency fragment noise wave-crest calculation. Increase ocean tessellation grid density. | View open ocean looking north and west from dock and tower gallery. Verify smooth, natural Gerstner wave crests. |
| **7** | **Jagged white artifacts and depth fighting where ocean meets terrain and dock.** | `HarrowSea.shader` uses `ZWrite On` on a Transparent queue pass (`Queue = Transparent`, `RenderType = Transparent`). When sampling `_CameraDepthTexture`, `ZWrite On` causes self-depth conflict and hard pixelated foam boundaries. | Shader: `TheLightHouse/Harrow Sea`<br>Pass Tags: `ZWrite On`, `Blend SrcAlpha OneMinusSrcAlpha` | Set `ZWrite Off` on the transparent water pass. Refine `LinearEyeDepth` shore foam fade with smooth noise perturbation. | Inspect water-beach and water-dock pile contact line. Verify soft, organic shoreline wash. |
| **8** | **Lighthouse tower walls look like dirty greenish-brown camouflage spots.** | `ReferenceVisualPass.cs` assigns `Salt Plaster.mat` to `Tapered Masonry`. `Salt Plaster` uses a 512x512 Perlin noise texture with large dark circular spots tiled 3x3 on top of tower UVs, creating thousands of repeating dirty spots. `PBR_LighthouseStone.mat` was never used. | GameObject: `Reference Visuals/Lighthouse/Exterior/Tapered Masonry`<br>Material: `Salt Plaster.mat`<br>Code: `ReferenceVisualPass.cs:329` | Assign `PBR_LighthouseStone.mat` with authentic 1K granite masonry albedo, normal, and cavity AO. Set UV coordinates so blocks are realistically proportioned (~0.3m x 0.5m). | Exterior full-tower screenshot and close-up stone masonry inspection at entrance. Verify historic stone texture. |
| **9** | **Cottage and interior rooms have ugly mottled blotchy walls and blocky furniture.** | Interior and exterior walls of cottage, vestibule, kitchen, archive, and living quarters are all assigned procedural `Salt Plaster.mat`. Furniture uses stretched procedural `Old Timber.mat`. | GameObjects: `AttachedCottage`, `GroundFloor`, `FirstFloor`, `SecondFloor` walls & furnishings<br>Materials: `Salt Plaster.mat`, `Old Timber.mat` | Assign `PBR_WeatheredPlaster.mat` to interior plaster walls, `PBR_WeatheredPlanks.mat` to floors, ceilings, and timber furnishings, and `PBR_RustedIron.mat` to metal fittings. | Interior walkthrough of Ground Vestibule, Kitchen, Archive, Quarters. Verify warm, authentic maritime materials. |
| **10** | **Entire environment is bathed in flat, muddy orange/brown light.** | `DuskTimer.cs:Update()` overrides `RenderSettings.ambientLight` with `Color.Lerp(new Color(0.5, 0.4, 0.35), ...)` every frame. `AtmosphereSettings` sets directional light to `{1.0, 0.61, 0.38}`. `Dock Lantern Light` has intensity `15.0` with no shadows. | Script: `DuskTimer.cs:6, 17`<br>Asset: `AtmosphereSettings.asset`<br>GameObject: `Gallows Lamp Post / Dock Lantern Light` | In `DuskTimer.cs`, set `duskAmbient = new Color(0.10, 0.13, 0.17)` (cold maritime twilight sky). In `AtmosphereSettings`, set dusk directional light to `{0.82, 0.76, 0.68}` (neutral cool silver-amber). Lower dock lantern intensity to `2.2` and enable soft shadows. | Test in Play Mode during Act 1 Dusk. Verify cold North Atlantic outdoor atmosphere with localized warm lantern pools. |
| **11** | **All lamps shine through solid walls and floors without casting shadows.** | In `URP_Mac.asset`, `m_AdditionalLightShadowsSupported` is set to `0` (False). URP suppresses shadows for all Point and Spot lights regardless of component settings. | Asset: `Assets/_Project/Settings/URP_Mac.asset`<br>Property: `m_AdditionalLightShadowsSupported = 0` | Set `m_AdditionalLightShadowsSupported = 1` and `m_SoftShadowsSupported = 1` in `URP_Mac.asset`. Configure Point and Spot lights to cast soft shadows with appropriate bias. | Walk inside cottage and tower. Verify light does not leak through closed doors, walls, or floors to the outside. |
| **12** | **Post-processing volume profiles are completely empty.** | `Volume_Dusk.asset`, `Volume_Night.asset`, `Volume_Storm.asset`, etc. have `components: []` because editor setup scripts created `VolumeComponent` objects without calling `AssetDatabase.AddObjectToAsset`. | Assets: `Assets/_Project/Settings/Volume_*.asset`<br>Scripts: `CinematicVolumeSetup.cs`, `ApplyGraphics1.cs` | Rewrite volume generation script to properly add volume components as persistent sub-assets. Configure ACES tonemapping, color adjustments, vignette, and film grain. | Inspect volumes in Inspector during runtime. Verify active components and cinematic contrast. |
| **13** | **Lighthouse rotating beam looks like a solid opaque cone cutting through the sky.** | `LampCone.asset` has only apex and rim vertices without intermediate rings. `LampBeamURP.shader` length fade formula evaluates to near-zero or renders with harsh edge clipping. | Mesh: `LampCone.asset`<br>Shader: `TheLightHouse/Lamp Beam URP`<br>Script: `ApplyGraphics1.cs:175` | Generate multi-segmented beam cone with smooth longitudinal gradient. Refine shader with inverse-radial falloff and soft depth fade. | Inspect rotating beam from courtyard and dock at night. Verify soft, atmospheric volumetric beam. |
| **14** | **Blury, soft image quality and texture shimmering.** | Camera uses `FastApproximateAntialiasing` (FXAA) with MSAA disabled in URP. FXAA blurs high-frequency textures and edge details. | Asset: `URP_Mac.asset` (`m_MSAA = 1`)<br>Camera: `Player Camera` (`antialiasing = FastApproximateAntialiasing`) | Enable MSAA 4x in `URP_Mac.asset` (Apple Silicon M3 tile memory handles 4x MSAA with virtually zero cost). Switch camera AA to SMAA or MSAA. | Compare texture sharpness and polygon edge clarity in first-person play. |

---

## 3. Render Pipeline & Hardware Configuration Verification

- **Engine Version:** Unity 6000.6.6f1  
- **Active Graphics API:** Metal (Apple Silicon M3, 8 GB Unified Memory)  
- **Pipeline Asset:** `Assets/_Project/Settings/URP_Mac.asset`  
- **Color Space:** Linear  
- **HDR:** Enabled (`supportsHDR = true`)  
- **Render Scale:** 1.0 (Full native display resolution)  
- **Depth Texture:** Required & Enabled  
- **Opaque Texture:** Required & Enabled  

### Hardware Constraints & Performance Budget
- **Target Frame Rate:** 60 FPS constant on MacBook Air M3.
- **Draw Call Budget:** $< 800$ batches per frame.
- **Memory Footprint:** Keep active textures within 1.5 GB VRAM budget. Use compressed textures (ASTC / BC7 / DXT5).
- **Shadow Cascades:** 4 cascades with maximum distance 64m (optimized for first-person courtyard and dock navigation).

---

## 4. Verification & Testing Protocol

All corrections will be implemented directly in project assets, shaders, and editor generation pipelines, followed by validation through:
1. **Compilation Verification:** 0 errors, 0 obsolete warnings.
2. **Editor Scene Building:** Automated regeneration of Acts 1–5 and MainMenu.
3. **Play Mode Inspection:** Running the game from Player start on the dock to the lantern room.
4. **Camera-Matched Screenshot Capture:** Capturing high-resolution PNGs at the 14 mandatory audit camera positions for direct visual verification.
5. **Performance Measurement:** Profiling frame times and draw calls on M3 hardware.
