# LIGHTHOUSE ROOM EXPANSION & ARCHITECTURAL REDESIGN REPORT

**Document Version:** 1.0.0  
**Project:** The LightHouse (Unity 6 / Universal Render Pipeline)  
**Task:** Critical Lighthouse Redesign — Larger Rooms, Proper Entrance and Spacious Floor Plans  
**Status:** Architecture Audit & Redesign Specification  

---

## 1. Architectural Audit of Existing Dimensions

Prior to modifying the building geometry, an exhaustive inspection of the existing lighthouse scene (`Act1.unity`), layout settings (`LighthouseLayoutSettings.asset`), and procedural generators (`ReferenceVisualPass.cs`, `IslandArrivalPass.cs`) was conducted to determine why the interior rooms felt cramped.

### 1.1 Root Cause Analysis

1. **Undersized Exterior Tower Diameter:**
   - The previous exterior base diameter was only **$10.40\text{ metres}$** ($R_{outer} = 5.20\text{m}$), tapering to **$7.60\text{ metres}$** ($R_{outer} = 3.80\text{m}$) at the lantern room ($y = 25.6\text{m}$).
   - With an exterior wall thickness of $0.70\text{m}$ at base, the interior diameter was only $9.00\text{m}$ ($R_{inner} = 4.50\text{m}$), and at the upper floors it choked down to $6.50\text{m}$ ($R_{inner} = 3.25\text{m}$).
2. **Oversized Staircase Footprint:**
   - The staircase was authored with a width of **$1.85\text{ metres}$** (`stairWidth = 1.85f`).
   - In a room of $4.50\text{m}$ radius, an outer stair of $1.85\text{m}$ extends inward to $r = 2.65\text{m}$.
   - On intermediate and upper floors where $R_{inner} \approx 3.50\text{m}$, the stair extended inward to $r = 1.65\text{m}$, consuming over $52\%$ of the room radius and leaving only a narrow circular corridor.
3. **Cottage Obstructing the Tower Entrance:**
   - The attached keeper's cottage previously wrapped across the entire front of the tower ($x = -12.0\text{m} \to +2.0\text{m}$ at $z = -9.0\text{m}$).
   - This enclosed the front of the tower and forced the player to enter through the cottage side door (`Keeper House Front Door` at $x = -5.9, z = -9.05$), squeeze through the cottage hallway, and enter the tower through an interior door at $z = -4.03\text{m}$.
   - The tower itself lacked a dedicated, grand, clearly identifiable exterior main entrance door facing the arrival path.
4. **Constrained Furniture Placement:**
   - Because the stair occupied so much radius, all furniture had to be packed into a tiny inner core ($r \le 1.8\text{m}$), preventing believable open room layouts.

---

## 2. Before vs. After Dimensional Comparison

| Metric / Architectural Feature | Previous Geometry (Cramped) | New Redesign (Spacious Target) | Expansion Delta |
|---|---|---|---|
| **Tower Base Exterior Diameter** | $10.40\text{ m}$ ($R_{outer} = 5.20\text{m}$) | **$13.00\text{ m}$** ($R_{outer} = 6.50\text{m}$) | **$+2.60\text{ m}$ ($+25.0\%$)** |
| **Tower Top Exterior Diameter** | $7.60\text{ m}$ ($R_{outer} = 3.80\text{m}$) | **$9.60\text{ m}$** ($R_{outer} = 4.80\text{m}$) | **$+2.00\text{ m}$ ($+26.3\%$)** |
| **Granite Base Wall Thickness** | $0.70\text{ m}$ | **$0.85\text{ m}$** | $+0.15\text{ m}$ (realistic masonry) |
| **Granite Top Wall Thickness** | $0.55\text{ m}$ | **$0.55\text{ m}$** | Identical |
| **Ground Floor Interior Diameter** | $9.00\text{ m}$ ($R_{inner} = 4.50\text{m}$) | **$11.30\text{ m}$** ($R_{inner} = 5.65\text{m}$) | **$+2.30\text{ m}$ ($+25.6\%$)** |
| **Ground Floor Usable Area** | $63.6\text{ m}^2$ (total) / $22.0\text{ m}^2$ (core) | **$100.3\text{ m}^2$ (total) / $63.6\text{ m}^2$ (core)** | **$+189\%$ Usable Core Area!** |
| **First Floor Interior Diameter** | $8.40\text{ m}$ ($R_{inner} = 4.20\text{m}$) | **$10.96\text{ m}$** ($R_{inner} = 5.48\text{m}$) | **$+2.56\text{ m}$ ($+30.5\%$)** |
| **First Floor Archive Area** | $55.4\text{ m}^2$ (total) / $17.3\text{ m}^2$ (core) | **$94.3\text{ m}^2$ (total) / $58.9\text{ m}^2$ (core)** | **$+240\%$ Usable Core Area!** |
| **Second Floor Interior Diameter** | $7.80\text{ m}$ ($R_{inner} = 3.90\text{m}$) | **$10.60\text{ m}$** ($R_{inner} = 5.30\text{m}$) | **$+2.80\text{ m}$ ($+35.9\%$)** |
| **Second Floor Living Area** | $47.8\text{ m}^2$ (total) / $13.2\text{ m}^2$ (core) | **$88.2\text{ m}^2$ (total) / $54.1\text{ m}^2$ (core)** | **$+310\%$ Usable Core Area!** |
| **Lantern Room Interior Diameter** | $6.50\text{ m}$ ($R_{inner} = 3.25\text{m}$) | **$8.50\text{ m}$** ($R_{inner} = 4.25\text{m}$) | **$+2.00\text{ m}$ ($+30.8\%$)** |
| **Lantern Walkway Clear Width** | $0.90\text{--}1.20\text{ m}$ | **$2.20\text{--}2.40\text{ m}$** | **$+100\%$ Wider Walkway!** |
| **Staircase Tread Width** | $1.85\text{ m}$ (excessive) | **$1.15\text{ m}$** (compact architectural standard)| **$-37.8\%$ Footprint!** |
| **Central Open Room Diameter** | $5.30\text{ m}$ (Ground) / $3.42\text{ m}$ (L2) | **$9.00\text{ m}$ (Ground) / $8.30\text{ m}$ (L2)** | **$+69.8\%$ to $+142.7\%$ Wider!** |
| **Main Entrance Doorway** | Hidden / Indirect via cottage | **Grand Exterior South Portal** | **Fully Visible & Interactive** |
| **Main Entrance Dimensions** | None on tower exterior | Width: **$1.25\text{ m}$**, Height: **$2.25\text{ m}$**| Full architectural door |
| **Cottage Relationship** | Wrapped across tower front | Abuts West flank only ($x \le -5.2\text{m}$) | Courtyard & Entrance Open |

