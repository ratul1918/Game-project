# LIGHTHOUSE MAIN ENTRANCE IMPLEMENTATION

**Document Version:** 1.0.0  
**Project:** The LightHouse (Unity 6 / Universal Render Pipeline)  
**System:** Interactive Ground-Floor Exterior Entrance Portal  
**Status:** Architecture & Interaction Specification  

---

## 1. Overview & Placement

In previous versions of the project, the lighthouse lacked a dedicated, visible, exterior entrance door facing the arrival path. The front of the tower was obscured by the attached cottage geometry, forcing players to enter indirectly through the cottage kitchen and an internal hallway door.

The new **Grand Main Entrance Portal** is positioned directly on the exterior ground floor along the South face of the tower ($x = 0.0\text{m}, z = -6.50\text{m}$), facing the island arrival road and open forecourt.

---

## 2. Architectural Design & Specifications

| Component | Technical Specification | Description / Visual Details |
|---|---|---|
| **Portal Location** | $x = 0.0\text{m}, y = 0.15\text{m}, z = -6.50\text{m}$ | South azimuth ($180^\circ$), facing the incoming cart road. |
| **Clear Opening** | Width: **$1.25\text{ metres}$**, Height: **$2.25\text{ metres}$** | Generous clearance for first-person player capsule ($r = 0.4\text{m}$, $h = 1.8\text{m}$). |
| **Stone Surround** | Dressed Granite Chamfered Architrave | $0.45\text{m}$ thick granite jambs with carved semicircular arched stone hood moulding. |
| **Inscription Plaque**| Weathered Cast Brass | Plaque mounted above arched lintel: *"HARROW ROCK LIGHT STATION — 1888"*. |
| **Door Leaf** | Weathered Oak Planking | $1.25\text{m} \times 2.25\text{m} \times 0.14\text{m}$ double-thickness vertical timber planks with 3 structural ledges and diagonal brace. |
| **Iron Hardware** | Hand-Forged Oxidized Iron | Twin strap hinges extending across $70\%$ of the leaf with fleur-de-lis finials, heavy drop-ring iron handle, keyhole escutcheon. |
| **Threshold Sill** | Raised Granite Flagstone | Bevelled stone sill at $y = 0.15\text{m}$ preventing rainwater ingress. |
| **Approach Steps** | Dual Granite Steps | Step 1 ($y = 0.08\text{m}$, $z = -6.95\text{m}$), Step 2 ($y = 0.02\text{m}$, $z = -7.45\text{m}$) descending into the gravel courtyard. |
| **Exterior Lighting**| Twin Cast-Iron Bracket Lanterns | Mounted to stone jambs at $y = 2.40\text{m}$ ($x = \pm 0.95\text{m}$), casting warm amber illumination ($1.8\text{ lux}$, $3000\text{K}$). |

---

## 3. Interactive Door Mechanics & Collision

The entrance integrates directly with the project's existing interaction framework without code duplication:

1. **GameObject Structure:**
   - Hierarchy: `Lighthouse Main Entrance Door`
   - Child: `Operable Leaf` (contains mesh, BoxCollider, and hardware).
2. **Components Attached:**
   - `Door.cs`:
     - `doorId = "tower-main-entrance"`
     - `locked = false`
     - `openAngle = -105.0f` (swings smoothly inward into the vestibule against the west interior reveal).
     - `turnSpeed = 120.0f` (smooth architectural rotation speed).
     - `loudness = 6.0f`
   - `NoiseEmitter.cs`: Alerts nearby creatures (or the Stalker in later acts) when operated.
   - `AudioSource`: Plays authentic wooden creak and latch clatter on toggle.
3. **Player Collision & Walkway Passage:**
   - When **Closed**: The `BoxCollider` on the `Operable Leaf` physically blocks the player capsule, stopping unauthorized walkthrough.
   - When **Opened**: The door rotates inward by $-105^\circ$, completely clearing the $1.25\text{m}$ doorway. The player capsule walks seamlessly across the granite sill into the grand entrance hall with zero snagging or collision lips.
4. **HUD & Interaction Prompt:**
   - When looking at the door, the HUD displays:  
     `[E] Open door` (when closed) / `[E] Close door` (when open).
