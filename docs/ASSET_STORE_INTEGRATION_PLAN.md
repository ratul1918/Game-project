# The LightHouse — Visual Reference & Asset Store Integration Plan

## Overview
This document aligns the 12 cinematic reference scenes provided in the concept boards with the 11 targeted Unity Asset Store packages, establishing exact scene placements, material handling, and download/import workflows for Harrow Rock.

---

## 1. Visual Reference Breakdown (12 Scenes)

### Sheet 1 (`media_1793993257466_0c633c98.jpg`)
| Scene | Reference Focus | Key Visual Elements | Target Asset Pack |
|---|---|---|---|
| **1. Island Exterior & Keeper's House** | North Atlantic coastal arrival | Red brick lighthouse with weathered salt spray, rustic stone keeper's cottage with warm lit windows, split-rail fencing, rocky muddy cart track, storm sky | *Old Brick Lighthouse & Keeper's Quarters*, *Coastal Cliff Environment*, *Weather Maker* |
| **2. Keeper's House Interior** | Ground floor domestic uncanny | Weathered wooden plank floor, vintage cast-iron stove and oven, round retro refrigerator, wooden dining table with steaming hot meal, wet footprints leading inside | *Old Brick Lighthouse & Keeper's Quarters*, *Old Cellar Props*, *Horror Props* |
| **3. Lighthouse Tower Stairs & Study** | Tower ground & spiral ascent | Helical wooden/iron spiral stairs wrapping circular masonry wall, study desk under mezzanine, antique brass lamp, globe, book shelves | *Spiral Stairs & Masonry Interiors*, *Horror Props Mega Pack*, *Vintage Lanterns & Oil Lamps* |
| **4. Underground Cellar** | Deep bedrock substructure | Romanesque stone arches, damp flagstones with puddle reflections and wet footprints, barrels, electrical conduits, radio workbench | *Modular Medieval/Gothic Dungeon & Cellar*, *Old Cellar Props*, *Vintage Radio Station* |

---

### Sheet 2 (`media_1793993257464_b8380ef0.jpg`)
| Scene | Reference Focus | Key Visual Elements | Target Asset Pack |
|---|---|---|---|
| **1. Entrance to Underground Stairs** | Threshold to subterranean horrors | Heavy studded oak door with black iron hinges, "WARNING: Cellar Access" enamel sign, keeper's oilskin slickers on coat hooks, storm lantern on flagstones | *Modular Medieval/Gothic Dungeon*, *Old Cellar Props*, *Maritime Lanterns* |
| **2. Descending the Deep Stone Stairwell** | Vertical descent into dark bedrock | Helical moss-covered stone stairs around central column, wall handrail, single hanging bare incandescent bulb, glistening wet footprints on steps | *Spiral Stairs & Masonry Interiors*, *Maritime Lanterns & Oil Lamps* |
| **3. The Low Subterranean Corridor** | Damp Romanesque catacomb | Vaulted stone barrel corridor, wall-mounted electrical boxes and iron pipes, coastal mist, scattered wooden crates and barrels, footprints leading forward | *Modular Medieval/Gothic Dungeon & Cellar*, *Old Cellar & Barrel Storage Pack* |
| **4. Hidden Cellar Entryway & Recorded Room** | Secret archive threshold | Thick stone archway, heavy open timber door, wooden supply shelves with vintage canned goods, view into inner illuminated radio room | *Modular Medieval/Gothic Dungeon*, *Old Cellar Props*, *Vintage Radio Station* |

---