---

## 3. Floor-by-Floor Architecture Plan

```
+=====================================================================================+
|  LEVEL 8 (y = 25.60m) - LANTERN ROOM & PANORAMA GALLERY                             |
|  Diameter: 8.50m | Walkway Width: 2.30m | 16 Faceted Glazed Mullions                 |
|  Central Fresnel Lens | Rotating Drive | Hand-Crank Station | Catwalk Storm Hatch   |
+-------------------------------------------------------------------------------------+
|  LEVEL 3 to 7 (y = 9.60m to 22.40m) - INTERMEDIATE TOWER SHAFT                      |
|  Level 3: Crane's Locked Quarters (Iron Bed, Child Photo Sea Chest, Story Anchor)  |
|  Level 4-7: Oil Locker, Maintenance Landing, Winch Drum, Upper Collar Service       |
+-------------------------------------------------------------------------------------+
|  LEVEL 2 (y = 6.40m) - KEEPER'S EXPANSIVE PRIVATE LIVING QUARTERS                   |
|  Diameter: 10.60m | Area: 88.2m² | Open Central Living Room: >8.30m across          |
|  4-Poster Oak Bed | Nightstand | Double Wardrobe | Franklin Stove & Rocking Chair   |
|  Writing Secretary Desk | Deep South Sea Window & Velvet Drapes | Station Clock     |
+-------------------------------------------------------------------------------------+
|  LEVEL 1 (y = 3.20m) - KEEPER'S ARCHIVE LIBRARY & WORKSHOP                          |
|  Diameter: 10.96m | Area: 94.3m² | Open Investigation Floor: >8.66m across          |
|  4.5m Grand Bookcase | Admiralty Sea Charts | Bureau Investigation Desk & Armchair  |
|  Globe | Armillary Sphere | Banker's Lamp | Clockwork Vise Bench | Persian Carpet   |
+-------------------------------------------------------------------------------------+
|  GROUND FLOOR (y = 0.15m) - GRAND ENTRANCE HALL & MARITIME SUPPLY                   |
|  Diameter: 11.30m | Area: 100.3m² | Open Foyer & Storage Concourse: >9.00m across   |
|  South Grand Arched Entrance Portal | 3.0m Brass Compass Rose Flagstone Inlay       |
|  Coat Peg Rack (Yellow Slicker & Wool Greatcoat) | Luggage Shelf | Boot Stool       |
|  3-Tier Maritime Parts Shelving | 4-Cask Paraffin Barrel Rack | Carpenter's Bench   |
|  West Arched Portal to Cottage | Flight 0 Foot at East Wall (1.15m Compact Spiral)  |
+=====================================================================================+
|  ATTACHED KEEPER'S COTTAGE (West Flank, x in [-12.5, -5.2], z in [-9.0, -1.0])      |
|  Footprint: 7.3m x 8.0m (58.4m²) | Separate Gabled Slate Roof | Front Porch Door    |
|  Spacious Kitchen (Stove, Sink, Cabinetry) | Separate Dining Room (Table, Hot Meal)  |
+=====================================================================================+
```
