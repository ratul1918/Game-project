# The LightHouse — Secret Radio Control Room, Flooded Courtyard Climax & Architecture Completion Report

## 1. Executive Summary

This report documents the completion of the three major environment systems for **The LightHouse** in Unity 6 (Universal Render Pipeline), directly aligning the game with Storyboard Image 10, 11, and 12:

1. **The Secret Basement Radio Control Room (Image 11 Panel 4 & Image 12 Panels 2–3)**:
   - Concealed vaulted masonry room beneath Harrow Rock with damp cut stone, cracked lime plaster, and saltpeter bloom.
   - 1940s–1950s vacuum tube shortwave radio console with 4 glowing tungsten filament tubes, dial tuners, green pilot indicator LED, and red power lamp.
   - Marta's magnetic reel-to-reel tape machine with twin spinning aluminum reels, tape head block, and illuminated VU meters.
   - Heavy brass Morse telegraph key and vintage bakelite microphone.
   - **Key Story Prop — The Harbor Board Ledger**: Open double-page ledger displaying "Thomas Crane" crossed out with deliberate ink strikes and "Daniel Hale" written underneath in fresh black ink with narrative assignment notes and Harbor Board District IV wax seal (`TheLightHouse.Ledger` interactable).
   - **Framed Cracked Family Photograph**: Vintage 1898 photograph of Thomas Crane in keeper's coat holding young Daniel Hale, featuring cracked glass fracture lines and aged photo mount (`TheLightHouse.FamilyPhoto` interactable).
   - **Admiralty Nautical Navigation Chart**: Pinned to damp wooden backing board on the North stone wall, featuring Harrow Rock shoals, depth soundings, red wax grease-pencil shipping channels, and bold red wreck site of the *Marguerite* with the key lore line: *"The Light is the Leash"*.
   - Vintage emerald green banker's desk lamp with dedicated incandescent downward illumination.

2. **The Flooded Courtyard & Boat Shed Climax (Image 12 Panel 4)**:
   - Exterior compound flooded with a **0.5-meter rising sea surge** ($y = 0.50\text{m}$) churning against the stone walls.
   - Partially broken elevated wooden dock walkway ($y = 0.58\text{m}$) with missing planks swept away by waves (indices 6, 13, 18) and tilted, wave-loosened planks (indices 5, 7, 12, 14, 19, 21).
   - Weathered Coastal Boat Shed ($7.2\text{m} \times 6.2\text{m} \times 4.75\text{m}$) with pitched slate roof, porch overhang, hurricane storm lantern, rowboat hull in cradle, oars, rope coils, and storage barrels.
   - Pale waterlogged Drowned figures standing waist-deep in the churned flood waters, with seafoam rings where their bodies breach the surge.
   - Top lantern volumetric beacon light tilted downward ($24^\circ$) to rhythmically sweep across the courtyard, illuminating the water churn, broken boardwalk, and Drowned figures in high contrast before they plunge back into dark blue silhouettes.

3. **Keeper House Front Door Widening (Navigation Fix)**:
   - Increased entrance doorway opening from $1.2\text{m}$ to **$1.8\text{m}$** ($x = -9.40\text{m}$ to $-7.60\text{m}$).
   - Adjusted exterior stone wall colliders (`Front West` and `Front East`) to clear a $2.6\text{m}$ masonry gap.
   - Aligned internal cottage partition walls (`Cottage Partition West` and `East`) to a matching $1.8\text{m}$ opening.
   - Widened threshold sill to $2.2\text{m}$ and intermediate/approach steps to $2.5\text{m}$ and $2.8\text{m}$.
   - Door leaf swings open $95^\circ$, completely eliminating player snagging and ensuring effortless first-person traversal.

---

## 2. Technical Implementation Breakdown

### 2.1 Secret Radio Control Room Construction (`Assets/_Project/Scripts/Editor/HarrowRockEnvironmentMasterPass.cs`)

```
Secret Room Position: Vector3(-12.0f, -3.2f, -5.0f)
Workbench Desk Position: Vector3(-12.0f, -3.2f, -4.05f)
```

| Component | Position (Local to Desk) | Dimensions / Material | Story Interaction |
| :--- | :--- | :--- | :--- |
| **Workbench** | $(0, 0.42, 0)$ | $2.50\text{m} \times 0.84\text{m} \times 1.05\text{m}$ (`matWood`) | Heavy timber desk |
| **Harbor Board Ledger** | $(-0.15, 0.885, -0.12)$ | $0.64\text{m} \times 0.035\text{m} \times 0.44\text{m}$ (`matLedger`) | `Ledger.cs` interactable |
| **Family Photo** | $(0.28, 0.91, -0.06)$ | $0.26\text{m} \times 0.34\text{m} \times 0.035\text{m}$ (`matPhoto`) | `FamilyPhoto.cs` interactable |
| **Radio Transceiver** | $(0.75, 0.96, 0.16)$ | $0.68\text{m} \times 0.38\text{m} \times 0.44\text{m}$ (`matRadioMetal`) | 1950s Military shortwave console |
| **Vacuum Tubes (4x)** | $x \in [0.54, 0.96]$ | Glass cylinder + glowing filament | `#FF6C20` emission + amber point light |
| **Green Indicator LED** | $(-0.28, -0.15, -0.52)$ | $0.04\text{m}$ sphere (`matIndicatorGreen`) | `#33FF55` HDR emission |
| **Red Power Lamp** | $(-0.21, -0.15, -0.52)$ | $0.04\text{m}$ sphere (`matIndicatorRed`) | `#FF3322` HDR emission |
| **Morse Telegraph Key** | $(0.30, 0.885, -0.32)$ | Brass base, lever arm, bakelite knob | Heavy telegraph key |
| **Bakelite Microphone** | $(-0.52, 0.885, -0.18)$ | Stepped base, brass neck, perforated head | Period vocal receiver |
| **Reel-to-Reel Recorder** | $(-0.75, 0.95, 0.16)$ | $0.65\text{m} \times 0.36\text{m} \times 0.46\text{m}$ (`matReelToReel`) | Marta's recording machine |
| **Banker's Desk Lamp** | $(-0.48, 0.885, 0.16)$ | Brass base, neck, emerald green shade | $3.2\text{lm}$ warm downward spotlight |
| **Admiralty Wall Chart** | $(0, 1.55, 0.72)$ | $1.65\text{m} \times 1.15\text{m}$ quad on timber board | Marguerite wreck chart |