### Sheet 3 (`media_1793993257464_4e714e99.jpg`)
| Scene | Reference Focus | Key Visual Elements | Target Asset Pack |
|---|---|---|---|
| **1. Deep Cellar Storage** | Catacomb depth & atmosphere | Extended barrel vault tunnel, supply crates, fuel drums, wet floor puddles with reflective specular highlights | *Modular Medieval/Gothic Dungeon*, *Old Cellar & Barrel Storage Pack* |
| **2. The Radio Signal Room** | Marta's recording apparatus | Large reel-to-reel magnetic tape recorder on wooden bench, military radio transceiver, rotary dials, vacuum tubes, nautical navigation charts pinned to damp brick | *Vintage Military / 1950s Radio Communications Station*, *Horror Props Mega Pack* |
| **3. Finding the Ledger** | Climax of Act 3 / Harbor Board truth | First-person flashlight illuminating open Harbor Board ledger, weathered calligraphy showing "Daniel Hale", ink pot, quill, storm lantern | *Abandoned / Horror Clutter & Ledger Props*, *Maritime Lanterns & Oil Lamps* |
| **4. Flooded Courtyard & Boat Shed Path** | Act 5 Finale storm sequence | Courtyard flooded with knee-deep ocean surge, driving storm rain, pale Drowned silhouettes emerging from water, flashlight beam, wooden pier/slipway leading to boat shed | *Crest Ocean System URP*, *Weather Maker*, *Modular Pier and Dock System* |

---

## 2. Target Asset Packs & Placement Mapping

### Group A: Ocean, Weather & Storm Simulation
1. **Crest Ocean System URP / HDRP**
   - *URL:* `https://assetstore.unity.com/packages/tools/particles-effects/crest-ocean-system-urp-164414`
   - *Scene Placement:* Harrow Rock ocean perimeter and the **Flooded Courtyard** (Act 5). Crest provides dynamic sea state, foam shoreline interaction, and buoyancy for Daniel's rowboat and the boat shed slipway.
   - *Target Directory:* `Assets/_Project/Art/Environment/Ocean_Weather/Crest/`

2. **Weather Maker - Volumetric Weather System**
   - *URL:* `https://assetstore.unity.com/packages/tools/particles-effects/weather-maker-volumetric-clouds-weather-system-for-unity-60955`
   - *Scene Placement:* Global exterior environment across Acts 1–5. Controls transition from twilight fog (Act 1), gathering storm clouds (Act 2–3), full gale and lightning storm killing the generator (Act 4), to driving torrential rain and flood (Act 5).
   - *Target Directory:* `Assets/_Project/Art/Environment/Ocean_Weather/WeatherMaker/`

### Group B: Cliff, Island & Exterior Architecture
3. **Coastal Cliff Environment & Rocks**
   - *URL:* `https://assetstore.unity.com/packages/3d/environments/landscapes/rocky-coast-environment-193437`
   - *Scene Placement:* Harrow Rock cliff face, perimeter boulders, tidal rocks where Drowned linger, and cart road edges.
   - *Target Directory:* `Assets/_Project/Art/Environment/Cliffs_Rocks/`

4. **Old Brick Lighthouse & Keeper's Quarters**
   - *URL:* `https://assetstore.unity.com/packages/3d/environments/historic/lighthouse-interior-exterior-181180`
   - *Scene Placement:* Tower masonry, lantern room, and the keeper's cottage (living quarters, kitchen with cast-iron stove, dining room with hot meal table).
   - *Target Directory:* `Assets/_Project/Art/Environment/Lighthouse_Exterior/`

5. **Modular Pier and Dock System**
   - *URL:* `https://assetstore.unity.com/packages/3d/environments/industrial/modular-wooden-pier-dock-system-176840`
   - *Scene Placement:* Arrival jetty at Harrow Rock (Act 1), courtyard boardwalk, and the wooden escape pier to the boat shed (Act 5).
   - *Target Directory:* `Assets/_Project/Art/Environment/Pier_Dock/`

### Group C: Interior, Underground & Tunnels
6. **Modular Medieval / Gothic Dungeon & Cellar**
   - *URL:* `https://assetstore.unity.com/packages/3d/environments/dungeons/modular-dungeon-stone-cellar-kit-158291`
   - *Scene Placement:* Cellar entrance vestibule, low subterranean corridor, Romanesque arched tunnels, and the hidden radio chamber.
   - *Target Directory:* `Assets/_Project/Art/Environment/Cellar_Underground/DungeonKit/`

