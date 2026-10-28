# Story Beat Contract

`StoryDirector` is the only authority that marks beats complete. IDs never change after implementation begins. A trigger is evaluated only after all earlier required beats in the same act are complete.

## Act 1 — Arrival

Implementation clarification: the low-fuel inspection and boat-shed fuel trip are intermediate interactions before A1-B06, not new beat IDs. Marta's automatic greeting follows the meal inspection per chapter1-script.md. The cellar shanty and footprints remain Act 2 content and are disabled in the Act 1 scene. Only the first six beats are implemented in StoryDirector's Act 1 asset list.

| ID | Beat | Trigger condition | Unlocks |
|---|---|---|---|
| A1-B01 | Ferry at dusk | New game begins and Intro completes. | Player look; boatman sequence. |
| A1-B02 | Boatman's warning | Daniel reaches the dock marker. | Player movement; island path; objective to reach tower. |
| A1-B03 | Unlocked tower | Front door is opened. | Tower ground floor; meal and radio interactions. |
| A1-B04 | Hot meal | Player inspects the warm meal. | Context clue; radio call becomes available. |
| A1-B05 | Marta knows Daniel | Player operates the radio after A1-B04. | Lamp room access; objective to light the lamp. |
| A1-B06 | First light | Lamp reaches `FuelLit` before the dusk deadline. | Night state; fuel drain; Act 2 eligibility. |

## Act 2 — Cellar and Stalker

| ID | Beat | Trigger condition | Unlocks |
|---|---|---|---|
| A2-B01 | Fuel runs low | Lamp fuel crosses the low threshold during Night. | Cellar fuel objective; cellar door interaction. |
| A2-B02 | Shanty below | Player enters the cellar audio zone. | Shanty clue; deeper cellar route. |
| A2-B03 | Footprints with no return | Player inspects the final wet footprint marker. | First stalker setup; shallows observation point. |
| A2-B04 | Hunted by sound | Player emits qualifying noise after A2-B03 and the stalker investigates it. | Full stalker patrol/investigation loop. |
| A2-B05 | Figures in the shallows | Player observes a Drowned figure and then looks away long enough for one advance. | `DrownedManager`; Act 3 storm trigger. |
| A2-B06 | Knocking | A figure reaches the front door after A2-B05. | Escalating door-knock sequence; objective not to open the door. |
| A2-B07 | Hide success | Player has remained hidden through a breathing interval without being caught, after A2-B06. | Act 2 completion objective. |

Act 2 test scene starts directly at Night, with the Act 1 chain already bypassed for testing.
A2-B01 uses a serialized low-fuel threshold (30 units by default) and reveals the reserve can.
A2-B02 requires the player to approach the cellar/storeroom zone; A2-B03 requires inspecting the wet trail.
A2-B04 completes on Crane's first audible investigation. A2-B05 requires prior observation of the advancing figure;
there is no separate first-seen beat. Figures freeze while observed and while the lamp is FuelLit or CrankPowered.
They advance only during a failing/unlit lamp, after A2-B04. A2-B05 makes a future storm eligible;
this slice does not implement or automatically start Act 3. A2-B07 signals can be remembered until their prerequisites finish.

## Act 3 — Storm and Crank

| ID | Beat | Trigger condition | Unlocks |
|---|---|---|---|
| A3-B01 | Storm breaks | A2-B05 is complete and the player returns to the lamp room. | Storm state; worsening exterior conditions. |
| A3-B02 | Generator dies | Storm timer expires or the authored lightning strike occurs. | Generator disabled; lamp decay; crank interaction. |
| A3-B03 | Hand-cranked light | Player completes the first effective crank stroke. | Crank-power loop; loud crank noise events. |
| A3-B04 | Darkness advances them | Lamp reaches `Failing` or `Extinguished`, causing a Drowned advance. | Sustained lamp-versus-noise pressure; Act 4 clue access. |
| A3-B05 | Restart fails | Generator restart is attempted after A3-B02. | Confirms generator is permanently flooded this act. |
| A3-B06 | Crane on the stairs | Crane pauses during a climb after A3-B03. | Footstep warning; lantern-room hiding pressure. |
| A3-B07 | First pulse | High charge is held long enough to pulse after A3-B03. | Demonstrates the lighthouse leash. |
| A3-B08 | Courtyard filling | Configured number of figures reach courtyard path ends after A3-B04. | Courtyard warning. |
| A3-B09 | Storm eases | Total healthy-lamp time is reached, with A3-B04 complete. | Safe Act 4 logbook placeholder; storm pressure stops. |

Act 3 primary order is A3-B01 → B02 → B03 → B04. B05–B09 are optional/conditional assets,
checked once by StoryDirector after their explicit prerequisiteId; they do not block the primary sequence.
The test scene begins directly in Storm; StormBeats requests B01, and the generator fails after a delay.
For an integrated Night sequence, StormBeats makes B01 eligible after A2-B05 and return to lantern height.
The chosen end condition is accumulated healthy-lamp time (90 seconds by default), after the first darkness advance.
This unlocks a placeholder rather than building Act 4. The game remains in Storm: the architecture permits
Storm → Finale only at A4-B06. Rain, pursuit and figure pressure stop, and the lamp is stabilized.
Figures still freeze while observed, even when extinguished. Pulse pushes back only unobserved figures,
preserving the existing observation contract.

## Act 4 — Truth

| ID | Beat | Trigger condition | Unlocks |
|---|---|---|---|
| A4-B01 | The *Marguerite* | Player reads the relevant logbook entry after A3-B04. | Wrecker history; lighthouse-leash clue. |
| A4-B02 | Lighthouse as leash | Player completes the second logbook page or linked inspection. | Hidden-room mechanism. |
| A4-B03 | Marta is a recording | Hidden cellar room opens and player operates its recorder. | Ledger interaction. |
| A4-B04 | Daniel was chosen | Player reads his pre-written ledger entry. | Crane's locked room can be opened. |
| A4-B05 | Father and son | Player inspects the photograph of Crane holding young Daniel. | Stalker reinterpretation; final diary interaction. |
| A4-B06 | Crane's bargain | Player reads Crane's final entry. | Finale state and courtyard route. |

## Act 5 — Dawn

| ID | Beat | Trigger condition | Unlocks |
|---|---|---|---|
| A5-B01 | Courtyard floods | Player exits toward the boat shed after A4-B06. | Finale pursuit; Drowned courtyard formation. |
| A5-B02 | Hold the light | Player keeps the failing lamp alive for the authored duration. | Crane's protective route and door slams. |
| A5-B03 | Crane remembers | Crane's lullaby state begins while he draws the Drowned away. | Dock escape route. |
| A5-B04 | First light | Finale objective completes and dawn threshold is reached. | `DawnReached`; Drowned retreat; Crane doorway block. |
| A5-B05 | Last line | Daniel reaches the dock trigger and Crane completes the lullaby. | Crane's rest; morning boat transition. |
| A5-B06 | The next keeper | Player views the ledger epilogue with Daniel crossed out and a new name entered. | Ending and credits. |
