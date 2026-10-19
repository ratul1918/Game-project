# Phase 2 Part 3 playtest

## Start

1. Open the `client/` folder in Unity `6000.6.6f1`.
2. Run `Ember Blade > Build Chapter 1 Scene` if `Assets/Scenes/Chapter1.unity`
   has not been generated.
3. Open `Assets/Scenes/Chapter1.unity`.
4. Press Play.

## Chapter 1 flow

### Mission 1.1 — Harvest Night

Move close to each village NPC and press `E`:

1. Tomas
2. Mara
3. Villager
4. Villager
5. Lira

Press `E` again to finish each line. After all five conversations, the smoke
marker appears on the horizon and the barricade phase begins.

### Mission 1.2 — Smoke on the Border

During the build phase:

- Move with `WASD`.
- Place walls with left-click.
- Press `T` to select spike traps.
- Press `R` to rotate a wall.
- Right-click a placed piece to remove it.
- Press `Enter` to start the raid early.

The raid contains two waves of scouts and skirmishers. Defeat both waves.
Walls should block paths and spike traps should damage enemies.

### Mission 1.3 — The Summons

After both waves are defeated, move to the royal rider and press `E`.
The objective changes to `Chapter 1 complete`.

## Checklist

- [ ] The objective text starts with Mission 1.1.
- [ ] Each NPC can be spoken to once and dialogue is shown from data assets.
- [ ] Smoke appears only after all Mission 1.1 conversations finish.
- [ ] The barricade build phase starts after Mission 1.1.
- [ ] Two enemy waves spawn after the build phase.
- [ ] Player death reports failure and restarts Mission 1.2.
- [ ] The royal rider appears after both waves are defeated.
- [ ] The Chapter 1 completion message appears after the rider dialogue.
- [ ] `MissionManager.OnMissionStarted` and `OnMissionCompleted` fire for the
  three mission IDs; these hooks are intentionally not connected to the server
  until Part 4.