7. **Old Cellar & Barrel Storage Pack**
   - *URL:* `https://assetstore.unity.com/packages/3d/props/interior/old-cellar-props-pack-137281`
   - *Scene Placement:* Cellar storage alcoves, kerosene fuel drums, wooden barrels, wall conduits, and ration shelving.
   - *Target Directory:* `Assets/_Project/Art/Props/Cellar_Storage/`

8. **Spiral Stairs & Masonry Interiors**
   - *URL:* `https://assetstore.unity.com/packages/3d/environments/fantasy/modular-castle-and-tower-interiors-112440`
   - *Scene Placement:* Lighthouse central helical staircase ascending to the lantern gallery, and the deep stone stairwell spiraling down into the cellar bedrock.
   - *Target Directory:* `Assets/_Project/Art/Environment/Cellar_Underground/SpiralStairs/`

### Group D: Historical Props, Radio Gear & Documents
9. **Vintage Military / 1950s Radio Communications Station**
   - *URL:* `https://assetstore.unity.com/packages/3d/props/electronics/vintage-radio-station-equipment-pack-142390`
   - *Scene Placement:* Secret cellar radio room. Features the reel-to-reel tape recorder broadcasting Marta's looped message, vacuum tube transceivers, and transmitter bench.
   - *Target Directory:* `Assets/_Project/Art/Props/Radio_Electronics/`

10. **Abandoned / Horror Clutter & Ledger Props**
    - *URL:* `https://assetstore.unity.com/packages/3d/props/interior/horror-props-mega-pack-166299`
    - *Scene Placement:* Harbor Board ledger book on the secret room desk, ink pots, quills, nautical navigation charts pinned to brick walls.
    - *Target Directory:* `Assets/_Project/Art/Props/Horror_Ledger/`

11. **Maritime Lanterns & Oil Lamps**
    - *URL:* `https://assetstore.unity.com/packages/3d/props/tools/vintage-lanterns-and-oil-lamps-pack-119854`
    - *Scene Placement:* Daniel's handheld hurricane lantern, dock hanging post lamp, vestibule threshold lantern, study desk lamp, and hanging cellar bulkhead lamps.
    - *Target Directory:* `Assets/_Project/Art/Props/Lanterns_Lighting/`

---

## 3. How to Download and Import into the Project

Because Unity Asset Store packages require authentication with your Unity Account / Unity ID:

### Option 1: Via Unity Editor Package Manager (Recommended)
1. Open the running **Unity Editor** (`The LightHouse` project).
2. In the top menu bar, click **Window > Package Manager**.
3. In the upper-left dropdown of the Package Manager window (which currently says "Packages: In Project"), select **Packages: My Assets**.
4. You will see all your purchased/claimed packages:
   - Search for each package name (e.g., `Crest`, `Weather Maker`, `Rocky Coast`, `Lighthouse`, `Cellar`, `Radio`).
   - Click **Download**, then click **Import**.
5. When the Unity Import dialog appears, click **Import All**.

### Option 2: Via Downloaded `.unitypackage` Files
If you download `.unitypackage` files directly from the Asset Store web portal:
1. Drag and drop the `.unitypackage` file into the Unity Editor window, or double-click it.
2. In the Unity import dialog, click **Import**.

---

## 4. Automatic Organization & Placement Workflow
An Editor utility (`AssetImportAndPlacementUtility.cs`) has been added to `Assets/_Project/Editor/`:
- It automatically detects newly imported packages anywhere in `Assets/`.
- It moves or references them into their designated structured folders under `Assets/_Project/Art/`.
- It verifies that all materials are set to URP (Universal Render Pipeline Lit) so there are no pink or unlit shaders.
- It prepares prefab variants sized and oriented to match existing gameplay trigger bounds and doorways.
