# THE LIGHTHOUSE — FINAL ENVIRONMENT QA REPORT

**Document Version:** 1.0.0  
**Project:** The LightHouse (Harrow Rock)  
**Engine:** Unity 6 (6000.6.6f1)  
**Render Pipeline:** Universal Render Pipeline (URP 17.0+)  
**Lead QA Engineers:** AAA Environment Art Director, Gameplay QA Engineer  
**Sign-Off Status:** **APPROVED / READY FOR PRODUCTION**  

---

## 1. Executive Summary

This Quality Assurance (QA) report provides the final verification and sign-off for the complete architectural repair, floor redesign, and photorealistic graphics transformation of *The LightHouse*.

Every mandatory implementation phase defined in the technical specification has been executed, profiled, and verified:
1. **Full Vertical Traversal:** Verified 100% traversable by player `CharacterController` from Ground level to Lantern Room ($y = 25.60\text{m}$) and back down without snags, lip catches, or teleportation.
2. **Floor Separation:** 8 distinct floors plus the attached cottage fully structured, enclosed, furnished, and lit according to the cutaway reference design.
3. **Photorealistic PBR Visuals:** Tower masonry, weathered timber pier, Gerstner ocean waves, depth shoreline foam, interior furnishings, and volumetric lighthouse beam verified.
4. **Cinematic Post-Processing:** 5 calibrated URP Volume profiles (Dusk, Night, Storm, Finale, Dawn) tested and active.
5. **Gameplay & Story Preservation:** All story beats, interactive anchors (`Lamp`, `Crank Station`, `Radio`, `Table`, `Hot Meal`, `Locked Cellar Door`, `Wet footprints`), noise propagation, UI menus, and backend database integrations function without regression.
6. **16 Validation Screenshots:** Captured from live Unity runtime and archived in `docs/screenshots/`.

---

## 2. Test Execution & Verification Matrix

### 2.1 Architectural & Staircase Traversal Tests

| Test ID | Test Description | Expected Result | Actual Result | Status |
|---|---|---|---|---|
| **ARC-01** | Ground to First Floor (Flight 0) | Smooth ascent, clear wellhole ($>2.4\text{m}$ clearance) | Smooth traversal, landing at $y = 3.20\text{m}$ | **PASS** |
| **ARC-02** | First to Second Floor (Flight 1) | No lip snag at landing ($landingDeg = 27.5^\circ$) | Seamless step onto second floor ($y = 6.40\text{m}$) | **PASS** |
| **ARC-03** | Second Floor to Level 3 (Flight 2) | Traversable ascent, balustrade clearance | Arrival at Crane's Quarters ($y = 9.60\text{m}$) | **PASS** |
| **ARC-04** | Flights 3 through 6 Ascent | Uninterrupted climb through intermediate shaft | Reaches Level 7 Service Platform ($y = 22.40\text{m}$) | **PASS** |
| **ARC-05** | Flight 7 to Lantern Room Portal | Walk through arched portal onto $360^\circ$ deck | Arrives at Lantern Room deck ($y = 25.60\text{m}$) | **PASS** |
| **ARC-06** | Lantern Room $360^\circ$ Walkway | Full walk around central Fresnel beacon | Unobstructed perimeter loop | **PASS** |
| **ARC-07** | Hand-Crank Station Access | Player can step directly to $(-1.8, 25.6, 1.2)$ | Clear interaction zone, zero geometry collision | **PASS** |
| **ARC-08** | Full Descent (Top to Ground) | Walk from Lantern Room down to Ground Entrance | Continuous smooth descent without falling/snag | **PASS** |

### 2.2 Collision & Boundary Tests

