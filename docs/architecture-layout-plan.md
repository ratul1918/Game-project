# The LightHouse — Architectural Layout Plan

## 1. Overview & Architectural Hierarchy

This document defines the authoritative 3D spatial layout, floor elevations, protected staircase shafts, and designated furniture placement zones for **The LightHouse** on Harrow Rock, strictly separating rooms, staircases, and corridors in accordance with the architectural reference cross-section.

### Core Hierarchy
```text
Lighthouse
├── Exterior
│   └── Tower Shell (Tapered Masonry, Cornices, Outer Doors)
├── AttachedCottage
│   ├── Architecture (Stone Walls, Timber Flooring, Ceiling Beams, Slate Roof)
│   ├── Kitchen (Cast Iron Stove, Chimney, Sink Counters, Meal Table)
│   ├── DiningArea (Table, Chairs, Window, Curtains, Household Props)
│   └── RadioOffice (Radio Station, Logbook Desk, Keeper Locker)
├── GroundFloor
│   ├── Architecture (Stone Foundation, Flagstone Floor, Ceiling Slab)
│   ├── EntranceRoom (Threshold Foyer, Coat Hooks, Oilskins, Sea Chest, Boot Stool)
│   ├── Storage (Maintenance Crates, Rope Coils, Oil Barrels, Equipment Bench)
│   └── Staircase_A (Flight 0, Stone Starter Curb, Newel Post, Ramp Collider)
├── FirstFloor
│   ├── Architecture (Solid Timber Floor Slab, Ceiling Slab)
│   ├── ArchiveRoom (Curved Bookcases, Writing Bureau, Globe, Astrolabe, Lamp)
│   └── Staircase_B (Flight 1, Balustrade, Gate Newel, Landing, Ramp Collider)
├── SecondFloor
│   ├── Architecture (Solid Timber Floor Slab, Ceiling Slab)
│   ├── LivingQuarters (Keeper Bed, Wardrobe, Station Clock, Charts, Spyglass Desk)
│   └── Staircase_C (Flight 2, Balustrade, Gate Newel, Landing, Ramp Collider)
├── UpperTower
│   ├── Architecture (Floors 3–7 Slabs, Ceilings, Masonry Openings)
│   ├── CraneQuarters_Level3 (Locked Door, Personal Belongings)
│   └── Staircases_D_to_G (Flights 3–7, Protected Shaft Colliders, Balustrades)
├── LanternFloor
│   ├── Architecture (Diamond Plate Iron Floor, 360° Glass, Pitched Roof Cap)
│   ├── LanternRoom (Grand Arched Portal, Flanking Brass Lanterns, Gallery Rails)
│   └── LampMechanism (Cast Iron Pedestal, Fresnel Lens Beacon, Lamp, Crank Station)
└── Shared
    ├── Doors (Exterior, Cottage-to-Tower, Locked Rooms)
    ├── Windows (Stone & Metal Casements)
    └── Lighting (Atmospheric Moon, Warm Practical Lanterns, Skybox)
```

---

## 2. Floor Elevations & Vertical Clearances

| Floor Level | Name | Elevation ($y$) | Floor Thickness | Ceiling Height | Clear Headroom | Staircase Flight |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **Level -1** | Cellar / Hidden Room | $-3.20\text{m}$ | $0.25\text{m}$ | $2.95\text{m}$ | $2.70\text{m}$ | Cellar Steps |
| **Level 0** | Ground Floor & Cottage | $0.15\text{m}$ | $0.14\text{m}$ | $3.05\text{m}$ | $2.91\text{m}$ | Staircase A (Flight 0) |
| **Level 1** | Archive Room & Workshop | $3.20\text{m}$ | $0.14\text{m}$ | $3.20\text{m}$ | $3.06\text{m}$ | Staircase B (Flight 1) |
| **Level 2** | Keeper's Living Quarters | $6.40\text{m}$ | $0.14\text{m}$ | $3.20\text{m}$ | $3.06\text{m}$ | Staircase C (Flight 2) |
| **Level 3** | Crane's Quarters / Storeroom | $9.60\text{m}$ | $0.14\text{m}$ | $3.20\text{m}$ | $3.06\text{m}$ | Flight 3 |
| **Levels 4–7** | Intermediate Machinery & Battery | $12.80\text{m} \to 22.40\text{m}$ | $0.14\text{m}$ | $3.20\text{m}$ | $3.06\text{m}$ | Flights 4–7 |
| **Level 8** | Lantern Room & Beacon Deck | $25.60\text{m}$ | $0.18\text{m}$ | $3.90\text{m}$ | $3.72\text{m}$ | Arched Portal Arrival |

---

## 3. Dedicated Staircase System Rules

