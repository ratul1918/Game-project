# Harrow Rock

## Story brief

Daniel Hale is a guilt-ridden former sailor who abandoned six sailors in a storm. At dusk, a boatman ferries him to Harrow Rock to replace lighthouse keeper Thomas Crane, who has been silent on the radio for three weeks. The boatman warns Daniel to light the lamp before dark and never answer any singing. The tower door is unlocked, the lamp is warm, a hot meal waits, and a radio voice named Marta greets Daniel by name.

As the lamp runs low on fuel, the cellar hums a sea shanty and wet footprints lead in but never out. A drowned thing in a keeper's coat stalks Daniel by sound. Pale Drowned figures wait in the shallows and advance whenever Daniel looks away. A storm kills the generator, forcing Daniel to hand-crank the lamp; the crank is loud and draws the stalker, while darkness lets the figures advance.

The logbook reveals that island wreckers deliberately sank the *Marguerite* and that the lighthouse is a leash holding the Drowned in the water. In a hidden cellar room, Daniel learns Marta is a recording and finds a ledger containing his name, written weeks earlier: the harbor board selected him for his guilt. Crane's locked room contains a family photograph of Crane holding Daniel as a child. The stalker is Daniel's father, herding him toward the boat shed and away from the water. Crane's final entry says he gave himself to the sea in place of his son, and the sea would not let him rest.

In the finale, the courtyard floods with Drowned as the lamp's fuel nearly runs out. Daniel holds the light while Crane, humming the lullaby to remain himself, draws the Drowned away and slams doors to cover Daniel's noise. At first light the Drowned must return to the sea. Crane blocks the doorway so Daniel can reach the dock, sings the lullaby's last line, and sinks, finally at rest. Daniel leaves on the morning boat. The ledger shows his name crossed out, while Marta has already written the next keeper's name.

## Code rules

- Write C# under the `HarrowRock` namespace.
- Give each script one responsibility.
- Place scripts in `Assets/_Project/Scripts/<System>/`.
- Use Unity's new Input System; do not add legacy `Input` API usage.
- Expose tunable values through serialized fields instead of magic numbers.
- Communicate between systems with the contracts in `docs/events.md`; prefer events over direct references.
- Add comments only when logic is non-obvious; do not narrate straightforward code.
- Treat the documents in `docs/` as source-of-truth contracts. Update the contract before changing behavior.
- Do not edit Unity scene files unless the user explicitly assigns scene work.

## Weekly rules

- Always keep a playable build.
- Fix a broken build before starting new features.
- Deliver vertical slices that run end to end.
- Greybox with boxes and placeholders before adding art.
- Cut scope early when a weekly slice is at risk.