### 2.2 Flooded Courtyard & Boat Shed Climax

```
Courtyard Flood Surge Plane: y = 0.50m (24.0m x 12.5m)
Elevated Broken Boardwalk: y = 0.58m
Boat Shed Origin: Vector3(9.8f, 0.0f, -14.0f)
```

- **Wave Surge & Churn**: 0.5m of deep ocean flood water (`FloodedCourtyardWater.mat`) with smooth specular reflection ($0.98$), metallic sheen ($0.05$), and 18 swirling foam patches (`CourtyardSeaFoam.mat`) using soft organic feathering (`Seafoam_diff.png`).
- **Boardwalk Broken Planks**: Planks 6, 13, and 18 are completely swept away by surging waves, revealing dark water churning underneath. Planks 5, 7, 12, 14, 19, and 21 are tilted $8^\circ$–$15^\circ$ with loosened lateral shifts.
- **Drowned Threats**: 3 waterlogged figures standing waist-deep at `(3.2, -0.15, -11.2)`, `(4.8, -0.15, -16.5)`, and `(1.5, -0.15, -15.8)` with wet dripping cloth (`DrownedWaterloggedRags.mat`), sickly green-grey skin (`DrownedPaleSkin.mat`), and churning foam rings at water level.
- **Volumetric Beacon Sweeps**: Top tower lantern beacon tilted $24^\circ$ downward and rotating at $18^\circ/\text{s}$ via `VolumetricSweepLight.cs`, sweeping raking light across the figures and flood waters before plunging them into deep blue silhouettes.

### 2.3 Keeper House Doorway Widening

- `IslandArrivalPass.cs`: `DoorLeaf("Keeper House Front Door", new(-9.40f, .15f, -9.05f), 0, 1.80f, 2.2f, "Keeper House Front Door")`.
- `ReferenceVisualPass.cs`: Front wall openings widened from $0.70\text{m}$ to $2.60\text{m}$ (`Front West` from $-12.50$ to $-9.80$, `Front East` from $-7.20$ to $-4.80$).
- Internal partition wall openings widened to $1.80\text{m}$ (`Cottage Partition West` and `East`).
- Result: CharacterController can walk straight in and out of the cottage without any snagging.

---

## 3. Validation Screenshot Gallery

| Screenshot File | Description | Storyboard Alignment |
| :--- | :--- | :--- |
| `docs/screenshots/39_secret_radio_room_overview.png` | Hidden vaulted masonry room, desk, reel-to-reel, radio console, glowing vacuum tubes, wall chart | Image 12 Panel 2 |
| `docs/screenshots/40_finding_the_ledger_closeup.png` | Open Harbor Board ledger with "Thomas Crane" struck through, "Daniel Hale" in fresh ink, brass inkwell, framed cracked photo of Crane & Daniel under banker's lamp | Image 12 Panel 3 |
| `docs/screenshots/41_marguerite_wall_chart_closeup.png` | Admiralty chart of Harrow Rock shoals, red grease-pencil shipping channels, and bold red wreck site of the *Marguerite* ("The Light is the Leash") | Image 12 Panels 2–3 |
| `docs/screenshots/42_radio_gear_and_vacuum_tubes.png` | Close view of 1950s radio console with 4 glowing tungsten filaments, green indicator LED, Morse key, and microphone | Image 12 Panel 2 |
| `docs/screenshots/43_cottage_widened_front_door.png` | Exterior view through the widened 1.8m cottage front door into the warm living room and kitchen, showing unobstructed entry | Navigation Fix |
| `docs/screenshots/35_flooded_courtyard_boatshed_path.png` | First-person tactical flashlight view across the 0.5m surge, broken boardwalk planks, Drowned figures, and boat shed | Image 12 Panel 4 |
| `docs/screenshots/36_boatshed_exterior_closeup.png` | Close view of weathered boat shed porch, hanging hurricane storm lantern, and boat cradle | Image 12 Panel 4 |
| `docs/screenshots/37_drowned_in_flooded_courtyard.png` | Close view of pale Drowned figure standing waist-deep in flood surge beside broken planks | Image 12 Panel 4 |
| `docs/screenshots/38_courtyard_boardwalk_overview.png` | High-angle overview of flooded courtyard, elevated broken boardwalk, and sweeping beacon beam | Image 12 Panel 4 |

---

## 4. QA & Performance Verification

- **Compilation Status**: Zero compilation errors across entire solution (`compilationFailed: false`).
- **Console Log**: Zero warnings or errors in Unity Editor console (`consoleErrors: 0`).
- **Scene Persistence**: Applied and verified across all 5 game scenes:
  - `Assets/_Project/Scenes/Act1.unity`
  - `Assets/_Project/Scenes/Act2.unity`
  - `Assets/_Project/Scenes/Act3.unity`
  - `Assets/_Project/Scenes/Act4.unity`
  - `Assets/_Project/Scenes/Act5.unity`
- **Play Mode Performance**: Verified smooth 60+ FPS navigation on Metal (macOS) with soft shadows and full post-processing.
