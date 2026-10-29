# Act 4 greybox playtest

Run `TheLightHouse/Build Act 4 Scene`, bake the scene's NavMeshSurface, save, and press Play. The scene starts
directly after the storm in `Storm`, with the charge lamp, Crane, figures, and hiding systems active. Act 5 is
not built; A4-B06 enters `Finale` and stops at a clean objective/subtitle placeholder.

## Expected sequence

1. Read the *Marguerite* page in the keeper's room: A4-B01. Read another page with J available for the journal.
2. Read the leash page in the storeroom: A4-B02. The cellar unlocks after the configured page count and the shanty clue points down.
3. Operate Marta's radio. Tune down/up with Q/T or use the mouse wheel. The needle combines tuning accuracy and distance; follow its increasing strength toward the cellar.
4. Slide the false shelf. It emits a 10 m noise. In the hidden room, the recorder repeats “Daniel Hale. You made good time.” identically until stopped. Stopping it commits A4-B03.
5. Read the ledger dated 03 October 1911, three weeks before Daniel's 24 October arrival. It names his guilt over six abandoned sailors, commits A4-B04, runs the short flashback, and is collected for Act 5.
6. Take `CraneRoomKey` from the boat shed, return upstairs, and unlock Crane's room. Inspect the family photo to commit A4-B05.
7. After the reveal, let Crane approach: he stops a few steps away, hums, and retreats instead of requesting the catch sequence. His boat-shed bias is stronger.
8. Read the now-unsealed final entry on Crane's desk: A4-B06. State becomes `Finale`; objective becomes **Hold the light until dawn** and the Act 5 placeholder remains stable.

Reading does not pause the scene. The default threat speed is 0.35, lamp charge decay factor is 0.30, and a
new page grants 0.08 charge once. Tune these in `Assets/_Project/Settings/ActFourSettings.asset`. Crane's core
hearing range and curve remain on StalkerAI.

## Verification boundaries

Unity strict compilation returned zero errors and warnings. Unity generated and baked the scene through editor
APIs. Runtime checks confirmed Storm/charge-mode start, active Crane patrol, six page pickups, cellar unlock,
Crane speed 3.50→1.23 during reading grace, a complete ground-to-lantern NavMesh path, the ordered
A4-B01–A4-B06 chain, the Finale transition, and post-reveal contact producing Retreat without catching Daniel.
The complete player-controlled route, UI readability at target resolution, signal tuning feel, camera zoom comfort,
Crane encounter difficulty, and assigned audio remain manual playtest work. AudioClip slots are intentionally empty.
