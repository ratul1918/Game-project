# Ember Blade Phase 2 final checklist

## Server

1. Start PostgreSQL:

   ```bash
   cd server
   docker compose up -d
   ```

2. Start the server:

   ```bash
   mvn spring-boot:run
   ```

3. Register and log in through the Phase 1 Unity scene.
4. Confirm the WebSocket connects and threat updates appear.
5. Confirm the server sends `STORY_BEAT` when Chapter 1 missions start and
   complete.
6. Confirm the Chapter 1 mission result is persisted through `/api/save`.

## Unity

1. Open `client/` in Unity `6000.6.6f1`.
2. Open `Assets/Scenes/Phase1.unity`.
3. Register a test user and log in.
4. Open `Assets/Scenes/CombatTest.unity` and verify movement, attacks, parry,
   dodge, Ember, enemies, and health.
5. Open `Assets/Scenes/BarricadeTest.unity` and verify the build phase, walls,
   traps, early raid, and NavMesh blocking.
6. Open `Assets/Scenes/Chapter1.unity`.
7. Complete Mission 1.1 by speaking to Tomas, Mara, both villagers, and Lira.
8. Build defenses, then complete both enemy waves in Mission 1.2.
9. Verify player death restarts Mission 1.2 with a fresh build phase.
10. Speak with the royal rider to complete Mission 1.3.
11. Confirm the temporary story-beat text appears when mission events are sent.
12. Confirm Chapter 1 displays its completion screen.

The Unity client remains playable when the server is unavailable; mission API
requests log warnings and do not stop local gameplay.
