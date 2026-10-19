# Phase 2 Part 1 playtest

## Start

1. Open the `client/` folder in Unity `6000.6.6f1`.
2. Open `Assets/Scenes/CombatTest.unity`.
3. Start the server only if you also want to compare the Phase 1 scene; the
   combat test scene itself is local and does not require login.
4. Press Play.

## Controls

- `WASD`: move Kael.
- Hold left mouse and move the mouse: rotate the follow camera.
- Left mouse button: light attack.
- `Left Shift`: dodge. Kael is invulnerable for the configured dodge window.
- Right mouse button: parry. A successful parry prevents the incoming hit and
  adds the configured Ember gain.

## Checklist

1. Confirm the blue capsule player moves over the ground.
2. Confirm the camera follows the player and responds to mouse movement.
3. Confirm three gray Silverhold Scouts and two blue Silverhold Skirmishers
   chase the player.
4. Confirm a light attack damages enemies and the enemies remaining HUD count
   decreases when an enemy dies.
5. Confirm Scout attacks damage the player health bar.
6. Confirm Skirmisher wind-up is visible as a yellow color flash.
7. Press right mouse during the Skirmisher wind-up and confirm the player is
   not damaged and Ember increases.
8. Fill Ember to 100 through successful parries, then use a light attack and
   confirm Ember resets and that attack deals empowered damage.
9. Press `Left Shift` during an enemy attack and confirm the player avoids the
   damage during the dodge window.

All combat values are stored in `Assets/Config/GameConfig.asset`, so tuning the
test does not require changing code.