1. **Independent Spatial Shaft**: All spiral stairs hug the outer masonry wall from $r = innerStairR$ to $outerStairR$ ($1.85\text{m}$ wide tread envelope).
2. **Strict Object Exclusion**: **NO furniture, cabinets, bookshelves, tables, beds, crates, barrels, or decorative items may be placed on stair treads, stair ramps, or stair landing platforms.**
3. **Physical & Visual Separation**: Every floor transition has:
   - A solid landing platform bridging the stair arrival into the room floor.
   - A protective wrought-iron balustrade ($1.05\text{m}$ height) guarding the inner edge of the stair opening.
   - A prominent brass newel post and finial marking the open landing gate.
4. **Stair Alternation Geometry**:
   - **Odd Levels (1, 3, 5, 7)**: Flight from below arrives at angle $-110^\circ$ (North-West arc). Headroom slot is isolated to $-115^\circ \to -30^\circ$. Living room floor occupies the remaining $275^\circ$ plus the entire $360^\circ$ central disc.
   - **Even Levels (2, 4, 6)**: Flight from below arrives at angle $+50^\circ$ (South-East arc). Headroom slot is isolated to $+45^\circ \to +130^\circ$. Living room floor occupies the remaining $275^\circ$ plus the entire $360^\circ$ central disc.
5. **Seamless Ramp Colliders**: Each flight has a continuous helical mesh ramp collider with slope $\le 19^\circ$, terminating $2\text{ steps}$ before the landing platform to guarantee completely open, unsnagged movement into the room.

---

## 4. Protected Room Placement Zones

| Room | Usable Living Zone | Protected Stair Zone (NO FURNITURE) | Key Architectural Elements |
| :--- | :--- | :--- | :--- |
| **Ground Floor Entrance & Equipment** | West & Center arc: $x \in [-2.8, 1.0], z \in [-3.0, 3.0]$ | East arc ($+100^\circ \to -110^\circ$): $x > 1.2, z > -0.5$ | Compass flagstone inlay, coat hooks, yellow oilskins, navy coats, boot bench, sea chest, storage crates. |
| **Attached Cottage** | Full structure: $x \in [-12.0, 2.0], z \in [-9.0, -1.0]$ | Tower passage at $z = -4.0\text{m}$ | Kitchen stove, range hood, sink counters, dining table, chairs, hot meal, radio desk, curtains. |
| **First Floor: Archive & Workshop** | North arc ($z > 0$): $x \in [-2.5, 2.5], z \in [0.5, 3.2]$ | South arc ($z < 0$, Flight 1 climbing): $z \in [-3.5, 0.0]$ | Floor-to-ceiling curved bookcase, leatherbound books, study writing bureau, globe, astrolabe, banker lamp, Persian rug. |
| **Second Floor: Keeper's Living Quarters** | South arc ($z < 0$): $x \in [-2.5, 2.5], z \in [-3.2, 0.0]$ | North arc ($z > 0$, Flight 2 climbing): $z \in [0.0, 3.5]$ | Keeper's bed, wool blanket, wardrobe locker, nautical station clock, navigation charts, drafting table, spyglass tripod, braided rug. |
| **Top Floor: Lantern Room** | Central circular deck ($r \le 2.0\text{m}$) & Gallery | Stair ascent slot at $+50^\circ \to +110^\circ$ | Arched iron entrance portal, flanking lanterns, Fresnel lens beacon, Lamp pedestal, Crank Station, glass dome. |

---

## 5. Architectural Quality Standards

- **Complete Structural Enclosure**: Every room has a real, solid floor and ceiling. No gaping open abysses or broken balcony edges.
- **Doorway Clearances**: All doorways maintain at least $1.2\text{m}$ width and $2.1\text{m}$ height with clear walking space on both sides.
- **Lighting Ambience**: Warm amber interior lantern light ($\approx 2100\text{K} - 2800\text{K}$) contrasting with the cold, tempestuous blue-grey maritime exterior.

## 2026-11-08 collision overhaul (supersedes legacy dimensions above)

The active master interior uses floor tops at 0.15, 3.35, 6.55 and 9.75 m.
The central floor discs end at radius 3.08 m; the annular stair well remains
open above every rising flight. Stair tread width is 1.35 m (r=3.25–4.60),
with a continuous persistent mesh ramp, no tread colliders, and at least
2.6 m vertical clearance. The outer lining inner face is r=5.0 m and the
room outer face is r=3.075 m, leaving 1.925 m of annular space.
Room doors provide 1.10 x 2.20 m clear apertures, hinged leaves and level
landings at least 1.5 x 1.5 m, continuous with the ramp and central floor.
All existing exterior door IDs, locks and noise events remain unchanged.
Cellar access has a 1.20 x 2.40 m clear masonry arch and a smooth straight
stair route descending 4.50 m from the cottage floor into the vaulted
corridor and hidden radio room. Existing story interactables move with it.
Table food uses UV-mapped lit ceramic, stew and bread meshes. FPV torch
barrel diameter is 0.04 m and total length 0.22 m; the hand-held anchor is
camera-local (0.24, -0.22, 0.45), with light originating at the lens and
converging on the crosshair. Geometry verification must inspect saved scenes,
not only freshly generated in-memory meshes.
