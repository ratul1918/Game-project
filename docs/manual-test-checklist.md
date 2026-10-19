# Ember Blade manual test checklist

## Before testing

- [ ] Start PostgreSQL with `cd server && docker compose up -d`.
- [ ] Start the server with `cd server && mvn spring-boot:run`.
- [ ] Open `client/` in Unity 6000.6.6f1.
- [ ] Confirm the four scenes are in Build Settings.

## Phase 1

- [ ] Open `Assets/Scenes/Phase1.unity`.
- [ ] Register a user with the server, then log in.
- [ ] Confirm the WebSocket connects without crashing.
- [ ] Confirm the HUD displays Emberfield threat updates.
- [ ] Confirm the red raid banner appears and hides after five seconds.
- [ ] Stop/restart the scene and confirm there are no duplicate network objects
  or event-driven UI updates.

## CombatTest

- [ ] Open `Assets/Scenes/CombatTest.unity` and press Play.
- [ ] Move with `WASD` and rotate the camera with left mouse.
- [ ] Confirm three scouts and two skirmishers appear on the NavMesh.
- [ ] Use left mouse for light attack and confirm enemies take damage.
- [ ] Use right mouse to parry a yellow skirmisher wind-up.
- [ ] Confirm a successful parry adds Ember and fills the Ember bar.
- [ ] Confirm a full Ember meter empowers the next light attack and is consumed.
- [ ] Use Left Shift to dodge and confirm temporary invulnerability.
- [ ] Confirm health and enemy counters update and dead enemies disappear.

## BarricadeTest

- [ ] Open `Assets/Scenes/BarricadeTest.unity` and press Play.
- [ ] Confirm the build timer and wall/trap counts appear.
- [ ] Place up to eight walls and three traps.
- [ ] Use `T` to switch piece type, `R` to rotate, and right-click to remove.
- [ ] Confirm invalid/duplicate grid placements are rejected.
- [ ] Press Enter or wait for the timer to start the raid.
- [ ] Confirm enemies spawn only after the build phase.
- [ ] Confirm walls carve/block paths and traps damage enemies.
- [ ] Confirm the player can fight during the raid.

## Chapter1

- [ ] Open `Assets/Scenes/Chapter1.unity` and press Play.
- [ ] Speak to Tomas, Mara, both villagers, and Lira with `E`.
- [ ] Confirm repeated dialogue does not count twice.
- [ ] Confirm smoke appears and Mission 1.2 begins after all five talks.
- [ ] Build barricades, start the raid, and defeat both enemy waves.
- [ ] Confirm Mission 1.2 failure restarts with full health and a fresh barricade
  phase.
- [ ] Confirm the royal rider appears after both waves.
- [ ] Speak to the rider and confirm the Chapter 1 complete screen.
- [ ] Confirm no movement or combat occurs while a dialogue box is open.
