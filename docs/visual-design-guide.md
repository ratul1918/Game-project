# The LightHouse — Visual Design & Art Direction Guide

**Art Direction:** Atmospheric Psychological Maritime Survival Horror  
**Visual Tone:** Somber, cold, salt-encrusted, weathered, grounded realism  
**Key Visual Influences:** *Still Wakes the Deep*, *Amnesia: The Bunker*, *Outlast*, 19th/early-20th-century coastal maritime isolation  

---

## 1. Core Visual Identity

The visual presentation of *The LightHouse* is anchored in relentless, unforgiving North Atlantic coastal isolation. Every object, wall, and horizon tells the story of prolonged assault by sea spray, gale-force winds, rust, and moral rot.

> **"You can fight back, but you cannot control the sea."**

The lighthouse stands as a lonely, flawed beacon against an endless, terrifying ocean. The player must feel small, vulnerable, and damp.

---

## 2. Color Palette & Lighting Hierarchy

### Color Palette

| Tone | Hex Code | RGB | Aesthetic Application |
|---|---|---|---|
| **Abyssal Black** | `#05080C` | `(5, 8, 12)` | Deep shadows, stormy sea depths, unlit interiors |
| **Desaturated Slate Teal** | `#1C2B30` | `(28, 43, 48)` | Wet granite, coastal rock, storm mist, twilight sky |
| **Cold Sea Foam Grey** | `#5F7379` | `(95, 115, 121)` | Breaking wave crests, salt crust, aged zinc roofing |
| **Muted Maritime Green** | `#26392E` | `(38, 57, 46)` | Clinging sea kelp, lichen on stone, rotten wood pilings |
| **Warm Lantern Amber** | `#D9943B` | `(217, 148, 59)` | Fresnel beacon light, kerosene lanterns, kitchen hearth fire |
| **Weathered Oak Brown** | `#453324` | `(69, 51, 36)` | Heavy door timbers, ship planking, stair steps |
| **Corrosion Rust Red** | `#69291D` | `(105, 41, 29)` | Iron ladders, balustrades, boiler rivets, oxidized tools |
| **Parchment Off-White** | `#D5CDBC` | `(213, 205, 188)` | Harbor board ledgers, logbook journals, yellowed letters |

### Lighting Rules
1. **Contrast & Chiaroscuro:** Deep, suffocating darkness punctuated by sharp, amber safety. Warm light represents temporary shelter and sanity; cold blue-gray represents the merciless sea.
2. **Rotating Lighthouse Beam:** The sweeping beam is a physical presence. It cuts through rain, casts long moving shadows across the courtyard and rocks, and momentarily illuminates pale silhouettes in the water.
3. **Restrained Real-Time Lights:** Directional moonlight, rotating lamp spotlight, and immediate handheld lantern. All static architectural bounce is baked into lightmaps to conserve M3 GPU memory.

---

## 3. Environmental Surfaces & Materiality

All materials use Physically Based Rendering (PBR) tuned for wet, weathered surfaces:

- **Salt Plaster:** Crumbling off-white lime wash over rough stone masonry, heavily streaked with damp salt efflorescence and water staining.
- **Old Timber:** Dark, splintered oak and pine with deep grain, weathered silver on exterior faces and dark varnish on interior furniture.
- **Oxidized Iron:** Pitted cast iron with deep reddish-brown rust nodules, peeling black enamel paint, and brass fittings.
- **Wet Shale & Shore Mud:** High-roughness micro-detail with low smoothness, punctuated by puddles with crisp, dark planar reflections.

Avoid clean, flat surfaces. Every asset must carry history: water damage, scratches, grease, barnacles, and corrosion.

---

## 4. UI/UX Visual Language

The user interface reflects commercial horror standards: unobtrusive, tactile, legible, and atmospheric.

### Typography
- **Headings & Titles:** Bold, restrained serif or vintage display typeface reminiscent of 19th-century maritime boards (e.g., *Cinzel*, *Baskerville*, or styled bold block).
- **Body & Prompts:** Crisp sans-serif with high readability against dark backgrounds (e.g., *Inter*, *Roboto*, or clean maritime grotesque).

### UI Components
- **Color Styling:** Dark slate translucent panels (`#0B1117`, 85% alpha) with hairline amber accents (`#D9943B`).
- **Interactive States:**
  - *Normal:* Subdued off-white (`#C0C8CF`).
  - *Hover / Focused:* Bright ivory (`#FFFFFF`) with subtle warm amber glow.
  - *Pressed / Active:* Vibrant lantern amber (`#E8A042`).
  - *Disabled:* Muted dark charcoal (`#303840`).
- **HUD Philosophy:** Zero screen clutter. Health, stamina, noise, and lamp status fade out during quiet exploration and fade in smoothly when values change or threats loom.

---

## 5. Visual Consistency Guidelines

1. **No Cartoon or Stylized Proportions:** Human characters, tools, and architecture strictly follow realistic historical proportions.
2. **Unified Texture Density:** All props adhere to a consistent texel density budget (1K max, 512 for small items).
3. **No Unmotivated Bloom or Lens Flare:** Optical artifacts are limited to wet lens rain droplets and the blinding focus of the Fresnel beacon.
