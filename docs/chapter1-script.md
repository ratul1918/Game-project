# Act 1 Script — Arrival

Target duration: 8–12 minutes. This page is the format template for later acts: purpose, scene flow, dialogue, required interactions, unlock, and exit condition.

## Purpose

Establish Daniel, the island rules, the impossible signs of recent occupancy, Marta's unsettling familiarity, and the lamp as the player's central responsibility. End with a playable transition into Night.

## Scene flow

### 1. Ferry approach — `A1-B01`

Grey sea, close fog, Harrow Rock ahead at dusk. Daniel stands on the small ferry while the boatman works in silence. Player control is limited to looking.

**Boatman:** “Crane's missed three weeks of calls. Could be the set. Could be worse.”

The lighthouse beam is dark. The boat nudges the dock.

### 2. The warning — `A1-B02`

Daniel steps ashore; movement unlocks. The boatman stays aboard.

**Boatman:** “Light the lamp before dark. And if you hear singing—don't answer it.”

**Daniel:** “When do you come back?”

**Boatman:** “Morning. If the light's on.”

The ferry pulls away as soon as Daniel crosses the dock marker. Objective: **Reach the lighthouse.**

### 3. Tower threshold — `A1-B03`

The tower door is visibly closed but unlocked. Required interaction: open it. Inside, no one answers Daniel's call.

**Daniel:** “Crane?”

Environmental facts: the interior is orderly; the lamp mechanism is not yet visible; wind and sea dull behind the closed door.

### 4. The waiting meal — `A1-B04`

A place is set for one. Steam or another placeholder effect shows that the meal is hot. Required interaction: inspect it.

**Daniel:** “This was just made.”

The radio activates immediately after the inspection.

### 5. Marta on the radio — `A1-B05`

Required interaction: operate the radio.

**Marta (radio):** “Daniel Hale. You made good time.”

**Daniel:** “Who is this? Where's Thomas Crane?”

**Marta:** “Marta. Harbor relay. Questions after the lamp, Daniel. You have minutes.”

**Daniel:** “I never gave you my name.”

Only static answers. Objective changes to: **Climb to the lamp room and light the lamp.**

### 6. First light — `A1-B06`

At the top of the tower, the lamp housing is warm to the touch. Required interactions: inspect the lamp, supply the available starter fuel if needed, and ignite it. The beam starts; the game enters Night.

**Daniel:** “Warm.”

Marta returns over the room speaker or radio.

**Marta:** “There. Now keep it fed.”

For one beat, a pale shape may be visible far out in the shallows, without advancing yet.

## Act exit

Act 1 completes when `LampStateChanged` reports `FuelLit` and `StoryDirector` commits `A1-B06`. Unlock fuel drain, Night state, and the Act 2 low-fuel threshold. The player must retain control in the lit tower so the end of the act is playable, not a cutscene-only handoff.

## Implementation guardrails

- Greybox all locations and use placeholder audio before art production.
- Do not reveal that Marta is recorded, that Crane is Daniel's father, or why Daniel was chosen.
- The pale shape is optional for the first Act 1 slice; the lamp and radio chain is not.
- Every required interaction must have one clear prompt and one resulting beat.
