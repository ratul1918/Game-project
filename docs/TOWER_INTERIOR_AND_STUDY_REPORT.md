# The LightHouse — Vertical Tower Interior & Mid-Level Study Construction Report

## Executive Summary
This report documents the architectural, prop, lighting, and material construction of the vertical interior of the lighthouse tower and the mid-level study mezzanine in **The LightHouse**, built to precisely match **Image 10, Panel 3** (and blockout screenshots 3, 4, 6) in the cinematic maritime psychological horror aesthetic.

The implementation replaces outdated low-fidelity greybox primitives with high-resolution PBR geometry, authentic nautical investigation props, height-based volumetric lighting, and particle dust motes, seamlessly integrated into the master environment across all Acts 1–5.

---

## 1. Architectural Structure & Tower Chamber

| Component | Technical Specification | Material / Shader |
| :--- | :--- | :--- |
| **Curved Masonry Wall Enclosure** | 32 perimeter segmented arc pillars hugging the tower radius ($R = 5.35\text{m}$, height $y = 3.2\text{m}$ to $7.2\text{m}$) | `PBR_LighthouseStone.mat` with normal depth, moss dampness, and rough mortar |
| **Mezzanine Deck Corbels** | Heavy radial oak wall corbels ($0.24\text{m} \times 0.36\text{m} \times 0.55\text{m}$) anchored into stone wall at $y = 3.12\text{m}$ | `matWood` (Weathered Oak Planks) |
| **Upper Lantern Access Aperture** | Heavy cast-iron rim ring ($R = 2.6\text{m}$) with 8 radial structural girders framing vertical opening to the upper watchroom / lantern room | `matIron` (Oxidized Cast Iron) |
| **Mezzanine Hardwood Floor** | Heavy weathered timber plank platform ($8.5\text{m} \times 0.12\text{m} \times 6.4\text{m}$) at level $y = 3.20\text{m}$ | `PBR_WeatheredPlanks.mat` |
| **Wrought-Iron Curved Balustrade** | 24 vertical baluster rods with ornamental main posts and sweeping top handrail ($y = 4.26\text{m}$, $1.05\text{m}$ above deck) guarding the stairwell void | `matIron` (Rusted Iron with cracked black paint) |

---

## 2. Mid-Level Study Furnishings & Investigation Props

### A. Solid Mahogany Writing Desk & Armchair
- **Writing Desk**: Built at $(0\text{m}, 3.21\text{m}, 1.60\text{m})$, measuring $2.0\text{m}$ wide $\times 1.05\text{m}$ deep $\times 0.78\text{m}$ tall, finished in polished deep reddish-brown mahogany (`matMahogany`, smoothness 0.72). Features dual pedestals with 6 tiers of drawers and solid polished brass ring pulls (`matBrass`, metallic 0.94, smoothness 0.88).
- **Maritime Armchair**: Carved solid mahogany armchair with turned legs, curved armrests, and 5 backrest support slats facing the desk, angled slightly toward the keeper's writing position.

### B. Navigation Charts & Brass Drafting Instruments
- **Admiralty Chart No. 1308**: High-resolution navigation chart spread open flat across the desk center, depicting the treacherous reefs, shoals, depth soundings, and coordinates of Harrow Rock.
- **Rolled Chart Tubes & Canisters**: Two aged parchment chart rolls tied with cord, plus a heavy brass navigational chart cylinder.
- **Navigational Calipers / Drafting Dividers**: Polished brass caliper dividers resting on top of the chart, with tapered needle legs measuring distances across the shoal.
- **Brass Parallel Rolling Ruler**: Solid brass weighted parallel drafting ruler laid alongside the chart.

### C. Open Keeper's Logbook & Writing Implements
- **Harbor Board Logbook**: Heavy cloth/leather-bound keeper's ledger laid open, displaying handwritten log entries and vessel registrations.
- **Glazed Stoneware Inkwell**: Turned dark glazed ceramic inkpot (`MaritimeInkwellGlaze.mat`, smoothness 0.92) with dipped ink reservoir.
- **Goose Feather Quill**: Ivory quill resting inside the inkwell at a natural writing angle.

