# The LightHouse — Architectural Audit & Spatial Analysis

**Date:** 2026-11-06  
**Document Contract:** Authoritative Architectural and Navigation Audit  
**Target Scenes:** `Assets/_Project/Scenes/Act1.unity` through `Act5.unity`  

---

## 1. Executive Summary

This audit assesses the physical geometry, player controller settings, staircase flight connections, and room boundary architecture across the lighthouse environment.

The audit identifies the exact physical causes of the inaccessible lantern room (top floor), structural intersections, broken stair flight arrivals, and furniture cluttering protected navigation routes. A concrete, step-by-step architectural repair plan is established to align the environment with the original cutaway reference.

---

## 2. Active Scene & Player Controller Specifications

### Active Scenes
- Primary Development Scene: `Assets/_Project/Scenes/Act1.unity`
- Target Gameplay Scenes: `Act1.unity`, `Act2.unity`, `Act3.unity`, `Act4.unity`, `Act5.unity`
- Master Scene Settings: `LighthouseLayoutSettings.asset` (`floorHeight = 3.2m`, `towerLevelCount = 8`, `outerRadius = 4.5m` tapering to `3.25m`).

### Player Controller Configuration (`Player` GameObject)
- **Component:** `UnityEngine.CharacterController` + `FirstPersonController`
- **Height:** $1.80\text{ m}$
- **Radius:** $0.35\text{ m}$
- **Center:** $(0.00, 0.90, 0.00)\text{ m}$
- **Step Offset:** $0.30\text{ m}$
- **Slope Limit:** $45^\circ$
- **Skin Width:** $0.08\text{ m}$
- **Min Move Distance:** $0.001\text{ m}$
- **Starting Position (Act 1):** $(2.50, 0.20, 0.00)\text{ m}$ (Ground floor entrance vestibule)

---

## 3. Floor Elevations & Intended Room Architecture

| Floor Level | Function / Room Name | Elevation ($y$) | Floor Thickness | Ceiling Height | Usable Room Zone |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **Level 0** | **Ground Floor**: Entrance Foyer & Storage | $0.15\text{ m}$ | $0.14\text{ m}$ | $3.05\text{ m}$ | West & Central floor arc ($r \le 2.8\text{m}$) |
| **Attached** | **Cottage**: Kitchen, Dining & Radio Office | $0.15\text{ m}$ | $0.14\text{ m}$ | $2.85\text{ m}$ | Structure west of tower ($x \in [-12, -0.7]$) |
| **Level 1** | **First Floor**: Archive & Workshop | $3.20\text{ m}$ | $0.14\text{ m}$ | $3.20\text{ m}$ | North arc ($z > 0$, outer walls) |
| **Level 2** | **Second Floor**: Keeper's Quarters | $6.40\text{ m}$ | $0.14\text{ m}$ | $3.20\text{ m}$ | South arc ($z < 0$, outer walls) |
| **Tower Shaft**| **Spiral Stair Shaft**: Continuous ascent | $6.40\text{ m} \to 25.60\text{ m}$ | Open central shaft | Continuous | Outer helical envelope ($r \in [1.40, 3.25]\text{m}$) |
| **Top Floor**| **Lantern Room**: Beacon, Walkway & Crank | $25.60\text{ m}$ | $0.18\text{ m}$ | $3.90\text{ m}$ | Full $360^\circ$ deck & walkway ($r \le 4.52\text{m}$) |

---

## 4. Staircase Connection Diagram & Geometry

```text
[ Ground Floor: y = 0.15m ]
        │
        ▼ (Flight 0: Spiral arc 100° -> -110°, radius 2.60m to 4.45m)
[ First Floor Landing: y = 3.20m ] ──► [ Archive & Maintenance Workshop ]
        │
        ▼ (Flight 1: Spiral arc -110° -> -310° (+50°), radius 2.45m to 4.30m)
[ Second Floor Landing: y = 6.40m ] ──► [ Keeper's Private Living Quarters ]
        │
        ▼ (Tower Spiral Shaft: Continuous ascending spiral to lantern deck)
        │    - Flight 2: 50° -> -110° (y = 6.40m -> 9.60m)
        │    - Flight 3: -110° -> +50° (y = 9.60m -> 12.80m)
        │    - Flight 4: 50° -> -110° (y = 12.80m -> 16.00m)
        │    - Flight 5: -110° -> +50° (y = 16.00m -> 19.20m)
        │    - Flight 6: 50° -> -110° (y = 19.20m -> 22.40m)
        │    - Flight 7: -110° -> +50° (y = 22.40m -> 25.60m)
        ▼
[ Lantern Room Arrival Portal: y = 25.60m ] ──► [ 360° Beacon Maintenance Walkway & Crank Station ]
```

---

## 5. Identified Physical Geometry & Collision Blockers

Our automated capsule overlap and continuous physical trajectory tests revealed the exact blockers preventing player navigation:

### A. The Inaccessible Top Floor (Lantern Room, $y = 25.60\text{m}$)
1. **`Crank Station` Collider Overhang:**  
   The gameplay anchor `Crank Station` is at $(2.20, 25.95, 0.00)\text{m}$ with a BoxCollider of size $(1.50, 1.50, 1.50)\text{m}$ (vertical bounds $y \in [25.20, 26.70]$). Flight 7 ascends directly below this position from $y = 24.32\text{m}$ to $25.28\text{m}$, creating a direct overhead collision where the player capsule's head strikes the bottom of the crank collider.
2. **`Lamp Hand-Crank Stand` Decorative Obstruction:**  
   `LanternRoom` procedurally spawned a duplicate `Lamp Hand-Crank Stand` at $(1.80, 26.10, -1.20)\text{m}$ with box collider bounds $y \in [25.60, 26.60]$, obstructing the stair arrival gate.
