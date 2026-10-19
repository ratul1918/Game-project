# Phase 2 Part 2 playtest

## Start

1. Open the `client/` folder in Unity `6000.6.6f1`.
2. Open `Assets/Scenes/BarricadeTest.unity`.
3. Press Play.

## Build-phase controls

- `WASD`: move the player.
- Hold left mouse and move the mouse: rotate the follow camera.
- Move the mouse over the ground: position the ghost preview.
- Left mouse button: place the selected piece.
- `T`: switch between wall and spike trap.
- `R`: rotate the ghost preview by 90 degrees.
- Right mouse button: remove a placed wall or trap.
- `Enter`: start the raid early.

The build phase ends automatically after the configured time in
`Assets/Resources/GameConfig.asset` (30 seconds by default).

## Checklist

1. Confirm the timer and remaining wall/trap counts appear in the HUD.
2. Confirm a green ghost preview appears over valid ground.
3. Confirm the preview turns red when placed too close to another piece or
   outside the placement range.
4. Place no more than eight walls and three traps.
5. Press `T`, place a spike trap, and confirm it uses the yellow material.
6. Press `R` and confirm a wall preview rotates.
7. Right-click a placed piece and confirm it is removed and its inventory count
   is restored.
8. Press `Enter` to start the raid early, or wait for the timer to reach zero.
9. Confirm enemies spawn only after the build phase ends.
10. Confirm walls have NavMesh obstacle carving and block enemy paths.
11. Confirm enemies stepping on spike traps take damage.
12. Confirm the final counts are stored in `BarricadeResult.WallsPlaced` and
    `BarricadeResult.TrapsPlaced`.