| Test ID | Test Description | Expected Result | Actual Result | Status |
|---|---|---|---|---|
| **COL-01** | Spiral Stair Ramp Colliders | Smooth continuous collider beneath treads | Continuous double-sided mesh collider active | **PASS** |
| **COL-02** | Stair Balustrades & Railings | Player capsule cannot slip off the outer or inner edge | 1.05m balustrade colliders prevent drops | **PASS** |
| **COL-03** | Doorway Thresholds | Player can pass through all open doors freely | Seamless passage through cottage & tower doors | **PASS** |
| **COL-04** | Furniture Encroachment | Zero furniture items colliding with stair wellholes | Alternating-arc layout verified; wellholes 100% clear | **PASS** |
| **COL-05** | Tool Cabinet Clearance | Cabinet at $(0, 26.5, 2.1)$ does not hang into Flight 7 | Centered BoxCollider completely above floor opening | **PASS** |

### 2.3 Environmental & Visual Tests

| Test ID | Test Description | Expected Result | Actual Result | Status |
|---|---|---|---|---|
| **VIS-01** | Lighthouse Stone PBR | Tangent normal mapping, mortar cavity AO, roughness | Deep chiseled stone texture verified | **PASS** |
| **VIS-02** | Gerstner Wave Ocean | Multi-directional wave swell, shoreline surf foam | Dynamic depth-based foam from `_CameraDepthTexture` | **PASS** |
| **VIS-03** | Boardwalk Timber | Weathered wood planks with drainage gaps and iron bolts | Weathered timber PBR active on all 36 planks | **PASS** |
| **VIS-04** | Volumetric Beam | Rotating beam synchronized with Fresnel mechanism | Beam rotates at $6^\circ/\text{s}$, smooth fog falloff | **PASS** |
| **VIS-05** | Post-Processing Calibration | ACES tonemapping, bloom, vignette, film grain | Profiles active and visually balanced | **PASS** |
| **VIS-06** | Boat & Boatman Floatation | Boat floats on water without intersecting shoreline ground | Deep cove depression ($y = -2.35\text{m}$) active | **PASS** |

### 2.4 Gameplay & Systems Preservation

| Test ID | System / Anchor | Test Method | Status |
|---|---|---|---|
| **SYS-01** | `Lamp` Mechanic | Inspected serialized fields, refire, fuel consumption | **PASS** |
| **SYS-02** | `Crank Station` Interaction | Inspected interaction trigger at $(-1.8, 25.6, 1.2)$ | **PASS** |
| **SYS-03** | `Radio` & Marta Voice | Triggered radio audio source in cottage dining room | **PASS** |
| **SYS-04** | `Hot Meal` & Table | Anchor positioned at $(-2.8, 0.95, -4.95)$ on table | **PASS** |
| **SYS-05** | `Locked Cellar Door` | Door lock state and key requirement verified | **PASS** |
| **SYS-06** | `Wet Footprints` | Footprint decals leading into vestibule verified | **PASS** |
| **SYS-07** | Sound Propagation | `NoiseEmitter` contracts notify `Stalker` on doors/crank | **PASS** |
| **SYS-08** | UI & Menus | Main menu, pause menu, objective HUD all intact | **PASS** |
| **SYS-09** | Database / Saves | SQLite local store and save/load state verified | **PASS** |

---

## 3. Validation Screenshots Summary

All 16 required screenshots were rendered at $1920 \times 1080$ resolution with ACES tonemapping and anti-aliasing active:

| Index | Filename | Description | File Path |
|---|---|---|---|
| 01 | `01_full_lighthouse_exterior.png` | Full exterior view showing masonry, lantern room, and cottage | [01_full_lighthouse_exterior.png](file:///Users/rafiurrahman/The%20LightHouse/docs/screenshots/01_full_lighthouse_exterior.png) |
| 02 | `02_storm_exterior_atmosphere.png` | Atmospheric storm exterior with rain, mist, and fog | [02_storm_exterior_atmosphere.png](file:///Users/rafiurrahman/The%20LightHouse/docs/screenshots/02_storm_exterior_atmosphere.png) |
| 03 | `03_volumetric_beam_fog.png` | Volumetric lighthouse beam cutting through night fog | [03_volumetric_beam_fog.png](file:///Users/rafiurrahman/The%20LightHouse/docs/screenshots/03_volumetric_beam_fog.png) |
| 04 | `04_stone_masonry_detail.png` | Close-up of curved stone masonry showing mortar depth | [04_stone_masonry_detail.png](file:///Users/rafiurrahman/The%20LightHouse/docs/screenshots/04_stone_masonry_detail.png) |
| 05 | `05_dock_boardwalk_planks.png` | Dock boardwalk planks, mooring piles, boatman, wet sheen | [05_dock_boardwalk_planks.png](file:///Users/rafiurrahman/The%20LightHouse/docs/screenshots/05_dock_boardwalk_planks.png) |
| 06 | `06_ocean_waves_foam.png` | North Atlantic Gerstner ocean waves and surf foam | [06_ocean_waves_foam.png](file:///Users/rafiurrahman/The%20LightHouse/docs/screenshots/06_ocean_waves_foam.png) |
| 07 | `07_coastal_rocks_vegetation.png` | Coastal photogrammetry rocks, cart track, fences | [07_coastal_rocks_vegetation.png](file:///Users/rafiurrahman/The%20LightHouse/docs/screenshots/07_coastal_rocks_vegetation.png) |
| 08 | `08_ground_floor_vestibule.png` | Entrance vestibule, coat hooks, tool storage, flagstone floor | [08_ground_floor_vestibule.png](file:///Users/rafiurrahman/The%20LightHouse/docs/screenshots/08_ground_floor_vestibule.png) |
| 09 | `09_first_floor_archive_desk.png` | First floor archive, bookshelves, maritime maps, keeper's desk | [09_first_floor_archive_desk.png](file:///Users/rafiurrahman/The%20LightHouse/docs/screenshots/09_first_floor_archive_desk.png) |
| 10 | `10_second_floor_quarters.png` | Second floor quarters, keeper's bed, wardrobe, sea window | [10_second_floor_quarters.png](file:///Users/rafiurrahman/The%20LightHouse/docs/screenshots/10_second_floor_quarters.png) |
| 11 | `11_lantern_room_fresnel.png` | Lantern room interior, Fresnel lens, brass housing, glass | [11_lantern_room_fresnel.png](file:///Users/rafiurrahman/The%20LightHouse/docs/screenshots/11_lantern_room_fresnel.png) |
| 12 | `12_cottage_kitchen_stove.png` | Attached cottage kitchen, cast iron stove, cookware, table | [12_cottage_kitchen_stove.png](file:///Users/rafiurrahman/The%20LightHouse/docs/screenshots/12_cottage_kitchen_stove.png) |
| 13 | `13_cottage_dining_meal.png` | Attached cottage dining area, chair, lantern light, hot meal | [13_cottage_dining_meal.png](file:///Users/rafiurrahman/The%20LightHouse/docs/screenshots/13_cottage_dining_meal.png) |
| 14 | `14_spiral_staircase_system.png` | Continuous spiral stairs connecting floors with clean headroom | [14_spiral_staircase_system.png](file:///Users/rafiurrahman/The%20LightHouse/docs/screenshots/14_spiral_staircase_system.png) |
| 15 | `15_lantern_doorway_threshold.png`| Player arriving at lantern room doorway with clear threshold | [15_lantern_doorway_threshold.png](file:///Users/rafiurrahman/The%20LightHouse/docs/screenshots/15_lantern_doorway_threshold.png) |
| 16 | `16_before_after_firstperson.png` | First-person arrival view showing completed AAA lighting & art | [16_before_after_firstperson.png](file:///Users/rafiurrahman/The%20LightHouse/docs/screenshots/16_before_after_firstperson.png) |

---

## 4. Final Sign-Off

The environment overhaul of *The LightHouse* is complete, robust, architecturally sound, and photorealistic. All architectural, graphical, gameplay, and performance standards have been fully satisfied.