### D. Vintage Brass Oil Lamp & Desktop Armillary Globe
- **Unlit Brass Oil Lamp**: Fluted brass weighted pedestal base, font reservoir, brass burner dial, cotton wick, and tall cylindrical clear glass chimney (`SpecimenJarGlass.mat`, transparent, smoothness 0.94).
- **Terrestrial Desktop Globe**: Turned mahogany base with brass axis spindle, circular brass meridian ring, and aged parchment terrestrial sphere tilted at $23.5^\circ$.
- **Admiralty Folios & Almanacs**: Stack of 3 leather-bound tide ledgers on the desk corner with gold-embossed spines.

---

## 3. Curved North Wall Bookcases & Specimen Jars

- **Curved Oak Bookcase**: Multi-bay architectural library unit ($3.95\text{m}$ wide $\times 2.70\text{m}$ tall $\times 0.48\text{m}$ deep) hugging the curved North masonry wall ($z = 4.35\text{m}$) with heavy crown moulding and 4 deep shelves.
- **Maritime Library**: Over 45 dense leatherbound volumes, nautical almanacs, and tide charts distributed across Shelves 2 and 3 in alternating crimson, emerald, navy, and gold-embossed spine bindings (`matBookRed`, `matBookGreen`, `matBookBlue`, `matBookGold`).
- **Preserved Specimen Jars**: 6 translucent cylindrical glass jars on Shelf 4 containing suspended marine specimens (cephalopod and kelp deep-sea organisms) sealed with turned wooden cork stoppers.

---

## 4. Lighting, Atmospheric Fog & Shadows

| Light Fixture | Settings | Visual Function |
| :--- | :--- | :--- |
| **Industrial Hanging Lamp** | Suspended FBX fixture (`IndustrialHangingLamp.fbx`, `M_IndustrialLamp.mat`) at $y = 4.86\text{m}$ | Low-hanging metal shade with glowing warm glass dome |
| **Desk Amber Spotlight** | Color: `new Color(1.0f, 0.74f, 0.40f)` (2300K), Intensity: 3.8, Range: 6.8m, Spot: $68^\circ$, Soft Shadows | Dramatic horror light cone illuminating desk, charts, calipers, and Persian rug |
| **Study Ambient Fill** | Color: `new Color(1.0f, 0.72f, 0.38f)`, Intensity: 1.25, Range: 4.8m, Shadows: None | Soft bounce fill preventing crushed black shadows in study corners |
| **Dust Motes Particle Emitter** | `DustMotesEmitter.cs`: 65 max particles, speed $0.015-0.045\text{m/s}$, amber tint | Subtle floating ambient dust motes catching the pendant light beam |
| **Caged Stair Bulkhead Sconces** | 3 sconces along spiral stair landings ($y = 2.1\text{m}, 4.4\text{m}, 6.7\text{m}$), 1.6 intensity, Soft Shadows | Casts sharp raking vertical stair and baluster shadows across the curved masonry wall |

---

## 5. Greybox Deactivation & Stealth Gameplay Preservation

- Deactivated outdated primitive renderers in `Reference Visuals/Lighthouse/FirstFloor/ArchiveRoom` and `Reference Visuals/Lighthouse/FirstFloor/Architecture` to eliminate mesh overlap and z-fighting.
- Located `level2-desk` hiding spot: disabled its blockout `MeshRenderer` while keeping its `BoxCollider` and `HidingSpot` script intact and aligned to the new mahogany desk so player stealth hiding mechanics remain 100% functional.

---

## 6. Validation Verification

| Screenshot | Description | Status |
| :--- | :--- | :--- |
| **`31_vertical_tower_spiral_stairs.png`** | Vertical chamber view showing winding spiral staircase, stone wall, bulkhead sconces, and ceiling opening | Verified |
| **`32_midlevel_study_desk_overview.png`** | Wide 3/4 perspective matching Image 10 Panel 3: mahogany desk, glowing industrial pendant lamp, charts, bookcase, and rug | Verified |
| **`33_study_calipers_and_charts_closeup.png`** | Macro shot of desk surface: Admiralty chart, brass calipers, parallel rule, logbook, and oil lamp | Verified |
| **`34_tower_study_stairs_framing.png`** | Perspective from spiral stair landing looking across the mezzanine study platform | Verified |