3. **`Lantern Stair Landing` Face Lip Collision:**  
   The landing platform at $y = 25.60\text{m}$ has a vertical $0.18\text{m}$ face collider (`Lantern Stair Landing`) spanning into the tread envelope where the ramp collider terminates at $y = 25.44\text{m}$, catching the player's capsule.
4. **Stair Guard Post Balustrade Overlap:**  
   The inner guard posts and circular gallery railing wedges extend past the entry portal angle, narrowing the walkable aperture onto the lantern deck to under $0.5\text{m}$.
5. **Horizontal Ceiling Beams & Bracket Intersections:**  
   Under-deck `Gallery Bracket` rods and lower support plates drop below $y = 25.0\text{m}$, cutting into the $2.0\text{m}$ vertical headroom needed above Flight 7 treads.

### B. Intermediate Stair Flight & Landing Obstructions
1. **Landing Platform Step Lips (Flights 0, 1, 2, 3, 4, 5, 6):**  
   On every level, `Stair Landing Platform` has a vertical collider edge at the top of the ramp collider, creating a lip that catches the CharacterController skin ($0.08\text{m}$).
2. **`level2-desk` Hiding Spot:**  
   A hiding spot collider `level2-desk` at $(2.20, 6.50, -1.00)\text{m}$ penetrates Flight 1's walking corridor between $y = 5.28\text{m}$ and $5.92\text{m}$.
3. **`Keeper Wardrobe Body`:**  
   The wardrobe at $(-2.55, 7.65, 0.70)\text{m}$ has box collider bounds extending to $x = -2.23\text{m}$, cutting into Flight 2's starting arc.
4. **`crane-bed` Hiding Spot Collider:**  
   A legacy hiding spot box collider `crane-bed` at $(-2.20, 9.70, -1.00)\text{m}$ occupies the stairwell arrival at $y = 9.60\text{m}$.
5. **Crates & Workbench Obstructions:**  
   `Supply Crate Large` at $(1.20, 13.25, 1.80)\text{m}$ and `Service Workbench Top` at $(0.00, 16.85, -2.40)\text{m}$ intersect Flights 4 and 5.
6. **Central Tower Support Visual Leftover:**  
   `Lighthouse Layout/Stairs/Tower Central Support` and `Tower Floor` segments in the legacy hierarchy contained overlapping components that previously rendered or created collision conflicts with the reference architecture.

---

## 6. Proposed Architectural Repair Plan

### Phase 2: Fix Structure and Staircases
1. **Clean Headroom & Ramp Colliders:**
   - Elevate the arrival ramp collar and bevel all `Stair Landing Platform` colliders so ramp tops blend flush with floor surfaces without any vertical collision step.
   - Adjust ramp colliders to slope smoothly ($\le 19^\circ$) with inner guard rail stops providing a full $1.2\text{m}$ wide exit gate at each floor.
2. **Clear Lantern Room Ascent Corridor:**
   - Reposition `Crank Station` slightly radially outward to $(2.40, 25.95, 0.00)\text{m}$ with a adjusted collider that does not drop below $y = 25.60\text{m}$.
   - Remove duplicate `Lamp Hand-Crank Stand` from the stairwell opening.
   - Ensure the lantern floor stair opening between $-145^\circ$ and $+60^\circ$ maintains $2.1\text{m}$ vertical clearance above every step of Flight 7.
   - Ensure the grand arched entry portal at $+50^\circ$ has an open, barrier-free passage of $1.4\text{m}$ width.
   - Provide a continuous solid circular maintenance deck ($r \in [1.20, 4.40]\text{m}$) around the Fresnel lens beacon allowing $360^\circ$ walkability.

### Phase 3 & 4: Separate Floor Architecture & Protected Zones
1. **Ground Floor (y = 0.15m):**
   - Retain entry stone foyer, compass flagstone inlay, coat rack, oilskins, boot bench, and maritime storage crates.
   - Keep eastern stair starter curb and stair base completely clear ($r > 2.2\text{m}, z > -0.5\text{m}$ protected zone).
2. **Attached Cottage (y = 0.15m):**
   - Kitchen with cast iron stove, sink counters, stew pot, warm fire embers.
   - Dining area with wooden table, chairs, hot meal, and radio desk.
   - Completely separated from the tower by the tower entrance doorway at $z = -4.03\text{m}$.
3. **First Floor — Archive & Workshop (y = 3.20m):**
   - Curving bookshelves along northern perimeter wall away from stairwell.
   - Maritime charts, astrolabe, globe, keeper's desk, and instrument workbench.
   - Relocate any desk or hiding spot colliders completely outside the stair landing envelope.
4. **Second Floor — Keeper's Living Quarters (y = 6.40m):**
   - Keeper's bed, wool blanket, wardrobe, bedside stand, station clock, spyglass.
   - Placed in the southern arc ($z < 0$), leaving the northern stairwell completely clear.
5. **Upper Tower Shaft (y = 6.40m to 25.60m):**
   - Clean, open, atmospheric spiral ascent with architectural stone wall brackets and iron balustrades.
   - Remove stray crates, benches, or hiding spots that infringe on the stair path.

### Phase 5 & 6: Visual Polish & Full Traversal Playtest
1. Apply authentic PBR materials (weathered stone, aged oak, oxidised iron, polished brass, beacon glass).
2. Validate complete traversal in Play Mode: Ground entrance $\to$ First floor $\to$ Second floor $\to$ Lantern room $\to$ Walk around lamp $\to$ Return down to entrance.
3. Export high-resolution screenshots and compile `docs/LIGHTHOUSE_ARCHITECTURE_FIX_REPORT.md`.
